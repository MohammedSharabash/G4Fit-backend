using G4Fit.Helpers;
using G4Fit.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [AdminAuthorizeAttribute(Roles = "Admin,SubAdmin")]
    public class CompanyDatasController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View(db.CompanyDatas.FirstOrDefault(d => d.IsDeleted == false));
        }

        [HttpPost]
        public ActionResult Index(CompanyData model)
        {
            var Data = db.CompanyDatas.FirstOrDefault(d => d.IsDeleted == false);
            if (Data != null)
            {
                CRUD<CompanyData>.Update(model);
                //Data.DeliveryFees = model.DeliveryFees;
                Data.VideoUrl = model.VideoUrl;
                Data.DescriptionAr = model.DescriptionAr;
                Data.DescriptionEn = model.DescriptionEn;
                Data.MessageAr = model.MessageAr;
                Data.MessageEn = model.MessageEn;
                Data.VisionAr = model.VisionAr;
                Data.VisionEn = model.VisionEn;
                Data.ValuesAr = model.ValuesAr;
                Data.ValuesEn = model.ValuesEn;
                Data.OtherNotesAr = model.OtherNotesAr;
                Data.OtherNotesEn = model.OtherNotesEn;
                Data.FooterTextAr = model.FooterTextAr;
                Data.FooterTextEn = model.FooterTextEn;
                Data.Instagram = model.Instagram;
                Data.WhatsApp = model.WhatsApp;
                Data.Twitter = model.Twitter;
                Data.Facebook = model.Facebook;
                Data.SnapChat = model.SnapChat;
                Data.TikTok = model.TikTok;
                Data.YouTube = model.YouTube;
                Data.Website = model.Website;
                Data.Hotline = model.Hotline;
                Data.AddressAr = model.AddressAr;
                Data.AddressEn = model.AddressEn;
                Data.Email = model.Email;
                db.SaveChanges();
            }
            else
            {
                db.CompanyDatas.Add(model);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}