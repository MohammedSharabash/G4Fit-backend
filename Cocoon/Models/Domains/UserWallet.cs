using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class UserWallet : BaseModel
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public decimal TransactionAmount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string AttachmentUrl { get; set; }
        public string TransactionWay { get; set; } // how admin paid to user
        public string TransactionId { get; set; }
        public long? OrderId { get; set; }
        public string OrderCode { get; set; }
    }
}