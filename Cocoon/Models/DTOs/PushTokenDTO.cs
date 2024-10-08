using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class PushTokenDTO
    {
        public string PushToken { get; set; }
        public OS OS { get; set; }
    }
}