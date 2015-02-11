#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
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
public class MemberDatabase : IRequiresSessionState {

    #region Private Variables

    private static Dictionary<string, Dictionary<string, string>> UsersTable = new Dictionary<string, Dictionary<string, string>>();

    private DatabaseCall dbCall = new DatabaseCall();
    private string _username;

    private string _membershipTableName = "Memberships";
    private string _usersTableName = "Users";
    private string _userCustomizeTable = "aspnet_UserCustomization";

    #endregion


    #region Initialize and Backups

    public MemberDatabase(string u) {
        SetTableNames();
        _username = u.ToLower();
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
            query.Add(new DatabaseQuery("UserId", UserId));
            query.Add(new DatabaseQuery("UserName", username));
            query.Add(new DatabaseQuery("WorkspaceRotate", _drDefaults["WorkspaceRotate"]));
            query.Add(new DatabaseQuery("WorkspaceRotateInterval", _drDefaults["WorkspaceRotateInterval"]));
            query.Add(new DatabaseQuery("WorkspaceRotateScreens", _drDefaults["WorkspaceRotateScreens"]));
            query.Add(new DatabaseQuery("RotateAutoRefresh", _drDefaults["RotateAutoRefresh"]));
            query.Add(new DatabaseQuery("BackgroundImgs", _drDefaults["BackgroundImgs"]));
            query.Add(new DatabaseQuery("EnableBackgrounds", _drDefaults["EnableBackgrounds"]));
            query.Add(new DatabaseQuery("ShowAppTitle", _drDefaults["ShowAppTitle"]));
            query.Add(new DatabaseQuery("AppHeaderIcon", _drDefaults["AppHeaderIcon"]));
            query.Add(new DatabaseQuery("TaskBarShowAll", _drDefaults["TaskBarShowAll"]));
            query.Add(new DatabaseQuery("HideAppIcon", _drDefaults["HideAppIcon"]));
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
            query.Add(new DatabaseQuery("AutoHideMode", _drDefaults["AutoHideMode"]));
            query.Add(new DatabaseQuery("PresentationMode", _drDefaults["PresentationMode"]));
            query.Add(new DatabaseQuery("TotalWorkspaces", _drDefaults["TotalWorkspaces"]));
            query.Add(new DatabaseQuery("HoverPreviewWorkspace", _drDefaults["HoverPreviewWorkspace"]));
            query.Add(new DatabaseQuery("IsSocialAccount", "0"));
            query.Add(new DatabaseQuery("WorkspaceMode", _drDefaults["WorkspaceMode"]));

            dbCall.CallInsert(_userCustomizeTable, query);
        }

        if (UsersTable.ContainsKey(username.ToLower())) {
            UsersTable.Remove(username.ToLower());
        }

