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

    private readonly Notifications _notifi = new Notifications();
    private UserNotificationMessages _usernoti;
    private List<string> _groupname;
    private string _username;
    private MemberDatabase _member;
    private string _serverLoc;

    public NotificationRetrieve() {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public String LoadUserNotifications(string _currIds, string siteTheme) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            InitializeVariables();
            string[] delims = { "," };
            string[] tempArray = _currIds.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            List<string> idList = new List<string>(tempArray);
            StringBuilder str = new StringBuilder();
            int total = 0;

            _usernoti.getEntries();
            str.Append("<table ID='table_NotiMessages' class='table-notiMessages' cellpadding='5' cellspacing='5'>");

            foreach (UserNotificationsMessage_Coll x in _usernoti.Messages) {
                if (total >= 5) break;
                Notifications_Coll coll = _notifi.GetNotification(x.NotificationID);
                if (!string.IsNullOrEmpty(coll.NotificationName)) {
                    if ((!idList.Contains(x.ID)) && (x.ID.ToLower() != "undefined") && (!string.IsNullOrEmpty(x.ID))) {
                        str.Append("<tr id='" + x.ID + "'><td>");
                        str = InsertNotiIcons(str, coll.NotificationImage, true, coll.NotificationName, siteTheme);
                        str = InsertNotiMessage(str, x.Message, x.Date);
                        str.Append("</td></tr>");
                        total++;
                    }
                }
            }

            str.Append("</table>");

            if (total == 0) {
                str.Clear();
                str.Append("<h3 id='no-notifications-id' class='pad-bottom-big'>No notifications available.</h3>");
            }

            return str.ToString();
        }
        return string.Empty;
    }

    [WebMethod]
    public String LoadMoreUserNotifications(string _currIds, string _currCount, string siteTheme) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            InitializeVariables();

            string[] delims = { "," };
            string[] tempArray = _currIds.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            List<string> idList = new List<string>(tempArray);

            StringBuilder str = new StringBuilder();
            int total = 0;

            int currCount = 0;
            if (int.TryParse(_currCount, out currCount)) {
                _usernoti.getEntries();
                for (int i = currCount; i < _usernoti.Messages.Count; i++) {
                    UserNotificationsMessage_Coll x = _usernoti.Messages[i];

                    if (total >= 5) break;
                    Notifications_Coll coll = _notifi.GetNotification(x.NotificationID);
                    if (!string.IsNullOrEmpty(coll.NotificationName)) {
                        if ((!idList.Contains(x.ID)) && (x.ID.ToLower() != "undefined") && (!string.IsNullOrEmpty(x.ID))) {
                            str.Append("<tr id='" + x.ID + "'><td>");
                            str = InsertNotiIcons(str, coll.NotificationImage, true, coll.NotificationName, siteTheme);
                            str = InsertNotiMessage(str, x.Message, x.Date);
                            str.Append("</td></tr>");
                            total++;
                        }
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
    public object[] RefreshUserNotifications(string _currIds, string siteTheme) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            InitializeVariables();
            string[] delims = { "," };
            string[] tempArray = _currIds.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            List<string> idList = new List<string>(tempArray);

            object[] returnVals = new object[2];
            List<string> deleteList = new List<string>();

            StringBuilder str = new StringBuilder();
            int total = 0;

            _usernoti.getEntries();

            // New Notification List
            foreach (UserNotificationsMessage_Coll x in _usernoti.Messages) {
                Notifications_Coll coll = _notifi.GetNotification(x.NotificationID);
                if ((!string.IsNullOrEmpty(coll.NotificationName)) && (!idList.Contains(x.ID))) {
                    if ((x.ID.ToLower() != "undefined") && (!string.IsNullOrEmpty(x.ID))) {
                        str.Append("<tr id='" + x.ID + "'><td>");
                        str = InsertNotiIcons(str, coll.NotificationImage, true, coll.NotificationName, siteTheme);
                        str = InsertNotiMessage(str, x.Message, x.Date);
                        str.Append("</td></tr>");
                        total++;
                    }
                }
                else
                    break;
            }

            // Delete Notification List
            foreach (string id in idList) {
                bool delete = true;
                foreach (UserNotificationsMessage_Coll x in _usernoti.Messages) {
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
                _usernoti.deleteAllUserNotification();
                _uuf.addFlag(_username, "workspace", "");
                returnVal = "true";
            }
            else {
                Guid outGuid = new Guid();
                if (Guid.TryParse(id, out outGuid)) {
                    _usernoti.deleteNotification(id);
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

    [WebMethod]
    public string CheckForNewNotifications() {
        int total = 0;
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                Notifications _notifi = new Notifications();
                UserNotificationMessages _usernoti = new UserNotificationMessages(HttpContext.Current.User.Identity.Name);
                _usernoti.getEntries("ASC");
                foreach (UserNotificationsMessage_Coll x in _usernoti.Messages) {
                    Notifications_Coll coll = _notifi.GetNotification(x.NotificationID);
                    if ((!string.IsNullOrEmpty(coll.NotificationName)) && (!string.IsNullOrEmpty(x.ID))) {
                        total++;
                    }
                }
            }
        }
        return total.ToString();
    }

    private static string BuildUserGroupList(List<string> groups) {
        return groups.Aggregate(string.Empty, (current, t) => current + (t + ServerSettings.StringDelimiter));
    }

    private void InitializeVariables() {
        IIdentity userId = HttpContext.Current.User.Identity;
        _serverLoc = ServerSettings.GetSitePath(HttpContext.Current.Request);
        _username = userId.Name;
        _member = new MemberDatabase(_username);
        _groupname = _member.GroupList;
        _usernoti = new UserNotificationMessages(_username);
    }

    private StringBuilder InsertNotiIcons(StringBuilder cell, string iconUrl, bool includeRemove, string errorName, string siteTheme) {
        cell.Append("<div class='alert-panel-noti-btns'>");
        cell.Append("<img title='" + errorName + "' class='notificationIcon' src='" + _serverLoc + "/" + iconUrl.Replace("~/", "") + "'>");

        if (includeRemove) {
            cell.Append("<br />");
            cell.Append("<img title='Hide' class='alert-panel-noti-btns-remove' src='" + _serverLoc + "/App_Themes/" + siteTheme + "/Icons/remove.png' onclick='openWSE.NotiActionsHideInd(this);'>");

        }
        cell.Append("</div>");

        return cell;
    }
    private StringBuilder InsertNotiMessage(StringBuilder cell, string message, string date) {
        cell.Append("<div class='pad-left-big pad-right pad-top-big pad-bottom-big alert-panel-description'>");
        cell.Append(message.Trim());
        cell.Append("<div class='clear-space-two'></div><span>" + HelperMethods.GetFormatedTime(Convert.ToDateTime(date.Trim())) + "</span>");
        cell.Append("</div>");

        return cell;
    }
}