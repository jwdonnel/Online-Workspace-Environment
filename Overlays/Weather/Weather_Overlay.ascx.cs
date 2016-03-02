using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Text;
using System.Xml.XPath;
using System.Net;
using System.Xml;
using OpenWSE_Tools.Overlays;

public partial class Overlays_Weather_Overlay : System.Web.UI.UserControl {
    private ServerSettings _ss = new ServerSettings();
    private OverlayInitializer _overlayInit;
    private MemberDatabase _member;
    private string _lat = string.Empty;
    private string _lng = string.Empty;
    private string _postalCode = "66212";

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;

        _overlayInit = new OverlayInitializer(userId.Name, "Overlays/Weather/Weather_Overlay.ascx");
        if (_overlayInit.TryLoadOverlay)
            LoadOverlay();
        else if ((!userId.IsAuthenticated) && (_ss.NoLoginRequired))
            LoadOverlay();
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }

    private void LoadOverlay() {
        RegisterPostbackScripts.RegisterStartupScript(this, BuildJS);
    }

    private string BuildJS {
        get {
            string theme = "Standard";
            if (_member != null)
                theme = _member.SiteTheme;

            _postalCode = "66212";
            StringBuilder str = new StringBuilder();
            str.Append("Sys.Application.add_load(function () {");
            str.Append("openWSE.WeatherBuilder('" + theme + "');");
            str.Append("});");
            str.Append("$(document).ready(function () {");
            str.Append("openWSE.WeatherBuilder('" + theme + "');");
            str.Append("});");
            return str.ToString();
        }
    }
}