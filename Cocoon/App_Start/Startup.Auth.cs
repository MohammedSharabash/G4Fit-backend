using System;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using G4Fit.Providers;
using Microsoft.AspNet.Identity.Owin;
using G4Fit.Models.Data;
using G4Fit.Models.Domains;

namespace G4Fit
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }
        public static string PublicClientId { get; private set; }

        public void ConfigureAuth(IAppBuilder app)
        {
            // Create per-request contexts for the ApplicationDbContext and UserManager
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Use cookie authentication to store the logged-in user's information.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                CookieName = "G4FitIdentity",
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = TimeSpan.FromHours(24), // تمديد الكوكي إلى 24 ساعة
                SlidingExpiration = true, // يجدد الجلسة مع كل طلب جديد
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),  // Set interval to revalidate identity.
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            // Enable third-party external login (like Google, Facebook, etc.)
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Configure OAuth server for issuing tokens
            PublicClientId = "self";  // For self-issued client IDs.

            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),  // The URL where tokens are requested.
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),  // Endpoint for external logins.
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(365),  // Set token expiration time.
                RefreshTokenProvider = new OAuthCustomRefreshTokenProvider(),  // Handle refresh tokens.
                AllowInsecureHttp = false  // Set to true in development environments, but always use HTTPS in production.
            };

            // Enable OAuth bearer tokens to authenticate requests.
            app.UseOAuthBearerTokens(OAuthOptions);
        }
    }
}
