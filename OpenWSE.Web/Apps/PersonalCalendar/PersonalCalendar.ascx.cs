using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Data;
using System.Text;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

public partial class Apps_PersonalCalendar_PersonalCalendar : System.Web.UI.UserControl {

    private const string app_id = "app-personalcalendar";

    protected void Page_Load(object sender, EventArgs e) {
        AppInitializer appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        appInitializer.LoadScripts_JS(true);
    }

}