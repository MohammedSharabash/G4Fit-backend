using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class PaymentTransactionHistory : BaseModel
    {
        public long? OrderId { get; set; }
        public virtual Order Order { get; set; }
        public long? PackageId { get; set; }
        public virtual Package Package { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public TransactionType TransactionType { get; set; }
        public string PaymentId { get; set; }
        public string TranId { get; set; }
        public string ECI { get; set; }
        public string Result { get; set; }
        public string TrackId { get; set; }
        public string ResponseCode { get; set; }
        public string AuthCode { get; set; }
        public string RRN { get; set; }
        public string responseHash { get; set; }
        public string amount { get; set; }
        public string cardBrand { get; set; }
    }
}