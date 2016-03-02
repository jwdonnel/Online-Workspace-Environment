using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.Apps;

public partial class Apps_Twitter_TwitterStation : System.Web.UI.UserControl {

    private ServerSettings _ss = new ServerSettings();
    private readonly App _apps = new App(string.Empty);
    private AppInitializer _appInitializer;
    private const string app_id = "app-twitterstation";

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

        string interval = GetParms();
        if (string.IsNullOrEmpty(interval)) {
            interval = "5";
        }

        _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        _appInitializer.SetGroupSessionControls(TwitterAdd_element, TwitterEditFeeds_element, btn_addNewTwitterFeed, btn_editTwitterFeeds, "#twitterstation-load .td-edit-btn", "#twitterstation-load .td-cancel-btn");
        _appInitializer.LoadScripts_JS(true, "twitterStation.Init(" + interval + ");");
    }

    private string GetParms() {
        string[] delim = { "=" };
        AppParams appParams = new AppParams(false);
        appParams.GetAllParameters_ForApp(app_id);
        Dictionary<string, string> dicParams = new Dictionary<string, string>();
        foreach (Dictionary<string, string> dr in appParams.listdt) {
            string[] paramSplit = dr["Parameter"].Split(delim, StringSplitOptions.RemoveEmptyEntries);
            if (paramSplit.Length == 2) {
                string key = paramSplit[0];
                string val = paramSplit[1];
                if (!dicParams.ContainsKey(key))
                    dicParams.Add(key, val);
            }
        }

        string interval = "0";
        if (dicParams.ContainsKey("Refresh_Interval")) {
            dicParams.TryGetValue("Refresh_Interval", out interval);
        }

        return interval;
    }

}