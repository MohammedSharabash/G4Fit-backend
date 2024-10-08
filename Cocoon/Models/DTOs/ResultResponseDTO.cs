using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class ResultResponseDTO
    {
        public Errors ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public object Data { get; set; }
    }
}