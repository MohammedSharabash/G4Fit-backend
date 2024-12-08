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
    [AdminAuthorizeAttribute(Roles = "Admin,SubAdmin")]
    public class CitiesController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string q, long? CountryId)
        {
            var Cities = db.Cities.ToList();
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                Cities = Cities.Where(x => x.IsDeleted == true).ToList();
            }
            else
            {
                Cities = Cities.Where(x => x.IsDeleted == false).ToList();
            }
            if (CountryId.HasValue == true)
            {
                var Country = db.Countries.Find(CountryId);
                if (Country == null)
                {
                    return RedirectToAction("Index", new { q });
                }
                ViewBag.Country = Country;
                Cities = Cities.Where(s => s.CountryId == CountryId).ToList();
            }
            ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
            return View(Cities);
        }

        [HttpPost]
        public ActionResult Create(City city)
        {
            List<string> Errors = new List<string>();
            try
            {
                db.Cities.Add(city);
                db.SaveChanges();
            }
            catch (Exception)
            {
                Errors.Add("حدث خطأ ما برجاء المحاولة لاحقاً");
                TempData["CountryErrors"] = Errors;
            }
            return RedirectToAction("Index", new { city.CountryId });
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? CityId)
        {
            if (CityId.HasValue == false)
                return RedirectToAction("Index");

            var City = db.Cities.Find(CityId);
            if (City != null)
            {
                if (City.IsDeleted == false)
                {
                    CRUD<City>.Delete(City);
                }
                else
                {
                    CRUD<City>.Restore(City);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(long? CityId)
        {
            if (CityId.HasValue == false)
                return RedirectToAction("Index");

            var City = db.Cities.Find(CityId);
            if (City != null)
            {
                ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
                return View(City);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(City city)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(city).State = EntityState.Modified;
                    CRUD<City>.Update(city);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "حدث خطأ ما برجاء المحاولة لاحقاً");
                    ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
                    return View(city);
                }
            }
            ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
            return View(city);
        }

    }
}