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
using ICSharpCode.SharpZipLib.Zip;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Overlays;

public partial class SiteSettings_Overlays : System.Web.UI.Page {

    private readonly WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
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
                CheckQueryString();
                if ((_username.ToLower() == ServerSettings.AdminUserName.ToLower()) || (!_ss.OverlaysLocked)) {
                    ltl_locked.Text = "";

                    string _ctrlname = string.Empty;
                    ScriptManager sm = ScriptManager.GetCurrent(Page);
                    if (sm != null)
                        _ctrlname = sm.AsyncPostBackSourceElementID;

                    if ((!IsPostBack) || (_ctrlname == "hf_UpdateAll")) {
                        BuildAppList();
                        BuildOverlays();
                        BuildAppAssociation();
                    }
                }
                else {
                    pnl_AddControls.Enabled = false;
                    pnl_AddControls.Visible = false;
                    aAddNewOverlay.Visible = false;
                    BuildOverlays_NonEditable();
                    BuildAppAssociation_NonEditable();
                    ltl_locked.Text = HelperMethods.GetLockedByMessage();
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void CheckQueryString() {
        if (Request.QueryString["action"] == "uploadsuccess") {
            lbl_uploadMessage.Text = "Overlay added successfully!";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Green;
        }
        else if (Request.QueryString["action"] == "uploadfailed") {
            lbl_uploadMessage.Text = "Could not upload overlay. Please try again.";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Red;
        }
        else if (Request.QueryString["action"] == "deletesuccess") {
            lbl_uploadMessage.Text = "Overlay deleted successfully!";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Green;
        }
        else if (Request.QueryString["action"] == "deletefailed") {
            lbl_uploadMessage.Text = "Could not delete overlay. Please try again.";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Red;
        }
        RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function () { $('#lbl_uploadMessage').html(''); }, 3000);");
    }


    #region Build Overlay List

