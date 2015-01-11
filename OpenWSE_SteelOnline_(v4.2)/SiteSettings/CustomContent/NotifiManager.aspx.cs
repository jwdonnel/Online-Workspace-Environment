using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Text;
using System.Data;
using System.Web.Security;
using System.Text.RegularExpressions;
using System.IO;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.AutoUpdates;

public partial class SiteSettings_NotifiManager : System.Web.UI.Page {
    private readonly Notifications _notifications = new Notifications();
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private string _username;
    private bool AssociateWithGroups = false;
    private MemberDatabase _member;
    private List<Apps_Coll> tempAppColl = new List<Apps_Coll>();

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                AssociateWithGroups = _ss.AssociateWithGroups;

                _username = userId.Name;
                _member = new MemberDatabase(_username);

                lbl_uploadMessage.Text = "";

                if ((_username.ToLower() == ServerSettings.AdminUserName.ToLower()) || (!_ss.NotificationsLocked)) {
                    ltl_locked.Text = "";

                    string _ctrlname = string.Empty;
                    ScriptManager sm = ScriptManager.GetCurrent(Page);
                    if (sm != null)
                        _ctrlname = sm.AsyncPostBackSourceElementID;

                    if ((!_ctrlname.Contains("btn_uploadNotifi")) || (_ctrlname == "hf_UpdateAll")) {
                        BuildAppList();
                    }

                    if ((!IsPostBack) || (_ctrlname == "hf_UpdateAll")) {
                        BuildNotifi();
                        BuildAppAssociation();
                    }
                }
                else {
                    pnl_AddControls.Enabled = false;
                    pnl_AddControls.Visible = false;
                    lbtn_Refresh.Enabled = false;
                    lbtn_Refresh.Visible = false;
                    aAddNewNoti.Visible = false;
                    BuildNotifi_NonEditable();
                    BuildAppAssociation_NonEditable();
                    ltl_locked.Text = HelperMethods.GetLockedByMessage();
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void CheckQueryString(string action) {
        if (action == "uploadsuccess") {
            lbl_uploadMessage.Text = "Notification added successfully!";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Green;
        }
        else if (action == "uploadfailed") {
            lbl_uploadMessage.Text = "Could not add notification. Please try again.";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Red;
        }
        else if (action == "deletesuccess") {
            lbl_uploadMessage.Text = "Notification deleted successfully!";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Green;
        }
        else if (action == "deletefailed") {
            lbl_uploadMessage.Text = "Could not delete notification. Please try again.";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Red;
        }
        RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function () { $('#lbl_uploadMessage').html(''); }, 3000);");
    }


    #region Build Notification List

    private void BuildNotifi() {
        pnl_notifi.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='150px'>Notification Name</td>");
        str.Append("<td width='45px'>Image</td>");
        str.Append("<td>Description</td>");
        str.Append("<td width='75px'>Actions</td></tr>");

        _notifications.GetNotifications();
        int count = 0;

        List<string> nIds = new List<string>();

        foreach (Notifications_Coll coll in _notifications.NotificationList) {
            string notifiId = coll.ID;
            if (!string.IsNullOrEmpty(notifiId)) {
                if (!string.IsNullOrEmpty(coll.NotificationName)) {
                    if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UserName.ToLower() == _username.ToLower()) {
                        bool isEditMode = false;
                        str.Append("<tr class='myItemStyle GridNormalRow'>");
                        str.Append("<td class='GridViewNumRow border-bottom' width='45px' style='text-align: center'>" + (count + 1) + "</td>");
                        if (!string.IsNullOrEmpty(hf_EditNotifi.Value)) {
                            if (hf_EditNotifi.Value == coll.ID)
                                isEditMode = true;
                        }

                        if (!nIds.Contains(coll.ID)) {
                            nIds.Add(coll.ID);
                        }

                        if (!isEditMode) {
                            str.Append("<td align='left' width='150px' class='border-right border-bottom'>" + coll.NotificationName + "</td>");

                            string notifiImg = coll.NotificationImage;
                            if (notifiImg.IndexOf("~/") == 0)
                                notifiImg = "../../" + notifiImg.Substring(2);

                            str.Append("<td align='center' width='45px' class='border-right border-bottom'><img alt='' src='" + notifiImg + "' style='height: 25px;' /></td>");

                            string desc = coll.Description;
                            if (string.IsNullOrEmpty(desc))
                                desc = "No description available";
                            str.Append("<td align='left' class='border-right border-bottom'>" + desc + "</td>");

                            string editButtons = BuildEditButtons(coll.ID, coll.NotificationName);
                            str.Append("<td width='75px' align='center' class='border-right border-bottom'>" + editButtons + "</td></tr>");
                        }
                        else {
                            str.Append("<td align='left' width='150px' class='border-right border-bottom'><input type='text' id='tb_udpatename' class='textEntry' style='width: 95%;' value='" + coll.NotificationName + "' /></td>");
                            str.Append("<td align='left' width='200px' class='border-right border-bottom'><input type='text' id='tb_udpateImg' class='textEntry' style='width: 97%;' value='" + coll.NotificationImage + "' /></td>");
                            str.Append("<td align='left' class='border-right border-bottom'><input type='text' id='tb_udpatedesc' class='textEntry' style='width: 97%;' value='" + coll.Description + "' /></td>");

                            string editButtons = BuildUpdateButtons(coll.ID);
                            str.Append("<td width='75px' align='center' class='border-right border-bottom'>" + editButtons + "</td></tr>");
                        }
                        count++;
                    }
                }
            }
        }

