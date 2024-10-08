using Matgarkom.Models.Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Matgarkom.Models.ViewModels
{
    public class UserVM
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "الاسم مطلوب")]
        public string Name { get; set; }
        [Required(ErrorMessage = "البريد الالكترونى مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الالكترونى غير صحيح")]
        public string Email { get; set; }
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }
        [Required(ErrorMessage = "الكود الدولى مطلوب")]
        public string PhoneNumberCountryCode { get; set; }
        public long? AreaId { get; set; }
        

        public static UserVM toUserVM(ApplicationUser user)
        {
            return new UserVM()
            {
                AreaId = user.AreaId,
                Id = user.Id,
                ImageUrl = user.ImageUrl,
                Name = user.Name,
                PhoneNumberCountryCode = user.PhoneNumberCountryCode,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }
    }
}