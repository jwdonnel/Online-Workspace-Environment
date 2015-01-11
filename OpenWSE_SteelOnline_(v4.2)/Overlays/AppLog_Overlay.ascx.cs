using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Text;
using System.Diagnostics;
using System.Security.Principal;
using System.Globalization;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.Overlays;

public partial class Overlays_AppLog_Overlay : System.Web.UI.UserControl {
    private ServerSettings _ss = new ServerSettings();
    private OverlayInitializer _overlayInit;
    private string _username;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;

        _overlayInit = new OverlayInitializer(userId.Name, "Overlays/AppLog_Overlay.ascx");
        if (_overlayInit.TryLoadOverlay) {
            _username = userId.Name;
            LoadOverlay();
        }
        else if ((!userId.IsAuthenticated) && (_ss.NoLoginRequired))
            LoadOverlay();
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }

    private void LoadOverlay() {
        RegisterPostbackScripts.RegisterStartupScript(this, BuildJS);
    }

    protected void refresh_applog_overlay(object sender, EventArgs e) {
        MembershipUserCollection coll = Membership.GetAllUsers();
        IPWatch _ipwatch = new IPWatch(true);
        var applog = new AppLog(true);
        var str = new StringBuilder();
        int errors = 0;
        int success = 0;
        int locked = coll.Cast<MembershipUser>().Count(u => !u.IsApproved);
        var str2 = new StringBuilder();
        try {
            string _username = HttpContext.Current.User.Identity.Name;
            var groups = new Groups(_username);
            var dbimports = new DBImporter();
            dbimports.BinaryDeserialize();
            groups.getEntries();
            int totalonline = Membership.GetNumberOfUsersOnline();
            var membershipUser = Membership.GetUser(ServerSettings.AdminUserName);
            if (membershipUser != null && membershipUser.IsOnline) {
                totalonline = totalonline - 1;
            }
            str2.Append("<h5><span class='pad-right'><b>Total Groups:</b></span><span>" + groups.group_dt.Count.ToString() + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>Total Users:</b></span><span>" + (coll.Count - 1).ToString() + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>Users Online:</b></span><span>" + totalonline.ToString() + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>Users Locked:</b></span><span>" + locked.ToString() + "</span></h5>");
            str2.Append("<div class='clear-space'></div><h5><span class='pad-right'><b>IP's Being Watched:</b></span><span>" + _ipwatch.ipwatchdt.Count + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>IP's Blocked:</b></span><span>" + ServerSettings.getTotalBlockedIP + "</span></h5>");

            string sslRedirect = "Off";
            if (_ss.SSLRedirect)
                sslRedirect = "On";

            string emailSystem = "Off";
            if (_ss.EmailSystemStatus)
                emailSystem = "On";

            string ipaddress = HttpContext.Current.Request.UserHostAddress;
            if (ipaddress == "::1")
                ipaddress = "127.0.0.1";

            str2.Append("<div class='clear-space'></div><h5><span class='pad-right'><b>Email System:</b></span><span>" + emailSystem + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>SSL Redirect:</b></span><span>" + sslRedirect + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>IP Listener Status:</b></span><span>" + ServerSettings.GetIpListener + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>Current IP:</b></span><span>" + ipaddress + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>Site Uptime (D:H:M:):</b></span><span>" + HelperMethods.UpTime.Days + ":" + HelperMethods.UpTime.Hours + ":" + HelperMethods.UpTime.Minutes + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>Memory Allocated:</b></span><span>" + HelperMethods.FormatBytes(GC.GetTotalMemory(true)) + "</span></h5>");
            str2.Append("<div class='clear-space'></div><h5><span class='pad-right'><b>Total Event Errors:</b></span><span>" + applog.app_coll.Count.ToString() + "</span></h5>");
            str2.Append("<h5><span class='pad-right'><b>Todays Hit Total:</b></span><span>" + GetSiteRequests.HitCountTotal + "</span></h5>");
        }
        catch { }

        string finalstr = str2.ToString() + str.ToString();
        applog_pnl_entries.Controls.Clear();
        applog_pnl_entries.Controls.Add(new LiteralControl(finalstr));
    }

    private string BuildJS {
        get {
            StringBuilder str = new StringBuilder();
            str.Append("Sys.Application.add_load(function () {");
            str.Append("$.ajax({");
            str.Append("url: '" + ResolveUrl("~/WebServices/NetworkLog.asmx/GetNetworkActivity_Overlay") + "',");
            str.Append("type: 'POST',");
            str.Append("data: '{ }',");
            str.Append("contentType: 'application/json; charset=utf-8',");
            str.Append("success: function (data) {");
            str.Append("var response = data.d;");
            str.Append("if (response != '') {");
            str.Append("$('#applog_pnl_entries').html(response);");
            str.Append("} } }); });");
            return str.ToString();
        }
    }
}