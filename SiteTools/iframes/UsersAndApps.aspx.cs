using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Collections.Specialized;
using System.Web.Security;
using System.Text;
using System.Data;

public partial class SiteTools_UsersAndApps : System.Web.UI.Page
{
    #region Variables
    private App apps;
    private ServerSettings _ss = new ServerSettings();
    private readonly AppLog _applog = new AppLog(false);
    private IIdentity _userId;
    private string _username;
    private string _ctrlname;
    private bool AssociateWithGroups = false;
    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userID = HttpContext.Current.User.Identity;
        PageLoadInit pageLoadInit = new PageLoadInit(this.Page, userID, IsPostBack, _ss.NoLoginRequired);
        if (pageLoadInit.CanLoadPage)
        {
            ScriptManager sm = ScriptManager.GetCurrent(Page);
            if (sm != null)
            {
                string ctlId = sm.AsyncPostBackSourceElementID;
                _ctrlname = ctlId;
            }
            StartUpPage(userID);
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }
    private void LoadUserBackground(string userName) {
        MemberDatabase member = new MemberDatabase(userName);
        AcctSettings.LoadUserBackground(this.Page, null, member);
    }

    private void StartUpPage(IIdentity userId)
    {
        _userId = userId;
        _username = _userId.Name;
        if ((Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) || (ServerSettings.AdminPagesCheck("useraccounts.aspx", _username)))
        {
            AssociateWithGroups = _ss.AssociateWithGroups;

            GetStartupScripts_JS();
            string forUser = Request.QueryString["u"];
            if (string.IsNullOrEmpty(forUser))
            {
                lbl_title.Text = "User List of Apps and Plugins";
                LoadAppsAndUsers();
                LoadUserBackground(_username);
            }
            else
            {
                lbl_title.Text = forUser + " | App and Plugin Installer";
                LoadUsersApps(forUser);
                LoadUserBackground(forUser);
            }
        }
        else
        {
            var applog = new AppLog(false);
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }
    }


    #region Dynamically Load Scripts

