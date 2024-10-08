using G4Fit.Models.Data;
using G4Fit.Models.DTOs;
using G4Fit.Models.Enums;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace G4Fit.Controllers.API
{
    [AllowAnonymous]
    [RoutePrefix("api/country")]
    public class CountriesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private BaseResponseDTO baseResponse;

        public CountriesController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [Route("all")]
        [HttpGet]
        public IHttpActionResult GetAllCountries(string lang = "en")
        {
            var Countries = db.Countries.Where(x => !x.IsDeleted).ToList();
            List<CountryDTO> countryDTOs = new List<CountryDTO>();
            foreach (var country in Countries)
            {
                var CountryDTO = new CountryDTO()
                {
                    Image = !string.IsNullOrEmpty(country.ImageUrl) ? "/Content/Images/Countries/" + country.ImageUrl : null,
                    Id = country.Id,
                    PhoneCode = country.PhoneCode,
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? country.NameAr : country.NameEn,
                    CurrencyAr = country.CurrencyAr,
                    CurrencyEn = country.CurrencyEn,
                };
                if (country.Cities != null && country.Cities.Any(s => s.IsDeleted == false))
                {
                    foreach (var city in country.Cities.Where(s => s.IsDeleted == false))
                    {
                        CountryDTO.Cities.Add(new CityDTO()
                        {
                            CityId = city.Id,
                            NameAr = city.NameAr,
                            NameEn = city.NameEn
                        });
                    }
                }
                countryDTOs.Add(CountryDTO);
            }
            baseResponse.Data = countryDTOs;
            return Ok(baseResponse);
        }

        
    }
}
