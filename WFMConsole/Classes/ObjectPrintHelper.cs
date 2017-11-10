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

        public static string PrintEvent(BUS_WFMDashboard_Event eventItem)
        {
            StringBuilder outputString = new StringBuilder("");

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
            return outputString.ToString();
        }
    }
}