        CheckAndCreateUserSettings();
    }

    #endregion


    #region Update methods

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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("FirstName", firstname));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("FirstName", firstname);
        }
    }
    public void UpdateLastName(string lastname) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("LastName", lastname));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("LastName", lastname);
        }
    }
    public void UpdateSignature(string signature) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Signature", signature));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("Signature", signature);
        }
    }
    public void UpdateTheme(string theme) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Theme", theme));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("Theme", theme);
        }
    }
    public void UpdateColor(string color) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("UserColor", color.Replace("#", "")));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("UserColor", color.Replace("#", ""));
        }
    }
    public void UpdateChatsMinimized(string chatsminimized) {
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("ChatsMinimized", chatsminimized));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("ChatsMinimized", chatsminimized);
            }
        }
    }
    public void UpdateChatsOpened(string chatsopened) {
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("ChatsOpened", chatsopened));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("ChatsOpened", chatsopened);
            }
        }
    }
    public void UpdateActivationCode(string code) {
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("ActivationCode", code));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("ActivationCode", code);
            }
        }
    }
    public void UpdateAdminPages(string page, bool remove) {
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("EmailUponLogin", templist));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("EmailUponLogin", templist);
        }
    }
    public void UpdateCommentDate(string date) {
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("IsNewMember", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("IsNewMember", temp);
        }
    }
    public void UpdateChatSoundNoti(bool allow) {
        string temp = "0";
        if (allow)
            temp = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatSoundNoti", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatSoundNoti", temp);
        }
    }
    public void UpdateReceiveAll(bool receiveAll) {
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
            string temp = "0";
            if (receiveAll)
                temp = "1";

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("ReceiveAllMail", temp));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("ReceiveAllMail", temp);
            }
        }
    }
    public void UpdateAdminChatControlled(bool controlled) {
        string temp = "0";
        if (controlled)
            temp = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AdminChatControlled", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("AdminChatControlled", temp);
        }
    }
    public void UpdateAppRemoteIDAndOptions(string app_id, string options) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("EnabledApps", applist));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("EnabledApps", applist);
            }
        }
    }
    public void UpdateEnabledApps(string app) {
        if (app.Length > 0 && UsersTable[_username].Count > 0) {
            if (app[app.Length - 1] == ',')
                app = app.Remove(app.Length - 1);

            string applist = UsersTable[_username]["EnabledApps"];
            if (applist.ToLower() == "none")
                applist = string.Empty;

            applist += (app + ",");

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("EnabledApps", applist));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("EnabledApps", applist);
            }
        }
    }
    public void UpdateEnabledApps_NewUser(string list) {
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("EnabledApps", list));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("EnabledApps", list);
            }
        }
    }
    public void RemoveAllEnabledApps() {
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("UserId", UsersTable[_username]["UserId"]));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("EnabledApps", string.Empty));

            if (dbCall.CallUpdate(_membershipTableName, updateQuery, query)) {
                UpdateUsersTable("EnabledApps", string.Empty);
            }
        }
    }
    public void RemoveEnabledApp(string app) {
        if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
            string applist = string.Empty;
            List<string> strApps = EnabledApps;
            applist = strApps.Where(w => !string.IsNullOrEmpty(w))
                                   .Where(w => w != app)
                                   .Aggregate(applist, (current, w) => current + (w + ","));

            List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatStatus", status));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatStatus", status);
        }
    }
    public void UpdateGroupName(string group) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("GroupName", group));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("GroupName", group);
        }
    }
    public void UpdateChatStatusMessage(string message) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatStatusMessage", message));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatStatusMessage", message);
        }
    }
    public void UpdateUserIpAddress(string ip) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("IsAway", temp));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("IsAway", temp);
        }
    }
    public void UpdateChatTimeStamp() {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        string time = DateTime.Now.ToString();
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatTimeStamp", time));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatTimeStamp", time);
        }
    }

    // UserCustomize Methods Below
    public void UpdateShowMinimizedPreview(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ShowMinimizedPreview", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("ShowMinimizedPreview", temp.ToString());
        }
    }
    public void UpdateAutoHideMode(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AutoHideMode", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AutoHideMode", temp.ToString());
        }
    }
    public void UpdatePresentationMode(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("PresentationMode", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("PresentationMode", temp.ToString());
        }
    }
    public void UpdateHoverPreviewWorkspace(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("IsSocialAccount", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("IsSocialAccount", temp.ToString());
        }
    }
    public void UpdateAcctImage(string img) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AcctImage", img));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AcctImage", img);
        }
    }
    public void UpdateTotalWorkspaces(int count) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("WorkspaceRotate", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("WorkspaceRotate", temp.ToString());
        }
    }
    public void UpdateAutoRotateWorkspaceInterval(string interval) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("WorkspaceRotateInterval", interval));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("WorkspaceRotateInterval", interval);
        }
    }
    public void UpdateWorkspaceRotateScreens(string screens) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("PrivateAccount", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("PrivateAccount", temp.ToString());
        }
    }
    public void UpdateAnimationSpeed(int seconds) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AnimationSpeed", seconds.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AnimationSpeed", seconds.ToString());
        }
    }
    public void UpdateBackgroundImg(string url) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("BackgroundImgs", newImg));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("BackgroundImgs", newImg);
        }
    }
    public void UpdateWorkspaceMode(string mode) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppHeaderIcon", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AppHeaderIcon", temp.ToString());
        }
    }
    public void UpdateHideAppIcon(bool hide) {
        int temp = 0;
        if (hide) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("HideAppIcon", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("HideAppIcon", temp.ToString());
        }
    }
    public void UpdateTaskBarShowAll(bool showAll) {
        int temp = 0;
        if (showAll) {
            temp = 1;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("LockAppIcons", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("LockAppIcons", temp.ToString());
        }
    }
    public void UpdateCurrentWorkspace(int workspace) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppSnapToGrid", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("AppSnapToGrid", temp.ToString());
        }
    }
    public void UpdateAppGridSize(string size) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
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
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("LoadLinksBlankPage", temp.ToString()));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("LoadLinksBlankPage", temp.ToString());
        }
    }
    public void UpdateVacationTime(string time) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("VacationTime", time));

        if (dbCall.CallUpdate(_userCustomizeTable, updateQuery, query)) {
            UpdateUsersTable("VacationTime", time);
        }
    }

    #endregion


    #region Auto Update Flags

    public void UpdateChatTimeout(string timeout) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
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

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatUpdateFlag", temp.ToString()));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, null)) {
            UpdateUsersTable("ChatUpdateFlag", temp.ToString());
        }
    }
    public void SetChatUpdateFlag(bool update, string user) {
        int temp = 0;
        if (update)
            temp = 1;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", _username));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ChatUpdateFlag", temp.ToString()));

        if (dbCall.CallUpdate(_usersTableName, updateQuery, query)) {
            UpdateUsersTable("ChatUpdateFlag", temp.ToString());
        }
    }

    #endregion


    #region Get methods

    public static List<string> GetListOfAvailableRoles() {
        return Roles.GetAllRoles().ToList();
    }
    private bool CheckAndCreateUserSettings() {
        if (!UsersTable.ContainsKey(_username) && !string.IsNullOrEmpty(_username)) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(_usersTableName, "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username) });
            if (dbSelect.Count > 0) {
                try {
                    if (!UsersTable.ContainsKey(_username) && !string.IsNullOrEmpty(_username)) {
                        UsersTable.Add(_username, new Dictionary<string, string>());

                        AddUserSettingToTable(dbSelect);

                        dbSelect = dbCall.CallSelect(_userCustomizeTable, "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username) });
                        AddUserSettingToTable(dbSelect);

                        if (!string.IsNullOrEmpty(UsersTable[_username]["UserId"])) {
                            dbSelect = dbCall.CallSelect(_membershipTableName, "Email, CommentDate, ReceiveAllMail, EnabledApps, AdminPages, ActivationCode", new List<DatabaseQuery>() { new DatabaseQuery("UserId", UsersTable[_username]["UserId"]) });
                            AddUserSettingToTable(dbSelect);
                        }
                    }
                }
                catch (Exception e) {
                    new AppLog(false).AddError(e);
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
            if (CheckAndCreateUserSettings()) {
                return UsersTable[_username]["UserId"];
            }

            return string.Empty;
        }
    }
    public string FirstName {
        get {
            if (CheckAndCreateUserSettings()) {
                string firstName = UsersTable[_username]["FirstName"];
                return string.IsNullOrEmpty(firstName) ? "N/A" : firstName;
            }

            return string.Empty;
        }
    }
    public string LastName {
        get {
            if (CheckAndCreateUserSettings()) {
                string lastName = UsersTable[_username]["LastName"];
                return string.IsNullOrEmpty(lastName) ? "N/A" : lastName;
            }

            return string.Empty;
        }
    }
    public string Signature {
        get {
            if (CheckAndCreateUserSettings()) {
                return UsersTable[_username]["Signature"];
            }

            return string.Empty;
        }
    }
    public string UserColor {
        get {
            if (CheckAndCreateUserSettings()) {
                string userColor = UsersTable[_username]["UserColor"];
                return string.IsNullOrEmpty(userColor) ? "N/A" : userColor;
            }

            return string.Empty;
        }
    }
    public bool ReceiveAll {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ReceiveAllMail"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public string ChatsMinimized {
        get {
            if (CheckAndCreateUserSettings()) {
                return UsersTable[_username]["ChatsMinimized"];
            }

            return string.Empty;
        }
    }
    public string ChatsOpened {
        get {
            if (CheckAndCreateUserSettings()) {
                return UsersTable[_username]["ChatsOpened"];
            }

            return string.Empty;
        }
    }
    public string ActivationCode {
        get {
            if (CheckAndCreateUserSettings()) {
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
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["AdminChatControlled"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public List<string> EnabledApps {
        get {
            var temp = new List<string>();

            if (CheckAndCreateUserSettings()) {
                if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
                    string val = UsersTable[_username]["EnabledApps"];
                    string[] ew = val.Trim().Split(',');
                    temp.AddRange(ew.Where(t => !string.IsNullOrEmpty(t)));
                }
            }

            return temp;
        }
    }
    public string ChatStatus {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ChatStatus"];
                return string.IsNullOrEmpty(val) ? "Offline" : val;
            }

            return "Offline";
        }
    }
    public string ChatStatusMessage {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ChatStatusMessage"];
                return string.IsNullOrEmpty(val) ? "Offline" : val;
            }

            return "Offline";
        }
    }
    public string IpAddress {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["IPAddress"];
                return string.IsNullOrEmpty(val) ? "Available" : val;
            }

            return string.Empty;
        }
    }
    public List<string> GroupList {
        get {
            var temp = new List<string>();
            if (CheckAndCreateUserSettings()) {
                GroupIPListener groupIplistener = new GroupIPListener();
                string x = UsersTable[_username]["GroupName"];

                string[] xArray = x.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                if ((HttpContext.Current.Session != null) && (HttpContext.Current.Session["LoginGroup"] != null)) {
                    string currLoginGroup = HttpContext.Current.Session["LoginGroup"].ToString();
                    temp.Add(currLoginGroup);
                }
                else {
                    foreach (string gs in xArray) {
                        if (groupIplistener.CheckGroupNetwork(gs)) {
                            temp.Add(gs);
                        }
                    }
                }

                #region TODO - Need to figure out why this is removing user from group when group id is wrong
                //if ((temp.Count > 0) && (_username.ToLower() == HttpContext.Current.User.Identity.Name.ToLower())) {
                //    Groups groups = new Groups(_username);
                //    List<string> allGroups = groups.GetEntryList();
                //    MemberDatabase member = new MemberDatabase(_username);
                //    foreach (string _g in temp) {
                //        if (!allGroups.Contains(_g)) {
                //            string templist = Groups.RemoveUserFromGroupList(temp, _g);
                //            member.UpdateGroupName(templist);

                //            if (string.IsNullOrEmpty(templist)) {
                //                break;
                //            }
                //        }
                //    }
                //}
                #endregion
            }

            return temp;
        }
    }
    public bool IsTyping {
        get {
            if (CheckAndCreateUserSettings()) {
                if (UsersTable[_username].ContainsKey("IsTypingChat")) {
                    return HelperMethods.ConvertBitToBoolean(UsersTable[_username]["IsTypingChat"]);
                }
            }
            return false;
        }
    }
    public bool StatusChanged {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["StatusChanged"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool IsAway {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["IsAway"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public DateTime LastUpdated {
        get {
            var d = new DateTime();

            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ChatTimeStamp"];

                DateTime o;
                if (!DateTime.TryParse(val, out o)) {
                    UpdateChatTimeStamp();
                    d = DateTime.Now;
                }
                else
                    d = o;
            }

            return d;
        }
    }
    public bool ChatUpdateFlag {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ChatUpdateFlag"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ChatEnabled {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["EnableChat"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool IsNewMember {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["IsNewMember"];
                return HelperMethods.ConvertBitToBoolean(val) && !string.IsNullOrEmpty(val);
            }

            return false;
        }
    }
    public int ChatTimeout {
        get {
            int timeout = 10;

            if (CheckAndCreateUserSettings()) {
                string temp = UsersTable[_username]["ChatTimeout"];
                if (!int.TryParse(temp, out timeout)) {
                    timeout = 10;
                }
            }

            return timeout;
        }
    }
    public string[] EmailUponLoginList {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["EmailUponLogin"];
                string[] splitusers = val.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                return splitusers;
            }

            return new string[0];
        }
    }
    private string EmailUponLoginNonlist {
        get {
            if (CheckAndCreateUserSettings()) {
                return UsersTable[_username]["EmailUponLogin"];
            }

            return string.Empty;
        }
    }
    public string[] AdminPagesList {
        get {
            if (CheckAndCreateUserSettings()) {
                if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
                    string temp = UsersTable[_username]["AdminPages"];
                    string[] splitusers = temp.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                    Array.Sort(splitusers, StringComparer.InvariantCulture);

                    return splitusers;
                }
            }

            string[] returnval = { "" };
            return returnval;
        }
    }
    public string AdminPagesNonlist {
        get {
            if (CheckAndCreateUserSettings()) {
                return UsersTable[_username]["AdminPages"];
            }

            return string.Empty;
        }
    }
    public string SiteTheme {
        get {
            if (CheckAndCreateUserSettings()) {
                string temp = UsersTable[_username]["Theme"];
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
            if (CheckAndCreateUserSettings()) {
                string temp = UsersTable[_username]["ChatSoundNoti"];
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
            if (CheckAndCreateUserSettings()) {
                temp[0] = UsersTable[_username]["AppRemoteID"];
                temp[1] = UsersTable[_username]["AppRemoteOptions"];
            }
            return temp;
        }
    }
    public DateTime CommentDate() {
        string temp = DateTime.Now.ToString();
        if (CheckAndCreateUserSettings()) {
            if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
                string x = UsersTable[_username]["CommentDate"];
                temp = string.IsNullOrEmpty(x) ? DateTime.Now.ToString(CultureInfo.InvariantCulture) : x;
            }
        }
        return Convert.ToDateTime(temp);
    }
    public DateTime CommentDate(string user) {
        if (CheckAndCreateUserSettings()) {
            string x = UsersTable[user.ToLower()]["CommentDate"];
            string temp = string.IsNullOrEmpty(x) ? DateTime.Now.ToString(CultureInfo.InvariantCulture) : x;

            return Convert.ToDateTime(temp);
        }

        return DateTime.Now;
    }
    public bool UserHasApp(string app) {
        if (CheckAndCreateUserSettings()) {
            if (!string.IsNullOrEmpty(_username) && UsersTable[_username].Count > 0) {
                bool temp = false;
                string[] delim = { "," };
                string[] ew = UsersTable[_username]["EnabledApps"].Trim().Split(delim, StringSplitOptions.RemoveEmptyEntries);
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
        string selectCols = string.Format("{0}.EnableChat=1 AND {0}.AdminChatControlled=0 AND {1}.IsLockedOut='False' AND {1}.IsApproved='True' AND UserName<>@UserName AND {0}.UserId={1}.UserId", _usersTableName, _membershipTableName);
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(_usersTableName + ", " + _membershipTableName, "UserName", selectCols, new List<DatabaseQuery>() { new DatabaseQuery("UserName", username) });
        foreach (Dictionary<string, string> row in dbSelect) {
            if (!dataTable.Contains(row["UserName"])) {
                dataTable.Add(row["UserName"]);
            }
        }

        return dataTable;
    }
    public string GetUsernameFromUserId(string userId) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle(_usersTableName, "UserName", new List<DatabaseQuery>() { new DatabaseQuery("UserId", userId) });
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
            if (CheckAndCreateUserSettings()) {
                return UsersTable[_username]["AcctImage"];
            }

            return string.Empty;
        }
    }
    public bool MultipleBackgrounds {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["EnableBackgrounds"];
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
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["AnimationSpeed"];
                if (!string.IsNullOrEmpty(val))
                    int.TryParse(val, out temp);

                if (temp == 0)
                    temp = 150;
            }

            return temp;
        }
    }
    public UserWorkspaceMode WorkspaceMode {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["WorkspaceMode"];
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

            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["TotalWorkspaces"];
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
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ShowToolTips"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool HoverPreviewWorkspace {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["HoverPreviewWorkspace"];
                return HelperMethods.ConvertBitToBoolean(val) || val == string.Empty;
            }

            return false;
        }
    }
    public bool IsSocialAccount {
        get {
            if (CheckAndCreateUserSettings()) {
                try {
                    string val = UsersTable[_username]["IsSocialAccount"];
                    return HelperMethods.ConvertBitToBoolean(val);
                }
                catch { }
            }

            return false;
        }
    }
    public bool AppContainer {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["AppContainer"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool AutoHideMode {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["AutoHideMode"];
                return HelperMethods.ConvertBitToBoolean(val) && !string.IsNullOrEmpty(val);
            }

            return false;
        }
    }
    public bool PresentationMode {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["PresentationMode"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ClearPropOnSignOff {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ClearPropOnSignOff"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowDateTime {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ShowDateTime"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool WorkspaceRotate {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["WorkspaceRotate"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public string WorkspaceRotateInterval {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["WorkspaceRotateInterval"];
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
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["LockAppIcons"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public int CurrentWorkspace {
        get {
            if (CheckAndCreateUserSettings()) {
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
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["AppSnapToGrid"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public string AppGridSize {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["AppGridSize"];
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
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["GroupIcons"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowCategoryCount {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ShowCategoryCount"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool LoadLinksBlankPage {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["LoadLinksBlankPage"];
                return HelperMethods.ConvertBitToBoolean(val) && !string.IsNullOrEmpty(val);
            }

            return false;
        }
    }
    public string WorkspaceRotateScreens {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["WorkspaceRotateScreens"];
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
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["RotateAutoRefresh"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowAppTitle {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ShowAppTitle"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool PrivateAccount {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["PrivateAccount"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowWorkspaceNumApp {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ShowWorkspaceNumApp"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool UserCustomizationsEnabled {
        get {
            if (CheckAndCreateUserSettings()) {
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
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["TaskBarShowAll"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool HideAppIcon {
        get {
            if (CheckAndCreateUserSettings()) {
                if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    return false;
                }
                else {
                    string val = UsersTable[_username]["HideAppIcon"];
                    return HelperMethods.ConvertBitToBoolean(val) && !string.IsNullOrEmpty(val);
                }
            }

            return false;
        }
    }
    public bool AppHeaderIcon {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["AppHeaderIcon"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public bool ShowMinimizedPreview {
        get {
            if (CheckAndCreateUserSettings()) {
                string val = UsersTable[_username]["ShowMinimizedPreview"];
                return HelperMethods.ConvertBitToBoolean(val);
            }

            return false;
        }
    }
    public string VacationTime {
        get {
            if (CheckAndCreateUserSettings()) {
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
        if (CheckAndCreateUserSettings()) {
            string val = UsersTable[_username]["BackgroundImgs"];

            Dictionary<int, string> returnVal = new Dictionary<int, string>();
            string[] splitTemp = val.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitTemp.Length; i++) {
                string[] splitVal = splitTemp[i].Split(new string[] { ":=:" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitVal.Length == 2) {
                    int outDb = 1;
                    if (int.TryParse(splitVal[0], out outDb)) {
                        returnVal.Add(outDb, splitVal[1].Trim());
                    }
                }
            }

            return returnVal;
        }

        return new Dictionary<int, string>();
    }
    public string GetBackgroundImg(int workspace) {
        if (CheckAndCreateUserSettings()) {
            string val = UsersTable[_username]["BackgroundImgs"];

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

            return returnVal;
        }

        return string.Empty;
    }

    #endregion


    #region Delete methods

    public void DeleteUserCustomizations(string userName) {
        dbCall.CallDelete(_userCustomizeTable, new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName) });

        #region Delete all user information

        new BlockedChats(userName).DeleteEntryByUserName(userName);
        new Chat(false).DeleteUserChats(userName);
        new ChatLogsDeleted(userName).DeleteUserEntries(userName);
        new ShowUpdatePopup().DeleteUserShowPopup(userName);
        new Bookmarks(userName).deleteBookmarks(userName);
        new UserCalendar(userName).DeleteEvents(userName);
        new UserNotificationMessages(userName).deleteAllUserNotification();
        new WorkspaceOverlays().DeleteAllUserOverlays(userName);
        new SiteMessageBoard(userName).deleteUserPosts(userName);
        new RSSFeeds(userName).DeleteUserFeeds(userName);
        new TwitterFeeds(userName, false).deleteUserFeeds(userName);
        new App(userName).DeleteUserProperties(userName);
        new UserUpdateFlags().deleteFlag_User(userName);

        #endregion

        if (UsersTable.ContainsKey(userName.ToLower())) {
            UsersTable.Remove(userName.ToLower());
        }
    }

    #endregion

}