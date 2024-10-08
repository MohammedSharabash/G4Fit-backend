using G4Fit.Models.Data;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;

namespace G4Fit.Helpers
{
    public class Notifications
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        //Android
        private static string UserServerKey = ConfigurationManager.AppSettings["USER_SERVER_API_KEY"];
        private static string UserSenderId = ConfigurationManager.AppSettings["USER_SERVER_SENDER_ID"];
        
        //IOS
        private static string UserFullPath = HostingEnvironment.MapPath("~") + "/Files/UserCertificate.p12";
        private static string certPassword = "Project1234";
        public static void SaveInDatabase(string UserId, string Title, string Message, NotificationType notificationType, long Id = -1)
        {
            try
            {
                var oldNotification = db.Notifications.FirstOrDefault(w => w.IsDeleted == false && w.IsSeen == false && w.Title == w.Title && w.Body == w.Body && w.Id == Id && w.NotificationType == notificationType && w.UserId == UserId);
                if (oldNotification == null)
                {
                    db.Notifications.Add(new Notification()
                    {
                        Body = Message,
                        IsSeen = false,
                        Title = Title,
                        UserId = UserId,
                        RequestId = Id,
                        NotificationType = notificationType,
                    });
                }
                else
                {
                    oldNotification.CreatedOn = DateTime.Now.ToUniversalTime();
                }
                db.SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        public async static Task SendToAllSpecificAndroidUserDevices(string UserId, string Title, string Message, NotificationType NotificationType, long Id = -1, bool IsSaveInDatabase = true)
        {
            var users = db.UserPushTokens.Where(d => d.OS == OS.Android && d.UserId == UserId && d.IsDeleted == false).ToList();
            foreach (var user in users)
            {
                var message = new
                {
                    to = user.PushToken,
                    priority = "high",
                    content_available = true,
                    notification = new
                    {
                        body = Message,
                        title = Title,
                        sound = "default"
                    },
                    data = new
                    {
                        id = Id.ToString(),
                        notificationtype = ((int)NotificationType).ToString(),
                    }
                };

                var Serializer = new JavaScriptSerializer();
                var Json = Serializer.Serialize(message);
                var byteArray = Encoding.UTF8.GetBytes(Json);

                try
                {
                    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    tRequest.Method = "post";
                    tRequest.ContentType = "application/json";
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", UserServerKey));
                    tRequest.Headers.Add(string.Format("Sender: id={0}", UserSenderId));
                    tRequest.ContentLength = byteArray.Length;

                    using (Stream dataStream = tRequest.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        using (WebResponse tResponse = tRequest.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    string sResponseFromServer = tReader.ReadToEnd();
                                    string str = sResponseFromServer;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }
            if (IsSaveInDatabase == true)
            {
                SaveInDatabase(UserId, Title, Message, NotificationType, Id);
            }
        }

        public async static Task SendToAllAndroidDevices(string Title, string Message, bool IsSaveInDatabase = true)
        {
            var users = db.UserPushTokens.Where(d => d.OS == OS.Android && d.IsDeleted == false).Select(d => new { d.PushToken, d.UserId }).Distinct().ToList();

            foreach (var user in users)
            {
                var message = new
                {
                    to = user.PushToken,
                    priority = "high",
                    content_available = true,
                    notification = new
                    {
                        body = Message,
                        title = Title,
                        sound = "default"
                    },
                    data = new
                    {
                        id = "-1",
                        notificationtype = ((int)NotificationType.General).ToString(),
                    }
                };

                var Serializer = new JavaScriptSerializer();
                var Json = Serializer.Serialize(message);
                var byteArray = Encoding.UTF8.GetBytes(Json);

                try
                {
                    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    tRequest.Method = "post";
                    tRequest.ContentType = "application/json";
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", UserServerKey));
                    tRequest.Headers.Add(string.Format("Sender: id={0}", UserSenderId));
                    tRequest.ContentLength = byteArray.Length;

                    using (Stream dataStream = tRequest.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        using (WebResponse tResponse = tRequest.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    string sResponseFromServer = tReader.ReadToEnd();
                                    string str = sResponseFromServer;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                if (IsSaveInDatabase == true)
                {
                    SaveInDatabase(user.UserId, Title, Message, NotificationType.General);
                }
            }
        }

        public async static Task SendToAllSpecificIOSUserDevices(string UserId, string Title, string Message, NotificationType NotificationType, long Id = 0, bool IsSaveInDatabase = true)
        {
            var users = db.UserPushTokens.Where(d => d.OS == OS.IOS && d.UserId == UserId && d.IsDeleted == false).ToList();

            foreach (var user in users)
            {
                try
                {
                    var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Production, UserFullPath, certPassword);


                    // Create a new broker
                    var apnsBroker = new ApnsServiceBroker(config);

                    // Start the broker
                    apnsBroker.Start();

                    var Data = new
                    {
                        aps = new
                        {
                            alert = new
                            {
                                title = Title,
                                body = Message,
                            },
                            badge = 1,
                            sound = "default",
                        },
                        Id = Id,
                        NotificationType = (int)NotificationType
                    };

                    var modelToJson = JsonConvert.SerializeObject(Data);
                    var Payloadd = JObject.Parse(modelToJson);

                    apnsBroker.QueueNotification(new ApnsNotification
                    {
                        DeviceToken = user.PushToken,
                        Payload = Payloadd,
                    });

                    // Stop the broker, wait for it to finish   
                    // This isn't done after every message, but after you're
                    // done with the broker
                    apnsBroker.Stop();

                }
                catch (Exception)
                {
                }

            }
            //if (IsSaveInDatabase == true)
            //{
            //    SaveInDatabase(UserId, Title, Message, NotificationType, Id, IsWorker: IsDriver);
            //}
        }

        public async static Task SendToAllIOSDevices(string Title, string Message, bool IsSaveInDatabase = true)
        {
            var users = db.UserPushTokens.Where(d => d.OS == OS.IOS && d.IsDeleted == false).Select(d => new { d.PushToken, d.UserId }).Distinct().ToList();

            foreach (var user in users)
            {
                try
                {
                    var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Production, UserFullPath, certPassword);


                    // Create a new broker
                    var apnsBroker = new ApnsServiceBroker(config);

                    // Start the broker
                    apnsBroker.Start();

                    var Data = new
                    {
                        aps = new
                        {
                            alert = new
                            {
                                title = Title,
                                body = Message,
                            },
                            badge = 1,
                            sound = "default",
                        },
                        Id = -1,
                        NotificationType = (int)NotificationType.General
                    };
                    var modelToJson = JsonConvert.SerializeObject(Data);
                    var Payloadd = JObject.Parse(modelToJson);

                    apnsBroker.QueueNotification(new ApnsNotification
                    {
                        DeviceToken = user.PushToken,
                        Payload = Payloadd,

                    });


                    // Stop the broker, wait for it to finish   
                    // This isn't done after every message, but after you're
                    // done with the broker
                    apnsBroker.Stop();
                }
                catch (Exception)
                {
                }
                if (IsSaveInDatabase == true)
                {
                    SaveInDatabase(user.UserId, Title, Message, NotificationType.General);
                }
            }
        }
    }
}