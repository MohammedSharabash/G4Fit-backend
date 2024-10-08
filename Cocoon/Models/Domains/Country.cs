using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class Country : BaseModel
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string ImageUrl { get; set; }
        public string TimeZoneId { get; set; }
        public string PhoneCode { get; set; }
        public string CurrencyAr { get; set; }
        public string CurrencyEn { get; set; }
        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}