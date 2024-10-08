using G4Fit.Helpers;
using G4Fit.Models.Data;
using G4Fit.Models.Domains;
using G4Fit.Models.DTOs;
using G4Fit.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace G4Fit.Controllers.API
{
    [Authorize]
    [RoutePrefix("api/data/notifications")]
    public class NotificationsController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public NotificationsController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [Route("post/pushtoken")]
        [HttpPost]
        public IHttpActionResult PostPushToken(PushTokenDTO pushTokenDTO)
        {
            if (pushTokenDTO.OS != OS.Android && pushTokenDTO.OS != OS.IOS)
            {
                baseResponse.ErrorCode = Errors.InvalidOperatingSystem;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (string.IsNullOrEmpty(pushTokenDTO.PushToken))
            {
                baseResponse.ErrorCode = Errors.InvalidToken;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            string CurrentUserId = User.Identity.GetUserId();
            var PushTokenInDB = db.UserPushTokens.FirstOrDefault(x => x.PushToken == pushTokenDTO.PushToken && x.OS == pushTokenDTO.OS && x.UserId == CurrentUserId);
            if (PushTokenInDB != null)
            {
                if (PushTokenInDB.IsDeleted == true)
                    CRUD<UserPushToken>.Restore(PushTokenInDB);
            }
            else
            {
                UserPushToken pushToken = new UserPushToken()
                {
                    PushToken = pushTokenDTO.PushToken,
                    OS = pushTokenDTO.OS,
                    UserId = CurrentUserId,
                };
                db.UserPushTokens.Add(pushToken);
            }
            db.SaveChanges();
            int BasketItemsCount = 0;
            var UserOrder = db.Orders.FirstOrDefault(x => x.UserId == CurrentUserId && x.OrderStatus == OrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder != null)
            {
                if (UserOrder.Items != null)
                {
                    BasketItemsCount = UserOrder.Items.Count(d => d.IsDeleted == false);
                }
            }
            baseResponse.Data = new { BasketItemsCount = BasketItemsCount, NotificationsCount = db.Notifications.Count(s => s.IsDeleted == false && s.IsSeen == false && s.UserId == CurrentUserId).ToString() };
            return Ok(baseResponse);
        }

        [Route("get")]
        [HttpGet]
        public IHttpActionResult GetUserNotifications()
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Notifications = db.Notifications.Where(d => d.IsDeleted == false && d.UserId == CurrentUserId).OrderByDescending(d => d.CreatedOn).ThenByDescending(d => d.IsSeen).ToList();
            if (Notifications == null || Notifications.Count() <= 0)
            {
                return Ok(baseResponse);
            }
            List<NotificationDTO> notificationDTOs = new List<NotificationDTO>();
            foreach (var item in Notifications)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(item.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                item.IsSeen = true;
                notificationDTOs.Add(new NotificationDTO()
                {
                    Description = item.Body,
                    IsSeen = item.IsSeen,
                    NotificationId = item.Id,
                    Time = CreatedOn.ToString("hh:mm tt"),
                    Title = item.Title,
                });
            }
            db.SaveChanges();
            baseResponse.Data = notificationDTOs;
            return Ok(baseResponse);
        }
    }
}
