#region

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

public partial class SiteTools_StartupScripts : BasePage {

    protected void Page_Load(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(AsyncPostBackSourceElementID) || (!string.IsNullOrEmpty(AsyncPostBackSourceElementID) && !AsyncPostBackSourceElementID.Contains("rb_appendTimestamp_"))) {
            if (MainServerSettings.AppendTimestampOnScripts) {
                rb_appendTimestamp_on.Checked = true;
                rb_appendTimestamp_off.Checked = false;
            }
            else {
                rb_appendTimestamp_on.Checked = false;
                rb_appendTimestamp_off.Checked = true;
            }
        }

        if (!MainServerSettings.LockStartupScripts && IsUserInAdminRole()) {
            pnl_findreplace.Enabled = true;
            pnl_findreplace.Visible = true;
            aFindReplace.Visible = true;
            aFindReplace_css.Visible = true;
            BuildStartupScripts();
            BuildStartupScripts_CSS();
        }
        else {
            pnl_findreplace.Enabled = false;
            pnl_findreplace.Visible = false;
            aFindReplace.Visible = false;
            aFindReplace_css.Visible = false;
            BuildStartupScripts_ViewOnly();
            BuildStartupScripts_CSS_ViewOnly();
            ltl_locked.Text = HelperMethods.GetLockedByMessage();
        }

        settings.Visible = false;
        if (IsUserInAdminRole() && !MainServerSettings.LockStartupScripts) {
            settings.Visible = true;
        }

        BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);

        if (Request.QueryString.Count > 0) {
            if (HelperMethods.ConvertBitToBoolean(Request.QueryString["css_view"])) {
                CssChecked();
            }
            else {
                JsChecked();
            }
        }
        else {
            if (!IsPostBack)
                JsChecked();
            else {
                if (string.IsNullOrEmpty(AsyncPostBackSourceElementID)) {
                    JsChecked();
                }
            }
        }
    }

    protected void btn_findreplace_Clicked(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(tb_findvalue.Text)) {
            string findvalue = tb_findvalue.Text;
            var startupscripts = new StartupScripts(true, true);
            var startupscriptsCss = new StartupStyleSheets(true, true);
            foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
                if (!coll.ScriptPath.Contains(findvalue)) continue;
                string replacevalue = coll.ScriptPath.Replace(findvalue, tb_replacevalue.Text);
                startupscripts.updateScriptPath(coll.ID, replacevalue, CurrentUsername, false);
            }
            foreach (StartupScriptsSheets_Coll coll in startupscriptsCss.StartupScriptsSheetsList) {
                if (!coll.ScriptPath.Contains(findvalue)) continue;
                string replacevalue = coll.ScriptPath.Replace(findvalue, tb_replacevalue.Text);
                startupscriptsCss.updateScriptPath(coll.ID, replacevalue, CurrentUsername, false);
            }

            BuildStartupScripts();
            BuildStartupScripts_CSS();
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "ReAssignButtonSelected();");
    }


    #region Startup Scripts Javascript

    private void BuildStartupScripts() {
        var startupscripts = new StartupScripts(true, true);
        pnl_startupscripts.Controls.Clear();
        var str = new StringBuilder();

        App apps = new App(string.Empty);

        // Build Header
        str.Append("<div id='js_sortable' class='margin-top-sml'>");
        str.Append("<table id='js_sortable-table' style='width: 100%;' cellpadding='5' cellspacing='0'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='55px'>#</td><td>Script Path</td><td width='200px'>Applies To</td><td class='edit-column-2-items'></td></tr>");
        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
            string id = coll.ID;
            int count = coll.Sequence;
            string path = coll.ScriptPath;

            str.Append("<tr class='myItemStyle GridNormalRow'>");
            str.Append("<td width='55px' class='GridViewNumRow sorted-js border-bottom' style='cursor:move'>" + count + "<span style='display:none'>" + coll.ID + "</span></td>");
            str.Append("<td class='non-moveable border-bottom'>" + GetPathLink(path) + "</td>");

            string applyto = coll.ApplyTo;
            if (applyto.StartsWith("app-")) {
                applyto = apps.GetAppName(applyto);
            }

            str.Append("<td width='200px' class='non-moveable border-bottom' align='left'>" + applyto + "</td>");
            str.Append("<td align='center' class='non-moveable border-bottom edit-column-2-items-nocontent myItemStyle-action-btns'>" + CreateRadioButtons_StartupScripts(coll.ID) + "</td>");
            str.Append("</tr>");
        }
        str.Append(CreateNew_StartupScripts());
        str.Append("</tbody></table></div>");
        str.Append("<div class='clear'></div>");
        pnl_startupscripts.Controls.Add(new LiteralControl(str.ToString()));
    }
    private void BuildStartupScripts_ViewOnly() {
        var startupscripts = new StartupScripts(true, true);
        pnl_startupscripts.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table style='width: 100%;' cellpadding='5' cellspacing='0'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='55px'>#</td><td>Script Path</td><td width='200px'>Applies To</td><td width='170px'>Date Updated</td></tr>");

        App apps = new App(string.Empty);

        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
            int count = coll.Sequence;
            if (count == 0) {
                startupscripts.updateSequence(coll.ID, 1, CurrentUsername);
                count = 1;
            }

            str.Append("<tr class='myItemStyle GridNormalRow'>");
            str.Append("<td width='55px' class='GridViewNumRow border-bottom'>" + count + "</td>");
            str.Append("<td class='border-bottom'><div class='float-left' style='height: 25px;'></div><span style='line-height: 25px;'>" + GetPathLink(coll.ScriptPath) + "</span></td>");

            string applyto = coll.ApplyTo;
            if (string.IsNullOrEmpty(applyto)) {
                applyto = "Base/Workspace";
            }
            else if (applyto.StartsWith("app-")) {
                applyto = apps.GetAppName(applyto);
            }

            str.Append("<td width='200px' class='border-bottom'>" + applyto + "</td>");
            str.Append("<td width='170px' align='left' class='border-bottom'>" + coll.DateUpdated + "</td>");
            str.Append("</tr>");
        }
        str.Append("</tbody></table></div>");

        if (startupscripts.StartupscriptsList.Count == 0)
            str.Append("<div class='emptyGridView'>No data available.</div>");

        str.Append("<div class='clear'></div>");
        pnl_startupscripts.Controls.Add(new LiteralControl(str.ToString()));
    }
    private string CreateNew_StartupScripts() {
        var str = new StringBuilder();
        string addBtn = "<a id='btn_createnew_startupscript' class='td-add-btn RandomActionBtns' onclick='return false;' title='Add' style='visibility: visible!important;'></a>";
        str.Append("<tr class='myItemStyle GridNormalRow'>");
        str.Append("<td width='55px' class='GridViewNumRow non-moveable border-bottom' style='cursor:default'>" + BuildSequenceDropDown() + "</td>");
        str.Append("<td align='left' class='non-moveable border-bottom'><input id='tb_createnew_startupscript' type='text' class='textEntry' placeholder='File Location' style='width: 95%;'></td>");
        str.Append("<td width='200px' class='non-moveable border-bottom' align='left'>" + BuildApplytoDropDown_addnew() + "</td>");
        str.Append("<td align='center' class='non-moveable border-bottom edit-column-2-items-nocontent'>" + addBtn + "</td>");
        str.Append("</tr>");
        return str.ToString();
    }
    private string BuildSequenceDropDown() {
        var startupscripts = new StartupScripts(true, true);
        var str = new StringBuilder();
        str.Append("<input type='hidden' id='dd_startupsequence' value='" +
                   (startupscripts.StartupscriptsList.Count + 1) + "' />");
        return str.ToString();
    }
    private static string BuildApplytoDropDown_addnew() {
        var str = new StringBuilder();
        str.Append("<select id='dd_startupApplyTo' style='width:95%'>");
        str.Append("<option value='Base/Workspace'>Base/Workspace</option>");
        str.Append("<option value='Workspace Only'>Workspace Only</option>");
        str.Append("<option value='App Remote'>App Remote</option>");
        str.Append("<option value='Chat Client'>Chat Client</option>");
        str.Append("<option value='All Components'>All Components</option>");
        str.Append("<option value='Table Imports'>Table Imports</option>");
        str.Append("<option value='Custom Tables'>Custom Tables</option>");
        var apps = new App(string.Empty);
        apps.GetAllApps();

        foreach (Apps_Coll dr in apps.AppList) {
            str.Append("<option value='" + dr.AppId + "'>" + dr.AppName + "</option>");
        }
        str.Append("</select>");
        return str.ToString();
    }
    private static string BuildApplytoDropDown_list(string id, string currAppliesTo) {
        var str = new StringBuilder();
        str.Append("<select id='ddl-edit-app-js' style='width:95%'>");
        string workspaceonly = "";
        string customtables = "";
        string chatclient = "";
        string dataimport = "";
        string allonly = "";
        string appremote = "";
        switch (currAppliesTo) {
            case "Workspace Only":
                workspaceonly = " selected";
                break;
            case "Custom Tables":
                customtables = " selected";
                break;
            case "Chat Client":
                chatclient = " selected";
                break;
            case "Table Imports":
                dataimport = " selected";
                break;
            case "All Components":
                allonly = "selected";
                break;
            case "App Remote":
                appremote = "selected";
                break;
        }
        str.Append("<option value='Base/Workspace'>Base/Workspace</option>");
        str.Append("<option value='Workspace Only'" + workspaceonly + ">Workspace Only</option>");
        str.Append("<option value='App Remote'" + appremote + ">App Remote</option>");
        str.Append("<option value='Chat Client'" + chatclient + ">Chat Client</option>");
        str.Append("<option value='All Components'" + allonly + ">All Components</option>");
        str.Append("<option value='Table Imports'" + dataimport + ">Table Imports</option>");
        str.Append("<option value='Custom Tables'" + customtables + ">Custom Tables</option>");
        var apps = new App(string.Empty);
        apps.GetAllApps();

        foreach (Apps_Coll dr in apps.AppList) {
            string selected = "";
            if (dr.AppId == currAppliesTo) {
                selected = " selected";
            }
            str.Append("<option value='" + dr.AppId + "'" + selected + ">" + dr.AppName + "</option>");
        }
        str.Append("</select>");
        return str.ToString();
    }
    private string CreateRadioButtons_StartupScripts(string id) {
        var str = new StringBuilder();
        str.Append("<a class='td-edit-btn' onclick='EditStartupScript(\"" + id + "\");return false;' title='Edit'></a>");
        str.Append("<a class='td-delete-btn' onclick='DeleteStartupScript(\"" + id + "\");return false;' title='Delete'></a>");
        return str.ToString();
    }
    private string CreateRadioButtonsEdit_StartupScripts(string id) {
        var str = new StringBuilder();
        str.Append("<a class='td-update-btn' onclick='DoneEditStartupScript(\"" + id + "\");return false;' title='Update'></a>");
        str.Append("<a class='td-cancel-btn' onclick='CancelStartupScript();return false;' title='Cancel'></a>");
        return str.ToString();
    }
    protected void hf_UpdateStartupScripts_ValueChanged(object sender, EventArgs e) {
        BuildStartupScripts();
    }
    protected void hf_EditStartupScripts_ValueChanged(object sender, EventArgs e) {
        var startupscripts = new StartupScripts(true, true);
        pnl_startupscripts.Controls.Clear();
        var str = new StringBuilder();
        App apps = new App(string.Empty);
        string editid = hf_EditStartupScripts.Value;

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table style='width: 100%;' cellpadding='5' cellspacing='0'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='55px'>#</td><td>Script Path</td><td width='200px'>Applies To</td><td class='edit-column-2-items'></td></tr>");

        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
            int count = coll.Sequence;
            string path = coll.ScriptPath;

            str.Append("<tr class='myItemStyle GridNormalRow'>");
            str.Append("<td width='55px' class='GridViewNumRow sorted-js border-bottom' style='cursor:default'>" + coll.Sequence + "<span style='display:none'>" + coll.ID + "</span></td>");
            if (coll.ID == editid) {
                str.Append("<td class='non-moveable border-bottom'><input id='tb_edit_startupscript' type='text' style='width: 95%' class='textEntry' onkeypress='tb_edit_startupscript_KeyPressed(event, \"" + coll.ID + "\");' value='" + coll.ScriptPath + "' /></td>");
                str.Append("<td class='non-moveable border-bottom' width='200px' align='left'>" + BuildApplytoDropDown_list(coll.ID, coll.ApplyTo) + "</td>");
                str.Append("<td align='center' class='non-moveable border-bottom edit-column-2-items-nocontent myItemStyle-action-btns'>" + CreateRadioButtonsEdit_StartupScripts(coll.ID) + "</td>");
            }
            else {
                str.Append("<td class='non-moveable border-bottom'><div class='float-left' style='height: 25px;'></div><span style='line-height: 25px;'>" + GetPathLink(coll.ScriptPath) + "</span></td>");

                string applyto = coll.ApplyTo;
                if (applyto.StartsWith("app-")) {
                    applyto = apps.GetAppName(applyto);
                }

                str.Append("<td class='non-moveable border-bottom' width='200px' align='left'>" + applyto + "</td>");
                str.Append("<td class='non-moveable border-bottom edit-column-2-items-nocontent myItemStyle-action-btns'></td>");
            }
            str.Append("</tr>");
        }
        str.Append("</tbody></table></div>");
        str.Append("<div class='clear'></div>");
        pnl_startupscripts.Controls.Add(new LiteralControl(str.ToString()));
        hf_EditStartupScripts.Value = "";
    }

    #endregion


    #region Startup Scripts CSS

    private void BuildStartupScripts_CSS() {
        var startupscripts = new StartupStyleSheets(true, true);
        pnl_startupscripts_css.Controls.Clear();
        var str = new StringBuilder();

        App apps = new App(string.Empty);

        // Build Header
        str.Append("<div id='css_sortable' class='margin-top-sml'>");
        str.Append("<table id='css_sortable-table' style='width: 100%;' cellpadding='0' cellspacing='0'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='55px'>#</td><td>Script Path</td><td width='200px'>Applies To</td>");
        str.Append("<td width='150px'>Theme</td><td class='edit-column-2-items'></td></tr>");
        foreach (StartupScriptsSheets_Coll coll in startupscripts.StartupScriptsSheetsList) {
            str.Append("<tr class='myItemStyle GridNormalRow'>");
            str.Append("<td width='55px' class='GridViewNumRow sorted-css border-bottom' style='cursor:move'>" + coll.Sequence + "<span style='display:none'>" + coll.ID + "</span></td>");
            str.Append("<td class='non-moveable border-bottom'><div class='float-left' style='height: 25px;'></div><span style='line-height: 25px;'>" + GetPathLink(coll.ScriptPath) + "</span></td>");

            string applyto = coll.ApplyTo;
            if (applyto.StartsWith("app-")) {
                applyto = apps.GetAppName(applyto);
            }

            str.Append("<td width='200px' class='non-moveable border-bottom' align='left'>" + applyto + "</td>");
            str.Append("<td width='150px' class='non-moveable border-bottom' align='left'>" + coll.Theme + "</td>");
            str.Append("<td align='center' class='non-moveable border-right border-bottom edit-column-2-items-nocontent myItemStyle-action-btns'>" + CreateRadioButtons_StartupScripts_CSS(coll.ID) + "</td>");
            str.Append("</tr>");
        }
        str.Append(CreateNew_StartupScripts_CSS());
        str.Append("</tbody></table></div>");
        str.Append("<div class='clear'></div>");
        pnl_startupscripts_css.Controls.Add(new LiteralControl(str.ToString()));
    }
    private void BuildStartupScripts_CSS_ViewOnly() {
        var startupscripts = new StartupStyleSheets(true, true);
        pnl_startupscripts_css.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table style='width: 100%;' cellpadding='0' cellspacing='0'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='55px'>#</td><td>Script Path</td><td width='200px'>Applies To</td>");
        str.Append("<td width='100px'>Theme</td><td width='170px'>Date Updated</td></tr>");

        App apps = new App(string.Empty);

        foreach (StartupScriptsSheets_Coll coll in startupscripts.StartupScriptsSheetsList) {
            int count = coll.Sequence;
            if (count == 0) {
                startupscripts.updateSequence(coll.ID, 1, CurrentUsername);
                count = 1;
            }

            str.Append("<tr class='myItemStyle GridNormalRow'>");
            str.Append("<td width='55px' class='GridViewNumRow border-bottom'>" + count + "</td>");
            str.Append("<td class='border-bottom'><div class='float-left' style='height: 25px;'></div><span style='line-height: 25px;'>" + GetPathLink(coll.ScriptPath) + "</span></td>");

            string applyto = coll.ApplyTo;
            if (string.IsNullOrEmpty(applyto))
                applyto = "Base/Workspace";
            else if (applyto.StartsWith("app-")) {
                applyto = apps.GetAppName(applyto);
            }

            str.Append("<td width='200px' class='border-bottom'>" + applyto + "</td>");

            string theme = "Standard";
            if (!string.IsNullOrEmpty(coll.Theme))
                theme = coll.Theme;

            str.Append("<td width='100px' align='left' class='border-bottom'>" + theme + "</td>");
            str.Append("<td width='170px' align='left' class='border-bottom'>" + coll.DateUpdated + "</td>");
            str.Append("</tr>");
        }
        str.Append("</tbody></table></div>");

        if (startupscripts.StartupScriptsSheetsList.Count == 0)
            str.Append("<div class='emptyGridView'>No data available.</div>");

        str.Append("<div class='clear'></div>");
        pnl_startupscripts_css.Controls.Add(new LiteralControl(str.ToString()));
    }
    private string CreateNew_StartupScripts_CSS() {
        var str = new StringBuilder();
        string addBtn = "<a id='btn_createnew_startupscript_CSS' class='td-add-btn RandomActionBtns' onclick='return false;' title='Add' style='visibility: visible!important;'></a>";
        str.Append("<tr class='myItemStyle GridNormalRow'>");
        str.Append("<td width='55px' class='GridViewNumRow non-moveable border-bottom' style='cursor:default'>" + BuildSequenceDropDown_CSS() + "</td>");
        str.Append("<td align='left' class='non-moveable border-bottom'><input id='tb_createnew_startupscript_CSS' type='text' class='textEntry' placeholder='File Location' style='width: 95%;'></td>");
        str.Append("<td width='200px' class='non-moveable border-bottom' align='left'>" + BuildApplytoDropDown_addnew_CSS() + "</td>");
        str.Append("<td width='150px' class='non-moveable border-bottom' align='left'>" + BuildThemeDropDown_addnew_CSS() + "</td>");
        str.Append("<td align='center' class='non-moveable border-bottom edit-column-2-items-nocontent'>" + addBtn + "</td>");
        str.Append("</tr>");
        return str.ToString();
    }
    private static string BuildSequenceDropDown_CSS() {
        var startupscripts = new StartupStyleSheets(true, true);
        var str = new StringBuilder();
        str.Append("<input type='hidden' id='dd_startupsequence_CSS' value='" +
                   (startupscripts.StartupScriptsSheetsList.Count + 1) + "' />");
        return str.ToString();
    }
    private static string BuildApplytoDropDown_addnew_CSS() {
        var str = new StringBuilder();
        str.Append("<select id='dd_startupApplyTo_CSS' style='width:95%'>");
        str.Append("<option value='Base/Workspace'>Base/Workspace</option>");
        str.Append("<option value='Workspace Only'>Workspace Only</option>");
        str.Append("<option value='App Remote'>App Remote</option>");
        str.Append("<option value='Chat Client'>Chat Client</option>");
        str.Append("<option value='All Components'>All Components</option>");
        str.Append("<option value='Table Imports'>Table Imports</option>");
        str.Append("<option value='Custom Tables'>Custom Tables</option>");
        var apps = new App(string.Empty);
        apps.GetAllApps();

        foreach (Apps_Coll dr in apps.AppList) {
            str.Append("<option value='" + dr.AppId + "'>" + dr.AppName + "</option>");
        }

        //var plugins = new SitePlugins(HttpContext.Current.User.Identity.Name);
        //plugins.BuildSitePlugins(true);
        //foreach (SitePlugins_Coll coll in plugins.siteplugins_dt)
        //{
        //    str.Append("<option value='plugin~" + coll.ID + "'>Plugin: " + coll.PluginName + "</option>");
        //}

        str.Append("</select>");
        return str.ToString();
    }
    private string BuildThemeDropDown_addnew_CSS() {
        var str = new StringBuilder();
        str.Append("<select id='dd_theme_CSS' style='width:95%;'>");
        str.Append("<option value='All' selected>All Themes</option>");
        var di = new DirectoryInfo(ServerSettings.GetServerMapLocation + "App_Themes\\");

        foreach (var dir in di.GetDirectories().Where(dir => !dir.Name.ToLower().Contains("login")))
            str.Append("<option value='" + dir.Name + "'>" + dir.Name + "</option>");

        str.Append("</select>");
        return str.ToString();
    }
    private static string BuildApplytoDropDown_list_CSS(string id, string currAppliesTo) {
        var str = new StringBuilder();
        str.Append("<select id='ddl-edit-app-css' style='width:95%'>");
        string workspaceonly = "";
        string customtables = "";
        string chatclient = "";
        string dataimport = "";
        string allonly = "";
        string appremote = "";
        switch (currAppliesTo) {
            case "Workspace Only":
                workspaceonly = " selected";
                break;
            case "Custom Tables":
                customtables = " selected";
                break;
            case "Chat Client":
                chatclient = " selected";
                break;
            case "Table Imports":
                dataimport = " selected";
                break;
            case "All Components":
                allonly = " selected";
                break;
            case "App Remote":
                appremote = " selected";
                break;
        }
        str.Append("<option value='Base/Workspace'>Base/Workspace</option>");
        str.Append("<option value='Workspace Only'" + workspaceonly + ">Workspace Only</option>");
        str.Append("<option value='App Remote'" + appremote + ">App Remote</option>");
        str.Append("<option value='Chat Client'" + chatclient + ">Chat Client</option>");
        str.Append("<option value='All Components'" + allonly + ">All Components</option>");
        str.Append("<option value='Table Imports'" + dataimport + ">Table Imports</option>");
        str.Append("<option value='Custom Tables'" + customtables + ">Custom Tables</option>");
        var apps = new App(string.Empty);
        apps.GetAllApps();

        foreach (Apps_Coll dr in apps.AppList) {
            string selected = "";
            if (dr.AppId == currAppliesTo)
                selected = " selected";
            str.Append("<option value='" + dr.AppId + "'" + selected + ">" + dr.AppName + "</option>");
        }

        //var plugins = new SitePlugins(HttpContext.Current.User.Identity.Name);
        //plugins.BuildSitePlugins(true);
        //foreach (SitePlugins_Coll coll in plugins.siteplugins_dt)
        //{
        //    string selected = "";
        //    if ("plugin~" + coll.ID == currAppliesTo)
        //        selected = " selected";
        //    str.Append("<option value='plugin~" + coll.ID + "'" + selected + ">Plugin: " + coll.PluginName + "</option>");
        //}

        str.Append("</select>");
        return str.ToString();
    }
    private string BuildThemeDropDown_list_CSS(string id, string theme) {
        var str = new StringBuilder();
        str.Append("<select id='ddl-edit-theme-css' style='max-width:95%'>");
        string sel = "";
        if (theme == "All") {
            sel = " selected";
        }
        str.Append("<option value='All'" + sel + ">All Themes</option>");
        string sitetheme = theme;

        if (string.IsNullOrEmpty(sitetheme)) {
            sitetheme = "Standard";
        }
        var di = new DirectoryInfo(ServerSettings.GetServerMapLocation + "App_Themes\\");
        foreach (var dir in di.GetDirectories()) {
            if (!dir.Name.ToLower().Contains("login")) {
                string selected = "";
                if (dir.Name == sitetheme) {
                    selected = " selected";
                }
                str.Append("<option value='" + dir.Name + "'" + selected + ">" + dir.Name + "</option>");
            }
        }
        str.Append("</select>");
        return str.ToString();
    }
    private string CreateRadioButtons_StartupScripts_CSS(string id) {
        var str = new StringBuilder();
        str.Append("<a class='td-edit-btn' onclick='EditStartupScript_CSS(\"" + id + "\");return false;' title='Edit'></a>");
        str.Append("<a class='td-delete-btn' onclick='DeleteStartupScript_CSS(\"" + id + "\");return false;' title='Delete'></a>");
        return str.ToString();
    }
    private string CreateRadioButtonsEdit_StartupScripts_CSS(string id) {
        var str = new StringBuilder();
        str.Append("<a class='td-update-btn' onclick='DoneEditStartupScript_CSS(\"" + id + "\");return false;' title='Update'></a>");
        str.Append("<a class='td-cancel-btn' onclick='CancelStartupScript_CSS();return false;' title='Cancel'></a>");
        return str.ToString();
    }
    protected void hf_UpdateStartupScripts_CSS_ValueChanged(object sender, EventArgs e) {
        BuildStartupScripts_CSS();
    }
    protected void hf_EditStartupScripts_CSS_ValueChanged(object sender, EventArgs e) {
        var startupscripts = new StartupStyleSheets(true, true);
        pnl_startupscripts_css.Controls.Clear();
        var str = new StringBuilder();
        App apps = new App(string.Empty);
        string editid = hf_EditStartupScripts_css.Value;

        Guid temp = new Guid();
        if (!Guid.TryParse(editid, out temp)) {
            BuildStartupScripts_CSS();
        }
        else {
            // Build Header
            str.Append("<div class='margin-top-sml'>");
            str.Append("<table style='width: 100%;' cellpadding='5' cellspacing='0'><tbody>");
            str.Append("<tr class='myHeaderStyle'><td width='55px'>#</td><td>Script Path</td><td width='200px'>Applies To</td>");
            str.Append("<td width='150px'>Theme</td><td class='edit-column-2-items'></td></tr>");

            foreach (StartupScriptsSheets_Coll coll in startupscripts.StartupScriptsSheetsList) {
                str.Append("<tr class='myItemStyle GridNormalRow'>");
                str.Append("<td width='55px' class='GridViewNumRow non-moveable border-bottom' style='cursor:default'>" + coll.Sequence + "</td>");
                if (coll.ID == editid) {
                    str.Append("<td class='non-moveable border-bottom'><input id='tb_edit_startupscript_CSS' type='text' style='width: 95%' class='textEntry' value='" + coll.ScriptPath + "' /></td>");
                    str.Append("<td class='non-moveable border-bottom' width='200px' align='left'>" + BuildApplytoDropDown_list_CSS(coll.ID, coll.ApplyTo) + "</td>");
                    str.Append("<td width='150px' class='non-moveable border-bottom' align='left'>" + BuildThemeDropDown_list_CSS(coll.ID, coll.Theme) + "</td>");
                    str.Append("<td align='center' class='non-moveable border-bottom edit-column-2-items-nocontent myItemStyle-action-btns'>" + CreateRadioButtonsEdit_StartupScripts_CSS(coll.ID) + "</td>");
                }
                else {
                    str.Append("<td class='non-moveable border-bottom'><div class='float-left' style='height: 25px;'></div><span style='line-height: 25px;'>" + GetPathLink(coll.ScriptPath) + "</span></td>");

                    string applyto = coll.ApplyTo;
                    if (applyto.StartsWith("app-")) {
                        applyto = apps.GetAppName(applyto);
                    }

                    str.Append("<td class='non-moveable border-bottom' width='200px' align='left'>" + applyto + "</td>");
                    str.Append("<td width='150px' class='non-moveable border-bottom' align='left'>" + coll.Theme + "</td>");
                    str.Append("<td class='non-moveable border-bottom edit-column-2-items-nocontent myItemStyle-action-btns'></td>");
                }
                str.Append("</tr>");
            }
            str.Append("</tbody></table></div>");
            str.Append("<div class='clear'></div>");
            pnl_startupscripts_css.Controls.Add(new LiteralControl(str.ToString()));
        }
        hf_EditStartupScripts_css.Value = "";
    }

    #endregion


    #region Buttons

    private void JsChecked() {
        var str = new StringBuilder();
        str.Append("openWSE.LoadSiteMenuTab('?tab=javascripts');");
        RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
    }
    private void CssChecked() {
        var str = new StringBuilder();
        str.Append("openWSE.LoadSiteMenuTab('?tab=stylesheets');");
        RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
    }

    #endregion

    private string GetPathLink(string path) {
        string tempPath = path;
        if (path.StartsWith("~/")) {
            path = ResolveUrl(path);
        }

        return "<a href='" + path + "' target='_blank'>" + tempPath + "</a>";
    }

    protected void rb_appendTimestamp_on_CheckedChanged(object sender, EventArgs e) {
        rb_appendTimestamp_on.Checked = true;
        rb_appendTimestamp_off.Checked = false;
        ServerSettings.update_AppendTimestampOnScripts(true);
    }
    protected void rb_appendTimestamp_off_CheckedChanged(object sender, EventArgs e) {
        rb_appendTimestamp_on.Checked = false;
        rb_appendTimestamp_off.Checked = true;
        ServerSettings.update_AppendTimestampOnScripts(false);
    }
}