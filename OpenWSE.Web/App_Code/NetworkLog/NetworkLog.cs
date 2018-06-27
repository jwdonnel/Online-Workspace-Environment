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
using System.Web.Configuration;

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
        object[] obj = new object[2];

        try {
            int outInt = 20;
            int.TryParse(_dataInt, out outInt);
            nab.dataInt = outInt;
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                try {
                    obj[0] = nab.BuildRequests;
                }
                catch {
                    obj[0] = new object[0];
                }

                try {
                    obj[1] = Buildeventlist;
                }
                catch {
                    obj[1] = string.Empty;
                }

            }
        }
        catch { }

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

                    DateTime outTime = new DateTime();
                    if (DateTime.TryParse(item.DateAdded, out outTime)) {
                        obj.Add(HelperMethods.GetPrettyDate(outTime));
                    }
                    else {
                        obj.Add(item.DateAdded);
                    }

                    returnObj.Add(obj.ToArray());
                }
            }
        }

        List<object> returnObj2 = new List<object>();
        returnObj2.Add(returnObj);
        returnObj2.Add(TableBuilder.UseAlternateGridviewRows.ToString().ToLower());
        returnObj2.Add(TableBuilder.ShowRowCountGridViewTable.ToString().ToLower());

        return returnObj2.ToArray();
    }

    [WebMethod]
    public object[] GetCommonStatistics(string div) {
        List<object> returnObj = new List<object>();
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            string daysToTrackCommonStatistics = WebConfigurationManager.AppSettings["DaysToTrackCommonStatistics"];

            int daysToTrack = 5;
            int.TryParse(daysToTrackCommonStatistics, out daysToTrack);
            TimeSpan timeSpan;

            if (div == "div_NewVisitors" || div == "div_RecentLogins") {
                LoginActivity tempLoginActivity = new LoginActivity();
                tempLoginActivity.GetActivity();

                List<string> guestUsers = new List<string>();
                List<string> loginUsers = new List<string>();

                foreach (LoginActivity_Coll coll in tempLoginActivity.ActivityList) {
                    timeSpan = ServerSettings.ServerDateTime.Subtract(coll.DateAdded);
                    if (coll.ActType == ActivityType.Guest && coll.IsSuccessful && div == "div_NewVisitors") {
                        if (timeSpan.Days <= daysToTrack && !guestUsers.Contains(coll.IpAddress)) {
                            List<object> obj = new List<object>();
                            obj.Add(coll.IpAddress);
                            obj.Add(HelperMethods.GetPrettyDate(coll.DateAdded));

                            returnObj.Add(obj.ToArray());
                            guestUsers.Add(coll.IpAddress);
                        }
                    }
                    if ((coll.ActType == ActivityType.Login || coll.ActType == ActivityType.Social) && coll.IsSuccessful && div == "div_RecentLogins") {
                        if (timeSpan.Days <= daysToTrack && !loginUsers.Contains(coll.UserName.ToLower())) {
                            List<object> obj = new List<object>();
                            obj.Add(coll.IpAddress);
                            obj.Add(coll.UserName);
                            obj.Add(coll.ActType.ToString());
                            obj.Add(HelperMethods.GetPrettyDate(coll.DateAdded));

                            returnObj.Add(obj.ToArray());
                            loginUsers.Add(coll.UserName.ToLower());
                        }
                    }
                }
            }
            else if (div == "div_RegisteredUsers") {
                MembershipUserCollection userCollection = Membership.GetAllUsers();
                foreach (MembershipUser user in userCollection) {
                    timeSpan = ServerSettings.ServerDateTime.Subtract(user.CreationDate);
                    if (timeSpan.Days <= daysToTrack) {
                        List<object> obj = new List<object>();
                        obj.Add(user.UserName);
                        obj.Add(user.IsOnline.ToString());
                        obj.Add(user.Email);
                        obj.Add(HelperMethods.GetPrettyDate(user.CreationDate));

                        returnObj.Add(obj.ToArray());
                    }
                }
            }
        }

        List<object> returnObj2 = new List<object>();
        returnObj2.Add(returnObj);
        returnObj2.Add(TableBuilder.UseAlternateGridviewRows.ToString().ToLower());
        returnObj2.Add(TableBuilder.ShowRowCountGridViewTable.ToString().ToLower());

        return returnObj2.ToArray();
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

                string altRowClass = "GridAlternate";
                if (!TableBuilder.UseAlternateGridviewRows) {
                    altRowClass = "GridNormalRow";
                }

                string numberCol = string.Empty;
                string numberCol2 = string.Empty;
                if (TableBuilder.ShowRowCountGridViewTable) {
                    numberCol = "<td width='45px' style='padding-left: 10px;'>#</td>";
                    numberCol2 = "<td class=\"GridViewNumRow\">{0}</td>";
                }

                str.Append("<div class='network-info-holder'>");
                str.Append("<table class='gridview-table network-info-tables'><tbody>");
                str.Append("<tr class='myHeaderStyle myHeaderStyle-network-tables'>" + numberCol + "<td colspan='2'>Server Stats</td></tr>");

                if (!string.IsNullOrEmpty(numberCol2)) {
                    str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "1") + "<td class='network-info-title'>Total Groups</td><td class='network-info-value'>" + groups.group_dt.Count.ToString() + "</td></tr>");
                }
                else {
                    str.Append("<tr class='GridNormalRow'><td class='network-info-title'>Total Groups</td><td class='network-info-value'>" + groups.group_dt.Count.ToString() + "</td></tr>");
                }

                if (!string.IsNullOrEmpty(numberCol2)) {
                    str.Append("<tr class='" + altRowClass + "'>" + string.Format(numberCol2, "2") + "<td class='network-info-title'>Total Users</td><td class='network-info-value'>" + (coll.Count - 1).ToString() + "</td></tr>");
                }
                else {
                    str.Append("<tr class='" + altRowClass + "'><td class='network-info-title'>Total Users</td><td class='network-info-value'>" + (coll.Count - 1).ToString() + "</td></tr>");
                }

                if (!string.IsNullOrEmpty(numberCol2)) {
                    str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "3") + "<td class='network-info-title'>Users Online</td><td class='network-info-value'>" + totalonline.ToString() + "</td></tr>");
                }
                else {
                    str.Append("<tr class='GridNormalRow'><td class='network-info-title'>Users Online</td><td class='network-info-value'>" + totalonline.ToString() + "</td></tr>");
                }

                if (!string.IsNullOrEmpty(numberCol2)) {
                    str.Append("<tr class='" + altRowClass + "'>" + string.Format(numberCol2, "4") + "<td class='network-info-title'>Users Locked</td><td class='network-info-value'>" + locked.ToString() + "</td></tr>");
                }
                else {
                    str.Append("<tr class='" + altRowClass + "'><td class='network-info-title'>Users Locked</td><td class='network-info-value'>" + locked.ToString() + "</td></tr>");
                }

                string upTimeStr = HelperMethods.UpTime.Days + ":" + HelperMethods.UpTime.Hours + ":" + HelperMethods.UpTime.Minutes;
                if (upTimeStr != "0:0:0") {
                    if (!string.IsNullOrEmpty(numberCol2)) {
                        str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "5") + "<td class='network-info-title'>Site Uptime (D:H:M)</td><td class='network-info-value'>" + upTimeStr + "</td></tr>");
                    }
                    else {
                        str.Append("<tr class='GridNormalRow'><td class='network-info-title'>Site Uptime (D:H:M)</td><td class='network-info-value'>" + upTimeStr + "</td></tr>");
                    }

                    if (!string.IsNullOrEmpty(numberCol2)) {
                        str.Append("<tr class='" + altRowClass + "'>" + string.Format(numberCol2, "6") + "<td class='network-info-title'>CPU Count</td><td class='network-info-value'>" + Environment.ProcessorCount + "</td></tr>");
                    }
                    else {
                        str.Append("<tr class='" + altRowClass + "'><td class='network-info-title'>CPU Count</td><td class='network-info-value'>" + Environment.ProcessorCount + "</td></tr>");
                    }

                    if (_ss.MonitorCpuUsage) {
                        double newCpuUsageVal = Math.Round(ServerSettings.CurrentCpuUsageValue, 1);
                        if (!string.IsNullOrEmpty(numberCol2)) {
                            str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "7") + "<td class='network-info-title'>Site CPU Usage</td><td class='network-info-value'>" + newCpuUsageVal.ToString() + "%</td></tr>");
                        }
                        else {
                            str.Append("<tr class='GridNormalRow'><td class='network-info-title'>Site CPU Usage</td><td class='network-info-value'>" + newCpuUsageVal.ToString() + "%</td></tr>");
                        }

                        string memoryUsage = string.Format("{0:n0}", ServerSettings.CurrentMemoryUsageValue) + " K";
                        if (!string.IsNullOrEmpty(numberCol2)) {
                            str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "8") + "<td class='network-info-title'>Site Memory Usage</td><td class='network-info-value'>" + memoryUsage + "</td></tr>");
                        }
                        else {
                            str.Append("<tr class='GridNormalRow'><td class='network-info-title'>Site Memory Usage</td><td class='network-info-value'>" + memoryUsage + "</td></tr>");
                        }
                    }
                }
                else {
                    if (!string.IsNullOrEmpty(numberCol2)) {
                        str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "5") + "<td class='network-info-title'>CPU Count</td><td class='network-info-value'>" + Environment.ProcessorCount + "</td></tr>");
                    }
                    else {
                        str.Append("<tr class='GridNormalRow'><td class='network-info-title'>CPU Count</td><td class='network-info-value'>" + Environment.ProcessorCount + "</td></tr>");
                    }

                    if (_ss.MonitorCpuUsage) {
                        double newCpuUsageVal = Math.Round(ServerSettings.CurrentCpuUsageValue, 1);
                        if (!string.IsNullOrEmpty(numberCol2)) {
                            str.Append("<tr class='" + altRowClass + "'>" + string.Format(numberCol2, "6") + "<td class='network-info-title'>Site CPU Usage</td><td class='network-info-value'>" + newCpuUsageVal.ToString() + "%</td></tr>");
                        }
                        else {
                            str.Append("<tr class='" + altRowClass + "'><td class='network-info-title'>Site CPU Usage</td><td class='network-info-value'>" + newCpuUsageVal.ToString() + "%</td></tr>");
                        }

                        string memoryUsage = string.Format("{0:n0}", ServerSettings.CurrentMemoryUsageValue) + " K";
                        if (!string.IsNullOrEmpty(numberCol2)) {
                            str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "7") + "<td class='network-info-title'>Site Memory Usage</td><td class='network-info-value'>" + memoryUsage + "</td></tr>");
                        }
                        else {
                            str.Append("<tr class='GridNormalRow'><td class='network-info-title'>Site Memory Usage</td><td class='network-info-value'>" + memoryUsage + "</td></tr>");
                        }
                    }
                }

                str.Append("</tbody></table>");
                str.Append("</div>");

                str.Append("<div class='network-info-holder'>");
                str.Append("<table class='gridview-table network-info-tables'><tbody>");
                str.Append("<tr class='myHeaderStyle myHeaderStyle-network-tables'>" + numberCol + "<td colspan='2'>Network Stats</td></tr>");

                if (!string.IsNullOrEmpty(numberCol2)) {
                    str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "1") + "<td class='network-info-title'>SSL Redirect</td><td class='network-info-value'>" + sslRedirect + "</td></tr>");
                }
                else {
                    str.Append("<tr class='GridNormalRow'><td class='network-info-title'>SSL Redirect</td><td class='network-info-value'>" + sslRedirect + "</td></tr>");
                }

                if (!string.IsNullOrEmpty(numberCol2)) {
                    str.Append("<tr class='" + altRowClass + "'>" + string.Format(numberCol2, "2") + "<td class='network-info-title'>IP Listener</td><td class='network-info-value'>" + ServerSettings.GetIpListener + "</td></tr>");
                }
                else {
                    str.Append("<tr class='" + altRowClass + "'><td class='network-info-title'>IP Listener</td><td class='network-info-value'>" + ServerSettings.GetIpListener + "</td></tr>");
                }

                if (!string.IsNullOrEmpty(numberCol2)) {
                    str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "3") + "<td class='network-info-title'>Current IP</td><td class='network-info-value'>" + ipaddress + "</td></tr>");
                }
                else {
                    str.Append("<tr class='GridNormalRow'><td class='network-info-title'>Current IP</td><td class='network-info-value'>" + ipaddress + "</td></tr>");
                }

                if (!string.IsNullOrEmpty(numberCol2)) {
                    str.Append("<tr class='" + altRowClass + "'>" + string.Format(numberCol2, "4") + "<td class='network-info-title'>IP WatchList Count</td><td class='network-info-value'>" + _ipwatch.ipwatchdt.Count + "</td></tr>");
                }
                else {
                    str.Append("<tr class='" + altRowClass + "'><td class='network-info-title'>IP WatchList Count</td><td class='network-info-value'>" + _ipwatch.ipwatchdt.Count + "</td></tr>");
                }

                if (!string.IsNullOrEmpty(numberCol2)) {
                    str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "5") + "<td class='network-info-title'>Email System</td><td class='network-info-value'>" + emailSystem + "</td></tr>");
                }
                else {
                    str.Append("<tr class='GridNormalRow'><td class='network-info-title'>Email System</td><td class='network-info-value'>" + emailSystem + "</td></tr>");
                }

                if (_ss.RecordSiteRequests) {
                    double current = Math.Round(NetworkActivityBuilder.CurrentRequests_request, 2);
                    double average = Math.Round(NetworkActivityBuilder.AverageRequests_request, 2);
                    double maximum = Math.Round(NetworkActivityBuilder.FindMax(), 2);

                    if (!string.IsNullOrEmpty(numberCol2)) {
                        str.Append("<tr class='" + altRowClass + "'>" + string.Format(numberCol2, "6") + "<td class='network-info-title'>Current Request(s)</td><td class='network-info-value'>" + current + "</td></tr>");
                    }
                    else {
                        str.Append("<tr class='" + altRowClass + "'><td class='network-info-title'>Current Request(s)</td><td class='network-info-value'>" + current + "</td></tr>");
                    }

                    if (!string.IsNullOrEmpty(numberCol2)) {
                        str.Append("<tr class='GridNormalRow'>" + string.Format(numberCol2, "7") + "<td class='network-info-title'>Average Request(s)</td><td class='network-info-value'>" + average + "</td></tr>");
                    }
                    else {
                        str.Append("<tr class='GridNormalRow'><td class='network-info-title'>Average Request(s)</td><td class='network-info-value'>" + average + "</td></tr>");
                    }

                    if (!string.IsNullOrEmpty(numberCol2)) {
                        str.Append("<tr class='" + altRowClass + "'>" + string.Format(numberCol2, "8") + "<td class='network-info-title'>Maximum Request(s)</td><td class='network-info-value'>" + maximum + "</td></tr>");
                    }
                    else {
                        str.Append("<tr class='" + altRowClass + "'><td class='network-info-title'>Maximum Request(s)</td><td class='network-info-value'>" + maximum + "</td></tr>");
                    }
                }

                str.Append("</tbody></table>");
                str.Append("</div>");

                str.Append("<div class='clear'></div>");
            }
            catch (Exception e) { 
                AppLog.AddError(e); 
            }

            return str.ToString();
        }
    }
}
