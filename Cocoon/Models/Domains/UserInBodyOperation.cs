using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class UserInBodyOperation : BaseModel
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public long OrderId { get; set; }
        public virtual Order Order { get; set; }
        public string ImageUrl { get; set; }
        public string Note { get; set; }
        public bool Confirmed { get; set; } = false;
        public string ConfirmationCode { get; set; }

    }
}