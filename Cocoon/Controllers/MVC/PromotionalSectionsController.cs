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
    public class PromotionalSectionsController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string q)
        {
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                ViewBag.PromotionalSections = db.PromotionalSections.Where(x => x.IsDeleted).ToList();
            }
            else
            {
                ViewBag.PromotionalSections = db.PromotionalSections.Where(x => !x.IsDeleted).ToList();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(PromotionalSection promotionalSection)
        {
            if (ModelState.IsValid)
            {
                var LatestSortingNumber = db.PromotionalSections.Select(s => s.SortingNumber).DefaultIfEmpty(0).Max();
                promotionalSection.SortingNumber = LatestSortingNumber + 1;
                db.PromotionalSections.Add(promotionalSection);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PromotionalSections = db.PromotionalSections.Where(x => !x.IsDeleted).ToList();
            return View(promotionalSection);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            PromotionalSection promotionalSection = db.PromotionalSections.Find(id);
            if (promotionalSection == null)
                return RedirectToAction("Index");
            return View(promotionalSection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PromotionalSection promotionalSection)
        {
            if (ModelState.IsValid)
            {
                db.Entry(promotionalSection).State = EntityState.Modified;
                CRUD<PromotionalSection>.Update(promotionalSection);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(promotionalSection);
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? CatId)
        {
            if (CatId.HasValue == false)
                return RedirectToAction("Index");

            var PromotionalSection = db.PromotionalSections.Find(CatId);
            if (PromotionalSection != null)
            {
                if (PromotionalSection.IsDeleted == false)
                {
                    CRUD<PromotionalSection>.Delete(PromotionalSection);
                }
                else
                {
                    CRUD<PromotionalSection>.Restore(PromotionalSection);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult SetSortingNumber(long CatId, int Number)
        {
            var PromotionalSection = db.PromotionalSections.Find(CatId);
            if (PromotionalSection == null)
            {
                return Json(new { Sucess = false, Message = "القسم المطلوب غير متوفر" }, JsonRequestBehavior.AllowGet);
            }

            var ExistingPromotionalSectionNumber = db.PromotionalSections.FirstOrDefault(w => w.Id != CatId && w.SortingNumber == Number);
            if (ExistingPromotionalSectionNumber != null)
            {
                return Json(new { Sucess = false, Message = $"القسم {ExistingPromotionalSectionNumber.NameAr} له نفس الترتيب" }, JsonRequestBehavior.AllowGet);
            }

            PromotionalSection.SortingNumber = Number;
            CRUD<PromotionalSection>.Update(PromotionalSection);
            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Services(long? CatId)
        {
            if (CatId.HasValue == false)
                return RedirectToAction("Index");

            var PromotionalSection = db.PromotionalSections.Find(CatId);
            if (PromotionalSection != null)
            {
                ViewBag.Services = db.Services.Where(w => w.IsDeleted == false && w.IsHidden == false && w.SubCategory.IsDeleted == false && /*w.SubCategory.Category.IsDeleted == false &&*/ w.PromotionalSections.Any(s => s.IsDeleted == false && s.ServiceId == w.Id) == false).ToList();
                return View(PromotionalSection);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddServices(long[] ServiceIds, long SectionId, string ReturnUrl)
        {
            if (ServiceIds != null)
            {
                foreach (var Service in ServiceIds)
                {
                    var SectionService = db.PromotionalSectionServices.FirstOrDefault(w => w.ServiceId == Service && w.PromotionalSectionId == SectionId);
                    if (SectionService == null)
                    {
                        PromotionalSectionService prod = new PromotionalSectionService()
                        {
                            ServiceId = Service,
                            PromotionalSectionId = SectionId,
                        };
                        db.PromotionalSectionServices.Add(prod);
                    }
                    else
                    {
                        if (SectionService.IsDeleted == true)
                        {
                            CRUD<PromotionalSectionService>.Restore(SectionService);
                        }
                    }
                }
                db.SaveChanges();
            }
            if (ReturnUrl != null)
            {
                return Redirect(ReturnUrl);
            }
            else
            {
                return RedirectToAction("Services", new { CatId = SectionId });
            }
        }

        [HttpGet]
        public ActionResult DeleteService(long? Id, string ReturnUrl)
        {
            if (Id.HasValue == true)
            {
                var Section = db.PromotionalSectionServices.Find(Id);
                if (Section != null)
                {
                    CRUD<PromotionalSectionService>.Delete(Section);
                    db.SaveChanges();
                    if (ReturnUrl != null)
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Services", new { CatId = Section.PromotionalSectionId });
                    }
                }
            }
            return RedirectToAction("Index");
        }
    }
}