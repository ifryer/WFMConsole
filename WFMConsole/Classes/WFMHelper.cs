using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using System.Reflection;

namespace WFMDashboard.Classes
{
    public static class WFMHelper
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool SubmitTimeOff(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, string date, bool fullDay, string startTime, string endTime, string notes, out string msg)
        {
            try
            {
                msg = "Successfully created event";
                var startDate = $"{date} {startTime}";
                var endDate = $"{date} {endTime}";

                var startDateTime = DateTime.Parse(startDate);
                var endDateTime = DateTime.Parse(endDate);

                if (endDateTime < startDateTime)
                {
                    msg = "Start time must be before end time";
                    return false;
                }

                SubmitTimeOff_Nice();

                

                SubmitTimeOff_GoogleCalendar(googleAuth, startDateTime, endDateTime, fullDay, notes);
                bool success = true;
                return success;
            }
            catch(Exception ex)
            {
                //Log error
                msg = ex.ToString();
                return false;
            }
            
        }
        private static bool SubmitTimeOff_Nice()
        {
            bool success = false;
            return success;
        }

        private static bool SubmitTimeOff_GoogleCalendar(Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp.AuthResult googleAuth, DateTime startDateTime, DateTime endDateTime, bool fullDay, string notes)
        {
            try
            {
                EventDateTime start = new EventDateTime();
                EventDateTime end = new EventDateTime();
                
                if (fullDay)
                {
                    start = new EventDateTime()
                    {
                        Date = startDateTime.ToString("yyyy-MM-dd"),
                        TimeZone = "America/New_York",

                    };
                    end = new EventDateTime()
                    {
                        Date = endDateTime.ToString("yyyy-MM-dd"),
                        TimeZone = "America/New_York",
                    };
                }
                else
                {
                    
                    start = new EventDateTime()
                    {
                        DateTime = startDateTime,
                        TimeZone = "America/New_York",

                    };
                    end = new EventDateTime()
                    {
                        DateTime = endDateTime,
                        TimeZone = "America/New_York",
                    };
                 }



                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = googleAuth.Credential,
                    ApplicationName = ApplicationName,
                });

                


                //var newEvent = new Event();
                //newEvent.Start = new EventDateTime() { DateTime = DateTime.Now };
                //newEvent.Description = "WFM DASHBOARD TEST EVENT";
                //newEvent.End = new EventDateTime() { DateTime = DateTime.Now.Add(TimeSpan) };

                //change primary to the calendar ID
                //var text = service.Events.Insert(newEvent, "primary");


                Event newEvent = new Event()
                {
                    Summary = "WFM Dashboard Test Event",
                    //Location = "800 Howard St., San Francisco, CA 94103",
                    Description = notes,
                    Start = start,
                    End = end,
                    //Recurrence = new String[] { "RRULE:FREQ=DAILY;COUNT=2" },
                    Attendees = new EventAttendee[] {
                        new EventAttendee() { Email = "rtyszka@kmbs.konicaminolta.us" },
                    },
                    //Reminders = new Event.RemindersData()
                    //{
                    //    UseDefault = false,
                    //    Overrides = new EventReminder[] {
                    //        new EventReminder() { Method = "email", Minutes = 24 * 60 },
                    //        new EventReminder() { Method = "sms", Minutes = 10 },
                    //    }
                    //}
                };

                String calendarId = "primary";
                EventsResource.InsertRequest request = service.Events.Insert(newEvent, calendarId);
                Event createdEvent = request.Execute();

                bool success = false;
                return success;
            }
            catch (Exception ex)
            {
                //Log error with google calendar
                throw ex;
            }
        }

        public static bool CreateDownByReport()
        {
            try
            {
                bool success = false;
                return success;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool CreateMOWReport()
        {
            try
            {
                bool success = false;
                return success;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public static bool GetTeamInfo()
        {
            try
            {
                bool success = false;
                return success;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }



        // ------------------- FROM GOOGLE CALENDAR API DOCUMENTATION ----------------------

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API .NET Quickstart";

        static void Main(string[] args)
        {
            UserCredential credential;



            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "574082988944-0f03tni3j5jjlles0prjjbdkvhog7qe4.apps.googleusercontent.com",
                    ClientSecret = "sEcShK1KM2gvvUE7BfR6jq4g",

                },
                Scopes,
                "user",
                CancellationToken.None).Result;
            //Console.WriteLine("Credential file saved to: " + credPath);

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
            Console.Read();
        }

    }
}