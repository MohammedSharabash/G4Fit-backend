using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class PromoCodeUser : BaseModel
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public long PromoId { get; set; }
        public virtual PromoCode Promo { get; set; }
        public DateTime UsedOn { get; set; }
        public decimal DiscountGot { get; set; }
        public long? OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}