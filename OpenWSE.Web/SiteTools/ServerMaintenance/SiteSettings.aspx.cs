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
using System.Web.Configuration;
using System.Configuration;
using System.Collections.Specialized;
using System.Reflection;

#endregion

public partial class SiteTools_SiteSettings : BasePage {

    protected void Page_Load(object sender, EventArgs e) {
        BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);

        MembershipUser mUser = Membership.GetUser(CurrentUsername);
        if (string.IsNullOrEmpty(mUser.Email)) {
            lbtn_SendTestEmail.Enabled = false;
            lbtn_SendTestEmail.Visible = false;
        }
        else
            lbtn_SendTestEmail.Text = "Send test email to " + mUser.Email;

        lbl_lastcacheclear.Text = MainServerSettings.LastCacheClearDate.ToString();
        BuildSettingsList();
        var scriptManager = ScriptManager.GetCurrent(Page);
        if (scriptManager != null) {
            scriptManager.RegisterPostBackControl(btn_uploadlogo);
            scriptManager.RegisterPostBackControl(btn_uploadlogo_fav);
            scriptManager.RegisterPostBackControl(btn_uploadbgImage);
        }
        if (!IsUserInAdminRole()) {
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

            pnl_adminShowUserFlags.Enabled = false;
            pnl_adminShowUserFlags.Visible = false;

            var strStandard = new StringBuilder();
            strStandard.Append("$('#MainContent_btn_updateadminnote_clear').remove();");
            strStandard.Append("$('#ClearAppProp_Controls').remove();");
            strStandard.Append("$('#ClearUserNoti_Controls').remove();");
            strStandard.Append("$('#ChatClient_Controls').remove();");
            RegisterPostbackScripts.RegisterStartupScript(this, strStandard.ToString());
        }

        if (!string.IsNullOrEmpty(MainServerSettings.AdminNote)) {
            lbl_adminnoteby.Text = "<b class='pad-right-sml'>Updated by:</b>" + MainServerSettings.AdminNoteBy;
        }

        if (!IsUserNameEqualToAdmin()) {
            txt_SiteName.Enabled = false;
            btn_UpdateSiteName.Enabled = false;
            btn_UpdateSiteName.Visible = false;
        }
        else {
            txt_SiteName.Enabled = true;
            btn_UpdateSiteName.Enabled = true;
            btn_UpdateSiteName.Visible = true;
        }

        if (!AsyncPostBackSourceElementID.Contains("btn_UpdateSiteName")) {
            txt_SiteName.Text = ServerSettings.SiteName;
        }

        SetServerTimezone(AsyncPostBackSourceElementID);
        LoadSystemInformation();

        if (!IsPostBack) {
            tb_totalWorkspacesAllowed.Text = MainServerSettings.TotalWorkspacesAllowed.ToString();
        }

