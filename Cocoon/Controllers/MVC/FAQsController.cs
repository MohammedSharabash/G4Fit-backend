using G4Fit.Helpers;
using G4Fit.Models.Domains;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class FAQsController : BaseController
    {
        [HttpGet]
        public ActionResult Dashboard(string q)
        {
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                ViewBag.FAQs = db.FAQs.Where(x => x.IsDeleted).ToList();
            }
            else
            {
                ViewBag.FAQs = db.FAQs.Where(x => !x.IsDeleted).ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Dashboard(FAQ faq)
        {
            if (ModelState.IsValid)
            {
                db.FAQs.Add(faq);
                db.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            ViewBag.FAQs = db.FAQs.Where(x => !x.IsDeleted).ToList();
            return View(faq);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
                return RedirectToAction("Dashboard");
            FAQ faq = db.FAQs.Find(id);
            if (faq == null)
                return RedirectToAction("Dashboard");
            return View(faq);
        }

        [HttpPost]
        public ActionResult Edit(FAQ faq)
        {
            if (ModelState.IsValid)
            {
                db.Entry(faq).State = EntityState.Modified;
                CRUD<FAQ>.Update(faq);
                db.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View(faq);
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? CatId)
        {
            if (CatId.HasValue == false)
                return RedirectToAction("Dashboard");

            var FAQ = db.FAQs.Find(CatId);
            if (FAQ != null)
            {
                if (FAQ.IsDeleted == false)
                {
                    CRUD<FAQ>.Delete(FAQ);
                }
                else
                {
                    CRUD<FAQ>.Restore(FAQ);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View(db.FAQs.Where(s => s.IsDeleted == false).ToList());
        }
    }
}