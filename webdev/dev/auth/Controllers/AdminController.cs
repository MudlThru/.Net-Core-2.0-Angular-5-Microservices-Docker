using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//
using auth.ViewModels;
using auth.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace auth.Controllers
{
	[Authorize(Roles = "Administrator")] //Requires authorisation and Administrator role for each request
    //[Route("auth/[controller]")]
    public class AdminController : BaseApiController
    {
		
		#region Constructor

        public AdminController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            ): base(context, roleManager, userManager, configuration){}

        #endregion
		
        // GET auth/admin
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET auth/admin/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return String.Format("value{0}", id);
        }

        // POST auth/admin
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT auth/admin/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE auth/admin/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
