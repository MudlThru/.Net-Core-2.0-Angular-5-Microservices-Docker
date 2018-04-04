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
//
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using auth.Data;

namespace auth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            //DB Conection
            services.AddEntityFrameworkSqlite();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
            );
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
            })
            //add Facebook support
            .AddFacebook(opts =>{
                opts.AppId = Configuration["Auth:Facebook:AppId"];
                opts.AppSecret = Configuration["Auth:Facebook:AppSecret"];
            })
            //add Google support
            .AddGoogle(opts =>{
                opts.ClientId = Configuration["Auth:Google:ClientId"];
                opts.ClientSecret = Configuration["Auth:Google:ClientSecret"];
                opts.Scope.Add("profile");
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
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

            app.UseMvc();

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

        }
    }
}
