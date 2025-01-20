using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.DTOs;
using G4Fit.Models.Enums;
using G4Fit.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using G4Fit.Helper;
using System.Web.UI.WebControls;

namespace G4Fit.Controllers.MVC
{
    [AdminAuthorizeAttribute(Roles = "Admin,SubAdmin")]
    public class UsersController : BaseController
    {
        public UserManager<ApplicationUser> UserManager;
        public UsersController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }
        [HttpGet]
        public ActionResult Clients()
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var user = db.Users.Find(CurrentUserId);
                if (user.Role != SubAdminRole.All && user.Role != SubAdminRole.Users)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion

            var nonAdminUsers = db.Users
            .Where(user => !db.Set<IdentityUserRole>()
                .Join(db.Roles, userRole => userRole.RoleId, role => role.Id,
                    (userRole, role) => new { userRole.UserId, role.Name })
                .Any(roleUser => roleUser.UserId == user.Id && (roleUser.Name == "Admin" || roleUser.Name == "SubAdmin")))
            .ToList();

            ViewBag.Cities = db.Cities
                .Where(s => !s.IsDeleted && !s.Country.IsDeleted)
                .ToList();

            return View(nonAdminUsers);
        }
        [AdminAuthorizeAttribute(Roles = "Admin")]
        [HttpGet]
        public ActionResult SubAdmins()
        {
            var nonAdminUsers = db.Users
                .Where(user => db.Set<IdentityUserRole>()
                    .Join(db.Roles, userRole => userRole.RoleId, role => role.Id,
                        (userRole, role) => new { userRole.UserId, role.Name })
                    .Any(roleUser => roleUser.UserId == user.Id && (/*roleUser.Name == "Admin" ||*/ roleUser.Name == "SubAdmin")))
                .ToList();

            ViewBag.Cities = db.Cities
                .Where(s => !s.IsDeleted && !s.Country.IsDeleted)
                .ToList();

            return View(nonAdminUsers);
        }
        [AdminAuthorizeAttribute(Roles = "Admin")]
        [HttpGet]
        public ActionResult CreateSubAdmin()
        {
            ViewBag.Countries = db.Cities.Where(s => s.IsDeleted == false).ToList();
            return View(new CreateSubAdminVM());
        }
        [AdminAuthorizeAttribute(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateSubAdmin(CreateSubAdminVM model)
        {

            RegisterDTO registerDTO = new RegisterDTO()
            {
                PhoneNumber = model.PhoneNumber,
                ConfirmPassword = model.ConfirmPassword,
                Name = model.Name,
                Address = model.Address,
                Email = model.Email,
                IDNumber = null,
                Password = model.Password,
                CountryId = 1,
            };

            var Errors = new List<string>();


            Errors = ValidateCreateSubAdmin(registerDTO, ModelState);
            if (Errors != null && Errors.Count() > 0)
                return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);

            if (ModelState.IsValid == false)
            {
                Errors.Add("حدث خطأ ما ، برجاء المحاوله لاحقاً");
                return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);
            }

            if (ModelState.IsValid)
            {

                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        string Email = registerDTO.Email;
                        if (registerDTO.Email == null)
                        {
                            do
                            {
                                Email = RandomGenerator.GenerateString(7) + "@" + RandomGenerator.GenerateString(4) + ".com";
                            }
                            while (UserValidation.IsEmailExists(Email) == true);
                        }

                        int Vcode = RandomGenerator.GenerateNumber(1000, 9999);
                        Vcode = 1111;
                        var City = db.Cities.Find(registerDTO.CountryId);
                        var Country = db.Countries.Find(City.CountryId);
                        var user = new ApplicationUser() { LoginType = LoginType.G4FitRegisteration, VerificationCode = Vcode, UserName = Email, Role = model.Role, Email = Email, Address = registerDTO.Address, IDNumber = registerDTO.IDNumber, Name = registerDTO.Name, PhoneNumber = registerDTO.PhoneNumber, PhoneNumberCountryCode = Country.PhoneCode, CityId = registerDTO.CountryId, CountryId = City.CountryId };

                        //var qr = QRCodes.GenerateQR(user.IDNumber);
                        //user.QR = qr;
                        IdentityResult result = await UserManager.CreateAsync(user, registerDTO.Password);
                        db.SaveChanges();
                        var res = UserManager.AddToRole(user.Id, "SubAdmin");
                        db.SaveChanges();
                        Transaction.Commit();
                        return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        Transaction.Rollback();
                        Errors.Add("حدث خطأ ما ، برجاء المحاوله لاحقاً");
                        return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            ViewBag.Countries = db.Cities.Where(s => s.IsDeleted == false).ToList();
            return View(model);
        }

        public List<string> ValidateCreateSubAdmin(RegisterDTO registerDTO, ModelStateDictionary Model)
        {

            var Errors = Model.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            if (Errors != null && Errors.Count > 0)
            {
                foreach (var error in Errors)
                {
                    Errors.Add(error);
                }
            }

            //if (!string.IsNullOrEmpty(registerDTO.PhoneNumber))
            //{
            //    // التأكد أن رقم الجوال السعودي يبدأ بـ 05 ويتكون من 10 أرقام
            //    string pattern = @"^05[0-9]{8}$";
            //    if (!Regex.IsMatch(registerDTO.PhoneNumber, pattern))
            //        Errors.Add("الرقم المطلوب غير متاح");
            //}



            var IsValidPassword = registerDTO.Password.Count() >= 6;
            if (!IsValidPassword)
            {
                Errors.Add("كلمة المرور يجب الا تقل عن 6 رموز");
            }



            if (IsPhoneExists(registerDTO.PhoneNumber))
                Errors.Add("هذا الرقم مستخدم من قبل");
            if (IsEmailExists(registerDTO.Email))
                Errors.Add("هذا البريد الالكتروني مستخدم من قبل");

            return Errors;
        }
        public bool IsEmailExists(string Email, string CurrentUserId = null)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return db.Users.Any(x => x.Email.ToLower() == Email.ToLower());
            else
                return db.Users.Any(x => x.Email.ToLower() == Email.ToLower() && x.Id != CurrentUserId);
        }

        public bool IsPhoneExists(string Phone, string CurrentUserId = null)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return db.Users.Any(x => x.PhoneNumber == Phone);
            else
                return db.Users.Any(x => x.PhoneNumber == Phone && x.Id != CurrentUserId);
        }
        [HttpGet]
        public ActionResult Details(string Id)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.Users)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
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
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.Users)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
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
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.Users)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
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
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.Users)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
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
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.Users)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
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
        [HttpGet]
        public ActionResult ChangePassword(string Id)
        {
            if (string.IsNullOrEmpty(Id) == false)
            {
                var user = db.Users.Find(Id);
                if (user != null)
                {
                    ViewBag.UserId = Id;
                    return View();
                }
            }

            return RedirectToAction("Clients");
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.ContainsKey("CurrentPassword"))
            {
                ModelState["CurrentPassword"].Errors.Clear();
            }

            if (ModelState.IsValid == true)
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var user = db.Users.Find(model.UserId);
                if (user != null)
                {
                    user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    db.SaveChanges();
                    TempData["Success"] = true;
                    return RedirectToAction("Clients");
                }
            }

            ViewBag.UserId = model.UserId;
            return View(model);
        }

    }
}