using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class City : BaseModel
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public decimal DeliveryFees { get; set; } = 0;
        public long CountryId { get; set; }
        public virtual Country Country { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}