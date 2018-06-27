#region

using System;
using System.Security.Principal;
using System.Web.UI;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_Bookmark_Viewer_BookmarkViewer : UserControl
{
    #region private variables

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

        img_Title.Visible = true;
        string clImg = _apps.GetAppIconName(app_id);
        img_Title.ImageUrl = "~/" + clImg;

        _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        _appInitializer.SetGroupSessionControls(pnl_BookmarkPnlBtns);
        _appInitializer.LoadScripts_JS(true);
    }
}