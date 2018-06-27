using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SiteTools_AppInstaller : BasePage {

    private App CurrentAppObject = new App(string.Empty);

    protected void Page_Load(object sender, EventArgs e) {
        if (!IsUserNameEqualToAdmin()) {
            CurrentAppObject = new App(CurrentUsername, false);
            BuildCategories();
            BuildAppList();
        }
        else {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }
    }

    private void BuildAppList() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, false, 0);

        #region Build Body

        StringBuilder strJavascript = new StringBuilder();

        AppPackages packages = new AppPackages(false);
        AppRatings ratings = new AppRatings(CurrentUsername);

        List<string> userApps = CurrentUserMemberDatabase.EnabledApps;

        int count = 0;
        string[] appList = packages.GetAppList(MainServerSettings.AppInstallerPackage);
        foreach (string app in appList) {
            Apps_Coll coll = CurrentAppObject.GetAppInformation(app);
            if (CanAddAppInList(coll)) {
                string categoryIds = string.Empty;
                string categoryNames = string.Empty;
                AppCategory _category = new AppCategory(false);
                Dictionary<string, string> categoryList = _category.BuildCategoryDictionary(coll.Category);

                if (!string.IsNullOrEmpty(ddl_categories.SelectedValue) && !categoryList.ContainsKey(ddl_categories.SelectedValue)) {
                    continue;
                }

                foreach (KeyValuePair<string, string> categoryItem in categoryList) {
                    categoryIds += categoryItem.Key + ServerSettings.StringDelimiter;
                    categoryNames += categoryItem.Value + ", ";
                }

                categoryNames = categoryNames.Trim();
                if (categoryNames.EndsWith(",")) {
                    categoryNames = categoryNames.Remove(categoryNames.Length - 1);
                }

                List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
                StringBuilder str = new StringBuilder();

                str.AppendFormat("<img alt='' src='{0}' class='app-installer-img' />", ResolveUrl("~/") + coll.Icon);

                if (MainServerSettings.AllowAppRating) {
                    str.AppendFormat("<div class='app-name-holder'><span class='app-name'>{0}</span><div class='clear-space-five'></div><div class='app-rater-{1} app-rater-installer'></div></div>", coll.AppName, coll.AppId);
                    strJavascript.AppendFormat("openWSE.RatingStyleInit('.app-rater-{0}', '" + ratings.GetAverageRating(coll.AppId) + "', false, '{0}', true);", coll.AppId);
                }
                else {
                    str.AppendFormat("<div class='app-name-holder' style='padding-top: 15px;'><span class='app-name'>{0}</span></div>", coll.AppName);
                }

                string description = coll.Description;
                if (!string.IsNullOrEmpty(description)) {
                    description += "<div class='clear'></div>";
                }
                description += "<small><i>" + coll.About + "</i></small>";

                if (MainServerSettings.AllowAppRating) {
                    str.AppendFormat("<div class='app-installer-category'>{0}</div>", categoryNames);
                }
                else {
                    str.AppendFormat("<div class='app-installer-category' style='padding-top: 18px;'>{0}</div>", categoryNames);
                }

                str.AppendFormat("<a title='View Information' class='install-btn td-details-btn' onclick=\"AppMoreDetails('{0}');return false;\"></a>", coll.AppId);

                if (userApps.Contains(coll.AppId)) {
                    str.AppendFormat("<a title='Uninstall App' class='install-btn td-subtract-btn' onclick=\"UninstallApp('{0}');return false;\"></a>", coll.AppId);
                }
                else {
                    str.AppendFormat("<a title='Install App' class='install-btn td-add-btn' onclick=\"InstallApp('{0}');return false;\"></a>", coll.AppId);
                }

                str.AppendFormat("<div class='app-description'>{0}</div>", description);

                if (MainServerSettings.AllowAppRating) {
                    str.Append("<div class='clear-space'></div>");
                    str.Append(CurrentAppObject.GetReviews(coll.AppId));
                }

                if (userApps.Contains(coll.AppId)) {
                    str.AppendFormat("<span class='installed'>Installed</span>");
                }
                str.AppendFormat("<div class='clear'></div>");


                bodyColumnValues.Add(new TableBuilderBodyColumnValues("", str.ToString(), TableBuilderColumnAlignment.Left));
                tableBuilder.AddBodyRow(bodyColumnValues, string.Empty, string.Format("data-appid='{0}' data-category='{1}'", coll.AppId, categoryIds), "app-item-installer");

                count++;
            }
        }

        if (!string.IsNullOrEmpty(strJavascript.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, strJavascript.ToString());
        }

        #endregion

        pnl_AppList.Controls.Clear();
        pnl_AppList.Controls.Add(tableBuilder.CompleteTableLiteralControl("No apps found"));

        updatePnl_AppList.Update();
    }
    private bool CanAddAppInList(Apps_Coll coll) {
        if (coll != null && !string.IsNullOrEmpty(coll.ID)) {
            return true;
        }

        return false;
    }

    protected void ddl_categories_Changed(object sender, EventArgs e) {
        BuildAppList();
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
            CurrentUserMemberDatabase.UpdateEnabledApps(hf_InstallApp.Value);
        }

        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.ReloadPage();");
        }
        else {
            // BuildDetails(hf_InstallApp.Value);
            hf_InstallApp.Value = string.Empty;
            BuildAppList();

            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
            aib.BuildAppsForUser();
        }
    }

    protected void hf_UninstallApp_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_UninstallApp.Value)) {
            CurrentUserMemberDatabase.RemoveEnabledApp(hf_UninstallApp.Value);
        }

        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.ReloadPage();");
        }
        else {
            // BuildDetails(hf_UninstallApp.Value);
            hf_UninstallApp.Value = string.Empty;
            BuildAppList();

            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
            aib.BuildAppsForUser();
        }
    }

    private void BuildDetails(string appId) {
        CurrentAppObject.BuildAboutApp(pnl_aboutHolder, appId, CurrentUsername);

        List<string> userApps = CurrentUserMemberDatabase.EnabledApps;

        updatepnl_aboutHolder.Update();

        string appName = CurrentAppObject.GetAppName(appId);

        StringBuilder strJavascript = new StringBuilder();
        AppRatings ratings = new AppRatings(CurrentUsername);
        strJavascript.AppendFormat("openWSE.RatingStyleInit('.app-rater-" + appId + "', '" + ratings.GetAverageRating(appId) + "', true, '{0}', true);", appId);
        strJavascript.AppendFormat("openWSE.LoadModalWindow(true, 'aboutApp-element', 'About " + appName + "');");
        RegisterPostbackScripts.RegisterStartupScript(this, strJavascript.ToString());
    }

    protected void hf_refreshAppAbout_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_refreshAppAbout.Value) && hf_refreshAppAbout.Value != "refresh") {
            BuildDetails(hf_refreshAppAbout.Value);
        }

        hf_refreshAppAbout.Value = string.Empty;
    }

}