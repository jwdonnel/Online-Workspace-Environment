#region

using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Image = System.Drawing.Image;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using OpenWSE_Tools.Overlays;

#endregion

public partial class SiteTools_AppManager : Page {

    #region Private Variables

    private readonly ImageConverter _imageConverter = new ImageConverter();
    private readonly AppLog _applog = new AppLog(false);
    private readonly AppCategory _category = new AppCategory(true);
    private ServerSettings _ss = new ServerSettings();
    private readonly WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
    private string _username;
    private App _apps;
    private string newAppID;

    #endregion


    #region PageLoad and help methods

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                _username = userId.Name;
                _apps = new App(_username);

                if (!IsPostBack) {
                    BuildLinks();
                }

                SideBarItems sidebar = new SideBarItems(_username);

                if (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    cb_InstallAfterLoad.Visible = false;
                    cb_InstallAfterLoad.Enabled = false;
                    cb_isPrivate.Enabled = false;
                    cb_isPrivate.Visible = false;
                }
                else {
                    cb_InstallAfterLoad.Visible = true;
                    cb_InstallAfterLoad.Enabled = true;
                    cb_isPrivate.Enabled = true;
                    cb_isPrivate.Visible = true;
                }

                if (_ss.HideAllAppIcons) {
                    pnl_appicon.Enabled = false;
                    pnl_appicon.Visible = false;
                    pnl_appIconEdit.Enabled = false;
                    pnl_appIconEdit.Visible = false;
                }
                else {
                    pnl_appicon.Enabled = true;
                    pnl_appicon.Visible = true;
                    pnl_appIconEdit.Enabled = true;
                    pnl_appIconEdit.Visible = true;
                }

                var scriptManager = ScriptManager.GetCurrent(this);
                if (scriptManager != null) {
                    scriptManager.RegisterPostBackControl(btn_uploadnew);
                    scriptManager.RegisterPostBackControl(btn_save_2);
                    scriptManager.RegisterAsyncPostBackControl(btn_save);
                    scriptManager.RegisterAsyncPostBackControl(hf_createapp);
                    scriptManager.RegisterAsyncPostBackControl(hf_saveapp);
                    scriptManager.RegisterAsyncPostBackControl(lbtn_close);
                }
                var manager = ScriptManager.GetCurrent(this);
                if ((dd_category.Items.Count == 0) || (dd_category_edit.Items.Count == 0))
                    BuildCategories();

                if (dd_package.Items.Count == 0)
                    BuildPackages();

                if (cc_associatedOverlayNew.Items.Count == 0)
                    BuildOverlayDropDown();

                if ((_ss.LockASCXEdit) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                    dd_filename_ext.Enabled = false;
                    dd_filename_ext.Visible = false;
                    lbl_dotHtml.Enabled = true;
                    lbl_dotHtml.Visible = true;
                }
                else {
                    dd_filename_ext.Enabled = true;
                    dd_filename_ext.Visible = true;
                    lbl_dotHtml.Enabled = false;
                    lbl_dotHtml.Visible = false;
                }

                if (!Roles.IsUserInRole(userId.Name, ServerSettings.AdminUserName)) {
                    btn_performCleanup.Enabled = false;
                    btn_performCleanup.Visible = false;
                    pnl_backupAllApps.Enabled = false;
                    pnl_backupAllApps.Visible = false;
                }

