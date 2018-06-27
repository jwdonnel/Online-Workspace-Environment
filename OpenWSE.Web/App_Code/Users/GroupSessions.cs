using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using OpenWSE_Tools.GroupOrganizer;

/// <summary>
/// Summary description for GroupSessions
/// </summary>
public class GroupSessions {

    private static Dictionary<string, string> GroupLoginSessions = new Dictionary<string, string>();

    /// <summary>
    /// Check if the user has a group login session key
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns></returns>
    public static bool DoesUserHaveGroupLoginSessionKey(string username) {
        if (!string.IsNullOrEmpty(username)) {
            username = username.ToLower();
            if (GroupLoginSessions.ContainsKey(username) && !string.IsNullOrEmpty(GroupLoginSessions[username])) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Clears all group login sessions
    /// </summary>
    public static void ClearAllGroupLoginSessions() {
        GroupLoginSessions.Clear();
    }

    /// <summary>
    /// Create or set a new group login session
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="groupId">Group Id</param>
    public static void AddOrSetNewGroupLoginSession(string username, string groupId) {
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(groupId)) {
            username = username.ToLower();
            if (GroupLoginSessions.ContainsKey(username)) {
                GroupLoginSessions[username] = groupId;
            }
            else {
                GroupLoginSessions.Add(username, groupId);
            }

            SetUserChatListRefresh(username);
        }
    }

    /// <summary>
    /// Remove a users group login session
    /// </summary>
    /// <param name="username">Username</param>
    public static void RemoveGroupLoginSession(string username) {
        if (!string.IsNullOrEmpty(username)) {
            username = username.ToLower();
            if (GroupLoginSessions.ContainsKey(username)) {
                GroupLoginSessions.Remove(username);
            }

            SetUserChatListRefresh(username);
        }
    }

    /// <summary>
    /// Get the current group id of the login session
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns></returns>
    public static string GetUserGroupSessionName(string username) {
        if (!string.IsNullOrEmpty(username)) {
            username = username.ToLower();
            if (DoesUserHaveGroupLoginSessionKey(username) && username != ServerSettings.AdminUserName.ToLower()) {
                return GroupLoginSessions[username];
            }
        }

        if (GroupLoginSessions.ContainsKey(username)) {
            GroupLoginSessions.Remove(username);
        }

        return username;
    }

    /// <summary>
    /// Need to update all the user chat lists when logging into or logging out of a group
    /// </summary>
    /// <param name="username">Username</param>
    private static void SetUserChatListRefresh(string username) {
        MemberDatabase member = new MemberDatabase(username);
        List<string> userList = member.ChattableUserList(username);

        foreach (string user in userList) {
            if (!ChatService._needRefreshUsers.ContainsKey(user)) {
                ChatService._needRefreshUsers.Add(user, true);
            }
            else {
                ChatService._needRefreshUsers[user] = true;
            }
        }
    }

}