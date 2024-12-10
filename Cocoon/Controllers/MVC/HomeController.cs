using G4Fit.Helpers;
using G4Fit.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            HomeVM homeVM = new HomeVM();
            //homeVM.Categories = db.Categories.Where(s => s.IsDeleted == false).OrderBy(s => s.Id).ToList();
            homeVM.Categories = db.SubCategories.Where(s => !s.HardDelete && s.IsDeleted == false).OrderBy(s => s.SortingNumber).ToList();
            homeVM.Sliders = db.Sliders.Where(s => s.IsDeleted == false).OrderByDescending(s => s.CreatedOn).ToList();
            homeVM.LatestServices = db.Services.Where(s => !s.HardDelete && s.IsDeleted == false && s.IsHidden == false && (s.Inventory > 0 || s.IsTimeBoundService) && s.SubCategory.IsDeleted == false /*&& s.SubCategory.Category.IsDeleted == false*/).OrderByDescending(s => s.CreatedOn).Take(9).ToList();
            homeVM.MostSoldServices = db.Services.Where(s => s.IsDeleted == false && s.IsHidden == false && (s.Inventory > 0 || s.IsTimeBoundService) && s.SubCategory.IsDeleted == false /*&& s.SubCategory.Category.IsDeleted == false*/).OrderByDescending(s => s.SellCounter).Take(9).ToList();
            homeVM.OffersServices = db.Services.Where(s => s.IsDeleted == false && s.IsHidden == false && (s.Inventory > 0 || s.IsTimeBoundService) && s.SubCategory.IsDeleted == false && /*s.SubCategory.Category.IsDeleted == false &&*/ s.OfferPrice.HasValue == true).OrderByDescending(s => s.CreatedOn).Take(9).ToList();
            homeVM.About = db.CompanyDatas.FirstOrDefault(s => s.IsDeleted == false);
            return View(homeVM);
        }

        public ActionResult About()
        {
            return View();
        }
        public ActionResult SetCulture(string culture, string ReturnUrl)
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            // Save culture in a cookie
            HttpCookie cookie = Request.Cookies["_G4FitCulture"];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddYears(-100);
                cookie = new HttpCookie("_G4FitCulture");
                cookie.Value = culture;
            }
            else
            {
                cookie = new HttpCookie("_G4FitCulture");
                cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            Response.Cookies.Add(cookie);
            return Redirect(ReturnUrl);
        }
    }
}