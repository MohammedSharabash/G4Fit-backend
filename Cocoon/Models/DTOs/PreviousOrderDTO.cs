using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class PreviousOrderDTO
    {
        public long OrderId { get; set; }
        public string OrderCode { get; set; }
        public string OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string ItemImage { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
    }
}