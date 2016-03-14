using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Security;
using System.Text;
using System.Web.Services.Protocols;
using System.Security.Principal;
using OpenWSE_Tools.GroupOrganizer;

/// <summary>
/// Summary description for NetworkLog
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class NetworkLog : System.Web.Services.WebService {

    private NetworkActivityBuilder nab;
    private ServerSettings _ss = new ServerSettings();
    private readonly IPWatch _ipwatch = new IPWatch(true);
    
    public NetworkLog()
    {
        if (Context != null && Context.Request != null && Context.Request.Url != null && !Context.Request.Url.OriginalString.ToLower().Contains("getgraphs")) {
            GetSiteRequests.AddRequest();
        }

        IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated)
        {
            HttpRequest Request = Context.Request;
            HttpResponse Response = Context.Response;
            nab = new NetworkActivityBuilder(Context, Request, Response);
        }
    }

    [WebMethod]
    public object[] GetGraphs(string _dataInt) {
        int outInt = 20;
        int.TryParse(_dataInt, out outInt);
        nab.dataInt = outInt;
        object[] obj = new object[2];
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            try {
                obj[0] = nab.BuildRequests;
            }
            catch (Exception e) {
                obj[0] = new object[0];
            }

            try {
                obj[1] = Buildeventlist;
            }
            catch (Exception e) {
                obj[1] = string.Empty;
            }

        }
        return obj;
    }

    [WebMethod]
    public string GetNetworkActivity()
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            return Buildeventlist;
        }
        return string.Empty;
    }

    [WebMethod]
    public object[] GetPageDetails(string pageName) {
        List<object> returnObj = new List<object>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                PageViews pageViews = new PageViews();
                List<PageViews_Coll> coll = pageViews.GetPageViewDetails(pageName);
                foreach (PageViews_Coll item in coll) {
                    List<object> obj = new List<object>();
                    obj.Add(item.ID);
                    obj.Add(item.IPAddress);
                    obj.Add(item.Username);
                    obj.Add(item.DateAdded);

                    returnObj.Add(obj.ToArray());
                }
            }
        }

        return returnObj.ToArray();
    }

    [WebMethod]
    public string DeletePageDetailsItem(string id) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) {
                PageViews pageViews = new PageViews();
                pageViews.DeleteItemByID(id);
                return "true";
            }
        }

        return "false";
    }

    private string Buildeventlist
    {
        get {
            MembershipUserCollection coll = Membership.GetAllUsers();
            var applog = new AppLog(true);
            int locked = coll.Cast<MembershipUser>().Count(u => !u.IsApproved);
            var str = new StringBuilder();
            try {
                string _username = HttpContext.Current.User.Identity.Name;
                var groups = new Groups(_username);
                var dbimports = new DBImporter();
                dbimports.GetImportList();
                groups.getEntries();
                int totalonline = Membership.GetNumberOfUsersOnline();
                var membershipUser = Membership.GetUser(ServerSettings.AdminUserName);
                if (membershipUser != null && membershipUser.IsOnline) {
                    totalonline = totalonline - 1;
                }

                string sslRedirect = "Off";
                if (_ss.SSLRedirect) {
                    sslRedirect = "On";
                }

                string emailSystem = "Off";
                if (_ss.EmailSystemStatus) {
                    emailSystem = "On";
                }

                string ipaddress = HttpContext.Current.Request.UserHostAddress;
                if (ipaddress == "::1") {
                    ipaddress = "127.0.0.1";
                }

                str.Append("<div class='network-info-holder'>");
                str.Append("<table class='network-info-tables'><tbody>");
                str.Append("<tr class='myHeaderStyle-network-tables'><td colspan='2'>Network Stats</td></tr>");
                str.Append("<tr><td class='network-info-title'>SSL Redirect</td><td class='network-info-value'>" + sslRedirect + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>IP Listener</td><td class='network-info-value'>" + ServerSettings.GetIpListener + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>Current IP</td><td class='network-info-value'>" + ipaddress + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>IP WatchList Count</td><td class='network-info-value'>" + _ipwatch.ipwatchdt.Count + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>Email System</td><td class='network-info-value'>" + emailSystem + "</td></tr>");
                double current = Math.Round(NetworkActivityBuilder.CurrentRequests_request, 2);
                double average = Math.Round(NetworkActivityBuilder.AverageRequests_request, 2);
                double maximum = Math.Round(NetworkActivityBuilder.FindMax(), 2);
                str.Append("<tr><td class='network-info-title'>Current Request(s)</td><td class='network-info-value'>" + current + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>Average Request(s)</td><td class='network-info-value'>" + average + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>Maximum Request(s)</td><td class='network-info-value'>" + maximum + "</td></tr>");
                str.Append("</tbody></table>");
                str.Append("</div>");

                str.Append("<div class='network-info-holder'>");
                str.Append("<table class='network-info-tables'><tbody>");
                str.Append("<tr class='myHeaderStyle-network-tables'><td colspan='2'>Server Stats</td></tr>");
                str.Append("<tr><td class='network-info-title'>Total Groups</td><td class='network-info-value'>" + groups.group_dt.Count.ToString() + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>Total Users</td><td class='network-info-value'>" + (coll.Count - 1).ToString() + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>Users Online</td><td class='network-info-value'>" + totalonline.ToString() + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>Users Locked</td><td class='network-info-value'>" + locked.ToString() + "</td></tr>");
                string upTimeStr = HelperMethods.UpTime.Days + ":" + HelperMethods.UpTime.Hours + ":" + HelperMethods.UpTime.Minutes;
                if (upTimeStr != "0:0:0") {
                    str.Append("<tr><td class='network-info-title'>Site Uptime (D:H:M)</td><td class='network-info-value'>" + upTimeStr + "</td></tr>");
                }
                str.Append("<tr><td class='network-info-title'>Memory Allocated</td><td class='network-info-value'>" + HelperMethods.FormatBytes(GC.GetTotalMemory(true)) + "</td></tr>");
                str.Append("<tr><td class='network-info-title'>Total Events</td><td class='network-info-value'>" + applog.app_coll.Count.ToString() + "</td></tr>");
                str.Append("</tbody></table>");
                str.Append("</div>");

                str.Append("<div class='clear'></div>");
            }
            catch (Exception e) { AppLog.AddError(e); }

            return str.ToString();
        }
    }
}
