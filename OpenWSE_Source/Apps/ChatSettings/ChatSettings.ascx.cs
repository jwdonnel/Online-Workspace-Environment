#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.IO;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_ChatSettings : UserControl {
    #region private variables

    private ServerSettings _ss = new ServerSettings();
    private readonly App _apps = new App();
    private string _ctrlname;
    private const string app_id = "app-chatsettings";
    private AppInitializer _appInitializer;

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = Page.User.Identity;
        string cl = _apps.GetAppName(app_id);
        lbl_Title.Text = cl;

        if (!_ss.HideAllAppIcons) {
            img_Title.Visible = true;
            string clImg = _apps.GetAppIconName(app_id);
            img_Title.ImageUrl = "~/Standard_Images/App_Icons/" + clImg;
        }
        else
            img_Title.Visible = false;

        _appInitializer = new AppInitializer(app_id, userId.Name, Page);
        _appInitializer.LoadScripts_JS(true, "GetChatTimeout()");
        LoadMessages(ref GV_Messages_chatlog, "0", "desc");
        LoadChatSettings(userId.Name);
    }

    private void LoadChatSettings(string username) {
        MemberDatabase member = new MemberDatabase(username);
        if (member.ChatSoundNoti)
            rb_chatsoundnoti_on.Checked = true;

        if ((!member.ChatEnabled) || (!_ss.ChatEnabled)) {
            pnl_chatsettings_options.Enabled = false;
            pnl_chatsettings_options.Visible = false;
        }

        emoticons_log_chatsettings.Controls.Clear();
        StringBuilder icons = new StringBuilder();

        string serverPath = ServerSettings.GetServerMapLocation + "ChatClient\\Emoticons";
        if (Directory.Exists(serverPath)) {
            string[] files = Directory.GetFiles(serverPath);
            Array.Sort(files);
            for (int i = 0; i < files.Length; i++) {
                FileInfo fi = new FileInfo(files[i]);
                if (fi.Name.ToLower() != "thumbs.db") {
                    icons.Append("<img alt='{" + i + "}' src='ChatClient/Emoticons/" + fi.Name + "' />");
                }
            }
        }

        emoticons_log_chatsettings.Controls.Add(new LiteralControl(icons.ToString()));
    }

    #region GridView Properties Methods

    protected void GV_Messages_RowCreated(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.Pager) {
            var pnlPager = (Panel)e.Row.FindControl("PnlPager_mymessages");
            for (int i = 0; i < GV_Messages_chatlog.PageCount; i++) {
                if (i % 10 == 0) {
                    pnlPager.Controls.Add(new LiteralControl("<div class=\"clear\" style=\"height: 10px;\"></div>"));
                }
                var lbtnPage = new LinkButton {
                    CommandArgument = i.ToString(CultureInfo.InvariantCulture),
                    CommandName = "PageNo",
                    CssClass =
                        GV_Messages_chatlog.PageIndex == i
                            ? "GVPagerNumActive RandomActionBtns"
                            : "GVPagerNum RandomActionBtns",
                    Text = (i + 1).ToString(CultureInfo.InvariantCulture)
                };
                pnlPager.Controls.Add(lbtnPage);
            }
        }
    }

    public void LoadMessages(ref GridView gv, string sortExp, string sortDir) {
        DataView dvMessages = GetMessages();
        if (dvMessages.Count > 0)
            if (sortExp != string.Empty)
                dvMessages.Sort = string.Format("{0} {1}", dvMessages.Table.Columns[Convert.ToInt32(sortExp)], sortDir);
        gv.DataSource = dvMessages;
        gv.DataBind();
    }

    public DataView GetMessages() {
        string u = HttpContext.Current.User.Identity.Name.ToLower();
        var dtMessages = new DataTable();
        dtMessages.Columns.Add(new DataColumn("Date"));
        dtMessages.Columns.Add(new DataColumn("Subject"));
        dtMessages.Columns.Add(new DataColumn("ID"));
        dtMessages.Columns.Add(new DataColumn("isRead"));

        var c = new Chat(false);
        ChatLogsDeleted cld = new ChatLogsDeleted(u);
        cld.BuildLog();
        List<ChatColl> rc = c.GetAllChats;
        var datesAlreadyIn = new List<string>();
        foreach (ChatColl x in rc) {
            if ((x.TUser == u) || (x.SUser == u)) {
                bool canContinue = true;
                foreach (ChatLogsDeletedColl cldColl in cld.ChatLogs) {
                    if (cldColl.Date.ToShortDateString() == x.Date.ToShortDateString()) {
                        canContinue = false;
                        break;
                    }
                }

                if (x.Date.ToShortDateString() == DateTime.Now.ToShortDateString())
                    canContinue = false;

                if (canContinue) {
                    if (!datesAlreadyIn.Contains(x.Date.ToShortDateString())) {
                        datesAlreadyIn.Add(x.Date.ToShortDateString());
                        dtMessages.Rows.Add(AssignRows(x, dtMessages));
                    }
                }
            }
        }

        var dvMessages = new DataView(dtMessages);
        return dvMessages;
    }

    private DataRow AssignRows(ChatColl x, DataTable dtMessages) {
        DataRow drMessages = dtMessages.NewRow();
        if (x.TUser == HttpContext.Current.User.Identity.Name.ToLower())
            drMessages["Subject"] =
                ShortenSubject("Conversation with " + HelperMethods.MergeFMLNames(new MemberDatabase(x.SUser)));
        else
            drMessages["Subject"] =
                ShortenSubject("Conversation with " + HelperMethods.MergeFMLNames(new MemberDatabase(x.TUser)));

        drMessages["Date"] = x.Date.ToShortDateString();
        drMessages["ID"] = x.ID;
        drMessages["isRead"] = "";
        return drMessages;
    }

    public string ShortenSubject(string subject) {
        string subjectTemp = string.Empty;
        for (var i = 0; i < subject.Length; i++) {
            if (i >= 35) {
                subjectTemp += " ..";
                break;
            }
            subjectTemp += subject[i];
        }
        return subjectTemp;
    }

    #endregion

}