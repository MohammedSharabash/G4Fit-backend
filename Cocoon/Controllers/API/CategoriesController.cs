using G4Fit.Helpers;
using G4Fit.Models.Data;
using G4Fit.Models.Enums;
using G4Fit.Models.DTOs;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using G4Fit.Models.Domains;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using Microsoft.Owin.Security;
using System.Configuration;
using Newtonsoft.Json;

namespace G4Fit.Controllers.API
{
    [RoutePrefix("api/categories")]
    public class CategoriesController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public CategoriesController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("main")]
        public IHttpActionResult GetMainCategories(string lang = "en")
        {
            //var Categories = db.Categories.Where(s => s.IsDeleted == false).OrderBy(s => s.SortingNumber).ToList();
            var Categories = db.SubCategories.Where(s => s.IsDeleted == false).OrderBy(s => s.SortingNumber).ToList();
            List<CategoryDTO> categoryDTOs = new List<CategoryDTO>();
            foreach (var category in Categories)
            {
                CategoryDTO categoryDTO = new CategoryDTO()
                {
                    Id = category.Id,
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? category.NameAr : category.NameEn,
                    ImageUrl = category.ImageUrl != null ? MediaControl.GetPath(FilePath.Category) + category.ImageUrl : null
                };
                categoryDTOs.Add(categoryDTO);
            }
            baseResponse.Data = categoryDTOs;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("mainwithslider")]
        public IHttpActionResult GetMainCategoriesWithSlider(string lang = "en")
        {
            //var Categories = db.Categories.Where(s => s.IsDeleted == false).OrderBy(s => s.SortingNumber).ToList();
            var Categories = db.SubCategories.Where(s => s.IsDeleted == false).OrderBy(s => s.SortingNumber).ToList();
            List<CategoryDTO> categoryDTOs = new List<CategoryDTO>();
            foreach (var category in Categories)
            {
                CategoryDTO categoryDTO = new CategoryDTO()
                {
                    Id = category.Id,
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? category.NameAr : category.NameEn,
                    ImageUrl = category.ImageUrl != null ? MediaControl.GetPath(FilePath.Category) + category.ImageUrl : null
                };
                categoryDTOs.Add(categoryDTO);
            }

            var Sliders = db.Sliders.Where(w => w.ImageUrl != null && w.IsDeleted == false).OrderByDescending(w => w.CreatedOn).ToList();
            List<string> dto = new List<string>();
            foreach (var slider in Sliders)
            {
                dto.Add(MediaControl.GetPath(FilePath.Slider) + slider.ImageUrl);
            }
            baseResponse.Data = new
            {
                Categories = categoryDTOs,
                Sliders = dto
            };

            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("mainPage")]
        public IHttpActionResult GetAllMainCategoriesPage(string lang = "en")
        {
            //var Categories = db.Categories.Where(x => !x.IsDeleted).ToList();
            var Categories = db.SubCategories.Where(x => !x.IsDeleted).ToList();
            List<CategoryPageDTO> categoryDTOs = new List<CategoryPageDTO>();

            foreach (var category in Categories)
            {
                var categoryDTO = new CategoryPageDTO()
                {
                    CategoryId = category.Id,
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? category.NameAr : category.NameEn,
                    Image = !string.IsNullOrEmpty(category.ImageUrl) ? "/Content/Images/Categories/" + category.ImageUrl : null
                };

                //if (category.SubCategories != null && category.SubCategories.Count(d => d.IsDeleted == false) > 0)
                //{
                //    foreach (var subcategory in category.SubCategories.Where(d => d.IsDeleted == false))
                //    {
                //        categoryDTO.SubCategories.Add(new SubCategoryDTO()
                //        {
                //            CategoryId = category.Id,
                //            Image = !string.IsNullOrEmpty(subcategory.ImageUrl) ? MediaControl.GetPath(FilePath.Category) + subcategory.ImageUrl : null,
                //            Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? subcategory.NameAr : subcategory.NameEn,
                //            SubCategoryId = subcategory.Id
                //        });
                //    }
                //}

                categoryDTOs.Add(categoryDTO);
            }

            baseResponse.Data = categoryDTOs;
            return Ok(baseResponse);
        }

        //[HttpGet]
        //[Route("subCats")]
        //public IHttpActionResult GetSubCategoriesByMainCategory(long CatId, string lang = "en")
        //{
        //    var MainCategory = db.Categories.FirstOrDefault(w => w.IsDeleted == false && w.Id == CatId);
        //    if (MainCategory == null)
        //    {
        //        baseResponse.ErrorCode = Errors.CategoryNotFound;
        //        return Content(HttpStatusCode.NotFound, baseResponse);
        //    }

        //    var SubCategories = db.SubCategories.Where(s => s.Category.IsDeleted == false && s.IsDeleted == false && s.CategoryId == CatId).OrderBy(s => s.SortingNumber).ToList();
        //    List<CategoryDTO> categoryDTOs = new List<CategoryDTO>();
        //    foreach (var category in SubCategories)
        //    {
        //        CategoryDTO categoryDTO = new CategoryDTO()
        //        {
        //            Id = category.Id,
        //            Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? category.NameAr : category.NameEn,
        //            ImageUrl = category.ImageUrl != null ? MediaControl.GetPath(FilePath.Category) + category.ImageUrl : null
        //        };
        //        categoryDTOs.Add(categoryDTO);
        //    }
        //    baseResponse.Data = new
        //    {
        //        MainCategory = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? MainCategory.NameAr : MainCategory.NameEn,
        //        SubCategories = categoryDTOs
        //    };
        //    return Ok(baseResponse);
        //}
    }
}