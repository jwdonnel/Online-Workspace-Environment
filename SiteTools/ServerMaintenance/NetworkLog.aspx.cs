#region

using System;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.AutoUpdates;
using System.Collections.Generic;
using System.IO;
using OpenWSE_Tools.Notifications;

#endregion

public partial class SiteTools_NetworkLog : Page {
    private ServerSettings _ss = new ServerSettings();
    private AppLog _applog;
    private IPWatch _ipwatch = new IPWatch(true);
    private LoginActivity _la = new LoginActivity();
    private IIdentity _userId;
    private string _username;

    protected void Page_Load(object sender, EventArgs e) {
        _userId = HttpContext.Current.User.Identity;
        if (!_userId.IsAuthenticated) {
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), _userId.Name)) {
                _username = _userId.Name;
                _applog = new AppLog(true);
                LoadEvents(ref GV_Requests);
                LoadIgnoredEvents(ref gv_Ignore);
                LoadLoginActivity(ref gv_LoginActivity);

                if (!IsPostBack) {
                    PageLoadInit.BuildLinks(pnlLinkBtns, _username, this.Page);
                    LoadIpAddresses(ref GV_WatchList);
                    if (!_ss.LockIPListenerWatchlist) {
                        pnl_ipAutoBlock.Enabled = true;
                        pnl_ipAutoBlock.Visible = true;
                        pnl_addip.Enabled = true;
                        pnl_addip.Visible = true;
                    }
                    else {
                        pnl_ipAutoBlock.Enabled = false;
                        pnl_ipAutoBlock.Visible = false;
                        pnl_addip.Enabled = false;
                        pnl_addip.Visible = false;
                        ltl_lockedipwatchlist.Text = HelperMethods.GetLockedByMessage();
                    }
                }

                RegisterPostbackScripts.RegisterStartupScript(this, "$('#imgPausePlay').removeClass('img-play');$('#imgPausePlay').addClass('img-pause');");

                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    rb_AutoIPBlock_on.Enabled = false;
                    rb_AutoIPBlock_off.Enabled = false;
                    pnl_addip.Enabled = false;
                    pnl_addip.Visible = false;
                    pnl_ipAutoBlock.Enabled = false;
                    pnl_ipAutoBlock.Visible = false;
                    pnl_rtk_E.Enabled = false;
                    pnl_rtk_E.Visible = false;
                    LinkButton1.Enabled = false;
                    LinkButton1.Visible = false;
                    lbtnClearAllIgnored.Enabled = false;
                    lbtnClearAllIgnored.Visible = false;
                    networkSettings.Visible = false;
                }

                if (_ss.AutoBlockIP) {
                    rb_AutoIPBlock_on.Checked = true;
                    rb_AutoIPBlock_off.Checked = false;
                    pnl_attemptsBeforeBlock.Enabled = true;
                    pnl_attemptsBeforeBlock.Visible = true;
                }
                else {
                    rb_AutoIPBlock_on.Checked = false;
                    rb_AutoIPBlock_off.Checked = true;
                    pnl_attemptsBeforeBlock.Enabled = false;
                    pnl_attemptsBeforeBlock.Visible = false;
                }

                if (_ss.LockIPListenerWatchlist) {
                    MemberDatabase member = new MemberDatabase(_username);
                    BuildIpAddresses_ViewOnly();
                    ltl_lockediplistener.Text = HelperMethods.GetLockedByMessage();
                }
                else
                    BuildIpAddresses();

                if (!IsPostBack) {
                    string ertk = _ss.WebEventsToKeep.ToString(CultureInfo.InvariantCulture);
                    if (ertk == "0")
                        ertk = "All";

                    tb_autoblock_count.Text = _ss.AutoBlockIPCount.ToString();
                    tb_Records_to_keep_E.Text = ertk;
                }

                LoadSettings();
                GetCallBack();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void GetCallBack() {
        string controlName = Request.Params["__EVENTTARGET"];
        switch (controlName) {
            case "MainContent_LinkButton1":
                _applog.deleteLog();
                _applog = new AppLog(true);
                LoadEvents(ref GV_Requests);
                LoadIgnoredEvents(ref gv_Ignore);
                break;
            case "MainContent_LinkButton2":
                DeleteLogFiles();
                _applog = new AppLog(true);
                LoadEvents(ref GV_Requests);
                LoadIgnoredEvents(ref gv_Ignore);
                break;
            case "MainContent_lbtnClearAllIgnored":
                AppLogIgnore ignore = new AppLogIgnore(false);
                ignore.deleteRecords();
                LoadEvents(ref GV_Requests);
                LoadIgnoredEvents(ref gv_Ignore);
                break;
            case "MainContent_lbtnClearAllLogin":
                _la.DeleteAllItems();
                _applog = new AppLog(true);
                LoadEvents(ref GV_Requests);
                LoadIgnoredEvents(ref gv_Ignore);
                LoadLoginActivity(ref gv_LoginActivity);
                break;
        }
    }

    private void DeleteLogFiles() {
        string filePath = ServerSettings.GetServerMapLocation + "Logging";
        if (Directory.Exists(filePath)) {
            string[] fileList = Directory.GetFiles(filePath);
            foreach (string file in fileList) {
                FileInfo fi = new FileInfo(file);
                if (fi.Extension.ToLower() == ".log") {
                    try {
                        File.Delete(file);
                    }
                    catch { }
                }
            }
        }
    }

    private void LoadSettings() {
        if (_ss.RecordActivity) {
            cb_netactOn.Checked = true;
            cb_netactOff.Checked = false;
            pnl_RecordLogFile.Enabled = true;
            pnl_RecordLogFile.Visible = true;
            tableEmailAct.Style["display"] = "block";
            if (_ss.EmailActivity) {
                cb_emailon.Checked = true;
                cb_emailoff.Checked = false;
            }
            else {
                cb_emailon.Checked = false;
                cb_emailoff.Checked = true;
            }
        }
        else {
            cb_netactOn.Checked = false;
            cb_netactOff.Checked = true;
            pnl_RecordLogFile.Enabled = false;
            pnl_RecordLogFile.Visible = false;
            tableEmailAct.Style["display"] = "none";
        }

        if (_ss.DisableJavascriptErrorAlerts) {
            rb_DisableJavascriptErrorAlerts_on.Checked = true;
            rb_DisableJavascriptErrorAlerts_off.Checked = false;
        }
        else {
            rb_DisableJavascriptErrorAlerts_on.Checked = false;
            rb_DisableJavascriptErrorAlerts_off.Checked = true;
        }

        if (_ss.RecordActivityToLogFile) {
            rb_recordLogFile_on.Checked = true;
            rb_recordLogFile_off.Checked = false;
        }
        else {
            rb_recordLogFile_on.Checked = false;
            rb_recordLogFile_off.Checked = true;
        }

        if (_ss.RecordErrorsOnly) {
            rb_RecordErrorsOnly_on.Checked = true;
            rb_RecordErrorsOnly_off.Checked = false;
        }
        else {
            rb_RecordErrorsOnly_on.Checked = false;
            rb_RecordErrorsOnly_off.Checked = true;
        }

        if (_ss.RecordLoginActivity) {
            rb_recordLoginActivity_on.Checked = true;
            rb_recordLoginActivity_off.Checked = false;
        }
        else {
            rb_recordLoginActivity_on.Checked = false;
            rb_recordLoginActivity_off.Checked = true;
        }
    }

    protected void imgbtn_search_Click(object sender, EventArgs e) {
        _applog = new AppLog(true);
        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
        LoadLoginActivity(ref gv_LoginActivity);
    }
    protected void hf_searchreset_Changed(object sender, EventArgs e) {
        _applog = new AppLog(true);
        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
        LoadLoginActivity(ref gv_LoginActivity);
    }

    protected void lbtn_refreshignore_Click(object sender, EventArgs e) {
        LoadIgnoredEvents(ref gv_Ignore);
    }

    protected void lbtn_refresherrors_Click(object sender, EventArgs e) {
        _applog = new AppLog(true);
        LoadEvents(ref GV_Requests);
    }

    protected void lbtn_refreshLoginactivity_Click(object sender, EventArgs e) {
        LoadLoginActivity(ref gv_LoginActivity);
    }

    protected void lbtn_refreshIPWatchList_Click(object sender, EventArgs e) {
        _ipwatch = new IPWatch(true);
        LoadIpAddresses(ref GV_WatchList);
    }

    private static bool Parse(string ipAddress) {
        try {
            var address = IPAddress.Parse(ipAddress);
        }
        catch {
            return false;
        }

        return true;
    }


    #region Event List

    public void LoadEvents(ref GridView gv) {
        DataView dvlist = GetRequestList();
        gv.DataSource = dvlist;
        gv.DataBind();
    }
    public DataView GetRequestList() {
        var dtlist = new DataTable();
        dtlist.Columns.Add(new DataColumn("date"));
        dtlist.Columns.Add(new DataColumn("name"));
        dtlist.Columns.Add(new DataColumn("exceptiontype"));
        dtlist.Columns.Add(new DataColumn("comment"));
        dtlist.Columns.Add(new DataColumn("stacktrace"));
        dtlist.Columns.Add(new DataColumn("machinename"));
        dtlist.Columns.Add(new DataColumn("requesturl"));
        dtlist.Columns.Add(new DataColumn("username"));
        dtlist.Columns.Add(new DataColumn("ipAddress"));
        dtlist.Columns.Add(new DataColumn("ignore"));
        dtlist.Columns.Add(new DataColumn("delete"));
        dtlist.Columns.Add(new DataColumn("refresh"));

        AppLogIgnore ignores = new AppLogIgnore(true);
        foreach (var al in _applog.app_coll) {
            string x = tb_search.Text.ToLower();
            var cancontinue = false;

            if (x.Length > 3) {
                if (x.Substring(x.Length - 3) == "...") {
                    x = x.Substring(0, x.Length - 3);
                }
            }

            if ((x == "search events") || (string.IsNullOrEmpty(x))) {
                cancontinue = true;
            }
            else if ((al.DatePosted.ToLower().Contains(x)) || (al.EventName.ToLower().Contains(x))
                 || (al.EventComment.ToLower().Contains(x)) || (al.UserName.ToLower().Contains(x))
                 || (al.StackTrace.ToLower().Contains(x)) || (al.RequestUrl.ToLower().Contains(x))
                 || (al.MachineName.ToLower().Contains(x)) || (al.ExceptionType.ToLower().Contains(x))
                 || (al.ApplicationPath.ToLower().Contains(x)) || (al.IPAddress.ToLower().Contains(x))) {
                cancontinue = true;
            }

            if (cb_ViewErrorsOnly.Checked && (string.IsNullOrEmpty(al.ExceptionType) || al.ExceptionType.ToLower() == "n/a")) {
                if (!al.EventComment.ToLower().Contains("error")) {
                    continue;
                }
            }


            if (cancontinue) {
                DataRow drlist = dtlist.NewRow();
                drlist["date"] = al.DatePosted;
                drlist["name"] = al.EventName;
                drlist["exceptiontype"] = al.ExceptionType;
                drlist["comment"] = al.EventComment;
                drlist["stacktrace"] = al.StackTrace;
                drlist["machinename"] = al.MachineName;
                drlist["ipAddress"] = al.IPAddress;

                string requestUrl = al.RequestUrl;
                try {
                    Uri uri = new Uri(requestUrl);
                    if (!string.IsNullOrEmpty(uri.PathAndQuery))
                        requestUrl = ".." + uri.PathAndQuery;
                }
                catch { }

                if (requestUrl.Length > 120) {
                    requestUrl = requestUrl.Insert(60, "<wbr>");
                    requestUrl = requestUrl.Insert(120, "<wbr>");
                }
                else if (requestUrl.Length > 60)
                    requestUrl = requestUrl.Insert(60, "<wbr>");

                drlist["requesturl"] = requestUrl;

                string currUser = al.UserName;
                if (string.IsNullOrEmpty(currUser))
                    currUser = "Not a system user.";

                drlist["username"] = currUser;

                if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    drlist["ignore"] = "<a href='#ignore' class='img-allow pad-all-sml margin-left-sml margin-right-sml' onclick=\"IgnoreError('" + al.EventID + "');return false;\" title='Click to Ignore'></a>";
                    if (ignores.appIgnore_coll.Count > 0) {
                        foreach (AppLogIgnore_Coll aliIgnore in ignores.appIgnore_coll) {
                            if (al.EventComment.ToLower().Contains(aliIgnore.EventComment.ToLower())) {
                                if (aliIgnore.RefreshOnError)
                                    drlist["refresh"] = "<a href='#NoRefresh' class='img-refresh pad-all-sml margin-left-sml margin-right-sml RandomActionBtns' onclick=\"RefreshPageOnError('" + aliIgnore.EventID + "');return false;\" title='Click to cancel refresh page'></a>";
                                else
                                    drlist["refresh"] = "<a href='#Refresh' class='img-norefresh pad-all-sml margin-left-sml margin-right-sml RandomActionBtns' onclick=\"RefreshPageOnError('" + aliIgnore.EventID + "');return false;\" title='Click to allow refresh of page upon this error'></a>";

                                drlist["ignore"] = "<a href='#Allow' class='img-ignore pad-all-sml margin-left-sml margin-right-sml' onclick=\"AllowError('" + aliIgnore.EventID + "');return false;\" title='Click to Allow'></a><div class='clear-space-five'></div>";
                                break;
                            }
                        }
                    }
                    drlist["delete"] = "<a href='#Delete' class='td-delete-btn margin-left-sml margin-right-sml' onclick=\"DeleteEvent('" + al.EventID + "');return false;\" title='Delete Event'></a>";
                }
                else {
                    drlist["delete"] = "-";
                }
                dtlist.Rows.Add(drlist);
            }
        }
        var dvlist = new DataView(dtlist);
        return dvlist;
    }
    protected void btn_Update_rtk_E_Clicked(object sender, EventArgs e) {
        string number = tb_Records_to_keep_E.Text;
        if (!string.IsNullOrEmpty(number)) {
            if (number.ToLower() == "all") {
                ServerSettings.update_WebEventsToKeep(0);
                tb_Records_to_keep_E.Text = "All";
            }
            else {
                int tempOut;
                if (int.TryParse(number, out tempOut)) {
                    ServerSettings.update_WebEventsToKeep(tempOut);

                    if (tempOut == 0) {
                        tb_Records_to_keep_E.Text = "All";
                    }
                }
            }
        }
    }
    protected void hf_updateAllow_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateAllow.Value)) {
            AppLogIgnore ignore = new AppLogIgnore(false);
            ignore.deleteRecord(hf_updateAllow.Value);
            hf_updateAllow.Value = string.Empty;
        }

        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
    }
    protected void hf_deleteError_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_deleteError.Value)) {
            _applog.deleteRecord(hf_deleteError.Value);
            hf_deleteError.Value = string.Empty;
        }

        _applog = new AppLog(true);
        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
    }

    #endregion


    #region Ignored Errors

    public void LoadIgnoredEvents(ref GridView gv) {
        DataView dvlist = GetIgnoredList();
        gv.DataSource = dvlist;
        gv.DataBind();
    }
    public DataView GetIgnoredList() {
        var dtlist = new DataTable();
        dtlist.Columns.Add(new DataColumn("date"));
        dtlist.Columns.Add(new DataColumn("comment"));
        dtlist.Columns.Add(new DataColumn("ignore"));
        dtlist.Columns.Add(new DataColumn("refresh"));
        dtlist.Columns.Add(new DataColumn("timesHit"));
        AppLogIgnore ignores = new AppLogIgnore(true);
        foreach (var al in ignores.appIgnore_coll) {
            string x = tb_SearchIgnore.Text.ToLower();
            var cancontinue = false;

            if (x.Length > 3) {
                if (x.Substring(x.Length - 3) == "...") {
                    x = x.Substring(0, x.Length - 3);
                }
            }

            if ((x.ToLower() == "search events") || (string.IsNullOrEmpty(x))) {
                cancontinue = true;
            }
            else if ((al.DatePosted.ToLower().Contains(x)) || (al.EventComment.ToLower().Contains(x))) {
                cancontinue = true;
            }

            if (cancontinue) {
                DataRow drlist = dtlist.NewRow();
                drlist["date"] = al.DatePosted;
                drlist["comment"] = al.EventComment;
                if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    drlist["timesHit"] = al.TimesHit.ToString() + "<div class='clear-space-two'></div><a href='#' onclick='ResetHitCount(\"" + al.EventID + "\");return false;' title='Reset Hit Count' style='font-size: 11px;'>Reset</a>";
                    drlist["ignore"] = "<a href='#Delete' class='td-delete-btn' onclick=\"AllowError('" + al.EventID + "');return false;\" title='Delete Ignored Event'></a>";
                    if (al.RefreshOnError)
                        drlist["refresh"] = "<a href='#NoRefresh' class='img-refresh pad-all-sml margin-right RandomActionBtns' onclick=\"RefreshPageOnError('" + al.EventID + "');return false;\" title='Click to cancel refresh page'></a>";
                    else
                        drlist["refresh"] = "<a href='#Refresh' class='img-norefresh pad-all-sml margin-right RandomActionBtns' onclick=\"RefreshPageOnError('" + al.EventID + "');return false;\" title='Click to allow refresh of page upon this error'></a>";
                }
                else {
                    drlist["timesHit"] = al.TimesHit.ToString();
                    drlist["ignore"] = "-";
                }

                dtlist.Rows.Add(drlist);
            }
        }
        var dvlist = new DataView(dtlist);
        return dvlist;
    }
    protected void hf_updateIgnore_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateIgnore.Value)) {
            string comment = string.Empty;
            try {
                comment = _applog.GetEventComment(hf_updateIgnore.Value);
            }
            catch { }

            if (comment != "") {
                int urloccurred = comment.IndexOf("URL Occurred");
                if (urloccurred != -1) {
                    string tempComment = comment.Substring(urloccurred);
                    comment = comment.Replace(tempComment, "");
                }

                AppLogIgnore ignore = new AppLogIgnore(false);
                ignore.addItem(comment);
                hf_updateIgnore.Value = string.Empty;
            }

            LoadEvents(ref GV_Requests);
            LoadIgnoredEvents(ref gv_Ignore);
        }
    }
    protected void hf_resetHitCount_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_resetHitCount.Value)) {
            AppLogIgnore ignore = new AppLogIgnore(false);
            ignore.UpdateTimesHit(hf_resetHitCount.Value, 0);
        }

        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);

        hf_resetHitCount.Value = string.Empty;
    }
    protected void hf_updateRefreshOnError_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_updateRefreshOnError.Value)) {
            AppLogIgnore ignore = new AppLogIgnore(false);

            bool temp = ignore.GetRefreshOnError(hf_updateRefreshOnError.Value);
            bool refresh = true;
            if (temp)
                refresh = false;

            ignore.UpdateRefreshOnError(hf_updateRefreshOnError.Value, refresh);
            hf_updateRefreshOnError.Value = string.Empty;
        }

        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
    }

    #endregion


    #region Login Activity

    public void LoadLoginActivity(ref GridView gv) {
        DataView dvlist = GetLoginList();
        gv.DataSource = dvlist;
        gv.DataBind();

        string ctrlPostback = ScriptManager.GetCurrent(this.Page).AsyncPostBackSourceElementID;
        if (!ctrlPostback.Contains("btn_daystokeepLoginActivity")) {
            if (_ss.DeleteOldLoginActivity) {
                rb_autoDeleteLoginActivity_on.Checked = true;
                rb_autoDeleteLoginActivity_off.Checked = false;
                pnl_autoDeleteLoginActivity_days.Enabled = true;
                pnl_autoDeleteLoginActivity_days.Visible = true;
            }
            else {
                rb_autoDeleteLoginActivity_on.Checked = false;
                rb_autoDeleteLoginActivity_off.Checked = true;
                pnl_autoDeleteLoginActivity_days.Enabled = false;
                pnl_autoDeleteLoginActivity_days.Visible = false;
            }

            tb_daystokeepLoginActivity.Text = _ss.LoginActivityToKeepInDays.ToString();
        }
    }
    public DataView GetLoginList() {
        var dtlist = new DataTable();
        dtlist.Columns.Add(new DataColumn("ID"));
        dtlist.Columns.Add(new DataColumn("Date"));
        dtlist.Columns.Add(new DataColumn("UserName"));
        dtlist.Columns.Add(new DataColumn("IpAddress"));
        dtlist.Columns.Add(new DataColumn("Message"));
        dtlist.Columns.Add(new DataColumn("Success"));
        dtlist.Columns.Add(new DataColumn("ActType"));
        dtlist.Columns.Add(new DataColumn("Delete"));
        dtlist.Columns.Add(new DataColumn("AllowBlock"));
        dtlist.Columns.Add(new DataColumn("IpAddressNoLink"));

        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            _la.GetActivity();
            lbtnClearAllLogin.Enabled = true;
            lbtnClearAllLogin.Visible = true;
        }
        else {
            lbtnClearAllLogin.Enabled = false;
            lbtnClearAllLogin.Visible = false;
        }

        Groups groups = new Groups();
        groups.getEntries();
        List<Dictionary<string, string>> drGroups = groups.group_dt;

        foreach (var act in _la.ActivityList) {
            DataRow drlist = dtlist.NewRow();
            drlist["ID"] = act.ID;
            drlist["Date"] = act.DateAdded.ToString();
            drlist["UserName"] = act.UserName;
            drlist["IpAddress"] = "<a href='#' onclick=\"SearchLoginIp('" + act.IpAddress + "');return false;\" style='opacity: 1.0!important; filter:  alpha(opacity=100)!important;'>" + act.IpAddress + "</a>";
            drlist["IpAddressNoLink"] = act.IpAddress;
            if (act.IsSuccessful) {
                if (act.ActType == ActivityType.Login || act.ActType == ActivityType.Social) {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        drlist["Message"] = act.UserName + " logged in successfully from " + act.IpAddress;
                    }
                    else {
                        drlist["Message"] = act.UserName + " logged in successfully to the Group " + GetGroupName(drGroups, act.LoginGroup) + " from " + act.IpAddress;
                    }
                }
                else if (act.ActType == ActivityType.Guest) {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        drlist["Message"] = act.UserName + " visited site from " + act.IpAddress;
                    }
                    else {
                        drlist["Message"] = act.UserName + " visited site for Group " + GetGroupName(drGroups, act.LoginGroup) + " from " + act.IpAddress;
                    }
                }
                else {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        drlist["Message"] = act.UserName + " logged out from " + act.IpAddress;
                    }
                    else {
                        drlist["Message"] = act.UserName + " logged out of the Group " + GetGroupName(drGroups, act.LoginGroup) + " from " + act.IpAddress;
                    }
                }

                drlist["Success"] = "<span class='img-checkmark'></span>";
            }
            else {
                if (act.ActType == ActivityType.Login || act.ActType == ActivityType.Social) {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        drlist["Message"] = act.IpAddress + " attempted to login using the name " + act.UserName + ". Login Failed.";
                    }
                    else {
                        drlist["Message"] = act.IpAddress + " attempted to login to the Group " + GetGroupName(drGroups, act.LoginGroup) + " using the name " + act.UserName + ". Login Failed.";
                    }
                }
                else if (act.ActType == ActivityType.Guest) {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        drlist["Message"] = act.UserName + " was not able to visit site from " + act.IpAddress;
                    }
                    else {
                        drlist["Message"] = act.UserName + " was not able to visit site for Group " + GetGroupName(drGroups, act.LoginGroup) + " from " + act.IpAddress;
                    }
                }
                else {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        drlist["Message"] = act.IpAddress + " attempted to log out using the name " + act.UserName + ". Login Failed.";
                    }
                    else {
                        drlist["Message"] = act.IpAddress + " attempted to log out from the Group " + GetGroupName(drGroups, act.LoginGroup) + " using the name " + act.UserName + ". Login Failed.";
                    }
                }

                drlist["Success"] = "";
            }

            drlist["ActType"] = act.ActType.ToString();

            if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                drlist["Delete"] = "<a href='#Delete' class='td-delete-btn' onclick=\"DeleteLoginActivity('" + act.ID + "');return false;\" title='Delete Login Event'></a>";

                _ipwatch = new IPWatch(true);
                bool found = false;
                foreach (Dictionary<string, string> dr in _ipwatch.ipwatchdt) {
                    if (dr["IPAddress"].ToString() == act.IpAddress) {
                        if (HelperMethods.ConvertBitToBoolean(dr["Blocked"])) {
                            drlist["AllowBlock"] = "<a href='#allow' class='img-ignore pad-all-sml margin-right RandomActionBtns' onclick=\"AllowBlockLoginEvent('" + act.ID + "');return false;\" title='Click to allow IP Address'></a>";
                        }
                        else {
                            drlist["AllowBlock"] = "<a href='#block' class='img-allow pad-all-sml margin-right RandomActionBtns' onclick=\"AllowBlockLoginEvent('" + act.ID + "');return false;\" title='Click to block IP Address'></a>";
                        }

                        found = true;
                        break;
                    }
                }

                if (!found) {
                    drlist["AllowBlock"] = "<a href='#allow' class='img-allow pad-all-sml margin-right RandomActionBtns' onclick=\"AllowBlockLoginEvent('" + act.ID + "');return false;\" title='Click to block IP Address'></a>";
                }
            }
            else {
                drlist["Delete"] = "N/A";
                drlist["AllowBlock"] = "";
            }

            dtlist.Rows.Add(drlist);
        }
        var dvlist = new DataView(dtlist);
        return dvlist;
    }
    private string GetGroupName(List<Dictionary<string, string>> drGroups, string groupId) {
        foreach (Dictionary<string, string> dr in drGroups) {
            if (dr["GroupID"] == groupId) {
                return dr["GroupName"];
            }
        }

        return "(Unknown)";
    }

    protected void hf_DeleteLoginEvent_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_DeleteLoginEvent.Value)) {
            _la.DeleteItem(hf_DeleteLoginEvent.Value);
        }

        hf_DeleteLoginEvent.Value = string.Empty;

        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
        LoadLoginActivity(ref gv_LoginActivity);
    }
    protected void hf_AllowBlockLoginEvent_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_AllowBlockLoginEvent.Value)) {
            LoginActivity_Coll act = _la.GetActivity(hf_AllowBlockLoginEvent.Value);

            NameValueCollection n = Request.ServerVariables;
            string remoteaddress = n["REMOTE_ADDR"];
            if (remoteaddress == "::1")
                remoteaddress = "127.0.0.1";

            if (!string.IsNullOrEmpty(act.ID)) {
                _ipwatch = new IPWatch(true);
                bool found = false;
                foreach (Dictionary<string, string> dr in _ipwatch.ipwatchdt) {
                    if (dr["IPAddress"].ToString() == act.IpAddress) {
                        if (HelperMethods.ConvertBitToBoolean(dr["Blocked"])) {
                            _ipwatch.updateBlocked(act.IpAddress, false);
                        }
                        else {
                            if ((act.IpAddress == remoteaddress) && (_userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                                RegisterPostbackScripts.RegisterStartupScript(this, "$('#loginactError').html(\"<span style='color: Red'>Cannot block current IP</span>\");setTimeout(function () { $('#loginactError').html(''); }, 5000);");
                            }
                            else {
                                _ipwatch.updateBlocked(act.IpAddress, true);
                            }
                        }
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    int totalAttempts = _la.GetTotalIPAttempts(act.IpAddress);
                    if ((act.IpAddress == remoteaddress) && (_userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                        _ipwatch.addItem(act.IpAddress, totalAttempts, false);
                        RegisterPostbackScripts.RegisterStartupScript(this, "$('#loginactError').html(\"<span style='color: Red'>Cannot block current IP</span>\");setTimeout(function () { $('#loginactError').html(''); }, 3000);");
                    }
                    else {
                        _ipwatch.addItem(act.IpAddress, totalAttempts, true);
                    }
                }
            }
        }

        hf_AllowBlockLoginEvent.Value = string.Empty;

        _ipwatch = new IPWatch(true);
        LoadIpAddresses(ref GV_WatchList);
        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
        LoadLoginActivity(ref gv_LoginActivity);
    }

    protected void rb_autoDeleteLoginActivity_on_CheckedChanged(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            rb_autoDeleteLoginActivity_on.Checked = true;
            rb_autoDeleteLoginActivity_off.Checked = false;
            pnl_autoDeleteLoginActivity_days.Enabled = true;
            pnl_autoDeleteLoginActivity_days.Visible = true;
            ServerSettings.update_DeleteOldLoginActivity(true);
        }

        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
        LoadLoginActivity(ref gv_LoginActivity);
    }
    protected void rb_autoDeleteLoginActivity_off_CheckedChanged(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            rb_autoDeleteLoginActivity_on.Checked = false;
            rb_autoDeleteLoginActivity_off.Checked = true;
            pnl_autoDeleteLoginActivity_days.Enabled = false;
            pnl_autoDeleteLoginActivity_days.Visible = false;
            ServerSettings.update_DeleteOldLoginActivity(false);
        }

        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
        LoadLoginActivity(ref gv_LoginActivity);
    }

    protected void btn_daystokeepLoginActivity_Clicked(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            string daysToKeep = tb_daystokeepLoginActivity.Text.Trim();
            int daysToKeepInt = 0;
            int.TryParse(daysToKeep, out daysToKeepInt);
            if (daysToKeepInt <= 0) {
                daysToKeepInt = 1;
            }

            ServerSettings.Update_LoginActivityToKeepInDays(daysToKeepInt);
            tb_daystokeepLoginActivity.Text = daysToKeepInt.ToString();
        }

        LoadEvents(ref GV_Requests);
        LoadIgnoredEvents(ref gv_Ignore);
        LoadLoginActivity(ref gv_LoginActivity);
    }

    #endregion


    #region IP Watch

    protected void GV_WatchList_RowDeleting(object sender, GridViewDeleteEventArgs e) {
        LoadIpAddresses(ref GV_WatchList);
    }
    protected void GV_WatchList_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || _ss.LockIPListenerWatchlist) {
            var lbBlock = (LinkButton)e.Row.FindControl("lb_block");
            var lbDelete = (LinkButton)e.Row.FindControl("lb_delete");
            var lbblockActionsna = (Label)e.Row.FindControl("lb_blockActions_na");
            var lbDeleteNa = (Label)e.Row.FindControl("lb_delete_na");
            if ((lbBlock != null) && (lbDelete != null) && (lbblockActionsna != null)) {
                lbBlock.Enabled = false;
                lbBlock.Visible = false;
                lbDelete.Enabled = false;
                lbDelete.Visible = false;

                lbblockActionsna.Enabled = true;
                lbblockActionsna.Visible = true;
            }
        }
    }
    protected void GV_WatchList_RowCommand(object sender, GridViewCommandEventArgs e) {
        NameValueCollection n = Request.ServerVariables;
        string ipaddress = n["REMOTE_ADDR"];
        if (ipaddress == "::1") {
            ipaddress = "127.0.0.1";
        }

        switch (e.CommandName) {
            case "block":
                bool blocked = true;
                if (_ipwatch.CheckIfBlocked(e.CommandArgument.ToString())) {
                    blocked = false;
                }
                else {
                    if ((e.CommandArgument.ToString() == ipaddress) && (_userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                        blocked = false;
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Cannot block current IP');");
                    }
                }
                _ipwatch.updateBlocked(e.CommandArgument.ToString(), blocked);

                if (blocked) {
                    MembershipUserCollection coll = Membership.GetAllUsers();
                    foreach (MembershipUser uColl in coll) {
                        MemberDatabase mb = new MemberDatabase(uColl.UserName);
                        if ((mb.IpAddress == e.CommandArgument.ToString()) && (uColl.IsOnline) && (uColl.UserName.ToLower() != _userId.Name.ToLower())) {
                            UserUpdateFlags uFlags = new UserUpdateFlags();
                            uFlags.addFlag(uColl.UserName, "", "");
                        }
                    }
                }

                _ipwatch = new IPWatch(true);
                LoadIpAddresses(ref GV_WatchList);
                LoadLoginActivity(ref gv_LoginActivity);
                break;
            case "delete":
                _ipwatch.deleteIP(e.CommandArgument.ToString());
                _ipwatch = new IPWatch(true);
                LoadIpAddresses(ref GV_WatchList);
                break;
        }
    }
    public void LoadIpAddresses(ref GridView gv) {
        DataView dvlist = GetList();
        gv.DataSource = dvlist;
        gv.DataBind();
    }
    public DataView GetList() {
        var dtlist = new DataTable();
        dtlist.Columns.Add(new DataColumn("ip"));
        dtlist.Columns.Add(new DataColumn("attempts"));
        dtlist.Columns.Add(new DataColumn("blocked"));
        dtlist.Columns.Add(new DataColumn("blockMessage"));
        dtlist.Columns.Add(new DataColumn("LastAttempt"));
        foreach (Dictionary<string, string> row in _ipwatch.ipwatchdt) {
            DataRow drlist = dtlist.NewRow();
            drlist["ip"] = row["IPAddress"].ToString();
            drlist["attempts"] = row["Attempts"].ToString();
            if (HelperMethods.ConvertBitToBoolean(row["Blocked"])) {
                drlist["blocked"] = "pad-all-sml RandomActionBtns margin-right img-ignore";
                drlist["blockMessage"] = "Blocked";
            }
            else {
                drlist["blocked"] = "pad-all-sml RandomActionBtns margin-right img-allow";
                drlist["blockMessage"] = "Active";
            }

            string lastAttempt = row["LastAttempt"].ToString();
            if (string.IsNullOrEmpty(lastAttempt)) {
                lastAttempt = "N/A";
            }

            drlist["LastAttempt"] = lastAttempt;

            dtlist.Rows.Add(drlist);
        }
        var dvlist = new DataView(dtlist);
        return dvlist;
    }
    protected void rb_AutoIPBlock_on_CheckedChanged(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            rb_AutoIPBlock_on.Checked = true;
            rb_AutoIPBlock_off.Checked = false;
            pnl_attemptsBeforeBlock.Enabled = true;
            pnl_attemptsBeforeBlock.Visible = true;
            ServerSettings.update_AutoBlockIP(true);
        }
    }
    protected void rb_AutoIPBlock_off_CheckedChanged(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            rb_AutoIPBlock_on.Checked = false;
            rb_AutoIPBlock_off.Checked = true;
            pnl_attemptsBeforeBlock.Enabled = false;
            pnl_attemptsBeforeBlock.Visible = false;
            ServerSettings.update_AutoBlockIP(false);
        }
    }
    protected void btn_autoblock_count_Clicked(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            int tempOut = 0;
            if (int.TryParse(tb_autoblock_count.Text, out tempOut)) {
                if (tempOut > 0)
                    ServerSettings.update_AutoBlockIPCount(tempOut);
                else
                    tb_autoblock_count.Text = _ss.AutoBlockIPCount.ToString();
            }
            else
                tb_autoblock_count.Text = _ss.AutoBlockIPCount.ToString();
        }
    }

    #endregion


    #region IP Listener

    private void BuildIpAddresses() {
        var listener = new IPListener(true);
        pnl_iplistener_holder.Controls.Clear();
        var str = new StringBuilder();
        int count = 1;

        str.Append("<div class=\"margin-top-sml\"><table class=\"myHeaderStyle\" style=\"width: 680px;\" cellpadding=\"0\" cellspacing=\"0\">");
        str.Append("<tr><td width=\"40px\"></td><td width=\"134px\">IP Address</td><td>Status/Actions</td><td width=\"135px\">Updated By</td><td width=\"170px\">Date Updated</td></tr></table></div>");

        bool hasOneActive = false;
        foreach (Dictionary<string, string> dr in listener.iplistener_table) {
            if (HelperMethods.ConvertBitToBoolean(dr["Active"])) {
                hasOneActive = true;
                break;
            }
        }

        foreach (Dictionary<string, string> dr in listener.iplistener_table) {
            var m = new MemberDatabase(dr["UpdatedBy"]);
            str.Append("<table class='myItemStyle GridNormalRow' style='width: 680px;' cellpadding='0' cellspacing='0'><tr>");
            str.Append("<td width='40px' class='GridViewNumRow' style='text-align: center;'>" + count.ToString() + "</td>");
            str.Append("<td width='135px' class='border-right' style='text-align: center;'>" + dr["IPAddress"] + "</td>");
            bool active = HelperMethods.ConvertBitToBoolean(dr["Active"]);
            if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                str.Append("<td class='border-right'><div class='pad-left-sml'>" + CreateRadioButtons_Listener(active, count, dr["IPAddress"], hasOneActive) + "</div></td>");
            }
            else {
                string activeText = "Listen";
                if (active && !hasOneActive) {
                    activeText = "Allow";
                }
                else {
                    activeText = "Blocked";
                }

                str.Append("<td class='border-right'><div class='pad-left-sml'>" + activeText + "</div></td>");
            }
            str.Append("<td width='135px' class='border-right' style='text-align: center;'>" + HelperMethods.MergeFMLNames(m) + "</td>");
            str.Append("<td width='170px' class='border-right' style='text-align: center;'>" + dr["Date"] + "</td>");
            str.Append("</tr></table>");
            count++;
        }
        if (listener.iplistener_table.Count == 0) {
            str.Append("<div class='emptyGridView' style='width: 665px;'>No Data Available</div>");
        }
        str.Append("<div class='clear-space'></div><div class='clear-space'></div>");

        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            str.Append(CreateNew_Listener());
        }
        pnl_iplistener_holder.Controls.Add(new LiteralControl(str.ToString()));
    }
    private void BuildIpAddresses_ViewOnly() {
        var listener = new IPListener(true);
        pnl_iplistener_holder.Controls.Clear();
        var str = new StringBuilder();
        int count = 1;

        str.Append("<div class=\"clear-margin\" style=\"font-size: 12px;\"><table class=\"myHeaderStyle\" style=\"width: 710px;\" cellpadding=\"0\" cellspacing=\"0\">");
        str.Append("<tr><td width=\"40px\"></td><td width=\"129px\">IP Address</td><td>Status</td><td width=\"135px\">Updated By</td><td width=\"170px\">Date Updated</td></tr></table></div>");

        bool hasOneActive = false;
        foreach (Dictionary<string, string> dr in listener.iplistener_table) {
            if (HelperMethods.ConvertBitToBoolean(dr["Active"])) {
                hasOneActive = true;
                break;
            }
        }

        foreach (Dictionary<string, string> dr in listener.iplistener_table) {
            var m = new MemberDatabase(dr["UpdatedBy"]);
            str.Append("<table class='myItemStyle GridNormalRow' style='width: 710px;' cellpadding='0' cellspacing='0'><tr>");
            str.Append("<td width='40px' class='GridViewNumRow' style='text-align: center;'>" + count.ToString() + "</td>");
            str.Append("<td width='130px' class='border-right'><div class='pad-left'>" + dr["IPAddress"] + "</div></td>");
            string activeText = "Listen";

            bool active = HelperMethods.ConvertBitToBoolean(dr["Active"]);
            if (active && !hasOneActive) {
                activeText = "Allowed";
            }
            else {
                activeText = "Blocked";
            }

            str.Append("<td class='border-right'><div class='pad-left'>" + activeText + "</div></td>");
            str.Append("<td width='135px' class='border-right'><div class='pad-left'>" + HelperMethods.MergeFMLNames(m) + "</div></td>");
            str.Append("<td width='170px' class='border-right'><div class='pad-left'>" + dr["Date"] + "</div></td>");
            str.Append("</tr></table>");
            count++;
        }
        if (listener.iplistener_table.Count == 0) {
            str.Append("<div class='emptyGridView' style='width: 695px;'>No Data Available</div>");
        }
        str.Append("<div class='clear-space'></div>");
        pnl_iplistener_holder.Controls.Add(new LiteralControl(str.ToString()));
    }
    private string CreateNew_Listener() {
        var str = new StringBuilder();
        NameValueCollection n = Request.ServerVariables;
        string ipaddress = n["REMOTE_ADDR"];
        if ((ipaddress == "::1") || (string.IsNullOrEmpty(ipaddress))) {
            ipaddress = "127.0.0.1";
        }
        str.Append("<div class='clear-margin'>");
        str.Append(
            "<table cellpadding='10' cellspacing='10'><tr><td align='right' style='width: 185px;'><span class='pad-right font-bold'>Add IP Address</span></td>");
        str.Append(
            "<td><input id='tb_createnew_listener' type='text' class='textEntry margin-right-big' style='width: 150px;'>");
        str.Append(
            "<input id='btn_createnew_listener' type='button' class='input-buttons margin-right-big RandomActionBtns' value='Add IP Address' />");
        str.Append(
            "<div class='clear-space-five'></div><small>Enter a new ip address for the site to listen for.</small></td></tr></table>");
        str.Append(
            "<div style='margin-left: 205px; margin-top: -10px;'><small><a href='#' onclick='SetToCurrentIP(\"" +
            ipaddress + "\");return false;'>Use Current IP Address</a></small></div>");
        str.Append("<div class='clear-space'></div></div>");
        return str.ToString();
    }
    private string CreateRadioButtons_Listener(bool active, int count, string ip, bool hasOneActive) {
        var str = new StringBuilder();
        str.Append("<div class='float-left'><div class='field switch'>");
        string enabledclass = "RandomActionBtns cb-enable";
        string disabledclass = "cb-disable selected";
        string onclickEnable = "onclick='UpdateActive(\"" + ip + "\", 1, this)'";
        string onclickDisable = "";
        if (active) {
            enabledclass = "cb-enable selected";
            disabledclass = "RandomActionBtns cb-disable";
            onclickEnable = "";
            onclickDisable = "onclick='UpdateActive(\"" + ip + "\", 0, this)'";
        }

        string listenText = "Allowed";
        if (hasOneActive) {
            listenText = "Blocked";
        }

        str.Append("<span class='" + enabledclass + "'><input id='rb_listener_active_" +
                   count.ToString(CultureInfo.InvariantCulture) +
                   "' type='radio' value='active' " + onclickEnable + " /><label for='rb_listener_active_" +
                   count.ToString(CultureInfo.InvariantCulture) + "'>Listen</label></span>");
        str.Append("<span class='" + disabledclass + "'><input id='rb_listener_deactive_" +
                   count.ToString(CultureInfo.InvariantCulture) +
                   "' type='radio' value='deactive' " + onclickDisable + " /><label for='rb_listener_deactive_" +
                   count.ToString(CultureInfo.InvariantCulture) + "'>" + listenText + "</label></span>");
        str.Append("</div></div><a href='#delete' class='td-delete-btn float-right margin-right-sml margin-top-sml' onclick='DeleteIP(\"" + ip + "\");return false;' title='Delete'></a>");
        return str.ToString();
    }
    protected void hf_UpdateIPListener_ValueChanged(object sender, EventArgs e) {
        BuildIpAddresses();
    }
    protected void btn_createnew_listener_Click(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(tb_createnew_listener.Text)) {
            if (Parse(tb_createnew_listener.Text)) {
                if (!_ipwatch.CheckIfExists(tb_createnew_listener.Text)) {
                    NameValueCollection n = Request.ServerVariables;
                    string ipaddress = n["REMOTE_ADDR"];
                    if (ipaddress == "::1") {
                        ipaddress = "127.0.0.1";
                    }

                    if ((tb_createnew_listener.Text == ipaddress) && (_userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                        _ipwatch.addItem(tb_createnew_listener.Text, 0, false);
                    }
                    else {
                        _ipwatch.addItem(tb_createnew_listener.Text, 0, true);
                    }
                    _ipwatch = new IPWatch(true);
                    LoadIpAddresses(ref GV_WatchList);
                    tb_createnew_listener.Text = string.Empty;
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('IP already exists');");
                }
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('IP address invalid');");
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('IP address invalid');");
        }
    }

    #endregion


    #region Settings

    private readonly Notifications _notifications = new Notifications();

    protected void cb_netactOn_CheckedChanged(object sender, EventArgs e) {
        cb_netactOff.Checked = false;
        cb_netactOn.Checked = true;
        ServerSettings.update_RecordActivity(true);

        pnl_RecordLogFile.Enabled = true;
        pnl_RecordLogFile.Visible = true;

        if (_ss.EmailActivity) {
            cb_emailon.Checked = true;
            cb_emailoff.Checked = false;
        }
        else {
            cb_emailon.Checked = false;
            cb_emailoff.Checked = true;
        }

        tableEmailAct.Style["display"] = "block";
        if (string.IsNullOrEmpty(_notifications.GetNotification("236a9dc9-c92a-437f-8825-27809af36a3f").ID)) {
            _notifications.AddNotification(_username, "Error Report", "~/Standard_Images/Notifications/error-lg-color.png", "Add alert when an error occurs on the site. (For Administrators only)", "236a9dc9-c92a-437f-8825-27809af36a3f");
        }
    }
    protected void cb_netactOff_CheckedChanged(object sender, EventArgs e) {
        cb_netactOff.Checked = true;
        cb_netactOn.Checked = false;

        pnl_RecordLogFile.Enabled = false;
        pnl_RecordLogFile.Visible = false;

        tableEmailAct.Style["display"] = "none";
        _notifications.DeleteNotification("236a9dc9-c92a-437f-8825-27809af36a3f");
        ServerSettings.update_RecordActivity(false);
    }

    protected void rb_DisableJavascriptErrorAlerts_on_CheckedChanged(object sender, EventArgs e) {
        rb_DisableJavascriptErrorAlerts_off.Checked = false;
        rb_DisableJavascriptErrorAlerts_on.Checked = true;
        ServerSettings.update_DisableJavascriptErrorAlerts(true);

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE_Config.reportOnError=false;");
    }
    protected void rb_DisableJavascriptErrorAlerts_off_CheckedChanged(object sender, EventArgs e) {
        rb_DisableJavascriptErrorAlerts_off.Checked = true;
        rb_DisableJavascriptErrorAlerts_on.Checked = false;
        ServerSettings.update_DisableJavascriptErrorAlerts(false);

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE_Config.reportOnError=true;");
    }

    protected void rb_RecordErrorsOnly_on_CheckedChanged(object sender, EventArgs e) {
        rb_RecordErrorsOnly_off.Checked = false;
        rb_RecordErrorsOnly_on.Checked = true;
        ServerSettings.update_RecordErrorsOnly(true);
    }
    protected void rb_RecordErrorsOnly_off_CheckedChanged(object sender, EventArgs e) {
        rb_RecordErrorsOnly_off.Checked = true;
        rb_RecordErrorsOnly_on.Checked = false;
        ServerSettings.update_RecordErrorsOnly(false);
    }

    protected void rb_recordLogFile_on_CheckedChanged(object sender, EventArgs e) {
        rb_recordLogFile_off.Checked = false;
        rb_recordLogFile_on.Checked = true;
        ServerSettings.update_RecordActivityToLogFile(true);
    }
    protected void rb_recordLogFile_off_CheckedChanged(object sender, EventArgs e) {
        rb_recordLogFile_off.Checked = true;
        rb_recordLogFile_on.Checked = false;
        ServerSettings.update_RecordActivityToLogFile(false);
    }

    protected void rb_recordLoginActivity_on_CheckedChanged(object sender, EventArgs e) {
        rb_recordLoginActivity_off.Checked = false;
        rb_recordLoginActivity_on.Checked = true;
        ServerSettings.update_RecordLoginActivity(true);
    }
    protected void rb_recordLoginActivity_off_CheckedChanged(object sender, EventArgs e) {
        rb_recordLoginActivity_off.Checked = true;
        rb_recordLoginActivity_on.Checked = false;
        ServerSettings.update_RecordLoginActivity(false);
    }

    protected void cb_emailon_CheckedChanged(object sender, EventArgs e) {
        cb_emailoff.Checked = false;
        cb_emailon.Checked = true;
        ServerSettings.update_EmailActivity(true);

        if (string.IsNullOrEmpty(_notifications.GetNotification("236a9dc9-c92a-437f-8825-27809af36a3f").ID)) {
            _notifications.AddNotification(_username, "Error Report", "~/Standard_Images/Notifications/error-lg-color.png", "Add alert when an error occurs on the site. (For Administrators only)", "236a9dc9-c92a-437f-8825-27809af36a3f");
        }
    }
    protected void cb_emailoff_CheckedChanged(object sender, EventArgs e) {
        cb_emailoff.Checked = true;
        cb_emailon.Checked = false;
        _notifications.DeleteNotification("236a9dc9-c92a-437f-8825-27809af36a3f");
        ServerSettings.update_EmailActivity(false);
    }

    #endregion

    protected void cb_ViewErrorsOnly_CheckedChanged(object sender, EventArgs e) {

    }

}