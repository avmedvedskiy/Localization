using System;

namespace LocalizationPackage.Utilities
{
    public static class TimeFormatManager
    {
        public enum TimeFrom
        {
            Seconds,
            Minutes,
            Hours,
            Days
        }

        const string TIME_FORMAT_SECONDS = "TimerFormat.Seconds";
        const string TIME_FORMAT_MINUTES = "TimerFormat.Minutes";
        const string TIME_FORMAT_HOURS = "TimerFormat.Hours";
        const string TIME_FORMAT_DAYS = "TimerFormat.Days";

        public static string FormatTime(TimeFrom formatFrom, int timeInSeconds)
        {
            var timeSpan = new TimeSpan(0, 0, timeInSeconds);
            switch (formatFrom)
            {
                case TimeFrom.Days:
                    return GetDaysFormat(timeSpan);
                case TimeFrom.Hours:
                    return GetHoursFormat(timeSpan);
                case TimeFrom.Minutes:
                    return GetMinutesFormat(timeSpan);
                case TimeFrom.Seconds:
                default:
                    return GetSecondsFormat(timeSpan);
            }
        }

        private static string GetSecondsFormat(TimeSpan time)
        {
            string text = Localization.Get(TIME_FORMAT_SECONDS);
            return string.Format(text, time.TotalSeconds);
        }

        private static string GetMinutesFormat(TimeSpan time)
        {
            string text = Localization.Get(TIME_FORMAT_MINUTES);
            int minutes = (int)time.TotalMinutes;
            return minutes != 0 
                ? string.Format(text, minutes, time.Seconds) 
                : GetSecondsFormat(time);
        }

        private static string GetHoursFormat(TimeSpan time)
        {
            string text = Localization.Get(TIME_FORMAT_HOURS);
            int hours = (int)time.TotalHours;
            return hours != 0 
                ? string.Format(text, hours, time.Minutes) 
                : GetMinutesFormat(time);
        }

        private static string GetDaysFormat(TimeSpan time)
        {
            string text = Localization.Get(TIME_FORMAT_DAYS);
            int days = (int)time.TotalDays;
            return days != 0 
                ? string.Format(text, days, time.Hours) 
                : GetHoursFormat(time);
        }
    }
}