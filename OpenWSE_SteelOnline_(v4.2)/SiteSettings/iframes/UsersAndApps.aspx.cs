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

public partial class SiteSettings_UsersAndApps : System.Web.UI.Page
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
        string str = AcctSettings.LoadUserBackground(userName, member.SiteTheme, this.Page);
        if (!string.IsNullOrEmpty(str)) {
            RegisterPostbackScripts.RegisterStartupScript(this, str);
        }
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
                lbl_title.Text = "Users and Apps";
                lbl_note.Text = "<span class='pad-right-sml font-bold'>Note:</span>This is a non-editable list.";
                LoadAppsAndUsers();
                LoadUserBackground(_username);
            }
            else
            {
                lbl_title.Text = forUser + " App Edit";
                lbl_note.Text = "<span class='pad-right-sml font-bold'>Note:</span>Add/Remove apps for the specified user.";
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

        str.Append("<table class='myHeaderStyle' style='width:100%' cellpadding='0' cellspacing='0'>");
        str.Append("<tr><td width='200px' valign='middle'>UserName</td>");
        str.Append("<td>Apps Installed</td></tr></table>");

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
                    str.Append("<table class='myItemStyle GridNormalRow' style='width:100%' cellpadding='0' cellspacing='0'>");

                    string imgColor = UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 40);
                    string userNameTitle = "<div class='float-left pad-left-sml' style='margin-top: -2px;'><h3>" + HelperMethods.MergeFMLNames(member) + "</h3><div class='clear-space-two'></div>" + member.Username + "</div>";

                    str.Append("<tr><td width='200px' valign='middle' style='border-right: 1px solid #CCC;'>" + imgColor + userNameTitle + "</td>");
                    str.Append("<td><div class='pad-all'>" + LoadAppIcons(member, apps) + "</div></td></tr></table>");
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

        foreach (string w in appList.Where(w => apps.IconExists(w)))
        {
            Apps_Coll coll = apps.GetAppInformation(w);

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(coll, member)) {
                    continue;
                }
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
            str.Append("<table class='myHeaderStyle' style='width:100%' cellpadding='0' cellspacing='0'>");
            str.Append("<tr><td width='200px' valign='middle'>UserName</td>");
            str.Append("<td>Apps Installed</td></tr></table>");
            str.Append("<table class='myItemStyle GridNormalRow' style='width:100%' cellpadding='0' cellspacing='0'>");

            string imgColor = UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 40);
            string userNameTitle = "<div class='float-left pad-left-sml' style='margin-top: -2px;'><h3>" + HelperMethods.MergeFMLNames(member) + "</h3><div class='clear-space-two'></div>" + member.Username + "</div>";

            str.Append("<tr><td width='200px' valign='middle' style='border-right: 1px solid #CCC;'>" + imgColor + userNameTitle + "</td>");
            str.Append("<td><div class='pad-all'>");
            str.Append("<span class='float-left pad-right font-bold pad-top-sml'>App Package:</span><select id='ddl_appPackages' class='float-left margin-right'>");

            AppPackages wp = new AppPackages(true);
            List<Dictionary<string, string>> dt = wp.listdt;
            foreach (Dictionary<string, string> dr in dt)
            {
                string packId = dr["ID"];
                string packName = dr["PackageName"];
                str.Append("<option value='" + packId + "'>" + packName + "</option>");
            }

            str.Append("</select><input type='button' class='float-left input-buttons' value='Install Package' onclick='AppPackageInstall()' />");
            str.Append("<a href='#' class='float-right sb-links' onclick='RemoveAllApp();return false;'>Uninstall All Apps</a>");
            str.Append("<div class='clear' style='height: 20px;'></div>" + LoadAppIconsEdit(member, apps) + "</div></td></tr></table>");
        }

        pnl_List.Controls.Clear();
        if (!string.IsNullOrEmpty(str.ToString()))
            pnl_List.Controls.Add(new LiteralControl(str.ToString()));
        else
            pnl_List.Controls.Add(new LiteralControl("User has privacy enabled. Cannot edit apps."));
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

            if ((_username.ToLower() != dr.CreatedBy.ToLower()) && (dr.IsPrivate)) {
                continue;
            }

            string appName = dr.AppName;
            string appId = dr.AppId;
            string iconname = dr.Icon;

            if (member.UserHasApp(appId)) {
                appScriptInstalled.Append("<div class='app-icon-admin inline-block' style='padding: 5px 0 !important;'>");
                appScriptInstalled.Append("<a href='#' class='img-collapse-sml cursor-pointer float-left' title='Uninstall " + appName + "' onclick='RemoveApp(\"" + appId + "\");return false;' style='margin-right: 5px; margin-top: 4px;'></a>");
                if (!hideIcon) {
                    appScriptInstalled.Append("<img alt='' src='../../Standard_Images/App_Icons/" + iconname + "' class='float-left pad-right' style='height: 20px;' />");
                }
                appScriptInstalled.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; padding: 2px 0 0 0 !important; width: 230px;'>" + appName + "</span>");
                appScriptInstalled.Append("</div>");
            }
            else {
                appScriptUninstalled.Append("<div class='app-icon-admin inline-block' style='padding: 5px 0 !important;'>");
                appScriptUninstalled.Append("<a href='#' class='img-expand-sml cursor-pointer float-left' title='Install " + appName + "' onclick='AddApp(\"" + appId + "\");return false;' style='margin-right: 5px; margin-top: 4px;'></a>");
                if (!hideIcon) {
                    appScriptUninstalled.Append("<img alt='' src='../../Standard_Images/App_Icons/" + iconname + "' class='float-left pad-right' style='height: 20px;' />");
                }
                appScriptUninstalled.Append("<span class='app-span-modify font-color-light-black' style='text-align: left; padding: 2px 0 0 0 !important; width: 230px;'>" + appName + "</span>");
                appScriptUninstalled.Append("</div>");
            }
        }

        return appScriptInstalled.ToString() + "<div class='clear' style='height: 30px;'></div>" + appScriptUninstalled.ToString();
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
}