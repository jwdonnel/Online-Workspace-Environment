using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Xml;
using System.Web.UI.HtmlControls;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;
using SocialSignInApis;
using OpenWSE_Tools.Overlays;
using OpenWSE_Tools.AppServices;

public partial class SiteMaster : BaseMaster {

    #region private variables

    private StringBuilder PostBackScriptString = new StringBuilder();
    private bool AssociateWithGroups = false;

    #endregion


    #region Authenticate user and load page

    protected void Page_Init(object sender, EventArgs e) {
        CheckIfDemoOrPreview();

        if (NoLoginRequired && !UserIsAuthenticated) {
            #region Demo User
            user_profile_tab.Visible = false;
            notifications_tab.Visible = false;
            pnl_chat_users.Enabled = false;
            pnl_chat_users.Visible = false;

            if (DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.Count > 0) {
                if (!IsPostBack || string.IsNullOrEmpty(BaseMaster.GetPostBackControlId(Page))) {
                    if (!HelperMethods.DoesPageContainStr("default.aspx", Page)) {
                        HelperMethods.PageRedirect("~/Default.aspx");
                    }

                    LoadDemo();
                }
            }
            #endregion
        }
        else if (Request.RawUrl.ToLower().Contains("sitesettings.aspx") && ServerSettings.NeedToLoadAdminNewMemberPage) {
            #region Admin User New Setup
            FormsAuthentication.SetAuthCookie(ServerSettings.AdminUserName, true);
            top_bar.Style["display"] = "none";
            sidebar_container.Style["display"] = "none";
            main_container.Style["display"] = "none";
            footer_container.Style["display"] = "none";
            RegisterPostbackScripts.RegisterStartupScript(this, PostBackScriptString.ToString() + "openWSE_Config.minPasswordLength=" + Membership.MinRequiredPasswordLength + ";openWSE.HelpOverlay(true);");
            #endregion
        }
        else {
            #region Current User
            if (CurrentUserMembership == null) {
                FormsAuthentication.SignOut();
                HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
            }

            if (!UserIsAuthenticated || CurrentUserMembership.IsLockedOut || !CurrentUserMembership.IsApproved) {
                HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
            }
            else {
                if (!CurrentUserMemberDatabase.IsNewMember) {
                    if (!IsPostBack) {
                        LoadUser();
                    }
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, PostBackScriptString.ToString() + "openWSE_Config.minPasswordLength=" + Membership.MinRequiredPasswordLength + ";openWSE.HelpOverlay(true);");
                }
            }
            #endregion
        }

        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["fullscreen"])) {
            top_bar.Visible = false;
            RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function() { $(window).resize(); }, 1); $('#footer-signdate').remove();");
        }

        ltl_footercopyright.Text = HelperMethods.GetCopyrightFooterText();
    }
    private void LoadUser() {
        if (CurrentUserMemberDatabase.AdminPagesList.Length == 0 && Roles.IsUserInRole(CurrentUsername, "Standard")) {
            AppLog.AddError(string.Format("{0} does not have any Admin Pages available. Check to make sure this user has at least one Admin Page when role is not set to {1}.", CurrentUsername, ServerSettings.AdminUserName));
            FormsAuthentication.SignOut();
            HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
        }

        string acctImg = CurrentUserMemberDatabase.AccountImage;
        lbl_UserName.Text = UserImageColorCreator.CreateImgColorTopBar(acctImg, CurrentUserMemberDatabase.UserColor, CurrentUserMemberDatabase.UserId, HelperMethods.MergeFMLNames(CurrentUserMemberDatabase), CurrentSiteTheme, CurrentUserMemberDatabase.ProfileLinkStyle);
        UserImageColorCreator.ApplyProfileLinkStyle(CurrentUserMemberDatabase.ProfileLinkStyle, CurrentUserMemberDatabase.UserColor, this.Page);

        lbl_UserFullName.Text = CurrentUserMemberDatabase.Username;
        lbl_UserEmail.Text = CurrentUserMembership.Email;
        if (!string.IsNullOrEmpty(acctImg)) {
            if (acctImg.ToLower().Contains("http") || acctImg.StartsWith("//") || acctImg.ToLower().Contains("www.")) {
                img_Profile.ImageUrl = acctImg;
            }
            else {
                if (File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + CurrentUserMemberDatabase.UserId + "/" + acctImg)) {
                    img_Profile.ImageUrl = ServerSettings.AccountImageLoc + CurrentUserMemberDatabase.UserId + "/" + acctImg;
                }
                else {
                    img_Profile.ImageUrl = ResolveUrl("~/App_Themes/" + CurrentSiteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
                }
            }
        }
        else {
            img_Profile.ImageUrl = ResolveUrl("~/App_Themes/" + CurrentSiteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
        }

        AssociateWithGroups = MainServerSettings.AssociateWithGroups;

        CurrentUserMembership.LastActivityDate = ServerSettings.ServerDateTime;
        Membership.UpdateUser(CurrentUserMembership);

        #region Query String Conditions
        if (HelperMethods.DoesPageContainStr("default.aspx", Page) && (HelperMethods.ConvertBitToBoolean(Request.QueryString["iframeMode"]) || HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"]))) {
            HelperMethods.PageRedirect("~/Default.aspx");
        }

        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["iframeMode"])) {
            sidebar_container.Visible = false;
            footer_container.Visible = true;
            top_bar.Visible = false;
            top_bar_toolview_holder.Visible = false;
            PostBackScriptString.Append("$('#footer-signdate').remove();");
        }
        else if (HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            sidebar_container.Visible = false;
            footer_container.Visible = true;
            top_bar.Visible = false;
            top_bar_toolview_holder.Visible = true;
            top_bar_toolview_holder.Controls.Add(user_profile_tab);
            PostBackScriptString.Append("openWSE.MobileMode();");
        }
        #endregion

        SetLogoImage();
        CheckUpdatesPopup();
        SetCurrentPageTitle();
        LoadCustomizations();

        // Load the Paged version of the workspace
        SetSimpleWorkspaceMode();

        #region Load Apps and Plugins
        if (!BasePage.IsUserNameEqualToAdmin(CurrentUsername) && !HelperMethods.ConvertBitToBoolean(Request.QueryString["iframeMode"]) && !HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            CurrentAppIconBuilderObject.BuildAppsForUser();
            GetUserPlugins();

            bool isComplexMode = MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode);

            if (CurrentAppIconBuilderObject.InAtLeastOneGroup) {
                string GetPageOverlayBtns = string.Empty;
                string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, CurrentWorkspaceMode);

                if (!isComplexMode && !string.IsNullOrEmpty(pnlId)) {
                    if (CurrentUserMemberDatabase.HideAllOverlays) {
                        GetPageOverlayBtns = "openWSE.DisableOverlaysOnPagedWorkspace();";
                    }
                }
                else if (!isComplexMode && string.IsNullOrEmpty(pnlId)) {
                    pnl_OverlaysAll.Enabled = false;
                    pnl_OverlaysAll.Visible = false;
                }

                if (!string.IsNullOrEmpty(pnlId)) {
                    PostBackScriptString.Append("openWSE_Config.overlayPanelId='" + pnlId + "';openWSE.UpdateOverlayTable();" + GetPageOverlayBtns);
                    string tempUsername = GroupSessions.GetUserGroupSessionName(CurrentUsername);
                    RefreshOverlays _ro = new RefreshOverlays(tempUsername, ServerSettings.GetServerMapLocation, this.Page, pnlId);
                    _ro.GetWorkspaceOverlays(CurrentAppIconBuilderObject.UserAppList);
                }
            }

            if (HelperMethods.DoesPageContainStr("default.aspx", Page) && isComplexMode) {
                CurrentAppIconBuilderObject.AppBuilderCaller.LoadSavedChatSessions();
            }

            PostBackScriptString.Append(CurrentAppIconBuilderObject.AppBuilderCaller.StrScriptreg);
        }
        #endregion

        CountNotiMessages();
        HideOverlaySettings();

        if (BasePage.IsUserNameEqualToAdmin(CurrentUsername)) {
            pnl_icons.Enabled = false;
            pnl_icons.Visible = false;
            pnl_chat_users.Enabled = false;
            pnl_chat_users.Visible = false;
            overlay_tab.Visible = false;
            group_tab.Visible = false;
        }

        // Hide the overlay button if logged into a group and no overlays exists
        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            string tempUsername = GroupSessions.GetUserGroupSessionName(CurrentUsername);
            WorkspaceOverlays wo = new WorkspaceOverlays();
            wo.GetUserOverlays(tempUsername);
            if (wo.UserOverlays.Count == 0) {
                overlay_tab.Visible = false;
            }

            PostBackScriptString.Append("openWSE_Config.groupLoginName=\"" + tempUsername + "\";");
        }

        RegisterPostbackScripts.RegisterStartupScript(this, PostBackScriptString.ToString());
    }
    private void LoadDemo() {
        var scriptManager = ScriptManager.GetCurrent(Page);

        SetLogoImage();
        SetCurrentPageTitle();
        LoadCustomizations();

        // Load the Paged version of the workspace
        SetSimpleWorkspaceMode();

        GetUserPlugins();

        CurrentAppIconBuilderObject.BuildAppsForDemo();

        bool isComplexMode = MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode);

        string GetPageOverlayBtns = string.Empty;
        string pnlId = WorkspaceOverlays.GetOverlayPanelId(this.Page, CurrentWorkspaceMode);

        if (!isComplexMode && !string.IsNullOrEmpty(pnlId)) {
            if (DemoCustomizations.DefaultTable.ContainsKey("HideAllOverlays") && HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["HideAllOverlays"])) {
                GetPageOverlayBtns = "openWSE.DisableOverlaysOnPagedWorkspace();";
            }
        }
        else if (!isComplexMode && string.IsNullOrEmpty(pnlId)) {
            pnl_OverlaysAll.Enabled = false;
            pnl_OverlaysAll.Visible = false;
        }

        if (!string.IsNullOrEmpty(pnlId)) {
            PostBackScriptString.Append("openWSE_Config.overlayPanelId='" + pnlId + "';openWSE.UpdateOverlayTable();" + GetPageOverlayBtns);
            RefreshOverlays _ro = new RefreshOverlays(string.Empty, ServerSettings.GetServerMapLocation, this.Page, pnlId);
            _ro.GetWorkspaceOverlays_NoLogin(CurrentAppIconBuilderObject.UserAppList);
        }

        if (string.IsNullOrEmpty(Request.QueryString["AppPage"]) || MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode)) {
            PostBackScriptString.Append("openWSE_Config.ShowLoginModalOnDemoMode=" + MainServerSettings.ShowLoginModalOnDemoMode.ToString().ToLower() + ";");
        }
        else {
            PostBackScriptString.Append("openWSE_Config.ShowLoginModalOnDemoMode=false;");
        }

        btn_addOverlayButton.Visible = false;
        PostBackScriptString.Append(CurrentAppIconBuilderObject.AppBuilderCaller.StrScriptreg);

        WorkspaceOverlays wo = new WorkspaceOverlays();
        wo.GetUserOverlays("DemoNoLogin");
        if (wo.UserOverlays.Count == 0) {
            overlay_tab.Visible = false;
        }

        HideOverlaySettings();

        LoginActivity la = new LoginActivity();
        la.AddItem(ServerSettings.GuestUsername, true, ActivityType.Guest);

        RegisterPostbackScripts.RegisterStartupScript(this, PostBackScriptString.ToString());
    }
    private void LoadCustomizations() {
        string siteColorOption = "1~;2~";
        string siteLayoutOption = "Wide";
        int currentWorkspace = 1;
        bool showDedicatedMinimizedArea = false;
        bool workspaceRotate = false;
        bool rotateAutoRefresh = false;
        string workspaceRotateInt = string.Empty;
        string workspaceRotateScreens = string.Empty;
        bool showworkspaceNumApp = false;
        bool showMinimizedPreview = false;
        bool taskBarShowAll = false;
        bool hoverPreviewWorkspace = false;
        bool allowNavMenuCollapseToggle = false;
        bool hideSearchBarInTopBar = false;

        #region Initialize Variables
        if (CurrentUserMemberDatabase != null) {
            #region Load Links In New Page
            bool loadLinksToNewPage = CurrentUserMemberDatabase.LoadLinksBlankPage;
            SideBarItems sidebar = new SideBarItems(CurrentUsername);

            if (!loadLinksToNewPage) {
                hyp_accountSettings.Attributes["target"] = "_self";
                lb_acctNotifications.Attributes["target"] = "_self";
                lb_manageGroups.Attributes["target"] = "_self";
                hyp_AccountCustomizations.Attributes["target"] = "_self";
            }

            if (!ServerSettings.AdminPagesCheck("acctsettings_aspx", CurrentUsername)) {
                hyp_accountSettings.Enabled = false;
                hyp_accountSettings.Visible = false;
                lb_acctNotifications.Visible = false;
            }
            else {
                lb_acctNotifications.HRef = ResolveUrl("~/SiteTools/UserTools/AcctSettings.aspx") + "#?tab=pnl_NotificationSettings";
                hyp_AccountCustomizations.HRef = ResolveUrl("~/SiteTools/UserTools/AcctSettings.aspx") + "#?tab=pnl_SiteCustomizations";
                hyp_AccountCustomizations.Visible = true;
            }

            if (ServerSettings.AdminPagesCheck("grouporg_aspx", CurrentUsername)) {
                lb_manageGroups.Visible = true;
            }

            StringBuilder strTabs = new StringBuilder();
            if (CurrentUserMemberDatabase.ShowSiteToolsInCategories) {
                List<string> adminPageCategories = sidebar.ListOfAdminPageCategories();
                foreach (string keyVal in adminPageCategories) {
                    string innerTabList = sidebar.BuildAdminPages(loadLinksToNewPage, keyVal);
                    if (!string.IsNullOrEmpty(innerTabList)) {
                        strTabs.AppendFormat("<div class=\"nav-title\">{0}</div><div class=\"site-tools-tablist\">", keyVal);
                        strTabs.AppendFormat("<div>{0}<div class='clear'></div></div>", innerTabList);
                        strTabs.Append("</div><div class=\"nav-divider\"></div>");
                    }
                }
            }
            else {
                string innerTabList = sidebar.BuildAdminPages(loadLinksToNewPage, string.Empty);
                if (!string.IsNullOrEmpty(innerTabList)) {
                    string noCategoryName = "Settings and Tools";
                    strTabs.AppendFormat("<div class=\"nav-title\">{0}</div><div class=\"site-tools-tablist\">", noCategoryName);
                    strTabs.AppendFormat("<div>{0}<div class='clear'></div></div>", innerTabList);
                    strTabs.Append("</div><div class=\"nav-divider\"></div>");
                }
            }

            placeHolder_SettingsTabs.Controls.Clear();
            placeHolder_SettingsTabs.Controls.Add(new LiteralControl(strTabs.ToString()));

            if (CurrentUserMemberDatabase.SiteToolsIconOnly) {
                sidebar_accordian.Attributes["data-icononly"] = "true";
            }
            #endregion

            siteColorOption = CurrentUserMemberDatabase.SiteColorOption;
            siteLayoutOption = CurrentUserMemberDatabase.SiteLayoutOption;

            #region Get Sidebar State
            string sessionKey = CurrentUsername + "_" + ServerSettings.ApplicationID + "_SidebarState";
            if (Session[sessionKey] != null && Session[sessionKey].ToString() == "hide" && !HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"]) && !HelperMethods.ConvertBitToBoolean(Request.QueryString["iframeMode"])) {
                top_bar.Attributes["class"] = "sidebar-minimized";
                sidebar_container.Attributes["class"] = "sidebar-minimized";
                main_container.Attributes["class"] = "sidebar-minimized";
                footer_container.Attributes["class"] = "sidebar-minimized";

                PostBackScriptString.Append("$(document).ready(function() { openWSE.HideSidebar(); });");
            }
            #endregion

            currentWorkspace = CurrentUserMemberDatabase.CurrentWorkspace;
            showDedicatedMinimizedArea = CurrentUserMemberDatabase.ShowDedicatedMinimizedArea;

            workspaceRotate = CurrentUserMemberDatabase.WorkspaceRotate;
            rotateAutoRefresh = CurrentUserMemberDatabase.RotateAutoRefresh;
            workspaceRotateInt = CurrentUserMemberDatabase.WorkspaceRotateInterval;
            workspaceRotateScreens = CurrentUserMemberDatabase.WorkspaceRotateScreens;
            showMinimizedPreview = CurrentUserMemberDatabase.ShowMinimizedPreview;
            taskBarShowAll = CurrentUserMemberDatabase.TaskBarShowAll;
            hoverPreviewWorkspace = CurrentUserMemberDatabase.HoverPreviewWorkspace;
            allowNavMenuCollapseToggle = CurrentUserMemberDatabase.AllowNavMenuCollapseToggle;
            hideSearchBarInTopBar = CurrentUserMemberDatabase.HideSearchBarInTopBar;

            if (CurrentUserMemberDatabase.ShowWorkspaceNumApp && CurrentUserMemberDatabase.TotalWorkspaces != 1) {
                showworkspaceNumApp = true;
            }
        }
        else if (DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.Count > 0) {
            if (DemoCustomizations.DefaultTable.ContainsKey("SiteColorOption") && !string.IsNullOrEmpty(DemoCustomizations.DefaultTable["SiteColorOption"])) {
                siteColorOption = DemoCustomizations.DefaultTable["SiteColorOption"];
            }

            if (DemoCustomizations.DefaultTable.ContainsKey("SiteLayoutOption") && !string.IsNullOrEmpty(DemoCustomizations.DefaultTable["SiteLayoutOption"])) {
                siteLayoutOption = DemoCustomizations.DefaultTable["SiteLayoutOption"];
            }

            showDedicatedMinimizedArea = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["ShowDedicatedMinimizedArea"]);
            workspaceRotate = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["WorkspaceRotate"]);
            rotateAutoRefresh = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["RotateAutoRefresh"]);
            workspaceRotateInt = DemoCustomizations.DefaultTable["WorkspaceRotateInterval"];
            workspaceRotateScreens = DemoCustomizations.DefaultTable["WorkspaceRotateScreens"];
            showMinimizedPreview = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["ShowMinimizedPreview"]);
            hoverPreviewWorkspace = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["HoverPreviewWorkspace"]) || DemoCustomizations.DefaultTable["HoverPreviewWorkspace"] == string.Empty;
            allowNavMenuCollapseToggle = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["AllowNavMenuCollapseToggle"]);
            hideSearchBarInTopBar = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["HideSearchBarInTopBar"]);

            if (HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["TaskBarShowAll"]) || string.IsNullOrEmpty(DemoCustomizations.DefaultTable["TaskBarShowAll"])) {
                taskBarShowAll = true;
            }

            if ((HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["ShowWorkspaceNumApp"]) || string.IsNullOrEmpty(DemoCustomizations.DefaultTable["ShowWorkspaceNumApp"])) && DemoCustomizations.DefaultTable["TotalWorkspaces"] != "1") {
                showworkspaceNumApp = true;
            }
        }
        #endregion

        #region Site Color Option
        string selectedColorIndex = "1";
        string[] siteOptionSplit = siteColorOption.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
        if (siteOptionSplit.Length >= 1) {
            selectedColorIndex = siteOptionSplit[0];
        }

        site_mainbody.Attributes["data-coloroption"] = selectedColorIndex;
        div_ColorOptionsHolder.InnerHtml = HelperMethods.BuildColorOptionList("openWSE.ThemeColorOption_Clicked(this);", "openWSE.ColorOption_Changed(this);", "openWSE.ResetColorOption_Clicked(this);", siteColorOption, this.Page);
        PostBackScriptString.Append("openWSE.InitializeThemeColorOption('div_ColorOptionsHolder');");
        #endregion

        #region Site Layout Option
        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            site_mainbody.Attributes["data-layoutoption"] = siteLayoutOption;
            if (siteLayoutOption == "Boxed") {
                rb_BoxedLayout.Checked = true;
                rb_WideLayout.Checked = false;
            }
            else {
                rb_WideLayout.Checked = true;
                rb_BoxedLayout.Checked = false;
            }
        }
        #endregion

        #region AllowNavMenuCollapseToggle
        PostBackScriptString.AppendFormat("openWSE_Config.allowNavMenuCollapseToggle={0};", allowNavMenuCollapseToggle.ToString().ToLower());
        if (allowNavMenuCollapseToggle) {
            Page.Header.Controls.Add(new LiteralControl("<style type=\"text/css\">#updatePnl_AppList, #minimized_app_bar_holder, #chat_sidebar_holder, #pnl_tools .site-tools-tablist { display: none; }</style>"));
        }
        #endregion

        #region Hide Top Search Bar
        PostBackScriptString.AppendFormat("openWSE_Config.hideSearchBarInTopBar={0};", hideSearchBarInTopBar.ToString().ToLower());
        #endregion

        if (HelperMethods.DoesPageContainStr("default.aspx", Page)) {
            #region Show Dedicated Minimized Area
            if (showDedicatedMinimizedArea) {
                minimized_app_bar.Attributes["data-show"] = "true";
            }
            else {
                minimized_app_bar.Attributes["data-show"] = "false";
            }
            #endregion

            #region Workspace Rotate
            if (workspaceRotate && workspaceRotateInt != "0" && !string.IsNullOrEmpty(workspaceRotateInt)) {
                PostBackScriptString.AppendFormat("openWSE.AutoRotateWorkspace({0}, 1, {1}, {2});", workspaceRotateInt, workspaceRotateScreens, rotateAutoRefresh.ToString().ToLower());
            }
            #endregion

            #region Show workspace Number App
            PostBackScriptString.AppendFormat("openWSE_Config.ShowWorkspaceNumApp={0};", showworkspaceNumApp.ToString().ToLower());
            #endregion

            #region Minimize Preview
            if (showMinimizedPreview) {
                PostBackScriptString.Append("$(document.body).on('mouseover', '.app-min-bar, .app-icon', function () { openWSE.HoverOverAppMin(this); }); $(document.body).on('mouseleave', '.app-min-bar, .app-icon', function () { openWSE.HoverOutAppMin(); });");
            }
            #endregion

            #region Task Bar Show All
            PostBackScriptString.AppendFormat("openWSE_Config.taskBarShowAll={0};", taskBarShowAll.ToString().ToLower());
            #endregion

            #region Hover Preview Workspace
            if (MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode)) {
                PostBackScriptString.AppendFormat("openWSE_Config.hoverPreviewWorkspace={0};", hoverPreviewWorkspace.ToString().ToLower());
            }
            #endregion
        }
    }

    #endregion


    #region Set Screen

    private void HideOverlaySettings() {
        if (CurrentUserMemberDatabase != null && CurrentUserMemberDatabase.HideAllOverlays) {
            overlay_tab.Visible = false;
        }
        else if (DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.ContainsKey("HideAllOverlays") && HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["HideAllOverlays"])) {
            overlay_tab.Visible = false;
        }
    }
    private void CheckIfDemoOrPreview() {
        if (HelperMethods.ConvertBitToBoolean(Request.QueryString["Demo"])) {
            pnl_Login_NonAuth.Visible = false;
        }
        else if (NoLoginRequired && !UserIsAuthenticated) {
            pnl_Login_NonAuth.Visible = true;

            #region Check LoginMessage
            if (!string.IsNullOrEmpty(MainServerSettings.LoginMessage)) {
                lbl_LoginMessage_Master.Enabled = true;
                lbl_LoginMessage_Master.Visible = true;
                lbl_LoginMessage_Master.Text = string.Format("<div class='Login-Message'><b>Site Message:</b>{0}</div>", MainServerSettings.LoginMessage);
            }
            else {
                lbl_LoginMessage_Master.Enabled = false;
                lbl_LoginMessage_Master.Visible = false;
            }
            #endregion

            #region Check Sign in with Google
            if (!MainServerSettings.SignInWithGoogle) {
                lbtn_signinwith_Google.Enabled = false;
                lbtn_signinwith_Google.Visible = false;
            }
            #endregion

            #region Check Sign in with Twitter
            if (!MainServerSettings.SignInWithTwitter) {
                lbtn_signinwith_Twitter.Enabled = false;
                lbtn_signinwith_Twitter.Visible = false;
            }
            #endregion

            #region Check Sign in with Facebook
            if (!MainServerSettings.SignInWithFacebook) {
                lbtn_signinwith_Facebook.Enabled = false;
                lbtn_signinwith_Facebook.Visible = false;
            }
            #endregion

            StringBuilder strScriptreg = new StringBuilder();
            if (MainServerSettings.SignInWithGoogle || MainServerSettings.SignInWithTwitter || MainServerSettings.SignInWithFacebook) {
                strScriptreg.Append("$('#SocialLogin_borderSep').addClass('loginwith-api-borderseperate');");
            }
            else {
                sociallogin_td.Visible = false;
                Login1.Style["width"] = "270px";
                Login1.Style["margin"] = "0 auto";
            }

            if (LoginOrRegister != null && !MainServerSettings.AllowUserSignUp) {
                strScriptreg.Append("$('#CreateAccount-holder').remove();$('#login_register_link').remove();");
                LoginOrRegister.Text = "Have an account? Login below";
            }
            else if (span_signinText != null) {
                span_signinText.InnerHtml = "Login / Register";
            }

            if (!MainServerSettings.EmailSystemStatus) {
                strScriptreg.Append("$('#lnk_forgotpassword').remove();");
            }

            if (!string.IsNullOrEmpty(strScriptreg.ToString())) {
                RegisterPostbackScripts.RegisterStartupScript(Page, strScriptreg.ToString());
            }
        }
    }
    private void SetSimpleWorkspaceMode() {
        if (!MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode)) {
            overlay_tab.Visible = false;

            string appToOpen = Request.QueryString["AppPage"];
            if (HelperMethods.DoesPageContainStr("default.aspx", Page)) {
                if (string.IsNullOrEmpty(appToOpen)) {
                    appToOpen = string.Empty;
                    main_container.Style["padding"] = "0";
                }
                else {
                    App _apps = new App(string.Empty);
                    Apps_Coll appInformation = _apps.GetAppInformation(appToOpen);
                    if (appInformation != null) {
                        bool showAppTitle = true;
                        bool showAppImg = true;
                        if (CurrentUserMemberDatabase != null) {
                            showAppTitle = CurrentUserMemberDatabase.ShowAppTitle;
                            showAppImg = CurrentUserMemberDatabase.AppHeaderIcon;
                        }
                        else if (DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.Count > 0) {
                            showAppTitle = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["ShowAppTitle"]);
                            showAppImg = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["AppHeaderIcon"]); ;
                        }

                        string pageTitle = string.Empty;
                        if (showAppImg && !string.IsNullOrEmpty(appInformation.Icon)) {
                            pageTitle = "<img alt='app-icon' src='" + ResolveUrl("~/" + appInformation.Icon) + "' class='page-icon' />";
                        }

                        if (showAppTitle || string.IsNullOrEmpty(pageTitle)) {
                            pageTitle += appInformation.AppName;
                        }
                    }
                }

            }

            PostBackScriptString.Append("openWSE_Config.workspaceMode='" + CurrentWorkspaceMode + "';openWSE.PagedWorkspace('" + appToOpen + "');");
        }
    }
    private void SetCurrentPageTitle() {
        if (SiteMap.CurrentNode != null) {
            if (top_bar.Visible) {
                if (!HelperMethods.DoesPageContainStr("default.aspx", Page)) {
                    SiteMapNode currentMapNode = SiteMap.CurrentNode;
                    if (currentMapNode != null && currentMapNode.ParentNode != SiteMap.RootNode) {
                        currentMapNode = SiteMap.CurrentNode.ParentNode;
                        Page.Title = currentMapNode.Title;
                    }

                    if (BasePage.IsUserNameEqualToAdmin(CurrentUsername)) {
                        lnk_BackToWorkspace.Attributes["onclick"] = "openWSE.SiteStatusAdmin_Clicked();return false;";
                        lnk_BackToWorkspace.Title = "View Network Stats";
                    }
                }
            }
            else if (top_bar_toolview_holder.Visible) {
                SiteMapNode currentMapNode = SiteMap.CurrentNode;
                if (currentMapNode != null && currentMapNode.ParentNode != SiteMap.RootNode) {
                    currentMapNode = SiteMap.CurrentNode.ParentNode;
                    Page.Title = currentMapNode.Title;
                }
            }
        }
    }
    private void SetLogoImage() {
        Dictionary<string, string> groupInformation = BaseMaster.GetGroupInformation(CurrentUsername);

        if (groupInformation.Count > 0) {
            AssociateWithGroups = false;
            background_tab.Visible = false;
            settings_tab.Visible = false;
            btn_addOverlayButton.Visible = false;
        }

        if (top_bar.Visible) {
            BaseMaster.SetTopLogoTags(Page, lnk_BackToWorkspace, groupInformation);
        }
        else if (top_bar_toolview_holder.Visible) {
            BaseMaster.SetTopLogoTags(Page, iframe_title_logo, groupInformation);
        }
    }

    #endregion


    #region Login

    protected void Login_LoggingIn(object sender, LoginCancelEventArgs e) {
        string email = Login1.UserName;
        MembershipUserCollection coll = Membership.GetAllUsers();

        int count = coll.Cast<MembershipUser>().Count(user => user.Email != null && user.Email.ToLower() == email);

        if (count == 1) {
            string username = Membership.GetUserNameByEmail(email);

            if (!string.IsNullOrEmpty(username)) {
                Login1.UserName = username;
            }
        }

        MembershipUser mUser = Membership.GetUser(Login1.UserName);
        if (mUser != null) {
            if (new MemberDatabase(Login1.UserName).IsSocialAccount) {
                e.Cancel = true;
            }
        }
    }
    protected void Login_Loggedin(object sender, EventArgs e) {
        bool cancontinue = false;
        IPListener listener = new IPListener(false);
        IPWatch _ipwatch = new IPWatch(false);
        string remoteaddress = BasePage.GetCurrentPageIpAddress(Request);

        if (BasePage.IsUserNameEqualToAdmin(Login1.UserName)) {
            cancontinue = true;
        }
        else if (_ipwatch.CheckIfBlocked(remoteaddress) && !BasePage.IsUserNameEqualToAdmin(Login1.UserName)) {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }
        else {
            if (!listener.TableEmpty) {
                if (listener.CheckIfActive(remoteaddress)) {
                    cancontinue = true;
                }
                else {
                    HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
                }
            }
            else {
                cancontinue = true;
            }
        }

        if (!cancontinue) return;
        var member = new MemberDatabase(Login1.UserName);
        member.UpdateUserIpAddress(remoteaddress);
        EmailUser(member);

        member.UpdateChatTimeStamp();
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser msu in coll) {
            string un = msu.UserName.ToLower();
            if ((msu.IsOnline) && (un != Login1.UserName.ToLower())) {
                if (ChatService._needRefreshUsers.ContainsKey(un))
                    ChatService._needRefreshUsers[un] = true;
                else
                    ChatService._needRefreshUsers.Add(un, true);

                if (ChatService._needRefreshMessager.ContainsKey(un))
                    ChatService._needRefreshMessager[un] = true;
                else
                    ChatService._needRefreshMessager.Add(un, true);
            }
        }

        LoginActivity la = new LoginActivity();
        la.AddItem(Login1.UserName, true, ActivityType.Login);

        if (!string.IsNullOrEmpty(member.DefaultLoginGroup) && !GroupSessions.DoesUserHaveGroupLoginSessionKey(member.Username)) {
            GroupSessions.AddOrSetNewGroupLoginSession(member.Username, member.DefaultLoginGroup);
        }

        MemberDatabase.AddUserSessionIds(member.Username);
        ServerSettings.SetRememberMeOnLogin(Login1, Response);

        if (!string.IsNullOrEmpty(Request.QueryString["ReturnUrl"])) {
            string redirectUrl = HttpUtility.UrlDecode(Request.QueryString["ReturnUrl"]);
            if (BasePage.IsUserInAdminRole(member.Username)) {
                HelperMethods.PageRedirect(redirectUrl);
            }
            else {
                foreach (string page in member.AdminPagesList) {
                    if (redirectUrl.ToLower().Contains(page)) {
                        HelperMethods.PageRedirect(redirectUrl);
                    }
                }
            }
        }

        HelperMethods.PageRedirect("~/Default.aspx");
    }
    protected void Login_error(object sender, EventArgs e) {
        string remoteaddress = BasePage.GetCurrentPageIpAddress(Request);

        if (BasePage.IsUserNameEqualToAdmin(Login1.UserName)) {
            MembershipUser mu = Membership.GetUser(ServerSettings.AdminUserName);
            if (mu != null && (mu.IsLockedOut || !mu.IsApproved)) {
                if (!mu.IsApproved) {
                    mu.IsApproved = true;
                }
                if (mu.IsLockedOut) {
                    mu.UnlockUser();
                }
                Membership.UpdateUser(mu);
            }
        }

        var listener = new IPListener(false);
        bool cancontinue = false;
        if (!listener.TableEmpty) {
            if (!listener.CheckIfActive(remoteaddress)) {
                cancontinue = true;
            }
        }
        else {
            cancontinue = true;
        }

        if (!cancontinue) return;
        bool foundIp = false;
        IPWatch _ipwatch = new IPWatch(true);

        for (var i = 0; i < _ipwatch.ipwatchdt.Count; i++) {
            if (_ipwatch.ipwatchdt[i]["IPAddress"].ToString() == remoteaddress) {
                int attempts = Convert.ToInt32(_ipwatch.ipwatchdt[i]["Attempts"].ToString()) + 1;
                _ipwatch.updateAttempts(_ipwatch.ipwatchdt[i]["IPAddress"].ToString(), attempts);

                if (attempts >= MainServerSettings.AutoBlockIPCount) {
                    BackupSite();
                }
                foundIp = true;
            }
        }

        if (!foundIp) {
            _ipwatch.addItem(remoteaddress, 1, false);

            if (1 >= MainServerSettings.AutoBlockIPCount) {
                BackupSite();
            }
        }

        LoginActivity la = new LoginActivity();
        la.AddItem(Login1.UserName, false, ActivityType.Login);
    }

    protected void lbtn_signinwith_Google_Click(object sender, EventArgs e) {
        SocialSignIn.GoogleSignIn();
    }
    protected void lbtn_signinwith_Twitter_Click(object sender, EventArgs e) {
        SocialSignIn.TwitterSignIn();
    }
    protected void lbtn_signinwith_Facebook_Click(object sender, EventArgs e) {
        SocialSignIn.FacebookSignIn();
    }

    private void BackupSite() {
        try {
            if (!Directory.Exists(ServerSettings.GetServerMapLocation + "Backups\\Temp")) {
                Directory.CreateDirectory(ServerSettings.GetServerMapLocation + "Backups\\Temp");
            }

            foreach (
                string filename in
                    Directory.GetFiles(ServerSettings.GetServerMapLocation + "Backups\\Temp")) {
                if (File.Exists(filename)) {
                    File.Delete(filename);
                }
            }
        }
        catch {
        }
        string f = "DBFull_" + ServerSettings.ServerDateTime.ToFileTime();
        string loc = ServerSettings.GetServerMapLocation + "Backups\\Temp\\" + f + "Temp" + ServerSettings.BackupFileExt;
        var sb = new ServerBackup(ServerSettings.AdminUserName.ToLower(), loc);
        var dbviewer = new DBViewer(true);
        sb.BinarySerialize_Current(dbviewer.dt);

        string backupfile = ServerSettings.GetServerMapLocation + "Backups\\BackupLog.xml";
        string tDesc = "Full Database Download";
        AutoBackupSystem.WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), tDesc);

        if (File.Exists(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt))) {
            File.Copy(loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt),
                      ServerSettings.GetServerMapLocation + "Backups\\" + f + ServerSettings.BackupFileExt, true);
        }

        ServerSettings.DeleteBackupTempFolderFiles();
    }

    private void EmailUser(MemberDatabase member) {
        string ipaddress = member.IpAddress;
        string date = ServerSettings.ServerDateTime.ToString(CultureInfo.InvariantCulture);
        foreach (var user in member.EmailUponLoginList) {
            MembershipUser u = Membership.GetUser(user);
            if (u != null && string.IsNullOrEmpty(u.Email)) continue;
            string fullname = HelperMethods.MergeFMLNames(member);
            if (u != null) SendEmailsForLogin(u.Email, member.Username, fullname, ipaddress, date);
        }
    }
    private void SendEmailsForLogin(string emailto, string loggedinusername, string loggedinuser, string ipaddress, string date) {
        try {
            var messagebody = new StringBuilder();
            messagebody.Append(loggedinuser + " has logged in:<br />");
            messagebody.Append("<b style='padding-right:5px'>Username:</b>" + loggedinusername + "<br />");
            messagebody.Append("<b style='padding-right:5px'>IP Address:</b>" + ipaddress + "<br />");
            messagebody.Append("<b style='padding-right:5px'>Date/Time:</b>" + date + "<br />");
            var message = new MailMessage();
            message.To.Add(emailto);

            ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>User Logon Notification</h1>", ServerSettings.SiteName + ": " + loggedinuser + " has Logged In", messagebody.ToString());
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
    }
    protected void btn_passwordrecovery_Click(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(tb_username_recovery.Text)) {
            MembershipUser msu = Membership.GetUser(tb_username_recovery.Text.Trim());
            if ((msu != null) && (!string.IsNullOrEmpty(msu.UserName))) {
                bool sendError = false;
                try {
                    MemberDatabase _mTemp = new MemberDatabase(msu.UserName);
                    if (((msu.IsLockedOut) || (!msu.IsApproved)) && (!_mTemp.IsNewMember)) {
                        sendError = true;
                    }
                    else if (_mTemp.IsSocialAccount) {
                        sendError = true;
                    }
                    else {
                        string password = msu.ResetPassword();
                        string str = "<div><b>Username: </b>" + msu.UserName + "<br /><b>New Password: </b>" + password;
                        //str += "<br /><input type='button' onClick='CopiedTxt=document.selection.createRange();CopiedTxt.execCommand(\"" + password + "\");' value='Copy password to clipboard' />";
                        str += "<br /><p>Copy and paste the new password above in the login screen. Make sure to reset your password after logging in.</p>";
                        string link = "http:" + ServerSettings.GetSitePath(Request) + "/" + ServerSettings.DefaultStartupPage;

                        if (((msu.IsLockedOut) || (!msu.IsApproved)) && (_mTemp.IsNewMember))
                            link += "?ActivateUser=" + msu.UserName + "&ActivationCode=" + _mTemp.ActivationCode;

                        string fakelink = "http:" + ServerSettings.GetSitePath(Request) + "/" + ServerSettings.DefaultStartupPage;
                        str += "<p>Click on the link to continue: <a href='" + link + "'>" + fakelink + "</a></p>";
                        SendEmails(msu, str);

                        lbl_passwordResetMessage.Text = "<div class='clear-space'></div>Password has been sent to your email address.";
                        lbl_passwordResetMessage.ForeColor = System.Drawing.Color.Green;
                    }
                }
                catch {
                    sendError = true;
                }

                if (sendError) {
                    lbl_passwordResetMessage.Text = "<div class='clear-space'></div>Failed to send email. Please try again.";
                    lbl_passwordResetMessage.ForeColor = System.Drawing.Color.Red;

                    string str2 = "We were unable to recovery your account. It appears that your account has been locked and can only be released by an administrator.";
                    str2 += "<br />Contact your administrator to have your account unlocked. <b style='padding-right:3px'>Note:</b>To avoid issues like this in the ";
                    str2 += "future, please do not enter the wrong password multiple times. - " + ServerSettings.SiteName;
                    SendEmails(msu, str2);
                }
            }
        }

        tb_username_recovery.Text = string.Empty;
        updatepnl_forgotPassword.Update();
    }
    private void SendEmails(MembershipUser u, string newpassword) {
        var message = new MailMessage();
        message.To.Add(u.Email);

        ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Password Recovery</h1>", ServerSettings.SiteName + " : Password Recovery", newpassword);
    }

    #endregion


    #region Logout

    protected void aGroupLogoff_Click(object sender, EventArgs e) {
        BaseMaster.GroupSignOff(CurrentUsername);
    }
    protected void lbtn_signoff_Click(object sender, EventArgs e) {
        BaseMaster.SignUserOff(CurrentUsername);
    }

    #endregion


    #region Auto Update

    protected void hf_UpdateAll_ValueChanged(object sender, EventArgs e) {
        bool cancontinue = false;
        if (CurrentUserMemberDatabase != null) {
            if (!string.IsNullOrEmpty(hf_UpdateAll.Value)) {
                string id = CurrentUserUpdateFlagsObject.getFlag_AppID(hf_UpdateAll.Value);
                if (id == "workspace") {
                    CurrentUserUpdateFlagsObject.deleteFlag(hf_UpdateAll.Value);
                    cancontinue = true;
                }
                else if (hf_UpdateAll.Value == "refresh") {
                    HelperMethods.PageRedirect(Request.RawUrl);
                }
            }

            ResetCurrentAppIconBuilderObject();
            CurrentAppIconBuilderObject.BuildAppsForUser();
        }

        if (cancontinue) {
            foreach (Control control in Page.Form.Controls) {
                if (control.HasControls()) {
                    RegisterAsynControls(control.Controls);
                }

                if (control is UpdatePanel) {
                    UpdatePanel up = control as UpdatePanel;
                    if (up.UpdateMode == UpdatePanelUpdateMode.Conditional) {
                        up.Update();
                    }
                    else {
                        if (control.HasControls()) {
                            RegisterAsynControls(control.Controls);
                        }
                    }
                }
            }
        }

        hf_UpdateAll.Value = "";
        CountNotiMessages();
    }
    private void RegisterAsynControls(ControlCollection page) {
        foreach (Control c in page) {
            if (c is UpdatePanel) {
                var up = c as UpdatePanel;
                if (up.UpdateMode == UpdatePanelUpdateMode.Conditional)
                    up.Update();
            }

            if (c.HasControls())
                RegisterAsynControls(c.Controls);
        }
    }

    #endregion


    #region Controls for Notifications

    private void CountNotiMessages() {
        int total = 0;

        if (CurrentUserNotificationMessagesObject != null) {
            List<UserNotificationsMessage_Coll> messageList = CurrentUserNotificationMessagesObject.getNonDismissedEntries("DESC");
            total = messageList.Count;
            BaseMaster.SetNotificationPopup(Page, total, messageList, CurrentSiteTheme);
        }

        if (!IsPostBack && !string.IsNullOrEmpty(CurrentUsername)) {
            List<UserNotifications_Coll> userNotificationColl_List = CurrentNotificationsObject.GetUserNotifications(CurrentUsername);

            bool setVisiblityToFalse = true;
            if (Page.ToString().ToLower().Contains("acctsettings_aspx")) {
                setVisiblityToFalse = false;
            }

            if (userNotificationColl_List.Count == 0) {
                if (!setVisiblityToFalse) {
                    PostBackScriptString.Append("$('#notifications_tab').hide();");
                }
                else {
                    notifications_tab.Visible = false;
                }
            }
            else {
                if (!setVisiblityToFalse) {
                    PostBackScriptString.Append("$('#notifications_tab').show();");
                }
                else {
                    notifications_tab.Visible = true;
                }
            }
        }

        RegisterPostbackScripts.RegisterStartupScript(Page, "openWSE.UpdateNotificationCount(" + total.ToString() + ");");
    }
    private void CheckUpdatesPopup() {
        ShowUpdatePopup sup = new ShowUpdatePopup();
        if (sup.isUserShowPopup(CurrentUsername)) {
            string message = sup.GetNewUpdateMessage(ServerSettings.GetServerMapLocation, CurrentSiteTheme);
            string encodedMessage = HttpUtility.UrlEncode(message, System.Text.Encoding.Default).Replace("+", "%20");
            sup.UpdateUser(false, CurrentUsername);
            PostBackScriptString.Append("openWSE.ShowUpdatesPopup('" + encodedMessage + "');");
        }
    }

    #endregion


    #region Jquery Plugins Load

    private void GetUserPlugins() {
        if (DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.Count > 0 && DemoCustomizations.DefaultTable.ContainsKey("PluginsToInstall")) {
            string[] pluginList = DemoCustomizations.DefaultTable["PluginsToInstall"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            foreach (string plId in pluginList) {
                AddPluginToPage(plId);
            }
        }
        else {
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
                string sessionGroup = GroupSessions.GetUserGroupSessionName(CurrentUsername);
                NewUserDefaults tempDefaults = new NewUserDefaults(sessionGroup);
                string pluginList = tempDefaults.GetDefault("PluginsToInstall");
                string[] groupPluginList = pluginList.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string plId in groupPluginList) {
                    AddPluginToPage(plId);
                }
            }
            else {
                SitePlugins _plugins = new SitePlugins(CurrentUsername);
                _plugins.BuildSitePluginsForUser();
                foreach (UserPlugins_Coll pl in _plugins.userplugins_dt) {
                    AddPluginToPage(pl.PluginID);
                }
            }
        }
    }
    private void GetPluginAssociations(string id) {
        SitePlugins _plugins = new SitePlugins(string.Empty);
        _plugins.BuildSitePlugins(true);
        ScriptManager sm = ScriptManager.GetCurrent(Page);

        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            if (!string.IsNullOrEmpty(coll.AssociatedWith)) {
                if (coll.AssociatedWith == id && coll.Enabled)
                    AddPluginToPage(coll.ID);
            }
        }
    }
    private void AddPluginToPage(string id) {
        SitePlugins sp = new SitePlugins(string.Empty);
        SitePlugins_Coll plugin = sp.GetPlugin(id);

        if (AssociateWithGroups) {
            if (!ServerSettings.CheckPluginGroupAssociation(plugin, CurrentUserMemberDatabase)) {
                return;
            }
        }

        if (!string.IsNullOrEmpty(plugin.ID)) {
            string loc = plugin.PluginLocation;
            bool canContinue = false;
            bool isJavascript = false;
            try {
                Uri uri = new Uri(loc);
                if (uri.IsAbsoluteUri) {
                    string x = uri.Segments[uri.Segments.Length - 1];
                    FileInfo fi = new FileInfo(x);
                    if (fi.Extension.ToLower() == ".js") {
                        canContinue = true;
                        isJavascript = true;
                    }
                    else if (fi.Extension.ToLower() == ".css") {
                        canContinue = true;
                        isJavascript = false;
                    }
                }
            }
            catch {
                try {
                    FileInfo fi = new FileInfo(loc);
                    if (fi.Extension.ToLower() == ".js") {
                        canContinue = true;
                        isJavascript = true;
                    }
                    else if (fi.Extension.ToLower() == ".css") {
                        canContinue = true;
                        isJavascript = false;
                    }
                }
                catch { }
            }

            if ((canContinue) && (plugin.Enabled)) {
                if (isJavascript) {
                    ScriptManager sm = ScriptManager.GetCurrent(Page);
                    if (sm != null) {
                        var sref = new ScriptReference(loc);
                        sm.Scripts.Add(sref);
                        string intiCode = HttpUtility.UrlDecode(plugin.InitCode);
                        intiCode = HttpUtility.UrlDecode(intiCode);
                        PostBackScriptString.Append(intiCode);
                        sm.ScriptMode = ScriptMode.Release;
                    }
                }
                else {
                    StartupStyleSheets startupscripts = new StartupStyleSheets(false);
                    startupscripts.AddCssToPage(loc, Page);
                }

                if (string.IsNullOrEmpty(plugin.AssociatedWith))
                    GetPluginAssociations(plugin.ID);
            }
        }
    }

    #endregion

}