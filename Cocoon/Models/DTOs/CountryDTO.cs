using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class CountryDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string PhoneCode { get; set; }
        public string CurrencyAr { get; set; }
        public string CurrencyEn { get; set; }
        public List<CityDTO> Cities { get; set; } = new List<CityDTO>();
    }
}