                if (Request.QueryString["c"] == "upload") {
                    pnl_appList2.Controls.Add(new LiteralControl(sidebar.BuildAppEditor(Page)));
                    if (Request.QueryString["b"] == "app_installed") {
                        if (!IsPostBack) {
                            SetReturnMessage("App installed successfully!", false, 3000);

                            if (!string.IsNullOrEmpty(Request.QueryString["isZip"])) {
                                if (HelperMethods.ConvertBitToBoolean(Request.QueryString["isZip"])) {
                                    if (!string.IsNullOrEmpty(Request.QueryString["appId"])) {
                                        radioButton_FileList_New.Items.Clear();
                                        string filePath = ServerSettings.GetServerMapLocation + "Apps\\" + Request.QueryString["appId"];
                                        try {
                                            DirectoryInfo di = new DirectoryInfo(filePath);
                                            GetLoaderFileListNew(di.FullName, di.Name);

                                            RegisterPostbackScripts.RegisterStartupScript(this, "$(window).load(function () { LoadNewDefaultPageSelector(); });");
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                        btn_uploadnew.Enabled = true;
                        btn_uploadnew.Visible = true;
                        pnl_filename.Enabled = false;
                        pnl_filename.Visible = false;
                    }
                    btn_viewCode.Visible = false;
                    pnl_htmleditor.Style["display"] = "none";
                    btn_create.Enabled = false;
                    btn_create.Visible = false;
                    btn_clear_controls.Enabled = true;
                    btn_clear_controls.Visible = true;
                    btn_uploadnew.Enabled = true;
                    btn_uploadnew.Visible = true;
                    pnl_filename.Enabled = false;
                    pnl_filename.Visible = false;
                    cb_wrapIntoIFrame.Enabled = false;
                    cb_wrapIntoIFrame.Visible = false;
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#_hdl3').addClass('active');$('#newupload').show();");
                }
                else if (Request.QueryString["c"] == "easycreate") {
                    pnl_appList2.Controls.Add(new LiteralControl(sidebar.BuildAppEditor(Page)));
                    if (Request.QueryString["b"] == "app_created") {
                        if (!IsPostBack)
                            SetReturnMessage("App installed successfully!", false, 3000);
                    }
                    btn_viewCode.Visible = false;
                    pnl_htmleditor.Style["display"] = "none";
                    btn_clear_controls.Enabled = true;
                    btn_clear_controls.Visible = true;
                    btn_create.Enabled = false;
                    btn_create.Visible = false;
                    btn_create_easy.Enabled = true;
                    btn_create_easy.Visible = true;
                    pnl_apphtml.Enabled = true;
                    pnl_apphtml.Visible = true;
                    dd_filename_ext.Enabled = false;
                    dd_filename_ext.Visible = false;
                    lbl_dotHtml.Enabled = true;
                    lbl_dotHtml.Visible = true;
                    cb_wrapIntoIFrame.Enabled = false;
                    cb_wrapIntoIFrame.Visible = false;
                    if (!IsPostBack) {
                        newAppID = HelperMethods.RandomString(10);
                        tb_filename_create.Text = newAppID;
                    }
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#_hdl2').addClass('active');");
                }
                else if (Request.QueryString["c"] == "params") {
                    pnl_appList1.Controls.Add(new LiteralControl(sidebar.BuildAppEditor(Page)));
                    btn_viewCode.Visible = false;
                    pnl_app_params.Enabled = true;
                    pnl_app_params.Visible = true;
                    pnl_app_information.Enabled = false;
                    pnl_app_information.Visible = false;
                    pnl_htmleditor.Style["display"] = "none";
                    btn_clear_controls.Enabled = false;
                    btn_clear_controls.Visible = false;
                    btn_create.Enabled = false;
                    btn_create.Visible = false;
                    btn_create_easy.Enabled = false;
                    btn_create_easy.Visible = false;
                    pnl_appicon.Enabled = false;
                    pnl_appicon.Visible = false;
                    pnl_apphtml.Enabled = false;
                    pnl_apphtml.Visible = false;
                    hf_isParams.Value = "1";
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#_hdl4').addClass('active');");
                }
                else {
                    pnl_appList2.Controls.Add(new LiteralControl(sidebar.BuildAppEditor(Page)));
                    if (Request.QueryString["b"] == "app_created") {
                        if (!IsPostBack)
                            SetReturnMessage("App installed successfully!", false, 3000);
                    }
                    newAppID = HelperMethods.RandomString(10);
                    if (!IsPostBack) {
                        tb_filename_create.Text = newAppID;
                        btn_viewCode.Visible = true;
                    }
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#_hdl1').addClass('active');");
                }

                GetPostBack();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }
    private void GetPostBack() {
        string controlName = Request.Params["__EVENTTARGET"];
        switch (controlName) {
            case "btn_updateLoaderFileCancel":
                if (!string.IsNullOrEmpty(Request.QueryString["appId"])) {
                    string appId = "app-" + Request.QueryString["appId"];
                    MembershipUserCollection coll = Membership.GetAllUsers();
                    foreach (var member in from MembershipUser u in coll select new MemberDatabase(u.UserName)) {
                        member.RemoveEnabledApp(appId);
                    }

                    App tempApp = new App();
                    string createdBy = tempApp.GetAppCreatedBy(appId);
                    if (createdBy.ToLower() == _username.ToLower()) {
                        string name = _apps.GetAppName(appId);
                        string icon = _apps.GetAppIconName(appId);

                        if (!string.IsNullOrEmpty(icon)) {
                            icon = icon.ToLower();
                            if (icon == "database.png") {
                                var db = new DBImporter();
                                db.DeleteEntry(appId.Replace("app-", "").ToUpper());
                            }
                            else if (icon != "generic.png") {
                                if (File.Exists(ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + icon)) {
                                    try {
                                        File.Delete(ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + icon);
                                    }
                                    catch { }
                                }
                            }
                        }

                        var di = new DirectoryInfo(ServerSettings.GetServerMapLocation + "Apps");
                        for (int i = 0; i < di.GetDirectories().Length; i++) {
                            string directoryname = di.GetDirectories()[i].Name;
                            var di2 = new DirectoryInfo(ServerSettings.GetServerMapLocation + "Apps\\" + directoryname);
                            if (appId.Replace("app-", "") == directoryname) {
                                try {
                                    di2.Delete(true);
                                    break;
                                }
                                catch { }
                            }
                        }

                        _apps.DeleteAppMain(appId);
                        _apps.DeleteAppLocal(appId);

                        var wp = new AppPackages(true);
                        foreach (Dictionary<string, string> dr in wp.listdt) {
                            string[] l = wp.GetAppList(dr["ID"]);
                            string tempL = l.Where(w => w.ToLower() != appId.ToLower())
                                            .Aggregate(string.Empty, (current, w) => current + (w + ","));

                            wp.updateAppList(dr["ID"], tempL, _username);
                        }
                    }

                    string rq = "";
                    if (!string.IsNullOrEmpty(Request.QueryString["c"]))
                        rq = "&c=" + Request.QueryString["c"];

                    ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?date=" + DateTime.Now.Ticks + rq);
                }
                break;
        }
    }
    private void BuildLinks() {
        StringBuilder str = new StringBuilder();
        Panel pnl_extraitems = (Panel)Master.FindControl("pnl_extraitems");
        if (pnl_extraitems != null) {
            pnl_extraitems.Controls.Clear();

            str.Append("<ul class='homedashlinks float-right'>");

            SiteMapNodeCollection nodes = SiteMap.CurrentNode.ChildNodes;
            if (nodes.Count == 0) {
                nodes = SiteMap.CurrentNode.ParentNode.ChildNodes;
            }

            for (int i = 0; i < nodes.Count; i++) {
                string url = nodes[i].Url;
                url = nodes[i].Url.Substring(nodes[i].Url.LastIndexOf("/") + 1);
                if (HelperMethods.ConvertBitToBoolean(Request.QueryString["toolView"])) {
                    url += "&toolView=true";
                }
                str.Append("<li id='_hdl" + (i + 1).ToString() + "'><a href='" + url + "'>" + nodes[i].Title + "</a></li>");
            }

            str.Append("</ul>");

            pnl_extraitems.Controls.Add(new LiteralControl(str.ToString()));
        }
    }

    private void BuildCategories() {
        dd_category.Items.Clear();
        dd_category_edit.Items.Clear();
        foreach (Dictionary<string, string> dr in _category.category_dt) {
            var item = new ListItem("&nbsp;" + dr["Category"], dr["ID"]);
            if (!dd_category.Items.Contains(item)) {
                dd_category.Items.Add(item);
            }
            if (!dd_category_edit.Items.Contains(item)) {
                dd_category_edit.Items.Add(item);
            }
        }
    }

    private void BuildPackages() {
        var packages = new AppPackages(true);
        dd_package.Items.Clear();

        var item1 = new ListItem("--No Package--", "");
        dd_package.Items.Add(item1);

        int index = 1;
        foreach (Dictionary<string, string> dr in packages.listdt) {
            var item2 = new ListItem(dr["PackageName"], dr["ID"]);
            if (!dd_package.Items.Contains(item2)) {
                dd_package.Items.Add(item2);
                if (index == 1)
                    dd_package.SelectedIndex = index;

                index++;
            }
        }
    }

    private static string MakeValidFileName(string name) {
        name = name.ToLower();
        int index = name.IndexOf('.');
        if (index != -1) {
            name = name.Replace(name.Substring(index), "");
        }
        string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidReStr = string.Format(@"[{0}]+", invalidChars);
        return Regex.Replace(name, invalidReStr, "_").Replace(" ", "_").ToLower();
    }

    protected void btn_performCleanup_Click(object sender, EventArgs e) {
        int count = 0;
        var apps = new App();
        apps.GetAllApps();

        bool deleteFiles = HelperMethods.ConvertBitToBoolean(hf_performCleanup.Value);

        foreach (Apps_Coll dr in apps.AppList) {
            string filepath = ServerSettings.GetServerMapLocation + "Apps\\" + dr.filename.Replace("/", "\\");
            if (!File.Exists(filepath)) {
                apps.DeleteAppComplete(dr.AppId, ServerSettings.GetServerMapLocation, deleteFiles);
                apps.DeleteAppLocal(dr.AppId);
                count++;
            }
        }
        if (count > 0) {
            string rq = "";
            if (!string.IsNullOrEmpty(Request.QueryString["c"]))
                rq = "&c=" + Request.QueryString["c"];

            ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?date=" + DateTime.Now.Ticks + rq);
        }

        hf_performCleanup.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#ConfirmCleanup-element').remove();");
    }

    private void SetReturnMessage(string text, bool error, int delayTime) {
        string foreColor = "style='color:#32A800'";
        if (error)
            foreColor = "style='color:#FF0000'";

        StringBuilder str = new StringBuilder();
        str.Append("$('#lbl_ErrorUpload').html(\"<span " + foreColor + ">" + text + "</span>\");");
        str.Append("setTimeout(function() { $('#lbl_ErrorUpload').html(\"\"); }, " + delayTime + ");");
        RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
    }

    #endregion


    #region App Parameters

    private void GetAppParams(string appid) {
        var appparams = new AppParams(false);
        appparams.GetAllParameters_ForApp(appid);
        pnl_app_params_holder.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div id='css_sortable'>");
        str.Append("<div class='margin-top-sml'>");
        str.Append(
            "<table class='myHeaderStyle' style='width: 98%; min-width: 1100px;' cellpadding='0' cellspacing='0'>");
        str.Append(
            "<tr><td width='35px'></td><td>Parameter</td><td width='300px'>Description</td><td width='150px'>Updated By</td>");
        str.Append("<td width='165px'>Date/Time</td><td width='75px'>Actions</td></tr></table></div>");
        int count = 1;
        foreach (
            var dr in
                from Dictionary<string, string> dr in appparams.listdt
                let m = new MemberDatabase(dr["CreatedBy"])
                select dr) {
            str.Append(
                "<table class='myItemStyle GridNormalRow' style='width: 98%; min-width: 1100px;' cellpadding='0' cellspacing='0'><tr>");
            str.Append("<td width='35px' align='center' class='GridViewNumRow'>" + count + "</td>");
            str.Append("<td style='border-right: 1px solid #CCC; min-width: 255px;'><span>" + dr["Parameter"] +
                       "</span></td>");

            string desc = dr["Description"];
            if (string.IsNullOrEmpty(desc)) {
                desc = "No description available";
            }
            str.Append("<td width='350px' style='border-right: 1px solid #CCC;'>" + desc + "</td>");

            str.Append("<td width='150px' align='center' style='border-right: 1px solid #CCC;'>" + dr["UserName"] +
                       "</td>");
            str.Append("<td width='165px' align='center' style='border-right: 1px solid #CCC;'>" + dr["DateTime"] +
                       "</td>");

            var actionbtns = new StringBuilder();
            actionbtns.Append("<a href='#edit' class='td-edit-btn margin-right' onclick='EditParameter(\"" + dr["ID"] +
                              "\");return false;' title='Edit'></a>");
            actionbtns.Append("<a href='#delete' class='td-delete-btn' onclick='DeleteParameter(\"" + dr["ID"] +
                              "\");return false;' title='Delete'></a>");

            str.Append("<td width='75px' align='center' style='border-right: 1px solid #CCC;'>" + actionbtns +
                       "</td>");
            str.Append("</tr></table>");
            count++;
        }
        str.Append("</div>");
        if (appparams.listdt.Count == 0) {
            str.Append("No Parameters Available for App");
        }
        pnl_app_params_holder.Controls.Add(new LiteralControl(str.ToString()));
    }

    private void GetAppParams_Edit(string appid, string parameter) {
        var appparams = new AppParams(false);
        appparams.GetAllParameters_ForApp(appid);
        pnl_app_params_holder.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div id='css_sortable'>");
        str.Append("<div class='margin-top-sml'>");
        str.Append(
            "<table class='myHeaderStyle' style='width: 98%; min-width: 1100px;' cellpadding='0' cellspacing='0'>");
        str.Append(
            "<tr><td width='35px'></td><td>Parameter</td><td width='300px'>Description</td><td width='150px'>Updated By</td>");
        str.Append("<td width='165px'>Date/Time</td><td width='75px'>Actions</td></tr></table></div>");
        int count = 1;
        foreach (
            var dr in
                from Dictionary<string, string> dr in appparams.listdt
                let m = new MemberDatabase(dr["CreatedBy"])
                select dr) {
            str.Append(
                "<table class='myItemStyle GridNormalRow' style='width: 98%; min-width: 1100px;' cellpadding='0' cellspacing='0'><tr>");
            str.Append("<td width='35px' align='center' class='GridViewNumRow'>" + count + "</td>");
            if (dr["ID"] == parameter) {
                str.Append(
                    "<td style='border-right: 1px solid #CCC; min-width: 255px;'><input type='text' id='txt_appparam_edit' ");
                str.Append("class='TextBoxControls' value=\"" + dr["Parameter"] +
                           "\" style='width:95%'></input></td>");
            }
            else {
                str.Append("<td style='border-right: 1px solid #CCC;'><span>" + dr["Parameter"] + "</span></td>");
            }

            string desc = dr["Description"].ToString();
            if (string.IsNullOrEmpty(desc)) {
                desc = "No description available";
            }
            str.Append("<td width='350px' style='border-right: 1px solid #CCC;'>" + desc + "</td>");

            str.Append("<td width='150px' align='center' style='border-right: 1px solid #CCC;'>" + dr["UserName"] +
                       "</td>");
            str.Append("<td width='165px' align='center' style='border-right: 1px solid #CCC;'>" + dr["DateTime"] +
                       "</td>");

            var actionbtns = new StringBuilder();
            if (dr["ID"] == parameter) {
                actionbtns.Append(
                    "<a href='#update' class='td-update-btn margin-right' onclick='UpdateParameter();return false;' title='Update'></a>");
                actionbtns.Append(
                    "<a href='#cancel' class='td-cancel-btn' onclick='CancelParameterEdit();return false;' title='Cancel'></a>");
            }

            str.Append("<td width='75px' align='center' style='border-right: 1px solid #CCC;'>" + actionbtns +
                       "</td>");
            str.Append("</tr></table>");
            count++;
        }
        str.Append("</div>");
        if (appparams.listdt.Count == 0) {
            str.Append("No Parameters Available for App");
        }
        pnl_app_params_holder.Controls.Add(new LiteralControl(str.ToString()));
    }

    protected void hf_appchange_params_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;
        if (!string.IsNullOrEmpty(hf_appchange_params.Value)) {
            string appname = _apps.GetAppName(hf_appchange_params.Value);
            ltl_app_params.Text = "<h3 class='float-left'>App Parameters for <b>" + appname +
                                     "</b></h3><div class='clear-space'></div>";
            pnl_params_holder.Enabled = true;
            pnl_params_holder.Visible = true;
            lbl_params_tip.Enabled = false;
            lbl_params_tip.Visible = false;
            GetAppParams(hf_appchange_params.Value);
        }
    }

    protected void lbtn_close_params_Click(object sender, EventArgs e) {
        hf_appchange_params.Value = string.Empty;
        ltl_app_params.Text = string.Empty;
        pnl_params_holder.Enabled = false;
        pnl_params_holder.Visible = false;
        lbl_params_tip.Enabled = true;
        lbl_params_tip.Visible = true;

        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;
    }

    protected void btn_app_params_Click(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;

        if ((!string.IsNullOrEmpty(hf_appchange_params.Value)) && (!string.IsNullOrEmpty(txt_app_params.Text))
            && (txt_app_params.Text.ToLower() != "new app parameter")) {
            var appparams = new AppParams(false);
            appparams.addItem(hf_appchange_params.Value, txt_app_params.Text.Trim(),
                                 txt_app_params_description.Text.Trim(), _username);
            txt_app_params.Text = "New App Parameter";
            txt_app_params_description.Text = "Parameter Description";
        }
        else {
            lbl_param_error.Text = "Parameter cannot be blank";
            lbl_param_error.Enabled = true;
            lbl_param_error.Visible = true;
        }

        GetAppParams(hf_appchange_params.Value);
    }

    protected void hf_appchange_params_edit_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;
        if (!string.IsNullOrEmpty(hf_appchange_params_edit.Value)) {
            GetAppParams_Edit(hf_appchange_params.Value, hf_appchange_params_edit.Value);
        }
    }

    protected void hf_appchange_params_delete_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;

        if (!string.IsNullOrEmpty(hf_appchange_params_delete.Value)) {
            var appparams = new AppParams(false);
            appparams.deleteParameter(hf_appchange_params_delete.Value);
            GetAppParams(hf_appchange_params.Value);
        }
        hf_appchange_params_edit.Value = "";
        hf_appchange_params_delete.Value = "";
    }

    protected void hf_appchange_params_update_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;

        if ((!string.IsNullOrEmpty(hf_appchange_params_update.Value)) &&
            (!string.IsNullOrEmpty(hf_appchange_params_edit.Value))) {
            var appparams = new AppParams(false);
            appparams.updateParameter(hf_appchange_params_edit.Value, hf_appchange_params.Value,
                                         hf_appchange_params_update.Value, _username);
            GetAppParams(hf_appchange_params.Value);
        }
        hf_appchange_params_edit.Value = "";
        hf_appchange_params_update.Value = "";
    }

