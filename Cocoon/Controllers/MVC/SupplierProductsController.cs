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
    [Authorize(Roles = "Supplier")]
    public class SupplierServicesController : BaseController
    {
        [HttpGet]
        public ActionResult Dashboard(long? CatId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (CatId.HasValue == true)
            {
                return View(db.Services.Where(s => s.SupplierId == Supplier.Id && s.SubCategoryId == CatId.Value).OrderByDescending(s => s.CreatedOn).ToList());
            }
            else
            {
                return View(db.Services.Where(s => s.SupplierId == Supplier.Id).OrderByDescending(s => s.CreatedOn).ToList());
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Categories = db.SubCategories.Where(d => d.IsDeleted == false /*&& d.Category.IsDeleted == false*/).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateServiceVM model)
        {
            var Errors = ValidateNewService(ModelState, model);
            if (Errors != null && Errors.Count() > 0)
                return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);

            if (ModelState.IsValid == false)
            {
                Errors.Add("حدث خطأ ما ، برجاء المحاوله لاحقاً");
                return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
                if (Supplier == null)
                {
                    Errors.Add("برجاء اعاده تحميل الصفحه");
                    return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);
                }

                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var LatestSortingNumber = db.Services.Select(s => s.SortingNumber).DefaultIfEmpty(0).Max();
                        Service Service = new Service()
                        {
                            DescriptionAr = model.DescriptionAr,
                            DescriptionEn = model.DescriptionEn,
                            NameAr = model.NameAr,
                            NameEn = model.NameEn,
                            OriginalPrice = model.Price,
                            SubCategoryId = model.CategoryId,
                            SortingNumber = LatestSortingNumber + 1,
                            SupplierId = Supplier.Id
                        };

                        db.Services.Add(Service);

                        if (model.Trainers != null && model.Trainers.Count() > 0)
                        {
                            foreach (var color in model.Trainers.Where(s => !string.IsNullOrEmpty(s)).Distinct())
                            {
                                db.ServiceTrainers.Add(new ServiceColor()
                                {
                                    Color = color,
                                    ServiceId = Service.Id,
                                });
                            }
                        }

                        if (model.Sizes != null && model.Sizes.Count() > 0)
                        {
                            foreach (var size in model.Sizes.Where(s => !string.IsNullOrEmpty(s)).Distinct())
                            {
                                db.ServiceSizes.Add(new ServiceSize()
                                {
                                    Size = size,
                                    ServiceId = Service.Id,
                                });
                            }
                        }

                        if (model.Images != null && model.Images.Count(d => d != null) > 0)
                        {
                            foreach (var image in model.Images.Where(d => d != null))
                            {
                                db.ServiceImages.Add(new ServiceImage()
                                {
                                    ImageUrl = MediaControl.Upload(FilePath.Service, image),
                                    ServiceId = Service.Id
                                });
                            }
                        }

                        db.SaveChanges();
                        Transaction.Commit();
                        return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception)
                    {
                        Transaction.Rollback();
                        Errors.Add("حدث خطأ ما ، برجاء المحاوله لاحقاً");
                        return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult Edit(long? ServiceId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.FirstOrDefault(s => s.SupplierId == Supplier.Id && s.Id == ServiceId.Value);
            if (Service == null)
                return RedirectToAction("Dashboard");

            var ServiceVM = EditServiceVM.ToEditServiceVM(Service);
            ViewBag.Service = Service;
            ViewBag.Categories = db.SubCategories.Where(d => d.IsDeleted == false /*&& d.Category.IsDeleted == false*/).ToList();
            return View(ServiceVM);
        }


        [HttpPost]
        public ActionResult Edit(EditServiceVM model)
        {
            var Errors = ValidateEditService(ModelState, model);
            if (Errors != null && Errors.Count() > 0)
                return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);

            if (ModelState.IsValid == false)
            {
                Errors.Add("حدث خطأ ما ، برجاء المحاوله لاحقاً");
                return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var Service = db.Services.FirstOrDefault(s => s.Id == model.ServiceId);
                        Service.DescriptionAr = model.DescriptionAr;
                        Service.DescriptionEn = model.DescriptionEn;
                        Service.NameAr = model.NameAr;
                        Service.NameEn = model.NameEn;
                        Service.OriginalPrice = model.Price;
                        Service.SubCategoryId = model.CategoryId;
                        CRUD<Service>.Update(Service);

                        if (Service.Offers != null)
                        {
                            var Offer = Service.Offers.FirstOrDefault(s => s.IsDeleted == false && s.IsFinished == false);
                            if (Offer != null)
                            {
                                Offer.OriginalPrice = Service.OriginalPrice;
                                Offer.AfterPrice = Service.OriginalPrice - Service.OriginalPrice * Offer.Percentage / 100;
                                CRUD<ServiceOffer>.Update(Offer);
                            }
                        }

                        if (model.Trainers != null && model.Trainers.Count() > 0)
                        {
                            foreach (var color in model.Trainers.Where(s => !string.IsNullOrEmpty(s)).Distinct())
                            {
                                var ProdColor = Service.Colors.FirstOrDefault(s => s.Color == color);
                                if (ProdColor != null)
                                {
                                    CRUD<ServiceColor>.Restore(ProdColor);
                                }
                                else
                                {
                                    db.ServiceTrainers.Add(new ServiceColor()
                                    {
                                        Color = color,
                                        ServiceId = Service.Id,
                                    });
                                }
                            }
                        }

                        if (model.Sizes != null && model.Sizes.Count() > 0)
                        {
                            foreach (var size in model.Sizes.Where(s => !string.IsNullOrEmpty(s)).Distinct())
                            {
                                var ProdSize = Service.Sizes.FirstOrDefault(s => s.Size == size);
                                if (ProdSize != null)
                                {
                                    CRUD<ServiceSize>.Restore(ProdSize);
                                }
                                else
                                {
                                    db.ServiceSizes.Add(new ServiceSize()
                                    {
                                        Size = size,
                                        ServiceId = Service.Id,
                                    });
                                }
                            }
                        }

                        if (model.NewImages != null && model.NewImages.Count(d => d != null) > 0)
                        {
                            foreach (var image in model.NewImages.Where(d => d != null))
                            {
                                db.ServiceImages.Add(new ServiceImage()
                                {
                                    ImageUrl = MediaControl.Upload(FilePath.Service, image),
                                    ServiceId = Service.Id
                                });
                            }
                        }

                        db.SaveChanges();
                        Transaction.Commit();
                        return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception)
                    {
                        Transaction.Rollback();
                        Errors.Add("حدث خطأ ما ، برجاء المحاوله لاحقاً");
                        return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }


        private List<string> ValidateEditService(ModelStateDictionary ModelState, EditServiceVM model)
        {
            List<string> Errors = new List<string>();
            var ModelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            if (ModelErrors != null && ModelErrors.Count > 0)
            {
                foreach (var error in ModelErrors)
                {
                    Errors.Add(error);
                }
            }

            var Service = db.Services.FirstOrDefault(s => s.Id == model.ServiceId);
            if (Service == null)
            {
                Errors.Add("الخدمه المطلوب غير صحيح");
            }

            var Category = db.SubCategories.FirstOrDefault(d => /*d.Category.IsDeleted == false &&*/ d.IsDeleted == false && d.Id == model.CategoryId);
            if (Category == null)
            {
                Errors.Add("القسم الرئيسى المطلوب غير متاح");
            }

            if (model.NewImages != null)
            {
                foreach (var image in model.NewImages.Where(d => d != null))
                {
                    if (CheckFiles.IsImage(image) == false)
                    {
                        Errors.Add("احدى الصور المرفوعه غير صحيحه");
                        break;
                    }
                }
            }

            if (model.Price < 0)
            {
                Errors.Add("سعر الخدمه غير صحيح");
            }

            return Errors;
        }


        private List<string> ValidateNewService(ModelStateDictionary ModelState, CreateServiceVM model)
        {
            List<string> Errors = new List<string>();
            var ModelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            if (ModelErrors != null && ModelErrors.Count > 0)
            {
                foreach (var error in ModelErrors)
                {
                    Errors.Add(error);
                }
            }

            var Category = db.SubCategories.FirstOrDefault(d => /*d.Category.IsDeleted == false &&*/ d.IsDeleted == false && d.Id == model.CategoryId);
            if (Category == null)
            {
                Errors.Add("القسم المطلوب غير متاح");
            }

            if (model.Images != null)
            {
                foreach (var image in model.Images.Where(d => d != null))
                {
                    if (CheckFiles.IsImage(image) == false)
                    {
                        Errors.Add("احدى الصور المرفوعه غير صحيحه");
                        break;
                    }
                }
            }

            if (model.Price < 0)
            {
                Errors.Add("سعر الخدمه غير صحيح");
            }

            return Errors;
        }

        [HttpGet]
        public ActionResult ToggleHide(long? ServiceId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.FirstOrDefault(s => s.Id == ServiceId.Value && s.SupplierId == Supplier.Id);
            if (Service == null)
                return RedirectToAction("Dashboard");

            if (Service.IsHidden == true)
            {
                Service.IsHidden = false;
                CRUD<Service>.Update(Service);
            }
            else
            {
                Service.IsHidden = true;
                CRUD<Service>.Update(Service);
            }
            db.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? ServiceId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.FirstOrDefault(s => s.Id == ServiceId.Value && s.SupplierId == Supplier.Id);
            if (Service == null)
                return RedirectToAction("Dashboard");

            if (Service.IsDeleted == true)
            {
                Service.IsDeleted = false;
                CRUD<Service>.Restore(Service);
            }
            else
            {
                Service.IsDeleted = true;
                CRUD<Service>.Delete(Service);
            }
            db.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        public ActionResult Details(long? ServiceId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.FirstOrDefault(s => s.Id == ServiceId.Value && s.SupplierId == Supplier.Id);
            if (Service == null)
                return RedirectToAction("Dashboard");

            return View(Service);
        }

        [HttpGet]
        public JsonResult DeleteColor(long ColorId)
        {
            var Color = db.ServiceTrainers.Find(ColorId);
            if (Color == null)
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
            else
            {
                CRUD<ServiceColor>.Delete(Color);
                db.SaveChanges();
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DeleteSize(long SizeId)
        {
            var Size = db.ServiceSizes.Find(SizeId);
            if (Size == null)
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
            else
            {
                CRUD<ServiceSize>.Delete(Size);
                db.SaveChanges();
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DeleteImage(long ImageId)
        {
            var Image = db.ServiceImages.Find(ImageId);
            if (Image == null)
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
            else
            {
                MediaControl.Delete(FilePath.Service, Image.ImageUrl);
                CRUD<ServiceImage>.Delete(Image);
                db.SaveChanges();
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Offers(long? ServiceId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.FirstOrDefault(s => s.Id == ServiceId.Value && s.SupplierId == Supplier.Id);
            if (Service == null)
                return RedirectToAction("Dashboard");

            return View(Service);
        }

        [HttpGet]
        public ActionResult CreateOffer(long? ServiceId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.FirstOrDefault(s => s.Id == ServiceId.Value && s.SupplierId == Supplier.Id);
            if (Service == null)
                return RedirectToAction("Dashboard");

            if (Service.Offers != null && Service.Offers.Any(s => s.IsDeleted == false && s.IsFinished == false))
            {
                return RedirectToAction("Offers", new { ServiceId });
            }
            ViewBag.Service = Service;
            return View();
        }

        [HttpPost]
        public ActionResult CreateOffer(ServiceOfferVM offerVM)
        {
            var Service = db.Services.Find(offerVM.ServiceId);
            if (offerVM.FinishOn.HasValue == true)
            {
                var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                if (offerVM.FinishOn.Value.Date < DateTimeNow.Date)
                {
                    ModelState.AddModelError("FinishOn", "تاريخ النهاية قد تخطى");
                }
            }
            if (ModelState.IsValid == true)
            {
                db.ServiceOffers.Add(new ServiceOffer()
                {
                    NumberOfUse = 0,
                    //Percentage = offerVM.OfferPrice ,
                    ServiceId = offerVM.ServiceId.Value,
                    OriginalPrice = Service.OriginalPrice,
                    AfterPrice = Service.OriginalPrice - Service.OriginalPrice * offerVM.OfferPrice  / 100,
                    FinishOn = offerVM.FinishOn
                });
                Service.OfferPrice = Service.OriginalPrice - Service.OriginalPrice * offerVM.OfferPrice  / 100;
                db.SaveChanges();
                return RedirectToAction("Offers", new { offerVM.ServiceId });
            }
            ViewBag.Service = Service;
            return View(offerVM);
        }

        [HttpGet]
        public ActionResult EditOffer(long? OfferId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (OfferId.HasValue == false)
                return RedirectToAction("Dashboard");

            var ServiceOffer = db.ServiceOffers.FirstOrDefault(x => x.Id == OfferId.Value && x.Service.SupplierId == Supplier.Id);
            if (ServiceOffer == null)
                return RedirectToAction("Dashboard");

            if (ServiceOffer.IsFinished == true)
                return RedirectToAction("Offers", new { ServiceOffer.ServiceId });

            ServiceOfferVM ServiceOfferVM = new ServiceOfferVM()
            {
                FinishOn = ServiceOffer.FinishOn,
                OfferId = ServiceOffer.Id,
                OfferPrice  = ServiceOffer.Percentage,
                ServiceId = ServiceOffer.ServiceId
            };
            return View(ServiceOfferVM);
        }

        [HttpPost]
        public ActionResult EditOffer(ServiceOfferVM offerVM)
        {
            var Service = db.Services.Find(offerVM.ServiceId);
            if (offerVM.FinishOn.HasValue == true)
            {
                var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                if (offerVM.FinishOn.Value.Date < DateTimeNow.Date)
                {
                    ModelState.AddModelError("FinishOn", "تاريخ النهاية قد تخطى");
                }
            }
            if (ModelState.IsValid == true)
            {
                var Offer = db.ServiceOffers.Find(offerVM.OfferId);
                if (Offer != null)
                {
                    Service.OfferPrice = Service.OriginalPrice - Service.OriginalPrice * offerVM.OfferPrice  / 100;
                    //Offer.Percentage = offerVM.OfferPrice ;
                    Offer.OriginalPrice = Service.OriginalPrice;
                    Offer.AfterPrice = Service.OriginalPrice - Service.OriginalPrice * offerVM.OfferPrice  / 100;
                    Offer.FinishOn = offerVM.FinishOn;
                    CRUD<ServiceOffer>.Update(Offer);
                    db.SaveChanges();
                }
                db.SaveChanges();
                return RedirectToAction("Offers", new { offerVM.ServiceId });
            }
            return View(offerVM);
        }

        [HttpGet]
        public ActionResult ToggleDeleteOffer(long? OfferId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (OfferId.HasValue == false)
                return RedirectToAction("Dashboard");

            var ServiceOffer = db.ServiceOffers.FirstOrDefault(x => x.Id == OfferId.Value && x.Service.SupplierId == Supplier.Id);
            if (ServiceOffer == null)
                return RedirectToAction("Dashboard");

            if (ServiceOffer.IsDeleted == true)
            {
                ServiceOffer.Service.OfferPrice = ServiceOffer.AfterPrice;
                CRUD<ServiceOffer>.Restore(ServiceOffer);
            }
            else
            {
                ServiceOffer.Service.OfferPrice = null;
                CRUD<ServiceOffer>.Delete(ServiceOffer);
            }
            db.SaveChanges();
            return RedirectToAction("Offers", new { ServiceOffer.ServiceId });
        }

        [HttpGet]
        public ActionResult FinishOffer(long? OfferId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (OfferId.HasValue == false)
                return RedirectToAction("Dashboard");

            var ServiceOffer = db.ServiceOffers.FirstOrDefault(x => x.Id == OfferId.Value && x.Service.SupplierId == Supplier.Id);
            if (ServiceOffer == null)
                return RedirectToAction("Dashboard");

            ServiceOffer.Service.OfferPrice = null;
            ServiceOffer.IsFinished = true;
            CRUD<ServiceOffer>.Update(ServiceOffer);
            db.SaveChanges();
            return RedirectToAction("Offers", new { ServiceOffer.ServiceId });
        }
    }
}