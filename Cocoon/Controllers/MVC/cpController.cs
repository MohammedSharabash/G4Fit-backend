using G4Fit.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    public class cpController : BaseController
    {
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var startOfDay = DateTime.UtcNow.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            ViewBag.OrdersCount = db.Orders.Count(i => i.OrderStatus == Models.Enums.OrderStatus.Placed);
            ViewBag.TotalProdfit = db.Orders.Where(i => i.IsPaid).Sum(x => (decimal?)x.Total) ?? 0;
            ViewBag.TotalProdfitMonth = db.Orders.Where(i => i.IsPaid && i.CreatedOn >= startOfMonth && i.CreatedOn <= endOfMonth).Sum(x => (decimal?)x.Total) ?? 0;
            ViewBag.OrdersTodayAmount = db.Orders.Where(i => i.OrderStatus == Models.Enums.OrderStatus.Delivered && i.CreatedOn >= startOfDay && i.CreatedOn <= endOfDay).Sum(x => (decimal?)x.Total) ?? 0;
            ViewBag.OrdersTodayCount = db.Orders.Count(i => i.OrderStatus == Models.Enums.OrderStatus.Delivered && i.CreatedOn >= startOfDay && i.CreatedOn <= endOfDay);
            ViewBag.OrdersMonthAmount = db.Orders.Where(i => i.OrderStatus == Models.Enums.OrderStatus.Delivered && i.CreatedOn >= startOfMonth && i.CreatedOn <= endOfMonth).Sum(x => (decimal?)x.Total) ?? 0;
            ViewBag.OrdersMonthCount = db.Orders.Count(i => i.OrderStatus == Models.Enums.OrderStatus.Delivered && i.CreatedOn >= startOfMonth && i.CreatedOn <= endOfMonth);
            ViewBag.UsersCount = db.Users.Count();
            ViewBag.ItemsCount = db.Services.Count();
            ViewBag.NewUsersCount = db.Users.Count(i => i.CreatedOn >= startOfMonth && i.CreatedOn <= endOfMonth);
            //ViewBag.CategoriesCount = db.Categories.Count(i => !i.IsDeleted);
            ViewBag.CategoriesCount = db.SubCategories.Count(i => !i.IsDeleted);
            ViewBag.SubCategoriesCount = db.SubCategories.Count(i => !i.IsDeleted);

            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (UserManager.IsInRole(CurrentUserId, "Admin"))
                    return RedirectToAction("Index", "cp");

                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(cpLoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, false, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    CurrentUserId = UserManager.FindByEmail(model.Email).Id;
                    if (UserManager.IsInRole(CurrentUserId, "Admin"))
                        return RedirectToAction("index", "cp");

                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    return RedirectToAction("Index", "Home");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "محاولة دخول خاطئه");
                    return View(model);
            }
        }

        [HttpPost]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

    }
}