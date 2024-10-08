using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using G4Fit.Models.Enums;

namespace G4Fit.Models.Domains
{
    public class Notification : BaseModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsSeen { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public NotificationType NotificationType { get; set; }
        public long? RequestId { get; set; }
        public string NotificationLink { get; set; }
    }
}