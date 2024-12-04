using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using G4Fit.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    public class SuppliersController : BaseController
    {
        public SuppliersController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        public ActionResult SellWithUs()
        {
            var Content = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            if (Content != null)
            {
                HTMLContentVM contentVM = new HTMLContentVM()
                {
                    ContentAr = Content.SellWithUsAr,
                    ContentEn = Content.SellWithUsEn,
                };
                return View(contentVM);
            }
            return View();
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        [HttpPost]
        public ActionResult SellWithUs(HTMLContentVM content)
        {
            try
            {
                var Data = db.CompanyDatas.FirstOrDefault(d => d.IsDeleted == false);
                if (Data != null)
                {
                    Data.SellWithUsAr = content.ContentAr;
                    Data.SellWithUsEn = content.ContentEn;
                    CRUD<CompanyData>.Update(Data);
                    db.SaveChanges();
                }
                else
                {
                    CompanyData CompanyData = new CompanyData()
                    {
                        SellWithUsAr = content.ContentAr,
                        SellWithUsEn = content.ContentEn
                    };
                    db.CompanyDatas.Add(CompanyData);
                    db.SaveChanges();
                }
                TempData["Success"] = true;
                return RedirectToAction("SellWithUs");
            }
            catch (Exception ex)
            {
                return RedirectToAction("SellWithUs");
            }
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        public ActionResult SellingPolicy()
        {
            var Content = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            if (Content != null)
            {
                HTMLContentVM contentVM = new HTMLContentVM()
                {
                    ContentAr = Content.SupplierSellingPolicyAr,
                    ContentEn = Content.SupplierSellingPolicyEn,
                };
                return View(contentVM);
            }
            return View();
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        [HttpPost]
        public ActionResult SellingPolicy(HTMLContentVM content)
        {
            try
            {
                var Data = db.CompanyDatas.FirstOrDefault(d => d.IsDeleted == false);
                if (Data != null)
                {
                    Data.SupplierSellingPolicyAr = content.ContentAr;
                    Data.SupplierSellingPolicyEn = content.ContentEn;
                    CRUD<CompanyData>.Update(Data);
                    db.SaveChanges();
                }
                else
                {
                    CompanyData CompanyData = new CompanyData()
                    {
                        SupplierSellingPolicyAr = content.ContentAr,
                        SupplierSellingPolicyEn = content.ContentEn
                    };
                    db.CompanyDatas.Add(CompanyData);
                    db.SaveChanges();
                }
                TempData["Success"] = true;
                return RedirectToAction("SellingPolicy");
            }
            catch (Exception ex)
            {
                return RedirectToAction("SellingPolicy");
            }
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        public ActionResult Dashboard()
        {
            var Suppliers = db.Suppliers.OrderByDescending(w => w.CreatedOn).ToList();
            return View(Suppliers);
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        public ActionResult Verify(long? SupplierId)
        {
            var supplier = db.Suppliers.Find(SupplierId);
            if (supplier != null)
            {
                supplier.User.PhoneNumberConfirmed = true;
                db.SaveChanges();
                TempData["Success"] = true;
            }
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        public ActionResult ToggleBlock(long? SupplierId)
        {
            var Supplier = db.Suppliers.Find(SupplierId);
            if (Supplier != null)
            {
                if (Supplier.IsDeleted == true)
                {
                    Supplier.IsDeleted = false;
                }
                else
                {
                    Supplier.IsDeleted = true;
                }
                db.SaveChanges();
                TempData["Success"] = true;
            }
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        public ActionResult AcceptSupplier(long? SupplierId)
        {
            var Supplier = db.Suppliers.Find(SupplierId);
            if (Supplier != null)
            {
                Supplier.IsAccepted = true;
                db.SaveChanges();
                TempData["Success"] = true;
            }
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        public ActionResult RejectSupplier(long? SupplierId)
        {
            var Supplier = db.Suppliers.Find(SupplierId);
            if (Supplier != null)
            {
                if (Supplier.IsAccepted.HasValue == false)
                {
                    Supplier.IsAccepted = false;
                    db.SaveChanges();
                    TempData["Success"] = true;
                }
            }
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin,SubAdmin")]
        public ActionResult Information(long? SupplierId)
        {
            var Supplier = db.Suppliers.Find(SupplierId);
            if (Supplier != null)
            {
                return View(Supplier);
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public ActionResult Information(Supplier supplier, HttpPostedFileBase CommercialRegisterFile, HttpPostedFileBase TaxNumberFile, HttpPostedFileBase IdentityFile)
        {
            if (CommercialRegisterFile != null)
            {
                supplier.CommercialRegisterFileUrl = MediaControl.Upload(FilePath.Other, CommercialRegisterFile);
            }
            if (TaxNumberFile != null)
            {
                supplier.TaxNumberFileUrl = MediaControl.Upload(FilePath.Other, TaxNumberFile);
            }
            if (IdentityFile != null)
            {
                supplier.IdentityFileUrl = MediaControl.Upload(FilePath.Other, IdentityFile);
            }
            CRUD<Supplier>.Update(supplier);
            db.Entry(supplier).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = true;
            return RedirectToAction("Information", new { SupplierId = supplier.Id });
        }

        public ActionResult Affiliate()
        {
            return View(db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false));
        }

        public ActionResult Policy()
        {
            return View(db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false));
        }

        public ActionResult Join()
        {
            ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Join(SupplierJoinVM supplier)
        {
            if (ModelState.IsValid == true)
            {
                if (!string.IsNullOrEmpty(supplier.PhoneNumber))
                {
                    //var IsValidPhoneNumber = supplier.PhoneNumber.All(char.IsDigit);
                    //if (IsValidPhoneNumber == false || supplier.PhoneNumber.Contains("+"))
                    //{
                    //    ModelState.AddModelError("PhoneNumber", culture == "ar" ? "رقم الهاتف غير صحيح" : "Invalid phone number");
                    //    goto Return;
                    //}
                }

                var Country = db.Countries.FirstOrDefault(s => s.IsDeleted == false && s.Id == supplier.CountryId);
                if (Country == null)
                {
                    ModelState.AddModelError("CountryId", culture == "ar" ? "رقم الهاتف غير صحيح" : "Invalid phone number");
                    goto Return;
                }

                if (UserValidation.IsPhoneExists(supplier.PhoneNumber) == true)
                {
                    ModelState.AddModelError("PhoneNumber", culture == "ar" ? "رقم الهاتف مسجل من قبل" : "Phone number already exists");
                    goto Return;
                }

                if (UserValidation.IsEmailExists(supplier.Email) == true)
                {
                    ModelState.AddModelError("Email", culture == "ar" ? "البريد الالكترونى مسجل من قبل" : "Email already exists");
                    goto Return;
                }

                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        int Vcode = RandomGenerator.GenerateNumber(1000, 9999);
                        var user = new ApplicationUser() { LoginType = LoginType.G4FitRegisteration, VerificationCode = Vcode, UserName = supplier.Email, Email = supplier.Email, Name = supplier.FirstName + " " + supplier.LastName, PhoneNumber = supplier.PhoneNumber, PhoneNumberCountryCode = Country.PhoneCode, CountryId = supplier.CountryId };

                        IdentityResult result = await UserManager.CreateAsync(user, supplier.Password);
                        if (!result.Succeeded)
                        {
                            Transaction.Rollback();
                            ModelState.AddModelError("Email", culture == "ar" ? "برجاء المحاوله مره اخرى" : "Please try again later");
                            goto Return;
                        }

                        await UserManager.AddToRoleAsync(user.Id, "Supplier");

                        Supplier newSupplier = new Supplier()
                        {
                            FirstName = supplier.FirstName,
                            LastName = supplier.LastName,
                            StoreName = supplier.StoreName,
                            UserId = user.Id,
                            Type = supplier.SupplierType
                        };
                        db.Suppliers.Add(newSupplier);
                        db.SaveChanges();
                        Transaction.Commit();

                        //await //SMS.SendMessageAsync(Country.PhoneCode, user.PhoneNumber, $"رمز التحقق هو [{Vcode}]");
                        TempData["Success"] = true;
                        return RedirectToAction("Success");
                    }
                    catch (Exception ex)
                    {
                        Transaction.Rollback();
                        ModelState.AddModelError("Email", culture == "ar" ? "برجاء المحاوله مره اخرى" : "Please try again later");
                        goto Return;
                    }
                }

            }

        Return:
            ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
            return View(supplier);
        }

        public ActionResult Success()
        {
            if (TempData["Success"] != null)
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Supplier")]
        public ActionResult Index()
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(Supplier);
        }
        public UserManager<ApplicationUser> UserManager;

    }
}