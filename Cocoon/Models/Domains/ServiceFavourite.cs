using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class ServiceFavourite : BaseModel
    {
        public string UnknownUserKeyIdentifier { get; set; }
        public long ServiceId { get; set; }
        public virtual Service Service { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}