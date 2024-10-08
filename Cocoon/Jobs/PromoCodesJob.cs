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
    public class PromoCodesJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var PromoCodes = db.PromoCodes.Where(w => w.IsDeleted == false && w.IsFinished == false && w.FinishOn.HasValue == true).ToList();
                var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                foreach (var item in PromoCodes)
                {
                    if (item.FinishOn.Value.Date < DateTimeNow.Date)
                    {
                        item.IsFinished = true;
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
                IJobDetail job = JobBuilder.Create<PromoCodesJob>()
                    .WithIdentity("job3", "group3")
                    .Build();

                // Trigger the job to run now, and then repeat every n seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger3", "group3")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(30)
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