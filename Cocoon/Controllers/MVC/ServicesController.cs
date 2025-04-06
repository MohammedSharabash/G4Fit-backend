using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Web.Hosting;
using ExcelDataReader;
using System.Data.Entity.Validation;

namespace G4Fit.Controllers.MVC
{
    [AdminAuthorizeAttribute(Roles = "Admin")]
    public class ServicesController : BaseController
    {
        [HttpGet]
        public ActionResult Dashboard(long? CatId)
        {
            if (CatId.HasValue == true)
            {
                return View(db.Services.Where(s => !s.HardDelete && s.SupplierId.HasValue == false && s.SubCategoryId == CatId.Value).OrderByDescending(s => s.CreatedOn).ToList());
            }
            else
            {
                return View(db.Services.Where(s => !s.HardDelete && s.SupplierId.HasValue == false).OrderByDescending(s => s.CreatedOn).ToList());
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Categories = db.SubCategories.Where(d => d.IsDeleted == false/* && d.Category.IsDeleted == false*/).ToList();
            var model = new CreateServiceVM()
            {
                ServiceDays = 1
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CreateServiceVM model)
        {
            if (model.IsTimeBoundService) model.Inventory = 0;
            else model.ServiceDays = 0;

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
                            SpecialPrice = model.SpecialPrice,
                            IsTimeBoundService = model.IsTimeBoundService,
                            Inventory = model.Inventory,
                            InBodyCount = model.InBodyCount,
                            ServiceDays = model.ServiceDays,
                            ServiceFreezingTimes = model.ServiceFreezingTimes,
                            ServiceFreezingDays = model.ServiceFreezingDays,
                            SubCategoryId = model.CategoryId,
                            HardDelete = false,
                            IsHidden = false,
                            SortingNumber = LatestSortingNumber + 1
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
            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.FirstOrDefault(s => s.SupplierId.HasValue == false && s.Id == ServiceId.Value);
            if (Service == null || Service.HardDelete)
                return RedirectToAction("Dashboard");

            var ServiceVM = EditServiceVM.ToEditServiceVM(Service);
            ViewBag.Service = Service;
            ViewBag.Categories = db.SubCategories.Where(d => d.IsDeleted == false /*&& d.Category.IsDeleted == false*/).ToList();
            return View(ServiceVM);
        }


        [HttpPost]
        public ActionResult Edit(EditServiceVM model)
        {
            if (model.IsTimeBoundService) model.Inventory = 0;
            else model.ServiceDays = 0;

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
                        Service.IsTimeBoundService = model.IsTimeBoundService;
                        Service.OriginalPrice = model.Price;
                        Service.SpecialPrice = model.SpecialPrice;
                        Service.Inventory = model.Inventory;
                        Service.ServiceDays = model.ServiceDays;
                        Service.ServiceFreezingTimes = model.ServiceFreezingTimes;
                        Service.ServiceFreezingDays = model.ServiceFreezingDays;
                        Service.InBodyCount = model.InBodyCount;
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
                            foreach (var trainer in model.Trainers.Where(s => !string.IsNullOrEmpty(s)).Distinct())
                            {
                                var Prodtrainer = Service.Colors.FirstOrDefault(s => s.Color == trainer);
                                if (Prodtrainer != null)
                                {
                                    CRUD<ServiceColor>.Restore(Prodtrainer);
                                }
                                else
                                {
                                    db.ServiceTrainers.Add(new ServiceColor()
                                    {
                                        Color = trainer,
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


        /// <summary>
        public ActionResult CreateServices()
        {
            ViewBag.Categories = new SelectList(db.SubCategories.Where(d => d.IsDeleted == false /*&& d.Category.IsDeleted == false*/).ToList(), "Id", "NameAr");
            return View();
        }
        public ActionResult DownloadFile()
        {
            string webRootPath = Server.MapPath("~/Content"); // Update the path accordingly
            string fileName = "ServicesDetails.xlsx";
            string filePath = Path.Combine(webRootPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                // Handle the case where the file doesn't exist
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult CreateServices(ServicesVM model)
        {
            if (model.file != null)
            {
                string extension = System.IO.Path.GetExtension(model.file.FileName);
                if (extension == ".xls" || extension == ".xlsx")
                {   /////Upload The Excel File (Services`s Data)
                    var filename = System.IO.Path.GetFullPath("~/Files/UsersExcel/") + model.file.FileName;

                    string webRootPath = HostingEnvironment.MapPath("~/Content");
                    string path = Path.Combine(webRootPath, "Services.xlsx");

                    if (model.file != null && model.file.ContentLength > 0)
                    {
                        // Use SaveAs to copy the file synchronously
                        model.file.SaveAs(path);
                    }
                    List<Service> Services = new List<Service>();
                    string fileRelativePath = "\\Content\\Services.xlsx";
                    string fileServerPath = HostingEnvironment.MapPath("~" + fileRelativePath);
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = System.IO.File.Open(fileServerPath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            //declare a variable to skip reading the first row in the excel sheet
                            int INDEX = 0;
                            while (reader.Read())
                            {
                                if (INDEX >= 1)
                                {
                                    try
                                    {
                                        ////// Check validation
                                        if (reader.GetValue(0) == null || reader.GetValue(1) == null || reader.GetValue(2) == null || reader.GetValue(3) == null || reader.GetValue(4) == null || reader.GetValue(5) == null || reader.GetValue(6) == null)
                                        {
                                            goto skip;
                                        }


                                        var ServiceVM = new CreateServiceVM()
                                        {
                                            CategoryId = Convert.ToInt32(reader.GetValue(0)),
                                            NameAr = reader.GetValue(1).ToString(),
                                            NameEn = reader.GetValue(2).ToString(),
                                            DescriptionAr = reader.GetValue(3).ToString(),
                                            DescriptionEn = reader.GetValue(4).ToString(),
                                            Inventory = Convert.ToInt32(reader.GetValue(5)),
                                            Price = Convert.ToInt32(reader.GetValue(6)),
                                            IsTimeBoundService = false,
                                            ServiceDays = 0,
                                        };
                                        var Errors = ValidateNewService(ModelState, ServiceVM);
                                        if (Errors != null && Errors.Count() > 0)
                                            goto skip;

                                        /////

                                        //new Service 


                                        var LatestSortingNumber = db.Services.Select(s => s.SortingNumber).DefaultIfEmpty(0).Max();
                                        Service NewService = new Service()
                                        {
                                            DescriptionAr = ServiceVM.DescriptionAr,
                                            DescriptionEn = ServiceVM.DescriptionEn,
                                            NameAr = ServiceVM.NameAr,
                                            NameEn = ServiceVM.NameEn,
                                            OriginalPrice = ServiceVM.Price,
                                            Inventory = ServiceVM.Inventory,
                                            SubCategoryId = ServiceVM.CategoryId,
                                            IsHidden = false,
                                            HardDelete = false,
                                            SortingNumber = LatestSortingNumber + 1,
                                            IsTimeBoundService = false,
                                            ServiceDays = 0,
                                        };

                                        Services.Add(NewService);

                                    }
                                    catch
                                    {
                                        //  return BadRequest("يوجد خطا في احد بيانات العملاء , من فضلك حاول لاحقاً");
                                        INDEX--;
                                        goto skip;
                                    }
                                    //////////////////////////////
                                    ///
                                    var Service = Services[INDEX - 1];

                                    db.Services.Add(Service);
                                    try
                                    {
                                        db.SaveChanges();
                                    }
                                    catch (DbEntityValidationException ex)
                                    {
                                        foreach (var validationErrors in ex.EntityValidationErrors)
                                        {
                                            foreach (var validationError in validationErrors.ValidationErrors)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                                            }
                                        }

                                        // Handle the exception or rethrow as needed
                                        throw;
                                    }

                                }

                            //////////
                            skip:
                                INDEX++;
                            }


                            return RedirectToAction(nameof(Dashboard));
                        }

                    }
                }
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "صيغة الملف غير صحيحه , من فضلك حاول لاحقاً");
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, " يجب ادخال ملف اكسيل , من فضلك حاول لاحقاً");
        }

        /// </summary>
        /// 
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
            if (model.IsTimeBoundService)
            {
                //if (model.ServiceDays == null)
                //    Errors.Add("عدد ايام الخدمه غسر صحيح");
                if (model.ServiceDays <= 0)
                    Errors.Add("عدد ايام الخدمه غير صحيح");
            }
            else
            {
                if (model.Inventory <= 0)
                    Errors.Add("الكميه غير صحيحه");
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

            if (model.IsTimeBoundService)
            {
                //if (model.ServiceDays == null)
                //    Errors.Add("عدد ايام الخدمه غسر صحيح");
                if (model.ServiceDays <= 0)
                    Errors.Add("عدد ايام الخدمه غير صحيح");
            }
            else
            {
                if (model.Inventory <= 0)
                    Errors.Add("الكميه غير صحيحه");
            }

            return Errors;
        }

        [HttpGet]
        public ActionResult ToggleHide(long? ServiceId)
        {
            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.Find(ServiceId.Value);
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
            if (Service.SupplierId.HasValue == true)
            {
                return RedirectToAction("Suppliers");
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? ServiceId)
        {
            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.Find(ServiceId.Value);
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
            if (Service.SupplierId.HasValue == true)
            {
                return RedirectToAction("Suppliers");
            }
            return RedirectToAction("Dashboard");
        }
        [HttpGet]
        public ActionResult HardDelete(long? ServiceId)
        {
            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.Find(ServiceId.Value);
            if (Service == null)
                return RedirectToAction("Dashboard");

            if (Service.IsDeleted == true)
            {
                Service.HardDelete = true;
                db.SaveChanges();
            }
            if (Service.SupplierId.HasValue == true)
            {
                return RedirectToAction("Suppliers");
            }
            return RedirectToAction("Dashboard");
        }

        public ActionResult Details(long? ServiceId)
        {
            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.Find(ServiceId.Value);
            if (Service == null || Service.HardDelete)
                return RedirectToAction("Dashboard");

            if (Service.SupplierId.HasValue == true)
            {
                return RedirectToAction("Suppliers");
            }
            return View(Service);
        }

        [HttpGet]
        public JsonResult DeleteColor(long ColorId)
        {
            var trainer = db.ServiceTrainers.Find(ColorId);
            if (trainer == null)
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
            else
            {
                CRUD<ServiceColor>.Delete(trainer);
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
            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.FirstOrDefault(s => s.Id == ServiceId.Value && s.SupplierId.HasValue == false);
            if (Service == null)
                return RedirectToAction("Dashboard");

            return View(Service);
        }

        [HttpGet]
        public ActionResult CreateOffer(long? ServiceId)
        {
            if (ServiceId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Service = db.Services.FirstOrDefault(s => s.Id == ServiceId.Value && s.SupplierId.HasValue == false);
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
                    Percentage = offerVM.Percentage,
                    ServiceId = offerVM.ServiceId.Value,
                    OriginalPrice = Service.OriginalPrice,
                    AfterPrice = Service.OriginalPrice - Service.OriginalPrice * offerVM.Percentage / 100,
                    FinishOn = offerVM.FinishOn
                });
                Service.OfferPrice = Service.OriginalPrice - Service.OriginalPrice * offerVM.Percentage / 100;
                db.SaveChanges();
                return RedirectToAction("Offers", new { offerVM.ServiceId });
            }
            ViewBag.Service = Service;
            return View(offerVM);
        }

        [HttpGet]
        public ActionResult EditOffer(long? OfferId)
        {
            if (OfferId.HasValue == false)
                return RedirectToAction("Dashboard");

            var ServiceOffer = db.ServiceOffers.FirstOrDefault(x => x.Id == OfferId.Value);
            if (ServiceOffer == null)
                return RedirectToAction("Dashboard");

            if (ServiceOffer.IsFinished == true)
                return RedirectToAction("Offers", new { ServiceOffer.ServiceId });

            ServiceOfferVM ServiceOfferVM = new ServiceOfferVM()
            {
                FinishOn = ServiceOffer.FinishOn,
                OfferId = ServiceOffer.Id,
                Percentage = ServiceOffer.Percentage,
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
                    Service.OfferPrice = Service.OriginalPrice - Service.OriginalPrice * offerVM.Percentage / 100;
                    Offer.Percentage = offerVM.Percentage;
                    Offer.OriginalPrice = Service.OriginalPrice;
                    Offer.AfterPrice = Service.OriginalPrice - Service.OriginalPrice * offerVM.Percentage / 100;
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
            if (OfferId.HasValue == false)
                return RedirectToAction("Dashboard");

            var ServiceOffer = db.ServiceOffers.FirstOrDefault(x => x.Id == OfferId.Value);
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
            if (OfferId.HasValue == false)
                return RedirectToAction("Dashboard");

            var ServiceOffer = db.ServiceOffers.FirstOrDefault(x => x.Id == OfferId.Value);
            if (ServiceOffer == null)
                return RedirectToAction("Dashboard");

            ServiceOffer.Service.OfferPrice = null;
            ServiceOffer.IsFinished = true;
            CRUD<ServiceOffer>.Update(ServiceOffer);
            db.SaveChanges();
            return RedirectToAction("Offers", new { ServiceOffer.ServiceId });
        }

        [AllowAnonymous]
        public ActionResult View(long? Id)
        {
            if (Id.HasValue == true)
            {
                var Service = db.Services.Include(x => x.Images).FirstOrDefault(s => s.Id == Id.Value && s.IsDeleted == false && s.IsHidden == false);
                if (Service != null)
                {
                    return View(Service);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public JsonResult SetSortingNumber(long ProdId, int Number)
        {
            var Service = db.Services.Find(ProdId);
            if (Service == null)
            {
                return Json(new { Sucess = false, Message = "الخدمه المطلوب غير متوفر" }, JsonRequestBehavior.AllowGet);
            }

            var ExistingServiceNumber = db.Services.FirstOrDefault(w => w.Id != ProdId && w.SubCategoryId == Service.SubCategoryId && w.SortingNumber == Number);
            if (ExistingServiceNumber != null)
            {
                return Json(new { Sucess = false, Message = $"الخدمه {ExistingServiceNumber.NameAr} له نفس الترتيب" }, JsonRequestBehavior.AllowGet);
            }

            Service.SortingNumber = Number;
            CRUD<Service>.Update(Service);
            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}