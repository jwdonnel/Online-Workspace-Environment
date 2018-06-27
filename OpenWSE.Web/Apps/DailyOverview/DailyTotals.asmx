<%@ WebService Language="C#" Class="DailyTotals" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class DailyTotals  : System.Web.Services.WebService 
{

    [WebMethod]
    public string GetTotals() 
    {
        int totalweight = 0;
        var ts = new TruckSchedule();
        var str = new System.Text.StringBuilder();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            var gd = new GeneralDirection(true);
            foreach (System.Collections.Generic.Dictionary<string, string> row in gd.generaldirection) {
                str.Append("<b class='pad-right'>" + row["GeneralDirection"].ToString() + ":</b>" + (ts.calTotalWeightGD(ServerSettings.ServerDateTime.ToShortDateString(), row["GeneralDirection"].ToString()).ToString("#,##0")) + " lbs<br />");
                totalweight += Convert.ToInt32(ts.calTotalWeightGD(ServerSettings.ServerDateTime.ToShortDateString(), row["GeneralDirection"].ToString()));
            }
            str.Append("<div class='clear' style='width: 75%; margin: 8px 0; border-top: 1px solid #B7B7B7;'></div><b class='pad-right'>Total:</b>" + totalweight.ToString("#,##0") + " lbs");
            str.Append("<div class='clear-space-two'></div><small>(<i>SMW Trucks + Common Carriers</i>)</small>");
        }
        return str.ToString();
    }
    
}