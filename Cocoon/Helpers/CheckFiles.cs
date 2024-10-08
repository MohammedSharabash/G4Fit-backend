using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace G4Fit.Helpers
{
    public static class CheckFiles
    {
        private static readonly List<string> ValidImage = new List<string>
        {
            "tiff", "pjp", "pjpeg", "jfif", "tif", "svg", "bmp", "jpeg", "jpg", "png", "gif", "svgz", "webp", "ico", "xbm", "dib"
        };

        private static readonly List<string> ValidVideo = new List<string>
        {
            "ogm", "wmv", "mpg", "webm", "ogv", "asx", "mov", "mpeg", "mp4", "m4v", "avi", "flv"
        };

        private static readonly List<string> ValidExcel = new List<string>
        {
            "xlsx", "xls", "csv"
        };

        private static readonly List<string> ValidPdf = new List<string>
        {
            "pdf"
        };

        private static readonly List<string> ValidTxt = new List<string>
        {
            "text","txt"
        };

        private static readonly List<string> ValidWord = new List<string>
        {
            "doc","docx","rtf"
        };
        public static bool IsValidFile(HttpPostedFileBase File)
        {
            var Extension = Path.GetExtension(File.FileName).Substring(1).ToLower();
            if (ValidImage.Contains(Extension))
                return true;
            else if (ValidVideo.Contains(Extension))
                return true;
            else if (ValidExcel.Contains(Extension))
                return true;
            else if (ValidPdf.Contains(Extension))
                return true;
            else if (ValidTxt.Contains(Extension))
                return true;
            else if (ValidWord.Contains(Extension))
                return true;
            return false;
        }

        public static bool IsTxt(HttpPostedFile TextFile)
        {
            var Extension = Path.GetExtension(TextFile.FileName).Substring(1).ToLower();

            if (ValidTxt.Contains(Extension))
                return true;

            return false;
        }
        public static bool IsImage(HttpPostedFileBase Image)
        {
            var Extension = Path.GetExtension(Image.FileName).Substring(1).ToLower();

            if (ValidImage.Contains(Extension))
                return true;

            return false;
        }

        public static bool IsVideo(HttpPostedFileBase Video)
        {
            var Extension = Path.GetExtension(Video.FileName).Substring(1).ToLower();

            if (ValidVideo.Contains(Extension))
                return true;

            return false;
        }

        public static bool IsPDF(HttpPostedFileBase PDF)
        {
            var Extension = Path.GetExtension(PDF.FileName).Substring(1).ToLower();

            if (ValidPdf.Contains(Extension))
                return true;

            return false;
        }

        public static bool IsExcel(HttpPostedFileBase Excel)
        {
            var Extension = Path.GetExtension(Excel.FileName).Substring(1).ToLower();

            if (ValidExcel.Contains(Extension))
                return true;

            return false;
        }

        public static bool IsWord(HttpPostedFileBase Word)
        {
            var Extension = Path.GetExtension(Word.FileName).Substring(1).ToLower();

            if (ValidWord.Contains(Extension))
                return true;

            return false;
        }

    }
}
