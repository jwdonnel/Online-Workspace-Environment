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
using OpenWSE.Core.Licensing;

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
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                _username = userId.Name;
                HelperMethods.SetIsSocialUserForDeleteItems(Page, _username);
                _apps = new App(_username, false);

                bool lockAppCreator = false;
                if (_ss.LockAppCreator) {
                    lockAppCreator = true;
                    pnl_app_information.Attributes.Add("data-appcreatorlocked", "true");
                }

                if (!IsPostBack) {
                    PageLoadInit.BuildLinks(pnlLinkBtns, _username, this.Page);
                    BuildDefaultWorkspaceDropDown();
                }

                SetReturnMessage_OnLoad();

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

                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                    cb_InstallAfterLoad.Enabled = false;
                    cb_InstallAfterLoad.Visible = false;
                    cb_isPrivate.Enabled = false;
                    cb_isPrivate.Visible = false;
                    cb_InstallAfterLoad.Checked = false;
                    cb_isPrivate.Checked = false;
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

                if (!Roles.IsUserInRole(userId.Name, ServerSettings.AdminUserName)) {
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

                                            RegisterPostbackScripts.RegisterStartupScript(this, "$(window).load(function () { LoadNewDefaultPageSelector(); });");
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
                    pnl_searchwrapper.Enabled = false;
                    pnl_searchwrapper.Visible = false;
                    pnl_appcount.Enabled = false;
                    pnl_appcount.Visible = false;
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
                    pnl_searchwrapper.Enabled = false;
                    pnl_searchwrapper.Visible = false;
                    pnl_appcount.Enabled = false;
                    pnl_appcount.Visible = false;
                    app_templatebtn.Visible = true;

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
                    pnl_searchwrapper.Enabled = false;
                    pnl_searchwrapper.Visible = false;
                    pnl_appcount.Enabled = false;
                    pnl_appcount.Visible = false;

                    hf_isParams.Value = "1";

                    if (!_ss.LockAppCreator) {
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

                    App tempApp = new App(string.Empty, false);
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
                            else if (icon != App.DefaultAppIcon.ToLower()) {
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
                    if (!string.IsNullOrEmpty(Request.QueryString["tab"]))
                        rq = "&tab=" + Request.QueryString["tab"];

                    ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?date=" + ServerSettings.ServerDateTime.Ticks + rq);
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

        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
            string groupId = GroupSessions.GetUserGroupSessionName(_username);
            NewUserDefaults defaults = new NewUserDefaults(groupId);
            string packageId = defaults.GetDefault("AppPackage");
            foreach (ListItem item in dd_package.Items) {
                if (item.Value == packageId) {
                    item.Selected = true;
                    break;
                }
            }
        }

        if (!ServerSettings.AdminPagesCheck(Page.ToString(), _username) || GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
            pnl_AppPackage.Enabled = false;
            pnl_AppPackage.Visible = false;
        }
    }
    private void BuildDefaultWorkspaceDropDown() {
        dd_defaultworkspace_create.Items.Clear();
        int totalWorkspaces = _ss.TotalWorkspacesAllowed;
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
        pnl_AppList.Controls.Clear();

        StringBuilder str = new StringBuilder();
        StringBuilder strJavascript = new StringBuilder();

        App appItem = new App(_username, false);
        appItem.GetAllApps();

        string appIconFolder = ResolveUrl("~/Standard_Images/App_Icons/");

        AppPackages packages = new AppPackages(false);
        AppRatings ratings = new AppRatings(_username);

        MemberDatabase _member = new MemberDatabase(_username);

        int count = 0;
        bool AssociateWithGroups = _ss.AssociateWithGroups;

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td>");
        str.Append("<td style='width: 150px;'>App Name</td>");
        if (!_ss.HideAllAppIcons && !_member.HideAppIcon) {
            str.Append("<td style='width: 45px;'>Image</td>");
        }
        else {
            str.Append("<td style='width: 45px;'>Color</td>");
        }

        str.Append("<td>Description</td>");
        str.Append("<td width='200px'>Created By</td>");
        str.Append("<td width='250px'>Category</td>");
        if (_ss.AllowAppRating) {
            str.Append("<td width='110px'>Rating</td>");
        }
        str.Append("<td width='75px'>Actions</td></tr>");

        foreach (Apps_Coll coll in appItem.AppList) {
            bool cancontinue = false;

            if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                cancontinue = true;
            }
            else {
                if (coll.CreatedBy.ToLower() == _username.ToLower()) {
                    cancontinue = true;
                }
            }

            if ((coll.CreatedBy.ToLower() != _username.ToLower()) && (coll.IsPrivate) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                cancontinue = false;
            }

            if (cancontinue) {
                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckAppGroupAssociation(coll, _member)) {
                        continue;
                    }
                }
                if (coll != null && !string.IsNullOrEmpty(coll.ID)) {
                    if (CanAddAppInList(coll)) {
                        string deleteBtn = string.Format("<a href='#' title='Delete App' class='td-delete-btn margin-left' onclick=\"OnDelete('{0}');return false;\"></a>", coll.AppId);
                        if (IsAStandardApp(coll.AppId)) {
                            deleteBtn = string.Empty;
                        }

                        string editBtn = string.Format("<a href='#' title='Edit App' class='td-edit-btn' onclick=\"appchange('{0}');return false;\"></a>", coll.AppId);

                        string categoryIds = string.Empty;
                        string categoryNames = string.Empty;

                        AppCategory _category = new AppCategory(false);
                        Dictionary<string, string> categoryList = _category.BuildCategoryDictionary(coll.Category);

                        foreach (KeyValuePair<string, string> categoryPair in categoryList) {
                            categoryIds += categoryPair.Key + ",";
                            categoryNames += categoryPair.Value + ", ";
                        }

                        categoryNames = categoryNames.Trim();
                        if (categoryNames.EndsWith(",")) {
                            categoryNames = categoryNames.Remove(categoryNames.Length - 1);
                        }

                        string description = coll.Description;
                        if (!string.IsNullOrEmpty(description)) {
                            description += " - ";
                        }

                        description += coll.About;

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
                        str.Append("<tr class='myItemStyle GridNormalRow app-item-installer' data-category='" + categoryIds + "'>");
                        str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + count.ToString() + "</td>");
                        str.Append("<td align='left' class='border-right border-bottom'" + appColorCss + ">" + coll.AppName + "</td>");

                        string iconColor = coll.IconBackgroundColor;
                        if (!iconColor.StartsWith("#")) {
                            iconColor = "#" + iconColor;
                        }

                        if (!_ss.HideAllAppIcons && !_member.HideAppIcon) {
                            string img = "<img alt='' src='" + ResolveUrl("~/Standard_Images/App_Icons/" + coll.Icon) + "' style='height: 25px;' />";
                            str.Append("<td align='center' class='border-right border-bottom' style='" + HelperMethods.GetCSSGradientStyles(iconColor, string.Empty) + "'>" + img + "</td>");
                        }
                        else {
                            str.Append("<td align='center' class='border-right border-bottom' style='" + HelperMethods.GetCSSGradientStyles(iconColor, string.Empty) + "'></td>");
                        }

                        str.Append("<td align='left' class='border-right border-bottom'>" + description + "</td>");
                        str.Append("<td align='center' class='border-right border-bottom'>" + HelperMethods.MergeFMLNames(new MemberDatabase(coll.CreatedBy)) + "</td>");
                        str.Append("<td align='center' class='border-right border-bottom'>" + categoryNames + "</td>");
                        if (_ss.AllowAppRating) {
                            str.AppendFormat("<td align='left' class='border-right border-bottom rating-column'><div class='pad-left'><div class='app-rater-{0} app-rater-installer rounded-corners-2'></div></div></td>", coll.AppId);
                            strJavascript.AppendFormat("openWSE.RatingStyleInit('.app-rater-{0}', '" + ratings.GetAverageRating(coll.AppId) + "', true, '{0}', true);", coll.AppId);
                        }
                        str.Append("<td align='center' class='border-right border-bottom'>" + editBtn + deleteBtn + "</td></tr>");
                    }
                }
            }
        }

        lbl_AppsAvailable.Text = count.ToString();

        str.Append("</tbody></table></div>");
        if (count == 0) {
            str.Append("<div class='emptyGridView'>No Apps Found</div>");
        }

        if (!string.IsNullOrEmpty(strJavascript.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, strJavascript.ToString());
        }

        pnl_AppList.Controls.Add(new LiteralControl(str.ToString()));

        updatePnl_AppTotals.Update();
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
    protected void imgbtn_search_Click(object sender, EventArgs e) {
        BuildAppList();
    }
    private bool CanAddAppInList(Apps_Coll coll) {
        string search = tb_search.Text.Trim().ToLower();

        if (coll != null && !string.IsNullOrEmpty(coll.ID)) {
            if (!string.IsNullOrEmpty(search) && search != "search apps" && !coll.Description.ToLower().Contains(search) && !coll.AppName.ToLower().Contains(search) && !coll.About.ToLower().Contains(search)) {
                return false;
            }

            return true;
        }

        return false;
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
            str.Append("<div class='emptyGridView '>No Parameters Available for App</div>");
        }
        pnl_app_params_holder.Controls.Add(new LiteralControl(str.ToString()));
        RegisterPostbackScripts.RegisterStartupScript(this, "$(\".app-icon-parms[data-appid='" + appid + "']\").addClass(\"active\");");
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
            str.Append("<table class='myItemStyle GridNormalRow' style='width: 98%; min-width: 1100px;' cellpadding='0' cellspacing='0'><tr>");
            str.Append("<td width='35px' align='center' class='GridViewNumRow'>" + count + "</td>");
            if (dr["ID"] == parameter) {
                str.Append("<td style='border-right: 1px solid #CCC; min-width: 255px;'><input type='text' id='txt_appparam_edit' ");
                str.Append("class='TextBoxControls' value=\"" + dr["Parameter"] + "\" style='width:93%'></input></td>");
            }
            else {
                str.Append("<td style='border-right: 1px solid #CCC;'><span>" + dr["Parameter"] + "</span></td>");
            }

            string desc = dr["Description"];
            if (string.IsNullOrEmpty(desc)) {
                desc = "No description available";
            }

            if (dr["ID"] == parameter) {
                str.Append("<td width='350px' style='border-right: 1px solid #CCC;'><input type='text' id='txt_appparamdesc_edit' ");
                str.Append("class='TextBoxControls' value=\"" + dr["Description"] + "\" style='width:95%'></input></td>");
            }
            else {
                str.Append("<td width='350px' style='border-right: 1px solid #CCC;'>" + desc + "</td>");
            }

            str.Append("<td width='150px' align='center' style='border-right: 1px solid #CCC;'>" + dr["UserName"] + "</td>");
            str.Append("<td width='165px' align='center' style='border-right: 1px solid #CCC;'>" + dr["DateTime"] + "</td>");

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
        RegisterPostbackScripts.RegisterStartupScript(this, "$(\".app-icon-parms[data-appid='" + appid + "']\").addClass(\"active\");");
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

        string appParamVal = HttpUtility.UrlDecode(hf_appchange_params_update.Value);
        string appParamDesc = HttpUtility.UrlDecode(hf_appchange_paramsdesc_update.Value);

        if ((!string.IsNullOrEmpty(appParamVal)) && (!string.IsNullOrEmpty(hf_appchange_params_edit.Value))) {
            var appparams = new AppParams(false);
            appparams.updateParameter(hf_appchange_params_edit.Value, hf_appchange_params.Value, appParamVal.Trim(), appParamDesc.Trim(), _username);
            GetAppParams(hf_appchange_params.Value);
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
            GetAppParams(hf_appchange_params.Value);
        }
        hf_appchange_params_edit.Value = "";
        hf_appchange_params_cancel.Value = "";
    }
    public string BuildAppParmListEditor() {
        var app_List = new StringBuilder();
        var app_script = new StringBuilder();
        var apps = new App(_username, false);
        apps.GetAllApps();

        MemberDatabase _member = new MemberDatabase(_username);

        List<string> appListCount = new List<string>();
        bool AssociateWithGroups = _ss.AssociateWithGroups;
        foreach (Apps_Coll dr in apps.AppList) {
            bool cancontinue = false;

            if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                cancontinue = true;
            }
            else {
                if (dr.CreatedBy.ToLower() == _username.ToLower()) {
                    cancontinue = true;
                }
            }

            if ((dr.CreatedBy.ToLower() != _username.ToLower()) && (dr.IsPrivate) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                cancontinue = false;
            }

            if (cancontinue && dr.AllowParams) {
                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckAppGroupAssociation(dr, _member)) {
                        continue;
                    }
                }

                string w = dr.AppName;
                string id = dr.AppId;
                string iconname = dr.Icon;

                if (!string.IsNullOrEmpty(dr.filename)) {
                    var fi = new FileInfo(dr.filename);

                    string iconImg = "<img alt='' src='" + ResolveUrl("~/Standard_Images/App_Icons/" + iconname) + "' />";
                    if (_ss.HideAllAppIcons)
                        iconImg = string.Empty;

                    if (!appListCount.Contains(dr.ID)) {
                        app_script.Append("<div class='app-icon-parms rbbuttons' data-appid=\"" + dr.AppId + "\" title=\"View " + w + "'s parameters\" onclick=\"appchange('" + dr.AppId + "')\">");
                        app_script.Append(iconImg + "<span class='app-icon-font'>" + w + "</span></div>");
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
            if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
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
        if (ServerSettings.AdminPagesCheck("overlays.aspx", _username) && !GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
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
                if (!string.IsNullOrEmpty(Request.QueryString["tab"]))
                    rq = "&tab=" + Request.QueryString["tab"];

                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?date=" + ServerSettings.ServerDateTime.Ticks + rq);
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
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'App-element', 'App Details');openWSE.LoadingMessage1('Loading. Please Wait...');");
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
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.RatingStyleInit('.app-rater-" + hf_appchange.Value + "', '" + ratings.GetAverageRating(hf_appchange.Value) + "', true, '" + hf_appchange.Value + "', true);");
                }

                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    if (db.CreatedBy.ToLower() == _username.ToLower()) {
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
        Apps_Coll db = _apps.GetAppInformation(hf_appchange.Value);
        if (db == null) return;
        lb_editsource.Enabled = false;
        lb_editsource.Visible = false;
        LoadAppDownloadBtn();
        img_edit.ImageUrl = ServerSettings.GetSitePath(Request) + "/Standard_Images/App_Icons/" + db.Icon;

        if (_ss.HideAllAppIcons || new MemberDatabase(_username).HideAppIcon)
            img_edit.Visible = false;
        else
            img_edit.Visible = true;

        tb_title_edit.Text = db.AppName;

        string about = db.About;
        if (string.IsNullOrEmpty(about))
            tb_about_edit.Text = OpenWSE.Core.Licensing.CheckLicense.SiteName + " | " + ServerSettings.ServerDateTime.Year + " | " + "John Donnelly";
        else
            tb_about_edit.Text = about;

        string description = db.Description;
        tb_description_edit.Text = string.IsNullOrEmpty(description) ? "No description available" : description;

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

        dd_allowUserOverrides_edit.SelectedIndex = db.AllowUserOverrides ? 0 : 1;

        if (db.CreatedBy.ToLower() == _username.ToLower() || _username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
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

        tb_backgroundColor_edit.Text = appColor;

        string iconColor = db.IconBackgroundColor.Replace("#", "");
        if (string.IsNullOrEmpty(iconColor)) {
            iconColor = "FFFFFF";
        }

        tb_iconColor_edit.Text = iconColor;

        int totalWorkspaces = _ss.TotalWorkspacesAllowed;
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

        if (HelperMethods.IsValidAscxFile(db.filename)) {
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
        }

        LoadAppDownloadBtn();
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#wlmd_editor_holder').css('display', 'block');");
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

        string appColor = tb_backgroundColor_edit.Text.Trim();
        if (!appColor.StartsWith("#")) {
            appColor = "#" + appColor;
        }

        string iconColor = tb_iconColor_edit.Text.Trim();
        if (!iconColor.StartsWith("#")) {
            iconColor = "#" + iconColor;
        }

        _apps.UpdateCategory(dt.AppId, categorySelectedVals);
        _apps.UpdateAppLocal(dt.AppId, title);
        _apps.UpdateAppList(dt.AppId, title, ar, am, tb_about_edit.Text,
                                  tb_description_edit.Text, dd_enablebg_edit.SelectedValue, tb_minheight_edit.Text.Replace("px", ""),
                                  tb_minwidth_edit.Text.Replace("px", ""), HelperMethods.ConvertBitToBoolean(dd_allowpopout_edit.SelectedValue), tb_allowpopout_edit.Text.Trim(),
                                  HelperMethods.ConvertBitToBoolean(dd_autoOpen_edit.SelectedValue), dd_defaultworkspace_edit.SelectedValue, HelperMethods.ConvertBitToBoolean(dd_isPrivate_Edit.SelectedValue),
                                  HelperMethods.ConvertBitToBoolean(dd_allowUserOverrides_edit.SelectedValue), appColor, iconColor);

        if (File.Exists(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\" + dt.AppId.Replace("app-", "") + ".ascx")) {
            var db = new DBImporter();
            DBImporter_Coll dbColl = db.GetImportTableByTableId(dt.AppId.Replace("app-", ""));
            if (!string.IsNullOrEmpty(dbColl.ID)) {
                db.UpdateEntry(dbColl.ID, title, dbColl.Description, dbColl.SelectCommand, dbColl.AllowEdit, dbColl.NotifyUsers);
            }
        }

        if (!string.IsNullOrEmpty(tb_filename_edit.Text.Trim()) && HelperMethods.IsValidHttpBasedAppType(tb_filename_edit.Text.Trim())) {
            _apps.UpdateAppFilename(dt.AppId, tb_filename_edit.Text.Trim());
        }

        if (fu_image_edit.HasFile) {
            string rq = !string.IsNullOrEmpty(Request.QueryString["tab"])
                            ? "?tab=" + Request.QueryString["tab"] + "&date=" + ServerSettings.ServerDateTime.Ticks
                            : "?date=" + ServerSettings.ServerDateTime.Ticks;

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
            RegisterPostbackScripts.RegisterStartupScript(this, "$('#wlmd_editor_holder').css('display', 'none');");
            Appchange();
        }

        BuildAppList();

        if (_username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            MemberDatabase _member = new MemberDatabase(_username);
            AppIconBuilder aib = new AppIconBuilder(Page, _member);
            aib.BuildAppsForUser();
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
        if ((appId == "app-rssfeed") || (appId == "app-documents") || (appId == "app-feedback") ||
            (appId == "app-notepad") || (appId == "app-bookmarkviewer") || (appId == "app-googletraffic") ||
            (appId == "app-chatsettings") || (appId == "app-messageboard") || (appId == "app-twitterstation") ||
            (appId == "app-personalcalendar") || (appId == "app-alarmclock")) {

                if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    return false;
                }

                return true;
        }

        return false;
    }
    private bool CanShowDeleteAndEditSource(string fileName, string createdBy) {
        if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            if (createdBy.ToLower() == _username.ToLower()) {
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

                if (!_ss.LockFileManager) {
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
                                if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
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
        string javascriptCode = "$('#App-element,#pnl_app_information, #pnl_app_EditList, #pnl_searchwrapper, #pnl_appcount').hide();_editmode=1;openWSE.LoadingMessage1('Loading Controls. Please Wait...');" + JavaScript;
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
        tinyscript.AppendLine("$('#App-element').show();$('#pnl_app_information, #pnl_app_EditList, #pnl_searchwrapper, #pnl_appcount').show();_editmode=2;RevertToProperties();");
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
        tinyscript.AppendLine("$('#App-element').show();$('#pnl_app_information, #pnl_app_EditList, #pnl_searchwrapper, #pnl_appcount').show();_editmode=2;RevertToProperties();");
        tinyscript.AppendLine("if ($('#wlmd_editor_holder').css('display') == 'block'){ $('#MainContent_wlmd_holder').hide(); $('#MainContent_btn_delete').hide(); }");
        RegisterPostbackScripts.RegisterStartupScript(this, tinyscript.ToString());
    }

    #endregion


    #region Create Buttons

    protected void btn_uploadnew_Click(object sender, EventArgs e) {
        bool cancontinue = false;
        bool appCreated = false;
        string randomString = string.Empty;

        try {
            if ((fu_uploadnew.HasFile) && (!string.IsNullOrEmpty(tb_appname.Text))) {
                var fi = new FileInfo(fu_uploadnew.FileName);
                if (!AppExists(tb_appname.Text.Trim(), "app-" + MakeValidFileName(tb_filename_create.Text))) {
                    #region Upload/Create Icon
                    string picname = App.DefaultAppIcon.ToLower();
                    if (fu_image_create.HasFile) {
                        var fiImage = new FileInfo(fu_image_create.FileName);
                        if ((fiImage.Extension.ToLower() == ".png") || (fiImage.Extension.ToLower() == ".gif")
                            || (fiImage.Extension.ToLower() == ".jpg") || (fiImage.Extension.ToLower() == ".jpeg")) {
                            picname = fu_image_create.FileName;
                            string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + fu_image_create.FileName;
                            cancontinue = _imageConverter.SaveNewImg(fu_image_create.PostedFile, filePath);
                        }
                        else {
                            SetReturnMessage("Error! The file extension " + fiImage.Extension + " is not allowed. Please visit the help page for more information.", true);
                            ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
                        }
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
                        randomString = tb_filename_create.Text;
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
                                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&b=app_installed&isZip=true&appId=" + randomString + "&date=" + ServerSettings.ServerDateTime.Ticks);
                            }
                            else {
                                SetReturnMessage("Error! Must contain at least one valid file in zip.", true);
                                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
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
                            ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&b=app_installed&isZip=false&date=" + ServerSettings.ServerDateTime.Ticks);
                        }
                        else {
                            SetReturnMessage("Error! The file extension " + fi.Extension + " is not allowed. Please visit the help page for more information.", true);
                            ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
                        }
                    }
                }
                else {
                    SetReturnMessage("Error! App already exists", true);
                    ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
                }
            }
            else {
                SetReturnMessage("Error! No file to upload or missing app name.", true);
                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
            }
        }
        catch {
            if (appCreated) {
                SetReturnMessage("App created successfully", false);
                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&b=app_installed&isZip=true&appId=" + randomString + "&date=" + ServerSettings.ServerDateTime.Ticks);
            }
            else {
                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
            }
        }

        ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=upload&date=" + ServerSettings.ServerDateTime.Ticks);
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
                            string picname = App.DefaultAppIcon.ToLower();
                            if (fu_image_create.HasFile) {
                                var fiImage = new FileInfo(fu_image_create.FileName);
                                if ((fiImage.Extension.ToLower() == ".png") || (fiImage.Extension.ToLower() == ".gif")
                                    || (fiImage.Extension.ToLower() == ".jpg") || (fiImage.Extension.ToLower() == ".jpeg")) {
                                    picname = fu_image_create.FileName;
                                    string filePath = ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + fu_image_create.FileName;
                                    cancontinue = _imageConverter.SaveNewImg(fu_image_create.PostedFile, filePath);
                                }
                                else {
                                    SetReturnMessage("Error! The file extension " + fiImage.Extension + " is not allowed. Please visit the help page for more information.", true);
                                    ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
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
                                var di = new DirectoryInfo(ServerSettings.GetServerMapLocation + "Apps\\");
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
                                if (string.IsNullOrEmpty(appDescription)) {
                                    appDescription = "No description available";
                                }

                                string aboutApp = tb_about_create.Text.Trim();
                                if (string.IsNullOrEmpty(aboutApp)) {
                                    aboutApp = CheckLicense.SiteName + " | " + DateTime.Now.Year.ToString() + " | John Donnelly";
                                }

                                _apps.CreateItem(appId, tb_appname.Text,
                                                    ConvertUrlsToLinks(htmllink), picname,
                                                    dd_allowresize_create.SelectedValue, dd_allowmax_create.SelectedValue,
                                                    aboutApp, appDescription,
                                                    dd_enablebg_create.SelectedValue, categorySelectedVals,
                                                    tb_minheight_create.Text.Replace("px", ""), tb_minwidth_create.Text.Replace("px", ""), allowparams,
                                                    allowpopout, popoutloc, oIds, "", autoOpen, dd_defaultworkspace_create.SelectedValue, _isPrivate,
                                                    _allowUserOverride, appColor, iconColor);

                                bool maxonload = HelperMethods.ConvertBitToBoolean(dd_maxonload_create.SelectedValue);
                                _apps.UpdateAutoFullScreen(appId, maxonload);

                                foreach (ListItem checkedItem in dd_package.Items) {
                                    if (checkedItem.Selected) {
                                        var package = new AppPackages(false);
                                        string list = package.GetAppList_nonarray(checkedItem.Value);
                                        list += appId + ",";
                                        package.updateAppList(checkedItem.Value, list, _username);
                                    }
                                }

                                if (cb_InstallAfterLoad.Checked) {
                                    var member = new MemberDatabase(_username);
                                    member.UpdateEnabledApps(appId);
                                }

                                appCreated = true;
                                SetReturnMessage("App created successfully", false);
                                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=easycreate&b=app_created&date=" + ServerSettings.ServerDateTime.Ticks);
                            }
                        }
                        else {
                            SetReturnMessage("Error! App already exists", true);
                            ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
                        }
                    }
                    else {
                        SetReturnMessage("Error! App already exists", true);
                        ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
                    }
                }
                else {
                    SetReturnMessage("Error! Must have a filename and app name defined.", true);
                    ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
                }
            }
            else {
                SetReturnMessage("Error! HTML Link MUST be a well formed url.", true);
                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
            }
        }
        catch (Exception ex) {
            if (appCreated) {
                SetReturnMessage("App created successfully", false);
                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=easycreate&b=app_created&date=" + ServerSettings.ServerDateTime.Ticks);
            }
            else {
                ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
            }
        }

        ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx?tab=easycreate&date=" + ServerSettings.ServerDateTime.Ticks);
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
        if (string.IsNullOrEmpty(appDescription)) {
            appDescription = "No description available";
        }

        string aboutApp = tb_about_create.Text.Trim();
        if (string.IsNullOrEmpty(aboutApp)) {
            aboutApp = CheckLicense.SiteName + " | " + DateTime.Now.Year.ToString() + " | John Donnelly";
        }

        string appId = "app-" + fi;
        _apps.CreateItem(appId, tb_appname.Text, filename, picname,
                            dd_allowresize_create.SelectedValue, dd_allowmax_create.SelectedValue,
                            aboutApp, appDescription,
                            dd_enablebg_create.SelectedValue, categorySelectedVals,
                            tb_minheight_create.Text, tb_minwidth_create.Text, allowparams,
                            allowpopout, popoutloc, oIds, "", autoOpen, dd_defaultworkspace_create.SelectedValue, 
                            _isPrivate, _allowUserOverride, appColor, iconColor);
        bool maxonload = HelperMethods.ConvertBitToBoolean(dd_maxonload_create.SelectedValue);
        _apps.UpdateAutoFullScreen(appId, maxonload);

        foreach (ListItem checkedItem in dd_package.Items) {
            if (checkedItem.Selected) {
                var package = new AppPackages(false);
                string list = package.GetAppList_nonarray(checkedItem.Value);
                list += appId + ",";
                package.updateAppList(checkedItem.Value, list, _username);
            }
        }

        if (cb_InstallAfterLoad.Checked) {
            var member = new MemberDatabase(_username);
            member.UpdateEnabledApps(appId);
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

        ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx" + rq);
    }
    private bool AppExists(string name, string id) {
        string _name = _apps.GetAppID(name);
        string _id = _apps.GetAppName(id);
        if ((!string.IsNullOrEmpty(_id)) || (!string.IsNullOrEmpty(_name)))
            return true;

        return false;
    }

    #endregion


    #region App Delete Password Check

    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(hf_appdeleteid.Value)) return;

        string createdBy = _apps.GetAppCreatedBy(hf_appdeleteid.Value);

        bool isGood = false;

        MemberDatabase tempMember = new MemberDatabase(_username);
        if (tempMember.IsSocialAccount && createdBy.ToLower() == _username.ToLower()) {
            isGood = true;
        }
        else {
            MembershipUser u = Membership.GetUser(createdBy);

            string passwordUser = ServerSettings.AdminUserName;
            if ((_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && (u != null)) {
                passwordUser = u.UserName;
            }

            if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                bool userLockedOut = MemberDatabase.CheckIfUserIsLockedOut(ServerSettings.AdminUserName);
                isGood = Membership.ValidateUser(ServerSettings.AdminUserName, tb_passwordConfirm.Text);
                MemberDatabase.UnlockUserIfNeeded(userLockedOut, ServerSettings.AdminUserName);
            }
            else if (!string.IsNullOrEmpty(createdBy) && createdBy.ToLower() == _username.ToLower()) {
                bool userLockedOut = MemberDatabase.CheckIfUserIsLockedOut(passwordUser);
                isGood = Membership.ValidateUser(passwordUser, tb_passwordConfirm.Text);
                MemberDatabase.UnlockUserIfNeeded(userLockedOut, passwordUser);
            }
        }

        if (isGood) {
            RegisterPostbackScripts.RegisterStartupScript(this, "BeginWork();");
        }
        else {
            if (tempMember.IsSocialAccount) {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.RemoveUpdateModal();openWSE.AlertWindow('You are not authorized to delete this app.');");
            }
            else {
                tb_passwordConfirm.Text = "";
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.RemoveUpdateModal();openWSE.AlertWindow('Password is invalid');");
            }
        }
    }
    protected void hf_StartDelete_Changed(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(hf_appdeleteid.Value)) return;

        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (var member in from MembershipUser u in coll select new MemberDatabase(u.UserName)) {
            member.RemoveEnabledApp(hf_appdeleteid.Value);
        }

        string name = _apps.GetAppName(hf_appdeleteid.Value);
        string icon = _apps.GetAppIconName(hf_appdeleteid.Value);

        bool isDB = false;
        if (!string.IsNullOrEmpty(icon)) {
            icon = icon.ToLower();
            if ((icon != App.DefaultAppIcon.ToLower()) && (icon != "database.png")) {
                if (File.Exists(ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + icon)) {
                    try {
                        File.Delete(ServerSettings.GetServerMapLocation + "Standard_Images\\App_Icons\\" + icon);
                    }
                    catch { }
                }
            }
        }

        _apps.DeleteAppComplete(hf_appdeleteid.Value, ServerSettings.GetServerMapLocation);
        _apps.DeleteAppLocal(hf_appdeleteid.Value);

        var wp = new AppPackages(true);
        foreach (Dictionary<string, string> dr in wp.listdt) {
            string[] l = wp.GetAppList(dr["ID"]);
            string tempL = l.Where(w => w.ToLower() != hf_appdeleteid.Value.ToLower())
                            .Aggregate(string.Empty, (current, w) => current + (w + ","));

            wp.updateAppList(dr["ID"], tempL, _username);
        }

        string rq = "";
        if (!string.IsNullOrEmpty(Request.QueryString["tab"])) {
            rq = "?tab=" + Request.QueryString["tab"];
        }

        ServerSettings.PageToolViewRedirect(this.Page, "AppManager.aspx" + rq);
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