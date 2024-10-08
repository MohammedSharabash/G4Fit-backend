using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class Supplier : BaseModel
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public SupplierType Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StoreName { get; set; }
        public bool? IsAccepted { get; set; }
        public string TaxNumber { get; set; }
        public string TaxNumberFileUrl { get; set; }
        public string CommercialRegister { get; set; }
        public string CommercialRegisterFileUrl { get; set; }
        public string BankAccount { get; set; }
        public string IBAN { get; set; }
        public string IdentityFileUrl { get; set; }
        public virtual ICollection<Service> Services { get; set; }
    }
}