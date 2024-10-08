using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class ServiceDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasDiscount { get; set; }
        public bool IsTimeBoundService { get; set; }
        public int ServiceDays { get; set; }
        public string Price { get; set; }
        public string PriceAfter { get; set; }
        public double? DiscountPercentage { get; set; }
        public bool IsFavourite { get; set; }
        public string Image { get; set; }
    }
}