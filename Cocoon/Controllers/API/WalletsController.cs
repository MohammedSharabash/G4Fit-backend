using G4Fit.Helpers;
using G4Fit.Models.Data;
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
    [RoutePrefix("api/wallets")]
    public class WalletsController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public WalletsController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("WalletData")]
        public IHttpActionResult GetWalletData(string lang = "en")
        {
            string CurrentUserId = User.Identity.GetUserId();
            var user = db.Users.Find(CurrentUserId);
            if (user == null)
            {
                baseResponse.ErrorCode = Errors.UserNotAuthorized;
                return Content(HttpStatusCode.Unauthorized, baseResponse);
            }

            baseResponse.Data = new
            {
                UserWallet = user.Wallet,
                UserWalletText = user.Wallet + (string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال سعودي" : "SAR")
            };
            return Ok(baseResponse);
        }
    }
}