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
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                _username = userId.Name;
                _sitePlugins = new SitePlugins(_username);
                var member = new MemberDatabase(_username);
                _sitetheme = member.SiteTheme;

                App apps = new App();
                lbl_appInstaller.Text = apps.GetAppName("app-appinstaller");

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
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void CheckQueryString() {
        if (Request.QueryString["action"] == "uploadsuccess") {
            lbl_uploadMessage.Text = "Plugin added successfully!";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Green;
        }
        else if (Request.QueryString["action"] == "uploadfailed") {
            lbl_uploadMessage.Text = "Could not upload plugin. Please try again.";
            lbl_uploadMessage.ForeColor = System.Drawing.Color.Red;
        }
        RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function () { $('#lbl_uploadMessage').html(''); }, 3000);");
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
        str.Append("<tr class='myHeaderStyle'><td width='55px'></td><td width='200px' align='left'>Plugin Name / Platform</td>");
        str.Append("<td align='left' style='min-width: 300px;'>Description</td>");
        str.Append("<td width='150px' align='left'>Created By / Date</td>");
        str.Append("<td width='190px'>Actions</td></tr>");
        bool needLoadEditor = false;

        bool AssociateWithGroups = _ss.AssociateWithGroups;
        MemberDatabase _member = new MemberDatabase(_username);

        int index = 1;
        int enabled = 0;
        int count = 0;
        foreach (SitePlugins_Coll coll in _sitePlugins.siteplugins_dt) {
            if (AssociateWithGroups) {
                if (!ServerSettings.CheckPluginGroupAssociation(coll, _member)) {
                    continue;
                }
            }

            MemberDatabase m = new MemberDatabase(coll.CreatedBy);
            string name = HelperMethods.MergeFMLNames(m);

            if (coll.Enabled)
                enabled++;

            if (string.IsNullOrEmpty(coll.AssociatedWith))
                count++;

            if ((!string.IsNullOrEmpty(editId)) && (editId == coll.ID)) {
                str.Append("<tr class='myItemStyle GridNormalRow'>");
                str.Append("<td width='55px' align='center' class='GridViewNumRow border-bottom'>" + index + "</td>");
                string inputName = "<b>Plugin Name</b><br /><input id='txt_updateName' type='text' class='textEntry' style='width:196px;' value='" + coll.PluginName + "' />";
                string dd_updateAW = BuildAWEdit(coll.AssociatedWith, coll.ID);

                str.Append("<td width='200px' valign='top' class='border-right border-bottom'>" + inputName + "<div class='clear-space'></div>" + dd_updateAW + "</td>");

                string inputLocation = "<b>Plugin Path</b><br /><input id='txt_updateLoc' type='text' class='textEntry' style='width:98%' value='" + coll.PluginLocation + "' />";
                string inputDesc = "<b>Description</b><br /><textarea id='txt_updateDesc' rows='5' class='textEntry' style='width:97%;font-family:Arial;padding:3px;border:1px solid #DDD;'>" + coll.Description + "</textarea>";
                string editor = "<b>Initialize Code</b><br /><div style='height: 155px;'><div id='updateEditor'></div></div>";
                hf_updateInitializeCode.Value = HttpUtility.UrlDecode(coll.InitCode);
                str.Append("<td class='border-right border-bottom' style='min-width: 300px;'>" + inputDesc + "<div class='clear-space'></div>" + editor + "</td>");

                string createdBy = "<h3>" + name + "</h3><div class='clear-space-two'></div>" + coll.Date;
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
                str.Append("<td width='55px' align='center' class='GridViewNumRow border-bottom'>" + index + "</td>");

                string aw = "Nothing";
                if (!string.IsNullOrEmpty(coll.AssociatedWith)) {
                    SitePlugins_Coll sitePluginTemp = _sitePlugins.GetPlugin(coll.AssociatedWith);
                    if (!string.IsNullOrEmpty(sitePluginTemp.PluginName))
                        aw = sitePluginTemp.PluginName;
                }

                str.Append("<td width='200px' class='border-right border-bottom'><h3>" + coll.PluginName + "</h3><div class='clear-space-two'></div><b class='pad-right-sml'>Associated With:</b>" + aw + "</td>");

                string desc = coll.Description;
                if (string.IsNullOrEmpty(desc))
                    desc = "N/A";

                str.Append("<td class='border-right border-bottom' style='min-width: 300px;'>" + desc + "</td>");

                string createdBy = "<h3>" + name + "</h3><div class='clear-space-two'></div>" + coll.Date;
                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
                    createdBy = coll.Date;

                str.Append("<td width='150px' class='border-right border-bottom'>" + createdBy + "</td>");
                if (string.IsNullOrEmpty(editId))
                    str.Append("<td width='190px' class='border-right border-bottom'>" + CreateRadioButtonsEdit(coll.Enabled, coll.ID, count, coll.PluginName) + "</td></tr>");
                else
                    str.Append("<td width='190px' class='border-right border-bottom'></td></tr>");
            }
            index++;
            lbl_TotalPlugins.Text = count.ToString();
            lbl_TotalEnabledPlugins.Text = enabled.ToString();
        }

        str.Append("</tbody></table></div>");

        if (count == 0) {
            lbl_TotalPlugins.Text = "0";
            lbl_TotalEnabledPlugins.Text = "0";
            str.Append("<div class='emptyGridView'>No plugins found</div>");
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
        str.Append("<tr class='myHeaderStyle'><td width='55px'></td><td width='200px' align='left'>Plugin Name / Platform</td>");
        str.Append("<td align='left' style='min-width: 300px;'>Description</td>");
        str.Append("<td width='150px' align='left'>Created By / Date</td></tr>");

        bool AssociateWithGroups = _ss.AssociateWithGroups;
        MemberDatabase _member = new MemberDatabase(_username);

        int index = 1;
        int enabled = 0;
        int count = 0;
        foreach (SitePlugins_Coll coll in _sitePlugins.siteplugins_dt) {

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
            str.Append("<td width='55px' align='center' class='GridViewNumRow border-bottom'>" + index + "</td>");

            str.Append("<td width='200px' class='border-right border-bottom'><h3>" + coll.PluginName + "</h3><div class='clear-space-two'></div><b class='pad-right-sml'>Associated With:</b>" + aw + "</td>");

            string desc = coll.Description;
            if (string.IsNullOrEmpty(desc))
                desc = "N/A";

            str.Append("<td class='border-right border-bottom' style='min-width: 300px;'>" + desc + "</td>");

            string createdBy = "<h3>" + name + "</h3><div class='clear-space-two'></div>" + coll.Date;
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
    #endregion


    #region Action Buttons
    protected void btn_addNewPlugin_Clicked(object sender, EventArgs e) {
        bool canContinue = false;
        string script = "";
        string initCode = HttpUtility.UrlEncode(hf_InitializeCode.Value.Trim());
        if ((!string.IsNullOrEmpty(txt_PluginName.Text)) && (!string.IsNullOrEmpty(txt_PluginLoc.Text))
            && (txt_PluginName.Text != "Plugin Name") && (txt_PluginLoc.Text != "Plugin Location/URL")) {
            try {
                Uri uri = new Uri(txt_PluginLoc.Text);
                if (uri.IsAbsoluteUri) {
                    string x = uri.Segments[uri.Segments.Length - 1];
                    FileInfo fi = new FileInfo(x);
                    if ((fi.Extension.ToLower() == ".js") || (fi.Extension.ToLower() == ".css"))
                        canContinue = true;
                }
            }
            catch {
                try {
                    FileInfo fi = new FileInfo(txt_PluginLoc.Text.Trim());
                    if ((fi.Extension.ToLower() == ".js") || (fi.Extension.ToLower() == ".css"))
                        canContinue = true;
                }
                catch { }
            }

            if (canContinue) {
                string pluginID = _sitePlugins.addItem(txt_PluginName.Text.Trim(), txt_PluginLoc.Text.Trim(), txt_Description.Text.Trim(), cb_enabledPlugin.Checked, initCode, dd_aw.SelectedValue, _username);
                txt_PluginName.Text = "Plugin Name";
                txt_PluginLoc.Text = "Plugin Location/URL";
                script += "$('#txt_Description').val('');ReInitAce();";
                script += "$('#lbl_uploadMessage').html(\"<span style='color:Green'>Plugin added successfully!</span>\");";

                if ((cb_installAfter.Checked) && (string.IsNullOrEmpty(dd_aw.SelectedValue))) {
                    if (!string.IsNullOrEmpty(pluginID))
                        _sitePlugins.addItemForUser(pluginID);
                }
                BuildTable_Editable("");
                Build_aw_Dropdown(true);
            }
            else {
                script += "$('#lbl_uploadMessage').html(\"<span style='color:Red'>File extention is not supported. Javascript and Style Sheets are only allowed.</span>\");";
                BuildTable_Editable("");
                Build_aw_Dropdown(false);
            }
        }
        else {
            script += "$('#lbl_uploadMessage').html(\"<span style='color:Red'>Could not upload plugin. Please try again.</span>\");";
            BuildTable_Editable("");
            Build_aw_Dropdown(false);
        }

        RegisterPostbackScripts.RegisterStartupScript(this, script + "setTimeout(function () { $('#lbl_uploadMessage').html(''); }, 3000);");
    }

    protected void btn_uploadPlugin_Clicked(object sender, EventArgs e) {
        string action = "uploadfailed";
        string initCode = HttpUtility.UrlEncode(hf_InitializeCode.Value.Trim());
        if ((!string.IsNullOrEmpty(txt_uploadPluginName.Text)) && (fileupload_Plugin.PostedFile != null) && (txt_uploadPluginName.Text != "Plugin Name")) {
            if (((fileupload_Plugin.PostedFile != null) && (fileupload_Plugin.PostedFile.ContentLength > 0) && (!string.IsNullOrEmpty(fileupload_Plugin.PostedFile.FileName))) || (fileupload_Plugin.HasFile)) {
                FileInfo fi = new FileInfo(fileupload_Plugin.PostedFile.FileName);
                switch (fi.Extension.ToLower()) {
                    #region ZIP FILES

                    case ".zip":
                        break;

                    #endregion

                    default:
                        if (((fileupload_Plugin.PostedFile.ContentType == "application/javascript") && (fi.Extension.ToLower() == ".js"))
                            || ((fileupload_Plugin.PostedFile.ContentType == "text/css") && (fi.Extension.ToLower() == ".css"))) {
                            string filename = fileupload_Plugin.PostedFile.FileName;
                            string folderName = Guid.NewGuid().ToString();
                            string ServerLoc = ServerSettings.GetServerMapLocation;
                            try {

                                if (!Directory.Exists(ServerLoc + "Plugins\\" + folderName)) {
                                    Directory.CreateDirectory(ServerLoc + "Plugins\\" + folderName);
                                }

                                string saveAs = "Plugins\\" + folderName + "\\" + Guid.NewGuid().ToString() + fi.Extension;
                                fileupload_Plugin.SaveAs(ServerLoc + saveAs);
                                string pluginID = _sitePlugins.addItem(txt_uploadPluginName.Text.Trim(), "~\\" + saveAs, txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, _username);
                                action = "uploadsuccess";

                                if ((cb_installAfter.Checked) && (string.IsNullOrEmpty(dd_aw.SelectedValue))) {
                                    if (!string.IsNullOrEmpty(pluginID))
                                        _sitePlugins.addItemForUser(pluginID);
                                }
                            }
                            catch {
                                action = "uploadfailed";
                            }
                        }
                        break;
                }
            }
            else if ((!string.IsNullOrEmpty(initCode)) && (!string.IsNullOrEmpty(txt_uploadPluginName.Text.Trim()))) {
                string pluginID = _sitePlugins.addItem(txt_uploadPluginName.Text.Trim(), "", txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, _username);
                action = "uploadsuccess";

                if ((cb_installAfter.Checked) && (string.IsNullOrEmpty(dd_aw.SelectedValue))) {
                    if (!string.IsNullOrEmpty(pluginID))
                        _sitePlugins.addItemForUser(pluginID);
                }
            }
        }
        else if ((!string.IsNullOrEmpty(initCode)) && (!string.IsNullOrEmpty(txt_uploadPluginName.Text.Trim()))) {
            string pluginID = _sitePlugins.addItem(txt_uploadPluginName.Text.Trim(), "", txt_Description.Text.Trim(), cb_enableUpload.Checked, initCode, dd_aw.SelectedValue, _username);
            action = "uploadsuccess";

            if ((cb_installAfter.Checked) && (string.IsNullOrEmpty(dd_aw.SelectedValue))) {
                if (!string.IsNullOrEmpty(pluginID))
                    _sitePlugins.addItemForUser(pluginID);
            }
        }

        ServerSettings.PageToolViewRedirect(this.Page, "SitePlugins.aspx?action=" + action + "&date=" + DateTime.Now.Ticks);
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

        str.Append("<span class='" + enabledclass + "'><input id='rb_script_active_" + count.ToString() + "' type='radio' value='active' " + onclickEnable + " /><label for='rb_script_active_" + count.ToString() + "'>Enabled</label></span>");
        str.Append("<span class='" + disabledclass + "'><input id='rb_script_deactive_" + count.ToString() + "' type='radio' value='deactive' " + onclickDisable + " /><label for='rb_script_deactive_" + count.ToString() + "'>Disabled</label></span>");
        str.Append("</div></div>");
        str.Append("<div class='float-left pad-left-big'>");
        str.Append("<a href='#edit' class='td-edit-btn margin-right Button-Action' onclick='EditPlugin(\"" + id + "\");return false;' title='Edit'></a>");
        str.Append("<a href='#delete' class='td-delete-btn' onclick='DeletePlugin(\"" + id + "\", \"" + name + "\");return false;' title='Delete'></a></div>");
        return str.ToString();
    }

    protected void hf_deletePlugin_Changed(object sender, EventArgs e) {
        string id = hf_deletePlugin.Value;
        if (!string.IsNullOrEmpty(id)) {
            try {
                string loc = _sitePlugins.GetPathLocation(id);
                if (!string.IsNullOrEmpty(loc)) {
                    if (loc.StartsWith("~")) {
                        loc = loc.Remove(0, 1);
                    }

                    if (!loc.ToLower().Contains("plugins\\") || !loc.ToLower().Contains("plugins/")) {
                        loc = "Plugins\\" + loc;
                    }

                    if (loc[0] == '\\') {
                        loc = loc.Remove(0, 1);
                    }

                    FileInfo fi = new FileInfo(ServerSettings.GetServerMapLocation + loc);
                    string dir = fi.Directory.Name;
                    string filename = fi.FullName;
                    if (dir.ToLower() != "plugins") {
                        try {
                            if (Directory.Exists(fi.Directory.FullName)) {
                                Directory.Delete(fi.Directory.FullName, true);
                            }
                        }
                        catch (Exception ex) {
                            AppLog.AddError(ex);
                        }
                    }
                    else if (File.Exists(filename)) {
                        try {
                            File.Delete(filename);
                        }
                        catch (Exception ex) {
                            AppLog.AddError(ex);
                        }
                    }
                }
            }
            catch { }

            _sitePlugins.deleteItem(id);
            BuildTable_Editable("");
            Build_aw_Dropdown(true);
        }

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