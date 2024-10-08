using Antlr.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class Service : BaseModel
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal? OfferPrice { get; set; } // if there is offer
        public bool IsHidden { get; set; }
        public bool IsTimeBoundService { get; set; } = false;
        public int ServiceDays { get; set; } = 0;
        public int SellCounter { get; set; }
        public long Inventory { get; set; } = 0;
        public int SortingNumber { get; set; }
        public long SubCategoryId { get; set; }
        public virtual SubCategory SubCategory { get; set; }
        public long? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<ServiceColor> Colors { get; set; }
        public virtual ICollection<ServiceSize> Sizes { get; set; }
        public virtual ICollection<ServiceImage> Images { get; set; }
        public virtual ICollection<ServiceOffer> Offers { get; set; }
        public virtual ICollection<ServiceFavourite> FavouriteUsers { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<PromotionalSectionService> PromotionalSections { get; set; }
    }
}