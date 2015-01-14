using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.Apps;

public partial class Apps_Feedback : System.Web.UI.UserControl
{
    private const string app_id = "app-feedback";
    private readonly App _apps = new App();

    protected void Page_Load(object sender, EventArgs e) {
        AppInitializer _appInitializer = new AppInitializer(app_id, string.Empty, Page);
    }
}