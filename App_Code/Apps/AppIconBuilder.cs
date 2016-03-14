using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.GroupOrganizer;

/// <summary>
/// Summary description for AppIconBuilder
/// </summary>
public class AppIconBuilder {

    #region Private Variables

    private readonly ServerSettings _ss = new ServerSettings();
    private Page _page;
    private MemberDatabase _member;
    private Dictionary<string, string> _demoMemberDatabase = new Dictionary<string,string>();

    private App _apps;
    private string _username;

    private bool AssociateWithGroups = false;
    private bool _onWorkspace = false;
    private int _totalWorkspaces = 4;
    private string _sitetheme;
    private string _workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();

    private PlaceHolder ph_iconList = null;
    private UpdatePanel updatePnl_AppList = null;
    private ContentPlaceHolder MainContent = null;

    #endregion


    #region Public Variables

    public List<Apps_Coll> UserAppList = new List<Apps_Coll>();
    public AppBuilder AppBuilderCaller;
    public bool InAtLeastOneGroup = false;

    #endregion


    #region Constructor/Initialization

    public AppIconBuilder(Page page, MemberDatabase member) {
        _page = page;
        _member = member;
        AssociateWithGroups = _ss.AssociateWithGroups;

        SetUserVars();
        SetPageControls();

        _apps = new App(_username);

        if (MainContent != null) {
            AppBuilderCaller = new AppBuilder(MainContent, _member);
        }
    }
    public AppIconBuilder(Page page, Dictionary<string, string> demoMemberDatabase) {
        _page = page;
        _demoMemberDatabase = demoMemberDatabase;
        AssociateWithGroups = _ss.AssociateWithGroups;

        _apps = new App("DemoNoLogin");

        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            SetDemoUserVars();
            SetPageControls();
        }

