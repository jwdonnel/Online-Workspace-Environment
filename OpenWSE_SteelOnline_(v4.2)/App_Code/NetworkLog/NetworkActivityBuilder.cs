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
    private static readonly List<double> uploadSpeedList = new List<double>();
    private static readonly List<double> downloadSpeedList = new List<double>();
    private static readonly List<float> cpu = new List<float>();
    private static readonly List<float> siterequests = new List<float>();
    private static float AverageRequests_request;
    private static float CurrentRequests_request;
    private static double AverageRequests_chart2;
    private static double CurrentRequests_chart2;
    private static double AverageRequests_chart3;
    private static double CurrentRequests_chart3;
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
            sb.Append("<div class='averagerequests'>");
            sb.Append("<table cellpadding='15' cellspacing='0' style='width: 175px;'><tbody>");
            sb.Append("<tr><td><h2 class='float-left font-bold pad-right'>Current:</h2><h2 class='float-left'>" + current + "</h2></td></tr>");
            sb.Append("<tr><td><div class='clear' style='height: 40px;'></div><h2 class='float-left font-bold pad-right'>Average:</h2><h2 class='float-left'>" + average + "</h2></td></tr>");
            sb.Append("<tr><td><div class='clear' style='height: 40px;'></div><h2 class='float-left font-bold pad-right'>Maximum:</h2><h2 class='float-left'>" + maximum + "</h2></td></tr>");
            sb.Append("</tbody></table>");
            sb.Append("</div>");

            if (!GetSiteRequests.GetRequests)
                sb.Append("<div class='clear'></div><div style='color: #FF0000'>Site requests activity off</div>");

            return sb.ToString();
        }
    }

    public string BuildChartLog2 {
        get {
            StringBuilder sb = new StringBuilder();
            double current = Math.Round(CurrentRequests_chart2, 2);
            double average = Math.Round(AverageRequests_chart2, 2);
            double maximum = Math.Round(FindMax_Speed(uploadSpeedList), 2);
            sb.Append("<div class='averagerequests'>");
            sb.Append("<table cellpadding='15' cellspacing='0' style='width: 175px;'><tbody>");
            sb.Append("<tr><td><h2 class='float-left font-bold pad-right'>Current:</h2><h2 class='float-left'>" + current + "</h2></td></tr>");
            sb.Append("<tr><td><div class='clear' style='height: 40px;'></div><h2 class='float-left font-bold pad-right'>Average:</h2><h2 class='float-left'>" + average + "</h2></td></tr>");
            sb.Append("<tr><td><div class='clear' style='height: 40px;'></div><h2 class='float-left font-bold pad-right'>Maximum:</h2><h2 class='float-left'>" + maximum + "</h2></td></tr>");
            sb.Append("</tbody></table>");
            sb.Append("</div>");

            return sb.ToString();
        }
    }

    public string BuildChartLog3 {
        get {
            StringBuilder sb = new StringBuilder();
            double current = Math.Round(CurrentRequests_chart3, 2);
            double average = Math.Round(AverageRequests_chart3, 2);
            double maximum = Math.Round(FindMax_Speed(downloadSpeedList), 2);
            sb.Append("<div class='averagerequests'>");
            sb.Append("<table cellpadding='15' cellspacing='0' style='width: 175px;'><tbody>");
            sb.Append("<tr><td><h2 class='float-left font-bold pad-right'>Current:</h2><h2 class='float-left'>" + current + "</h2></td></tr>");
            sb.Append("<tr><td><div class='clear' style='height: 40px;'></div><h2 class='float-left font-bold pad-right'>Average:</h2><h2 class='float-left'>" + average + "</h2></td></tr>");
            sb.Append("<tr><td><div class='clear' style='height: 40px;'></div><h2 class='float-left font-bold pad-right'>Maximum:</h2><h2 class='float-left'>" + maximum + "</h2></td></tr>");
            sb.Append("</tbody></table>");
            sb.Append("</div>");

            return sb.ToString();
        }
    }

    public object[] BuildRequests {
        get {
            List<object> series = new List<object>();
            try {
                float count = 0.0F;
                foreach (double s in from t in GetSiteRequests.SiteRequests let targetDt = DateTime.Now let date1 = t select targetDt.Subtract(date1).TotalSeconds into s where s < 2 select s) {
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
            catch { }
            return series.ToArray();
        }
    }

    public object[] BuildChart2 {
        get {
            List<object> series1 = new List<object>();
            try {
                uploadSpeed = CalculateUploadSpeed();
                uploadSpeedList.Add(uploadSpeed);
                AverageRequests_chart2 = 0.0d;
                CurrentRequests_chart2 = 0.0d;
                for (int i = 0; i < uploadSpeedList.Count; i++) {
                    if (i <= dataInt) {
                        series1.Add(uploadSpeedList[i]);
                        AverageRequests_chart2 += uploadSpeedList[i];
                        CurrentRequests_chart2 = uploadSpeedList[i];
                    }
                    else {
                        uploadSpeedList.RemoveAt(0);
                        break;
                    }
                }

                AverageRequests_chart2 = (AverageRequests_chart2 / siterequests.Count);
            }
            catch { }

            return series1.ToArray();
        }
    }

    public object[] BuildChart3 {
        get {
            List<object> series1 = new List<object>();
            try {
                downloadSpeed = CalculateDownloadSpeed();
                downloadSpeedList.Add(downloadSpeed);
                AverageRequests_chart3 = 0.0d;
                CurrentRequests_chart3 = 0.0d;
                for (int i = 0; i < downloadSpeedList.Count; i++) {
                    if (i <= dataInt) {
                        series1.Add(downloadSpeedList[i]);
                        AverageRequests_chart3 += downloadSpeedList[i];
                        CurrentRequests_chart3 = downloadSpeedList[i];
                    }
                    else {
                        downloadSpeedList.RemoveAt(0);
                        break;
                    }
                }

                AverageRequests_chart3 = (AverageRequests_chart3 / siterequests.Count);
            }
            catch { }

            return series1.ToArray();
        }
    }

    public IpCityState[] BuildMap {
        get {
            List<IpCityState> countryList = new List<IpCityState>();
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser user in coll) {
                if (user.IsOnline) {
                    MemberDatabase member = new MemberDatabase(user.UserName);
                    string ip = member.IpAddress;
                    IpCityState content = HelperMethods.GetCityStateFromIP(ip);
                    if (content != null && !ContainsIp(content, countryList))
                        countryList.Add(content);
                }
            }

            return countryList.ToArray();
        }
    }
    private bool ContainsIp(IpCityState content, List<IpCityState> list) {
        foreach (IpCityState val in list) {
            if (val.IpAddress == content.IpAddress && val.Country == content.Country && val.City == content.City) {
                return true;
            }
        }

        return false;
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
        catch {
            return 0.0f;
        }
    }
    private static double FindMax_Speed(IEnumerable<double> list) {
        try {
            double returnVal = list.Concat(new[] { double.MinValue }).Max();
            if (returnVal < 0)
                return 0.0d;
            else
                return returnVal;
        }
        catch {
            return 0.0d;
        }
    }
    private static void BuildActivity_ForOverlay() {
        // Populate new series with data 
        float count = 0.0F;
        foreach (double s in from t in GetSiteRequests.SiteRequests let targetDt = DateTime.Now let date1 = t select targetDt.Subtract(date1).TotalSeconds into s where s < 2 select s) {
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
    private double CalculateUploadSpeed() {
        if (networkInterface == null) {
            foreach (NetworkInterface currentNetworkInterface in NetworkInterface.GetAllNetworkInterfaces()) {
                if (currentNetworkInterface.OperationalStatus == OperationalStatus.Up) {
                    networkInterface = currentNetworkInterface;
                    break;
                }
            }
        }

        IPv4InterfaceStatistics interfaceStatistic = networkInterface.GetIPv4Statistics();

        if (lngBytesSend == 0)
            lngBytesSend = interfaceStatistic.BytesSent;

        double bytesSentSpeed = ((interfaceStatistic.BytesSent - lngBytesSend) / 1024);
        lngBytesSend = interfaceStatistic.BytesSent;

        return bytesSentSpeed;
    }
    private double CalculateDownloadSpeed() {
        if (networkInterface == null) {
            foreach (NetworkInterface currentNetworkInterface in NetworkInterface.GetAllNetworkInterfaces()) {
                if (currentNetworkInterface.OperationalStatus == OperationalStatus.Up) {
                    networkInterface = currentNetworkInterface;
                    break;
                }
            }
        }

        IPv4InterfaceStatistics interfaceStatistic = networkInterface.GetIPv4Statistics();

        if (lngBtyesReceived == 0)
            lngBtyesReceived = interfaceStatistic.BytesReceived;

        double bytesReceivedSpeed = ((interfaceStatistic.BytesReceived - lngBtyesReceived) / 1024);
        lngBtyesReceived = interfaceStatistic.BytesReceived;

        return bytesReceivedSpeed;
    }

    #endregion
}