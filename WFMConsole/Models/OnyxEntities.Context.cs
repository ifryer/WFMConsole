﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class OnyxEntities : DbContext
    {
        public OnyxEntities()
            : base("name=OnyxEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BUS_WFMDashboard_Google_Credentials> BUS_WFMDashboard_Google_Credentials { get; set; }
        public virtual DbSet<BUS_WFMDashboard_ICM> BUS_WFMDashboard_ICM { get; set; }
        public virtual DbSet<BUS_WFMDashboard_ICM_Order> BUS_WFMDashboard_ICM_Order { get; set; }
        public virtual DbSet<BUS_WFMDashboard_ReportLog> BUS_WFMDashboard_ReportLog { get; set; }
        public virtual DbSet<BUS_WFMDashboard_WFO_Schedule> BUS_WFMDashboard_WFO_Schedule { get; set; }
        public virtual DbSet<BUS_WFMDashboard_LateShiftManager> BUS_WFMDashboard_LateShiftManager { get; set; }
        public virtual DbSet<BUS_WFMDashboard_Action_History> BUS_WFMDashboard_Action_History { get; set; }
        public virtual DbSet<BUS_WFMDashboard_Event> BUS_WFMDashboard_Event { get; set; }
        public virtual DbSet<BUS_WFMDashboard_Repeating_Event> BUS_WFMDashboard_Repeating_Event { get; set; }
        public virtual DbSet<BUS_WFMDashboard_Event_Notification> BUS_WFMDashboard_Event_Notification { get; set; }
        public virtual DbSet<BUS_WFMDashboard_Event_Invitee> BUS_WFMDashboard_Event_Invitee { get; set; }
    }
}
