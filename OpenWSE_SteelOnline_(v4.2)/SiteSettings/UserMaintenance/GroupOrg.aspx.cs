#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;
using System.Collections.Specialized;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Notifications;

#endregion

public partial class SiteSettings_GroupOrg : Page {

    private const string _userWidth = "165px";
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private Groups _groups;
    private string _username;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                _username = userId.Name;
                _groups = new Groups(_username);

                if (Session["LoginGroup"] != null) {
                    pnl_addgroupbtn.Enabled = false;
                    pnl_addgroupbtn.Visible = false;
                }

                ScriptManager sm = ScriptManager.GetCurrent(Page);
                if (sm != null) {
                    string ctlId = sm.AsyncPostBackSourceElementID;

                    if ((!IsPostBack) || (ctlId == "hf_UpdateAll")) {
                        BuildGroupList();

                        // Reload the group add and remove modal
                        if (Session["ReloadGroupModal"] != null && !string.IsNullOrEmpty(Session["ReloadGroupModal"].ToString())) {
                            RegisterPostbackScripts.RegisterStartupScript(this, "try { ViewGroup('" + Session["ReloadGroupModal"].ToString() + "'); }catch(evt) { }");

                            Session["ReloadGroupModal"] = null;
                            Session.Remove("ReloadGroupModal");
                        }
                    }
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }
    protected void imgbtn_search_Click(object sender, EventArgs e) {
        BuildGroupList();
    }
    protected void hf_clearsearch_Changed(object sender, EventArgs e) {
        BuildGroupList();
    }

    #region Group builder methods

    private void BuildGroupList() {
        pnl_companyholder.Controls.Clear();
        _groups.getEntries();
        var str = new StringBuilder();
        int count = 0;
        var member = new MemberDatabase(_username);
        int groupCount = member.GroupList.Count;

        List<string> groupList = member.GroupList;

        foreach (Dictionary<string, string> dr in _groups.group_dt) {
            bool canContinue = true;
            StringBuilder strTemp = new StringBuilder();

            string imgurl = HelperMethods.ConvertBitToBoolean(dr["IsURL"]) ? dr["Image"] : ResolveUrl("~/Standard_Images/Groups/Logo/" + dr["Image"]);
            if (imgurl.StartsWith("~/")) {
                imgurl = ResolveUrl(imgurl);
            }

            strTemp.Append("<div class='contact-card-main contact-card-main-category-packages'><img alt='logo' src='" + imgurl + "'/ >");

            if (dr["CreatedBy"].ToLower() == _username.ToLower() || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                strTemp.Append("<a href='#delete' onclick='DeleteGroup(\"" + dr["GroupID"] + "\");return false;' class='td-delete-btn float-right margin-left-sml margin-right-sml' title='Delete Group'></a>");
                strTemp.Append("<a href='#edit' onclick='EditGroup(\"" + dr["GroupID"] + "\");return false;' class='td-edit-btn float-right margin-left-sml margin-right-sml' title='Edit Group'></a>");
            }

            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (groupList.Contains(dr["GroupID"])) {
                    if (groupCount > 1) {
                        strTemp.Append("<a href='#users' title='Remove yourself from this group' onclick='RemoveUser(\"" + _username + "\", \"" + dr["GroupID"] + "\");return false;' class='joinquit td-cancel-btn float-right margin-left-sml margin-right-sml'></a>");
                    }
                }
                else {
                    if ((CompareCreatedByGroup(_username, dr["CreatedBy"])) || (groupList.Contains(dr["GroupID"]))) {
                        strTemp.Append("<a href='#users' title='Join this group'  onclick='AddUser(\"" + _username + "\", \"" + dr["GroupID"] + "\");return false;' class='joinquit td-add-btn float-right margin-left-sml margin-right-sml'></a>");
                    }
                    else
                        canContinue = false;
                }
                strTemp.Append("<a href='#users' title='View users in group (Non-Editable)' onclick='ViewGroup_Standard(\"" + dr["GroupID"] + "\");return false;' class='img-users float-right margin-left-sml margin-right-sml pad-all-sml'></a>");
            }
            else {
                strTemp.Append("<a href='#users' title='View users in group (Non-Editable)' onclick='ViewGroup_Standard(\"" + dr["GroupID"].ToString() + "\");return false;' class='img-users float-right margin-right pad-all-sml'></a>");
                strTemp.Append("<a href='#users' title='Add/Remove users from group' onclick='ViewGroup(\"" + dr["GroupID"].ToString() + "\");return false;' class='img-addremove float-right margin-right pad-all-sml'></a>");
            }

            strTemp.Append("<a href='#network' title='Setup your Group to listen to certain IP Addresses' onclick='GroupNetwork(\"" + dr["GroupID"] + "\");return false;' class='img-network float-right margin-right pad-all-sml'></a>");

            if (HelperMethods.ConvertBitToBoolean((dr["IsPrivate"])) && (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) && (!groupList.Contains(dr["GroupID"]))) {
                canContinue = false;
            }
            else if (dr["CreatedBy"].ToLower() == _username.ToLower() || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                strTemp.Append("<a href='#invite' title='Invite user(s) from group' onclick='InviteToGroup(\"" + dr["GroupID"] + "\");return false;' class='img-share float-right margin-right pad-all-sml'></a>");
            }

            if (canContinue) {

                string searchText = tb_search.Text.Trim().ToLower();
                if (!dr["GroupName"].ToLower().Contains(searchText) && !string.IsNullOrEmpty(searchText) && searchText != "search groups") {
                    continue;
                }

                strTemp.Append("<div class='clear-space-two'></div>");
                strTemp.Append("<div class='clear'><h2 style='padding-top:15px!important;'>" + dr["GroupName"] + "</h2></div>");
                strTemp.Append("<div class='clear-space-two'></div>");

                if (!string.IsNullOrEmpty(dr["Address"]))
                    strTemp.Append("<p><b>Address:</b>" + dr["Address"] + "</p>");

                if ((!string.IsNullOrEmpty(dr["City"])) && (!string.IsNullOrEmpty(dr["State"])) && (!string.IsNullOrEmpty(dr["PostalCode"])))
                    strTemp.Append("<p><b>City:</b>" + dr["City"] + "<b class='pad-left-big'>State:</b>" + dr["State"] + "<b class='pad-left-big'>Postal Code:</b>" + dr["PostalCode"].ToString() + "</p>");

                if ((!string.IsNullOrEmpty(dr["PhoneNumber"])) && (dr["PhoneNumber"] != "--"))
                    strTemp.Append("<p><b>Phone Number:</b>" + dr["PhoneNumber"] + "</p>");

                strTemp.Append("<p><b>Date Created:</b>" + dr["Date"] + "</p>");
                strTemp.Append("<div class='clear'></div>");
                strTemp.Append("<p><b>Created By:</b>" + dr["CreatedBy"] + "</p>");
                strTemp.Append("<div class='clear'></div>");
                strTemp.Append("<p><b>Is Private:</b>" + (HelperMethods.ConvertBitToBoolean(dr["IsPrivate"])).ToString() + "</p>");
                strTemp.Append("<div class='clear'></div>");
                strTemp.Append("<p><b>Group Network:</b>" + CheckIfListening(dr["GroupID"]) + "</p>");

                strTemp.Append("<div class='clear-space'></div>");
                if (_groups.IsApartOfGroup(groupList, dr["GroupID"])) {
                    if ((Session["LoginGroup"] != null) && (dr["GroupID"] == Session["LoginGroup"].ToString())) {
                        strTemp.Append("<p class='float-left'><b>Login Query:</b><a href='#' title='Click to logout of " + dr["GroupName"] + "' onclick='LogoutOfGroup(); return false;'>Logout of group</a></p>");
                    }
                    else {
                        strTemp.Append("<p class='float-left'><b>Login Query:</b><a href='#' title='Click to login to " + dr["GroupName"] + "' onclick='LoginToGroup(\"" + dr["GroupID"] + "\"); return false;'>Default.aspx?group=" + dr["GroupID"] + "</a></p>");
                    }
                }
                else {
                    strTemp.Append("<p class='float-left'><b>Login Query:</b>Default.aspx?group=" + dr["GroupID"] + "</p>");
                }
                strTemp.Append("<p class='float-right'><b>Total Users:</b>" + (CountUsersPerGroup(dr["GroupID"])).ToString() + "</p>");
                strTemp.Append("<div class='clear-space-two'></div></div>");

                str.Append(strTemp.ToString());
                count++;
            }
        }

