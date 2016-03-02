#region

using System;
using System.Drawing;
using System.Globalization;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Script.Services;
using System.Web.Security;
using System.Web.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

#endregion

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class ChatService : WebService {
    #region Variables
    public readonly static Dictionary<string, bool> _needRefreshUsers = new Dictionary<string, bool>();
    public readonly static Dictionary<string, bool> _needRefreshMessager = new Dictionary<string, bool>();
    private static Chat _chat;
    private readonly object[] _array = new object[3];
    private readonly bool _canContinue;
    private readonly MemberDatabase _member;
    private readonly StringBuilder _result = new StringBuilder();
    private readonly object _sync = new object();
    private readonly IIdentity _userId;
    private List<string> newChatList = new List<string>();
    private int _countU1;
    private string _tempuser = string.Empty;
    #endregion


    public ChatService() {
        _userId = HttpContext.Current.User.Identity;
        if (!_userId.IsAuthenticated) return;
        _canContinue = true;
        _member = new MemberDatabase(_userId.Name);

        if (_chat == null)
            _chat = new Chat(true);
    }


    #region Async Updater

    private ChatUpdaterDelegate _su;

    public void SiteUpdater() {
        _su = _ChatUpdater;
    }

    private bool _ChatUpdater(string userto, string isPostBack) {
        return BuildMessages_Check(userto, isPostBack);
    }

    public bool CheckChatData(string userto, string isPostBack) {
        IAsyncResult result = _su.BeginInvoke(userto, isPostBack, null, @_sync);
        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
        while (!result.IsCompleted)
            Thread.Sleep(0);

        bool returnvalue = _su.EndInvoke(result);
        return returnvalue;
    }

    private delegate bool ChatUpdaterDelegate(string userto, string isPostBack);

    #endregion


    #region WebMethods

    [WebMethod]
    public string[] LoadFontStyle() {
        List<string> returnList = new List<string>();
        if (_canContinue) {
            returnList.Add(CustomFonts.GetCustomFontFamily(_member));
            returnList.Add(CustomFonts.GetCustomFontSize(_member));
            returnList.Add(CustomFonts.GetCustomFontColor(_member));
        }

        return returnList.ToArray();
    }

    [WebMethod]
    public string CallUpdateStatus(string status) {
        if (_canContinue) {
            UpdateStatus(status);
            _result.Append(GetUserStatus);
        }
        return _result.ToString();
    }


    [WebMethod]
    public string CallWindowFocus() {
        if (_canContinue) {
            if (_member.IsAway)
                UpdateStatus("Available");
            _result.Append(GetUserStatus);
        }
        return _result.ToString();
    }


    [WebMethod]
    public string CallWindowBlur() {
        if (_canContinue) {
            if ((_member.ChatStatus != "Offline") && (_member.ChatStatus != "Busy")) {
                _member.UpdateIsAway(true);
                UpdateStatus("Away");
            }
            _result.Append(GetUserStatus);
        }
        return _result.ToString();
    }


    [WebMethod]
    public object[] CallUserList(string currstatus, string isPostBack) {
        if (_canContinue) {
            bool ok = false;
            if (HelperMethods.ConvertBitToBoolean(isPostBack))
                ok = true;
            else {
                try {
                    string userName = _userId.Name.ToLower();
                    if (!_needRefreshUsers.ContainsKey(userName))
                        _needRefreshUsers.Add(userName, false);

                    for (int i = 0; i < 100; i++) {
                        if (GetUserTimeout_Check) {
                            ok = true;
                            break;
                        }
                        if (_needRefreshUsers[userName]) {
                            _needRefreshUsers[userName] = false;
                            ok = true;
                            break;
                        }
                        Thread.Sleep(400);
                    }
                }
                catch { }
            }

            if (ok) {
                _array[0] = GetUserStatus;
                _array[1] = GetUserList;
                _array[2] = newChatList.ToArray();
            }
        }
        return _array;
    }


    [WebMethod]
    public object[] CallMessages(string userto, string isPostBack) {
        if (_canContinue) {
            bool ok = false;
            if (HelperMethods.ConvertBitToBoolean(isPostBack))
                ok = true;
            else {
                SiteUpdater();

                string userName = _userId.Name.ToLower();
                if (!_needRefreshMessager.ContainsKey(userName))
                    _needRefreshMessager.Add(userName, false);

                for (int i = 0; i < 100; i++) {
                    if (_needRefreshMessager[userName]) {
                        _needRefreshMessager[userName] = false;
                        ok = true;
                        break;
                    }
                    else {
                        bool returnvalue = CheckChatData(userto, isPostBack);
                        if (returnvalue) {
                            ok = true;
                            break;
                        }
                    }
                    Thread.Sleep(400);
                }
            }

            if (ok) {
                _array[0] = BuildMessages(userto, isPostBack);
            }
        }
        return _array;
    }


    [WebMethod]
    public object[] CallAddMessage(string message, string userto) {
        if (_canContinue) {
            message = HttpUtility.UrlDecode(message);
            if ((!string.IsNullOrEmpty(message)) && (!string.IsNullOrEmpty(userto))) {
                _chat.AddMessage(userto, _userId.Name, message);

                MembershipUser temp = Membership.GetUser(_userId.Name);
                if (temp != null) {
                    temp.LastActivityDate = ServerSettings.ServerDateTime;
                    Membership.UpdateUser(temp);
                }
                _member.UpdateChatTimeStamp();
                _member.UpdateIsTyping(false);

                string username2 = userto.ToLower();
                if (_needRefreshUsers.ContainsKey(username2))
                    _needRefreshUsers[username2] = true;
                else
                    _needRefreshUsers.Add(username2, true);

                if (_needRefreshMessager.ContainsKey(username2))
                    _needRefreshMessager[username2] = true;
                else
                    _needRefreshMessager.Add(username2, true);

                return BuildMessages(username2, "0");
            }
        }
        return null;
    }


    [WebMethod]
    public void CallIsTyping(string typing, string userto) {
        if (_canContinue) {
            bool temp = HelperMethods.ConvertBitToBoolean(typing);
            _member.UpdateIsTyping(temp);

            userto = userto.ToLower();
            if (!HelperMethods.ConvertBitToBoolean(typing))
                CallUpdate(userto);

            if (_needRefreshMessager.ContainsKey(userto)) {
                if (!_needRefreshMessager[userto])
                    _needRefreshMessager[userto] = true;
            }
            else
                _needRefreshMessager.Add(userto, true);
        }
    }


    [WebMethod]
    public string CallUpdate(string userto) {
        if (!string.IsNullOrEmpty(userto)) {
            userto = userto.ToLower();
            _chat.UpdateMessage(userto, _userId.Name);
            if ((_chat.IsNewChat(userto, _userId.Name))
                || (!_chat.IsNewChat(_userId.Name, userto))) {
                _result.Clear();
                _result.Append("false");
            }
            else if ((!_chat.IsNewChat(userto, _userId.Name))
                     || (_chat.IsNewChat(_userId.Name, userto))) {
                _result.Clear();
                _result.Append("false");
            }

            string userName = _userId.Name.ToLower();
            if (!_needRefreshUsers.ContainsKey(userName))
                _needRefreshUsers.Add(userName, true);
            else
                _needRefreshUsers[userName] = true;
        }

        return _result.ToString();
    }

    #endregion


    #region Check if need update

    private bool BuildMessages_Check(string userto, string isPostBack) {
        bool canContinue = true;
        string ut = userto;
        try {
            if (!string.IsNullOrEmpty(ut)) {
                MembershipUser tempUser = Membership.GetUser(ut);
                if (tempUser == null)
                    tempUser = Membership.GetUser(ut.Replace("_", " "));
                if (tempUser == null)
                    canContinue = false;
            }
            if (canContinue) {
                if ((HelperMethods.ConvertBitToBoolean(isPostBack)) || (_chat.TotalFlagsGreaterThanZero(ut, _userId.Name.ToLower())))
                    return true;
            }
        }
        catch { }
        return false;
    }

    private bool GetUserTimeout_Check {
        get {
            string currentStatus = _member.ChatStatus;
            if ((currentStatus != "Offline") && (currentStatus != "Busy")) {
                TimeSpan timeframe = ServerSettings.ServerDateTime.Subtract(_member.LastUpdated);
                int timeout = _chat.GetUserTimeout(_member.Username.ToLower());
                if (timeframe.Minutes > timeout) {
                    return true;
                }
            }
            return false;
        }
    }

    #endregion


    #region Support Methods

    [WebMethod]
    public string[] GetEmoticons() {
        List<string> icons = new List<string>();
        string serverPath = ServerSettings.GetServerMapLocation + "ChatClient\\Emoticons";
        if (Directory.Exists(serverPath)) {
            string[] files = Directory.GetFiles(serverPath);
            Array.Sort(files);
            foreach (string file in files) {
                FileInfo fi = new FileInfo(file);
                if ((!icons.Contains("Emoticons/" + fi.Name)) && (fi.Name.ToLower() != "thumbs.db")) {
                    icons.Add("Emoticons/" + fi.Name);
                }
            }
        }

        return icons.ToArray();
    }

    private string GetUserList {
        get {
            var str = new StringBuilder();
            str.Append("<ul class='listofusers'>");
            newChatList = new List<string>();
            var mdata = new MemberDatabase(_userId.Name);
            List<string> dataTable = mdata.ChattableUserList(_userId.Name);

            BlockedChats blockedChats = new BlockedChats(_userId.Name);
            blockedChats.BuildEntries();

            int count = 0;
            foreach (string dr in dataTable) {
                mdata = new MemberDatabase(dr);
                if (!HelperMethods.CompareUserGroups(_member, mdata)) continue;
                if (blockedChats.CheckIfBlocked(dr)) continue;

                MembershipUser u = Membership.GetUser(dr);
                str = BuildUserList(mdata, u, _userId.Name, str);
                count++;
            }

            str.Append("</ul><div class='display-none'><span id='chattersOnline'>" + _countU1 + "</span></div>");
            return str.ToString();
        }
    }

    private string GetUserStatus {
        get {
            string currStatusClass = _member.ChatStatus;
            bool getStatus = true;
            if ((currStatusClass != "Offline") && (currStatusClass != "Busy")) {
                TimeSpan timeframe = ServerSettings.ServerDateTime.Subtract(_member.LastUpdated);
                int minutesCompare = _chat.GetUserTimeout(_userId.Name.ToLower());
                if (timeframe.Minutes > minutesCompare) {
                    _member.UpdateIsAway(true);
                    UpdateStatus("Away");
                    currStatusClass = "statusUserAway";
                    getStatus = false;
                }
            }

            if (getStatus) {
                string currentStatus = string.IsNullOrEmpty(currStatusClass) ? "Offline" : _member.ChatStatus;

                switch (currentStatus) {
                    case "Busy":
                        currStatusClass = "statusUserBusy";
                        break;
                    case "Offline":
                        currStatusClass = "statusUserOffline";
                        break;
                    case "Available":
                        currStatusClass = "statusUserOnline";
                        break;
                    case "Away":
                        currStatusClass = "statusUserAway";
                        break;
                    default:
                        currStatusClass = "statusUserOnline";
                        break;
                }
            }

            return "<div class='statusUserDiv2 " + currStatusClass + "'></div><span id='span_currStatus' class='float-left'>" + _member.ChatStatus + "</span>";
        }
    }

    public void UpdateStatus(string status) {
        string currentStatus;

        switch (status) {
            case "Out":
                currentStatus = "Offline";
                _member.UpdateIsAway(false);
                break;
            case "Away":
                currentStatus = "Away";
                break;
            case "Available":
                currentStatus = "Available";
                _member.UpdateIsAway(false);
                break;
            default:
                currentStatus = status;
                _member.UpdateIsAway(false);
                break;
        }

        //if (currentStatus == _member.GetChatStatus) return;
        _member.UpdateChatTimeStamp();
        _member.UpdateStatusChanged(true);
        _member.UpdateChatStatus(currentStatus);
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser msu in coll) {
            string un = msu.UserName.ToLower();
            if ((msu.IsOnline) && (un != _userId.Name.ToLower())) {
                if (_needRefreshUsers.ContainsKey(un))
                    _needRefreshUsers[un] = true;
                else
                    _needRefreshUsers.Add(un, true);

                if (_needRefreshMessager.ContainsKey(un))
                    _needRefreshMessager[un] = true;
                else
                    _needRefreshMessager.Add(un, true);
            }
        }
    }

    private StringBuilder BuildUserList(MemberDatabase mdata, MembershipUser u, string currUser, StringBuilder str) {
        string userstatus = string.Empty;
        string currStatus = mdata.ChatStatus;
        string cl = getStatusClass(currStatus, mdata);
        if (!u.IsOnline)
            cl = "statusUserOffline";

        if ((cl != "statusUserOffline") && (u.IsOnline))
            _countU1++;

        if ((currStatus == "Away") && (u.IsOnline))
            userstatus = "Away";

        if ((currStatus == "Busy") && (u.IsOnline))
            userstatus = "Busy";

        string fullName = HelperMethods.MergeFMLNames(mdata);

        if (_chat.IsNewChat(u.UserName, currUser)) {
            if (!newChatList.Contains(mdata.UserId + "~|~" + fullName))
                newChatList.Add(mdata.UserId + "~|~" + fullName);
        }

        string userColor = RgbConverter(ColorTranslator.FromHtml("#" + mdata.UserColor));
        string uc = UserImageColorCreator.CreateImgColorChatList(mdata.AccountImage, userColor, mdata.UserId);

        str.Append("<li><div class='ChatUserNotSelected' chat-username='" + u.UserName + "'><div class='statusUserDiv " + cl + "'></div>" + uc + "<span class='cb-links usersclick' chat-userid='" + mdata.UserId + "'>" + fullName + "</span>");
        if (userstatus != "")
            str.Append("</div></li>");

        else
            str.Append("</div></li>");

        return str;
    }

    private static String RgbConverter(Color c) {
        return "rgb(" + c.R.ToString() + ", " + c.G.ToString() + ", " + c.B.ToString() + ")";
    }

    private string getStatusClass(string status, MemberDatabase mdata) {
        string c = "statusUserOffline";
        BlockedChats blockedChats = new BlockedChats(_userId.Name);
        if (!blockedChats.CheckIfBeingBlocked(mdata.Username)) {
            switch (status) {
                case "Available":
                    c = "statusUserOnline";
                    break;
                case "Busy":
                    c = "statusUserBusy";
                    break;
                case "Away":
                    c = "statusUserAway";
                    break;
                case "Offline":
                    c = "statusUserOffline";
                    break;
            }
        }

        return c;
    }

    private object[] BuildMessages(string userto, string isPostBack) {
        var messageStr = new object[3];
        try {
            string ut = userto;
            if (!string.IsNullOrEmpty(ut)) {
                var tempUser = Membership.GetUser(ut);
                if (tempUser == null) {
                    ut = ut.Replace("_", " ");
                    tempUser = Membership.GetUser(ut);
                }
                var mdMessages = new MemberDatabase(ut);
                if (tempUser != null) {
                    BlockedChats blockedChats = new BlockedChats(_userId.Name);
                    if (!blockedChats.CheckIfBeingBlocked(userto))
                        messageStr[0] = (tempUser.IsOnline) && (mdMessages.ChatStatus != "Offline") ? "online" : "offline";
                    else
                        messageStr[0] = "offline";

                    if (HelperMethods.ConvertBitToBoolean(isPostBack)) {
                        messageStr[1] = _chat.GetAllMessages(ut, _userId.Name);
                        if (string.IsNullOrEmpty(messageStr[1].ToString()))
                            messageStr[1] = "<div class='chatLineSep-noposts'>No posts available</div>";
                    }
                    else if (_chat.TotalFlagsGreaterThanZero(ut, _userId.Name))
                        messageStr[1] = _chat.GetLatestMessages(ut, _userId.Name);


                    if (mdMessages.IsTyping)
                        messageStr[2] = "istyping";
                    else
                        messageStr[2] = string.Empty;
                }
            }
        }
        catch { }
        return messageStr;
    }

    #endregion
}