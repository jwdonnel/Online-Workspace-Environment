#region

using System;
using System.Data;
using System.Drawing;
using System.Security.Principal;
using System.Text;
using System.Web.Security;
using System.Web.UI;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_Bookmark_Viewer_BookmarkViewer : UserControl
{
    #region private variables

    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private string _ctrlname;
    private MemberDatabase _member;
    private AppInitializer _appInitializer;
    private const string app_id = "app-bookmarkviewer";

    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userId = Page.User.Identity;
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        if (sm != null)
        {
            string ctlId = sm.AsyncPostBackSourceElementID;
            _ctrlname = ctlId;
        }

        _member = new MemberDatabase(userId.Name);
        UpdateBookmarkPanel();
    }

    private void UpdateBookmarkPanel() {
        App _apps = new App(string.Empty);
        string cl = _apps.GetAppName(app_id);
        lbl_Title.Text = cl;

        if (!_ss.HideAllAppIcons) {
            img_Title.Visible = true;
            string clImg = _apps.GetAppIconName(app_id);
            img_Title.ImageUrl = "~/Standard_Images/App_Icons/" + clImg;
        }
        else
            img_Title.Visible = false;

        _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        _appInitializer.SetGroupSessionControls(pnl_BookmarkPnlBtns);
        _appInitializer.LoadScripts_JS(true);
    }
}