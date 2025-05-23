﻿using G4Fit.Helpers;
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
    public class HowToController : BaseController
    {
        public ActionResult Dashboard()
        {
            var Content = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            if (Content != null)
            {
                HTMLContentVM contentVM = new HTMLContentVM()
                {
                    ContentAr = Content.HowToOrderAr,
                    ContentEn = Content.HowToOrderEn,
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
                    Data.HowToOrderAr = content.ContentAr;
                    Data.HowToOrderEn = content.ContentEn;
                    CRUD<CompanyData>.Update(Data);
                    db.SaveChanges();
                }
                else
                {
                    CompanyData CompanyData = new CompanyData()
                    {
                        HowToOrderAr = content.ContentAr,
                        HowToOrderEn = content.ContentEn
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