using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class PromoCode : BaseModel
    {
        public string UserId { get; set; } // in this case, the coupon available for this user only
        public virtual ApplicationUser User { get; set; }
        public string Text { get; set; }
        public decimal? DiscountMoney { get; set; }
        public int? DiscountPercentage { get; set; }
        public int NumberOfUse { get; set; }
        public int? CouponQuantity { get; set; }
        public int NumberOfAllowedUsingTimes { get; set; }
        public decimal? MinimumOrderCost { get; set; }
        public decimal? MaximumDiscountMoney { get; set; }
        public bool IsFinished { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FinishOn { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<PromoCodeUser> Users { get; set; }
    }
}