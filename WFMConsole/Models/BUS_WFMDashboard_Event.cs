//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WFMConsole.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BUS_WFMDashboard_Event
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public string CalendarEventId { get; set; }
        public int AgentNo { get; set; }
        public int TeamId { get; set; }
        public string Description { get; set; }
        public string TeamName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public bool FullDay { get; set; }
        public string Notes { get; set; }
        public string Color { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
