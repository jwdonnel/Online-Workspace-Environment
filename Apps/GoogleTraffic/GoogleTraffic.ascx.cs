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
    private AppInitializer _appInitializer;
    private const string app_id = "app-googletraffic";

    protected void Page_Load(object sender, EventArgs e) { }
}