using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.IO;
using System.Security.Principal;

public partial class SiteSettings_About : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Page.Title = "About OpenWSE";

        About about = new About(ServerSettings.GetServerMapLocation + "ChangeLog.xml");
        about.ParseXml();

        lbl_currentVer.InnerHtml = about.CurrentVersion;

        var str = new StringBuilder();
        str.Append("<table width='100%' cellpadding='10' cellspacing='10'><tbody>");

        Dictionary<string, string> aboutItems = about.AboutItems;
        foreach (KeyValuePair<string, string> entry in aboutItems)
        {
            string name = entry.Key;
            string value = entry.Value;
            str.Append("<tr>");
            str.Append("<td class='usercolor_Office pad-all logdates'>" + name + "</td>");
            str.Append("<td class='pad-all'>" + value + "</td>");
            str.Append("</tr>");
        }
        str.Append("</tbody></table>");

        ltl_changeLog.Text = str.ToString();

        LoadUserBackground();

        page_title.InnerHtml = "About OpenWSE";

        pnlCreativeCommonLicense.Controls.Add(new LiteralControl(CheckLicense.GetLicenseTermLinks()));

        if (Request.QueryString.Count != 0)
        {
            if (Request.QueryString["a"] == "changelog") {
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#siteInfo').hide();$('#changeLog').show(); $('#hdl1').removeClass('active'); $('#hdl2').addClass('active');");
            }
        }
    }

    private void LoadUserBackground() {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (userId.IsAuthenticated) {
            string _username = userId.Name;
            try {
                MemberDatabase _member = new MemberDatabase(_username);
                string img = _member.GetBackgroundImg(1);
                if (!string.IsNullOrEmpty(img)) {
                    if (img.Length == 6) {
                        app_title_bg.Style["background"] = "#" + img;
                    }
                    else {
                        app_title_bg.Style["background"] = "#EFEFEF url(" + img + ") repeat top left";
                    }
                }
                else
                    app_title_bg.Style["background"] = "#EFEFEF url(App_Themes/" + _member.SiteTheme + "/Body/default-bg.jpg) repeat top left";
            }
            catch {
                app_title_bg.Style["background"] = "#EFEFEF url(App_Themes/Standard/Body/default-bg.jpg) repeat top left";
            }
        }
        else {
            try {
                NewUserDefaults _demoCustomizations = new NewUserDefaults("DemoNoLogin");
                _demoCustomizations.GetDefaults();
                string _sitetheme = "Standard";
                if (!string.IsNullOrEmpty(_demoCustomizations.DefaultTable["Theme"])) {
                    _sitetheme = _demoCustomizations.DefaultTable["Theme"];
                }
                app_title_bg.Style["background"] = "#EFEFEF url(App_Themes/" + _sitetheme + "/Body/default-bg.jpg) repeat top left";
            }
            catch {
                app_title_bg.Style["background"] = "#EFEFEF url(App_Themes/Standard/Body/default-bg.jpg) repeat top left";
            }
        }
    }
}