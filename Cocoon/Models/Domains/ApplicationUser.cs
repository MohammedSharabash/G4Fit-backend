using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using G4Fit.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace G4Fit.Models.Domains
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            CreatedOn = DateTime.Now.ToUniversalTime();
        }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumberCountryCode { get; set; }
        public int VerificationCode { get; set; }
        public string ForgotPasswordGUID { get; set; }
        public string Address { get; set; }
        public string IDNumber { get; set; }
        public string QR { get; set; }
        public DateTime CreatedOn { get; set; }
        public LoginType LoginType { get; set; }
        public SubAdminRole Role { get; set; } = SubAdminRole.All;
        public long? CountryId { get; set; }
        public long? CityId { get; set; }
        public decimal Wallet { get; set; }
        public double? weight { get; set; }
        public double? length { get; set; }
        public bool IsDeleted { get; set; }
        public virtual Country Country { get; set; }
        public virtual City City { get; set; }
        public virtual ICollection<UserPushToken> PushTokens { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<ServiceFavourite> FavouriteServices { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<PaymentTransactionHistory> TransactionHistories { get; set; }
        public virtual ICollection<UserPackage> Packages { get; set; }
        public virtual ICollection<PromoCode> PromoCodes { get; set; }
        public virtual ICollection<PromoCodeUser> PromoCodeUsers { get; set; }
        public virtual ICollection<UserWallet> UserWallets { get; set; }
        public virtual ICollection<Supplier> Suppliers { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }
}