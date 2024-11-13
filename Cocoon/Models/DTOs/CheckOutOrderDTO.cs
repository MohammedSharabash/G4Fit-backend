using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class CheckOutOrderDTO
    {
        public PaymentMethod PaymentMethod { get; set; }
        //public long CityId { get; set; }
        public string Address { get; set; }
    }
}