using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using OpenWSE_Tools.Apps;

public partial class Apps_CTLReport_CTLReports : System.Web.UI.UserControl
{
    private readonly App _apps = new App();
    private MemberDatabase _member;
    private AppInitializer _appInitializer;
    private const string app_id = "app-ctlreport";
    private ServerSettings _ss = new ServerSettings();

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userId = Page.User.Identity;
        _member = new MemberDatabase(userId.Name);
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        string cl = _apps.GetAppName(app_id);
        lbl_Title.Text = cl;

        if (!_ss.HideAllAppIcons)
        {
            img_Title.Visible = true;
            string clImg = _apps.GetAppIconName(app_id);
            img_Title.ImageUrl = "~/Standard_Images/App_Icons/" + clImg;
        }
        else
            img_Title.Visible = false;

        _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        _appInitializer.LoadScripts_JS(true);
    }
}