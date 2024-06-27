using NodaTime.Extensions;
using NodaTime;
using System;

namespace LibraryManagement.API.Helper
{
    public class TimeZoneConverter
    {
        public static DateTimeOffset ConvertUtcToTimeZone(DateTime utcDateTime, string timeZoneId)
        {

            // If dateTime's kind is not UTC, convert it to UTC
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            // Create a DateTimeZone object using the provided time zone ID
            DateTimeZone userTimeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

            // Create a ZonedDateTime object from the UTC DateTime and the user's time zone
            ZonedDateTime userZonedDateTime = utcDateTime.ToInstant().InZone(userTimeZone);

            // Convert the ZonedDateTime to a DateTimeOffset
            DateTimeOffset userDateTimeOffset = userZonedDateTime.ToDateTimeOffset();

            return userDateTimeOffset;
        }
    }
}
