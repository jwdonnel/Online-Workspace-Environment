using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Globalization;
using System.Data;
using System.Text;
using OpenWSE_Tools.Overlays;
using OpenWSE_Tools.Apps;

public partial class Overlays_MessageBoard_Overlay : System.Web.UI.UserControl
{
    private ServerSettings _ss = new ServerSettings();
    private OverlayInitializer _overlayInit;
    private MemberDatabase _member;
    private string _username;

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userId = HttpContext.Current.User.Identity;

        _overlayInit = new OverlayInitializer(userId.Name, "Overlays/MessageBoard/MessageBoard_Overlay.ascx");
        if (_overlayInit.TryLoadOverlay) {
            _username = userId.Name;
            _member = new MemberDatabase(_username);
        }
        else if ((!userId.IsAuthenticated) && (!_ss.NoLoginRequired)) {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }
    }
}