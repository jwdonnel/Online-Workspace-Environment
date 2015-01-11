using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.Apps;

public partial class Apps_GoogleTraffic_GoogleTraffic : System.Web.UI.UserControl {
    private ServerSettings _ss = new ServerSettings();
    private readonly App _apps = new App();
    private AppInitializer _appInitializer;
    private const string app_id = "app-googletraffic";

    protected void Page_Load(object sender, EventArgs e) {
        string cl = _apps.GetAppName(app_id);
        lbl_Title.Text = cl;

        if (!_ss.HideAllAppIcons) {
            img_Title.Visible = true;
            string clImg = _apps.GetAppIconName(app_id);
            img_Title.ImageUrl = "~/Standard_Images/App_Icons/" + clImg;
        }
        else
            img_Title.Visible = false;

        StringBuilder str = new StringBuilder();
        str.Append("$(document).ready(function () { openWSE.ResizeAppBody('#app-googletraffic', false); });");
        str.Append("Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { openWSE.ResizeAppBody('#app-googletraffic', false); });");

        RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
    }
}