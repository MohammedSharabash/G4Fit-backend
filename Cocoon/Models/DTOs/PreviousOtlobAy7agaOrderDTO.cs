using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.DTOs
{
    public class PreviousOtlobAy7agaOrderDTO
    {
        public long OrderId { get; set; }
        public string OrderCode { get; set; }
        public string OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string ItemImage { get; set; }
        public string ItemDescription { get; set; }
    }
}