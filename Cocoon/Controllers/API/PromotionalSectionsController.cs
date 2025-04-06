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
    [RoutePrefix("api/PromotionalSections")]
    public class PromotionalSectionsController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public PromotionalSectionsController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("Get")]
        public IHttpActionResult GetAllPromotionalSections(string lang = "ar")
        {
            string CurrentUserId = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var Sections = db.PromotionalSections.Where(s => s.IsDeleted == false).OrderBy(s => s.SortingNumber).ToList();
            List<CategoryDTO> sectionDtos = new List<CategoryDTO>();
            foreach (var category in Sections)
            {
                CategoryDTO categoryDTO = new CategoryDTO()
                {
                    Id = category.Id,
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? category.NameAr : category.NameEn,
                };
                if (category.Services != null && category.Services.Count(s => s.IsDeleted == false) > 0)
                {
                    foreach (var Service in category.Services.Where(s => s.IsDeleted == false && s.Service.IsHidden == false && s.Service.IsDeleted == false).OrderBy(s => s.Service.SortingNumber))
                    {
                        ServiceDTO ServiceDTO = new ServiceDTO()
                        {
                            Id = Service.ServiceId,
                            Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Service.Service.DescriptionAr : Service.Service.DescriptionEn,
                            Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Service.Service.NameAr : Service.Service.NameEn,
                            HasDiscount = Service.Service.OfferPrice.HasValue == true ? true : false,
                            ServiceDays = Service.Service.ServiceDays,
                            ServiceFreezingDays = Service.Service.ServiceFreezingDays,
                            ServiceFreezingTimes = Service.Service.ServiceFreezingTimes,
                            Price = Service.Service.OriginalPrice.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR"),
                            PriceAfter = Service.Service.OfferPrice.HasValue == true ? Service.Service.OfferPrice.Value.ToString() + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR") : null,
                            Image = Service.Service.Images != null && Service.Service.Images.FirstOrDefault(s => s.IsDeleted == false) != null ? "/Content/Images/Services/" + Service.Service.Images.FirstOrDefault(s => s.IsDeleted == false).ImageUrl : null,
                            IsFavourite = Service.Service.FavouriteUsers != null && Service.Service.FavouriteUsers.Any(s => s.IsDeleted == false && s.UserId == CurrentUserId) == true ? true : false
                        };
                        var Offer = Service.Service.Offers.FirstOrDefault(s => s.IsDeleted == false && s.IsFinished == false);
                        if (Offer != null)
                        {
                            ServiceDTO.DiscountPercentage = Offer.Percentage;
                        }
                        categoryDTO.Services.Add(ServiceDTO);
                    }
                    sectionDtos.Add(categoryDTO);
                }
            }
            baseResponse.Data = sectionDtos;
            return Ok(baseResponse);
        }
    }
}