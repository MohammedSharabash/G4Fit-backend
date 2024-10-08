using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class ServiceColor : BaseModel
    {
        public string Color { get; set; }
        public long ServiceId { get; set; }
        public virtual Service Service { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}