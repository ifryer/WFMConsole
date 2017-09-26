using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WFMDashboard.Classes;

namespace WFMConsole.Controllers
{
    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        protected override Google.Apis.Auth.OAuth2.Mvc.FlowMetadata FlowData
        {
            get { return new AppFlowMetadata(); }
        }

        //public async Task<ActionResult> IndexAsync(CancellationToken cancellationToken)
        //{
        //    var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);

        //    if (result.Credential != null)
        //    {
        //        var service = new CalendarService(new BaseClientService.Initializer
        //        {
        //            HttpClientInitializer = result.Credential,
        //            ApplicationName = "ASP.NET MVC Sample"
        //        });


        //        // YOUR CODE SHOULD BE HERE..
        //        // SAMPLE CODE:
        //        //var list = await service.Files.List().ExecuteAsync();
        //        //ViewBag.Message = "FILE COUNT IS: " + list.Items.Count();

        //        //TODO: redirect to index page, or just return a JSON????
        //        return View();
        //    }
        //    else
        //    {
        //        return new RedirectResult(result.RedirectUri);
        //    }
        //}
    }
}