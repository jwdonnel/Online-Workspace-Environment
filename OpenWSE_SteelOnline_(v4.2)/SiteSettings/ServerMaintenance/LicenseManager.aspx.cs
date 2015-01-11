using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Web.Security;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Web.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlServerCe;

public partial class SiteSettings_LicenseManager : System.Web.UI.Page {

    private readonly AppLog _applog = new AppLog(false);
    private LicenseFile lf = null;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;

        if (!ServerSettings.CheckWebConfigFile()) {
            LoadScripts();
        }

        lf = CheckLicense.LicenseFile;
        if ((CheckLicense.IsExpired && CheckLicense.TrialActivated) || (HelperMethods.ConvertBitToBoolean(Request.QueryString["purchase"]) && (!CheckLicense.LicenseValid || CheckLicense.TrialActivated))) {
            StartUpPage_Purchase();
        }
        else if (lf != null && lf.LicenseId == CheckLicense.TrialKey) {
            StartUpPage_Trial();
        }
        else {
            if (!userId.IsAuthenticated) {
                Page.Response.Redirect("~/Default.aspx");
            }
            else if (Roles.IsUserInRole(userId.Name, ServerSettings.AdminUserName)) {
                StartUpPage();
                if (HelperMethods.ConvertBitToBoolean(Request.QueryString["purchase"])) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('This license has already been purchased.');");
                }

                GetPostBack();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void LoadScripts() {
        ScriptManager sm = ScriptManager.GetCurrent(Page);

        sm.Scripts.Add(new ScriptReference("//code.jquery.com/jquery-1.11.1.min.js"));
        sm.Scripts.Add(new ScriptReference("//code.jquery.com/jquery-migrate-1.2.1.min.js"));
        sm.Scripts.Add(new ScriptReference("//code.jquery.com/ui/1.11.1/jquery-ui.min.js"));
        sm.Scripts.Add(new ScriptReference(ResolveUrl("~/Scripts/jquery/combined-scripts.min.js")));
        sm.Scripts.Add(new ScriptReference(ResolveUrl("~/Scripts/SiteCalls/Min/openwse.min.js")));

        if (sm != null) sm.ScriptMode = ScriptMode.Release;

        StartupStyleSheets sss = new StartupStyleSheets(false);
        sss.AddCssToPage("~/App_Themes/Standard/site_desktop.css", Page);
        sss.AddCssToPage("~/App_Themes/Standard/jqueryUI.css", Page);

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE_Config.siteRootFolder='" + ResolveUrl("~/").Replace("/", "") + "';");
    }

    private void StartUpPage() {
        if (CheckLicense.LicenseValid) {
            lbl_licenseStatus.Text = "Valid";
            lbl_licenseStatus.ForeColor = System.Drawing.Color.Green;
            lbtn_tryValidate.Text = "Refresh";
            lbl_licenseInvalidHint.Enabled = false;
            lbl_licenseInvalidHint.Visible = false;

            LinkButton lbtn_signoff_NoLicense = (LinkButton)Master.FindControl("lbtn_signoff_NoLicense");
            if (lbtn_signoff_NoLicense != null) {
                lbtn_signoff_NoLicense.Enabled = false;
                lbtn_signoff_NoLicense.Visible = false;
            }
        }
        else {
            if (CheckLicense.IsExpired) {
                lbl_licenseStatus.Text = "Expired";
            }
            else {
                lbl_licenseStatus.Text = "Invalid";
            }
            lbl_licenseStatus.ForeColor = System.Drawing.Color.Red;
            lbtn_tryValidate.Text = "Try to validate";
            lbl_licenseInvalidHint.Enabled = true;
            lbl_licenseInvalidHint.Visible = true;

            LinkButton lbtn_signoff_NoLicense = (LinkButton)Master.FindControl("lbtn_signoff_NoLicense");
            if (lbtn_signoff_NoLicense != null) {
                lbtn_signoff_NoLicense.Enabled = true;
                lbtn_signoff_NoLicense.Visible = true;
            }

            RegisterPostbackScripts.RegisterStartupScript(this, "$('#workspace-selector, .top-options, .sidebar-padding-menulinks').hide();$('.bgchange-icon').remove();");
        }

        string licenseFile = ServerSettings.GetServerMapLocation + "App_Data\\" + CheckLicense.LicenseFileName;
        if (File.Exists(licenseFile)) {
            lbl_licenseFile.Text = ResolveUrl("~/App_Data/" + CheckLicense.LicenseFileName);
            pnl_licenseFileContents.Enabled = true;
            pnl_licenseFileContents.Visible = true;

            pnl_licenseContents.Controls.Clear();

            if ((string.IsNullOrEmpty(lf.Host)) || (string.IsNullOrEmpty(lf.ExpirationDate))
                        || (string.IsNullOrEmpty(lf.WebsiteUrl)) || (string.IsNullOrEmpty(lf.EmailAddress))
                        || (string.IsNullOrEmpty(lf.DateIssued)) || (string.IsNullOrEmpty(lf.LicenseId))
                        || (string.IsNullOrEmpty(lf.WebsiteName))) {
                pnl_licenseContents.Controls.Add(new LiteralControl("<h4>ERROR - License file is either corrupted or invalid.</h4>"));
            }
            else {
                StringBuilder str = new StringBuilder();
                str.Append("<div class='clear-space-two'></div>");
                // str.AppendFormat("<span>Host Url:</span>{0}<div class='clear-space-five'></div>", lf.Host);
                str.AppendFormat("<span>Date Issued:</span>{0}<div class='clear-space-five'></div>", lf.DateIssued);
                str.AppendFormat("<span>Expiration Date:</span>{0}<div class='clear-space-five'></div>", lf.ExpirationDate);
                str.AppendFormat("<span>Website Url:</span>{0}<div class='clear-space-five'></div>", lf.WebsiteUrl);

                string siteName = lf.WebsiteName;
                if (ServerSettings.SiteName != lf.WebsiteName) {
                    ServerSettings.update_SiteName(lf.WebsiteName);
                    siteName = lf.WebsiteName + " - <b>Refresh the page to update the website name.</b>";
                }

                str.AppendFormat("<span>Website Name:</span>{0}<div class='clear-space-five'></div>", siteName);
                str.AppendFormat("<span>Email Address:</span>{0}<div class='clear-space-five'></div>", lf.EmailAddress);

                string licenseType = "Full";
                if (lf.LicenseId.ToLower().Contains("-trial")) {
                    DateTime issued = new DateTime();
                    DateTime expires = new DateTime();
                    DateTime.TryParse(lf.DateIssued, out issued);
                    DateTime.TryParse(lf.ExpirationDate, out expires);
                    int days = expires.Subtract(issued).Days;
                    licenseType = days.ToString() + " Day Trial";
                }

                str.AppendFormat("<span>License Type:</span>{0}<div class='clear-space-five'></div>", licenseType);
                str.AppendFormat("<span>Creative Commons License:</span><div class='clear-space-five'></div><div class='cc-type'>{0}</div><div class='clear-space-five'></div>", CheckLicense.GetLicenseTermLinks(lf.CCLicenseType));
                pnl_licenseContents.Controls.Add(new LiteralControl(str.ToString()));
            }
        }
        else {
            lbl_licenseFile.Text = "No license file present";
            pnl_licenseFileContents.Enabled = false;
            pnl_licenseFileContents.Visible = false;
        }
    }

    private void GetPostBack() {
        string controlName = Request.Params["__EVENTTARGET"];
        switch (controlName) {
            case "MainContent_btn_uploadFile":
                if (fu_newLicenseFile.HasFile) {
                    FileInfo fileInfo = new FileInfo(fu_newLicenseFile.FileName);
                    if (fileInfo.Extension.ToLower() == CheckLicense.LicenseFileNameExt) {
                        string licenseFile = ServerSettings.GetServerMapLocation + "App_Data\\" + CheckLicense.LicenseFileName;
                        try {
                            if (File.Exists(licenseFile)) {
                                File.Delete(licenseFile);
                            }
                            fu_newLicenseFile.SaveAs(licenseFile);
                            CheckLicense.ValidateLicense();
                        }
                        catch { }
                    }
                }

                ServerSettings.PageToolViewRedirect(this.Page, "LicenseManager.aspx");
                break;
        }
    }

    private void StartUpPage_Trial() {
        pnl_nonTrialVersion.Enabled = false;
        pnl_nonTrialVersion.Visible = false;
        pnl_trialVersion.Enabled = true;
        pnl_trialVersion.Visible = true;
        pnl_purchaseVersion.Enabled = false;
        pnl_purchaseVersion.Visible = false;
        pnl_purchaseFinishVersion.Enabled = false;
        pnl_purchaseFinishVersion.Visible = false;

        string text = CheckLicense.DaysLeftBeforeExpired + " days";
        if (CheckLicense.DaysLeftBeforeExpired == "1") {
            text = CheckLicense.DaysLeftBeforeExpired + " day";
        }

        pnlCCLicenseType1.Controls.Add(new LiteralControl(CheckLicense.GetLicenseTermLinks()));

        Page.Title = "Trial License Activation";

        trialLength.InnerHtml = text;
        trialtext.InnerHtml = " trial";

        StringBuilder _strScriptreg = new StringBuilder();
        _strScriptreg.Append("$('#app_title_bg').css('background', '#EFEFEF url(\"" + ResolveUrl("~/App_Themes/Standard/Body/default-bg.jpg") + "\") repeat top left');");
        _strScriptreg.Append("$('#app_title_bg').find('.page-title').html('Trial License Activation');");
        _strScriptreg.Append("$('#always-visible, .top-options, .sidebar-padding-menulinks').hide();$('.bgchange-icon').remove();");
        RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
    }

    private void StartUpPage_Purchase() {
        pnl_nonTrialVersion.Enabled = false;
        pnl_nonTrialVersion.Visible = false;
        pnl_trialVersion.Enabled = false;
        pnl_trialVersion.Visible = false;
        pnl_purchaseVersion.Enabled = true;
        pnl_purchaseVersion.Visible = true;
        pnl_purchaseFinishVersion.Enabled = false;
        pnl_purchaseFinishVersion.Visible = false;

        Page.Title = "Purchase License";

        pnlCCLicenseType2.Controls.Add(new LiteralControl(CheckLicense.GetLicenseTermLinks()));

        StringBuilder _strScriptreg = new StringBuilder();
        _strScriptreg.Append("$('#app_title_bg').css('background', '#EFEFEF url(\"" + ResolveUrl("~/App_Themes/Standard/Body/default-bg.jpg") + "\") repeat top left');");
        if (CheckLicense.IsExpired) {
            _strScriptreg.Append("$('#app_title_bg').find('.page-title').html('Trial Expired - Purchase Full Version');");
        }
        else {
            _strScriptreg.Append("$('#app_title_bg').find('.page-title').html('Purchase Full Version');");
        }
        _strScriptreg.Append("$('#always-visible, .top-options, .sidebar-padding-menulinks').hide();$('.bgchange-icon').remove();");
        RegisterPostbackScripts.RegisterStartupScript(this, _strScriptreg.ToString());
    }

    protected void lbtn_tryValidate_Clicked(object sender, EventArgs e) {
        CheckLicense.LicenseValid = false;
        CheckLicense.ValidateLicense();
        if (CheckLicense.LicenseValid) {
            ServerSettings.PageToolViewRedirect(this.Page, "LicenseManager.aspx");
        }
        else {
            StartUpPage();
        }
    }

    protected void btn_SubmitTrial_Clicked(object sender, EventArgs e) {
        if ((txt_emailAddress.Text.Contains("@")) && (txt_emailAddress.Text.Contains("."))) {
            string errorMessage = string.Empty;
            if (CheckLicense.GetTrialLicense(txt_WebsiteUrl.Text.Trim(), txt_WebsiteName.Text.Trim(), txt_emailAddress.Text.Trim(), out errorMessage)) {
                ServerSettings.PageToolViewRedirect(this.Page, "LicenseManager.aspx");
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('" + errorMessage + "');");
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Email address is not valid.');");
        }
    }

    protected void btn_ValidationCodePurchased_Click(object sender, EventArgs e) {
        string errorMessage = string.Empty;
        lbtn_goFoward_purchased.Enabled = false;
        lbtn_goFoward_purchased.Visible = false;
        lbl_valError.Text = string.Empty;

        if (!string.IsNullOrEmpty(txt_ValidationCodePurchased.Text)) {
            if (CheckLicense.CheckValidationCode(txt_ValidationCodePurchased.Text, out errorMessage)) {
                pnl_nonTrialVersion.Enabled = false;
                pnl_nonTrialVersion.Visible = false;
                pnl_trialVersion.Enabled = false;
                pnl_trialVersion.Visible = false;
                pnl_purchaseVersion.Enabled = false;
                pnl_purchaseVersion.Visible = false;
                pnl_purchaseFinishVersion.Enabled = true;
                pnl_purchaseFinishVersion.Visible = true;

                LicenseFile lf = CheckLicense.LicenseFile;

                pnlCCLicenseType3.Controls.Add(new LiteralControl(CheckLicense.GetLicenseTermLinks()));

                lbtn_goFoward_purchased.Enabled = true;
                lbtn_goFoward_purchased.Visible = true;

                txt_WebsiteName_Purchased.Text = lf.WebsiteName;
                txt_WebsiteUrl_Purchased.Text = lf.WebsiteUrl;
                txt_emailAddress_Purchased.Text = lf.EmailAddress;
            }
            else {
                if (string.IsNullOrEmpty(errorMessage)) {
                    errorMessage = "An unknown error occurred while trying to validate code. Please try again.";
                }
                lbl_valError.Text = errorMessage;
            }
        }
        else {
            lbl_valError.Text = "Validation Code cannot be blank.";
        }
    }

    protected void btn_SubmitFull_Clicked(object sender, EventArgs e) {
        string valCode = txt_ValidationCodePurchased.Text.Trim();
        string siteName = txt_WebsiteName_Purchased.Text.Trim();
        string url = txt_WebsiteUrl_Purchased.Text.Trim();
        string email = txt_emailAddress_Purchased.Text.Trim();
        if (string.IsNullOrEmpty(valCode) || string.IsNullOrEmpty(siteName) || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(email)) {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('One or more fields are missing or are invalid. Please try again.');");
        }
        else {
            if ((email.Contains("@")) && (email.Contains("."))) {
                string errorMessage = string.Empty;
                if (CheckLicense.GetFullLicense(valCode, url, siteName, email, out errorMessage)) {
                    ServerSettings.PageToolViewRedirect(this.Page, "LicenseManager.aspx");
                }
                else {
                    if (string.IsNullOrEmpty(errorMessage)) {
                        errorMessage = "An unknown error occurred. Please try again.";
                    }
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('" + errorMessage + "');");
                }
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Email address is not valid.');");
            }
        }
    }
    protected void lbtn_goBack_purchased_Click(object sender, EventArgs e) {
        pnl_nonTrialVersion.Enabled = false;
        pnl_nonTrialVersion.Visible = false;
        pnl_trialVersion.Enabled = false;
        pnl_trialVersion.Visible = false;
        pnl_purchaseVersion.Enabled = true;
        pnl_purchaseVersion.Visible = true;
        pnl_purchaseFinishVersion.Enabled = false;
        pnl_purchaseFinishVersion.Visible = false;
    }

    protected void lbtn_goFoward_purchased_Click(object sender, EventArgs e) {
        pnl_nonTrialVersion.Enabled = false;
        pnl_nonTrialVersion.Visible = false;
        pnl_trialVersion.Enabled = false;
        pnl_trialVersion.Visible = false;
        pnl_purchaseVersion.Enabled = false;
        pnl_purchaseVersion.Visible = false;
        pnl_purchaseFinishVersion.Enabled = true;
        pnl_purchaseFinishVersion.Visible = true;
    }

    protected void lbtn_useDefaultUrl_Click(object sender, EventArgs e) {
        txt_WebsiteUrl.Text = GetDefaultUrl();
    }
    protected void lbtn_useDefaultUrl_Purchased_Click(object sender, EventArgs e) {
        txt_WebsiteUrl_Purchased.Text = GetDefaultUrl();
        pnl_nonTrialVersion.Enabled = false;
        pnl_nonTrialVersion.Visible = false;
        pnl_trialVersion.Enabled = false;
        pnl_trialVersion.Visible = false;
        pnl_purchaseVersion.Enabled = false;
        pnl_purchaseVersion.Visible = false;
        pnl_purchaseFinishVersion.Enabled = true;
        pnl_purchaseFinishVersion.Visible = true;
    }

    private string GetDefaultUrl() {
        string orgUrl = Request.Url.AbsoluteUri;
        if (orgUrl.IndexOf("SiteSettings/ServerMaintenance/LicenseManager.aspx") != -1) {
            orgUrl = orgUrl.Remove(orgUrl.IndexOf("SiteSettings/ServerMaintenance/LicenseManager.aspx"));
        }
        return orgUrl;
    }

}