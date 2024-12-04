using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [Authorize(Roles = "Admin,SubAdmin")]
    public class PackagesController : BaseController
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View(db.Packages.Where(s => s.IsDeleted == false).ToList());
        }
        [HttpGet]
        public ActionResult Dashboard(string q)
        {
            var Packages = db.Packages.ToList();
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                Packages = Packages.Where(x => x.IsDeleted == true).ToList();
            }
            else
            {
                Packages = Packages.Where(x => x.IsDeleted == false).ToList();
            }
            return View(Packages);
        }

        [HttpPost]
        public ActionResult Create(Package package)
        {
            List<string> Errors = new List<string>();
            try
            {
                db.Packages.Add(package);
                db.SaveChanges();
            }
            catch (Exception)
            {
                Errors.Add("حدث خطأ ما برجاء المحاولة لاحقاً");
                TempData["PackageErrors"] = Errors;
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? PackageId)
        {
            if (PackageId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Package = db.Packages.Find(PackageId);
            if (Package != null)
            {
                if (Package.IsDeleted == false)
                {
                    CRUD<Package>.Delete(Package);
                }
                else
                {
                    CRUD<Package>.Restore(Package);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public ActionResult Edit(long? PackageId)
        {
            if (PackageId.HasValue == false)
                return RedirectToAction("Dashboard");

            var Package = db.Packages.Find(PackageId);
            if (Package != null)
            {
                return View(Package);
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public ActionResult Edit(Package package)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(package).State = EntityState.Modified;
                    CRUD<Package>.Update(package);
                    db.SaveChanges();
                    return RedirectToAction("Dashboard");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "حدث خطأ ما برجاء المحاولة لاحقاً");
                    return View(package);
                }
            }
            return View(package);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Subscribe(long PackageId, PeriodType type)
        {
            if (User.Identity.IsAuthenticated == false || CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var Package = db.Packages.FirstOrDefault(w => w.IsDeleted == false && w.Id == PackageId);
            if (Package == null)
                return RedirectToAction("Index");

            string ReturnUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/Packages/Result";
            string Url = null;
            if (type == PeriodType.Monthly)
            {
                Url = PaymentActions.GetPackagePaymentGatewayUrl(Package.MonthlyPrice, PackageId, type, CurrentUserId, ReturnUrl);
            } 
            if (type == PeriodType.Yearly) //yearly
            {
                Url = PaymentActions.GetPackagePaymentGatewayUrl(Package.YearlyPrice, PackageId, type, CurrentUserId, ReturnUrl);
            }

            if (Url != null)
            {
                return Redirect(Url);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Result(string PaymentId, string TranId, string ECI, string Result, string TrackId, string ResponseCode, string AuthCode, string RRN, string responseHash, string amount, string cardBrand)
        {
            try
            {
                var Data = TrackId.Split('_');
                var PackageId = long.Parse(Data.ElementAtOrDefault(1));
                PaymentActions.SaveResponseInDatabase(PaymentId, TranId, ECI, Result, TrackId, ResponseCode, AuthCode, RRN, responseHash, amount, cardBrand, Models.Enums.TransactionType.PurchasingPackage, CurrentUserId, null, PackageId);
                bool IsPaymentSuccess = PaymentActions.VerifyResponse(TranId, Result, TrackId, ResponseCode, responseHash, amount);
                if (IsPaymentSuccess == true)
                {
                    var Type = (PeriodType) Enum.Parse(typeof(PeriodType), Data.ElementAtOrDefault(2));
                    var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                    var user = db.Users.Find(CurrentUserId);
                    if (user != null)
                    {
                        if (user.CountryId.HasValue == true)
                        {
                            DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(user.Country.TimeZoneId));
                        }
                    }
                    var packageUser = new UserPackage()
                    {
                        TrackId = TrackId,
                        IsActive = true,
                        IsPaid = true,
                        NumberOfTimesUsed = 0,
                        PackageId = PackageId,
                        UserId = CurrentUserId,
                        StartOn = DateTimeNow,
                        FinishOn = Type == PeriodType.Monthly ? DateTimeNow.AddMonths(1) : DateTimeNow.AddYears(1)
                    };
                    db.UserPackages.Add(packageUser);
                    db.SaveChanges();
                    TempData["OrderPlaced"] = true;
                    TempData["Package"] = packageUser;
                    return RedirectToAction("Success");
                }
                else
                {
                    TempData["PaymentFailed"] = PaymentActions.HandleResponseStatusCode(culture, ResponseCode);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Success()
        {
            var userPackage = TempData["Package"] as UserPackage;
            if (TempData["OrderPlaced"] == null || userPackage == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(userPackage);
        }
    }
}