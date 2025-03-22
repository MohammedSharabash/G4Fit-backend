﻿using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class Order : BaseModel
    {
        public string UnknownUserKeyIdentifier { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PurposeOfSubscription? PurposeOfSubscription { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public OrderUserType UserType { get; set; } = OrderUserType.Normal;
        public string UserTypeImageUrl { get; set; }
        public long? CityId { get; set; }
        public virtual City City { get; set; }
        public decimal DeliveryFees { get; set; }
        public decimal PackageDiscount { get; set; }
        public decimal PromoDiscount { get; set; }
        public decimal WalletDiscount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string Tabby_PaymentId { get; set; } = "-1";
        public string Tabby_reference_id { get; set; } = "-1";
        public string TamaraPaymentId { get; set; } = "-1";
        public string Tamara_reference_id { get; set; } = "-1";
        public virtual ICollection<OrderItem> Items { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int InBodyCount { get; set; } = 0;
        public int InBodyUsedCount { get; set; } = 0;
        public bool IsPaid { get; set; }
        public long? PackageId { get; set; }
        public virtual UserPackage Package { get; set; }
        public int SMSNotificationsCount { get; set; }
        public DateTime? LastSMSNotificationDateSent { get; set; }
        public long? PromoId { get; set; }
        public virtual PromoCode Promo { get; set; }
        public virtual ICollection<PromoCodeUser> UsedPromos { get; set; }
        public virtual ICollection<PaymentTransactionHistory> PaymentTransactionHistories { get; set; }
    }
}