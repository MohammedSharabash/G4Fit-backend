using System.Web;
using System.Web.Mvc;

namespace G4Fit
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // إعادة التوجيه إلى صفحة تسجيل الدخول الخاصة بلوحة التحكم
                filterContext.Result = new RedirectResult("~/cp/Login");
            }
            else
            {
                // إذا كان المستخدم مسجلاً الدخول ولكن لا يملك الصلاحيات
                filterContext.Result = new RedirectResult("~/Home/AccessDenied");
            }
        }
    }
}
