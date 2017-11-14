using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WFMConsole.Models;

namespace WFMConsole.Classes
{
    public class ObjectPrintHelper
    {
        public static string PrintEvent(BUS_WFMDashboard_Event eventItem, BUS_WFMDashboard_Repeating_Event repeatingItem)
        {
            StringBuilder outputString = new StringBuilder("Event:");
            outputString.AppendLine("AgentNo: " + eventItem.AgentNo);
            outputString.AppendLine("CalendarEventId: " + eventItem.CalendarEventId);
            outputString.AppendLine("Color: " + eventItem.Color);
            outputString.AppendLine("CreatedBy: " + eventItem.CreatedBy);
            outputString.AppendLine("CreatedAt: " + eventItem.CreatedAt);
            outputString.AppendLine("Description: " + eventItem.Description);
            outputString.AppendLine("StartTime: " + eventItem.StartTime.ToString("MM/dd/YYYY HH:mm"));
            outputString.AppendLine("EndTime: " + eventItem.EndTime.ToString("MM/dd/YYYY HH:mm"));
            outputString.AppendLine("EventType: " + eventItem.EventType);
            outputString.AppendLine("FirstName: " + eventItem.FirstName);
            outputString.AppendLine("LastName: " + eventItem.LastName);
            outputString.AppendLine("FullDay: " + eventItem.FullDay);
            outputString.AppendLine("Notes: " + eventItem.Notes);
            outputString.AppendLine("TeamName: " + eventItem.TeamName);
            outputString.AppendLine("TeamId: " + eventItem.TeamId);
            if(repeatingItem != null)
            {
                outputString.AppendLine("Repeating Event: ");
                outputString.AppendLine("Repeat Type: " + repeatingItem.RepeatType);
                outputString.AppendLine("Repeat Every Number: " + repeatingItem.RepeatEveryNumber);
                outputString.AppendLine("Repeat On Days: " + repeatingItem.RepeatOnDays);
                outputString.AppendLine("Start Date: " + repeatingItem.StartDate.ToShortDateString());
                outputString.AppendLine("End After Occurences: " + repeatingItem.EndAfterOccurences);
                if(repeatingItem.EndDate != null)
                    outputString.AppendLine("End Date: " + repeatingItem.EndDate.Value.ToShortDateString());
                outputString.AppendLine("Repeat Summary: " + repeatingItem.RepeatSummary);
                outputString.AppendLine("End Type: " + repeatingItem.EndType);
                if(repeatingItem.CalculatedEndDate != null)
                    outputString.AppendLine("Calculated End Date: " + repeatingItem.CalculatedEndDate.Value.ToShortDateString());
            }
            return outputString.ToString();
        }
    }
}