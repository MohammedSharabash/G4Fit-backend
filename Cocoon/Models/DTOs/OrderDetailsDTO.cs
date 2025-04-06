using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class OrderDetailsDTO
    {
        public string Code { get; set; }
        public string Date { get; set; }
        public string PhoneNumber { get; set; }
        public string OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public OrderUserType type { get; set; }
        public decimal Discount { get; set; }
        public bool IsHaveWalletDiscount { get; set; }
        public decimal WalletDiscount { get; set; }
        public bool IsHavePackageDiscount { get; set; }
        public bool IsHavePromoDiscount { get; set; }
        public decimal PackageDiscount { get; set; }
        public decimal PromoDiscount { get; set; }
        public string PromoText { get; set; }
        public decimal DeliveryFees { get; set; }
        public bool Frezzed { get; set; } = false;

        public List<OrderDetailsItemDTO> Items { get; set; } = new List<OrderDetailsItemDTO>();
    }
}