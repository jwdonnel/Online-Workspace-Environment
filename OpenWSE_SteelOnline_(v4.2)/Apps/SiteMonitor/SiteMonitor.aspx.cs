#region

using System;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_SiteMonitor_SiteMonitor : Page
{
    private readonly IPWatch ipwatch = new IPWatch(true);
    private MemberDatabase member;
    private AppInitializer _appInitializer;


    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/Default.aspx");

        _appInitializer = new AppInitializer("app-sitemonitor", userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent)
        {
            member = _appInitializer.memberDatabase;
            if (!IsPostBack)
            {
                // Initialize all the scripts and style sheets
                _appInitializer.SetHeaderLabelImage(lbl_Title, img_Title);
                _appInitializer.LoadScripts_JS(false);
                _appInitializer.LoadScripts_CSS();

                MemberDatabase _member = new MemberDatabase(userId.Name);
                string theme = _member.SiteTheme;
                RegisterPostbackScripts.RegisterStartupScript(this, "$('#imgPausePlay').attr('src', '../../App_Themes/" + theme + "/App/pause.png');");
            }
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }
}