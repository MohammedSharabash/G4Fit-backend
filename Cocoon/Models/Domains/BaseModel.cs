using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public abstract class BaseModel
    {
        public BaseModel()
        {
            IsDeleted = false;
            IsModified = false;
            CreatedOn = DateTime.Now.ToUniversalTime();
        }
        public long Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsModified { get; set; }
        public DateTime? RestoredOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}