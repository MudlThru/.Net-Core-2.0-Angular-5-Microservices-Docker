# .Net Core 2.0, Angular 5, Microservices & Docker
## Viewing the all of the projects in VS Code
_We are now going to do a bit of work on our WebAPI projects, cue the streamers, balloons and fanfare._

In order to make navigating the projects a bit easier we are going to open the `webdev\dev` folder in VS Code. From the file menu select **File** > **Open Folder** and select the `dev` folder.

Alternatively, if VS Code is already closed, you can just right-click on the `webdev\dev` folder and select **Open in Code**.
 
You should now see the folder structure reflected within the VS Code Explorer panel:

![Folders](img/WebApi%20-%2001%20-%20Folders.png)
 
### Adding extensions
At some point when editing certain types of files, at the bottom-right corner of the screen, you may be prompted to install recommended extensions, click on "Show Recommendations".

![Extensions Prompt](img/WebApi%20-%2003%20-%20Extensions%20Prompt.png)
   
You will then be presented with other extensions to install; feel free to install any of them, the following are especially useful:
+ C#
+ TSLint
+ Debugger for Chrome, if you are using Chrome (I would recommend testing on multiple browsers)
+ Angular Language Services

![Extensions](img/WebApi%20-%2004%20-%20Extensions%20Install.png)

To get back to the Explorer panel either press `Ctrl + Shift + E` or click on the "Files" icon at the top.

## Configure the Auth WebAPI project
### Configuring the application and middleware
In `Program.cs`, we need to ensure the `BuildWebHost` method includes the `UseUrls` line, we are going to use this as a way to pass-in the urls/ports we want the Kestrel webserver to listen on.
```csharp
public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseUrls(new ConfigurationBuilder().AddCommandLine(args).Build()["urls"])
        .UseStartup<Startup>()
        .Build();
```
Let’s wade into some C# ‘proper’ now and edit the `Startup.cs`, and the `appsettings.config` files, in the root of the `auth` folder.

First thing we need to do is import some more namespaces so we can use some other classes:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// Add the following
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using auth.Data;
```
_**Please note:** The `auth.Data` namespace doesn’t exist yet, so the project will not build at the moment. Don’t let that put you off though, as we will be adding it in a bit._

Now for the heavy lifting. We are going to start by editing the `ConfigureServices` method:
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
}
```
After `services.AddMvc();` add the following sections:

Add a connection to a database, for the moment we are going to be using SQLite, to persist our user data. This retrieves the connection string from the config file, which we will amend in a moment.
```csharp
//DB Conection
services.AddEntityFrameworkSqlite();
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
);
```
In `appsettings.config` paste in the following database connection for our SQLite database _(**Note:** I would recommend putting it at the top, just so it is easier to find later on)_. We will likely change this in the future for either a SQL or MySQL database. 

This will create and use a file called `app.db` in the root of the `/auth` folder, which is fine for now.
```json
"ConnectionStrings": {
    "DefaultConnection": "DataSource=app.db"
},
```
Back in `Startup.cs`, we need to add the standard ASP.Net Identity middleware:
```csharp
//Add Asp.net Identity support
services.AddIdentity<ApplicationUser, IdentityRole>(
    opts => {
        opts.Password.RequireDigit = true;
        opts.Password.RequireLowercase = true;
        opts.Password.RequireUppercase = true;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequiredLength = 7;
    }
).AddEntityFrameworkStores<ApplicationDbContext>();
```
Next, we add the authentication middleware, specifying that we want to use JWT:
```csharp
//Add Authentication with JWT Tokens
services.AddAuthentication(opts => {
    opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
```
And setup the JWT bearer:
```csharp
.AddJwtBearer(cfg => {
    cfg.RequireHttpsMetadata = false;//Normally you would only set this to false for testing, we are going to hide behind a proxy so we’re not too concerned about HTTPS for our APIs
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters(){
        //standhard configuration
        ValidIssuer = Configuration["Auth:Jwt:Issuer"],
        ValidAudience = Configuration["Auth:Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Configuration["Auth:Jwt:Key"])
        ),
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = true
    };
})
```
In `appsetting.config` add the following authentication secret. The Jwt Key should just be a random string, about 16 characters long, this will be used by your other APIs to validate the Jwt token.
```json
"Auth": {
    "Jwt": {
    "Issuer": "https://localhost/",
    "Audience": "https://localhost/",
    "Key": "<--YOUR SECRET KEY-->",
    "TokenExpirationInMinutes": 86400
    }
}
```
We can then add the 3rd party authentication provider middleware to `Startup.cs`:

____

**Note:** to learn about 3rd party/social logins visit [the Microsoft documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/) which will provide furtehr details on how to set this up.

____

