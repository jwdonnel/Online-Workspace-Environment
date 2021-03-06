﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Data;
using System.Security.Principal;
using OpenWSE_Tools.Overlays;

public partial class Overlays_WeightTotals_Overlay : System.Web.UI.UserControl {
    private ServerSettings _ss = new ServerSettings();
    private OverlayInitializer _overlayInit;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;

        _overlayInit = new OverlayInitializer(userId.Name, "Overlays/WeightTotals/WeightTotals_Overlay.ascx");
        if (_overlayInit.TryLoadOverlay)
            LoadOverlay();
        else if ((!userId.IsAuthenticated) && (_ss.NoLoginRequired))
            LoadOverlay();
    }

    private void LoadOverlay() {
        ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), BuildJS, true);
    }

    private string BuildJS {
        get {
            StringBuilder str = new StringBuilder();
            str.Append("Sys.Application.add_load(function () {");
            str.Append("    openWSE.AjaxCall('" + ResolveUrl("~/Apps/DailyOverview/DailyTotals.asmx/GetTotals") + "', '{ }', null, function (data) {");
            str.Append("        var response = data.d;");
            str.Append("        if (response != '') {");
            str.Append("            $('#weighttotals_pnl_entries').html(response);");
            str.Append("        }");
            str.Append("    });");
            str.Append("});");
            str.Append("$(document).ready(function () {");
            str.Append("    openWSE.AjaxCall('" + ResolveUrl("~/Apps/DailyOverview/DailyTotals.asmx/GetTotals") + "', '{ }', null, function (data) {");
            str.Append("        var response = data.d;");
            str.Append("        if (response != '') {");
            str.Append("            $('#weighttotals_pnl_entries').html(response);");
            str.Append("        }");
            str.Append("    });");
            str.Append("});");
            return str.ToString();
        }
    }
}