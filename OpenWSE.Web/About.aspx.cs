using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.IO;
using System.Security.Principal;
using System.Web.UI.HtmlControls;
using System.Web.Security;

public partial class SiteTools_About : Page {

    protected void Page_Load(object sender, EventArgs e) {
        ServerSettings.StartServerApplication(IsPostBack);

        About about = new About(ServerSettings.GetServerMapLocation + "ChangeLog.xml");
        about.ParseXml();

        lbl_currentVer.InnerHtml = about.CurrentVersion;
        BaseMaster.SetTopLogoTags(this.Page, iframe_title_logo, null);

        var str = new StringBuilder();
        str.Append("<table width='100%' cellpadding='10' cellspacing='10'><tbody>");

        MembershipUser adminUser = Membership.GetUser(ServerSettings.AdminUserName);
        if (adminUser != null) {
            if (!string.IsNullOrEmpty(adminUser.Email)) {
                hyp_AdminUserEmail.NavigateUrl = "mailto:" + adminUser.Email;
            }
            else {
                hyp_AdminUserEmail.NavigateUrl = "mailto:jwdonnel@gmail.com";
            }
        }
        else {
            hyp_AdminUserEmail.NavigateUrl = "mailto:jwdonnel@gmail.com";
        }

        Dictionary<string, string> aboutItems = about.AboutItems;
        foreach (KeyValuePair<string, string> entry in aboutItems) {
            string name = entry.Key;
            string value = entry.Value;
            str.Append("<tr>");
            str.Append("<td class='usercolor_Office pad-all logdates'>" + name + "</td>");
            str.Append("<td class='pad-all logentry'>" + value.Replace("<br />", "<div class='clear-space-five'></div>") + "</td>");
            str.Append("</tr>");
        }
        str.Append("</tbody></table>");

        ltl_changeLog.Text = str.ToString();

        LoadUserCustomizations();

        if (Request.QueryString.Count != 0) {
            if (Request.QueryString["a"] == "changelog") {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#siteInfo').hide();$('#changeLog').show(); $('#hdl1').removeClass('active'); $('#hdl3').addClass('active');");
            }
            else if (Request.QueryString["a"] == "termsofuse") {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#siteInfo').hide();$('#termsofuse').show(); $('#hdl1').removeClass('active'); $('#hdl2').addClass('active');");
            }

            if (!string.IsNullOrEmpty(Request.QueryString["redirect"])) {
                close_iframe.HRef = ResolveUrl("~/" + Request.QueryString["redirect"]);
            }
        }

        string tempSiteName = ServerSettings.SiteName;
        if (!string.IsNullOrEmpty(tempSiteName) && tempSiteName.ToLower() != "openwse" && tempSiteName.ToLower() != "onlinewse") {
            forkme_banner.Visible = false;
        }

        ServerSettings.AddMetaTagsToPage(this.Page);
    }

    private void LoadUserCustomizations() {
        if (ServerSettings.CheckWebConfigFile) {
            string siteColorOption = "1~;2~";

            try {
                IIdentity userId = HttpContext.Current.User.Identity;
                if (userId.IsAuthenticated) {
                    MemberDatabase _member = new MemberDatabase(userId.Name);
                    if (!Roles.IsUserInRole(userId.Name, ServerSettings.AdminUserName)) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "availableHelpPages='" + _member.AdminPagesNonlist + "';");
                    }

                    BaseMaster baseMaster = new BaseMaster();
                    baseMaster.LoadAllDefaultScriptsStyleSheets(Page, _member);

                    siteColorOption = _member.SiteColorOption;
                }
                else {
                    NewUserDefaults _demoCustomizations = new NewUserDefaults("DemoNoLogin");
                    _demoCustomizations.GetDefaults();
                    RegisterPostbackScripts.RegisterStartupScript(this, "availableHelpPages='Workspace" + ServerSettings.StringDelimiter + "';");

                    BaseMaster baseMaster = new BaseMaster();
                    baseMaster.LoadAllDefaultScriptsStyleSheets(Page, _demoCustomizations);

                    if (_demoCustomizations.DefaultTable.ContainsKey("SiteColorOption") && !string.IsNullOrEmpty(_demoCustomizations.DefaultTable["SiteColorOption"])) {
                        siteColorOption = _demoCustomizations.DefaultTable["SiteColorOption"];
                    }
                }
            }
            catch {
                BaseMaster baseMaster = new BaseMaster();
                baseMaster.LoadAllDefaultScriptsStyleSheets(Page);
            }

            #region Site Color Option
            string selectedColorIndex = "1";
            string[] siteOptionSplit = siteColorOption.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
            if (siteOptionSplit.Length >= 1) {
                selectedColorIndex = siteOptionSplit[0];
            }

            site_mainbody_about.Attributes["data-coloroption"] = selectedColorIndex;
            #endregion
        }
    }

}