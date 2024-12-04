using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using G4Fit.Models.Data;
using G4Fit.Models.Domains;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;

[assembly: OwinStartup(typeof(G4Fit.Startup))]

namespace G4Fit
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            var issuer = Convert.ToString(ConfigurationManager.AppSettings["config:JwtValidIssuer"]);
            var audience = Convert.ToString(ConfigurationManager.AppSettings["config:JwtValidAudiance"]);
            var secret = Convert.ToString(ConfigurationManager.AppSettings["config:JwtKey"]);  // Should be a strong key

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                    }
                });


            CreateRoles();
            CreateAdminUser();
        }

        public void CreateRoles()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            if (!roleManager.RoleExists("Admin"))
            {
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("SubAdmin"))
            {
                var role = new IdentityRole();
                role.Name = "SubAdmin";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("Supplier"))
            {
                var role = new IdentityRole();
                role.Name = "Supplier";
                roleManager.Create(role);
            }
        }

        public void CreateAdminUser()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var user = new ApplicationUser()
            {
                EmailConfirmed = true,
                Name = "G4Fit Admin",
                Email = "Admin@G4Fit.com",
                PhoneNumber = "01234567890",
                PhoneNumberConfirmed = true,
                UserName = "Admin@G4Fit.com",
            };

            string userPWD = "123456";
            var IsExist = UserManager.FindByEmail(user.Email);
            if (IsExist == null)
            {
                var chkUser = UserManager.Create(user, userPWD);
                if (chkUser.Succeeded)
                {
                    var result = UserManager.AddToRole(user.Id, "Admin");
                }

            }

        }
    }
}
