using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.AutoUpdates;
using SocialSignInApis;
using System.Text;

public partial class GroupLogin : BasePage {


    protected void Page_Load(object sender, EventArgs e) {
        ServerSettings.StartServerApplication(IsPostBack);

        // Check to see if social Login is valid
        SocialSignIn.CheckSocialSignIn();
        if (IsUserNameEqualToAdmin()) {
            HelperMethods.PageRedirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
        }

        if (BaseMaster.IsPageValid(Page, MainServerSettings.NoLoginRequired)) {
            if (CurrentUserMembership == null) {
                FormsAuthentication.SignOut();
                HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
            }

            if (CurrentUserGroupList.Count == 1) {
                GroupSessions.AddOrSetNewGroupLoginSession(CurrentUsername, CurrentUserMemberDatabase.GroupList[0]);
                HelperMethods.PageRedirect("~/Default.aspx");
            }

            BaseMaster baseMaster = new BaseMaster();
            baseMaster.LoadAllDefaultScriptsStyleSheets(Page, CurrentUserMemberDatabase);

            StringBuilder _strScriptreg = new StringBuilder();
            _strScriptreg.Append("openWSE_Config.siteTheme='" + CurrentUserMemberDatabase.SiteTheme + "';");
            _strScriptreg.Append("openWSE_Config.siteName='" + ServerSettings.SiteName + "';");
            _strScriptreg.Append("openWSE_Config.siteRootFolder='" + ResolveUrl("~/").Replace("/", "") + "';");
            _strScriptreg.Append("openWSE_Config.showToolTips=" + CurrentUserMemberDatabase.ShowToolTips.ToString().ToLower() + ";");
            _strScriptreg.Append("openWSE_Config.reportOnError=" + (!MainServerSettings.DisableJavascriptErrorAlerts).ToString().ToLower() + ";");
            RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());

            BaseMaster.SetHelpIcon(Page);
            LoadCustomizations();
        }
        else {
            FormsAuthentication.SignOut();
            HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
        }

        ltl_footercopyright.Text = HelperMethods.GetCopyrightFooterText();
    }

    private void LoadCustomizations() {
        BaseMaster.SetTopLogoTags(this.Page, lnk_BackToWorkspace, null);

        lnk_BackToWorkspace.HRef = string.Empty;
        lnk_BackToWorkspace.Title = string.Empty;
        lnk_BackToWorkspace.Attributes["onclick"] = "return false";
        lnk_BackToWorkspace.Style["cursor"] = "default!important";

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

        #region Site Color Option
        string siteColorOption = CurrentUserMemberDatabase.SiteColorOption;
        string selectedColorIndex = "1";
        string[] siteOptionSplit = siteColorOption.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
        if (siteOptionSplit.Length >= 1) {
            selectedColorIndex = siteOptionSplit[0];
        }

        site_mainbody.Attributes["data-coloroption"] = selectedColorIndex;
        div_ColorOptionsHolder.InnerHtml = HelperMethods.BuildColorOptionList("openWSE.ThemeColorOption_Clicked(this);", "openWSE.ColorOption_Changed(this);", "openWSE.ResetColorOption_Clicked(this);", siteColorOption, this.Page);
        #endregion

        #region Site Layout Option
        site_mainbody.Attributes["data-layoutoption"] = CurrentUserMemberDatabase.SiteLayoutOption;
        if (CurrentUserMemberDatabase.SiteLayoutOption == "Boxed") {
            rb_BoxedLayout.Checked = true;
            rb_WideLayout.Checked = false;
        }
        else {
            rb_WideLayout.Checked = true;
            rb_BoxedLayout.Checked = false;
        }
        #endregion
    }

    protected void lbtn_signoff_Click(object sender, EventArgs e) {
        BaseMaster.SignUserOff(CurrentUsername);
    }

}