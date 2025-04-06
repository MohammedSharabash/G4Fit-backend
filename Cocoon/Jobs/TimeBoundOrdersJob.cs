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
    public class TimeBoundOrdersJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var TimeBoundOrders = db.Orders.Where(w => w.IsDeleted == false && w.OrderStatus != OrderStatus.Initialized && w.OrderStatus != OrderStatus.Delivered).ToList();
                var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                foreach (var order in TimeBoundOrders)
                {
                    foreach (var item in order.Items.Where(i => !i.IsDeleted))
                    {
                        if (order.Frezzed)
                        {
                            if (item.RemainFreezingDays > 0)
                            {
                                item.RemainFreezingDays -= 1;
                            }
                            else
                            {
                                // فك التجميد وابدأ تنقيص أيام الاشتراك
                                order.Frezzed = false;

                                if (item.RemainServiceDays > 0)
                                    item.RemainServiceDays -= 1;
                            }
                        }
                        else
                        {
                            if (item.RemainServiceDays > 0)
                                item.RemainServiceDays -= 1;
                        }
                    }

                    // لو كل العناصر خلصت أيام الاشتراك
                    if (order.Items.All(i => i.RemainServiceDays <= 0))
                    {
                        order.OrderStatus = OrderStatus.Delivered;
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
                // إعدادات الـ Quartz
                NameValueCollection props = new NameValueCollection
        {
            { "quartz.serializer.type", "binary" }
        };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                await scheduler.Start();

                // تعريف الـ Job
                IJobDetail job = JobBuilder.Create<TimeBoundOrdersJob>()
                    .WithIdentity("TimeBoundOrdersJob", "DailyJobs")
                    .Build();

                // Trigger لتشغيل الـ Job يوميًا الساعة 12:01 صباحًا فقط
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("DailyTrigger", "DailyJobs")
                    .WithCronSchedule("0 1 0 * * ?") // الساعة 12:01 صباحًا
                    .Build();

                // ربط الـ Job بالـ Trigger
                await scheduler.ScheduleJob(job, trigger);
            }
            catch (SchedulerException se)
            {
                // التعامل مع الخطأ حسب الحاجة
            }
        }
    }
}