using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class PromotionalSectionService : BaseModel
    {
        public long PromotionalSectionId { get; set; }
        public virtual PromotionalSection PromotionalSection { get; set; }
        public long ServiceId { get; set; }
        public virtual Service Service { get; set; }
    }
}