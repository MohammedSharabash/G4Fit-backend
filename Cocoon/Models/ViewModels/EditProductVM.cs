using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Models.ViewModels
{
    public class EditServiceVM
    {
        public long ServiceId { get; set; }
        [Required(ErrorMessage = "الاسم باللغه العربيه مطلوب")]
        public string NameAr { get; set; }
        [Required(ErrorMessage = "الاسم باللغه الانجليزية مطلوب")]
        public string NameEn { get; set; }
        [AllowHtml]
        public string DescriptionAr { get; set; }
        [AllowHtml]
        public string DescriptionEn { get; set; }
        [Required(ErrorMessage = "الخدمه متاح؟")]
        public bool IsHidden { get; set; }
        //[Required(ErrorMessage = "هل الخدمة محددة بوقت؟")]
        public bool IsTimeBoundService { get; set; }
        //[Required(ErrorMessage = "عدد ايام الاشتراك بالخدمه مطلوب")]
        public int ServiceDays { get; set; }
        public int ServiceFreezingDays { get; set; }
        public int ServiceFreezingTimes { get; set; }
        public int InBodyCount { get; set; }
        //[Required(ErrorMessage = "الكميه مطلوبه")]
        public long Inventory { get; set; }
        [Required(ErrorMessage = "سعر الخدمه مطلوب")]
        [DataType(DataType.Currency, ErrorMessage = "سعر الخدمه غير صحيح")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "سعر الخدمه الخاص بعروض [حماة وطن - طالبات - جامعة الملك خالد ] مطلوب")]
        [DataType(DataType.Currency, ErrorMessage = "سعر الخدمه الخاص بعروض [حماة وطن - طالبات - جامعة الملك خالد ] غير صحيح")]
        public decimal SpecialPrice { get; set; }
        [Required(ErrorMessage = "القسم الرئيسى مطلوب")]
        public long CategoryId { get; set; }
        public List<HttpPostedFileBase> NewImages { get; set; }
        public List<ServiceImageVM> Images { get; set; } = new List<ServiceImageVM>();
        public List<string> Trainers { get; set; } = new List<string>();
        public List<string> Sizes { get; set; } = new List<string>();

        public static EditServiceVM ToEditServiceVM(Service Service)
        {
            var EditServiceVM = new EditServiceVM()
            {
                ServiceId = Service.Id,
                CategoryId = Service.SubCategoryId,
                DescriptionAr = Service.DescriptionAr,
                DescriptionEn = Service.DescriptionEn,
                NameAr = Service.NameAr,
                NameEn = Service.NameEn,
                Inventory = Service.Inventory,
                IsHidden = !Service.IsHidden,
                IsTimeBoundService = Service.IsTimeBoundService,
                ServiceDays = Service.ServiceDays,
                ServiceFreezingTimes = Service.ServiceFreezingTimes,
                ServiceFreezingDays = Service.ServiceFreezingDays,
                InBodyCount = Service.InBodyCount,
                Price = Service.OriginalPrice,
                SpecialPrice = Service.SpecialPrice,
            };

            if (Service.Images != null && Service.Images.Count(s => s.IsDeleted == false) > 0)
            {
                foreach (var image in Service.Images.Where(s => s.IsDeleted == false))
                {
                    EditServiceVM.Images.Add(new ServiceImageVM()
                    {
                        ImageId = image.Id,
                        ImageUrl = image.ImageUrl
                    });
                }
            }
            return EditServiceVM;
        }
    }
}