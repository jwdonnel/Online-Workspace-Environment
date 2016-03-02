using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SiteTools_AppInstaller : System.Web.UI.Page
{
    private ServerSettings _ss = new ServerSettings();
    private MemberDatabase _member;
    private string _username;
    private App _appItem;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name) && (userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                _username = userId.Name;
                _member = new MemberDatabase(_username);
                _appItem = new App(_username, false);

                BuildCategories();
                BuildAppList();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void BuildAppList() {
        pnl_AppList.Controls.Clear();

        StringBuilder str = new StringBuilder();
        StringBuilder strJavascript = new StringBuilder();

        string appIconFolder = ResolveUrl("~/Standard_Images/App_Icons/");

        AppPackages packages = new AppPackages(false);
        AppRatings ratings = new AppRatings(_username);

        List<string> userApps = _member.EnabledApps;

        int count = 0;
        string[] appList = packages.GetAppList(_ss.AppInstallerPackage);
        foreach (string app in appList) {
            Apps_Coll coll = _appItem.GetAppInformation(app);
            if (CanAddAppInList(coll)) {
                string categoryIds = string.Empty;
                string categoryNames = string.Empty;
                AppCategory _category = new AppCategory(false);
                Dictionary<string, string> categoryList = _category.BuildCategoryDictionary(coll.Category);
                foreach (KeyValuePair<string, string> categoryItem in categoryList) {
                    categoryIds += categoryItem.Key + ServerSettings.StringDelimiter;
                    categoryNames += categoryItem.Value + ", ";
                }

                categoryNames = categoryNames.Trim();
                if (categoryNames.EndsWith(",")) {
                    categoryNames = categoryNames.Remove(categoryNames.Length - 1);
                }

                str.AppendFormat("<div class='table-settings-box contact-card-main app-item-installer' data-appid='{0}' data-category='{1}'>", coll.AppId, categoryIds);
                if (!_ss.HideAllAppIcons && !_member.HideAppIcon) {
                    str.AppendFormat("<img alt='' src='{0}' />", appIconFolder + coll.Icon);
                }

                if (_ss.AllowAppRating) {
                    str.AppendFormat("<div class='app-name-holder'><span class='app-name'>{0}</span><div class='clear-space-five'></div><div class='app-rater-{1} app-rater-installer'></div></div>", coll.AppName, coll.AppId);
                    strJavascript.AppendFormat("openWSE.RatingStyleInit('.app-rater-{0}', '" + ratings.GetAverageRating(coll.AppId) + "', true, '{0}', true);", coll.AppId);
                }
                else {
                    str.AppendFormat("<div class='app-name-holder' style='padding-top: 15px;'><span class='app-name'>{0}</span></div>", coll.AppName);
                }

                string description = coll.Description;
                if (!string.IsNullOrEmpty(description)) {
                    description += " - ";
                }
                description += coll.About;

                str.AppendFormat("<div class='app-installer-category'>{0}</div>", categoryNames);
                str.AppendFormat("<a href='#' title='View Information' class='install-btn td-details-btn' onclick=\"AppMoreDetails('{0}');return false;\"></a>", coll.AppId);

                if (userApps.Contains(coll.AppId)) {
                    str.AppendFormat("<a href='#' title='Uninstall App' class='install-btn margin-right td-subtract-btn' onclick=\"UninstallApp('{0}');return false;\"></a>", coll.AppId);
                }
                else {
                    str.AppendFormat("<a href='#' title='Install App' class='install-btn margin-right td-add-btn' onclick=\"InstallApp('{0}');return false;\"></a>", coll.AppId);
                }

                str.AppendFormat("<div class='app-description'>{0}</div>", description);
                if (userApps.Contains(coll.AppId)) {
                    str.AppendFormat("<span class='installed'>Installed</span>");
                }
                str.AppendFormat("</div>");
                count++;
            }
        }

        lbl_AppsAvailable.InnerHtml = "<b class='pad-right'>Apps Available</b>" + count.ToString();

        if (!string.IsNullOrEmpty(strJavascript.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, strJavascript.ToString());
        }

        if (!string.IsNullOrEmpty(str.ToString())) {
            pnl_AppList.Controls.Add(new LiteralControl(str.ToString()));
        }
        else {
            pnl_AppList.Controls.Add(new LiteralControl("<h3 class='pad-top-big pad-bottom-big'>No Apps Found</h3>"));
        }

        updatePnl_AppTotals.Update();
        updatePnl_AppList.Update();
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

    private void BuildCategories() {
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

    protected void hf_ViewDetails_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_ViewDetails.Value)) {
            BuildDetails(hf_ViewDetails.Value);
            hf_ViewDetails.Value = string.Empty;
        }
    }

    protected void hf_InstallApp_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_InstallApp.Value)) {
            _member.UpdateEnabledApps(hf_InstallApp.Value);
        }

        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.ReloadPage();");
        }
        else {
            // BuildDetails(hf_InstallApp.Value);
            hf_InstallApp.Value = string.Empty;
            BuildAppList();

            AppIconBuilder aib = new AppIconBuilder(Page, _member);
            aib.BuildAppsForUser();
        }
    }

    protected void hf_UninstallApp_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_UninstallApp.Value)) {
            _member.RemoveEnabledApp(hf_UninstallApp.Value);
        }

        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.ReloadPage();");
        }
        else {
            // BuildDetails(hf_UninstallApp.Value);
            hf_UninstallApp.Value = string.Empty;
            BuildAppList();

            AppIconBuilder aib = new AppIconBuilder(Page, _member);
            aib.BuildAppsForUser();
        }
    }

    private void BuildDetails(string appId) {
        _appItem.BuildAboutApp(pnl_aboutHolder, appId, _username);

        List<string> userApps = _member.EnabledApps;

        updatepnl_aboutHolder.Update();

        string appName = _appItem.GetAppName(appId);

        StringBuilder strJavascript = new StringBuilder();
        AppRatings ratings = new AppRatings(_username);
        strJavascript.AppendFormat("openWSE.RatingStyleInit('.app-rater-" + appId + "', '" + ratings.GetAverageRating(appId) + "', true, '{0}', true);", appId);
        strJavascript.AppendFormat("openWSE.LoadModalWindow(true, 'aboutApp-element', 'About " + appName + "');");
        RegisterPostbackScripts.RegisterStartupScript(this, strJavascript.ToString());
    }

    protected void imgbtn_search_Click(object sender, EventArgs e) {
        BuildAppList();
    }
}