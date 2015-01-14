using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Text;
using OpenWSE_Tools.Overlays;
using OpenWSE_Tools.Apps;

public partial class Overlays_RSSFeed_Overlay : System.Web.UI.UserControl {
    private ServerSettings _ss = new ServerSettings();
    private OverlayInitializer _overlayInit;
    private string _username;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;

        _overlayInit = new OverlayInitializer(userId.Name, "Overlays/RSSFeed_Overlay.ascx");
        if (_overlayInit.TryLoadOverlay) {
            _username = userId.Name;
            LoadOverlay();
        }
        else if ((!userId.IsAuthenticated) && (_ss.NoLoginRequired))
            LoadOverlay();
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }

    private void LoadOverlay() {
        if (string.IsNullOrEmpty(_username)) {
            _username = string.Empty;
        }

        AppInitializer _appInitializer = new AppInitializer("app-rssfeed", _username, Page);
        _appInitializer.LoadScripts_JS(true, "StartRSSFeederOverlay()");
    }
}