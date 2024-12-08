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
    [AdminAuthorizeAttribute(Roles = "Admin")]
    public class CountriesController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string q)
        {
            var Countries = db.Countries.ToList();
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                Countries = Countries.Where(x => x.IsDeleted == true).ToList();
            }
            else
            {
                Countries = Countries.Where(x => x.IsDeleted == false).ToList();
            }
            ViewBag.TimeZoneId = TimeZoneInfo.GetSystemTimeZones();
            return View(Countries);
        }

        [HttpPost]
        public ActionResult Create(Country country, HttpPostedFileBase Image)
        {
            List<string> Errors = new List<string>();
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (IsImage == false)
                {
                    Errors.Add("علم الدوله غير صحيح");
                }
            }

            if (Errors.Count() > 0)
            {
                TempData["CountryErrors"] = Errors;
                return RedirectToAction("Index");
            }

            try
            {
                if (Image != null)
                {
                    country.ImageUrl = MediaControl.Upload(FilePath.Country, Image);
                }
                db.Countries.Add(country);
                db.SaveChanges();
            }
            catch (Exception)
            {
                Errors.Add("حدث خطأ ما برجاء المحاولة لاحقاً");
                TempData["CountryErrors"] = Errors;
            }
            return RedirectToAction("Index");

        }

        [HttpGet]
        public ActionResult ToggleDelete(long? CountryId)
        {
            if (CountryId.HasValue == false)
                return RedirectToAction("Index");

            var Country = db.Countries.Find(CountryId);
            if (Country != null)
            {
                if (Country.IsDeleted == false)
                {
                    CRUD<Country>.Delete(Country);
                }
                else
                {
                    CRUD<Country>.Restore(Country);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(long? CountryId)
        {
            if (CountryId.HasValue == false)
                return RedirectToAction("Index");

            var Country = db.Countries.Find(CountryId);
            if (Country != null)
            {
                ViewBag.TimeZoneId = TimeZoneInfo.GetSystemTimeZones();
                return View(Country);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(Country country, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (IsImage == false)
                {
                    ModelState.AddModelError("", "علم الدولة غير صحيح");
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (Image != null)
                    {
                        if (country.ImageUrl != null)
                        {
                            MediaControl.Delete(FilePath.Country, country.ImageUrl);
                        }
                        country.ImageUrl = MediaControl.Upload(FilePath.Country, Image);
                    }
                    db.Entry(country).State = EntityState.Modified;
                    CRUD<Country>.Update(country);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "حدث خطأ ما برجاء المحاولة لاحقاً");
                    ViewBag.TimeZoneId = TimeZoneInfo.GetSystemTimeZones();
                    return View(country);
                }
            }
            ViewBag.TimeZoneId = TimeZoneInfo.GetSystemTimeZones();
            return View(country);

        }

    }
}