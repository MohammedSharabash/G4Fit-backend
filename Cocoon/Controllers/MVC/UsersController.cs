using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using G4Fit.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        [HttpGet]
        public ActionResult Clients()
        {
            var Users = db.Users.Where(s => s.Email.ToLower().Contains("admin2@") == false || s.Email.ToLower().Contains("admin@") == false).ToList();
            ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
            return View(Users);
        }

        [HttpGet]
        public ActionResult Details(string Id)
        {
            var user = db.Users.Find(Id);
            if (user != null)
            {
                return View(user);
            }
            return RedirectToAction("Clients");
        }

        [HttpGet]
        public ActionResult ToggleBlock(string Id)
        {
            var user = db.Users.Find(Id);
            if (user != null)
            {
                if (user.IsDeleted == true)
                {
                    user.IsDeleted = false;
                }
                else
                {
                    user.IsDeleted = true;
                }
                db.SaveChanges();
                TempData["Success"] = true;
            }
            return RedirectToAction("Clients");
        }

        public ActionResult Verify(string Id)
        {
            var user = db.Users.Find(Id);
            if (user != null)
            {
                user.PhoneNumberConfirmed = true;
                db.SaveChanges();
                TempData["Success"] = true;
            }
            return RedirectToAction("Clients");
        }

        [HttpGet]
        public ActionResult Wallet(string Id)
        {
            var user = db.Users.Find(Id);
            if (user != null)
            {
                return View(user);
            }
            return RedirectToAction("Clients");
        }

        [HttpPost]
        public ActionResult AddOrSubtractToUserWallet(string UserId, decimal Amount, string Way, HttpPostedFileBase Attachment, bool IsAdd)
        {
            var user = db.Users.Find(UserId);
            if (user == null)
            {
                TempData["SubmitError"] = "العضو المطلوب غير متاح";
                return RedirectToAction("Wallet", new { Id = UserId });
            }
            if (Amount <= 0)
            {
                TempData["SubmitError"] = "المبلغ المطلوب اضافته غير صحيح";
                return RedirectToAction("Wallet", new { Id = UserId });
            }
            if (Attachment != null)
            {
                if (CheckFiles.IsValidFile(Attachment) == false)
                {
                    TempData["SubmitError"] = "البيان المرفق ملف غير صحيح";
                    return RedirectToAction("Wallet", new { Id = UserId });
                }
            }
            if (IsAdd == true)
            {
                user.Wallet += Amount;
                UserWallet userWallet = new UserWallet()
                {
                    TransactionAmount = Amount,
                    TransactionType = TransactionType.AddedByAdminManually,
                    UserId = UserId,
                    TransactionWay = Way,
                };
                if (Attachment != null)
                {
                    userWallet.AttachmentUrl = MediaControl.Upload(FilePath.Other, Attachment);
                }
                db.UserWallets.Add(userWallet);
            }
            else
            {
                user.Wallet -= Amount;
                UserWallet userWallet = new UserWallet()
                {
                    TransactionAmount = Amount,
                    TransactionType = TransactionType.SubtractedByAdminManually,
                    UserId = UserId,
                    TransactionWay = Way,
                };
                if (Attachment != null)
                {
                    userWallet.AttachmentUrl = MediaControl.Upload(FilePath.Other, Attachment);
                }
                db.UserWallets.Add(userWallet);
            }
            db.SaveChanges();
            TempData["Success"] = true;
            TempData["SubmitSuccess"] = true;
            return RedirectToAction("Wallet", new { Id = UserId });
        }
    }
}