        bool showRefresh = true;
        if ((nIds.Contains("1159aca6-2449-4aff-bacb-5f29e479e2d7")) && (nIds.Contains("adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f"))) {
            showRefresh = false;
        }

        if (showRefresh && Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            lbtn_Refresh.Enabled = true;
            lbtn_Refresh.Visible = true;
        }
        else {
            lbtn_Refresh.Enabled = false;
            lbtn_Refresh.Visible = false;
        }

        str.Append("</tbody></table></div>");
        if (count == 0)
            str.Append("<div class='emptyGridView'>No Notifications Added</div>");

        lbl_notifiEnabled.Text = count.ToString();
        pnl_notifi.Controls.Add(new LiteralControl(str.ToString()));
        updatepnl_notifi.Update();
    }

    private void BuildNotifi_NonEditable() {
        pnl_notifi.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='150px'>Notification Name</td>");
        str.Append("<td width='45px'>Image</td>");
        str.Append("<td>Description</td>");
        str.Append("</tr>");

        _notifications.GetNotifications();
        int count = 0;
        foreach (Notifications_Coll coll in _notifications.NotificationList) {
            string notifiId = coll.ID;
            if (!string.IsNullOrEmpty(notifiId)) {
                if (!string.IsNullOrEmpty(coll.NotificationName)) {
                    if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UserName.ToLower() == _username.ToLower()) {
                        str.Append("<tr class='myItemStyle GridNormalRow'>");
                        str.Append("<td class='GridViewNumRow border-bottom' width='45px' style='text-align: center'>" + (count + 1) + "</td>");

                        str.Append("<td align='left' width='150px' class='border-right border-bottom'>" + coll.NotificationName + "</td>");

                        string notifiImg = coll.NotificationImage;
                        if (notifiImg.IndexOf("~/") == 0)
                            notifiImg = "../../" + notifiImg.Substring(2);

                        str.Append("<td align='center' width='45px' class='border-right border-bottom'><img alt='' src='" + notifiImg + "' style='height: 25px;' /></td>");

                        string desc = coll.Description;
                        if (string.IsNullOrEmpty(desc))
                            desc = "No description available";
                        str.Append("<td align='left' class='border-right border-bottom'>" + desc + "</td>");
                        str.Append("</tr>");
                        count++;
                    }
                }
            }
        }
        str.Append("</tbody></table></div>");
        if (count == 0)
            str.Append("<div class='emptyGridView' style='width: 1039px;'>No Notifications Added</div>");

        lbl_notifiEnabled.Text = count.ToString();
        pnl_notifi.Controls.Add(new LiteralControl(str.ToString()));
        updatepnl_notifi.Update();
    }

    private string BuildEditButtons(string id, string name) {
        StringBuilder str = new StringBuilder();

        str.Append("<a href='#edit' class='td-edit-btn margin-right' onclick='EditNotifi(\"" + id + "\");return false;' title='Edit'></a>");
        str.Append("<a href='#delete' class='td-delete-btn' onclick='DeleteNotifi(\"" + id + "\", \"" + name + "\");return false;' title='Delete'></a>");

        return str.ToString();
    }

    private string BuildUpdateButtons(string id) {
        StringBuilder str = new StringBuilder();

        str.Append("<a href='#update' class='td-update-btn margin-right' onclick='UpdateNotifi(\"" + id + "\");return false;' title='Update'></a>");
        str.Append("<a href='#cancel' class='td-cancel-btn' onclick='EditNotifi(\"cancel\");return false;' title='Cancel'></a>");

        return str.ToString();
    }

    protected void lbtn_Refresh_Clicked(object sender, EventArgs e) {
        // create eRequest notification if not available
        if (string.IsNullOrEmpty(_notifications.GetNotification("1159aca6-2449-4aff-bacb-5f29e479e2d7").ID))
            _notifications.AddNotification(_username, "eRequests", "~/Standard_Images/Notifications/email.png", "Displays the emails from the eRequest system.", "1159aca6-2449-4aff-bacb-5f29e479e2d7");

        // create New Group notification if not available
        if (string.IsNullOrEmpty(_notifications.GetNotification("adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f").ID))
            _notifications.AddNotification(_username, "Group Alerts", "~/Standard_Images/Notifications/group.png", "Alerts you when you've been added/removed or invited to a group. (If turned off, you will not get invites to groups)", "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f");

        // create Error Report notification if not available
        if (string.IsNullOrEmpty(_notifications.GetNotification("236a9dc9-c92a-437f-8825-27809af36a3f").ID))
            _notifications.AddNotification(_username, "Error Report", "~/Standard_Images/Notifications/error-lg-color.png", "Add alert when an error occurs on the site. (For Administrators only)", "236a9dc9-c92a-437f-8825-27809af36a3f");

        BuildNotifi();
        BuildAppAssociation();
    }

    #endregion


    #region App Association

    private void BuildAppAssociation() {
        pnl_AppAssociation.Controls.Clear();
        var str = new StringBuilder();
        _notifications.GetNotifications();
        foreach (Notifications_Coll coll in _notifications.NotificationList) {
            if (!string.IsNullOrEmpty(coll.NotificationName)) {
                if ((coll.ID == "236a9dc9-c92a-437f-8825-27809af36a3f") || (coll.ID == "1159aca6-2449-4aff-bacb-5f29e479e2d7") || (coll.ID == "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f")) {
                    continue;
                }

                if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UserName.ToLower() == _username.ToLower()) {
                    str.Append("<div class='contact-card-main contact-card-main-category-packages'>");
                    str.Append("<a href='#edit' class='float-right td-edit-btn' onclick='EditAssociation(\"" + coll.ID + "\");return false;' title='Edit'></a>");
                    str.Append("<div class='float-left'><h2><b class='pad-right'>Notification:</b><span style='font-weight: normal!important;'>" + coll.NotificationName + "</span></h2></div><div class='clear-space'></div>");
                    str.Append("<div class='clear'></div>");
                    str.Append("<div class='clear-margin package-contents pad-top pad-bottom'>" + LoadAppIcons(coll.ID) + "</div>");
                    str.Append("</div>");
                }
            }
        }
        pnl_AppAssociation.Controls.Add(new LiteralControl(str.ToString()));
        updatepnl_Associations.Update();
    }

    private void BuildAppAssociation_NonEditable() {
        pnl_AppAssociation.Controls.Clear();
        var str = new StringBuilder();
        _notifications.GetNotifications();
        foreach (Notifications_Coll coll in _notifications.NotificationList) {
            if (!string.IsNullOrEmpty(coll.NotificationName)) {
                if ((_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && ((coll.ID == "236a9dc9-c92a-437f-8825-27809af36a3f") || (coll.ID == "1159aca6-2449-4aff-bacb-5f29e479e2d7") || (coll.ID == "adaefeb2-9ef2-4ffa-b6ca-c76fc2815d4f"))) {
                    continue;
                }

                if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UserName.ToLower() == _username.ToLower()) {
                    str.Append("<div class='contact-card-main contact-card-main-category-packages'>");
                    str.Append("<div class='float-left'><h2><b class='pad-right'>Notification:</b><span style='font-weight: normal!important;'>" + coll.NotificationName + "</span></h2></div><div class='clear-space'></div>");
                    str.Append("<div class='clear'></div>");
                    str.Append("<div class='clear-margin package-contents pad-top pad-bottom'>" + LoadAppIcons(coll.ID) + "</div>");
                    str.Append("</div>");
                }
            }
        }
        pnl_AppAssociation.Controls.Add(new LiteralControl(str.ToString()));
        updatepnl_Associations.Update();
    }

    private string LoadAppIcons(string id) {
        var apps = new App();

        if (tempAppColl.Count == 0) {
            apps.GetAllApps();
            tempAppColl = apps.AppList;
        }

        var appScript = new StringBuilder();
        foreach (Apps_Coll dr in tempAppColl) {
            bool cancontinue = false;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (dr.CreatedBy.ToLower() == _username.ToLower()) {
                    cancontinue = true;
                }
            }
            else
                cancontinue = true;

            if ((_username.ToLower() != dr.CreatedBy.ToLower()) && (dr.IsPrivate) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                cancontinue = false;
            }

            if (!cancontinue) continue;
            if (apps.IconExists(dr.AppId)) {
                string[] nIds = dr.NotificationID.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string nId in nIds) {
                    if (nId == id) {
                        string name = dr.AppName;
                        string icon = dr.Icon;
                        string image = "<img alt='icon' src='../../Standard_Images/App_Icons/" + icon + "' style='height: 25px; padding-right: 7px;' />";

                        if ((string.IsNullOrEmpty(icon)) || (_ss.HideAllAppIcons))
                            image = string.Empty;

                        appScript.Append("<div class='app-icon-admin inline-block' style='padding: 0 !important;'>" + image);
                        appScript.Append("<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 182px;'>" + name + "</span></div>");
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(appScript.ToString()))
            return appScript.ToString();

        return "<i class='font-color-gray'>No associated apps</i><div class='clear-space-five'></div>";
    }

    private void LoadAppIconsEdit(string id) {
        var apps = new App();

        Notifications_Coll tempDob = _notifications.GetNotification(id);
        if (!string.IsNullOrEmpty(tempDob.ID)) {

            Dictionary<string, string> addTemp = new Dictionary<string, string>();
            Dictionary<string, string> removeTemp = new Dictionary<string, string>();

            lbl_typeEdit_Name.Text = "<h2><b class='pad-right'>Notification:</b><span style='font-weight: normal!important;'>" + tempDob.NotificationName + "</span></h2>";
            foreach (Apps_Coll dr in tempAppColl) {
                bool cancontinue = false;
                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    if (dr.CreatedBy.ToLower() == _username.ToLower()) {
                        cancontinue = true;
                    }
                }
                else
                    cancontinue = true;

                if ((_username.ToLower() != dr.CreatedBy.ToLower()) && (dr.IsPrivate) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                    cancontinue = false;
                }

                if (!cancontinue) continue;
                if (apps.IconExists(dr.AppId)) {
                    string name = dr.AppName;
                    string icon = dr.Icon;
                    string image = "<img alt='icon' src='../../Standard_Images/App_Icons/" + icon + "' style='height: 25px; padding-right: 7px;' />";

                    if ((string.IsNullOrEmpty(icon)) || (_ss.HideAllAppIcons))
                        image = string.Empty;

                    string _appId = dr.AppId;
                    string[] nIds = dr.NotificationID.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    if (nIds.Length == 0) {
                        if (!removeTemp.ContainsKey(_appId)) {
                            StringBuilder appScript = new StringBuilder();
                            appScript.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin inline-block' style='padding: 0 !important;'>" + image);
                            appScript.Append("<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 190px;'>" + dr.AppName);
                            appScript.Append("<a href='#' onclick=\"AddAssociation(this, '" + _appId + "');return false;\" title='Add " + dr.AppName + "'>");
                            appScript.Append("<div title='Add' class='img-expand-sml cursor-pointer float-left'></div></a></span></div>");
                            removeTemp.Add(_appId, appScript.ToString());
                        }
                    }
                    else {
                        foreach (string nId in nIds) {
                            if (nId == id) {
                                if (!addTemp.ContainsKey(_appId)) {
                                    StringBuilder appScript = new StringBuilder();
                                    appScript.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin inline-block' style='padding: 0 !important;'>" + image);
                                    appScript.Append("<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 190px;'>" + dr.AppName);
                                    appScript.Append("<a href='#' onclick=\"RemoveAssociation(this, '" + _appId + "');return false;\" title='Remove " + dr.AppName + "'>");
                                    appScript.Append("<div title='Remove' class='img-collapse-sml cursor-pointer float-left'></div></a></span></div>");
                                    addTemp.Add(_appId, appScript.ToString());
                                }
                            }
                            else {
                                if (!removeTemp.ContainsKey(_appId)) {
                                    StringBuilder appScript = new StringBuilder();
                                    appScript.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin inline-block' style='padding: 0 !important;'>" + image);
                                    appScript.Append("<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 190px;'>" + dr.AppName);
                                    appScript.Append("<a href='#' onclick=\"AddAssociation(this, '" + _appId + "');return false;\" title='Add " + dr.AppName + "'>");
                                    appScript.Append("<div title='Add' class='img-expand-sml cursor-pointer float-left'></div></a></span></div>");
                                    removeTemp.Add(_appId, appScript.ToString());
                                }
                            }
                        }
                    }
                }
            }

            StringBuilder addScript = new StringBuilder();
            StringBuilder removeScript = new StringBuilder();

            foreach (KeyValuePair<string, string> kvp in removeTemp) {
                if (!addTemp.ContainsKey(kvp.Key)) {
                    removeScript.Append(kvp.Value);
                }
            }

            foreach (KeyValuePair<string, string> kvp in addTemp) {
                addScript.Append(kvp.Value);
            }

            string strAdded = "<div id='package-added'>" + addScript + "</div>";
            string strRemoved = "<div id='package-removed'>" + removeScript + "</div>";
            Typelist.Controls.Clear();
            Typelist.Controls.Add(new LiteralControl(strAdded + "<div class='clear' style='height: 30px;'></div>" + strRemoved + "</div>"));
            updatepnl_AssociationsEdit.Update();
        }
    }

    #endregion


    #region Add Notification

    protected void btn_uploadNotifi_Clicked(object sender, EventArgs e) {
        string action = "uploadfailed";
        string name = txt_uploadNotifiName.Text.Trim();
        if (!string.IsNullOrEmpty(name)) {
            string description = txt_uploadNotifiDesc.Text.Trim();

            string img = txt_uploadNofiImg.Text;
            if (cb_UseAppImg.Checked) {
                App w = new App();
                foreach (ListItem item in cb_associatedNoti.Items) {
                    if (item.Selected) {
                        string x = w.GetAppIconName(item.Value);
                        img = "~/Standard_Images/App_Icons/" + x;
                        break;
                    }
                }
            }

            string notifiID = _notifications.AddNotification(_username, name, img, description);
            var apps = new App();

            string list = "";
            foreach (ListItem item in cb_associatedNoti.Items) {
                if (item.Selected) {
                    if ((!string.IsNullOrEmpty(item.Value)) && (!string.IsNullOrEmpty(notifiID))) {
                        string[] notiList = apps.GetAppNotificationIds(item.Value);
                        string tempNotis = string.Empty;
                        foreach (string nId in notiList) {
                            tempNotis += nId + ";";
                        }
                        if (!tempNotis.Contains(notifiID)) {
                            tempNotis += notifiID + ";";
                        }
                        apps.UpdateAppNotificationID(item.Value, tempNotis);
                    }
                }
            }
            action = "uploadsuccess";
        }

        cb_UseAppImg.Checked = false;
        txt_uploadNofiImg.Text = "";
        txt_uploadNotifiDesc.Text = "";
        txt_uploadNotifiName.Text = "";
        BuildAppList();
        BuildNotifi();
        BuildAppAssociation();
        CheckQueryString(action);

        if (action == "uploadsuccess") {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'AddNotification-element', '');");
        }
    }
    private void BuildAppList() {
        cb_associatedNoti.Items.Clear();

        var apps = new App(_username);
        apps.GetAllApps();
        tempAppColl.Clear();
        List<string> enabledApps = _member.EnabledApps;
        foreach (Apps_Coll dr in apps.AppList) {

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(dr, _member)) {
                    continue;
                }
            }

            tempAppColl.Add(dr);

            string name = dr.AppName;
            string id = dr.AppId;
            if ((Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) || ((enabledApps.Contains(id)))) {
                ListItem item = new ListItem(name, id);
                if (!cb_associatedNoti.Items.Contains(item))
                    cb_associatedNoti.Items.Add(item);
            }
        }
    }

    #endregion


    #region Edit/Delete

    protected void hf_EditNotifi_Changed(object sender, EventArgs e) {
        if (hf_EditNotifi.Value == "cancel")
            hf_EditNotifi.Value = "";

        BuildNotifi();
    }

    protected void hf_UpdateNameNotifi_Changed(object sender, EventArgs e) {
        string id = hf_EditNotifi.Value;
        if (!string.IsNullOrEmpty(id)) {
            if (!string.IsNullOrEmpty(hf_UpdateNameNotifi.Value.Trim()))
                _notifications.UpdateNotificationName(id, hf_UpdateNameNotifi.Value.Trim());

            if (!string.IsNullOrEmpty(hf_UpdateDescNotifi.Value.Trim()))
                _notifications.UpdateNotificationDescription(id, hf_UpdateDescNotifi.Value.Trim());

            if (!string.IsNullOrEmpty(hf_UpdateImgNotifi.Value.Trim()))
                _notifications.UpdateNotificationImg(id, hf_UpdateImgNotifi.Value.Trim());
        }

        hf_EditNotifi.Value = "";
        hf_UpdateDescNotifi.Value = "";
        hf_UpdateNameNotifi.Value = "";
        hf_UpdateImgNotifi.Value = "";
        BuildNotifi();
        BuildAppAssociation();
    }

    protected void hf_DeleteNotifi_Changed(object sender, EventArgs e) {
        string action = "deletefailed";
        string id = hf_DeleteNotifi.Value;
        if (!string.IsNullOrEmpty(id)) {
            _notifications.DeleteNotification(id);
            action = "deletesuccess";
        }

        if (id == "236a9dc9-c92a-437f-8825-27809af36a3f") {
            ServerSettings.update_EmailActivity(false);
        }

        hf_DeleteNotifi.Value = "";
        BuildNotifi();
        BuildAppAssociation();

        CheckQueryString(action);
    }

    protected void hf_AssociationNotifi_Changed(object sender, EventArgs e) {
        string script = "openWSE.LoadModalWindow(true, 'App-element', 'Associated Apps');";
        if (hf_AssociationNotifi.Value == "close") {
            script = string.Empty;
            Typelist.Controls.Clear();
            lbl_typeEdit_Name.Text = "";
        }
        else
            LoadAppIconsEdit(hf_AssociationNotifi.Value);

        hf_AssociationNotifi.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, script);
    }

    protected void hf_addapp_ValueChanged(object sender, EventArgs e) {
        var apps = new App();
        if (!string.IsNullOrEmpty(hf_addapp.Value)) {
            string[] oIds = apps.GetAppNotificationIds(hf_addapp.Value);
            string list = "";
            foreach (string oId in oIds) {
                list += oId + ";";
            }
            if (!list.Contains(hf_NotifiID.Value)) {
                list += hf_NotifiID.Value + ";";
            }

            apps.UpdateAppNotificationID(hf_addapp.Value, list);
        }

        hf_NotifiID.Value = "";
        hf_addapp.Value = "";
    }

    protected void hf_removeapp_ValueChanged(object sender, EventArgs e) {
        var apps = new App();
        if (!string.IsNullOrEmpty(hf_removeapp.Value)) {
            string[] oIds = apps.GetAppNotificationIds(hf_removeapp.Value);
            string list = "";
            foreach (string oId in oIds) {
                if (oId != hf_NotifiID.Value) {
                    list += oId + ";";
                }
            }

            apps.UpdateAppNotificationID(hf_removeapp.Value, list);
        }

        hf_NotifiID.Value = "";
        hf_removeapp.Value = "";
    }

    protected void hf_refreshList_ValueChanged(object sender, EventArgs e) {
        BuildAppAssociation();
        hf_refreshList.Value = string.Empty;
    }

    #endregion

}