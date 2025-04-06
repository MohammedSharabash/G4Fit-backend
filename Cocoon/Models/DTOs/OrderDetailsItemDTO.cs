using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class OrderDetailsItemDTO
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int FreezingDays { get; set; } = 0;
        public int FreezingTimes { get; set; } = 0;
        public int RemainServiceDays { get; set; } = 0;
        public int RemainFreezingDays { get; set; } = 0;
        public int RemainFreezingTimes { get; set; } = 0;
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}