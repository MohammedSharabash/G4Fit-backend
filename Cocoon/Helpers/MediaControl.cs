using G4Fit.Models.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace G4Fit.Helpers
{
    public class MediaControl
    {
        public static string Upload(FilePath filePath, HttpPostedFileBase File)
        {
            string FolderPath = string.Empty;
            if (File != null)
            {
                string FileExtension = Path.GetExtension(File.FileName);
                string Name = Guid.NewGuid().ToString() + FileExtension;
                switch (filePath)
                {
                    case FilePath.Category:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Categories");
                        break;
                    case FilePath.Service:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Services");
                        break;
                    case FilePath.Users:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Users");
                        break;
                    case FilePath.Slider:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Sliders");
                        break;
                    case FilePath.Country:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Countries");
                        break;
                    case FilePath.Other:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Other");
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(FolderPath))
                {
                    if (!Directory.Exists(FolderPath))
                        Directory.CreateDirectory(FolderPath);

                    File.SaveAs(Path.Combine(FolderPath, Name));
                    return Name;
                }
                return null;
            }
            return null;
        }

        public static string Upload(FilePath filePath, byte[] FileBytes, MediaType mediaType)
        {
            string FolderPath = string.Empty;
            string FileName = string.Empty;
            if (FileBytes != null && FileBytes.Length > 0)
            {
                switch (mediaType)
                {
                    case MediaType.Image:
                        FileName = Guid.NewGuid().ToString() + ".jpg";
                        break;
                    case MediaType.Excel:
                        FileName = Guid.NewGuid().ToString() + ".xlsx";
                        break;
                    case MediaType.PDF:
                        FileName = Guid.NewGuid().ToString() + ".pdf";
                        break;
                    case MediaType.Video:
                        FileName = Guid.NewGuid().ToString() + ".mp4";
                        break;
                    case MediaType.Word:
                        FileName = Guid.NewGuid().ToString() + ".docx";
                        break;
                    default:
                        break;
                }
                switch (filePath)
                {
                    case FilePath.Users:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Users");
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(FolderPath))
                {
                    if (!Directory.Exists(FolderPath))
                        Directory.CreateDirectory(FolderPath);
                    File.WriteAllBytes(Path.Combine(FolderPath, FileName), FileBytes);
                    return FileName;
                }
                return null;
            }
            return null;
        }

        public static void Delete(FilePath filePath, string FileName)
        {
            string FolderPath = string.Empty;
            if (FileName != null)
            {
                switch (filePath)
                {
                    case FilePath.Category:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Categories");
                        break;
                    case FilePath.Service:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Services");
                        break;
                    case FilePath.Users:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Users");
                        break;
                    case FilePath.Slider:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Sliders");
                        break;
                    case FilePath.Country:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Countries");
                        break;
                    case FilePath.Other:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Other");
                        break;
                    default:
                        break;
                }
                string FullPath = Path.Combine(FolderPath, FileName);
                if (File.Exists(FullPath))
                    File.Delete(FullPath);
            }
        }

        public static string GetPath(FilePath filePath)
        {
            switch (filePath)
            {
                case FilePath.Category:
                    return "/Content/Images/Categories/";
                case FilePath.Service:
                    return "/Content/Images/Services/";
                case FilePath.Users:
                    return "/Content/Images/Users/";
                case FilePath.Slider:
                    return "/Content/Images/Sliders/";
                case FilePath.Country:
                    return "/Content/Images/Countries/";
                case FilePath.Other:
                    return "/Content/Images/Other/";
                default:
                    return null;
            }
        }

        public static string ConvertImageToBase64(HttpPostedFileBase image)
        {
            if (image != null)
            {
                Stream fs = image.InputStream;
                BinaryReader br = new BinaryReader(fs);
                Byte[] bytes = br.ReadBytes((int)fs.Length);
                return Convert.ToBase64String(bytes, 0, bytes.Length);
            }
            else
                return null;
        }
    }

    public enum FilePath
    {
        Category,
        Service,
        Users,
        Slider,
        Country,
        Other,
    }
}
