<%@ WebService Language="C#" Class="AppInstallerService" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Text;
using System.Security.Principal;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class AppInstallerService : System.Web.Services.WebService {
    private ServerSettings _ss = new ServerSettings();
    private readonly AppCategory _appCategory = new AppCategory(true);
    private string _username;
    private MemberDatabase _member;
    private App _apps;
    private SitePlugins _plugins;
    private const string _appId = "app-appinstaller";

    public AppInstallerService() {
        IIdentity userId = HttpContext.Current.User.Identity;
        _apps = new App(userId.Name);
        _plugins = new SitePlugins(userId.Name);
        _username = userId.Name;
        _member = new MemberDatabase(userId.Name);
    }

    [WebMethod]
    public string GetCategories() {
        List<string> tempList = new List<string>();
        StringBuilder app_script = new StringBuilder();

        app_script.Append("<div class='sidebar-divider-no-margin'></div>");

        int count = 0;
        foreach (Dictionary<string, string> dr in _appCategory.category_dt) {
            if (!tempList.Contains(dr["ID"])) {
                string cssClass = "tsdiv";
                if (count == 0) {
                    cssClass += " tsactive";
                }
                app_script.Append("<div class='" + cssClass + "' onclick=\"SetCategory(this, '" + dr["ID"] + "')\"><div class='pad-all-sml'>");
                app_script.Append("<h4 class='font-bold float-left'>" + dr["Category"] + "</h4>");
                app_script.Append("<span class='float-right' style='font-size: 12px'>(" + GetAppCount_Category(dr["ID"]) + ")</span></div></div>");
                app_script.Append("<div class='sidebar-divider-no-margin'></div>");
                tempList.Add(dr["ID"]);
                count++;
            }
        }
        return app_script.ToString();
    }

    [WebMethod]
    public string GetApps(string category, string search) {
        _apps.GetAllApps();
        StringBuilder str = new StringBuilder();

        bool AssociateWithGroups = _ss.AssociateWithGroups;

        int count = 1;
        foreach (Apps_Coll dr in _apps.AppList) {
            bool cancontinue = true;

            string[] categorySplit = dr.Category.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            string categoryName = dr.Category;
            
            foreach (string c in categorySplit) {
                categoryName = _appCategory.GetCategoryName(c);
                if (dr.AppId.ToLower() == _appId.ToLower())
                    cancontinue = false;
                else {
                    if ((category.ToLower() == "all") || (string.IsNullOrEmpty(category))) {
                        if ((!string.IsNullOrEmpty(search)) && (search.ToLower() != "search apps/plugins")) {
                            if ((dr.AppName.ToLower().Contains(search.ToLower()))
                                || (dr.AppId.ToLower().Contains(search.ToLower()))
                                || (categoryName.ToLower().Contains(search.ToLower()))
                                || (dr.Description.ToLower().Contains(search.ToLower()))) {
                                cancontinue = MatchApp(dr.AppId);
                            }
                            else {
                                cancontinue = false;
                            }
                        }
                        else {
                            cancontinue = MatchApp(dr.AppId);
                            break;
                        }
                    }
                    else {
                        if (c == category) {
                            if ((!string.IsNullOrEmpty(search)) && (search.ToLower() != "search apps/plugins")) {
                                if ((dr.AppName.ToLower().Contains(search.ToLower()))
                                    || (dr.AppId.ToLower().Contains(search.ToLower()))
                                    || (categoryName.ToLower().Contains(search.ToLower()))
                                    || (dr.Description.ToLower().Contains(search.ToLower()))) {
                                    cancontinue = MatchApp(dr.AppId);
                                }
                                else {
                                    cancontinue = false;
                                }
                            }
                            else {
                                cancontinue = MatchApp(dr.AppId);
                                break;
                            }
                        }
                        else {
                            cancontinue = false;
                        }
                    }
                }
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
                var memberdata = new MemberDatabase(_username);
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
                    str.Append("<a href='#' class='margin-bottom margin-right-big' onclick=\"RemoveApp('" + dr.AppId + "');return false;\"><span class='float-left td-delete-btn margin-right-sml' style='padding: 0px!important;'></span>Uninstall " + dr.AppName + "</a>");


                str.Append("<a href='#' class='margin-left' onclick=\"AboutApp_AppInstaller('" + dr.AppName + "','" + dr.AppId + "');return false;\">Learn More</a>");
                str.Append("</div></div></div>");
                str.Append("<div class='clear' style='height: 20px'></div>");
                count++;
            }
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Clear();
            str.Append("<div class='clear-space'></div><h4 class='pad-left-big'>No Apps Available</h4>");
        }

        return str.ToString();
    }

    [WebMethod]
    public string AddApp(string id) {
        try {
            _member.UpdateEnabledApps(id);
            return "true";
        }
        catch { return ""; }
    }

    [WebMethod]
    public string RemoveApp(string id) {
        try {
            _member.RemoveEnabledApp(id);
            string appId = _apps.GetAppID(id);
            _apps.DeleteAppLocal(appId, _username);
            return "true";
        }
        catch { return ""; }
    }

    [WebMethod]
    public string InstallBulk(object apps) {
        try {
            object[] appList = apps as object[];
            foreach (object id in appList) {
                _member.UpdateEnabledApps(id.ToString());
            }
            return "true";
        }
        catch { return ""; }
    }

    [WebMethod]
    public object[] AboutApp(string appId) {
        List<object> obj = new List<object>();
        obj.Add(_apps.BuildAboutApp(appId, _username));
        if (_ss.AllowAppRating) {
            AppRatings ratings = new AppRatings();
            obj.Add(ratings.GetAverageRating(appId));
            obj.Add(ratings.GetAppRatings(appId));
        }
        return obj.ToArray();
    }

    [WebMethod]
    public string LoadPlugins(string search, string path) {
        StringBuilder str = new StringBuilder();
        _plugins.BuildSitePlugins(true);
        _plugins.BuildSitePluginsForUser();

        bool AssociateWithGroups = _ss.AssociateWithGroups;

        int count = 1;
        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            bool cancontinue = false;
            if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                if ((!string.IsNullOrEmpty(search)) && (search.ToLower() != "search apps/plugins")) {
                    if ((coll.PluginName.ToLower().Contains(search.ToLower()))
                        || (coll.Description.ToLower().Contains(search.ToLower()))
                        || (coll.CreatedBy.ToLower().Contains(search.ToLower()))
                        || (coll.PluginLocation.ToLower().Contains(search.ToLower()))) {
                        cancontinue = true;
                    }
                }
                else
                    cancontinue = true;

                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckPluginGroupAssociation(coll, _member)) {
                        continue;
                    }
                }

                if (cancontinue) {
                    string descText = coll.Description;
                    if (string.IsNullOrEmpty(descText))
                        descText = "N/A";

                    str.Append("<div class='pad-all'>");
                    str.Append("<h4 class='float-left font-color-black'>" + count + "</h4>");
                    str.Append("<div class='pad-left-big pad-right-big pad-top-big plugin-panel-description'>");
                    str.Append("<h4><span class='pad-right font-bold'>Plugin Name:</span>" + coll.PluginName + "</h4>");
                    str.Append("<div class='clear-space-five'></div>");
                    str.Append("<h4><span class='pad-right font-bold'>Description:</span>" + descText + "</h4>");
                    str.Append("<div class='clear-space-five'></div>");
                    str.Append("<h4><span class='pad-right font-bold'>Uploaded By:</span>" + coll.CreatedBy + "</h4>");
                    str.Append("<div class='clear-space-five'></div>");
                    str.Append("<h4><span class='pad-right font-bold'>Date Uploaded:</span>" + coll.Date + "</h4>");
                    str.Append("<div class='pad-top-big'>");

                    string userPluginID = "";
                    bool isInstalled = false;
                    foreach (UserPlugins_Coll userColl in _plugins.userplugins_dt) {
                        if (userColl.PluginID == coll.ID) {
                            userPluginID = userColl.ID;
                            isInstalled = true;
                            break;
                        }
                    }

                    if (!isInstalled) {
                        str.Append("<a href='#' style='margin-bottom: 15px;' onclick=\"AddPlugin('" + coll.ID + "');return false;\"><span class='float-left td-add-btn margin-right-sml' style='padding: 0px!important;'></span>Install " + coll.PluginName + " Plugin</a>");
                    }
                    else {
                        if (!string.IsNullOrEmpty(userPluginID)) {
                            str.Append("<span style='font-size: 14px; color: #436A87'><b>Already installed</b></span><div class='clear-space'></div>");
                            str.Append("<a href='#' style='margin-bottom: 15px;' onclick=\"RemovePlugin('" + userPluginID + "');return false;\"><span class='float-left td-delete-btn margin-right-sml' style='padding: 0px!important;'></span>Uninstall " + coll.PluginName + " Plugin</a>");
                        }
                    }
                    str.Append("</div></div></div>");
                    str.Append("<div class='clear' style='height: 20px'></div>");
                }
                count++;
            }
        }

        return str.ToString();
    }

    [WebMethod]
    public string AddPlugin(string id) {
        try {
            _plugins.addItemForUser(id);
            return "true";
        }
        catch { return ""; }
    }

    [WebMethod]
    public string RemovePlugin(string id) {
        try {
            _plugins.deleteItemForUser(id);
            return "true";
        }
        catch { return ""; }
    }

    private bool MatchApp(string appid) {
        var packages = new AppPackages(true);
        string[] packagelist = packages.GetAppList(_ss.AppInstallerPackage);

        return packagelist.Any(package => package.ToLower() == appid.ToLower());
    }

    private int GetAppCount_Category(string id) {
        var packages = new AppPackages(true);
        string[] packagelist = packages.GetAppList(_ss.AppInstallerPackage);
        return _apps.GetApps_byCategory(id).Cast<Apps_Coll>().Where(dr => dr.AppId.ToLower() != "app-appinstaller").Count(dr => packagelist.Contains(dr.AppId));
        return 0;
    }
}