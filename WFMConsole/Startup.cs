using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WFMDashboard.Startup))]

namespace WFMDashboard
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<KonicaMinolta.Authentication.Authentication>();
        }
    }
}