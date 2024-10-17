using QRCoder;
using System.Drawing;
using System.IO;
using Imagekit;
using Imagekit.Sdk;
using System;

namespace G4Fit.Helper
{
    public class QRCodes
    {
        private static readonly string publicKey = "public_VEw8COpGC5k9R9rKf5vDtF83NCI=";
        private static readonly string privateKey = "private_6nC3KvTvYSrxDeyNB8qV7dkEYK4=";
        private static readonly string urlEndpoint = "https://ik.imagekit.io/G4fit/";

        public static string GenerateQR(string txtQRCode)
        {
            // Generate QR Code
            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            // Convert Bitmap to Byte Array
            byte[] qrCodeBytes = BitmapToBytesCode(qrCodeImage);

            // Upload to ImageKit
            var uploadUrl = UploadToImageKit(qrCodeBytes);
            return uploadUrl; // Return the URL of the uploaded image
        }

        private static byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private static string UploadToImageKit(byte[] qrCodeBytes)
        {
            // Initialize ImageKit Client
            ImagekitClient imagekit = new ImagekitClient(publicKey, privateKey, urlEndpoint);

            // Upload File
            var uploadFileRequest = new FileCreateRequest
            {
                file = Convert.ToBase64String(qrCodeBytes), // The file data as base64
                fileName = "QRCode.png",
                useUniqueFileName = true,
                folder = "/qrcodes/" // Optional: specify a folder in ImageKit
            };

            try
            {
                var response = imagekit.Upload(uploadFileRequest);

                // Check if upload is successful
                if (response != null && response.url != null)
                {
                    return response.url; // Return the URL of the uploaded file
                }
                else
                {
                    throw new Exception("Image upload failed: Unknown error.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Image upload failed: " + ex.Message);
            }
        }
    }
}