        if (MainContent != null) {
            AppBuilderCaller = new AppBuilder(MainContent, _demoMemberDatabase);
        }
    }

    private void SetUserVars() {
        _username = _member.Username;
        _workspaceMode = _member.WorkspaceMode.ToString();
        _totalWorkspaces = _member.TotalWorkspaces;
        _sitetheme = _member.SiteTheme;

        #region Set OnWorkspace Var
        if ((SiteMap.CurrentNode != null) && (HelperMethods.DoesPageContainStr("workspace.aspx"))) {
            _onWorkspace = true;
        }
        #endregion

        #region Check if in a group
        InAtLeastOneGroup = false;
        Groups groups = new Groups();
        List<string> ugArray = _member.GroupList;
        foreach (string g in ugArray) {
            if (!string.IsNullOrEmpty(groups.GetGroupName_byID(g))) {
                InAtLeastOneGroup = true;
                break;
            }
        }
        #endregion
    }
    private void SetDemoUserVars() {
        _workspaceMode = _demoMemberDatabase["WorkspaceMode"];
        if (string.IsNullOrEmpty(_workspaceMode)) {
            _workspaceMode = MemberDatabase.UserWorkspaceMode.Complex.ToString();
        }

        int.TryParse(_demoMemberDatabase["TotalWorkspaces"], out _totalWorkspaces);
        int totalAllowed = _ss.TotalWorkspacesAllowed;
        if (_totalWorkspaces > totalAllowed || _totalWorkspaces == 0) {
            _totalWorkspaces = totalAllowed;
        }

        if (!MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
            _totalWorkspaces = 1;
        }

        _sitetheme = _demoMemberDatabase["Theme"];

        #region Set OnWorkspace Var
        if ((SiteMap.CurrentNode != null) && (HelperMethods.DoesPageContainStr("workspace.aspx"))) {
            _onWorkspace = true;
        }
        #endregion
    }
    private void SetPageControls() {
        if (_page.Master != null) {
            ph_iconList = (PlaceHolder)_page.Master.FindControl("ph_iconList");
            updatePnl_AppList = (UpdatePanel)_page.Master.FindControl("updatePnl_AppList");
            MainContent = (ContentPlaceHolder)_page.Master.FindControl("MainContent");
        }
        else {
            ph_iconList = (PlaceHolder)_page.FindControl("ph_iconList");
            updatePnl_AppList = (UpdatePanel)_page.FindControl("updatePnl_AppList");
        }
    }

    #endregion


    #region Build App List

    public void BuildAppsForUser() {
        if (ph_iconList != null && updatePnl_AppList != null 
            && _member != null) {
            BuildApps();
        }
    }
    public void BuildAppsForDemo() {
        if (ph_iconList != null && updatePnl_AppList != null 
            && _demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            BuildApps_NoLogin();
        }
    }

    #endregion


    #region Build App Icons (Private)

    private int _totalApps = 0;
    private void BuildApps() {
        var appCategoryScript = new StringBuilder();
        var appScript = new StringBuilder();
        var categories = new List<string>();
        var appCategory = new AppCategory(false);
        bool groupIcons = _member.GroupIcons;

        if (groupIcons) {
            appCategory = new AppCategory(true);
            appCategoryScript.Append("<div id='Category-Back' style='display:none'>");
            appCategoryScript.Append("<span class='category-back-img'></span>");
            appCategoryScript.Append("<h4 id='Category-Back-btn' class='float-left'>" + "</h4>"); // Place 'Back' text if needed
            appCategoryScript.Append("<h4 id='Category-Back-Name' class='float-left'></h4></div>");
            appCategoryScript.Append("<div id='Category-Back-Name-id' style='display:none'></div>");
        }

        if (ph_iconList == null) {
            return;
        }

        if (!InAtLeastOneGroup) {
            ph_iconList.Controls.Add(new LiteralControl("<h3 class='pad-left pad-top-big pad-bottom-big pad-right'>Must be associated with a group</h3>"));
        }
        else {
            _apps.GetUserInstalledApps();
            bool hideAllIcons = _ss.HideAllAppIcons;

            List<string> memberApps = _apps.DeleteDuplicateEnabledApps(_member);

            foreach (var w in memberApps) {
                Apps_Coll dt = _apps.GetAppInformation(w);

                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckAppGroupAssociation(dt, _member)) {
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(dt.AppId)) {
                    _member.RemoveEnabledApp(w);
                    continue;
                }

                if ((_username.ToLower() != dt.CreatedBy.ToLower()) && (dt.IsPrivate)) {
                    continue;
                }

                UserAppList.Add(dt);

                int ar = 0;
                int am = 0;
                if (dt.AllowMaximize)
                    am = 1;
                if (dt.AllowResize)
                    ar = 1;

                string css = dt.CssClass;
                if (string.IsNullOrEmpty(css))
                    css = "app-main";

                if (!string.IsNullOrEmpty(dt.filename) && HelperMethods.IsValidAppFileType(dt.filename)) {
                    if (groupIcons) {
                        Dictionary<string, string> categoryList = appCategory.BuildCategoryDictionary(dt.Category);
                        foreach (KeyValuePair<string, string> categoryPair in categoryList) {
                            string cId = categoryPair.Key;
                            string categoryName = categoryPair.Value;

                            if (!categories.Contains(cId)) {
                                appCategoryScript.Append(BuildCategory(cId, categoryName));
                                categories.Add(cId);
                            }

                            appScript.Append("<div class='" + cId + " app-category-div' style='display: none'>");
                            appScript.Append(BuildIcon(dt.AppId, dt.filename, dt.Icon, cId, dt, dt.AppName, hideAllIcons));
                            appScript.Append("</div>");
                        }
                    }
                    else {
                        appScript.Append(BuildIcon(dt.AppId, dt.filename, dt.Icon, dt.Category, dt, dt.AppName, hideAllIcons));
                    }

                    if (_onWorkspace) {
                        // Build the app while building the icon
                        string workspace = _apps.GetCurrentworkspace(dt.AppId);
                        if (string.IsNullOrEmpty(workspace)) {
                            if (dt.DefaultWorkspace != "0") {
                                workspace = "workspace_" + dt.DefaultWorkspace;
                            }
                            else {
                                workspace = "workspace_1";
                            }
                        }

                        if (AppBuilderCaller != null) {
                            bool canBuild = true;
                            if (dt.AppId != _page.Request.QueryString["AppPage"] && !MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
                                canBuild = false;
                            }

                            if (canBuild) {
                                AppBuilderCaller.AppDivGenerator(dt.AppId, dt.AppName, dt.Icon, ar, am, css, workspace, dt.filename, HelperMethods.IsValidAscxFile(dt.filename),
                                                   dt.MinHeight, dt.MinWidth, dt.AllowPopOut,
                                                   dt.PopOutLoc, dt.AutoFullScreen, dt.AutoLoad, dt.AutoOpen, dt.AppBackgroundColor);
                            }
                        }
                    }
                    _totalApps++;
                }
            }

            ph_iconList.Controls.Clear();

            if (_totalApps > 0) {
                string xIcons = appScript.ToString();
                if (!string.IsNullOrEmpty(appCategoryScript.ToString())) {
                    xIcons = appCategoryScript.ToString() + xIcons;
                }

                ph_iconList.Controls.Add(new LiteralControl(xIcons));
            }
            else {
                ph_iconList.Controls.Add(new LiteralControl("<h3 class='pad-left pad-top-big pad-bottom-big'>No Apps Installed</h3>"));
            }
        }

        updatePnl_AppList.Update();
        AreAppIconLocked();

        RegisterPostbackScripts.RegisterStartupScript(_page, "openWSE.UpdateAppSelector();");

        if (_page.IsPostBack) {
            string registerJs = "openWSE.LoadCategoryCookies();";
            RegisterPostbackScripts.RegisterStartupScript(_page, registerJs);
        }
    }
    private void BuildApps_NoLogin() {
        var appCategoryScript = new StringBuilder();
        var appScript = new StringBuilder();
        var categories = new List<string>();
        var appCategory = new AppCategory(false);
        bool groupIcons = HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["GroupIcons"]);

        if (groupIcons) {
            appCategory = new AppCategory(true);
            appCategoryScript.Append("<div id='Category-Back' style='display:none'>");
            appCategoryScript.Append("<span class='category-back-img'></span>");
            appCategoryScript.Append("<h4 id='Category-Back-btn' class='float-left'>" + "</h4>"); // Place 'Back' text if needed
            appCategoryScript.Append("<h4 id='Category-Back-Name' class='float-left'></h4></div>");
            appCategoryScript.Append("<div id='Category-Back-Name-id' style='display:none'></div>");
        }

        if (ph_iconList == null) {
            return;
        }

        AppPackages package = new AppPackages(false);
        string[] appList = package.GetAppList(_demoMemberDatabase["AppPackage"]);

        bool hideAllIcons = _ss.HideAllAppIcons;
        foreach (var w in appList) {
            Apps_Coll dt = _apps.GetAppInformation(w);

            UserAppList.Add(dt);

            int ar = 0;
            int am = 0;
            if (dt.AllowMaximize)
                am = 1;
            if (dt.AllowResize)
                ar = 1;

            string css = dt.CssClass;
            if (string.IsNullOrEmpty(css))
                css = "app-main";

            if (!string.IsNullOrEmpty(dt.filename) && HelperMethods.IsValidAppFileType(dt.filename)) {
                if (groupIcons) {
                    Dictionary<string, string> categoryList = appCategory.BuildCategoryDictionary(dt.Category);
                    foreach (KeyValuePair<string, string> categoryPair in categoryList) {
                        string cId = categoryPair.Key;
                        string categoryName = categoryPair.Value;

                        if (!categories.Contains(cId)) {
                            appCategoryScript.Append(BuildCategory(cId, categoryName));
                            categories.Add(cId);
                        }

                        appScript.Append("<div class='" + cId + " app-category-div' style='display: none'>");
                        appScript.Append(BuildIcon(dt.AppId, dt.filename, dt.Icon, cId, dt, dt.AppName, hideAllIcons));
                        appScript.Append("</div>");
                    }
                }
                else {
                    appScript.Append(BuildIcon(dt.AppId, dt.filename, dt.Icon, dt.Category, dt, dt.AppName, hideAllIcons));
                }

                if (_onWorkspace) {
                    // Build the app while building the icon
                    string workspace = "workspace_1";
                    if (dt.DefaultWorkspace != "0") {
                        workspace = "workspace_" + dt.DefaultWorkspace;
                    }

                    if (AppBuilderCaller != null) {
                        bool canBuild = true;
                        if (dt.AppId != _page.Request.QueryString["AppPage"] && !MemberDatabase.IsComplexWorkspaceMode(_workspaceMode)) {
                            canBuild = false;
                        }

                        if (canBuild) {
                            AppBuilderCaller.AppDivGenerator_NoLogin(dt.AppId, dt.AppName, dt.Icon, ar, am, css, workspace, dt.filename, HelperMethods.IsValidAscxFile(dt.filename),
                                               dt.MinHeight, dt.MinWidth, dt.AllowPopOut,
                                               dt.PopOutLoc, dt.AutoFullScreen, dt.AutoLoad, dt.AutoOpen, dt.AppBackgroundColor);
                        }
                    }
                }

                _totalApps++;
            }
        }

        ph_iconList.Controls.Clear();

        if (_totalApps > 0) {
            string xIcons = appScript.ToString();
            if (!string.IsNullOrEmpty(appCategoryScript.ToString())) {
                xIcons = appCategoryScript.ToString() + xIcons;
            }

            ph_iconList.Controls.Add(new LiteralControl(xIcons));
        }
        else {
            ph_iconList.Controls.Add(new LiteralControl("<h3 class='pad-left pad-top-big pad-bottom-big'>No Apps Installed</h3>"));
        }

        updatePnl_AppList.Update();
        AreAppIconLocked();

        RegisterPostbackScripts.RegisterStartupScript(_page, "openWSE.UpdateAppSelector();");

        if (_page.IsPostBack && MainContent != null) {
            string registerJs = "openWSE.LoadCategoryCookies();";
            RegisterPostbackScripts.RegisterStartupScript(_page, registerJs);
        }
    }

    private string BuildIcon(string id, string filename, string iconname, string category, Apps_Coll dt, string w, bool hideAllIcons) {
        StringBuilder popup = new StringBuilder();
        var appScript = new StringBuilder();
        var appCategory = new AppCategory(false);
        popup.Append("<div class='app-popup'>");
        popup.Append("Workspace:<select class='app-popup-selector margin-left'>");
        popup.Append("<option>-</option>");
        for (int ii = 0; ii < _totalWorkspaces; ii++) {
            popup.Append("<option>" + (ii + 1).ToString() + "</option>");
        }
        popup.Append("</select></div>");

        #region App Description

        string appDescription = string.Empty;
        string tooltip = string.Empty;
        if (!string.IsNullOrEmpty(dt.Description)) {
            appDescription = dt.Description.Replace("'", "");
            tooltip = " title='" + appDescription + "'";
        }

        #endregion

        #region App Popout

        string canPopoutAttr = string.Empty;
        if ((!_onWorkspace) && (dt.AllowPopOut) && (!string.IsNullOrEmpty(dt.PopOutLoc)) && AppBuilderCaller != null) {
            canPopoutAttr = " popoutloc='" + AppBuilderCaller.CheckPopoutURL(dt.PopOutLoc) + "'";
        }

        #endregion

        #region Current Workspace

        string currWorkspace = string.Empty;
        if (!_onWorkspace && _member != null) {
            currWorkspace = " currentWorkspace='" + _member.CurrentWorkspace + "'";
        }

        #endregion

        #region App Selector Mode

        MemberDatabase.AppIconSelectorStyle tempAppStyle = MemberDatabase.AppIconSelectorStyle.Default;
        string iconStyleMode = string.Empty;
        string extraSpace = string.Empty;
        string fontColorStyle = string.Empty;
        string iconColorStyle = string.Empty;

        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0 && _demoMemberDatabase.ContainsKey("AppSelectorStyle") && !string.IsNullOrEmpty(_demoMemberDatabase["AppSelectorStyle"])) {
            Enum.TryParse<MemberDatabase.AppIconSelectorStyle>(_demoMemberDatabase["AppSelectorStyle"], out tempAppStyle);
        }
        else if (_member != null) {
            tempAppStyle = _member.AppSelectorStyle;
        }

        #region App Icon

        string iconImg = "<img alt='' src='" + _page.ResolveUrl("~/Standard_Images/App_Icons/" + iconname) + "' />";
        if (hideAllIcons) {
            iconImg = string.Empty;
        }
        else if (tempAppStyle != MemberDatabase.AppIconSelectorStyle.Icon_Only && tempAppStyle != MemberDatabase.AppIconSelectorStyle.Icon_And_Text_Only
            && tempAppStyle != MemberDatabase.AppIconSelectorStyle.Icon_And_Color_Only && tempAppStyle != MemberDatabase.AppIconSelectorStyle.Icon_Plus_Color_And_Text) {
            if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0 && HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["HideAppIcon"])) {
                iconImg = string.Empty;
            }
            else if (_member != null && _member.HideAppIcon) {
                iconImg = string.Empty;
            }
        }

        #endregion

        switch (tempAppStyle) {
            #region Color And Description
            case MemberDatabase.AppIconSelectorStyle.Color_And_Description:
                iconStyleMode = " " + tempAppStyle.ToString();
                extraSpace = "<div class='clear'></div>";
                string color = dt.IconBackgroundColor;
                if (!color.StartsWith("#")) {
                    color = "#" + color;
                }

                if (color == "#inherit") {
                    color = "#FFFFFF";
                }

                string fontColor = "#555555";
                Color _color = ColorTranslator.FromHtml(color);
                if (_color.R + _color.G + _color.B < 515) {
                    fontColor = "#FFFFFF";
                }

                iconColorStyle = "style='" + HelperMethods.GetCSSGradientStyles(color, string.Empty) + "border-bottom:1px solid rgba(0,0,0,0.15);'";
                fontColorStyle = "style='color: " + fontColor + "!important;'";

                if (!string.IsNullOrEmpty(appDescription)) {
                    extraSpace += "<div class='app-description' " + fontColorStyle + ">" + appDescription + "</div><div class='clear'></div>";
                }
                break;
            #endregion

            #region Name And Description
            case MemberDatabase.AppIconSelectorStyle.Name_And_Description:
                iconStyleMode = " " + tempAppStyle.ToString();
                extraSpace = "<div class='clear'></div>";
                if (!string.IsNullOrEmpty(appDescription)) {
                    extraSpace += "<div class='app-description'>" + appDescription + "</div><div class='clear'></div>";
                }
                break;
            #endregion

            #region Icon Only
            case MemberDatabase.AppIconSelectorStyle.Icon_Only:
                if (!string.IsNullOrEmpty(iconImg)) {
                    iconStyleMode = " " + tempAppStyle.ToString();
                    tooltip = " title='" + w + "'";
                }
                break;
            #endregion

            #region Icon And Text Only
            case MemberDatabase.AppIconSelectorStyle.Icon_And_Text_Only:
                if (!string.IsNullOrEmpty(iconImg)) {
                    iconStyleMode = " " + tempAppStyle.ToString();
                }
                break;
            #endregion

            #region Icon And Color Only
            case MemberDatabase.AppIconSelectorStyle.Icon_And_Color_Only:
                if (!string.IsNullOrEmpty(iconImg)) {
                    string colorBg = dt.IconBackgroundColor;
                    if (!colorBg.StartsWith("#")) {
                        colorBg = "#" + colorBg;
                    }

                    if (colorBg == "#inherit") {
                        colorBg = "#FFFFFF";
                    }

                    string ftColor = "#555555";
                    Color _ftColor = ColorTranslator.FromHtml(colorBg);
                    if (_ftColor.R + _ftColor.G + _ftColor.B < 515) {
                        ftColor = "#FFFFFF";
                    }

                    iconColorStyle = "style='" + HelperMethods.GetCSSGradientStyles(colorBg, string.Empty) + "border-bottom:1px solid rgba(0,0,0,0.15);'";
                    fontColorStyle = "style='color: " + ftColor + "!important;'";

                    iconStyleMode = " " + tempAppStyle.ToString();
                    tooltip = " title='" + w + "'";
                }
                break;
            #endregion

            #region Icon Plus Color And Text
            case MemberDatabase.AppIconSelectorStyle.Icon_Plus_Color_And_Text:
                if (!string.IsNullOrEmpty(iconImg)) {
                    string colorBg = dt.IconBackgroundColor;
                    if (!colorBg.StartsWith("#")) {
                        colorBg = "#" + colorBg;
                    }

                    if (colorBg == "#inherit") {
                        colorBg = "#FFFFFF";
                    }

                    string ftColor = "#555555";
                    Color _ftColor = ColorTranslator.FromHtml(colorBg);
                    if (_ftColor.R + _ftColor.G + _ftColor.B < 515) {
                        ftColor = "#FFFFFF";
                    }

                    iconColorStyle = "style='" + HelperMethods.GetCSSGradientStyles(colorBg, string.Empty) + "border-bottom:1px solid rgba(0,0,0,0.15);'";
                    fontColorStyle = "style='color: " + ftColor + "!important;'";

                    iconStyleMode = " " + tempAppStyle.ToString();
                }
                break;
            #endregion
        }

        #endregion

        #region App Background Color Info

        string data_AppBGColor = string.Empty;
        if (_page != null && _page.ToString().ToLower().Contains("appremote") && !string.IsNullOrEmpty(dt.AppBackgroundColor) && dt.AppBackgroundColor.ToLower() != "inherit") {
            string appBGColor = dt.AppBackgroundColor;
            if (!appBGColor.StartsWith("#")) {
                appBGColor = "#" + appBGColor;
            }

            data_AppBGColor = " data-appbgcolor='" + appBGColor + "'";
        }

        #endregion

        if (MainContent != null) {
            string needtoload = "0";
            if (HelperMethods.IsValidAscxFile(filename)) {
                needtoload = "1";
                if (_apps.GetAutoLoad(id)) {
                    needtoload = "0";
                    appScript.Append("<input type='hidden' id='hf_" + id + "' />");
                }
            }

            appScript.Append("<div" + canPopoutAttr + currWorkspace + " data-appId='" + id + "'" + data_AppBGColor + " class='app-icon" + iconStyleMode + "'" + tooltip + " onclick=\"openWSE.DetermineNeedPostBack(this, " + needtoload + ")\" " + iconColorStyle + ">");
            if (_totalWorkspaces > 1) {
                appScript.Append("<span class='app-options' style='visibility: hidden;'>" + popup + "</span>");
            }
            appScript.Append(iconImg + "<span class='app-icon-font' " + fontColorStyle + ">" + w + "</span>" + extraSpace + "</div>");
        }
        else {
            appScript.Append("<div data-appid='" + id + "'" + data_AppBGColor + " class='app-icon" + iconStyleMode + "'" + tooltip + " onclick=\"appRemote.LoadOptions('" + id + "', '" + w + "', true)\" " + iconColorStyle + ">");
            appScript.Append(iconImg);
            appScript.Append("<span class='app-icon-font' " + fontColorStyle + ">" + w + "</span>" + extraSpace + "</div>");
        }

        // Need to add a div to clear make sure the parent div has the correct height for categorizing icons
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0 && _demoMemberDatabase.ContainsKey("GroupIcons") && HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["GroupIcons"])) {
            appScript.Append("<div class='clear'></div>");
        }
        else if (_member != null && _member.GroupIcons) {
            appScript.Append("<div class='clear'></div>");
        }

        return appScript.ToString();
    }
    private string BuildCategory(string id, string category) {
        var str = new StringBuilder();

        int categoryCount = 0;
        if (id == category && id != AppCategory.Uncategorized_Name) {
            categoryCount = GetAppCount_Category("");
        }
        else if (id == AppCategory.Uncategorized_Name) {
            categoryCount = GetAppCount_Category_Uncategorized();
        }
        else {
            categoryCount = GetAppCount_Category(id);
        }


        if (categoryCount > 0) {
            string count = string.Empty;
            if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
                if (HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["IconCategoryCount"]))
                    count = " (" + categoryCount + ")";
            }
            else {
                if (_member.ShowCategoryCount)
                    count = " (" + categoryCount + ")";
            }

            if (MainContent != null) {
                str.Append("<div data-appId='" + id + "' class='app-icon-category-list' onclick=\"openWSE.CategoryClick('" + id + "', '" + category + "')\">");
                str.Append("<span class='app-icon-font'>" + category + count + "</span>");
                str.Append("<span class='app-category-nextpage'></span></div>");
            }
            else {
                str.Append("<div data-appId='" + id + "' class='app-icon-category-list' onclick=\"appRemote.CategoryClick('" + id + "', '" + category + "', true)\">");
                str.Append("<span class='app-icon-font'>" + category + count + "</span>");
                str.Append("<span class='app-category-nextpage'></span></div>");
            }
        }
        return str.ToString();
    }
    private int GetAppCount_Category(string id) {
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            AppPackages package = new AppPackages(false);
            string[] appList = package.GetAppList(_demoMemberDatabase["AppPackage"]);

            List<string> tempList = appList.ToList();
            return _apps.GetApps_byCategoryForSidebar(id).Cast<Apps_Coll>().Count(dr => tempList.Contains(dr.AppId));
        }
        else {
            List<string> userapps = _member.EnabledApps;
            return _apps.GetApps_byCategoryForSidebar(id).Cast<Apps_Coll>().Count(dr => userapps.Contains(dr.AppId));
        }

        return 0;
    }
    private int GetAppCount_Category_Uncategorized() {
        int count = 0;

        List<string> appColl = new List<string>();
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            AppPackages package = new AppPackages(false);
            string[] appList = package.GetAppList(_demoMemberDatabase["AppPackage"]);
            appColl = appList.ToList();
        }
        else {
            appColl = _member.EnabledApps;
        }

        AppCategory app_category = new AppCategory(false);

        foreach (string app in appColl) {
            Dictionary<string, string> categoryList = _apps.GetCategoriesForApp(app);
            if (categoryList.Count == 0) {
                count++;
            }
            else {
                foreach (KeyValuePair<string, string> categoryItem in categoryList) {
                    if (categoryItem.Key == AppCategory.Uncategorized_Name || categoryItem.Value == AppCategory.Uncategorized_Name) {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    private void AreAppIconLocked() {
        bool appsLocked = true;
        string canSave = "false";
        if (_demoMemberDatabase != null && _demoMemberDatabase.Count > 0) {
            if (!HelperMethods.ConvertBitToBoolean(_demoMemberDatabase["LockAppIcons"]))
                appsLocked = false;
        }
        else {
            canSave = "true";
            if (!_member.LockAppIcons) {
                appsLocked = false;
            }
        }

        if (!appsLocked) {
            if (MainContent != null) {
                RegisterPostbackScripts.RegisterStartupScript(_page, "openWSE.AppsSortUnlocked(" + canSave + ");");
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(_page, "appRemote.AppsSortUnlocked(" + canSave + ");");
            }
        }
    }

    #endregion

}