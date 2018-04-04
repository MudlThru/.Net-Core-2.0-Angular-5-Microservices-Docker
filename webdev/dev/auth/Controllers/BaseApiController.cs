using System; 
using Microsoft.AspNetCore.Mvc; 
using Newtonsoft.Json; 
using System.Collections.Generic;
//
using auth.ViewModels;
using System.Linq;
using auth.Data;
//using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace auth.Controllers{
    
    [Route("auth/[controller]")]
    public class BaseApiController : Controller {
        
        #region Contructor
        public BaseApiController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
        )
        {
            //Instatiate the ApplicationDbContext through DI
            dbContext = context;
            RoleManager = roleManager;
            UserManager = userManager;
            Configuration = configuration;

            //Instantiate a single JsonSerializerSettings object that can be resused multiple times
            JsonSettings = new JsonSerializerSettings(){ Formatting = Formatting.Indented }; 
        }
        #endregion

        #region Shared Properties
        protected ApplicationDbContext dbContext { get; set; }
        protected RoleManager<IdentityRole> RoleManager { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }
        protected IConfiguration Configuration { get; set; }
        protected JsonSerializerSettings JsonSettings {get; private set; }
        #endregion

    }

}
