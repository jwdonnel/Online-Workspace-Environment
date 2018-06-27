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

public partial class SiteTools_GroupOrg : BasePage {

    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private Groups _groups;

    protected void Page_Load(object sender, EventArgs e) {
        _groups = new Groups(CurrentUsername);

        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            pnl_addgroupbtn.Enabled = false;
            pnl_addgroupbtn.Visible = false;
        }

        if (!IsPostBack || PostbackControlIsUpdateAll()) {
            BuildGroupList();

            // Reload the group add and remove modal
            if (Session["ReloadGroupModal"] != null && !string.IsNullOrEmpty(Session["ReloadGroupModal"].ToString())) {
                RegisterPostbackScripts.RegisterStartupScript(this, "try { ViewGroup('" + Session["ReloadGroupModal"].ToString() + "'); }catch(evt) { }");

                Session["ReloadGroupModal"] = null;
                Session.Remove("ReloadGroupModal");
            }
        }
    }

    #region Group builder methods

    private void BuildGroupList() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 3, "GroupList_Table");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Icon", "55px", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Name", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Description", "300px", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Created By", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Created", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Is Private", "75px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Group Network", "75px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Users", "50px", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body

        _groups.getEntriesForGroupOrgPage();
        int count = 0;
        List<string> groupList = CurrentUserMemberDatabase.GroupList;
        int groupCount = groupList.Count;

        foreach (Dictionary<string, string> dr in _groups.group_dt) {
            bool canContinue = true;
            StringBuilder strTemp = new StringBuilder();

            if (dr["CreatedBy"].ToLower() == CurrentUsername.ToLower() || IsUserNameEqualToAdmin()) {
                strTemp.Append("<a onclick='DeleteGroup(\"" + dr["GroupID"] + "\");return false;' class='td-delete-btn' title='Delete Group'></a>");
                strTemp.Append("<a onclick='EditGroup(\"" + dr["GroupID"] + "\");return false;' class='td-edit-btn' title='Edit Group'></a>");
            }

            if (!IsUserInAdminRole()) {
                if (groupList.Contains(dr["GroupID"])) {
                    if (groupCount > 1) {
                        strTemp.Append("<a title='Remove yourself from this group' onclick='RemoveUser(\"" + CurrentUsername + "\", \"" + dr["GroupID"] + "\");return false;' class='joinquit td-cancel-btn'></a>");
                    }
                }
                else {
                    if (CompareCreatedByGroup(CurrentUsername, dr["CreatedBy"]) || groupList.Contains(dr["GroupID"])) {
                        strTemp.Append("<a title='Join this group'  onclick='AddUser(\"" + CurrentUsername + "\", \"" + dr["GroupID"] + "\");return false;' class='joinquit td-add-btn'></a>");
                    }
                    else
                        canContinue = false;
                }
                strTemp.Append("<a title='View users in group (Non-Editable)' onclick='ViewGroup_Standard(\"" + dr["GroupID"] + "\");return false;' class='td-users-btn'></a>");
            }
            else {
                strTemp.Append("<a title='View users in group (Non-Editable)' onclick='ViewGroup_Standard(\"" + dr["GroupID"].ToString() + "\");return false;' class='td-users-btn'></a>");
                strTemp.Append("<a title='Add/Remove users from group' onclick='ViewGroup(\"" + dr["GroupID"].ToString() + "\");return false;' class='td-addremove-btn'></a>");
            }

            strTemp.Append("<a title='Setup your Group to listen to certain IP Addresses' onclick='GroupNetwork(\"" + dr["GroupID"] + "\");return false;' class='td-network-btn'></a>");

            if (HelperMethods.ConvertBitToBoolean((dr["IsPrivate"])) && (!IsUserInAdminRole()) && (!groupList.Contains(dr["GroupID"]))) {
                canContinue = false;
            }
            else if (dr["CreatedBy"].ToLower() == CurrentUsername.ToLower() || IsUserNameEqualToAdmin()) {
                strTemp.Append("<a title='Invite user(s) from group' onclick='InviteToGroup(\"" + dr["GroupID"] + "\");return false;' class='td-share-btn'></a>");
            }

            if (canContinue) {
                List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();

                string imgurl = HelperMethods.ConvertBitToBoolean(dr["IsURL"]) ? dr["Image"] : ResolveUrl("~/Standard_Images/Groups/Logo/" + dr["Image"]);
                if (imgurl.StartsWith("~/")) {
                    imgurl = ResolveUrl(imgurl);
                }
                else {
                    imgurl = HelperMethods.RemoveProtocolFromUrl(imgurl);
                }

                bodyColumns.Add(new TableBuilderBodyColumnValues("Icon", "<img alt='logo' src='" + imgurl + "' style='max-height: 32px; width: 32px;' />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Name", dr["GroupName"], TableBuilderColumnAlignment.Left));

                string description = dr["Description"];
                if (string.IsNullOrEmpty(description)) {
                    description = "No description available";
                }

                bodyColumns.Add(new TableBuilderBodyColumnValues("Description", description, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Created By", dr["CreatedBy"], TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Date Created", dr["Date"], TableBuilderColumnAlignment.Left));

                string isPrivateStr = "<input type='checkbox' disabled='disabled' value='0' />";
                if (HelperMethods.ConvertBitToBoolean(dr["IsPrivate"])) {
                    isPrivateStr = "<input type='checkbox' disabled='disabled' checked='checked' value='1' />";
                }

                bodyColumns.Add(new TableBuilderBodyColumnValues("Is Private", isPrivateStr, TableBuilderColumnAlignment.Left));

                string privateNetwork = "<input type='checkbox' disabled='disabled' value='0' />";
                if (CheckIfListening(dr["GroupID"]).ToLower() == "yes") {
                    privateNetwork = "<input type='checkbox' disabled='disabled' checked='checked' value='1' />";
                }

                bodyColumns.Add(new TableBuilderBodyColumnValues("Group Network", privateNetwork, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Users", (CountUsersPerGroup(dr["GroupID"])).ToString(), TableBuilderColumnAlignment.Left));

                tableBuilder.AddBodyRow(bodyColumns, strTemp.ToString());
                count++;
            }
        }

        if (!IsUserNameEqualToAdmin()) {
            StringBuilder _strScriptreg = new StringBuilder();
            if (groupCount == 0) {
                _strScriptreg.Append("$(\"#main_container\").prepend(\"<h3 id='noapartofgroupmessage' class='pad-bottom-big'>You must add yourself to a group</h3>\");");
            }
            else {
                _strScriptreg.Append("$(\"#noapartofgroupmessage\").remove();");
            }

            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
        }

        #endregion

        pnl_companyholder.Controls.Clear();
        pnl_companyholder.Controls.Add(tableBuilder.CompleteTableLiteralControl("No groups found"));
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
            tb_description.Text = drc[0]["Description"];
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
            return "Yes";
        }

        return "No";
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
        if (((currOwner.ToLower() == CurrentUsername.ToLower()) && (!string.IsNullOrEmpty(hf_groupNetwork_Update.Value))) || (IsUserNameEqualToAdmin())) {
            var listener = new GroupIPListener();
            string groupId = listener.GetGroupIdFromId(hf_groupNetwork_Update.Value);
            listener.GetGroupIPs(groupId);
            string ip = listener.GetIPAddress(hf_groupNetwork_Update.Value);
            bool isActive = listener.IpIsActive(hf_groupNetwork_Update.Value);
            if (isActive) {
                if (!GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername) || (listener.GroupIPListenerColl.Count == 1) || (listener.GroupIPListenerColl.Count > 1 && !listener.HasAtLeastOneActive(groupId, CurrentIpAddress)) || (ip != CurrentIpAddress && listener.GroupIPListenerColl.Count > 1 && listener.HasAtLeastOneActive(groupId))) {
                    listener.UpdateRow(hf_groupNetwork_Update.Value, false);
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Cannot disable current IP');");
                }
            }
            else {
                if (ip == CurrentIpAddress || CurrentIPActive(groupId)) {
                    listener.UpdateRow(hf_groupNetwork_Update.Value, true);
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Must enable current IP first');");
                }
            }

        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Not authorized to add');");
        }

        BuildGroupList();
        BuildIpAddresses();
        updatepnl_groupNetwork.Update();
        hf_groupNetwork_Update.Value = "";
    }
    protected void hf_groupNetwork_Delete_ValueChanged(object sender, EventArgs e) {
        string currOwner = _groups.GetOwner(CurrentGroupIdNetwork);
        if (((currOwner.ToLower() == CurrentUsername.ToLower()) && (!string.IsNullOrEmpty(hf_groupNetwork_Delete.Value))) || (IsUserNameEqualToAdmin())) {
            var listener = new GroupIPListener();
            string groupId = listener.GetGroupIdFromId(hf_groupNetwork_Delete.Value);
            listener.GetGroupIPs(groupId);
            string ip = listener.GetIPAddress(hf_groupNetwork_Delete.Value);
            bool isActive = listener.IpIsActive(hf_groupNetwork_Delete.Value);
            if (isActive) {
                if (!GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername) || (listener.GroupIPListenerColl.Count == 1) || (listener.GroupIPListenerColl.Count > 1 && !listener.HasAtLeastOneActive(groupId, CurrentIpAddress)) || (ip != CurrentIpAddress && listener.GroupIPListenerColl.Count > 1 && listener.HasAtLeastOneActive(groupId))) {
                    listener.DeleteRow(hf_groupNetwork_Delete.Value);
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Cannot delete current IP');");
                }
            }
            else {
                if (ip == CurrentIpAddress || CurrentIPActive(groupId) || !listener.HasAtLeastOneActive(groupId)) {
                    listener.DeleteRow(hf_groupNetwork_Delete.Value);
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Cannot delete current IP');");
                }
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Not authorized to add');");
        }

        BuildGroupList();
        BuildIpAddresses();
        updatepnl_groupNetwork.Update();
        hf_groupNetwork_Delete.Value = "";
    }
    private void BuildIpAddresses() {
        var listener = new GroupIPListener();
        listener.GetGroupIPs(CurrentGroupIdNetwork);

        pnl_groupNetwork.Controls.Clear();
        string owner = _groups.GetOwner(CurrentGroupIdNetwork);
        bool canEdit = owner.ToLower() == CurrentUsername.ToLower();

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, canEdit, 1, "GroupIpAddressList_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("IP Address", "150px", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Status", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Updated", "200px", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        bool hasOneActive = listener.HasAtLeastOneActive(CurrentGroupIdNetwork);
        if (owner.ToLower() == CurrentUsername.ToLower() || IsUserNameEqualToAdmin()) {
            // Editable Table
            pnl_groupNetworkAdd.Enabled = true;
            pnl_groupNetworkAdd.Visible = true;
            foreach (GroupIPListener_Coll dr in listener.GroupIPListenerColl) {
                List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();

                bodyColumns.Add(new TableBuilderBodyColumnValues("IP Address", dr.IPAddress, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Status", CreateRadioButtons_Listener(dr.Active, dr.ID, hasOneActive), TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Date Updated", dr.DateUpdated.ToString(), TableBuilderColumnAlignment.Left));

                tableBuilder.AddBodyRow(bodyColumns, "<a class='td-delete-btn' onclick='DeleteGroupNetwork(\"" + dr.ID + "\");return false;' title='Delete'></a>");
            }
        }
        else {
            // Read-Only Table
            pnl_groupNetworkAdd.Enabled = false;
            pnl_groupNetworkAdd.Visible = false;
            foreach (GroupIPListener_Coll dr in listener.GroupIPListenerColl) {
                List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();

                bodyColumns.Add(new TableBuilderBodyColumnValues("IP Address", dr.IPAddress, TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Status", CreateRadioButtons_Listener_ReadOnly(dr.Active, dr.ID, hasOneActive), TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Date Updated", dr.DateUpdated.ToString(), TableBuilderColumnAlignment.Left));

                tableBuilder.AddBodyRow(bodyColumns, string.Empty);
            }
        }
        #endregion

        if (!canEdit) {
            pnl_groupNetwork.Controls.Add(new LiteralControl("This is a read-only table and is only to be used to view the current groups network.<div class='clear-space'></div>"));
        }
        else {
            List<TableBuilderInsertColumnValues> insertColumns = new List<TableBuilderInsertColumnValues>();

            insertColumns.Add(new TableBuilderInsertColumnValues("IP Address", "tb_createnew_listener", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text));
            insertColumns.Add(new TableBuilderInsertColumnValues("Status", string.Empty, TableBuilderColumnAlignment.Left));
            insertColumns.Add(new TableBuilderInsertColumnValues("Date Updated", string.Empty, TableBuilderColumnAlignment.Left));

            tableBuilder.AddInsertRow(insertColumns, "AddGroupNetworkIP();");
        }

        pnl_groupNetwork.Controls.Add(tableBuilder.CompleteTableLiteralControl());
    }
    private string CreateRadioButtons_Listener(bool active, string id, bool hasOneActive) {
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

        str.Append("<span class='" + enabledclass + "'><input id='rb_listener_active_" + id + "' type='radio' value='active' " + onclickEnable + " /><label for='rb_listener_active_" + id + "'>Listen</label></span>");
        str.Append("<span class='" + disabledclass + "'><input id='rb_listener_deactive_" + id + "' type='radio' value='deactive' " + onclickDisable + " /><label for='rb_listener_deactive_" + id + "'>" + listenText + "</label></span>");
        str.Append("</div></div>");
        return str.ToString();
    }
    private string CreateRadioButtons_Listener_ReadOnly(bool active, string id, bool hasOneActive) {
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
        string ip = hf_addIp.Value.Trim();

        if (!string.IsNullOrEmpty(ip)) {
            if (Parse(ip)) {
                var listener = new GroupIPListener();
                if (!listener.CheckIfExists(CurrentGroupIdNetwork, ip)) {
                    string currOwner = _groups.GetOwner(CurrentGroupIdNetwork);

                    if (currOwner.ToLower() == CurrentUsername.ToLower()) {
                        listener.AddGroupIp(CurrentGroupIdNetwork, ip, false, CurrentUsername);
                    }
                    else {
                        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Not authorized to add');");
                    }
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

        BuildIpAddresses();
        hf_addIp.Value = string.Empty;
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
        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            var listener = new GroupIPListener();
            bool active = listener.IpIsActive(groupId, CurrentIpAddress);
            if (active) {
                return true;
            }
        }
        else {
            return true;
        }
        return false;
    }

    #endregion


    #region Edit and delete the groups (non users)

    protected void hf_logoutGroup_ValueChanged(object sender, EventArgs e) {
        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            GroupSessions.RemoveGroupLoginSession(CurrentUsername);
        }

        ServerSettings.RefreshPage(Page, string.Empty);
    }
    protected void hf_loginGroup_ValueChanged(object sender, EventArgs e) {
        string requestGroup = hf_loginGroup.Value;
        if ((!string.IsNullOrEmpty(CurrentUsername)) && (!string.IsNullOrEmpty(requestGroup))) {
            if (ServerSettings.CanLoginToGroup(CurrentUsername, requestGroup, HttpContext.Current)) {
                GroupSessions.AddOrSetNewGroupLoginSession(CurrentUsername, requestGroup);
            }
        }

        ServerSettings.RefreshPage(Page, string.Empty);
    }
    protected void hf_edit_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_edit.Value)) {
            _groups.getEntriesForGroupOrgPage(hf_edit.Value);
            if (_groups.group_dt.Count > 0) {
                BuildEditBox(_groups.group_dt);
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'NewEdit-Group-element', 'Edit a Group');");
            }

            hf_edit.Value = string.Empty;
        }
    }
    protected void hf_delete_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_delete.Value)) {
            _groups.getEntriesForGroupOrgPage(hf_delete.Value);
            if (_groups.group_dt.Count > 0) {
                string groupname = _groups.group_dt[0]["GroupName"];
                DeleteImage(_groups.group_dt[0]);
                DeleteAssociatedUsers(groupname);

                var post = new SiteMessageBoard(CurrentUsername);
                post.DeleteGroupPosts(groupname);

                var docs = new FileDrive(CurrentUsername);
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
            string filename = UploadImage();

            if (string.IsNullOrEmpty(currId) || string.IsNullOrEmpty(_groups.GetGroupName_byID(currId))) {
                string id = Guid.NewGuid().ToString();
                _groups.addGroup(id, groupname.Replace("'", ""), tb_description.Text.Trim(), filename,
                                 ServerSettings.ServerDateTime.ToString(CultureInfo.InvariantCulture),
                                 CheckUploadUrl(), cb_isprivate.Checked);

                if (!IsUserInAdminRole()) {
                    string templist = BuildUserGroupList(CurrentUserMemberDatabase);
                    CurrentUserMemberDatabase.UpdateGroupName(templist + id);
                }
            }
            else {
                string currImg = _groups.GetGroupImg_byID(currId);
                if (currImg == "default-group.png") {
                    currImg = string.Empty;
                }

                if ((filename != currImg) && (!string.IsNullOrEmpty(currImg)) && currImg.Contains("Standard_Images") && currImg.Contains("Groups")) {
                    try {
                        if (File.Exists(ServerSettings.GetServerMapLocation + currImg.Replace("~/", "").Replace("/", "\\"))) {
                            File.Delete(ServerSettings.GetServerMapLocation + currImg.Replace("~/", "").Replace("/", "\\"));
                        }
                    }
                    catch { }
                }

                _groups.UpdateItem(currId, groupname.Replace("'", ""), tb_description.Text.Trim(), filename,
                                 ServerSettings.ServerDateTime.ToString(CultureInfo.InvariantCulture),
                                 CheckUploadUrl(), cb_isprivate.Checked);
            }

            ServerSettings.PageIFrameRedirect(this.Page, "GroupOrg.aspx");
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
                filename = "~/Standard_Images/Groups/Logo/" + companylogo;
            }
        }
        else {
            if ((!string.IsNullOrEmpty(tb_imageurl.Text)) && (tb_imageurl.Text.ToLower() != "link to image")) {
                filename = tb_imageurl.Text.Trim();
            }
            else if (!string.IsNullOrEmpty(img_logo.ImageUrl)) {
                filename = img_logo.ImageUrl.Replace(ResolveUrl("~/Standard_Images/Groups/Logo/"), "");
                filename = "~/Standard_Images/Groups/Logo/" + filename;
            }
        }

        return filename;
    }
    private bool CheckUploadUrl() {
        if (fu_image_create.HasFile) {
            var fi = new FileInfo(fu_image_create.FileName);
            if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                return true;
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
            _groups.getEntriesForGroupOrgPage(hf_viewusers_Standard.Value);
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
                strTitle.Append("<div id ='associatedusers_clear'><div class='float-left pad-right'><img alt='' src='" + imgurl + "' style='max-height: 30px;' /></div>");
                strTitle.Append("<div class='float-left pad-top'><h2>" + dr["GroupName"] + "</h2></div>");
                strTitle.Append("<div class='float-right'><b class='pad-right'>Associated Users</b>" + count + "</div><div class='clear-space'></div>");
                strTitle.Append("Add and remove users to this group. Users may be associated with multiple groups. Users with Administrative roles cannot be edited unless that user is you.<div class='clear-space'></div></div>");

                MembershipUserCollection coll = Membership.GetAllUsers();
                foreach (MembershipUser u in coll) {
                    if (!BasePage.IsUserNameEqualToAdmin(u.UserName)) {
                        var member = new MemberDatabase(u.UserName);

                        List<string> groupList = new List<string>();
                        if (CurrentUsername.ToLower() == u.UserName.ToLower()) {
                            groupList = member.GroupListForGroupOrgPage;
                        }
                        else {
                            groupList = member.GroupList;
                        }

                        string un = HelperMethods.MergeFMLNames(member);
                        if ((u.UserName.Length > 15) && (!string.IsNullOrEmpty(member.LastName)))
                            un = member.FirstName + " " +
                                 member.LastName[0].ToString(CultureInfo.InvariantCulture) + ".";

                        if (groupList.Contains(groupname)) {
                            string userNameTitle = "<span class='app-span-modify'>" + un + "</span>";
                            strUsers.Append("<div class='app-icon-admin'>");
                            strUsers.Append(UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 24, member.SiteTheme) + userNameTitle);
                            strUsers.Append("<div class='clear'></div></div>");
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
            pnl_users.Controls.Add(new LiteralControl(strUsers + "<div class='clear-space'></div>"));
        }
        hf_viewusers_Standard.Value = string.Empty;
        updatepnl_viewusers.Update();

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'GroupEdit-element', 'Associated Users');");
    }
    protected void hf_viewusers_ValueChanged(object sender, EventArgs e) {
        pnl_modalTitle.Controls.Clear();
        pnl_users.Controls.Clear();
        var strUsers = new StringBuilder();
        var strUsers2 = new StringBuilder();
        var strTitle = new StringBuilder();
        if (!string.IsNullOrEmpty(hf_viewusers.Value)) {
            _groups.getEntriesForGroupOrgPage(hf_viewusers.Value);
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
                strTitle.Append("<div id ='associatedusers_clear'><div class='float-left pad-right'><img alt='' src='" + imgurl + "' style='max-height: 30px;' /></div>");
                strTitle.Append("<div class='float-left pad-top'><h2>" + dr["GroupName"] + "</h2></div>");
                strTitle.Append("<div class='float-right'><b class='pad-right'>Associated Users</b>" + count + "</div><div class='clear-space'></div>");
                strTitle.Append("Add and remove users to this group. Users may be associated with multiple groups. Users with Administrative roles cannot be edited unless that user is you.<div class='clear-space'></div></div>");

                MembershipUserCollection coll = Membership.GetAllUsers();
                foreach (MembershipUser u in coll) {
                    if (BasePage.IsUserNameEqualToAdmin(u.UserName)) continue;
                    var member = new MemberDatabase(u.UserName);

                    List<string> groupList = new List<string>();
                    if (CurrentUsername.ToLower() == u.UserName.ToLower()) {
                        groupList = member.GroupListForGroupOrgPage;
                    }
                    else {
                        groupList = member.GroupList;
                    }

                    bool canContinue = true;

                    if (MainServerSettings.AllowPrivacy && !IsUserNameEqualToAdmin()) {
                        if (member.PrivateAccount && u.UserName.ToLower() != CurrentUsername.ToLower()) {
                            canContinue = false;
                        }
                    }

                    if (canContinue) {
                        string un = HelperMethods.MergeFMLNames(member);
                        if ((u.UserName.Length > 15) && (!string.IsNullOrEmpty(member.LastName)))
                            un = member.FirstName + " " + member.LastName[0].ToString(CultureInfo.InvariantCulture) + ".";

                        if (un.ToLower() == "n/a")
                            un = u.UserName;

                        string userNameTitle = "<span class='app-span-modify'>" + un + "</span>";
                        string acctImage = member.AccountImage;

                        if (groupList.Contains(groupname)) {
                            strUsers.Append("<div class='app-icon-admin app-icon-admin-group'>");
                            if (dr["CreatedBy"].ToString().ToLower() == CurrentUsername.ToLower() || IsUserNameEqualToAdmin() || u.UserName.ToLower() == CurrentUsername.ToLower()) {
                                strUsers.Append("<a title='Remove " + u.UserName + " from this group' class='img-collapse-sml cursor-pointer float-left' onclick='RemoveUser(this, \"" + u.UserName + "\");return false;'></a>");
                            }
                            else {
                                strUsers.Append("<div class='float-left' style='width: 23px; height: 17px;'></div>");
                            }
                            strUsers.Append(UserImageColorCreator.CreateImgColor(acctImage, member.UserColor, member.UserId, 24, member.SiteTheme) + userNameTitle);
                            strUsers.Append("<div class='clear'></div></div>");
                        }
                        else {
                            strUsers2.Append("<div class='app-icon-admin app-icon-admin-group'>");
                            if (dr["CreatedBy"].ToString().ToLower() == CurrentUsername.ToLower() || IsUserNameEqualToAdmin() || u.UserName.ToLower() == CurrentUsername.ToLower()) {
                                strUsers2.Append("<a title='Add " + u.UserName + " to this group' class='img-expand-sml cursor-pointer float-left' onclick='AddUser(this, \"" + u.UserName + "\");return false;'></a>");
                            }
                            else {
                                strUsers2.Append("<div class='float-left' style='width: 23px; height: 17px;'></div>");
                            }
                            strUsers2.Append(UserImageColorCreator.CreateImgColor(acctImage, member.UserColor, member.UserId, 24, member.SiteTheme) + userNameTitle);
                            strUsers2.Append("<div class='clear'></div></div>");
                        }
                    }
                }

                if (count == 0) {
                    strUsers.Append("<div id='noUsersdiv' class='pad-all'>No users associated with this group.</div>");
                }
            }
            else {
                strUsers.Append("<div id='noUsersdiv' class='pad-all'>No users associated with this group.</div>");
            }

            pnl_modalTitle.Controls.Add(new LiteralControl(strTitle.ToString()));
            pnl_users.Controls.Add(new LiteralControl(HelperMethods.TableAddRemove(strUsers2.ToString(), strUsers.ToString(), "Users Available to Join", "Associated Users", false)));
        }
        hf_viewusers.Value = string.Empty;
        updatepnl_viewusers.Update();

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'GroupEdit-element', 'Associated Users');");
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

            if (username.ToLower() != CurrentUsername.ToLower())
                SendNotification("<span style='color: #2F9E00;'>Added</span> to", groupname, username);

            if (string.IsNullOrEmpty(templist) && username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                Session["ReloadGroupModal"] = groupname;
                ServerSettings.RefreshPage(Page, string.Empty);
            }
        }

        hf_adduser.Value = string.Empty;

        if (!IsUserInAdminRole()) {
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

            if (username.ToLower() != CurrentUsername.ToLower())
                SendNotification("<span style='color: #D80000;'>Removed</span> from", groupname, username);
        }

        hf_removeuser.Value = string.Empty;

        if (!IsUserInAdminRole()) {
            BuildGroupList();
        }
    }

    protected void hf_refreshList_ValueChanged(object sender, EventArgs e) {
        BuildGroupList();

        StringBuilder _strScriptreg = new StringBuilder();
        if (!IsUserNameEqualToAdmin() && CurrentUserMemberDatabase.GroupList.Count == 0) {
            _strScriptreg.Append("$(\"body\").append(\"<h3 id='noapartofgroupmessage' style='position: fixed; bottom: 28px; right: 0; z-index: 10000; padding: 15px; background: #2F2F2F; color: #FFF;'>You must add yourself to a group</h3>\");");
        }
        else {
            _strScriptreg.Append("$(\"#noapartofgroupmessage\").remove();");
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
        RegisterPostbackScripts.RegisterStartupScript(this, "CreateGroup();");
    }
    private void ResetControls() {
        tb_companyname.Text = string.Empty;
        lbl_tempcompanyname.Text = string.Empty;
        tb_description.Text = string.Empty;
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

    private int CountUsersPerGroup(string groupname) {
        MembershipUserCollection coll = Membership.GetAllUsers();

        int count = 0;
        foreach (MembershipUser u in coll) {
            if (!BasePage.IsUserNameEqualToAdmin(u.UserName)) {
                MemberDatabase _temp = new MemberDatabase(u.UserName);

                List<string> groupList = new List<string>();
                if (CurrentUsername.ToLower() == u.UserName.ToLower()) {
                    groupList = _temp.GroupListForGroupOrgPage;
                }
                else {
                    groupList = _temp.GroupList;
                }
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

        string groupName = _groups.GetGroupName_byID(group);

        messagebody.Append("<p>You have been " + action + " <b>" + groupName + "</b> by " + HelperMethods.MergeFMLNames(CurrentUserMemberDatabase) + ".</p>");
        var un = new UserNotificationMessages(user);
        string email = un.attemptAdd(UserNotifications.GroupAlertID, messagebody.ToString(), true);
        if (!string.IsNullOrEmpty(email))
            mailTo.To.Add(email);

        UserNotificationMessages.finishAdd(mailTo, UserNotifications.GroupAlertID, messagebody.ToString());
    }
    private void SendInviteNotification(string id, string group, string user) {
        MailMessage mailTo = new MailMessage();
        var messagebody = new StringBuilder();
        messagebody.Append("<h3>Group Notification</h3><br />");

        string btnAccept = "<a href='#AcceptInvite' class='margin-right' onclick='openWSE.AcceptGroupNotification(this, \"" + id + "\");return false;'>Accept</a>";
        string btnDecline = "<a href='#DeclineInvite' class='margin-left' onclick='openWSE.NotiActionsHideInd(this);return false;'>Decline</a>";

        messagebody.Append("<p>" + HelperMethods.MergeFMLNames(CurrentUserMemberDatabase) + " has invited you to join the group <b>" + group + "</b>.<div class='clear-space-five'></div>" + btnAccept + btnDecline + "</p>");

        var un = new UserNotificationMessages(user);
        string email = un.attemptAdd(UserNotifications.GroupAlertID, messagebody.ToString(), true);
        if (!string.IsNullOrEmpty(email))
            mailTo.To.Add(email);

        UserNotificationMessages.finishAdd(mailTo, UserNotifications.GroupAlertID, messagebody.ToString());
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
            if ((!BasePage.IsUserNameEqualToAdmin(u.UserName)) && (u.UserName.ToLower() != CurrentUsername.ToLower()) && (!_groups.IsApartOfGroup(m.GroupList, groupId))) {
                UserNotifications notifications = new UserNotifications();
                if (notifications.IsUserNotificationEnabled(u.UserName, UserNotifications.GroupAlertID)) {
                    UserNotificationMessages unm = new UserNotificationMessages(u.UserName);
                    List<UserNotificationsMessage_Coll> messageList = unm.getEntries();

                    bool canAdd = true;

                    foreach (UserNotificationsMessage_Coll userMessages in messageList) {
                        if (userMessages.NotificationID == UserNotifications.GroupAlertID) {
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