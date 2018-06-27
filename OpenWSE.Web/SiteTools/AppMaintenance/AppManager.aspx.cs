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

public partial class SiteTools_AppManager : BasePage {

    #region Private Variables

    private App CurrentAppObject = new App(string.Empty);
    private readonly ImageConverter _imageConverter = new ImageConverter();
    private readonly AppCategory _category = new AppCategory(true);
    private readonly WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
    private string newAppID;

    #endregion


    #region PageLoad and help methods

    protected void Page_Load(object sender, EventArgs e) {
        HelperMethods.SetIsSocialUserForDeleteItems(Page, CurrentUsername);
        CurrentAppObject = new App(CurrentUsername, false);

        bool lockAppCreator = MainServerSettings.LockAppCreator;
        if (lockAppCreator) {
            pnl_app_information.Attributes.Add("data-appcreatorlocked", "true");
        }

        if (!IsPostBack) {
            BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);
            BuildDefaultWorkspaceDropDown();
        }

        SetReturnMessage_OnLoad();

        if (IsUserNameEqualToAdmin()) {
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

        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            cb_InstallAfterLoad.Enabled = false;
            cb_InstallAfterLoad.Visible = false;
            cb_isPrivate.Enabled = false;
            cb_isPrivate.Visible = false;
            cb_InstallAfterLoad.Checked = false;
            cb_isPrivate.Checked = false;
        }

        pnl_appicon.Enabled = true;
        pnl_appicon.Visible = true;
        pnl_appIconEdit.Enabled = true;
        pnl_appIconEdit.Visible = true;

        var scriptManager = ScriptManager.GetCurrent(this);
        if (scriptManager != null) {
            scriptManager.RegisterPostBackControl(btn_uploadnew);
            scriptManager.RegisterPostBackControl(btn_save_2);
            scriptManager.RegisterAsyncPostBackControl(btn_save);
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

        lbl_dotHtml.Enabled = true;
        lbl_dotHtml.Visible = true;

        if (!IsUserInAdminRole()) {
            pnl_backupAllApps.Enabled = false;
            pnl_backupAllApps.Visible = false;
        }

        if (Request.QueryString["tab"] == "upload" && !lockAppCreator) {
            if (Request.QueryString["b"] == "app_installed") {
                if (!IsPostBack) {
                    SetReturnMessage("App installed successfully!", false, false);

                    if (!string.IsNullOrEmpty(Request.QueryString["isZip"])) {
                        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["isZip"])) {
                            if (!string.IsNullOrEmpty(Request.QueryString["appId"])) {
                                radioButton_FileList_New.Items.Clear();
                                string filePath = ServerSettings.GetServerMapLocation + "Apps\\" + Request.QueryString["appId"];
                                try {
                                    DirectoryInfo di = new DirectoryInfo(filePath);
                                    GetLoaderFileListNew(di.FullName, di.Name);

                                    RegisterPostbackScripts.RegisterStartupScript(this, "window.onload = function () { LoadNewDefaultPageSelector(); };");
                                }
                                catch { }
                            }
                        }
                    }
                }
                btn_uploadnew.Enabled = true;
                btn_uploadnew.Visible = true;
            }
            pnl_app_information.Enabled = true;
            pnl_app_information.Visible = true;
            pnl_htmleditor.Style["display"] = "none";
            btn_clear_controls.Enabled = true;
            btn_clear_controls.Visible = true;
            btn_uploadnew.Enabled = true;
            btn_uploadnew.Visible = true;
            cb_wrapIntoIFrame.Enabled = false;
            cb_wrapIntoIFrame.Visible = false;
            pnl_app_EditList.Enabled = false;
            pnl_app_EditList.Visible = false;
            app_templatebtn.Visible = true;

