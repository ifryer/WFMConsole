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
    
    public partial class BUS_WFMDashboard_Action_History
    {
        public int Id { get; set; }
        public Nullable<int> EventId { get; set; }
        public Nullable<int> ScheduleId { get; set; }
        public string ScheduleType { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string Before { get; set; }
        public string After { get; set; }
        public string PerformedBy { get; set; }
        public System.DateTime PerformedAt { get; set; }
    }
}