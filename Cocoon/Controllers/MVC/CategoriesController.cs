//using G4Fit.Helpers;
//using G4Fit.Models.Domains;
//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace G4Fit.Controllers.MVC
//{
//    [AdminAuthorizeAttribute(Roles = "Admin,SubAdmin")]
//    public class CategoriesController : BaseController
//    {
//        [HttpGet]
//        public ActionResult Index(string q)
//        {
//            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
//            {
//                ViewBag.Categories = db.Categories.Where(x => x.IsDeleted).ToList();
//            }
//            else
//            {
//                ViewBag.Categories = db.Categories.Where(x => !x.IsDeleted).ToList();
//            }
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Index(Category category, HttpPostedFileBase Image)
//        {
//            if (Image != null)
//            {
//                bool IsImage = CheckFiles.IsImage(Image);
//                if (!IsImage)
//                {
//                    ModelState.AddModelError("", "الصوره غير صحيحة");
//                }
//            }
//            if (ModelState.IsValid)
//            {
//                var LatestSortingNumber = db.Categories.Select(s => s.SortingNumber).DefaultIfEmpty(0).Max();
//                category.SortingNumber = LatestSortingNumber + 1;
//                if (Image != null)
//                {
//                    category.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
//                }
//                db.Categories.Add(category);
//                db.SaveChanges();
//                return RedirectToAction("Index");
//            }
//            ViewBag.Categories = db.Categories.Where(x => !x.IsDeleted).ToList();
//            return View(category);
//        }

//        public ActionResult Edit(long? id)
//        {
//            if (id == null)
//                return RedirectToAction("Index");
//            Category category = db.Categories.Find(id);
//            if (category == null)
//                return RedirectToAction("Index");
//            return View(category);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Edit(Category category, HttpPostedFileBase Image)
//        {
//            if (Image != null)
//            {
//                bool IsImage = CheckFiles.IsImage(Image);
//                if (!IsImage)
//                {
//                    ModelState.AddModelError("", "الصوره غير صحيحة");
//                }
//            }
//            if (ModelState.IsValid)
//            {
//                if (Image != null)
//                {
//                    if (category.ImageUrl != null)
//                    {
//                        MediaControl.Delete(FilePath.Category, category.ImageUrl);
//                    }
//                    category.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
//                }
//                db.Entry(category).State = EntityState.Modified;
//                CRUD<Category>.Update(category);
//                db.SaveChanges();
//                return RedirectToAction("Index");
//            }
//            return View(category);
//        }

//        [HttpGet]
//        public ActionResult ToggleDelete(long? CatId)
//        {
//            if (CatId.HasValue == false)
//                return RedirectToAction("Index");

//            var Category = db.Categories.Find(CatId);
//            if (Category != null)
//            {
//                if (Category.IsDeleted == false)
//                {
//                    CRUD<Category>.Delete(Category);
//                }
//                else
//                {
//                    CRUD<Category>.Restore(Category);
//                }
//                db.SaveChanges();
//            }
//            return RedirectToAction("Index");
//        }

//        [HttpGet]
//        [AllowAnonymous]
//        public ActionResult View(long? CatId, long? SubId)
//        {
//            if (CatId.HasValue == false)
//            {
//                return RedirectToAction("Index", "Home");
//            }
//            var Category = db.Categories.FirstOrDefault(s => s.IsDeleted == false && s.Id == CatId.Value);
//            if (Category == null)
//            {
//                return RedirectToAction("Index", "Home");
//            }
//            ViewBag.SubId = SubId;
//            return View(Category);
//        }

//        [HttpPost]
//        public JsonResult SetSortingNumber(long CatId, int Number)
//        {
//            var Category = db.Categories.Find(CatId);
//            if (Category == null)
//            {
//                return Json(new { Sucess = false, Message = "القسم المطلوب غير متوفر" }, JsonRequestBehavior.AllowGet);
//            }

//            var ExistingCategoryNumber = db.Categories.FirstOrDefault(w => w.Id != CatId && w.SortingNumber == Number);
//            if (ExistingCategoryNumber != null)
//            {
//                return Json(new { Sucess = false, Message = $"القسم {ExistingCategoryNumber.NameAr} له نفس الترتيب" }, JsonRequestBehavior.AllowGet);
//            }

//            Category.SortingNumber = Number;
//            CRUD<Category>.Update(Category);
//            db.SaveChanges();
//            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
//        }
//    }
//}