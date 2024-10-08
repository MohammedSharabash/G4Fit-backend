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
        [Required(ErrorMessage = "نسبة الخصم مطلوبة")]
        [Range(1, 100, ErrorMessage = "نسبة الخصم غير صحيحة")]
        public int Percentage { get; set; }
        public long? ServiceId { get; set; }
        public long? OfferId { get; set; }
    }
}