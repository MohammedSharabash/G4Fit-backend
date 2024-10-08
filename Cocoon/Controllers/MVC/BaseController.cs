using G4Fit.Models.Data;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    public class BaseController : Controller
    {
        protected ApplicationDbContext db = new ApplicationDbContext();
        protected string CurrentUserId;
        protected string culture;
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            culture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            CurrentUserId = User.Identity.GetUserId();
            base.OnActionExecuting(filterContext);
        }
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string cultureName = null;

            // Attempt to read the culture cookie from Request
            HttpCookie cultureCookie = Request.Cookies["_G4FitCulture"];
            if (cultureCookie != null)
                cultureName = cultureCookie.Value;
            else
                cultureName = "ar-SA";
            //cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
            //        Request.UserLanguages[0] :  // obtain it from HTTP header AcceptLanguages
            //        null;
            // Validate culture name
            cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe

            // Modify current thread's cultures            
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            if (cultureName.ToLower().Contains("ar"))
            {
                Thread.CurrentThread.CurrentUICulture.DateTimeFormat = new CultureInfo("ar-EG").DateTimeFormat;
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture.DateTimeFormat = new CultureInfo("en-US").DateTimeFormat;
            }

            return base.BeginExecuteCore(callback, state);
        }
    }
    public static class CultureHelper
    {
        private static readonly List<string> _validCultures = new List<string>
        {
            "ar", "ar-DZ", "ar-BH", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA", "ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-AE", "ar-YE",
            "en", "en-AU", "en-BZ", "en-CA", "en-029", "en-IN", "en-IE", "en-JM", "en-MY", "en-NZ", "en-PH", "en-SG", "en-ZA", "en-TT", "en-GB", "en-US", "en-ZW", "et", "et-EE"
        };

        private static readonly List<string> _cultures = new List<string> {
        "en-US",  // first culture is the DEFAULT
        "ar"  // Arabic NEUTRAL culture
        
    };
        /// <summary>
        /// Returns true if the language is a right-to-left language. Otherwise, false.
        /// </summary>
        public static bool IsRighToLeft()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft;

        }
        /// <summary>
        /// Returns a valid culture name based on "name" parameter. If "name" is not valid, it returns the default culture "en-US"
        /// </summary>
        /// <param name="name" />Culture's name (e.g. en-US)</param>
        public static string GetImplementedCulture(string name)
        {
            // make sure it's not null
            if (string.IsNullOrEmpty(name))
                return GetDefaultCulture(); // return Default culture
                                            // make sure it is a valid culture first
            if (_validCultures.Where(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() == 0)
                return GetDefaultCulture(); // return Default culture if it is invalid
                                            // if it is implemented, accept it
            if (_cultures.Where(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() > 0)
                return name; // accept it
                             // Find a close match. For example, if you have "en-US" defined and the user requests "en-GB", 
                             // the function will return closes match that is "en-US" because at least the language is the same (ie English)  
            var n = GetNeutralCulture(name);
            foreach (var c in _cultures)
                if (c.StartsWith(n))
                    return c;
            // else 
            // It is not implemented
            return GetDefaultCulture(); // return Default culture as no match found
        }
        /// <summary>
        /// Returns default culture name which is the first name decalared (e.g. en-US)
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultCulture()
        {
            return _cultures[0]; // return Default culture
        }
        public static string GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture.Name;
        }
        public static string GetCurrentNeutralCulture()
        {
            return GetNeutralCulture(Thread.CurrentThread.CurrentCulture.Name);
        }
        public static string GetNeutralCulture(string name)
        {
            if (!name.Contains("-")) return name;

            return name.Split('-')[0]; // Read first part only. E.g. "en", "es"
        }
    }
}