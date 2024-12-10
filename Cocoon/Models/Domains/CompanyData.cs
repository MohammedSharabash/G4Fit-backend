using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Models.Domains
{
    public class CompanyData : BaseModel
    {
        public string VideoUrl { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string VisionAr { get; set; }
        public string VisionEn { get; set; }
        public string MessageAr { get; set; }
        public string MessageEn { get; set; }
        public string ValuesAr { get; set; }
        public string ValuesEn { get; set; }
        public string OtherNotesAr { get; set; }
        public string OtherNotesEn { get; set; }
        public string FooterTextAr { get; set; }
        public string FooterTextEn { get; set; }
        public string WhatsApp { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string Facebook { get; set; }
        public string SnapChat { get; set; }
        public string TikTok { get; set; }
        public string YouTube { get; set; }
        public string Website { get; set; }
        public string Hotline { get; set; }
        public string AddressAr { get; set; }
        public string AddressEn { get; set; }
        public string Email { get; set; }
        [AllowHtml]
        public string PrivacyPolicyAr { get; set; }
        [AllowHtml]
        public string PrivacyPolicyEn { get; set; }
        [AllowHtml]
        public string TermsConditionsAr { get; set; }
        [AllowHtml]
        public string TermsConditionsEn { get; set; }
        [AllowHtml]
        public string aboutConditionsAr { get; set; }
        [AllowHtml]
        public string aboutConditionsEn { get; set; }
        [AllowHtml]
        public string DeliveringConditionsAr { get; set; }
        [AllowHtml]
        public string DeliveringConditionsEn { get; set; }
        [AllowHtml]
        public string ReturnAndExchangePolicyAr { get; set; }
        [AllowHtml]
        public string ReturnAndExchangePolicyEn { get; set; }
        [AllowHtml]
        public string CustomerServiceAr { get; set; }
        [AllowHtml]
        public string CustomerServiceEn { get; set; }
        [AllowHtml]
        public string HowToOrderAr { get; set; }
        [AllowHtml]
        public string HowToOrderEn { get; set; }
        [AllowHtml]
        public string InformationAr { get; set; }
        [AllowHtml]
        public string InformationEn { get; set; }
        [AllowHtml]
        public string SellWithUsAr { get; set; }
        [AllowHtml]
        public string SellWithUsEn { get; set; }
        [AllowHtml]
        public string SupplierSellingPolicyAr { get; set; }
        [AllowHtml]
        public string SupplierSellingPolicyEn { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}