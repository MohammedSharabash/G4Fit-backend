using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace G4Fit.Models.ViewModels
{
    public class CreateServiceVM
    {
        [Required(ErrorMessage = "الاسم باللغه العربيه مطلوب")]
        public string NameAr { get; set; }
        [Required(ErrorMessage = "الاسم باللغه الانجليزية مطلوب")]
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        [Required(ErrorMessage = "سعر الخدمه مطلوب")]
        [DataType(DataType.Currency, ErrorMessage = "سعر الخدمه غير صحيح")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "الخدمه متاح؟")]
        public bool IsHidden { get; set; }
        //[Required(ErrorMessage = "هل الخدمة محددة بوقت؟")]
        public bool IsTimeBoundService { get; set; }
        //[Required(ErrorMessage = "عدد ايام الاشتراك بالخدمه مطلوب")]
        public int ServiceDays { get; set; }
        //[Required(ErrorMessage = "الكميه مطلوبه")]
        public long Inventory { get; set; }
        [Required(ErrorMessage = "القسم مطلوب")]
        public long CategoryId { get; set; }
        public HttpPostedFileBase[] Images { get; set; }
        public List<string> Colors { get; set; }
        public List<string> Sizes { get; set; }
    }
}