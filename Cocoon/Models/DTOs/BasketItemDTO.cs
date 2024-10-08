using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class BasketItemDTO
    {
        public long BasketItemId { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public long ServiceId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
    }
}