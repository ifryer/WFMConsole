using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WFMConsole.Models;

namespace WFMConsole.ViewModels
{
    public class ViewAgent
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int AgentNo { get; set; } 
        public string TeamName { get; set; }

        public ViewAgent(Agent agent)
        {
            LastName = agent.LastName;
            FirstName = agent.FirstName;
            AgentNo = agent.AgentNo;
            TeamName = agent.TeamName.Replace("Team", "").Replace(" ", "");
        }
    }
}