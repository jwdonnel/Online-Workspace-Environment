using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Web.Security;
using System.Collections.Specialized;
using System.Text;
using TextEditor;

public partial class SiteTools_EmailSettings : System.Web.UI.Page {
    #region Variables
    private bool newUserMode = true;
    private NewUserDefaults defaults;
    private Dictionary<string, string> _drDefaults;
    private string _roleSelect = ServerSettings.AdminUserName;
    private ServerSettings _ss = new ServerSettings();
    private AppLog _applog;
    private IIdentity _userId;
    private string _username;
    private string _ctrlname;
    #endregion

    #region Variables for current Users only
    private MemberDatabase _member;
    private MembershipUser _membershipuser;
    private App _apps;
    private string _sitetheme = "Standard";
    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userID = HttpContext.Current.User.Identity;

        PageLoadInit pageLoadInit = new PageLoadInit(this.Page, userID, IsPostBack, _ss.NoLoginRequired);
        if (pageLoadInit.CanLoadPage) {
            ScriptManager sm = ScriptManager.GetCurrent(Page);
            if (sm != null) {
                string ctlId = sm.AsyncPostBackSourceElementID;
                _ctrlname = ctlId;
            }
            StartUpPage(userID);
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }
    private void LoadUserBackground() {
        string str = AcctSettings.LoadUserBackground(_username, _sitetheme, this.Page);
        if (!string.IsNullOrEmpty(str)) {
            RegisterPostbackScripts.RegisterStartupScript(this, str);
        }
    }

    private void StartUpPage(IIdentity userId) {
        _userId = userId;
        _username = userId.Name;
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            _member = new MemberDatabase(_username);
            if (!IsPostBack) {
                LoadUserBackground();
                GetStartupScripts_JS();
                HeaderChecked();
            }
        }
        else {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }
    }

    protected void lbtn_ClearHeader_Checked(object sender, EventArgs e) {
        ServerSettings.update_EmailHeader(string.Empty);
        HeaderChecked();
    }

    protected void lbtn_ClearFooter_Checked(object sender, EventArgs e) {
        ServerSettings.update_EmailFooter(string.Empty);
        FooterChecked();
    }

    protected void hf_UpdateHeader_Changed(object sender, EventArgs e) {
        ServerSettings.update_EmailHeader(HttpUtility.UrlDecode(hf_UpdateHeader.Value));
        HeaderChecked();
        hf_UpdateHeader.Value = string.Empty;
    }

    protected void hf_UpdateFooter_Changed(object sender, EventArgs e) {
        ServerSettings.update_EmailFooter(HttpUtility.UrlDecode(hf_UpdateFooter.Value));
        FooterChecked();
        hf_UpdateFooter.Value = string.Empty;
    }


    #region Buttons

    protected void lbtn_header_Checked(object sender, EventArgs e) {
        HeaderChecked();
    }

    protected void lbtn_footer_Checked(object sender, EventArgs e) {
        FooterChecked();
    }

    private void HeaderChecked() {
        var str = new StringBuilder();
        str.Append("$('#hdl2').removeClass('active');$('#hdl1').addClass('active');");
        str.Append("UnescapeCode(" + HttpUtility.JavaScriptStringEncode(ServerSettings.EmailHeader, true) + ");");
        str.Append("$('#btn1').val('Update Header');$('#lbtn_ClearFooter').hide();$('#lbtn_ClearHeader').show();");
        RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
    }

    private void FooterChecked() {
        var str = new StringBuilder();
        str.Append("$('#hdl1').removeClass('active');$('#hdl2').addClass('active');");
        str.Append("UnescapeCode(" + HttpUtility.JavaScriptStringEncode(ServerSettings.EmailFooter, true) + ");");
        str.Append("$('#btn1').val('Update Footer');$('#lbtn_ClearHeader').hide();$('#lbtn_ClearFooter').show();");
        RegisterPostbackScripts.RegisterStartupScript(this, str.ToString());
    }

    #endregion


    #region Dynamically Load Scripts

    private void GetStartupScripts_JS() {
        var startupscripts = new StartupScripts(true);
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList) {
            if (coll.ApplyTo == "All Components") {
                var sref = new ScriptReference(coll.ScriptPath);
                if (sm != null)
                    sm.Scripts.Add(sref);
            }
        }
        if (sm != null) sm.ScriptMode = ScriptMode.Release;
    }

    #endregion

}