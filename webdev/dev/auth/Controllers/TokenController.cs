using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Text;
//
using auth.ViewModels;
using auth.Data;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net.Http;

namespace auth.Controllers{

    public class TokenController : BaseApiController
    {
        #region Private Members
        protected SignInManager<ApplicationUser> SignInManager { get; private set; }
        #endregion Private Members

        #region Constructor

        public TokenController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration
            ): base(context, roleManager, userManager, configuration){
                SignInManager= signInManager;
            }

        #endregion Constructor

        [HttpPost("Auth")]
        public async Task<IActionResult> Jwt([FromBody]TokenRequestViewModel model){
            //return a generic HTTP 500 (Server Error) if the client payload is invalid
            if(model == null) return new StatusCodeResult(500);

            switch (model.grant_type)
            {
                case "password":
                    return await GetToken(model);
                case "refresh_token":
                    return await RefreshToken(model);
                default:
                //not supported - return a HTTP 401 (Unauthorised)
                return new UnauthorizedResult();
            }
        }

        private async Task<IActionResult> GetToken(TokenRequestViewModel model){
            try{
                //check if there's a user with the given username
                var user = await UserManager.FindByNameAsync(model.username);
                //fallback to support email address instead of username
                if(user == null && model.username.Contains("@")){
                    user = await UserManager.FindByEmailAsync(model.username);
                }

                if(user == null || !await UserManager.CheckPasswordAsync(user, model.password))
                {
                    //user doesn't exist or password mismatch
                    return new UnauthorizedResult();
                }

                //username & password matches: cerate the refresh token
                var rt = CreateRefreshToken(model.client_id, user.Id);

                //add the refresh token to the DB
                dbContext.Tokens.Add(rt);
                dbContext.SaveChanges();

                //create & return the access token
                var t = CreateAccessToken(user.Id, rt.Value);

                return Json(t);

            }
            catch (Exception)
            {
                return new UnauthorizedResult();
            }
        }

        private async Task<IActionResult> RefreshToken(TokenRequestViewModel model)
        {
            try
            {
                // check if the received refreshToken exists for the given clientId
                var rt = dbContext.Tokens.FirstOrDefault(t => 
                    t.ClientId == model.client_id && 
                    t.Value == model.refresh_token
                );

                if (rt == null)
                {
                    //refresh token not found or invalid (or invalid clientId)
                    return new UnauthorizedResult();
                }

                //check if there's a user with the refresh token's userId
                var user = await UserManager.FindByIdAsync(rt.UserId);

                if (user == null)
                {
                    //UserId not found or invalid
                    return new UnauthorizedResult();
                }

                //generate a new refresh token
                var rtNew = CreateRefreshToken(rt.ClientId, rt.UserId);

                //invalidate the old refresh token (by deleting it)
                dbContext.Tokens.Remove(rt);

                //add the new refresh token
                dbContext.Tokens.Add(rtNew);

                //persist changes to the db
                dbContext.SaveChanges();

                //create a new access token...
                var response = CreateAccessToken(rtNew.UserId, rtNew.Value);

                //...and send it to the client
                return Json(response);

            }
            catch (Exception)
            {
                return new UnauthorizedResult();
            }

        }

        private Token CreateRefreshToken(string clientId, string userId)
        {
            return new Token()
            {
                ClientId = clientId,
                UserId = userId,
                Type = 0,
                Value = Guid.NewGuid().ToString("N"),
                CreatedDate = DateTime.UtcNow
            };
        }

