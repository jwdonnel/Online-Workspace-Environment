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

public partial class SiteTools_OverlayManager : BasePage {

    private App CurrentAppObject = new App(string.Empty);
    private readonly WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private List<Apps_Coll> tempAppColl = new List<Apps_Coll>();

    protected void Page_Load(object sender, EventArgs e) {
        lbl_uploadMessage.Text = "";
        CheckQueryString();

        if (!MainServerSettings.OverlaysLocked) {
            ltl_locked.Text = "";
            if (!IsPostBack || PostbackControlIsUpdateAll()) {
                BuildAppList();
                BuildOverlays();
            }
        }
        else {
            pnl_AddControls.Enabled = false;
            pnl_AddControls.Visible = false;
            aAddNewOverlay.Visible = false;
            BuildOverlays();
            ltl_locked.Text = HelperMethods.GetLockedByMessage();
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
    }


    #region Build Overlay List

    private void BuildOverlays() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, !MainServerSettings.OverlaysLocked, 2);

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Name", "200", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Description", "350", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Display Type", "130", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Associated Apps", "475", true));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        _workspaceOverlays.GetWorkspaceOverlays();
        int count = 0;
        foreach (WorkspaceOverlay_Coll coll in _workspaceOverlays.OverlayList) {
            string overlayId = coll.OverlayName;
            if (!string.IsNullOrEmpty(overlayId)) {
                if (!string.IsNullOrEmpty(coll.OverlayName)) {
                    if (IsUserInAdminRole() || coll.UserName.ToLower() == CurrentUsername.ToLower()) {
                        bool isEditMode = false;
                        if (!string.IsNullOrEmpty(hf_EditOverlay.Value)) {
                            if (hf_EditOverlay.Value == coll.ID) {
                                isEditMode = true;
                            }
                        }

                        string overlayName = coll.OverlayName;
                        string description = coll.Description;
                            if (string.IsNullOrEmpty(description)) {
                                description = "No description available";
                            }
                        string displayType = "Transparent";
                        if ((coll.DisplayType == "workspace-overlays") || (string.IsNullOrEmpty(coll.DisplayType))) {
                            displayType = "Solid Background";
                        }
                        else if (coll.DisplayType == "workspace-overlays-custom") {
                            displayType = "Custom / No Header";
                        }
                        string editButtons = BuildEditButtons(coll.ID, coll.OverlayName);

                        if (isEditMode) {
                            string overlayNameEdit = "<div class='input-settings-holder'><span class='font-bold'>Name</span><div class='clear-space-two'></div><input type='text' id='tb_udpatename' class='textEntry-noWidth' style='width: 100%;' value='" + coll.OverlayName + "' /></div>";
                            string descriptionEdit = "<div class='input-settings-holder'><span class='font-bold'>Description</span><div class='clear-space-two'></div><input type='text' id='tb_udpatedesc' class='textEntry-noWidth' style='width: 100%;' value='" + coll.Description + "' /></div>";
                            string displayTypeEdit = "<div class='input-settings-holder'><span class='font-bold'>Display Type</span><div class='clear-space-two'></div>" + BuildDropDownDisplayType(coll.ID, coll.DisplayType) + "</div>";

                            pnl_editControls.Controls.Clear();
                            pnl_editControls.Controls.Add(new LiteralControl(overlayNameEdit + descriptionEdit + displayTypeEdit));
                            LoadAppIconsEdit(coll.ID);
                            RegisterPostbackScripts.RegisterStartupScript(this, "EditAssociation('" + coll.ID + "');");
                        }

                        List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Name", overlayName, TableBuilderColumnAlignment.Left));
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Description", description, TableBuilderColumnAlignment.Left));
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Display Type", displayType, TableBuilderColumnAlignment.Left));

                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Associated Apps", LoadAppIcons(coll.ID), TableBuilderColumnAlignment.Left));
                        tableBuilder.AddBodyRow(bodyColumnValues, editButtons);

                        count++;
                    }
                }
            }
        }
        #endregion

        pnl_overlays.Controls.Clear();
        pnl_overlays.Controls.Add(tableBuilder.CompleteTableLiteralControl("No overlays found"));

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

        str.Append("<a class='td-edit-btn' onclick='EditOverlay(\"" + id + "\");return false;' title='Edit'></a>");
        str.Append("<a class='td-delete-btn' onclick='DeleteOverlay(\"" + id + "\", \"" + overlayName + "\");return false;' title='Delete'></a>");

        return str.ToString();
    }


    #endregion


    #region App Association

    private string LoadAppIcons(string id) {
        CurrentAppObject = new App(string.Empty);

        if (tempAppColl.Count == 0) {
            CurrentAppObject.GetAllApps();
            tempAppColl = CurrentAppObject.AppList;
        }

        var appScript = new StringBuilder();
        foreach (Apps_Coll dr in tempAppColl) {
            bool cancontinue = false;
            if (!IsUserInAdminRole()) {
                if (dr.CreatedBy.ToLower() == CurrentUsername.ToLower()) {
                    cancontinue = true;
                }
            }
            else
                cancontinue = true;

            if (string.IsNullOrEmpty(dr.ID)) {
                continue;
            }

            if (CurrentUsername.ToLower() != dr.CreatedBy.ToLower() && dr.IsPrivate && !IsUserNameEqualToAdmin()) {
                cancontinue = false;
            }

            if (!cancontinue) continue;
            string[] oIds = dr.OverlayID.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string oId in oIds) {
                if (oId == id) {
                    string name = dr.AppName;
                    string image = "<img alt='icon' src='" + ResolveUrl("~/" + dr.Icon) + "' class='app-icon-admin-icon' />";
                    if (string.IsNullOrEmpty(dr.Icon)) {
                        image = string.Empty;
                    }

                    appScript.Append("<div class='app-icon-admin'>" + image);
                    appScript.Append("<span class='app-span-modify'>" + name + "</span><div class='clear'></div></div>");
                }
            }
        }

        if (!string.IsNullOrEmpty(appScript.ToString()))
            return appScript.ToString();

        return "<i class='font-color-gray'>No associated apps</i><div class='clear-space-five'></div>";
    }
    private void LoadAppIconsEdit(string id) {
        CurrentAppObject = new App(string.Empty);
        WorkspaceOverlay_Coll tempDob = _workspaceOverlays.GetWorkspaceOverlay(id);
        if (!string.IsNullOrEmpty(tempDob.ID)) {

            Dictionary<string, string> addTemp = new Dictionary<string, string>();
            Dictionary<string, string> removeTemp = new Dictionary<string, string>();

            if (tempAppColl.Count == 0) {
                CurrentAppObject.GetAllApps();
                tempAppColl = CurrentAppObject.AppList;
            }

            lbl_typeEdit_Name.Text = "<h2><span style='font-weight: normal!important;'>" + tempDob.OverlayName + "</span></h2>";
            foreach (Apps_Coll dr in tempAppColl) {
                bool cancontinue = false;
                if (!IsUserInAdminRole()) {
                    if (dr.CreatedBy.ToLower() == CurrentUsername.ToLower()) {
                        cancontinue = true;
                    }
                }
                else
                    cancontinue = true;

                if (string.IsNullOrEmpty(dr.ID)) {
                    continue;
                }

                if (CurrentUsername.ToLower() != dr.CreatedBy.ToLower() && dr.IsPrivate && !IsUserNameEqualToAdmin()) {
                    cancontinue = false;
                }

                if (!cancontinue) continue;
                string name = dr.AppName;
                string image = "<img alt='icon' src='" + ResolveUrl("~/" + dr.Icon) + "' class='app-icon-admin-icon' />";
                if (string.IsNullOrEmpty(dr.Icon)) {
                    image = string.Empty;
                }

                string _appId = dr.AppId;
                string[] oIds = dr.OverlayID.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                if (oIds.Length == 0) {
                    if (!removeTemp.ContainsKey(_appId)) {
                        StringBuilder appScript = new StringBuilder();
                        appScript.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin'>");
                        appScript.Append("<a onclick=\"AddAssociation(this, '" + dr.AppId + "');return false;\" title='Add " + dr.AppName + "' class='float-left img-expand-sml cursor-pointer'></a>");
                        appScript.Append(image + "<span class='app-span-modify'>" + dr.AppName + "</span>");
                        appScript.Append("<div class='clear'></div></div>");
                        appScript.Append("<div class='clear'></div>");
                        removeTemp.Add(_appId, appScript.ToString());
                    }
                }
                else {
                    foreach (string oId in oIds) {
                        if (oId == id) {
                            if (!addTemp.ContainsKey(_appId)) {
                                StringBuilder appScript = new StringBuilder();
                                appScript.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin'>");
                                appScript.Append("<a onclick=\"RemoveAssociation(this, '" + dr.AppId + "');return false;\" title='Remove " + dr.AppName + "' class='float-left img-collapse-sml cursor-pointer'></a>");
                                appScript.Append(image + "<span class='app-span-modify'>" + dr.AppName + "</span>");
                                appScript.Append("<div class='clear'></div></div>");
                                appScript.Append("<div class='clear'></div>");
                                addTemp.Add(_appId, appScript.ToString());
                            }
                        }
                        else {
                            if (!removeTemp.ContainsKey(_appId)) {
                                StringBuilder appScript = new StringBuilder();
                                appScript.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin'>");
                                appScript.Append("<a onclick=\"AddAssociation(this, '" + dr.AppId + "');return false;\" title='Add " + dr.AppName + "' class='float-left img-expand-sml cursor-pointer'></a>");
                                appScript.Append(image + "<span class='app-span-modify'>" + dr.AppName + "</span>");
                                appScript.Append("<div class='clear'></div></div>");
                                appScript.Append("<div class='clear'></div>");
                                removeTemp.Add(_appId, appScript.ToString());
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

            Typelist.Controls.Clear();
            string table = HelperMethods.TableAddRemove(removeScript.ToString(), addScript.ToString(), "App Available to Add", "Associated Apps", false);
            Typelist.Controls.Add(new LiteralControl(table));
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

            string mainFolder = HelperMethods.RandomString(10);

            string overlayName = txt_uploadOverlayName.Text.Trim();
            if (!string.IsNullOrEmpty(overlayName)) {
                overlayName = MakeValidFileName(overlayName);
                string ServerLoc = ServerSettings.GetServerMapLocation;
                try {
                    if (!Directory.Exists(ServerLoc + "Overlays\\" + mainFolder)) {
                        Directory.CreateDirectory(ServerLoc + "Overlays\\" + mainFolder);
                    }

                    #region ZIP FILES
                    if (fi.Extension.ToLower() == ".zip") {
                        if (HasAtLeastOneValidPage(fileupload_Overlay.FileContent)) {
                            fileupload_Overlay.FileContent.Position = 0;
                            string overlayID = "";
                            string dllFolder = HelperMethods.RandomString(10);
                            using (Stream fileStreamIn = fileupload_Overlay.FileContent) {
                                using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn)) {
                                    ZipEntry entry;
                                    string tmpEntry = String.Empty;

                                    List<string> dlls = new List<string>();
                                    while ((entry = zipInStream.GetNextEntry()) != null) {
                                        string fn = Path.GetFileName(entry.Name);

                                        if (string.IsNullOrEmpty(fn)) {
                                            continue;
                                        }

                                        var tempfi = new FileInfo(fn);
                                        if ((tempfi.Extension.ToLower() != ".exe") && (tempfi.Extension.ToLower() != ".com") && (tempfi.Extension.ToLower() != ".pif")
                                             && (tempfi.Extension.ToLower() != ".bat") && (tempfi.Extension.ToLower() != ".scr")) {

                                            if ((fn != String.Empty) && (entry.Name.IndexOf(".ini") < 0)) {
                                                string en = tempfi.Name.Replace(tempfi.Extension, string.Empty) + tempfi.Extension;
                                                string filePath = ServerSettings.GetServerMapLocation + "Overlays\\" + mainFolder + "\\";

                                                if (tempfi.Extension.ToLower() == ".pdb") {
                                                    continue;
                                                }

                                                if ((tempfi.Extension.ToLower() == ".dll") || (tempfi.Extension.ToLower() == ".compiled")) {
                                                    filePath = ServerSettings.GetServerMapLocation + "Bin\\" + dllFolder + "\\";
                                                    if (!Directory.Exists(filePath)) {
                                                        Directory.CreateDirectory(filePath);
                                                    }

                                                    ServerSettings.AddRuntimeAssemblyBinding("Bin\\" + dllFolder);
                                                    en = tempfi.Name;
                                                    dlls.Add(en);
                                                }
                                                else {
                                                    if (tempfi.Extension.ToLower() == ".ascx") {
                                                        string fileLoc = "Overlays/" + mainFolder + "/" + en;
                                                        string description = txt_uploadOverlayDesc.Text.Trim();
                                                        overlayID = _workspaceOverlays.AddOverlay(CurrentUsername, overlayName, fileLoc, description, dd_displayTypeNew.SelectedValue);
                                                        CurrentAppObject = new App(string.Empty);

                                                        string tempAppAssociationList = HttpUtility.UrlDecode(hf_newAppAssocationList_Checked.Value);
                                                        string[] cb_associatedOverlay = tempAppAssociationList.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                                                        foreach (string cbItem in cb_associatedOverlay) {
                                                            if ((!string.IsNullOrEmpty(cbItem)) && (!string.IsNullOrEmpty(overlayID))) {
                                                                string[] overlayList = CurrentAppObject.GetAppOverlayIds(cbItem);
                                                                string tempOverlays = string.Empty;
                                                                foreach (string oId in overlayList) {
                                                                    tempOverlays += oId + ";";
                                                                }
                                                                if (!tempOverlays.Contains(overlayID)) {
                                                                    tempOverlays += overlayID + ";";
                                                                }
                                                                CurrentAppObject.UpdateAppOverlayID(cbItem, tempOverlays);
                                                            }
                                                        }
                                                    }
                                                }
                                                string fullPath = filePath + en;
                                                fullPath = fullPath.Replace("\\ ", "\\").Replace("/", "\\");

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

                    #region ASCX FILES
                    else if (fi.Extension.ToLower() == ".ascx") {
                        string fileGuid = fi.Name.Replace(fi.Extension, string.Empty) + fi.Extension;
                        string fileLoc = "Overlays/" + mainFolder + "/" + fileGuid;
                        string saveAs = "\\Overlays\\" + mainFolder + "\\" + fileGuid;
                        fileupload_Overlay.SaveAs(ServerLoc + saveAs);
                        string description = txt_uploadOverlayDesc.Text.Trim();
                        string overlayID = _workspaceOverlays.AddOverlay(CurrentUsername, overlayName, fileLoc, description, dd_displayTypeNew.SelectedValue);
                        CurrentAppObject = new App(string.Empty);

                        string tempAppAssociationList = HttpUtility.UrlDecode(hf_newAppAssocationList_Checked.Value);
                        string[] cb_associatedOverlay = tempAppAssociationList.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string cbItem in cb_associatedOverlay) {
                            if ((!string.IsNullOrEmpty(cbItem)) && (!string.IsNullOrEmpty(overlayID))) {
                                string[] overlayList = CurrentAppObject.GetAppOverlayIds(cbItem);
                                string tempOverlays = string.Empty;
                                foreach (string oId in overlayList) {
                                    tempOverlays += oId + ";";
                                }
                                if (!tempOverlays.Contains(overlayID)) {
                                    tempOverlays += overlayID + ";";
                                }
                                CurrentAppObject.UpdateAppOverlayID(cbItem, tempOverlays);
                            }
                        }
                        action = "uploadsuccess";
                    }
                    #endregion

                }
                catch (Exception ex) {
                    AppLog.AddError(ex);
                }
            }
        }

        hf_newAppAssocationList_Checked.Value = string.Empty;
        ServerSettings.PageIFrameRedirect(this.Page, "OverlayManager.aspx?action=" + action + "&date=" + ServerSettings.ServerDateTime.Ticks);
    }

    private static string MakeValidFileName(string name) {
        string invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
        string final = Regex.Replace(name, invalidReStr, "_");
        final = final.Replace(" ", "_");
        return final;
    }

    private void BuildAppList() {
        Dictionary<string, string> removeTemp = new Dictionary<string, string>();

        CurrentAppObject = new App(CurrentUsername);
        CurrentAppObject.GetAllApps();
        tempAppColl.Clear();
        List<string> enabledApps = CurrentUserMemberDatabase.EnabledApps;
        foreach (Apps_Coll dr in CurrentAppObject.AppList) {
            if (MainServerSettings.AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(dr, CurrentUserMemberDatabase)) {
                    continue;
                }
            }

            string id = dr.AppId;
            if ((IsUserInAdminRole()) || ((enabledApps.Contains(id)))) {
                string name = dr.AppName;
                string image = "<img alt='icon' src='" + ResolveUrl("~/" + dr.Icon) + "' class='app-icon-admin-icon' />";
                if (string.IsNullOrEmpty(dr.Icon)) {
                    image = string.Empty;
                }

                if (!removeTemp.ContainsKey(id)) {
                    StringBuilder appScript = new StringBuilder();
                    appScript.Append("<div id='app-icon-" + id + "' class='app-icon-admin'>");
                    appScript.Append("<a onclick=\"AddAssociation_New(this, '" + dr.AppId + "');return false;\" title='Add " + dr.AppName + "' class='float-left img-expand-sml cursor-pointer'></a>");
                    appScript.Append(image + "<span class='app-span-modify'>" + dr.AppName + "</span>");
                    appScript.Append("<div class='clear'></div></div>");
                    appScript.Append("<div class='clear'></div>");
                    removeTemp.Add(id, appScript.ToString());
                }
            }
        }

        StringBuilder removeScript = new StringBuilder();

        foreach (KeyValuePair<string, string> kvp in removeTemp) {
            removeScript.Append(kvp.Value);
        }

        pnl_associatedOverlays.Controls.Clear();
        string table = HelperMethods.TableAddRemove(removeScript.ToString(), string.Empty, "Apps Available to Add", "Associated Apps", false);
        pnl_associatedOverlays.Controls.Add(new LiteralControl(table));
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

        return returnVal;
    }

    #endregion


    #region Edit/Delete

    protected void hf_EditOverlay_Changed(object sender, EventArgs e) {
        BuildAppList();
        BuildOverlays();
    }

    protected void hf_UpdateOverlayName_Changed(object sender, EventArgs e) {
        string id = hf_EditOverlay.Value;
        if (!string.IsNullOrEmpty(id)) {
            _workspaceOverlays.UpdateOverlayName(id, hf_UpdateOverlayName.Value.Trim());
            _workspaceOverlays.UpdateOverlayDescription(id, hf_UpdateDescOverlay.Value.Trim());
            _workspaceOverlays.UpdateOverlayDisplayType(id, hf_displayType.Value.Trim());

            CurrentAppObject = new App(string.Empty);

            string[] appSlit_added = hf_appAssocationList_added.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string appAdd in appSlit_added) {
                string[] oIds = CurrentAppObject.GetAppOverlayIds(appAdd);
                string list = "";
                foreach (string oId in oIds) {
                    list += oId + ";";
                }
                if (!list.Contains(id)) {
                    list += id + ";";
                }

                CurrentAppObject.UpdateAppOverlayID(appAdd, list);
            }

            string[] appSlit_removed = hf_appAssocationList_removed.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string appRemoved in appSlit_removed) {
                string[] oIds = CurrentAppObject.GetAppOverlayIds(appRemoved);
                string list = "";
                foreach (string oId in oIds) {
                    if (oId != id) {
                        list += oId + ";";
                    }
                }

                CurrentAppObject.UpdateAppOverlayID(appRemoved, list);
            }
        }

        hf_EditOverlay.Value = "";
        hf_UpdateDescOverlay.Value = "";
        hf_UpdateOverlayName.Value = "";
        hf_displayType.Value = "";
        hf_appAssocationList_added.Value = "";
        hf_appAssocationList_removed.Value = "";

        BuildAppList();
        BuildOverlays();
    }

    protected void hf_DeleteOverlay_Changed(object sender, EventArgs e) {
        BuildAppList();
        string action = "deletefailed";
        string id = hf_DeleteOverlay.Value;
        if (!string.IsNullOrEmpty(id)) {
            try {
                string loc = _workspaceOverlays.GetWorkspaceFileLoc(id);
                loc = loc.Replace("/", "\\");
                string filename = ServerSettings.GetServerMapLocation + loc;
                if (loc.ToLower().StartsWith("overlays\\")) {
                    if ((File.Exists(filename))) {
                        FileInfo fi = new FileInfo(filename);
                        if (fi.Directory.Name == "Overlays") {
                            File.Delete(filename);
                        }
                        else {
                            Directory.Delete(fi.Directory.FullName, true);
                        }
                    }
                }

                OverlayDLLs overlayDLLs = new OverlayDLLs();
                string folderPath = overlayDLLs.GetFolderPath(id);

                overlayDLLs.DeleteItem(id);

                if (!string.IsNullOrEmpty(folderPath)) {
                    ServerSettings.RemoveRuntimeAssemblyBinding("Bin\\" + folderPath);
                    if (Directory.Exists(ServerSettings.GetServerMapLocation + "Bin\\" + folderPath)) {
                        Directory.Delete(ServerSettings.GetServerMapLocation + "Bin\\" + folderPath, true);
                    }
                }

                _workspaceOverlays.DeleteOverlay(id);
                action = "deletesuccess";
            }
            catch { }
        }

        ServerSettings.PageIFrameRedirect(this.Page, "OverlayManager.aspx?action=" + action + "&date=" + ServerSettings.ServerDateTime.Ticks);
    }

    protected void hf_refreshList_ValueChanged(object sender, EventArgs e) {
        hf_EditOverlay.Value = "";
        hf_refreshList.Value = "";
        BuildAppList();
        BuildAppList();
    }

    #endregion


}