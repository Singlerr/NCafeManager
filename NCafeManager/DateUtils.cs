using System;
using System.Globalization;

namespace Utilities
{
    internal class DateUtils
    {
        public static DateTime? ParseDateTime(string date)
        {
            var ko = new CultureInfo("ko-KR");
            DateTime dateTime;
            var result = DateTime.TryParseExact(date, "yyyy.MM.dd.", ko,
                DateTimeStyles.None, out dateTime);
            if (!result)
            {
                result = DateTime.TryParseExact(date, "HH:mm", ko,
                    DateTimeStyles.None, out dateTime);
                if (!result) return null;

                return dateTime;
            }

            return dateTime;
        }
    }
}