using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class StoreDetailsDTO
    {
        public string Name { get; set; }
        public double? StoreLatitude { get; set; }
        public double? StoreLongitude { get; set; }
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }
        public string Rate { get; set; }
        public string Description { get; set; }
        public string Distance { get; set; }
        public int NumberOfDrivers { get; set; }
        public int NumberOfOrders { get; set; }
        public bool CanApply { get; set; }
    }
}