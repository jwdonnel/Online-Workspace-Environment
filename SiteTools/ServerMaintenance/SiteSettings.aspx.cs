#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Image = System.Drawing.Image;
using System.Net.Mail;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.AutoUpdates;
using System.Xml;

#endregion

public partial class SiteTools_SiteSettings : Page {
    #region private variables

    private readonly Notifications _notifications = new Notifications();
    private ServerSettings _ss = new ServerSettings();
    private string _sitetheme = "Standard";
    private string _username;

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated && !ServerSettings.NeedToLoadAdminNewMemberPage) {
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else if (userId.IsAuthenticated) {
            #region Load Settings
            if ((ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) || (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower())) {
                _username = userId.Name;

                PageLoadInit.BuildLinks(pnlLinkBtns, _username, this.Page);

                var member = new MemberDatabase(_username);
                _sitetheme = member.SiteTheme;

                MembershipUser mUser = Membership.GetUser(_username);
                if (string.IsNullOrEmpty(mUser.Email)) {
                    lbtn_SendTestEmail.Enabled = false;
                    lbtn_SendTestEmail.Visible = false;
                }
                else
                    lbtn_SendTestEmail.Text = "Send test email to " + mUser.Email;

                lbl_lastcacheclear.Text = _ss.LastCacheClearDate.ToString();
                BuildSettingsList();
                var scriptManager = ScriptManager.GetCurrent(Page);
                if (scriptManager != null) {
                    scriptManager.RegisterPostBackControl(btn_uploadlogo);
                    scriptManager.RegisterPostBackControl(btn_uploadlogo_fav);
                    scriptManager.RegisterPostBackControl(btn_uploadbgImage);
                }
                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    lbl_adminnoteby.Enabled = false;
                    btn_updateadminnote.Enabled = false;
                    btn_updateadminnote.Visible = false;
                    pnl_updateadminnote.Enabled = false;
                    pnl_updateadminnote.Visible = false;
                    pnl_Customizations.Enabled = false;
                    pnl_Customizations.Visible = false;

                    btn_updatemailsettings.Enabled = false;
                    btn_updatemailsettings.Visible = false;
                    lbtn_testconnection.Enabled = false;
                    lbtn_testconnection.Visible = false;

                    var strStandard = new StringBuilder();
                    strStandard.Append("$('#MainContent_btn_updateadminnote_clear').remove();");
                    strStandard.Append("$('#ClearAppProp_Controls').remove();");
                    strStandard.Append("$('#ClearUserNoti_Controls').remove();");
                    strStandard.Append("$('#ChatClient_Controls').remove();");
                    RegisterPostbackScripts.RegisterStartupScript(this, strStandard.ToString());
                }

                if (!string.IsNullOrEmpty(_ss.AdminNote)) {
                    lbl_adminnoteby.Text = "<b class='pad-right-sml'>Updated by:</b>" + _ss.AdminNoteBy;
                }

                if (_ss.NoLoginRequired) {
                    pnlLoginMessage.Enabled = false;
                    pnlLoginMessage.Visible = false;
                }
                else {
                    pnlLoginMessage.Enabled = true;
                    pnlLoginMessage.Visible = true;
                }

                string postbackCtrl = ScriptManager.GetCurrent(Page).AsyncPostBackSourceElementID;
                SetServerTimezone(postbackCtrl);

                if (!IsPostBack) {
                    tb_totalWorkspacesAllowed.Text = _ss.TotalWorkspacesAllowed.ToString();
                }

                if (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    if (!IsPostBack) {
                        LoadTwitterSettings();
                        LoadGoogleSettings();
                        LoadFacebookSettings();
                        pnl_admincontrolsonly.Enabled = true;
                        pnl_admincontrolsonly.Visible = true;
                        lbl_dateUpdated_sup.Text = _ss.ShowUpdatesPopupDate;
                        if (_ss.SiteOffLine) {
                            rb_siteoffline.Checked = true;
                            rb_siteonline.Checked = false;
                        }
                        else {
                            rb_siteoffline.Checked = false;
                            rb_siteonline.Checked = true;
                        }

                        if (_ss.SSLRedirect) {
                            rb_sslredirect_on.Checked = true;
                            rb_sslredirect_off.Checked = false;
                            pnl_sslValidation.Enabled = true;
                            pnl_sslValidation.Visible = true;
                        }
                        else {
                            rb_sslredirect_on.Checked = false;
                            rb_sslredirect_off.Checked = true;
                            pnl_sslValidation.Enabled = false;
                            pnl_sslValidation.Visible = false;
                        }


                        tbCustomErrorPageRedirect.Text = GetCustomErrorRedirectSettings();
                        if (GetCustomErrorSettings()) {
                            rb_CustomErrorPage_on.Checked = true;
                            rb_CustomErrorPage_off.Checked = false;
                            pnl_ErrorPageRedirect.Enabled = true;
                            pnl_ErrorPageRedirect.Visible = true;
                        }
                        else {
                            rb_CustomErrorPage_on.Checked = false;
                            rb_CustomErrorPage_off.Checked = true;
                            pnl_ErrorPageRedirect.Enabled = false;
                            pnl_ErrorPageRedirect.Visible = false;
                        }

                        if (_ss.SaveCookiesAsSessions) {
                            rb_SaveCookiesAsSessions_on.Checked = true;
                            rb_SaveCookiesAsSessions_off.Checked = false;
                        }
                        else {
                            rb_SaveCookiesAsSessions_on.Checked = false;
                            rb_SaveCookiesAsSessions_off.Checked = true;
                        }

                        if (_ss.AssociateWithGroups) {
                            rb_AssociateWithGroups_on.Checked = true;
                            rb_AssociateWithGroups_off.Checked = false;
                        }
                        else {
                            rb_AssociateWithGroups_on.Checked = false;
                            rb_AssociateWithGroups_off.Checked = true;
                        }

                        if (_ss.URLValidation) {
                            rb_urlvalidation_on.Checked = true;
                            rb_urlvalidation_off.Checked = false;
                        }
                        else {
                            rb_urlvalidation_on.Checked = false;
                            rb_urlvalidation_off.Checked = true;
                        }

                        if (_ss.LockFileManager) {
                            rb_LockFileManager_on.Checked = true;
                            rb_LockFileManager_off.Checked = false;
                        }
                        else {
                            rb_LockFileManager_on.Checked = false;
                            rb_LockFileManager_off.Checked = true;
                        }

                        if (_ss.LockStartupScripts) {
                            rb_Lockstartupscripts_on.Checked = true;
                            rb_Lockstartupscripts_off.Checked = false;
                        }
                        else {
                            rb_Lockstartupscripts_on.Checked = false;
                            rb_Lockstartupscripts_off.Checked = true;
                        }

                        if (_ss.LockIPListenerWatchlist) {
                            rb_Lockiplisteneron.Checked = true;
                            rb_Lockiplisteneroff.Checked = false;
                        }
                        else {
                            rb_Lockiplisteneron.Checked = false;
                            rb_Lockiplisteneroff.Checked = true;
                        }

                        if (_ss.CustomizationsLocked) {
                            rb_siteCustomizations_On.Checked = true;
                            rb_siteCustomizations_Off.Checked = false;
                        }
                        else {
                            rb_siteCustomizations_On.Checked = false;
                            rb_siteCustomizations_Off.Checked = true;
                        }

                        if (_ss.AllowAppRating) {
                            rb_allowapprating_on.Checked = true;
                            rb_allowapprating_off.Checked = false;
                        }
                        else {
                            rb_allowapprating_on.Checked = false;
                            rb_allowapprating_off.Checked = true;
                        }

                        if (_ss.AllowPrivacy) {
                            rb_allowUserPrivacy_on.Checked = true;
                            rb_allowUserPrivacy_off.Checked = false;
                        }
                        else {
                            rb_allowUserPrivacy_on.Checked = false;
                            rb_allowUserPrivacy_off.Checked = true;
                        }

                        if (_ss.LockCustomTables) {
                            rb_lockcustomtables_on.Checked = true;
                            rb_lockcustomtables_off.Checked = false;
                        }
                        else {
                            rb_lockcustomtables_on.Checked = false;
                            rb_lockcustomtables_off.Checked = true;
                        }

                        if (_ss.LockAppCreator) {
                            rb_LockAppCreator_on.Checked = true;
                            rb_LockAppCreator_off.Checked = false;
                        }
                        else {
                            rb_LockAppCreator_on.Checked = false;
                            rb_LockAppCreator_off.Checked = true;
                        }

                        if (_ss.LockBackgroundServices) {
                            rb_LockBackgroundServices_on.Checked = true;
                            rb_LockBackgroundServices_off.Checked = false;
                        }
                        else {
                            rb_LockBackgroundServices_on.Checked = false;
                            rb_LockBackgroundServices_off.Checked = true;
                        }

                        #region Preview and No Login Settings
                        bool noLoginrequired = _ss.NoLoginRequired;
                        bool showPreviewBtn = _ss.ShowPreviewButtonLogin;
                        if (noLoginrequired) {
                            rb_nologinrequired_on.Checked = true;
                            rb_nologinrequired_off.Checked = false;
                            pnl_showpreviewbutton.Enabled = false;
                            pnl_showpreviewbutton.Visible = false;
                            pnl_showloginmodalondemomode.Enabled = true;
                            pnl_showloginmodalondemomode.Visible = true;
                        }
                        else {
                            rb_nologinrequired_on.Checked = false;
                            rb_nologinrequired_off.Checked = true;
                            pnl_showpreviewbutton.Enabled = true;
                            pnl_showpreviewbutton.Visible = true;
                            pnl_showloginmodalondemomode.Enabled = false;
                            pnl_showloginmodalondemomode.Visible = false;
                        }

                        if (_ss.ShowLoginModalOnDemoMode) {
                            rb_ShowLoginModalOnDemoMode_on.Checked = true;
                            rb_ShowLoginModalOnDemoMode_off.Checked = false;
                        }
                        else {
                            rb_ShowLoginModalOnDemoMode_on.Checked = false;
                            rb_ShowLoginModalOnDemoMode_off.Checked = true;
                        }

                        if (showPreviewBtn) {
                            rb_ShowPreviewButtonLogin_on.Checked = true;
                            rb_ShowPreviewButtonLogin_off.Checked = false;
                        }
                        else {
                            rb_ShowPreviewButtonLogin_on.Checked = false;
                            rb_ShowPreviewButtonLogin_off.Checked = true;
                        }

                        if ((!noLoginrequired) && (!showPreviewBtn)) {
                            pnl_NoLoginMainPage.Enabled = false;
                            pnl_NoLoginMainPage.Visible = false;
                            pnl_showpreviewbutton.Enabled = true;
                            pnl_showpreviewbutton.Visible = true;
                        }
                        else if ((noLoginrequired) && (showPreviewBtn)) {
                            pnl_NoLoginMainPage.Enabled = true;
                            pnl_NoLoginMainPage.Visible = true;
                            pnl_showpreviewbutton.Enabled = false;
                            pnl_showpreviewbutton.Visible = false;
                        }
                        else {
                            pnl_NoLoginMainPage.Enabled = true;
                            pnl_NoLoginMainPage.Visible = true;
                        }


                        if (_ss.ForceGroupLogin) {
                            rb_ForceGroupLogin_on.Checked = true;
                            rb_ForceGroupLogin_off.Checked = false;
                            pnl_showpreviewbutton.Enabled = false;
                            pnl_showpreviewbutton.Visible = false;
                            pnl_nologinrequired.Enabled = false;
                            pnl_nologinrequired.Visible = false;
                            pnl_NoLoginMainPage.Enabled = false;
                            pnl_NoLoginMainPage.Visible = false;
                        }
                        else {
                            rb_ForceGroupLogin_on.Checked = false;
                            rb_ForceGroupLogin_off.Checked = true;
                        }
                        #endregion


                        if (_ss.SitePluginsLocked) {
                            rb_siteplugins_on.Checked = true;
                            rb_siteplugins_off.Checked = false;
                        }
                        else {
                            rb_siteplugins_on.Checked = false;
                            rb_siteplugins_off.Checked = true;
                        }

                        if (_ss.NotificationsLocked) {
                            rb_sitenotifi_on.Checked = true;
                            rb_sitenotifi_off.Checked = false;
                        }
                        else {
                            rb_sitenotifi_on.Checked = false;
                            rb_sitenotifi_off.Checked = true;
                        }

                        if (_ss.OverlaysLocked) {
                            rb_siteoverlay_on.Checked = true;
                            rb_siteoverlay_off.Checked = false;
                        }
                        else {
                            rb_siteoverlay_on.Checked = false;
                            rb_siteoverlay_off.Checked = true;
                        }

                        if (_ss.EmailOnRegister) {
                            rb_emailonReg_on.Checked = true;
                            rb_emailonReg_off.Checked = false;
                        }
                        else {
                            rb_emailonReg_on.Checked = false;
                            rb_emailonReg_off.Checked = true;
                        }
                    }

                    CustomizationOptionsEnabledDisabled(true);
                    LoadCustomizations();

                    if (_ss.CustomizationsLocked) {
                        pnl_Customizations.Enabled = false;
                        pnl_Customizations.Visible = false;
                        btn_customizeSMTP.Visible = false;
                    }
                }
                else {
                    CustomizationOptionsEnabledDisabled(false);
                    if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                        LoadCustomizationControls();
                    }
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
            #endregion

        }
    }

    private bool GetCustomErrorSettings() {
        string path = HttpContext.Current.Server.MapPath("~/Web.Config");
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        XmlNode node = doc.DocumentElement.SelectSingleNode("system.web/customErrors");
        if (node != null) {
            if (node.Attributes["mode"] != null && !string.IsNullOrEmpty(node.Attributes["mode"].Value)) {
                if (node.Attributes["mode"].Value == "Off") {
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    private string GetCustomErrorRedirectSettings() {
        string path = HttpContext.Current.Server.MapPath("~/Web.Config");
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        XmlNode node = doc.DocumentElement.SelectSingleNode("system.web/customErrors");
        if (node != null) {
            if (node.Attributes["defaultRedirect"] != null) {
                return node.Attributes["defaultRedirect"].Value;
            }
        }

        return string.Empty;
    }

    private void CustomizationOptionsEnabledDisabled(bool x) {
        pnl_ImageCustomizations.Enabled = x;
        pnl_ImageCustomizations.Visible = x;
        pnl_MainSiteLogoDesc.Enabled = x;
        pnl_MainSiteLogoDesc.Visible = x;
        pnl_MainSiteLogoUpload.Enabled = x;
        pnl_MainSiteLogoUpload.Visible = x;
    }

    private void LoadCustomizationControls() {
        pnl_Customizations.Enabled = true;
        pnl_Customizations.Visible = true;
        if (_ss.CustomizationsLocked) {
            pnl_Customizations.Enabled = false;
            pnl_Customizations.Visible = false;
            btn_customizeSMTP.Visible = false;
        }
        else {
            LoadCustomizations();
        }
    }

    private void LoadCustomizations() {
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        if (sm != null) {
            string ctlId = sm.AsyncPostBackSourceElementID;
            if (!ctlId.Contains("btn_updateLogoOpacity")) {
                GetLogoOpacity();
            }

            if (!ctlId.Contains("hf_keywordsMetaTag") && !ctlId.Contains("btn_descriptionMetaTag")) {
                if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    pnl_meteTagCustomizations.Enabled = true;
                    pnl_meteTagCustomizations.Visible = true;
                    LoadMetaTags();
                }
                else {
                    pnl_meteTagCustomizations.Enabled = false;
                    pnl_meteTagCustomizations.Visible = false;
                }
            }

        }

        BuildCategoryIconList();

        if (!IsPostBack) {
            BuildBGList();
            if (_ss.EmailSystemStatus) {
                pnl_emailStatus_holder.Enabled = true;
                pnl_emailStatus_holder.Visible = true;
                rb_emailStatus_on.Checked = true;
                rb_emailStatus_off.Checked = false;
                lbl_emailStatus.Text = "On";
                lbl_emailStatus.Style["color"] = "#267F00";
                tb_smtpserver.Enabled = true;
                tb_portnumber.Enabled = true;
                tb_usernamesmtp.Enabled = true;
                tb_passwordsmtp.Enabled = true;
                btn_updatemailsettings.Enabled = true;
                lbtn_SendTestEmail.Visible = true;
                lbtn_testconnection.Visible = true;
            }
            else {
                pnl_emailStatus_holder.Enabled = false;
                pnl_emailStatus_holder.Visible = false;
                rb_emailStatus_on.Checked = false;
                rb_emailStatus_off.Checked = true;
                lbl_emailStatus.Text = "Off";
                lbl_emailStatus.Style["color"] = "#FF0000";
                tb_smtpserver.Enabled = false;
                tb_portnumber.Enabled = false;
                tb_usernamesmtp.Enabled = false;
                tb_passwordsmtp.Enabled = false;
                btn_updatemailsettings.Enabled = false;
                lbtn_SendTestEmail.Visible = false;
                lbtn_testconnection.Visible = false;
            }

            if (_ss.HideAllAppIcons) {
                rb_hideAllAppIcons_on.Checked = true;
                rb_hideAllAppIcons_off.Checked = false;
            }
            else {
                rb_hideAllAppIcons_on.Checked = false;
                rb_hideAllAppIcons_off.Checked = true;
            }

            if (_ss.AddBackgroundToLogo) {
                rb_AddBackgroundToLogo_on.Checked = true;
                rb_AddBackgroundToLogo_off.Checked = false;
                pnl_logobackgroundColor.Enabled = true;
                pnl_logobackgroundColor.Visible = true;
            }
            else {
                rb_AddBackgroundToLogo_on.Checked = false;
                rb_AddBackgroundToLogo_off.Checked = true;
                pnl_logobackgroundColor.Enabled = false;
                pnl_logobackgroundColor.Visible = false;
            }

            tb_logoBgColor.Text = _ss.LogoBackgroundColor_NonTranslated;

            if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                pnl_meteTagCustomizations.Enabled = true;
                pnl_meteTagCustomizations.Visible = true;
                LoadMetaTags();
                GetSiteMapXml();
                GetRobotsTxt();
            }
            else {
                pnl_meteTagCustomizations.Enabled = false;
                pnl_meteTagCustomizations.Visible = false;
            }

            img_workspaceLogo.ImageUrl = "~/Standard_Images/logo.png?date=" + ServerSettings.ServerDateTime.Ticks;
            img_Favicon.ImageUrl = "~/Standard_Images/favicon.ico?date=" + ServerSettings.ServerDateTime.Ticks;
        }
    }

    private void LoadMetaTags() {
        tb_descriptionMetaTag.Text = string.Empty;
        pnl_keywordsMetaTag.Controls.Clear();

        tb_descriptionMetaTag.Text = _ss.MetaTagDescription;
        if (string.IsNullOrEmpty(tb_descriptionMetaTag.Text)) {
            lbtn_clearDescriptionMeta.Enabled = false;
            lbtn_clearDescriptionMeta.Visible = false;
        }
        else {
            lbtn_clearDescriptionMeta.Enabled = true;
            lbtn_clearDescriptionMeta.Visible = true;
        }

        string[] splitKeywords = _ss.MetaTagKeywords.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        if (splitKeywords.Length == 0) {
            lbtn_clearAllKeywordsMeta.Enabled = false;
            lbtn_clearAllKeywordsMeta.Visible = false;
        }
        else {
            lbtn_clearAllKeywordsMeta.Enabled = true;
            lbtn_clearAllKeywordsMeta.Visible = true;
            foreach (string keyword in splitKeywords) {
                pnl_keywordsMetaTag.Controls.Add(new LiteralControl("<div class='keyword-split-array-item'><span class='text'>" + keyword + "</span><span class='keyword-split-array-input-remove' title='Remove'></span></div>"));
            }
        }
    }

    private void GetSiteMapXml() {
        string path = ServerSettings.GetServerMapLocation + CreateSitemap.SiteMapFileName;
        if (File.Exists(path)) {
            lbl_siteMapModified.Text = File.GetLastWriteTime(path).ToString();
            pnl_viewDeleteSiteMap.Enabled = true;
            pnl_viewDeleteSiteMap.Visible = true;
            hyp_viewSiteMap.NavigateUrl = ResolveUrl("~/" + CreateSitemap.SiteMapFileName);
        }
        else {
            lbl_siteMapModified.Text = "N/A";
            pnl_viewDeleteSiteMap.Enabled = false;
            pnl_viewDeleteSiteMap.Visible = false;
        }
    }

    private void GetRobotsTxt() {
        string path = ServerSettings.GetServerMapLocation + CreateRobotTxt.RobotsFileName;
        if (File.Exists(path)) {
            lbl_robotsLastModified.Text = File.GetLastWriteTime(path).ToString();
            pnl_viewDeleteRobotTxt.Enabled = true;
            pnl_viewDeleteRobotTxt.Visible = true;
            hyp_viewRobotsTxt.NavigateUrl = ResolveUrl("~/" + CreateRobotTxt.RobotsFileName);
        }
        else {
            lbl_robotsLastModified.Text = "N/A";
            pnl_viewDeleteRobotTxt.Enabled = false;
            pnl_viewDeleteRobotTxt.Visible = false;
        }
    }

    private void LoadTwitterSettings() {
        tb_updateTwitterAccessToken.Text = _ss.TwitterAccessToken;

        tb_updateTwitterAccessTokenSecret.Text = string.Empty;
        if (!string.IsNullOrEmpty(_ss.TwitterAccessToken)) {
            tb_updateTwitterAccessTokenSecret.Text = "***************************************";
        }

        tb_updateTwitterConsumerKey.Text = _ss.TwitterConsumerKey;

        tb_updateTwitterConsumerSecret.Text = string.Empty;
        if (!string.IsNullOrEmpty(_ss.TwitterConsumerSecret)) {
            tb_updateTwitterConsumerSecret.Text = "***************************************";
        }
    }

    private void LoadGoogleSettings() {
        txt_GoogleClientId.Text = _ss.GoogleClientId;

        txt_GoogleClientSecret.Text = string.Empty;
        if (!string.IsNullOrEmpty(_ss.GoogleClientSecret)) {
            txt_GoogleClientSecret.Text = "***************************************";
        }

        StringBuilder strUrls = new StringBuilder();
        List<string> redirectUrls = GetRedirectUrls(SocialSignInApis.SocialSignIn.GoogleRedirectQuery);
        foreach (string url in redirectUrls) {
            strUrls.Append("<h4>" + url + "</h4><div class='clear-space-two'></div>");
        }

        ltl_googleRedirect.Text = strUrls.ToString();
    }

    private void LoadFacebookSettings() {
        txt_facebookAppId.Text = _ss.FacebookAppId;

        txt_facebookAppSecret.Text = string.Empty;
        if (!string.IsNullOrEmpty(_ss.FacebookAppSecret)) {
            txt_facebookAppSecret.Text = "***************************************";
        }

        StringBuilder strUrls = new StringBuilder();
        List<string> redirectUrls = GetRedirectUrls(SocialSignInApis.SocialSignIn.FacebookRedirectQuery);
        foreach (string url in redirectUrls) {
            strUrls.Append("<h4>" + url + "</h4><div class='clear-space-two'></div>");
        }

        ltl_facebookRedirect.Text = strUrls.ToString();
    }

    private List<string> GetRedirectUrls(string query) {
        string mainUrl = Request.Url.AbsoluteUri.Split('?')[0].Replace("SiteTools/ServerMaintenance/SiteSettings.aspx", "");

        List<string> urls = new List<string>();
        urls.Add(mainUrl + "Workspace.aspx" + query);
        urls.Add(mainUrl + ServerSettings.DefaultStartupPage + query);
        urls.Add(mainUrl + "AppRemote.aspx" + query);

        return urls;
    }

    private void GetLogoOpacity() {
        string logoopacity = _ss.LogoOpacity;
        if (string.IsNullOrEmpty(logoopacity)) {
            logoopacity = "1.0";
        }

        lbl_workspaceLogo.Text = string.Empty;
        string initializeSlider = string.Empty;
        if ((logoopacity) == "0.0" || (logoopacity == "0")) {
            lbl_workspaceLogo.Text = "Logo is 100% transparent";
            initializeSlider += "$('#MainContent_img_workspaceLogo').hide();";
        }

        initializeSlider += "$('#MainContent_img_workspaceLogo').css('opacity', '" + logoopacity + "');$('#Slider1').slider({value: " + logoopacity + ", range: 'min', min: 0,max: 1.05, step: .05, slide: function (event, ui) { ";
        initializeSlider += "$('#hf_opacity').val($('#Slider1').slider('value')); $('#currentLogoOpacity').html($('#Slider1').slider('value')); $('#MainContent_lbl_workspaceLogo').html(''); $('#MainContent_img_workspaceLogo').show(); $('#MainContent_img_workspaceLogo').css('opacity', $('#Slider1').slider('value')); }});";
        initializeSlider += "$('#currentLogoOpacity').html('" + logoopacity + "');";
        RegisterPostbackScripts.RegisterStartupScript(this, initializeSlider);
    }

    private void BuildSettingsList() {
        if (IsPostBack) return;
        tb_adminnote.Text = _ss.AdminNote;
        tb_updateFolder.Text = _ss.DocumentsFolder;

        tb_loginPageMessage.Text = _ss.LoginMessage;
        lbl_loginMessageDate.Text = "<b class='pad-right-sml'>Date Updated:</b>" + _ss.LoginMessageDate;

        rb_cachehp_off.Checked = true;
        rb_cachehp_on.Checked = false;
        if (_ss.CacheHomePage) {
            rb_cachehp_off.Checked = false;
            rb_cachehp_on.Checked = true;
        }

        BuildDefaultFontFamilyList();

        tb_defaultfontsize.Text = "";
        int tempFontSize = 0;
        if (!string.IsNullOrEmpty(_ss.DefaultBodyFontSize) && int.TryParse(_ss.DefaultBodyFontSize, out tempFontSize)) {
            tb_defaultfontsize.Text = _ss.DefaultBodyFontSize;
        }

        tb_defaultfontcolor.Text = "";
        string fontColor = _ss.DefaultBodyFontColor;
        if (!string.IsNullOrEmpty(fontColor)) {
            tb_defaultfontcolor.Text = fontColor.Replace("#", string.Empty);
        }

        if (_ss.ChatEnabled) {
            rb_chatclient_on.Checked = true;
            rb_chatclient_off.Checked = false;
        }
        else {
            rb_chatclient_on.Checked = false;
            rb_chatclient_off.Checked = true;
        }

        rb_ssl_disabled.Checked = true;
        rb_ssl_enabled.Checked = false;
        ServerSettings.GetMailSettingList();
        if (ServerSettings.MailSettings_Coll.Count > 0) {
            int index = ServerSettings.MailSettings_Coll.Count - 1;
            tb_smtpserver.Text = ServerSettings.MailSettings_Coll[index].SMTP_Address;
            tb_portnumber.Text = ServerSettings.MailSettings_Coll[index].PortNumber;
            tb_usernamesmtp.Text = ServerSettings.MailSettings_Coll[index].EmailAddress;
            tb_passwordsmtp.Text = "**********";
            var member = new MemberDatabase(ServerSettings.MailSettings_Coll[index].User);
            lbl_updatedbymailsettings.Text = HelperMethods.MergeFMLNames(member);
            lbl_dateupdatedmailsettings.Text = ServerSettings.MailSettings_Coll[index].Date;
            if (ServerSettings.MailSettings_Coll[index].SSL) {
                rb_ssl_disabled.Checked = false;
                rb_ssl_enabled.Checked = true;
            }
            else {
                rb_ssl_disabled.Checked = true;
                rb_ssl_enabled.Checked = false;
            }
        }

        if (_ss.EmailSystemStatus) {
            pnl_emailStatus_holder.Enabled = true;
            pnl_emailStatus_holder.Visible = true;
            rb_emailStatus_on.Checked = true;
            rb_emailStatus_on.Checked = false;
            lbl_emailStatus.Text = "On";
            lbl_emailStatus.Style["color"] = "#267F00";
        }
        else {
            pnl_emailStatus_holder.Enabled = false;
            pnl_emailStatus_holder.Visible = false;
            rb_emailStatus_on.Checked = false;
            rb_emailStatus_on.Checked = true;
            lbl_emailStatus.Text = "Off";
            lbl_emailStatus.Style["color"] = "#FF0000";
        }
    }

    private void SetServerTimezone(string postbackCtrl) {
        if (string.IsNullOrEmpty(postbackCtrl)) {
            postbackCtrl = string.Empty;
        }

        if (!postbackCtrl.Contains("btn_timezoneset")) {
            string currTimezone = _ss.ServerTimezone.ToString();
            foreach (ListItem item in dd_timezoneset.Items) {
                if (item.Value == currTimezone) {
                    item.Selected = true;
                }
                else {
                    item.Selected = false;
                }
            }

            lbl_currentServerTime.Text = ServerSettings.ServerDateTime.ToString();

            double timezone = ServerSettings.GetTimezoneDaylightSavingsOffset(_ss.ServerTimezone);
            RegisterPostbackScripts.RegisterStartupScript(this, "UpdateCurrentTime(" + timezone.ToString() + ");");
        }
    }

    private void BuildBGList() {
        dd_bgmanage.Items.Clear();
        ListItem item = new ListItem("--Select a background--", "");
        dd_bgmanage.Items.Add(item);

        string[] directories = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
        foreach (string filename in directories) {
            var fi = new FileInfo(filename);
            if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                ListItem item2 = new ListItem(fi.Name, "~/Standard_Images/Backgrounds/" + fi.Name);
                dd_bgmanage.Items.Add(item2);
            }
        }
    }

    private void BuildCategoryIconList() {
        pnl_SidebarCategoryIcons.Controls.Clear();

        SideBarItems sbi = new SideBarItems(_username);
        SortedDictionary<string, string> categoryList = sbi.ListOfAdminPageCategories();

        StringBuilder str = new StringBuilder();

        #region App List
        str.AppendFormat("<div class='table-settings-box no-margin no-padding no-border'>");

        string keyValImgAL = _ss.GetSidebarCategoryIcon("My Apps");
        string imgUrlAL = string.Empty;
        if (!string.IsNullOrEmpty(keyValImgAL)) {
            if (keyValImgAL.StartsWith("~/")) {
                imgUrlAL = ResolveUrl(keyValImgAL.Trim());
            }
            else {
                imgUrlAL = keyValImgAL.Trim();
            }
        }

        string imgTagAL = "<img alt='' src='" + imgUrlAL + "' class='float-left margin-right' style='height: 16px;' />";
        str.AppendFormat("<div class='td-settings-title'>{0}</div><div class='title-line'></div>", imgTagAL + "My Apps");

        string inputTextAL = "<input type='text' class='textEntry margin-right sidebar-categoryicon' data-field='My_Apps' value='" + keyValImgAL.Trim() + "' style='width: 80%;' />";
        string inputBtnAL = "<input type='button' class='input-buttons' value='Update' onclick='UpdateSidebarImage(this);' />";

        str.AppendFormat("<div class='td-settings-ctrl'>{0}{1}</div>", inputTextAL, inputBtnAL);
        str.AppendFormat("</div>");
        #endregion


        #region Chat Client
        str.AppendFormat("<div class='table-settings-box no-margin no-padding no-border'>");

        string keyValImgC = _ss.GetSidebarCategoryIcon("Chat");
        string imgUrlC = string.Empty;
        if (!string.IsNullOrEmpty(keyValImgC)) {
            if (keyValImgC.StartsWith("~/")) {
                imgUrlC = ResolveUrl(keyValImgC.Trim());
            }
            else {
                imgUrlC = keyValImgC.Trim();
            }
        }

        string imgTagC = "<img alt='' src='" + imgUrlC + "' class='float-left margin-right' style='height: 16px;' />";
        str.AppendFormat("<div class='td-settings-title'>{0}</div><div class='title-line'></div>", imgTagC + "Chat");

        string inputTextC = "<input type='text' class='textEntry margin-right sidebar-categoryicon' data-field='Chat' value='" + keyValImgC.Trim() + "' style='width: 80%;' />";
        string inputBtnC = "<input type='button' class='input-buttons' value='Update' onclick='UpdateSidebarImage(this);' />";

        str.AppendFormat("<div class='td-settings-ctrl'>{0}{1}</div>", inputTextC, inputBtnC);
        str.AppendFormat("</div>");
        #endregion


        foreach (KeyValuePair<string, string> keyVal in categoryList) {
            str.AppendFormat("<div class='table-settings-box no-margin no-padding no-border'>");

            string imgUrl = string.Empty;
            if (!string.IsNullOrEmpty(keyVal.Value)) {
                if (keyVal.Value.StartsWith("~/")) {
                    imgUrl = ResolveUrl(keyVal.Value.Trim());
                }
                else {
                    imgUrl = keyVal.Value.Trim();
                }
            }

            string imgTag = "<img alt='' src='" + imgUrl + "' class='float-left margin-right' style='height: 16px;' />";
            str.AppendFormat("<div class='td-settings-title'>{0}</div><div class='title-line'></div>", imgTag + keyVal.Key.Trim());

            string inputText = "<input type='text' class='textEntry margin-right sidebar-categoryicon' data-field='" + keyVal.Key.Trim() + "' value='" + keyVal.Value.Trim() + "' style='width: 80%;' />";
            string inputBtn = "<input type='button' class='input-buttons' value='Update' onclick='UpdateSidebarImage(this);' />";

            str.AppendFormat("<div class='td-settings-ctrl'>{0}{1}</div>", inputText, inputBtn);
            str.AppendFormat("</div>");
        }


        #region No Category

        str.AppendFormat("<div class='table-settings-box no-margin no-padding no-border'>");

        string keyValImgNoCategory = _ss.GetSidebarCategoryIcon("Settings and Tools");
        string imgUrlNoCategory = string.Empty;
        if (!string.IsNullOrEmpty(keyValImgNoCategory)) {
            if (keyValImgNoCategory.StartsWith("~/")) {
                imgUrlNoCategory = ResolveUrl(keyValImgNoCategory.Trim());
            }
            else {
                imgUrlNoCategory = keyValImgNoCategory.Trim();
            }
        }

        string imgTagNoCategory = "<img alt='' src='" + imgUrlNoCategory + "' class='float-left margin-right' style='height: 16px;' />";
        str.AppendFormat("<div class='td-settings-title'>{0}</div><div class='title-line'></div>", imgTagNoCategory + "Settings/Tools");

        string inputTextNoCategory = "<input type='text' class='textEntry margin-right sidebar-categoryicon' data-field='Settings_and_Tools' value='" + keyValImgNoCategory.Trim() + "' style='width: 80%;' />";
        string inputBtnNoCategory = "<input type='button' class='input-buttons' value='Update' onclick='UpdateSidebarImage(this);' />";

        str.AppendFormat("<div class='td-settings-ctrl'>{0}{1}</div>", inputTextNoCategory, inputBtnNoCategory);
        str.AppendFormat("</div>");

        #endregion

        pnl_SidebarCategoryIcons.Controls.Add(new LiteralControl(str.ToString()));
    }
    protected void hf_SidebarCategoryIcons_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_SidebarCategoryIcons.Value)) {
            string[] vals = hf_SidebarCategoryIcons.Value.Split('=');
            if (vals.Length == 2) {
                ServerSettings.update_SidebarToolCategoryIcons(vals[0], vals[1]);
            }
        }

        hf_SidebarCategoryIcons.Value = string.Empty;
        BuildCategoryIconList();
    }

    private void BuildDefaultFontFamilyList() {
        dd_defaultbodyfontfamily.Items.Clear();

        dd_defaultbodyfontfamily.Items.Add(new ListItem("Theme Default", string.Empty));
        dd_defaultbodyfontfamily.SelectedIndex = 0;

        string path = ServerSettings.GetServerMapLocation + "CustomFonts";
        if (Directory.Exists(path)) {
            string[] fileList = Directory.GetFiles(path);
            foreach (string file in fileList) {
                if (!string.IsNullOrEmpty(file)) {
                    FileInfo fi = new FileInfo(file);
                    if (fi.Extension.ToLower() == ".css") {
                        string name = fi.Name.Replace("_", " ").Replace(".css", string.Empty);
                        ListItem item = new ListItem(name, fi.Name);
                        if (!dd_defaultbodyfontfamily.Items.Contains(item)) {
                            dd_defaultbodyfontfamily.Items.Add(item);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < dd_defaultbodyfontfamily.Items.Count; i++) {
            if (dd_defaultbodyfontfamily.Items[i].Value == _ss.DefaultBodyFontFamily) {
                dd_defaultbodyfontfamily.SelectedIndex = i;
                break;
            }
        }
    }


    #region Buttons and Server Management

    protected void btn_showUpdates_Click(object sender, EventArgs e) {
        ShowUpdatePopup sup = new ShowUpdatePopup();
        sup.BuildUsers();
        sup.UpdateAllUsers(true);
        string date = ServerSettings.ServerDateTime.ToString();
        ServerSettings.Update_ShowUpdatesPopupDate(date);
        lbl_dateUpdated_sup.Text = date;

    }

    protected void rb_CustomErrorPage_on_CheckedChanged(object sender, EventArgs e) {
        UpdateCustomErrorPageConfigFile(true);
        rb_CustomErrorPage_on.Checked = true;
        rb_CustomErrorPage_off.Checked = false;
        pnl_ErrorPageRedirect.Enabled = true;
        pnl_ErrorPageRedirect.Visible = true;
    }
    protected void rb_CustomErrorPage_off_CheckedChanged(object sender, EventArgs e) {
        UpdateCustomErrorPageConfigFile(false);
        rb_CustomErrorPage_on.Checked = false;
        rb_CustomErrorPage_off.Checked = true;
        pnl_ErrorPageRedirect.Enabled = false;
        pnl_ErrorPageRedirect.Visible = false;
    }
    private void UpdateCustomErrorPageConfigFile(bool isOn) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            try {
                string path = HttpContext.Current.Server.MapPath("~/Web.Config");
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode node = doc.DocumentElement.SelectSingleNode("system.web/customErrors");
                if (node != null) {
                    if (node.Attributes["mode"] == null) {
                        doc.CreateAttribute("mode");
                        node.Attributes.Append(doc.CreateAttribute("mode"));
                    }

                    if (isOn) {
                        node.Attributes["mode"].Value = "On";
                    }
                    else {
                        node.Attributes["mode"].Value = "Off";
                    }

                    doc.Save(path);
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }

    private void UpdateCustomErrorPageRedirectConfigFile(string redirectLoc) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            try {
                string path = HttpContext.Current.Server.MapPath("~/Web.Config");
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode node = doc.DocumentElement.SelectSingleNode("system.web/customErrors");
                if (node != null) {
                    if (node.Attributes["defaultRedirect"] == null) {
                        doc.CreateAttribute("defaultRedirect");
                        node.Attributes.Append(doc.CreateAttribute("defaultRedirect"));
                    }

                    if (string.IsNullOrEmpty(redirectLoc)) {
                        redirectLoc = "ErrorPages\\Error.html";
                    }

                    node.Attributes["defaultRedirect"].Value = redirectLoc;
                    doc.Save(path);
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }

    protected void lbtn_UserDefaultRedirectPage_Click(object sender, EventArgs e) {
        tbCustomErrorPageRedirect.Text = "ErrorPages\\Error.html";
        UpdateCustomErrorPageRedirectConfigFile(tbCustomErrorPageRedirect.Text);
    }

    protected void btnCustomErrorPageRedirect_Click(object sender, EventArgs e) {
        UpdateCustomErrorPageRedirectConfigFile(tbCustomErrorPageRedirect.Text);
    }


    protected void btn_updateTotalWorkspaces_Click(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            int count = 4;
            int.TryParse(tb_totalWorkspacesAllowed.Text.Trim(), out count);
            if ((count <= 0) || (count > 500)) {
                count = 4;
                tb_totalWorkspacesAllowed.Text = "4";
            }

            ServerSettings.update_TotalWorkspacesAllowed(count);

        }
    }

    protected void rb_emailStatus_on_Checked(object sender, EventArgs e) {
        pnl_emailStatus_holder.Enabled = true;
        pnl_emailStatus_holder.Visible = true;
        rb_emailStatus_on.Checked = true;
        rb_emailStatus_off.Checked = false;
        ServerSettings.update_EmailSystemStatus(true);
        lbl_emailStatus.Text = "On";
        lbl_emailStatus.Style["color"] = "#267F00";
        tb_smtpserver.Enabled = true;
        tb_portnumber.Enabled = true;
        tb_usernamesmtp.Enabled = true;
        tb_passwordsmtp.Enabled = true;
        btn_updatemailsettings.Enabled = true;
        lbtn_SendTestEmail.Visible = true;
        lbtn_testconnection.Visible = true;
    }
    protected void rb_emailStatus_off_Checked(object sender, EventArgs e) {
        pnl_emailStatus_holder.Enabled = false;
        pnl_emailStatus_holder.Visible = false;
        rb_emailStatus_on.Checked = false;
        rb_emailStatus_off.Checked = true;
        ServerSettings.update_EmailSystemStatus(false);
        lbl_emailStatus.Text = "Off";
        lbl_emailStatus.Style["color"] = "#FF0000";
        tb_smtpserver.Enabled = false;
        tb_portnumber.Enabled = false;
        tb_usernamesmtp.Enabled = false;
        tb_passwordsmtp.Enabled = false;
        btn_updatemailsettings.Enabled = false;
        lbtn_SendTestEmail.Visible = false;
        lbtn_testconnection.Visible = false;
    }


    protected void btn_usedefaultloc_Click(object sender, EventArgs e) {
        string dir = ServerSettings.GetServerMapLocation + "CloudFiles";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        tb_updateFolder.Text = "~/CloudFiles";
    }

    protected void btn_CreateSiteMap_Click(object sender, EventArgs e) {
        CreateSitemap createSiteMap = new CreateSitemap();
        createSiteMap.Create();
        GetSiteMapXml();
    }

    protected void btn_CreateRobotsFile_Click(object sender, EventArgs e) {
        CreateRobotTxt createRobotTxt = new CreateRobotTxt();
        createRobotTxt.Create();
        GetRobotsTxt();
    }

    protected void lbtn_deleteSiteMap_Click(object sender, EventArgs e) {
        if (File.Exists(ServerSettings.GetServerMapLocation + CreateSitemap.SiteMapFileName)) {
            try {
                File.Delete(ServerSettings.GetServerMapLocation + CreateSitemap.SiteMapFileName);
            }
            catch { }
        }
        GetSiteMapXml();
    }

    protected void lbtn_deleteRobotsTxt_Click(object sender, EventArgs e) {
        if (File.Exists(ServerSettings.GetServerMapLocation + CreateRobotTxt.RobotsFileName)) {
            try {
                File.Delete(ServerSettings.GetServerMapLocation + CreateRobotTxt.RobotsFileName);
            }
            catch { }
        }
        GetRobotsTxt();
    }

    protected void dd_bgmanage_change(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(dd_bgmanage.SelectedValue)) {
            lbtn_bgmanage.Enabled = true;
            lbtn_bgmanage.Visible = true;
            lbtn_bgmanage_SetasDefault.Enabled = true;
            lbtn_bgmanage_SetasDefault.Visible = true;
            img_previewbg.Enabled = true;
            img_previewbg.Visible = true;
            img_previewbg.ImageUrl = dd_bgmanage.SelectedValue;
        }
        else {
            lbtn_bgmanage.Enabled = false;
            lbtn_bgmanage.Visible = false;
            lbtn_bgmanage_SetasDefault.Enabled = false;
            lbtn_bgmanage_SetasDefault.Visible = false;
            img_previewbg.Enabled = false;
            img_previewbg.Visible = false;
        }

        if (_username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
            lbtn_bgmanage.Enabled = false;
            lbtn_bgmanage.Visible = false;
            lbtn_bgmanage_SetasDefault.Enabled = false;
            lbtn_bgmanage_SetasDefault.Visible = false;
        }

        dd_bgmanage.Focus();
        RegisterPostbackScripts.RegisterStartupScript(this, "$('img').one('load', function () { $('#maincontent_overflow').scrollTop($('.maincontent-padding').height()); });");
    }
    protected void lbtn_bgmanage_click(object sender, EventArgs e) {
        string[] directories = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
        foreach (string filename in directories) {
            var fi = new FileInfo(filename);

            string path = "~/Standard_Images/Backgrounds/" + fi.Name;
            if (path == dd_bgmanage.SelectedValue) {
                try {
                    File.Delete(fi.FullName);
                    BuildBGList();
                    lbtn_bgmanage.Enabled = false;
                    lbtn_bgmanage.Visible = false;
                    lbtn_bgmanage_SetasDefault.Enabled = false;
                    lbtn_bgmanage_SetasDefault.Visible = false;
                    img_previewbg.Enabled = false;
                    img_previewbg.Visible = false;
                }
                catch (Exception ex) {
                    AppLog.AddError(ex);
                }
            }
        }
    }

    protected void lbtn_bgmanage_SetasDefault_click(object sender, EventArgs e) {
        string[] directories = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds");
        foreach (string filename in directories) {
            var fi = new FileInfo(filename);

            string path = "~/Standard_Images/Backgrounds/" + fi.Name;
            if (path == dd_bgmanage.SelectedValue) {
                string currTheme = new MemberDatabase(_username).SiteTheme;
                if (string.IsNullOrEmpty(currTheme)) {
                    currTheme = "Standard";
                }

                string defaultImg = "default-bg.jpg";
                string defaultDir = ServerSettings.GetServerMapLocation + "App_Themes\\" + currTheme + "\\Body\\";
                string selectedFile = ServerSettings.GetServerMapLocation + dd_bgmanage.SelectedValue.Replace("~/", "").Replace("/", "\\");

                try {
                    if (File.Exists(defaultDir + defaultImg)) {
                        File.Delete(defaultDir + defaultImg);
                    }
                }
                catch { }

                try {
                    System.Drawing.Image image1 = System.Drawing.Image.FromFile(selectedFile);
                    image1.Save(defaultDir + defaultImg, System.Drawing.Imaging.ImageFormat.Jpeg);
                    RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function() { openWSE.ConfirmWindow('This image is now the default background for " + currTheme + " theme.'); }, 100);");
                }
                catch {
                    RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function() { openWSE.ConfirmWindow('Failed to set " + fi.Name + " as the default background.'); }, 100);");
                }

                break;
            }
        }
    }


    protected void rb_LockFileManager_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_LockFileManager_on.Checked = true;
            rb_LockFileManager_off.Checked = false;
            ServerSettings.update_LockFileManager(true);

        }
    }
    protected void rb_LockFileManager_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_LockFileManager_on.Checked = false;
            rb_LockFileManager_off.Checked = true;
            ServerSettings.update_LockFileManager(false);

        }
    }


    protected void rb_SaveCookiesAsSessions_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_SaveCookiesAsSessions_on.Checked = true;
            rb_SaveCookiesAsSessions_off.Checked = false;
            ServerSettings.update_SaveCookiesAsSessions(true);

        }
    }
    protected void rb_SaveCookiesAsSessions_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_SaveCookiesAsSessions_on.Checked = false;
            rb_SaveCookiesAsSessions_off.Checked = true;
            ServerSettings.update_SaveCookiesAsSessions(false);

        }
    }


    protected void rb_nologinrequired_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_nologinrequired_on.Checked = true;
            rb_nologinrequired_off.Checked = false;
            pnl_showpreviewbutton.Enabled = false;
            pnl_showpreviewbutton.Visible = false;
            ServerSettings.update_NoLoginRequired(true);

            pnl_NoLoginMainPage.Enabled = true;
            pnl_NoLoginMainPage.Visible = true;

            pnlLoginMessage.Enabled = false;
            pnlLoginMessage.Visible = false;

            pnl_showloginmodalondemomode.Enabled = true;
            pnl_showloginmodalondemomode.Visible = true;
        }
    }
    protected void rb_nologinrequired_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_nologinrequired_on.Checked = false;
            rb_nologinrequired_off.Checked = true;
            pnl_showpreviewbutton.Enabled = true;
            pnl_showpreviewbutton.Visible = true;
            ServerSettings.update_NoLoginRequired(false);
            if (!_ss.ShowPreviewButtonLogin) {
                pnl_NoLoginMainPage.Enabled = false;
                pnl_NoLoginMainPage.Visible = false;
            }
            else {
                pnl_NoLoginMainPage.Enabled = true;
                pnl_NoLoginMainPage.Visible = true;
            }

            pnlLoginMessage.Enabled = true;
            pnlLoginMessage.Visible = true;

            pnl_showloginmodalondemomode.Enabled = false;
            pnl_showloginmodalondemomode.Visible = false;
        }
    }


    protected void rb_ShowLoginModalOnDemoMode_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_ShowLoginModalOnDemoMode_on.Checked = true;
            rb_ShowLoginModalOnDemoMode_off.Checked = false;
            ServerSettings.update_ShowLoginModalOnDemoMode(true);

        }
    }
    protected void rb_ShowLoginModalOnDemoMode_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_ShowLoginModalOnDemoMode_on.Checked = false;
            rb_ShowLoginModalOnDemoMode_off.Checked = true;
            ServerSettings.update_ShowLoginModalOnDemoMode(false);

        }
    }


    protected void rb_emailonReg_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_emailonReg_on.Checked = true;
            rb_emailonReg_off.Checked = false;
            ServerSettings.update_EmailOnRegister(true);

        }
    }
    protected void rb_emailonReg_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_emailonReg_on.Checked = false;
            rb_emailonReg_off.Checked = true;
            ServerSettings.update_EmailOnRegister(false);

        }
    }


    protected void rb_ShowPreviewButtonLogin_on_CheckedChanged(object sender, EventArgs e) {
        rb_ShowPreviewButtonLogin_on.Checked = true;
        rb_ShowPreviewButtonLogin_off.Checked = false;
        ServerSettings.update_ShowPreviewButtonLogin(true);
        if ((!_ss.NoLoginRequired) && (!_ss.ShowPreviewButtonLogin)) {
            pnl_NoLoginMainPage.Enabled = false;
            pnl_NoLoginMainPage.Visible = false;
        }
        else {
            pnl_NoLoginMainPage.Enabled = true;
            pnl_NoLoginMainPage.Visible = true;
        }


    }
    protected void rb_ShowPreviewButtonLogin_off_CheckedChanged(object sender, EventArgs e) {
        rb_ShowPreviewButtonLogin_on.Checked = false;
        rb_ShowPreviewButtonLogin_off.Checked = true;
        ServerSettings.update_ShowPreviewButtonLogin(false);
        if ((!_ss.NoLoginRequired) && (!_ss.ShowPreviewButtonLogin)) {
            pnl_NoLoginMainPage.Enabled = false;
            pnl_NoLoginMainPage.Visible = false;
        }
        else {
            pnl_NoLoginMainPage.Enabled = true;
            pnl_NoLoginMainPage.Visible = true;
        }


    }


    protected void rb_hideAllAppIcons_on_CheckedChanged(object sender, EventArgs e) {
        rb_hideAllAppIcons_on.Checked = true;
        rb_hideAllAppIcons_off.Checked = false;
        ServerSettings.update_HideAllAppIcons(true);
    }
    protected void rb_hideAllAppIcons_off_CheckedChanged(object sender, EventArgs e) {
        rb_hideAllAppIcons_on.Checked = false;
        rb_hideAllAppIcons_off.Checked = true;
        ServerSettings.update_HideAllAppIcons(false);
    }


    protected void rb_AddBackgroundToLogo_on_Checked(object sender, EventArgs e) {
        rb_AddBackgroundToLogo_on.Checked = true;
        rb_AddBackgroundToLogo_off.Checked = false;
        ServerSettings.update_AddBackgroundToLogo(true);
        pnl_logobackgroundColor.Enabled = true;
        pnl_logobackgroundColor.Visible = true;

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AddBackgroundColorToLogo('" + _ss.LogoBackgroundColor + "');");
    }
    protected void rb_AddBackgroundToLogo_off_Checked(object sender, EventArgs e) {
        rb_AddBackgroundToLogo_on.Checked = false;
        rb_AddBackgroundToLogo_off.Checked = true;
        ServerSettings.update_AddBackgroundToLogo(false);
        pnl_logobackgroundColor.Enabled = false;
        pnl_logobackgroundColor.Visible = false;

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.RemoveBackgroundColorToLogo();");
    }

    protected void btn_logoBgColor_Click(object sender, EventArgs e) {
        string text = tb_logoBgColor.Text.Trim();
        ServerSettings.update_LogoBackgroundColor(text);
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AddBackgroundColorToLogo('" + _ss.LogoBackgroundColor + "');");
    }


    protected void rb_siteplugins_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_siteplugins_on.Checked = true;
            rb_siteplugins_off.Checked = false;
            ServerSettings.update_SitePluginsLocked(true);

        }
    }
    protected void rb_siteplugins_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_siteplugins_on.Checked = false;
            rb_siteplugins_off.Checked = true;
            ServerSettings.update_SitePluginsLocked(false);

        }
    }


    protected void rb_sitenotifi_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_sitenotifi_on.Checked = true;
            rb_sitenotifi_off.Checked = false;
            ServerSettings.update_NotificationsLocked(true);

        }
    }
    protected void rb_sitenotifi_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_sitenotifi_on.Checked = false;
            rb_sitenotifi_off.Checked = true;
            ServerSettings.update_NotificationsLocked(false);

        }
    }


    protected void rb_siteoverlay_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_siteoverlay_on.Checked = true;
            rb_siteoverlay_off.Checked = false;
            ServerSettings.update_OverlaysLocked(true);

        }
    }
    protected void rb_siteoverlay_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_siteoverlay_on.Checked = false;
            rb_siteoverlay_off.Checked = true;
            ServerSettings.update_OverlaysLocked(false);

        }
    }


    protected void rb_lockcustomtables_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_lockcustomtables_on.Checked = true;
            rb_lockcustomtables_off.Checked = false;
            ServerSettings.update_LockCustomTables(true);

        }
    }
    protected void rb_lockcustomtables_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_lockcustomtables_on.Checked = false;
            rb_lockcustomtables_off.Checked = true;
            ServerSettings.update_LockCustomTables(false);

        }
    }


    protected void rb_LockAppCreator_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_LockAppCreator_on.Checked = true;
            rb_LockAppCreator_off.Checked = false;
            ServerSettings.update_LockAppCreator(true);

        }
    }
    protected void rb_LockAppCreator_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_LockAppCreator_on.Checked = false;
            rb_LockAppCreator_off.Checked = true;
            ServerSettings.update_LockAppCreator(false);

        }
    }


    protected void rb_LockBackgroundServices_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_LockBackgroundServices_on.Checked = true;
            rb_LockBackgroundServices_off.Checked = false;
            ServerSettings.update_LockBackgroundServices(true);

        }
    }
    protected void rb_LockBackgroundServices_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_LockBackgroundServices_on.Checked = false;
            rb_LockBackgroundServices_off.Checked = true;
            ServerSettings.update_LockBackgroundServices(false);

        }
    }


    protected void rb_Lockstartupscripts_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_Lockstartupscripts_on.Checked = true;
            rb_Lockstartupscripts_off.Checked = false;
            ServerSettings.update_LockStartupScripts(true);

        }
    }

    protected void rb_Lockstartupscripts_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_Lockstartupscripts_on.Checked = false;
            rb_Lockstartupscripts_off.Checked = true;
            ServerSettings.update_LockStartupScripts(false);

        }
    }


    protected void rb_Lockiplisteneron_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_Lockiplisteneron.Checked = true;
            rb_Lockiplisteneroff.Checked = false;
            ServerSettings.update_LockIPListenerWatchlist(true);

        }
    }

    protected void rb_Lockiplisteneroff_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_Lockiplisteneron.Checked = false;
            rb_Lockiplisteneroff.Checked = true;
            ServerSettings.update_LockIPListenerWatchlist(false);

        }
    }


    protected void rb_sslredirect_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_sslredirect_on.Checked = true;
            rb_sslredirect_off.Checked = false;
            ServerSettings.update_SSLRedirect(true);
            pnl_sslValidation.Enabled = true;
            pnl_sslValidation.Visible = true;

        }
    }

    protected void rb_sslredirect_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_sslredirect_on.Checked = false;
            rb_sslredirect_off.Checked = true;
            ServerSettings.update_SSLRedirect(false);
            pnl_sslValidation.Enabled = false;
            pnl_sslValidation.Visible = false;

        }
    }


    protected void rb_urlvalidation_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_urlvalidation_on.Checked = true;
            rb_urlvalidation_off.Checked = false;
            ServerSettings.update_URLValidation(true);

        }
    }

    protected void rb_urlvalidation_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_urlvalidation_on.Checked = false;
            rb_urlvalidation_off.Checked = true;
            ServerSettings.update_URLValidation(false);

        }
    }


    protected void rb_AssociateWithGroups_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_AssociateWithGroups_on.Checked = true;
            rb_AssociateWithGroups_off.Checked = false;
            ServerSettings.update_AssociateWithGroups(true);

        }
    }
    protected void rb_AssociateWithGroups_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_AssociateWithGroups_on.Checked = false;
            rb_AssociateWithGroups_off.Checked = true;
            ServerSettings.update_AssociateWithGroups(false);

        }
    }


    protected void rb_ForceGroupLogin_on_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_ForceGroupLogin_on.Checked = true;
            rb_ForceGroupLogin_off.Checked = false;
            ServerSettings.update_ForceGroupLogin(true);

            pnl_showpreviewbutton.Enabled = false;
            pnl_showpreviewbutton.Visible = false;
            pnl_nologinrequired.Enabled = false;
            pnl_nologinrequired.Visible = false;
            pnl_NoLoginMainPage.Enabled = false;
            pnl_NoLoginMainPage.Visible = false;
        }
    }
    protected void rb_ForceGroupLogin_off_CheckedChanged(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            rb_ForceGroupLogin_on.Checked = false;
            rb_ForceGroupLogin_off.Checked = true;
            ServerSettings.update_ForceGroupLogin(false);

            pnl_nologinrequired.Enabled = true;
            pnl_nologinrequired.Visible = true;

            if ((!_ss.NoLoginRequired) && (!_ss.ShowPreviewButtonLogin)) {
                pnl_NoLoginMainPage.Enabled = false;
                pnl_NoLoginMainPage.Visible = false;
                pnl_showpreviewbutton.Enabled = true;
                pnl_showpreviewbutton.Visible = true;
            }
            else if ((_ss.NoLoginRequired) && (_ss.ShowPreviewButtonLogin)) {
                pnl_NoLoginMainPage.Enabled = true;
                pnl_NoLoginMainPage.Visible = true;
                pnl_showpreviewbutton.Enabled = false;
                pnl_showpreviewbutton.Visible = false;
            }
            else {
                pnl_NoLoginMainPage.Enabled = true;
                pnl_NoLoginMainPage.Visible = true;
            }
        }
    }


    protected void rb_cachehp_on_CheckedChanged(object sender, EventArgs e) {
        rb_cachehp_off.Checked = false;
        rb_cachehp_on.Checked = true;
        ServerSettings.update_CacheHomePage(true);
    }

    protected void rb_cachehp_off_CheckedChanged(object sender, EventArgs e) {
        rb_cachehp_off.Checked = true;
        rb_cachehp_on.Checked = false;
        ServerSettings.update_CacheHomePage(false);

        HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
        HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        HttpContext.Current.Response.Cache.SetNoStore();
        Response.Cache.SetExpires(ServerSettings.ServerDateTime.AddSeconds(60));
        Response.Cache.SetValidUntilExpires(true);
    }


    protected void btn_updateFolder_Click(object sender, EventArgs e) {
        if (tb_updateFolder.Text != _ss.DocumentsFolder) {
            ServerSettings.update_docFolder(tb_updateFolder.Text);

        }
    }

    protected void btn_uploadlogo_Click(object sender, EventArgs e) {
        if (FileUpload2.HasFile) {
            var fi = new FileInfo(FileUpload2.PostedFile.FileName);
            if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                string filename = ServerSettings.GetServerMapLocation + "Standard_Images\\logo.png";

                Image imgurl = Image.FromStream(FileUpload2.PostedFile.InputStream);

                if ((imgurl.Size.Height > 249) || (imgurl.Size.Width > 600)) {
                    float h = imgurl.Size.Height;
                    float w = imgurl.Size.Width;
                    double height = Math.Round((h / w) * 600, 0);
                    Image newImage = new Bitmap(600, Convert.ToInt32(height));
                    using (Graphics gr = Graphics.FromImage(newImage)) {
                        gr.SmoothingMode = SmoothingMode.HighQuality;

                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        gr.DrawImage(imgurl, new Rectangle(0, 0, 600, Convert.ToInt32(height)));
                        newImage.Save(filename, ImageFormat.Png);
                    }
                }
                else {
                    FileUpload2.SaveAs(filename);
                }

                ServerSettings.PageToolViewRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
            }
        }
    }

    protected void btn_uploadlogo_fav_Click(object sender, EventArgs e) {
        if (FileUpload4.HasFile) {
            var fi = new FileInfo(FileUpload4.PostedFile.FileName);
            if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")
                || (fi.Extension.ToLower() == ".ico")) {
                string filename = ServerSettings.GetServerMapLocation + "Standard_Images\\favicon.ico";

                Image imgurl = Image.FromStream(FileUpload4.PostedFile.InputStream);

                Image newImage = new Bitmap(48, 48);
                using (Graphics gr = Graphics.FromImage(newImage)) {
                    gr.SmoothingMode = SmoothingMode.HighQuality;

                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(imgurl, new Rectangle(0, 0, 48, 48));
                    newImage.Save(filename, ImageFormat.Icon);
                }

                ServerSettings.PageToolViewRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
            }
        }
    }

    protected void btn_uploadbgImage_Click(object sender, EventArgs e) {
        if (FileUpload5.HasFile) {
            var fi = new FileInfo(FileUpload5.PostedFile.FileName);
            if ((fi.Extension.ToLower() == ".png") || (fi.Extension.ToLower() == ".jpg")
                || (fi.Extension.ToLower() == ".jpeg") || (fi.Extension.ToLower() == ".gif")) {
                string filename = ServerSettings.GetServerMapLocation + "Standard_Images\\Backgrounds\\" + HelperMethods.RandomString(10) + fi.Extension;

                FileUpload5.SaveAs(filename);
                ServerSettings.PageToolViewRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
            }
        }
    }

    protected void btn_updateserversettingscache_Click(object sender, EventArgs e) {
        ServerSettings.ResetServerSettings();
        ServerSettings.PageToolViewRedirect(Page, "SiteSettings.aspx?tab=admincontrolsonly");
    }

    protected void btn_clearnotiall_Click(object sender, EventArgs e) {
        var un = new UserNotificationMessages(_username);
        un.deleteNotification_All();
    }

    protected void btn_clearflagall_Click(object sender, EventArgs e) {
        var uuf = new UserUpdateFlags();
        uuf.deleteAllFlags();
    }

    protected void btn_updateLogoOpacity_Click(object sender, EventArgs e) {
        string opacity = hf_opacity.Value;
        double tempOut = 0.0d;
        if (double.TryParse(opacity, out tempOut)) {
            if (tempOut <= 0.05d)
                opacity = "0.0";
            else if (tempOut > 1.0d)
                opacity = "1.0";
            ServerSettings.Update_LogoOpacity(opacity);
        }

        GetLogoOpacity();
        HelperMethods.SetLogoOpacity(Page, (System.Web.UI.WebControls.Image)Master.FindControl("img_LoginGroup"));
    }
    

    protected void rb_chatclient_on_CheckedChanged(object sender, EventArgs e) {
        rb_chatclient_off.Checked = false;
        rb_chatclient_on.Checked = true;
        ServerSettings.update_ChatEnabled(true);

    }
    protected void rb_chatclient_off_CheckedChanged(object sender, EventArgs e) {
        rb_chatclient_off.Checked = true;
        rb_chatclient_on.Checked = false;
        ServerSettings.update_ChatEnabled(false);

    }


    protected void rb_siteonline_CheckedChanged(object sender, EventArgs e) {
        rb_siteoffline.Checked = false;
        rb_siteonline.Checked = true;
        ServerSettings.update_SiteOffLine(false);

        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            string message = "$('#workspace-selector').html(\"<div id='sitestatus_app_bar'><span id='siteoffline_b' class='font-bold' style='color: #00E000;'>ONLINE</span></div>\");";
            RegisterPostbackScripts.RegisterStartupScript(this, message);
        }

    }
    protected void rb_siteoffline_CheckedChanged(object sender, EventArgs e) {
        rb_siteoffline.Checked = true;
        rb_siteonline.Checked = false;
        ServerSettings.update_SiteOffLine(true);

        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            string message = "$('#workspace-selector').html(\"<div id='sitestatus_app_bar'><span id='siteoffline_b' class='font-bold' style='color: #FF0000;'>OFFLINE</span></div>\");";
            RegisterPostbackScripts.RegisterStartupScript(this, message);
        }

    }

    protected void rb_siteCustomizations_On_CheckedChanged(object sender, EventArgs e) {
        rb_siteCustomizations_On.Checked = true;
        rb_siteCustomizations_Off.Checked = false;
        ServerSettings.update_CustomizationsLocked(true);

        pnl_Customizations.Enabled = false;
        pnl_Customizations.Visible = false;
        btn_customizeSMTP.Visible = false;

    }
    protected void rb_siteCustomizations_Off_CheckedChanged(object sender, EventArgs e) {
        rb_siteCustomizations_Off.Checked = true;
        rb_siteCustomizations_On.Checked = false;
        ServerSettings.update_CustomizationsLocked(false);

        pnl_Customizations.Enabled = true;
        pnl_Customizations.Visible = true;
        btn_customizeSMTP.Visible = true;

    }

    protected void rb_allowapprating_on_CheckedChanged(object sender, EventArgs e) {
        rb_allowapprating_on.Checked = true;
        rb_allowapprating_off.Checked = false;
        ServerSettings.update_AllowAppRating(true);

    }
    protected void rb_allowapprating_off_CheckedChanged(object sender, EventArgs e) {
        rb_allowapprating_on.Checked = false;
        rb_allowapprating_off.Checked = true;
        ServerSettings.update_AllowAppRating(false);

    }

    protected void rb_allowUserPrivacy_on_CheckedChanged(object sender, EventArgs e) {
        rb_allowUserPrivacy_on.Checked = true;
        rb_allowUserPrivacy_off.Checked = false;
        ServerSettings.update_AllowPrivacy(true);

    }
    protected void rb_allowUserPrivacy_off_CheckedChanged(object sender, EventArgs e) {
        rb_allowUserPrivacy_on.Checked = false;
        rb_allowUserPrivacy_off.Checked = true;
        ServerSettings.update_AllowPrivacy(false);

    }

    protected void btn_descriptionMetaTag_Click(object sender, EventArgs e) {
        string description = tb_descriptionMetaTag.Text.Trim();
        if (description.Length > 3999) {
            description = description.Substring(0, 3999);
        }

        ServerSettings.update_MetaTagDescription(description);
        LoadMetaTags();
    }
    protected void lbtn_clearDescriptionMeta_Click(object sender, EventArgs e) {
        ServerSettings.update_MetaTagDescription(string.Empty);
        LoadMetaTags();
    }
    protected void hf_keywordsMetaTag_Changed(object sender, EventArgs e) {
        string keywords = HttpUtility.UrlDecode(hf_keywordsMetaTag.Value);
        if (keywords.Length > 0) {
            if (keywords == "REMOVEKEYWORDS") {
                keywords = string.Empty;
            }
            else if (keywords[keywords.Length - 1] == ',') {
                keywords = keywords.Remove(keywords.Length - 1);
            }

            if (keywords.Length > 3999) {
                keywords = keywords.Substring(0, 3999);
            }

            ServerSettings.update_MetaTagKeywords(keywords);
        }
        LoadMetaTags();
        hf_keywordsMetaTag.Value = string.Empty;
    }
    protected void lbtn_clearAllKeywordsMeta_Click(object sender, EventArgs e) {
        ServerSettings.update_MetaTagKeywords(string.Empty);
        LoadMetaTags();
    }

    protected void btn_clearCache_Click(object sender, EventArgs e) {
        foreach (DictionaryEntry c in Cache) {
            try {
                Cache.Remove(c.Key.ToString());
            }
            catch (Exception ex) {
                AppLog.AddError(ex);
            }
        }
        Session.Clear();
        lbl_lastcacheclear.Text = ServerSettings.update_LastCacheClearDate();

    }

    protected void btn_clearapps_Click(object sender, EventArgs e) {
        var w = new App(_username);
        w.DeleteUserProperties();
    }

    protected void btn_updateadminnote_Click(object sender, EventArgs e) {
        string note = tb_adminnote.Text.Trim();
        if (_ss.AdminNote != tb_adminnote.Text) {
            ServerSettings.update_AdminNote(note);

            string by = HelperMethods.MergeFMLNames(new MemberDatabase(_username));

            ServerSettings.update_AdminNoteBy(by);


            lbl_adminnoteby.Text = "<b class='pad-right-sml'>Updated by:</b>" + by;

        }
    }

    protected void btn_loginPageMessage_Click(object sender, EventArgs e) {
        string message = tb_loginPageMessage.Text.Trim();
        if (!string.IsNullOrEmpty(message)) {
            string date = ServerSettings.ServerDateTime.ToString();
            ServerSettings.UpdateLoginMessage(message, date);

            lbl_loginMessageDate.Text = "<b class='pad-right'>Date Updated:</b>" + date;
        }
    }

    protected void lbtn_loginPageMessage_clear_Click(object sender, EventArgs e) {
        string date = ServerSettings.ServerDateTime.ToString();
        ServerSettings.UpdateLoginMessage("", date);

        tb_loginPageMessage.Text = "";
        lbl_loginMessageDate.Text = "<b class='pad-right'>Date Updated:</b>" + date;
    }

    protected void btn_updateadminnote_clear_Click(object sender, EventArgs e) {
        if (tb_adminnote.Text == "") return;
        ServerSettings.update_AdminNote(string.Empty);
        ServerSettings.update_AdminNoteBy(string.Empty);

        lbl_adminnoteby.Text = string.Empty;
        tb_adminnote.Text = string.Empty;
    }

    protected void btn_clearuserchats_Click(object sender, EventArgs e) {
        var chat = new Chat(false);
        chat.DeleteChatLog();
    }

    protected void rb_ssl_enabled_Checked(object sender, EventArgs e) {
        rb_ssl_disabled.Checked = false;
        rb_ssl_enabled.Checked = true;
    }

    protected void rb_ssl_disabled_Checked(object sender, EventArgs e) {
        rb_ssl_disabled.Checked = true;
        rb_ssl_enabled.Checked = false;
    }

    protected void lbtn_testconnection_Click(object sender, EventArgs e) {
        if (lbl_mailsettings_error.Visible)
            lbl_mailsettings_error.Visible = false;

        // Test credentials as well -- NEED TO COMPLETE
        using (var client = new TcpClient()) {
            int portnumber = 587;

            int.TryParse(tb_portnumber.Text.Trim(), out portnumber);
            var server = tb_smtpserver.Text.Trim();
            var port = portnumber;
            try {
                client.Connect(server, port);
                if (client.Connected) {
                    lbl_testconnection.ForeColor = Color.Green;
                    lbl_testconnection.Text = "Successful Connection";
                }
                else {
                    lbl_testconnection.ForeColor = Color.Red;
                    lbl_testconnection.Text = "Connection Failed";
                }
            }
            catch {
                lbl_testconnection.ForeColor = Color.Red;
                lbl_testconnection.Text = "Connection Failed";
            }
        }
        lbl_testconnection.Visible = true;
    }

    protected void lbtn_SendTestEmail_Click(object sender, EventArgs e) {
        var message = new MailMessage();
        MembershipUser mUser = Membership.GetUser(_username);
        if (!string.IsNullOrEmpty(mUser.Email)) {
            message.To.Add(mUser.Email);
            ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Message Title Holder</h1>", OpenWSE.Core.Licensing.CheckLicense.SiteName + ": Test Email", "Email body text holder");
        }
    }

    protected void btn_updatemailsettings_Click(object sender, EventArgs e) {
        if (lbl_testconnection.Visible) {
            lbl_testconnection.Visible = false;
            lbl_testconnection.Text = string.Empty;
        }
        if ((string.IsNullOrEmpty(tb_smtpserver.Text.Trim())) || (string.IsNullOrEmpty(tb_portnumber.Text.Trim())) ||
            (string.IsNullOrEmpty(tb_usernamesmtp.Text.Trim()))
            || (string.IsNullOrEmpty(tb_passwordsmtp.Text.Trim())) || (tb_passwordsmtp.Text.Trim() == "**********")) {
            lbl_mailsettings_error.ForeColor = Color.Red;
            lbl_mailsettings_error.Text = "All fields must be filled out.";
            lbl_mailsettings_error.Visible = true;
        }
        else {
            ServerSettings.UpdateMailSettings(_username, tb_smtpserver.Text.Trim(), tb_portnumber.Text.Trim(), tb_usernamesmtp.Text.Trim(), tb_passwordsmtp.Text.Trim(), rb_ssl_enabled.Checked);
            lbl_mailsettings_error.Visible = false;

            ServerSettings.GetMailSettingList();
            if (ServerSettings.MailSettings_Coll.Count > 0) {
                int index = ServerSettings.MailSettings_Coll.Count - 1;
                tb_smtpserver.Text = ServerSettings.MailSettings_Coll[index].SMTP_Address;
                tb_portnumber.Text = ServerSettings.MailSettings_Coll[index].PortNumber;
                tb_usernamesmtp.Text = ServerSettings.MailSettings_Coll[index].EmailAddress;
                tb_passwordsmtp.Text = "**********";
                var member = new MemberDatabase(ServerSettings.MailSettings_Coll[index].User);
                lbl_updatedbymailsettings.Text = HelperMethods.MergeFMLNames(member);
                lbl_dateupdatedmailsettings.Text = ServerSettings.MailSettings_Coll[index].Date;
                lbl_mailsettings_error.ForeColor = Color.Green;
                lbl_mailsettings_error.Text = "Mail settings have been updated.";
                lbl_mailsettings_error.Visible = true;
            }
            else {
                lbl_mailsettings_error.ForeColor = Color.Red;
                lbl_mailsettings_error.Text = "Error trying to update mail settings. Please try again.";
                lbl_mailsettings_error.Visible = true;
            }
        }

    }

    protected void btn_ClearGroupSessions_Click(object sender, EventArgs e) {
        if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            GroupSessions.ClearAllGroupLoginSessions();
        }
    }

    protected void btn_timezoneset_Click(object sender, EventArgs e) {
        ServerSettings.update_ServerTimezone(dd_timezoneset.SelectedValue);
        SetServerTimezone(string.Empty);
    }

    protected void btn_defaultbodyfontfamily_Click(object sender, EventArgs e) {
        ServerSettings.update_DefaultBodyFontFamily(dd_defaultbodyfontfamily.SelectedValue);
        ServerSettings.PageToolViewRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
    }

    protected void btn_defaultfontsize_Click(object sender, EventArgs e) {
        string fontSize = tb_defaultfontsize.Text.Trim();
        if (!string.IsNullOrEmpty(fontSize)) {
            int tempSize = 0;
            if (int.TryParse(fontSize, out tempSize) && tempSize > 0) {
                ServerSettings.update_DefaultBodyFontSize(fontSize);
            }
        }
        else {
            ServerSettings.update_DefaultBodyFontSize(string.Empty);
        }

        ServerSettings.PageToolViewRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
    }

    protected void lbtn_defaultfontsize_clear_Click(object sender, EventArgs e) {
        ServerSettings.update_DefaultBodyFontSize(string.Empty);
        ServerSettings.PageToolViewRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
    }

    protected void btn_defaultfontcolor_Click(object sender, EventArgs e) {
        string fontColor = tb_defaultfontcolor.Text.Trim();
        if (!string.IsNullOrEmpty(fontColor)) {
            if (!fontColor.StartsWith("#")) {
                fontColor = "#" + fontColor;
            }

            ServerSettings.update_DefaultBodyFontColor(fontColor);
        }
        else {
            ServerSettings.update_DefaultBodyFontColor(string.Empty);
        }

        ServerSettings.PageToolViewRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
    }

    protected void lbtn_defaultfontcolor_clear_Click(object sender, EventArgs e) {
        ServerSettings.update_DefaultBodyFontColor(string.Empty);
        ServerSettings.PageToolViewRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
    }

    #endregion


    #region Twitter Update Buttons

    protected void btn_updateTwitterAccessToken_Click(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            ServerSettings.update_TwitterAccessToken(tb_updateTwitterAccessToken.Text.Trim());

        }
    }
    protected void btn_updateTwitterAccessTokenSecret_Click(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            if (tb_updateTwitterAccessTokenSecret.Text.Trim().StartsWith("**********")) {
                return;
            }
            else {
                ServerSettings.update_TwitterAccessTokenSecret(tb_updateTwitterAccessTokenSecret.Text.Trim());
                if (tb_updateTwitterAccessTokenSecret.Text.Trim() == "") {
                    return;
                }
                else {
                    tb_updateTwitterAccessTokenSecret.Text = "***************************************";
                }
            }
        }
    }
    protected void btn_updateTwitterConsumerKey_Click(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            ServerSettings.update_TwitterConsumerKey(tb_updateTwitterConsumerKey.Text.Trim());

        }
    }
    protected void btn_updateTwitterConsumerSecret_Click(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            if (tb_updateTwitterConsumerSecret.Text.Trim().StartsWith("**********")) {
                return;
            }
            else {
                ServerSettings.update_TwitterConsumerSecret(tb_updateTwitterConsumerSecret.Text.Trim());
                if (tb_updateTwitterConsumerSecret.Text.Trim() == "") {
                    return;
                }
                else {
                    tb_updateTwitterConsumerSecret.Text = "***************************************";
                }
            }
        }
    }

    #endregion


    #region Google Update Buttons

    protected void btn_updateGoogleClientId_Click(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            ServerSettings.update_GoogleClientId(txt_GoogleClientId.Text.Trim());

        }
    }
    protected void btn_updateGoogleClientSecret_Click(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            if (txt_GoogleClientSecret.Text.Trim().StartsWith("**********")) {
                return;
            }
            else {
                ServerSettings.update_GoogleClientSecret(txt_GoogleClientSecret.Text.Trim());
                if (txt_GoogleClientSecret.Text.Trim() == "") {
                    return;
                }
                else {
                    txt_GoogleClientSecret.Text = "***************************************";
                }
            }
        }
    }

    #endregion


    #region Facebook Update Buttons

    protected void btn_updateFacebookAppId_Click(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            ServerSettings.update_FacebookAppId(txt_facebookAppId.Text.Trim());

        }
    }
    protected void btn_updateFacebookAppSecret_Click(object sender, EventArgs e) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            if (txt_facebookAppSecret.Text.Trim().StartsWith("**********")) {
                return;
            }
            else {
                ServerSettings.update_FacebookAppSecret(txt_facebookAppSecret.Text.Trim());
                if (txt_facebookAppSecret.Text.Trim() == "") {
                    return;
                }
                else {
                    txt_facebookAppSecret.Text = "***************************************";
                }
            }

        }
    }

    #endregion

}