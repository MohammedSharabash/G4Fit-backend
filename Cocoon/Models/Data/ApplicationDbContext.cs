using G4Fit.Models.Domains;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;

namespace G4Fit.Models.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("G4FitConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<UserPushToken> UserPushTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceColor> ServiceColors { get; set; }
        public DbSet<ServiceOffer> ServiceOffers { get; set; }
        public DbSet<ServiceSize> ServiceSizes { get; set; }
        public DbSet<ServiceImage> ServiceImages { get; set; }
        public DbSet<ServiceFavourite> ServiceFavourites { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<CompanyData> CompanyDatas { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<PaymentTransactionHistory> PaymentTransactionHistories { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<UserPackage> UserPackages { get; set; }
        public DbSet<PromotionalSection> PromotionalSections { get; set; }
        public DbSet<PromotionalSectionService> PromotionalSectionServices { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<PromoCodeUser> PromoCodeUsers { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
    }
}