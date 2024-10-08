using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using G4Fit.Models.Enums;

namespace G4Fit.Models.Domains
{
    public class UserPushToken : BaseModel
    {
        public string PushToken { get; set; }
        public OS OS { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}