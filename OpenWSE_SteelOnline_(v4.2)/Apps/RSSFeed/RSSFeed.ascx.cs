using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

public partial class Apps_RSSFeed_RSSFeed : System.Web.UI.UserControl {
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly App _apps = new App();
    private string _ctrlname;
    private MemberDatabase _member;
    private AppInitializer _appInitializer;
    private const string app_id = "app-rssfeed";

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = Page.User.Identity;
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        if (sm != null) {
            string ctlId = sm.AsyncPostBackSourceElementID;
            _ctrlname = ctlId;
        }

        _member = new MemberDatabase(userId.Name);
        UpdatePanel();
    }

    private void UpdatePanel() {
        string cl = _apps.GetAppName(app_id);
        lbl_Title.Text = cl;

        if (!_ss.HideAllAppIcons) {
            img_Title.Visible = true;
            string clImg = _apps.GetAppIconName(app_id);
            img_Title.ImageUrl = "~/Standard_Images/App_Icons/" + clImg;
        }
        else
            img_Title.Visible = false;

        if (_member != null) {
            _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        }
        else {
            _appInitializer = new AppInitializer(app_id, string.Empty, Page);
        }

        _appInitializer.LoadScripts_JS(true, "StartRSSFeeder()");
    }
}