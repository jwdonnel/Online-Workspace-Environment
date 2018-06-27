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
    private static readonly List<float> cpu = new List<float>();
    private static readonly List<float> siterequests = new List<float>();
    public static float AverageRequests_request;
    public static float CurrentRequests_request;
    private readonly HttpContext Context;
    private readonly HttpResponse Response;
    private HttpRequest Request;
    public object _sync = new object();

    #endregion

    public NetworkActivityBuilder() { }

    public NetworkActivityBuilder(HttpContext _context, HttpRequest _request, HttpResponse _response) {
        Context = _context;
        Request = _request;
        Response = _response;
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
            catch { 
            }
            return series.ToArray();
        }
    }

    public static float FindMax() {
        try {
            float returnVal = siterequests.Concat(new[] { float.MinValue }).Max();
            if (returnVal < 0)
                return 0.0f;
            else
                return returnVal;
        }
        catch {
            return 0.0f;
        }
    }
}