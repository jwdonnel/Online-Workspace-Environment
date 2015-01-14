using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;
using System.Web.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web.Security;
using System.Data.SqlServerCe;


[Serializable]
public struct ShowUpdates_Coll {
    private readonly string _username;
    private readonly string _needShow;

    public ShowUpdates_Coll(string username, string needShow) {
        _username = username;
        _needShow = needShow;
    }

    public string UserName {
        get { return _username; }
    }

    public bool ShowPopup {
        get {
            if (HelperMethods.ConvertBitToBoolean(_needShow))
                return true;

            return false;
        }
    }
}


/// <summary>
/// Summary description for ShowUpdatePopup
/// </summary>
public class ShowUpdatePopup {
    private List<ShowUpdates_Coll> dataTable = new List<ShowUpdates_Coll>();
    private readonly DatabaseCall dbCall = new DatabaseCall();

    public ShowUpdatePopup() { }

    public void BuildUsers() {
        BuildList();
        MembershipUserCollection userColl = Membership.GetAllUsers();

        foreach (ShowUpdates_Coll showColl in dataTable.ToList()) {
            bool needDelete = true;
            foreach (MembershipUser user in userColl) {
                if (user.UserName.ToLower() == showColl.UserName.ToLower()) {
                    needDelete = false;
                    break;
                }
            }

            if (needDelete) {
                string showPopup = "0";
                if (showColl.ShowPopup)
                    showPopup = "1";

                ShowUpdates_Coll temp = new ShowUpdates_Coll(showColl.UserName, showPopup);
                dataTable.Remove(temp);
                DeleteUserShowPopup(showColl.UserName);
            }
        }

        foreach (MembershipUser user in userColl) {
            if (user.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                ShowUpdates_Coll tempShow = new ShowUpdates_Coll(user.UserName.ToLower(), "1");
                ShowUpdates_Coll tempNoShow = new ShowUpdates_Coll(user.UserName.ToLower(), "0");

                if ((!dataTable.Contains(tempNoShow)) && (!dataTable.Contains(tempShow))) {
                    AddUser(user.UserName, false);
                }
            }
        }
    }

    private void AddUser(string username, bool needtoShow) {
        string show = "0";
        if (needtoShow)
            show = "1";

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", username.ToLower()));
        query.Add(new DatabaseQuery("ShowPopup", show));

        dbCall.CallInsert("aspnet_ShowUpdates", query);
    }

    private void BuildList() {
        dataTable.Clear();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_ShowUpdates", "", null);
        foreach (Dictionary<string, string> row in dbSelect) {
            string user = row["UserName"];
            string show = row["ShowPopup"];
            var coll = new ShowUpdates_Coll(user, show);
            dataTable.Add(coll);
        }
    }

    public void UpdateAllUsers(bool needToShow) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();

        if (!needToShow) {
            query.Add(new DatabaseQuery("ShowPopup", "1"));
            updateQuery.Add(new DatabaseQuery("ShowPopup", "0"));
        }
        else {
            query.Add(new DatabaseQuery("ShowPopup", "0"));
            updateQuery.Add(new DatabaseQuery("ShowPopup", "1"));
        }

        dbCall.CallUpdate("aspnet_ShowUpdates", updateQuery, query);
    }

    public bool isUserShowPopup(string username) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_ShowUpdates", "ShowPopup", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username.ToLower()) });
        return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
    }

    public void UpdateUser(bool needToShow, string username) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", username.ToLower()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();

        if (!needToShow) {
            updateQuery.Add(new DatabaseQuery("ShowPopup", "0"));
        }
        else {
            updateQuery.Add(new DatabaseQuery("ShowPopup", "1"));
        }

        dbCall.CallUpdate("aspnet_ShowUpdates", updateQuery, query);
    }

    public string GetNewUpdateMessage(string serverPath, string theme, bool showLink = true) {
        StringBuilder str = new StringBuilder();
        About about = new About(serverPath + "\\ChangeLog.xml");
        about.ParseXml();

        // Grab the latest entry only
        Dictionary<string, string> aboutItems = about.AboutItems;
        foreach (KeyValuePair<string, string> entry in aboutItems) {
            string date = entry.Key;
            string value = entry.Value;

            str.Append("<h2 class='float-left font-bold'>" + OpenWSE.Core.Licensing.CheckLicense.SiteName + " Updates</h2>");
            str.Append("<h2 class='float-right'>" + date + " - " + about.CurrentVersion + "</h2>");
            str.Append("<div class='clear-space'></div>");

            string[] delim = { "<br />", "<br/>" };
            string[] splitValue = value.Split(delim, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder updateList = new StringBuilder();

            if (splitValue.Length > 1)
                updateList.Append("<ul>");

            foreach (string m in splitValue) {
                string tempM = m.Replace('\n', ' ').Trim();
                if (!string.IsNullOrEmpty(tempM)) {
                    if (splitValue.Length > 1)
                        updateList.Append("<li class='pad-bottom'>" + tempM + "</li>");
                    else
                        updateList.Append(tempM);
                }
            }

            if (splitValue.Length > 1)
                updateList.Append("</ul>");

            str.Append("<div class='pad-all new-update-holder'>" + updateList.ToString() + "</div>");
            str.Append("<div class='clear-space-five'></div>");

            string img = "<img alt='' class='float-left pad-right-sml' src='App_Themes/" + theme + "/App/approve.png' />";
            string text = "<span style='font-size: 18px'>Got it</span>";
            string button = "<a href='#gotit' class='input-buttons no-margin' onclick='openWSE.CloseUpdatesPopup();return false;' ";
            button += "style='text-decoration: none!important;'>" + img + " " + text + "</a>";

            string link = string.Empty;
            if (showLink) {
                link = "<a href='About.aspx?a=changelog' target='_blank' ";
                link += "style='text-decoration: underline; position: absolute; right: 20px; bottom: 20px;'>View Change Log</a>";
            }

            str.Append("<div class='pad-all'>" + button + link + "</div>");
            str.Append("<div class='clear-space'></div>");
            break;
        }

        return str.ToString();
    }

    public void DeleteUserShowPopup(string username) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("UserName", username.ToLower()));

        dbCall.CallDelete("aspnet_ShowUpdates", query);
    }
}