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
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}