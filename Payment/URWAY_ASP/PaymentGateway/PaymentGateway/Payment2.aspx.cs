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


public partial class Payment2 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {

            Random rand = new Random((int)DateTime.Now.Ticks);
            int RandomNumber;
            RandomNumber = rand.Next(100000, 999999);
            txtTrackid.Text = Convert.ToString(RandomNumber);
        }
        catch (Exception Ex)
        {
            WriteErrorToFile("Page_Load: " + Ex.Message);
        }

         
    }

    #region btnsubmit_Click
    protected void btnsubmit_Click(object sender, EventArgs e)
    {

   

        string strpipeSeperatedString = null;
      
        string merchantIp;
        string hostName = Dns.GetHostName();
        merchantIp = Dns.GetHostEntry(hostName).AddressList[1].ToString();

 

        try
        {

            String Terminal = System.Configuration.ConfigurationManager.AppSettings.Get("terminal").ToString();
            String password = System.Configuration.ConfigurationManager.AppSettings.Get("password").ToString();
            String secret = System.Configuration.ConfigurationManager.AppSettings.Get("secret").ToString();
            var baseAddress = System.Configuration.ConfigurationManager.AppSettings.Get("url").ToString();
            //  String trackid = System.Configuration.ConfigurationManager.AppSettings.Get("trackid").ToString();


            strpipeSeperatedString = txtTrackid.Text + "|" + Terminal + "|" + password + "|" + secret + "|" + txtAmount.Text + "|" + txtCurrency.Text;

    


            string strHash = sha256_hash(strpipeSeperatedString);
            JObject generatedJson = generateJson(txtCountry.Text, txtfirst_name.Text, txtlast_name.Text, txtaddress.Text, txtcity.Text, txtstate.Text, txtzip.Text, txtPhoneno.Text, txtcustomerEmail.Text, Terminal, password, secret, txtAmount.Text, txtCurrency.Text, "1", strHash, merchantIp, txtTrackid.Text);



            


            var http = (HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";
            string parsedContent = generatedJson.ToString();
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(parsedContent);

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();
            string strcontentlength = Convert.ToString(response);
            if (strcontentlength == null )
            {
                Label1.Text = "Wrong value entered";
            }
            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();
         
            //   var contentJson = await SendRequest(request);
            dynamic dvresponse = JsonConvert.DeserializeObject(content);
            string strTargetUrl = string.Empty;
            string strpayid = string.Empty;

            if (dvresponse["targetUrl"].Value != null)
            {
                strTargetUrl = dvresponse["targetUrl"].Value;
            }
          
            if (dvresponse["payid"].Value != null && dvresponse["payid"].Value != "")
            {
                strpayid = dvresponse["payid"].Value;
            }

            string finalUrl = strTargetUrl + "?paymentid=" + strpayid;

            if (strTargetUrl != null && strTargetUrl != "")
            {


                Response.Redirect(finalUrl);


            }    

        }
       
        catch (Exception Ex)
        {
            Label1.Text = "The operation has timed out";
            WriteErrorToFile("btnsubmit_Click: " + Ex.Message);

           
        }

    }
    #endregion
    #region generateJson
    public JObject generateJson(String txtCountry, String txtfirst_name, String txtlast_name, String txtaddress, String txtcity, String txtstate, String txtzip, String txtPhoneno, String txtcustomerEmail, String Terminal, String password, String secret, String amount, String Currency, String Action, String strHash, String merchantIp, String Trackid)
        {
            JObject testJson = new JObject();

            try
            {
                testJson["country"] = txtCountry;
                testJson["First_name"]= txtfirst_name;
                testJson["Last_name"]= txtlast_name;
                testJson["address"] = txtaddress;
                testJson["city"] = txtcity;
                testJson["State"]= txtstate;
                testJson["Zip"] = txtzip;
                testJson["Phoneno"] = txtPhoneno;
                testJson["customerEmail"] = txtcustomerEmail;
                testJson["transid"] = "";
                testJson["terminalId"] = Terminal;
                testJson["password"] = password;
                testJson["Secret"] = secret;
                testJson["amount"] = amount;
                testJson["currency"] = Currency;
                testJson["action"] = Action;
                testJson["requestHash"] = strHash;
                testJson["merchantIp"] = merchantIp;
                testJson["trackid"] = Trackid;

 

            }

         catch (Exception ex)
            {
         }
           return testJson;
     }
    #endregion

    #region Sha256hsh

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
        catch (Exception ex)
        {
        }
       return Sb.ToString();
    }
    #endregion
 
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
}

 