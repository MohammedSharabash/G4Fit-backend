using G4Fit.Helpers;
using G4Fit.Models.Data;
using G4Fit.Models.Enums;
using G4Fit.Models.DTOs;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using G4Fit.Models.Domains;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using Microsoft.Owin.Security;
using System.Configuration;
using Newtonsoft.Json;

namespace G4Fit.Controllers.API
{
    [RoutePrefix("api/data/Services")]
    public class ServicesController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public ServicesController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("details")]
        public IHttpActionResult GetServiceDetails(long ServiceId, string lang = "en")
        {
            string CurrentUserId = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var Service = db.Services.Find(ServiceId);
            if (Service == null || Service.IsHidden == true || Service.IsDeleted == true)
            {
                baseResponse.ErrorCode = Errors.ServiceNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            ServiceDetailsDTO detailsDTO = new ServiceDetailsDTO()
            {
                Id = Service.Id,
                ServiceName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Service.NameAr : Service.NameEn,
                CategoryName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Service.SubCategory.NameAr : Service.SubCategory.NameEn,
                Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Service.DescriptionAr : Service.DescriptionEn,
                HasDiscount = Service.OfferPrice.HasValue == true ? true : false,
                IsTimeBoundService = Service.IsTimeBoundService,
                ServiceDays = Service.ServiceDays,
                Price = Service.OriginalPrice.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "ريال سعودي" : "SAR"),
                PriceAfter = Service.OfferPrice.HasValue == true ? Service.OfferPrice.Value.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "ريال سعودي" : "SAR") : null,
            };
            if (Service.Images != null)
            {
                foreach (var image in Service.Images.Where(s => s.IsDeleted == false))
                {
                    detailsDTO.Images.Add("/Content/Images/Services/" + image.ImageUrl);
                }
            }

            if (Service.Colors != null)
            {
                foreach (var color in Service.Colors.Where(s => s.IsDeleted == false))
                {
                    detailsDTO.Trainers.Add(new ServiceColorDTO()
                    {
                        Name = color.Color,
                        TrainerId = color.Id
                    });
                }
            }

            if (Service.Sizes != null)
            {
                foreach (var size in Service.Sizes.Where(s => s.IsDeleted == false))
                {
                    detailsDTO.Sizes.Add(new ServiceSizeDTO()
                    {
                        Size = size.Size,
                        SizeId = size.Id
                    });
                }
            }

            baseResponse.Data = detailsDTO;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("togglefavourite")]
        public IHttpActionResult ToggleUserFavourite(long ServiceId)
        {
            var Service = db.Services.Find(ServiceId);
            if (Service == null)
            {
                baseResponse.ErrorCode = Errors.ServiceNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            string CurrentUserId = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                var user = db.Users.Find(CurrentUserId);
                if (user == null)
                {
                    baseResponse.ErrorCode = Errors.UserNotAuthorized;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            if (string.IsNullOrEmpty(CurrentUserId) && User.Identity.IsAuthenticated == false /*&& string.IsNullOrEmpty(AnonymousKey)*/)
            {
                baseResponse.ErrorCode = Errors.UserIdentityIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            bool IsLiked = false;
            var UserFavourite = db.ServiceFavourites.FirstOrDefault(d => d.ServiceId == ServiceId && (d.UserId == CurrentUserId || d.UnknownUserKeyIdentifier == AnonymousKey));
            if (UserFavourite == null)
            {
                db.ServiceFavourites.Add(new ServiceFavourite()
                {
                    UserId = CurrentUserId,
                    ServiceId = ServiceId,
                    UnknownUserKeyIdentifier = AnonymousKey
                });
                IsLiked = true;
            }
            else
            {
                if (UserFavourite.IsDeleted == true)
                {
                    CRUD<ServiceFavourite>.Restore(UserFavourite);
                    IsLiked = true;
                }
                else
                {
                    CRUD<ServiceFavourite>.Delete(UserFavourite);
                    IsLiked = false;
                }
            }
            db.SaveChanges();
            baseResponse.Data = new { IsLiked };
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("myFavourite")]
        public IHttpActionResult GetMyFavouriteServices(string lang = "en")
        {
            string CurrentUserId = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            var Services = db.ServiceFavourites.Include("Service").Where(s => s.Service.IsDeleted == false && s.Service.IsHidden == false && s.Service.SubCategory.IsDeleted == false/* && s.Service.SubCategory.Category.IsDeleted == false*/ && s.IsDeleted == false && (s.UserId == CurrentUserId || s.UnknownUserKeyIdentifier == AnonymousKey)).ToList();
            List<ServiceDTO> ServiceDTOs = new List<ServiceDTO>();
            foreach (var Service in Services.OrderByDescending(s => s.CreatedOn))
            {
                ServiceDTO ServiceDTO = new ServiceDTO()
                {
                    Id = Service.ServiceId,
                    Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Service.Service.DescriptionAr : Service.Service.DescriptionEn,
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Service.Service.NameAr : Service.Service.NameEn,
                    HasDiscount = Service.Service.OfferPrice.HasValue == true ? true : false,
                    IsTimeBoundService = Service.Service.IsTimeBoundService,
                    ServiceDays = Service.Service.ServiceDays,
                    ServiceFreezingDays = Service.Service.ServiceFreezingDays,
                    ServiceFreezingTimes = Service.Service.ServiceFreezingTimes,
                    Price = Service.Service.OriginalPrice.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR"),
                    PriceAfter = Service.Service.OfferPrice.HasValue == true ? Service.Service.OfferPrice.Value.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR") : null,
                    Image = Service.Service.Images != null && Service.Service.Images.FirstOrDefault(s => s.IsDeleted == false) != null ? "/Content/Images/Services/" + Service.Service.Images.FirstOrDefault(s => s.IsDeleted == false).ImageUrl : null,
                    IsFavourite = true
                };
                var Offer = Service.Service.Offers.FirstOrDefault(s => s.IsDeleted == false && s.IsFinished == false);
                if (Offer != null)
                {
                    ServiceDTO.DiscountPercentage = Offer.Percentage;
                }
                ServiceDTOs.Add(ServiceDTO);
            }
            baseResponse.Data = ServiceDTOs;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAllServicesByCategoryId(long CatId, string lang = "en"/*, int Page = 1*/)
        {
            /*  int PageSize = 16;
              int Skip = (Page - 1) * PageSize;
              if (Page <= 0)
              {
                  baseResponse.ErrorCode = Errors.PageMustBeGreaterThanZero;
                  return Content(HttpStatusCode.BadRequest, baseResponse);
              }*/

            string CurrentUserId = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var category = db.SubCategories.FirstOrDefault(s => s.IsDeleted == false && s.Id == CatId /*&& s.Category.IsDeleted == false*/);
            if (category == null)
            {
                baseResponse.ErrorCode = Errors.CategoryNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            CategoryDTO categoryDTO = new CategoryDTO()
            {
                Id = category.Id,
                Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? category.NameAr : category.NameEn,
            };
            if (category.Services != null && category.Services.Count(s => s.IsDeleted == false) > 0)
            {
                foreach (var Service in category.Services.Where(s => s.IsDeleted == false && s.IsHidden == false).OrderByDescending(s => s.SellCounter)/*.Skip(Skip).Take(PageSize)*/.ToList())
                {
                    ServiceDTO ServiceDTO = new ServiceDTO()
                    {
                        Id = Service.Id,
                        Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Service.DescriptionAr : Service.DescriptionEn,
                        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Service.NameAr : Service.NameEn,
                        HasDiscount = Service.OfferPrice.HasValue == true ? true : false,
                        IsTimeBoundService = Service.IsTimeBoundService,
                        ServiceDays = Service.ServiceDays,
                        ServiceFreezingDays = Service.ServiceFreezingDays,
                        ServiceFreezingTimes = Service.ServiceFreezingTimes,
                        Price = Service.OriginalPrice.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR"),
                        PriceAfter = Service.OfferPrice.HasValue == true ? Service.OfferPrice.Value.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR") : null,
                        Image = Service.Images != null && Service.Images.FirstOrDefault(s => s.IsDeleted == false) != null ? "/Content/Images/Services/" + Service.Images.FirstOrDefault(s => s.IsDeleted == false).ImageUrl : null,
                        IsFavourite = Service.FavouriteUsers != null && Service.FavouriteUsers.Any(s => s.IsDeleted == false && s.UserId == CurrentUserId) == true ? true : false
                    };
                    var Offer = Service.Offers.FirstOrDefault(s => s.IsDeleted == false && s.IsFinished == false);
                    if (Offer != null)
                    {
                        ServiceDTO.DiscountPercentage = Offer.Percentage;
                    }
                    categoryDTO.Services.Add(ServiceDTO);
                }
            }
            baseResponse.Data = categoryDTO;
            return Ok(baseResponse);
        }
    }
}
