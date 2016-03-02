using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;
using System.Web.Security;

public partial class Apps_RSSFeed_RSSFeed : System.Web.UI.UserControl {
    private ServerSettings _ss = new ServerSettings();
    private readonly App _apps = new App(string.Empty);
    private string _ctrlname;
    private MemberDatabase _member;
    private AppInitializer _appInitializer;
    private const string app_id = "app-rssfeed";
    private int _forceUpdateInterval = 15;

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

        LoadAppParams();

        if (!_ss.HideAllAppIcons) {
            img_Title.Visible = true;
            string clImg = _apps.GetAppIconName(app_id);
            img_Title.ImageUrl = "~/Standard_Images/App_Icons/" + clImg;
        }
        else {
            img_Title.Visible = false;
        }

        pnl_AdminRSSFeedSettings.Enabled = false;
        pnl_AdminRSSFeedSettings.Visible = false;

        if (_member != null) {
            _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
            if (Page.User.Identity.IsAuthenticated && Roles.IsUserInRole(Page.User.Identity.Name, ServerSettings.AdminUserName)) {
                pnl_AdminRSSFeedSettings.Enabled = true;
                pnl_AdminRSSFeedSettings.Visible = true;
            }
        }
        else {
            _appInitializer = new AppInitializer(app_id, string.Empty, Page);
        }

        _appInitializer.SetGroupSessionControls(btn_AddRemoveFeeds);
        _appInitializer.LoadScripts_JS(true, "RSSFeedApp.StartRSSFeeder();RSSFeedApp.SetTimeFeedUpdateInterval(" + _forceUpdateInterval + ")");
    }

    private void LoadAppParams() {
        AppParams appParams = new AppParams(false);
        appParams.GetAllParameters_ForApp("app-rssfeed");
        foreach (Dictionary<string, string> dr in appParams.listdt) {
            try {
                string param = dr["Parameter"];
                int indexOf = param.IndexOf("=") + 1;
                string subParam = param.Substring(indexOf);
                if (param.Replace("=" + subParam, string.Empty) == "OnlyUpdateInteveral") {
                    int tempOut = 15;
                    if (int.TryParse(subParam, out tempOut) && tempOut >= 0) {
                        _forceUpdateInterval = tempOut;
                    }
                }
            }
            catch { }
        }
    }
}