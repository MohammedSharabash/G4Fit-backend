using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Enums
{
    public enum TransactionType
    {
        CheckingoutOrder,
        PurchasingPackage,
        AddedByAdminManually,
        SubtractedByAdminManually,
        CheckingoutOrderRefund,
    }
}