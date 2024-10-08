using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Models.Domains
{
    public class FAQ : BaseModel
    {
        public string QuestionAr { get; set; }
        public string QuestionEn { get; set; }
        public string AnswerAr { get; set; }
        public string AnswerEn { get; set; }
    }
}