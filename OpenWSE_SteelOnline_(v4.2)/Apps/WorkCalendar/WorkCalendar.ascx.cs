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

public partial class Apps_WorkCalendar_WorkCalendar : System.Web.UI.UserControl
{
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly App _apps = new App();
    private string _ctrlname;
    private MemberDatabase _member;
    private AppInitializer _appInitializer;
    private const string app_id = "app-workcalendar";

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

        if (Request.QueryString["print"] == "true") {
            RegisterPostbackScripts.RegisterStartupScript(this, "$.getScript('" + ResolveUrl("~/Scripts/jquery/html2canvas.js") + "');");
        }
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

        if (!LoadAppParams(Page.User.Identity.Name))
        {
            menubtn_employees.Visible = false;
            menubtn_reasons.Visible = false;
            menubtn_requests.Visible = false;
        }
        LoadGroups();
    }

    private void LoadGroups()
    {
        Groups _groups = new Groups();
        List<string> groupList = _member.GroupList;
        foreach (string group in groupList)
        {
            string groupName = _groups.GetGroupName_byID(group);
            var item2 = new ListItem(groupName, group);
            if (group_select_wc.Items.Contains(item2))
                continue;
            group_select_wc.Items.Add(item2);
        }
    }

    private bool LoadAppParams(string currUser)
    {
        AppParams appParams = new AppParams(false);
        appParams.GetAllParameters_ForApp(app_id);
        foreach (Dictionary<string, string> dr in appParams.listdt)
        {
            try
            {
                string param = dr["Parameter"];
                int indexOf = param.IndexOf("=") + 1;
                string subParam = param.Substring(indexOf);
                string[] splitUsers = subParam.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string user in splitUsers)
                {
                    if (currUser.ToLower() == user.ToLower())
                        return true;
                }
            }
            catch { }
        }

        return false;
    }
}