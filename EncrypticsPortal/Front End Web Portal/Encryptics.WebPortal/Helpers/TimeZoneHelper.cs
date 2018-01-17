using System;
using System.Diagnostics;

namespace Encryptics.WebPortal.Helpers
{
    public static class TimeZoneHelper
    {
        public static DateTime? FromUTC(this DateTime? dateTime, int offset, bool supportsDaylightSavingsTime)
        {
            if (dateTime.HasValue)
            {
                return UTCHelper.GetLocalDateTime(dateTime.Value, offset, supportsDaylightSavingsTime);
            }

            return null;
        }

        public static DateTime? ToUTC(this DateTime? dateTime, int offset, bool supportsDaylightSavingsTime)
        {
            if (dateTime.HasValue)
            {
                return UTCHelper.GetUTCDateTime(dateTime.Value, offset, supportsDaylightSavingsTime);
            }

            return null;
        }

        public static DateTime FromUTC(this DateTime dateTime, int offset, bool supportsDaylightSavingsTime)
        {
            return UTCHelper.GetLocalDateTime(dateTime, offset, supportsDaylightSavingsTime);
        }

        public static DateTime ToUTC(this DateTime dateTime, int offset, bool supportsDaylightSavingsTime)
        {
            return UTCHelper.GetUTCDateTime(dateTime, offset, supportsDaylightSavingsTime);
        }
    }

    public class UTCHelper
    {
        private readonly bool _supportsDaylightSavingsTime;
        private readonly int _localUTCOffsetMinutes;

        public UTCHelper(int localUTCOffsetMinutes, bool supportsDaylightSavingsTime)
        {
            _supportsDaylightSavingsTime = supportsDaylightSavingsTime;
            _localUTCOffsetMinutes = localUTCOffsetMinutes;
        }

        public DateTime GetUTCDateTime(DateTime dateTime)
        {
            Debug.Print("GetUTCDateTime");
            Debug.Print("dateTime: {0}", dateTime);
            var localOffsetTimeSpan = GetLocalOffsetTimeSpan(dateTime);
            Debug.Print("localOffsetTimeSpan: {0}", localOffsetTimeSpan);
            var dateTimeOffset = new DateTimeOffset(dateTime, localOffsetTimeSpan);
            Debug.Print("dateTimeOffset: {0}", dateTimeOffset);
            var utcDateTime = dateTimeOffset.UtcDateTime;
            Debug.Print("utcDateTime: {0}", utcDateTime);

            return utcDateTime;
        }

        public static DateTime GetUTCDateTime(DateTime dateTime, int localUTCOffsetMinutes,
                                              bool supportsDaylightSavingsTime)
        {
            var utcHelper = new UTCHelper(localUTCOffsetMinutes, supportsDaylightSavingsTime);
            var utcDateTime = utcHelper.GetUTCDateTime(dateTime);

            return utcDateTime;
        }

        public DateTime GetLocalDateTime(DateTime dateTime)
        {
            Debug.Print("GetLocalDateTime");
            Debug.Print("dateTime: {0}", dateTime);
            var dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.Zero);
            Debug.Print("dateTimeOffset: {0}", dateTimeOffset);
            var localOffsetTimeSpan = GetLocalOffsetTimeSpan(dateTime);
            Debug.Print("localOffsetTimeSpan: {0}", localOffsetTimeSpan);
            var timeOffset = dateTimeOffset.ToOffset(localOffsetTimeSpan);
            Debug.Print("timeOffset: {0}", timeOffset);
            var localDateTime = timeOffset.DateTime;
            Debug.Print("localDateTime: {0}", localDateTime);

            return localDateTime;
        }

        public static DateTime GetLocalDateTime(DateTime dateTime, int localUTCOffsetMinutes,
                                                bool supportsDaylightSavingsTime)
        {
            var utcHelper = new UTCHelper(localUTCOffsetMinutes, supportsDaylightSavingsTime);
            var localDateTime = utcHelper.GetLocalDateTime(dateTime);

            return localDateTime;
        }