Add external login via Facebook:
```csharp
//add Facebook support
.AddFacebook(opts =>{
    opts.AppId = Configuration["Auth:Facebook:AppId"];
    opts.AppSecret = Configuration["Auth:Facebook:AppSecret"];
})
```
Add external login via Google:
```csharp
//add Google support
.AddGoogle(opts =>{
    opts.ClientId = Configuration["Auth:Google:ClientId"];
    opts.ClientSecret = Configuration["Auth:Google:ClientSecret"];
    opts.Scope.Add("profile");
});
```
The Facebook and Google secrets will be provided to you when you’ve setup your applications in the provider’s portal, the whole Auth section of the appsettings.config file should look something like this:
```json
"Auth": {
    "Jwt": {
        "Issuer": "https://localhost/",
        "Audience": "https://localhost/",
        "Key": "<--YOUR SECRET KEY-->",
        "TokenExpirationInMinutes": 86400
    },
    "Google": {
        "ClientId": "<--YOUR CLIENT ID-->",
        "ClientSecret":"<--YOUR CLIENT SECRET-->",
        "Uri":"https://localhost/signin-google"
    },
    "Facebook": {
        "AppId": "<--YOUR APP ID-->",
        "AppSecret":"<--YOUR APP SECRET-->",
        "Uri":"https://localhost/signin-facebook"
    }
}
```
The next thing to edit is the Configure method
```csharp
// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseMvc();
}
```
Before the `app.UseMvc();` line, insert the following section:
```csharp
//Add forward headers to allow correct paths to be used - https://github.com/aspnet/Identity/issues/1036
// https://stackoverflow.com/questions/43860128/asp-net-core-google-authentication
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);
```
We also need to add the following:
```csharp
//Add the AuthenticationMiddleware to the pipeline
app.UseAuthentication();
```
After the `app.UseMvc();` line paste in the following section:
```csharp
using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) { 
    var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>(); 
    try {
        // Create the Db if it doesn't exist
        dbContext.Database.EnsureCreated();
        // and applies any pending migration.
        dbContext.Database.Migrate(); 
        
        var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
        var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        // Seed the Db. 
        DbSeeder.Seed(dbContext, roleManager, userManager); 
    } catch(System.Exception e){
        throw e;
    }
}
```
That’s it for `Startup.cs`, now we can move onto our `DataHelper.cs` and `DbSeeder.cs` classes which we will be creating in a new `auth\Data` folder.

____
**Attribution:** 