    protected void hf_appchange_params_cancel_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;

        if (!string.IsNullOrEmpty(hf_appchange_params_cancel.Value)) {
            GetAppParams(hf_appchange_params.Value);
        }
        hf_appchange_params_edit.Value = "";
        hf_appchange_params_cancel.Value = "";
    }

    #endregion


    #region App Overlay Updater

    private void GetOverlayList() {
        if (ServerSettings.AdminPagesCheck("overlays.aspx", _username)) {
            cb_associatedOverlay.Items.Clear();
            _workspaceOverlays.GetWorkspaceOverlays();
            foreach (WorkspaceOverlay_Coll doc in _workspaceOverlays.OverlayList) {
                if ((Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
                    || ((doc.UserName.ToLower() == _username.ToLower()) && (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)))) {
                    ListItem item = new ListItem("&nbsp;" + doc.OverlayName + " - " + doc.Description, doc.ID);
                    if (!cb_associatedOverlay.Items.Contains(item))
                        cb_associatedOverlay.Items.Add(item);
                }
            }
        }
        else {
            pnl_edit_AssociatedOverlay.Enabled = false;
            pnl_edit_AssociatedOverlay.Visible = false;
        }
    }

    private void BuildOverlayDropDown() {
        if (ServerSettings.AdminPagesCheck("overlays.aspx", _username)) {
            cc_associatedOverlayNew.Items.Clear();
            _workspaceOverlays.GetWorkspaceOverlays();
            foreach (WorkspaceOverlay_Coll doc in _workspaceOverlays.OverlayList) {
                if ((Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
                    || ((doc.UserName.ToLower() == _username.ToLower()) && (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)))) {
                        ListItem item = new ListItem("&nbsp;" + doc.OverlayName + " - " + doc.Description, doc.ID);
                    if (!cc_associatedOverlayNew.Items.Contains(item))
                        cc_associatedOverlayNew.Items.Add(item);
                }
            }
        }
        else {
            pnl_new_AssociatedOverlay.Enabled = false;
            pnl_new_AssociatedOverlay.Visible = false;
        }
    }

    private void UpdateAssociatedOverlay() {
        string name = "";
        foreach (ListItem item in cb_associatedOverlay.Items) {
            if (item.Selected) {
                name += item.Value + ";";
            }
        }

        string[] names = name.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        string appId = hf_appchange.Value;
        if ((!string.IsNullOrEmpty(name)) && (!string.IsNullOrEmpty(appId))) {
            _apps.UpdateAppOverlayID(appId, name);
        }
        else {
            _apps.UpdateAppOverlayID(appId, "");
        }
        LoadAppDownloadBtn();
    }

    #endregion


    #region App Loader File Updater

    private void GetLoaderFileList(string filePath, string rootDir) {
        DirectoryInfo di = new DirectoryInfo(filePath);
        for (int i = 0; i < di.GetDirectories().Length; i++) {
            GetLoaderFileList(di.GetDirectories()[i].FullName, rootDir + "/" + di.GetDirectories()[i].Name);
        }
        for (int i = 0; i < di.GetFiles().Length; i++) {
            FileInfo fi = new FileInfo(di.GetFiles()[i].FullName);
            if ((fi.Extension.ToLower() == ".html") || (fi.Extension.ToLower() == ".htm") || (fi.Extension.ToLower() == ".ascx")
                || (fi.Extension.ToLower() == ".aspx")) {
                ListItem item = new ListItem("&nbsp;" + rootDir + "/" + di.GetFiles()[i].Name, rootDir + "/" + di.GetFiles()[i].Name);
                if (!radioButton_FileList.Items.Contains(item))
                    radioButton_FileList.Items.Add(item);
            }
        }
    }

    private void GetLoaderFileListNew(string filePath, string rootDir) {
        DirectoryInfo di = new DirectoryInfo(filePath);
        for (int i = 0; i < di.GetDirectories().Length; i++) {
            GetLoaderFileListNew(di.GetDirectories()[i].FullName, rootDir + "/" + di.GetDirectories()[i].Name);
        }
        for (int i = 0; i < di.GetFiles().Length; i++) {
            FileInfo fi = new FileInfo(di.GetFiles()[i].FullName);
            if ((fi.Extension.ToLower() == ".html") || (fi.Extension.ToLower() == ".htm") || (fi.Extension.ToLower() == ".ascx")
                || (fi.Extension.ToLower() == ".aspx")) {
                ListItem item = new ListItem("&nbsp;" + rootDir + "/" + di.GetFiles()[i].Name, rootDir + "/" + di.GetFiles()[i].Name);
                if (!radioButton_FileList_New.Items.Contains(item))
                    radioButton_FileList_New.Items.Add(item);
            }
        }
    }

    protected void btn_updateLoaderFile_Clicked(object sender, EventArgs e) {
        string name = radioButton_FileList.SelectedValue;
        string appId = hf_appchange.Value;
        if ((!string.IsNullOrEmpty(name)) && (!string.IsNullOrEmpty(appId))) {
            _apps.UpdateAppFilename(appId, name);
            tb_filename_edit.Text = name;
        }
        LoadAppDownloadBtn();
    }

    protected void btn_updateLoaderFileNew_Clicked(object sender, EventArgs e) {
        if (radioButton_FileList_New.SelectedIndex != -1) {
            if (!string.IsNullOrEmpty(Request.QueryString["appId"])) {
                string appId = "app-" + Request.QueryString["appId"];
                string name = radioButton_FileList_New.SelectedValue;
                if (!string.IsNullOrEmpty(name))
                    _apps.UpdateAppFilename(appId, name);

                string rq = "";
                if (!string.IsNullOrEmpty(Request.QueryString["c"]))
                    rq = "&c=" + Request.QueryString["c"];

                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?date=" + DateTime.Now.Ticks + rq);
            }
        }
    }

    #endregion


    #region App Box/Edit

    protected void hf_appchange_ValueChanged(object sender, EventArgs e) {
        if (hf_appchange.Value == "reset") {
            RegisterPostbackScripts.RegisterStartupScript(this, "appchange('reset');openWSE.LoadingMessage1('Loading. Please Wait...');");
        }
        else {
            btn_delete.Enabled = false;
            btn_delete.Visible = false;
            btn_edit.Enabled = false;
            btn_edit.Visible = false;
            lb_editsource.Enabled = false;
            lb_editsource.Visible = false;
            btn_save.Enabled = false;
            btn_save.Visible = false;
            btn_save_2.Enabled = false;
            btn_save_2.Visible = false;
            btn_cancel.Enabled = false;
            btn_cancel.Visible = false;

            wlmd_holder.Style["display"] = "block";

            wlmd_holder.Controls.Clear();
            Appchange();
            RegisterPostbackScripts.RegisterStartupScript(this, "AdjustEdit();openWSE.LoadModalWindow(true, 'App-element', 'App Details');openWSE.LoadingMessage1('Loading. Please Wait...');");
        }
    }

    private void Appchange() {
        if ((!string.IsNullOrEmpty(tb_title_edit.Text)) &&
            (tb_title_edit.Text != _apps.GetAppName(hf_appchange.Value))) return;
        if (!string.IsNullOrEmpty(hf_appchange.Value)) {
            Apps_Coll db = _apps.GetAppInformation(hf_appchange.Value);
            if (db != null) {
                _apps.BuildAboutApp(wlmd_holder, hf_appchange.Value, _username);

                if (_ss.AllowAppRating) {
                    AppRatings ratings = new AppRatings();
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.RatingStyleInit('.app-rater', '" + ratings.GetAverageRating(hf_appchange.Value) + "', true, '" + hf_appchange.Value + "', true);");
                }

                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    if (db.CreatedBy.ToLower() == _username.ToLower()) {
                        Appedit();

                        if (new FileInfo(db.filename).Extension.ToLower() == ".ascx") {
                            if (!_ss.LockASCXEdit) {
                                lb_editsource.Enabled = true;
                                lb_editsource.Visible = true;
                                btn_delete.Enabled = true;
                                btn_delete.Visible = true;
                            }
                            else {
                                lb_editsource.Enabled = false;
                                lb_editsource.Visible = false;
                                btn_delete.Enabled = false;
                                btn_delete.Visible = false;
                            }
                        }
                        else {
                            lb_editsource.Enabled = true;
                            lb_editsource.Visible = true;
                            btn_delete.Enabled = true;
                            btn_delete.Visible = true;
                        }

                        btn_edit.Enabled = true;
                        btn_edit.Visible = true;
                    }
                    else {
                        btn_delete.Enabled = false;
                        btn_delete.Visible = false;
                        btn_edit.Enabled = false;
                        btn_edit.Visible = false;
                        lb_editsource.Enabled = false;
                        lb_editsource.Visible = false;
                    }
                }
                else {
                    Appedit();
                    if (new FileInfo(db.filename).Extension.ToLower() == ".ascx") {
                        if ((_username.ToLower() == ServerSettings.AdminUserName.ToLower()) || (!_ss.LockASCXEdit)) {
                            lb_editsource.Enabled = true;
                            lb_editsource.Visible = true;
                            btn_delete.Enabled = true;
                            btn_delete.Visible = true;
                        }
                        else {
                            lb_editsource.Enabled = false;
                            lb_editsource.Visible = false;
                            btn_delete.Enabled = false;
                            btn_delete.Visible = false;
                        }
                    }
                    else {
                        lb_editsource.Enabled = true;
                        lb_editsource.Visible = true;
                        btn_delete.Enabled = true;
                        btn_delete.Visible = true;
                    }


                    btn_edit.Enabled = true;
                    btn_edit.Visible = true;
                }

                SetStandardApps(db.AppId);
            }
        }
    }

    private void Appedit() {
        if (string.IsNullOrEmpty(hf_appchange.Value)) return;
        Apps_Coll db = _apps.GetAppInformation(hf_appchange.Value);
        if (db == null) return;
        lb_editsource.Enabled = false;
        lb_editsource.Visible = false;
        LoadAppDownloadBtn();
        img_edit.ImageUrl = ServerSettings.GetSitePath(Request) + "/Standard_Images/App_Icons/" + db.Icon;

        if (_ss.HideAllAppIcons)
            img_edit.Visible = false;
        else
            img_edit.Visible = true;

        tb_title_edit.Text = db.AppName;

        string about = db.About;
        if (string.IsNullOrEmpty(about))
            tb_about_edit.Text = OpenWSE.Core.Licensing.CheckLicense.SiteName + " | " + DateTime.Now.Year + " | " + "John Donnelly";
        else
            tb_about_edit.Text = about;

        string description = db.Description;
        tb_description_edit.Text = string.IsNullOrEmpty(description) ? "No description available" : description;

        tb_filename_edit.Text = db.filename;

        List<string> categoryList = db.Category.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (ListItem item in dd_category_edit.Items) {
            if (categoryList.Contains(item.Value))
                item.Selected = true;
        }

        dd_allowresize_edit.SelectedIndex = db.AllowResize ? 0 : 1;
        dd_allowmax_edit.SelectedIndex = db.AllowMaximize ? 0 : 1;

        dd_allow_params_edit.SelectedIndex = db.AllowParams ? 0 : 1;
        dd_allowpopout_edit.SelectedIndex = db.AllowPopOut ? 0 : 1;

        dd_enablebg_edit.SelectedIndex = db.CssClass.ToLower() == "app-main" ? 0 : 1;

        dd_maxonload_edit.SelectedIndex = db.AutoFullScreen ? 0 : 1;
        dd_autoOpen_edit.SelectedIndex = db.AutoOpen ? 0 : 1;

        if (db.CreatedBy.ToLower() == _username.ToLower() || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            dd_isPrivate_Edit.SelectedIndex = db.IsPrivate ? 0 : 1;
            div_isprivate_edit.Visible = true;
        }
        else {
            dd_isPrivate_Edit.SelectedIndex = 1;
            div_isprivate_edit.Visible = false;
        }

        switch (db.DefaultWorkspace) {
            case "1":
                dd_defaultworkspace_edit.SelectedIndex = 0;
                break;
            case "2":
                dd_defaultworkspace_edit.SelectedIndex = 1;
                break;
            case "3":
                dd_defaultworkspace_edit.SelectedIndex = 2;
                break;
            case "4":
                dd_defaultworkspace_edit.SelectedIndex = 3;
                break;
            default:
                dd_defaultworkspace_edit.SelectedIndex = 0;
                break;
        }

        tb_minheight_edit.Text = db.MinHeight;
        tb_minwidth_edit.Text = db.MinWidth;
        tb_allowpopout_edit.Text = db.PopOutLoc;

        hf_AppOverlay.Value = string.Empty;
        if (!string.IsNullOrEmpty(db.OverlayID)) {
            string[] oIds = db.OverlayID.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < oIds.Length; i++) {
                WorkspaceOverlay_Coll doc = _workspaceOverlays.GetWorkspaceOverlay(oIds[i]);
                if (!string.IsNullOrEmpty(doc.OverlayName)) {
                    hf_AppOverlay.Value += doc.ID;
                    if (i < oIds.Length - 1) {
                        hf_AppOverlay.Value += ServerSettings.StringDelimiter;
                    }
                }
            }
        }

        if (new FileInfo(db.filename).Extension.ToLower() == ".ascx") {
            pnl_autocreate_edit.Enabled = true;
            pnl_autocreate_edit.Visible = true;

            dd_AutoLoad_edit.SelectedIndex = db.AutoLoad ? 0 : 1;
        }
        else {
            pnl_autocreate_edit.Enabled = false;
            pnl_autocreate_edit.Visible = false;
        }
    }

    protected void btn_edit_Click(object sender, EventArgs e) {
        btn_delete.Enabled = false;
        btn_delete.Visible = false;
        btn_edit.Enabled = false;
        btn_edit.Visible = false;
        btn_save.Enabled = true;
        btn_save.Visible = true;
        btn_save_2.Enabled = true;
        btn_save_2.Visible = true;
        btn_cancel.Enabled = true;
        btn_cancel.Visible = true;

        wlmd_holder.Style["display"] = "none";

        GetOverlayList();
        string[] oIds = hf_AppOverlay.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < cb_associatedOverlay.Items.Count; i++) {
            foreach (string oId in oIds) {
                if (cb_associatedOverlay.Items[i].Value == oId) {
                    cb_associatedOverlay.Items[i].Selected = true;
                }
            }
        }

        radioButton_FileList.Items.Clear();
        string filePath = ServerSettings.GetServerMapLocation + "Apps\\" + tb_filename_edit.Text.Replace("/", "\\");
        FileInfo tempFi = new FileInfo(filePath);
        if ((tempFi.Directory.Name.ToLower() != "apps") && (tb_filename_edit.Text.Contains('/'))) {
            changeLoadFile.Visible = true;
            tb_filename_edit.Visible = false;
            DirectoryInfo di = new DirectoryInfo(filePath);
            GetLoaderFileList(di.Parent.FullName, di.Parent.Name);

            for (int i = 0; i < radioButton_FileList.Items.Count; i++) {
                if (radioButton_FileList.Items[i].Value == tb_filename_edit.Text) {
                    radioButton_FileList.SelectedIndex = i;
                    break;
                }
            }
        }
        else {
            changeLoadFile.Visible = false;
            tb_filename_edit.Visible = true;
        }

        LoadAppDownloadBtn();
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#wlmd_editor_holder').css('display', 'block');AdjustEdit();");
    }

    protected void btn_save_Click(object sender, EventArgs e) {
        Apps_Coll dt = _apps.GetAppInformation(hf_appchange.Value);
        if (fu_image_edit.HasFile) {
            string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + fu_image_edit.FileName;
            _imageConverter.SaveNewImg(fu_image_edit.PostedFile, filePath);
            _apps.UpdateAppImage(dt.AppId, fu_image_edit.FileName);
        }
        else if (!string.IsNullOrEmpty(tb_imageurl_edit.Text)) {
            try {
                string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + MakeValidFileName(tb_title_edit.Text) + ".png";
                _imageConverter.SaveNewImg(tb_imageurl_edit.Text, filePath);
                _apps.UpdateAppImage(dt.AppId, MakeValidFileName(tb_title_edit.Text) + ".png");
            }
            catch { }
        }

        string title = tb_title_edit.Text;
        if (string.IsNullOrEmpty(title)) {
            title = dt.Icon;
        }

        UpdateAssociatedOverlay();

        string ar = "false";
        string am = "false";
        if (HelperMethods.ConvertBitToBoolean(dd_allowresize_edit.SelectedValue)) {
            ar = "true";
        }
        if (HelperMethods.ConvertBitToBoolean(dd_allowmax_edit.SelectedValue)) {
            am = "true";
        }
        _apps.UpdateAutoLoad(dt.AppId, HelperMethods.ConvertBitToBoolean(dd_AutoLoad_edit.SelectedValue));

        _apps.UpdateAutoFullScreen(dt.AppId, HelperMethods.ConvertBitToBoolean(dd_maxonload_edit.SelectedValue));
        _apps.UpdateAllowParams(dt.AppId, HelperMethods.ConvertBitToBoolean(dd_allow_params_edit.SelectedValue));

        string categorySelectedVals = string.Empty;
        foreach (ListItem checkedItem in dd_category_edit.Items) {
            if (checkedItem.Selected) {
                categorySelectedVals += checkedItem.Value + ServerSettings.StringDelimiter;
            }
        }

        _apps.UpdateCategory(dt.AppId, categorySelectedVals);
        _apps.UpdateAppLocal(dt.AppId, title);
        _apps.UpdateAppList(dt.AppId, title, ar, am, tb_about_edit.Text,
                                  tb_description_edit.Text, dd_enablebg_edit.SelectedValue, tb_minheight_edit.Text.Replace("px", ""),
                                  tb_minwidth_edit.Text.Replace("px", ""), HelperMethods.ConvertBitToBoolean(dd_allowpopout_edit.SelectedValue), tb_allowpopout_edit.Text.Trim(),
                                  HelperMethods.ConvertBitToBoolean(dd_autoOpen_edit.SelectedValue), dd_defaultworkspace_edit.SelectedValue, HelperMethods.ConvertBitToBoolean(dd_isPrivate_Edit.SelectedValue));

        if (File.Exists(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\" + dt.AppId.Replace("app-", "") + ".ascx")) {
            var db = new DBImporter();
            db.BinaryDeserialize();
            foreach (DBImporter_Coll dbColl in db.DBColl) {
                if (dbColl.ID == dt.AppId.Replace("app-", "")) {
                    db.UpdateEntry(dbColl.ID, title, dbColl.SelectCommand, dbColl.AllowEdit, dbColl.Chart_Type, dbColl.ChartTitle, dbColl.NotifyUsers);
                    break;
                }
            }
        }

        if (fu_image_edit.HasFile) {
            string rq = !string.IsNullOrEmpty(Request.QueryString["c"])
                            ? "?c=" + Request.QueryString["c"] + "&date=" + DateTime.Now.Ticks
                            : "?date=" + DateTime.Now.Ticks;

            ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx" + rq);
        }
        else {
            btn_delete.Enabled = true;
            btn_delete.Visible = true;
            btn_edit.Enabled = true;
            btn_edit.Visible = true;
            btn_save.Enabled = false;
            btn_save.Visible = false;
            btn_save_2.Enabled = false;
            btn_save_2.Visible = false;
            btn_cancel.Enabled = false;
            btn_cancel.Visible = false;

            wlmd_holder.Style["display"] = "block";

            tb_title_edit.Text = string.Empty;
            htmlEditor.Text = string.Empty;
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#wlmd_editor_holder').css('display', 'none');");
            Appchange();

            //using (new SideBarItems(_username, Page, true))
            //{
            //    if (Master == null) return;
            //    var pnl = (UpdatePanel)Master.FindControl("updatepnl_ect_holder");
            //    if (pnl != null)
            //        pnl.Update();
            //}
        }
    }

    protected void btn_cancel_Click(object sender, EventArgs e) {
        btn_delete.Enabled = true;
        btn_delete.Visible = true;
        btn_edit.Enabled = true;
        btn_edit.Visible = true;
        btn_save.Enabled = false;
        btn_save.Visible = false;
        btn_save_2.Enabled = false;
        btn_save_2.Visible = false;
        btn_cancel.Enabled = false;
        btn_cancel.Visible = false;

        wlmd_holder.Style["display"] = "block";

        tb_title_edit.Text = string.Empty;
        htmlEditor.Text = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#wlmd_editor_holder').css('display', 'none');");
        Appchange();
    }

    private void LoadAppDownloadBtn() {
        string appName_dl = "$('#iframe-appDownloader').attr('src', '../iframes/AppDownloadBtn.aspx?id=" + hf_appchange.Value + "');";
        RegisterPostbackScripts.RegisterStartupScript(this, appName_dl);
    }

    private void SetStandardApps(string appId) {
        if ((appId == "app-appinstaller") || (appId == "app-rssfeed") || (appId == "app-documents") ||
            (appId == "app-notepad") || (appId == "app-bookmarkviewer") || (appId == "app-googletraffic") ||
            (appId == "app-chatsettings") || (appId == "app-messageboard") || (appId == "app-twitter") ||
            (appId == "app-personalcalendar") || (appId == "app-alarmclock") || (appId == "app-feedback")) {
            btn_delete.Enabled = false;
            btn_delete.Visible = false;
            lb_editsource.Enabled = false;
            lb_editsource.Visible = false;
        }
    }
    #endregion


    #region App Source Edit

    protected void lb_editsource_Click(object sender, EventArgs e) {
        Edit_Controls.Enabled = true;
        Edit_Controls.Visible = true;
        string htmlMarkup = string.Empty;
        string JavaScript = string.Empty;
        if (!string.IsNullOrEmpty(tb_filename_edit.Text)) {
            string additionalCode = string.Empty;
            string htmlcode = string.Empty;
            string scriptcode = string.Empty;
            if (File.Exists(ServerSettings.GetServerMapLocation + "Apps\\" + tb_filename_edit.Text)) {
                string srcode;
                using (var sr = new StreamReader(ServerSettings.GetServerMapLocation + "Apps\\" + tb_filename_edit.Text)) {
                    srcode = sr.ReadToEnd();
                }

                FileInfo fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps\\" + tb_filename_edit.Text);

                if ((!_ss.LockFileManager) || (_username.ToLower() == ServerSettings.AdminUserName.ToLower())) {
                    StringBuilder extLinks = new StringBuilder();
                    string dir = fi.Directory.Name;
                    if (dir != "Apps") {
                        string[] filesInDir = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Apps\\" + dir);
                        foreach (string fn in filesInDir) {
                            FileInfo fi_indir = new FileInfo(fn);
                            if ((fi_indir.Extension.ToLower() == ".cs") || (fi_indir.Extension.ToLower() == ".js")) {
                                string parentFolder = fi_indir.FullName.Replace(fi_indir.Directory.Root.Name, "\\");
                                string parentFolder2 = parentFolder.Replace("\\", "!");
                                string editable = "false";
                                if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                                    editable = "true";
                                }
                                extLinks.Append("<a href='" + ResolveUrl("~/SiteTools/ServerMaintenance/FileManager.aspx?edit=" + editable + "&file=" + parentFolder2) + "' ");
                                extLinks.Append("class='sb-links margin-left-big margin-right margin-top-sml float-left' target='_blank'>View " + fi_indir.Name + "</a>");
                            }
                        }

                        if (!string.IsNullOrEmpty(extLinks.ToString()))
                            links_externalCode.Text = extLinks.ToString();
                    }
                }

                if ((fi.Extension.ToLower() == ".ascx") || (fi.Extension.ToLower() == ".aspx")) {
                    if ((_username.ToLower() == ServerSettings.AdminUserName.ToLower()) || (!_ss.LockASCXEdit))
                        scriptcode = srcode;
                    else
                        scriptcode = ".ascx file extensions are locked. Contact your administrator for access.";
                    additionalCode = "$('#HTMLCODE').hide();$('#javascriptcode_Title').html('Source File Editor');";
                }
                else {
                    if (srcode.IndexOf("<script type='text/javascript'>", StringComparison.Ordinal) != -1) {
                        scriptcode = BuildScriptCode(srcode, "<script type='text/javascript'>");
                        if (srcode.IndexOf("<body>", StringComparison.Ordinal) != -1)
                            htmlcode = BuildHtmlCode(srcode, "<script type='text/javascript'>");
                    }
                    else if (srcode.IndexOf("<script type=\"text/javascript\">", StringComparison.Ordinal) != -1) {
                        scriptcode = BuildScriptCode(srcode, "<script type=\"text/javascript\">");
                        if (srcode.IndexOf("<body>", StringComparison.Ordinal) != -1)
                            htmlcode = BuildHtmlCode(srcode, "<script type=\"text/javascript\">");
                    }
                    else if (srcode.IndexOf("<script>", StringComparison.Ordinal) != -1) {
                        scriptcode = BuildScriptCode(srcode, "<script>");
                        if (srcode.IndexOf("<body>", StringComparison.Ordinal) != -1)
                            htmlcode = BuildHtmlCode(srcode, "<script>");
                    }
                    else {
                        if (srcode.IndexOf("<body>", StringComparison.Ordinal) != -1) {
                            htmlcode = srcode.Substring(srcode.IndexOf("<body>", StringComparison.Ordinal));
                            htmlcode = htmlcode.Replace("</body>", "");
                            htmlcode = htmlcode.Replace("</html>", "");
                        }
                    }
                }

                htmlMarkup = "UnescapeCode(" + HttpUtility.JavaScriptStringEncode(htmlcode, true) + ");";
                JavaScript = "UnescapeJavascriptCode(" + HttpUtility.JavaScriptStringEncode(scriptcode, true) + ");" + additionalCode;
            }
        }
        lbl_currfile.Text = tb_filename_edit.Text;
        pnl_htmleditor.Style["display"] = "block";
        btn_backProp.Visible = false;
        string javascriptCode = "$('#App-element,#create').fadeOut(300);_editmode=1;openWSE.LoadingMessage1('Loading Controls. Please Wait...');" + htmlMarkup + JavaScript;
        RegisterPostbackScripts.RegisterStartupScript(this, javascriptCode);
    }

    private static string BuildScriptCode(string htmlcode, string index) {
        string scriptcode = htmlcode.Substring(htmlcode.IndexOf(index, StringComparison.Ordinal) + (index).Length);
        int lengthof = scriptcode.LastIndexOf("</script>", StringComparison.Ordinal);
        scriptcode = scriptcode.Substring(0, lengthof);
        return scriptcode;
    }

    private static string BuildHtmlCode(string srcode, string index) {
        string htmlcode = srcode.Substring(srcode.IndexOf("<body>", StringComparison.Ordinal) + ("<body>").Length);
        string scriptcode = htmlcode.Substring(htmlcode.IndexOf(index, StringComparison.Ordinal));
        htmlcode = htmlcode.Replace(scriptcode, "");

        return htmlcode;
    }

    protected void lbtn_close_Click(object sender, EventArgs e) {
        Edit_Controls.Enabled = false;
        Edit_Controls.Visible = false;
        btn_edit.Enabled = false;
        btn_edit.Visible = false;
        lb_editsource.Enabled = false;
        lb_editsource.Visible = false;
        htmlEditor.Text = string.Empty;
        links_externalCode.Text = string.Empty;
        var tinyscript = new StringBuilder();
        Appchange();
        pnl_htmleditor.Style["display"] = "none";
        btn_backProp.Visible = true;
        if (btn_save.Visible && ((btn_save.Enabled) || (btn_save_2.Enabled))) {
            btn_edit.Enabled = false;
            btn_edit.Visible = false;
        }
        tinyscript.AppendLine("$('#App-element').css('display', 'block');$('#create').fadeIn(300);_editmode=2;RevertToProperties();");
        tinyscript.AppendLine("if ($('#wlmd_editor_holder').css('display') == 'block'){ $('#MainContent_wlmd_holder').css('display', 'none'); $('#MainContent_btn_delete').css('display', 'none'); }");
        tinyscript.AppendLine("$('#HTMLCODE').show();$('#javascriptcode_Title').html('Javascript File Editor');");
        RegisterPostbackScripts.RegisterStartupScript(this, tinyscript.ToString());
    }

    protected void hf_saveapp_Changed(object sender, EventArgs e) {
        var html = new StringBuilder();
        var fi = new FileInfo(tb_filename_edit.Text);
        bool cansave = false;
        string htmlText = HttpUtility.UrlDecode(hf_saveapp.Value.Trim());
        string editor = HttpUtility.UrlDecode(hidden_editor.Value.Trim());

        // Need to unescape the code one more time due to security reasons.
        htmlText = HttpUtility.UrlDecode(htmlText);
        editor = HttpUtility.UrlDecode(editor);

        if ((fi.Extension.ToLower() == ".html") || (fi.Extension.ToLower() == ".htm")) {
            html.AppendLine(
                "<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");
            html.AppendLine("<html xmlns='http://www.w3.org/1999/xhtml'>");
            html.AppendLine("<head>");
            html.AppendLine("<title>" + tb_title_edit.Text + "</title>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine(htmlText);
            html.AppendLine("<script type='text/javascript'>" + editor + "</script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            cansave = true;
        }
        else if (fi.Extension.ToLower() == ".ascx") {
            if ((_username.ToLower() == ServerSettings.AdminUserName.ToLower()) || (!_ss.LockASCXEdit)) {
                html.Append(editor);
                cansave = true;
            }
        }

        if (cansave) {
            string filename = tb_filename_edit.Text;
            if (filename.Contains("/")) {
                filename = filename.Replace("/", "\\");
            }
            File.WriteAllText(ServerSettings.GetServerMapLocation + "Apps\\" + filename, html.ToString());
            Edit_Controls.Enabled = false;
            Edit_Controls.Visible = false;
            htmlEditor.Text = string.Empty;
            hidden_editor.Value = string.Empty;

            string extraWork = "";
            if (dd_filename_ext.SelectedValue == ".html")
                extraWork = "document.getElementById(\"hidden_temp_script\").value = \"$(document).ready(function () {  });\";";

            hf_saveapp.Value = string.Empty;
            links_externalCode.Text = string.Empty;
            var tinyscript = new StringBuilder();
            Appchange();
            btn_backProp.Visible = true;
            if (btn_save.Visible && ((btn_save.Enabled) || (btn_save_2.Enabled))) {
                btn_edit.Enabled = false;
                btn_edit.Visible = false;
            }
            tinyscript.AppendLine("$('#App-element').css('display', 'block');$('#create').fadeIn(300);_editmode=2;RevertToProperties();");
            tinyscript.AppendLine("if ($('#wlmd_editor_holder').css('display') == 'block'){ $('#MainContent_wlmd_holder').css('display', 'none'); $('#MainContent_btn_delete').css('display', 'none'); }");
            tinyscript.AppendLine("$('#HTMLCODE').show();$('#javascriptcode_Title').html('Javascript File Editor');" + extraWork);
            RegisterPostbackScripts.RegisterStartupScript(this, tinyscript.ToString());
        }
    }

    #endregion


    #region Create Buttons

    protected void btn_uploadnew_Click(object sender, EventArgs e) {
        bool cancontinue = false;
        if ((fu_uploadnew.HasFile) && (!string.IsNullOrEmpty(tb_appname.Text)) &&
            (!_apps.IconExists(tb_appname.Text))) {
            var fi = new FileInfo(fu_uploadnew.FileName);
            if (!AppExists(tb_appname.Text.Trim(), "app-" + MakeValidFileName(fi.Name))) {
                #region Upload/Create Icon
                string picname = "generic.png";
                if (fu_image_create.HasFile) {
                    var fiImage = new FileInfo(fu_image_create.FileName);
                    if ((fiImage.Extension.ToLower() == ".png") || (fiImage.Extension.ToLower() == ".gif")
                        || (fiImage.Extension.ToLower() == ".jpg") || (fiImage.Extension.ToLower() == ".jpeg")) {
                        picname = fu_image_create.FileName;
                        string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + fu_image_create.FileName;
                        cancontinue = _imageConverter.SaveNewImg(fu_image_create.PostedFile, filePath);
                    }
                    else
                        SetReturnMessage("Error! The file extension " + fiImage.Extension + " is not allowed. Please visit the help page for more information.", true, 3000);
                }
                else if (!string.IsNullOrEmpty(tb_imageurl.Text)) {
                    try {
                        string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + MakeValidFileName(tb_appname.Text) + ".png";
                        cancontinue = _imageConverter.SaveNewImg(tb_imageurl.Text, filePath);
                        picname = MakeValidFileName(tb_appname.Text) + ".png";
                    }
                    catch {
                        cancontinue = false;
                    }
                }
                else
                    cancontinue = true;
                #endregion

                if (cancontinue) {
                    string randomString = HelperMethods.RandomString(10);
                    if (fi.Extension.ToLower() == ".zip") {
                        if (HasAtLeastOneValidPage(fu_uploadnew.FileContent)) {
                            fu_uploadnew.FileContent.Position = 0;
                            string dir = MakeValidFileName(fi.Name.Replace(fi.Extension, ""));
                            string fileName = randomString;

                            using (Stream fileStreamIn = fu_uploadnew.FileContent) {
                                using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn)) {
                                    ZipEntry entry;
                                    string tmpEntry = String.Empty;
                                    List<string> dlls = new List<string>();
                                    while ((entry = zipInStream.GetNextEntry()) != null) {
                                        string fn = Path.GetFileName(entry.Name);

                                        string filePath = ServerSettings.GetServerMapLocation + "Apps\\" + fileName + "\\";
                                        if (!Directory.Exists(filePath)) {
                                            Directory.CreateDirectory(filePath);
                                        }

                                        if (string.IsNullOrEmpty(fn)) {
                                            continue;
                                        }

                                        var tempfi = new FileInfo(fn);
                                        if ((tempfi.Extension.ToLower() != ".exe") && (tempfi.Extension.ToLower() != ".com") && (tempfi.Extension.ToLower() != ".pif")
                                             && (tempfi.Extension.ToLower() != ".bat") && (tempfi.Extension.ToLower() != ".scr")) {

                                            if ((fn != String.Empty) && (entry.Name.IndexOf(".ini") < 0)) {
                                                string en = entry.Name;

                                                if (tempfi.Extension.ToLower() == ".pdb") {
                                                    continue;
                                                }

                                                if ((tempfi.Extension.ToLower() == ".dll") || (tempfi.Extension.ToLower() == ".compiled")) {
                                                    filePath = ServerSettings.GetServerMapLocation + "Bin\\" + fileName + "\\";

                                                    if (!Directory.Exists(filePath)) {
                                                        Directory.CreateDirectory(filePath);
                                                        ServerSettings.AddRuntimeAssemblyBinding("Bin\\" + fileName);
                                                    }

                                                    en = tempfi.Name;
                                                    dlls.Add(en);
                                                }
                                                else if (!IsValidFormat(tempfi.Extension)) {
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

                                    if (dlls.Count > 0) {
                                        AppDLLs appDLLs = new AppDLLs();
                                        appDLLs.AddItem("app-" + fileName);
                                    }
                                }
                            }

                            UploadAppParameters("", randomString, picname);
                            ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?c=upload&b=app_installed&isZip=true&appId=" + randomString + "&date=" + DateTime.Now.Ticks);
                        }
                        else {
                            SetReturnMessage("Error! Must contain at least one valid file in zip.", true, 3000);
                        }
                    }
                    else if (IsValidFormat(fi.Extension)) {
                        int increment = 1;
                        string fileName = fi.Name.Replace(fi.Extension, "");
                        string filePath = ServerSettings.GetServerMapLocation + "Apps\\" + randomString;
                        var di = new DirectoryInfo(ServerSettings.GetServerMapLocation + "Apps\\");
                        for (int i = 0; i < di.GetFiles().Length; i++) {
                            string filename = di.GetFiles()[i].Name;
                            if (filename != fu_uploadnew.FileName) continue;
                            filePath = filePath + increment.ToString(CultureInfo.InvariantCulture);
                            fileName = fileName + increment.ToString(CultureInfo.InvariantCulture);
                            increment++;
                        }
                        fu_uploadnew.SaveAs(filePath + fi.Extension.ToLower());

                        UploadAppParameters(fileName + fi.Extension.ToLower(), randomString, picname);
                        ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?c=upload&b=app_installed&isZip=false&date=" + DateTime.Now.Ticks);
                    }
                    else
                        SetReturnMessage("Error! The file extension " + fi.Extension + " is not allowed. Please visit the help page for more information.", true, 3000);
                }
            }
            else
                SetReturnMessage("Error! App already exists", true, 3000);
        }
        else
            SetReturnMessage("Error! No file to upload or missing app name.", true, 3000);
    }
    private bool HasAtLeastOneValidPage(Stream str) {
        bool returnVal = false;
        ZipInputStream zipInStream = new ZipInputStream(str);
        ZipEntry entry;
        while ((entry = zipInStream.GetNextEntry()) != null) {
            string fn = Path.GetFileName(entry.Name);

            if (string.IsNullOrEmpty(fn)) {
                continue;
            }

            var tempfi = new FileInfo(fn);
            if ((tempfi.Extension.ToLower() != ".exe") && (tempfi.Extension.ToLower() != ".com") && (tempfi.Extension.ToLower() != ".pif")
                 && (tempfi.Extension.ToLower() != ".bat") && (tempfi.Extension.ToLower() != ".scr")) {

                if ((fn != String.Empty) && (entry.Name.IndexOf(".ini") < 0)) {
                    string en = entry.Name;

                    if (tempfi.Extension.ToLower() == ".pdb") {
                        continue;
                    }

                    if (IsValidFormat(tempfi.Extension)) {
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
    private bool IsValidFormat(string fileExt) {
        if ((fileExt.ToLower() == ".aspx") || (fileExt.ToLower() == ".html") || (fileExt.ToLower() == ".php") || (fileExt.ToLower() == ".asp") ||
            (fileExt.ToLower() == ".htm") || (fileExt.ToLower() == ".xhtml") || (fileExt.ToLower() == ".jhtml") || (fileExt.ToLower() == ".php4") ||
            (fileExt.ToLower() == ".php3") || (fileExt.ToLower() == ".phtml") || (fileExt.ToLower() == ".xml") || (fileExt.ToLower() == ".rss") ||
            (fileExt.ToLower() == ".txt") || (fileExt.ToLower() == ".ascx") || (fileExt.ToLower() == ".pdf") || (fileExt.ToLower() == ".docx") ||
            (fileExt.ToLower() == ".xlsx") || (fileExt.ToLower() == ".xls") || (fileExt.ToLower() == ".doc")) {
            return true;
        }

        return false;
    }

    protected void hf_createapp_Changed(object sender, EventArgs e) {
        bool cancontinue = false;
        if ((!string.IsNullOrEmpty(tb_filename_create.Text)) && (!string.IsNullOrEmpty(tb_appname.Text))) {
            newAppID = MakeValidFileName(tb_filename_create.Text).ToLower();
            string appId = "app-" + newAppID;
            tb_appname.Text = tb_appname.Text.Trim();
            if (!AppExists(tb_appname.Text, appId)) {
                if ((!string.IsNullOrEmpty(tb_appname.Text)) && (!_apps.IconExists(appId))) {
                    string picname = "generic.png";
                    if (fu_image_create.HasFile) {
                        var fiImage = new FileInfo(fu_image_create.FileName);
                        if ((fiImage.Extension.ToLower() == ".png") || (fiImage.Extension.ToLower() == ".gif")
                            || (fiImage.Extension.ToLower() == ".jpg") || (fiImage.Extension.ToLower() == ".jpeg")) {
                            picname = fu_image_create.FileName;
                            string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + fu_image_create.FileName;
                            cancontinue = _imageConverter.SaveNewImg(fu_image_create.PostedFile, filePath);
                        }
                        else
                            SetReturnMessage("Error! The file extension " + fiImage.Extension + " is not allowed. Please visit the help page for more information.", true, 3000);
                    }
                    else if (!string.IsNullOrEmpty(tb_imageurl.Text)) {
                        try {
                            string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + newAppID + ".png";
                            cancontinue = _imageConverter.SaveNewImg(tb_imageurl.Text, filePath);
                            picname = newAppID + ".png";
                        }
                        catch {
                            cancontinue = false;
                        }
                    }
                    else
                        cancontinue = true;

                    if (cancontinue) {
                        int increment = 1;

                        string filePath = ServerSettings.GetServerMapLocation + "Apps\\" + newAppID;

                        var html = new StringBuilder();
                        string htmlText = HttpUtility.UrlDecode(hf_createapp.Value);
                        string editor = HttpUtility.UrlDecode(hidden_editor.Value.Trim());

                        // Need to unescape the code one more time due to security reasons.
                        htmlText = HttpUtility.UrlDecode(htmlText);
                        editor = HttpUtility.UrlDecode(editor);

                        if ((dd_filename_ext.SelectedValue == ".html") || (dd_filename_ext.Enabled == false) || (dd_filename_ext.Visible == false)) {
                            html.AppendLine("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");
                            html.AppendLine("<html xmlns='http://www.w3.org/1999/xhtml'>");
                            html.AppendLine("<head>");
                            html.AppendLine("<title>" + tb_appname.Text + "</title>");
                            html.AppendLine("</head>");
                            html.AppendLine("<body>");
                            html.AppendLine("<div class='main-div-app-bg'>");
                            html.AppendLine(htmlText);
                            //html.AppendLine(htmlscript.ToString());
                            html.AppendLine("</div>");
                            html.AppendLine("<script type='text/javascript'>" + editor + "</script>"); //jsscript.ToString().Trim()
                            html.AppendLine("</body>");
                            html.AppendLine("</html>");
                        }
                        else if (dd_filename_ext.SelectedValue == ".ascx") {
                            html.AppendLine("<%@ Control Language='C#' AutoEventWireup='true' ClientIDMode='Static' %>");
                            html.AppendLine("<div id='" + newAppID + "-load' class='main-div-app-bg'>");
                            if ((_username.ToLower() == ServerSettings.AdminUserName.ToLower()) || (!_ss.LockASCXEdit)) {
                                html.AppendLine(editor);
                            }
                            html.AppendLine("</div>");
                        }

                        bool allowparams = HelperMethods.ConvertBitToBoolean(dd_allow_params.SelectedValue);
                        bool allowpopout = HelperMethods.ConvertBitToBoolean(dd_allowpopout_create.SelectedValue);
                        bool autoOpen = HelperMethods.ConvertBitToBoolean(dd_autoOpen_create.SelectedValue);

                        string popoutloc = tb_popoutLoc_create.Text.Trim();

                        string fileLocation = newAppID + dd_filename_ext.SelectedValue.ToLower();
                        if ((cb_wrapIntoIFrame.Checked) && (dd_filename_ext.SelectedValue.ToLower() == ".html")) {
                            Directory.CreateDirectory(filePath);
                            filePath = ServerSettings.GetServerMapLocation + "Apps\\" + newAppID + "\\" + newAppID;
                            string iframe = CreateIFrame(tb_appname.Text, appId, "Apps/" + newAppID + "/" + fileLocation);
                            File.WriteAllText(filePath + "Loader.html", iframe);
                            fileLocation = newAppID + "/" + newAppID + "Loader.html";
                        }

                        File.WriteAllText(filePath + dd_filename_ext.SelectedValue, html.ToString());

                        string oIds = "";
                        foreach (ListItem cbItem in cc_associatedOverlayNew.Items) {
                            if (cbItem.Selected) {
                                oIds += cbItem.Value + ";";
                            }
                        }

                        bool _isPrivate = cb_isPrivate.Checked;
                        if (!cb_InstallAfterLoad.Checked) {
                            _isPrivate = false;
                        }

                        string categorySelectedVals = string.Empty;
                        foreach (ListItem checkedItem in dd_category.Items) {
                            if (checkedItem.Selected) {
                                categorySelectedVals += checkedItem.Value + ServerSettings.StringDelimiter;
                            }
                        }

                        _apps.CreateItem(appId, tb_appname.Text, fileLocation, picname,
                                            dd_allowresize_create.SelectedValue, dd_allowmax_create.SelectedValue,
                                            tb_about_create.Text, tb_description_create.Text,
                                            dd_enablebg_create.SelectedValue, categorySelectedVals,
                                            tb_minheight_create.Text.Replace("px", ""), tb_minwidth_create.Text.Replace("px", ""), allowparams,
                                            allowpopout, popoutloc, oIds, "", autoOpen, dd_defaultworkspace_create.SelectedValue, _isPrivate);

                        bool maxonload = HelperMethods.ConvertBitToBoolean(dd_maxonload_create.SelectedValue);
                        _apps.UpdateAutoFullScreen(appId, maxonload);

                        if (dd_filename_ext.SelectedValue == ".ascx") {
                            bool autocreate = HelperMethods.ConvertBitToBoolean(dd_autocreate_create.SelectedValue);
                            _apps.UpdateAutoLoad(appId, autocreate);
                        }

                        if (dd_package.SelectedValue != "") {
                            var package = new AppPackages(false);
                            string list = package.GetAppList_nonarray(dd_package.SelectedValue);
                            list += appId + ",";
                            package.updateAppList(dd_package.SelectedValue, list, _username);
                        }

                        if (cb_InstallAfterLoad.Checked) {
                            var member = new MemberDatabase(_username);
                            member.UpdateEnabledApps(appId);
                        }

                        ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?c=create&b=app_created&date=" + DateTime.Now.Ticks);
                    }
                }
            }
            else
                SetReturnMessage("Error! App already exists", true, 3000);
        }
        else
            SetReturnMessage("Error! Must have a filename and app name defined.", true, 3000);

        hf_createapp.Value = string.Empty;
    }

    protected void btn_createEasy_Click(object sender, EventArgs e) {
        Thread.Sleep(1000);
        try {
            string htmllink = tb_html_create.Text;
            if (tb_html_create.Text.Substring(0, 4) != "http")
                htmllink = "http://" + htmllink;

            var uri = new Uri(htmllink);
            if (uri.IsAbsoluteUri) {
                if ((!string.IsNullOrEmpty(tb_appname.Text)) && (!string.IsNullOrEmpty(tb_html_create.Text))) {
                    newAppID = MakeValidFileName(tb_filename_create.Text).ToLower();
                    string appId = "app-" + MakeValidFileName(tb_filename_create.Text).ToLower();
                    tb_appname.Text = tb_appname.Text.Trim();
                    if ((!string.IsNullOrEmpty(tb_appname.Text)) && (!_apps.IconExists(appId))) {
                        if (!AppExists(tb_appname.Text, appId)) {
                            bool cancontinue = true;
                            bool canCreateNewImg = true;

                            #region Upload/Create Icon
                            string picname = "generic.png";
                            if (fu_image_create.HasFile) {
                                var fiImage = new FileInfo(fu_image_create.FileName);
                                if ((fiImage.Extension.ToLower() == ".png") || (fiImage.Extension.ToLower() == ".gif")
                                    || (fiImage.Extension.ToLower() == ".jpg") || (fiImage.Extension.ToLower() == ".jpeg")) {
                                    picname = fu_image_create.FileName;
                                    string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + fu_image_create.FileName;
                                    cancontinue = _imageConverter.SaveNewImg(fu_image_create.PostedFile, filePath);
                                }
                                else {
                                    SetReturnMessage("Error! The file extension " + fiImage.Extension + " is not allowed. Please visit the help page for more information.", true, 3000);
                                    cancontinue = false;
                                }
                            }
                            else if (!string.IsNullOrEmpty(tb_imageurl.Text)) {
                                try {
                                    string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + newAppID + ".png";
                                    cancontinue = _imageConverter.SaveNewImg(tb_imageurl.Text, filePath);
                                    picname = newAppID + ".png";

                                    canCreateNewImg = false;
                                }
                                catch { }
                            }
                            #endregion

                            if (cancontinue) {
                                int increment = 1;

                                string filePath = ServerSettings.GetServerMapLocation + "Apps\\" + newAppID;
                                var di = new DirectoryInfo(ServerSettings.GetServerMapLocation + "Apps\\");

                                var html = new StringBuilder();
                                html.AppendLine(
                                    "<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");
                                html.AppendLine("<html xmlns='http://www.w3.org/1999/xhtml'>");
                                html.AppendLine("<head>");
                                html.AppendLine("<title>" + tb_appname.Text + "</title>");
                                html.AppendLine("</head>");
                                html.AppendLine("<body>");

                                string id = "iframe_" + newAppID;

                                if (canCreateNewImg) {
                                    if ((htmllink.Contains("http://www.youtube")) || (htmllink.Contains("www.youtube"))
                                        || (htmllink.Contains("http://www.youtube.com/watch")) ||
                                        (htmllink.Contains("www.youtube.com/watch"))) {
                                        var uriYt = new Uri(htmllink);
                                        string queryString = uriYt.Query.Replace("?v=", "");
                                        if (uriYt.Query.IndexOf('&') != -1)
                                            queryString = queryString.Replace(uriYt.Query.Substring(uriYt.Query.IndexOf('&')), "");

                                        string imgfilePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + newAppID + ".png";
                                        _imageConverter.SaveNewImg("http://img.youtube.com/vi/" + queryString + "/default.jpg", imgfilePath);
                                        picname = newAppID + ".png";
                                    }
                                    else {
                                        try {
                                            string imgfilePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + newAppID + ".png";
                                            _imageConverter.SaveNewImg(htmllink + "/favicon.ico", imgfilePath);
                                            picname = newAppID + ".png";
                                        }
                                        catch { }
                                    }
                                }

                                html.AppendLine(ConvertUrlsToLinks(id, htmllink));

                                string editor = hidden_editor.Value.Trim();

                                html.AppendLine("<script type='text/javascript'>" + editor + "</script>");
                                html.AppendLine("</body>");
                                html.AppendLine("</html>");

                                bool allowparams = HelperMethods.ConvertBitToBoolean(dd_allow_params.SelectedValue);
                                bool allowpopout = HelperMethods.ConvertBitToBoolean(dd_allowpopout_create.SelectedValue);
                                string popoutloc = tb_popoutLoc_create.Text.Trim();
                                bool autoOpen = HelperMethods.ConvertBitToBoolean(dd_autoOpen_create.SelectedValue);

                                string oIds = "";
                                foreach (ListItem cbItem in cc_associatedOverlayNew.Items) {
                                    if (cbItem.Selected) {
                                        oIds += cbItem.Value + ";";
                                    }
                                }

                                bool _isPrivate = cb_isPrivate.Checked;
                                if (!cb_InstallAfterLoad.Checked) {
                                    _isPrivate = false;
                                }

                                string categorySelectedVals = string.Empty;
                                foreach (ListItem checkedItem in dd_category.Items) {
                                    if (checkedItem.Selected) {
                                        categorySelectedVals += checkedItem.Value + ServerSettings.StringDelimiter;
                                    }
                                }

                                File.WriteAllText(filePath + ".html", html.ToString());
                                _apps.CreateItem(appId, tb_appname.Text,
                                                    newAppID + ".html", picname,
                                                    dd_allowresize_create.SelectedValue, dd_allowmax_create.SelectedValue,
                                                    tb_about_create.Text, tb_description_create.Text,
                                                    dd_enablebg_create.SelectedValue, categorySelectedVals,
                                                    tb_minheight_create.Text.Replace("px", ""), tb_minwidth_create.Text.Replace("px", ""), allowparams,
                                                    allowpopout, popoutloc, oIds, "", autoOpen, dd_defaultworkspace_create.SelectedValue, _isPrivate);

                                bool maxonload = HelperMethods.ConvertBitToBoolean(dd_maxonload_create.SelectedValue);
                                _apps.UpdateAutoFullScreen(appId, maxonload);

                                if (dd_package.SelectedValue != "") {
                                    var package = new AppPackages(false);
                                    string list = package.GetAppList_nonarray(dd_package.SelectedValue);
                                    list += appId + ",";
                                    package.updateAppList(dd_package.SelectedValue, list, _username);
                                }

                                if (cb_InstallAfterLoad.Checked) {
                                    var member = new MemberDatabase(_username);
                                    member.UpdateEnabledApps(appId);
                                }

                                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?c=easycreate&b=app_created&date=" + DateTime.Now.Ticks);
                            }
                        }
                    }
                    else
                        SetReturnMessage("Error! App already exists", true, 3000);
                }
                else
                    SetReturnMessage("Error! Must have a filename and app name defined.", true, 3000);
            }
            else
                SetReturnMessage("Error! HTML Link MUST be a well formed url.", true, 3000);
        }
        catch {
            SetReturnMessage("Error! HTML Link MUST be a well formed url.", true, 3000);
        }
    }

    private void UploadAppParameters(string filename, string fi, string picname) {
        bool allowparams = HelperMethods.ConvertBitToBoolean(dd_allow_params.SelectedValue);
        bool allowpopout = HelperMethods.ConvertBitToBoolean(dd_allowpopout_create.SelectedValue);
        string popoutloc = tb_popoutLoc_create.Text.Trim();
        bool autoOpen = HelperMethods.ConvertBitToBoolean(dd_autoOpen_create.SelectedValue);

        string oIds = "";
        foreach (ListItem cbItem in cc_associatedOverlayNew.Items) {
            if (cbItem.Selected) {
                oIds += cbItem.Value + ";";
            }
        }

        bool _isPrivate = cb_isPrivate.Checked;
        if (!cb_InstallAfterLoad.Checked) {
            _isPrivate = false;
        }

        string categorySelectedVals = string.Empty;
        foreach (ListItem checkedItem in dd_category.Items) {
            if (checkedItem.Selected) {
                categorySelectedVals += checkedItem.Value + ServerSettings.StringDelimiter;
            }
        }

        string appId = "app-" + fi;
        _apps.CreateItem(appId, tb_appname.Text, filename, picname,
                            dd_allowresize_create.SelectedValue, dd_allowmax_create.SelectedValue,
                            tb_about_create.Text, tb_description_create.Text,
                            dd_enablebg_create.SelectedValue, categorySelectedVals,
                            tb_minheight_create.Text, tb_minwidth_create.Text, allowparams,
                            allowpopout, popoutloc, oIds, "", autoOpen, dd_defaultworkspace_create.SelectedValue, _isPrivate);
        bool maxonload = HelperMethods.ConvertBitToBoolean(dd_maxonload_create.SelectedValue);
        _apps.UpdateAutoFullScreen(appId, maxonload);

        if (dd_package.SelectedValue != "") {
            var package = new AppPackages(false);
            string list = package.GetAppList_nonarray(dd_package.SelectedValue);
            list += appId + ",";
            package.updateAppList(dd_package.SelectedValue, list, _username);
        }

        if (cb_InstallAfterLoad.Checked) {
            var member = new MemberDatabase(_username);
            member.UpdateEnabledApps(appId);
        }
    }

    private string ConvertUrlsToLinks(string id, string htmllink) {
        if ((htmllink.Contains("http://www.youtube")) || (htmllink.Contains("www.youtube"))
            || (htmllink.Contains("http://www.youtube.com/watch")) || (htmllink.Contains("www.youtube.com/watch"))) {
            return ConvertToObjectEmbeded(id, htmllink);
        }

        var striFrame = new StringBuilder();
        striFrame.Append("<iframe class='iFrame-apps' frameborder='0' id='" + id + "' marginheight='0' marginwidth='0' src='" + htmllink + "'");
        striFrame.Append(" width='100%' style='border: 0'></iframe>");
        return striFrame.ToString();
    }

    private static string ConvertToObjectEmbeded(string id, string htmllink) {
        string[] del = { "?v=" };
        string[] vidid = htmllink.Split(del, StringSplitOptions.RemoveEmptyEntries);
        if (vidid.Length > 0) {
            var str = new StringBuilder();

            // IFRAME VERSION (RECOMMENDED)
            str.Append("<iframe class='iFrame-apps' frameborder='0' id='" + id + "' type='text/html' marginheight='0' marginwidth='0' ");
            str.Append("src='http://www.youtube.com/embed/" + vidid[1] + "?autoplay=0' width='100%' style='border: 0'></iframe>");

            htmllink = str.ToString();
        }
        return htmllink;
    }

    protected void btn_clear_controls_Click(object sender, EventArgs e) {
        string rq = !string.IsNullOrEmpty(Request.QueryString["c"])
                        ? "?c=" + Request.QueryString["c"] + "&date=" + DateTime.Now.Ticks
                        : "?date=" + DateTime.Now.Ticks;

        ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx" + rq);
    }

    private bool AppExists(string name, string id) {
        string _name = _apps.GetAppID(name);
        string _id = _apps.GetAppName(id);
        if ((!string.IsNullOrEmpty(_id)) || (!string.IsNullOrEmpty(_name)))
            return true;

        return false;
    }

    private string CreateIFrame(string title, string id, string loc) {
        StringBuilder html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");
        html.AppendLine("<html xmlns='http://www.w3.org/1999/xhtml'>");
        html.AppendLine("<head>");
        html.AppendLine("<title>" + title + "</title>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<div id='loadiframe_" + id + "'>");
        html.AppendLine("<iframe id='iframe_" + id + "' class='iFrame-apps' width='100%' frameborder='0' marginheight='0' marginwidth='0' src='" + loc + "?iframe=true'></iframe>");
        html.AppendLine("</div>");
        html.AppendLine("<script type='text/javascript'></script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    #endregion


    #region App Delete Password Check
    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(hf_appchange.Value)) return;

        string createdBy = _apps.GetAppCreatedBy(hf_appchange.Value);
        MembershipUser u = Membership.GetUser(createdBy);

        string passwordUser = ServerSettings.AdminUserName;
        if ((_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && (u != null)) {
            passwordUser = u.UserName;
        }

        if (!string.IsNullOrEmpty(passwordUser)) {
            bool isGood = Membership.ValidateUser(passwordUser, tb_passwordConfirm.Text);
            if (isGood) {
                RegisterPostbackScripts.RegisterStartupScript(this, "BeginWork();");
            }
            else {
                tb_passwordConfirm.Text = "";
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Password is invalid');");
            }
        }
    }

    protected void hf_StartDelete_Changed(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(hf_appchange.Value)) return;

        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (var member in from MembershipUser u in coll select new MemberDatabase(u.UserName)) {
            member.RemoveEnabledApp(hf_appchange.Value);
        }

        string name = _apps.GetAppName(hf_appchange.Value);
        string icon = _apps.GetAppIconName(hf_appchange.Value);

        bool isDB = false;
        if (!string.IsNullOrEmpty(icon)) {
            icon = icon.ToLower();
            if ((icon != "generic.png") && (icon != "database.png")) {
                if (File.Exists(ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + icon)) {
                    try {
                        File.Delete(ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + icon);
                    }
                    catch { }
                }
            }
        }

        _apps.DeleteAppComplete(hf_appchange.Value, ServerSettings.GetServerMapLocation);
        _apps.DeleteAppLocal(hf_appchange.Value);

        var wp = new AppPackages(true);
        foreach (Dictionary<string, string> dr in wp.listdt) {
            string[] l = wp.GetAppList(dr["ID"]);
            string tempL = l.Where(w => w.ToLower() != hf_appchange.Value.ToLower())
                            .Aggregate(string.Empty, (current, w) => current + (w + ","));

            wp.updateAppList(dr["ID"], tempL, _username);
        }

        string rq = "";
        if (!string.IsNullOrEmpty(Request.QueryString["c"])) {
            rq = "?c=" + Request.QueryString["c"];
        }

        ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx" + rq);
    }
    #endregion

}