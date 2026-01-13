
using System;
using System.Linq;

using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Rock;

/*
 * This class hosts any methods in the Rock.Model.EventCalendarService class that are currently inaccessible.
 * We'll add and remove methods from here as they become public
 */
namespace com.bemaservices.RoomManagement.Utility.RockInternalMethods
{

    public static class EventCalendarServiceOverrides
    {

        /// <summary>
        /// Gets the sequence number.
        /// </summary>
        /// <param name="createdDateTime">The created date time.</param>
        /// <param name="modifiedDateTime">The modified date time.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetSequenceNumber( DateTime? createdDateTime, DateTime? modifiedDateTime )
        {
            var minCreatedDateTime = RockDateTime.New( 2020, 1, 1 ).Value;

            createdDateTime = createdDateTime ?? minCreatedDateTime;
            if ( createdDateTime < minCreatedDateTime )
            {
                createdDateTime = minCreatedDateTime;
            }

            modifiedDateTime = modifiedDateTime ?? createdDateTime;
            if ( modifiedDateTime < createdDateTime )
            {
                modifiedDateTime = createdDateTime;
            }

            var sequenceNo = ( int ) modifiedDateTime.Value.Subtract( createdDateTime.Value ).TotalSeconds;
            return sequenceNo;
        }

        /// <summary>
        /// Copies the calendar event.
        /// </summary>
        /// <param name="iCalEvent">The i cal event.</param>
        /// <returns>CalendarEvent.</returns>
        internal static CalendarEvent CopyCalendarEvent( CalendarEvent iCalEvent )
        {
            // The iCal.Net serializer is not thread-safe, so we need to create a new instance for each serialization.
            // See https://github.com/rianjs/ical.net/issues/553.
            var serializer = new CalendarSerializer();
            var iCalString = serializer.SerializeToString( iCalEvent );

            var eventCopy = Calendar.Load<CalendarEvent>( iCalString )
                .FirstOrDefault();

            return eventCopy;
        }


        /// <summary>
        /// Adjust the date and time information for this event to ensure that the serialized iCalendar data can be
        /// processed by calendaring applications such as Microsoft Outlook Web, Google Calendar and Apple Calendar.
        /// These applications require specific date/time formats and value combinations for a valid import format.
        /// </summary>
        /// <param name="iCalEvent">The iCal.NET calendar event.</param>
        /// <param name="timeZoneId">The IANA time zone identifier.</param>
        internal static void SetCalendarEventDateTimeInfo( CalendarEvent iCalEvent, string timeZoneId = null )
        {
            // Determine the start and end time for the event.
            // For an all-day event, omit the End date.
            // see https://stackoverflow.com/questions/1716237/single-day-all-day-appointments-in-ics-files
            var start = iCalEvent.Start;

            timeZoneId = timeZoneId ?? iCalEvent.Start.TzId;

            iCalEvent.Start = ConvertToCalDateTime( start, timeZoneId );

            // Determine if this is an all-day event. The Rock ScheduleBuilder component adopts a convention of
            // assigning a 1 second duration to an event if the duration was not specified as part of the input.
            // Therefore, if the event starts at midnight and has a duration of <= 1s, assume it is an all day event.
            var startTime = new TimeSpan( start.Hour, start.Minute, start.Second );
            if ( startTime.TotalSeconds == 0 && ( iCalEvent.Duration == null || iCalEvent.Duration.TotalSeconds <= 1 ) )
            {
                iCalEvent.IsAllDay = true;
            }

            if ( iCalEvent.IsAllDay )
            {
                iCalEvent.End = null;
            }
            else
            {
                iCalEvent.End = ConvertToCalDateTime( iCalEvent.Start.Add( iCalEvent.Duration ), timeZoneId );
            }
        }



        /// <summary>
        /// Converts to cal date time.
        /// </summary>
        /// <param name="newDateTime">The new date time.</param>
        /// <param name="tzId">The tz identifier.</param>
        /// <returns>CalDateTime.</returns>
        internal static CalDateTime ConvertToCalDateTime( IDateTime newDateTime, string tzId )
        {
            if ( newDateTime is CalDateTime cdt )
            {
                if ( tzId != null )
                {
                    cdt.TzId = tzId;
                }
                return cdt;
            }

            var dateTime = new DateTime( newDateTime.Year, newDateTime.Month, newDateTime.Day, newDateTime.Hour, newDateTime.Minute, newDateTime.Second, newDateTime.Millisecond, DateTimeKind.Local );

            var newDate = ConvertToCalDateTime( dateTime, tzId );

            return newDate;
        }

        /// <summary>
        /// Converts to cal date time.
        /// </summary>
        /// <param name="newDateTime">The new date time.</param>
        /// <param name="tzId">The tz identifier.</param>
        /// <returns>CalDateTime.</returns>
        internal static CalDateTime ConvertToCalDateTime( DateTime newDateTime, string tzId )
        {
            var newDate = new CalDateTime( newDateTime );
            if ( tzId != null )
            {
                newDate.TzId = tzId;
            }

            // Set the HasTime property to ensure that iCal.Net serializes the date value as an iCalendar "DATE" rather than a "PERIOD".
            // Microsoft Outlook ignores date values that are expressed using the iCalendar "PERIOD" type.
            // (see: MS-STANOICAL - v20210817 - 2.2.86)
            newDate.HasTime = true;

            return newDate;
        }

    }
}
