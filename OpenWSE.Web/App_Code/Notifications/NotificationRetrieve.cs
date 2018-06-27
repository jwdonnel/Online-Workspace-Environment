using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Security.Principal;
using System.Text;
using System.Web.Services.Protocols;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;

/// <summary>
/// Summary description for NotificationRetrieve
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class NotificationRetrieve : System.Web.Services.WebService {

    private readonly UserNotifications _notifi = new UserNotifications();
    private UserNotificationMessages _usernoti;
    private List<string> _groupname;
    private string _username;
    private MemberDatabase _member;
    private string _serverLoc;

    public NotificationRetrieve() {
        GetSiteRequests.AddRequest();

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    private void InitializeVariables() {
        IIdentity userId = HttpContext.Current.User.Identity;
        _serverLoc = ServerSettings.GetSitePath(HttpContext.Current.Request);
        _username = userId.Name;
        _member = new MemberDatabase(_username);
        _groupname = _member.GroupList;
        _usernoti = new UserNotificationMessages(_username);
    }


    [WebMethod]
    public String LoadUserNotifications(string _currIds) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            InitializeVariables();
            string[] delims = { "," };
            string[] tempArray = _currIds.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            List<string> idList = new List<string>(tempArray);
            StringBuilder str = new StringBuilder();
            int total = 0;

            List<UserNotificationsMessage_Coll> messageList = _usernoti.getNonDismissedEntries();
            str.Append("<table ID='table_NotiMessages' class='table-notiMessages' cellpadding='5' cellspacing='5'>");

            foreach (UserNotificationsMessage_Coll x in messageList) {
                if (total >= 5)
                    break;

                if (!idList.Contains(x.ID) && x.ID.ToLower() != "undefined" && !string.IsNullOrEmpty(x.ID)) {
                    str.Append("<tr id='" + x.ID + "'>");
                    string notificationImage = UserNotifications.GetCorrectNotificationImage(x.Message, x.NotificationID, UserNotifications.GetNotificationIcon(x.NotificationID, _member.SiteTheme));
                    str = InsertNotiIcons(str, notificationImage, UserNotifications.GetNotificationName(x.NotificationID));
                    str = InsertNotiMessage(str, x.Message, x.Date);
                    str = InsertRemoveMessage(str, true);
                    str.Append("</tr>");
                    total++;
                }
            }

            str.Append("</table>");

            if (total == 0) {
                str.Clear();
                str.Append("<div id='no-notifications-id'>No notifications found</div>");
            }

            return str.ToString();
        }
        return string.Empty;
    }

    [WebMethod]
    public String LoadMoreUserNotifications(string _currIds, string _currCount) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            InitializeVariables();

            string[] delims = { "," };
            string[] tempArray = _currIds.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            List<string> idList = new List<string>(tempArray);

            StringBuilder str = new StringBuilder();
            int total = 0;

            int currCount = 0;
            if (int.TryParse(_currCount, out currCount)) {
                List<UserNotificationsMessage_Coll> messageList = _usernoti.getNonDismissedEntries();
                for (int i = currCount; i < messageList.Count; i++) {
                    UserNotificationsMessage_Coll x = messageList[i];

                    if (total >= 5)
                        break;

                    if (!idList.Contains(x.ID) && x.ID.ToLower() != "undefined" && !string.IsNullOrEmpty(x.ID)) {
                        str.Append("<tr id='" + x.ID + "'>");
                        string notificationImage = UserNotifications.GetCorrectNotificationImage(x.Message, x.NotificationID, UserNotifications.GetNotificationIcon(x.NotificationID, _member.SiteTheme));
                        str = InsertNotiIcons(str, notificationImage, UserNotifications.GetNotificationName(x.NotificationID));
                        str = InsertNotiMessage(str, x.Message, x.Date);
                        str = InsertRemoveMessage(str, true);
                        str.Append("</tr>");
                        total++;
                    }
                }
            }
            if (total == 0)
                str.Clear();

            return str.ToString();
        }
        return string.Empty;
    }

    [WebMethod]
    public object[] RefreshUserNotifications(string _currIds) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            InitializeVariables();
            string[] delims = { "," };
            string[] tempArray = _currIds.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            List<string> idList = new List<string>(tempArray);

            object[] returnVals = new object[2];
            List<string> deleteList = new List<string>();

            StringBuilder str = new StringBuilder();
            int total = 0;

            List<UserNotificationsMessage_Coll> messageList = _usernoti.getNonDismissedEntries();

            // New Notification List
            foreach (UserNotificationsMessage_Coll x in messageList) {
                if (!idList.Contains(x.ID)) {
                    if (x.ID.ToLower() != "undefined" && !string.IsNullOrEmpty(x.ID)) {
                        str.Append("<tr id='" + x.ID + "'>");
                        string notificationImage = UserNotifications.GetCorrectNotificationImage(x.Message, x.NotificationID, UserNotifications.GetNotificationIcon(x.NotificationID, _member.SiteTheme));
                        str = InsertNotiIcons(str, notificationImage, UserNotifications.GetNotificationName(x.NotificationID));
                        str = InsertNotiMessage(str, x.Message, x.Date);
                        str = InsertRemoveMessage(str, true);
                        str.Append("</tr>");
                        total++;
                    }
                }
                else
                    break;
            }

            // Delete Notification List
            foreach (string id in idList) {
                bool delete = true;
                foreach (UserNotificationsMessage_Coll x in messageList) {
                    if (x.ID == id)
                        delete = false;
                }

                if (delete)
                    deleteList.Add(id);
            }

            if (total == 0)
                str.Clear();

            returnVals[0] = str.ToString();
            returnVals[1] = deleteList.ToArray();

            return returnVals;
        }
        return new object[0];
    }

    [WebMethod]
    public string DeleteNotifications(string id) {
        string returnVal = "false";
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            UserUpdateFlags _uuf = new UserUpdateFlags();
            InitializeVariables();
            if (id == "ClearAll") {
                _usernoti.dismissAllUserNotification();
                _uuf.addFlag(_username, "workspace", "");
                returnVal = "true";
            }
            else {
                Guid outGuid = new Guid();
                if (Guid.TryParse(id, out outGuid)) {
                    _usernoti.dismissNotification(id);
                    _uuf.addFlag(_username, "workspace", "");
                    returnVal = "true";
                }
            }
        }
        return returnVal;
    }


    [WebMethod]
    public string AcceptInviteNotifications(string id, string groupId) {
        string returnVal = "false";
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            UserUpdateFlags _uuf = new UserUpdateFlags();
            InitializeVariables();
            Guid outGuid = new Guid();
            if (Guid.TryParse(id, out outGuid)) {
                Groups groups = new Groups(_username);
                var member = new MemberDatabase(_username);
                List<string> groupList = member.GroupList;
                if (!groups.IsApartOfGroup(groupList, groupId)) {
                    string templist = BuildUserGroupList(groupList);
                    member.UpdateGroupName(templist + groupId);
                }

                _usernoti.deleteNotification(id);
                _uuf.addFlag(_username, "workspace", "");
                returnVal = "true";
            }
        }
        return returnVal;
    }
    private static string BuildUserGroupList(List<string> groups) {
        return groups.Aggregate(string.Empty, (current, t) => current + (t + ServerSettings.StringDelimiter));
    }


    private StringBuilder InsertNotiIcons(StringBuilder cell, string iconUrl, string errorName) {
        cell.Append("<td><div align='center' class='pad-left'>");
        cell.Append("<img title='" + errorName + "' class='notification-icon' src='" + _serverLoc + "/" + iconUrl.Replace("~/", "") + "'>");
        cell.Append("<div class='clear'></div>");
        cell.Append("</div></td>");
        return cell;
    }
    private StringBuilder InsertRemoveMessage(StringBuilder cell, bool includeRemove) {
        if (includeRemove) {
            cell.Append("<td><div align='center' class='pad-left pad-right'>");
            cell.Append("<a title='Dismiss' class='alert-panel-noti-btns-remove' onclick='openWSE.NotiActionsHideInd(this);'></a>");
            cell.Append("<div class='clear'></div>");
            cell.Append("</div></td>");
        }
        return cell;
    }
    private StringBuilder InsertNotiMessage(StringBuilder cell, string message, string date) {
        cell.Append("<td>");
        cell.Append(message.Trim());
        cell.Append("<div class='clear'></div><div class='notification-date'>" + HelperMethods.GetFormatedTime(Convert.ToDateTime(date.Trim())) + "</div><div class='clear'></div>");
        cell.Append("</td>");
        return cell;
    }

}