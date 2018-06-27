#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.Overlays;

#endregion

/// <summary>
///     Add/Retrieve user information from the database (Apart of MembershipUser)
/// </summary>
public class MemberDatabase {

    #region Variables

    private static Dictionary<string, List<string>> UserSessionIds = new Dictionary<string, List<string>>();
    private static Dictionary<string, string> UserUnloadSessionsIds = new Dictionary<string, string>();
    private static Dictionary<string, Dictionary<string, string>> UsersTable = new Dictionary<string, Dictionary<string, string>>();

    private Dictionary<string, string> GroupDefaults = new Dictionary<string, string>();

    private DatabaseCall dbCall = new DatabaseCall();
    private string _username;

    private string _membershipTableName = "Memberships";
    private string _usersTableName = "Users";
    private string _userCustomizeTable = "aspnet_UserCustomization";

    public const string BackgroundSeperator = "|";

    #endregion


    #region Initialize and Backups

    public MemberDatabase(string u) {
        SetTableNames();
        _username = u.ToLower();
        SetGroupLoginID();
    }
    public MemberDatabase() {
        SetTableNames();
    }

    public string Username {
        get { return _username; }
        set { _username = value; }
    }
    private void SetTableNames() {
        if (dbCall.DataProvider == "System.Data.SqlClient") {
            _membershipTableName = "aspnet_Membership";
            _usersTableName = "aspnet_Users";
        }
    }

