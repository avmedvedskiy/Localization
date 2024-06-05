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

        public const string LOCALIZATION_SHEET_COMMON = "Common";
        const string TIME_FORMAT_SECONDS = "TimerFormat.Seconds";
        const string TIME_FORMAT_MINUTES = "TimerFormat.Minutes";
        const string TIME_FORMAT_HOURS = "TimerFormat.Hours";
        const string TIME_FORMAT_DAYS = "TimerFormat.Days";

        public static string FormatTime(TimeFrom formatFrom, int timeInSeconds, string sheet = LOCALIZATION_SHEET_COMMON)
        {
            var timeSpan = new TimeSpan(0, 0, timeInSeconds);
            switch (formatFrom)
            {
                case TimeFrom.Days:
                    return GetDaysFormat(timeSpan, sheet);
                case TimeFrom.Hours:
                    return GetHoursFormat(timeSpan, sheet);
                case TimeFrom.Minutes:
                    return GetMinutesFormat(timeSpan, sheet);
                case TimeFrom.Seconds:
                default:
                    return GetSecondsFormat(timeSpan, sheet);
            }
        }

        private static string GetSecondsFormat(TimeSpan time, string sheet)
        {
            string text = Localization.Get(TIME_FORMAT_SECONDS, sheet);
            return string.Format(text, time.TotalSeconds);
        }

        private static string GetMinutesFormat(TimeSpan time, string sheet)
        {
            var ts = time;
            string text = Localization.Get(TIME_FORMAT_MINUTES, sheet);
            int minutes = (int)ts.TotalMinutes;
            if (minutes != 0)
                return string.Format(text, minutes, ts.Seconds);
            else
                return GetSecondsFormat(time, sheet);
        }

        private static string GetHoursFormat(TimeSpan time, string sheet)
        {
            var ts = time;
            string text = Localization.Get(TIME_FORMAT_HOURS, sheet);
            int hours = (int)ts.TotalHours;
            if (hours != 0)
                return string.Format(text, hours, ts.Minutes);
            else
                return GetMinutesFormat(time, sheet);
        }

        private static string GetDaysFormat(TimeSpan time, string sheet)
        {
            var ts = time;
            string text = Localization.Get(TIME_FORMAT_DAYS, sheet);
            int days = (int)ts.TotalDays;
            if (days != 0)
                return string.Format(text, days, ts.Hours);
            else
                return GetHoursFormat(time, sheet);
        }
    }
}