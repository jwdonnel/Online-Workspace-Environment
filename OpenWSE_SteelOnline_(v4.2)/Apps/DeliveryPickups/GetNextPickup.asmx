<%@ WebService Language="C#" Class="GetNextPickup" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data;
using System.Linq;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class GetNextPickup  : System.Web.Services.WebService 
{

    [WebMethod]
    public string GetNext()
    {
        var str = new System.Text.StringBuilder();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            var schedule = new Scheduler();
            DateTime currTime = DateTime.Now;
            var dates = (from t in schedule.scheduler_coll
                         let final = currTime.Subtract(Convert.ToDateTime(t.ScheduleDate))
                         where (final.Ticks < 0) && (!t.Complete)
                         select Convert.ToDateTime(t.ScheduleDate)).ToList();
            if (dates.Count > 0) {
                try {
                    dates.Sort((x, y) => x.Ticks.CompareTo(y.Ticks));
                }
                catch {
                    dates.Sort();
                }
                var timeslot = new Scheduler();
                if (timeslot.scheduler_coll.Count > 0) {
                    try {
                        timeslot.scheduler_coll.Sort((x, y) => Convert.ToDateTime(y.ScheduleDate).Ticks.CompareTo(Convert.ToDateTime(x.ScheduleDate).Ticks));
                        str.Append("<span class='PadRight'><b>Company:</b></span><span>" +
                                   timeslot.scheduler_coll[0].Company + "</span><div class='clear-space-five'></div>");
                        str.Append("<span class='PadRight'><b>Truck #:</b></span><span>" +
                                   timeslot.scheduler_coll[0].TruckNum + "</span><div class='clear-space-five'></div>");
                        str.Append("<span class='PadRight'><b>Del. From:</b></span><span>" +
                                   timeslot.scheduler_coll[0].DeliveryFrom + "</span><div class='clear-space-five'></div>");
                        str.Append("<span class='PadRight'><b>Type:</b></span><span>" +
                                   timeslot.scheduler_coll[0].ScheduleType + "</span><div class='clear-space-five'></div>");
                        str.Append("<span class='PadRight'><b>Date:</b></span><span>" +
                                   timeslot.scheduler_coll[0].ScheduleDate + "</span>");
                    }
                    catch {
                        str.Append("Error Sorting Schedule! Please Try Again.");
                    }
                }
                else
                    str.Append("No upcoming deliveries or pickups.");
            }
            else
                str.Append("No upcoming deliveries or pickups.");
        }
        return str.ToString();
    }
    
}