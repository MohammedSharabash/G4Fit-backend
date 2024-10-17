using G4Fit.Models.Domains;
using G4Fit.Models.Data;
using System.Linq;
using G4Fit.Models.Enums;

namespace G4Fit.Models.DTOs
{
    public class UserDTO
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string IDNumber { get; set; }
        public string Address { get; set; }
        public string Qr { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int NotificationsNumber { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Image { get; set; }
        public string ImageName { get; set; }
        public int BasketItemsCount { get; set; }
        public long? CountryId { get; set; }
        public LoginType LoginType { get; set; }
        public static UserDTO ToUserDTO(ApplicationUser requiredUser)
        {
            var userDTO = new UserDTO()
            {
                NotificationsNumber = requiredUser.Notifications != null ? requiredUser.Notifications.Count(s => s.IsDeleted == false && s.IsSeen == false) : 0,
                Name = requiredUser.Name,
                PhoneNumber = requiredUser.PhoneNumber,
                Image = string.IsNullOrEmpty(requiredUser.ImageUrl) ? null : "/Content/Images/Users/" + requiredUser.ImageUrl,
                BasketItemsCount = 0,
                LoginType = requiredUser.LoginType,
                ImageName = requiredUser.ImageUrl,
                Email = requiredUser.Email,
                Qr = requiredUser.QR,
                Address = requiredUser.Address,
                CountryId = requiredUser.CountryId,
                IDNumber = requiredUser.IDNumber,
                UserId = requiredUser.Id
            };

            ApplicationDbContext db = new ApplicationDbContext();
            var UserOrder = db.Orders.FirstOrDefault(x => x.UserId == requiredUser.Id && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder != null)
            {
                if (UserOrder.Items != null)
                {
                    userDTO.BasketItemsCount = UserOrder.Items.Count(d => d.IsDeleted == false);
                }
            }

            return userDTO;
        }
    }
}