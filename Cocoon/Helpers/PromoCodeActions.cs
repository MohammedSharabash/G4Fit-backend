using G4Fit.Models.Data;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Helpers
{
    public class PromoCodeActions
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static Errors CheckValidity(PromoCode PromoCode, string UserId, Order order = null)
        {
            if (PromoCode == null)
                return Errors.PromoNotFound;

            if (PromoCode.IsDeleted == true)
                return Errors.PromoNotFound;

            if (PromoCode.IsFinished == true)
                return Errors.PromoExpired;

            if (PromoCode.UserId != null && PromoCode.UserId != UserId)
            {
                return Errors.PromoCodeCannotBeAppliedToCurrentUser;
            }

            if (PromoCode.Users != null)
            {
                if (PromoCode.CouponQuantity.HasValue == true && PromoCode.NumberOfUse >= PromoCode.CouponQuantity)
                    return Errors.PromoExpired;

                var UserPromoCodes = db.PromoCodeUsers.Count(w => w.IsDeleted == false && w.UserId == UserId && w.PromoId == PromoCode.Id);
                if (UserPromoCodes >= PromoCode.NumberOfAllowedUsingTimes)
                    return Errors.UserReachedItsLimitInUsingThePromoCode;
            }


            if (order != null)
            {
                if (PromoCode.MinimumOrderCost.HasValue == true)
                {
                    if (order.SubTotal + order.DeliveryFees - order.PackageDiscount < PromoCode.MinimumOrderCost.Value)
                        return Errors.OrderCostDidnotMeatMinimumPromoCodeRequiredCost;
                }
            }
            return Errors.Success;
        }

        public static void ApplyPromo(Order userOrder, PromoCode promo)
        {
            if (userOrder != null && promo != null)
            {
                decimal PackageDiscount = userOrder.PackageDiscount;
                decimal SubTotal = userOrder.SubTotal;
                decimal DeliveryFees = userOrder.DeliveryFees;
                decimal PromoDiscount = promo.DiscountMoney.HasValue == true ? promo.DiscountMoney.Value : ((SubTotal + DeliveryFees) * promo.DiscountPercentage.Value / 100);

                if (PromoDiscount > SubTotal + DeliveryFees - PackageDiscount)
                {
                    PromoDiscount = SubTotal + DeliveryFees - PackageDiscount;
                }

                if (promo.MaximumDiscountMoney.HasValue == true && PromoDiscount > promo.MaximumDiscountMoney.Value)
                {
                    PromoDiscount = promo.MaximumDiscountMoney.Value;
                }
                userOrder.PromoId = promo.Id;
                userOrder.PromoDiscount = PromoDiscount;
            }
        }

        public static void ExecutePromo(Order userOrder, PromoCode promo)
        {
            if (promo != null && userOrder != null)
            {
                promo.NumberOfUse += 1;
                if (userOrder.UserId != null)
                {
                    db.PromoCodeUsers.Add(new PromoCodeUser()
                    {
                        UserId = userOrder.UserId,
                        DiscountGot = userOrder.PromoDiscount,
                        OrderId = userOrder.Id,
                        PromoId = promo.Id,
                        UsedOn = DateTime.Now.ToUniversalTime(),
                    });
                }
                if (promo.NumberOfUse >= promo.NumberOfAllowedUsingTimes)
                {
                    promo.IsFinished = true;
                }
            }
        }
    }
}