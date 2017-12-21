using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WFMConsole.Models;

namespace WFMConsole.ViewModels
{
    public class ViewLateShiftSchedule
    {
        public int Id { get; set; }
        public int AgentNo { get; set; }
        public string ManagerName { get; set; }
        public DateTime Date { get; set; }
    }


    public class ViewIcmSchedule
    {
        public int AgentNo { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string ManagerName { get; set; }
        public ViewIcmSchedule() { }
    }

    public class ViewIcmLatest
    {
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public int CurrentManager { get; set; }
        public List<ViewManagerListItem> ManagerOrder { get; set; }

        public ViewIcmLatest(ViewIcmSchedule lastSchedule)
        {
            if (lastSchedule.Month == 12)
            {
                CurrentMonth = 1;
                CurrentYear = lastSchedule.Year + 1;
            }
            else
            {
                CurrentMonth = lastSchedule.Month + 1;
                CurrentYear = lastSchedule.Year;
            }
            using(var db = new OnyxEntities())
            {
                ManagerOrder = db.BUS_WFMDashboard_ICM_Order.OrderBy(t => t.Order).Select(t => new ViewManagerListItem() {AgentNo = t.AgentNo, Name = t.ManagerName }).ToList();
            }

            var indexOfLastManager = ManagerOrder.IndexOf(ManagerOrder.Where(t => t.AgentNo == lastSchedule.AgentNo).FirstOrDefault());
            if((indexOfLastManager == ManagerOrder.Count - 1) || indexOfLastManager < 0)
            {
                CurrentManager = ManagerOrder[0].AgentNo;
            }
            else
            {
                CurrentManager = ManagerOrder[indexOfLastManager + 1].AgentNo;
            }
        }
    }

    public class ViewManagerListItem
    {
        public string Name { get; set; }
        public int AgentNo { get; set; }
    }

    public class ViewMowSchedule
    {
        public int Id { get; set; }
        public int AgentNo { get; set; }
        public string Task { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime Date { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ViewMowSchedule() { }

    }

    public class MowFormInput
    {
        public List<MowFormInputItem> InputItems { get; set; }
        public string Date { get; set; }

        override public string ToString()
        {
            var to_s = $"Date: {Date} \r\n InputItems: \r\n";
            foreach (var item in InputItems)
            {
                to_s += $"{item.ToString()} \r\n";
            }
            return to_s;
        }
    }
    public class MowFormInputItem
    {
        public string shiftStart { get; set; }
        public string shiftEnd { get; set; }
        public string task { get; set; }
        public int agentNo { get; set; }
        public int rowId { get; set; }
        override public string ToString()
        {
            var to_s = $" shiftStart: {shiftStart}  -  shiftEnd: {shiftEnd}  -  task: {task}  -  agentNo: {agentNo}  -  rowId: {rowId}";

            return to_s;
        }
    }
}