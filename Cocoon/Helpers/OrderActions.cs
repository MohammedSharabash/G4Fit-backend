using G4Fit.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Helpers
{
    public static class OrderActions
    {
        public static void CalculateOrderPrice(Order order)
        {
            order.SubTotal = order.Items.Where(x => x.IsDeleted == false).Sum(d => d.SubTotal);

            decimal Discount = 0;
            if (order.PackageId.HasValue == true)
            {
                Discount = (order.SubTotal + order.DeliveryFees) * order.Package.Package.DiscountPercentage / 100;
            }

            if (Discount > order.SubTotal + order.DeliveryFees - order.PromoDiscount)
            {
                Discount = order.SubTotal + order.DeliveryFees - order.PromoDiscount;
            }

            order.PackageDiscount = Discount;

            if (order.PromoId.HasValue == true && order.Promo != null)
            {
                PromoCodeActions.ApplyPromo(order, order.Promo);
            }

            order.Total = order.SubTotal + order.DeliveryFees - order.PromoDiscount - order.PackageDiscount;
            if (order.Total <= 0)
            {
                order.Total = 0;
            }

            if (order.Total > 0 && order.User != null && order.User.Wallet > 0)
            {
                if (order.User.Wallet >= order.Total)
                {
                    order.WalletDiscount = order.Total;
                }
                else
                {
                    order.WalletDiscount = order.User.Wallet;
                }
            }

            order.Total = order.SubTotal + order.DeliveryFees - order.PromoDiscount - order.PackageDiscount - order.WalletDiscount;
        }
    }
}