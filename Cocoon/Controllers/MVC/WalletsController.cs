using G4Fit.Helpers;
using G4Fit.Models.Domains;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [Authorize]
    public class WalletsController : BaseController
    {
        public ActionResult Index()
        {
            var user = db.Users.Find(CurrentUserId);
            return View(user);
        }
    }
}