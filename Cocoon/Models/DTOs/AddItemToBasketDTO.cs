using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class AddItemToBasketDTO
    {
        public int Quantity { get; set; }
        public long ServiceId { get; set; }
        public long? SizeId { get; set; }
        public long? ColorId { get; set; }
    }
    public class AddTimeBoundServiceItemToBasketDTO
    {
        public DateTime StartDate { get; set; }
        public long ServiceId { get; set; }

    }
}