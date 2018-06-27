using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.Notifications;

public partial class SiteTools_MyNotifications : BasePage {

    private UserNotificationMessages _usernoti;

    protected void Page_Load(object sender, EventArgs e) {
        _usernoti = new UserNotificationMessages(CurrentUsername);
        BuildNotificationList();
    }

    private void BuildNotificationList() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 1);

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("", "40", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Message", "500", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Date", "150", false));
        tableBuilder.AddHeaderRow(headerColumns, true, "Date", "DESC");
        #endregion

        #region Build Body
        List<UserNotificationsMessage_Coll> messageList = _usernoti.getEntries();
        string serverLoc = ServerSettings.GetSitePath(HttpContext.Current.Request);
        foreach (UserNotificationsMessage_Coll x in messageList) {
            if (x.ID.ToLower() != "undefined") {
                List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();

                string notificationImage = UserNotifications.GetCorrectNotificationImage(x.Message, x.NotificationID, UserNotifications.GetNotificationIcon(x.NotificationID, CurrentSiteTheme));
                string notificationIcon = "<img title='" + UserNotifications.GetNotificationName(x.NotificationID) + "' src='" + serverLoc + "/" + notificationImage.Replace("~/", "") + "' style='height: 28px;' />";

                bodyColumnValues.Add(new TableBuilderBodyColumnValues(string.Empty, notificationIcon, TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Message", x.Message, TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Date", x.Date.Trim(), TableBuilderColumnAlignment.Left));

                string editButtons = "<a class='td-delete-btn' onclick=\"deleteNotification('" + x.ID + "');\" title='Delete'></a>";

                tableBuilder.AddBodyRow(bodyColumnValues, editButtons);
            }
        }
        #endregion

        pnl_myNotificationList.Controls.Clear();
        pnl_myNotificationList.Controls.Add(tableBuilder.CompleteTableLiteralControl("No notifications found"));
    }

    protected void hf_deleteNotification_ValueChanged(object sender, EventArgs e) {
        string id = hf_deleteNotification.Value;
        Guid outGuid = new Guid();
        if (Guid.TryParse(id, out outGuid)) {
            _usernoti.deleteNotification(id);
        }

        hf_deleteNotification.Value = string.Empty;
        BuildNotificationList();
    }
    protected void hf_deleteAllNotification_ValueChanged(object sender, EventArgs e) {
        _usernoti.deleteAllUserNotification();
        hf_deleteAllNotification.Value = string.Empty;
        BuildNotificationList();
    }

}