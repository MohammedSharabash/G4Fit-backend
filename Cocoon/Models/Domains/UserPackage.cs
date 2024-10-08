using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class UserPackage : BaseModel
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public long PackageId { get; set; }
        public virtual Package Package { get; set; }
        public DateTime StartOn { get; set; }
        public DateTime FinishOn { get; set; }
        public bool IsPaid { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfTimesUsed { get; set; }
        public string TrackId { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}