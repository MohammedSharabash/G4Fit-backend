using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace G4Fit.Helpers
{
    public abstract class SMS
    {
        // API URL الخاص بـ sms.to
        private const string baseUrl = "https://api.sms.to/sms/send";

        // مفتاح API الخاص بحسابك في sms.to
        private const string apiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczovL2F1dGg6ODA4MC9hcGkvdjEvdXNlcnMvYXBpL2tleXMvZ2VuZXJhdGUiLCJpYXQiOjE3MzQ0NDIyNDUsIm5iZiI6MTczNDQ0MjI0NSwianRpIjoidjh4YXF4MmdFcGJBdGcwaCIsInN1YiI6NDY5ODczLCJwcnYiOiIyM2JkNWM4OTQ5ZjYwMGFkYjM5ZTcwMWM0MDA4NzJkYjdhNTk3NmY3In0.b2zTxm3D0keq2Qc2sVyjv9vTDh4nj7AkYM6wcUgJMrw";
        private const string sender = "G4Fit";

        public async static Task SendMessageAsync(string CountryCode, string PhoneNumber, string Message)
        {
            try
            {
                if (!string.IsNullOrEmpty(CountryCode) && !string.IsNullOrEmpty(PhoneNumber) && !string.IsNullOrEmpty(Message))
                {
                    string fullNumber = $"{CountryCode}{PhoneNumber}";

                    var requestBody = new
                    {
                        to = fullNumber,
                        message = Message,
                        sender_id = sender
                    };

                    var json = JsonConvert.SerializeObject(requestBody);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await client.PostAsync(baseUrl, data);
                        string result = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }
}
