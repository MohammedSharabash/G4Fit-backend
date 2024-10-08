using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class OrderItem : BaseModel
    {
        public long OrderId { get; set; }
        public virtual Order Order { get; set; }
        public decimal Price { get; set; }
        public long ServiceId { get; set; }
        public virtual Service Service { get; set; }
        public long? ColorId { get; set; }
        public virtual ServiceColor Color { get; set; }
        public long? SizeId { get; set; }
        public virtual ServiceSize Size { get; set; }
        public int Quantity { get; set; }
        public DateTime? StartDate { get; set; }
        public decimal SubTotal { get; set; }
    }
}