        public TimeSpan GetLocalOffsetTimeSpan(DateTime dateTime)
        {
            Debug.Print("GetLocalOffsetTimeSpan");
            Debug.Print("dateTime: {0}", dateTime);
            var localToday = GetUTCOffsetToday().ToLocalTime();
            Debug.Print("localToday: {0}", localToday);

            Debug.Print("_supportsDaylightSavingsTime: {0}", _supportsDaylightSavingsTime);
            var isDaylightSavingsTime = IsDaylightSavingsTime(dateTime);
            Debug.Print("IsDaylightSavingsTime(dateTime): {0}", isDaylightSavingsTime);
            var daylightSavingsTime = IsDaylightSavingsTime(localToday);
            Debug.Print("IsDaylightSavingsTime(localToday): {0}", daylightSavingsTime);
            if (!_supportsDaylightSavingsTime ||
                (_supportsDaylightSavingsTime && isDaylightSavingsTime == daylightSavingsTime))
            {
                return new TimeSpan(0, 0, _localUTCOffsetMinutes, 0);
            }

            if (daylightSavingsTime && !isDaylightSavingsTime)
            {
                return new TimeSpan(0, 0, _localUTCOffsetMinutes - 60, 0);
            }

            return new TimeSpan(0, 0, _localUTCOffsetMinutes + 60, 0);
        }

        public DateTimeOffset GetUTCDateTimeOffset(DateTime dateTime)
        {
            Debug.Print("GetUTCDateTimeOffset");
            var utcDateTimeOffset = new DateTimeOffset(DateTime.UtcNow);
            Debug.Print("utcDateTimeOffset: {0}", utcDateTimeOffset);
            var localOffsetTimeSpan = GetLocalOffsetTimeSpan(dateTime);
            Debug.Print("localOffsetTimeSpan: {0}", localOffsetTimeSpan);
            var localDateTimeOffset = utcDateTimeOffset.ToOffset(localOffsetTimeSpan);
            Debug.Print("localDateTimeOffset: {0}", localDateTimeOffset);
            var dateTimeOffset = new DateTimeOffset(localDateTimeOffset.Date, localOffsetTimeSpan);
            Debug.Print("dateTimeOffset: {0}", dateTimeOffset);
            var utcDateTime = dateTimeOffset.UtcDateTime;
            Debug.Print("utcDateTime: {0}", utcDateTime);

            return utcDateTime;
        }

        public static DateTime GetUTCOffsetToday(int localOffsetMinutes)
        {
            Debug.Print("GetUTCOffsetToday");
            Debug.Print("localOffsetMinutes: {0}", localOffsetMinutes);
            var utcDateTimeOffset = new DateTimeOffset(DateTime.UtcNow);
            Debug.Print("utcDateTimeOffset: {0}", utcDateTimeOffset);
            var localOffsetTimeSpan = new TimeSpan(0, localOffsetMinutes, 0);
            Debug.Print("localOffsetTimeSpan: {0}", localOffsetTimeSpan);
            var localDateTimeOffset = utcDateTimeOffset.ToOffset(localOffsetTimeSpan);
            Debug.Print("localDateTimeOffset: {0}", localDateTimeOffset);
            Debug.Print("Today: {0}", localDateTimeOffset.Date);
            utcDateTimeOffset = new DateTimeOffset(localDateTimeOffset.Date, localOffsetTimeSpan);
            Debug.Print("utcDateTimeOffset: {0}", utcDateTimeOffset);
            var utcOffsetToday = utcDateTimeOffset.UtcDateTime;
            Debug.Print("Today UTC: {0}", utcOffsetToday);

            return utcOffsetToday;
        }

        public DateTime GetUTCOffsetToday()
        {
            return GetUTCOffsetToday(_localUTCOffsetMinutes);
        }

        public static bool IsDaylightSavingsTime(DateTime dateTime)
        {
            var month = dateTime.Month;
            var day = dateTime.Day;
            var dow = (int) dateTime.DayOfWeek;
            int previousSunday = day - dow;

            //January, february, and december are out.
            if (month < 3 || month > 11)
            {
                return false;
            }

            //April to October are in
            if (month > 3 && month < 11)
            {
                return true;
            }

            //In march, we are DST if our previous sunday was on or after the 8th. After 2 am.
            if (month == 3)
            {
                if (previousSunday == 8 && dateTime.TimeOfDay > new TimeSpan(0, 2, 0, 0))
                {
                    return true;
                }

                return previousSunday >= 8;
            }

            //In november we must be before the first sunday to be dst.
            //That means the previous sunday must be before the 1st. Before 2 am.
            return previousSunday <= 0 && dateTime.TimeOfDay < new TimeSpan(0, 2, 0, 0);
        }
    }
}