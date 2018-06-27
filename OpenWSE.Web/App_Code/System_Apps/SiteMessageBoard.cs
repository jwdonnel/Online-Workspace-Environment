#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.GroupOrganizer;
using System.Web;
using System.Linq;

#endregion

[Serializable]
public class SiteMessageBoard {

    private const string TableName = "MessageBoard";
    private readonly DatabaseCall _dbCall = new DatabaseCall();
    private readonly string _username;
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private readonly Groups _groups = new Groups();

    public SiteMessageBoard(string username) {
        _username = username;
    }

    public void AddPost(string title, string message, string group, string responseToID) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("UserName", _username));
        query.Add(new DatabaseQuery("Title", title));
        query.Add(new DatabaseQuery("Post", HttpUtility.UrlEncode(message)));
        query.Add(new DatabaseQuery("Date", ServerSettings.ServerDateTime.ToString()));
        query.Add(new DatabaseQuery("GroupName", group));
        query.Add(new DatabaseQuery("ResponseToID", responseToID));

        if (_dbCall.CallInsert(TableName, query)) {
            _uuf.addFlag("app-messageboard", group, false);
            _uuf.addFlag("workspace", group, false);
        }
    }

    public List<Dictionary<string, string>> GetEntriesByGroup(string groupname, string sortDir = "DESC") {
        if (((sortDir.ToLower() != "asc") && (sortDir.ToLower() != "desc")) || (string.IsNullOrEmpty(sortDir))) {
            sortDir = "DESC";
        }

        List<Dictionary<string, string>> dataTable = _dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery("GroupName", groupname), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Date " + sortDir);
        List<Dictionary<string, string>> finaldataTable = new List<Dictionary<string, string>>();
        foreach (Dictionary<string, string> entry in dataTable) {
            if (string.IsNullOrEmpty(entry["ResponseToID"])) {
                finaldataTable.Add(CreateEntry(entry));
            }
        }

        return finaldataTable;
    }
    public List<Dictionary<string, string>> GetEntries(string sortDir = "DESC") {
        if (((sortDir.ToLower() != "asc") && (sortDir.ToLower() != "desc")) || (string.IsNullOrEmpty(sortDir))) {
            sortDir = "DESC";
        }

        List<Dictionary<string, string>> dataTable = _dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Date " + sortDir);
        List<Dictionary<string, string>> finaldataTable = new List<Dictionary<string, string>>();
        foreach (Dictionary<string, string> entry in dataTable) {
            if (string.IsNullOrEmpty(entry["ResponseToID"])) {
                finaldataTable.Add(CreateEntry(entry));
            }
        }

        return finaldataTable;
    }
    public List<Dictionary<string, string>> GetResponseEntries(string id) {
        List<Dictionary<string, string>> dataTable = _dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery("ResponseToID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Date DESC");
        List<Dictionary<string, string>> finaldataTable = new List<Dictionary<string, string>>();
        foreach (Dictionary<string, string> entry in dataTable) {
            finaldataTable.Add(CreateEntry(entry));
        }

        return finaldataTable;
    }

    public string GetGroupNameFromMessage(string id) {
        DatabaseQuery dataQuery = _dbCall.CallSelectSingle(TableName, "GroupName", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dataQuery.Value;
    }

    public void DeletePost(string id, string groupname) {
        if (_dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("UserName", _username), new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) })) {
            _uuf.addFlag("app-messageboard", groupname, false);
            _uuf.addFlag("workspace", groupname, false);
        }
    }
    public void DeleteUserPosts(string username) {
        if (_dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("UserName", username), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) })) {
            _uuf.addFlag("app-messageboard", string.Empty, false);
            _uuf.addFlag("workspace", string.Empty, false);
        }
    }
    public void DeleteGroupPosts(string groupname) {
        if (_dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("GroupName", groupname), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) })) {
            _uuf.addFlag("app-messageboard", groupname, false);
            _uuf.addFlag("workspace", groupname, false);
        }
    }

    private Dictionary<string, string> CreateEntry(Dictionary<string, string> entry) {
        Dictionary<string, string> modified = new Dictionary<string, string>();
        modified.Add(DatabaseCall.ApplicationIdString, entry[DatabaseCall.ApplicationIdString]);
        modified.Add("ID", entry["ID"]);
        modified.Add("UserName", entry["UserName"]);
        modified.Add("Title", entry["Title"]);
        modified.Add("Post", ModifyMessage(entry["ID"], entry["Post"], entry["Date"], entry["UserName"], entry["GroupName"]));
        modified.Add("OriginalPost", entry["Post"]);
        modified.Add("Date", entry["Date"]);
        modified.Add("GroupName", entry["GroupName"]);
        modified.Add("ResponseToID", entry["ResponseToID"]);

        return modified;
    }
    private string ModifyMessage(string id, string message, string date, string username, string groupname) {
        message = HttpUtility.UrlDecode(message);

        #region Modify message with links
        // Check if message is a link to an article and grab the contents of the url
        if (message.IndexOf("<p><a href=\"") != -1) {
            try {
                string _link = message.Replace("<p><a href=\"", "");
                string _tempMessage = message.Replace("<p><a href=\"", "");
                string _restofMessage = "";
                if (_tempMessage.IndexOf("\"") > 0) {
                    int indexOf = _tempMessage.IndexOf("\"");
                    int length = Math.Abs(indexOf - (_tempMessage.IndexOf("</p>") + ("</p>").Length));
                    _tempMessage = _tempMessage.Replace(_tempMessage.Substring(indexOf, length), "");

                    _link = _link.Replace(_link.Substring(indexOf), "");
                    _restofMessage = _tempMessage.Replace(_link, "");
                }

                Uri uri = new Uri(_link);
                if (uri.IsAbsoluteUri) {
                    StringBuilder newMessage = new StringBuilder();
                    string _title = HelperMethods.GetWebPageTitle(_link);
                    string _img = HelperMethods.GetWebPageImage(_link);
                    if (!string.IsNullOrEmpty(_title)) {
                        newMessage.Append("<h2><a href='" + _link + "' target='_blank'>" + _title + "</a></h2>");
                        newMessage.Append("<div style='clear: both;'></div>");
                        if (!string.IsNullOrEmpty(_img)) {
                            try {
                                Uri uri2 = new Uri(_img);
                                if (uri2.IsAbsoluteUri)
                                    newMessage.Append("<img alt='' src='" + _img + "' style='max-height: 150px;' />");
                            }
                            catch { }
                        }

                        if ((_link.Contains("http://www.youtube.com/watch")) || (_link.Contains("www.youtube.com/watch")))
                            newMessage.Append(ConvertToObjectEmbeded(_link));

                        newMessage.Append("<div style='clear: both; height: 10px;'></div>");
                        newMessage.Append(_restofMessage);
                        newMessage.Append("<div style='clear: both; height: 5px;'></div>");
                        newMessage.Append("<span style='font-size: 11px!important;'><a href='" + _link + "' target='_blank'>" + _link + "</a></span>");

                        message = newMessage.ToString();
                    }
                }
            }
            catch { }
        }
        #endregion

        MemberDatabase tempmember = new MemberDatabase(username);
        string realGroupNames = "";
        string[] groupList = groupname.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        foreach (string group in groupList) {
            if (!string.IsNullOrEmpty(group)) {
                if (realGroupNames != "") {
                    realGroupNames += ", ";
                }
                realGroupNames += _groups.GetGroupName_byID(group);
            }
        }

        return BuildMessageBody(id, tempmember, realGroupNames, message, date, username.ToLower() == _username.ToLower());
    }
    private static string BuildMessageBody(string id, MemberDatabase tempmember, string realGroupNames, string messageText, string datePosted, bool canShowDelete) {
        StringBuilder str = new StringBuilder();

        str.Append("<div class='message-entry-holder'>");
        str.Append("<div class='message-entry-holder-userInfo'>");
        str.Append("<div class='message-userimage'>" + UserImageColorCreator.CreateImgColor(tempmember.AccountImage, tempmember.UserColor, tempmember.UserId, 35, tempmember.SiteTheme) + "</div>");

        DateTime _date = ServerSettings.ServerDateTime;
        DateTime.TryParse(datePosted, out _date);

        str.Append("<div class='message-username'><span class='message-fullname-text'>" + HelperMethods.MergeFMLNames(tempmember) + "</span>");
        str.Append("<span class='message-username-text'>(" + tempmember.Username + ")</span></div>");

        str.Append("<div class='message-date'>" + HelperMethods.GetPrettyDate(_date) + "</div>");
        str.Append("</div>");

        str.Append("<div class='message-entry-holder-messageInfo'>");
        str.Append("<div class='message-button-holder'>");
        if (canShowDelete) {
            str.Append("<a class='message-delete-btn' onclick=\"messageBoardApp.DeleteMessage('" + id + "');return false\" title='Delete'></a>");
        }
        str.Append("<a class='respond-to-btn' onclick=\"messageBoardApp.RespondToMessage('" + id + "');return false\" title='Reply'></a>");
        str.Append("</div>");

        str.Append("<div class='clear'></div>");
        str.Append("<div class='message-text'>" + messageText + "</div>");
        str.Append("<div class='clear'></div>");

        str.Append("</div>");
        str.Append("<div class='clear'></div>");
        str.Append("</div>");

        return str.ToString();
    }
    private static string ConvertToObjectEmbeded(string msg) {
        string[] del = { "?v=" };
        string[] vidid = msg.Split(del, StringSplitOptions.RemoveEmptyEntries);
        if (vidid.Length > 0) {
            var str = new StringBuilder();

            // EMBEDED VERSION
            str.Append("<object width='450' height='350'>");
            str.Append("<param name='movie' value='https://www.youtube.com/v/" + vidid[1] +
                       "?version=3&autoplay=0'></param>");
            str.Append("<param name='allowScriptAccess' value='always'></param>");
            str.Append("<embed src='https://www.youtube.com/v/" + vidid[1] + "?version=3&autoplay=0' ");
            str.Append("type='application/x-shockwave-flash' ");
            str.Append("allowscriptaccess='always' ");
            str.Append("width='500' height='300'></embed> ");
            str.Append("</object>");

            // IFRAME VERSION (RECOMMENDED)
            //str.Append("<iframe id='ytplayer' type='text/html' width='250' height='150' ");
            //str.Append("src='http://www.youtube.com/embed/"+ vidid[1] + "?autoplay=0' ");
            //str.Append("frameborder='0' />");

            msg = str.ToString();
        }
        return msg;
    }

}