    public void AddUserCustomizationRow(string username) {
        string[] roles = Roles.GetRolesForUser(username);
        if (roles.Length == 0) {
            Roles.AddUserToRole(username, "Standard");
        }

        roles = Roles.GetRolesForUser(username);
        if (roles.Length == 0) {
            return;
        }

        NewUserDefaults defaults = new NewUserDefaults(roles[0]);
        defaults.GetDefaults();
        Dictionary<string, string> _drDefaults = defaults.DefaultTable;
        if (_drDefaults != null && _drDefaults.Count > 0 && !string.IsNullOrEmpty(UserId)) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UserId));
            query.Add(new DatabaseQuery("UserName", username.ToLower()));
            query.Add(new DatabaseQuery("WorkspaceRotate", _drDefaults["WorkspaceRotate"]));
            query.Add(new DatabaseQuery("WorkspaceRotateInterval", _drDefaults["WorkspaceRotateInterval"]));
            query.Add(new DatabaseQuery("WorkspaceRotateScreens", _drDefaults["WorkspaceRotateScreens"]));
            query.Add(new DatabaseQuery("RotateAutoRefresh", _drDefaults["RotateAutoRefresh"]));
            query.Add(new DatabaseQuery("BackgroundImgs", _drDefaults["BackgroundImgs"]));
            query.Add(new DatabaseQuery("EnableBackgrounds", _drDefaults["EnableBackgrounds"]));
            query.Add(new DatabaseQuery("ShowAppTitle", _drDefaults["ShowAppTitle"]));
            query.Add(new DatabaseQuery("AppHeaderIcon", _drDefaults["AppHeaderIcon"]));
            query.Add(new DatabaseQuery("TaskBarShowAll", _drDefaults["TaskBarShowAll"]));
            query.Add(new DatabaseQuery("ShowWorkspaceNumApp", _drDefaults["ShowWorkspaceNumApp"]));
            query.Add(new DatabaseQuery("PrivateAccount", "1"));
            query.Add(new DatabaseQuery("AnimationSpeed", "150"));
            query.Add(new DatabaseQuery("ShowMinimizedPreview", _drDefaults["ShowMinimizedPreview"]));
            query.Add(new DatabaseQuery("ShowToolTips", _drDefaults["ToolTips"]));
            query.Add(new DatabaseQuery("AppContainer", _drDefaults["AppContainer"]));
            query.Add(new DatabaseQuery("ClearPropOnSignOff", _drDefaults["ClearPropOnSignOff"]));
            query.Add(new DatabaseQuery("ShowDateTime", _drDefaults["ShowDateTime"]));
            query.Add(new DatabaseQuery("LockAppIcons", _drDefaults["LockAppIcons"]));
            query.Add(new DatabaseQuery("CurrentWorkspace", "1"));
            query.Add(new DatabaseQuery("AppSnapToGrid", _drDefaults["SnapToGrid"]));
            query.Add(new DatabaseQuery("AppGridSize", _drDefaults["AppGridSize"]));
            query.Add(new DatabaseQuery("GroupIcons", _drDefaults["GroupIcons"]));
            query.Add(new DatabaseQuery("ShowCategoryCount", _drDefaults["IconCategoryCount"]));
            query.Add(new DatabaseQuery("LoadLinksBlankPage", _drDefaults["LoadLinksBlankPage"]));
            query.Add(new DatabaseQuery("VacationTime", "0"));
            query.Add(new DatabaseQuery("AcctImage", ""));
            query.Add(new DatabaseQuery("TotalWorkspaces", _drDefaults["TotalWorkspaces"]));
            query.Add(new DatabaseQuery("HoverPreviewWorkspace", _drDefaults["HoverPreviewWorkspace"]));
            query.Add(new DatabaseQuery("IsSocialAccount", "0"));
            query.Add(new DatabaseQuery("WorkspaceMode", _drDefaults["WorkspaceMode"]));
            query.Add(new DatabaseQuery("AppSnapHelper", _drDefaults["AppSnapHelper"]));
            query.Add(new DatabaseQuery("UserAppStyle", _drDefaults["UserAppStyle"]));
            query.Add(new DatabaseQuery("DefaultLoginGroup", _drDefaults["DefaultLoginGroup"]));
            query.Add(new DatabaseQuery("MobileAutoSync", _drDefaults["MobileAutoSync"]));
            query.Add(new DatabaseQuery("BackgroundLoopTimer", _drDefaults["BackgroundLoopTimer"]));
            query.Add(new DatabaseQuery("BackgroundPosition", _drDefaults["BackgroundPosition"]));
            query.Add(new DatabaseQuery("BackgroundSize", _drDefaults["BackgroundSize"]));
            query.Add(new DatabaseQuery("BackgroundRepeat", _drDefaults["BackgroundRepeat"]));
            query.Add(new DatabaseQuery("BackgroundColor", _drDefaults["BackgroundColor"]));
            query.Add(new DatabaseQuery("SiteTipsOnPageLoad", _drDefaults["SiteTipsOnPageLoad"]));
            query.Add(new DatabaseQuery("AppSelectorStyle", _drDefaults["AppSelectorStyle"]));
            query.Add(new DatabaseQuery("ProfileLinkStyle", _drDefaults["ProfileLinkStyle"]));
            query.Add(new DatabaseQuery("HideAllOverlays", _drDefaults["HideAllOverlays"]));
            query.Add(new DatabaseQuery("DefaultBodyFontFamily", _drDefaults["DefaultBodyFontFamily"]));
            query.Add(new DatabaseQuery("DefaultBodyFontSize", _drDefaults["DefaultBodyFontSize"]));
            query.Add(new DatabaseQuery("DefaultBodyFontColor", _drDefaults["DefaultBodyFontColor"]));
            query.Add(new DatabaseQuery("ShowSiteToolsPageDescription", _drDefaults["ShowSiteToolsPageDescription"]));
            query.Add(new DatabaseQuery("ShowSiteToolsInCategories", _drDefaults["ShowSiteToolsInCategories"]));
            query.Add(new DatabaseQuery("SiteColorOption", _drDefaults["SiteColorOption"]));
            query.Add(new DatabaseQuery("SiteLayoutOption", _drDefaults["SiteLayoutOption"]));
            query.Add(new DatabaseQuery("ShowDedicatedMinimizedArea", _drDefaults["ShowDedicatedMinimizedArea"]));
            query.Add(new DatabaseQuery("AllowNavMenuCollapseToggle", _drDefaults["AllowNavMenuCollapseToggle"]));
            query.Add(new DatabaseQuery("ShowRowCountGridViewTable", _drDefaults["ShowRowCountGridViewTable"]));
            query.Add(new DatabaseQuery("UseAlternateGridviewRows", _drDefaults["UseAlternateGridviewRows"]));
            query.Add(new DatabaseQuery("SiteToolsIconOnly", _drDefaults["SiteToolsIconOnly"]));
            query.Add(new DatabaseQuery("HideSearchBarInTopBar", _drDefaults["HideSearchBarInTopBar"]));
            dbCall.CallInsert(_userCustomizeTable, query);
        }

        if (UsersTable.ContainsKey(username.ToLower())) {
            UsersTable.Remove(username.ToLower());
        }

        CheckAndCreateUserSettings();
    }

    public void ClearUserTable() {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            UsersTable.Clear();
        }
    }

    #endregion


    #region Update methods

    public static void AddUserSessionIds(string userName) {
        if (!string.IsNullOrEmpty(userName)) {
            string sessionId = GetUserSessionId(userName);

            if (UserSessionIds == null) {
                UserSessionIds = new Dictionary<string, List<string>>();
            }

            bool doesNotContainSession = true;
            if (UserSessionIds.ContainsKey(userName.ToLower())) {
                foreach (string key in UserSessionIds[userName.ToLower()]) {
                    if (key == sessionId) {
                        doesNotContainSession = false;
                        break;
                    }
                }
            }

            if (UserSessionIds.ContainsKey(userName.ToLower()) && doesNotContainSession) {
                UserSessionIds[userName.ToLower()].Add(sessionId);
            }
            else if (!UserSessionIds.ContainsKey(userName.ToLower())) {
                UserSessionIds.Add(userName.ToLower(), new List<string>() { sessionId });
            }
        }
    }

    public void UpdateUsersTable(string property, string value) {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username)) {
            UsersTable[_username][property] = value;
        }
    }
    public string FixGroupNameColumn(string id) {
        if (!string.IsNullOrEmpty(_username)) {
            string _groupNames = string.Empty;

            Groups groups = new Groups();
            groups.getEntries();
            List<string> groupList = GroupList;
            foreach (string groupName in groupList) {
                Guid tempGuid = new Guid();
                if (!Guid.TryParse(groupName, out tempGuid)) {
                    foreach (Dictionary<string, string> x in groups.group_dt) {
                        if (x["GroupName"] == id) {
                            id = x["GroupID"];
                        }

                        if (x["GroupName"] == groupName) {
                            _groupNames += x["GroupID"] + ServerSettings.StringDelimiter.ToString();
                            continue;
                        } 
                    }
                }
                else {
                    _groupNames += groupName + ServerSettings.StringDelimiter.ToString();
                }
            }

            if (!string.IsNullOrEmpty(_groupNames)) {
                UpdateGroupName(_groupNames);
            }
        }

        return id;
    }
    public void UpdateFirstName(string firstname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("FirstName", firstname));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("FirstName", firstname);
        }
    }
    public void UpdateLastName(string lastname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("LastName", lastname));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("LastName", lastname);
        }
    }
    public void UpdateSignature(string signature) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Signature", signature));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("Signature", signature);
        }
    }
    public void UpdateTheme(string theme) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Theme", theme));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("Theme", theme);
        }
    }
    public void UpdateColor(string color) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        if (!color.StartsWith("#")) {
            color = "#" + color;
        }

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("UserColor", color));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("UserColor", color);
        }
    }
    public void UpdateChatsMinimized(string chatsminimized) {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("ChatsMinimized", chatsminimized));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("ChatsMinimized", chatsminimized);
            }
        }
    }
    public void UpdateChatsOpened(string chatsopened) {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("ChatsOpened", chatsopened));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("ChatsOpened", chatsopened);
            }
        }
    }
    public void UpdateActivationCode(string code) {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("ActivationCode", code));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("ActivationCode", code);
            }
        }
    }
    public void UpdateAdminPages(string page, bool remove) {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            var templist = AdminPagesNonlist;
            if (remove) {
                templist = AdminPagesList.Where(un => un.ToLower() != page.ToLower())
                                        .Aggregate(string.Empty, (current, un) => current + (un + ServerSettings.StringDelimiter));
            }
            else {
                bool cancontinue = AdminPagesList.All(un => un.ToLower() != page.ToLower());
                if (cancontinue)
                    templist += page + ServerSettings.StringDelimiter;
            }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("AdminPages", templist));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("AdminPages", templist);
            }
        }
    }
    public void UpdateEnableChat(bool enablechat) {
        if ((Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName)) &&
            (!(Roles.IsUserInRole(_username, ServerSettings.AdminUserName))))
            UpdateAdminChatControlled(!enablechat);
        else
            UpdateAdminChatControlled(false);

        string temp = "0";
        if (enablechat)
            temp = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("EnableChat", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("EnableChat", temp);
        }
    }
    public void UpdateEmailUponLogin(string user, bool remove) {
        string templist = EmailUponLoginNonlist;
        if (remove) {
            templist = EmailUponLoginList.Where(un => un.ToLower() != user.ToLower())
                                        .Aggregate(string.Empty, (current, un) => current + (un + ServerSettings.StringDelimiter));
        }
        else {
            bool cancontinue = EmailUponLoginList.All(un => un.ToLower() != user.ToLower());
            if (cancontinue)
                templist += user + ServerSettings.StringDelimiter;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("EmailUponLogin", templist));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("EmailUponLogin", templist);
        }
    }
    public void UpdateCommentDate(string date) {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("CommentDate", date));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("CommentDate", date);
            }
        }
    }
    public void UpdateIsNewMember(bool isNew) {
        string temp = "0";
        if (isNew)
            temp = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("IsNewMember", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("IsNewMember", temp);
        }
    }
    public void UpdateSiteTipsOnPageLoad(bool showTips) {
        string temp = "0";
        if (showTips)
            temp = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("SiteTipsOnPageLoad", temp));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("SiteTipsOnPageLoad", temp);
        }
    }
    public void UpdateAppSnapHelper(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppSnapHelper", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AppSnapHelper", temp.ToString());
        }
    }
    public void UpdateChatSoundNoti(bool allow) {
        string temp = "0";
        if (allow)
            temp = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatSoundNoti", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatSoundNoti", temp);
        }
    }
    public void UpdateAdminChatControlled(bool controlled) {
        string temp = "0";
        if (controlled)
            temp = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AdminChatControlled", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("AdminChatControlled", temp);
        }
    }
    public void UpdateAppRemoteIDAndOptions(string app_id, string options) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppRemoteID", app_id));
        updateQuery.Add(new DatabaseQuery("AppRemoteOptions", options));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("AppRemoteID", app_id);
            UpdateUsersTable("AppRemoteOptions", options);
        }
    }
    public void ReorderAppList(string applist) {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("EnabledApps", applist));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("EnabledApps", applist);
            }
        }
    }
    public void UpdateEnabledApps(string app) {
        if (app.Length > 0 && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            if (app[app.Length - 1] == ',')
                app = app.Remove(app.Length - 1);

            string applist = UsersTable[_username]["EnabledApps"];
            if (applist.ToLower() == "none")
                applist = string.Empty;

            applist += (app + ",");

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("EnabledApps", applist));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("EnabledApps", applist);
            }
        }
    }
    public void UpdateEnabledApps_NewUser(string list) {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("EnabledApps", list));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("EnabledApps", list);
            }
        }
    }
    public void RemoveAllEnabledApps() {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("EnabledApps", string.Empty));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("EnabledApps", string.Empty);
            }
        }
    }
    public void RemoveEnabledApp(string app) {
        if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
            string applist = string.Empty;
            List<string> strApps = EnabledApps;
            applist = strApps.Where(w => !string.IsNullOrEmpty(w))
                                   .Where(w => w != app)
                                   .Aggregate(applist, (current, w) => current + (w + ","));

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("EnabledApps", applist));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("EnabledApps", applist);
            }
        }
    }
    public void UpdateChatStatus(string status) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatStatus", status));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatStatus", status);
        }
    }
    public void UpdateGroupName(string group) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("GroupName", group));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("GroupName", group);
        }
    }
    public void UpdateChatStatusMessage(string message) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatStatusMessage", message));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatStatusMessage", message);
        }
    }
    public void UpdateUserIpAddress(string ip) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("IPAddress", ip));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("IPAddress", ip);
        }
    }
    public void UpdateIsTyping(bool istyping) {
        if (CheckAndCreateUserSettings()) {
            if (!UsersTable[_username].ContainsKey("IsTypingChat")) {
                UsersTable[_username].Add("IsTypingChat", istyping.ToString().ToLower());
            }
            else {
                UsersTable[_username]["IsTypingChat"] = istyping.ToString().ToLower();
            }
        }
    }
    public void UpdateStatusChanged(bool changed) {
        string temp = "0";
        if (changed)
            temp = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("StatusChanged", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("StatusChanged", temp);
        }
    }
    public void UpdateIsAway(bool away) {
        string temp = "0";
        if (away)
            temp = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("IsAway", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("IsAway", temp);
        }
    }
    public void UpdateChatTimeStamp() {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        string time = ServerSettings.ServerDateTime.ToString();
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatTimeStamp", time));

        var needToLog = dbCall.NeedToLog;
        dbCall.NeedToLog = false;
        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatTimeStamp", time);
        }

        dbCall.NeedToLog = needToLog;
    }

    // UserCustomize Methods Below
    public void UpdateShowMinimizedPreview(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowMinimizedPreview", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowMinimizedPreview", temp.ToString());
        }
    }
    public void UpdateHoverPreviewWorkspace(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("HoverPreviewWorkspace", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("HoverPreviewWorkspace", temp.ToString());
        }
    }
    public void UpdateIsSocialAccount(bool isSocial) {
        int temp = 0;
        if (isSocial) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("IsSocialAccount", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("IsSocialAccount", temp.ToString());
        }
    }
    public void UpdateAcctImage(string img) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AcctImage", img));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AcctImage", img);
        }
    }
    public void UpdateTotalWorkspaces(int count) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("TotalWorkspaces", count.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("TotalWorkspaces", count.ToString());
        }
    }
    public void UpdateAutoRotateWorkspace(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("WorkspaceRotate", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("WorkspaceRotate", temp.ToString());
        }
    }
    public void UpdateAutoRotateWorkspaceInterval(string interval) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("WorkspaceRotateInterval", interval));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("WorkspaceRotateInterval", interval);
        }
    }
    public void UpdateWorkspaceRotateScreens(string screens) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("WorkspaceRotateScreens", screens));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("WorkspaceRotateScreens", screens);
        }
    }
    public void UpdateRotateAutoRefresh(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("RotateAutoRefresh", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("RotateAutoRefresh", temp.ToString());
        }
    }
    public void UpdatePrivateAccount(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("PrivateAccount", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("PrivateAccount", temp.ToString());
        }
    }
    public void UpdateAnimationSpeed(int seconds) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AnimationSpeed", seconds.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AnimationSpeed", seconds.ToString());
        }
    }
    public void UpdateBackgroundImg(string url) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("BackgroundImgs", url));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("BackgroundImgs", url);
        }
    }
    public void UpdateBackgroundImg(string url, int workspace) {
        string newImg = string.Empty;
        Dictionary<int, string> bgs = GetBackgroundImg();
        foreach (KeyValuePair<int, string> keyPair in bgs) {
            if (workspace != keyPair.Key) {
                newImg += keyPair.Key.ToString() + ":=:" + keyPair.Value + ServerSettings.StringDelimiter;
            }
        }

        if (!string.IsNullOrEmpty(url)) {
            newImg += workspace.ToString() + ":=:" + url + ServerSettings.StringDelimiter;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("BackgroundImgs", newImg));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("BackgroundImgs", newImg);
        }
    }
    public void UpdateWorkspaceMode(string mode) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("WorkspaceMode", mode));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("WorkspaceMode", mode);
        }
    }
    public void UpdateEnableBackgrounds(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("EnableBackgrounds", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("EnableBackgrounds", temp.ToString());
        }
    }
    public void UpdateShowAppTitle(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowAppTitle", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowAppTitle", temp.ToString());
        }
    }
    public void UpdateAppHeaderIcon(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppHeaderIcon", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AppHeaderIcon", temp.ToString());
        }
    }
    public void UpdateTaskBarShowAll(bool showAll) {
        int temp = 0;
        if (showAll) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("TaskBarShowAll", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("TaskBarShowAll", temp.ToString());
        }
    }
    public void UpdateShowWorkspaceNumApp(bool hide) {
        int temp = 0;
        if (hide) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowWorkspaceNumApp", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowWorkspaceNumApp", temp.ToString());
        }
    }
    public void UpdateShowToolTip(bool show) {
        int temp = 0;
        if (show) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowToolTips", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowToolTips", temp.ToString());
        }
    }
    public void UpdateAppContainer(bool container) {
        int temp = 0;
        if (container) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppContainer", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AppContainer", temp.ToString());
        }
    }
    public void UpdateClearPropOnSignOff(bool clearprop) {
        int temp = 0;
        if (clearprop) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ClearPropOnSignOff", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ClearPropOnSignOff", temp.ToString());
        }
    }
    public void UpdateShowDateTime(bool show) {
        int temp = 0;
        if (show) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowDateTime", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowDateTime", temp.ToString());
        }
    }
    public void UpdateLockAppIcons(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("LockAppIcons", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("LockAppIcons", temp.ToString());
        }
    }
    public void UpdateCurrentWorkspace(int workspace) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("CurrentWorkspace", workspace.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("CurrentWorkspace", workspace.ToString());
        }
    }
    public void UpdateAppSnapToGrid(bool snap) {
        int temp = 0;
        if (snap) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppSnapToGrid", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AppSnapToGrid", temp.ToString());
        }
    }
    public void UpdateAppGridSize(string size) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppGridSize", size));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AppGridSize", size);
        }
    }
    public void UpdateGroupIcons(bool groupicons) {
        int temp = 0;
        if (groupicons) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("GroupIcons", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("GroupIcons", temp.ToString());
        }
    }
    public void UpdateShowCategoryCount(bool show) {
        int temp = 0;
        if (show) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowCategoryCount", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowCategoryCount", temp.ToString());
        }
    }
    public void UpdateLoadLinksBlankPage(bool blank) {
        int temp = 0;
        if (blank) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("LoadLinksBlankPage", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("LoadLinksBlankPage", temp.ToString());
        }
    }
    public void UpdateVacationTime(string time) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("VacationTime", time));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("VacationTime", time);
        }
    }
    public void UpdateUserAppStyle(string appStyle) {
        AppStyle _appStyle = AppStyle.Style_1;
        Enum.TryParse<AppStyle>(appStyle, out _appStyle);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("UserAppStyle", _appStyle.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("UserAppStyle", _appStyle.ToString());
        }
    }
    public void UpdateDefaultLoginGroup(string groupid) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DefaultLoginGroup", groupid));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("DefaultLoginGroup", groupid);
        }
    }
    public void UpdateMobileAutoSync(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("MobileAutoSync", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("MobileAutoSync", temp.ToString());
        }
    }
    public void UpdateBackgroundLoopTimer(string time) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("BackgroundLoopTimer", time));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("BackgroundLoopTimer", time);
        }
    }
    public void UpdateBackgroundPosition(string position) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("BackgroundPosition", position));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("BackgroundPosition", position);
        }
    }
    public void UpdateBackgroundSize(string size) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("BackgroundSize", size));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("BackgroundSize", size);
        }
    }
    public void UpdateBackgroundRepeat(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("BackgroundRepeat", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("BackgroundRepeat", temp.ToString());
        }
    }
    public void UpdateBackgroundColor(string color) {
        if (!color.StartsWith("#")) {
            color = "#" + color;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("BackgroundColor", color));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("BackgroundColor", color);
        }
    }
    public void UpdateAppSelectorStyle(string mode) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppSelectorStyle", mode));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AppSelectorStyle", mode);
        }
    }
    public void UpdateProfileLinkStyle(string mode) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ProfileLinkStyle", mode));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ProfileLinkStyle", mode);
        }
    }
    public void UpdateHideAllOverlays(bool hideAll) {
        int temp = 0;
        if (hideAll) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("HideAllOverlays", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("HideAllOverlays", temp.ToString());
        }
    }
    public void UpdateDefaultBodyFontFamily(string fontfamily) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DefaultBodyFontFamily", fontfamily));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("DefaultBodyFontFamily", fontfamily);
        }
    }
    public void UpdateDefaultBodyFontSize(string fontSize) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DefaultBodyFontSize", fontSize));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("DefaultBodyFontSize", fontSize);
        }
    }
    public void UpdateDefaultBodyFontColor(string fontColor) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DefaultBodyFontColor", fontColor));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("DefaultBodyFontColor", fontColor);
        }
    }
    public void UpdateShowSiteToolsPageDescription(bool showDescription) {
        int temp = 0;
        if (showDescription) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowSiteToolsPageDescription", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowSiteToolsPageDescription", temp.ToString());
        }
    }
    public void UpdateAllowNavMenuCollapseToggle(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AllowNavMenuCollapseToggle", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AllowNavMenuCollapseToggle", temp.ToString());
        }
    }
    public void UpdateShowSiteToolsInCategories(bool show) {
        int temp = 0;
        if (show) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowSiteToolsInCategories", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowSiteToolsInCategories", temp.ToString());
        }
    }
    public void UpdateSiteColorOption(string option) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();

        string tempOption = option + ServerSettings.StringDelimiter;
        string currentSelected = option.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries)[0];
        string[] splitOptions = SiteColorOption.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        foreach (string splitOption in splitOptions) {
            string splitValIndex = splitOption.Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries)[0];
            if (splitValIndex != currentSelected) {
                tempOption += splitOption + ServerSettings.StringDelimiter;
            }
        }

        updateQuery.Add(new DatabaseQuery("SiteColorOption", tempOption));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("SiteColorOption", tempOption);
        }
    }
    public void UpdateSiteLayoutOption(string option) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("SiteLayoutOption", option));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("SiteLayoutOption", option);
        }
    }
    public void UpdateShowDedicatedMinimizedArea(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowDedicatedMinimizedArea", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowDedicatedMinimizedArea", temp.ToString());
        }
    }
    public void UpdateShowRowCountGridViewTable(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowRowCountGridViewTable", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowRowCountGridViewTable", temp.ToString());
        }
    }
    public void UpdateUseAlternateGridviewRows(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("UseAlternateGridviewRows", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("UseAlternateGridviewRows", temp.ToString());
        }
    }
    public void UpdateSiteToolsIconOnly(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("SiteToolsIconOnly", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("SiteToolsIconOnly", temp.ToString());
        }
    }
    public void UpdateHideSearchBarInTopBar(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("HideSearchBarInTopBar", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("HideSearchBarInTopBar", temp.ToString());
        }
    }

    #endregion


    #region Auto Update Flags

    public void UpdateChatTimeout(string timeout) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatTimeout", timeout));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatTimeout", timeout);
        }

        var chat = new Chat(false);
        chat.Load_usertimeout_list();
    }
    public void SetChatUpdateFlag(bool update) {
        int temp = 0;
        if (update)
            temp = 1;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatUpdateFlag", temp.ToString()));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatUpdateFlag", temp.ToString());
        }
    }
    public void SetChatUpdateFlag(bool update, string user) {
        int temp = 0;
        if (update)
            temp = 1;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatUpdateFlag", temp.ToString()));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatUpdateFlag", temp.ToString());
        }
    }

    #endregion


    #region Get methods

    public static List<string> GetUserSessionIds(string userName) {
        if (!string.IsNullOrEmpty(userName) && UserSessionIds.ContainsKey(userName.ToLower())) {
            return UserSessionIds[userName.ToLower()];
        }

        // Add at least one empty session just in case session states were reset
        List<string> sessions = new List<string>();
        sessions.Add(string.Empty);
        return sessions;
    }
    public static string GetUserSessionId(string userName) {
        try {
            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Cookies != null) {
                HttpCookie cookie = HttpContext.Current.Request.Cookies.Get("ASP.NET_SessionId");
                if (cookie != null) {
                    return cookie.Value;
                }
            }
        }
        catch {
            // Do Nothing
        }

        return string.Empty;
    }

    public static void SetUserUnloadSessionId(string userName) {
        if (UserUnloadSessionsIds == null) {
            UserUnloadSessionsIds = new Dictionary<string, string>();
        }

        string userSessionId = GetUserSessionId(userName);
        if (!string.IsNullOrEmpty(userSessionId)) {
            if (UserUnloadSessionsIds.ContainsKey(userName.ToLower())) {
                UserUnloadSessionsIds[userName.ToLower()] = userSessionId;
            }
            else {
                UserUnloadSessionsIds.Add(userName.ToLower(), userSessionId);
            }
        }
    }
    public static string GetUserUnloadSessionId(string userName) {
        try {
            if (UserUnloadSessionsIds.ContainsKey(userName.ToLower())) {
                return UserUnloadSessionsIds[userName.ToLower()];
            }
        }
        catch { }

        return string.Empty;
    }
    public static void DeleteUserUnloadSessionId(string userName) {
        if (UserUnloadSessionsIds.ContainsKey(userName.ToLower())) {
            UserUnloadSessionsIds.Remove(userName.ToLower());
        }
    }

    public bool DoesHaveGroupDefaults {
        get {
            if (GroupDefaults.Count > 0) {
                return true;
            }
            return false;
        }
    }
    private void SetGroupLoginID() {
        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
            try {
                string sessionGroup = GroupSessions.GetUserGroupSessionName(_username);
                Groups tempGroup = new Groups();
                tempGroup.getEntries(sessionGroup);
                if (tempGroup.group_dt.Count > 0) {
                    NewUserDefaults defaults = new NewUserDefaults(sessionGroup);
                    defaults.GetDefaults();
                    GroupDefaults = defaults.DefaultTable;
                }
            }
            catch { }
        }
    }

    public static List<string> GetListOfAvailableRoles() {
        return Roles.GetAllRoles().ToList();
    }
    private bool CheckAndCreateUserSettings() {
        if (string.IsNullOrEmpty(_username)) {
            return false;
        }

        if (!UsersTable.ContainsKey(_username) && !string.IsNullOrEmpty(_username)) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(_usersTableName, "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            if (dbSelect.Count > 0) {
                try {
                    if (!UsersTable.ContainsKey(_username) && !string.IsNullOrEmpty(_username)) {
                        UsersTable.Add(_username, new Dictionary<string, string>());

                        AddUserSettingToTable(dbSelect);

                        dbSelect = dbCall.CallSelect(_userCustomizeTable, "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
                        AddUserSettingToTable(dbSelect);

                        if (!string.IsNullOrEmpty(UsersTable[_username]["UserId"])) {
                            dbSelect = dbCall.CallSelect(_membershipTableName, "Email, CommentDate, EnabledApps, AdminPages, ActivationCode", new List<DatabaseQuery>() { new DatabaseQuery("UserId", UsersTable[_username]["UserId"]), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
                            AddUserSettingToTable(dbSelect);
                        }
                    }
                }
                catch (Exception e) {
                    AppLog.AddError(e);
                }
            }
            else {
                return false;
            }
        }

        return true;
    }
    private void AddUserSettingToTable(List<Dictionary<string, string>> dbSelect) {
        foreach (Dictionary<string, string> entries in dbSelect) {
            foreach (KeyValuePair<string, string> keyValPair in entries) {
                if (!UsersTable[_username].ContainsKey(keyValPair.Key)) {
                    UsersTable[_username].Add(keyValPair.Key, keyValPair.Value);
                }
            }
        }
    }
    public string UserId {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("UserId")) {
                return UsersTable[_username]["UserId"];
            }

            return string.Empty;
        }
    }
    public string FirstName {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("FirstName")) {
                string firstName = UsersTable[_username]["FirstName"];
                return string.IsNullOrEmpty(firstName) ? "N/A" : firstName;
            }

            return string.Empty;
        }
    }
    public string LastName {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("LastName")) {
                string lastName = UsersTable[_username]["LastName"];
                return string.IsNullOrEmpty(lastName) ? "N/A" : lastName;
            }

            return string.Empty;
        }
    }
    public string Signature {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("Signature")) {
                return UsersTable[_username]["Signature"];
            }

            return string.Empty;
        }
    }
    public string UserColor {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("UserColor")) {
                string userColor = UsersTable[_username]["UserColor"];
                if (!string.IsNullOrEmpty(userColor) && !userColor.StartsWith("#")) {
                    userColor = "#" + userColor;
                }

                return userColor;
            }

            return string.Empty;
        }
    }
    public string ChatsMinimized {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ChatsMinimized")) {
                return UsersTable[_username]["ChatsMinimized"];
            }

            return string.Empty;
        }
    }
    public string ChatsOpened {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ChatsOpened")) {
                return UsersTable[_username]["ChatsOpened"];
            }

            return string.Empty;
        }
    }
    public string ActivationCode {
        get {
            if (CheckAndCreateUserSettings() && UsersTable.ContainsKey(_username) && UsersTable[_username].ContainsKey("ActivationCode")) {
                if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
                    string activationCode = UsersTable[_username]["ActivationCode"];
                    return string.IsNullOrEmpty(activationCode) ? "N/A" : activationCode;
                }
                else {
                    return string.Empty;
                }
            }

            return string.Empty;
        }
    }
    public bool AdminChatControlled {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AdminChatControlled")) {
                string val = UsersTable[_username]["AdminChatControlled"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool AppSnapHelper {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AppSnapHelper")) {
                string val = UsersTable[_username]["AppSnapHelper"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("AppSnapHelper")) {
                    val = GroupDefaults["AppSnapHelper"];
                }
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public List<string> EnabledApps {
        get {
            var temp = new List<string>();

            if (CheckAndCreateUserSettings()) {
                if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
                    if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("AppPackage")) {
                        AppPackages packages = new AppPackages(false);
                        temp = packages.GetAppList(GroupDefaults["AppPackage"]).ToList();
                    }
                    else {
                        if (UsersTable[_username].ContainsKey("EnabledApps")) {
                            string val = UsersTable[_username]["EnabledApps"];
                            string[] ew = val.Trim().Split(',');
                            temp.AddRange(ew.Where(t => !string.IsNullOrEmpty(t)));
                        }
                    }
                }
            }

            return temp;
        }
    }
    public string ChatStatus {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ChatStatus")) {
                string val = UsersTable[_username]["ChatStatus"];
                return string.IsNullOrEmpty(val) ? "Offline" : val;
            }

            return "Offline";
        }
    }
    public string ChatStatusMessage {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ChatStatusMessage")) {
                string val = UsersTable[_username]["ChatStatusMessage"];
                return string.IsNullOrEmpty(val) ? "Offline" : val;
            }

            return "Offline";
        }
    }
    public string IpAddress {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("IPAddress")) {
                string val = UsersTable[_username]["IPAddress"];
                return string.IsNullOrEmpty(val) ? "Available" : val;
            }

            return string.Empty;
        }
    }
    public List<string> GroupList {
        get {
            var temp = new List<string>();
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("GroupName")) {
                GroupIPListener groupIplistener = new GroupIPListener();
                string x = UsersTable[_username]["GroupName"];

                string[] xArray = x.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                    string currLoginGroup = GroupSessions.GetUserGroupSessionName(_username);
                    temp.Add(currLoginGroup);
                }
                else {
                    foreach (string gs in xArray) {
                        if (groupIplistener.CheckGroupNetwork(gs)) {
                            temp.Add(gs);
                        }
                    }
                }
            }

            return temp;
        }
    }
    public List<string> GetCompleteUserGroupList {
        get {
            var temp = new List<string>();
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("GroupName")) {
                GroupIPListener groupIplistener = new GroupIPListener();
                string x = UsersTable[_username]["GroupName"];

                string[] xArray = x.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                foreach (string gs in xArray) {
                    if (groupIplistener.CheckGroupNetwork(gs)) {
                        temp.Add(gs);
                    }
                }
            }

            return temp;
        }
    }
    public List<string> GroupListForGroupOrgPage {
        get {
            var temp = new List<string>();
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("GroupName")) {
                GroupIPListener groupIplistener = new GroupIPListener();
                string x = UsersTable[_username]["GroupName"];

                string[] xArray = x.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                Groups groups = new Groups(_username);

                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                    string currLoginGroup = GroupSessions.GetUserGroupSessionName(_username);
                    temp.Add(currLoginGroup);
                }
                else {
                    foreach (string gs in xArray) {
                        if (groupIplistener.CheckGroupNetwork(gs) || groups.GetOwner(gs).ToLower() == _username.ToLower()) {
                            temp.Add(gs);
                        }
                    }
                }
            }

            return temp;
        }
    }
    public bool IsTyping {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("IsTypingChat")) {
                return HelperMethods.ConvertBitToBoolean(UsersTable[_username]["IsTypingChat"]);
            }
            return false;
        }
    }
    public bool StatusChanged {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("StatusChanged")) {
                string val = UsersTable[_username]["StatusChanged"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool IsAway {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("IsAway")) {
                string val = UsersTable[_username]["IsAway"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public DateTime LastUpdated {
        get {
            var d = new DateTime();

            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ChatTimeStamp")) {
                string val = UsersTable[_username]["ChatTimeStamp"];

                DateTime o;
                if (!DateTime.TryParse(val, out o)) {
                    UpdateChatTimeStamp();
                    d = ServerSettings.ServerDateTime;
                }
                else
                    d = o;
            }

            return d;
        }
    }
    public bool ChatUpdateFlag {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ChatUpdateFlag")) {
                string val = UsersTable[_username]["ChatUpdateFlag"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ChatEnabled {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("EnableChat")) {
                string val = UsersTable[_username]["EnableChat"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("EnableChat")) {
                    val = GroupDefaults["EnableChat"];
                }
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool IsNewMember {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("IsNewMember")) {
                string val = UsersTable[_username]["IsNewMember"];
                return HelperMethods.ConvertBitToBoolean(val) && !string.IsNullOrEmpty(val);
            }

            return false;
        }
    }
    public int ChatTimeout {
        get {
            int timeout = 10;

            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ChatTimeout")) {
                string temp = UsersTable[_username]["ChatTimeout"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ChatTimeout")) {
                    temp = GroupDefaults["ChatTimeout"];
                }

                if (!int.TryParse(temp, out timeout)) {
                    timeout = 10;
                }
            }

            return timeout;
        }
    }
    public string[] EmailUponLoginList {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("EmailUponLogin")) {
                string val = UsersTable[_username]["EmailUponLogin"];
                string[] splitusers = val.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                return splitusers;
            }

            return new string[0];
        }
    }
    private string EmailUponLoginNonlist {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("EmailUponLogin")) {
                return UsersTable[_username]["EmailUponLogin"];
            }

            return string.Empty;
        }
    }
    public string[] AdminPagesList {
        get {
            if (CheckAndCreateUserSettings()) {
                if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0 && UsersTable[_username].ContainsKey("AdminPages")) {
                    string temp = UsersTable[_username]["AdminPages"];
                    string[] splitPages = temp.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                    Array.Sort(splitPages, StringComparer.InvariantCulture);

                    return splitPages;
                }
            }

            string[] returnval = { "" };
            return returnval;
        }
    }
    public string AdminPagesNonlist {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AdminPages")) {
                return UsersTable[_username]["AdminPages"];
            }

            return string.Empty;
        }
    }
    public string SiteTheme {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("Theme")) {
                string temp = UsersTable[_username]["Theme"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("Theme")) {
                    temp = GroupDefaults["Theme"];
                }

                if (string.IsNullOrEmpty(temp)) {
                    temp = "Standard";
                }

                return temp;
            }

            return "Standard";
        }
    }
    public bool ChatSoundNoti {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ChatSoundNoti")) {
                string temp = UsersTable[_username]["ChatSoundNoti"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ChatSoundNoti")) {
                    temp = GroupDefaults["ChatSoundNoti"];
                }

                temp = temp.ToLower();
                if ((!HelperMethods.ConvertBitToBoolean(temp)) && (!string.IsNullOrEmpty(temp)))
                    return false;
            }

            return true;
        }
    }
    public string[] AppRemoteIDAndOptions {
        get {
            string[] temp = new string[2];
            if (CheckAndCreateUserSettings() 
                && UsersTable[_username].ContainsKey("AppRemoteID") 
                && UsersTable[_username].ContainsKey("AppRemoteOptions")) {
                temp[0] = UsersTable[_username]["AppRemoteID"];
                temp[1] = UsersTable[_username]["AppRemoteOptions"];
            }
            return temp;
        }
    }
    public DateTime CommentDate() {
        string temp = ServerSettings.ServerDateTime.ToString();
        if (CheckAndCreateUserSettings() && UsersTable.ContainsKey(_username) && UsersTable[_username].ContainsKey("CommentDate")) {
            if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
                string x = UsersTable[_username]["CommentDate"];
                temp = string.IsNullOrEmpty(x) ? ServerSettings.ServerDateTime.ToString(CultureInfo.InvariantCulture) : x;
            }
        }
        return Convert.ToDateTime(temp);
    }
    public DateTime CommentDate(string user) {
        if (CheckAndCreateUserSettings() && UsersTable[user.ToLower()].ContainsKey("CommentDate")) {
            string x = UsersTable[user.ToLower()]["CommentDate"];
            string temp = string.IsNullOrEmpty(x) ? ServerSettings.ServerDateTime.ToString(CultureInfo.InvariantCulture) : x;

            return Convert.ToDateTime(temp);
        }

        return ServerSettings.ServerDateTime;
    }
    public bool UserHasApp(string app) {
        if (CheckAndCreateUserSettings()) {
            if (!string.IsNullOrEmpty(_username) && UsersTable.ContainsKey(_username) && UsersTable[_username].Count > 0) {
                bool temp = false;
                List<string> ew = EnabledApps;
                foreach (string t in ew) {
                    if (string.IsNullOrEmpty(t)) continue;
                    if (t.ToLower() != app.ToLower()) continue;
                    temp = true;
                }

                return temp;
            }
        }

        return false;
    }
    public List<string> ChattableUserList(string username) {
        var dataTable = new List<string>();
        string selectCols = string.Format("{0}.AdminChatControlled=0 AND {1}.IsLockedOut='False' AND {1}.IsApproved='True' AND UserName<>@UserName AND {0}.UserId={1}.UserId", _usersTableName, _membershipTableName);
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(_usersTableName + ", " + _membershipTableName, "UserName", selectCols, new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string tempUser = row["UserName"].ToLower();
            if (!dataTable.Contains(tempUser)) {
                MemberDatabase tempMember = new MemberDatabase(tempUser);
                if (tempMember.ChatEnabled) {
                    dataTable.Add(tempUser);
                }
            }
        }

        return dataTable;
    }
    public string GetUsernameFromUserId(string userId) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(_usersTableName, "UserName", new List<DatabaseQuery>() { new DatabaseQuery("UserId", userId), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    // UserCustomize Table below
    public enum UserWorkspaceMode { Complex, Simple };
    public static bool IsComplexWorkspaceMode(string mode) {
        if (mode.ToLower() == UserWorkspaceMode.Complex.ToString().ToLower() || string.IsNullOrEmpty(mode)) {
            return true;
        }

        return false;
    }
    public string AccountImage {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AcctImage")) {
                return HelperMethods.RemoveProtocolFromUrl(UsersTable[_username]["AcctImage"]);
            }

            return string.Empty;
        }
    }
    public bool MultipleBackgrounds {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("EnableBackgrounds")) {
                string val = UsersTable[_username]["EnableBackgrounds"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("EnableBackgrounds")) {
                    val = GroupDefaults["EnableBackgrounds"];
                }

                if ((HelperMethods.ConvertBitToBoolean(val)) || (string.IsNullOrEmpty(val))) {
                    return true;
                }
            }

            return false;
        }
    }
    public int AnimationSpeed {
        get {
            int temp = 200;
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AnimationSpeed")) {
                string val = UsersTable[_username]["AnimationSpeed"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("AnimationSpeed")) {
                    val = GroupDefaults["AnimationSpeed"];
                }

                if (!string.IsNullOrEmpty(val))
                    int.TryParse(val, out temp);

                if (temp < 0) {
                    temp = 150;
                }
            }

            return temp;
        }
    }
    public UserWorkspaceMode WorkspaceMode {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("WorkspaceMode")) {
                string val = UsersTable[_username]["WorkspaceMode"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("WorkspaceMode")) {
                    val = GroupDefaults["WorkspaceMode"];
                }

                if (string.IsNullOrEmpty(val) || val.ToLower() == UserWorkspaceMode.Complex.ToString().ToLower()) {
                    return UserWorkspaceMode.Complex;
                }
            }

            return UserWorkspaceMode.Simple;
        }
    }
    public int TotalWorkspaces {
        get {
            int temp = 4;

            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("TotalWorkspaces")) {
                string val = UsersTable[_username]["TotalWorkspaces"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("TotalWorkspaces")) {
                    val = GroupDefaults["TotalWorkspaces"];
                }

                if (!string.IsNullOrEmpty(val))
                    int.TryParse(val, out temp);

                ServerSettings _ss = new ServerSettings();
                int totalAllowed = _ss.TotalWorkspacesAllowed;
                if (temp > totalAllowed || temp == 0) {
                    temp = totalAllowed;
                }

                if (!MemberDatabase.IsComplexWorkspaceMode(WorkspaceMode.ToString())) {
                    temp = 1;
                }
            }

            return temp;
        }
    }
    public bool ShowToolTips {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowToolTips")) {
                string val = UsersTable[_username]["ShowToolTips"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ToolTips")) {
                    val = GroupDefaults["ToolTips"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool HoverPreviewWorkspace {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("HoverPreviewWorkspace")) {
                string val = UsersTable[_username]["HoverPreviewWorkspace"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("HoverPreviewWorkspace")) {
                    val = GroupDefaults["HoverPreviewWorkspace"];
                }

                return HelperMethods.ConvertBitToBoolean(val) || val == string.Empty;
            }

            return false;
        }
    }
    public bool IsSocialAccount {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("IsSocialAccount")) {
                string val = UsersTable[_username]["IsSocialAccount"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool AppContainer {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AppContainer")) {
                string val = UsersTable[_username]["AppContainer"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("AppContainer")) {
                    val = GroupDefaults["AppContainer"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ClearPropOnSignOff {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ClearPropOnSignOff")) {
                string val = UsersTable[_username]["ClearPropOnSignOff"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ClearPropOnSignOff")) {
                    val = GroupDefaults["ClearPropOnSignOff"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowDateTime {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowDateTime")) {
                string val = UsersTable[_username]["ShowDateTime"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ShowDateTime")) {
                    val = GroupDefaults["ShowDateTime"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool WorkspaceRotate {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("WorkspaceRotate")) {
                string val = UsersTable[_username]["WorkspaceRotate"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("WorkspaceRotate")) {
                    val = GroupDefaults["WorkspaceRotate"];
                }

                if (string.IsNullOrEmpty(val)) {
                    return false;
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public string WorkspaceRotateInterval {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("WorkspaceRotateInterval")) {
                string val = UsersTable[_username]["WorkspaceRotateInterval"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("WorkspaceRotateInterval")) {
                    val = GroupDefaults["WorkspaceRotateInterval"];
                }

                if (string.IsNullOrEmpty(val)) {
                    return "60";
                }

                return val;
            }

            return string.Empty;
        }
    }
    public bool LockAppIcons {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("LockAppIcons")) {
                string val = UsersTable[_username]["LockAppIcons"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("LockAppIcons")) {
                    return true;
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public int CurrentWorkspace {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("CurrentWorkspace")) {
                string val = UsersTable[_username]["CurrentWorkspace"];

                int temp = 1;
                int.TryParse(val, out temp);
                if (temp == 0) {
                    return 1;
                }

                return temp;
            }

            return 1;
        }
    }
    public bool AppSnapToGrid {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AppSnapToGrid")) {
                string val = UsersTable[_username]["AppSnapToGrid"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("SnapToGrid")) {
                    val = GroupDefaults["SnapToGrid"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public string AppGridSize {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AppGridSize")) {
                string val = UsersTable[_username]["AppGridSize"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("AppGridSize")) {
                    val = GroupDefaults["AppGridSize"];
                }

                if (string.IsNullOrEmpty(val)) {
                    return "20";
                }

                return val;
            }

            return string.Empty;
        }
    }
    public bool GroupIcons {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("GroupIcons")) {
                string val = UsersTable[_username]["GroupIcons"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("GroupIcons")) {
                    val = GroupDefaults["GroupIcons"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowCategoryCount {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowCategoryCount")) {
                string val = UsersTable[_username]["ShowCategoryCount"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("IconCategoryCount")) {
                    val = GroupDefaults["IconCategoryCount"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool LoadLinksBlankPage {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("LoadLinksBlankPage")) {
                string val = UsersTable[_username]["LoadLinksBlankPage"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("LoadLinksBlankPage")) {
                    val = GroupDefaults["LoadLinksBlankPage"];
                }

                return HelperMethods.ConvertBitToBoolean(val) && !string.IsNullOrEmpty(val);
            }

            return false;
        }
    }
    public string WorkspaceRotateScreens {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("WorkspaceRotateScreens")) {
                string val = UsersTable[_username]["WorkspaceRotateScreens"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("WorkspaceRotateScreens")) {
                    val = GroupDefaults["WorkspaceRotateScreens"];
                }

                if (string.IsNullOrEmpty(val)) {
                    return "4";
                }

                return val;
            }

            return string.Empty;
        }
    }
    public bool RotateAutoRefresh {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("RotateAutoRefresh")) {
                string val = UsersTable[_username]["RotateAutoRefresh"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("RotateAutoRefresh")) {
                    val = GroupDefaults["RotateAutoRefresh"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowAppTitle {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowAppTitle")) {
                string val = UsersTable[_username]["ShowAppTitle"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ShowAppTitle")) {
                    val = GroupDefaults["ShowAppTitle"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool PrivateAccount {
        get {
            try {
                if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("PrivateAccount")) {
                    string val = UsersTable[_username]["PrivateAccount"];
                    if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                        return false;
                    }

                    return HelperMethods.ConvertBitToBoolean(val);
                }
            }
            catch { }
            return false;
        }
    }
    public bool ShowWorkspaceNumApp {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowWorkspaceNumApp")) {
                string val = UsersTable[_username]["ShowWorkspaceNumApp"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ShowWorkspaceNumApp")) {
                    val = GroupDefaults["ShowWorkspaceNumApp"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool UserCustomizationsEnabled {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("UserId")) {
                string val = UsersTable[_username]["UserId"];
                if (string.IsNullOrEmpty(val)) {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
    public bool TaskBarShowAll {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("TaskBarShowAll")) {
                string val = UsersTable[_username]["TaskBarShowAll"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("TaskBarShowAll")) {
                    val = GroupDefaults["TaskBarShowAll"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool AppHeaderIcon {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AppHeaderIcon")) {
                string val = UsersTable[_username]["AppHeaderIcon"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("AppHeaderIcon")) {
                    val = GroupDefaults["AppHeaderIcon"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowMinimizedPreview {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowMinimizedPreview")) {
                string val = UsersTable[_username]["ShowMinimizedPreview"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ShowMinimizedPreview")) {
                    val = GroupDefaults["ShowMinimizedPreview"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public string BackgroundLoopTimer {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("BackgroundLoopTimer")) {
                string val = UsersTable[_username]["BackgroundLoopTimer"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("BackgroundLoopTimer")) {
                    val = GroupDefaults["BackgroundLoopTimer"];
                }

                if (string.IsNullOrEmpty(val)) {
                    val = "30";
                }

                int timerOut = 30;
                int.TryParse(val, out timerOut);

                if (timerOut <= 0) {
                    timerOut = 30;
                }

                return timerOut.ToString();
            }

            return "30";
        }
    }
    public string BackgroundPosition {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("BackgroundPosition")) {
                string val = UsersTable[_username]["BackgroundPosition"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("BackgroundPosition")) {
                    val = GroupDefaults["BackgroundPosition"];
                }

                if (string.IsNullOrEmpty(val)) {
                    val = "right center";
                }

                return val;
            }

            return "right center";
        }
    }
    public string BackgroundSize {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("BackgroundSize")) {
                string val = UsersTable[_username]["BackgroundSize"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("BackgroundSize")) {
                    val = GroupDefaults["BackgroundSize"];
                }

                if (string.IsNullOrEmpty(val)) {
                    val = "auto";
                }

                return val;
            }

            return "auto";
        }
    }
    public bool BackgroundRepeat {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("BackgroundRepeat")) {
                string val = UsersTable[_username]["BackgroundRepeat"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("BackgroundRepeat")) {
                    val = GroupDefaults["BackgroundRepeat"];
                }

                if (string.IsNullOrEmpty(val)) {
                    return true;
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return true;
        }
    }
    public string BackgroundColor {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("BackgroundColor")) {
                string val = UsersTable[_username]["BackgroundColor"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("BackgroundColor")) {
                    val = GroupDefaults["BackgroundColor"];
                }

                if (string.IsNullOrEmpty(val)) {
                    val = "#FFFFFF";
                }

                if (!val.StartsWith("#")) {
                    val = "#" + val;
                }

                return val;
            }

            return "#FFFFFF";
        }
    }
    public bool ShowSiteToolsPageDescription {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowSiteToolsPageDescription")) {
                string val = UsersTable[_username]["ShowSiteToolsPageDescription"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ShowSiteToolsPageDescription")) {
                    val = GroupDefaults["ShowSiteToolsPageDescription"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool AllowNavMenuCollapseToggle {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AllowNavMenuCollapseToggle")) {
                string val = UsersTable[_username]["AllowNavMenuCollapseToggle"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("AllowNavMenuCollapseToggle")) {
                    val = GroupDefaults["AllowNavMenuCollapseToggle"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowSiteToolsInCategories {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowSiteToolsInCategories")) {
                string val = UsersTable[_username]["ShowSiteToolsInCategories"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ShowSiteToolsInCategories")) {
                    val = GroupDefaults["ShowSiteToolsInCategories"];
                }

                if (string.IsNullOrEmpty(val)) {
                    return true;
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public string SiteColorOption {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("SiteColorOption")) {
                string val = UsersTable[_username]["SiteColorOption"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("SiteColorOption")) {
                    val = GroupDefaults["SiteColorOption"];
                }

                if (string.IsNullOrEmpty(val)) {
                    val = "1~;2~";
                }

                return val;
            }

            return "1~;2~";
        }
    }
    public string SiteLayoutOption {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("SiteLayoutOption")) {
                string val = UsersTable[_username]["SiteLayoutOption"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("SiteLayoutOption")) {
                    val = GroupDefaults["SiteLayoutOption"];
                }

                if (string.IsNullOrEmpty(val)) {
                    val = "Wide";
                }

                return val;
            }

            return "Wide";
        }
    }
    public bool ShowDedicatedMinimizedArea {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowDedicatedMinimizedArea")) {
                string val = UsersTable[_username]["ShowDedicatedMinimizedArea"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ShowDedicatedMinimizedArea")) {
                    val = GroupDefaults["ShowDedicatedMinimizedArea"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }

    public enum AppIconSelectorStyle { Default, Name_And_Description, Color_And_Description, Icon_Only, Icon_And_Color_Only, Icon_And_Text_Only, Icon_Plus_Color_And_Text };
    public AppIconSelectorStyle AppSelectorStyle {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("AppSelectorStyle")) {
                string val = UsersTable[_username]["AppSelectorStyle"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("AppSelectorStyle")) {
                    val = GroupDefaults["AppSelectorStyle"];
                }

                AppIconSelectorStyle appStyle = AppIconSelectorStyle.Default;
                Enum.TryParse<AppIconSelectorStyle>(val, out appStyle);

                return appStyle;
            }

            return AppIconSelectorStyle.Default;
        }
    }

    public enum AppStyle { Style_1, Style_2, Style_3 };
    public AppStyle UserAppStyle {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("UserAppStyle")) {
                string val = UsersTable[_username]["UserAppStyle"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("UserAppStyle")) {
                    val = GroupDefaults["UserAppStyle"];
                }

                AppStyle appStyle = AppStyle.Style_1;
                Enum.TryParse<AppStyle>(val, out appStyle);

                return appStyle;
            }

            return AppStyle.Style_1;
        }
    }

    public enum UserProfileLinkStyle { Default, Image_Plus_Name_And_Color_Cover, Image_And_Name, Image_And_Color, Image_Only, Name_And_Color_Cover, Name_And_Color, Name_Only };
    public UserProfileLinkStyle ProfileLinkStyle {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ProfileLinkStyle")) {
                string val = UsersTable[_username]["ProfileLinkStyle"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ProfileLinkStyle")) {
                    val = GroupDefaults["ProfileLinkStyle"];
                }

                UserProfileLinkStyle profileStyle = UserProfileLinkStyle.Default;
                Enum.TryParse<UserProfileLinkStyle>(val, out profileStyle);

                return profileStyle;
            }

            return UserProfileLinkStyle.Default;
        }
    }

    public string DefaultLoginGroup {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("DefaultLoginGroup")) {
                if (UsersTable[_username].ContainsKey("DefaultLoginGroup")) {
                    return UsersTable[_username]["DefaultLoginGroup"];
                }
            }

            return string.Empty;
        }
    }

    public string VacationTime {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("VacationTime")) {
                string val = UsersTable[_username]["VacationTime"];
                if (string.IsNullOrEmpty(val)) {
                    return "0";
                }

                return val;
            }

            return string.Empty;
        }
    }
    public Dictionary<int, string> GetBackgroundImg() {
        if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("BackgroundImgs")) {
            string val = UsersTable[_username]["BackgroundImgs"];
            if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("BackgroundImgs")) {
                val = GroupDefaults["BackgroundImgs"];
            }

            Dictionary<int, string> returnVal = new Dictionary<int, string>();
            string[] splitTemp = val.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitTemp.Length; i++) {
                string[] splitVal = splitTemp[i].Split(new string[] { ":=:" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitVal.Length == 2) {
                    int outDb = 1;
                    if (int.TryParse(splitVal[0], out outDb)) {
                        returnVal.Add(outDb, HelperMethods.RemoveProtocolFromUrl(splitVal[1].Trim()));
                    }
                }
            }

            return returnVal;
        }

        return new Dictionary<int, string>();
    }
    public string GetBackgroundImg(int workspace) {
        if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("BackgroundImgs")) {
            string val = UsersTable[_username]["BackgroundImgs"];
            if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("BackgroundImgs")) {
                val = GroupDefaults["BackgroundImgs"];
            }

            string returnVal = string.Empty;
            string[] splitTemp = val.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitTemp.Length; i++) {
                string[] vals = splitTemp[i].Trim().Split(new string[] { ":=:" }, StringSplitOptions.RemoveEmptyEntries);
                if (vals.Length == 2) {
                    if (vals[0] == workspace.ToString()) {
                        returnVal = vals[1];
                        break;
                    }
                }
            }

            returnVal = HelperMethods.RemoveProtocolFromUrl(returnVal);
            return returnVal;
        }

        return string.Empty;
    }
    public bool SiteTipsOnPageLoad {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("SiteTipsOnPageLoad")) {
                string val = UsersTable[_username]["SiteTipsOnPageLoad"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("SiteTipsOnPageLoad")) {
                    val = GroupDefaults["SiteTipsOnPageLoad"];
                }

                if (string.IsNullOrEmpty(val)) {
                    return true;
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }

    public const string DefaultBackgroundColor = "#FFFFFF";
    public const string DefaultBackgroundPosition = "right center";

    public static bool CheckIfUserIsLockedOut(string user) {
        MembershipUser _userLock = Membership.GetUser(user);
        if (_userLock != null) {
            if ((_userLock.IsLockedOut) || (!_userLock.IsApproved)) {
                return true;
            }
        }

        return false;
    }
    public static void UnlockUserIfNeeded(bool prevLockOutVal, string user) {
        bool userNowLockedOut = CheckIfUserIsLockedOut(user);
        if (!prevLockOutVal && userNowLockedOut) {
            MembershipUser _userLock = Membership.GetUser(user);
            if (_userLock != null) {
                _userLock.UnlockUser();
                _userLock.IsApproved = true;
                Membership.UpdateUser(_userLock);
            }
        }
    }

    public bool MobileAutoSync {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("MobileAutoSync")) {
                string val = UsersTable[_username]["MobileAutoSync"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("MobileAutoSync")) {
                    val = GroupDefaults["MobileAutoSync"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }

    public bool HideAllOverlays {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("HideAllOverlays")) {
                string val = UsersTable[_username]["HideAllOverlays"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("HideAllOverlays")) {
                    val = GroupDefaults["HideAllOverlays"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public string DefaultBodyFontFamily {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("DefaultBodyFontFamily")) {
                string val = UsersTable[_username]["DefaultBodyFontFamily"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("DefaultBodyFontFamily")) {
                    val = GroupDefaults["DefaultBodyFontFamily"];
                }

                return val;
            }

            return string.Empty;
        }
    }
    public string DefaultBodyFontSize {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("DefaultBodyFontSize")) {
                string val = UsersTable[_username]["DefaultBodyFontSize"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("DefaultBodyFontSize")) {
                    val = GroupDefaults["DefaultBodyFontSize"];
                }

                return val;
            }

            return string.Empty;
        }
    }
    public string DefaultBodyFontColor {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("DefaultBodyFontColor")) {
                string val = UsersTable[_username]["DefaultBodyFontColor"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("DefaultBodyFontColor")) {
                    val = GroupDefaults["DefaultBodyFontColor"];
                }

                return val;
            }

            return string.Empty;
        }
    }
    public bool ShowRowCountGridViewTable {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("ShowRowCountGridViewTable")) {
                string val = UsersTable[_username]["ShowRowCountGridViewTable"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("ShowRowCountGridViewTable")) {
                    val = GroupDefaults["ShowRowCountGridViewTable"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool UseAlternateGridviewRows {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("UseAlternateGridviewRows")) {
                string val = UsersTable[_username]["UseAlternateGridviewRows"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("UseAlternateGridviewRows")) {
                    val = GroupDefaults["UseAlternateGridviewRows"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool SiteToolsIconOnly {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("SiteToolsIconOnly")) {
                string val = UsersTable[_username]["SiteToolsIconOnly"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("SiteToolsIconOnly")) {
                    val = GroupDefaults["SiteToolsIconOnly"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool HideSearchBarInTopBar {
        get {
            if (CheckAndCreateUserSettings() && UsersTable[_username].ContainsKey("HideSearchBarInTopBar")) {
                string val = UsersTable[_username]["HideSearchBarInTopBar"];
                if (GroupDefaults.Count > 0 && GroupDefaults.ContainsKey("HideSearchBarInTopBar")) {
                    val = GroupDefaults["HideSearchBarInTopBar"];
                }

                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }

    #endregion


    #region Delete methods

    public void DeleteUserCustomizations(string userName) {
        dbCall.CallDelete(_userCustomizeTable, new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });

        #region Delete all user information

        new BlockedChats(userName).DeleteEntryByUserName(userName);
        new Chat(false).DeleteUserChats(userName);
        new ChatLogsDeleted(userName).DeleteUserEntries(userName);
        new ShowUpdatePopup().DeleteUserShowPopup(userName);
        new Bookmarks(userName).deleteBookmarks(userName);
        new UserCalendar(userName).DeleteEvents(userName);
        new UserNotificationMessages(userName).deleteAllUserNotification();
        new WorkspaceOverlays().DeleteAllUserOverlays(userName);
        new SiteMessageBoard(userName).DeleteUserPosts(userName);
        new RSSFeeds(userName).DeleteUserFeeds(userName);
        new TwitterFeeds(userName, false).deleteUserFeeds(userName);
        new App(userName).DeleteUserProperties(userName);
        MemberDatabase.DeleteUserSessionId(userName);

        #endregion

        if (UsersTable.ContainsKey(userName.ToLower())) {
            UsersTable.Remove(userName.ToLower());
        }
    }

    public static void DeleteUserSessionId(string userName) {
        if (!string.IsNullOrEmpty(userName)) {
            string sessionId = GetUserSessionId(userName);
            if (!string.IsNullOrEmpty(sessionId)) {
                if (UserSessionIds.ContainsKey(userName.ToLower()) && UserSessionIds[userName.ToLower()].Contains(sessionId)) {
                    UserSessionIds[userName.ToLower()].Remove(sessionId);
                }

                UserUpdateFlags uuf = new UserUpdateFlags();
                uuf.deleteFlag_User_And_SessionId(sessionId, userName);
            }
        }
    }

    #endregion


    #region Roles

    /// <summary> Will create a new Role using either the Roles CreateRole method or by creating a new one manually 
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns>True if Role was created successfully</returns>
    public static bool CreateRole(string roleName) {
        if (!string.IsNullOrEmpty(roleName)) {
            try {
                Roles.CreateRole(roleName);
                return true;
            }
            catch {
                DatabaseCall dbCall = new DatabaseCall();
                List<DatabaseQuery> query = new List<DatabaseQuery>();
                query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
                query.Add(new DatabaseQuery("RoleId", Guid.NewGuid().ToString()));
                query.Add(new DatabaseQuery("RoleName", roleName));

                string roleTableName = "Roles";
                if (dbCall.DataProvider == "System.Data.SqlClient") {
                    query.Add(new DatabaseQuery("LoweredRoleName", roleName.ToLower()));
                    roleTableName = "aspnet_Roles";
                }

                query.Add(new DatabaseQuery("Description", string.Empty));
                return dbCall.CallInsert(roleTableName, query);
            }
        }

        return false;
    }

    #endregion

}