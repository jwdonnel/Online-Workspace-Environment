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

public partial class Apps_MessageBoard_MessageBoard : System.Web.UI.UserControl {
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private App _apps = new App(string.Empty);
    private string _ctrlname;
    private MemberDatabase _member;
    private AppInitializer _appInitializer;
    private const string app_id = "app-messageboard";

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
        LoadGroups();
        _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        _appInitializer.LoadScripts_JS(true);
    }

    private void LoadGroups() {
        Groups _groups = new Groups();
        List<string> groupNames = _member.GroupList;

        if (!GroupSessions.DoesUserHaveGroupLoginSessionKey(HttpContext.Current.User.Identity.Name)) {
            dd_groups_messageboard.Items.Add(new ListItem("- All Groups -", ""));
            dd_currentGroup_messageboard.Items.Add(new ListItem("- All Groups -", ""));
        }

        foreach (string group in groupNames) {
            string groupName = _groups.GetGroupName_byID(group);
            var item = new ListItem(groupName, group);
            if (!dd_groups_messageboard.Items.Contains(item)) {
                dd_groups_messageboard.Items.Add(item);
            }

            if (!dd_currentGroup_messageboard.Items.Contains(item)) {
                dd_currentGroup_messageboard.Items.Add(item);
            }
        }

        if (dd_groups_messageboard.Items.Count == 1) {
            dd_groups_messageboard.Style["display"] = "none";
        }

        if (dd_currentGroup_messageboard.Items.Count == 1) {
            dd_currentGroup_messageboard.Style["display"] = "none";
        }
    }
}