using G4Fit.Helpers;
using G4Fit.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class SlidersController : BaseController
    {
        public ActionResult Index()
        {
            return View(db.Sliders.Where(s => s.IsDeleted == false).ToList());
        }

        [HttpPost]
        public ActionResult Index(List<HttpPostedFileBase> Image)
        {
            for (int i = 0; i < Image.Count(); i++)
            {

                if (Image != null && CheckFiles.IsImage(Image[i]) == true)
                {
                    Slider slider = new Slider()
                    {
                        ImageUrl = MediaControl.Upload(FilePath.Slider, Image[i]),
                    };
                    db.Sliders.Add(slider);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(long? Id)
        {
            if (Id.HasValue == true)
            {
                var Slider = db.Sliders.Find(Id.Value);
                if (Slider != null)
                {
                    MediaControl.Delete(FilePath.Slider, Slider.ImageUrl);
                    CRUD<Slider>.Delete(Slider);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }
    }
}