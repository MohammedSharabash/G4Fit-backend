using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class SubCategory : BaseModel
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string ImageUrl { get; set; }
        //public long CategoryId { get; set; }
        public int SortingNumber { get; set; }
        //public virtual Category Category { get; set; }
        public virtual ICollection<Service> Services { get; set; }
    }
}