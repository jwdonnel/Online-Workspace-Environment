#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.Overlays;

#endregion

public partial class Default : BasePage {

    private readonly StringBuilder PostBackScriptString = new StringBuilder();

    #region Load Methods

    protected void Page_Load(object sender, EventArgs e) {
        if (!IsPostBack) {
            if (!UserIsAuthenticated && DemoCustomizations != null && DemoCustomizations.DefaultTable.Count > 0) {
                if (MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode)) {
                    AppContainer();
                }

                GetAdminNote();

                PostBackScriptString.Append("openWSE_Config.demoMode=true;");
                PostBackScriptString.Append("$('#MainContent_workspace_1').css({ visibility:'visible',opacity:1.0,filter:'alpha(opacity=100)' });");

                if (HelperMethods.ConvertBitToBoolean(Request.QueryString["activation"]) && !string.IsNullOrEmpty(Request.QueryString["user"])) {
                    string message = "<div align='center'>Your account for " + Request.QueryString["user"] + " has been activated. You may now login to your account.</div>";
                    string encodedMessage = HttpUtility.UrlEncode(message, System.Text.Encoding.Default).Replace("+", "%20");
                    PostBackScriptString.Append("openWSE.ShowActivationPopup('" + encodedMessage + "');");
                }
            }
            else if (UserIsAuthenticated && CurrentUserMemberDatabase != null) {
                if (BasePage.IsUserNameEqualToAdmin(CurrentUsername)) {
                    HelperMethods.PageRedirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
                }
                else {
                    if (CurrentUserGroupList.Count == 0 && !CurrentUserMemberDatabase.IsNewMember) {
                        if (ServerSettings.AdminPagesCheck("GroupOrg", CurrentUsername)) {
                            HelperMethods.PageRedirect("~/SiteTools/UserMaintenance/GroupOrg.aspx");
                        }
                    }

                    if (!CurrentUserMemberDatabase.IsNewMember) {
                        if (CurrentUserGroupList.Count > 0 && MemberDatabase.IsComplexWorkspaceMode(CurrentWorkspaceMode)) {
                            AppContainer();
                        }

                        GetAdminNote();
                    }
                }
            }
        }

        RegisterPostbackScripts.RegisterStartupScript(this, PostBackScriptString.ToString());
    }
    protected void Page_Init(object sender, EventArgs e) {
        if (!IsPostBack && HelperMethods.IsMobileDevice) {
            HelperMethods.PageRedirect("~/AppRemote.aspx");
        }
    }

    #endregion

    #region Customizations

    private void AppContainer() {
        bool snapHelper = false;
        bool snapToGrid = false;
        bool appContainer = false;
        string gridSize = "20";
        hf_appContainer.Value = "";

        if (CurrentUserMemberDatabase != null) {
            appContainer = CurrentUserMemberDatabase.AppContainer;
            snapHelper = CurrentUserMemberDatabase.AppSnapHelper;
            snapToGrid = CurrentUserMemberDatabase.AppSnapToGrid;
            gridSize = CurrentUserMemberDatabase.AppGridSize;
        }
        else if (DemoCustomizations != null && DemoCustomizations.DefaultTable != null && DemoCustomizations.DefaultTable.Count > 0) {
            appContainer = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["AppContainer"]);
            snapHelper = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["AppSnapHelper"]) && !string.IsNullOrEmpty(DemoCustomizations.DefaultTable["AppSnapHelper"]);
            snapToGrid = HelperMethods.ConvertBitToBoolean(DemoCustomizations.DefaultTable["SnapToGrid"]) && !string.IsNullOrEmpty(DemoCustomizations.DefaultTable["SnapToGrid"]);
            gridSize = DemoCustomizations.DefaultTable["AppGridSize"];
            if (string.IsNullOrEmpty(gridSize)) {
                gridSize = "20";
            }
        }

        if (appContainer) {
            hf_appContainer.Value = "#main_container";
        }

        if (snapHelper) {
            PostBackScriptString.Append("openWSE_Config.appSnapHelper=true;");
        }

        string arg = string.Empty;
        string arg2 = string.Empty;

        if (snapToGrid) {
            arg2 = string.Format("grid: [ {0}, {0} ]", gridSize);
            if (!string.IsNullOrEmpty(arg)) {
                arg += string.Format(", grid: [ {0}, {0} ]", gridSize);
            }
            else {
                arg += string.Format("grid: [ {0}, {0} ]", gridSize);
            }
        }

        if (!string.IsNullOrEmpty(arg)) {
            string js = string.Format("$(function () {{ $('.app-main-holder').draggable({{ {0} }})", arg);
            if (!string.IsNullOrEmpty(arg2)) {
                js += string.Format(".resizable({{ {0} }})", arg2);
            }
            js += "; });";
            PostBackScriptString.Append(js);
        }
        else if (!string.IsNullOrEmpty(arg2)) {
            PostBackScriptString.AppendFormat("$(function () {{ $('.app-main-holder').draggable({{ {0} }}).resizable({{ {0} }}); }});", arg2);
        }
    }

    #endregion

    #region Admin Note Overlay

    private void GetAdminNote() {
        string note = MainServerSettings.AdminNote;
        string pnlId = WorkspaceOverlays.GetOverlayPanelId(Page, CurrentWorkspaceMode);
        if (!string.IsNullOrEmpty(note) && !string.IsNullOrEmpty(pnlId)) {
            pnl_adminnote.Enabled = true;
            pnl_adminnote.Visible = true;
            lbl_adminnote.Text = "<table cellpadding='0' cellspacing='0' width='100%'><tr><td valign='top' style='width: 50px;'><img alt='' src='" + ResolveUrl("~/App_Themes/" + CurrentSiteTheme + "/Icons/SiteMaster/info.png") + "' class='float-left' style='height: 32px;' /><div class='clear'></div></td><td valign='middle'>" + note + "</td></tr></table>";
            lbl_adminnoteby.Text = MainServerSettings.AdminNoteBy;
        }
        else {
            pnl_adminnote.Enabled = false;
            pnl_adminnote.Visible = false;
        }
    }

    #endregion

}