#region

using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;

#endregion

public partial class SiteSettings_ChangePassword : Page {
    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (userId.IsAuthenticated) {
            if (!ServerSettings.AdminPagesCheck("AcctSettings", userId.Name)) {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
        else {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }
    }

    protected void OnContinueButtonClick(object sender, EventArgs e) {
        Page.Response.Redirect(Page.Request.RawUrl);
    }
}