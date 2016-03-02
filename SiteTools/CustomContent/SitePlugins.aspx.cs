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

public partial class SiteTools_SitePlugins : System.Web.UI.Page {
    #region private variables

    private ServerSettings _ss = new ServerSettings();
    private string _sitetheme = "Standard";
    private string _username;
    private SitePlugins _sitePlugins;

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                _username = userId.Name;
                _sitePlugins = new SitePlugins(_username);
                var member = new MemberDatabase(_username);
                _sitetheme = member.SiteTheme;

                lbl_appInstaller.Text = "Plugin Installer";

                CheckQueryString();
                if (!_ss.SitePluginsLocked) {
                    pnl_AddControls.Enabled = true;
                    pnl_AddControls.Visible = true;
                    BuildTable_Editable("");
                    Build_aw_Dropdown(false);
                    ltl_locked.Text = "";
                }
                else {
                    pnl_AddControls.Enabled = false;
                    pnl_AddControls.Visible = false;
                    BuildTable_NonEditable();
                    ltl_locked.Text = HelperMethods.GetLockedByMessage();
                }

                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                    cb_installAfter.Enabled = false;
                    cb_installAfter.Visible = false;
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
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
            lbl_uploadMessage.Text = "Plugin added successfully!";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Green;
            RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function () { $('#lbl_uploadMessage').html(''); }, 3000);");
        }
        else if (Request.QueryString["action"] == "uploadfailed") {
            lbl_uploadMessage.Text = "Could not upload plugin. Please try again.";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Red;
            RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function () { $('#lbl_uploadMessage').html(''); }, 3000);");
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
                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
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
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            _sitePlugins.BuildSitePlugins(false);
        }
        else {
            _sitePlugins.BuildSitePluginsUploadByUser();
        }

        pnl_siteplugins.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='200px' align='left'>Plugin Name</td>");
        str.Append("<td align='left' style='min-width: 300px;'>Description</td>");
        str.Append("<td width='150px' align='left'>Created By / Date</td>");
        str.Append("<td width='190px'>Actions</td></tr>");
        bool needLoadEditor = false;

        bool AssociateWithGroups = _ss.AssociateWithGroups;
        MemberDatabase _member = new MemberDatabase(_username);

        int index = 1;
        int enabled = 0;
        int totalFound = 0;
        int count = 0;
        foreach (SitePlugins_Coll coll in _sitePlugins.siteplugins_dt) {
            if (!SearchFilterValid(coll)) {
                continue;
            }

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckPluginGroupAssociation(coll, _member)) {
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

            if ((string.IsNullOrEmpty(aw) && (string.IsNullOrEmpty(hf_BuildAssociatedTable.Value))) || (hf_BuildAssociatedTable.Value == aw)) {
                if ((!string.IsNullOrEmpty(editId)) && (editId == coll.ID)) {
                    str.Append("<tr class='myItemStyle GridNormalRow'>");
                    str.Append("<td width='45px' align='center' class='GridViewNumRow border-bottom'>" + index + "</td>");
                    string inputName = "<b>Plugin Name</b><br /><input id='txt_updateName' type='text' class='textEntry' style='width:196px;' value='" + coll.PluginName + "' />";
                    string dd_updateAW = BuildAWEdit(coll.AssociatedWith, coll.ID);

                    str.Append("<td width='200px' valign='top' class='border-right border-bottom'>" + inputName + "<div class='clear-space'></div>" + dd_updateAW + "</td>");

                    string inputLocation = "<b>Plugin Path</b><br /><input id='txt_updateLoc' type='text' class='textEntry' style='width:98%' value='" + coll.PluginLocation + "' />";
                    string inputDesc = "<b>Description</b><br /><textarea id='txt_updateDesc' rows='5' class='textEntry' style='width:97%;font-family:Arial;padding:3px;border:1px solid #DDD;'>" + coll.Description + "</textarea>";
                    string editor = "<b>Initialize Code</b><br /><div style='height: 155px;'><div id='updateEditor'></div></div>";
                    hf_updateInitializeCode.Value = HttpUtility.UrlDecode(coll.InitCode);
                    str.Append("<td class='border-right border-bottom' style='min-width: 300px;'>" + inputDesc + "<div class='clear-space'></div>" + editor + "</td>");

                    string createdBy = name + "<div class='clear-space-two'></div>" + coll.Date;
                    if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
                        createdBy = coll.Date;

                    str.Append("<td width='150px' class='border-right border-bottom'>" + createdBy + "</td>");
                    string editButtons = "<div><a href='#update' class='td-update-btn margin-right' onclick='UpdatePlugin(\"" + coll.ID + "\");return false;' title='Update'></a>";
                    editButtons += "<a href='#cancel' class='td-cancel-btn Button-Action' onclick='CancelPlugin();return false;' title='Cancel'></a></div>";
                    str.Append("<td align='center' width='190px' class='border-right border-bottom'>" + editButtons + "</td></tr>");
                    needLoadEditor = true;
                }
                else {
                    str.Append("<tr class='myItemStyle GridNormalRow'>");
                    str.Append("<td width='45px' align='center' class='GridViewNumRow border-bottom'>" + index + "</td>");

                    string associationBtn = string.Empty;
                    if (string.IsNullOrEmpty(aw)) {
                        associationBtn = "<div class='clear-space-two'></div><a href='#' onclick=\"ViewAssociations('" + coll.ID + "');return false;\">View Associations</a>";
                    }
                    else if (hf_BuildAssociatedTable.Value == aw) {
                        associationBtn = "<div class='clear-space-two'></div><small><i>Associated with " + an + "</i></small>";
                    }

                    str.Append("<td width='200px' class='border-right border-bottom'>" + coll.PluginName + associationBtn + "</td>");

                    string desc = coll.Description;
                    if (string.IsNullOrEmpty(desc))
                        desc = "No description available";

                    str.Append("<td class='border-right border-bottom' style='min-width: 300px;'>" + desc + "</td>");

                    string createdBy = name + "<div class='clear-space-two'></div>" + coll.Date;
                    if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
                        createdBy = coll.Date;

                    str.Append("<td width='150px' class='border-right border-bottom'>" + createdBy + "</td>");
                    if (string.IsNullOrEmpty(editId))
                        str.Append("<td width='190px' class='border-right border-bottom'>" + CreateRadioButtonsEdit(coll.Enabled, coll.ID, count, coll.PluginName) + "</td></tr>");
                    else
                        str.Append("<td width='190px' class='border-right border-bottom'></td></tr>");
                }
                totalFound++;
                index++;
            }
        }

        str.Append("</tbody></table></div>");

        lbl_TotalPlugins.Text = count.ToString();
        lbl_TotalEnabledPlugins.Text = enabled.ToString();
        if (totalFound == 0) {
            str.Append("<div class='emptyGridView'>No plugins found</div>");
        }

        lbtn_associationBack.Enabled = false;
        lbtn_associationBack.Visible = false;
        if (!string.IsNullOrEmpty(hf_BuildAssociatedTable.Value) && hf_BuildAssociatedTable.Value != "-") {
            lbtn_associationBack.Enabled = true;
            lbtn_associationBack.Visible = true;
        }

        pnl_siteplugins.Controls.Add(new LiteralControl(str.ToString()));

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

    private void BuildTable_NonEditable() {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
            _sitePlugins.BuildSitePlugins(false);

        else if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
            _sitePlugins.BuildSitePluginsUploadByUser();

        pnl_siteplugins.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='0' cellspacing='0' style='min-width: 100%;'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='200px' align='left'>Plugin Name / Platform</td>");
        str.Append("<td align='left' style='min-width: 300px;'>Description</td>");
        str.Append("<td width='150px' align='left'>Created By / Date</td></tr>");

        bool AssociateWithGroups = _ss.AssociateWithGroups;
        MemberDatabase _member = new MemberDatabase(_username);

        int index = 1;
        int enabled = 0;
        int count = 0;
        foreach (SitePlugins_Coll coll in _sitePlugins.siteplugins_dt) {
            if (!SearchFilterValid(coll)) {
                continue;
            }

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckPluginGroupAssociation(coll, _member)) {
                    continue;
                }
            }

            if (coll.Enabled)
                enabled++;

            if (string.IsNullOrEmpty(coll.AssociatedWith))
                count++;

            string aw = "Nothing";
            if (!string.IsNullOrEmpty(coll.AssociatedWith)) {
                SitePlugins_Coll sitePluginTemp = _sitePlugins.GetPlugin(coll.AssociatedWith);
                if (!string.IsNullOrEmpty(sitePluginTemp.PluginName))
                    aw = sitePluginTemp.PluginName;
            }

            MemberDatabase m = new MemberDatabase(coll.CreatedBy);
            string name = HelperMethods.MergeFMLNames(m);
            str.Append("<tr class='myItemStyle GridNormalRow'>");
            str.Append("<td width='45px' align='center' class='GridViewNumRow border-bottom'>" + index + "</td>");

            str.Append("<td width='200px' class='border-right border-bottom'>" + coll.PluginName + "<div class='clear-space-two'></div><b class='pad-right-sml'>Associated With:</b>" + aw + "</td>");

            string desc = coll.Description;
            if (string.IsNullOrEmpty(desc))
                desc = "N/A";

            str.Append("<td class='border-right border-bottom' style='min-width: 300px;'>" + desc + "</td>");

            string createdBy = name + "<div class='clear-space-two'></div>" + coll.Date;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
                createdBy = coll.Date;

            str.Append("<td width='150px' class='border-right border-bottom'>" + createdBy + "</td></tr>");
            index++;
        }
        lbl_TotalPlugins.Text = count.ToString();
        lbl_TotalEnabledPlugins.Text = enabled.ToString();
        str.Append("</tbody></table></div>");

        if (count == 0) {
            lbl_TotalPlugins.Text = "0";
            lbl_TotalEnabledPlugins.Text = "0";
            str.Append("<div class='emptyGridView'>No plugins found</div>");
        }


        pnl_siteplugins.Controls.Add(new LiteralControl(str.ToString()));
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

                                _sitePlugins.addItem(folderName, txt_uploadPluginName.Text.Trim(), string.Empty, txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, _username);;
                                if ((cb_installAfter.Checked) && (string.IsNullOrEmpty(dd_aw.SelectedValue)) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                                    if (!string.IsNullOrEmpty(folderName))
                                        _sitePlugins.addItemForUser(folderName);
                                }

                                ServerSettings.PageToolViewRedirect(this.Page, "SitePlugins.aspx?newpluginid=" + folderName + "&date=" + ServerSettings.ServerDateTime.Ticks);
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
                                string pluginID = _sitePlugins.addItem(txt_uploadPluginName.Text.Trim(), "~\\" + saveAs, txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, _username);
                                action = "uploadsuccess";

                                if ((cb_installAfter.Checked) && (string.IsNullOrEmpty(dd_aw.SelectedValue)) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
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
                string pluginID = _sitePlugins.addItem(txt_uploadPluginName.Text.Trim(), "", txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, _username);
                action = "uploadsuccess";

                if ((cb_installAfter.Checked) && (string.IsNullOrEmpty(dd_aw.SelectedValue)) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                    if (!string.IsNullOrEmpty(pluginID))
                        _sitePlugins.addItemForUser(pluginID);
                }
            }
        }
        else if ((!string.IsNullOrEmpty(initCode)) && (!string.IsNullOrEmpty(txt_uploadPluginName.Text.Trim()))) {
            string pluginID = _sitePlugins.addItem(txt_uploadPluginName.Text.Trim(), "", txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, _username);
            action = "uploadsuccess";

            if ((cb_installAfter.Checked) && (string.IsNullOrEmpty(dd_aw.SelectedValue)) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                if (!string.IsNullOrEmpty(pluginID))
                    _sitePlugins.addItemForUser(pluginID);
            }
        }

        ServerSettings.PageToolViewRedirect(this.Page, "SitePlugins.aspx?action=" + action + "&date=" + ServerSettings.ServerDateTime.Ticks);
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
                    _sitePlugins.addItem(fi.Name, item.Value, plugin.Description, true, string.Empty, id, _username);
                }
            }
        }

        ServerSettings.PageToolViewRedirect(this.Page, "SitePlugins.aspx?date=" + ServerSettings.ServerDateTime.Ticks);
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

        ServerSettings.PageToolViewRedirect(this.Page, "SitePlugins.aspx?date=" + ServerSettings.ServerDateTime.Ticks);
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
        str.Append("<div class='float-left pad-left'><div class='field switch'>");
        string enabledclass = "cb-enable";
        string disabledclass = "cb-disable selected";
        string onclickEnable = "onclick=\"UpdateEnabled('" + id + "')\"";
        string onclickDisable = "onclick='RemoveUpdateModal();'";
        if (active) {
            enabledclass = "cb-enable selected";
            disabledclass = "cb-disable";
            onclickEnable = "onclick='RemoveUpdateModal();'";
            onclickDisable = "onclick=\"UpdateDisabled('" + id + "')\"";
        }

        str.Append("<span class='" + enabledclass + "'><input id='rb_script_active_" + count.ToString() + "' type='radio' value='active' " + onclickEnable + " /><label for='rb_script_active_" + count.ToString() + "'>On</label></span>");
        str.Append("<span class='" + disabledclass + "'><input id='rb_script_deactive_" + count.ToString() + "' type='radio' value='deactive' " + onclickDisable + " /><label for='rb_script_deactive_" + count.ToString() + "'>Off</label></span>");
        str.Append("</div></div>");
        str.Append("<div class='float-left pad-left-big'>");
        str.Append("<a href='#edit' class='td-edit-btn margin-right Button-Action' onclick='EditPlugin(\"" + id + "\");return false;' title='Edit'></a>");
        str.Append("<a href='#delete' class='td-delete-btn' onclick='DeletePlugin(\"" + id + "\", \"" + name + "\");return false;' title='Delete'></a></div>");
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


    #region Search Methods

    protected void imgbtn_search_Click(object sender, EventArgs e) {
        if (!_ss.SitePluginsLocked) {
            BuildTable_Editable("");
        }
        else {
            BuildTable_NonEditable();
        }
    }

    protected void imgbtn_clearsearch_Click(object sender, EventArgs e) {
        tb_search.Text = tb_search.Attributes["data-defaultvalue"];
        if (!_ss.SitePluginsLocked) {
            BuildTable_Editable("");
        }
        else {
            BuildTable_NonEditable();
        }
        updatepnl_search.Update();
    }

    private bool SearchFilterValid(SitePlugins_Coll sp) {
        string searchText = tb_search.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(searchText) || searchText == tb_search.Attributes["data-defaultvalue"].ToLower()
            || searchText.Contains(sp.PluginName.ToLower()) || sp.PluginName.ToLower().Contains(searchText)
            || searchText.Contains(sp.Description.ToLower()) || sp.Description.ToLower().Contains(searchText)
            || searchText.Contains(sp.CreatedBy.ToLower()) || sp.CreatedBy.ToLower().Contains(searchText)) {
            return true;
        }

        return false;
    }

    #endregion

}