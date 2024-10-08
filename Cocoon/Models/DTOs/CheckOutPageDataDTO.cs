using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class CheckOutPageDataDTO
    {
        public decimal DeliveryFees { get; set; }
        public decimal Discount { get; set; }
        public bool IsHaveWalletDiscount { get;  set; }
        public decimal WalletDiscount { get;  set; }
        public bool IsHavePackageDiscount { get; set; }
        public bool IsHavePromoDiscount { get; set; }
        public decimal PackageDiscount { get; set; }
        public decimal PromoDiscount { get; set; }
        public string PromoText { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public List<ServiceDTO> SimilarServices { get; set; } = new List<ServiceDTO>();
        
    }
}