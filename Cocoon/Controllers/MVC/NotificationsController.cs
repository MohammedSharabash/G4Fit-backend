using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using G4Fit.Models.ViewModels;
using G4Fit.Helpers;
using G4Fit.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace G4Fit.Controllers.MVC
{
    [AdminAuthorizeAttribute(Roles = "Admin,SubAdmin")]
    public class NotificationsController : BaseController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ActionResult Index()
        {

            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.Notifications)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateNotificationVM notificationVM)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.Notifications)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            if (ModelState.IsValid == true)
            {
                switch (notificationVM.OS)
                {
                    case Models.Enums.OS.Android:
                        await Notifications.SendToAllAndroidDevices(notificationVM.Title, notificationVM.Body);
                        break;
                    case Models.Enums.OS.IOS:
                        await Notifications.SendToAllIOSDevices(notificationVM.Title, notificationVM.Body);
                        break;
                    default:
                        await Notifications.SendToAllAndroidDevices(notificationVM.Title, notificationVM.Body);
                        await Notifications.SendToAllIOSDevices(notificationVM.Title, notificationVM.Body);
                        break;
                }
                TempData["SentSuccess"] = true;
                return RedirectToAction("Index");
            }
            TempData["SentError"] = false;
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult All()
        {
            return View(db.Notifications.Where(s => s.IsDeleted == false && s.UserId == CurrentUserId).OrderByDescending(s => s.CreatedOn).ToList());
        }

        [AllowAnonymous]
        public ActionResult MarkSeen()
        {
            db.Notifications.Where(s => s.IsDeleted == false && s.IsSeen == false && s.UserId == CurrentUserId).ToList().ForEach(w => w.IsSeen = true);
            db.SaveChanges();
            return RedirectToAction("All");
        }
    }
}