    private void GetStartupScripts_JS()
    {
        var startupscripts = new StartupScripts(true);
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList)
        {
            if (coll.ApplyTo == "All Components")
            {
                var sref = new ScriptReference(coll.ScriptPath);
                if (sm != null)
                    sm.Scripts.Add(sref);
            }
        }
        if (sm != null) sm.ScriptMode = ScriptMode.Release;
    }

    #endregion


    private void LoadAppsAndUsers()
    {
        pnl_List.Controls.Clear();

        StringBuilder str = new StringBuilder();
        MembershipUserCollection coll = Membership.GetAllUsers();
        int count = 0;
        foreach (MembershipUser u in coll)
        {
            if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower())
            {
                MemberDatabase member = new MemberDatabase(u.UserName);
                bool canContinue = true;

                if (_ss.AllowPrivacy)
                {
                    if ((member.PrivateAccount) && (u.UserName.ToLower() != _username.ToLower())
                        && (_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && ((!u.IsLockedOut) || (u.IsApproved)))
                    {
                        canContinue = false;
                    }
                }

                if (canContinue)
                {
                    count++;
                    apps = new App(u.UserName);
                    str.Append("<table style='width:100%' cellpadding='0' cellspacing='0'>");

                    string imgColor = UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 30);
                    string userNameTitle = "<div class='float-left pad-left-sml' style='margin-top: -2px;'><h3>" + HelperMethods.MergeFMLNames(member) + "</h3><div class='clear-space-two'></div><span>" + member.Username + "</span></div>";

                    str.Append("<tr><td width='200px' valign='middle' class='border-right'><div class='clear' style='height: 20px;'></div>" + imgColor + userNameTitle + "</td>");
                    str.Append("<td class='pad-left-big'><div class='pad-all'><b>Apps</b><div class='clear-space'></div>" + LoadAppIcons(member, apps) + "</div></td></tr>");
                    str.Append("<tr><td class='border-right'></td><td class='pad-left-big'><div class='pad-all'><div class='pad-top-big'><b>Plugins</b><div class='clear-space'></div>" + LoadPlugins(member) + "</div></div></td></tr></table>");

                    str.Append("<div class='pad-all-big border-bottom'></div>");
                }
            }
        }

        if (count == 0) {
            str.Append("<div class='emptyGridView'>No Users Found</div>");
        }

        pnl_List.Controls.Clear();
        pnl_List.Controls.Add(new LiteralControl(str.ToString()));
    }
    private string LoadAppIcons(MemberDatabase member, App apps)
    {
        var appScript = new StringBuilder();
        List<string> appList = member.EnabledApps;

        foreach (string w in appList)
        {
            Apps_Coll coll = apps.GetAppInformation(w);

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(coll, member)) {
                    continue;
                }
            }

            if (string.IsNullOrEmpty(coll.ID)) {
                continue;
            }
            if ((_username.ToLower() != coll.CreatedBy.ToLower()) && (coll.IsPrivate)) {
                continue;
            }

            appScript.Append("<div class='app-icon-admin inline-block' style='padding: 4px 0 !important;'>");

            if (!_ss.HideAllAppIcons)
                appScript.Append("<img alt='' src='../../Standard_Images/App_Icons/" + coll.Icon + "' class='float-left pad-right' style='height: 20px;' />");

            appScript.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; ");
            appScript.Append("padding: 2px 0 0 0 !important; width: 185px;'>" + coll.AppName + "</span></div>");
        }

        return !string.IsNullOrEmpty(appScript.ToString())
                   ? appScript.ToString()
                   : "<div class='float-left'>No apps installed for user</div>";
    }
    private string LoadPlugins(MemberDatabase member) {
        StringBuilder pluginScriptInstalled = new StringBuilder();

        string username = member.Username;

        SitePlugins _plugins = new SitePlugins(username);
        _plugins.BuildSitePlugins(true);
        _plugins.BuildSitePluginsForUser();

        bool AssociateWithGroups = _ss.AssociateWithGroups;
        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckPluginGroupAssociation(coll, member)) {
                        continue;
                    }
                }

                bool isInstalled = false;
                foreach (UserPlugins_Coll userColl in _plugins.userplugins_dt) {
                    if (userColl.PluginID == coll.ID) {
                        isInstalled = true;
                        break;
                    }
                }

                if (isInstalled) {
                    pluginScriptInstalled.Append("<div class='app-icon-admin inline-block' style='padding: 5px 0 !important;'>");
                    pluginScriptInstalled.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; padding: 2px 0 0 0 !important; width: 230px;'>" + coll.PluginName + "</span>");
                    pluginScriptInstalled.Append("</div>");
                }
            }
        }

        return !string.IsNullOrEmpty(pluginScriptInstalled.ToString())
                   ? pluginScriptInstalled.ToString()
                   : "<div class='float-left'>No plugins installed for user</div>";
    }

    private void LoadUsersApps(string user)
    {
        StringBuilder str = new StringBuilder();

        MembershipUser u = Membership.GetUser(user);
        MemberDatabase member = new MemberDatabase(u.UserName);
        bool canContinue = true;

        if (_ss.AllowPrivacy)
        {
            if ((member.PrivateAccount) && (u.UserName.ToLower() != _username.ToLower())
                && (_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && ((!u.IsLockedOut) || (u.IsApproved)))
            {
                canContinue = false;
            }
        }

        if (canContinue)
        {
            apps = new App(u.UserName);
            str.Append("<table style='width:100%' cellpadding='0' cellspacing='0'>");

            string imgColor = UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 30);
            string userNameTitle = "<div class='float-left pad-left-sml' style='margin-top: -2px;'><h3>" + HelperMethods.MergeFMLNames(member) + "</h3><div class='clear-space-two'></div><span>" + member.Username + "</span></div>";

            str.Append("<tr><td width='200px' valign='top' class='border-right'><div class='clear' style='height: 20px;'></div>" + imgColor + userNameTitle + "</td>");
            str.Append("<td class='pad-left-big'><div class='pad-all'>");
            str.Append("<span class='float-left pad-right font-bold pad-top-sml'>App Package</span><select id='ddl_appPackages' class='float-left margin-right'>");

            AppPackages wp = new AppPackages(true);
            List<Dictionary<string, string>> dt = wp.listdt;
            foreach (Dictionary<string, string> dr in dt)
            {
                string packId = dr["ID"];
                string packName = dr["PackageName"];
                str.Append("<option value='" + packId + "'>" + packName + "</option>");
            }

            str.Append("</select><input type='button' class='float-left input-buttons' value='Install Package' onclick='AppPackageInstall()' />");
            str.Append("<a href='#' class='float-right' onclick='RemoveAllApp();return false;'>Uninstall All Apps</a>");
            str.Append("<div class='clear' style='height: 20px;'></div>" + LoadAppIconsEdit(member, apps) + "</div></td></tr>");
            str.Append("<tr><td class='border-right'></td><td class='pad-left-big'><div class='pad-all'><div class='pad-top-big border-top'><div class='clear-space'></div>");
            str.Append("<a href='#' class='float-right' onclick='RemoveAllPlugins();return false;'>Uninstall All Plugins</a>");
            str.Append("<div class='clear' style='height: 20px;'></div>");
            str.Append(LoadPluginListEdit(member) + "</div></div></td></tr></table>");
        }

        pnl_List.Controls.Clear();

        if (!string.IsNullOrEmpty(str.ToString())) {
            pnl_List.Controls.Add(new LiteralControl(str.ToString()));
        }
        else {
            pnl_List.Controls.Add(new LiteralControl("User has privacy enabled. Cannot edit apps."));
        }
    }
    private string LoadAppIconsEdit(MemberDatabase member, App apps)
    {
        var appScriptInstalled = new StringBuilder();
        var appScriptUninstalled = new StringBuilder();

        bool hideIcon = _ss.HideAllAppIcons;

        apps.GetAllApps();
        var dt = apps.AppList;
        foreach (Apps_Coll dr in dt)
        {
            if (AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(dr, member)) {
                    continue;
                }
            }

            if (string.IsNullOrEmpty(dr.ID)) {
                continue;
            }

            if ((member.Username.ToLower() != dr.CreatedBy.ToLower()) && (dr.IsPrivate)) {
                continue;
            }

            string appName = dr.AppName;
            string appId = dr.AppId;
            string iconname = dr.Icon;

            if (member.UserHasApp(appId)) {
                appScriptInstalled.Append("<div class='app-icon-admin inline-block' style='padding: 5px 0 !important;'>");
                if (!hideIcon) {
                    appScriptInstalled.Append("<img alt='' src='../../Standard_Images/App_Icons/" + iconname + "' class='float-left pad-right' style='height: 20px;' />");
                }
                appScriptInstalled.Append("<a href='#' class='img-collapse-sml cursor-pointer float-left' title='Uninstall " + appName + "' onclick='RemoveApp(\"" + appId + "\");return false;' style='margin-right: 5px; margin-top: 4px;'></a>");
                appScriptInstalled.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; padding: 2px 0 0 0 !important; width: 230px;'>" + appName + "</span>");
                appScriptInstalled.Append("</div>");
            }
            else {
                appScriptUninstalled.Append("<div class='app-icon-admin inline-block' style='padding: 5px 0 !important;'>");
                if (!hideIcon) {
                    appScriptUninstalled.Append("<img alt='' src='../../Standard_Images/App_Icons/" + iconname + "' class='float-left pad-right' style='height: 20px;' />");
                }
                appScriptUninstalled.Append("<a href='#' class='img-expand-sml cursor-pointer float-left' title='Install " + appName + "' onclick='AddApp(\"" + appId + "\");return false;' style='margin-right: 5px; margin-top: 4px;'></a>");
                appScriptUninstalled.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; padding: 2px 0 0 0 !important; width: 230px;'>" + appName + "</span>");
                appScriptUninstalled.Append("</div>");
            }
        }

        return HelperMethods.TableAddRemove(appScriptUninstalled.ToString(), appScriptInstalled.ToString(), "Apps Available to Install", "Apps Installed", false, true);
    }
    private string LoadPluginListEdit(MemberDatabase member) {
        StringBuilder pluginScriptUninstalled = new StringBuilder();
        StringBuilder pluginScriptInstalled = new StringBuilder();

        string username = member.Username;

        SitePlugins _plugins = new SitePlugins(username);
        _plugins.BuildSitePlugins(true);
        _plugins.BuildSitePluginsForUser();

        bool AssociateWithGroups = _ss.AssociateWithGroups;
        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                if (AssociateWithGroups) {
                    if (!ServerSettings.CheckPluginGroupAssociation(coll, member)) {
                        continue;
                    }
                }

                string userPluginID = "";
                bool isInstalled = false;
                foreach (UserPlugins_Coll userColl in _plugins.userplugins_dt) {
                    if (userColl.PluginID == coll.ID) {
                        userPluginID = userColl.ID;
                        isInstalled = true;
                        break;
                    }
                }

                if (isInstalled) {
                    pluginScriptInstalled.Append("<div class='app-icon-admin inline-block' style='padding: 5px 0 !important;'>");
                    pluginScriptInstalled.Append("<a href='#' class='img-collapse-sml cursor-pointer float-left' title='Uninstall " + coll.PluginName + " Plugin' onclick='RemovePlugin(\"" + userPluginID + "\");return false;' style='margin-right: 5px; margin-top: 4px;'></a>");
                    pluginScriptInstalled.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; padding: 2px 0 0 0 !important; width: 230px;'>" + coll.PluginName + "</span>");
                    pluginScriptInstalled.Append("</div>");
                }
                else {
                    pluginScriptUninstalled.Append("<div class='app-icon-admin inline-block' style='padding: 5px 0 !important;'>");
                    pluginScriptUninstalled.Append("<a href='#' class='img-expand-sml cursor-pointer float-left' title='Install " + coll.PluginName + " Plugin' onclick='AddPlugin(\"" + coll.ID + "\");return false;' style='margin-right: 5px; margin-top: 4px;'></a>");
                    pluginScriptUninstalled.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; padding: 2px 0 0 0 !important; width: 230px;'>" + coll.PluginName + "</span>");
                    pluginScriptUninstalled.Append("</div>");
                }
            }
        }

        return HelperMethods.TableAddRemove(pluginScriptUninstalled.ToString(), pluginScriptInstalled.ToString(), "Plugins Available to Install", "Plugins Installed", false, true);
    }

    protected void hf_addApp_Changed(object sender, EventArgs e)
    {
        string forUser = Request.QueryString["u"];

        if (!string.IsNullOrEmpty(hf_addApp.Value))
        {
            MemberDatabase member = new MemberDatabase(forUser);
            member.UpdateEnabledApps(hf_addApp.Value);
            LoadUsersApps(forUser);
        }

        hf_addApp.Value = string.Empty;
    }
    protected void hf_removeApp_Changed(object sender, EventArgs e)
    {
        string forUser = Request.QueryString["u"];

        if (!string.IsNullOrEmpty(hf_removeApp.Value))
        {
            MemberDatabase member = new MemberDatabase(forUser);
            member.RemoveEnabledApp(hf_removeApp.Value);
            LoadUsersApps(forUser);
        }

        hf_removeApp.Value = string.Empty;
    }
    protected void hf_removeAllApp_Changed(object sender, EventArgs e)
    {
        string forUser = Request.QueryString["u"];

        MemberDatabase member = new MemberDatabase(forUser);
        member.RemoveAllEnabledApps();
        LoadUsersApps(forUser);

        hf_removeAllApp.Value = string.Empty;
    }
    protected void hf_appPackage_Changed(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(hf_appPackage.Value))
        {
            string forUser = Request.QueryString["u"];

            MemberDatabase member = new MemberDatabase(forUser);
            AppPackages wp = new AppPackages(false);
            string[] wpList = wp.GetAppList(hf_appPackage.Value);

            foreach (string appId in wpList)
            {
                if (!member.UserHasApp(appId))
                    member.UpdateEnabledApps(appId);
            }

            LoadUsersApps(forUser);
        }

        hf_appPackage.Value = string.Empty;
    }
    protected void hf_removePlugin_ValueChanged(object sender, EventArgs e) {
        string forUser = Request.QueryString["u"];

        if (!string.IsNullOrEmpty(hf_removePlugin.Value)) {
            SitePlugins _plugins = new SitePlugins(forUser);
            _plugins.deleteItemForUser(hf_removePlugin.Value);
            LoadUsersApps(forUser);
        }

        hf_removePlugin.Value = string.Empty;
    }
    protected void hf_addPlugin_ValueChanged(object sender, EventArgs e) {
        string forUser = Request.QueryString["u"];

        if (!string.IsNullOrEmpty(hf_addPlugin.Value)) {
            SitePlugins _plugins = new SitePlugins(forUser);
            _plugins.addItemForUser(hf_addPlugin.Value);
            LoadUsersApps(forUser);
        }

        hf_addPlugin.Value = string.Empty;
    }
    protected void hf_removeAllPlugins_ValueChanged(object sender, EventArgs e) {
        string forUser = Request.QueryString["u"];

        if (!string.IsNullOrEmpty(forUser)) {
            SitePlugins _plugins = new SitePlugins(forUser);
            _plugins.BuildSitePluginsForUser();
            foreach (UserPlugins_Coll userColl in _plugins.userplugins_dt) {
                _plugins.deleteItemForUser(userColl.ID);
            }

            LoadUsersApps(forUser);
        }

        hf_removeAllPlugins.Value = string.Empty;
    }
}