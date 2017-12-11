using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WFMConsole.ViewModels
{
    //This item is for getting the info from the create/edit form for events.
    public class EventForm
    {
        public int eventId { get; set; }
        public int agentId { get; set; }
        public string title { get; set; } 
        public string color { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public bool fullDay { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string notes { get; set; }
        public string eventType { get; set; }
        public string repeatType { get; set; }
        public int repeatEveryNumber { get; set; }
        public string repeatOnDays { get; set; }
        public string repeatEndType { get; set; }
        public string repeatEndDate { get; set; }
        public int repeatEndAfterNumber { get; set; }
        public bool repeatingEvent { get; set; }
        public string repeatSummary { get; set; }
        public ViewNotificationItem [] notifications { get; set; }
        public bool notificationsPresent { get; set; }
        public ViewInviteeItem [] invitees { get; set; }
        public bool hasInvitees { get; set; }

        public override string ToString()
        {
            return $"\r\n eventId: {eventId} /r/n agentId: {agentId} \r\n title: {title} \r\n color: {color} \r\n start date: {startDate} \r\n end date: {endDate} \r\n fullDay: {fullDay} \r\n startTime: {startTime} \r\n endTime: {endTime} \r\n notes: {notes} \r\n eventType: {eventType} \r\n repeatType: {repeatType} \r\n repeatEveryNumber: {repeatEveryNumber} \r\n repeatOnDays: {repeatOnDays} \r\n repeatEndType: {repeatEndType} \r\n repeatEnddate: {repeatEndDate} \r\n repeatEndAfterNumber: {repeatEndAfterNumber} \r\n repeatingEvent: {repeatingEvent}";
        }
    }
}