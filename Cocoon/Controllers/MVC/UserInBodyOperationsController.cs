using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static QRCoder.PayloadGenerator;
using SMS = G4Fit.Helpers.SMS;

namespace G4Fit.Controllers.MVC
{
    [AdminAuthorizeAttribute(Roles = "Admin")]
    public class UserInBodyOperationsController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string UserId)
        {
            List<UserInBodyOperation> UserInBodyOperations = new List<UserInBodyOperation>();

            UserInBodyOperations = UserInBodyOperations.Where(w => w.UserId == UserId).OrderBy(x => x.CreatedOn).ToList();
            var Orders = db.Orders.Where(s => s.IsDeleted == false && s.IsPaid && s.UserId == UserId
            && s.OrderStatus != OrderStatus.Initialized
            && s.OrderStatus != OrderStatus.Delivered
            && s.OrderStatus != OrderStatus.Canceled
            && s.InBodyCount > 0
            && s.UserId != null).OrderByDescending(s => s.CreatedOn).ToList();
            ViewBag.UserInBodyOperations = UserInBodyOperations;
            ViewBag.Orders = Orders;
            //ViewBag.UserId = UserId;

            UserInBodyOperation vm = new UserInBodyOperation()
            {
                UserId = UserId,
                ConfirmationCode = new Random().Next(1000, 9999).ToString(),
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(UserInBodyOperation UserInBodyOperation, HttpPostedFileBase Image)
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
                    UserInBodyOperation.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
                }
                var user = db.Users.Find(UserInBodyOperation.UserId);
                var code = UserInBodyOperation.ConfirmationCode;
                if (string.IsNullOrWhiteSpace(UserInBodyOperation.ConfirmationCode))
                {
                    UserInBodyOperation.ConfirmationCode = new Random().Next(1000, 9999).ToString();
                    code = UserInBodyOperation.ConfirmationCode;
                }
                //if (user.PhoneNumber.StartsWith("010") || user.PhoneNumber.StartsWith("011") || user.PhoneNumber.StartsWith("012") || user.PhoneNumber.StartsWith("015"))
                //{
                //    await SMS.SendTaqnyatMessageAsync("2", user.PhoneNumber, $"كود تأكيد تنفيذ InBody الخاص بك  هو  [{code}]");
                //    //await SMS.SendMessageAsync("2", user.PhoneNumber, $"كود تأكيد تنفيذ InBody الخاص بك  هو  [{code}]");
                //}
                //else
                    await SMS.SendTaqnyatMessageAsync("966", user.PhoneNumber.StartsWith("0") ? user.PhoneNumber.Substring(1) : user.PhoneNumber, $"كود تأكيد تنفيذ InBody الخاص بك  هو  [{code}]");
                //await SMS.SendMessageAsync("966", user.PhoneNumber.StartsWith("0") ? user.PhoneNumber.Substring(1) : user.PhoneNumber, $"كود تأكيد تنفيذ InBody الخاص بك  هو  [{code}]");


                db.UserInBodyOperations.Add(UserInBodyOperation);
                db.SaveChanges();
                return RedirectToAction("Index", new { UserId = UserInBodyOperation.UserId });
            }
            ViewBag.UserInBodyOperations = db.UserInBodyOperations.Where(x => !x.IsDeleted).ToList();
            var Orders = db.Orders.Where(s => s.IsDeleted == false && s.IsPaid && s.UserId == UserInBodyOperation.UserId
             && s.OrderStatus != OrderStatus.Initialized
             && s.OrderStatus != OrderStatus.Delivered
             && s.OrderStatus != OrderStatus.Canceled
             && s.InBodyCount > 0
             && s.UserId != null).OrderByDescending(s => s.CreatedOn).ToList();
            ViewBag.Orders = Orders;
            ViewBag.UserId = UserInBodyOperation.UserId;
            return View(UserInBodyOperation);
        }

        [HttpPost]
        public JsonResult Confirm(long Id, string code)
        {
            var model = db.UserInBodyOperations.Find(Id);
            if (model == null)
            {
                return Json(new { Sucess = false, Message = "العنصر  غير متوفر" }, JsonRequestBehavior.AllowGet);
            }
            if (model.ConfirmationCode != code)
                return Json(new { Sucess = false, Message = "كود التاكيد خطأ" }, JsonRequestBehavior.AllowGet);
            model.Confirmed = true;
            CRUD<UserInBodyOperation>.Update(model);
            var Order = db.Orders.Find(model.OrderId);

            Order.InBodyCount -= 1;
            Order.InBodyUsedCount += 1;
            db.SaveChanges();

            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult Edit(long? id)
        //{
        //    if (id == null)
        //        return RedirectToAction("Index");
        //    UserInBodyOperation UserInBodyOperation = db.UserInBodyOperations.Find(id);
        //    if (UserInBodyOperation == null)
        //        return RedirectToAction("Index");
        //    return View(UserInBodyOperation);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(UserInBodyOperation UserInBodyOperation, HttpPostedFileBase Image)
        //{
        //    if (Image != null)
        //    {
        //        bool IsImage = CheckFiles.IsImage(Image);
        //        if (!IsImage)
        //        {
        //            ModelState.AddModelError("", "الصوره غير صحيحة");
        //        }
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        if (Image != null)
        //        {
        //            if (UserInBodyOperation.ImageUrl != null)
        //            {
        //                MediaControl.Delete(FilePath.Category, UserInBodyOperation.ImageUrl);
        //            }
        //            UserInBodyOperation.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
        //        }
        //        db.Entry(UserInBodyOperation).State = EntityState.Modified;
        //        CRUD<UserInBodyOperation>.Update(UserInBodyOperation);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    //ViewBag.Categories = db.Categories.Where(w => w.IsDeleted == false).OrderBy(w => w.NameAr).ToList();
        //    return View(UserInBodyOperation);
        //}


    }
}