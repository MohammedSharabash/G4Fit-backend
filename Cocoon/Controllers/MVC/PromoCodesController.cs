﻿using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [AdminAuthorizeAttribute(Roles = "Admin,SubAdmin")]
    public class PromoCodesController : BaseController
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        //All general coupons
        public ActionResult Index()
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            return View(db.PromoCodes.Where(w => string.IsNullOrEmpty(w.UserId)).ToList());
        }

        //All special coupons
        public ActionResult Specials()
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            return View(db.PromoCodes.Where(w => !string.IsNullOrEmpty(w.UserId)).ToList());
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? Id)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Promo = db.PromoCodes.Find(Id);
            if (Promo == null)
                return RedirectToAction("Index");

            if (Promo.IsDeleted == true)
                CRUD<PromoCode>.Restore(Promo);
            else
                CRUD<PromoCode>.Delete(Promo);

            db.SaveChanges();
            if (Promo.UserId != null)
            {
                return RedirectToAction("Specials");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Finish(long? Id)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Promo = db.PromoCodes.Find(Id);
            if (Promo == null)
                return RedirectToAction("Index");

            Promo.IsFinished = true;
            CRUD<PromoCode>.Update(Promo);
            db.SaveChanges();
            if (Promo.UserId != null)
            {
                return RedirectToAction("Specials");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(long? Id)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Promo = db.PromoCodes.Find(Id);
            if (Promo == null)
                return RedirectToAction("Index");

            if (Promo.UserId != null)
            {
                return RedirectToAction("EditSpecial", new { Id });
            }

            return View(Promo);
        }

        [HttpGet]
        public ActionResult EditSpecial(long? Id)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            if (Id.HasValue == false)
                return RedirectToAction("Specials");

            var Promo = db.PromoCodes.Find(Id);
            if (Promo == null)
                return RedirectToAction("Specials");

            if (Promo.UserId == null)
            {
                return RedirectToAction("Edit", new { Id });
            }

            ViewBag.Users = db.Users.Where(user => !db.Set<IdentityUserRole>()
                    .Join(db.Roles, userRole => userRole.RoleId, role => role.Id,
                        (userRole, role) => new { userRole.UserId, role.Name })
                    .Any(roleUser => roleUser.UserId == user.Id && (roleUser.Name == "Admin" || roleUser.Name == "SubAdmin"))).ToList();
            return View(Promo);
        }

        [HttpPost]
        public ActionResult Edit(PromoCode PromoCode)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));
            if (PromoCode.FinishOn.HasValue)
            {
                if (PromoCode.FinishOn.Value.Date < DateTimeNow.Date)
                {
                    ModelState.AddModelError("FinishOn", "تاريخ النهاية غير صحيح");
                }
            }

            if (PromoCode.DiscountMoney.HasValue == false && PromoCode.DiscountPercentage.HasValue == false)
            {
                ModelState.AddModelError("DiscountMoney", "يجب الاختيار ما بين الخصم بنسبة مئوية او نقداً");
                ModelState.AddModelError("DiscountPercentage", "يجب الاختيار ما بين الخصم بنسبة مئوية او نقداً");
            }
            if (PromoCode.MaximumDiscountMoney.HasValue == true)
            {
                if (PromoCode.DiscountMoney.HasValue == true && PromoCode.DiscountMoney > PromoCode.MaximumDiscountMoney)
                {
                    ModelState.AddModelError("DiscountMoney", "الحد الاقصى للخصم اكبر من قيمه الخصم نفسه");
                    ModelState.AddModelError("MaximumDiscountMoney", "الحد الاقصى للخصم اكبر من قيمه الخصم نفسه");
                }
            }
            if (ModelState.IsValid == true)
            {
                if (string.IsNullOrEmpty(PromoCode.Text))
                {
                    PromoCode.Text = RandomGenerator.GenerateString(5);
                }

                if (PromoCode.DiscountPercentage.HasValue == true)
                {
                    PromoCode.DiscountMoney = null;
                }
                if (PromoCode.DiscountMoney.HasValue == true)
                {
                    PromoCode.DiscountPercentage = null;
                }
                if (PromoCode.NumberOfAllowedUsingTimes <= 1)
                {
                    PromoCode.NumberOfAllowedUsingTimes = 1;
                }
                db.Entry(PromoCode).State = System.Data.Entity.EntityState.Modified;
                CRUD<PromoCode>.Update(PromoCode);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(PromoCode);
        }

        [HttpPost]
        public ActionResult EditSpecial(PromoCode PromoCode)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            var user = db.Users.Find(PromoCode.UserId);
            if (user == null)
            {
                ModelState.AddModelError("UserId", "هذا المستخدم غير صحيح");
            }
            var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));
            if (PromoCode.FinishOn.HasValue)
            {
                if (PromoCode.FinishOn.Value.Date < DateTimeNow.Date)
                {
                    ModelState.AddModelError("FinishOn", "تاريخ النهاية غير صحيح");
                }
            }

            if (PromoCode.DiscountMoney.HasValue == false && PromoCode.DiscountPercentage.HasValue == false)
            {
                ModelState.AddModelError("DiscountMoney", "يجب الاختيار ما بين الخصم بنسبة مئوية او نقداً");
                ModelState.AddModelError("DiscountPercentage", "يجب الاختيار ما بين الخصم بنسبة مئوية او نقداً");
            }
            if (PromoCode.MaximumDiscountMoney.HasValue == true)
            {
                if (PromoCode.DiscountMoney.HasValue == true && PromoCode.DiscountMoney > PromoCode.MaximumDiscountMoney)
                {
                    ModelState.AddModelError("DiscountMoney", "الحد الاقصى للخصم اكبر من قيمه الخصم نفسه");
                    ModelState.AddModelError("MaximumDiscountMoney", "الحد الاقصى للخصم اكبر من قيمه الخصم نفسه");
                }
            }
            if (ModelState.IsValid == true)
            {
                if (string.IsNullOrEmpty(PromoCode.Text))
                {
                    PromoCode.Text = RandomGenerator.GenerateString(5);
                }

                if (PromoCode.DiscountPercentage.HasValue == true)
                {
                    PromoCode.DiscountMoney = null;
                }
                if (PromoCode.DiscountMoney.HasValue == true)
                {
                    PromoCode.DiscountPercentage = null;
                }
                if (PromoCode.NumberOfAllowedUsingTimes <= 1)
                {
                    PromoCode.NumberOfAllowedUsingTimes = 1;
                }
                db.Entry(PromoCode).State = System.Data.Entity.EntityState.Modified;
                CRUD<PromoCode>.Update(PromoCode);
                db.SaveChanges();
                return RedirectToAction("Specials");
            }
            ViewBag.Users = db.Users.Where(u => !db.Set<IdentityUserRole>()
                    .Join(db.Roles, userRole => userRole.RoleId, role => role.Id,
                        (userRole, role) => new { userRole.UserId, role.Name })
                    .Any(roleUser => roleUser.UserId == u.Id && (roleUser.Name == "Admin" || roleUser.Name == "SubAdmin"))).ToList();
            return View(PromoCode);
        }

        [HttpGet]
        public ActionResult Create()
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            return View();
        }

        [HttpGet]
        public ActionResult CreateSpecial()
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            ViewBag.Users = db.Users.Where(u => !db.Set<IdentityUserRole>()
                    .Join(db.Roles, userRole => userRole.RoleId, role => role.Id,
                        (userRole, role) => new { userRole.UserId, role.Name })
                    .Any(roleUser => roleUser.UserId == u.Id && (roleUser.Name == "Admin" || roleUser.Name == "SubAdmin"))).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(PromoCode PromoCode)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));
            if (PromoCode.FinishOn.HasValue)
            {
                if (PromoCode.FinishOn.Value.Date < DateTimeNow.Date)
                {
                    ModelState.AddModelError("FinishOn", "تاريخ النهاية غير صحيح");
                }
            }
            if (PromoCode.DiscountMoney.HasValue == false && PromoCode.DiscountPercentage.HasValue == false)
            {
                ModelState.AddModelError("DiscountMoney", "يجب الاختيار ما بين الخصم بنسبة مئوية او نقداً");
                ModelState.AddModelError("DiscountPercentage", "يجب الاختيار ما بين الخصم بنسبة مئوية او نقداً");
            }

            if (PromoCode.MaximumDiscountMoney.HasValue == true)
            {
                if (PromoCode.DiscountMoney.HasValue == true && PromoCode.DiscountMoney > PromoCode.MaximumDiscountMoney)
                {
                    ModelState.AddModelError("DiscountMoney", "الحد الاقصى للخصم اكبر من قيمه الخصم نفسه");
                    ModelState.AddModelError("MaximumDiscountMoney", "الحد الاقصى للخصم اكبر من قيمه الخصم نفسه");
                }
            }
            if (ModelState.IsValid == true)
            {
                if (string.IsNullOrEmpty(PromoCode.Text))
                {
                    PromoCode.Text = RandomGenerator.GenerateString(5);
                }
                if (PromoCode.DiscountPercentage.HasValue == true)
                {
                    PromoCode.DiscountMoney = null;
                }
                if (PromoCode.DiscountMoney.HasValue == true)
                {
                    PromoCode.DiscountPercentage = null;
                }
                if (PromoCode.NumberOfAllowedUsingTimes <= 1)
                {
                    PromoCode.NumberOfAllowedUsingTimes = 1;
                }
                db.PromoCodes.Add(PromoCode);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(PromoCode);
        }

        [HttpPost]
        public ActionResult CreateSpecial(PromoCode PromoCode)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            var user = db.Users.Find(PromoCode.UserId);
            if (user == null)
            {
                ModelState.AddModelError("UserId", "هذا المستخدم غير صحيح");
            }
            var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));
            if (PromoCode.FinishOn.HasValue)
            {
                if (PromoCode.FinishOn.Value.Date < DateTimeNow.Date)
                {
                    ModelState.AddModelError("FinishOn", "تاريخ النهاية غير صحيح");
                }
            }
            if (PromoCode.DiscountMoney.HasValue == false && PromoCode.DiscountPercentage.HasValue == false)
            {
                ModelState.AddModelError("DiscountMoney", "يجب الاختيار ما بين الخصم بنسبة مئوية او نقداً");
                ModelState.AddModelError("DiscountPercentage", "يجب الاختيار ما بين الخصم بنسبة مئوية او نقداً");
            }
            if (PromoCode.MaximumDiscountMoney.HasValue == true)
            {
                if (PromoCode.DiscountMoney.HasValue == true && PromoCode.DiscountMoney > PromoCode.MaximumDiscountMoney)
                {
                    ModelState.AddModelError("DiscountMoney", "الحد الاقصى للخصم اكبر من قيمه الخصم نفسه");
                    ModelState.AddModelError("MaximumDiscountMoney", "الحد الاقصى للخصم اكبر من قيمه الخصم نفسه");
                }
            }
            if (ModelState.IsValid == true)
            {
                if (string.IsNullOrEmpty(PromoCode.Text))
                {
                    PromoCode.Text = RandomGenerator.GenerateString(5);
                }

                if (PromoCode.DiscountPercentage.HasValue == true)
                {
                    PromoCode.DiscountMoney = null;
                }
                if (PromoCode.DiscountMoney.HasValue == true)
                {
                    PromoCode.DiscountPercentage = null;
                }
                if (PromoCode.NumberOfAllowedUsingTimes <= 1)
                {
                    PromoCode.NumberOfAllowedUsingTimes = 1;
                }
                db.PromoCodes.Add(PromoCode);
                db.SaveChanges();
                return RedirectToAction("Specials");
            }
            ViewBag.Users = db.Users.Where(u => !db.Set<IdentityUserRole>()
                    .Join(db.Roles, userRole => userRole.RoleId, role => role.Id,
                        (userRole, role) => new { userRole.UserId, role.Name })
                    .Any(roleUser => roleUser.UserId == u.Id && (roleUser.Name == "Admin" || roleUser.Name == "SubAdmin"))).ToList();
            return View(PromoCode);
        }

        [HttpGet]
        public ActionResult Items(long? PromoCodeId)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            if (PromoCodeId.HasValue == true)
            {
                var PromoCode = db.PromoCodes.Find(PromoCodeId);
                if (PromoCode != null)
                {
                    if (PromoCode.UserId != null)
                    {
                        return RedirectToAction("Items", new { PromoCodeId });
                    }
                    return View(PromoCode);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ItemsSpecial(long? PromoCodeId)
        {
            #region Check SubAdmin Role
            CurrentUserId = User.Identity.GetUserId();
            if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
            {
                var SubAdmin = db.Users.Find(CurrentUserId);
                if (SubAdmin.Role != SubAdminRole.All && SubAdmin.Role != SubAdminRole.PromoCodes)
                    return RedirectToAction("Index", "Cp");
            }
            #endregion
            if (PromoCodeId.HasValue == true)
            {
                var PromoCode = db.PromoCodes.Find(PromoCodeId);
                if (PromoCode != null)
                {
                    if (PromoCode.UserId == null)
                    {
                        return RedirectToAction("Items", new { PromoCodeId });
                    }
                    return View(PromoCode);
                }
            }
            return RedirectToAction("Specials");
        }
    }
}