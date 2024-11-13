using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.DTOs;
using G4Fit.Models.Enums;
using G4Fit.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [Authorize]
    public class OrdersController : BaseController
    {
        [Authorize(Roles = "Admin")]
        public ActionResult Dashboard()
        {
            return View(db.Orders.Where(s => s.IsDeleted == false && s.OrderStatus != OrderStatus.Initialized && s.UserId != null).OrderByDescending(s => s.CreatedOn));
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ToggleDelivered(long? OrderId)
        {
            if (OrderId.HasValue == false)
            {
                return RedirectToAction("Dashboard");
            }

            var Order = db.Orders.Find(OrderId.Value);
            if (Order.OrderStatus == OrderStatus.Delivered)
            {
                Order.OrderStatus = OrderStatus.Placed;
            }
            else
            {
                Order.OrderStatus = OrderStatus.Delivered;
            }
            db.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult ToggleCanceled(long? OrderId)
        {
            if (OrderId.HasValue == false)
            {
                return RedirectToAction("Dashboard");
            }

            var Order = db.Orders.Find(OrderId.Value);
            if (Order.OrderStatus == OrderStatus.Placed)
            {
                Order.OrderStatus = OrderStatus.Canceled;
            }
            else
            {
                Order.OrderStatus = OrderStatus.Placed;
            }
            db.SaveChanges();
            if (Order.WalletDiscount > 0 && Order.OrderStatus == OrderStatus.Canceled)
            {

                //ارجاع المبلغ الي رصيد المحفظة
                var user = db.Users.Find(Order.UserId);
                user.Wallet += Order.WalletDiscount;
                UserWallet userWallet = new UserWallet()
                {
                    TransactionAmount = Order.WalletDiscount,
                    TransactionType = TransactionType.CheckingoutOrderRefund,
                    UserId = user.Id,
                    OrderId = Order.Id,
                    OrderCode = Order.Code
                };

                db.UserWallets.Add(userWallet);

                db.SaveChanges();
                //
            }
            else if (Order.WalletDiscount > 0 && Order.OrderStatus == OrderStatus.Placed)
            {

                //خصم المبلغ من رصيد المحفظة
                var user = db.Users.Find(Order.UserId);
                user.Wallet -= Order.WalletDiscount;
                UserWallet userWallet = new UserWallet()
                {
                    TransactionAmount = Order.WalletDiscount,
                    TransactionType = TransactionType.CheckingoutOrder,
                    UserId = user.Id,
                    OrderId = Order.Id,
                    OrderCode = Order.Code
                };

                db.UserWallets.Add(userWallet);

                db.SaveChanges();
                //

            }
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult TogglePaid(long? OrderId)
        {
            if (OrderId.HasValue == false)
            {
                return RedirectToAction("Dashboard");
            }

            var Order = db.Orders.Find(OrderId.Value);
            if (Order.PaymentMethod == PaymentMethod.UrWay)
            {
                return RedirectToAction("Dashboard");
            }

            if (Order.IsPaid == true)
            {
                Order.IsPaid = false;
            }
            else
            {
                Order.IsPaid = true;
            }
            db.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(long? OrderId)
        {
            if (OrderId.HasValue == false)
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                var Order = db.Orders.Find(OrderId);
                if (Order != null)
                {
                    return View(Order);
                }
            }
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult TransactionHistory(long? OrderId)
        {
            if (OrderId.HasValue == false)
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                var Order = db.Orders.Find(OrderId);
                if (Order != null)
                {
                    return View(Order);
                }
            }
            return RedirectToAction("Dashboard");
        }

        [AllowAnonymous]
        public async Task<JsonResult> AddToCart(AddItemToBasketDTO toCartDTO)
        {
            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/orders/addtobasket";
            var Token = Request.Cookies["G4Fit-data-token"];
            string TokenValue = null;
            string Anonymous = null;
            HttpCookie anonymousCooky = Request.Cookies["Anonymous"];
            if (anonymousCooky != null)
            {
                Anonymous = anonymousCooky.Value;
            }
            else
            {
                Anonymous = Session.SessionID;
                anonymousCooky = new HttpCookie("Anonymous");
                anonymousCooky.Value = Anonymous;
                anonymousCooky.Expires = DateTime.Now.AddYears(20);
                Response.Cookies.Add(anonymousCooky);
            }

            if (Token != null && User.Identity.IsAuthenticated == true)
            {
                CurrentUserId = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(CurrentUserId))
                {
                    var user = db.Users.Find(CurrentUserId);

                    if (user == null)
                    {
                        return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                    }
                    else if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        return Json(new { Success = false, Message = culture == "ar" ? "عذراً مدير, هذا الحساب لا يمكنه اضافة منتجات للسله, هذه العملية مخصصه للعملاء فقط" : "Sorry Admin, this account cannot add Services to the cart, This process is intended for customers only ." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                }
                TokenValue = Token.Value;
            }
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    httpClient.DefaultRequestHeaders.Add("AnonymousKey", Anonymous);
                    var bodyJS = JsonConvert.SerializeObject(toCartDTO);
                    var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(base_url, body);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                        var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                        return Json(new { Success = false, Message = ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json(new { Success = false, Message = culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AllowAnonymous]
        public async Task<JsonResult> AddTimeBoundServiceToCart(AddTimeBoundServiceItemToBasketDTO toCartDTO)
        {
            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/orders/addTimeBoundServicetobasket";
            var Token = Request.Cookies["G4Fit-data-token"];
            string TokenValue = null;
            string Anonymous = null;
            HttpCookie anonymousCooky = Request.Cookies["Anonymous"];
            if (anonymousCooky != null)
            {
                Anonymous = anonymousCooky.Value;
            }
            else
            {
                Anonymous = Session.SessionID;
                anonymousCooky = new HttpCookie("Anonymous");
                anonymousCooky.Value = Anonymous;
                anonymousCooky.Expires = DateTime.Now.AddYears(20);
                Response.Cookies.Add(anonymousCooky);
            }

            if (Token != null && User.Identity.IsAuthenticated == true)
            {
                CurrentUserId = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(CurrentUserId))
                {
                    var user = db.Users.Find(CurrentUserId);

                    if (user == null)
                    {
                        return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                    }
                    else if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        return Json(new { Success = false, Message = culture == "ar" ? "عذراً مدير, هذا الحساب لا يمكنه اضافة منتجات للسله, هذه العملية مخصصه للعملاء فقط" : "Sorry Admin, this account cannot add Services to the cart, This process is intended for customers only ." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                }

                TokenValue = Token.Value;
            }
            else
            {
                return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
            }
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    httpClient.DefaultRequestHeaders.Add("AnonymousKey", Anonymous);
                    var bodyJS = JsonConvert.SerializeObject(toCartDTO);
                    var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(base_url, body);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                        var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                        return Json(new { Success = false, Message = ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json(new { Success = false, Message = culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> IncreaseItem(long OrderItemId)
        {
            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/orders/increase?BasketItemId=" + OrderItemId;
            var Token = Request.Cookies["G4Fit-data-token"];
            string TokenValue = null;
            if (Token != null && User.Identity.IsAuthenticated == true)
            {
                CurrentUserId = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(CurrentUserId))
                {
                    var user = db.Users.Find(CurrentUserId);

                    if (user == null)
                    {
                        return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                    }
                    else if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        return Json(new { Success = false, Message = culture == "ar" ? "عذراً مدير, هذا الحساب لا يمكنه اضافة منتجات للسله, هذه العملية مخصصه للعملاء فقط" : "Sorry Admin, this account cannot add Services to the cart, This process is intended for customers only ." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                }
                TokenValue = Token.Value;
            }
            string Anonymous = null;
            HttpCookie anonymousCooky = Request.Cookies["Anonymous"];
            if (anonymousCooky != null)
            {
                Anonymous = anonymousCooky.Value;
            }
            else
            {
                Anonymous = Session.SessionID;
                anonymousCooky = new HttpCookie("Anonymous");
                anonymousCooky.Value = Anonymous;
                anonymousCooky.Expires = DateTime.Now.AddYears(20);
                Response.Cookies.Add(anonymousCooky);
            }
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    httpClient.DefaultRequestHeaders.Add("AnonymousKey", Anonymous);
                    var response = await httpClient.GetAsync(base_url);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var OrderItem = db.OrderItems.Find(OrderItemId);
                        return PartialView("_OrderItem", OrderItem);
                    }
                    else
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                        var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                        return Json(new { Success = false, Message = ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json(new { Success = false, Message = culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> DecreaseItem(long OrderItemId)
        {
            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/orders/decrease?BasketItemId=" + OrderItemId;
            var Token = Request.Cookies["G4Fit-data-token"];
            string TokenValue = null;
            if (Token != null && User.Identity.IsAuthenticated == true)
            {
                CurrentUserId = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(CurrentUserId))
                {
                    var user = db.Users.Find(CurrentUserId);

                    if (user == null)
                    {
                        return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                    }
                    else if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        return Json(new { Success = false, Message = culture == "ar" ? "عذراً مدير, هذا الحساب لا يمكنه اضافة منتجات للسله, هذه العملية مخصصه للعملاء فقط" : "Sorry Admin, this account cannot add Services to the cart, This process is intended for customers only ." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                }
                TokenValue = Token.Value;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    httpClient.DefaultRequestHeaders.Add("AnonymousKey", Session.SessionID);
                    var response = await httpClient.GetAsync(base_url);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var OrderItem = db.OrderItems.Find(OrderItemId);
                        return PartialView("_OrderItem", OrderItem);
                    }
                    else
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                        var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                        return Json(new { Success = false, Message = ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json(new { Success = false, Message = culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AllowAnonymous]
        [HttpDelete]
        public async Task<ActionResult> DeleteItem(long OrderItemId)
        {
            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/orders/delete?BasketItemId=" + OrderItemId;
            var Token = Request.Cookies["G4Fit-data-token"];
            string TokenValue = null;
            if (Token != null && User.Identity.IsAuthenticated == true)
            {
                CurrentUserId = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(CurrentUserId))
                {
                    var user = db.Users.Find(CurrentUserId);

                    if (user == null)
                    {
                        return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                    }
                    else if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        return Json(new { Success = false, Message = culture == "ar" ? "عذراً مدير, هذا الحساب لا يمكنه اضافة منتجات للسله, هذه العملية مخصصه للعملاء فقط" : "Sorry Admin, this account cannot add Services to the cart, This process is intended for customers only ." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                }
                TokenValue = Token.Value;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    httpClient.DefaultRequestHeaders.Add("AnonymousKey", Session.SessionID);
                    var response = await httpClient.DeleteAsync(base_url);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var UserOrder = db.Orders.FirstOrDefault(x => x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
                        if (UserOrder == null)
                        {
                            return Json(new { Success = true, Message = "refresh" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { Success = true, Message = "" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                        var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                        return Json(new { Success = false, Message = ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json(new { Success = false, Message = culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult Cart()
        {
            string Anonymous = null;
            HttpCookie anonymousCooky = Request.Cookies["Anonymous"];
            if (anonymousCooky != null)
            {
                Anonymous = anonymousCooky.Value;
            }
            else
            {
                Anonymous = Session.SessionID;
                anonymousCooky = new HttpCookie("Anonymous");
                anonymousCooky.Value = Anonymous;
                anonymousCooky.Expires = DateTime.Now.AddYears(20);
                Response.Cookies.Add(anonymousCooky);
            }
            var UserOrder = db.Orders.FirstOrDefault(x => ((x.UserId == CurrentUserId && x.UserId != null) || x.UnknownUserKeyIdentifier == Anonymous) && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            ApplicationUser user = new ApplicationUser();
            if (UserOrder != null)
            {
                user = db.Users.Find(UserOrder.UserId);
                var ListOfServicesIDs = UserOrder.Items.Where(w => w.IsDeleted == false).Select(w => w.ServiceId).ToList();
                ViewBag.SuggestedServices = db.Services.Where(s => s.IsDeleted == false && s.IsHidden == false && (s.Inventory > 0 || s.IsTimeBoundService) && s.SubCategory.IsDeleted == false && /*s.SubCategory.Category.IsDeleted == false &&*/ ListOfServicesIDs.Contains(s.Id) == false).OrderByDescending(q => q.CreatedOn).Take(4).ToList();
                ViewBag.SimilarServices = db.Services.Where(s => s.IsDeleted == false && s.IsHidden == false && (s.Inventory > 0 || s.IsTimeBoundService) && s.SubCategory.IsDeleted == false && /*s.SubCategory.Category.IsDeleted == false &&*/ ListOfServicesIDs.Contains(s.Id) == false).OrderByDescending(q => q.SellCounter).Take(8).ToList();
            }

            var city = db.Cities.FirstOrDefault(s => s.IsDeleted == false && s.Id == user.CityId);
            if (city != null)
            {
                ViewBag.DeliveryFees = city.DeliveryFees.ToString();
            }
            else
            {
                ViewBag.DeliveryFees = 0.ToString();
            }
            ViewBag.User = user;
            ViewBag.Address = user.Address;
            ViewBag.PhoneNumber = user.PhoneNumber;
            return View(UserOrder);
        }
        [HttpPost]
        public ActionResult UpdateUserData(string Address, string PhoneNumber, PurposeOfSubscription PurposeOfSubscription, double? weight, double? length)
        {
            string Anonymous = null;
            HttpCookie anonymousCooky = Request.Cookies["Anonymous"];
            if (anonymousCooky != null)
            {
                Anonymous = anonymousCooky.Value;
            }
            else
            {
                Anonymous = Session.SessionID;
                anonymousCooky = new HttpCookie("Anonymous");
                anonymousCooky.Value = Anonymous;
                anonymousCooky.Expires = DateTime.Now.AddYears(20);
                Response.Cookies.Add(anonymousCooky);
            }
            var UserOrder = db.Orders.FirstOrDefault(x => ((x.UserId == CurrentUserId && x.UserId != null) || x.UnknownUserKeyIdentifier == Anonymous) && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                return RedirectToAction("Index", "Home");
            }
            UserOrder.PurposeOfSubscription = PurposeOfSubscription;
            var User = db.Users.FirstOrDefault(x => (x.Id == CurrentUserId) && !x.IsDeleted);
            User.Address = Address;
            User.PhoneNumber = PhoneNumber;
            User.weight = weight;
            User.length = length;
            db.SaveChanges();

            return RedirectToAction("Cart");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ApplyCoupon(string Text)
        {
            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/orders/ApplyPromo?Text=" + Text;
            var Token = Request.Cookies["G4Fit-data-token"];
            string TokenValue = null;
            if (Token != null && User.Identity.IsAuthenticated == true)
            {
                CurrentUserId = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(CurrentUserId))
                {
                    var user = db.Users.Find(CurrentUserId);

                    if (user == null)
                    {
                        return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                    }
                    else if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        return Json(new { Success = false, Message = culture == "ar" ? "عذراً مدير, هذا الحساب لا يمكنه اضافة منتجات للسله, هذه العملية مخصصه للعملاء فقط" : "Sorry Admin, this account cannot add Services to the cart, This process is intended for customers only ." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                }
                TokenValue = Token.Value;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    httpClient.DefaultRequestHeaders.Add("AnonymousKey", Session.SessionID);
                    var response = await httpClient.GetAsync(base_url);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return Json(new { Success = true, Message = culture == "ar" ? "تم التفعيل بنجاح" : "Coupon applied successfully" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                        var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                        return Json(new { Success = false, Message = ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json(new { Success = false, Message = culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> RemoveCoupoun()
        {
            string base_url = Request.Url.GetLeftPart(UriPartial.Authority) + "/api/orders/RemovePromo";
            var Token = Request.Cookies["G4Fit-data-token"];
            string TokenValue = null;
            if (Token != null && User.Identity.IsAuthenticated == true)
            {
                CurrentUserId = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(CurrentUserId))
                {
                    var user = db.Users.Find(CurrentUserId);

                    if (user == null)
                    {
                        return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                    }
                    else if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        return Json(new { Success = false, Message = culture == "ar" ? "عذراً مدير, هذا الحساب لا يمكنه اضافة منتجات للسله, هذه العملية مخصصه للعملاء فقط" : "Sorry Admin, this account cannot add Services to the cart, This process is intended for customers only ." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, IsNotLogin = true, Message = culture == "ar" ? "عذراً ، يجب تسجيل الدخول أولاً" : "Please Log in First ." }, JsonRequestBehavior.AllowGet);
                }
                TokenValue = Token.Value;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    httpClient.DefaultRequestHeaders.Add("AnonymousKey", Session.SessionID);
                    var response = await httpClient.GetAsync(base_url);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ResultResponseDTO>(data);
                        var ErrorMessage = APIResponseValidation.Validate(result.ErrorCode, culture);
                        return Json(new { Success = false, Message = ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json(new { Success = false, Message = culture == "ar" ? "عذراً ، حدث خطأ ما" : "Something went wrong" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var UserOrders = db.Orders.Where(x => x.UserId == CurrentUserId && x.OrderStatus != OrderStatus.Initialized && !x.IsDeleted);
            return View(UserOrders);
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            string Anonymous = null;
            HttpCookie anonymousCooky = Request.Cookies["Anonymous"];
            if (anonymousCooky != null)
            {
                Anonymous = anonymousCooky.Value;
            }
            var UserOrder = db.Orders.FirstOrDefault(x => ((x.UserId == CurrentUserId && x.UserId != null) || x.UnknownUserKeyIdentifier == Anonymous) && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                return RedirectToAction("Index", "Home");
            }
            UserOrder.UserId = CurrentUserId;
            var user = db.Users.Find(CurrentUserId);
            db.SaveChanges();
            ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
            ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
            ViewBag.CountryId = user.City.CountryId;
            CheckoutVM model = new CheckoutVM()
            {
                CityId = user.CityId.Value,
                Address = user.Address,

            };
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Checkout(CheckoutVM model)
        {
            var City = db.Cities.FirstOrDefault(s => s.IsDeleted == false && s.Country.IsDeleted == false && s.Id == 1);
            if (City == null)
            {
                ModelState.AddModelError("CityId", culture == "ar" ? "المنطقة المطلوبة غير متاحة" : "City is not available");
                ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
                ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
                return View(model);
            }
            var UserOrder = db.Orders.FirstOrDefault(x => x.UserId != null && x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder != null)
            {
                UserOrder.PaymentMethod = model.PaymentMethod;
                UserOrder.CityId = 1;
                UserOrder.DeliveryFees = City.DeliveryFees == 0 || City.DeliveryFees == null ? 0 : City.DeliveryFees;
                UserOrder.IsPaid = false;
                UserOrder.Address = model.Address;
                UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
                db.SaveChanges();
                if (model.PaymentMethod == PaymentMethod.UrWay)
                {

                    string Url = GetPaymentGatewayUrl();
                    if (string.IsNullOrEmpty(Url))
                    {
                        TempData["PaymentError"] = true;
                        ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
                        ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
                        return View(model);
                    }
                    else
                    {
                        return Redirect(Url);
                    }
                }
                else if (model.PaymentMethod == PaymentMethod.Tabby)
                {

                    var jsonResponse = await GetTabbyCheckoutUrl(UserOrder);
                    var responseObject = JObject.Parse(jsonResponse);

                    // Extract the URL from the response
                    var installments = (JArray)responseObject["configuration"]["available_products"]["installments"];
                    var url = (string)installments[0]["web_url"]; // Assuming there's only one installment
                    if (url != null)
                    {
                        //save Payment Id 
                        var reference_id = (string)responseObject["id"];
                        UserOrder.Tabby_reference_id = reference_id;

                        //save reference_id
                        var paymentId = (string)responseObject["payment"]["id"];
                        UserOrder.Tabby_PaymentId = paymentId;
                        db.SaveChanges();

                        return Redirect(url);
                    }
                    else
                    {
                        TempData["PaymentError"] = true;
                        ViewBag.Countries = db.Countries.Where(s => s.IsDeleted == false).ToList();
                        ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
                        return View(model);
                    }
                }
                else
                {
                    UserOrder.OrderStatus = OrderStatus.Placed;
                    if (UserOrder.PackageId.HasValue == true)
                    {
                        UserOrder.Package.NumberOfTimesUsed += 1;
                    }
                    foreach (var item in UserOrder.Items.Where(d => d.IsDeleted == false))
                    {
                        item.Service.SellCounter += 1;
                    }
                    if (UserOrder.PromoId.HasValue == true)
                    {
                        PromoCodeActions.ExecutePromo(UserOrder, UserOrder.Promo);
                    }
                    db.SaveChanges();
                    if (UserOrder.WalletDiscount > 0)
                    {

                        //خصم المبلغ من رصيد المحفظة
                        var user = db.Users.Find(UserOrder.UserId);
                        user.Wallet -= UserOrder.WalletDiscount;
                        UserWallet userWallet = new UserWallet()
                        {
                            TransactionAmount = UserOrder.WalletDiscount,
                            TransactionType = TransactionType.CheckingoutOrder,
                            UserId = user.Id,
                            OrderId = UserOrder.Id,
                            OrderCode = UserOrder.Code
                        };

                        db.UserWallets.Add(userWallet);

                        db.SaveChanges();
                        //
                    }
                    TempData["OrderPlaced"] = true;
                    TempData["Order"] = UserOrder;
                    return RedirectToAction("Success");
                }
            }
            else
            {
                return RedirectToAction("Cart");
            }
        }
        private async Task<string> GetTabbyCheckoutUrl(Order order)
        {
            try
            {
                using (var client = new HttpClient()) // Create an instance of HttpClient
                {
                    //var domain = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                    var domain = Request.Url.GetLeftPart(UriPartial.Authority);

                    var publicKey = "pk_test_0b12d816-6187-4ce2-b5bb-a4bd1e29dfb6"; // Replace with your public key
                    var secret_key = "sk_test_e65f1167-311a-496c-94de-acb067614827"; // Replace with your Secret key
                    var merchant_code = "g4fitonline";
                    var user = order.User;
                    //var tax = (order.SubTotal * order.StoreOrdersTaxs) / 100;
                    var Address = user.Address;
                    //var Address = db.UserAddresses.FirstOrDefault(s => s.Id == order.UserAddressId);
                    List<TabbyOrderItem> orderItems = new List<TabbyOrderItem>();

                    foreach (var item in order.Items)
                    {
                        var Service = db.Services.FirstOrDefault(w => w.Id == item.ServiceId);
                        var image = db.ServiceImages.FirstOrDefault(w => w.ServiceId == item.ServiceId);
                        var category = db.SubCategories.FirstOrDefault(w => w.Id == Service.SubCategoryId);
                        var size = db.ServiceSizes.FirstOrDefault(w => w.Id == item.SizeId);

                        // Create an anonymous object for each item
                        var orderItem = new TabbyOrderItem
                        {
                            title = Service.NameEn,
                            description = Service.DescriptionEn,
                            quantity = item.Quantity,
                            unit_price = item.Price.ToString(),
                            discount_amount = "0.00",
                            reference_id = item.ServiceId.ToString(),
                            image_url = domain + MediaControl.GetPath(FilePath.Service) + image.ImageUrl,
                            Service_url = "",
                            category = category.NameEn,
                            size = size != null ? size.Size : null,
                        };

                        // Add the item to the list
                        orderItems.Add(orderItem);
                    }
                    var discount = order.PackageDiscount + order.PromoDiscount + order.WalletDiscount;
                    // Define your request data
                    var requestData = new
                    {
                        payment = new
                        {
                            amount = order.Total,
                            currency = "SAR",
                            description = "description",
                            buyer = new
                            {
                                phone = user.PhoneNumber,
                                email = user.Email,
                                name = user.Name,
                                //dob = "2019-08-24"
                            },
                            buyer_history = new
                            {
                                //registered_since = "2019-08-24T14:15:22Z",
                                registered_since = user.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                loyalty_level = 0,
                                wishlist_count = 0,
                                is_social_networks_connected = true,
                                is_phone_number_verified = true,
                                is_email_verified = true
                            },
                            order = new
                            {
                                tax_amount = 0,
                                shipping_amount = order.DeliveryFees,
                                discount_amount = discount,
                                reference_id = order.Id.ToString(),
                                items = orderItems.ToArray() // Convert the list to an array
                            },
                            shipping_address = new
                            {
                                //city = Address.Name,
                                city = user.City.NameEn,
                                address = Address,
                                zip = "-"
                            },
                            order_history = new[] // Add order history
                            {
                    new
                    {
                        purchased_at = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        amount = order.Total,
                        status = "new",
                    }
                },
                            meta = new
                            {
                                order_id = order.Id.ToString(),
                                customer = user.Id.ToString()
                            }

                        },
                        lang = "ar",
                        merchant_code = merchant_code, // Replace with your merchant code
                        merchant_urls = new
                        {
                            success = domain + "/Orders/TabbyconfrimPayment?id=" + order.Id,
                            cancel = domain + "/Orders/Failed",
                            failure = domain + "/Orders/Failed"
                        }
                    };


                    // Set the Authorization header
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", publicKey);

                    // Convert request data to JSON
                    var jsonRequestData = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonRequestData, Encoding.UTF8, "application/json");

                    // Make the API call
                    var response = await client.PostAsync("https://api.tabby.ai/api/v2/checkout", content);


                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var responseObject = JObject.Parse(jsonResponse);

                        // Extract the URL from the response
                        if (responseObject["configuration"] != null &&
                            responseObject["configuration"]["available_products"] != null &&
                            responseObject["configuration"]["available_products"]["installments"] != null)
                        {
                            var installments = (JArray)responseObject["configuration"]["available_products"]["installments"];
                            var webUrl = (string)installments[0]["web_url"]; // Assuming there's only one installment

                            return jsonResponse; // If you need the entire JSON response for further processing or logging
                        }
                        else
                        {
                            Console.WriteLine("Error: Installments data not found in the response.");
                            return null;
                        }
                    }
                    else
                    {
                        // Handle unsuccessful response
                        Console.WriteLine("Error: " + response.ReasonPhrase);
                        return null;
                    }

                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        [AllowAnonymous]
        public PartialViewResult _OrderFees()
        {
            var UserOrder = db.Orders.FirstOrDefault(x => ((x.UserId == CurrentUserId && x.UserId != null) || x.UnknownUserKeyIdentifier == Session.SessionID) && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            return PartialView(UserOrder);
        }

        public string GetPaymentGatewayUrl()
        {
            var UserOrder = db.Orders.FirstOrDefault(x => x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder != null)
            {
                string UserCountry = "Saudi Arabia";
                string UserCity = "Saudi Arabia";

                // Check if the city and country are available from the order
                if (UserOrder.CityId.HasValue)
                {
                    UserCity = !string.IsNullOrEmpty(UserOrder.City.NameEn) ? UserOrder.City.NameEn : UserOrder.City.NameAr;
                    UserCountry = !string.IsNullOrEmpty(UserOrder.City.Country.NameEn) ? UserOrder.City.Country.NameEn : UserOrder.City.Country.NameAr;
                    UserCity = string.IsNullOrEmpty(UserCity) ? "Saudi Arabia" : UserCity;
                    UserCountry = string.IsNullOrEmpty(UserCountry) ? "Saudi Arabia" : UserCountry;
                }

                // Fetch payment gateway details from config
                string TerminalId = ConfigurationManager.AppSettings["TerminalId"];
                string TerminalPassword = ConfigurationManager.AppSettings["TerminalPassword"];
                string Secret = ConfigurationManager.AppSettings["Secret"];

                // Prepare the sequence for generating the hash
                //string Sequence = $"{UserOrder.Code}|{TerminalId}|{TerminalPassword}|{Secret}|{UserOrder.Total}|SAR";
                string Sequence = UserOrder.Code + "|" + TerminalId + "|" + TerminalPassword + "|" + Secret + "|" + UserOrder.Total.ToString() + "|" + "SAR";
                string Hash = PaymentActions.SHA256_HASH(Sequence);

                // Set return URL after payment
                string ReturnUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/Orders/Result?Id=" + UserOrder.Id;

                // Create JSON object for payment request
                JObject Json = PaymentActions.GenerateJson(
                    UserCountry,
                    UserOrder.User.Name,
                    "", "",
                    UserCity,
                    "", "",
                    UserOrder.User.PhoneNumber,
                    UserOrder.User.Email,
                    UserOrder.Total.ToString(),
                    "SAR",
                    "1",
                    Hash,
                    UserOrder.Code,
                    ReturnUrl
                );

                // Generate the final payment URL
                string FinalUrl = PaymentActions.GeneratePaymentUrl(Json);

                return FinalUrl;
            }
            return null;
        }

        public ActionResult Success()
        {
            var UserOrder = TempData["Order"] as G4Fit.Models.Domains.Order;
            if (UserOrder == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(UserOrder);
        }

        public ActionResult Result(long Id, string PaymentId, string TranId, string ECI, string Result, string TrackId, string ResponseCode, string AuthCode, string RRN, string responseHash, string amount, string cardBrand)
        {
            var UserOrder = db.Orders.FirstOrDefault(x => x.Id == Id && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                return RedirectToAction("Cart");
            }
            //if (string.IsNullOrEmpty(PaymentId) || string.IsNullOrEmpty(TranId) || string.IsNullOrEmpty(ECI) || string.IsNullOrEmpty(Result) || string.IsNullOrEmpty(TrackId) || string.IsNullOrEmpty(ResponseCode) || string.IsNullOrEmpty(AuthCode) || string.IsNullOrEmpty(RRN) || string.IsNullOrEmpty(responseHash) || string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(cardBrand))
            //{
            //    return RedirectToAction("Checkout");
            //}

            //PaymentActions.SaveResponseInDatabase(PaymentId, TranId, ECI, Result, TrackId, ResponseCode, AuthCode, RRN, responseHash, amount, cardBrand, TransactionType.CheckingoutOrder, CurrentUserId, UserOrder.Id);
            bool IsPaymentSuccess = PaymentActions.VerifyResponse(TranId, Result, TrackId, ResponseCode, responseHash, amount);
            if (IsPaymentSuccess == true)
            {
                UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
                UserOrder.IsPaid = true;
                UserOrder.OrderStatus = OrderStatus.Placed;
                if (UserOrder.PackageId.HasValue == true)
                {
                    UserOrder.Package.NumberOfTimesUsed += 1;
                }
                foreach (var item in UserOrder.Items.Where(d => d.IsDeleted == false))
                {
                    item.Service.SellCounter += 1;
                }
                if (UserOrder.PromoId.HasValue == true)
                {
                    PromoCodeActions.ExecutePromo(UserOrder, UserOrder.Promo);
                }
                db.SaveChanges();
                TempData["OrderPlaced"] = true;
                TempData["Order"] = UserOrder;
                return RedirectToAction("Success");
            }
            else
            {
                TempData["PaymentFailed"] = PaymentActions.HandleResponseStatusCode(culture, ResponseCode);
                return RedirectToAction("Checkout");
            }
        }
        //public ActionResult Result(string PaymentId, string TranId, string ECI, string Result, string TrackId, string ResponseCode, string AuthCode, string RRN, string responseHash, string amount, string cardBrand)
        //{
        //    var UserOrder = db.Orders.FirstOrDefault(x => x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
        //    if (UserOrder == null)
        //    {
        //        return RedirectToAction("Cart");
        //    }
        //    //if (string.IsNullOrEmpty(PaymentId) || string.IsNullOrEmpty(TranId) || string.IsNullOrEmpty(ECI) || string.IsNullOrEmpty(Result) || string.IsNullOrEmpty(TrackId) || string.IsNullOrEmpty(ResponseCode) || string.IsNullOrEmpty(AuthCode) || string.IsNullOrEmpty(RRN) || string.IsNullOrEmpty(responseHash) || string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(cardBrand))
        //    //{
        //    //    return RedirectToAction("Checkout");
        //    //}

        //    PaymentActions.SaveResponseInDatabase(PaymentId, TranId, ECI, Result, TrackId, ResponseCode, AuthCode, RRN, responseHash, amount, cardBrand, TransactionType.CheckingoutOrder, CurrentUserId, UserOrder.Id);
        //    bool IsPaymentSuccess = PaymentActions.VerifyResponse(TranId, Result, TrackId, ResponseCode, responseHash, amount);
        //    if (IsPaymentSuccess == true)
        //    {
        //        UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
        //        UserOrder.IsPaid = true;
        //        UserOrder.OrderStatus = OrderStatus.Placed;
        //        if (UserOrder.PackageId.HasValue == true)
        //        {
        //            UserOrder.Package.NumberOfTimesUsed += 1;
        //        }
        //        foreach (var item in UserOrder.Items.Where(d => d.IsDeleted == false))
        //        {
        //            item.Service.SellCounter += 1;
        //        }
        //        if (UserOrder.PromoId.HasValue == true)
        //        {
        //            PromoCodeActions.ExecutePromo(UserOrder, UserOrder.Promo);
        //        }
        //        db.SaveChanges();
        //        TempData["OrderPlaced"] = true;
        //        TempData["Order"] = UserOrder;
        //        return RedirectToAction("Success");
        //    }
        //    else
        //    {
        //        TempData["PaymentFailed"] = PaymentActions.HandleResponseStatusCode(culture, ResponseCode);
        //        return RedirectToAction("Checkout");
        //    }
        //}

        #region confirm Payment
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> TabbyconfrimPayment(long id, bool api = false, bool confirm = true)
        {
            if (confirm)
            {
                var UserOrder = db.Orders
                  .FirstOrDefault(w => w.Id == id && w.OrderStatus == OrderStatus.Initialized && !w.IsDeleted);
                UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
                UserOrder.IsPaid = true;
                UserOrder.OrderStatus = OrderStatus.Placed;
                if (UserOrder.PackageId.HasValue == true)
                {
                    UserOrder.Package.NumberOfTimesUsed += 1;
                }
                var Items = db.OrderItems.Where(d => !d.IsDeleted && d.OrderId == UserOrder.Id).ToList();
                foreach (var item in Items)
                {
                    item.Service.SellCounter += 1;
                }
                if (UserOrder.PromoId.HasValue == true)
                {
                    PromoCodeActions.ExecutePromo(UserOrder, UserOrder.Promo);
                }
                db.SaveChanges();
                if (UserOrder.PaymentMethod == PaymentMethod.Tabby)
                {
                    var b = await TabbyCapturepayment(UserOrder, this.Request);
                }
                TempData["OrderPlaced"] = true;
                TempData["Order"] = UserOrder;
                if (api)
                {
                    return RedirectToAction("ApiSuccess");
                }
                return RedirectToAction("Success");
            }

            TempData["PaymentFailed"] = PaymentActions.HandleResponseStatusCode(culture, "Payment Failed");
            string err = PaymentActions.HandleResponseStatusCode(culture, "Payment Failed");
            if (api)
            {
                return RedirectToAction("Failed", new { error = err });
            }
            return RedirectToAction("Checkout");

        }
        #endregion

        private async Task<bool> TabbyCapturepayment(Order order, HttpRequestBase request)
        {
            try
            {
                HttpClient _client = new HttpClient(); ;

                //var domain = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                var domain = $"{request.Url.Scheme}://{request.Url.Host}:{request.Url.Port}";
                var publicKey = "pk_test_0b12d816-6187-4ce2-b5bb-a4bd1e29dfb6"; // Replace with your public key
                var secret_key = "sk_test_e65f1167-311a-496c-94de-acb067614827"; // Replace with your Secret key
                var merchant_code = "g4fitonline";
                var user = order.User;
                //var tax = (order.SubTotal * order.StoreOrdersTaxs) / 100;
                var Address = user.Address;
                List<TabbyOrderItem> orderItems = new List<TabbyOrderItem>();

                foreach (var item in order.Items)
                {
                    var Service = db.Services.FirstOrDefault(w => w.Id == item.ServiceId);
                    var image = db.ServiceImages.FirstOrDefault(w => w.ServiceId == item.ServiceId);
                    var category = db.SubCategories.FirstOrDefault(w => w.Id == Service.SubCategoryId);
                    var size = db.ServiceSizes.FirstOrDefault(w => w.Id == item.SizeId);

                    // Create an anonymous object for each item
                    var orderItem = new TabbyOrderItem
                    {
                        title = Service.NameEn,
                        description = Service.DescriptionEn,
                        quantity = item.Quantity,
                        unit_price = item.Price.ToString(),
                        discount_amount = "0.00",
                        reference_id = item.ServiceId.ToString(),
                        image_url = domain + MediaControl.GetPath(FilePath.Service) + image.ImageUrl,
                        Service_url = "",
                        category = category.NameEn,
                        size = size != null ? size.Size : null,
                    };

                    // Add the item to the list
                    orderItems.Add(orderItem);
                }

                // Define your request data
                var requestData = new
                {
                    amount = order.Total.ToString(),
                    //reference_id = order.Tabby_reference_id,
                    //tax_amount = tax.ToString(),
                    //shipping_amount = order.DeliveryFees.ToString(),
                    //discount_amount = "0.00",
                    //created_at = created_at_formatted,
                    //items = orderItems
                };
                // Convert request data to JSON
                var jsonRequestData = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequestData, Encoding.UTF8, "application/json");
                string apiUrl = $"https://api.tabby.ai/api/v2/payments/{order.Tabby_PaymentId}/captures";

                // Set the Authorization header
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secret_key);

                // Make the API call
                var response = await _client.PostAsync(apiUrl, content);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(jsonResponse);

                    var status = (string)responseObject["status"];
                    /*	
                    Enum: "CREATED" "AUTHORIZED" "CLOSED" "REJECTED" "EXPIRED"
                    Status of the current payment.
                    Kindly note that "created" means that the payment is created successfully, but not finished yet; 
                    "authorized" and "closed" mark the succesfully approved and captured payments accordingly;
                    "rejected" is retuned when a customer is rejected during Tabby Checkout 
                    and "expired" is used when a customer cancels a payment or 
                    when Tabby doesn't receive a successfully paid transaction after timeout.
                    */

                    if (status == "created" || status == "authorized" || status == "closed")
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    // Handle unsuccessful response
                    Console.WriteLine("Error: " + response.ReasonPhrase);
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }


        public ActionResult ResultApi(long Id, string PaymentId, string TranId, string ECI, string Result, string TrackId, string ResponseCode, string AuthCode, string RRN, string responseHash, string amount, string cardBrand)
        {
            var UserOrder = db.Orders.FirstOrDefault(x => x.Id == Id && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                return RedirectToAction("Cart");
            }
            //if (string.IsNullOrEmpty(PaymentId) || string.IsNullOrEmpty(TranId) || string.IsNullOrEmpty(ECI) || string.IsNullOrEmpty(Result) || string.IsNullOrEmpty(TrackId) || string.IsNullOrEmpty(ResponseCode) || string.IsNullOrEmpty(AuthCode) || string.IsNullOrEmpty(RRN) || string.IsNullOrEmpty(responseHash) || string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(cardBrand))
            //{
            //    return RedirectToAction("Checkout");
            //}

            //PaymentActions.SaveResponseInDatabase(PaymentId, TranId, ECI, Result, TrackId, ResponseCode, AuthCode, RRN, responseHash, amount, cardBrand, TransactionType.CheckingoutOrder, CurrentUserId, UserOrder.Id);
            bool IsPaymentSuccess = PaymentActions.VerifyResponse(TranId, Result, TrackId, ResponseCode, responseHash, amount);
            if (IsPaymentSuccess == true)
            {
                UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
                UserOrder.IsPaid = true;
                UserOrder.OrderStatus = OrderStatus.Placed;
                if (UserOrder.PackageId.HasValue == true)
                {
                    UserOrder.Package.NumberOfTimesUsed += 1;
                }
                foreach (var item in UserOrder.Items.Where(d => d.IsDeleted == false))
                {
                    item.Service.SellCounter += 1;
                }
                if (UserOrder.PromoId.HasValue == true)
                {
                    PromoCodeActions.ExecutePromo(UserOrder, UserOrder.Promo);
                }
                db.SaveChanges();
                TempData["OrderPlaced"] = true;
                TempData["Order"] = UserOrder;
                return RedirectToAction("ApiSuccess");
            }
            else
            {
                TempData["PaymentFailed"] = PaymentActions.HandleResponseStatusCode(culture, ResponseCode);
                string err = PaymentActions.HandleResponseStatusCode(culture, ResponseCode);

                return RedirectToAction("Failed", new { error = err });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ApiSuccess()
        {

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Failed(string error)
        {
            TempData["ErrorMessage"] = error;
            return View();
        }
        public ActionResult Invoice(long? OrderId)
        {
            var Order = db.Orders.FirstOrDefault(s => s.Id == OrderId && s.OrderStatus == OrderStatus.Placed && s.UserId == CurrentUserId);
            if (Order == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.About = db.CompanyDatas.FirstOrDefault(s => s.IsDeleted == false);
            return View(Order);
        }

        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
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
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        public class TabbyOrderItem
        {
            public string title { get; set; }
            public string description { get; set; }
            public int quantity { get; set; }
            public string unit_price { get; set; }
            public string discount_amount { get; set; }
            public string reference_id { get; set; }
            public string image_url { get; set; }
            public string Service_url { get; set; }
            public string gender { get; set; }
            public string category { get; set; }
            public string color { get; set; }
            public string Service_material { get; set; }
            public string size_type { get; set; }
            public string size { get; set; }
            public string brand { get; set; }
        }

    }
}