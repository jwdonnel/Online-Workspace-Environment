using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Web.Security;
using System.Text;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;

public partial class SiteTools_JqueryPlugins : BasePage {

    #region private variables

    private SitePlugins _sitePlugins;

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        _sitePlugins = new SitePlugins(CurrentUsername);
        lbl_appInstaller.Text = "Plugin Installer";

        CheckQueryString();
        if (!MainServerSettings.SitePluginsLocked) {
            pnl_AddControls.Enabled = true;
            pnl_AddControls.Visible = true;
            BuildTable_Editable("");
            Build_aw_Dropdown(false);
            ltl_locked.Text = "";
        }
        else {
            pnl_AddControls.Enabled = false;
            pnl_AddControls.Visible = false;
            BuildTable_Editable("");
            ltl_locked.Text = HelperMethods.GetLockedByMessage();
        }

        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            cb_installAfter.Enabled = false;
            cb_installAfter.Visible = false;
        }
    }

    private void CheckQueryString() {
        if (!string.IsNullOrEmpty(Request.QueryString["newpluginid"])) {
            hf_defaultID.Value = Request.QueryString["newpluginid"];

            string controlName = ScriptManager.GetCurrent(Page).AsyncPostBackSourceElementID;
            if (controlName == null || (controlName != null && !controlName.ToLower().Contains("btn_updatedefaultfile"))) {
                SitePlugins_Coll plugin = _sitePlugins.GetPlugin(hf_defaultID.Value);
                if (string.IsNullOrEmpty(plugin.PluginLocation) && !string.IsNullOrEmpty(plugin.PluginName)) {
                    radioButton_FileList.Items.Clear();
                    checkbox_FileList.Items.Clear();
                    RecursiveFolderBuild(plugin, ServerSettings.GetServerMapLocation + "Plugins\\" + hf_defaultID.Value);

                    if (radioButton_FileList.Items.Count == 0) {
                        mainfileEmpty.Enabled = true;
                        mainfileEmpty.Visible = true;
                    }
                    if (checkbox_FileList.Items.Count == 0) {
                        associatedfileEmpty.Enabled = true;
                        associatedfileEmpty.Visible = true;
                    }

                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'LoaderApp-element', 'Plugin File Selection');");
                }
            }
        }
        else if (Request.QueryString["action"] == "uploadsuccess") {
            lbl_uploadMessage.Text = "<div class='clear-space'></div>Plugin added successfully!";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Green;
        }
        else if (Request.QueryString["action"] == "uploadfailed") {
            lbl_uploadMessage.Text = "<div class='clear-space'></div>Could not upload plugin. Please try again.";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Red;
        }

        if (IsPostBack) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#lbl_uploadMessage').html('');");
        }
    }

    private void RecursiveFolderBuild(SitePlugins_Coll plugin, string folder) {
        string[] folders = Directory.GetDirectories(folder);
        foreach (string entry in folders) {
            RecursiveFolderBuild(plugin, entry);
            RecursiveFileBuild(plugin, entry);
        }

        RecursiveFileBuild(plugin, folder);
    }
    private void RecursiveFileBuild(SitePlugins_Coll plugin, string folder) {
        string[] files = Directory.GetFiles(folder);
        foreach (string file in files) {
            string newFilename = file.Replace(ServerSettings.GetServerMapLocation + "Plugins\\" + hf_defaultID.Value, "");
            FileInfo fi = new FileInfo(file);
            
            ListItem item = new ListItem(newFilename, file.Replace(ServerSettings.GetServerMapLocation, "~\\"));

            if (fi.Extension.ToLower() == ".js") {
                if (string.IsNullOrEmpty(plugin.AssociatedWith) && !radioButton_FileList.Items.Contains(item)) {
                    radioButton_FileList.Items.Add(item);
                }
                if (!checkbox_FileList.Items.Contains(item)) {
                    checkbox_FileList.Items.Add(item);
                }
            }
            else if (fi.Extension.ToLower() == ".css") {
                if (!checkbox_FileList.Items.Contains(item)) {
                    checkbox_FileList.Items.Add(item);
                }
            }
        }
    }

    private void Build_aw_Dropdown(bool _override) {
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        if (sm != null) {
            string ctlId = sm.AsyncPostBackSourceElementID;
            if (((!ctlId.Contains("btn_addNewPlugin")) && (!string.IsNullOrEmpty(ctlId))) || (_override) || (!IsPostBack)) {
                dd_aw.Items.Clear();

                // Initialize the list
                if (!IsUserInAdminRole()) {
                    _sitePlugins.BuildSitePluginsForUser();
                }
                else {
                    _sitePlugins.BuildSitePlugins(false);
                }

                dd_aw.Items.Add(new ListItem("--- Nothing ---", ""));
                dd_aw.SelectedIndex = 0;
                foreach (SitePlugins_Coll coll in _sitePlugins.siteplugins_dt) {
                    if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                        ListItem item = new ListItem(coll.PluginName, coll.ID);
                        if (!dd_aw.Items.Contains(item))
                            dd_aw.Items.Add(item);
                    }
                }
            }
        }
    }


    #region Build Table
    private void BuildTable_Editable(string editId) {
        if (IsUserInAdminRole()) {
            _sitePlugins.BuildSitePlugins(false);
        }
        else {
            _sitePlugins.BuildSitePluginsUploadByUser();
        }

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, !MainServerSettings.SitePluginsLocked, 2);

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Name", "200", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Description", "300", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Updated By", "175", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Updated", "175", false));
        if (!MainServerSettings.SitePluginsLocked) {
            headerColumns.Add(new TableBuilderHeaderColumns("State", "100", false, false));
        }
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        bool needLoadEditor = false;

        int index = 1;
        int enabled = 0;
        int totalFound = 0;
        int count = 0;
        foreach (SitePlugins_Coll coll in _sitePlugins.siteplugins_dt) {
            if (MainServerSettings.AssociateWithGroups) {
                if (!ServerSettings.CheckPluginGroupAssociation(coll, CurrentUserMemberDatabase)) {
                    continue;
                }
            }

            MemberDatabase m = new MemberDatabase(coll.CreatedBy);
            string name = HelperMethods.MergeFMLNames(m);

            if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                count++;
                if (coll.Enabled) {
                    enabled++;
                }
            }

            string an = string.Empty;
            string aw = string.Empty;
            if (!string.IsNullOrEmpty(coll.AssociatedWith)) {
                SitePlugins_Coll sitePluginTemp = _sitePlugins.GetPlugin(coll.AssociatedWith);
                if (!string.IsNullOrEmpty(sitePluginTemp.PluginName)) {
                    aw = sitePluginTemp.ID;
                    an = sitePluginTemp.PluginName;
                }
            }

            string pluginName = string.Empty;
            string description = string.Empty;
            string createdBy = string.Empty;
            string dateUpdated = string.Empty;
            string state = string.Empty;
            string editButtons = string.Empty;

            if ((string.IsNullOrEmpty(aw) && (string.IsNullOrEmpty(hf_BuildAssociatedTable.Value))) || (hf_BuildAssociatedTable.Value == aw)) {
                if ((!string.IsNullOrEmpty(editId)) && (editId == coll.ID)) {

                    string inputName = "<b>Plugin Name</b><br /><input id='txt_updateName' type='text' class='textEntry-noWidth' value='" + coll.PluginName + "' />";
                    string dd_updateAW = BuildAWEdit(coll.AssociatedWith, coll.ID);

                    pluginName = inputName + "<div class='clear-space'></div>" + dd_updateAW;

                    string inputDesc = "<b>Description</b><br /><textarea id='txt_updateDesc' rows='3' class='textEntry-noWidth'>" + coll.Description + "</textarea>";
                    string editor = "<b>Initialize Code</b><br /><div style='height: 155px;'><div id='updateEditor'></div></div>";
                    hf_updateInitializeCode.Value = HttpUtility.UrlDecode(coll.InitCode);
                    description = inputDesc + "<div class='clear-space'></div>" + editor;

                    createdBy = name;
                    dateUpdated = coll.Date;
                    state = "-";

                    editButtons = "<a class='td-update-btn' onclick='UpdatePlugin(\"" + coll.ID + "\");return false;' title='Update'></a>";
                    editButtons += "<a class='td-cancel-btn Button-Action' onclick='CancelPlugin();return false;' title='Cancel'></a>";
                    needLoadEditor = true;
                }
                else {
                    string associationBtn = string.Empty;
                    if (string.IsNullOrEmpty(aw)) {
                        associationBtn = "<div class='clear'></div><a href='#' onclick=\"ViewAssociations('" + coll.ID + "');return false;\" class=\"associated-plugin-text\">View Associations</a>";
                    }
                    else if (hf_BuildAssociatedTable.Value == aw) {
                        associationBtn = "<div class='clear'></div><span class=\"associated-plugin-text\">Associated with " + an + "</span>";
                    }

                    pluginName = coll.PluginName + associationBtn;
                    description = coll.Description;
                    if (string.IsNullOrEmpty(description)) {
                        description = "No description available";
                    }

                    createdBy = name;
                    dateUpdated = coll.Date;

                    if (string.IsNullOrEmpty(editId)) {
                        state = CreateRadioButtonsEdit(coll.Enabled, coll.ID, count, coll.PluginName);
                        editButtons = "<a class='td-edit-btn Button-Action' onclick='EditPlugin(\"" + coll.ID + "\");return false;' title='Edit'></a>";
                        editButtons += "<a class='td-delete-btn' onclick='DeletePlugin(\"" + coll.ID + "\", \"" + coll.PluginName + "\");return false;' title='Delete'></a>";
                    }
                }

                List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Name", pluginName, TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Description", description, TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Updated By", createdBy, TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Date Updated", dateUpdated, TableBuilderColumnAlignment.Left));
                if (!MainServerSettings.SitePluginsLocked) {
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("State", state, TableBuilderColumnAlignment.Left));
                }
                tableBuilder.AddBodyRow(bodyColumnValues, editButtons);

                totalFound++;
                index++;
            }
        }
        #endregion

        lbtn_associationBack.Enabled = false;
        lbtn_associationBack.Visible = false;
        if (!string.IsNullOrEmpty(hf_BuildAssociatedTable.Value) && hf_BuildAssociatedTable.Value != "-") {
            lbtn_associationBack.Enabled = true;
            lbtn_associationBack.Visible = true;
        }

        pnl_siteplugins.Controls.Clear();
        pnl_siteplugins.Controls.Add(tableBuilder.CompleteTableLiteralControl("No plugins found"));

        if (needLoadEditor) {
            RegisterPostbackScripts.RegisterStartupScript(this, "startAceUpdateEditor();");
        }
    }

    private string BuildAWEdit(string id, string currId) {
        List<string> tempList = new List<string>();
        string dd_updateAW = "<b>Associated With</b><br /><select id='dd_updateAW' class='pad-right-sml'>";
        dd_updateAW += "<option value=''>--- Nothing ---</option>";
        foreach (SitePlugins_Coll coll in _sitePlugins.siteplugins_dt) {
            if ((string.IsNullOrEmpty(coll.AssociatedWith)) && (coll.ID != currId)) {
                if (!tempList.Contains(coll.ID)) {
                    string selected = "";
                    if (coll.ID == id)
                        selected = " selected='selected'";

                    dd_updateAW += "<option value='" + coll.ID + "'" + selected + ">" + coll.PluginName + "</option>";
                    tempList.Add(coll.ID);
                }
            }
        }
        dd_updateAW += "</select>";

        return dd_updateAW;
    }

    protected void BuildAssociatedTable_ValueChanged(object sender, EventArgs e) {
        BuildTable_Editable("");
    }

    protected void lbtn_associationBack_Click(object sender, EventArgs e) {
        hf_BuildAssociatedTable.Value = string.Empty;
        updatePnl_association.Update();

        BuildTable_Editable("");
    }

    #endregion


    #region Action Buttons

    protected void btn_uploadPlugin_Clicked(object sender, EventArgs e) {
        string action = "uploadfailed";
        string initCode = HttpUtility.UrlEncode(hf_InitializeCode.Value.Trim());
        if ((!string.IsNullOrEmpty(txt_uploadPluginName.Text)) && (fileupload_Plugin.PostedFile != null) && (txt_uploadPluginName.Text != "Plugin Name")) {
            if (((fileupload_Plugin.PostedFile != null) && (fileupload_Plugin.PostedFile.ContentLength > 0) && (!string.IsNullOrEmpty(fileupload_Plugin.PostedFile.FileName))) || (fileupload_Plugin.HasFile)) {
                FileInfo fi = new FileInfo(fileupload_Plugin.PostedFile.FileName);
                string folderName = Guid.NewGuid().ToString();

                switch (fi.Extension.ToLower()) {
                    #region ZIP FILES

                    case ".zip":
                        string filePath = ServerSettings.GetServerMapLocation + "Plugins\\" + folderName;
                        try {
                            if (!Directory.Exists(filePath)) {
                                Directory.CreateDirectory(filePath);
                            }
                        }
                        catch { }
                        filePath += "\\";

                        using (Stream fileStreamIn = fileupload_Plugin.FileContent) {
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
                                         && (tempfi.Extension.ToLower() != ".bat") && (tempfi.Extension.ToLower() != ".scr") && (tempfi.Extension.ToLower() != ".dll") && (tempfi.Extension.ToLower() != ".compiled")) {

                                        if ((fn != String.Empty) && (entry.Name.IndexOf(".ini") < 0)) {
                                            string en = entry.Name;
                                            if (tempfi.Extension.ToLower() == ".pdb") {
                                                continue;
                                            }

                                            if (!HelperMethods.IsValidFileFolderFormat(tempfi.Extension) && !HelperMethods.IsImageFileType(tempfi.Extension)) {
                                                continue;
                                            }

                                            FileInfo fnTemp = new FileInfo(en);
                                            string tempPath = en.Replace(fnTemp.Name, "").Replace("/", "\\");
                                            if (!Directory.Exists(filePath + tempPath)) {
                                                Directory.CreateDirectory(filePath + tempPath);
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

                                _sitePlugins.addItem(folderName, txt_uploadPluginName.Text.Trim(), string.Empty, txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, CurrentUsername); ;
                                if (cb_installAfter.Checked && string.IsNullOrEmpty(dd_aw.SelectedValue) && !IsUserNameEqualToAdmin()) {
                                    if (!string.IsNullOrEmpty(folderName))
                                        _sitePlugins.addItemForUser(folderName);
                                }

                                ServerSettings.PageIFrameRedirect(this.Page, "JqueryPlugins.aspx?newpluginid=" + folderName + "&date=" + ServerSettings.ServerDateTime.Ticks);
                            }
                        }
                        break;

                    #endregion

                    #region DEFAULT

                    default:
                        if (((fileupload_Plugin.PostedFile.ContentType == "application/javascript") && (fi.Extension.ToLower() == ".js"))
                            || ((fileupload_Plugin.PostedFile.ContentType == "text/css") && (fi.Extension.ToLower() == ".css"))) {
                            string ServerLoc = ServerSettings.GetServerMapLocation;
                            try {

                                if (!Directory.Exists(ServerLoc + "Plugins\\" + folderName)) {
                                    Directory.CreateDirectory(ServerLoc + "Plugins\\" + folderName);
                                }

                                string saveAs = "Plugins\\" + folderName + "\\" + Guid.NewGuid().ToString() + fi.Extension;
                                fileupload_Plugin.SaveAs(ServerLoc + saveAs);
                                string pluginID = _sitePlugins.addItem(txt_uploadPluginName.Text.Trim(), "~\\" + saveAs, txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, CurrentUsername);
                                action = "uploadsuccess";

                                if (cb_installAfter.Checked && string.IsNullOrEmpty(dd_aw.SelectedValue) && !IsUserNameEqualToAdmin()) {
                                    if (!string.IsNullOrEmpty(pluginID))
                                        _sitePlugins.addItemForUser(pluginID);
                                }
                            }
                            catch {
                                action = "uploadfailed";
                            }
                        }
                        break;

                    #endregion
                }
            }
            else if ((!string.IsNullOrEmpty(initCode)) && (!string.IsNullOrEmpty(txt_uploadPluginName.Text.Trim()))) {
                string pluginID = _sitePlugins.addItem(txt_uploadPluginName.Text.Trim(), "", txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, CurrentUsername);
                action = "uploadsuccess";

                if (cb_installAfter.Checked && string.IsNullOrEmpty(dd_aw.SelectedValue) && !IsUserNameEqualToAdmin()) {
                    if (!string.IsNullOrEmpty(pluginID))
                        _sitePlugins.addItemForUser(pluginID);
                }
            }
        }
        else if ((!string.IsNullOrEmpty(initCode)) && (!string.IsNullOrEmpty(txt_uploadPluginName.Text.Trim()))) {
            string pluginID = _sitePlugins.addItem(txt_uploadPluginName.Text.Trim(), "", txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, CurrentUsername);
            action = "uploadsuccess";

            if (cb_installAfter.Checked && string.IsNullOrEmpty(dd_aw.SelectedValue) && !IsUserNameEqualToAdmin()) {
                if (!string.IsNullOrEmpty(pluginID))
                    _sitePlugins.addItemForUser(pluginID);
            }
        }

        ServerSettings.PageIFrameRedirect(this.Page, "JqueryPlugins.aspx?action=" + action + "&date=" + ServerSettings.ServerDateTime.Ticks);
    }

    protected void btn_updateDefaultFile_Clicked(object sender, EventArgs e) {
        string id = Request.QueryString["newpluginid"];
        if (!string.IsNullOrEmpty(id)) {
            string mainFile = radioButton_FileList.SelectedValue;
            if (!string.IsNullOrEmpty(mainFile)) {
                _sitePlugins.updatePluginLocation(id, mainFile);
            }

            SitePlugins_Coll plugin = _sitePlugins.GetPlugin(id);

            foreach (ListItem item in checkbox_FileList.Items) {
                if (item.Selected && !string.IsNullOrEmpty(item.Value)) {
                    FileInfo fi = new FileInfo(item.Text);
                    _sitePlugins.addItem(fi.Name, item.Value, plugin.Description, true, string.Empty, id, CurrentUsername);
                }
            }
        }

        ServerSettings.PageIFrameRedirect(this.Page, "JqueryPlugins.aspx?date=" + ServerSettings.ServerDateTime.Ticks);
    }

    protected void btn_cancelUpload_Clicked(object sender, EventArgs e) {
        string id = hf_defaultID.Value;
        if (!string.IsNullOrEmpty(id)) {
            _sitePlugins.deleteItem(id);
            _sitePlugins.BuildSitePluginsForUser();
            List<UserPlugins_Coll> userPlugins = _sitePlugins.userplugins_dt;
            foreach (UserPlugins_Coll entry in userPlugins) {
                if (entry.PluginID == id) {
                    _sitePlugins.deleteItemForUser(entry.ID);
                }
            }

            try {
                if (Directory.Exists(ServerSettings.GetServerMapLocation + "Plugins\\" + id)) {
                    Directory.Delete(ServerSettings.GetServerMapLocation + "Plugins\\" + id, true);
                }
            }
            catch (Exception ex) {
                AppLog.AddError(ex);
            }
        }

        ServerSettings.PageIFrameRedirect(this.Page, "JqueryPlugins.aspx?date=" + ServerSettings.ServerDateTime.Ticks);
    }

    private static string MakeValidFileName(string name) {
        string invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
        string final = Regex.Replace(name, invalidReStr, "_");
        final = final.Replace(" ", "_");
        return final;
    }

    private string CreateRadioButtonsEdit(bool active, string id, int count, string name) {
        var str = new StringBuilder();
        str.Append("<div class='field switch'>");
        string enabledclass = "cb-enable";
        string disabledclass = "cb-disable selected";
        string onclickEnable = "onclick=\"UpdateEnabled('" + id + "')\"";
        string onclickDisable = "onclick='loadingPopup.RemoveMessage();'";
        if (active) {
            enabledclass = "cb-enable selected";
            disabledclass = "cb-disable";
            onclickEnable = "onclick='loadingPopup.RemoveMessage();'";
            onclickDisable = "onclick=\"UpdateDisabled('" + id + "')\"";
        }

        str.Append("<span class='" + enabledclass + "'><input id='rb_script_active_" + count.ToString() + "' type='radio' value='active' " + onclickEnable + " /><label for='rb_script_active_" + count.ToString() + "'>On</label></span>");
        str.Append("<span class='" + disabledclass + "'><input id='rb_script_deactive_" + count.ToString() + "' type='radio' value='deactive' " + onclickDisable + " /><label for='rb_script_deactive_" + count.ToString() + "'>Off</label></span>");
        str.Append("</div>");
        return str.ToString();
    }

    protected void hf_deletePlugin_Changed(object sender, EventArgs e) {
        string id = hf_deletePlugin.Value;

        SitePlugins_Coll sitePlugin = _sitePlugins.GetPlugin(id);
        _sitePlugins.deleteItem(id);
        if (string.IsNullOrEmpty(sitePlugin.AssociatedWith) && !string.IsNullOrEmpty(id)) {
            try {
                if (Directory.Exists(ServerSettings.GetServerMapLocation + "Plugins\\" + id)) {
                    Directory.Delete(ServerSettings.GetServerMapLocation + "Plugins\\" + id, true);
                }
            }
            catch (Exception ex) {
                AppLog.AddError(ex);
            }
        }

        _sitePlugins.BuildSitePlugins(false);
        foreach (SitePlugins_Coll entry in _sitePlugins.siteplugins_dt) {
            if (entry.AssociatedWith == id) {
                _sitePlugins.deleteItem(entry.ID);
            }
        }

        BuildTable_Editable("");
        Build_aw_Dropdown(true);
        hf_deletePlugin.Value = string.Empty;
    }

    protected void hf_editPlugin_Changed(object sender, EventArgs e) {
        string id = hf_editPlugin.Value;
        if (!string.IsNullOrEmpty(id))
            BuildTable_Editable(id);

        hf_editPlugin.Value = string.Empty;
    }

    protected void hf_enablePlugin_Changed(object sender, EventArgs e) {
        string id = hf_enablePlugin.Value;
        if (!string.IsNullOrEmpty(id)) {
            _sitePlugins.updateEnabled(id, true);
            BuildTable_Editable("");
        }

        hf_enablePlugin.Value = string.Empty;
    }

    protected void hf_disablePlugin_Changed(object sender, EventArgs e) {
        string id = hf_disablePlugin.Value;
        if (!string.IsNullOrEmpty(id)) {
            _sitePlugins.updateEnabled(id, false);
            BuildTable_Editable("");
        }

        hf_disablePlugin.Value = string.Empty;
    }

    protected void hf_cancelPlugin_Changed(object sender, EventArgs e) {
        BuildTable_Editable("");
        hf_cancelPlugin.Value = string.Empty;
    }

    protected void hf_updatePlugin_Changed(object sender, EventArgs e) {
        string id = hf_updatePlugin.Value;
        if (!string.IsNullOrEmpty(id)) {
            if (!string.IsNullOrEmpty(hf_updateName.Value))
                _sitePlugins.updatePluginName(id, hf_updateName.Value.Trim());

            if (!string.IsNullOrEmpty(hf_updateLoc.Value)) {
                string loc = hf_updateLoc.Value.Trim();
                bool canContinue = false;
                try {
                    Uri uri = new Uri(loc);
                    if (uri.IsAbsoluteUri) {
                        string x = uri.Segments[uri.Segments.Length - 1];
                        FileInfo fi = new FileInfo(x);
                        if (fi.Extension.ToLower() == ".js")
                            canContinue = true;
                    }
                }
                catch {
                    try {
                        FileInfo fi = new FileInfo(loc.Trim());
                        if (fi.Extension.ToLower() == ".js")
                            canContinue = true;
                    }
                    catch { }
                }

                if (canContinue)
                    _sitePlugins.updatePluginLocation(id, loc);
            }

            if (!string.IsNullOrEmpty(hf_updateDesc.Value))
                _sitePlugins.updatePluginDescription(id, hf_updateDesc.Value.Trim());

            string inicode = HttpUtility.UrlEncode(hf_updateInitializeCode.Value.Trim());
            _sitePlugins.updatePluginInitCode(id, inicode);

            _sitePlugins.updatePluginAssociatedWith(id, hf_updateaw.Value);
            BuildTable_Editable("");
            Build_aw_Dropdown(true);
        }

        hf_updateName.Value = string.Empty;
        hf_updateLoc.Value = string.Empty;
        hf_updateDesc.Value = string.Empty;
        hf_updateInitializeCode.Value = string.Empty;
        hf_updateaw.Value = string.Empty;
        hf_updatePlugin.Value = string.Empty;
    }
    #endregion


}