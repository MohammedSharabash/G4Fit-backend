using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class Package : BaseModel
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public int DiscountPercentage { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public virtual ICollection<UserPackage> Users { get; set; }
        public virtual ICollection<PaymentTransactionHistory> TransactionHistories { get; set; }
    }
}