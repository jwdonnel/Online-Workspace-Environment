#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.Security;
using System.Net;
using System.Xml.Linq;
using System.Net.NetworkInformation;

#endregion


/// <summary>
///     Summary description for NetworkActivity
/// </summary>
public class NetworkActivityBuilder {
    #region Variables

    public int dataInt = 20;

    public static bool _inUse = false;
    private const int numKB = 512;
    private static NetworkInterface networkInterface;
    private static readonly List<float> cpu = new List<float>();
    private static readonly List<float> siterequests = new List<float>();
    private static float AverageRequests_request;
    private static float CurrentRequests_request;
    private static long lngBytesSend = 0;
    private static long lngBtyesReceived = 0;
    private readonly HttpContext Context;
    private readonly HttpResponse Response;
    private HttpRequest Request;
    public object _sync = new object();
    private double uploadSpeed;
    private double downloadSpeed;

    #endregion

    public NetworkActivityBuilder() { }

    public NetworkActivityBuilder(HttpContext _context, HttpRequest _request, HttpResponse _response) {
        Context = _context;
        Request = _request;
        Response = _response;
    }

    public string BuildAppLog {
        get {
            StringBuilder sb = new StringBuilder();
            double current = Math.Round(CurrentRequests_request, 2);
            double average = Math.Round(AverageRequests_request, 2);
            double maximum = Math.Round(FindMax(siterequests), 2);
            sb.Append("<div class='averagerequests' align='center'>");
            sb.Append("<table cellpadding='15' cellspacing='0' style='width: 700px;'><tbody>");
            sb.Append("<tr><td><h2 class='float-left font-bold pad-right'>Current:</h2><h2 class='float-left'>" + current + "</h2></td>");
            sb.Append("<td><h2 class='float-left font-bold pad-right'>Average:</h2><h2 class='float-left'>" + average + "</h2></td>");
            sb.Append("<td><h2 class='float-left font-bold pad-right'>Maximum:</h2><h2 class='float-left'>" + maximum + "</h2></td></tr>");
            sb.Append("</tbody></table>");
            sb.Append("</div>");

            if (!GetSiteRequests.GetRequests)
                sb.Append("<div class='clear'></div><div style='color: #FF0000'>Site requests activity off</div>");

            return sb.ToString();
        }
    }

    public object[] BuildRequests {
        get {
            List<object> series = new List<object>();
            try {
                float count = 0.0F;
                foreach (double s in from t in GetSiteRequests.SiteRequests let targetDt = ServerSettings.ServerDateTime let date1 = t select targetDt.Subtract(date1).TotalSeconds into s where s < 2 select s) {
                    count++;
                }

                siterequests.Add(count);
                AverageRequests_request = 0.0F;
                CurrentRequests_request = 0.0F;
                for (int i = 0; i < siterequests.Count; i++) {
                    if (i <= dataInt) {
                        series.Add(siterequests[i]);
                        AverageRequests_request += siterequests[i];
                        CurrentRequests_request = siterequests[i];
                    }
                    else {
                        siterequests.RemoveAt(0);
                        break;
                    }
                }

                AverageRequests_request = (AverageRequests_request / siterequests.Count);
            }
            catch (Exception e) { 
                AppLog.AddError(e); 
            }
            return series.ToArray();
        }
    }

    #region Private Methods

    private static float FindMax(IEnumerable<float> list) {
        try {
            float returnVal = list.Concat(new[] { float.MinValue }).Max();
            if (returnVal < 0)
                return 0.0f;
            else
                return returnVal;
        }
        catch (Exception e) {
            AppLog.AddError(e); 
            return 0.0f;
        }
    }
    private static void BuildActivity_ForOverlay() {
        // Populate new series with data 
        float count = 0.0F;
        foreach (double s in from t in GetSiteRequests.SiteRequests let targetDt = ServerSettings.ServerDateTime let date1 = t select targetDt.Subtract(date1).TotalSeconds into s where s < 2 select s) {
            count++;
        }
        siterequests.Add(count);
        AverageRequests_request = 0.0F;
        CurrentRequests_request = 0.0F;
        for (int i = 0; i < siterequests.Count; i++) {
            if (i <= 30) {
                AverageRequests_request += siterequests[i];
                CurrentRequests_request = siterequests[i];
            }
            else {
                siterequests.RemoveAt(0);
                break;
            }
        }

        AverageRequests_request = (AverageRequests_request / 60.0F);
    }

    #endregion
}