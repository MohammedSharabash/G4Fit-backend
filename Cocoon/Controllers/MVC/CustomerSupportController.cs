using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [AdminAuthorizeAttribute(Roles = "Admin")]
    public class CustomerSupportController : BaseController
    {
        public ActionResult Dashboard()
        {
            var Content = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            if (Content != null)
            {
                HTMLContentVM contentVM = new HTMLContentVM()
                {
                    ContentAr = Content.CustomerServiceAr,
                    ContentEn = Content.CustomerServiceEn,
                };
                return View(contentVM);
            }
            return View();
        }

        [HttpPost]
        public ActionResult Dashboard(HTMLContentVM content)
        {
            try
            {
                var Data = db.CompanyDatas.FirstOrDefault(d => d.IsDeleted == false);
                if (Data != null)
                {
                    Data.CustomerServiceAr = content.ContentAr;
                    Data.CustomerServiceEn = content.ContentEn;
                    CRUD<CompanyData>.Update(Data);
                    db.SaveChanges();
                }
                else
                {
                    CompanyData CompanyData = new CompanyData()
                    {
                        CustomerServiceAr = content.ContentAr,
                        CustomerServiceEn = content.ContentEn
                    };
                    db.CompanyDatas.Add(CompanyData);
                    db.SaveChanges();
                }
                TempData["Success"] = true;
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Dashboard");
            }
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View(db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false));
        }
    }
}