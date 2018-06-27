using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Text;
using OpenWSE_Tools.Overlays;

public partial class Overlays_PersonalCalendar_Overlay : System.Web.UI.UserControl
{
    private ServerSettings _ss = new ServerSettings();
    private OverlayInitializer _overlayInit;
    private MemberDatabase _member;

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userId = HttpContext.Current.User.Identity;

        _overlayInit = new OverlayInitializer(userId.Name, "Overlays/PersonalCalendar/PersonalCalendar_Overlay.ascx");
        if (_overlayInit.TryLoadOverlay) {
            _member = new MemberDatabase(userId.Name);
            RegisterPostbackScripts.RegisterStartupScript(this, BuildJS);
        }
    }

    private string BuildJS
    {
        get
        {
            StringBuilder str = new StringBuilder();
            str.Append("Sys.Application.add_load(function () {");
            str.Append("    openWSE.AjaxCall('" + ResolveUrl("~/Apps/PersonalCalendar/PersonalCalendar.asmx/GetOverview") + "', '{ }', null, function (data) {");
            str.Append("        var response = data.d;");
            str.Append("        if (response != '') {");
            str.Append("            $('#PersonalCalendar_pnl_entries').html(response);");
            str.Append("        }");
            str.Append("    });");
            str.Append("});");
            str.Append("$(document).ready(function () {");
            str.Append("    openWSE.AjaxCall('" + ResolveUrl("~/Apps/PersonalCalendar/PersonalCalendar.asmx/GetOverview") + "', '{ }', null, function (data) {");
            str.Append("        var response = data.d;");
            str.Append("        if (response != '') {");
            str.Append("            $('#PersonalCalendar_pnl_entries').html(response);");
            str.Append("        }");
            str.Append("    });");
            str.Append("});");
            return str.ToString();
        }
    }
}