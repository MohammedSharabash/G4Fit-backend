using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.DTOs
{
    public class PaymentDTO
    {
        public string PaymentId { get; set; }
        public string TranId { get; set; }
        public string Result { get; set; }
        public string Amount { get; set; }
        public string TrackId { get;  set; }
        public string ResponseCode { get;  set; }
    }
}