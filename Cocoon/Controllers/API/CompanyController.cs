using G4Fit.Models.Data;
using G4Fit.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace G4Fit.Controllers.API
{
    [RoutePrefix("api/company")]
    public class CompanyController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private BaseResponseDTO baseResponse;
        public CompanyController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("about")]
        public IHttpActionResult GetAboutCompany()
        {
            var CompanyInfo = db.CompanyDatas.FirstOrDefault(d => !d.IsDeleted);
            if (CompanyInfo != null)
            {
                var Data = new
                {
                    CompanyInfo.AddressAr,
                    CompanyInfo.AddressEn,
                    CompanyInfo.Email,
                    CompanyInfo.WhatsApp,
                    CompanyInfo.Twitter,
                    CompanyInfo.Instagram,
                    CompanyInfo.Facebook,
                    CompanyInfo.Hotline,
                    CompanyInfo.SnapChat,
                    CompanyInfo.TikTok,
                    CompanyInfo.Website,
                    CompanyInfo.YouTube,
                };
                baseResponse.Data = Data;
                return Ok(baseResponse);
            }
            return Ok(baseResponse);
        }
    }
}