        private TokenResponseViewModel CreateAccessToken(string userId, string refreshToken)
        {
            DateTime now = DateTime.UtcNow;

            //add the registered claims for JWT (RFC7519).  For more info, look at https://tools.ietf.org/html/rfc7519#section-4.1
            var user = UserManager.FindByIdAsync(userId).Result;
            var userRoles = UserManager.GetRolesAsync(user).Result;

            //var claims = new[]
            var claims = new List<Claim>(new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, userId),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString())
                    //TODO: add additional claims here
                    //new Claim("roles", )
            });

            //https://www.codeproject.com/articles/1203975/JWT-Security-Part-Create-Token
            // Add userclaims from storage
            claims.AddRange(UserManager.GetClaimsAsync(user).Result);

            // Add user role, they are converted to claims
            var roleNames = UserManager.GetRolesAsync(user).Result;
            foreach (var roleName in roleNames)
            {
                // Find IdentityRole by name
                var role = RoleManager.FindByNameAsync(roleName).Result;
                if (role != null)
                {
                // Convert Identity to claim and add 
                var roleClaim = new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, Configuration["Auth:Jwt:Issuer"]);
                claims.Add(roleClaim);

                // Add claims belonging to the role
                var roleClaims = RoleManager.GetClaimsAsync(role).Result;
                claims.AddRange(roleClaims);
                }
            }

            var tokenExpirationMins = Configuration.GetValue<int>("Auth:Jwt:TokenExpirationInMinutes");
            var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Auth:Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: Configuration["Auth:Jwt:Issuer"],
                audience: Configuration["Auth:Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromMinutes(tokenExpirationMins)),
                signingCredentials: new SigningCredentials(issuerSigningKey, SecurityAlgorithms.HmacSha256)  
            );

            var encodedToekn = new JwtSecurityTokenHandler().WriteToken(token);

            //build and return the response
            return new TokenResponseViewModel(){
                token = encodedToekn,
                expiration = tokenExpirationMins,
                refresh_token = refreshToken
            };

        }

        [HttpGet("ExternalLogin/{provider}")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            switch (provider.ToLower())
            {
                case "facebook":
                case "google":
                //case "twitter":
                    //redirect the request to the external provider.
                    var redirectUrl = Url.Action(nameof(ExternalLoginCallBack), "Token", new { returnUrl });
                    var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                    return Challenge(properties, provider);
                default:
                //provider not supported
                return BadRequest(new {Error = String.Format("Provider '{0}' is not supported.", provider)});
            }
        }

        [HttpGet("ExternalLoginCallBack")]
        public async Task<IActionResult> ExternalLoginCallBack(
            string returnUrl = null,
            string remoteError = null
        )
        {
            if(!string.IsNullOrEmpty(remoteError))
            {
                //todo: handler external provider errors
                throw new Exception(String.Format("External Provider error: {0}", remoteError));
            }

            //extract the login info obtained from teh external provider
            var info = await SignInManager.GetExternalLoginInfoAsync();
            if(info==null){
                //if there's none, emit an error
                throw new Exception("Error: No login info avlailable.");
            }

            //Check if this user has already registered himself wiht this eternal provider before
            var user = await UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if(user == null)
            {
                //if we reach this point, it means that this user never tried to log in using this eternal provider. however they could have used and/or have a local account.
                //We can find out if that's the case by looking for their email address.

                //retirve the 'emailaddress' claim
                var emailKey = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
                var email = info.Principal.FindFirst(emailKey).Value;

                //lookup idf theresds a username with this email address in teh db
                user = await UserManager.FindByEmailAsync(email);
                if(user == null)
                {
                    //no user has been found: register a new user using the info retrieved from thr provider
                    DateTime now = DateTime.Now;

                    //create a unique username using the 'nameidentfoer' claim
                    var idKey = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
                    var username = String.Format(
                        "{0}{1}{2}", 
                        info.LoginProvider, 
                        info.Principal.FindFirst(idKey).Value, 
                        Guid.NewGuid().ToString("N")
                    );

                    user = new ApplicationUser(){
                        SecurityStamp=Guid.NewGuid().ToString(),
                        UserName = username,
                        Email = email,
                        CreatedDate = now,
                        LastModifiedDate = now
                    };

                    //add the user to teh Db with a random password
                    await UserManager.CreateAsync(user, DataHelper.GenerateRandomPassword());

                    //remove lokout and email confirmation
                    user.EmailConfirmed = true;
                    user.LockoutEnabled = false;

                    //persist everything to the DB
                    await dbContext.SaveChangesAsync();

                }

                //register this ecternal provider to the user
                var ir = await UserManager.AddLoginAsync(user, info);
                if(ir.Succeeded)
                {
                    dbContext.SaveChanges();
                }
                else throw new Exception("Authentication error");
            }

            //create the refresh token
            var rt = CreateRefreshToken("TestMakerFree", user.Id);

            //add the new refresh token to the DB
            dbContext.Tokens.Add(rt);
            dbContext.SaveChanges();

            //create and return the access token
            var t = CreateAccessToken(user.Id, rt.Value);

            //output a <Script> tag to call a JS function registered into the parent window global scope
            return Content(
                "<script type=\"text/javascript\">" +
                "window.opener.externalProviderLogin(" +
                JsonConvert.SerializeObject(t, JsonSettings) +
                ");" + 
                "window.close();" +
                "</script>", 
                "text/html"
            );

        }

    }

}
