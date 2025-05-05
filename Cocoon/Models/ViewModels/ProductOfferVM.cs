using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class ServiceOfferVM
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FinishOn { get; set; }
        [Required(ErrorMessage = "سعر العرض مطلوب")]
        [Range(1, double.MaxValue, ErrorMessage = "يجب أن يكون سعر العرض أكبر من الصفر")]
        public decimal OfferPrice  { get; set; }
        public long? ServiceId { get; set; }
        public long? OfferId { get; set; }
    }
}