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
    public class PackagesJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var Packages = db.UserPackages.Where(d => d.IsActive == true).ToList();
                var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                foreach (var package in Packages)
                {
                    var FinishOnDate = TimeZoneInfo.ConvertTimeFromUtc(package.FinishOn, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                    if (package.User.CountryId.HasValue == true)
                    {
                        FinishOnDate = TimeZoneInfo.ConvertTimeFromUtc(package.FinishOn, TimeZoneInfo.FindSystemTimeZoneById(package.User.Country.TimeZoneId));
                        DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(package.User.Country.TimeZoneId));
                    }
                    if (FinishOnDate < DateTimeNow)
                    {
                        package.IsActive = false;
                        await Notifications.SendToAllSpecificAndroidUserDevices(package.UserId, $"انتهاء اشتراكك فى الباقة [{package.Package.NameAr}]", $"لقد تم انتهاء فتره اشتراكك للباقة ، للتجديد برجاء معاودة الشراء", NotificationType.PackagesPage, IsSaveInDatabase: true);
                        await Notifications.SendToAllSpecificIOSUserDevices(package.UserId, $"انتهاء اشتراكك فى الباقة [{package.Package.NameAr}]", $"لقد تم انتهاء فتره اشتراكك للباقة ، للتجديد برجاء معاودة الشراء", NotificationType.PackagesPage, IsSaveInDatabase: true);
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
                IJobDetail job = JobBuilder.Create<PackagesJob>()
                    .WithIdentity("job2", "group2")
                    .Build();

                // Trigger the job to run now, and then repeat every n seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger2", "group2")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(10)
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