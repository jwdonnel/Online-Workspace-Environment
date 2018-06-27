using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.GroupOrganizer;
using System.Web.Configuration;

public partial class SiteTools_Analytics : BasePage {

    #region Private Variables

    private LoginActivity _la = new LoginActivity();
    private IPWatch _ipwatch = new IPWatch(true);

    #endregion


    #region Page Load Events

    protected void Page_Load(object sender, EventArgs e) {
        if (!IsPostBack) {
            BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);
        }

        hf_GoogleMapsAPIKey.Value = MainServerSettings.GoogleMapsApiKey;

        BuildPageViewsTable();
        BuildUsersToIgnoreSettings();
        LoadLoginActivity();

        RegisterPostbackScripts.RegisterStartupScript(this, "$('#imgPausePlay').removeClass('img-play');$('#imgPausePlay').addClass('img-pause');");

        if (!IsUserInAdminRole()) {
            networkSettings.Visible = false;
        }

        LoadSettings();
        GetCallBack();
        SetCommonStatistics();
    }
    private void GetCallBack() {
        switch (AsyncPostBackSourceElementID) {
            case "MainContent_lbtnClearAllLogin":
                _la.DeleteAllItems();
                LoadLoginActivity();
                break;
        }
    }
    private void LoadSettings() {
        if (MainServerSettings.RecordLoginActivity) {
            rb_recordLoginActivity_on.Checked = true;
            rb_recordLoginActivity_off.Checked = false;
        }
        else {
            rb_recordLoginActivity_on.Checked = false;
            rb_recordLoginActivity_off.Checked = true;
        }

        if (MainServerSettings.RecordPageViews) {
            rb_RecordPageViews_on.Checked = true;
            rb_RecordPageViews_off.Checked = false;
            pnl_individualpageviews.Visible = true;
            pnl_userstoignore_holder.Enabled = true;
            pnl_userstoignore_holder.Visible = true;
        }
        else {
            rb_RecordPageViews_on.Checked = false;
            rb_RecordPageViews_off.Checked = true;
            pnl_individualpageviews.Visible = false;
            pnl_userstoignore_holder.Enabled = false;
            pnl_userstoignore_holder.Visible = false;
        }

        if (MainServerSettings.RecordSiteRequests) {
            rb_recordSiteRequests_on.Checked = true;
            rb_recordSiteRequests_off.Checked = false;
            pnl_MaxRequestSize.Enabled = true;
            pnl_MaxRequestSize.Visible = true;
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#MainContent_pnl_interactivityholder, #ChartRequests, #pnl_interactivityholder_Description').show();");
        }
        else {
            rb_recordSiteRequests_on.Checked = false;
            rb_recordSiteRequests_off.Checked = true;
            pnl_MaxRequestSize.Enabled = false;
            pnl_MaxRequestSize.Visible = false;
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#MainContent_pnl_interactivityholder, #ChartRequests, #pnl_interactivityholder_Description').hide();");
        }

        if (!AsyncPostBackSourceElementID.Contains("btn_requestRecordSize")) {
            tb_maxRequestSize.Text = MainServerSettings.MaxRequestRecordSize.ToString();
        }
    }

    #endregion


    #region Click/Change Events

    protected void hf_resetPageCount_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_resetPageCount.Value)) {
            pageViews.DeleteItemsByPageName(hf_resetPageCount.Value);
        }

        hf_resetPageCount.Value = string.Empty;
        BuildPageViewsTable();
    }
    protected void lbtn_refreshLoginactivity_Click(object sender, EventArgs e) {
        LoadLoginActivity();
    }

    #endregion


    #region Common Statistics

    private void SetCommonStatistics() {
        lbl_DaysToTrackCommonStatistics.Text = WebConfigurationManager.AppSettings["DaysToTrackCommonStatistics"];

        int daysToTrack = 5;
        int.TryParse(lbl_DaysToTrackCommonStatistics.Text, out daysToTrack);

        LoginActivity tempLoginActivity = new LoginActivity();
        tempLoginActivity.GetActivity();

        int guestCount = 0;
        int loginCount = 0;
        int registeredUsers = 0;
        TimeSpan timeSpan;
        List<string> guestUsers = new List<string>();
        List<string> loginUsers = new List<string>();

        foreach (LoginActivity_Coll coll in tempLoginActivity.ActivityList) {
            timeSpan = ServerSettings.ServerDateTime.Subtract(coll.DateAdded);
            if (coll.ActType == ActivityType.Guest && coll.IsSuccessful) {
                if (timeSpan.Days <= daysToTrack && !guestUsers.Contains(coll.IpAddress)) {
                    guestCount++;
                    guestUsers.Add(coll.IpAddress);
                }
            }
            if ((coll.ActType == ActivityType.Login || coll.ActType == ActivityType.Social) && coll.IsSuccessful) {
                if (timeSpan.Days <= daysToTrack && !loginUsers.Contains(coll.UserName.ToLower())) {
                    loginCount++;
                    loginUsers.Add(coll.UserName.ToLower());
                }
            }
        }

        MembershipUserCollection userCollection = Membership.GetAllUsers();
        foreach (MembershipUser user in userCollection) {
            timeSpan = ServerSettings.ServerDateTime.Subtract(user.CreationDate);
            if (timeSpan.Days <= daysToTrack) {
                registeredUsers++;
            }
        }

        lbl_NewVisitors_Count.Text = guestCount.ToString();
        lbl_RecentLogins_Count.Text = loginCount.ToString();
        lbl_RegisteredUsers_Count.Text = registeredUsers.ToString();
    }

    #endregion


    #region Network Activity

    private readonly PageViews pageViews = new PageViews();
    private readonly PageViewsUsersToIgnore pageViewsUsersToIgnore = new PageViewsUsersToIgnore();
    private void BuildPageViewsTable() {
        Dictionary<string, PageViewsCount_Coll> pageCounts = pageViews.GetCountForEachPage();
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, IsUserInAdminRole(), 2, "PageViewsTable_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Page Name", "200", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Page Url", "300", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Views", "75", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        foreach (KeyValuePair<string, PageViewsCount_Coll> coll in pageCounts) {
            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Page Name", coll.Value.PageName, TableBuilderColumnAlignment.Left, "data-pageviewname='" + HttpUtility.UrlEncode(coll.Value.PageName).Replace("+", " ") + "'", "page-view-name"));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Page Url", coll.Value.PageUrl, TableBuilderColumnAlignment.Left, string.Empty, "page-url-column"));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Views", coll.Value.Count.ToString(), TableBuilderColumnAlignment.Left, "data-pageviewcount='" + HttpUtility.UrlEncode(coll.Value.PageName).Replace("+", " ").ToLower() + "'", "page-view-count"));

            string editButtons = "<a href='javascript:void(0);' class='td-details-btn' title='View Details' onclick=\"ViewPageDetails('" + coll.Key + "', '" + HttpUtility.UrlEncode(coll.Value.PageName) + "');return false;\"></a>";
            editButtons += "<a href='javascript:void(0);' class='td-reset-btn' title='Reset page view count' onclick=\"ResetPageViewCount('" + coll.Key + "');return false;\"></a>";

            tableBuilder.AddBodyRow(bodyColumnValues, editButtons);
        }
        #endregion

        pnl_individualPageRequests.Controls.Clear();
        pnl_individualPageRequests.Controls.Add(tableBuilder.CompleteTableLiteralControl("No data available"));
    }
    private void BuildUsersToIgnoreSettings() {
        if (IsUserInAdminRole()) {
            List<PageViewsToIgnore_Coll> usersToIgnore = pageViewsUsersToIgnore.GetListOfUsersToIgnore();

            StringBuilder str = new StringBuilder();

            foreach (PageViewsToIgnore_Coll item in usersToIgnore) {
                string encodedUsername = HttpUtility.UrlEncode(item.Username);
                MembershipUser mu = Membership.GetUser(item.Username);
                if (mu != null) {
                    MemberDatabase tempMember = new MemberDatabase(mu.UserName);
                    str.Append("<div class='usertoignore-item'>");
                    str.Append("<div class='float-left pad-all-sml margin-right'><h4 class='font-bold'>" + item.Username + "</h4><div class='clear'></div><small>" + HelperMethods.MergeFMLNames(tempMember) + "</small><div class='clear'></div></div>");
                    str.Append("<a href='#' class='td-delete-btn float-left' title='Delete User' onclick=\"DeleteUserToIgnore('" + encodedUsername + "');return false;\"></a>");
                    str.Append("<div class='clear'></div>");
                    str.Append("</div>");
                }
            }

            pnl_userstoignore.Controls.Clear();
            pnl_userstoignore.Controls.Add(new LiteralControl(str.ToString()));
        }
        else {
            pnl_userstoignore_holder.Enabled = false;
            pnl_userstoignore_holder.Visible = false;
        }
    }

    protected void btn_AddUserToIgnore_Clicked(object sender, EventArgs e) {
        string val = tb_AddUserToIgnore.Text.Trim().ToLower();
        if (!string.IsNullOrEmpty(val)) {
            bool canAdd = true;
            List<PageViewsToIgnore_Coll> coll = pageViewsUsersToIgnore.GetListOfUsersToIgnore();
            foreach (PageViewsToIgnore_Coll item in coll) {
                if (item.Username.ToLower() == val) {
                    canAdd = false;
                    break;
                }
            }

            if (canAdd) {
                MembershipUser mu = Membership.GetUser(val);
                if (mu != null) {
                    pageViewsUsersToIgnore.AddItem(val, CurrentUsername);
                    tb_AddUserToIgnore.Text = string.Empty;
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Username does not exists. Please try again.');");
                }
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Username already exists. Please enter a new username.');");
            }
        }

        BuildUsersToIgnoreSettings();
    }
    protected void btn_refreshPageViews_Click(object sender, EventArgs e) {
        BuildPageViewsTable();
    }

    #endregion


    #region Login Activity

    public void LoadLoginActivity() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, IsUserInAdminRole(), 2, "LoginActivityTable_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Login Information", "", false));
        tableBuilder.AddHeaderRow(headerColumns, false);
        #endregion

        #region Build Body
        if (IsUserInAdminRole()) {
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
            string ipLink = "<a href=\"#\" onclick=\"SearchLoginIp('" + act.IpAddress + "');return false;\">" + act.IpAddress + "</a>";
            if (act.IpAddress == "127.0.0.1") {
                ipLink = act.IpAddress;
            }

            string ipDiv = string.Format("<div class=\"login-entry-holder ipaddress-login-entry\"><div class=\"login-entry-title\">IP Address</div><div class=\"login-entry-text\"><span class=\"ipsearchval\" style=\"display: none;\">{0}</span>{1}</div></div>", act.IpAddress, ipLink);
            string nameUsedDiv = string.Format("<div class=\"login-entry-holder nameused-login-entry\"><div class=\"login-entry-title\">Name Used</div><div class=\"login-entry-text\">{0}</div></div>", act.UserName);
            string actTypeDiv = string.Format("<div class=\"login-entry-holder type-login-entry\"><div class=\"login-entry-title\">Type</div><div class=\"login-entry-text\">{0}</div></div>", act.ActType.ToString());
            string validDiv = "<div class=\"login-entry-holder valid-login-entry\"><div class=\"login-entry-title\">Valid</div><div class=\"login-entry-text\">{0}</div></div>";
            string dateDiv = string.Format("<div class=\"login-entry-holder date-login-entry\"><div class=\"login-entry-title\">Date</div><div class=\"login-entry-text\">{0}</div></div>", act.DateAdded.ToString());
            string eventMessageDiv = "<div class=\"login-entry-holder message-login-entry\"><div class=\"login-entry-title\">Event Message</div><div class=\"login-entry-text\">{0}</div></div>";
            string httpRefererDiv = "<div class=\"login-entry-holder httpreferer-login-entry\"><div class=\"login-entry-title\">Http Referer</div><div class=\"login-entry-text http-referer-text\">{0}</div></div>";

            string allowBlockBtn = string.Empty;
            string deleteBtn = string.Empty;

            #region Build Valid Image and Message
            if (act.IsSuccessful) {
                string messageStr = string.Empty;
                if (act.ActType == ActivityType.Login || act.ActType == ActivityType.Social) {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        messageStr = act.UserName + " logged in successfully from " + act.IpAddress;
                    }
                    else {
                        messageStr = act.UserName + " logged in successfully to the Group " + GetGroupName(drGroups, act.LoginGroup) + " from " + act.IpAddress;
                    }
                }
                else if (act.ActType == ActivityType.Guest) {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        messageStr = act.UserName + " visited site from " + act.IpAddress;
                    }
                    else {
                        messageStr = act.UserName + " visited site for Group " + GetGroupName(drGroups, act.LoginGroup) + " from " + act.IpAddress;
                    }
                }
                else {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        messageStr = act.UserName + " logged out from " + act.IpAddress;
                    }
                    else {
                        messageStr = act.UserName + " logged out of the Group " + GetGroupName(drGroups, act.LoginGroup) + " from " + act.IpAddress;
                    }
                }

                validDiv = string.Format(validDiv, "<span class='img-checkmark'></span>");
                if (!string.IsNullOrEmpty(messageStr)) {
                    eventMessageDiv = string.Format(eventMessageDiv, messageStr);
                }
            }
            else {
                string messageStr = string.Empty;
                if (act.ActType == ActivityType.Login || act.ActType == ActivityType.Social) {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        messageStr = act.IpAddress + " attempted to login using the name " + act.UserName + ". Login Failed.";
                    }
                    else {
                        messageStr = act.IpAddress + " attempted to login to the Group " + GetGroupName(drGroups, act.LoginGroup) + " using the name " + act.UserName + ". Login Failed.";
                    }
                }
                else if (act.ActType == ActivityType.Guest) {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        messageStr = act.UserName + " was not able to visit site from " + act.IpAddress;
                    }
                    else {
                        messageStr = act.UserName + " was not able to visit site for Group " + GetGroupName(drGroups, act.LoginGroup) + " from " + act.IpAddress;
                    }
                }
                else {
                    if (string.IsNullOrEmpty(act.LoginGroup)) {
                        messageStr = act.IpAddress + " attempted to log out using the name " + act.UserName + ". Login Failed.";
                    }
                    else {
                        messageStr = act.IpAddress + " attempted to log out from the Group " + GetGroupName(drGroups, act.LoginGroup) + " using the name " + act.UserName + ". Login Failed.";
                    }
                }

                validDiv = string.Format(validDiv, "<span class='img-xmark'></span>");
                if (!string.IsNullOrEmpty(messageStr)) {
                    eventMessageDiv = string.Format(eventMessageDiv, messageStr);
                }
            }
            #endregion

            #region Build HttpRefererDiv
            string httpRefererStr = "Unknown";
            if (!string.IsNullOrEmpty(act.HttpReferer)) {
                if (!act.HttpReferer.EndsWith("...")) {
                    httpRefererStr = "<a href='" + act.HttpReferer + "' target='_blank'>" + act.HttpReferer + "</a>";
                }
                else {
                    httpRefererStr = act.HttpReferer;
                }
            }

            httpRefererDiv = string.Format(httpRefererDiv, httpRefererStr);
            #endregion

            #region Create Buttons
            if (IsUserInAdminRole()) {
                deleteBtn = "<a href='#Delete' class='td-delete-btn' onclick=\"DeleteLoginActivity('" + act.ID + "');return false;\" title='Delete Login Event'></a>";

                _ipwatch = new IPWatch(true);
                bool found = false;
                foreach (Dictionary<string, string> dr in _ipwatch.ipwatchdt) {
                    if (dr["IPAddress"] == act.IpAddress) {
                        if (HelperMethods.ConvertBitToBoolean(dr["Blocked"])) {
                            allowBlockBtn = "<a href='#allow' class='td-ignore-btn RandomActionBtns' onclick=\"AllowBlockLoginEvent('" + act.ID + "');return false;\" title='Click to allow IP Address'></a>";
                        }
                        else {
                            allowBlockBtn = "<a href='#block' class='td-allow-btn RandomActionBtns' onclick=\"AllowBlockLoginEvent('" + act.ID + "');return false;\" title='Click to block IP Address'></a>";
                        }

                        found = true;
                        break;
                    }
                }

                if (!found) {
                    allowBlockBtn = "<a href='#allow' class='td-allow-btn RandomActionBtns' onclick=\"AllowBlockLoginEvent('" + act.ID + "');return false;\" title='Click to block IP Address'></a>";
                }
            }
            #endregion

            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Login Information", ipDiv + nameUsedDiv + actTypeDiv + validDiv + dateDiv + eventMessageDiv + httpRefererDiv, TableBuilderColumnAlignment.Left));
            tableBuilder.AddBodyRow(bodyColumnValues, allowBlockBtn + deleteBtn, string.Empty, "ipsearchrow");
        }
        #endregion

        pnl_loginactivity.Controls.Clear();
        pnl_loginactivity.Controls.Add(tableBuilder.CompleteTableLiteralControl("No login activity available"));

        string ctrlPostback = ScriptManager.GetCurrent(this.Page).AsyncPostBackSourceElementID;
        if (!ctrlPostback.Contains("btn_daystokeepLoginActivity")) {
            if (MainServerSettings.DeleteOldLoginActivity) {
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

            tb_daystokeepLoginActivity.Text = MainServerSettings.LoginActivityToKeepInDays.ToString();
        }
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
        LoadLoginActivity();
    }
    protected void hf_AllowBlockLoginEvent_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_AllowBlockLoginEvent.Value)) {
            LoginActivity_Coll act = _la.GetActivity(hf_AllowBlockLoginEvent.Value);

            if (!string.IsNullOrEmpty(act.ID)) {
                _ipwatch = new IPWatch(true);
                bool found = false;
                foreach (Dictionary<string, string> dr in _ipwatch.ipwatchdt) {
                    if (dr["IPAddress"].ToString() == act.IpAddress) {
                        if (HelperMethods.ConvertBitToBoolean(dr["Blocked"])) {
                            _ipwatch.updateBlocked(act.IpAddress, false);
                        }
                        else {
                            if (act.IpAddress == CurrentIpAddress && !IsUserNameEqualToAdmin()) {
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
                    if (act.IpAddress == CurrentIpAddress && !IsUserNameEqualToAdmin()) {
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
        LoadLoginActivity();
    }

    protected void rb_autoDeleteLoginActivity_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_autoDeleteLoginActivity_on.Checked = true;
            rb_autoDeleteLoginActivity_off.Checked = false;
            pnl_autoDeleteLoginActivity_days.Enabled = true;
            pnl_autoDeleteLoginActivity_days.Visible = true;
            ServerSettings.update_DeleteOldLoginActivity(true);
        }

        LoadLoginActivity();
    }
    protected void rb_autoDeleteLoginActivity_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_autoDeleteLoginActivity_on.Checked = false;
            rb_autoDeleteLoginActivity_off.Checked = true;
            pnl_autoDeleteLoginActivity_days.Enabled = false;
            pnl_autoDeleteLoginActivity_days.Visible = false;
            ServerSettings.update_DeleteOldLoginActivity(false);
        }

        LoadLoginActivity();
    }

    protected void btn_daystokeepLoginActivity_Clicked(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            string daysToKeep = tb_daystokeepLoginActivity.Text.Trim();
            int daysToKeepInt = 0;
            int.TryParse(daysToKeep, out daysToKeepInt);
            if (daysToKeepInt <= 0) {
                daysToKeepInt = 1;
            }

            ServerSettings.Update_LoginActivityToKeepInDays(daysToKeepInt);
            tb_daystokeepLoginActivity.Text = daysToKeepInt.ToString();
        }

        LoadLoginActivity();
    }

    #endregion


    #region Settings

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

    protected void hf_DeleteUserToIgnore_ValueChanged(object sender, EventArgs e) {
        string val = hf_DeleteUserToIgnore.Value.Trim().ToLower();
        if (!string.IsNullOrEmpty(val)) {
            pageViewsUsersToIgnore.DeleteItemsByUsername(HttpUtility.UrlDecode(val));
        }

        BuildUsersToIgnoreSettings();
    }

    protected void rb_RecordPageViews_on_CheckedChanged(object sender, EventArgs e) {
        rb_RecordPageViews_on.Checked = true;
        rb_RecordPageViews_off.Checked = false;
        pnl_individualpageviews.Visible = true;
        pnl_userstoignore_holder.Enabled = true;
        pnl_userstoignore_holder.Visible = true;
        ServerSettings.update_RecordPageViews(true);

        BuildPageViewsTable();
        BuildUsersToIgnoreSettings();
    }
    protected void rb_RecordPageViews_off_CheckedChanged(object sender, EventArgs e) {
        rb_RecordPageViews_on.Checked = false;
        rb_RecordPageViews_off.Checked = true;
        pnl_individualpageviews.Visible = false;
        pnl_userstoignore_holder.Enabled = false;
        pnl_userstoignore_holder.Visible = false;
        ServerSettings.update_RecordPageViews(false);

        BuildPageViewsTable();
        BuildUsersToIgnoreSettings();
    }

    protected void rb_recordSiteRequests_on_CheckedChanged(object sender, EventArgs e) {
        rb_recordSiteRequests_on.Checked = true;
        rb_recordSiteRequests_off.Checked = false;
        pnl_MaxRequestSize.Enabled = true;
        pnl_MaxRequestSize.Visible = true;
        ServerSettings.update_RecordSiteRequests(true);
        GetSiteRequests.SetEnableSiteRequestTracking(true);

        RegisterPostbackScripts.RegisterStartupScript(this, "$('#MainContent_pnl_interactivityholder, #ChartRequests, #pnl_interactivityholder_Description').show();");
    }
    protected void rb_recordSiteRequests_off_CheckedChanged(object sender, EventArgs e) {
        rb_recordSiteRequests_on.Checked = false;
        rb_recordSiteRequests_off.Checked = true;
        pnl_MaxRequestSize.Enabled = false;
        pnl_MaxRequestSize.Visible = false;
        ServerSettings.update_RecordSiteRequests(false);
        GetSiteRequests.SetEnableSiteRequestTracking(false);

        RegisterPostbackScripts.RegisterStartupScript(this, "$('#MainContent_pnl_interactivityholder, #ChartRequests, #pnl_interactivityholder_Description').hide();");
    }

    protected void btn_requestRecordSize_Clicked(object sender, EventArgs e) {
        string maxSize = tb_maxRequestSize.Text;
        int outVal = 4000;
        if (!int.TryParse(maxSize, out outVal)) {
            outVal = 4000;
        }

        if (outVal < 1 || outVal > 5000) {
            outVal = 4000;
        }

        ServerSettings.update_MaxRequestRecordSize(outVal);
        GetSiteRequests.SetMaxSiteRequestSize(outVal);

        tb_maxRequestSize.Text = outVal.ToString();
    }

    #endregion

}