        if (IsUserNameEqualToAdmin()) {
            if (!IsPostBack) {
                pnl_admincontrolsonly.Enabled = true;
                pnl_admincontrolsonly.Visible = true;
                lbl_dateUpdated_sup.Text = MainServerSettings.ShowUpdatesPopupDate;
                if (MainServerSettings.SiteOffLine) {
                    rb_siteoffline.Checked = true;
                    rb_siteonline.Checked = false;
                }
                else {
                    rb_siteoffline.Checked = false;
                    rb_siteonline.Checked = true;
                }

                if (MainServerSettings.SSLRedirect) {
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
                tb_mapsAPIKey.Text = MainServerSettings.GoogleMapsApiKey;

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

                if (MainServerSettings.SaveCookiesAsSessions) {
                    rb_SaveCookiesAsSessions_on.Checked = true;
                    rb_SaveCookiesAsSessions_off.Checked = false;
                }
                else {
                    rb_SaveCookiesAsSessions_on.Checked = false;
                    rb_SaveCookiesAsSessions_off.Checked = true;
                }

                if (MainServerSettings.AssociateWithGroups) {
                    rb_AssociateWithGroups_on.Checked = true;
                    rb_AssociateWithGroups_off.Checked = false;
                }
                else {
                    rb_AssociateWithGroups_on.Checked = false;
                    rb_AssociateWithGroups_off.Checked = true;
                }

                if (MainServerSettings.URLValidation) {
                    rb_urlvalidation_on.Checked = true;
                    rb_urlvalidation_off.Checked = false;
                }
                else {
                    rb_urlvalidation_on.Checked = false;
                    rb_urlvalidation_off.Checked = true;
                }

                if (MainServerSettings.LockFileManager) {
                    rb_LockFileManager_on.Checked = true;
                    rb_LockFileManager_off.Checked = false;
                }
                else {
                    rb_LockFileManager_on.Checked = false;
                    rb_LockFileManager_off.Checked = true;
                }

                if (MainServerSettings.LockStartupScripts) {
                    rb_Lockstartupscripts_on.Checked = true;
                    rb_Lockstartupscripts_off.Checked = false;
                }
                else {
                    rb_Lockstartupscripts_on.Checked = false;
                    rb_Lockstartupscripts_off.Checked = true;
                }

                if (MainServerSettings.LockIPListenerWatchlist) {
                    rb_Lockiplisteneron.Checked = true;
                    rb_Lockiplisteneroff.Checked = false;
                }
                else {
                    rb_Lockiplisteneron.Checked = false;
                    rb_Lockiplisteneroff.Checked = true;
                }

                if (MainServerSettings.CustomizationsLocked) {
                    rb_siteCustomizations_On.Checked = true;
                    rb_siteCustomizations_Off.Checked = false;
                }
                else {
                    rb_siteCustomizations_On.Checked = false;
                    rb_siteCustomizations_Off.Checked = true;
                }

                if (MainServerSettings.AllowAppRating) {
                    rb_allowapprating_on.Checked = true;
                    rb_allowapprating_off.Checked = false;
                }
                else {
                    rb_allowapprating_on.Checked = false;
                    rb_allowapprating_off.Checked = true;
                }

                if (MainServerSettings.AllowPrivacy) {
                    rb_allowUserPrivacy_on.Checked = true;
                    rb_allowUserPrivacy_off.Checked = false;
                }
                else {
                    rb_allowUserPrivacy_on.Checked = false;
                    rb_allowUserPrivacy_off.Checked = true;
                }

                if (MainServerSettings.LockCustomTables) {
                    rb_lockcustomtables_on.Checked = true;
                    rb_lockcustomtables_off.Checked = false;
                }
                else {
                    rb_lockcustomtables_on.Checked = false;
                    rb_lockcustomtables_off.Checked = true;
                }

                if (MainServerSettings.LockAppCreator) {
                    rb_LockAppCreator_on.Checked = true;
                    rb_LockAppCreator_off.Checked = false;
                }
                else {
                    rb_LockAppCreator_on.Checked = false;
                    rb_LockAppCreator_off.Checked = true;
                }

                if (MainServerSettings.LockBackgroundServices) {
                    rb_LockBackgroundServices_on.Checked = true;
                    rb_LockBackgroundServices_off.Checked = false;
                }
                else {
                    rb_LockBackgroundServices_on.Checked = false;
                    rb_LockBackgroundServices_off.Checked = true;
                }

                if (MainServerSettings.SitePluginsLocked) {
                    rb_siteplugins_on.Checked = true;
                    rb_siteplugins_off.Checked = false;
                }
                else {
                    rb_siteplugins_on.Checked = false;
                    rb_siteplugins_off.Checked = true;
                }

                if (MainServerSettings.OverlaysLocked) {
                    rb_siteoverlay_on.Checked = true;
                    rb_siteoverlay_off.Checked = false;
                }
                else {
                    rb_siteoverlay_on.Checked = false;
                    rb_siteoverlay_off.Checked = true;
                }

                if (MainServerSettings.EmailOnRegister) {
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

            if (MainServerSettings.CustomizationsLocked) {
                pnl_Customizations.Enabled = false;
                pnl_Customizations.Visible = false;
                pnl_mailCustomize.Visible = false;
                pnl_mailCustomize_tab.Visible = false;
            }
        }
        else {
            CustomizationOptionsEnabledDisabled(false);
            LoadEmailSettings();
            if (IsUserInAdminRole()) {
                LoadCustomizationControls();
            }
        }
    }

    private void LoadSystemInformation() {
        About about = new About(ServerSettings.GetServerMapLocation + "ChangeLog.xml");
        about.ParseXml();

        lbl_systemInfo_Version.Text = about.CurrentVersion;
        lbl_systemInfo_OperatingSystem.Text = Environment.OSVersion.ToString();
        lbl_systemInfo_ASPInfo.Text = "v" + Environment.Version.ToString();
        Configuration cfg = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
        if (cfg != null) {
            try {
                SystemWebSectionGroup swsg = (SystemWebSectionGroup)cfg.SectionGroups["system.web"];
                lbl_systemInfo_TrustLevel.Text = swsg.Trust.Level;
            }
            catch {
                div_TrustLevel.Visible = false;
            }
        }

        lbl_systemInfo_HTTPHOST.Text = Request.Headers["Host"];

        if (IsUserNameEqualToAdmin()) {
            StringBuilder strAppSettings = new StringBuilder();
            strAppSettings.Append("<ul>");
            foreach (string appSettingName in WebConfigurationManager.AppSettings.AllKeys) {
                strAppSettings.AppendFormat("<li><b class='header-li-padding'>{0}:</b>{1}</li>", appSettingName, WebConfigurationManager.AppSettings[appSettingName]);
            }
            strAppSettings.Append("</ul>");
            lbl_systemInfo_AppSettings.Text = strAppSettings.ToString();
        }
        else {
            div_AppSettings.Visible = false;
        }

        StringBuilder strHeaders = new StringBuilder();
        strHeaders.Append("<ul>");
        foreach (string headerName in Request.Headers.AllKeys) {
            strHeaders.AppendFormat("<li><b class='header-li-padding'>{0}:</b>{1}</li>", headerName, Request.Headers[headerName]);
        }
        strHeaders.Append("</ul>");
        lbl_systemInfo_Headers.Text = strHeaders.ToString();

        StringBuilder strAssemblies = new StringBuilder();
        strAssemblies.Append("<ul>");
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies) {
            string debugString = string.Empty;
            string location = string.Empty;
            string buildDate = string.Empty;

            try {
                debugString = DebugAssemblyString(assembly);
            }
            catch (Exception) { }
            try {
                location = assembly.IsDynamic ? string.Empty : "<i>" + assembly.Location + "</i>";
            }
            catch (Exception) { }
            try {
                buildDate = assembly.IsDynamic ? string.Empty : ((DateTime)TimeZoneInfo.ConvertTimeFromUtc(System.IO.File.GetLastWriteTimeUtc(assembly.Location), TimeZoneInfo.Local)).ToString() + "<div class='assembly-info-space'></div>";
            }
            catch (Exception) { }
            strAssemblies.AppendFormat("<li><b class='pad-right-sml'>{0}</b>{1}<div class='assembly-info-space'></div><div class='assembly-info-style'>{2}{3}</div></li>", assembly.FullName, debugString, location, buildDate);
        }
        strAssemblies.Append("</ul>");
        lbl_systemInfo_Assemblies.Text = strAssemblies.ToString();
    }
    private string DebugAssemblyString(Assembly assembly) {
        var attribs = assembly.GetCustomAttributes(typeof(System.Diagnostics.DebuggableAttribute), false);

        if (attribs.Length > 0) {
            var attr = attribs[0] as System.Diagnostics.DebuggableAttribute;
            if (attr != null) {
                if (attr.IsJITOptimizerDisabled) {
                    return "<span class='label-success'>Release</span>";
                }
            }
        }

        return "<span class='label-warning'>Debug</span>";
    }

    private void LoadEmailSettings() {
        if (MainServerSettings.EmailSystemStatus) {
            pnl_emailStatus_holder.Visible = true;
            if (!MainServerSettings.CustomizationsLocked) {
                pnl_mailCustomize_tab.Visible = true;
                pnl_mailCustomize.Visible = true;
            }
            else {
                pnl_mailCustomize_tab.Visible = false;
                pnl_mailCustomize.Visible = false;
            }
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
            pnl_emailStatus_holder.Visible = false;
            pnl_mailCustomize_tab.Visible = false;
            pnl_mailCustomize.Visible = false;
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

        LoadHeaderFooterEmailSettings();
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
        pnl_ImageCustomizations.Visible = x;
        pnl_ImageCustomizations_tab.Visible = x;
        pnl_MainSiteLogoDesc.Enabled = x;
        pnl_MainSiteLogoDesc.Visible = x;
        pnl_MainSiteLogoUpload.Enabled = x;
        pnl_MainSiteLogoUpload.Visible = x;
    }

    private void LoadCustomizationControls() {
        pnl_Customizations.Enabled = true;
        pnl_Customizations.Visible = true;
        if (MainServerSettings.CustomizationsLocked) {
            pnl_Customizations.Enabled = false;
            pnl_Customizations.Visible = false;
            pnl_mailCustomize.Visible = false;
            pnl_mailCustomize_tab.Visible = false;
        }
        else {
            LoadCustomizations();
        }
    }

    private void LoadCustomizations() {
        if (!AsyncPostBackSourceElementID.Contains("hf_keywordsMetaTag") && !AsyncPostBackSourceElementID.Contains("btn_descriptionMetaTag")) {
            if (IsUserInAdminRole()) {
                pnl_meteTagCustomizations.Visible = true;
                pnl_meteTagCustomizations_tab.Visible = true;
                pnl_lookandfeelCustomizations.Visible = true;
                pnl_lookandfeelCustomizations_tab.Visible = true;
                pnl_sitemaprobotFiles.Visible = true;
                pnl_sitemaprobotFiles_tab.Visible = true;
                LoadMetaTags();
            }
            else {
                pnl_meteTagCustomizations.Visible = false;
                pnl_meteTagCustomizations_tab.Visible = false;
                pnl_lookandfeelCustomizations.Visible = false;
                pnl_lookandfeelCustomizations_tab.Visible = false;
                pnl_sitemaprobotFiles.Visible = false;
                pnl_sitemaprobotFiles_tab.Visible = false;
            }
        }

        if (!IsPostBack) {
            BuildBGList();
            LoadEmailSettings();

            if (IsUserInAdminRole()) {
                pnl_meteTagCustomizations.Visible = true;
                pnl_meteTagCustomizations_tab.Visible = true;
                pnl_lookandfeelCustomizations.Visible = true;
                pnl_lookandfeelCustomizations_tab.Visible = true;
                pnl_sitemaprobotFiles.Visible = true;
                pnl_sitemaprobotFiles_tab.Visible = true;
                LoadMetaTags();
                GetSiteMapXml();
                GetRobotsTxt();
            }
            else {
                pnl_meteTagCustomizations.Visible = false;
                pnl_meteTagCustomizations_tab.Visible = false;
                pnl_lookandfeelCustomizations.Visible = false;
                pnl_lookandfeelCustomizations_tab.Visible = false;
                pnl_sitemaprobotFiles.Visible = false;
                pnl_sitemaprobotFiles_tab.Visible = false;
            }

            img_workspaceLogo.ImageUrl = "~/Standard_Images/logo.png?date=" + ServerSettings.ServerDateTime.Ticks;
            img_Favicon.ImageUrl = "~/Standard_Images/favicon.ico?date=" + ServerSettings.ServerDateTime.Ticks;
        }
    }

    private void LoadMetaTags() {
        tb_descriptionMetaTag.Text = string.Empty;
        pnl_keywordsMetaTag.Controls.Clear();

        tb_descriptionMetaTag.Text = MainServerSettings.MetaTagDescription;
        if (string.IsNullOrEmpty(tb_descriptionMetaTag.Text)) {
            lbtn_clearDescriptionMeta.Enabled = false;
            lbtn_clearDescriptionMeta.Visible = false;
        }
        else {
            lbtn_clearDescriptionMeta.Enabled = true;
            lbtn_clearDescriptionMeta.Visible = true;
        }

        string[] splitKeywords = MainServerSettings.MetaTagKeywords.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
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

    private void BuildSettingsList() {
        rb_MonitorCpuUsage_on.Checked = false;
        rb_MonitorCpuUsage_off.Checked = true;
        pnl_CPUMonitorUsageValue.Enabled = false;
        pnl_CPUMonitorUsageValue.Visible = false;
        if (MainServerSettings.MonitorCpuUsage) {
            rb_MonitorCpuUsage_on.Checked = true;
            rb_MonitorCpuUsage_off.Checked = false;
            pnl_CPUMonitorUsageValue.Enabled = true;
            pnl_CPUMonitorUsageValue.Visible = true;
        }

        if (IsPostBack) return;
        tb_adminnote.Text = MainServerSettings.AdminNote;

        rb_cachehp_off.Checked = true;
        rb_cachehp_on.Checked = false;
        if (MainServerSettings.CacheHomePage) {
            rb_cachehp_off.Checked = false;
            rb_cachehp_on.Checked = true;
        }

        tb_MonitorCpuUsagePercentAlert.Text = string.Format("{0:N0}", MainServerSettings.MonitorCpuUsagePercentAlert);
        tb_MonitorMemoryUsageAlert.Text = MainServerSettings.MonitorMemoryUsageAlert.ToString();

        BuildDefaultFontFamilyList();

        tb_defaultfontsize.Text = "";
        int tempFontSize = 0;
        if (!string.IsNullOrEmpty(MainServerSettings.DefaultBodyFontSize) && int.TryParse(MainServerSettings.DefaultBodyFontSize, out tempFontSize)) {
            tb_defaultfontsize.Text = MainServerSettings.DefaultBodyFontSize;
        }

        tb_defaultfontcolor.Text = "";
        string fontColor = MainServerSettings.DefaultBodyFontColor;
        if (!string.IsNullOrEmpty(fontColor)) {
            tb_defaultfontcolor.Text = HelperMethods.CreateFormattedHexColor(fontColor);
        }

        if (string.IsNullOrEmpty(tb_defaultfontcolor.Text)) {
            tb_defaultfontcolor.CssClass += " use-default";
        }

        if (MainServerSettings.ChatEnabled) {
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

        if (MainServerSettings.EmailSystemStatus) {
            pnl_emailStatus_holder.Visible = true;
            if (!MainServerSettings.CustomizationsLocked) {
                pnl_mailCustomize_tab.Visible = true;
                pnl_mailCustomize.Visible = true;
            }
            else {
                pnl_mailCustomize_tab.Visible = false;
                pnl_mailCustomize.Visible = false;
            }
            rb_emailStatus_on.Checked = true;
            rb_emailStatus_on.Checked = false;
            lbl_emailStatus.Text = "On";
            lbl_emailStatus.Style["color"] = "#267F00";
        }
        else {
            pnl_emailStatus_holder.Visible = false;
            pnl_mailCustomize_tab.Visible = false;
            pnl_mailCustomize.Visible = false;
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
            string currTimezone = MainServerSettings.ServerTimezone.ToString();
            foreach (ListItem item in dd_timezoneset.Items) {
                if (item.Value == currTimezone) {
                    item.Selected = true;
                }
                else {
                    item.Selected = false;
                }
            }

            lbl_currentServerTime.Text = ServerSettings.ServerDateTime.ToString();

            double timezone = ServerSettings.GetTimezoneDaylightSavingsOffset(MainServerSettings.ServerTimezone);
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
            if (dd_defaultbodyfontfamily.Items[i].Value == MainServerSettings.DefaultBodyFontFamily) {
                dd_defaultbodyfontfamily.SelectedIndex = i;
                break;
            }
        }
    }


    #region Buttons and Server Management

    protected void btn_showUpdates_Click(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            ShowUpdatePopup sup = new ShowUpdatePopup();
            sup.BuildUsers();
            sup.UpdateAllUsers(true);
            string date = ServerSettings.ServerDateTime.ToString();
            ServerSettings.Update_ShowUpdatesPopupDate(date);
            lbl_dateUpdated_sup.Text = date;
        }
    }

    protected void btn_mapsAPIKey_Click(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            ServerSettings.update_GoogleMapsApiKey(tb_mapsAPIKey.Text);
        }
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
        if (IsUserNameEqualToAdmin()) {
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
        if (IsUserNameEqualToAdmin()) {
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
        int count = 4;
        int.TryParse(tb_totalWorkspacesAllowed.Text.Trim(), out count);
        if ((count <= 0) || (count > 500)) {
            count = 4;
            tb_totalWorkspacesAllowed.Text = "4";
        }

        ServerSettings.update_TotalWorkspacesAllowed(count);
    }

    protected void rb_emailStatus_on_Checked(object sender, EventArgs e) {
        ServerSettings.update_EmailSystemStatus(true);
        ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=pnl_emailSettings");
    }
    protected void rb_emailStatus_off_Checked(object sender, EventArgs e) {
        ServerSettings.update_EmailSystemStatus(false);
        ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=pnl_emailSettings");
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

        if (!IsUserNameEqualToAdmin()) {
            lbtn_bgmanage.Enabled = false;
            lbtn_bgmanage.Visible = false;
            lbtn_bgmanage_SetasDefault.Enabled = false;
            lbtn_bgmanage_SetasDefault.Visible = false;
        }

        dd_bgmanage.Focus();
        RegisterPostbackScripts.RegisterStartupScript(this, "$('img').one('load', function () { $('#main_container').scrollTop($('#main_container').height()); });");
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
                string defaultImg = "default-bg.jpg";
                string defaultDir = ServerSettings.GetServerMapLocation + "App_Themes\\" + CurrentSiteTheme + "\\Body\\";
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
                    RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function() { openWSE.ConfirmWindow('This image is now the default background for " + CurrentSiteTheme + " theme.'); }, 100);");
                }
                catch {
                    RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function() { openWSE.ConfirmWindow('Failed to set " + fi.Name + " as the default background.'); }, 100);");
                }

                break;
            }
        }
    }


    protected void rb_LockFileManager_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_LockFileManager_on.Checked = true;
            rb_LockFileManager_off.Checked = false;
            ServerSettings.update_LockFileManager(true);

        }
    }
    protected void rb_LockFileManager_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_LockFileManager_on.Checked = false;
            rb_LockFileManager_off.Checked = true;
            ServerSettings.update_LockFileManager(false);

        }
    }


    protected void rb_SaveCookiesAsSessions_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_SaveCookiesAsSessions_on.Checked = true;
            rb_SaveCookiesAsSessions_off.Checked = false;
            ServerSettings.update_SaveCookiesAsSessions(true);

        }
    }
    protected void rb_SaveCookiesAsSessions_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_SaveCookiesAsSessions_on.Checked = false;
            rb_SaveCookiesAsSessions_off.Checked = true;
            ServerSettings.update_SaveCookiesAsSessions(false);

        }
    }


    protected void rb_emailonReg_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_emailonReg_on.Checked = true;
            rb_emailonReg_off.Checked = false;
            ServerSettings.update_EmailOnRegister(true);

        }
    }
    protected void rb_emailonReg_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_emailonReg_on.Checked = false;
            rb_emailonReg_off.Checked = true;
            ServerSettings.update_EmailOnRegister(false);

        }
    }


    protected void rb_siteplugins_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_siteplugins_on.Checked = true;
            rb_siteplugins_off.Checked = false;
            ServerSettings.update_SitePluginsLocked(true);

        }
    }
    protected void rb_siteplugins_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_siteplugins_on.Checked = false;
            rb_siteplugins_off.Checked = true;
            ServerSettings.update_SitePluginsLocked(false);

        }
    }


    protected void rb_siteoverlay_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_siteoverlay_on.Checked = true;
            rb_siteoverlay_off.Checked = false;
            ServerSettings.update_OverlaysLocked(true);

        }
    }
    protected void rb_siteoverlay_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_siteoverlay_on.Checked = false;
            rb_siteoverlay_off.Checked = true;
            ServerSettings.update_OverlaysLocked(false);

        }
    }


    protected void rb_lockcustomtables_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_lockcustomtables_on.Checked = true;
            rb_lockcustomtables_off.Checked = false;
            ServerSettings.update_LockCustomTables(true);

        }
    }
    protected void rb_lockcustomtables_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_lockcustomtables_on.Checked = false;
            rb_lockcustomtables_off.Checked = true;
            ServerSettings.update_LockCustomTables(false);

        }
    }


    protected void rb_LockAppCreator_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_LockAppCreator_on.Checked = true;
            rb_LockAppCreator_off.Checked = false;
            ServerSettings.update_LockAppCreator(true);

        }
    }
    protected void rb_LockAppCreator_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_LockAppCreator_on.Checked = false;
            rb_LockAppCreator_off.Checked = true;
            ServerSettings.update_LockAppCreator(false);

        }
    }


    protected void rb_LockBackgroundServices_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_LockBackgroundServices_on.Checked = true;
            rb_LockBackgroundServices_off.Checked = false;
            ServerSettings.update_LockBackgroundServices(true);

        }
    }
    protected void rb_LockBackgroundServices_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_LockBackgroundServices_on.Checked = false;
            rb_LockBackgroundServices_off.Checked = true;
            ServerSettings.update_LockBackgroundServices(false);

        }
    }


    protected void rb_Lockstartupscripts_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_Lockstartupscripts_on.Checked = true;
            rb_Lockstartupscripts_off.Checked = false;
            ServerSettings.update_LockStartupScripts(true);

        }
    }

    protected void rb_Lockstartupscripts_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_Lockstartupscripts_on.Checked = false;
            rb_Lockstartupscripts_off.Checked = true;
            ServerSettings.update_LockStartupScripts(false);

        }
    }


    protected void rb_Lockiplisteneron_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_Lockiplisteneron.Checked = true;
            rb_Lockiplisteneroff.Checked = false;
            ServerSettings.update_LockIPListenerWatchlist(true);

        }
    }

    protected void rb_Lockiplisteneroff_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_Lockiplisteneron.Checked = false;
            rb_Lockiplisteneroff.Checked = true;
            ServerSettings.update_LockIPListenerWatchlist(false);

        }
    }


    protected void rb_sslredirect_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_sslredirect_on.Checked = true;
            rb_sslredirect_off.Checked = false;
            ServerSettings.update_SSLRedirect(true);
            pnl_sslValidation.Enabled = true;
            pnl_sslValidation.Visible = true;

        }
    }

    protected void rb_sslredirect_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_sslredirect_on.Checked = false;
            rb_sslredirect_off.Checked = true;
            ServerSettings.update_SSLRedirect(false);
            pnl_sslValidation.Enabled = false;
            pnl_sslValidation.Visible = false;

        }
    }


    protected void rb_urlvalidation_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_urlvalidation_on.Checked = true;
            rb_urlvalidation_off.Checked = false;
            ServerSettings.update_URLValidation(true);

        }
    }

    protected void rb_urlvalidation_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_urlvalidation_on.Checked = false;
            rb_urlvalidation_off.Checked = true;
            ServerSettings.update_URLValidation(false);

        }
    }


    protected void rb_AssociateWithGroups_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_AssociateWithGroups_on.Checked = true;
            rb_AssociateWithGroups_off.Checked = false;
            ServerSettings.update_AssociateWithGroups(true);

        }
    }
    protected void rb_AssociateWithGroups_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            rb_AssociateWithGroups_on.Checked = false;
            rb_AssociateWithGroups_off.Checked = true;
            ServerSettings.update_AssociateWithGroups(false);

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

                ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
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

                ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
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
                ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
            }
        }
    }

    protected void btn_updateserversettingscache_Click(object sender, EventArgs e) {
        ServerSettings.ResetServerSettings();
        ServerSettings.PageIFrameRedirect(Page, "SiteSettings.aspx");
    }

    protected void btn_clearnotiall_Click(object sender, EventArgs e) {
        UserNotificationMessages un = new UserNotificationMessages(CurrentUsername);
        un.deleteNotification_All();
    }

    protected void btn_clearflagall_Click(object sender, EventArgs e) {
        UserUpdateFlags uuf = new UserUpdateFlags();
        uuf.deleteAllFlags();
    }
    protected void lbtn_showUserFlags_Click(object sender, EventArgs e) {
        BuildUserUpdatePanelTable();
    }
    protected void hf_deleteUserUpdateFlag_ValueChanged(object sender, EventArgs e) {
        UserUpdateFlags uuf = new UserUpdateFlags();
        uuf.deleteFlag(hf_deleteUserUpdateFlag.Value);
        hf_deleteUserUpdateFlag.Value = string.Empty;
        BuildUserUpdatePanelTable();
    }
    private void BuildUserUpdatePanelTable() {
        pnl_UserUpdateFlagList.Controls.Clear();

        UserUpdateFlags uuf = new UserUpdateFlags();
        List<Dictionary<string, string>> updateTable = uuf.GetAllFlags(CurrentUsername);

        TableBuilder tableBuilder = new TableBuilder(Page, true, true, 1, "userUpdateTable_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("SessionID", string.Empty, false));
        headerColumns.Add(new TableBuilderHeaderColumns("UserName", string.Empty, true));
        headerColumns.Add(new TableBuilderHeaderColumns("AppID", string.Empty, false));
        headerColumns.Add(new TableBuilderHeaderColumns("GroupName", string.Empty, false));
        tableBuilder.AddHeaderRow(headerColumns, true, "UserName");
        #endregion

        #region Build Body
        foreach (Dictionary<string, string> item in updateTable) {
            List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();
            bodyColumns.Add(new TableBuilderBodyColumnValues("SessionID", item["SessionID"], TableBuilderColumnAlignment.Left));
            bodyColumns.Add(new TableBuilderBodyColumnValues("UserName", item["UserName"], TableBuilderColumnAlignment.Left));
            bodyColumns.Add(new TableBuilderBodyColumnValues("AppID", item["AppID"], TableBuilderColumnAlignment.Left));
            bodyColumns.Add(new TableBuilderBodyColumnValues("GroupName", item["GroupName"], TableBuilderColumnAlignment.Left));

            string deleteBtn = "<a onclick=\"DeleteUserUpdateFlag('" + item["ID"] + "');return false;\" class=\"td-delete-btn\"></a>";
            tableBuilder.AddBodyRow(bodyColumns, deleteBtn);
        }
        #endregion

        pnl_UserUpdateFlagList.Controls.Add(tableBuilder.CompleteTableLiteralControl("No user flags available"));
        updatepnl_UserUpdateFlagList.Update();
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'UserUpdateFlags-element', 'User Update Flag Table');");
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
    }
    protected void rb_siteoffline_CheckedChanged(object sender, EventArgs e) {
        rb_siteoffline.Checked = true;
        rb_siteonline.Checked = false;
        ServerSettings.update_SiteOffLine(true);
    }

    protected void rb_siteCustomizations_On_CheckedChanged(object sender, EventArgs e) {
        rb_siteCustomizations_On.Checked = true;
        rb_siteCustomizations_Off.Checked = false;
        ServerSettings.update_CustomizationsLocked(true);
        ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=pnl_admincontrolsonly");
    }
    protected void rb_siteCustomizations_Off_CheckedChanged(object sender, EventArgs e) {
        rb_siteCustomizations_Off.Checked = true;
        rb_siteCustomizations_On.Checked = false;
        ServerSettings.update_CustomizationsLocked(false);
        ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=pnl_admincontrolsonly");
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
        var w = new App(CurrentUsername);
        w.DeleteUserProperties();
    }

    protected void btn_updateadminnote_Click(object sender, EventArgs e) {
        string note = tb_adminnote.Text.Trim();
        if (MainServerSettings.AdminNote != tb_adminnote.Text) {
            ServerSettings.update_AdminNote(note);

            string by = HelperMethods.MergeFMLNames(CurrentUserMemberDatabase);

            ServerSettings.update_AdminNoteBy(by);


            lbl_adminnoteby.Text = "<b class='pad-right-sml'>Updated by:</b>" + by;

        }
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
        MembershipUser mUser = Membership.GetUser(CurrentUsername);
        if (!string.IsNullOrEmpty(mUser.Email)) {
            message.To.Add(mUser.Email);
            ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Message Title Holder</h1>", ServerSettings.SiteName + ": Test Email", "Email body text holder");
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
            ServerSettings.UpdateMailSettings(CurrentUsername, tb_smtpserver.Text.Trim(), tb_portnumber.Text.Trim(), tb_usernamesmtp.Text.Trim(), tb_passwordsmtp.Text.Trim(), rb_ssl_enabled.Checked);
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
        if (IsUserNameEqualToAdmin()) {
            GroupSessions.ClearAllGroupLoginSessions();
        }
    }

    protected void btn_ClearUserCahce_Click(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin()) {
            CurrentUserMemberDatabase.ClearUserTable();
        }
    }

    protected void btn_timezoneset_Click(object sender, EventArgs e) {
        ServerSettings.update_ServerTimezone(dd_timezoneset.SelectedValue);
        SetServerTimezone(string.Empty);
    }

    protected void btn_defaultbodyfontfamily_Click(object sender, EventArgs e) {
        ServerSettings.update_DefaultBodyFontFamily(dd_defaultbodyfontfamily.SelectedValue);
        ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
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

        ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
    }

    protected void lbtn_defaultfontsize_clear_Click(object sender, EventArgs e) {
        ServerSettings.update_DefaultBodyFontSize(string.Empty);
        ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
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

        ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
    }

    protected void lbtn_defaultfontcolor_clear_Click(object sender, EventArgs e) {
        ServerSettings.update_DefaultBodyFontColor(string.Empty);
        ServerSettings.PageIFrameRedirect(Page, "~/SiteTools/ServerMaintenance/SiteSettings.aspx?tab=Customizations");
    }

    protected void hf_UpdateHeader_Changed(object sender, EventArgs e) {
        if (hf_UpdateHeader.Value == "CLEARHEADER") {
            ServerSettings.UpdateHeaderStringMailSettings(string.Empty);
        }
        else {
            ServerSettings.UpdateHeaderStringMailSettings(HttpUtility.UrlDecode(hf_UpdateHeader.Value));
        }
        hf_UpdateHeader.Value = string.Empty;
        LoadHeaderFooterEmailSettings();
    }
    protected void hf_UpdateFooter_Changed(object sender, EventArgs e) {
        if (hf_UpdateFooter.Value == "CLEARFOOTER") {
            ServerSettings.UpdateFooterStringMailSettings(string.Empty);
        }
        else {
            ServerSettings.UpdateFooterStringMailSettings(HttpUtility.UrlDecode(hf_UpdateFooter.Value));
        }
        hf_UpdateFooter.Value = string.Empty;
        LoadHeaderFooterEmailSettings();
    }
    private void LoadHeaderFooterEmailSettings() {
        var str = new StringBuilder();
        str.Append("UnescapeCode(" + HttpUtility.JavaScriptStringEncode(ServerSettings.EmailHeader, true) + ", 'htmlEditorHeader');");
        str.Append("UnescapeCode(" + HttpUtility.JavaScriptStringEncode(ServerSettings.EmailFooter, true) + ", 'htmlEditorFooter');");
        RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
    }

    protected void rb_MonitorCpuUsage_on_CheckedChanged(object sender, EventArgs e) {
        ServerSettings.update_MonitorCpuUsage(true);

        rb_MonitorCpuUsage_on.Checked = true;
        rb_MonitorCpuUsage_off.Checked = false;
        pnl_CPUMonitorUsageValue.Enabled = true;
        pnl_CPUMonitorUsageValue.Visible = true;
    }
    protected void rb_MonitorCpuUsage_off_CheckedChanged(object sender, EventArgs e) {
        ServerSettings.update_MonitorCpuUsage(false);

        rb_MonitorCpuUsage_on.Checked = false;
        rb_MonitorCpuUsage_off.Checked = true;
        pnl_CPUMonitorUsageValue.Enabled = false;
        pnl_CPUMonitorUsageValue.Visible = false;
    }

    protected void btn_MonitorCpuUsagePercentAlert_Click(object sender, EventArgs e) {
        string monitorCpuUsagePercentAlert = tb_MonitorCpuUsagePercentAlert.Text.Trim();
        if (string.IsNullOrEmpty(monitorCpuUsagePercentAlert)) {
            monitorCpuUsagePercentAlert = "75";
        }

        double monitorCpuUsagePercentAlertFloat = 0.0f;
        monitorCpuUsagePercentAlert = monitorCpuUsagePercentAlert.Replace("%", string.Empty);
        double.TryParse(monitorCpuUsagePercentAlert, out monitorCpuUsagePercentAlertFloat);

        ServerSettings.update_MonitorCpuUsagePercentAlert(monitorCpuUsagePercentAlertFloat);

        tb_MonitorCpuUsagePercentAlert.Text = string.Format("{0:N0}", MainServerSettings.MonitorCpuUsagePercentAlert);
    }

    protected void btn_MonitorMemoryUsageAlert_Click(object sender, EventArgs e) {
        string monitorMemoryUsageAlert = tb_MonitorMemoryUsageAlert.Text.Trim();
        if (string.IsNullOrEmpty(monitorMemoryUsageAlert)) {
            monitorMemoryUsageAlert = "150000";
        }

        long monitorMemoryUsageAlertLong = 0;
        long.TryParse(monitorMemoryUsageAlert, out monitorMemoryUsageAlertLong);

        ServerSettings.update_MonitorMemoryUsageAlert(monitorMemoryUsageAlertLong);

        tb_MonitorMemoryUsageAlert.Text = MainServerSettings.MonitorMemoryUsageAlert.ToString();
    }

    protected void btn_UpdateSiteName_Click(object sender, EventArgs e) {
        ServerSettings.update_SiteName(txt_SiteName.Text);
        HelperMethods.PageRedirect(Request.RawUrl);
    }

    protected void btn_RestartServerApplication_Click(object sender, EventArgs e) {
        ServerSettings.RunStartServerApplication = true;
        ServerSettings.StartServerApplication(false);
    }

    #endregion

}