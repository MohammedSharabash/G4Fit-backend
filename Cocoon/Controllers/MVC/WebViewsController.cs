using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace G4Fit.Controllers.MVC
{
    public class WebViewsController : BaseController
    {
        public ActionResult Informations(string lang = "ar")
        {
            var MetaData = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            return View(MetaData);
        }

        public ActionResult HowToOrder(string lang = "ar")
        {
            var MetaData = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            return View(MetaData);
        }

        public ActionResult CustomerService(string lang = "ar")
        {
            var MetaData = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            return View(MetaData);
        }

        public ActionResult ReturnAndExchangePolicy(string lang = "ar")
        {
            var MetaData = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            return View(MetaData);
        }

        public ActionResult DeliveringConditions(string lang = "ar")
        {
            var MetaData = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            return View(MetaData);
        }

        public ActionResult TermsConditions(string lang = "ar")
        {
            var MetaData = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            return View(MetaData);
        }

        public ActionResult PrivacyPolicy(string lang = "ar")
        {
            var MetaData = db.CompanyDatas.FirstOrDefault(w => w.IsDeleted == false);
            return View(MetaData);
        }

        public ActionResult Packages(string UserId, string lang = "ar")
        {
            ViewBag.culture = lang;
            ViewBag.UserId = UserId;
            return View(db.Packages.Where(s => s.IsDeleted == false).ToList());
        }

        public ActionResult SubscribeToPackage(long PackageId, PeriodType type, string UserId)
        {
            if (UserId == null)
            {
                return RedirectToAction("Packages", new { UserId, lang = "ar" });
            }
            var user = db.Users.Find(UserId);
            if (user == null)
            {
                return null;
            }
            var Package = db.Packages.FirstOrDefault(w => w.IsDeleted == false && w.Id == PackageId);
            if (Package == null)
                return RedirectToAction("Packages", new { lang = "ar", UserId });

            string ReturnUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/WebViews/PackageSubscribtionResult/" + UserId;
            string Url = null;
            if (type == PeriodType.Monthly)
            {
                Url = PaymentActions.GetPackagePaymentGatewayUrl(Package.MonthlyPrice, PackageId, type, UserId, ReturnUrl);
            }
            if (type == PeriodType.Yearly) //yearly
            {
                Url = PaymentActions.GetPackagePaymentGatewayUrl(Package.YearlyPrice, PackageId, type, UserId, ReturnUrl);
            }

            if (Url != null)
            {
                return Redirect(Url);
            }
            else
            {
                return RedirectToAction("Packages", new { lang = "ar", UserId });
            }
        }

        [Route("WebViews/PackageSubscribtionResult/{UserId}")]
        public ActionResult PackageSubscribtionResult(string UserId, string PaymentId, string TranId, string ECI, string Result, string TrackId, string ResponseCode, string AuthCode, string RRN, string responseHash, string amount, string cardBrand)
        {
            if (UserId == null)
            {
                return null;
            }
            try
            {
                var user = db.Users.Find(UserId);
                if (user == null)
                {
                    return null;
                }
                var Data = TrackId.Split('_');
                var PackageId = long.Parse(Data.ElementAtOrDefault(1));
                PaymentActions.SaveResponseInDatabase(PaymentId, TranId, ECI, Result, TrackId, ResponseCode, AuthCode, RRN, responseHash, amount, cardBrand, TransactionType.PurchasingPackage, UserId, null, PackageId);
                bool IsPaymentSuccess = PaymentActions.VerifyResponse(TranId, Result, TrackId, ResponseCode, responseHash, amount);
                if (IsPaymentSuccess == true)
                {
                    var Type = (PeriodType)Enum.Parse(typeof(PeriodType), Data.ElementAtOrDefault(2));
                    var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
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
                        UserId = UserId,
                        StartOn = DateTimeNow,
                        FinishOn = Type == PeriodType.Monthly ? DateTimeNow.AddMonths(1) : DateTimeNow.AddYears(1)
                    };
                    db.UserPackages.Add(packageUser);
                    db.SaveChanges();
                    TempData["OrderPlaced"] = true;
                    TempData["Package"] = packageUser;
                    return RedirectToAction("PackageSubscribtionSuccess");
                }
                else
                {
                    TempData["PaymentFailed"] = PaymentActions.HandleResponseStatusCode("ar", ResponseCode);
                    return RedirectToAction("Packages", new { lang = "ar", UserId });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Packages", new { lang = "ar", UserId });
            }
        }
        public ActionResult PackageSubscribtionSuccess(string lang = "ar")
        {
            ViewBag.culture = lang;
            var userPackage = TempData["Package"] as UserPackage;
            if (TempData["OrderPlaced"] == null || userPackage == null)
            {
                return null;
            }
            return View(userPackage);
        }
    }
}