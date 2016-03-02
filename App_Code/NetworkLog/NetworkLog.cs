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
        object[] obj = new object[3];
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            try {
                obj[0] = nab.BuildRequests;
            }
            catch (Exception e) {
                obj[0] = new object[0];
            }

            try {
                obj[1] = nab.BuildAppLog;
            }
            catch (Exception e) {
                obj[1] = string.Empty;
            }

            try {
                obj[2] = Buildeventlist;
            }
            catch (Exception e) {
                obj[2] = string.Empty;
            }

        }
        return obj;
    }

    [WebMethod]
    public string GetNetworkActivity()
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            eventListFontSize = "12px";
            return Buildeventlist;
        }
        return string.Empty;
    }

    private string eventListFontSize = "11px";
    private string Buildeventlist
    {
        get {
            MembershipUserCollection coll = Membership.GetAllUsers();
            var applog = new AppLog(true);
            var str = new StringBuilder();
            int locked = coll.Cast<MembershipUser>().Count(u => !u.IsApproved);
            var str2 = new StringBuilder();
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
                str2.Append("<div style='font-size: " + eventListFontSize + ";'>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Total Groups</span>" + groups.group_dt.Count.ToString() + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Total Users</span>" + (coll.Count - 1).ToString() + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Users Online</span>" + totalonline.ToString() + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Users Locked</span>" + locked.ToString() + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>IP's Being Watched</span>" + _ipwatch.ipwatchdt.Count + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>IP's Blocked</span>" + ServerSettings.getTotalBlockedIP + "<div class='clear-space-two'></div>");

                string sslRedirect = "Off";
                if (_ss.SSLRedirect)
                    sslRedirect = "On";

                string emailSystem = "Off";
                if (_ss.EmailSystemStatus)
                    emailSystem = "On";

                string ipaddress = HttpContext.Current.Request.UserHostAddress;
                if (ipaddress == "::1")
                    ipaddress = "127.0.0.1";

                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Email System</span>" + emailSystem + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>SSL Redirect</span>" + sslRedirect + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>IP Listener Status</span>" + ServerSettings.GetIpListener + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Current IP</span>" + ipaddress + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Site Uptime (D:H:M)</span>" + HelperMethods.UpTime.Days + ":" + HelperMethods.UpTime.Hours + ":" + HelperMethods.UpTime.Minutes + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Memory Allocated</span>" + HelperMethods.FormatBytes(GC.GetTotalMemory(true)) + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Total Events</span>" + applog.app_coll.Count.ToString() + "<div class='clear-space-two'></div>");
                str2.Append("<span class='settings-name-column float-left' style='padding-top: 0px!important;'>Todays Hit Total</span>" + GetSiteRequests.HitCountTotal + "</div>");
            }
            catch (Exception e) { AppLog.AddError(e); }

            return str2.ToString() + str.ToString();
        }
    }
}