    private void BuildOverlays() {
        pnl_overlays.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='150px' align='left'>Overlay Name</td>");
        str.Append("<td align='left'>Description</td>");
        str.Append("<td width='130px'>Display Type</td>");
        str.Append("<td width='75px'>Actions</td></tr>");

        _workspaceOverlays.GetWorkspaceOverlays();
        int count = 0;
        foreach (WorkspaceOverlay_Coll coll in _workspaceOverlays.OverlayList) {
            string overlayId = coll.OverlayName;
            if (!string.IsNullOrEmpty(overlayId)) {
                if (!string.IsNullOrEmpty(coll.OverlayName)) {
                    if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UserName.ToLower() == _username.ToLower()) {
                        bool isEditMode = false;
                        str.Append("<tr class='myItemStyle GridNormalRow'>");
                        str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + (count + 1) + "</td>");
                        if (!string.IsNullOrEmpty(hf_EditOverlay.Value)) {
                            if (hf_EditOverlay.Value == coll.ID)
                                isEditMode = true;
                        }

                        if (!isEditMode) {
                            str.Append("<td align='left' class='border-right border-bottom'>" + coll.OverlayName + "</td>");

                            string desc = coll.Description;
                            if (string.IsNullOrEmpty(desc))
                                desc = "No description available";
                            str.Append("<td align='left' class='border-right border-bottom'>" + desc + "</td>");

                            string displayType = "Transparent";
                            if ((coll.DisplayType == "workspace-overlays") || (string.IsNullOrEmpty(coll.DisplayType))) {
                                displayType = "Solid Background";
                            }
                            else if (coll.DisplayType == "workspace-overlays-custom") {
                                displayType = "Custom / No Header";
                            }

                            string editButtons = BuildEditButtons(coll.ID, coll.OverlayName);
                            str.Append("<td align='center' class='border-right border-bottom'>" + displayType + "</td>");
                            str.Append("<td align='center' class='border-right border-bottom'>" + editButtons + "</td></tr>");
                        }
                        else {
                            str.Append("<td align='left' class='border-right border-bottom'><input type='text' id='tb_udpatename' class='textEntry' style='width: 95%;' value='" + coll.OverlayName + "' /></td>");
                            str.Append("<td align='left' class='border-right border-bottom'><input type='text' id='tb_udpatedesc' class='textEntry' style='width: 97%;' value='" + coll.Description + "' /></td>");

                            string dropdowndisplayType = BuildDropDownDisplayType(coll.ID, coll.DisplayType);
                            string editButtons = BuildUpdateButtons(coll.ID);
                            str.Append("<td align='center' class='border-right border-bottom'>" + dropdowndisplayType + "</td>");
                            str.Append("<td align='center' class='border-right border-bottom'>" + editButtons + "</td></tr>");
                        }
                        count++;
                    }
                }
            }
        }
        str.Append("</tbody></table></div>");
        if (count == 0)
            str.Append("<div class='emptyGridView'>No Overlays Uploaded</div>");

        lbl_overlaysEnabled.Text = count.ToString();
        pnl_overlays.Controls.Add(new LiteralControl(str.ToString()));
        updatepnl_overlays.Update();
    }

    private void BuildOverlays_NonEditable() {
        pnl_overlays.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='150px' align='left'>Overlay Name</td>");
        str.Append("<td align='left'>Description</td>");
        str.Append("<td width='130px'>Display Type</td>");
        str.Append("</tr>");

        _workspaceOverlays.GetWorkspaceOverlays();
        int count = 0;
        foreach (WorkspaceOverlay_Coll coll in _workspaceOverlays.OverlayList) {
            string overlayId = coll.OverlayName;
            if (!string.IsNullOrEmpty(overlayId)) {
                if (!string.IsNullOrEmpty(coll.OverlayName)) {
                    if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UserName.ToLower() == _username.ToLower()) {
                        str.Append("<tr class='myItemStyle GridNormalRow'>");
                        str.Append("<td class='GridViewNumRow border-bottom' width='45px' style='text-align: center'>" + (count + 1) + "</td>");
                        str.Append("<td align='left' class='border-right border-bottom'>" + coll.OverlayName + "</td>");

                        string desc = coll.Description;
                        if (string.IsNullOrEmpty(desc))
                            desc = "No description available";

                        string displayType = "Transparent";
                        if ((coll.DisplayType == "workspace-overlays") || (string.IsNullOrEmpty(coll.DisplayType))) {
                            displayType = "Solid Background";
                        }
                        else if (coll.DisplayType == "workspace-overlays-custom") {
                            displayType = "Custom / No Header";
                        }

                        str.Append("<td align='left' class='border-right border-bottom'>" + desc + "</td>");
                        str.Append("<td class='border-right border-bottom'>" + displayType + "</td>");
                        str.Append("</tr>");
                        count++;
                    }
                }
            }
        }
        str.Append("</tbody></table></div>");
        if (count == 0)
            str.Append("<div class='emptyGridView' style='width: 1039px;'>No Overlays Uploaded</div>");

        lbl_overlaysEnabled.Text = count.ToString();
        pnl_overlays.Controls.Add(new LiteralControl(str.ToString()));
        updatepnl_overlays.Update();
    }

    private static string UppercaseFirst(string s) {
        // Check for empty string.
        if (string.IsNullOrEmpty(s)) {
            return string.Empty;
        }
        // Return char and concat substring.
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    private string BuildDropDownDisplayType(string id, string displayType) {
        StringBuilder strDropDown = new StringBuilder();

        strDropDown.Append("<select id='dd_displayTypeUpdate'>");
        if (displayType == "workspace-overlays")
            strDropDown.Append("<option value='workspace-overlays' selected='selected'>Solid Background</option>");
        else
            strDropDown.Append("<option value='workspace-overlays'>Solid Background</option>");

        if (displayType == "workspace-overlays-nobg")
            strDropDown.Append("<option value='workspace-overlays-nobg' selected='selected'>Transparent</option>");
        else
            strDropDown.Append("<option value='workspace-overlays-nobg'>Transparent</option>");

        if (displayType == "workspace-overlays-custom")
            strDropDown.Append("<option value='workspace-overlays-custom' selected='selected'>Custom / No Header</option>");
        else
            strDropDown.Append("<option value='workspace-overlays-custom'>Custom / No Header</option>");


        strDropDown.Append("</select>");

        return strDropDown.ToString();
    }

    private string BuildEditButtons(string id, string overlayName) {
        StringBuilder str = new StringBuilder();

        str.Append("<a href='#edit' class='td-edit-btn margin-right' onclick='EditOverlay(\"" + id + "\");return false;' title='Edit'></a>");
        str.Append("<a href='#delete' class='td-delete-btn' onclick='DeleteOverlay(\"" + id + "\", \"" + overlayName + "\");return false;' title='Delete'></a>");

        return str.ToString();
    }

    private string BuildUpdateButtons(string id) {
        StringBuilder str = new StringBuilder();

        str.Append("<a href='#update' class='td-update-btn margin-right' onclick='UpdateOverlay(\"" + id + "\");return false;' title='Update'></a>");
        str.Append("<a href='#cancel' class='td-cancel-btn' onclick='EditOverlay(\"cancel\");return false;' title='Cancel'></a>");

        return str.ToString();
    }

    #endregion


    #region App Association

    private void BuildAppAssociation() {
        pnl_AppAssociation.Controls.Clear();
        var str = new StringBuilder();
        _workspaceOverlays.GetWorkspaceOverlays();
        foreach (WorkspaceOverlay_Coll coll in _workspaceOverlays.OverlayList) {
            if (!string.IsNullOrEmpty(coll.OverlayName)) {
                if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UserName.ToLower() == _username.ToLower()) {
                    str.Append("<div class='contact-card-main contact-card-main-category-packages'>");
                    str.Append("<a href='#edit' class='float-right td-edit-btn' onclick='EditAssociation(\"" + coll.ID + "\");return false;' title='Edit'></a>");
                    str.Append("<div class='float-left'><h2><b class='pad-right'>Overlay:</b><span style='font-weight: normal!important;'>" + coll.OverlayName + "</span></h2></div><div class='clear-space'></div>");
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
        _workspaceOverlays.GetWorkspaceOverlays();
        foreach (WorkspaceOverlay_Coll coll in _workspaceOverlays.OverlayList) {
            if (!string.IsNullOrEmpty(coll.OverlayName)) {
                if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName) || coll.UserName.ToLower() == _username.ToLower()) {
                    str.Append("<div class='contact-card-main contact-card-main-category-packages'>");
                    str.Append("<div class='float-left'><h2><b class='pad-right'>Overlay:</b><span style='font-weight: normal!important;'>" + coll.OverlayName + "</span></h2></div><div class='clear-space'></div>");
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
                string[] oIds = dr.OverlayID.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string oId in oIds) {
                    if (oId == id) {
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
        WorkspaceOverlay_Coll tempDob = _workspaceOverlays.GetWorkspaceOverlay(id);
        if (!string.IsNullOrEmpty(tempDob.ID)) {

            Dictionary<string, string> addTemp = new Dictionary<string, string>();
            Dictionary<string, string> removeTemp = new Dictionary<string, string>();

            lbl_typeEdit_Name.Text = "<h2><b class='pad-right'>Overlay:</b><span style='font-weight: normal!important;'>" + tempDob.OverlayName + "</span></h2>";
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
                    string[] oIds = dr.OverlayID.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    if (oIds.Length == 0) {
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
                        foreach (string oId in oIds) {
                            if (oId == id) {
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


    #region Upload Overlay

    protected void btn_uploadOverlay_Clicked(object sender, EventArgs e) {
        string action = "uploadfailed";
        if (((fileupload_Overlay.PostedFile != null) && (fileupload_Overlay.PostedFile.ContentLength > 0) && (!string.IsNullOrEmpty(fileupload_Overlay.PostedFile.FileName))) || (fileupload_Overlay.HasFile)) {
            string fileName = fileupload_Overlay.PostedFile.FileName;
            FileInfo fi = new FileInfo(fileName);

            string overlayName = txt_uploadOverlayName.Text.Trim();
            if (!string.IsNullOrEmpty(overlayName)) {
                overlayName = MakeValidFileName(overlayName);
                string ServerLoc = ServerSettings.GetServerMapLocation;
                try {
                    if (!Directory.Exists(ServerLoc + "Overlays")) {
                        Directory.CreateDirectory(ServerLoc + "Overlays");
                    }

                    #region ZIP FILES
                    if (fi.Extension.ToLower() == ".zip") {
                        if (HasAtLeastOneValidPage(fileupload_Overlay.FileContent)) {
                            fileupload_Overlay.FileContent.Position = 0;
                            int totalFiles = 0;
                            string overlayID = "";
                            string dllFolder = HelperMethods.RandomString(10);
                            using (Stream fileStreamIn = fileupload_Overlay.FileContent) {
                                using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn)) {
                                    ZipEntry entry;
                                    string tmpEntry = String.Empty;

                                    List<string> dlls = new List<string>();
                                    while ((entry = zipInStream.GetNextEntry()) != null) {
                                        if (totalFiles >= 2) {
                                            break;
                                        }

                                        string fn = Path.GetFileName(entry.Name);

                                        if (string.IsNullOrEmpty(fn)) {
                                            continue;
                                        }

                                        var tempfi = new FileInfo(fn);
                                        if ((tempfi.Extension.ToLower() != ".exe") && (tempfi.Extension.ToLower() != ".com") && (tempfi.Extension.ToLower() != ".pif")
                                             && (tempfi.Extension.ToLower() != ".bat") && (tempfi.Extension.ToLower() != ".scr")) {

                                            if ((fn != String.Empty) && (entry.Name.IndexOf(".ini") < 0)) {
                                                string en = Guid.NewGuid().ToString() + tempfi.Extension;
                                                string filePath = ServerSettings.GetServerMapLocation + "Overlays\\";

                                                if (tempfi.Extension.ToLower() == ".pdb") {
                                                    continue;
                                                }

                                                if ((tempfi.Extension.ToLower() == ".dll") || (tempfi.Extension.ToLower() == ".compiled")) {
                                                    filePath = ServerSettings.GetServerMapLocation + "Bin\\" + dllFolder + "\\";
                                                    if (!Directory.Exists(filePath)) {
                                                        Directory.CreateDirectory(filePath);
                                                        ServerSettings.AddRuntimeAssemblyBinding("Bin\\" + dllFolder);
                                                    }

                                                    en = tempfi.Name;
                                                    dlls.Add(en);
                                                }
                                                else {
                                                    if (tempfi.Extension.ToLower() == ".ascx") {
                                                        string fileLoc = "Overlays/" + en;
                                                        string description = txt_uploadOverlayDesc.Text.Trim();
                                                        overlayID = _workspaceOverlays.AddOverlay(_username, overlayName, fileLoc, description, dd_displayTypeNew.SelectedValue);
                                                        var apps = new App();
                                                        foreach (ListItem cbItem in cb_associatedOverlay.Items) {
                                                            if (cbItem.Selected) {
                                                                if ((!string.IsNullOrEmpty(cbItem.Value)) && (!string.IsNullOrEmpty(overlayID))) {
                                                                    string[] overlayList = apps.GetAppOverlayIds(cbItem.Value);
                                                                    string tempOverlays = string.Empty;
                                                                    foreach (string oId in overlayList) {
                                                                        tempOverlays += oId + ";";
                                                                    }
                                                                    if (!tempOverlays.Contains(overlayID)) {
                                                                        tempOverlays += overlayID + ";";
                                                                    }
                                                                    apps.UpdateAppOverlayID(cbItem.Value, tempOverlays);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else {
                                                        continue;
                                                    }
                                                }
                                                string fullPath = filePath + en;
                                                fullPath = fullPath.Replace("\\ ", "\\").Replace("/", "\\");

                                                totalFiles++;

                                                FileStream streamWriter = File.Create(fullPath);
                                                int size = 2048;
                                                byte[] data = new byte[2048];
                                                while (true) {
                                                    size = zipInStream.Read(data, 0, data.Length);
                                                    if (size > 0)
                                                        streamWriter.Write(data, 0, size);
                                                    else
                                                        break;
                                                }
                                                streamWriter.Close();
                                            }
                                        }
                                    }

                                    if (dlls.Count > 0) {
                                        OverlayDLLs overlayDLLs = new OverlayDLLs();
                                        overlayDLLs.AddItem(overlayID, dllFolder);
                                    }
                                }
                            }

                            action = "uploadsuccess";
                        }
                    }
                    #endregion

                    #region ASCX Files
                    else if (fi.Extension.ToLower() == ".ascx") {
                        string fileGuid = Guid.NewGuid().ToString() + fi.Extension;
                        string fileLoc = "Overlays/" + fileGuid;
                        string saveAs = "\\Overlays\\" + fileGuid;
                        fileupload_Overlay.SaveAs(ServerLoc + saveAs);
                        string description = txt_uploadOverlayDesc.Text.Trim();
                        string overlayID = _workspaceOverlays.AddOverlay(_username, overlayName, fileLoc, description, dd_displayTypeNew.SelectedValue);
                        var apps = new App();
                        foreach (ListItem cbItem in cb_associatedOverlay.Items) {
                            if (cbItem.Selected) {
                                if ((!string.IsNullOrEmpty(cbItem.Value)) && (!string.IsNullOrEmpty(overlayID))) {
                                    string[] overlayList = apps.GetAppOverlayIds(cbItem.Value);
                                    string tempOverlays = string.Empty;
                                    foreach (string oId in overlayList) {
                                        tempOverlays += oId + ";";
                                    }
                                    if (!tempOverlays.Contains(overlayID)) {
                                        tempOverlays += overlayID + ";";
                                    }
                                    apps.UpdateAppOverlayID(cbItem.Value, tempOverlays);
                                }
                            }
                        }
                        action = "uploadsuccess";
                    }
                    #endregion

                }
                catch { }
            }
        }

        ServerSettings.PageToolViewRedirect(this.Page, "Overlays.aspx?action=" + action + "&date=" + DateTime.Now.Ticks);
    }

    private static string MakeValidFileName(string name) {
        string invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
        string final = Regex.Replace(name, invalidReStr, "_");
        final = final.Replace(" ", "_");
        return final;
    }

    private void BuildAppList() {
        cb_associatedOverlay.Items.Clear();
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
                if (!cb_associatedOverlay.Items.Contains(item))
                    cb_associatedOverlay.Items.Add(item);
            }
        }
    }

    private bool HasAtLeastOneValidPage(Stream str) {
        bool returnVal = false;
        ZipInputStream zipInStream = new ZipInputStream(str);
        ZipEntry entry;
        while ((entry = zipInStream.GetNextEntry()) != null) {
            string fn = Path.GetFileName(entry.Name);
            var tempfi = new FileInfo(fn);
            if ((tempfi.Extension.ToLower() != ".exe") && (tempfi.Extension.ToLower() != ".com") && (tempfi.Extension.ToLower() != ".pif")
                 && (tempfi.Extension.ToLower() != ".bat") && (tempfi.Extension.ToLower() != ".scr")) {

                if ((fn != String.Empty) && (entry.Name.IndexOf(".ini") < 0)) {
                    string en = entry.Name;

                    if (tempfi.Extension.ToLower() == ".pdb") {
                        continue;
                    }

                    if (tempfi.Extension.ToLower() == ".ascx") {
                        returnVal = true;
                        break;
                    }
                    else {
                        continue;
                    }
                }
            }
        }

        zipInStream.Flush();

        return returnVal;
    }

    #endregion


    #region Edit/Delete

    protected void hf_EditOverlay_Changed(object sender, EventArgs e) {
        if (hf_EditOverlay.Value == "cancel")
            hf_EditOverlay.Value = "";

        BuildAppList();
        BuildOverlays();
    }

    protected void hf_UpdateNameOverlay_Changed(object sender, EventArgs e) {
        BuildAppList();
        string id = hf_EditOverlay.Value;
        if (!string.IsNullOrEmpty(id)) {
            if (!string.IsNullOrEmpty(hf_UpdateNameOverlay.Value.Trim()))
                _workspaceOverlays.UpdateOverlayName(id, hf_UpdateNameOverlay.Value.Trim());

            if (!string.IsNullOrEmpty(hf_UpdateDescOverlay.Value.Trim()))
                _workspaceOverlays.UpdateOverlayDescription(id, hf_UpdateDescOverlay.Value.Trim());

            if (!string.IsNullOrEmpty(hf_displayType.Value.Trim()))
                _workspaceOverlays.UpdateOverlayDisplayType(id, hf_displayType.Value.Trim());
        }

        hf_EditOverlay.Value = "";
        hf_UpdateDescOverlay.Value = "";
        hf_UpdateNameOverlay.Value = "";
        hf_displayType.Value = "";
        BuildOverlays();
        BuildAppAssociation();
    }

    protected void hf_DeleteOverlay_Changed(object sender, EventArgs e) {
        BuildAppList();
        string action = "deletefailed";
        string id = hf_DeleteOverlay.Value;
        if (!string.IsNullOrEmpty(id)) {
            try {
                string loc = _workspaceOverlays.GetWorkspaceFileLoc(id);
                FileInfo fi = new FileInfo(loc);
                string dir = fi.Directory.Name;
                string filename = ServerSettings.GetServerMapLocation + "Overlays\\" + fi.Name;
                if ((File.Exists(filename)) && (dir == "Overlays"))
                    File.Delete(filename);

                OverlayDLLs overlayDLLs = new OverlayDLLs();
                string folderPath = overlayDLLs.GetFolderPath(id);

                overlayDLLs.DeleteItem(id);
                ServerSettings.RemoveRuntimeAssemblyBinding("Bin\\" + folderPath);

                if (!string.IsNullOrEmpty(folderPath)) {
                    if (Directory.Exists(ServerSettings.GetServerMapLocation + "Bin\\" + folderPath)) {
                        Directory.Delete(ServerSettings.GetServerMapLocation + "Bin\\" + folderPath, true);
                    }
                }

                _workspaceOverlays.DeleteOverlay(id);
                action = "deletesuccess";
            }
            catch { }
        }

        ServerSettings.PageToolViewRedirect(this.Page, "Overlays.aspx?action=" + action + "&date=" + DateTime.Now.Ticks);
    }

    protected void hf_AssociationOverlay_Changed(object sender, EventArgs e) {
        BuildAppList();
        string script = "openWSE.LoadModalWindow(true, 'App-element', 'Associated Apps');";
        if (hf_AssociationOverlay.Value == "close") {
            script = string.Empty;
            Typelist.Controls.Clear();
            lbl_typeEdit_Name.Text = "";
        }
        else
            LoadAppIconsEdit(hf_AssociationOverlay.Value);

        hf_AssociationOverlay.Value = "";
        RegisterPostbackScripts.RegisterStartupScript(this, script);
    }

    protected void hf_addapp_ValueChanged(object sender, EventArgs e) {
        BuildAppList();
        var apps = new App();
        if (!string.IsNullOrEmpty(hf_addapp.Value)) {
            string[] oIds = apps.GetAppOverlayIds(hf_addapp.Value);
            string list = "";
            foreach (string oId in oIds) {
                list += oId + ";";
            }
            if (!list.Contains(hf_OverlayID.Value)) {
                list += hf_OverlayID.Value + ";";
            }

            apps.UpdateAppOverlayID(hf_addapp.Value, list);
        }

        hf_OverlayID.Value = "";
        hf_addapp.Value = "";
    }

    protected void hf_removeapp_ValueChanged(object sender, EventArgs e) {
        BuildAppList();
        var apps = new App();
        if (!string.IsNullOrEmpty(hf_removeapp.Value)) {
            string[] oIds = apps.GetAppOverlayIds(hf_removeapp.Value);
            string list = "";
            foreach (string oId in oIds) {
                if (oId != hf_OverlayID.Value) {
                    list += oId + ";";
                }
            }

            apps.UpdateAppOverlayID(hf_removeapp.Value, list);
        }

        hf_OverlayID.Value = "";
        hf_removeapp.Value = "";
    }

    protected void hf_refreshList_ValueChanged(object sender, EventArgs e) {
        BuildAppList();
        BuildAppAssociation();
        hf_refreshList.Value = string.Empty;
    }

    #endregion

}