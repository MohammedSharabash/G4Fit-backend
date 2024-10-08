using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G4Fit.Helpers
{
    public static class FormatDate
    {
        public static string TimeToString(string lang, TimeSpan time)
        {
            string DateFormat = null;
            if (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar")
            {
                if (time.Hours <= 0 && time.Minutes > 0)
                {
                    if (time.Minutes == 1)
                    {
                        DateFormat = $"دقيقة";
                    }
                    else if (time.Minutes == 2)
                    {
                        DateFormat = $"دقيقتان";
                    }
                    else if (time.Minutes > 2 && time.Minutes <= 10)
                    {
                        DateFormat = $"{time.Minutes} دقائق";
                    }
                    else
                    {
                        DateFormat = $"{time.Minutes} دقيقة";
                    }
                }
                else if (time.Hours > 0 && time.Minutes <= 0)
                {
                    if (time.Hours == 1)
                    {
                        DateFormat = $"ساعة واحدة";
                    }
                    else if (time.Hours == 2)
                    {
                        DateFormat = $"ساعتان";
                    }
                    else if (time.Hours > 2 && time.Hours <= 10)
                    {
                        DateFormat = $"{time.Hours} ساعات";
                    }
                    else if (time.Hours > 10)
                    {
                        DateFormat = $"{time.Hours} ساعة";
                    }
                }
                else if (time.Hours > 0 && time.Minutes > 0)
                {
                    if (time.Hours == 1)
                    {
                        if (time.Minutes == 1)
                        {
                            DateFormat = $"ساعة و دقيقة";
                        }
                        else if (time.Minutes == 2)
                        {
                            DateFormat = $"ساعة و دقيقتان";
                        }
                        else if (time.Minutes > 2 && time.Minutes <= 10)
                        {
                            DateFormat = $"ساعة و {time.Minutes} دقائق";
                        }
                        else
                        {
                            DateFormat = $"ساعة و {time.Minutes} دقيقة";
                        }
                    }
                    else if (time.Hours == 2)
                    {
                        if (time.Minutes == 1)
                        {
                            DateFormat = $"ساعتان و دقيقة";
                        }
                        else if (time.Minutes == 2)
                        {
                            DateFormat = $"ساعتان و دقيقتان";
                        }
                        else if (time.Minutes > 2 && time.Minutes <= 10)
                        {
                            DateFormat = $"ساعتان و {time.Minutes} دقائق";
                        }
                        else
                        {
                            DateFormat = $"ساعتان و {time.Minutes} دقيقة";
                        }
                    }
                    else if (time.Hours > 2 && time.Hours <= 10)
                    {
                        if (time.Minutes == 1)
                        {
                            DateFormat = $"{time.Hours} ساعات و دقيقة";
                        }
                        else if (time.Minutes == 2)
                        {
                            DateFormat = $"{time.Hours} ساعات و دقيقتان";
                        }
                        else if (time.Minutes > 2 && time.Minutes <= 10)
                        {
                            DateFormat = $"{time.Hours} ساعات و {time.Minutes} دقائق";
                        }
                        else
                        {
                            DateFormat = $"{time.Hours} ساعات و {time.Minutes} دقيقة";
                        }
                    }
                    else if (time.Hours > 10)
                    {
                        if (time.Minutes == 1)
                        {
                            DateFormat = $"{time.Hours} {time.Hours} ساعة و دقيقة";
                        }
                        else if (time.Minutes == 2)
                        {
                            DateFormat = $"{time.Hours} {time.Hours} ساعة و دقيقتان";
                        }
                        else if (time.Minutes > 2 && time.Minutes <= 10)
                        {
                            DateFormat = $"{time.Hours} {time.Hours} ساعة و {time.Minutes} دقائق";
                        }
                        else
                        {
                            DateFormat = $"{time.Hours} {time.Hours} ساعة و {time.Minutes} دقيقة";
                        }
                    }
                }
            }
            else
            {
                if (time.Hours <= 0 && time.Minutes > 0)
                {
                    if (time.Minutes == 1)
                    {
                        DateFormat = $"1 Mintue";
                    }
                    else
                    {
                        DateFormat = $"{time.Minutes} Minutes";
                    }
                }
                else if (time.Hours > 0 && time.Minutes <= 0)
                {
                    if (time.Hours == 1)
                    {
                        DateFormat = $"1 Hour";
                    }
                    else
                    {
                        DateFormat = $"{time.Hours} Hours";
                    }
                }
                else if (time.Hours > 0 && time.Minutes > 0)
                {
                    if (time.Hours == 1)
                    {
                        if (time.Minutes == 1)
                        {
                            DateFormat = $"1 Hour, 1 Minute";
                        }
                        else
                        {
                            DateFormat = $"1 Hour, {time.Minutes} Minutes";
                        }
                    }
                    else
                    {
                        if (time.Minutes == 1)
                        {
                            DateFormat = $"{time.Hours} Hours, 1 Minute";
                        }
                        else
                        {
                            DateFormat = $"{time.Hours} Hours, {time.Minutes} Minutes";
                        }
                    }
                }
            }
            return DateFormat;
        }
    }
}