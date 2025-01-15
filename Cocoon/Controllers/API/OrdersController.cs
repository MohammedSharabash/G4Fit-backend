using G4Fit.Helpers;
using G4Fit.Models.Data;
using G4Fit.Models.Domains;
using G4Fit.Models.DTOs;
using G4Fit.Models.Enums;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using static G4Fit.Controllers.MVC.OrdersController;

namespace G4Fit.Controllers.API
{
    [Authorize]
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;
        private string CurrentUserId;
        private UserManager<ApplicationUser> UserManager;

        public OrdersController()
        {
            baseResponse = new BaseResponseDTO();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("addtobasket")]
        public IHttpActionResult AddItemToBasket(AddItemToBasketDTO model)
        {
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                if (Headers.AllKeys.Contains("AnonymousKey") && Headers.GetValues("AnonymousKey").Any())
                    AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            CurrentUserId = User.Identity.GetUserId();
            ApplicationUser user = new ApplicationUser();
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                user = UserManager.FindById(CurrentUserId);
                if (user == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotAuthorized;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            var Validation = ValidateAddItemToBasket(model, CurrentUserId);
            if (Validation != Errors.Success)
            {
                baseResponse.ErrorCode = Validation;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false /*&& string.IsNullOrEmpty(AnonymousKey)*/)
            {
                baseResponse.ErrorCode = Errors.UserIdentityIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Service = db.Services.Find(model.ServiceId);
            var Package = db.UserPackages.FirstOrDefault(w => w.IsDeleted == false && w.IsActive == true && w.UserId == CurrentUserId && w.IsPaid == true);
            Order UserOrder = null;
            if (!string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == true)
            {
                UserOrder = db.Orders.FirstOrDefault(x => x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            }
            else
            {
                UserOrder = db.Orders.FirstOrDefault(x => /*x.UnknownUserKeyIdentifier == AnonymousKey &&*/ x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            }
            user = UserManager.FindById(CurrentUserId);

            var city = db.Cities.FirstOrDefault(s => s.IsDeleted == false && s.Id == user.CityId);
            var DeliveryFees = city?.DeliveryFees;
            if (UserOrder == null)
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        string Code = RandomGenerator.GenerateNumber(10000, 99999).ToString();
                        Order order = new Order()
                        {
                            UserId = CurrentUserId,
                            Code = Code,
                            DeliveryFees = DeliveryFees ?? 0,
                            InBodyCount = Service.InBodyCount,
                            UnknownUserKeyIdentifier = AnonymousKey
                        };
                        if (Package != null)
                        {
                            order.PackageId = Package.Id;
                        }
                        db.Orders.Add(order);

                        OrderItem orderItem = new OrderItem()
                        {
                            OrderId = order.Id,
                            Price = Service.OfferPrice.HasValue == true ? Service.OfferPrice.Value : Service.OriginalPrice,
                            ServiceId = model.ServiceId,
                            Quantity = model.Quantity,
                            SubTotal = Service.OfferPrice.HasValue == true ? model.Quantity * Service.OfferPrice.Value : model.Quantity * Service.OriginalPrice,
                        };
                        if (model.SizeId.HasValue && model.SizeId.Value > 0)
                        {
                            orderItem.SizeId = model.SizeId;
                        }
                        if (model.ColorId.HasValue && model.ColorId.Value > 0)
                        {
                            orderItem.ColorId = model.ColorId;
                        }

                        order.Items = new List<OrderItem>();
                        order.Items.Add(orderItem);
                        OrderActions.CalculateOrderPrice(order);
                        db.SaveChanges();
                        Transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.SomethingIsWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
            }
            else
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var OrderItem = db.OrderItems.FirstOrDefault(x => x.OrderId == UserOrder.Id && x.ServiceId == model.ServiceId && !x.IsDeleted && x.SizeId == model.SizeId && x.ColorId == model.ColorId);
                        if (OrderItem == null)
                        {
                            OrderItem orderItem = new OrderItem()
                            {
                                OrderId = UserOrder.Id,
                                Price = Service.OfferPrice.HasValue == true ? Service.OfferPrice.Value : Service.OriginalPrice,
                                ServiceId = model.ServiceId,
                                Quantity = model.Quantity,
                                SubTotal = Service.OfferPrice.HasValue == true ? model.Quantity * Service.OfferPrice.Value : model.Quantity * Service.OriginalPrice,
                            };
                            if (model.SizeId.HasValue && model.SizeId.Value > 0)
                            {
                                orderItem.SizeId = model.SizeId;
                            }
                            if (model.ColorId.HasValue && model.ColorId.Value > 0)
                            {
                                orderItem.ColorId = model.ColorId;
                            }
                            UserOrder.Items.Add(orderItem);
                        }
                        else
                        {
                            OrderItem.Quantity += model.Quantity;
                            OrderItem.SubTotal = OrderItem.Price * OrderItem.Quantity;
                            CRUD<OrderItem>.Update(OrderItem);
                        }

                        UserOrder.InBodyCount = (Service.InBodyCount > UserOrder.InBodyCount) ? Service.InBodyCount : UserOrder.InBodyCount;
                        CRUD<Order>.Update(UserOrder);
                        OrderActions.CalculateOrderPrice(UserOrder);
                        db.SaveChanges();
                        Transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.SomethingIsWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
            }
            return Ok(baseResponse);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("AddTimeBoundServiceToCart")]
        public IHttpActionResult AddTimeBoundServiceItemToBasket(AddTimeBoundServiceItemToBasketDTO model)
        {
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                if (Headers.AllKeys.Contains("AnonymousKey") && Headers.GetValues("AnonymousKey").Any())
                    AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            CurrentUserId = User.Identity.GetUserId();
            ApplicationUser user = new ApplicationUser();
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                user = UserManager.FindById(CurrentUserId);
                if (user == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotAuthorized;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            var Validation = ValidateAddTimeBoundServiceItemToBasket(model, CurrentUserId);
            if (Validation != Errors.Success)
            {
                baseResponse.ErrorCode = Validation;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false /*&& string.IsNullOrEmpty(AnonymousKey)*/)
            {
                baseResponse.ErrorCode = Errors.UserIdentityIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Service = db.Services.Find(model.ServiceId);
            var Package = db.UserPackages.FirstOrDefault(w => w.IsDeleted == false && w.IsActive == true && w.UserId == CurrentUserId && w.IsPaid == true);
            Order UserOrder = null;
            if (!string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == true)
            {
                UserOrder = db.Orders.FirstOrDefault(x => x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            }
            else
            {
                UserOrder = db.Orders.FirstOrDefault(x => /*x.UnknownUserKeyIdentifier == AnonymousKey &&*/ x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            }
            user = UserManager.FindById(CurrentUserId);

            var city = db.Cities.FirstOrDefault(s => s.IsDeleted == false && s.Id == user.CityId);
            var DeliveryFees = city?.DeliveryFees;
            if (UserOrder == null)
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        string Code = RandomGenerator.GenerateNumber(10000, 99999).ToString();
                        Order order = new Order()
                        {
                            UserId = CurrentUserId,
                            Code = Code,
                            DeliveryFees = DeliveryFees ?? 0,
                            UnknownUserKeyIdentifier = AnonymousKey
                        };
                        if (Package != null)
                        {
                            order.PackageId = Package.Id;
                        }
                        db.Orders.Add(order);

                        OrderItem orderItem = new OrderItem()
                        {
                            OrderId = order.Id,
                            Price = Service.OfferPrice.HasValue == true ? Service.OfferPrice.Value : Service.OriginalPrice,
                            ServiceId = model.ServiceId,
                            ColorId = model.trainerId,
                            Quantity = Service.ServiceDays,
                            StartDate = model.StartDate,
                            SubTotal = Service.OfferPrice.HasValue == true ? Service.OfferPrice.Value : Service.OriginalPrice,
                        };

                        order.Items = new List<OrderItem>();
                        order.Items.Add(orderItem);
                        OrderActions.CalculateOrderPrice(order);
                        db.SaveChanges();
                        Transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.SomethingIsWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
            }
            else
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var OrderItem = db.OrderItems.FirstOrDefault(x => x.OrderId == UserOrder.Id && x.ServiceId == model.ServiceId && !x.IsDeleted);
                        if (OrderItem == null)
                        {
                            OrderItem orderItem = new OrderItem()
                            {
                                OrderId = UserOrder.Id,
                                Price = Service.OfferPrice.HasValue == true ? Service.OfferPrice.Value : Service.OriginalPrice,
                                ServiceId = model.ServiceId,
                                ColorId = model.trainerId,
                                Quantity = Service.ServiceDays,
                                StartDate = model.StartDate,
                                SubTotal = Service.OfferPrice.HasValue == true ? Service.OfferPrice.Value : Service.OriginalPrice,
                            };

                            UserOrder.Items.Add(orderItem);
                        }

                        CRUD<Order>.Update(UserOrder);
                        OrderActions.CalculateOrderPrice(UserOrder);
                        db.SaveChanges();
                        Transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.SomethingIsWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
            }
            return Ok(baseResponse);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("basket")]
        public IHttpActionResult GetBasketDetails(string lang = "en")
        {
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                if (Headers.AllKeys.Contains("AnonymousKey") && Headers.GetValues("AnonymousKey").Any())
                    AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            ApplicationUser user = null;
            CurrentUserId = User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                user = UserManager.FindById(CurrentUserId);
                if (user == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotAuthorized;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false/* && string.IsNullOrEmpty(AnonymousKey)*/)
            {
                return Ok(baseResponse);
            }

            Order UserOrder = null;
            if (!string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == true)
            {
                UserOrder = db.Orders.FirstOrDefault(x => x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            }
            else
            {
                UserOrder = db.Orders.FirstOrDefault(x =>/* x.UnknownUserKeyIdentifier == AnonymousKey &&*/ x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            }

            if (UserOrder == null)
            {
                return Ok(baseResponse);
            }

            BasketDetailsDTO basketDetailsDTO = new BasketDetailsDTO()
            {
                SubTotal = UserOrder.SubTotal.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR"),
                Total = UserOrder.Total.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR"),
            };

            if (user == null)
            {
                basketDetailsDTO.UserStatus = 4; // user must login
            }
            else if (user != null && (user.CountryId.HasValue == false || user.PhoneNumber == null))
            {
                basketDetailsDTO.UserStatus = 1; // user must have country and phone.
            }
            else if (user != null && user.PhoneNumber != null && user.PhoneNumberConfirmed == false)
            {
                basketDetailsDTO.UserStatus = 2; // user must have verify his phone.
            }
            else
            {
                basketDetailsDTO.UserStatus = 0; // Ok
            }

            if (UserOrder.Items != null)
            {
                foreach (var item in UserOrder.Items.Where(d => d.IsDeleted == false))
                {
                    BasketItemDTO basketItemDTO = new BasketItemDTO()
                    {
                        ServiceId = item.ServiceId,
                        Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Service.DescriptionAr : item.Service.DescriptionEn,
                        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Service.NameAr : item.Service.NameEn,
                        BasketItemId = item.Id,
                        Image = item.Service.Images != null && item.Service.Images.FirstOrDefault(d => d.IsDeleted == false) != null ? "/Content/Images/Services/" + item.Service.Images.FirstOrDefault(d => d.IsDeleted == false).ImageUrl : null,
                        Price = item.Price.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR"),
                        IsTimeBoundService = item.Service.IsTimeBoundService,
                        Quantity = item.Quantity,
                        Color = item.ColorId.HasValue == true ? item.Color.Color : null,
                        Size = item.SizeId.HasValue == true ? item.Size.Size : null,
                    };
                    basketDetailsDTO.BasketItems.Add(basketItemDTO);
                }
            }

            baseResponse.Data = basketDetailsDTO;
            return Ok(baseResponse);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("increase")]
        public IHttpActionResult IncreaseBasketItem(long BasketItemId)
        {
            var Headers = HttpContext.Current.Request.Headers;
            CurrentUserId = User.Identity.GetUserId();
            string AnonymousKey = string.Empty;
            try
            {
                if (Headers.AllKeys.Contains("AnonymousKey") && Headers.GetValues("AnonymousKey").Any())
                    AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false /*&& string.IsNullOrEmpty(AnonymousKey)*/)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
            }

            var BasketItem = db.OrderItems.FirstOrDefault(x => x.Id == BasketItemId && !x.IsDeleted && !x.Order.IsDeleted && (x.Order.UserId == CurrentUserId /*|| x.Order.UnknownUserKeyIdentifier == AnonymousKey*/));
            if (BasketItem == null)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (BasketItem.Order.OrderStatus != OrderStatus.Initialized)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            BasketItem.Quantity += 1;
            BasketItem.SubTotal = BasketItem.Price * BasketItem.Quantity;
            CRUD<OrderItem>.Update(BasketItem);
            CRUD<Order>.Update(BasketItem.Order);
            OrderActions.CalculateOrderPrice(BasketItem.Order);
            db.SaveChanges();
            baseResponse.Data = new
            {
                NewBasketItemSubTotal = BasketItem.SubTotal
            };
            return Ok(baseResponse);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("decrease")]
        public IHttpActionResult DecreaseBasketItem(long BasketItemId)
        {
            CurrentUserId = User.Identity.GetUserId();
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                if (Headers.AllKeys.Contains("AnonymousKey") && Headers.GetValues("AnonymousKey").Any())
                    AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false /*&& string.IsNullOrEmpty(AnonymousKey)*/)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
            }

            var BasketItem = db.OrderItems.FirstOrDefault(x => x.Id == BasketItemId && !x.IsDeleted && !x.Order.IsDeleted && (x.Order.UserId == CurrentUserId /*|| x.Order.UnknownUserKeyIdentifier == AnonymousKey*/));
            if (BasketItem == null)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (BasketItem.Order.OrderStatus != OrderStatus.Initialized)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (BasketItem.Quantity > 1)
            {
                BasketItem.Quantity -= 1;
                BasketItem.SubTotal = BasketItem.Price * BasketItem.Quantity;
                CRUD<OrderItem>.Update(BasketItem);
                CRUD<Order>.Update(BasketItem.Order);
                OrderActions.CalculateOrderPrice(BasketItem.Order);
                db.SaveChanges();
                baseResponse.Data = new
                {
                    NewBasketItemSubTotal = BasketItem.SubTotal
                };
                return Ok(baseResponse);
            }
            else
            {
                baseResponse.ErrorCode = Errors.CannotDecrease;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
        }

        [AllowAnonymous]
        [HttpDelete]
        [Route("delete")]
        public IHttpActionResult DeleteBasketItem(long BasketItemId)
        {
            CurrentUserId = User.Identity.GetUserId();
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                if (Headers.AllKeys.Contains("AnonymousKey") && Headers.GetValues("AnonymousKey").Any())
                    AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false /*&& string.IsNullOrEmpty(AnonymousKey)*/)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
            }

            var BasketItem = db.OrderItems.FirstOrDefault(x => x.Id == BasketItemId && !x.IsDeleted && !x.Order.IsDeleted && (x.Order.UserId == CurrentUserId /*|| x.Order.UnknownUserKeyIdentifier == AnonymousKey*/));
            if (BasketItem == null)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (BasketItem.Order.OrderStatus != OrderStatus.Initialized)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            CRUD<Order>.Update(BasketItem.Order);
            CRUD<OrderItem>.Delete(BasketItem);
            OrderActions.CalculateOrderPrice(BasketItem.Order);
            if (BasketItem.Order.Items.All(d => d.IsDeleted))
            {
                CRUD<Order>.Delete(BasketItem.Order);
            }
            db.SaveChanges();
            return Ok(baseResponse);
        }

        #region update Data
        [HttpPut]
        [Route("updatedata")]
        public IHttpActionResult UpdateUserData(PurposeOfSubscription PurposeOfSubscription, double? weight, double? length)
        {
            CurrentUserId = User.Identity.GetUserId();
            var Headers = HttpContext.Current.Request.Headers;


            ApplicationUser user = null;
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                user = UserManager.FindById(CurrentUserId);
                if (user == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotAuthorized;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            if (user != null && (user.CountryId.HasValue == false || user.PhoneNumber == null))
            {
                baseResponse.ErrorCode = Errors.UserDoesNotHaveCountry;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (user != null && user.PhoneNumberConfirmed == false)
            {
                baseResponse.ErrorCode = Errors.UserNotVerified;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            user.weight = weight != null ? weight : user.weight;
            user.length = length != null ? length : user.length;
            db.SaveChanges();

            var UserOrder = db.Orders.FirstOrDefault(x => ((x.UserId == CurrentUserId && x.UserId != null) /*|| x.UnknownUserKeyIdentifier == AnonymousKey*/) && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);

            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserBasketIsEmpty;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            UserOrder.PurposeOfSubscription = PurposeOfSubscription;
            db.SaveChanges();
            return Ok(baseResponse);

        }

        #endregion

        [AllowAnonymous]
        [HttpGet]
        [Route("checkout")]
        public async Task<IHttpActionResult> CheckOut(CheckOutDTO model)
        {
            if (model.type != OrderUserType.Normal && model.ImageBase64.IsNullOrWhiteSpace())
            {
                baseResponse.ErrorCode = Errors.FailedToUploadImage;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            CurrentUserId = User.Identity.GetUserId();
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                if (Headers.AllKeys.Contains("AnonymousKey") && Headers.GetValues("AnonymousKey").Any())
                    AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false /*&& string.IsNullOrEmpty(AnonymousKey)*/)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            ApplicationUser user = null;
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                user = UserManager.FindById(CurrentUserId);
                if (user == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotAuthorized;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            if (user != null && (user.CountryId.HasValue == false || user.PhoneNumber == null))
            {
                baseResponse.ErrorCode = Errors.UserDoesNotHaveCountry;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (user != null && user.PhoneNumberConfirmed == false)
            {
                baseResponse.ErrorCode = Errors.UserNotVerified;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var UserOrder = db.Orders.FirstOrDefault(x => ((x.UserId == CurrentUserId && x.UserId != null) /*|| x.UnknownUserKeyIdentifier == AnonymousKey*/) && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);

            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserBasketIsEmpty;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            string ImageName = null;
            if (!string.IsNullOrEmpty(model.ImageBase64))
            {
                var Image = Convert.FromBase64String(model.ImageBase64);
                ImageName = MediaControl.Upload(FilePath.Other, Image, MediaType.Image);
                UserOrder.UserTypeImageUrl = ImageName;
            }
            if (model.type != UserOrder.UserType)
            {
                UserOrder.UserType = model.type;
                foreach (var item in UserOrder.Items)
                {
                    if (UserOrder.UserType == OrderUserType.Normal)
                        item.Price = item.Service.OfferPrice.HasValue == true ? item.Service.OfferPrice.Value : item.Service.OriginalPrice;

                    else
                        item.Price = item.Service.SpecialOfferPrice.HasValue == true ? item.Service.SpecialOfferPrice.Value : item.Service.SpecialPrice;
                    db.SaveChanges();
                    if (item.Service.IsTimeBoundService)
                        item.SubTotal = item.Price;
                    else
                        item.SubTotal = item.Price * item.Quantity;
                    db.SaveChanges();
                }

                OrderActions.CalculateOrderPrice(UserOrder);
                db.SaveChanges();
            }
            var Package = db.UserPackages.FirstOrDefault(w => w.IsDeleted == false && w.IsActive == true && w.UserId == CurrentUserId && w.IsPaid == true);
            if (Package != null)
            {
                UserOrder.PackageId = Package.Id;
                UserOrder.Package = Package;
                OrderActions.CalculateOrderPrice(UserOrder);
                db.SaveChanges();
            }
            CheckOutPageDataDTO basketDetailsDTO = new CheckOutPageDataDTO()
            {
                DeliveryFees = UserOrder.DeliveryFees,
                Discount = UserOrder.PackageDiscount + UserOrder.PromoDiscount,
                SubTotal = UserOrder.SubTotal,
                Total = UserOrder.Total,
            };

            if (UserOrder.PackageId.HasValue == true)
            {
                basketDetailsDTO.IsHavePackageDiscount = true;
                basketDetailsDTO.PackageDiscount = UserOrder.PackageDiscount;
            }

            if (UserOrder.PromoId.HasValue == true)
            {
                basketDetailsDTO.IsHavePromoDiscount = true;
                basketDetailsDTO.PromoDiscount = UserOrder.PromoDiscount;
                basketDetailsDTO.PromoText = UserOrder.Promo.Text;
            }

            if (UserOrder.WalletDiscount > 0)
            {
                basketDetailsDTO.IsHaveWalletDiscount = true;
                basketDetailsDTO.WalletDiscount = UserOrder.WalletDiscount;
            }

            if (UserOrder.Items != null && UserOrder.Items.Count(s => s.IsDeleted == false) > 0)
            {
                //long CategoryId = UserOrder.Items.FirstOrDefault(w => w.IsDeleted == false).Service.SubCategory.SubCategoryId;
                long CategoryId = UserOrder.Items.FirstOrDefault(w => w.IsDeleted == false).Service.SubCategoryId;
                var SimilarServices = db.Services.Where(s => s.IsDeleted == false && s.IsHidden == false && (s.Inventory > 0 || s.IsTimeBoundService) && s.SubCategoryId == CategoryId).OrderByDescending(s => s.SellCounter).Take(6).ToList();
                foreach (var Service in SimilarServices)
                {
                    ServiceDTO ServiceDTO = new ServiceDTO()
                    {
                        Id = Service.Id,
                        Description = !string.IsNullOrEmpty(model.lang) && model.lang.ToLower() == "ar" ? Service.DescriptionAr : Service.DescriptionEn,
                        Name = !string.IsNullOrEmpty(model.lang) && model.lang.ToLower() == "ar" ? Service.NameAr : Service.NameEn,
                        HasDiscount = Service.OfferPrice.HasValue == true ? true : false,
                        Price = Service.OriginalPrice.ToString() + (!string.IsNullOrEmpty(model.lang) && model.lang.ToLower() == "ar" ? " ريال سعودي" : "SAR"),
                        PriceAfter = Service.OfferPrice.HasValue == true ? Service.OfferPrice.Value.ToString() + (!string.IsNullOrEmpty(model.lang) && model.lang.ToLower() == "ar" ? " ريال سعودي" : "SAR") : null,
                        Image = Service.Images != null && Service.Images.FirstOrDefault(s => s.IsDeleted == false) != null ? "/Content/Images/Services/" + Service.Images.FirstOrDefault(s => s.IsDeleted == false).ImageUrl : null,
                        IsFavourite = Service.FavouriteUsers != null && Service.FavouriteUsers.Any(s => s.IsDeleted == false && s.UserId == CurrentUserId) == true ? true : false
                    };
                    if (Service.Offers != null)
                    {
                        var Offer = Service.Offers.FirstOrDefault(s => s.IsDeleted == false && s.IsFinished == false);
                        if (Offer != null)
                        {
                            ServiceDTO.DiscountPercentage = Offer.Percentage;
                        }
                    }
                    basketDetailsDTO.SimilarServices.Add(ServiceDTO);
                }
            }

            baseResponse.Data = basketDetailsDTO;
            return Ok(baseResponse);
        }

        [HttpPost]
        [Route("checkout")]
        public async Task<IHttpActionResult> CheckOut(CheckOutOrderDTO model)
        {

            string CurrentUserId = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)?.Value;

            #region user data
            var Headers = HttpContext.Current.Request.Headers;

            ApplicationUser userData = null;
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                userData = UserManager.FindById(CurrentUserId);
                if (userData == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotAuthorized;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            if (userData != null && (userData.CountryId.HasValue == false || userData.PhoneNumber == null))
            {
                baseResponse.ErrorCode = Errors.UserDoesNotHaveCountry;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (userData != null && userData.PhoneNumberConfirmed == false)
            {
                baseResponse.ErrorCode = Errors.UserNotVerified;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            userData.weight = model.weight != null ? model.weight : userData.weight;
            userData.length = model.length != null ? model.length : userData.length;
            db.SaveChanges();
            #endregion

            var UserOrder = db.Orders.FirstOrDefault(x => x.UserId != null && x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserBasketIsEmpty;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            UserOrder.PurposeOfSubscription = model.PurposeOfSubscription;
            db.SaveChanges();

            var Validation = ValidateBasketItems(UserOrder);
            if (Validation != Errors.Success)
            {
                if (Validation == Errors.ServiceNotFound)
                {
                    foreach (var item in UserOrder.Items.Where(d => d.IsDeleted == false))
                    {
                        if (item.Service.IsDeleted == true || item.Service.IsHidden == true || item.Service.SubCategory.IsDeleted == true /*|| item.Service.SubCategory.Category.IsDeleted == true*/)
                        {
                            baseResponse.ErrorCode = Errors.ServiceNotFound;
                            baseResponse.Data = new { NameAr = item.Service.NameAr, NameEn = item.Service.NameEn, item.ServiceId };
                            return Content(HttpStatusCode.BadRequest, baseResponse);
                        }
                    }
                }
                baseResponse.ErrorCode = Validation;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //var City = db.Cities.FirstOrDefault(s => s.IsDeleted == false && s.Country.IsDeleted == false && s.Id == model.CityId);
                    //if (City == null)
                    //{
                    //    baseResponse.ErrorCode = Errors.CityNotFound;
                    //    return Content(HttpStatusCode.BadRequest, baseResponse);
                    //}
                    UserOrder.PaymentMethod = model.PaymentMethod;
                    UserOrder.CityId = 1;
                    //UserOrder.DeliveryFees = City.DeliveryFees == 0 || City.DeliveryFees == null ? 0 : City.DeliveryFees;
                    UserOrder.DeliveryFees = 0;
                    UserOrder.IsPaid = false;
                    UserOrder.Address = model.Address;
                    UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
                    db.SaveChanges();
                    if (model.PaymentMethod == PaymentMethod.UrWay)
                    {
                        return GetPaymentGatewayUrl();
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
                            baseResponse.Data = url;
                            return Ok(baseResponse);
                        }
                        else
                        {
                            baseResponse.ErrorCode = Errors.SomethingIsWrong;
                            return Content(HttpStatusCode.BadRequest, baseResponse);
                        }
                    }

                    else if (model.PaymentMethod == PaymentMethod.Cash)
                    {
                        UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
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

                        CRUD<Order>.Update(UserOrder);
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
                                OrderCode = UserOrder.Code,
                                OrderId = UserOrder.Id
                            };

                            db.UserWallets.Add(userWallet);

                            db.SaveChanges();
                            //
                        }
                    }
                    Transaction.Commit();
                    await Notifications.SendToAllSpecificAndroidUserDevices(UserOrder.UserId, "تم استقبال طلبكم", "تم ارسال طلبكم بنجاح الى التطبيق", NotificationType.CurrentOrders, UserOrder.Id, true);
                    await Notifications.SendToAllSpecificIOSUserDevices(UserOrder.UserId, "تم استقبال طلبكم", "تم ارسال طلبكم بنجاح الى التطبيق", NotificationType.CurrentOrders, UserOrder.Id, true);
                    return Ok(baseResponse);
                }
                catch (Exception ex)
                {
                    Transaction.Rollback();
                    baseResponse.ErrorCode = Errors.SomethingIsWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }

        }
        private async Task<string> GetTabbyCheckoutUrl(Order order)
        {
            try
            {
                using (var client = new HttpClient()) // Create an instance of HttpClient
                {
                    var domain = Request.RequestUri.GetLeftPart(UriPartial.Authority);

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
                            success = domain + "/Orders/TabbyconfrimPayment?id=" + order.Id + "&api=true",
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

        [HttpGet]
        [Route("currentorders")]
        public IHttpActionResult GetCurrentOrders(string lang = "en")
        {
            string CurrentUserId = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var Orders = db.Orders.Where(d => d.UserId == CurrentUserId && d.OrderStatus == OrderStatus.Placed && d.IsDeleted == false).OrderByDescending(s => s.CreatedOn).ToList();
            List<CurrentOrderDTO> orderDTOs = new List<CurrentOrderDTO>();
            foreach (var order in Orders)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                var dTO = new CurrentOrderDTO()
                {
                    OrderCode = order.Code,
                    OrderDate = CreatedOn.ToString("dd MMMM yyyy hh:mm tt"),
                    OrderId = order.Id,
                };
                var FirstItemInOrder = order.Items.FirstOrDefault(d => d.IsDeleted == false);
                if (FirstItemInOrder != null)
                {
                    dTO.ItemDescription = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? FirstItemInOrder.Service.DescriptionAr : FirstItemInOrder.Service.DescriptionEn;
                    dTO.ItemName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? FirstItemInOrder.Service.NameAr : FirstItemInOrder.Service.NameEn;
                    dTO.ItemImage = FirstItemInOrder.Service.Images != null && FirstItemInOrder.Service.Images.FirstOrDefault(d => d.IsDeleted == false) != null ? "/Content/Images/Services/" + FirstItemInOrder.Service.Images.FirstOrDefault(d => d.IsDeleted == false).ImageUrl : null;
                }
                switch (order.OrderStatus)
                {
                    case OrderStatus.Placed:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                        break;
                    default:
                        break;
                }
                orderDTOs.Add(dTO);
            }
            baseResponse.Data = orderDTOs;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("previousorders")]
        public IHttpActionResult GetPreviousOrders(string lang = "en")
        {
            string CurrentUserId = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var Orders = db.Orders.Where(d => d.UserId == CurrentUserId && d.OrderStatus == OrderStatus.Delivered && d.IsDeleted == false).OrderByDescending(s => s.CreatedOn).ToList();
            List<PreviousOrderDTO> orderDTOs = new List<PreviousOrderDTO>();
            foreach (var order in Orders)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));

                var dTO = new PreviousOrderDTO()
                {
                    OrderCode = order.Code,
                    OrderDate = CreatedOn.ToString("dd MMMM yyyy hh:mm tt"),
                    OrderId = order.Id,
                };
                var FirstItemInOrder = order.Items.FirstOrDefault(d => d.IsDeleted == false);
                if (FirstItemInOrder != null)
                {
                    dTO.ItemDescription = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? FirstItemInOrder.Service.DescriptionAr : FirstItemInOrder.Service.DescriptionEn;
                    dTO.ItemName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? FirstItemInOrder.Service.NameAr : FirstItemInOrder.Service.NameEn;
                    dTO.ItemImage = FirstItemInOrder.Service.Images != null && FirstItemInOrder.Service.Images.FirstOrDefault(d => d.IsDeleted == false) != null ? "/Content/Images/Services/" + FirstItemInOrder.Service.Images.FirstOrDefault(d => d.IsDeleted == false).ImageUrl : null;
                }
                switch (order.OrderStatus)
                {
                    case OrderStatus.Delivered:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Delivered";
                        break;
                    default:
                        break;
                }
                orderDTOs.Add(dTO);
            }
            baseResponse.Data = orderDTOs;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("details")]
        public IHttpActionResult GetOrderDetails(long OrderId, string lang = "en")
        {
            var Order = db.Orders.Find(OrderId);
            if (Order == null || Order.IsDeleted == true)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));

            OrderDetailsDTO detailsDTO = new OrderDetailsDTO()
            {
                PhoneNumber = Order.User.PhoneNumber,
                Code = Order.Code,
                Date = CreatedOn.ToString("dd MMMM yyyy"),
                PaymentMethod = Order.PaymentMethod,
                Subtotal = Order.SubTotal,
                Total = Order.Total,
                type = Order.UserType,
                DeliveryFees = Order.DeliveryFees,
                Discount = Order.PackageDiscount + Order.PromoDiscount,
            };

            if (Order.PackageId.HasValue == true)
            {
                detailsDTO.IsHavePackageDiscount = true;
                detailsDTO.PackageDiscount = Order.PackageDiscount;
            }

            if (Order.PromoId.HasValue == true)
            {
                detailsDTO.IsHavePromoDiscount = true;
                detailsDTO.PromoDiscount = Order.PromoDiscount;
                detailsDTO.PromoText = Order.Promo.Text;
            }

            if (Order.WalletDiscount > 0)
            {
                detailsDTO.IsHaveWalletDiscount = true;
                detailsDTO.WalletDiscount = Order.WalletDiscount;
            }

            switch (Order.OrderStatus)
            {
                case OrderStatus.Placed:
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                    break;
                case OrderStatus.Delivered:
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Deliverd";
                    break;
                default:
                    break;
            }

            if (Order.Items != null)
            {
                foreach (var item in Order.Items.Where(d => d.IsDeleted == false))
                {
                    detailsDTO.Items.Add(new OrderDetailsItemDTO()
                    {
                        Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Service.DescriptionAr : item.Service.DescriptionEn,
                        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Service.NameAr : item.Service.NameEn,
                        Image = item.Service.Images != null && item.Service.Images.FirstOrDefault(d => d.IsDeleted == false) != null ? "/Content/Images/Services/" + item.Service.Images.FirstOrDefault(d => d.IsDeleted == false).ImageUrl : null,
                        Price = item.Price,
                        Quantity = item.Quantity,
                    });
                }
            }
            baseResponse.Data = detailsDTO;
            return Ok(baseResponse);
        }

        [HttpPost]
        [Route("PaymentCallback")]
        public async Task<IHttpActionResult> PaymentCallback(PaymentDTO model, string lang = "en")
        {
            CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.Orders.FirstOrDefault(x => x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserBasketIsEmpty;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            //PaymentActions.SaveResponseInDatabase(model.PaymentId, model.TranId, null, model.Result, null, null, null, null, null, model.Amount, null, TransactionType.CheckingoutOrder, CurrentUserId, UserOrder.Id);
            bool IsPaymentSuccess = PaymentActions.VerifyResponse(model.TranId, model.Result, model.TrackId, model.ResponseCode, null, model.Amount);
            if (IsPaymentSuccess == true)
            {
                UserOrder.IsPaid = true;
                UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
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
                if (UserOrder.User.PhoneNumber.StartsWith("010") || UserOrder.User.PhoneNumber.StartsWith("011") || UserOrder.User.PhoneNumber.StartsWith("012") || UserOrder.User.PhoneNumber.StartsWith("015"))
                {
                    // إرسال الرسالة إذا كان الرقم مصرياً
                    await SMS.SendMessageAsync("20", UserOrder.User.PhoneNumber, $"تم إتمام عمليه تنفيذ الطلب رقم  [{UserOrder.Code}]");
                }
                else
                    await SMS.SendMessageAsync("966", UserOrder.User.PhoneNumber, $"تم إتمام عمليه تنفيذ الطلب رقم  [{UserOrder.Code}]");

                await Notifications.SendToAllSpecificAndroidUserDevices(UserOrder.UserId, "تم الدفع بنجاح", "تمت عمليه دفع الطلب بنجاح", NotificationType.CurrentOrders, UserOrder.Id, true);
                await Notifications.SendToAllSpecificIOSUserDevices(UserOrder.UserId, "تم الدفع بنجاح", "تمت عمليه دفع الطلب بنجاح", NotificationType.CurrentOrders, UserOrder.Id, true);
                await Notifications.SendToAllSpecificAndroidUserDevices(UserOrder.UserId, "تم استقبال طلبكم", "تم ارسال طلبكم بنجاح الى التطبيق", NotificationType.CurrentOrders, UserOrder.Id, true);
                await Notifications.SendToAllSpecificIOSUserDevices(UserOrder.UserId, "تم استقبال طلبكم", "تم ارسال طلبكم بنجاح الى التطبيق", NotificationType.CurrentOrders, UserOrder.Id, true);
                return Ok(baseResponse);
            }
            else
            {
                baseResponse.ErrorCode = Errors.SomethingIsWrong;
                baseResponse.Data = PaymentActions.HandleResponseStatusCode(lang, model.ResponseCode);
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ApplyPromo")]
        public IHttpActionResult ApplyPromo(string Text)
        {
            CurrentUserId = User.Identity.GetUserId();
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                if (Headers.AllKeys.Contains("AnonymousKey") && Headers.GetValues("AnonymousKey").Any())
                    AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            ApplicationUser user = null;
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                user = UserManager.FindById(CurrentUserId);
                if (user == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotAuthorized;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            var UserOrder = db.Orders.FirstOrDefault(x => ((x.UserId == CurrentUserId && x.UserId != null) /*|| x.UnknownUserKeyIdentifier == AnonymousKey*/) && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserBasketIsEmpty;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Promo = db.PromoCodes.FirstOrDefault(w => w.Text.ToLower() == Text.ToLower() && w.IsDeleted == false && w.IsFinished == false);
            var ValidatePromo = PromoCodeActions.CheckValidity(Promo, CurrentUserId, UserOrder);
            if (ValidatePromo != Errors.Success)
            {
                baseResponse.ErrorCode = ValidatePromo;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            PromoCodeActions.ApplyPromo(UserOrder, Promo);
            OrderActions.CalculateOrderPrice(UserOrder);
            db.SaveChanges();
            baseResponse.Data = new
            {
                UserOrder.SubTotal,
                UserOrder.PackageDiscount,
                UserOrder.PromoDiscount,
                UserOrder.DeliveryFees,
                UserOrder.Total,
                IsPackageDiscount = UserOrder.PackageId.HasValue,
            };
            return Ok(baseResponse);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("RemovePromo")]
        public IHttpActionResult RemovePromo()
        {
            CurrentUserId = User.Identity.GetUserId();
            var Headers = HttpContext.Current.Request.Headers;
            string AnonymousKey = string.Empty;
            try
            {
                if (Headers.AllKeys.Contains("AnonymousKey") && Headers.GetValues("AnonymousKey").Any())
                    AnonymousKey = Headers.GetValues("AnonymousKey").FirstOrDefault();
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            ApplicationUser user = null;
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                user = UserManager.FindById(CurrentUserId);
                if (user == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotAuthorized;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            var UserOrder = db.Orders.FirstOrDefault(x => ((x.UserId == CurrentUserId && x.UserId != null) /*|| x.UnknownUserKeyIdentifier == AnonymousKey*/) && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserBasketIsEmpty;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            UserOrder.PromoId = null;
            UserOrder.Promo = null;
            UserOrder.PromoDiscount = 0;
            OrderActions.CalculateOrderPrice(UserOrder);
            db.SaveChanges();
            baseResponse.Data = new
            {
                UserOrder.SubTotal,
                UserOrder.PackageDiscount,
                UserOrder.PromoDiscount,
                UserOrder.DeliveryFees,
                UserOrder.Total,
                IsPackageDiscount = UserOrder.PackageId.HasValue,
            };
            return Ok(baseResponse);
        }

        private Errors ValidateBasketItems(Order order)
        {
            foreach (var item in order.Items.Where(d => d.IsDeleted == false))
            {
                if (item.Service.IsDeleted == true || item.Service.IsHidden == true || item.Service.SubCategory.IsDeleted == true /*|| item.Service.SubCategory.Category.IsDeleted == true*/)
                    return Errors.ServiceNotFound;

                if (item.SizeId.HasValue == true)
                {
                    var Size = db.ServiceSizes.FirstOrDefault(s => s.Id == item.SizeId.Value);
                    if (Size == null)
                    {
                        return Errors.SizeNotFound;
                    }
                }

                if (item.ColorId.HasValue == true)
                {
                    var Color = db.ServiceTrainers.FirstOrDefault(s => s.Id == item.ColorId.Value);
                    if (Color == null)
                    {
                        return Errors.ColorNotFound;
                    }
                }

            }
            return Errors.Success;
        }

        private Errors ValidateAddItemToBasket(AddItemToBasketDTO model, string CurrentUserId)
        {
            if (!string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == true)
            {
                var user = UserManager.FindById(CurrentUserId);
                if (user == null)
                    return Errors.UserNotAuthorized;
            }

            var Service = db.Services.FirstOrDefault(d => d.Id == model.ServiceId && d.IsDeleted == false && (d.Inventory > 0 || d.IsTimeBoundService) && d.IsHidden == false && d.SubCategory.IsDeleted == false/* && d.SubCategory.Category.IsDeleted == false*/);
            if (Service == null)
                return Errors.ServiceNotFound;

            if (model.SizeId.HasValue == true && model.SizeId.Value > 0)
            {
                var Size = db.ServiceSizes.FirstOrDefault(s => s.Id == model.SizeId.Value);
                if (Size == null)
                {
                    return Errors.SizeNotFound;
                }
            }

            if (model.ColorId.HasValue == true && model.ColorId.Value > 0)
            {
                var Color = db.ServiceTrainers.FirstOrDefault(s => s.Id == model.ColorId.Value);
                if (Color == null)
                {
                    return Errors.ColorNotFound;
                }
            }

            return Errors.Success;
        }
        private Errors ValidateAddTimeBoundServiceItemToBasket(AddTimeBoundServiceItemToBasketDTO model, string CurrentUserId)
        {
            if (!string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == true)
            {
                var user = UserManager.FindById(CurrentUserId);
                if (user == null)
                    return Errors.UserNotAuthorized;
            }

            var Service = db.Services.FirstOrDefault(d => d.Id == model.ServiceId && d.IsDeleted == false && d.ServiceDays > 0 && d.IsHidden == false && d.SubCategory.IsDeleted == false /*&& d.SubCategory.Category.IsDeleted == false*/);
            if (Service == null)
                return Errors.ServiceNotFound;



            if (model.StartDate.Date < DateTime.Now.Date)
            {
                return Errors.InvalidStartDate;

            }

            return Errors.Success;
        }

        [HttpGet]
        [Route("GetPaymentGatewayUrl")]
        public IHttpActionResult GetPaymentGatewayUrl()
        {
            CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.Orders.FirstOrDefault(x => x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder != null)
            {
                string UserCountry = "Saudi Arabia";
                string UserCity = "Saudi Arabia";

                if (UserOrder.CityId.HasValue == true)
                {
                    UserCity = !string.IsNullOrEmpty(UserOrder.City.NameEn) ? UserOrder.City.NameEn : UserOrder.City.NameAr;
                    UserCountry = !string.IsNullOrEmpty(UserOrder.City.Country.NameEn) ? UserOrder.City.Country.NameEn : UserOrder.City.Country.NameAr;
                    if (string.IsNullOrEmpty(UserCity))
                        UserCity = "Saudi Arabia";

                    if (string.IsNullOrEmpty(UserCountry))
                        UserCountry = "Saudi Arabia";
                }

                string TerminalId = ConfigurationManager.AppSettings["TerminalId"];
                string TerminalPassword = ConfigurationManager.AppSettings["TerminalPassword"];
                string Secret = ConfigurationManager.AppSettings["Secret"];
                string Sequence = UserOrder.Code + "|" + TerminalId + "|" + TerminalPassword + "|" + Secret + "|" + (UserOrder.Total.ToString()) + "|" + "SAR";
                string Hash = PaymentActions.SHA256_HASH(Sequence);
                string ReturnUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + $"/Orders/ResultApi?Id={UserOrder.Id}";
                JObject Json = PaymentActions.GenerateJson(UserCountry, UserOrder.User.Name, "", "", UserCity, "", "", UserOrder.User.PhoneNumber, UserOrder.User.Email, UserOrder.Total.ToString(), "SAR", "1", Hash, UserOrder.Code, ReturnUrl);
                string FinalUrl = PaymentActions.GeneratePaymentUrl(Json);
                baseResponse.Data = FinalUrl;
                return Ok(baseResponse);
            }
            baseResponse.ErrorMessage = "No Order Find";
            return BadRequest(baseResponse.ErrorMessage);
        }

    }
}
