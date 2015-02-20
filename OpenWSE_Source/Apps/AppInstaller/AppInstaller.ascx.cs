using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Text;
using System.Data;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

public partial class Apps_AppInstaller_AppInstaller : System.Web.UI.UserControl
{
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly App _apps = new App();
    private AppInitializer _appInitializer;
    private const string app_id = "app-appinstaller";

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userId = Page.User.Identity;
        // InitializeAppList();
        GetCounts();
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        string cl = _apps.GetAppName(app_id);
        lbl_Title.Text = cl;

        if (!_ss.HideAllAppIcons)
        {
            img_Title.Visible = true;
            img_Title.ImageUrl = "~/Standard_Images/App_Icons/installer.png";
        }
        else
            img_Title.Visible = false;

        _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        _appInitializer.LoadScripts_JS(true, "StartAppInstaller()");

        // RegisterPostbackScripts.RegisterStartupScript(this, "StartAppInstaller();");
    }

    private void InitializeAppList()
    {
        string list = GetApps("all", "");
        all_apps_holder.InnerHtml = list;
    }

    public string GetApps(string category, string search)
    {
        AppCategory _appCategory = new AppCategory(true);
        _apps.GetAllApps();
        StringBuilder str = new StringBuilder();

        int count = 1;

        bool AssociateWithGroups = _ss.AssociateWithGroups;
        MemberDatabase _member = new MemberDatabase(HttpContext.Current.User.Identity.Name);

        foreach (Apps_Coll dr in _apps.AppList)
        {
            bool cancontinue = true;

            if (dr.AppId.ToLower() == app_id.ToLower())
                cancontinue = false;
            else
            {
                if (category.ToLower() == "all")
                    cancontinue = MatchApp(dr.AppId);
            }

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(dr, _member)) {
                    continue;
                }
            }

            if ((HttpContext.Current.User.Identity.Name.ToLower() != dr.CreatedBy.ToLower()) && (dr.IsPrivate)) {
                continue;
            }

            if (cancontinue) {
                string categoryName = _appCategory.GetCategoryName(dr.Category);
                var memberdata = new MemberDatabase(Page.User.Identity.Name);
                string image = "<img alt='icon' src='Standard_Images/App_Icons/" + dr.Icon + "' style='width: 48px; position: absolute;' />";
                if (_ss.HideAllAppIcons)
                    image = string.Empty;

                string checkbox = string.Empty;
                bool isInstalled = false;
                if (!memberdata.UserHasApp(dr.AppId))
                    checkbox = "<input id='cb_" + dr.AppId + "' class='cb-appinstaller cb_" + dr.AppId + "' type='checkbox' onchange=\"CheckBoxSelect_AppInstaller(this, '" + dr.AppId + "')\" value='" + dr.AppId + "' style='position: absolute; top: 80px; left: 18px;'>";
                else
                    isInstalled = true;

                string createdbyText = dr.CreatedBy;
                if (string.IsNullOrEmpty(createdbyText))
                    createdbyText = "N/A";

                string descriptionText = dr.Description;
                if (string.IsNullOrEmpty(descriptionText))
                    descriptionText = "No description available.";

                str.Append("<div class='pad-all'>");
                str.Append("<div class='float-left' style='position: relative;'>" + image + "<br />" + checkbox + "</div>");
                str.Append("<div class='pad-left-big pad-right-big pad-top-big app-panel-description'>");
                str.Append("<h4><span class='pad-right font-bold'>App Name:</span>" + dr.AppName + "</h4>");
                str.Append("<div class='clear-space-five'></div>");
                str.Append("<h4><span class='pad-right font-bold'>Category:</span>" + categoryName + "</h4>");
                str.Append("<div class='clear-space-five'></div>");
                str.Append("<h4><span class='pad-right font-bold'>Description:</span>" + descriptionText + "</h4>");
                str.Append("<div class='clear-space'></div>");
                str.Append("<div class='pad-top-big'>");

                if (!isInstalled)
                    str.Append("<a href='#' class='margin-bottom margin-right-big' onclick=\"AddApp('" + dr.AppId + "');return false;\"><span class='float-left td-add-btn margin-right-sml' style='padding: 0px!important;'></span>Install " + dr.AppName + "</a>");
                else
                    str.Append("<a href='#' class='margin-bottom margin-right-big' onclick=\"RemoveApp('" + dr.AppId + "');return false;\"><span class='float-left td-delete-btn margin-right-sml' style='padding: 0px!important;'></span>Remove " + dr.AppName + "</a>");


                str.Append("<a href='#' class='margin-left' onclick=\"AboutApp_AppInstaller('" + dr.AppName + "','" + dr.AppId + "');return false;\">Learn More</a>");
                str.Append("</div></div></div>");
                str.Append("<div class='clear' style='height: 20px'></div>");
                count++;
            }
        }

        if (string.IsNullOrEmpty(str.ToString()))
        {
            str.Clear();
            str.Append("<div class='clear-space'></div><h3 class='pad-left-big'>No Apps Available</h3>");
        }

        return str.ToString();
    }

    private bool MatchApp(string appid)
    {
        var packages = new AppPackages(true);
        string[] packagelist = packages.GetAppList(_ss.AppInstallerPackage);

        return packagelist.Any(package => package.ToLower() == appid.ToLower());
    }

    private void GetCounts()
    {
        AppPackages appPackage = new AppPackages(false);
        string[] list = appPackage.GetAppList(_ss.AppInstallerPackage);

        int count = list.Length - 1;
        if (count < 0) {
            count = 0;
        }

        lbl_TotalApps.Text = count.ToString();

        SitePlugins plugins = new SitePlugins(HttpContext.Current.User.Identity.Name);
        plugins.BuildSitePlugins(true);
        lbl_TotalPlugins.Text = plugins.siteplugins_dt.Count.ToString();
    }
}