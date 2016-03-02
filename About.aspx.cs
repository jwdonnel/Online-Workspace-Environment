using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.IO;
using System.Security.Principal;
using OpenWSE.Core.Licensing;
using System.Web.UI.HtmlControls;
using System.Web.Security;

public partial class SiteTools_About : System.Web.UI.Page {

    protected void Page_Load(object sender, EventArgs e) {
        About about = new About(ServerSettings.GetServerMapLocation + "ChangeLog.xml");
        about.ParseXml();

        lbl_currentVer.InnerHtml = about.CurrentVersion;

        var str = new StringBuilder();
        str.Append("<table width='100%' cellpadding='10' cellspacing='10'><tbody>");

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

        LoadUserBackground();

        page_title.InnerHtml = "About " + CheckLicense.SiteName;

        pnlCreativeCommonLicense.Controls.Add(new LiteralControl(CheckLicense.GetLicenseTermLinks()));

        if (Request.QueryString.Count != 0) {
            if (Request.QueryString["a"] == "changelog") {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#siteInfo').hide();$('#changeLog').show(); $('#hdl1').removeClass('active'); $('#hdl3').addClass('active');");
            }
            else if (Request.QueryString["a"] == "termsofuse") {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#siteInfo').hide();$('#termsofuse').show(); $('#hdl1').removeClass('active'); $('#hdl2').addClass('active');");
            }
        }

        ServerSettings.AddMetaTagsToPage(this.Page);
    }

    private void LoadUserBackground() {
        if (ServerSettings.CheckWebConfigFile()) {
            try {
                IIdentity userId = HttpContext.Current.User.Identity;
                if (userId.IsAuthenticated) {
                    MemberDatabase _member = new MemberDatabase(userId.Name);
                    AcctSettings.LoadUserBackground(this.Page, null, _member);
                    if (!Roles.IsUserInRole(userId.Name, ServerSettings.AdminUserName)) {
                        RegisterPostbackScripts.RegisterStartupScript(this, "availableHelpPages='" + _member.AdminPagesNonlist + "';");
                    }

                    CustomFonts.SetCustomValues(this.Page, _member);
                }
                else {
                    NewUserDefaults _demoCustomizations = new NewUserDefaults("DemoNoLogin");
                    _demoCustomizations.GetDefaults();
                    AcctSettings.LoadUserBackground(this.Page, _demoCustomizations, null);
                    RegisterPostbackScripts.RegisterStartupScript(this, "availableHelpPages='Workspace" + ServerSettings.StringDelimiter + "';");

                    CustomFonts.SetCustomValues(this.Page, _demoCustomizations.DefaultTable);
                }
            }
            catch { }
        }
    }

}