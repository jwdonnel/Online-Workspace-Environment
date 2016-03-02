using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE.Core.Licensing;
using OpenWSE_Tools.AutoUpdates;
using SocialSignInApis;

public partial class GroupLogin : System.Web.UI.Page {

    private MemberDatabase _member;
    private string _username;
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();

    protected void Page_Load(object sender, EventArgs e) {
        if (!IsPostBack && !ServerSettings.CheckWebConfigFile()) {
            return;
        }

        // Check to see if social Login is valid
        SocialSignIn.CheckSocialSignIn();

        IIdentity userId = HttpContext.Current.User.Identity;

        if (!userId.IsAuthenticated) {
            Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else if (userId.Name.ToLower() == ServerSettings.AdminUserName.ToLower()) {
            Response.Redirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
        }

        PageLoadInit pageLoadInit = new PageLoadInit(this.Page, userId, IsPostBack, _ss.NoLoginRequired);
        pageLoadInit.CheckSSLRedirect();
        _username = userId.Name;
        _member = new MemberDatabase(_username);

        if (_member.GroupList.Count == 1) {
            GroupSessions.AddOrSetNewGroupLoginSession(_username, _member.GroupList[0]);
            Response.Redirect("~/Workspace.aspx");
        }

        CustomFonts.SetCustomValues(this.Page, _member);

        lbl_UserName.Text = UserImageColorCreator.CreateImgColorTopBar(_member.AccountImage, _member.UserColor, _member.UserId, HelperMethods.MergeFMLNames(_member), _member.SiteTheme, _member.ProfileLinkStyle);
        UserImageColorCreator.ApplyProfileLinkStyle(_member.ProfileLinkStyle, _member.UserColor, this.Page);
        ServerSettings.AddMetaTagsToPage(this.Page);
    }
    protected void Page_PreInit(object sender, EventArgs e) {
        string userRole = _ss.UserSignUpRole;
        if (!string.IsNullOrEmpty(Request.QueryString["group"])) {
            userRole = Request.QueryString["group"];
            if (userRole.Contains("#")) {
                userRole = userRole.Replace(userRole.Substring(userRole.IndexOf("#")), "");
            }
        }

        string theme = new NewUserDefaults(userRole).GetDefault("Theme");
        if (string.IsNullOrEmpty(theme)) {
            theme = "Standard";
        }

        Page.Theme = theme;
    }

    protected void lbtn_Logoff_Click(object sender, EventArgs e) {
        if (_member != null) {
            Thread.Sleep(500);
            if (_member.ClearPropOnSignOff) {
                var app = new App(_username);
                app.DeleteUserProperties(_username);

                HttpCookieCollection cookieColl = new HttpCookieCollection();
                foreach (object key in Request.Cookies.Keys) {
                    HttpCookie cookie = Request.Cookies[key.ToString()];
                    if (cookie != null) {
                        cookie.Expires = ServerSettings.ServerDateTime.AddDays(-1d);
                        cookieColl.Add(cookie);
                    }
                }

                foreach (object c in cookieColl.Keys) {
                    HttpCookie cookie = Request.Cookies[c.ToString()];
                    Response.Cookies.Add(cookie);
                }
            }

            int hour = ServerSettings.ServerDateTime.Hour;
            int min = ServerSettings.ServerDateTime.Minute;
            int seconds = ServerSettings.ServerDateTime.Second;

            if (min >= 20)
                min = min - 20;
            else {
                int tempmin = min - 20;
                min = 60 + tempmin;
                if (hour > 1)
                    hour = hour - 1;
                else
                    hour = 24 - hour - 1;
            }
            int year = ServerSettings.ServerDateTime.Year;
            int month = ServerSettings.ServerDateTime.Month;
            int day = ServerSettings.ServerDateTime.Day - 1;
            if (ServerSettings.ServerDateTime.Day <= 1) {
                month = ServerSettings.ServerDateTime.Month - 1;
                if (month <= 1) {
                    year = ServerSettings.ServerDateTime.Year - 1;
                    month = 1;
                }
                day = 28;
            }

            try {
                var newDate = new DateTime(ServerSettings.ServerDateTime.Year, month, day, hour, min, seconds);
                MembershipUser _membershipuser = Membership.GetUser(_username);
                if (_membershipuser != null) {
                    _membershipuser.LastActivityDate = newDate;
                    if (!string.IsNullOrEmpty(_membershipuser.Email))
                        Membership.UpdateUser(_membershipuser);
                }
            }
            catch { }

            _member.UpdateChatTimeStamp();
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser msu in coll) {
                string un = msu.UserName.ToLower();
                if ((msu.IsOnline) && (un != _username.ToLower())) {
                    if (ChatService._needRefreshUsers.ContainsKey(un))
                        ChatService._needRefreshUsers[un] = true;
                    else
                        ChatService._needRefreshUsers.Add(un, true);

                    if (ChatService._needRefreshMessager.ContainsKey(un))
                        ChatService._needRefreshMessager[un] = true;
                    else
                        ChatService._needRefreshMessager.Add(un, true);
                }
            }

            MemberDatabase.DeleteUserSessionId(_username);

            FormsAuthentication.SignOut();
            if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                string groupName = GroupSessions.GetUserGroupSessionName(_username);
                GroupSessions.RemoveGroupLoginSession(_username);
                if (!string.IsNullOrEmpty(groupName)) {
                    Response.Redirect("~/" + ServerSettings.DefaultStartupPage + "?group=" + groupName);
                }
            }

            LoginActivity la = new LoginActivity();
            la.AddItem(_username, true, ActivityType.Logout);

            FormsAuthentication.RedirectToLoginPage();
        }
    }
}