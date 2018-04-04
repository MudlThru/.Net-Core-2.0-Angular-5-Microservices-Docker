using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Mapster;
//
using auth.ViewModels;
using auth.Data;

namespace auth.Controllers
{

    public class UserController : BaseApiController
    {

        #region Constructor

        public UserController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            ): base(context, roleManager, userManager, configuration){}

        #endregion

        #region Restful conventions
        /// <summmary>
        /// POST: api/user
        /// </summary>
        /// <returns>Creates a new User and return it accordingly </returns>
        [HttpPut()]
        public async Task<IActionResult> Add([FromBody]UserViewModel model)
        {
            // return a generic HTTP status 500 (Server Error) if the client payload is invalid
            if (model == null) return new StatusCodeResult(500);

            //check if username/email already exists
            ApplicationUser user = await UserManager.FindByNameAsync(model.UserName);
            if(user != null) return BadRequest("Username already exists");

            user = await UserManager.FindByEmailAsync(model.Email);
            if(user != null) return BadRequest("Email already exists");

            var now = DateTime.Now;

            //create a new Item wiht the client-sent json data
            user = new ApplicationUser(){
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                Email = model.Email,
                DisplayName = model.DisplayName,
                CreatedDate = now,
                LastModifiedDate = now
            };

            //Add the user to the Db with the chosen password
            await UserManager.CreateAsync(user, model.Password);

            // Add the user to the 'RegisteredUser' role
            await UserManager.AddToRoleAsync(user, "RegisteredUser");

            //Remove lockout and E-mail confirmation, you might want to change this in production
            user.EmailConfirmed = true;
            user.LockoutEnabled = false;

            //persist the changes into the Database
            dbContext.SaveChanges();

            //return the newly-created User to the client
            return Json(user.Adapt<UserViewModel>(), JsonSettings);

        }

        #endregion

    }

}
