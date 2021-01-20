using System;
using System.Text;

namespace WinOSExtensions.Extensions
{
    public static class TimeSpanExtensions
    {
        // TODO: Find a more efficient way of creating the timer string.
        public static string ToTimerString(this TimeSpan span)
        {
            var sb = new StringBuilder();
            var hours = (int)span.TotalHours;
            var minutes = (int)span.TotalMinutes;

            if (hours > 0)
            {
                sb.Append(hours);
                sb.Append(':');
            }

            if (hours > 0 || minutes > 0)
            {
                sb.Append(span.Minutes.ToString("00"));
                sb.Append(':');
            }

            sb.Append(span.Seconds.ToString("00"));
            sb.Append('.');

            var msstr = span.Milliseconds.ToString();
            if (msstr.Length > 3)
            {
                msstr = msstr.Substring(0, 3);
            }

            while (msstr.Length < 3)
            {
                msstr += "0";
            }

            sb.Append(msstr);

            return sb.ToString();
        }
    }
}
