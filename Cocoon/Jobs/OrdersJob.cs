using G4Fit.Helpers;
using G4Fit.Models.Data;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace G4Fit.Jobs
{
    public class OrdersJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var InitializedOrders = db.Orders.Where(d => d.IsDeleted == false && d.OrderStatus == OrderStatus.Initialized && d.SMSNotificationsCount < 1 && d.UserId != null).ToList();
                var UncompletedOrders = db.Orders.Where(w => w.IsDeleted == false && w.OrderStatus == OrderStatus.Initialized).ToList();
                var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                foreach (var Order in InitializedOrders)
                {
                    var CreatedOnDate = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                    DateTime? ModifiedOn = null;
                    DateTime? LastSMSDate = null;
                    if (Order.User.CountryId.HasValue == true)
                    {
                        CreatedOnDate = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById(Order.User.Country.TimeZoneId));
                        DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(Order.User.Country.TimeZoneId));
                    }
                    if (Order.ModifiedOn.HasValue == true)
                    {
                        ModifiedOn = TimeZoneInfo.ConvertTimeFromUtc(Order.ModifiedOn.Value, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                        if (Order.User.CountryId.HasValue == true)
                        {
                            ModifiedOn = TimeZoneInfo.ConvertTimeFromUtc(Order.ModifiedOn.Value, TimeZoneInfo.FindSystemTimeZoneById(Order.User.Country.TimeZoneId));
                        }
                    }
                    if (Order.LastSMSNotificationDateSent.HasValue == true)
                    {
                        LastSMSDate = TimeZoneInfo.ConvertTimeFromUtc(Order.LastSMSNotificationDateSent.Value, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                        if (Order.User.CountryId.HasValue == true)
                        {
                            LastSMSDate = TimeZoneInfo.ConvertTimeFromUtc(Order.LastSMSNotificationDateSent.Value, TimeZoneInfo.FindSystemTimeZoneById(Order.User.Country.TimeZoneId));
                        }
                    }

                    TimeSpan DateDifference = DateTimeNow - CreatedOnDate;
                    if (ModifiedOn.HasValue == true)
                    {
                        DateDifference = DateTimeNow - ModifiedOn.Value;
                    }
                    if (LastSMSDate.HasValue == true)
                    {
                        DateDifference = DateTimeNow - LastSMSDate.Value;
                    }

                    var Timer = TimeSpan.FromHours(1);
                    var TimeLeftInHours = (int)(Timer - DateDifference).TotalHours;
                    if (TimeLeftInHours <= 0)
                    {
                        //TODO: send reminder sms.
                        await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, "يتبقى خطوه اخيره لانهاء طلبك", "ماذا تنتظر ؟ قم باستكمال الطلب الان", NotificationType.BasketPage, Order.Id, true);
                        await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, "يتبقى خطوه اخيره لانهاء طلبك", "ماذا تنتظر ؟ قم باستكمال الطلب الان", NotificationType.BasketPage, Order.Id, true);
                        Order.SMSNotificationsCount += 1;
                        Order.LastSMSNotificationDateSent = DateTime.UtcNow.ToUniversalTime();
                    }
                }

                foreach (var Order in UncompletedOrders)
                {
                    var CreatedOnDate = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                    TimeSpan DateDifference = DateTimeNow - CreatedOnDate;
                    var Timer = TimeSpan.FromDays(3);
                    var TimeLeftInHours = (int)(Timer - DateDifference).TotalDays;
                    if (TimeLeftInHours <= 0)
                    {
                        CRUD<Order>.Delete(Order);
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                db.SaveChanges();
            }
        }

        public static async Task RunProgram()
        {
            try
            {
                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<OrdersJob>()
                    .WithIdentity("job1", "group1")
                    .Build();

                // Trigger the job to run now, and then repeat every n seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await scheduler.ScheduleJob(job, trigger);

                // some sleep to show what's happening

                // and last shut down the scheduler when you are ready to close your program
            }
            catch (SchedulerException se)
            {
            }
        }
    }
}