using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class ServiceImage : BaseModel
    {
        public string ImageUrl { get; set; }
        public long ServiceId { get; set; }
        public virtual Service Service { get; set; }
    }
}