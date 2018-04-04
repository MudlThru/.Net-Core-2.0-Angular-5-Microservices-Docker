using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore.Metadata; 
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
//
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace auth.Data{

    public class DbSeeder{

    #region Public Methods

    public static void Seed(
        ApplicationDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager
        ){
        
        // Create default users (if there are none)
        if(!dbContext.Users.Any()) {
            CreateUsers(dbContext, roleManager, userManager).GetAwaiter().GetResult();
        }

        //Check for and create other entities (if there are none)
        //if(!dbContext.Entity.Any()) CreateEntities(dbContext);
    }

    #endregion

    #region Seed Methods

    private static async Task CreateUsers(
        ApplicationDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager
        ) {

        // local variables 
        DateTime createdDate = new DateTime( 2016, 03, 01, 12, 30, 00); 
        DateTime lastModifiedDate = DateTime.Now; 

        string role_Administrator = "Administrator";
        string role_RegisteredUser = "RegisteredUser";

        //Create roles (if they don't exist yet)
        if(!await roleManager.RoleExistsAsync(role_Administrator)){
            await roleManager.CreateAsync(new IdentityRole(role_Administrator));
        }
        if(!await roleManager.RoleExistsAsync(role_RegisteredUser)){
            await roleManager.CreateAsync(new IdentityRole(role_RegisteredUser));
        }
        
        // Create the "Admin" ApplicationUser account (if it doesn't exist already) 
        var user_Admin = new ApplicationUser() { 
            //Id = Guid.NewGuid().ToString(), 
            SecurityStamp = Guid.NewGuid().ToString(), 
            UserName = "Admin", 
            Email = "admin@myapp.com", 
            CreatedDate = createdDate, 
            LastModifiedDate = lastModifiedDate 
            }; 
        
        // Insert the Admin user into the Database and assign the "Administrator" and "RegisteredUser" roles to them.
        //dbContext.Users.Add(user_Admin); 
        if(await userManager.FindByNameAsync(user_Admin.UserName) == null){
            await userManager.CreateAsync(user_Admin, "Pass4Admin"); //You'll want to change this in production
            await userManager.AddToRoleAsync(user_Admin, role_RegisteredUser);
            await userManager.AddToRoleAsync(user_Admin, role_Administrator);
            //remove Lockout and E-mail confirmation.
            user_Admin.EmailConfirmed = true;
            user_Admin.LockoutEnabled = false;
        }
        
        #if DEBUG 
        
        // Create some sample registered user accounts (if they don't exist already) 
        var userList = new System.Collections.Generic.List<ApplicationUser>();

        userList.Add(new ApplicationUser() { SecurityStamp = Guid.NewGuid(). ToString(), UserName = "Steve", Email = "steve@myapp.com", CreatedDate = createdDate, LastModifiedDate = lastModifiedDate });
        userList.Add(new ApplicationUser() { SecurityStamp = Guid.NewGuid(). ToString(), UserName = "Terry", Email = "terry@myapp.com", CreatedDate = createdDate, LastModifiedDate = lastModifiedDate });
        userList.Add(new ApplicationUser() { SecurityStamp = Guid.NewGuid(). ToString(), UserName = "Phil", Email = "phil@myapp.com", CreatedDate = createdDate, LastModifiedDate = lastModifiedDate });

        // Insert sample registered users into the Database 
        foreach (var user in userList)
        {
            if(await userManager.FindByNameAsync(user.UserName) == null){
                await userManager.CreateAsync(user, string.Format("Pass4{0}", user.UserName));
                await userManager.AddToRoleAsync(user, role_RegisteredUser);
                //remove Lockout and E-mail confirmation, you might want to change this in production
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
            }
        }
        
        #endif 
        
        await dbContext.SaveChangesAsync(); 

    }

// Any other main seed methods should go here
    
    #endregion

    #region Utility Methods 
// Put any other useful seed methods here
    #endregion

    }

}
