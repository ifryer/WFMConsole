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
    
    public partial class Agent
    {
        public int AgentNo { get; set; }
        public string Password { get; set; }
        public int TeamNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public int SecurityProfileID { get; set; }
        public string TeamName { get; set; }
        public System.DateTime LastLogin { get; set; }
        public System.DateTime LastModified { get; set; }
        public string CurrentState { get; set; }
        public int CurrentStationID { get; set; }
        public int CurrentSkillNo { get; set; }
        public string CurrentSkillName { get; set; }
        public Nullable<bool> IsSupervisor { get; set; }
        public Nullable<int> ReportTo { get; set; }
        public Nullable<int> NiceId { get; set; }
    }
}
