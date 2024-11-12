using G4Fit.Models.Data;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace G4Fit.Helpers
{
    public static class PaymentActions
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static string TerminalId = "g4fit";
        public static string TerminalPassword = "URWAY@123";
        public static string Secret = "eaffe62bff4872f25c228d19fd600f9401c1c0c949e33f72718760497a9794d5";
        public static string Url = "https://payments-dev.urway-tech.com/URWAYPGService/transaction/jsonProcess/JSONrequest";

        //public static string SHA256_HASH(string value)
        //{
        //    StringBuilder Sb = new StringBuilder();
        //    try
        //    {
        //        using (var hash = SHA256.Create())
        //        {
        //            Encoding enc = Encoding.UTF8;
        //            Byte[] result = hash.ComputeHash(enc.GetBytes(value));

        //            foreach (Byte b in result)
        //                Sb.Append(b.ToString("x2"));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteErrorToFile(ex.Message);
        //    }
        //    return Sb.ToString();
        //}
        public static string SHA256_HASH(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Build hex string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }

                // Convert to lowercase
                return builder.ToString().ToLower();
            }
        }
        //    public static void test(string text)
        //    {
        //        string filepath = HttpContext.Current.Server.MapPath("~/ExceptionDetailsFile/");  //Text File Path

        //        if (!Directory.Exists(filepath))
        //        {
        //            Directory.CreateDirectory(filepath);

        //        }
        //        filepath = filepath + DateTime.Today.ToString("dd-MM-yy") + ".txt";   //Text File Name
        //        if (!System.IO.File.Exists(filepath))
        //        {


        //            System.IO.File.Create(filepath).Dispose();

        //        }
        //        using (StreamWriter sw = System.IO.File.AppendText(filepath))
        //        {
        //            sw.WriteLine(text);
        //            sw.WriteLine("-------------------------------------------------------------------------------------");
        //            sw.Flush();
        //            sw.Close();
        //        }
        //}
        public static JObject GenerateJson(string Country, string FirstName, string LastName, string Address, string City, string State, string Zip, string PhoneNumber, string Email, string Amount, string Currency, string Action, string Hash, string TrackId, string ReturnUrl)
        {

            string MerchantIp = /*"2.90.128.128";*/Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString(); // server ipv6
            JObject Json = new JObject();
            try
            {
                Json["country"] = Country;
                Json["First_name"] = FirstName;
                Json["Last_name"] = LastName;
                Json["address"] = Address;
                Json["city"] = City;
                Json["State"] = State;
                Json["Zip"] = Zip;
                Json["Phoneno"] = PhoneNumber;
                Json["customerEmail"] = Email;
                Json["transid"] = "";
                Json["terminalId"] = TerminalId;
                Json["password"] = TerminalPassword;
                Json["Secret"] = Secret;
                Json["amount"] = Amount;
                Json["currency"] = Currency;
                Json["action"] = Action;
                Json["requestHash"] = Hash;
                Json["merchantIp"] = MerchantIp;
                Json["trackid"] = TrackId;
                Json["udf2"] = ReturnUrl;
            }
            catch (Exception ex)
            {
                WriteErrorToFile(ex.Message);
            }
            return Json;
        }

        public static string GeneratePaymentUrl(JObject Json)
        {
            //return  Startup.StaticConfig.GetSection("PaymentConfigration:TerminalId").Value ;
            try
            {
                var BaseAddress = Url;
                var http = (HttpWebRequest)WebRequest.Create(new Uri(BaseAddress));
                http.Accept = "application/json";
                http.ContentType = "application/json";
                http.Method = "POST";
                string ParsedContent = Json.ToString();
                ASCIIEncoding encoding = new ASCIIEncoding();
                Byte[] bytes = encoding.GetBytes(ParsedContent);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Stream newStream = http.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();
                WriteErrorToFile("Start sending GetResponse()" + Environment.NewLine);
                var response = http.GetResponse();
                WriteErrorToFile("finish sending GetResponse()" + Environment.NewLine);
                string strcontentlength = Convert.ToString(response);
                if (string.IsNullOrEmpty(strcontentlength))
                {
                    WriteErrorToFile("strcontentlength is null" + Environment.NewLine);
                    return null;
                }
                var stream = response.GetResponseStream();
                WriteErrorToFile("GetResponseStream is done" + Environment.NewLine);
                var sr = new StreamReader(stream);
                WriteErrorToFile("sr is done" + Environment.NewLine);
                var content = sr.ReadToEnd();
                WriteErrorToFile("content is done" + Environment.NewLine);
                dynamic dvresponse = JsonConvert.DeserializeObject(content);
                WriteErrorToFile($"dvresponse is done {dvresponse}" + Environment.NewLine);
                string strTargetUrl = string.Empty;
                string strpayid = string.Empty;

                if (dvresponse["targetUrl"].Value != null)
                {
                    WriteErrorToFile($"dvresponse[targetUrl].Value != null {dvresponse["targetUrl"].Value}" + Environment.NewLine);
                    strTargetUrl = dvresponse["targetUrl"].Value;
                }

                if (dvresponse["payid"].Value != null && dvresponse["payid"].Value != "")
                {
                    WriteErrorToFile($"dvresponse[payid].Value != null {dvresponse["payid"].Value}" + Environment.NewLine);
                    strpayid = dvresponse["payid"].Value;
                }

                string finalUrl = strTargetUrl + "?paymentid=" + strpayid;

                if (strTargetUrl != null && strTargetUrl != "")
                {
                    return finalUrl;
                }
            }
            catch (Exception ex)
            {
                WriteErrorToFile(ex.Message);
            }
            return null;
        }

        public static bool WriteErrorToFile(string sText)
        {
            try
            {
                var DateTimeNowSaudi = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                string sFileName = "Error_" + DateTimeNowSaudi.Date.ToString("yyyyMMdd") + ".txt";
                string sFolder = HttpContext.Current.Server.MapPath("~/Errors/");
                string sHeaderMessage = "PGLog " + DateTimeNowSaudi.ToString() + Environment.NewLine;

                if (Directory.Exists(sFolder) == false)
                {
                    Directory.CreateDirectory(sFolder);
                }
                if (!File.Exists(sFolder + sFileName))
                {
                    sText = Environment.NewLine + sHeaderMessage + sText + Environment.NewLine;
                }
                else
                {
                    sText = Environment.NewLine + DateTimeNowSaudi.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + sText + Environment.NewLine;
                }

                StreamWriter str = new StreamWriter(sFolder + sFileName, true);
                str.Write(sText);
                str.Flush();
                str.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool VerifyResponse(string TranId, string Result, string TrackId, string ResponseCode, string responseHash, string amount)
        {
            //string TerminalId = options.Value.TerminalId;
            //string TerminalPassword = options.Value.TerminalPassword;
            //string Secret = options.Value.Secret;
            var BaseAddress = Url;
            string MerchantIp = /*"2.90.128.128";*/Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
            string strpipeSeperatedString = TranId + "|" + Secret + "|" + ResponseCode + "|" + amount;
            string strHash = SHA256_HASH(strpipeSeperatedString);
            string secHash = TrackId + "|" + TerminalId + "|" + TerminalPassword + "|" + Secret + "|" + amount + "|" + "SAR";
            string strHashSec = SHA256_HASH(secHash);
            JObject secJson = GenerateSecJson(TranId, TerminalId, TerminalPassword, amount, "SAR", "10", strHashSec, MerchantIp, TrackId);
            var http = (HttpWebRequest)WebRequest.Create(new Uri(BaseAddress));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";
            string parsedContent = secJson.ToString();
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(parsedContent);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();
            string strcontentlength = Convert.ToString(response);
            if (strcontentlength == null)
            {
                return false;
            }

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();

            dynamic dvresponse = JsonConvert.DeserializeObject(content);

            string inquiryResponsecode = string.Empty;
            string inquirystatus = string.Empty;

            if (dvresponse["responseCode"].Value != null)
            {
                inquiryResponsecode = dvresponse["responseCode"];

            }

            if (dvresponse["result"].Value != null)
            {
                inquirystatus = dvresponse["result"];

            }

            if ((responseHash != null ? responseHash == strHash ? true : false : true) && (Result == "Successful" || Result == "SUCCESS" || Result == "Success"))
            {
                if (inquirystatus == "Successful" || inquiryResponsecode == "000")
                {
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static JObject GenerateSecJson(string TranId, string Terminal, string password, string amount, string currency, string Action, string HashSec, string merchantIp, string TrackId)
        {
            JObject Json = new JObject();
            try
            {
                Json["transid"] = TranId;
                Json["terminalId"] = Terminal;
                Json["amount"] = amount;
                Json["currency"] = currency;
                Json["action"] = Action;
                Json["password"] = password;
                Json["requestHash"] = HashSec;
                Json["merchantIp"] = merchantIp;
                Json["trackid"] = TrackId;
            }

            catch (Exception ex)
            {
                WriteErrorToFile(ex.Message);
            }
            return Json;
        }

        public static void SaveResponseInDatabase(string paymentId, string tranId, string eCI, string result, string trackId, string responseCode, string authCode, string rRN, string responseHash, string amount, string cardBrand, TransactionType transactionType, string UserId, long? OrderId = null, long? PackageId = null)
        {
            try
            {
                PaymentTransactionHistory history = new PaymentTransactionHistory()
                {
                    TransactionType = transactionType,
                    UserId = UserId,
                    OrderId = OrderId,
                    PackageId = PackageId,
                    amount = amount,
                    AuthCode = authCode,
                    cardBrand = cardBrand,
                    ECI = eCI,
                    PaymentId = paymentId,
                    ResponseCode = responseCode,
                    responseHash = responseHash,
                    Result = result,
                    RRN = rRN,
                    TrackId = trackId,
                    TranId = tranId,
                };
                db.PaymentTransactionHistories.Add(history);
                db.SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        public static string HandleResponseStatusCode(string lang, string StatusCode)
        {
            string Error = string.Empty;
            if (StatusCode == "625" || StatusCode == "604" || StatusCode == "204")
                Error = lang == "ar" ? "رقم البطاقة غير صحيح" : "Invalid card number";

            if (StatusCode == "605" || StatusCode == "626")
                Error = lang == "ar" ? "رقم الـ CVV  غير صحيح" : "Invalid CVV number";

            if (StatusCode == "617")
                Error = lang == "ar" ? "تاريخ الانتهاء غير صحيح" : "Invalid expiry date";

            if (StatusCode == "620")
                Error = lang == "ar" ? "اسم صاحب البطاقة غير صحيح" : "Invalid card holder name";

            if (StatusCode == "5T3")
                Error = lang == "ar" ? "هذه البطاقة غير مدعومة" : "This card is not supported";

            if (StatusCode == "603")
                Error = lang == "ar" ? "انتهت مهله العملية ، برجاء المحاولة مره اخرى" : "Transaction timeout, please try again";

            if (StatusCode == "614")
                Error = lang == "ar" ? "برجاء التأكد من صحة المدخلات" : "Invalid fields entered";

            if (StatusCode == "624")
                Error = lang == "ar" ? "تم الغاء العمليه من قبل المستخدم" : "Transaction has been cancelled by the user";

            if (string.IsNullOrEmpty(Error))
            {
                Error = lang == "ar" ? "لم يتم اتمام العملية ، برجاء مراجعة ادارة التطبيق" : "Your payment did not complete, please contact application management";
            }
            return Error;
        }

        public static string GetPackagePaymentGatewayUrl(decimal Price, long PackageId, PeriodType Type, string UserId, string ReturnUrl)
        {
            var user = db.Users.Find(UserId);
            if (user != null)
            {
                string UserCountry = "Saudi Arabia";
                string UserPhone = user.PhoneNumber;
                if (user.CountryId.HasValue == true)
                {
                    UserCountry = !string.IsNullOrEmpty(user.Country.NameEn) ? user.Country.NameEn : user.Country.NameAr;
                    if (string.IsNullOrEmpty(UserCountry))
                        UserCountry = "Saudi Arabia";

                    UserPhone = $"+{user.Country.PhoneCode}{user.PhoneNumber}";
                }
                string RandomCode = RandomGenerator.GenerateString(7) + "_" + PackageId + "_" + (int)Type;
                string TerminalId = ConfigurationManager.AppSettings["TerminalId"];
                string TerminalPassword = ConfigurationManager.AppSettings["TerminalPassword"];
                string Secret = ConfigurationManager.AppSettings["Secret"];
                string Sequence = RandomCode + "|" + TerminalId + "|" + TerminalPassword + "|" + Secret + "|" + Price + "|" + "SAR";
                string Hash = PaymentActions.SHA256_HASH(Sequence);
                JObject Json = PaymentActions.GenerateJson(UserCountry, user.Name, "", "", "", "", "", UserPhone, user.Email, Price.ToString(), "SAR", "1", Hash, RandomCode, ReturnUrl);
                string FinalUrl = PaymentActions.GeneratePaymentUrl(Json);
                return FinalUrl;
            }
            return null;
        }
    }
}