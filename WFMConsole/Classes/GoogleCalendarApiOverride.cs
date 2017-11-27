using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualBasic;
using System.Collections;
using System.Data;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WFMConsole.Classes
{
    public class GoogleCalendarApiOverride
    {
    }

    

    public class CTMEventDateTime : global::Google.Apis.Calendar.v3.Data.EventDateTime
    {

        [JsonProperty("date", NullValueHandling = NullValueHandling.Include)]
        public override string Date
        {
            get { return base.Date; }
            set { base.Date = value; }
        }
        [JsonProperty("dateTime", NullValueHandling = NullValueHandling.Include)]
        public override string DateTimeRaw
        {
            get { return base.DateTimeRaw; }
            set { base.DateTimeRaw = value; }
        }
    }

}