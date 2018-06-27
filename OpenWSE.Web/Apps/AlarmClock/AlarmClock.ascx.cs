using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using OpenWSE_Tools.Apps;

public partial class Apps_AlarmClock_AlarmClock : System.Web.UI.UserControl
{
    private AppInitializer _appInitializer;
    private const string app_id = "app-alarmclock";

    protected void Page_Load(object sender, EventArgs e)
    {
        string currUser = "guest";
        IIdentity userId = Page.User.Identity;
        if (userId != null) {
            currUser = userId.Name;
        }

        hf_currUser_AlarmClock.Value = currUser;
        UpdateAlarmClock();
    }

    private void UpdateAlarmClock() {
        _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        _appInitializer.LoadScripts_JS(true);
    }
}