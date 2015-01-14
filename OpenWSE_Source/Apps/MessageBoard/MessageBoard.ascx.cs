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
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.Apps;

public partial class Apps_MessageBoard_MessageBoard : System.Web.UI.UserControl
{
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly App _apps = new App();
    private string _ctrlname;
    private MemberDatabase _member;
    private AppInitializer _appInitializer;
    private const string app_id = "app-messageboard";

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
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        LoadGroups();

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
        _appInitializer.LoadScripts_JS(true, "StartMessageBoard();LoadMessageBoardPosts()");

        string script = "$.getScript('//tinymce.cachefly.net/4.1/tinymce.min.js').done(function( script, textStatus ) { LoadTinyMCEControls_Simple();$('#Editor_messageboard_ifr').tooltip({ disabled: true }) });";
        RegisterPostbackScripts.RegisterStartupScript(Page, script);
    }

    private void LoadGroups()
    {
        Groups _groups = new Groups();
        List<string> groupNames = _member.GroupList;

        string groupArray = "";
        foreach (string group in groupNames)
        {
            string groupName = _groups.GetGroupName_byID(group);
            var item = new ListItem(groupName, group);
            if (!dd_groups_messageboard.Items.Contains(item)) {
                dd_groups_messageboard.Items.Add(item);
                groupArray += group + ServerSettings.StringDelimiter;
            }
        }

        if (dd_groups_messageboard.Items.Count > 0) {
            dd_groups_messageboard.Items.Add(new ListItem("- All Groups -", groupArray));
        }
    }
}