The following sections refers to classes taken from [ASP.NET Core 2 and Angular 5](https://www.packtpub.com/application-development/aspnet-core-2-and-angular-5) by Valerio De Sanctis (Published by [Packt](https://github.com/PacktPublishing)) ( [Source](https://github.com/PacktPublishing/ASP.NET-Core-2-and-Angular-5) ,
[License](https://github.com/PacktPublishing/ASP.NET-Core-2-and-Angular-5/blob/master/LICENSE) ). 

I do not intend to explain in too much detail what these classes are doing, they are in essence placeholders for your own code but provide a good reference point from which to work from.

If you require further information or would like to learn more I fully encourage you to read, and work through, this book.
____

### Helper methods

`auth/Data/DataHelper.cs` contains static helper methods, of which we only need one of for the moment. This will be used to generate random passwords for our seeded users.

An area of interest in this file is the default PasswordOption settings:
```csharp
        if (opts == null) opts = new PasswordOptions() { 
            RequiredLength = 7, 
            RequiredUniqueChars = 4, 
            RequireDigit = true, 
            RequireLowercase = true, 
            RequireNonAlphanumeric = false, 
            RequireUppercase = true }; 
```
### Seeder methods

`auth/Data/DbSeeder.cs` contains a set of methods to seed/pre-populate our database for testing, and in some scenarios production.

I've made a slight change here to allow you to add new users via a collection rather than having to create the user and password individually, instead you populate a list using `userList.Add()`.

### Database Context

`ApplicationDbContext.cs` holds information about the types of information available within the context of the database; along with information on their relationships to one another. Our database is going to be pretty light, as we are only interested in users etc. at the moment.

### Models
Under the `auth/Data` folder, create a new folder called Models and create the following:

#### Application User
`ApplicationUser.cs` to hold information about our user records.

#### Tokens
`Token.cs` to hold information about our authentication tokens.

These are the only models we need to provide authentication for our site.

### Controllers
#### Routing changes
Unlike the `api` project, the routing for all of the controllers in the `auth` WebAPI project are going to need to be set to `auth/[controller]`, not `api/[controller]`. This is because when the client requests a url for `/api/values` the request is going to be sent to the `api` WebAPI not the `auth` WebAPI, we therefore need to ensure that the `auth` WebAPI controllers are configured to receive requests for urls going to `/auth/values`.
For example, the `ValuesController.cs` would need to be changed to look like the following:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace auth.Controllers
{
    [Route("auth/[controller]")] // << Changed the routing to match nGinx configuration
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Yey", "it", "works!" };
        }
```
That was rather a lot of explanation for changing 3 or 4 characters; however, if you don’t change it the auth WebAPI will never work when run via nGinx but will work when testing locally.

One thing we could do is to remove the `auth/` part of the route entirely, leaving just the [controller] part and change the nGinx config to reflect this. Here I have chosen not to do this; for the reason that, when I am working on multiple files for different controllers, I like to see the routing to ensure I don’t confuse myself.

So, when creating future WebAPI services ensure you update the routing for the controller; you will need to do this for every controller in this project, fortunately the bootstrapped project only has one of them. We are going to build our controllers from scratch so go ahead and delete `ValuesController.cs`.

Now onto our controllers. Open the `auth\Controllers` folder and create the following files:

#### Base Controller
In order to make developing our APIs easier it is generally a good option to inherit your controllers from a base class. This allows for common values and methods to be in place in each controller and reduces the amount of code you need to re-write in each controller.

In our case we are going to use a class called `BaseApiController`. So go ahead and create a new file in the controllers folder called `BaseApiController.cs`.
```csharp
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
```
#### User Controller
The `UserController.cs` file is going to be used to manage the creation of our users, nothing else.

##### Token Controller
`TokenController.cs` manages how our JWT tokens are created, refreshed and authenticated.

### View Models
View Models are classes we will use the transfer information between our Angular application and the WebAPI, when we get to the Angular side you will find that we will be required to create a corresponding set of interfaces for these View Models.

#### User
The `UserViewModel` class contains basic user information and an empty constructor. 

#### Json Response
The `JsonResponse` class provides a structure for responses in the JSON format, we will make extensive use of this in the auth WebAPI project.

#### Token Request
The `TokenRequestViewModel` class holds all of the values for our token requests, if you want to change the data in your tokens you will likely have to amend this class.

#### Token Response
The `TokenResponseViewModel` provides the structure for the output token.

#### External Login Request
The `ExternalLoginRequestViewModel` class is a basic class with values to authenticate with external providers.

### Build
You should now be able to build this WebAPI project using your test script, but don’t expect anything to show in the browser.

### Configure the API WebAPI project
#### Configuring the application & middleware
Unlike the auth project, there is less configuration required; although we do need to alter the `Startup.cs` and `appsettings.config` files.

In Program.cs, we need to ensure the BuildWebHost method includes the UseUrls line, we are going to use this as a way to pass-in the urls/ports we want the Kestrel webserver to listen on.
```csharp
public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseUrls(new ConfigurationBuilder().AddCommandLine(args).Build()["urls"])
        .UseStartup<Startup>()
        .Build();
```
Add the following namespaces to Startup.cs:
```csharp
//
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
```
In Startup.cs find the ConfigureServices method and insert the following before the services.AddMvc(); line:
```csharp
//Add Authentication with JWT Tokens
services.AddAuthentication(opts => {
    opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(cfg => {
    cfg.RequireHttpsMetadata = false;//Only for testing!!!
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters(){
        //standhard configuration
        ValidIssuer = Configuration["Auth:Jwt:Issuer"],
        ValidAudience = Configuration["Auth:Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Configuration["Auth:Jwt:Key"])
        ),
        ClockSkew = TimeSpan.Zero,
        //securty switched
        RequireExpirationTime = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = true
    };
});
```
Next, find the Configure method and insert the following before the app.UseMvc(); line:
```csharp
//Add forward headers to allow correct paths to be used - https://github.com/aspnet/Identity/issues/1036
// https://stackoverflow.com/questions/43860128/asp-net-core-google-authentication
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

//Add the AuthenticationMiddleware to the pipeline
app.UseAuthentication();
```
Don’t forget to add the Auth section to the appsettings.config file, we don’t need any 3rd party stuff here.
```json
,
"Auth": {
    "Jwt": {
    "Issuer": "https://localhost/",
    "Audience": "https://localhost/",
    "Key": "<--YOUR SECRET KEY-->",
    "TokenExpirationInMinutes": 86400
    }
}
```
## Checking User Authentication

Now in the ValuesController.cs file, of the `api` project, we can decorate the controller class with the Authorize annotation:
```csharp
[Authorize] //Requires authorisation for each request
[Route("api/[controller]")]
public class ValuesController : Controller
{
    // GET api/values
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }
```
Don’t forget to add the Authorisation namespace:
```csharp
using Microsoft.AspNetCore.Authorization;
```
This will prevent any unauthorised users from using this controller.

## Checking Role Authentication

If you want to provide authorisation by role you can amend the annotation to include one, or more, roles:

```csharp
[Authorize(Roles = "Administrator")] //Requires authorisation and Administrator role for each request
```

Again you should be able to build the project using you test script but don’t expect anything to show in the browser.

## Testing
At this point you could run the `testApi.bat` file you created earlier to check everything is working; however, at most all you will see is an HTTP 401 error. For the `api` WebAPI this is currently our expected behaviour, as we are checking the user is authorised before they can access the controller.

![Unauthorised](img/WebApi%20-%2005%20-%20Unauthorised.png)