            if (!IsPostBack) {
                newAppID = HelperMethods.RandomString(10);
                tb_filename_create.Text = newAppID;
            }
            RegisterPostbackScripts.RegisterStartupScript(this, "$('.sitemenu-selection').find('li').eq(2).addClass('active');$('#newupload').show();");
        }
        else if (Request.QueryString["tab"] == "easycreate" && !lockAppCreator) {
            if (Request.QueryString["b"] == "app_created") {
                if (!IsPostBack) {
                    SetReturnMessage("App installed successfully!", false, false);
                }
            }
            pnl_htmleditor.Style["display"] = "none";
            btn_clear_controls.Enabled = true;
            btn_clear_controls.Visible = true;
            pnl_app_information.Enabled = true;
            pnl_app_information.Visible = true;
            btn_create_easy.Enabled = true;
            btn_create_easy.Visible = true;
            pnl_apphtml.Enabled = true;
            pnl_apphtml.Visible = true;
            lbl_dotHtml.Enabled = true;
            lbl_dotHtml.Visible = true;
            cb_wrapIntoIFrame.Enabled = false;
            cb_wrapIntoIFrame.Visible = false;
            pnl_app_EditList.Enabled = false;
            pnl_app_EditList.Visible = false;
            app_templatebtn.Visible = false;
            div_AllowNotifications_Create.Visible = false;

            if (!IsPostBack) {
                newAppID = HelperMethods.RandomString(10);
                tb_filename_create.Text = newAppID;
            }
            RegisterPostbackScripts.RegisterStartupScript(this, "$('.sitemenu-selection').find('li').eq(1).addClass('active');");
        }
        else if (Request.QueryString["tab"] == "params") {
            pnl_appList1.Controls.Add(new LiteralControl(BuildAppParmListEditor()));
            pnl_app_params.Enabled = true;
            pnl_app_params.Visible = true;
            pnl_app_information.Enabled = false;
            pnl_app_information.Visible = false;
            pnl_htmleditor.Style["display"] = "none";
            btn_clear_controls.Enabled = false;
            btn_clear_controls.Visible = false;
            btn_create_easy.Enabled = false;
            btn_create_easy.Visible = false;
            pnl_appicon.Enabled = false;
            pnl_appicon.Visible = false;
            pnl_apphtml.Enabled = false;
            pnl_apphtml.Visible = false;
            pnl_app_EditList.Enabled = false;
            pnl_app_EditList.Visible = false;

            hf_isParams.Value = "1";

            if (!MainServerSettings.LockAppCreator) {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('.sitemenu-selection').find('li').eq(3).addClass('active');");
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('.sitemenu-selection').find('li').eq(1).addClass('active');");
            }
        }
        else {
            BuildAppList();
            BuildAppListCategories();
            if (Request.QueryString["b"] == "app_created") {
                if (!IsPostBack) {
                    SetReturnMessage("App installed successfully!", false, false);
                }
            }
            newAppID = HelperMethods.RandomString(10);
            if (!IsPostBack) {
                tb_filename_create.Text = newAppID;
            }
            RegisterPostbackScripts.RegisterStartupScript(this, "$('.sitemenu-selection').find('li').eq(0).addClass('active');");
        }

        GetPostBack();
    }
    private void GetPostBack() {
        switch (AsyncPostBackSourceElementID) {
            case "btn_updateLoaderFileCancel":
                if (!string.IsNullOrEmpty(Request.QueryString["appId"])) {
                    string appId = "app-" + Request.QueryString["appId"];
                    MembershipUserCollection coll = Membership.GetAllUsers();
                    foreach (var member in from MembershipUser u in coll select new MemberDatabase(u.UserName)) {
                        member.RemoveEnabledApp(appId);
                    }

                    App tempApp = new App(string.Empty, false);
                    string createdBy = tempApp.GetAppCreatedBy(appId);
                    if (createdBy.ToLower() == CurrentUsername.ToLower()) {
                        string name = CurrentAppObject.GetAppName(appId);
                        string icon = CurrentAppObject.GetAppIconName(appId);

                        if (!string.IsNullOrEmpty(icon)) {
                            icon = icon.ToLower();
                            if (icon == App.DefaultAppIconLocation.ToLower() + DBImporter.DefaultDatabaseIcon.ToLower()) {
                                var db = new DBImporter();
                                db.DeleteEntry(appId.Replace("app-", "").ToUpper());
                            }
                            else if (icon != App.DefaultAppIconLocation.ToLower() + App.DefaultAppIcon.ToLower() && icon != App.DefaultAppIconLocation.ToLower() + DBImporter.DefaultDatabaseIcon.ToLower()) {
                                if (File.Exists(ServerSettings.GetServerMapLocation + "\\" + icon.Replace("/", "\\"))) {
                                    try {
                                        File.Delete(ServerSettings.GetServerMapLocation + "\\" + icon.Replace("/", "\\"));
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

                        CurrentAppObject.DeleteAppMain(appId);
                        CurrentAppObject.DeleteAppLocal(appId);

                        var wp = new AppPackages(true);
                        foreach (Dictionary<string, string> dr in wp.listdt) {
                            string[] l = wp.GetAppList(dr["ID"]);
                            string tempL = l.Where(w => w.ToLower() != appId.ToLower())
                                            .Aggregate(string.Empty, (current, w) => current + (w + ","));

                            wp.updateAppList(dr["ID"], tempL, CurrentUsername);
                        }
                    }

                    string rq = "";
                    if (!string.IsNullOrEmpty(Request.QueryString["tab"]))
                        rq = "&tab=" + Request.QueryString["tab"];

                    ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?date=" + ServerSettings.ServerDateTime.Ticks + rq);
                }
                break;
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
        foreach (Dictionary<string, string> dr in packages.listdt) {
            var item2 = new ListItem("&nbsp;" + dr["PackageName"], dr["ID"]);
            if (!dd_package.Items.Contains(item2)) {
                dd_package.Items.Add(item2);
            }
        }

        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            string groupId = GroupSessions.GetUserGroupSessionName(CurrentUsername);
            NewUserDefaults defaults = new NewUserDefaults(groupId);
            string packageId = defaults.GetDefault("AppPackage");
            foreach (ListItem item in dd_package.Items) {
                if (item.Value == packageId) {
                    item.Selected = true;
                    break;
                }
            }
        }

        if (!ServerSettings.AdminPagesCheck(Page.ToString(), CurrentUsername) || GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            pnl_AppPackage.Enabled = false;
            pnl_AppPackage.Visible = false;
        }
    }
    private void BuildDefaultWorkspaceDropDown() {
        dd_defaultworkspace_create.Items.Clear();
        int totalWorkspaces = MainServerSettings.TotalWorkspacesAllowed;
        for (int i = 0; i < totalWorkspaces; i++) {
            string iStr = (i + 1).ToString();
            dd_defaultworkspace_create.Items.Add(new ListItem(iStr, iStr));
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
    private void SetReturnMessage(string text, bool error, bool isSession = true) {
        string foreColor = "style='color:#32A800'";
        if (error)
            foreColor = "style='color:#FF0000'";

        if (!isSession) {
            StringBuilder str = new StringBuilder();
            str.Append("$('#lbl_ErrorUpload').html(\"<span " + foreColor + ">" + text + "</span>\");");
            str.Append("setTimeout(function() { $('#lbl_ErrorUpload').html(\"\"); }, 5000);");
            RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
        }
        else if (Session != null) {
            Session["ReturnMessage"] = "<span " + foreColor + ">" + text + "</span>";
        }
    }
    private void SetReturnMessage_OnLoad() {
        if (Session != null && Session["ReturnMessage"] != null) {
            string returnMessage = Session["ReturnMessage"].ToString();
            if (!string.IsNullOrEmpty(returnMessage)) {
                StringBuilder str = new StringBuilder();
                str.Append("$('#lbl_ErrorUpload').html(\"" + returnMessage + "\");");
                str.Append("setTimeout(function() { $('#lbl_ErrorUpload').html(\"\"); }, 5000);");
                RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
            }

            Session["ReturnMessage"] = null;
        }
    }

    #endregion


    #region Main Tab (Edit Apps)

    private void BuildAppList() {
        StringBuilder strJavascript = new StringBuilder();

        App appItem = new App(CurrentUsername, false);
        appItem.GetAllApps();

        AppPackages packages = new AppPackages(false);
        AppRatings ratings = new AppRatings(CurrentUsername);

        int count = 0;

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 2, "AppTable_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns(string.Empty, "45", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("App Name", "200", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Description", "200", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Created By", "200", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Category", "250", false));
        if (MainServerSettings.AllowAppRating) {
            headerColumns.Add(new TableBuilderHeaderColumns("Rating", "110", false));
        }
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        foreach (Apps_Coll coll in appItem.AppList) {
            bool cancontinue = false;

            if (IsUserInAdminRole()) {
                cancontinue = true;
            }
            else {
                if (coll.CreatedBy.ToLower() == CurrentUsername.ToLower()) {
                    cancontinue = true;
                }
            }

            if (coll.CreatedBy.ToLower() != CurrentUsername.ToLower() && coll.IsPrivate && !IsUserNameEqualToAdmin()) {
                cancontinue = false;
            }

            if (cancontinue) {
                if (MainServerSettings.AssociateWithGroups) {
                    if (!ServerSettings.CheckAppGroupAssociation(coll, CurrentUserMemberDatabase)) {
                        continue;
                    }
                }
                if (coll != null && !string.IsNullOrEmpty(coll.ID)) {
                    string deleteBtn = string.Format("<a class='td-delete-btn' onclick=\"OnDelete('{0}');return false;\" title=\"Delete\"></a>", coll.AppId);
                    if (IsAStandardApp(coll.AppId)) {
                        deleteBtn = string.Empty;
                    }

                    string editBtn = string.Format("<a class='td-edit-btn' onclick=\"appchange('{0}');return false;\" title=\"Edit\"></a>", coll.AppId);

                    string appName = coll.AppName;
                    string categoryIds = string.Empty;
                    string categoryNames = string.Empty;
                    string ratingStr = string.Empty;

                    AppCategory _category = new AppCategory(false);
                    Dictionary<string, string> categoryList = _category.BuildCategoryDictionary(coll.Category);

                    if (!string.IsNullOrEmpty(ddl_categories.SelectedValue) && !categoryList.ContainsKey(ddl_categories.SelectedValue)) {
                        continue;
                    }

                    foreach (KeyValuePair<string, string> categoryPair in categoryList) {
                        categoryIds += categoryPair.Key + ",";
                        categoryNames += categoryPair.Value + ", ";
                    }

                    categoryNames = categoryNames.Trim();
                    if (categoryNames.EndsWith(",")) {
                        categoryNames = categoryNames.Remove(categoryNames.Length - 1);
                    }

                    string description = coll.Description;
                    if (string.IsNullOrEmpty(description)) {
                        description = "No description available";
                    }

                    if (!string.IsNullOrEmpty(coll.About)) {
                        description += "<div class='clear'></div><small><i>" + coll.About + "</i></small>";
                    }

                    string appColorCss = string.Empty;
                    if (!string.IsNullOrEmpty(coll.AppBackgroundColor) && coll.AppBackgroundColor.ToLower() != "inherit") {
                        string appColor = coll.AppBackgroundColor;
                        if (!appColor.StartsWith("#")) {
                            appColor = "#" + appColor;
                        }

                        string fontColor = "color: #515151;";
                        if (!HelperMethods.UseDarkTextColorWithBackground(appColor)) {
                            fontColor = "color: #FFFFFF;";
                        }

                        appColorCss = " style=\"" + fontColor + "background: " + appColor + ";\"";
                    }

                    count++;

                    string iconColor = coll.IconBackgroundColor;
                    if (!iconColor.StartsWith("#")) {
                        iconColor = "#" + iconColor;
                    }

                    string imgIcon = "<img alt='' src='" + ResolveUrl("~/" + coll.Icon) + "?" + ServerSettings.TimestampQuery + HelperMethods.GetTimestamp() + "' style='height: 25px; padding-left: 4px;' />";
                    string colorGradient = "style='" + HelperMethods.GetCSSGradientStyles(iconColor, string.Empty) + "'";

                    if (MainServerSettings.AllowAppRating) {
                        string averageRating = ratings.GetAverageRating(coll.AppId);
                        ratingStr = string.Format("<div class='sort-value-class' data-sortvalue=\"" + averageRating + "\"><div class='app-rater-{0} app-rater-installer rounded-corners-2'></div></div>", coll.AppId);
                        strJavascript.AppendFormat("openWSE.RatingStyleInit('.app-rater-{0}', '" + averageRating + "', true, '{0}', true);", coll.AppId);
                    }

                    List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues(string.Empty, imgIcon, TableBuilderColumnAlignment.Center, colorGradient));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("App Name", appName, TableBuilderColumnAlignment.Left, appColorCss));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Description", description, TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Created By", HelperMethods.MergeFMLNames(new MemberDatabase(coll.CreatedBy)), TableBuilderColumnAlignment.Left));
                    bodyColumnValues.Add(new TableBuilderBodyColumnValues("Category", categoryNames, TableBuilderColumnAlignment.Left));
                    if (MainServerSettings.AllowAppRating) {
                        bodyColumnValues.Add(new TableBuilderBodyColumnValues("Rating", ratingStr, TableBuilderColumnAlignment.Left, "", "rating-column"));
                    }

                    tableBuilder.AddBodyRow(bodyColumnValues, editBtn + deleteBtn, "data-category='" + categoryIds + "'", "app-item-installer");
                }
            }
        }
        #endregion

        pnl_AppList.Controls.Clear();
        pnl_AppList.Controls.Add(tableBuilder.CompleteTableLiteralControl("No apps found"));

        if (!string.IsNullOrEmpty(strJavascript.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, strJavascript.ToString());
        }

        updatePnl_AppList.Update();
    }
    private void BuildAppListCategories() {
        AppCategory appCategory = new AppCategory(true);

        ddl_categories.Items.Add(new ListItem("Show All", ""));
        ddl_categories.Items.Add(new ListItem(AppCategory.Uncategorized_Name, AppCategory.Uncategorized_Name));

        foreach (Dictionary<string, string> item in appCategory.category_dt) {
            if (item["Category"].ToLower() != AppCategory.Uncategorized_Name) {
                ListItem listItem = new ListItem(item["Category"], item["ID"]);
                if (!ddl_categories.Items.Contains(listItem)) {
                    ddl_categories.Items.Add(listItem);
                }
            }
        }
    }
    protected void ddl_categories_Changed(object sender, EventArgs e) {
        BuildAppList();
    }

    #endregion


    #region App Parameters

    private void GetAppParams(string appid, string parameterId) {
        var appparams = new AppParams(false);
        appparams.GetAllParameters_ForApp(appid);

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 2, "ParmsTable_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Parameter Value", "250", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Description", "300", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Updated By", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Updated", "165", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        foreach (var dr in from Dictionary<string, string> dr in appparams.listdt let m = new MemberDatabase(dr["CreatedBy"]) select dr) {
            string parmValue = dr["Parameter"];
            string description = dr["Description"];
            if (string.IsNullOrEmpty(description)) {
                description = "No description available";
            }

            // Try to hide any secret info
            if (parmValue.StartsWith("Access_Token_Secret=")) {
                parmValue = "Access_Token_Secret=***********************************";
            }
            else if (parmValue.StartsWith("Consumer_Secret=")) {
                parmValue = "Consumer_Secret=***********************************";
            }

            var actionbtns = new StringBuilder();

            if (dr["ID"] == parameterId) {
                parmValue = "<input type=\"text\" id=\"txt_appparam_edit\" class=\"textEntry-noWidth\" data-value=\"" + HttpUtility.UrlEncode(parmValue.Replace(" ", "&nbsp;")) + "\" />";
                description = "<input type=\"text\" id=\"txt_appparamdesc_edit\" class=\"textEntry-noWidth\" data-value=\"" + HttpUtility.UrlEncode(dr["Description"].Replace(" ", "&nbsp;")) + "\" />";
                actionbtns.Append("<a class='td-update-btn' onclick='UpdateParameter();return false;' title=\"Update\"></a>");
                actionbtns.Append("<a class='td-cancel-btn' onclick='CancelParameterEdit();return false;' title=\"Cancel\"></a>");
            }
            else if (string.IsNullOrEmpty(parameterId)) {
                actionbtns.Append("<a class='td-edit-btn' onclick='EditParameter(\"" + dr["ID"] + "\");return false;' title=\"Edit\"></a>");
                actionbtns.Append("<a class='td-delete-btn' onclick='DeleteParameter(\"" + dr["ID"] + "\");return false;' title=\"Delete\"></a>");
            }

            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Parameter Value", parmValue, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Description", description, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Updated By", dr["UserName"], TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Date Updated", dr["DateTime"], TableBuilderColumnAlignment.Left));
            tableBuilder.AddBodyRow(bodyColumnValues, actionbtns.ToString());
        }
        #endregion

        #region Build Insert
        if (string.IsNullOrEmpty(parameterId)) {
            List<TableBuilderInsertColumnValues> insertColumns = new List<TableBuilderInsertColumnValues>();
            insertColumns.Add(new TableBuilderInsertColumnValues("Parameter Value", "txt_app_params", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text));
            insertColumns.Add(new TableBuilderInsertColumnValues("Description", "txt_app_params_description", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text));
            insertColumns.Add(new TableBuilderInsertColumnValues("Updated By", string.Empty, TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
            insertColumns.Add(new TableBuilderInsertColumnValues("Date Updated", string.Empty, TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
            tableBuilder.AddInsertRow(insertColumns, "AddAppParameter()");
        }
        #endregion

        pnl_app_params_holder.Controls.Clear();
        pnl_app_params_holder.Controls.Add(tableBuilder.CompleteTableLiteralControl("No parameters found"));

        string postbackJavascript = "$(\".app-icon-parms[data-appid='" + appid + "']\").addClass(\"active\");";
        if (!string.IsNullOrEmpty(parameterId)) {
            postbackJavascript += "UnescapeEditValues();";
        }

        RegisterPostbackScripts.RegisterStartupScript(this, postbackJavascript);
    }
    protected void hf_appchange_params_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;
        if (!string.IsNullOrEmpty(hf_appchange_params.Value)) {
            string appname = CurrentAppObject.GetAppName(hf_appchange_params.Value);
            ltl_app_params.Text = "<h3 class='float-left'>App Parameters for <b>" + appname +
                                     "</b></h3><div class='clear-space'></div>";
            pnl_params_holder.Enabled = true;
            pnl_params_holder.Visible = true;
            lbl_params_tip.Enabled = false;
            lbl_params_tip.Visible = false;
            GetAppParams(hf_appchange_params.Value, string.Empty);
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "$(\".app-icon-parms\").removeClass(\"active\");");
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
        RegisterPostbackScripts.RegisterStartupScript(this, "$(\".app-icon-parms\").removeClass(\"active\");");
    }
    protected void btn_app_params_Click(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;

        string parmValue = HttpUtility.UrlDecode(hf_app_params.Value.Trim());

        if (!string.IsNullOrEmpty(hf_appchange_params.Value) && !string.IsNullOrEmpty(parmValue)) {
            var appparams = new AppParams(false);
            string description = HttpUtility.UrlDecode(hf_app_params_description.Value.Trim());
            appparams.addItem(hf_appchange_params.Value, parmValue, description, CurrentUsername);
        }
        else {
            lbl_param_error.Text = "Parameter cannot be blank<div class='clear-space-five'></div>";
            lbl_param_error.Enabled = true;
            lbl_param_error.Visible = true;
        }

        hf_btnapp_addParms.Value = string.Empty;

        GetAppParams(hf_appchange_params.Value, string.Empty);
    }
    protected void hf_appchange_params_edit_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;
        if (!string.IsNullOrEmpty(hf_appchange_params_edit.Value)) {
            GetAppParams(hf_appchange_params.Value, hf_appchange_params_edit.Value);
        }
    }
    protected void hf_appchange_params_delete_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;

        if (!string.IsNullOrEmpty(hf_appchange_params_delete.Value)) {
            var appparams = new AppParams(false);
            appparams.deleteParameter(hf_appchange_params_delete.Value);
            GetAppParams(hf_appchange_params.Value, string.Empty);
        }
        hf_appchange_params_edit.Value = "";
        hf_appchange_params_delete.Value = "";
    }
    protected void hf_appchange_params_update_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;

        string appParamVal = HttpUtility.UrlDecode(hf_appchange_params_update.Value);
        string appParamDesc = HttpUtility.UrlDecode(hf_appchange_paramsdesc_update.Value);

        if ((!string.IsNullOrEmpty(appParamVal)) && (!string.IsNullOrEmpty(hf_appchange_params_edit.Value))) {
            var appparams = new AppParams(false);
            appparams.updateParameter(hf_appchange_params_edit.Value, hf_appchange_params.Value, appParamVal.Trim(), appParamDesc.Trim(), CurrentUsername);
            GetAppParams(hf_appchange_params.Value, string.Empty);
        }

        hf_appchange_params_edit.Value = "";
        hf_appchange_params_update.Value = "";
        hf_appchange_paramsdesc_update.Value = "";
    }
    protected void hf_appchange_params_cancel_Changed(object sender, EventArgs e) {
        lbl_param_error.Text = string.Empty;
        lbl_param_error.Enabled = false;
        lbl_param_error.Visible = false;

        if (!string.IsNullOrEmpty(hf_appchange_params_cancel.Value)) {
            GetAppParams(hf_appchange_params.Value, string.Empty);
        }
        hf_appchange_params_edit.Value = "";
        hf_appchange_params_cancel.Value = "";
    }
    public string BuildAppParmListEditor() {
        var app_List = new StringBuilder();
        var app_script = new StringBuilder();
        var apps = new App(CurrentUsername, false);
        apps.GetAllApps();

        List<string> appListCount = new List<string>();
        foreach (Apps_Coll dr in apps.AppList) {
            bool cancontinue = false;

            if (IsUserInAdminRole()) {
                cancontinue = true;
            }
            else {
                if (dr.CreatedBy.ToLower() == CurrentUsername.ToLower()) {
                    cancontinue = true;
                }
            }

            if (dr.CreatedBy.ToLower() != CurrentUsername.ToLower() && dr.IsPrivate && !IsUserNameEqualToAdmin()) {
                cancontinue = false;
            }

            if (cancontinue && dr.AllowParams) {
                if (MainServerSettings.AssociateWithGroups) {
                    if (!ServerSettings.CheckAppGroupAssociation(dr, CurrentUserMemberDatabase)) {
                        continue;
                    }
                }

                string w = dr.AppName;
                string id = dr.AppId;
                string iconname = dr.Icon;

                if (!string.IsNullOrEmpty(dr.filename)) {
                    string iconImg = "<img alt='' src='" + ResolveUrl("~/" + iconname) + "' />";
                    if (string.IsNullOrEmpty(iconname))
                        iconImg = string.Empty;

                    if (!appListCount.Contains(dr.ID)) {
                        app_script.Append("<div class='app-icon app-icon-parms rbbuttons' data-appid=\"" + dr.AppId + "\" title=\"View " + w + "'s parameters\" onclick=\"appchange('" + dr.AppId + "')\">");
                        app_script.Append(iconImg + "<span class='app-icon-font'>" + w + "</span><div class=\"clear\"></div></div>");
                    }

                    if (!appListCount.Contains(dr.ID)) {
                        appListCount.Add(dr.ID);
                    }
                }
            }
        }

        int count = appListCount.Count;

        if (!string.IsNullOrEmpty(app_script.ToString())) {
            app_script.Append("</div>");
            if (IsUserInAdminRole()) {
                string appcount = "<b class='pad-right'>Apps Available</b><span>" + count.ToString() + "</span><div class='clear-space'></div>";
                appcount += "<small>Click on one of the apps below<br/>to view/edit the properties.</small>";
                appcount += "<div class='clear-space'></div><div class='clear-space'></div>";
                return appcount + app_List + app_script;
            }
            else {
                string appcount = "<b class='pad-right'>Apps Created</b><span>" + count.ToString() + "</span><div class='clear-space'></div>";
                appcount += "<small>Click on one of the apps below<br/>to view/edit the properties.</small>";
                appcount += "<div class='clear-space'></div><div class='clear-space'></div>";
                return appcount + app_List + app_script;
            }
        }
        else
            return "<span class='font-bold'>No Apps Available</span>";
    }

    #endregion


    #region App Overlay Updater

    private void GetOverlayList() {
        if (ServerSettings.AdminPagesCheck("overlaymanager.aspx", CurrentUsername)) {
            cb_associatedOverlay.Items.Clear();
            _workspaceOverlays.GetWorkspaceOverlays();
            foreach (WorkspaceOverlay_Coll doc in _workspaceOverlays.OverlayList) {
                if ((IsUserInAdminRole())
                    || ((doc.UserName.ToLower() == CurrentUsername.ToLower()) && (!IsUserInAdminRole()))) {
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
        if (ServerSettings.AdminPagesCheck("overlaymanager.aspx", CurrentUsername) && !GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            cc_associatedOverlayNew.Items.Clear();
            _workspaceOverlays.GetWorkspaceOverlays();
            foreach (WorkspaceOverlay_Coll doc in _workspaceOverlays.OverlayList) {
                if ((IsUserInAdminRole())
                    || ((doc.UserName.ToLower() == CurrentUsername.ToLower()) && (!IsUserInAdminRole()))) {
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
            CurrentAppObject.UpdateAppOverlayID(appId, name);
        }
        else {
            CurrentAppObject.UpdateAppOverlayID(appId, "");
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
            CurrentAppObject.UpdateAppFilename(appId, name);
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
                    CurrentAppObject.UpdateAppFilename(appId, name);

                string rq = "";
                if (!string.IsNullOrEmpty(Request.QueryString["tab"]))
                    rq = "&tab=" + Request.QueryString["tab"];

                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?date=" + ServerSettings.ServerDateTime.Ticks + rq);
            }
        }
    }

    #endregion


    #region App Box/Edit

    protected void hf_appchange_ValueChanged(object sender, EventArgs e) {
        if (hf_appchange.Value == "reset") {
            RegisterPostbackScripts.RegisterStartupScript(this, "appchange('reset');loadingPopup.Message('Loading. Please Wait...');");
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
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'App-element', 'App Details');loadingPopup.Message('Loading. Please Wait...');");
        }
    }
    private void Appchange() {
        if ((!string.IsNullOrEmpty(tb_title_edit.Text)) &&
            (tb_title_edit.Text != CurrentAppObject.GetAppName(hf_appchange.Value))) return;
        if (!string.IsNullOrEmpty(hf_appchange.Value)) {
            Apps_Coll db = CurrentAppObject.GetAppInformation(hf_appchange.Value);
            if (db != null) {
                CurrentAppObject.BuildAboutApp(wlmd_holder, hf_appchange.Value, CurrentUsername);

                if (MainServerSettings.AllowAppRating) {
                    AppRatings ratings = new AppRatings();
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.RatingStyleInit('.app-rater-" + hf_appchange.Value + "', '" + ratings.GetAverageRating(hf_appchange.Value) + "', true, '" + hf_appchange.Value + "', true);");
                }

                if (!IsUserInAdminRole()) {
                    if (db.CreatedBy.ToLower() == CurrentUsername.ToLower()) {
                        Appedit();
                        lb_editsource.Enabled = true;
                        lb_editsource.Visible = true;
                        btn_delete.Enabled = true;
                        btn_delete.Visible = true;
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
                    if (db.filename.Contains("Custom_Tables/") || db.filename.Contains("Database_Imports/")) {
                        lb_editsource.Enabled = false;
                        lb_editsource.Visible = false;
                    }
                    else {
                        lb_editsource.Enabled = true;
                        lb_editsource.Visible = true;
                    }
                    btn_delete.Enabled = true;
                    btn_delete.Visible = true;
                    btn_edit.Enabled = true;
                    btn_edit.Visible = true;
                }

                if (db.filename.Contains("Custom_Tables/") || db.filename.Contains("Database_Imports/")) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "$('#iframe-appDownloader').hide();");
                    changeLoadFile_holder.Visible = false;
                }
                else {
                    changeLoadFile_holder.Visible = true;
                    if (HelperMethods.IsValidHttpBasedAppType(db.filename)) {
                        lb_editsource.Enabled = false;
                        lb_editsource.Visible = false;
                        RegisterPostbackScripts.RegisterStartupScript(this, "$('#iframe-appDownloader').hide();");
                    }
                    else {
                        RegisterPostbackScripts.RegisterStartupScript(this, "$('#iframe-appDownloader').show();");
                    }
                }

                SetStandardApps(db.AppId, db.filename);
            }
        }
    }
    private void Appedit() {
        if (string.IsNullOrEmpty(hf_appchange.Value)) return;
        Apps_Coll db = CurrentAppObject.GetAppInformation(hf_appchange.Value);
        if (db == null) return;
        lb_editsource.Enabled = false;
        lb_editsource.Visible = false;
        LoadAppDownloadBtn();
        img_edit.ImageUrl = ServerSettings.GetSitePath(Request) + "/" + db.Icon + "?" + ServerSettings.TimestampQuery + HelperMethods.GetTimestamp();
        img_edit.Visible = true;
        tb_title_edit.Text = db.AppName;

        string about = db.About;
        if (string.IsNullOrEmpty(about)) {
            string siteName = ServerSettings.SiteName;
            if (!string.IsNullOrEmpty(siteName)) {
                siteName += " | ";
            }

            tb_about_edit.Text = siteName + ServerSettings.ServerDateTime.Year;
        }
        else {
            tb_about_edit.Text = about;
        }

        tb_description_edit.Text = db.Description;
        tb_filename_edit.Text = db.filename;

        List<string> categoryList = db.Category.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (ListItem item in dd_category_edit.Items) {
            if (categoryList.Contains(item.Value)) {
                item.Selected = true;
            }
            else {
                item.Selected = false;
            }
        }

        dd_allowresize_edit.SelectedIndex = db.AllowResize ? 0 : 1;
        dd_allowmax_edit.SelectedIndex = db.AllowMaximize ? 0 : 1;

        dd_allow_params_edit.SelectedIndex = db.AllowParams ? 0 : 1;
        dd_allowpopout_edit.SelectedIndex = db.AllowPopOut ? 0 : 1;

        dd_enablebg_edit.SelectedIndex = db.CssClass.ToLower() == "app-main" ? 0 : 1;
        if (db.CssClass.ToLower() == "app-main") {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#backgroundcolorholder_edit').show();");
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#backgroundcolorholder_edit').hide();");
        }

        dd_maxonload_edit.SelectedIndex = db.AutoFullScreen ? 0 : 1;
        dd_autoOpen_edit.SelectedIndex = db.AutoOpen ? 0 : 1;
        dd_allowNotifications_edit.SelectedIndex = db.AllowNotifications ? 0 : 1;
        dd_allowUserOverrides_edit.SelectedIndex = db.AllowUserOverrides ? 0 : 1;

        if (db.CreatedBy.ToLower() == CurrentUsername.ToLower() || IsUserNameEqualToAdmin()) {
            dd_isPrivate_Edit.SelectedIndex = db.IsPrivate ? 0 : 1;
            div_isprivate_edit.Visible = true;
        }
        else {
            dd_isPrivate_Edit.SelectedIndex = 1;
            div_isprivate_edit.Visible = false;
        }

        dd_defaultworkspace_edit.Items.Clear();

        string appColor = db.AppBackgroundColor.Replace("#", "");
        if (string.IsNullOrEmpty(appColor)) {
            appColor = "FFFFFF";
        }

        if (appColor == "inherit") {
            tb_backgroundColor_edit.Text = "#FFFFFF";
            cb_backgroundColor_edit_default.Checked = true;
        }
        else {
            tb_backgroundColor_edit.Text = HelperMethods.CreateFormattedHexColor(appColor);
            cb_backgroundColor_edit_default.Checked = false;
        }

        string iconColor = db.IconBackgroundColor.Replace("#", "");
        if (string.IsNullOrEmpty(iconColor)) {
            iconColor = "FFFFFF";
        }

        if (iconColor == "inherit") {
            tb_iconColor_edit.Text = "#FFFFFF";
            cb_iconColor_edit_default.Checked = true;
        }
        else {
            tb_iconColor_edit.Text = HelperMethods.CreateFormattedHexColor(iconColor);
            cb_iconColor_edit_default.Checked = false;
        }

        int totalWorkspaces = MainServerSettings.TotalWorkspacesAllowed;
        for (int i = 0; i < totalWorkspaces; i++) {
            string iStr = (i + 1).ToString();
            dd_defaultworkspace_edit.Items.Add(new ListItem(iStr, iStr));
        }

        for (int i = 1; i <= totalWorkspaces; i++) {
            if (i.ToString() == db.DefaultWorkspace) {
                dd_defaultworkspace_edit.SelectedIndex = i - 1;
                break;
            }
        }

        tb_minheight_edit.Text = db.MinHeight;
        tb_minwidth_edit.Text = db.MinWidth;
        tb_allowpopout_edit.Text = db.PopOutLoc;

        tb_minwidth_edit.Enabled = true;
        tb_minheight_edit.Enabled = true;
        dd_allowresize_edit.Enabled = true;
        dd_allowmax_edit.Enabled = true;
        if (db.AutoFullScreen) {
            tb_minwidth_edit.Enabled = false;
            tb_minheight_edit.Enabled = false;
            dd_allowresize_edit.Enabled = false;
            dd_allowmax_edit.Enabled = false;
        }

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

        if (HelperMethods.IsValidAscxOrDllFile(db.filename)) {
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
                else {
                    cb_associatedOverlay.Items[i].Selected = false;
                }
            }
        }

        radioButton_FileList.Items.Clear();
        tb_filename_edit.Enabled = false;

        div_allowNotifications_edit.Visible = false;
        if (!HelperMethods.IsValidHttpBasedAppType(tb_filename_edit.Text)) {
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
                div_allowNotifications_edit.Visible = true;
            }
            else {
                changeLoadFile.Visible = false;
                tb_filename_edit.Visible = true;
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#iframe-appDownloader').hide();");
            changeLoadFile.Visible = false;
            tb_filename_edit.Visible = true;
            tb_filename_edit.Enabled = true;
        }

        changeLoadFile_holder.Visible = true;
        if (tb_filename_edit.Text.Contains("Custom_Tables/") || tb_filename_edit.Text.Contains("Database_Imports/")) {
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#iframe-appDownloader').hide();");
            changeLoadFile_holder.Visible = false;
            div_allowNotifications_edit.Visible = false;
        }

        LoadAppDownloadBtn();
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#wlmd_editor_holder').css('display', 'block');");
    }
    protected void btn_save_Click(object sender, EventArgs e) {
        Apps_Coll dt = CurrentAppObject.GetAppInformation(hf_appchange.Value);

        string newAppImage = dt.AppId.Replace("app-", string.Empty) + ".png";
        string currAppImagePath = ServerSettings.GetServerMapLocation + App.CreateFullAppIconPath(dt.filename, newAppImage).Replace("/", "\\");

        if (!string.IsNullOrEmpty(dt.Icon)) {
            newAppImage = App.GetAppIconNameOnly(dt.Icon);
            currAppImagePath = ServerSettings.GetServerMapLocation + dt.Icon.Replace("/", "\\");
        }

        if (newAppImage == App.DefaultAppIcon && (fu_image_edit.HasFile || !string.IsNullOrEmpty(tb_imageurl_edit.Text))) {
            newAppImage = dt.AppId.Replace("app-", string.Empty) + ".png";
            currAppImagePath = ServerSettings.GetServerMapLocation + App.DefaultAppIconLocation.Replace("/", "\\") + newAppImage;
        }

        if (fu_image_edit.HasFile) {
            _imageConverter.SaveNewImg(fu_image_edit.PostedFile, currAppImagePath);
            CurrentAppObject.UpdateAppImage(dt.AppId, newAppImage);
        }
        else if (!string.IsNullOrEmpty(tb_imageurl_edit.Text)) {
            try {
                _imageConverter.SaveNewImg(tb_imageurl_edit.Text, currAppImagePath);
                CurrentAppObject.UpdateAppImage(dt.AppId, newAppImage);

                tb_imageurl_edit.Text = string.Empty;
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
        CurrentAppObject.UpdateAutoLoad(dt.AppId, HelperMethods.ConvertBitToBoolean(dd_AutoLoad_edit.SelectedValue));

        CurrentAppObject.UpdateAutoFullScreen(dt.AppId, HelperMethods.ConvertBitToBoolean(dd_maxonload_edit.SelectedValue));
        CurrentAppObject.UpdateAllowParams(dt.AppId, HelperMethods.ConvertBitToBoolean(dd_allow_params_edit.SelectedValue));

        string categorySelectedVals = string.Empty;
        foreach (ListItem checkedItem in dd_category_edit.Items) {
            if (checkedItem.Selected) {
                categorySelectedVals += checkedItem.Value + ServerSettings.StringDelimiter;
            }
        }

        string appColor = tb_backgroundColor_edit.Text.Trim();
        if (cb_backgroundColor_edit_default.Checked) {
            appColor = "inherit";
        }
        else if (!appColor.StartsWith("#") && appColor != "inherit") {
            appColor = "#" + appColor;
        }

        string iconColor = tb_iconColor_edit.Text.Trim();
        if (cb_iconColor_edit_default.Checked) {
            iconColor = "inherit";
        }
        else if (!iconColor.StartsWith("#") && iconColor != "inherit") {
            iconColor = "#" + iconColor;
        }

        CurrentAppObject.UpdateCategory(dt.AppId, categorySelectedVals);
        CurrentAppObject.UpdateAppLocal(dt.AppId, title);
        CurrentAppObject.UpdateAppList(dt.AppId, title, ar, am, tb_about_edit.Text,
                                  tb_description_edit.Text, dd_enablebg_edit.SelectedValue, tb_minheight_edit.Text.Replace("px", ""),
                                  tb_minwidth_edit.Text.Replace("px", ""), HelperMethods.ConvertBitToBoolean(dd_allowpopout_edit.SelectedValue), tb_allowpopout_edit.Text.Trim(),
                                  HelperMethods.ConvertBitToBoolean(dd_autoOpen_edit.SelectedValue), dd_defaultworkspace_edit.SelectedValue, HelperMethods.ConvertBitToBoolean(dd_isPrivate_Edit.SelectedValue),
                                  HelperMethods.ConvertBitToBoolean(dd_allowUserOverrides_edit.SelectedValue), appColor, iconColor, HelperMethods.ConvertBitToBoolean(dd_allowNotifications_edit.SelectedValue));

        if (!string.IsNullOrEmpty(tb_filename_edit.Text.Trim()) && HelperMethods.IsValidHttpBasedAppType(tb_filename_edit.Text.Trim())) {
            CurrentAppObject.UpdateAppFilename(dt.AppId, tb_filename_edit.Text.Trim());
        }

        if (fu_image_edit.HasFile) {
            string rq = !string.IsNullOrEmpty(Request.QueryString["tab"])
                            ? "?tab=" + Request.QueryString["tab"] + "&date=" + ServerSettings.ServerDateTime.Ticks
                            : "?date=" + ServerSettings.ServerDateTime.Ticks;

            ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx" + rq);
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
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#wlmd_editor_holder').css('display', 'none');");
            Appchange();
        }

        BuildAppList();

        if (!IsUserNameEqualToAdmin()) {
            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
            aib.BuildAppsForUser(true);
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
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#wlmd_editor_holder').css('display', 'none');");
        Appchange();
    }
    private void LoadAppDownloadBtn() {
        string appName_dl = "$('#iframe-appDownloader').attr('src', '../iframes/AppDownloadBtn.aspx?id=" + hf_appchange.Value + "');";
        RegisterPostbackScripts.RegisterStartupScript(this, appName_dl);
    }
    private void SetStandardApps(string appId, string filename) {
        if (IsAStandardApp(appId)) {
            btn_delete.Enabled = false;
            btn_delete.Visible = false;

            if (!HelperMethods.IsValidHttpBasedAppType(filename)) {
                lb_editsource.Enabled = false;
                lb_editsource.Visible = false;
            }
        }
    }
    private bool IsAStandardApp(string appId) {
        if ((appId == "app-rssfeed") || (appId == "app-documents") || (appId == "app-alarmclock") ||
            (appId == "app-notepad") || (appId == "app-bookmarkviewer") || (appId == "app-chatsettings") || 
            (appId == "app-messageboard") || (appId == "app-twitterstation") || (appId == "app-personalcalendar")) {

                if (IsUserNameEqualToAdmin()) {
                    return false;
                }

                return true;
        }

        return false;
    }
    private bool CanShowDeleteAndEditSource(string fileName, string createdBy) {
        if (!IsUserInAdminRole()) {
            if (createdBy.ToLower() == CurrentUsername.ToLower()) {
                return true;
            }
        }
        else {
            return true;
        }

        return false;
    }

    #endregion


    #region App Source Edit

    protected void lb_editsource_Click(object sender, EventArgs e) {
        Edit_Controls.Enabled = true;
        Edit_Controls.Visible = true;
        string JavaScript = string.Empty;
        if (!string.IsNullOrEmpty(tb_filename_edit.Text)) {
            string additionalCode = string.Empty;
            if (File.Exists(ServerSettings.GetServerMapLocation + "Apps\\" + tb_filename_edit.Text)) {
                string srcode;
                using (var sr = new StreamReader(ServerSettings.GetServerMapLocation + "Apps\\" + tb_filename_edit.Text)) {
                    srcode = sr.ReadToEnd();
                }

                FileInfo fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps\\" + tb_filename_edit.Text);

                if (!MainServerSettings.LockFileManager) {
                    StringBuilder extLinks = new StringBuilder();
                    string dir = fi.Directory.Name;
                    if (dir != "Apps") {
                        string[] filesInDir = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Apps\\" + dir);
                        foreach (string fn in filesInDir) {
                            FileInfo fi_indir = new FileInfo(fn);
                            if ((fi_indir.Extension.ToLower() == ".css") || (fi_indir.Extension.ToLower() == ".js")) {
                                string parentFolder = fi_indir.FullName.Replace(fi_indir.Directory.Root.Name, "\\");
                                string parentFolder2 = HttpUtility.UrlEncode(parentFolder);
                                string editable = "false";
                                if (IsUserNameEqualToAdmin()) {
                                    editable = "true";
                                }
                                extLinks.Append("<a href='" + ResolveUrl("~/SiteTools/ServerMaintenance/FileManager.aspx?edit=" + editable + "&file=" + parentFolder2) + "' ");
                                extLinks.Append("class='margin-right-big margin-top-sml float-left' target='_blank'>" + fi_indir.Name + "</a>");
                            }
                        }

                        if (!string.IsNullOrEmpty(extLinks.ToString()))
                            links_externalCode.Text = extLinks.ToString();
                    }
                }

                JavaScript = "UnescapeJavascriptCode(" + HttpUtility.JavaScriptStringEncode(srcode, true) + ");" + additionalCode;
            }
        }
        lbl_currfile.Text = tb_filename_edit.Text;
        pnl_htmleditor.Style["display"] = "block";
        btn_backProp.Visible = false;
        string javascriptCode = "$('#App-element,#pnl_app_information, #pnl_app_EditList').hide();_editmode=1;loadingPopup.Message('Loading Controls. Please Wait...');" + JavaScript;
        RegisterPostbackScripts.RegisterStartupScript(this, javascriptCode);
    }

    protected void lbtn_close_Click(object sender, EventArgs e) {
        Edit_Controls.Enabled = false;
        Edit_Controls.Visible = false;
        btn_edit.Enabled = false;
        btn_edit.Visible = false;
        lb_editsource.Enabled = false;
        lb_editsource.Visible = false;
        links_externalCode.Text = string.Empty;
        var tinyscript = new StringBuilder();
        Appchange();
        pnl_htmleditor.Style["display"] = "none";
        btn_backProp.Visible = true;
        if (btn_save.Visible && ((btn_save.Enabled) || (btn_save_2.Enabled))) {
            btn_edit.Enabled = false;
            btn_edit.Visible = false;
        }
        tinyscript.AppendLine("$('#App-element').show();$('#pnl_app_information, #pnl_app_EditList').show();_editmode=2;RevertToProperties();");
        tinyscript.AppendLine("if ($('#wlmd_editor_holder').css('display') == 'block'){ $('#MainContent_wlmd_holder').hide(); $('#MainContent_btn_delete').hide(); }");
        RegisterPostbackScripts.RegisterStartupScript(this, tinyscript.ToString());
    }
    protected void hf_saveapp_Changed(object sender, EventArgs e) {
        var fi = new FileInfo(tb_filename_edit.Text);
        string editor = HttpUtility.UrlDecode(hidden_editor.Value.Trim());

        // Need to unescape the code one more time due to security reasons.
        editor = HttpUtility.UrlDecode(editor);
        string filename = tb_filename_edit.Text;
        if (filename.Contains("/")) {
            filename = filename.Replace("/", "\\");
        }
        File.WriteAllText(ServerSettings.GetServerMapLocation + "Apps\\" + filename, editor);
        Edit_Controls.Enabled = false;
        Edit_Controls.Visible = false;
        hidden_editor.Value = string.Empty;

        hf_saveapp.Value = string.Empty;
        links_externalCode.Text = string.Empty;
        var tinyscript = new StringBuilder();
        Appchange();
        btn_backProp.Visible = true;
        if (btn_save.Visible && ((btn_save.Enabled) || (btn_save_2.Enabled))) {
            btn_edit.Enabled = false;
            btn_edit.Visible = false;
        }
        tinyscript.AppendLine("$('#App-element').show();$('#pnl_app_information, #pnl_app_EditList').show();_editmode=2;RevertToProperties();");
        tinyscript.AppendLine("if ($('#wlmd_editor_holder').css('display') == 'block'){ $('#MainContent_wlmd_holder').hide(); $('#MainContent_btn_delete').hide(); }");
        RegisterPostbackScripts.RegisterStartupScript(this, tinyscript.ToString());
    }

    #endregion


    #region Create Buttons

    protected void btn_uploadnew_Click(object sender, EventArgs e) {
        bool cancontinue = false;
        bool appCreated = false;
        string randomString = MakeValidFileName(tb_filename_create.Text);

        try {
            if ((fu_uploadnew.HasFile) && (!string.IsNullOrEmpty(tb_appname.Text))) {
                var fi = new FileInfo(fu_uploadnew.FileName);
                if (!AppExists(tb_appname.Text.Trim(), "app-" + randomString)) {
                    #region Upload/Create Icon
                    string picname = App.DefaultAppIcon;
                    if (fu_image_create.HasFile) {
                        picname = randomString + ".png";
                        var fiImage = new FileInfo(fu_image_create.FileName);
                        if ((fiImage.Extension.ToLower() == ".png") || (fiImage.Extension.ToLower() == ".gif")
                            || (fiImage.Extension.ToLower() == ".jpg") || (fiImage.Extension.ToLower() == ".jpeg")) {
                            string filePath = ServerSettings.GetServerMapLocation + App.DefaultAppIconLocation.Replace("/", "\\") + picname;
                            cancontinue = _imageConverter.SaveNewImg(fu_image_create.PostedFile, filePath);
                        }
                        else {
                            SetReturnMessage("Error! The file extension " + fiImage.Extension + " is not allowed. Please visit the help page for more information.", true);
                            ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
                        }
                    }
                    else if (!string.IsNullOrEmpty(tb_imageurl.Text)) {
                        try {
                            string filePath = ServerSettings.GetServerMapLocation + App.DefaultAppIconLocation.Replace("/", "\\") + picname;
                            cancontinue = _imageConverter.SaveNewImg(tb_imageurl.Text, filePath);
                        }
                        catch {
                            cancontinue = false;
                        }
                    }
                    else
                        cancontinue = true;
                    #endregion

                    if (cancontinue) {
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

                                appCreated = true;
                                UploadAppParameters("", randomString, picname);
                                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&b=app_installed&isZip=true&appId=" + randomString + "&date=" + ServerSettings.ServerDateTime.Ticks);
                            }
                            else {
                                SetReturnMessage("Error! Must contain at least one valid file in zip.", true);
                                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
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
                            ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&b=app_installed&isZip=false&date=" + ServerSettings.ServerDateTime.Ticks);
                        }
                        else {
                            SetReturnMessage("Error! The file extension " + fi.Extension + " is not allowed. Please visit the help page for more information.", true);
                            ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
                        }
                    }
                }
                else {
                    SetReturnMessage("Error! App already exists", true);
                    ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
                }
            }
            else {
                SetReturnMessage("Error! No file to upload or missing app name.", true);
                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
            }
        }
        catch {
            if (appCreated) {
                SetReturnMessage("App created successfully", false);
                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&b=app_installed&isZip=true&appId=" + randomString + "&date=" + ServerSettings.ServerDateTime.Ticks);
            }
            else {
                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
            }
        }

        ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
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
            (fileExt.ToLower() == ".xlsx") || (fileExt.ToLower() == ".xls") || (fileExt.ToLower() == ".doc") || (fileExt.ToLower() == ".dll")) {
            return true;
        }

        return false;
    }

    protected void btn_createEasy_Click(object sender, EventArgs e) {
        bool appCreated = false;

        try {
            string htmllink = tb_html_create.Text;
            if (tb_html_create.Text.Substring(0, 4) != "http")
                htmllink = "http://" + htmllink;

            var uri = new Uri(htmllink);
            if (uri.IsAbsoluteUri) {
                if ((!string.IsNullOrEmpty(tb_appname.Text)) && (!string.IsNullOrEmpty(tb_html_create.Text))) {
                    newAppID = MakeValidFileName(tb_filename_create.Text).ToLower();
                    string appId = "app-" + newAppID;
                    tb_appname.Text = tb_appname.Text.Trim();
                    if (!string.IsNullOrEmpty(tb_appname.Text)) {
                        if (!AppExists(tb_appname.Text, appId)) {
                            bool cancontinue = true;
                            bool canCreateNewImg = true;

                            #region Upload/Create Icon
                            string picname = App.DefaultAppIcon;
                            if (fu_image_create.HasFile) {
                                picname = newAppID + ".png";
                                FileInfo fiImage = new FileInfo(fu_image_create.FileName);
                                if ((fiImage.Extension.ToLower() == ".png") || (fiImage.Extension.ToLower() == ".gif")
                                    || (fiImage.Extension.ToLower() == ".jpg") || (fiImage.Extension.ToLower() == ".jpeg")) {
                                        string filePath = ServerSettings.GetServerMapLocation + App.DefaultAppIconLocation.Replace("/", "\\") + picname;
                                    cancontinue = _imageConverter.SaveNewImg(fu_image_create.PostedFile, filePath);
                                }
                                else {
                                    SetReturnMessage("Error! The file extension " + fiImage.Extension + " is not allowed. Please visit the help page for more information.", true);
                                    ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
                                }
                            }
                            else if (!string.IsNullOrEmpty(tb_imageurl.Text)) {
                                try {
                                    string filePath = ServerSettings.GetServerMapLocation + App.DefaultAppIconLocation.Replace("/", "\\") + picname;
                                    cancontinue = _imageConverter.SaveNewImg(tb_imageurl.Text, filePath);
                                    canCreateNewImg = false;
                                }
                                catch { }
                            }
                            #endregion

                            if (cancontinue) {
                                var di = new DirectoryInfo(ServerSettings.GetServerMapLocation + "Apps\\");
                                if (canCreateNewImg) {
                                    if ((htmllink.Contains("http://www.youtube")) || (htmllink.Contains("www.youtube"))
                                        || (htmllink.Contains("http://www.youtube.com/watch")) ||
                                        (htmllink.Contains("www.youtube.com/watch"))) {
                                        var uriYt = new Uri(htmllink);
                                        string queryString = uriYt.Query.Replace("?v=", "");
                                        if (uriYt.Query.IndexOf('&') != -1)
                                            queryString = queryString.Replace(uriYt.Query.Substring(uriYt.Query.IndexOf('&')), "");

                                        string imgfilePath = ServerSettings.GetServerMapLocation + App.DefaultAppIconLocation.Replace("/", "\\") + picname;
                                        _imageConverter.SaveNewImg("http://img.youtube.com/vi/" + queryString + "/default.jpg", imgfilePath);
                                    }
                                    else {
                                        try {
                                            string imgfilePath = ServerSettings.GetServerMapLocation + App.DefaultAppIconLocation.Replace("/", "\\") + picname;
                                            _imageConverter.SaveNewImg(htmllink + "/favicon.ico", imgfilePath);
                                            picname = newAppID + ".png";
                                        }
                                        catch { }
                                    }
                                }

                                bool allowparams = HelperMethods.ConvertBitToBoolean(dd_allow_params.SelectedValue);
                                bool allowpopout = HelperMethods.ConvertBitToBoolean(dd_allowpopout_create.SelectedValue);
                                string popoutloc = tb_popoutLoc_create.Text.Trim();
                                bool autoOpen = HelperMethods.ConvertBitToBoolean(dd_autoOpen_create.SelectedValue);
                                bool allowNotifications = HelperMethods.ConvertBitToBoolean(dd_allowNotifications_create.SelectedValue);

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

                                bool _allowUserOverride = HelperMethods.ConvertBitToBoolean(dd_allowUserOverrides.SelectedValue);

                                string appColor = tb_backgroundColor_create.Text.Trim();
                                if (!appColor.StartsWith("#")) {
                                    appColor = "#" + appColor;
                                }

                                string iconColor = tb_iconColor_create.Text.Trim();
                                if (!iconColor.StartsWith("#")) {
                                    iconColor = "#" + iconColor;
                                }

                                string appDescription = tb_description_create.Text.Trim();
                                string aboutApp = tb_about_create.Text.Trim();
                                if (string.IsNullOrEmpty(aboutApp)) {
                                    string siteName = ServerSettings.SiteName;
                                    if (!string.IsNullOrEmpty(siteName)) {
                                        siteName += " | ";
                                    }

                                    aboutApp = siteName + DateTime.Now.Year.ToString();
                                }

                                CurrentAppObject.CreateItem(appId, tb_appname.Text,
                                                    ConvertUrlsToLinks(htmllink), picname,
                                                    dd_allowresize_create.SelectedValue, dd_allowmax_create.SelectedValue,
                                                    aboutApp, appDescription,
                                                    dd_enablebg_create.SelectedValue, categorySelectedVals,
                                                    tb_minheight_create.Text.Replace("px", ""), tb_minwidth_create.Text.Replace("px", ""), allowparams,
                                                    allowpopout, popoutloc, oIds, autoOpen, dd_defaultworkspace_create.SelectedValue, _isPrivate,
                                                    _allowUserOverride, appColor, iconColor, allowNotifications);

                                bool maxonload = HelperMethods.ConvertBitToBoolean(dd_maxonload_create.SelectedValue);
                                CurrentAppObject.UpdateAutoFullScreen(appId, maxonload);

                                foreach (ListItem checkedItem in dd_package.Items) {
                                    if (checkedItem.Selected) {
                                        var package = new AppPackages(false);
                                        string list = package.GetAppList_nonarray(checkedItem.Value);
                                        list += appId + ",";
                                        package.updateAppList(checkedItem.Value, list, CurrentUsername);
                                    }
                                }

                                if (cb_InstallAfterLoad.Checked) {
                                    CurrentUserMemberDatabase.UpdateEnabledApps(appId);
                                }

                                appCreated = true;
                                SetReturnMessage("App created successfully", false);
                                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=easycreate&b=app_created&date=" + ServerSettings.ServerDateTime.Ticks);
                            }
                        }
                        else {
                            SetReturnMessage("Error! App already exists", true);
                            ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
                        }
                    }
                    else {
                        SetReturnMessage("Error! App already exists", true);
                        ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
                    }
                }
                else {
                    SetReturnMessage("Error! Must have a filename and app name defined.", true);
                    ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
                }
            }
            else {
                SetReturnMessage("Error! HTML Link MUST be a well formed url.", true);
                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
            }
        }
        catch {
            if (appCreated) {
                SetReturnMessage("App created successfully", false);
                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=easycreate&b=app_created&date=" + ServerSettings.ServerDateTime.Ticks);
            }
            else {
                ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
            }
        }

        ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
    }
    private void UploadAppParameters(string filename, string fi, string picname) {
        bool allowparams = HelperMethods.ConvertBitToBoolean(dd_allow_params.SelectedValue);
        bool allowpopout = HelperMethods.ConvertBitToBoolean(dd_allowpopout_create.SelectedValue);
        string popoutloc = tb_popoutLoc_create.Text.Trim();
        bool autoOpen = HelperMethods.ConvertBitToBoolean(dd_autoOpen_create.SelectedValue);
        bool allowNotifications = HelperMethods.ConvertBitToBoolean(dd_allowNotifications_create.SelectedValue);

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

        bool _allowUserOverride = HelperMethods.ConvertBitToBoolean(dd_allowUserOverrides.SelectedValue);

        string appColor = tb_backgroundColor_create.Text.Trim();
        if (!appColor.StartsWith("#")) {
            appColor = "#" + appColor;
        }
        if (cb_backgroundColor_create_default.Checked) {
            appColor = "inherit";
        }

        string iconColor = tb_iconColor_create.Text.Trim();
        if (!iconColor.StartsWith("#")) {
            iconColor = "#" + iconColor;
        }
        if (cb_iconColor_create_default.Checked) {
            iconColor = "inherit";
        }

        string appDescription = tb_description_create.Text.Trim();
        string aboutApp = tb_about_create.Text.Trim();
        if (string.IsNullOrEmpty(aboutApp)) {
            string siteName = ServerSettings.SiteName;
            if (!string.IsNullOrEmpty(siteName)) {
                siteName += " | ";
            }

            aboutApp = siteName + DateTime.Now.Year.ToString();
        }

        string appId = "app-" + fi;
        CurrentAppObject.CreateItem(appId, tb_appname.Text, filename, picname,
                            dd_allowresize_create.SelectedValue, dd_allowmax_create.SelectedValue,
                            aboutApp, appDescription,
                            dd_enablebg_create.SelectedValue, categorySelectedVals,
                            tb_minheight_create.Text, tb_minwidth_create.Text, allowparams,
                            allowpopout, popoutloc, oIds, autoOpen, dd_defaultworkspace_create.SelectedValue,
                            _isPrivate, _allowUserOverride, appColor, iconColor, allowNotifications);
        bool maxonload = HelperMethods.ConvertBitToBoolean(dd_maxonload_create.SelectedValue);
        CurrentAppObject.UpdateAutoFullScreen(appId, maxonload);

        foreach (ListItem checkedItem in dd_package.Items) {
            if (checkedItem.Selected) {
                var package = new AppPackages(false);
                string list = package.GetAppList_nonarray(checkedItem.Value);
                list += appId + ",";
                package.updateAppList(checkedItem.Value, list, CurrentUsername);
            }
        }

        if (cb_InstallAfterLoad.Checked) {
            CurrentUserMemberDatabase.UpdateEnabledApps(appId);
        }
    }

    private string ConvertUrlsToLinks(string htmllink) {
        if ((htmllink.Contains("http://www.youtube")) || (htmllink.Contains("www.youtube"))
            || (htmllink.Contains("http://www.youtube.com/watch")) || (htmllink.Contains("www.youtube.com/watch"))) {
            return ConvertToObjectEmbeded(htmllink);
        }

        return htmllink;
    }
    private static string ConvertToObjectEmbeded(string htmllink) {
        string[] del = { "?v=" };
        string[] vidid = htmllink.Split(del, StringSplitOptions.RemoveEmptyEntries);
        if (vidid.Length > 0) {
            return "http://www.youtube.com/embed/" + vidid[1] + "?autoplay=0";
        }

        return htmllink;
    }
    protected void btn_clear_controls_Click(object sender, EventArgs e) {
        string rq = !string.IsNullOrEmpty(Request.QueryString["tab"])
                        ? "?tab=" + Request.QueryString["tab"] + "&date=" + ServerSettings.ServerDateTime.Ticks
                        : "?date=" + ServerSettings.ServerDateTime.Ticks;

        ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx" + rq);
    }
    private bool AppExists(string name, string id) {
        string _name = CurrentAppObject.GetAppID(name);
        string _id = CurrentAppObject.GetAppName(id);
        if ((!string.IsNullOrEmpty(_id)) || (!string.IsNullOrEmpty(_name)))
            return true;

        return false;
    }

    #endregion


    #region App Delete Password Check

    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(hf_appdeleteid.Value)) return;

        string createdBy = CurrentAppObject.GetAppCreatedBy(hf_appdeleteid.Value);

        bool isGood = false;

        if (CurrentUserMemberDatabase.IsSocialAccount && createdBy.ToLower() == CurrentUsername.ToLower()) {
            isGood = true;
        }
        else {
            MembershipUser u = Membership.GetUser(createdBy);

            string passwordUser = ServerSettings.AdminUserName;
            if (!IsUserNameEqualToAdmin() && u != null) {
                passwordUser = u.UserName;
            }

            if (IsUserNameEqualToAdmin(passwordUser)) {
                bool userLockedOut = MemberDatabase.CheckIfUserIsLockedOut(ServerSettings.AdminUserName);
                isGood = Membership.ValidateUser(ServerSettings.AdminUserName, tb_passwordConfirm.Text);
                MemberDatabase.UnlockUserIfNeeded(userLockedOut, ServerSettings.AdminUserName);
            }
            else if (!string.IsNullOrEmpty(createdBy) && createdBy.ToLower() == CurrentUsername.ToLower()) {
                bool userLockedOut = MemberDatabase.CheckIfUserIsLockedOut(passwordUser);
                isGood = Membership.ValidateUser(passwordUser, tb_passwordConfirm.Text);
                MemberDatabase.UnlockUserIfNeeded(userLockedOut, passwordUser);
            }
        }

        if (isGood) {
            RegisterPostbackScripts.RegisterStartupScript(this, "BeginWork();");
        }
        else {
            if (CurrentUserMemberDatabase.IsSocialAccount) {
                RegisterPostbackScripts.RegisterStartupScript(this, "loadingPopup.RemoveMessage();openWSE.AlertWindow('You are not authorized to delete this app.');");
            }
            else {
                tb_passwordConfirm.Text = "";
                RegisterPostbackScripts.RegisterStartupScript(this, "loadingPopup.RemoveMessage();openWSE.AlertWindow('Password is invalid');");
            }
        }
    }
    protected void hf_StartDelete_Changed(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(hf_appdeleteid.Value)) return;

        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (var member in from MembershipUser u in coll select new MemberDatabase(u.UserName)) {
            member.RemoveEnabledApp(hf_appdeleteid.Value);
        }

        string name = CurrentAppObject.GetAppName(hf_appdeleteid.Value);
        string icon = CurrentAppObject.GetAppIconName(hf_appdeleteid.Value);

        if (!string.IsNullOrEmpty(icon)) {
            icon = icon.ToLower();
            if (icon != App.DefaultAppIconLocation.ToLower() + App.DefaultAppIcon.ToLower() && icon != App.DefaultAppIconLocation.ToLower() + DBImporter.DefaultDatabaseIcon.ToLower()) {
                if (File.Exists(ServerSettings.GetServerMapLocation + "\\" + icon)) {
                    try {
                        File.Delete(ServerSettings.GetServerMapLocation + "\\" + icon);
                    }
                    catch { }
                }
            }
        }

        CurrentAppObject.DeleteAppComplete(hf_appdeleteid.Value, ServerSettings.GetServerMapLocation);
        CurrentAppObject.DeleteAppLocal(hf_appdeleteid.Value);

        var wp = new AppPackages(true);
        foreach (Dictionary<string, string> dr in wp.listdt) {
            string[] l = wp.GetAppList(dr["ID"]);
            string tempL = l.Where(w => w.ToLower() != hf_appdeleteid.Value.ToLower())
                            .Aggregate(string.Empty, (current, w) => current + (w + ","));

            wp.updateAppList(dr["ID"], tempL, CurrentUsername);
        }

        string rq = "";
        if (!string.IsNullOrEmpty(Request.QueryString["tab"])) {
            rq = "?tab=" + Request.QueryString["tab"];
        }

        ServerSettings.PageIFrameRedirect(this.Page, "AppManager.aspx" + rq);
    }

    #endregion


    #region Template Download

    protected void lbtn_aspxTemplate_Click(object sender, EventArgs e) {
        string realFilePath = ServerSettings.GetServerMapLocation + "App_Data\\App_Templates\\aspx_template.zip";
        if (File.Exists(realFilePath)) {
            try {
                string strFileName = "Aspx_Template.zip";
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + strFileName);
                Response.Clear();
                Response.TransmitFile(realFilePath);
                Response.Flush();
                Response.End();
            }
            catch (Exception ex) {
                AppLog.AddError(ex);
            }
        }
        else {
            ServerSettings.RefreshPage(Page, string.Empty);
        }
    }
    protected void lbtn_ascxTemplate_Click(object sender, EventArgs e) {
        string realFilePath = ServerSettings.GetServerMapLocation + "App_Data\\App_Templates\\ascx_template.zip";
        if (File.Exists(realFilePath)) {
            try {
                string strFileName = "Ascx_Template.zip";
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + strFileName);
                Response.Clear();
                Response.TransmitFile(realFilePath);
                Response.Flush();
                Response.End();
            }
            catch (Exception ex) {
                AppLog.AddError(ex);
            }
        }
        else {
            ServerSettings.RefreshPage(Page, string.Empty);
        }
    }

    #endregion

}