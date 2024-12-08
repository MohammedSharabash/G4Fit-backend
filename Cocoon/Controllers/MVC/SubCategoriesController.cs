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
    public class SubCategoriesController : BaseController
    {
        [HttpGet]
        public ActionResult Index(long? CatId, string q)
        {
            List<SubCategory> SubCategories = new List<SubCategory>();
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                SubCategories = db.SubCategories.Where(x => x.IsDeleted).ToList();
            }
            else
            {
                SubCategories = db.SubCategories.Where(x => !x.IsDeleted).ToList();
            }
            if (CatId.HasValue == true)
            {
                SubCategories = SubCategories.Where(w => /*w.SubCategoryId == CatId*/ true).ToList();
            }
            ViewBag.SubCategories = SubCategories;
            //ViewBag.Categories = db.Categories.Where(w => w.IsDeleted == false).OrderBy(w => w.NameAr).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SubCategory subCategory, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (!IsImage)
                {
                    ModelState.AddModelError("", "الصوره غير صحيحة");
                }
            }
            if (ModelState.IsValid)
            {
                var LatestSortingNumber = db.SubCategories.Select(s => s.SortingNumber).DefaultIfEmpty(0).Max();
                subCategory.SortingNumber = LatestSortingNumber + 1;
                if (Image != null)
                {
                    subCategory.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
                }
                db.SubCategories.Add(subCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SubCategories = db.SubCategories.Where(x => !x.IsDeleted).ToList();
            //ViewBag.Categories = db.Categories.Where(w => w.IsDeleted == false).OrderBy(w => w.NameAr).ToList();
            return View(subCategory);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            SubCategory subCategory = db.SubCategories.Find(id);
            if (subCategory == null)
                return RedirectToAction("Index");
            //ViewBag.Categories = db.Categories.Where(w => w.IsDeleted == false).OrderBy(w => w.NameAr).ToList();
            return View(subCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SubCategory subCategory, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (!IsImage)
                {
                    ModelState.AddModelError("", "الصوره غير صحيحة");
                }
            }
            if (ModelState.IsValid)
            {
                if (Image != null)
                {
                    if (subCategory.ImageUrl != null)
                    {
                        MediaControl.Delete(FilePath.Category, subCategory.ImageUrl);
                    }
                    subCategory.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
                }
                db.Entry(subCategory).State = EntityState.Modified;
                CRUD<SubCategory>.Update(subCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.Categories = db.Categories.Where(w => w.IsDeleted == false).OrderBy(w => w.NameAr).ToList();
            return View(subCategory);
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? CatId)
        {
            if (CatId.HasValue == false)
                return RedirectToAction("Index");

            var SubCategory = db.SubCategories.Find(CatId);
            if (SubCategory != null)
            {
                if (SubCategory.IsDeleted == false)
                {
                    CRUD<SubCategory>.Delete(SubCategory);
                }
                else
                {
                    CRUD<SubCategory>.Restore(SubCategory);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult View(long? CatId)
        {
            if (CatId.HasValue == false)
            {
                return RedirectToAction("Index", "Home");
            }
            var Category = db.SubCategories.FirstOrDefault(s => s.IsDeleted == false && s.Id == CatId.Value);
            if (Category == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(Category);
        }

        [HttpPost]
        public JsonResult SetSortingNumber(long CatId, int Number)
        {
            var SubCategory = db.SubCategories.Find(CatId);
            if (SubCategory == null)
            {
                return Json(new { Sucess = false, Message = "القسم المطلوب غير متوفر" }, JsonRequestBehavior.AllowGet);
            }

            var ExistingSubCategoryNumber = db.SubCategories.FirstOrDefault(w => w.Id != CatId && w.SortingNumber == Number);
            if (ExistingSubCategoryNumber != null)
            {
                return Json(new { Sucess = false, Message = $"القسم {ExistingSubCategoryNumber.NameAr} له نفس الترتيب" }, JsonRequestBehavior.AllowGet);
            }

            SubCategory.SortingNumber = Number;
            CRUD<SubCategory>.Update(SubCategory);
            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}