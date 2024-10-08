using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace G4Fit.Helpers
{
    public abstract class SMS
    {
        /// <summary>
        /// Documentation: https://msegat.docs.apiary.io/#/reference/operations
        /// </summary>

        private const string msgEncoding = "UTF8";
        private const string userSender = "G4Fit";
        private const string password = "Deef@12345";
        private const string baseUrl = "https://www.hisms.ws/api.php?send_sms&";
        private const string userName = "966567199991";
        private const string apiKey = "9d3bc03Da81b653e5f6d054e5a090365";

        public async static Task SendMessageAsync(string CountryCode, string PhoneNumber, string Message)
        {
            try
            {
                if (CountryCode != null && PhoneNumber != null && Message != null)
                {
                    string To = PhoneNumber;
                    if (PhoneNumber.StartsWith("+"))
                    {
                        To = PhoneNumber.Substring(1);
                    }
                    else if (PhoneNumber.StartsWith("00"))
                    {
                        To = PhoneNumber.Substring(2);
                    }
                    else
                    {
                        To = PhoneNumber;
                    }
                    var endpoint = baseUrl + "username=" + userName + "&password=" + password + "&numbers=" + To + "&sender=" + userSender + "&message=" + Message;

                   /* var RequestBody = new
                    {
                        apiKey,
                        userName,
                        userSender,
                        msgEncoding,
                        numbers = $"{CountryCode}{To}",
                        msg = Message
                    };
                    var json = JsonConvert.SerializeObject(RequestBody);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");*/
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var bodyjs = JsonConvert.SerializeObject(new { });
                        var body = new StringContent(bodyjs, Encoding.UTF8, "application/json");
                        var response =  client.PostAsync(endpoint, body).GetAwaiter().GetResult();
                  /*      var response = await client.PostAsync(baseUrl, data);*/
                        //var x = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}