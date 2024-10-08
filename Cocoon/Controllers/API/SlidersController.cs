using G4Fit.Helpers;
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
    [RoutePrefix("api/sliders")]
    public class SlidersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private BaseResponseDTO baseResponse;
        public SlidersController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("get")]
        public IHttpActionResult GetSliders()
        {
            var Sliders = db.Sliders.Where(w => w.ImageUrl != null && w.IsDeleted == false).OrderByDescending(w => w.CreatedOn).ToList();
            List<string> dto = new List<string>();
            foreach (var slider in Sliders)
            {
                dto.Add(MediaControl.GetPath(FilePath.Slider) + slider.ImageUrl);
            }
            baseResponse.Data = new
            {
                dto
            };
            return Ok(baseResponse);
        }
    }
}
