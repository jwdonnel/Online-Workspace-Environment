#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

#endregion

/// <summary>
///     Summary description for SideBarItems
/// </summary>
public class SideBarItems : Page {
    #region private variable assignments

    private readonly string _username;
    private readonly IPWatch _ipwatch = new IPWatch(true);
    private readonly MemberDatabase _member;
    private ServerSettings _ss = new ServerSettings();
    private string _sitetheme;
    private string _sitePath;

    #endregion

    public SideBarItems(string username) {
        if (!string.IsNullOrEmpty(username)) {
            _username = username;
            _member = new MemberDatabase(_username);
            _sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);

            if (_sitePath.LastIndexOf('/') != _sitePath.Length - 1)
                _sitePath += "/";

            _sitetheme = _member.SiteTheme;
        }
    }


    #region Build Admin Page Links

    public string BuildAdminPages(bool loadToNewPage) {
        var appScript = new StringBuilder();
        if (!string.IsNullOrEmpty(_username)) {
            bool hideIcon = _member.HideAppIcon;

            List<string> memberPages = _member.AdminPagesList.ToList();
            List<string> pagesList = ServerSettings.AdminPages();
            foreach (string p in pagesList) {
                if ((Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) || (memberPages.Contains(p))) {
                    appScript.Append(BuildLink(p.ToLower(), loadToNewPage, hideIcon));
                }
            }
        }

        if (string.IsNullOrEmpty(appScript.ToString())) {
            appScript.Append("<h3 class='pad-left pad-top-big pad-bottom-big'>No Links Available</h3>");
        }

        return appScript.ToString();
    }
    private string BuildLink(string page, bool loadToNewPage, bool hideIcon) {
        StringBuilder appScript = new StringBuilder();
        string target = "";
        if (loadToNewPage) {
            target = " target='_blank'";
        }

        if ((page == "workspace") && (_username.ToLower() == ServerSettings.AdminUserName.ToLower())) {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(page)) {
            SiteMapNodeCollection siteNodes = SiteMap.RootNode.ChildNodes;
            foreach (SiteMapNode node in siteNodes) {
                try {
                    string url = node.Url.Substring(node.Url.LastIndexOf('/') + 1);
                    string[] urlSplit = url.Split('.');
                    string tempUrl = string.Empty;
                    if (urlSplit.Length > 0) {
                        tempUrl = urlSplit[0].ToLower();
                    }

                    if (page.ToLower() == tempUrl) {
                        string icon = string.Empty;
                        if (!string.IsNullOrEmpty(node["icon"]) && !_ss.HideAllAppIcons && !hideIcon) {
                            string iconFile = ServerSettings.GetServerMapLocation + "App_Themes\\" + _sitetheme + "\\" + node["icon"].Replace("/", "\\");
                            if (File.Exists(iconFile)) {
                                icon = "<img alt='' src='" + ServerSettings.ResolveUrl("~/App_Themes/" + _sitetheme + "/" + node["icon"]) + "' />";
                            }
                        }

                        string toolTip = "";
                        if (!string.IsNullOrEmpty(node["tooltip"])) {
                            toolTip = " title='" + node["tooltip"] + "'";
                        }

                        string title = node.Title;

                        if ((string.IsNullOrEmpty(node["nodisplaychildren"]) || node["nodisplaychildren"] == "false") && (node.HasChildNodes)) {
                            string expandBtn = "<span class='img-expand-sml' title='Expand links' onclick='openWSE.ExpandAdminLinks(this, \"" + tempUrl + "-sub-link\");return false;'></span>";
                            appScript.Append("<a class='app-icon-links' href='" + node.Url + "'" + target + toolTip + ">" + icon + "<span class='app-icon-font'>" + expandBtn + title + "</span></a>");
                            appScript.Append("<div class='app-icon-sub-link-holder " + tempUrl + "-sub-link' style='display: none;'>");
                            foreach (SiteMapNode childNode in node.ChildNodes) {
                                string iconChild = string.Empty;
                                if (!string.IsNullOrEmpty(childNode["icon"]) && !_ss.HideAllAppIcons && !hideIcon) {
                                    string iconFile_Child = ServerSettings.GetServerMapLocation + "App_Themes\\" + _sitetheme + "\\" + childNode["icon"].Replace("/", "\\");
                                    if (File.Exists(iconFile_Child)) {
                                        iconChild = "<img alt='' src='" + ServerSettings.ResolveUrl("~/App_Themes/" + _sitetheme + "/" + childNode["icon"]) + "' />";
                                    }
                                }

                                string childToolTip = "";
                                if (!string.IsNullOrEmpty(childNode["tooltip"])) {
                                    childToolTip = " title='" + childNode["tooltip"] + "'";
                                }

                                string childTitle = childNode.Title;
                                appScript.Append("<a class='app-icon-sub-links' href='" + childNode.Url + "'" + target + childToolTip + ">" + iconChild + "<span class='app-icon-font'>" + childTitle + "</span></a>");
                            }
                            appScript.Append("</div>");
                        }
                        else {
                            appScript.Append("<a class='app-icon-links' href='" + node.Url + "'" + target + toolTip + ">" + icon + "<span class='app-icon-font'>" + title + "</span></a>");
                        }

                        break;
                    }
                }
                catch {
                }
            }
        }

        return appScript.ToString();
    }

    #endregion


    #region Admin Tools

    public string BuildAppEditor(Page currentPage) {
        bool appparms = false;
        if (currentPage.ClientQueryString.Length > 0) {
            string qs = currentPage.ClientQueryString;
            if (qs == "c=params") {
                appparms = true;
            }
        }

        var app_List = new StringBuilder();
        var app_script = new StringBuilder();
        var app_category = new AppCategory(true);
        var categories = new List<string>();
        var apps = new App();
        apps.GetAllApps();

        List<string> appListCount = new List<string>();

        app_List.Append("<div class='clear-space'></div><div id='app-editor-holder' style='display:none'>");
        app_List.Append("<div id='Category-Back' style='display:none'>");
        app_List.Append("<img alt='back' src='" + _sitePath + "App_Themes/" + _sitetheme + "/Icons/prevpage.png' />");
        app_List.Append("<h4 id='Category-Back-btn' class='float-left'>" + "</h4>"); // Place 'Back' text here
        app_List.Append("<h4 id='Category-Back-Name' class='float-left'></h4></div>");
        app_List.Append("<div id='Category-Back-Name-id' style='display:none'></div>");

        bool AssociateWithGroups = _ss.AssociateWithGroups;
        foreach (Apps_Coll dr in apps.AppList) {
            if (apps.IconExists(dr.AppId)) {
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

                if (cancontinue) {
                    if (appparms) {
                        cancontinue = false;
                        if (dr.AllowParams) {
                            cancontinue = true;
                        }
                    }

                    if (cancontinue) {
                        if (AssociateWithGroups) {
                            if (!ServerSettings.CheckAppGroupAssociation(dr, _member)) {
                                continue;
                            }
                        }

                        string w = dr.AppName;
                        string id = dr.AppId;
                        string iconname = dr.Icon;
                        string categoryid = dr.Category;
                        string[] categorySplit = categoryid.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string c in categorySplit) {
                            string cId = c;
                            string categoryname = app_category.GetCategoryName(cId);
                            if (categoryname == "Uncategorized") {
                                cId = "Uncategorized";
                            }


                            if (string.IsNullOrEmpty(cId))
                                cId = categoryname;

                            if (!string.IsNullOrEmpty(dr.filename)) {
                                var fi = new FileInfo(dr.filename);

                                if (!appListCount.Contains(dr.ID)) {
                                    appListCount.Add(dr.ID);
                                }

                                string iconImg = "<img alt='' src='" + _sitePath + "Standard_Images/App_Icons/" + iconname + "' />";
                                if (_ss.HideAllAppIcons)
                                    iconImg = string.Empty;

                                if (appparms) {
                                    app_script.Append("<div class='app-icon rbbuttons' title=\"View " + w + "'s parameters\" onclick=\"appchange('" + dr.AppId + "')\">");
                                    app_script.Append(iconImg + "<span class='app-icon-font'>" + w + "</span></div>");
                                }
                                else {
                                    if (!categories.Contains(categoryname)) {
                                        app_List.Append(BuildCategory(cId, categoryname));
                                        categories.Add(categoryname);
                                    }

                                    app_script.Append("<div class='" + cId + " app-category-div' style='display: none'>");
                                    app_script.Append("<div class='app-icon' title=\"View " + w + "'s properites\" onclick=\"appchange('" + dr.AppId + "')\">");
                                    app_script.Append(iconImg + "<span class='app-icon-font'>" + w + "</span></div>");
                                    app_script.Append("</div>");
                                }
                            }
                        }
                    }
                }
            }
        }

        int count = appListCount.Count;

        if (!string.IsNullOrEmpty(app_script.ToString())) {
            app_script.Append("</div>");
            if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                string appcount =
                    "<b class='pad-right'>Apps Available</b><span>" +
                    count.ToString() + "</span><div class='clear-space'></div>";
                appcount +=
                    "<small>Click on one of the apps below<br/>to view/edit the properties.</small>";
                appcount += "<div class='clear-space'></div><div class='clear-space'></div>";
                return appcount + app_List + app_script;
            }
            else {
                string appcount =
                    "<b class='pad-right'>Apps Created</b><span>" +
                    count.ToString() + "</span><div class='clear-space'></div>";
                appcount +=
                    "<small>Click on one of the apps below<br/>to view/edit the properties.</small>";
                appcount += "<div class='clear-space'></div><div class='clear-space'></div>";
                return appcount + app_List + app_script;
            }
        }
        else
            return "<span class='font-bold'>No Apps Available</span>";
    }

    #endregion


    #region Icon/Category Builder

    private string BuildCategory(string id, string category) {
        var w = new App();
        var str = new StringBuilder();
        string count = string.Empty;

        if (id == category && id != "Uncategorized") {
            count = " (" + GetAppCount_Category("") + ")";
        }
        else if (id == "Uncategorized") {
            count = " (" + GetAppCount_Category_Uncategorized() + ")";
        }
        else {
            count = " (" + GetAppCount_Category(id) + ")";
        }

        str.Append("<div data-appid='" + id + "' class='app-icon-category-list' runat='server' onclick=\"CategoryClick('" + id + "', '" + category + "')\">");
        str.Append("<span class='app-icon-font'>" + category + count + "</span>");
        str.Append("<img alt='forward' src='" + _sitePath + "App_Themes/" + _sitetheme + "/Icons/nextpage.png' /></div>");

        return str.ToString();
    }
    private int GetAppCount_Category(string id) {
        int count = 0;
        var w = new App();
        var userapps = new List<string>();
        bool account = false;
        bool standard = false;
        if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            standard = true;
        }

        List<Apps_Coll> appColl = w.GetApps_byCategory(id);
        foreach (Apps_Coll dr in appColl) {
            if (account) {
                if (userapps.Contains(dr.AppId))
                    count++;
            }
            else if (standard) {
                if (dr.CreatedBy.ToLower() == _username.ToLower())
                    count++;
            }
            else {
                count = appColl.Count;
                break;
            }
        }
        return count;
    }
    private int GetAppCount_Category_Uncategorized() {
        int count = 0;
        var w = new App();

        w.GetAllApps();
        List<Apps_Coll> appColl = w.AppList;

        AppCategory app_category = new AppCategory(false);

        foreach (Apps_Coll dr in appColl) {
            string categoryname = app_category.GetCategoryName(dr.Category);
            if (categoryname == "Uncategorized") {
                count++;
            }
        }
        return count;
    }

    #endregion

}