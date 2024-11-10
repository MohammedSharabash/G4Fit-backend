using G4Fit.Models.Enums;
using G4Fit.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [AllowAnonymous]
    public class PartialsController : BaseController
    {
        public PartialViewResult WebsiteNavbar()
        {
            var UserOrder = db.Orders.FirstOrDefault(x => ((x.UserId == CurrentUserId && x.UserId != null) || x.UnknownUserKeyIdentifier == Session.SessionID) && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            NavbarVM navbarVM = new NavbarVM()
            {
                NotificationsCount = db.Notifications.Count(s => s.IsDeleted == false && s.IsSeen == false && s.UserId == CurrentUserId),
                ShoppingCartCount = UserOrder != null && UserOrder.Items != null ? UserOrder.Items.Count(s => s.IsDeleted == false) : 0,
                //Categories = db.Categories.Where(s => s.IsDeleted == false).ToList(),
                Categories = db.SubCategories.Where(s => s.IsDeleted == false).ToList(),
                Notifications = db.Notifications.Where(s => s.IsDeleted == false && s.IsSeen == false && s.UserId == CurrentUserId).OrderByDescending(s => s.CreatedOn).Take(7).ToList()
            };
            if (User.Identity.IsAuthenticated == true)
            {
                var user = db.Users.Find(CurrentUserId);
                if (user != null)
                {
                    navbarVM.UserImage = user.ImageUrl;
                    navbarVM.Username = user.Name;
                    navbarVM.UserPhoneNumber = user.PhoneNumber;
                }
            }
            return PartialView(navbarVM);
        }

        public PartialViewResult WebsiteFooter()
        {
            FooterVM footer = new FooterVM();
            footer.About = db.CompanyDatas.FirstOrDefault(s => s.IsDeleted == false);
            return PartialView(footer);
        }
        [Authorize]
        public ActionResult Header()
        {
            var CurrentUser = db.Users.Find(CurrentUserId);
            HeaderVM headerVM = new HeaderVM();
            if (CurrentUser != null)
            {
                headerVM.Name = CurrentUser.Name;
                headerVM.UserImage = CurrentUser.ImageUrl;
            }
            return PartialView(headerVM);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminSideMenu()
        {
            var CurrentUser = db.Users.Find(CurrentUserId);
            SideMenuVM sideMenuVM = new SideMenuVM();
            if (CurrentUser != null)
            {
                sideMenuVM.UserImage = CurrentUser.ImageUrl;
                sideMenuVM.Name = CurrentUser.Name;
            }
            return PartialView(sideMenuVM);
        }

        [Authorize(Roles = "Supplier")]
        public ActionResult SupplierSideMenu()
        {
            var CurrentUser = db.Users.Find(CurrentUserId);
            SideMenuVM sideMenuVM = new SideMenuVM();
            if (CurrentUser != null)
            {
                sideMenuVM.UserImage = CurrentUser.ImageUrl;
                sideMenuVM.Name = CurrentUser.Name;
            }
            return PartialView(sideMenuVM);
        }
    }
}