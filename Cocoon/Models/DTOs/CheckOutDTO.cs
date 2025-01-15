using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class CheckOutDTO
    {
        public string ImageBase64 { get; set; }
        public string lang { get; set; } = "en";
        public OrderUserType type { get; set; } = OrderUserType.Normal;

    }
}