        if (_username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            StringBuilder _strScriptreg = new StringBuilder();
            if (count == 0) {
                _strScriptreg.Append("$('#aGroupLogin').css('display', 'none');$(\"body\").append(\"<h3 id='noapartofgroupmessage' style='position: fixed; bottom: 28px; right: 0; z-index: 10000; padding: 15px; background: #2F2F2F; color: #FFF;'>You must add yourself to a group</h3>\");");
            }
            else {
                _strScriptreg.Append("$('#aGroupLogin').css('display', '');$(\"#noapartofgroupmessage\").remove();");
            }

            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
        }

        if (count == 0) {
            str.Append("<h3 class='pad-all'>No groups found</h3>");
        }

        lbl_companycount.Text = count.ToString(CultureInfo.InvariantCulture);
        pnl_companyholder.Controls.Add(new LiteralControl(str.ToString()));
        updatepnl_header.Update();
    }
    private bool CompareCreatedByGroup(string currUser, string createdby) {
        Groups groups = new Groups(currUser);
        MemberDatabase m = new MemberDatabase(createdby);
        MemberDatabase m2 = new MemberDatabase(currUser);
        List<string> gList = m.GroupList;
        List<string> gList2 = m2.GroupList;
        foreach (string g in gList) {
            if (groups.IsApartOfGroup(gList2, g))
                return true;
        }

        return false;
    }
    private void BuildEditBox(List<Dictionary<string, string>> drc) {
        try {
            tb_companyname.Text = drc[0]["GroupName"];
            lbl_tempcompanyname.Text = drc[0]["GroupID"];
        }
        catch {
        }

        try {
            string[] phonenumber = drc[0]["PhoneNumber"].Split('-');
            company_phone1.Value = phonenumber[0];
            company_phone2.Value = phonenumber[1];
            company_phone3.Value = phonenumber[2];
        }
        catch {
        }

        try {
            tb_address.Text = drc[0]["Address"];
        }
        catch {
        }

        try {
            tb_city.Text = drc[0]["City"];
        }
        catch {
        }

        try {
            string state = drc[0]["State"];
            int count = dd_state.Items.Cast<ListItem>().TakeWhile(item => item.Text != state).Count();

            dd_state.SelectedIndex = count;
        }
        catch {
        }

        try {
            tb_postalcode.Text = drc[0]["PostalCode"];
        }
        catch {
        }

        try {
            if (HelperMethods.ConvertBitToBoolean(drc[0]["IsURL"])) {
                img_logo.Style["display"] = "block";
                string imgurl = drc[0]["Image"];
                if (imgurl.StartsWith("~/")) {
                    imgurl = ResolveUrl(imgurl);
                }
                tb_imageurl.Text = drc[0]["Image"];
                img_logo.ImageUrl = imgurl;
            }
            else if (!string.IsNullOrEmpty(drc[0]["Image"])) {
                img_logo.Style["display"] = "block";
                img_logo.ImageUrl = ResolveUrl("~/Standard_Images/Groups/Logo/" + drc[0]["Image"]);
            }
        }
        catch {
        }

        try {
            cb_isprivate.Checked = false;
            if (HelperMethods.ConvertBitToBoolean(drc[0]["IsPrivate"])) {
                cb_isprivate.Checked = true;
            }
        }
        catch { }

        updatepnl_editmode_1.Update();
        updatepnl_editmode_2.Update();
    }
    private string CheckIfListening(string groupId) {
        GroupIPListener groupIp = new GroupIPListener();
        if (groupIp.HasAtLeastOneActive(groupId)) {
            return "On";
        }

        return "Off";
    }

    #endregion


    #region Group Network

    private string CurrentGroupIdNetwork {
        get {
            if (ViewState["GroupNetworkId"] != null) {
                return ViewState["GroupNetworkId"].ToString();
            }

            return string.Empty;
        }
        set {
            ViewState["GroupNetworkId"] = value;
        }
    }
    protected void hf_groupNetwork_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_groupNetwork.Value)) {
            CurrentGroupIdNetwork = hf_groupNetwork.Value;
            imgGroupNetwork.ImageUrl = _groups.GetGroupImg_byID(CurrentGroupIdNetwork);
            lblGroupNetworkName.Text = _groups.GetGroupName_byID(CurrentGroupIdNetwork);
            BuildIpAddresses();
            updatepnl_groupNetwork.Update();
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'GroupNetwork-element', 'Group Network Listener');");
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('No Group Id specified.');");
        }

        hf_groupNetwork.Value = "";
    }
    protected void hf_groupNetwork_Update_ValueChanged(object sender, EventArgs e) {
        string currOwner = _groups.GetOwner(CurrentGroupIdNetwork);
        if (((currOwner.ToLower() == _username.ToLower()) && (!string.IsNullOrEmpty(hf_groupNetwork_Update.Value))) || (_username.ToLower() == ServerSettings.AdminUserName.ToLower())) {
            var listener = new GroupIPListener();
            string groupId = listener.GetGroupIdFromId(hf_groupNetwork_Update.Value);
            listener.GetGroupIPs(groupId);
            string ip = listener.GetIPAddress(hf_groupNetwork_Update.Value);
            bool isActive = listener.IpIsActive(hf_groupNetwork_Update.Value);
            if (isActive) {
                if ((HttpContext.Current.Session["LoginGroup"] == null) || (listener.GroupIPListenerColl.Count == 1) || (listener.GroupIPListenerColl.Count > 1 && !listener.HasAtLeastOneActive(groupId, CurrentAddress)) || (ip != CurrentAddress && listener.GroupIPListenerColl.Count > 1 && listener.HasAtLeastOneActive(groupId))) {
                    listener.UpdateRow(hf_groupNetwork_Update.Value, false);
                    ipMessage.InnerHtml = "<span style='color: Green'>IP address updated</span>";
                }
                else {
                    ipMessage.InnerHtml = "<span style='color: Red'>Cannot disable current IP</span>";
                }
            }
            else {
                if (ip == CurrentAddress || CurrentIPActive(groupId)) {
                    listener.UpdateRow(hf_groupNetwork_Update.Value, true);
                    ipMessage.InnerHtml = "<span style='color: Green'>IP address updated</span>";
                }
                else {
                    ipMessage.InnerHtml = "<span style='color: Red'>Must enable current IP first</span>";
                }
            }

        }
        else {
            ipMessage.InnerHtml = "<span style='color: Red'>Not authorized to add</span>";
        }

        BuildGroupList();
        BuildIpAddresses();
        updatepnl_groupNetwork.Update();
        hf_groupNetwork_Update.Value = "";
    }
    protected void hf_groupNetwork_Delete_ValueChanged(object sender, EventArgs e) {
        string currOwner = _groups.GetOwner(CurrentGroupIdNetwork);
        if (((currOwner.ToLower() == _username.ToLower()) && (!string.IsNullOrEmpty(hf_groupNetwork_Delete.Value))) || (_username.ToLower() == ServerSettings.AdminUserName.ToLower())) {
            var listener = new GroupIPListener();
            string groupId = listener.GetGroupIdFromId(hf_groupNetwork_Delete.Value);
            listener.GetGroupIPs(groupId);
            string ip = listener.GetIPAddress(hf_groupNetwork_Delete.Value);
            bool isActive = listener.IpIsActive(hf_groupNetwork_Delete.Value);
            if (isActive) {
                if ((HttpContext.Current.Session["LoginGroup"] == null) || (listener.GroupIPListenerColl.Count == 1) || (listener.GroupIPListenerColl.Count > 1 && !listener.HasAtLeastOneActive(groupId, CurrentAddress)) || (ip != CurrentAddress && listener.GroupIPListenerColl.Count > 1 && listener.HasAtLeastOneActive(groupId))) {
                    listener.DeleteRow(hf_groupNetwork_Delete.Value);
                    ipMessage.InnerHtml = "<span style='color: Green'>IP address deleted</span>";
                }
                else {
                    ipMessage.InnerHtml = "<span style='color: Red'>Cannot delete current IP</span>";
                }
            }
            else {
                if (ip == CurrentAddress || CurrentIPActive(groupId) || !listener.HasAtLeastOneActive(groupId)) {
                    listener.DeleteRow(hf_groupNetwork_Delete.Value);
                    ipMessage.InnerHtml = "<span style='color: Green'>IP address deleted</span>";
                }
                else {
                    ipMessage.InnerHtml = "<span style='color: Red'>Cannot delete current IP</span>";
                }
            }
        }
        else {
            ipMessage.InnerHtml = "<span style='color: Red'>Not authorized to add</span>";
        }

        BuildGroupList();
        BuildIpAddresses();
        updatepnl_groupNetwork.Update();
        hf_groupNetwork_Delete.Value = "";
    }
    private void BuildIpAddresses() {
        var listener = new GroupIPListener();
        listener.GetGroupIPs(CurrentGroupIdNetwork);

        tb_createnew_listener.Text = string.Empty;

        pnl_groupNetwork.Controls.Clear();
        var str = new StringBuilder();
        int count = 1;

        string owner = _groups.GetOwner(CurrentGroupIdNetwork);

        if (owner.ToLower() != _username.ToLower()) {
            str.Append("<small><b class='pad-right-sml'>Note:</b>This is a read-only table and is only to be used to view the current groups network.</small><div class='clear-space'></div>");
        }

        str.Append("<div class=\"margin-top-sml\"><table class=\"myHeaderStyle\" cellpadding=\"0\" cellspacing=\"0\">");
        str.Append("<tr><td width=\"40px\"></td><td width=\"134px\">IP Address</td><td>Status/Actions</td><td width=\"170px\">Date Updated</td></tr></table></div>");

        bool hasOneActive = listener.HasAtLeastOneActive(CurrentGroupIdNetwork);
        if (owner.ToLower() == _username.ToLower() || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            // Editable Table
            pnl_groupNetworkAdd.Enabled = true;
            pnl_groupNetworkAdd.Visible = true;
            foreach (GroupIPListener_Coll dr in listener.GroupIPListenerColl) {
                str.Append("<table class='myItemStyle GridNormalRow' cellpadding='0' cellspacing='0'><tr>");
                str.Append("<td width='40px' class='GridViewNumRow' style='text-align: center;'>" + count.ToString(CultureInfo.InvariantCulture) + "</td>");
                str.Append("<td width='135px' class='border-right' style='text-align: center;'>" + dr.IPAddress + "</td>");
                str.Append("<td class='border-right'><div class='pad-left-sml'>" + CreateRadioButtons_Listener(dr.Active, count, dr.ID, hasOneActive) + "</div></td>");
                str.Append("<td width='170px' class='border-right' style='text-align: center;'>" + dr.DateUpdated.ToString() + "</td>");
                str.Append("</tr></table>");
                count++;
            }
        }
        else {
            // Read-Only Table
            pnl_groupNetworkAdd.Enabled = false;
            pnl_groupNetworkAdd.Visible = false;
            foreach (GroupIPListener_Coll dr in listener.GroupIPListenerColl) {
                str.Append("<table class='myItemStyle GridNormalRow' cellpadding='0' cellspacing='0'><tr>");
                str.Append("<td width='40px' class='GridViewNumRow' style='text-align: center;'>" + count.ToString(CultureInfo.InvariantCulture) + "</td>");
                str.Append("<td width='135px' class='border-right' style='text-align: center;'>" + dr.IPAddress + "</td>");
                str.Append("<td class='border-right'><div class='pad-left-sml'>" + CreateRadioButtons_Listener_ReadOnly(dr.Active, count, dr.ID, hasOneActive) + "</div></td>");
                str.Append("<td width='170px' class='border-right' style='text-align: center;'>" + dr.DateUpdated.ToString() + "</td>");
                str.Append("</tr></table>");
                count++;
            }
        }

        if (listener.GroupIPListenerColl.Count == 0) {
            str.Append("<div class='emptyGridView'>No Data Available</div>");
        }
        str.Append("<div class='clear-space'></div><div class='clear-space'></div>");

        pnl_groupNetwork.Controls.Add(new LiteralControl(str.ToString()));

        RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function(){ $('#" + ipMessage.ClientID + "').html(''); }, 3000);");
    }
    private string CreateRadioButtons_Listener(bool active, int count, string id, bool hasOneActive) {
        var str = new StringBuilder();
        str.Append("<div class='float-left'><div class='field switch'>");
        string enabledclass = "RandomActionBtns cb-enable";
        string disabledclass = "cb-disable selected";
        string onclickEnable = "onclick='UpdateGroupNetwork(\"" + id + "\")'";
        string onclickDisable = "";
        if (active) {
            enabledclass = "cb-enable selected";
            disabledclass = "RandomActionBtns cb-disable";
            onclickEnable = "";
            onclickDisable = "onclick='UpdateGroupNetwork(\"" + id + "\")'";
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
        str.Append("</div></div><a href='#delete' class='td-delete-btn float-right margin-right-sml margin-top-sml' onclick='DeleteGroupNetwork(\"" + id + "\");return false;' title='Delete'></a>");
        return str.ToString();
    }
    private string CreateRadioButtons_Listener_ReadOnly(bool active, int count, string id, bool hasOneActive) {
        var str = new StringBuilder();
        string listenText = "Allowed";
        if (hasOneActive) {
            listenText = "Blocked";
        }

        if (active) {
            str.Append("Allowed");
        }
        else {
            str.Append(listenText);
        }

        return str.ToString();
    }
    protected void btn_addIp_Click(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(tb_createnew_listener.Text)) {
            if (Parse(tb_createnew_listener.Text)) {
                var listener = new GroupIPListener();
                if (!listener.CheckIfExists(CurrentGroupIdNetwork, tb_createnew_listener.Text)) {
                    string currOwner = _groups.GetOwner(CurrentGroupIdNetwork);

                    if (currOwner.ToLower() == _username.ToLower()) {
                        listener.AddGroupIp(CurrentGroupIdNetwork, tb_createnew_listener.Text.Trim(), false, _username);
                        ipMessage.InnerHtml = "<span style='color: Green'>IP address added</span>";
                    }
                    else {
                        ipMessage.InnerHtml = "<span style='color: Green'>Not authorized to add</span>";
                    }
                }
                else {
                    ipMessage.InnerHtml = "<span style='color: Red'>IP already exists</span>";
                }
            }
            else {
                ipMessage.InnerHtml = "<span style='color: Red'>IP address invalid</span>";
            }
        }
        else {
            ipMessage.InnerHtml = "<span style='color: Red'>IP address invalid</span>";
        }

        BuildIpAddresses();
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
    private bool CurrentIPActive(string groupId) {
        if (HttpContext.Current.Session["LoginGroup"] != null) {
            var listener = new GroupIPListener();
            bool active = listener.IpIsActive(groupId, CurrentAddress);
            if (active) {
                return true;
            }
        }
        else {
            return true;
        }
        return false;
    }
    private string CurrentAddress {
        get {
            NameValueCollection n = HttpContext.Current.Request.ServerVariables;
            string ipaddress = n["REMOTE_ADDR"];
            if (ipaddress == "::1") {
                ipaddress = "127.0.0.1";
            }

            return ipaddress;
        }
    }

    #endregion


    #region Edit and delete the groups (non users)

    protected void hf_logoutGroup_ValueChanged(object sender, EventArgs e) {
        if (Session["LoginGroup"] != null) {
            Session.Remove("LoginGroup");
        }

        Page.Response.Redirect(Request.RawUrl);
    }
    protected void hf_loginGroup_ValueChanged(object sender, EventArgs e) {
        string requestGroup = hf_loginGroup.Value;
        if ((!string.IsNullOrEmpty(_username)) && (!string.IsNullOrEmpty(requestGroup))) {
            if (ServerSettings.CanLoginToGroup(_username, requestGroup, HttpContext.Current)) {
                Session["LoginGroup"] = requestGroup;
            }
        }

        Page.Response.Redirect(Request.RawUrl);
    }
    protected void hf_edit_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_edit.Value)) {
            _groups.getEntries(hf_edit.Value);
            if (_groups.group_dt.Count > 0) {
                BuildEditBox(_groups.group_dt);
                hf_edit.Value = string.Empty;
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'NewEdit-Group-element', 'Edit a Group');");
            }
        }
    }
    protected void hf_delete_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_delete.Value)) {
            _groups.getEntries(hf_delete.Value);
            if (_groups.group_dt.Count > 0) {
                string groupname = _groups.group_dt[0]["GroupName"];
                DeleteImage(_groups.group_dt[0]);
                DeleteAssociatedUsers(groupname);

                var post = new SiteMessageBoard(_username);
                post.getEntries(hf_delete.Value);
                foreach (Dictionary<string, string> dr in post.post_dt) {
                    if (dr["GroupName"] == groupname) {
                        post.deletePost(Guid.Parse(dr["ID"]), groupname);
                    }
                }

                var docs = new FileDrive(_username);
                docs.GetAllFiles();
                docs.GetAllFolders();
                foreach (var doccoll in docs.documents_coll) {
                    if (doccoll.GroupName == groupname) {
                        docs.deleteFile(doccoll.ID);
                    }
                }

                foreach (var foldercoll in docs.folders_coll) {
                    if (foldercoll.GroupName == groupname) {
                        docs.deleteFolder(foldercoll.ID, groupname);
                    }
                }

                string rssFeedsLoc = ServerSettings.GetServerMapLocation + "Apps\\MessageBoard\\RSS_Feeds";
                if (Directory.Exists(rssFeedsLoc)) {
                    string[] fileList = System.IO.Directory.GetFiles(rssFeedsLoc);
                    foreach (string file in fileList) {
                        System.IO.FileInfo fi = new System.IO.FileInfo(file);
                        string tempFileName = groupname + ".rss";
                        if (fi.Name == tempFileName) {
                            System.IO.File.Delete(fi.FullName);
                            break;
                        }
                    }
                }

                _groups.deleteGroup(hf_delete.Value);
            }


            hf_delete.Value = string.Empty;
            BuildGroupList();

            RegisterPostbackScripts.RegisterStartupScript(this, "BuildGroupArray();");
        }
    }
    protected void btn_finish_add_Click(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(tb_companyname.Text)) {
            lbl_error.Text = "Please enter a group name.";
        }
        else {
            lbl_error.Text = string.Empty;

            string currId = lbl_tempcompanyname.Text;
            string groupname = tb_companyname.Text.Trim().Replace(ServerSettings.StringDelimiter, "");
            string phonenumber = company_phone1.Value + "-" + company_phone2.Value + "-" + company_phone3.Value;
            string filename = UploadImage();

            if (string.IsNullOrEmpty(currId) || string.IsNullOrEmpty(_groups.GetGroupName_byID(currId))) {
                string id = Guid.NewGuid().ToString();
                _groups.addGroup(id, groupname.Replace("'", ""), tb_address.Text.Trim(), tb_city.Text.Trim(),
                                 GetState, "US", tb_postalcode.Text.Trim(), phonenumber, filename,
                                 DateTime.Now.ToString(CultureInfo.InvariantCulture),
                                 CheckUploadUrl(), cb_isprivate.Checked);

                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    var member = new MemberDatabase(_username);
                    string templist = BuildUserGroupList(member);
                    member.UpdateGroupName(templist + id);
                }
            }
            else {
                string currImg = _groups.GetGroupImg_byID(currId);
                if (currImg == "default-group.png") {
                    currImg = string.Empty;
                }

                if ((filename != currImg) && (!string.IsNullOrEmpty(currImg))) {
                    try {
                        if (File.Exists(ServerSettings.GetServerMapLocation + "Standard_Images\\Groups\\Logo\\" + currImg)) {
                            File.Delete(ServerSettings.GetServerMapLocation + "Standard_Images\\Groups\\Logo\\" + currImg);
                        }
                    }
                    catch { }
                }

                _groups.UpdateItem(currId, groupname.Replace("'", ""), tb_address.Text.Trim(), tb_city.Text.Trim(),
                                 GetState, "US", tb_postalcode.Text.Trim(), phonenumber, filename,
                                 DateTime.Now.ToString(CultureInfo.InvariantCulture),
                                 CheckUploadUrl(), cb_isprivate.Checked);
            }

            ServerSettings.PageToolViewRedirect(this.Page, "GroupOrg.aspx");
        }
    }
    private string UploadImage() {
        string filename = "default-group.png";

        if (fu_image_create.HasFile) {
            Thread.Sleep(500);
            var fi = new FileInfo(fu_image_create.FileName);
            if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                string companylogo = HelperMethods.RandomString(10) + fi.Extension;
                fu_image_create.SaveAs(ServerSettings.GetServerMapLocation + "Standard_Images\\Groups\\Logo\\" + companylogo);
                filename = companylogo;
            }
        }
        else {
            if ((!string.IsNullOrEmpty(tb_imageurl.Text)) && (tb_imageurl.Text.ToLower() != "link to image")) {
                filename = tb_imageurl.Text.Trim();
            }
            else if (!string.IsNullOrEmpty(img_logo.ImageUrl)) {
                filename = img_logo.ImageUrl.Replace(ResolveUrl("~/Standard_Images/Groups/Logo/"), "");
            }
        }

        return filename;
    }
    private bool CheckUploadUrl() {
        if (fu_image_create.HasFile) {
            var fi = new FileInfo(fu_image_create.FileName);
            if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                return false;
            }
        }
        else {
            if ((!string.IsNullOrEmpty(tb_imageurl.Text)) && (tb_imageurl.Text.ToLower() != "link to image")) {
                return true;
            }
        }

        return false;
    }

    #endregion


    #region View, add users

    protected void hf_viewusers_Standard_ValueChanged(object sender, EventArgs e) {
        pnl_modalTitle.Controls.Clear();
        pnl_users.Controls.Clear();
        var strUsers = new StringBuilder();
        var strTitle = new StringBuilder();
        if (!string.IsNullOrEmpty(hf_viewusers_Standard.Value)) {
            _groups.getEntries(hf_viewusers_Standard.Value);
            if (_groups.group_dt.Count > 0) {
                string groupname = hf_viewusers_Standard.Value;
                Dictionary<string, string> dr = _groups.group_dt[0];
                string imgurl;
                if (HelperMethods.ConvertBitToBoolean(dr["IsURL"])) {
                    imgurl = dr["Image"];
                    if (imgurl.StartsWith("~/")) {
                        imgurl = ResolveUrl(imgurl);
                    }
                }
                else
                    imgurl = ResolveUrl("~/Standard_Images/Groups/Logo/" + dr["Image"]);

                int count = CountUsersPerGroup(groupname);
                strTitle.Append("<div id ='associatedusers_clear' class='pad-all'><div class='float-left pad-right-big'><img alt='' src='" + imgurl + "' style='max-height: 30px;' /></div>");
                strTitle.Append("<div class='float-left pad-top'><h2>" + dr["GroupName"] + "</h2></div>");
                strTitle.Append("<div class='float-right'><b class='pad-right'>Associated Users</b>" + count + "</div><div class='clear-space'></div>");
                strTitle.Append("<small>Add and remove users to this group. Users may be associated with multiple groups. Users with Administrative roles cannot be edited unless that user is you.</small><div class='clear-space'></div></div>");

                MembershipUserCollection coll = Membership.GetAllUsers();
                foreach (MembershipUser u in coll) {
                    if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                        var member = new MemberDatabase(u.UserName);
                        bool canContinue = true;

                        if (canContinue) {
                            string un = HelperMethods.MergeFMLNames(member);
                            if ((u.UserName.Length > 15) && (!string.IsNullOrEmpty(member.LastName)))
                                un = member.FirstName + " " +
                                     member.LastName[0].ToString(CultureInfo.InvariantCulture) + ".";

                            if (member.GroupList.Contains(groupname)) {
                                string userNameTitle = "<h4>" + un + "</h4>";
                                string padding = "padding-top: 17px; padding-bottom: 17px;";
                                string acctImage = member.AccountImage;
                                if (!string.IsNullOrEmpty(acctImage)) {
                                    padding = string.Empty;
                                    userNameTitle = "<h4 class='float-left pad-top pad-left-sml'>" + un + "</h4>";
                                }

                                strUsers.Append("<div class='float-left pad-all' style='width: " + _userWidth + "; " + padding + "'>");
                                strUsers.Append(UserImageColorCreator.CreateImgColor(acctImage, member.UserColor, member.UserId, 30) + userNameTitle + "</div>");
                            }
                        }
                    }
                }

                if (count == 0) {
                    strUsers.Append("<div id='noUsersdiv'><div class='clear-space'></div>No users associated with this group.<div class='clear-space'></div></div>");
                }
            }
            else {
                strUsers.Append("<div id='noUsersdiv'><div class='clear-space'></div>No users associated with this group.<div class='clear-space'></div></div>");
            }
            pnl_modalTitle.Controls.Add(new LiteralControl(strTitle.ToString()));
            pnl_users.Controls.Add(new LiteralControl(strUsers + "</div>"));
        }
        hf_viewusers_Standard.Value = string.Empty;
        updatepnl_viewusers.Update();
    }
    protected void hf_viewusers_ValueChanged(object sender, EventArgs e) {
        pnl_modalTitle.Controls.Clear();
        pnl_users.Controls.Clear();
        var strUsers = new StringBuilder();
        var strUsers2 = new StringBuilder();
        var strTitle = new StringBuilder();
        if (!string.IsNullOrEmpty(hf_viewusers.Value)) {
            _groups.getEntries(hf_viewusers.Value);
            if (_groups.group_dt.Count > 0) {
                string groupname = hf_viewusers.Value;
                Dictionary<string, string> dr = _groups.group_dt[0];
                string imgurl;
                if (HelperMethods.ConvertBitToBoolean(dr["IsURL"])) {
                    imgurl = dr["Image"];
                    if (imgurl.StartsWith("~/")) {
                        imgurl = ResolveUrl(imgurl);
                    }
                }
                else
                    imgurl = ResolveUrl("~/Standard_Images/Groups/Logo/" + dr["Image"]);
                int count = CountUsersPerGroup(groupname);
                strTitle.Append("<div id ='associatedusers_clear' class='pad-all'><div class='float-left pad-right-big'><img alt='' src='" + imgurl + "' style='max-height: 30px;' /></div>");
                strTitle.Append("<div class='float-left pad-top'><h2>" + dr["GroupName"] + "</h2></div>");
                strTitle.Append("<div class='float-right'><b class='pad-right'>Associated Users</b>" + count + "</div><div class='clear-space'></div>");
                strTitle.Append("<small>Add and remove users to this group. Users may be associated with multiple groups. Users with Administrative roles cannot be edited unless that user is you.</small><div class='clear-space'></div></div>");

                MembershipUserCollection coll = Membership.GetAllUsers();
                foreach (MembershipUser u in coll) {
                    if (u.UserName.ToLower() == ServerSettings.AdminUserName.ToLower()) continue;
                    var member = new MemberDatabase(u.UserName);
                    bool canContinue = true;

                    if ((_ss.AllowPrivacy) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                        if ((member.PrivateAccount) && (u.UserName.ToLower() != _username.ToLower())) {
                            canContinue = false;
                        }
                    }

                    if (canContinue) {
                        string un = HelperMethods.MergeFMLNames(member);
                        if ((u.UserName.Length > 15) && (!string.IsNullOrEmpty(member.LastName)))
                            un = member.FirstName + " " + member.LastName[0].ToString(CultureInfo.InvariantCulture) + ".";

                        if (un.ToLower() == "n/a")
                            un = u.UserName;

                        string marginTop = "3px";
                        string userNameTitle = "<h4>" + un + "</h4>";
                        string padding = "padding-top: 17px; padding-bottom: 17px;";
                        string acctImage = member.AccountImage;
                        if (!string.IsNullOrEmpty(acctImage)) {
                            padding = string.Empty;
                            userNameTitle = "<h4 class='float-left pad-top pad-left-sml'>" + un + "</h4>";
                            marginTop = "8px";
                        }

                        if (member.GroupList.Contains(groupname)) {
                            strUsers.Append("<div class='app-icon-admin-group float-left pad-all' style='width: " + _userWidth + "; " + padding + "'>");
                            if ((dr["CreatedBy"].ToString().ToLower() == _username.ToLower()) || (!Roles.IsUserInRole(u.UserName, ServerSettings.AdminUserName)) || (u.UserName.ToLower() == _username.ToLower()) || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                                strUsers.Append("<a href='#' title='Remove " + u.UserName + " from this group' class='img-collapse-sml cursor-pointer float-left' onclick='RemoveUser(this, \"" + u.UserName + "\");return false;' style='margin-right: 5px; margin-top: " + marginTop + "'></a>");
                            }
                            strUsers.Append(UserImageColorCreator.CreateImgColor(acctImage, member.UserColor, member.UserId, 30) + userNameTitle);
                            strUsers.Append("</div>");
                        }
                        else {
                            strUsers2.Append("<div class='app-icon-admin-group float-left pad-all' style='width: " + _userWidth + "; " + padding + "'>");
                            if ((dr["CreatedBy"].ToString().ToLower() == _username.ToLower()) || (!Roles.IsUserInRole(u.UserName, ServerSettings.AdminUserName)) || (u.UserName.ToLower() == _username.ToLower()) || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                                strUsers2.Append("<a href='#' title='Add " + u.UserName + " to this group' class='img-expand-sml cursor-pointer float-left' onclick='AddUser(this, \"" + u.UserName + "\");return false;' style='margin-right: 5px; margin-top: " + marginTop + ";'></a>");
                            }
                            strUsers2.Append(UserImageColorCreator.CreateImgColor(acctImage, member.UserColor, member.UserId, 30) + userNameTitle);
                            strUsers2.Append("</div>");
                        }
                    }
                }

                if (count == 0) {
                    strUsers.Append("<div id='noUsersdiv'><div class='clear-space'></div>No users associated with this group.<div class='clear-space'></div></div>");
                }
            }
            else {
                strUsers.Append("<div id='noUsersdiv'><div class='clear-space'></div>No users associated with this group.<div class='clear-space'></div></div>");
            }

            pnl_modalTitle.Controls.Add(new LiteralControl(strTitle.ToString()));
            string strAdded = "<div id='package-added'>" + strUsers + "</div>";
            string strRemoved = "<div id='package-removed'>" + strUsers2 + "</div>";
            pnl_users.Controls.Add(new LiteralControl(strAdded + "<div class='clear' style='height: 30px;'></div>" + strRemoved + "</div>"));
        }
        hf_viewusers.Value = string.Empty;
        updatepnl_viewusers.Update();
    }
    protected void hf_inviteUser_ValueChanged(object sender, EventArgs e) {
        string userList = BuildInvite(hf_inviteUser.Value);

        hf_inviteUser.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#invite-innermodal').html(\"" + userList + "\");openWSE.LoadModalWindow(true, 'GroupInviteUser-element', 'Invite users to group');");
    }
    protected void hf_finishInviteUser_ValueChanged(object sender, EventArgs e) {
        string groupName = _groups.GetGroupName_byID(hf_finishInviteUser.Value);
        string[] splitUsers = hf_inviteUserList.Value.Split(',');
        foreach (string user in splitUsers) {
            if (!string.IsNullOrEmpty(user)) {
                SendInviteNotification(hf_finishInviteUser.Value, groupName, user);
            }
        }

        hf_finishInviteUser.Value = string.Empty;
        hf_inviteUserList.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'GroupInviteUser-element', '');setTimeout(function() { $('#invite-innermodal').html(''); }, openWSE_Config.animationSpeed);RefreshList();");
    }
    protected void hf_adduser_ValueChanged(object sender, EventArgs e) {
        string username = string.Empty;
        string groupname = string.Empty;
        string[] values = hf_adduser.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length > 1) {
            username = values[0];
            groupname = values[1];
        }

        if (!string.IsNullOrEmpty(username)) {
            var member = new MemberDatabase(username);
            string templist = BuildUserGroupList(member);
            member.UpdateGroupName(templist + groupname);

            if (username.ToLower() != _username.ToLower())
                SendNotification("<span style='color: #2F9E00;'>Added</span> to", groupname, username);

            if (string.IsNullOrEmpty(templist) && username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                Session["ReloadGroupModal"] = groupname;
                Response.Redirect(Request.RawUrl);
            }
        }

        hf_adduser.Value = string.Empty;

        if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            BuildGroupList();
        }
    }
    protected void hf_removeuser_ValueChanged(object sender, EventArgs e) {
        string username = string.Empty;
        string groupname = string.Empty;
        string[] values = hf_removeuser.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length > 1) {
            username = values[0];
            groupname = values[1];
        }

        if (!string.IsNullOrEmpty(username)) {
            var member = new MemberDatabase(username);
            string templist = Groups.RemoveUserFromGroupList(member, groupname);
            member.UpdateGroupName(templist);

            if (username.ToLower() != _username.ToLower())
                SendNotification("<span style='color: #D80000;'>Removed</span> from", groupname, username);
        }

        hf_removeuser.Value = string.Empty;

        if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            BuildGroupList();
        }
    }

    protected void hf_refreshList_ValueChanged(object sender, EventArgs e) {
        BuildGroupList();

        StringBuilder _strScriptreg = new StringBuilder();
        MemberDatabase _member = new MemberDatabase(_username);
        if ((_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && (_member.GroupList.Count == 0)) {
            _strScriptreg.Append("$('#aGroupLogin').css('display', 'none');$(\"body\").append(\"<h3 id='noapartofgroupmessage' style='position: fixed; bottom: 28px; right: 0; z-index: 10000; padding: 15px; background: #2F2F2F; color: #FFF;'>You must add yourself to a group</h3>\");");
        }
        else {
            _strScriptreg.Append("$('#aGroupLogin').css('display', '');$(\"#noapartofgroupmessage\").remove();");
        }

        RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());

        hf_refreshList.Value = string.Empty;
    }

    private void DeleteImage(Dictionary<string, string> dr) {
        if (dr["Image"] != "default-group.png") {
            string filename = ServerSettings.GetServerMapLocation + "Standard_Images\\Groups\\Logo\\" + dr["Image"];
            if (!File.Exists(filename)) return;
            try {
                File.Delete(filename);
            }
            catch {
            }
        }
    }
    private static void DeleteAssociatedUsers(string groupName) {
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll) {
            var member = new MemberDatabase(u.UserName);
            if (member.GroupList.Contains(groupName)) {
                string templist = Groups.RemoveUserFromGroupList(member, groupName);
                member.UpdateGroupName(templist);
            }
        }
    }

    #endregion


    #region Reset Controls

    protected void hf_reset_ValueChanged(object sender, EventArgs e) {
        ResetControls();
    }
    private void ResetControls() {
        tb_companyname.Text = string.Empty;
        lbl_tempcompanyname.Text = string.Empty;
        company_phone1.Value = string.Empty;
        company_phone2.Value = string.Empty;
        company_phone3.Value = string.Empty;
        tb_address.Text = string.Empty;
        tb_city.Text = string.Empty;
        dd_state.SelectedIndex = 0;
        tb_postalcode.Text = string.Empty;
        img_logo.Style["display"] = "none";
        img_logo.ImageUrl = string.Empty;
        tb_imageurl.Text = string.Empty;
        lbl_error.Enabled = false;
        lbl_error.Visible = false;
        updatepnl_editmode_1.Update();
        updatepnl_editmode_2.Update();
    }

    #endregion


    #region Support methods

    private string GetState {
        get {
            int i = dd_state.SelectedIndex;
            return dd_state.Items[i].Value.Trim();
        }
    }
    private bool IsUrlChecker {
        get {
            bool isurl = (!string.IsNullOrEmpty(tb_imageurl.Text)) &&
                         (tb_imageurl.Text.ToLower() != "link to image");
            return isurl;
        }
    }
    private static string BuildUserGroupList(MemberDatabase member) {
        List<string> groups = member.GroupList;

        return groups.Aggregate(string.Empty, (current, t) => current + (t + ServerSettings.StringDelimiter));
    }

    private static int CountUsersPerGroup(string groupname) {
        MembershipUserCollection coll = Membership.GetAllUsers();

        int count = 0;
        foreach (MembershipUser u in coll) {
            if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                MemberDatabase _temp = new MemberDatabase(u.UserName);
                List<string> groupList = _temp.GroupList;
                foreach (string _group in groupList) {
                    if (_group == groupname)
                        count++;
                }
            }
        }

        return count;
    }
    private void SendNotification(string action, string group, string user) {
        MailMessage mailTo = new MailMessage();
        var messagebody = new StringBuilder();
        messagebody.Append("<h3>Group Notification</h3><br />");
        MemberDatabase tempMember = new MemberDatabase(_username);

        string groupName = _groups.GetGroupName_byID(group);

        messagebody.Append("<p>You have been " + action + " <b>" + groupName + "</b> by " + HelperMethods.MergeFMLNames(tempMember) + ".</p>");
        var un = new UserNotificationMessages(user);
        string email = un.attemptAdd("adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", messagebody.ToString(), true);
        if (!string.IsNullOrEmpty(email))
            mailTo.To.Add(email);

        UserNotificationMessages.finishAdd(mailTo, "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", messagebody.ToString());
    }
    private void SendInviteNotification(string id, string group, string user) {
        MailMessage mailTo = new MailMessage();
        var messagebody = new StringBuilder();
        messagebody.Append("<h3>Group Notification</h3><br />");
        MemberDatabase tempMember = new MemberDatabase(_username);

        string btnAccept = "<a href='#AcceptInvite' class='margin-right' onclick='openWSE.AcceptGroupNotification(this, \"" + id + "\");return false;'>Accept</a>";
        string btnDecline = "<a href='#DeclineInvite' class='margin-left' onclick='openWSE.NotiActionsHideInd(this);return false;'>Decline</a>";

        messagebody.Append("<p>" + HelperMethods.MergeFMLNames(tempMember) + " has invited you to join the group <b>" + group + "</b>.<div class='clear-space-five'></div>" + btnAccept + btnDecline + "</p>");

        var un = new UserNotificationMessages(user);
        string email = un.attemptAdd("adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", messagebody.ToString(), true);
        if (!string.IsNullOrEmpty(email))
            mailTo.To.Add(email);

        UserNotificationMessages.finishAdd(mailTo, "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f", messagebody.ToString());
    }

    private string BuildInvite(string groupId) {
        string groupName = _groups.GetGroupName_byID(groupId);

        var str = new StringBuilder();
        str.Append("<h4 class='float-left pad-top-sml font-bold'>Invite users to '" + groupName + "'</h4>");
        str.Append("<div class='clear-space'></div><div class='listofusers'>");
        var str_list = new StringBuilder();
        int count = 0;
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll) {
            var m = new MemberDatabase(u.UserName);
            if ((u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) && (u.UserName.ToLower() != _username.ToLower()) && (!_groups.IsApartOfGroup(m.GroupList, groupId))) {
                Notifications notifications = new Notifications();
                if (notifications.IsUserNotificationEnabled(u.UserName, "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f")) {
                    UserNotificationMessages unm = new UserNotificationMessages(u.UserName);
                    unm.getEntries();

                    bool canAdd = true;

                    foreach (UserNotificationsMessage_Coll userMessages in unm.Messages) {
                        if (userMessages.NotificationID == "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f") {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd) {
                        str_list.Append("<div class='float-left' style='width:190px'><input class='invite-tb-list' type='checkbox' value='" + u.UserName + "'>&nbsp;" + HelperMethods.MergeFMLNames(m) + "</div>");
                        count++;
                    }
                }
            }
        }
        if (count == 0) {
            str.Append("No users available<div class='clear-space'></div>");
        }
        else {
            str.Append(str_list + "</div>");
            str.Append("<div class='clear' style='height: 25px;'></div>");
            str.Append("<input type='button' class='input-buttons' value='Send Invite(s)' onclick='invite_click()' /><div class='clear-space'></div>");
        }

        return str.ToString();
    }

    #endregion
}