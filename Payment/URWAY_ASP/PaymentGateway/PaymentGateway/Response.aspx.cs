using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IdentityModel.Metadata;


public partial class Response : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        try
        {
            pnlpaymentsuccessinfo.Visible = false;
            pnlpaymentunsuccessinfo.Visible = false;


            string strpipeSeperatedString = null;

            string strPaymentId = string.Empty;
            string strTranId = string.Empty;
            string strECI = string.Empty;
            string strResult = string.Empty;
            string strTrackId = string.Empty;
            string strResponseCode = string.Empty;
            string strAuthCode = string.Empty;
            string strRRN = string.Empty;
            string stramount = string.Empty;
            string strresponseHash = string.Empty;

            if (Request.QueryString["PaymentId"] != null)
            {
                strPaymentId = Request.QueryString["PaymentId"];
            }


            if (Request.QueryString["TranId"] != null)
            {
                strTranId = Request.QueryString["TranId"];
            }

            if (Request.QueryString["ECI"] != null)
            {
                strECI = Request.QueryString["ECI"];
            }

            if (Request.QueryString["Result"] != null)
            {
                strResult = Request.QueryString["Result"];
            }

            if (Request.QueryString["TrackId"] != null)
            {
                strTrackId = Request.QueryString["TrackId"];
            }


            if (Request.QueryString["ResponseCode"] != null)
            {
                strResponseCode = Request.QueryString["ResponseCode"];
            }

            if (Request.QueryString["AuthCode"] != null)
            {
                strAuthCode = Request.QueryString["AuthCode"];
            }

            if (Request.QueryString["RRN"] != null)
            {
                strRRN = Request.QueryString["RRN"];
            }

            if (Request.QueryString["amount"] != null)
            {
                stramount = Request.QueryString["amount"];
            }

            if (Request.QueryString["responseHash"] != null)
            {
                strresponseHash = Request.QueryString["responseHash"];
            }

            String Terminal = System.Configuration.ConfigurationManager.AppSettings.Get("terminal").ToString();
            String merchant = System.Configuration.ConfigurationManager.AppSettings.Get("merchant").ToString();
            String password = System.Configuration.ConfigurationManager.AppSettings.Get("password").ToString();
            String secret = System.Configuration.ConfigurationManager.AppSettings.Get("secret").ToString();
            var baseAddress = System.Configuration.ConfigurationManager.AppSettings.Get("url").ToString();
            string merchantIp;
            string hostName = Dns.GetHostName();
            merchantIp = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            strpipeSeperatedString = strTranId + "|" + secret + "|" + strResponseCode + "|" + stramount; //+ "|" + secret + "|" + txtAmount.Text + "|" + txtCurrency.Text;
            //  strpipeSeperatedString = strTranId + "|" + strResponseCode + "|" + stramount; //+ "|" + secret + "|" + txtAmount.Text + "|" + txtCurrency.Text;
            string currency = "SAR";
            string strHash = sha256_hash(strpipeSeperatedString);

            //Secured API Start
            string secHash = strTrackId + "|" + Terminal + "|" + password + "|" + secret + "|" + stramount + "|" + currency;
            string strHashSec = sha256_hash(secHash);
            JObject secJson = generateSecJson(strTranId, Terminal, password, stramount, currency, "10", strHashSec, "10.10.10.10", strTrackId);

            var http = (HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
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
               // Label1.Text = "Wrong value entered";
            }
            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();

            //   var contentJson = await SendRequest(request);
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

            //Secured API Stop



            if (strresponseHash == strHash && (strResult == "Successful" || strResult == "SUCCESS" || strResult == "Success"))
            {
                if (inquirystatus == "Successful" || inquiryResponsecode == "000")

                {
                    //Sucess Transaction
                    txtPaymentId.Text = strPaymentId;
                    txtTrackId.Text = strTrackId;
                    txtResult.Text = strResult;
                    txtamount.Text = stramount;
                    //txtResponseHash.Text = strresponseHash;
                    //txtPGResponseHash.Text = strHash;
                    pnlpaymentsuccessinfo.Visible = true;
                    dvpayment.InnerText = "Payment" + " " + strResult;

                }
                else
                {
                    //Contact administrator Message

                }
            }
            else
            {

                //UnSucessfull Transaction
                pnlpaymentunsuccessinfo.Visible = true;

            }
        }
        catch (Exception Ex)
        {
            WriteErrorToFile("Page_Load: " + Ex.Message);
        }

        //Response.aspx?PaymentId=1916318545458150556&TranId=1916318545458150556&ECI=""&Result=UnSuccessful&TrackId=908188&ResponseCode=602&AuthCode=""&RRN=""&amount=1&responseHash=f7a98c32d13227749d75a2cd4f74869a1a0f3ccc33583bda79409d166fbb2dca

    }

    public static String sha256_hash(string value)
    {
        StringBuilder Sb = new StringBuilder();
        try
        {
            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
        }

        catch (Exception Ex)
        {

        }
        return Sb.ToString();
    }

    #region Write Error Log

    public bool WriteErrorToFile(string sText)
    {
        try
        {

            string sFileName = "Error_" + DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";
            string sMonth = "";
            string sFolder = "C:\\PG_Log\\";
            string sHeaderMessage = "PGLog " + DateTime.Now.ToString() + Environment.NewLine;

            if (System.IO.Directory.Exists(sFolder) == false)
            {
                System.IO.Directory.CreateDirectory(sFolder);
            }
            if (!System.IO.File.Exists(sFolder + sFileName))
            {
                sText = Environment.NewLine + sHeaderMessage + sText + Environment.NewLine;
            }
            else
            {
                sText = Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + sText + Environment.NewLine;
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
    #endregion

    #region generateJSON
    public JObject generateSecJson(String TranId, String Terminal, String password, String amount, String currency, String Action, String HashSec, String merchantIp, String TrackId)
    {
        JObject testJson = new JObject();

        try
        {

            testJson["transid"] = TranId;
            testJson["terminalId"] = Terminal;
            testJson["amount"] = amount;
            testJson["currency"] = currency;
            testJson["action"] = Action;
            testJson["password"] = password;
            testJson["requestHash"] = HashSec;
            testJson["merchantIp"] = merchantIp;
            testJson["trackid"] = TrackId;



        }

        catch (Exception ex)
        {
        }
        return testJson;
    }
    #endregion
}