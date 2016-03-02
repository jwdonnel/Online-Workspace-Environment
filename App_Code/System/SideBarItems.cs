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
using System.Configuration;
using System.Web.SessionState;
using OpenWSE_Tools.GroupOrganizer;

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
    private bool _isGroupLoggedIn = false;
    private bool _showPageDescription = false;
    private bool _showSiteIcons = true;

    #endregion

    public SideBarItems(string username) {
        if (!string.IsNullOrEmpty(username)) {
            _username = username;
            _member = new MemberDatabase(_username);
            _sitePath = ServerSettings.GetSitePath(HttpContext.Current.Request);

            if (_sitePath.LastIndexOf('/') != _sitePath.Length - 1)
                _sitePath += "/";

            _sitetheme = _member.SiteTheme;
            _showPageDescription = _member.ShowSiteToolsPageDescription;
            _showSiteIcons = _member.ShowSiteToolsIcons;
            _isGroupLoggedIn = GroupSessions.DoesUserHaveGroupLoginSessionKey(_username);
        }
    }

    #region Build Admin Page Links

    public string BuildAdminPages(bool loadToNewPage, string category) {
        var appScript = new StringBuilder();
        if (!string.IsNullOrEmpty(_username)) {
            List<string> memberPages = _member.AdminPagesList.ToList();
            List<string> pagesList = ServerSettings.AdminPages();
            foreach (string p in pagesList) {
                if ((Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) || (memberPages.Contains(p))) {

                    if ((_username.ToLower() == ServerSettings.AdminUserName.ToLower() || _isGroupLoggedIn) && (p.ToLower() == "appinstaller" || p.ToLower() == "plugininstaller")) {
                        continue;
                    }

                    if (p.ToLower() == "workspace" && HttpContext.Current != null && HttpContext.Current.Request != null && 
                        HttpContext.Current.Request.RawUrl.ToLower().Contains("appremote.aspx")) {
                        continue;
                    }


                    appScript.Append(BuildLink(p.ToLower(), loadToNewPage, category));
                }
            }
        }

        return appScript.ToString();
    }
    private string BuildLink(string page, bool loadToNewPage, string category) {
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
                    if (!string.IsNullOrEmpty(category) && (string.IsNullOrEmpty(node["category"]) || node["category"] != category)) {
                        continue;
                    }

                    if (!CanCreateTabLink(node, _username)) {
                        continue;
                    }

                    string url = node.Url.Substring(node.Url.LastIndexOf('/') + 1);
                    string[] urlSplit = url.Split('.');
                    string tempUrl = string.Empty;
                    if (urlSplit.Length > 0) {
                        tempUrl = urlSplit[0].ToLower();
                    }

                    if (page.ToLower() == tempUrl) {
                        string icon = string.Empty;
                        if (!string.IsNullOrEmpty(node["icon"]) && !_ss.HideAllAppIcons) {
                            if (_showSiteIcons) {
                                string iconFile = ServerSettings.GetServerMapLocation + "Standard_Images\\" + node["icon"].Replace("/", "\\");
                                if (File.Exists(iconFile)) {
                                    icon = "<img alt='' src='" + ServerSettings.ResolveUrl("~/Standard_Images/" + node["icon"]) + "' />";
                                }
                            }
                        }

                        string toolTipText = string.Empty;
                        string toolTip = string.Empty;
                        if (!string.IsNullOrEmpty(node["tooltip"])) {
                            toolTipText = node["tooltip"];
                            toolTip = " title='" + toolTipText + "'";
                        }

                        string pageDescriptionNode = string.Empty;
                        if (_showPageDescription && !string.IsNullOrEmpty(toolTipText)) {
                            toolTip = string.Empty;
                            pageDescriptionNode = "<div class='page-description'>" + toolTipText + "</div>";
                        }

                        string title = node.Title;

                        if ((string.IsNullOrEmpty(node["nodisplaychildren"]) || node["nodisplaychildren"] == "false") && (node.HasChildNodes)) {
                            string expandBtn = "<span class='img-expand-sml' title='Expand links' onclick='openWSE.ExpandAdminLinks(this, \"" + tempUrl + "-sub-link\");return false;'></span>";

                            StringBuilder childNodes = new StringBuilder();
                            childNodes.Append("<a class='app-icon-links' href='" + RebuildUrlLink(node) + "'" + target + toolTip + ">" + icon + "<span class='app-icon-font' data-pagetitle='" + title + "'>" + expandBtn + title + pageDescriptionNode + "</span></a>");
                            childNodes.Append("<div class='app-icon-sub-link-holder " + tempUrl + "-sub-link' style='display: none;'>");

                            int totalFound = 0;
                            foreach (SiteMapNode childNode in node.ChildNodes) {
                                if (!CanCreateTabLink(childNode, _username)) {
                                    continue;
                                }

                                string iconChild = string.Empty;
                                if (!string.IsNullOrEmpty(childNode["icon"]) && !_ss.HideAllAppIcons) {
                                    if (_showSiteIcons) {
                                        string iconFile_Child = ServerSettings.GetServerMapLocation + "Standard_Images\\" + childNode["icon"].Replace("/", "\\");
                                        if (File.Exists(iconFile_Child)) {
                                            iconChild = "<img alt='' src='" + ServerSettings.ResolveUrl("~/Standard_Images/" + childNode["icon"]) + "' />";
                                        }
                                    }
                                }

                                string childToolTipText = string.Empty;
                                string childToolTip = string.Empty;
                                if (!string.IsNullOrEmpty(childNode["tooltip"])) {
                                    childToolTipText = childNode["tooltip"];
                                    childToolTip = " title='" + childToolTipText + "'";
                                }

                                string childPageDescriptionNode = string.Empty;
                                if (_showPageDescription && !string.IsNullOrEmpty(childToolTipText)) {
                                    childToolTip = string.Empty;
                                    childPageDescriptionNode = "<div class='page-description'>" + childToolTipText + "</div>";
                                }

                                string childTitle = childNode.Title;
                                childNodes.Append("<a class='app-icon-sub-links' href='" + RebuildUrlLink(childNode) + "'" + target + childToolTip + ">" + iconChild + "<span class='app-icon-font' data-pagetitle='" + childTitle + "'>" + childTitle + childPageDescriptionNode + "</span></a>");
                                totalFound++;
                            }

                            childNodes.Append("</div>");
                            if (totalFound > 0) {
                                appScript.Append(childNodes.ToString());
                            }
                            else {
                                appScript.Append("<a class='app-icon-links' href='" + RebuildUrlLink(node) + "'" + target + toolTip + ">" + icon + "<span class='app-icon-font' data-pagetitle='" + title + "'>" + title + pageDescriptionNode + "</span></a>");
                            }
                        }
                        else {
                            appScript.Append("<a class='app-icon-links' href='" + RebuildUrlLink(node) + "'" + target + toolTip + ">" + icon + "<span class='app-icon-font' data-pagetitle='" + title + "'>" + title + pageDescriptionNode + "</span></a>");
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

    private static string RebuildUrlLink(SiteMapNode node) {
        string url = node.Url;
        if (string.IsNullOrEmpty(node["overwriteevent"]) || !HelperMethods.ConvertBitToBoolean(node["overwriteevent"])) {
            if (url.Contains("?tab=")) {
                url = url.Replace("?tab=", "#?tab=");
            }
        }

        return url;
    }

    public const string EmptySidebarImg = "<div class='pad-all-sml float-left'></div>";
    public SortedDictionary<string, string> ListOfAdminPageCategories() {
        bool hideIcons = _ss.HideAllAppIcons;
        SortedDictionary<string, string> categories = new SortedDictionary<string, string>();
        SiteMapNodeCollection siteNodes = SiteMap.RootNode.ChildNodes;
        foreach (SiteMapNode node in siteNodes) {
            try {
                string category = node["category"];
                if (!string.IsNullOrEmpty(category) && !categories.ContainsKey(category)) {
                    string icon = _ss.GetSidebarCategoryIcon(category);
                    if (hideIcons) {
                        icon = string.Empty;
                    }
                    categories.Add(category, icon);
                }
            }
            catch { }
        }
        return categories;
    }

    public static bool CanCreateTabLink(SiteMapNode node, string currUser) {
        ServerSettings ss = new ServerSettings();
        if (node["adminuser-only"] != null && HelperMethods.ConvertBitToBoolean(node["adminuser-only"]) && currUser.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            return false;
        }

        if (node.Roles != null && node.Roles.Count > 0) {
            int foundInRoles = 0;
            foreach (var role in node.Roles) {
                if (Roles.IsUserInRole(currUser, role.ToString())) {
                    foundInRoles++;
                }
            }

            if (foundInRoles == 0) {
                return false;
            }
        }

        string nodeUrl = string.Empty;
        if (node.Url != null) {
            nodeUrl = node.Url.ToLower();
        }

        #region AppManager
        if (nodeUrl.Contains("appmanager.aspx")) {
            if ((nodeUrl.Contains("tab=easycreate") || nodeUrl.Contains("tab=upload")) && ss.LockAppCreator) {
                return false;
            }
        }
        #endregion

        #region AcctSettings
        if (nodeUrl.Contains("acctsettings.aspx") && !string.IsNullOrEmpty(currUser)) {
            if (currUser.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                if (nodeUrl.Contains("tab=pnl_workspaceoverlays")
                    || nodeUrl.Contains("tab=pnl_userappoverrides")
                    || nodeUrl.Contains("tab=pnl_workspacecontainer")
                    || nodeUrl.Contains("tab=pnl_iconselector")
                    || nodeUrl.Contains("tab=pnl_chatclient")) {
                    return false;
                }
            }
            else {
                string u = GroupSessions.GetUserGroupSessionName(currUser);
                if (u != currUser.ToLower() && nodeUrl.Contains("tab=")) {
                    return false;
                }
                else if (nodeUrl.Contains("tab=pnl_chatclient") && !ss.ChatEnabled) {
                    return false;
                }
                else if (nodeUrl.Contains("tab=pnl_workspacecontainer")) {
                    if (!ServerSettings.AdminPagesCheck("Workspace", currUser) && !Roles.IsUserInRole(currUser, ServerSettings.AdminUserName)) {
                        return false;
                    }
                    else {
                        MemberDatabase _member = new MemberDatabase(currUser);
                        string mode = _member.WorkspaceMode.ToString().ToLower();

                        if (string.IsNullOrEmpty(mode)) {
                            mode = MemberDatabase.UserWorkspaceMode.Complex.ToString().ToLower();
                        }

                        if (!MemberDatabase.IsComplexWorkspaceMode(mode)) {
                            return false;
                        }
                    }
                }
            }
        }
        #endregion

        return true;
    }

    #endregion

}