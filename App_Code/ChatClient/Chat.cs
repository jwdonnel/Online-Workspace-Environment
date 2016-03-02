#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Security;
using System.Web;
using System.Data.SqlServerCe;
using OpenWSE.Core;

#endregion


[Serializable]
public class ChatColl
{
    private string _id;
    private string _tUser;
    private string _sUser;
    private bool _flagT;
    private bool _flagS;
    private bool _isNew;
    private string _sMsg;
    private DateTime _sessionDate;
    private DateTime _date;

    public ChatColl(string id, string tUser, string sUser, bool flagT, bool flagS, bool isNew, string sMsg, string sessionDate, string date)
    {
        _id = id;
        _tUser = tUser;
        _sUser = sUser;
        _flagT = flagT;
        _flagS = flagS;
        _isNew = isNew;
        _sMsg = sMsg;

        _sessionDate = new DateTime();
        DateTime.TryParse(sessionDate, out _sessionDate);

        _date = new DateTime();
        DateTime.TryParse(date, out _date);
    }

    public string ID
    {
        get { return _id; }
    }

    public string TUser
    {
        get { return _tUser; }
    }

    public string SUser
    {
        get { return _sUser; }
    }

    public bool FlagT
    {
        get { return _flagT; }
        set { _flagT = value; }
    }

    public bool FlagS
    {
        get { return _flagS; }
        set { _flagS = value; }
    }

    public bool IsNew
    {
        get { return _isNew; }
        set { _isNew = value; }
    }

    public string Message
    {
        get { return _sMsg; }
    }

    public DateTime SessionDate
    {
        get { return _sessionDate; }
    }

    public DateTime Date
    {
        get { return _date; }
    }
}


[Serializable]
public class Chat
{

    #region Private Variables

    private readonly DatabaseCall dbCall = new DatabaseCall();
    private static List<ChatColl> _dataTable = new List<ChatColl>();
    private static readonly Dictionary<string, int> UsertimeoutList = new Dictionary<string, int>();

    #endregion


    #region Constructor

    public Chat(bool getvalues)
    {
        if (getvalues)
        {
            _dataTable.Clear();
            if (UsertimeoutList.Count == 0)
                Load_usertimeout_list();

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_Chat", "", new List<DatabaseQuery>() { new DatabaseQuery("SessionDate", ServerSettings.ServerDateTime.ToShortDateString()), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "Date ASC");
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string tuser = row["tUser"].ToLower();
                string suser = row["sUser"].ToLower();
                string smsg = row["sMsg"];
                string sessiondate = row["SessionDate"];
                string date = row["Date"];
                var coll = new ChatColl(id, tuser, suser, false, false, false, smsg, sessiondate, date);
                _dataTable.Add(coll);
            }
        }
    }

    #endregion


    #region Public Methods

    public void Load_usertimeout_list()
    {
        UsertimeoutList.Clear();
        try {
            MembershipUserCollection usercoll = Membership.GetAllUsers();
            foreach (MembershipUser u in usercoll) {
                var m = new MemberDatabase(u.UserName);
                if (m.ChatEnabled) {
                    try {
                        UsertimeoutList.Add(u.UserName.ToLower(), m.ChatTimeout);
                    }
                    catch { }
                }
            }
        }
        catch { }
    }

    public int GetUserTimeout(string user)
    {
        int timeout;
        UsertimeoutList.TryGetValue(user, out timeout);
        return timeout;
    }

    public void DeleteChatLog() {
        dbCall.CallDelete("aspnet_Chat", new List<DatabaseQuery>() { new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void DeleteUserChats(string userName) {
        dbCall.CallDelete("aspnet_Chat", new List<DatabaseQuery>() { new DatabaseQuery("tUser", userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        dbCall.CallDelete("aspnet_Chat", new List<DatabaseQuery>() { new DatabaseQuery("sUser", userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
    }

    public void AddMessage(string tUser, string sUser, string sMsg)
    {
        string id = Guid.NewGuid().ToString();
        sMsg = StringEncryption.Encrypt(sMsg, "@" + id.Replace("-", "").Substring(0, 15));
        var timenow = ServerSettings.ServerDateTime;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("tUser", tUser.ToLower()));
        query.Add(new DatabaseQuery("sUser", sUser.ToLower()));
        query.Add(new DatabaseQuery("sMsg", sMsg));
        query.Add(new DatabaseQuery("SessionDate", timenow.ToShortDateString()));
        query.Add(new DatabaseQuery("Date", timenow.ToString()));
        if (dbCall.CallInsert("aspnet_Chat", query)) {
            ChatColl coll = new ChatColl(id, tUser.ToLower(), sUser.ToLower(), true, true, true, sMsg, timenow.ToShortDateString(), timenow.ToString());
            _dataTable.Add(coll);

            var member = new MemberDatabase();
            member.SetChatUpdateFlag(true, tUser);
        }
        else {
            System.Threading.Thread.Sleep(500);
            AddMessage(tUser, sUser, sMsg);
        }
    }

    public string GetAllMessages(string tuser, string suser)
    {
        List<string> CurrIds = new List<string>();

        var sResponse = new StringBuilder();
        tuser = tuser.ToLower();
        suser = suser.ToLower();
        CheckIfNextDay();
        for (var i = 0; i < _dataTable.Count; i++)
        {
            DateTime date = _dataTable[i].Date;
            DateTime prevDate = new DateTime();
            string prevUser = string.Empty;
            bool checkPrev = false;
            if ((_dataTable.Count - 1 > 0) && (i != 0))
            {
                prevUser = _dataTable[i - 1].SUser;
                prevDate = _dataTable[i - 1].Date;
                checkPrev = true;
            }

            DateTime now = ServerSettings.ServerDateTime;
            string thistuser = _dataTable[i].TUser;
            string thissuser = _dataTable[i].SUser;
            if ((now.Month == date.Month) && (now.Day == date.Day) &&
                (now.Year == date.Year))
            {
                string id = _dataTable[i].ID;
                string smsg = StringEncryption.Decrypt(_dataTable[i].Message, "@" + id.Replace("-", "").Substring(0, 15));

                int result = 0;
                if (checkPrev)
                    result = date.Subtract(prevDate).Minutes;

                if (((tuser == thistuser) && (suser == thissuser))
                    || ((suser == thistuser) && (tuser == thissuser)))
                {
                    if (!CurrIds.Contains(id)) {
                        CurrIds.Add(id);
                        smsg = ConvertUrlsToLinks(smsg);

                        var member = new MemberDatabase(thissuser);
                        string un = HelperMethods.MergeFMLNames(member);
                        if (thissuser == HttpContext.Current.User.Identity.Name.ToLower()) {
                            un = "You";
                        }

                        if ((prevUser == thissuser) && (result < 1)) {
                            sResponse.Append("<div class='chatLineSep continued-chat'>" + smsg + "</div>");
                        }
                        else {
                            sResponse.Append(BuildMessageLine(member, date, un, id, smsg));
                        }
                    }
                }
            }
        }
        return (sResponse.ToString());
    }

    public string GetLatestMessages(string tuser, string suser)
    {
        var sResponse = new StringBuilder();
        tuser = tuser.ToLower();
        suser = suser.ToLower();
        CheckIfNextDay();
        for (var i = 0; i < _dataTable.Count; i++)
        {
            DateTime date = _dataTable[i].Date;
            DateTime prevDate = new DateTime();
            string prevUser = string.Empty;
            bool checkPrev = false;
            if ((_dataTable.Count - 1 > 0) && (i != 0))
            {
                prevUser = _dataTable[i - 1].SUser;
                prevDate = _dataTable[i - 1].Date;
                checkPrev = true;
            }

            DateTime now = ServerSettings.ServerDateTime;

            string id = _dataTable[i].ID;
            string thistuser = _dataTable[i].TUser;
            string thissuser = _dataTable[i].SUser;

            if ((now.Month == date.Month) && (now.Day == date.Day) && (now.Year == date.Year))
            {
                string smsg = StringEncryption.Decrypt(_dataTable[i].Message, "@" + id.Replace("-", "").Substring(0, 15));

                int result = 0;
                if (checkPrev)
                    result = date.Subtract(prevDate).Minutes;

                if ((tuser == thistuser) && (suser == thissuser))
                {
                    if (_dataTable[i].FlagS) {
                        smsg = ConvertUrlsToLinks(smsg);
                        var member = new MemberDatabase(thissuser);
                        string un = HelperMethods.MergeFMLNames(member);
                        if (thissuser == HttpContext.Current.User.Identity.Name.ToLower()) {
                            un = "You";
                        }

                        if ((prevUser == thissuser) && (result < 1)) {
                            sResponse.Append("<div class='chatLineSep continued-chat date-nodisplay'>" + smsg + "</div>");
                        }
                        else {
                            sResponse.Append(BuildMessageLine(member, date, un, id, smsg));
                        }

                        UpdatesUserFlag(id);
                        break;
                    }
                }
                else if ((suser == thistuser) && (tuser == thissuser))
                {
                    if (_dataTable[i].FlagT) {
                        smsg = ConvertUrlsToLinks(smsg);
                        var member = new MemberDatabase(thissuser);

                        string un = HelperMethods.MergeFMLNames(member);
                        if (thissuser == HttpContext.Current.User.Identity.Name.ToLower()) {
                            un = "You";
                        }

                        if ((prevUser == thissuser) && (result < 1)) {
                            sResponse.Append("<div class='chatLineSep continued-chat date-nodisplay'>" + smsg + "</div>");
                        }
                        else {
                            sResponse.Append(BuildMessageLine(member, date, un, id, smsg));
                        }

                        UpdatetUserFlag(id);
                        break;
                    }
                }
            }
        }

        return (sResponse.ToString());
    }

    public bool TotalFlagsGreaterThanZero(string tuser, string suser)
    {
        for (var i = 0; i < _dataTable.Count; i++)
        {
            suser = suser.ToLower();
            tuser = tuser.ToLower();
            string thistuser = _dataTable[i].TUser;
            string thissuser = _dataTable[i].SUser;

            if ((tuser == thistuser) && (suser == thissuser))
            {
                if (_dataTable[i].FlagS)
                    return true;
            }
            else if ((suser == thistuser) && (tuser == thissuser))
            {
                if (_dataTable[i].FlagT)
                    return true;
            }
        }
        return false;
    }

    public void UpdateMessage(string tuser, string suser)
    {
        try
        {
            for (var i = 0; i < _dataTable.Count; i++)
            {
                tuser = tuser.ToLower();
                suser = suser.ToLower();
                string thistuser = _dataTable[i].TUser;
                string thissuser = _dataTable[i].SUser;
                bool isNew = _dataTable[i].IsNew;

                if (((tuser != thistuser) || (suser != thissuser)) && ((suser != thistuser) || (tuser != thissuser))) continue;
                if (((tuser == thistuser) || (suser != thistuser)) && ((suser == thissuser) || (thissuser != tuser))) continue;
                if (!isNew) continue;

                _dataTable[i].IsNew = false;
            }
        }
        catch { }
    }

    public bool IsNewChat(string tuser, string suser)
    {
        try
        {
            for (var i = 0; i < _dataTable.Count; i++)
            {
                tuser = tuser.ToLower();
                suser = suser.ToLower();
                string thistuser = _dataTable[i].TUser;
                string thissuser = _dataTable[i].SUser;

                if (((tuser == thistuser) && (suser == thissuser))
                    || ((suser == thistuser) && (tuser == thissuser)))
                {
                    if (((tuser != thistuser) && (suser == thistuser))
                        || ((suser != thissuser) && (thissuser == tuser)))
                    {
                        if (_dataTable[i].IsNew)
                            return true;
                    }
                }
            }
        }
        catch { }
        return false;
    }

    #endregion


    #region ChatLog

    public List<ChatColl> GetAllChats
    {
        get {
            List<ChatColl> temp = new List<ChatColl>();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_Chat", "", query, "Date ASC");
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string tuser = row["tUser"].ToLower();
                string suser = row["sUser"].ToLower();
                string smsg = row["sMsg"];
                string sessiondate = row["SessionDate"];
                string date = row["Date"];
                var coll = new ChatColl(id, tuser, suser, false, false, false, smsg, sessiondate, date);
                temp.Add(coll);
            }
            return temp;
        }
    }

    public List<ChatColl> GetChatLog(string dateSelected)
    {
        List<ChatColl> temp = new List<ChatColl>();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_Chat", "", new List<DatabaseQuery>() { new DatabaseQuery("SessionDate", dateSelected), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) }, "Date ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string tuser = row["tUser"].ToLower();
            string suser = row["sUser"].ToLower();
            string smsg = row["sMsg"];
            string sessiondate = row["SessionDate"];
            string date = row["Date"];
            var coll = new ChatColl(id, tuser, suser, false, false, false, smsg, sessiondate, date);
            temp.Add(coll);
        }
        return temp;
    }

    public string GetAllMessages(List<ChatColl> dt, string tuser, string suser)
    {
        var sResponse = new StringBuilder();

        tuser = tuser.ToLower();
        suser = suser.ToLower();
        for (var i = 0; i < dt.Count; i++) {
            DateTime thisDate = dt[i].Date;
            string prevUser = string.Empty;
            DateTime prevDate = new DateTime();
            bool checkPrev = false;
            if ((dt.Count - 1 > 0) && (i != 0)) {
                prevUser = dt[i - 1].SUser;
                prevDate = dt[i - 1].Date;
                checkPrev = true;
            }

            DateTime now = ServerSettings.ServerDateTime;
            string thistuser = dt[i].TUser;
            string thissuser = dt[i].SUser;
            string id = dt[i].ID;
            string smsg = StringEncryption.Decrypt(dt[i].Message, "@" + id.Replace("-", "").Substring(0, 15));

            int result = 0;
            if (checkPrev)
                result = thisDate.Subtract(prevDate).Minutes;

            if (((tuser == thistuser) && (suser == thissuser))
                || ((suser == thistuser) && (tuser == thissuser))) {
                smsg = ConvertUrlsToLinks(smsg, 500, 300);

                var member = new MemberDatabase(thissuser);
                string yourClass = "style='padding: 5px; white-space: normal; position: relative;";
                string un = HelperMethods.MergeFMLNames(member);
                if (thissuser == HttpContext.Current.User.Identity.Name.ToLower()) {
                    un = "You";
                    yourClass += "background:#EFEFEF;";
                }

                sResponse.Append("<div " + yourClass + "'>");
                if ((prevUser == thissuser) && (result < 1)) {
                    sResponse.Append("<div style='padding-left: 65px;'><span class='smsg-text'>" + smsg + "</span></div></div>");
                }
                else {
                    sResponse.Append("<table cellpadding='0' cellspacing='0' style='width: 100%;'><tr>");

                    sResponse.Append("<td valign='top' style='width: 105px;'>" + UserImageColorCreator.CreateImgColor(member.AccountImage, member.UserColor, member.UserId, 40) + "</td>");
                    sResponse.Append("<td valign='top'><div>");

                    sResponse.Append("<h3 style='font-family:Arial,Helvetica,sans-serif;font-size:13px;float:left;color:#5F5F5F;font-weight:bold;padding-bottom:5px'>" + un + "</h3>");
                    sResponse.Append("<span class='float-right' style='color: #707070; font-size: 11px;'>" + thisDate.ToShortTimeString() + " " + AbbreviateTimeZone() + "</span>");
                    sResponse.Append("<div class='clear'></div>");
                    sResponse.Append("<span class='smsg-text'>" + smsg + "</span></div>");
                    sResponse.Append("</div></td></tr></table></div>");
                }
            }
        }

        return (sResponse.ToString());
    }

    private static string AbbreviateTimeZone() {
        string[] splitName = TimeZone.CurrentTimeZone.StandardName.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        string abbrName = string.Empty;
        foreach (string sn in splitName) {
            if (sn.Length > 0) {
                // Take the first letter of each word
                abbrName += sn[0];
            }
        }

        return abbrName;
    }

    public string GettUser(string id)
    {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_Chat", "tUser", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public string GetsUser(string id)
    {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_Chat", "sUser", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public DateTime GetDate(string id)
    {
        DateTime temp = new DateTime();
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("aspnet_Chat", "Date", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        DateTime.TryParse(dbSelect.Value, out temp);
        return temp;
    }

    #endregion


    #region Private Methods

    private void UpdatesUserFlag(string id)
    {
        foreach (var dr in _dataTable.Cast<ChatColl>().Where(dr => id == dr.ID))
        {
            dr.FlagS = false;
            break;
        }
    }

    private void UpdatetUserFlag(string id)
    {
        foreach (var dr in _dataTable.Cast<ChatColl>().Where(dr => id == dr.ID))
        {
            dr.FlagT = false;
            break;
        }
    }

    private void CheckIfNextDay()
    {
        if (_dataTable.Count > 0)
        {
            DateTime date = _dataTable[0].SessionDate;
            int totalTime = ServerSettings.ServerDateTime.Subtract(date).Days;
            if (totalTime > 0)
                _dataTable.Clear();
        }
    }


    private string BuildMessageLine(MemberDatabase member, DateTime date, string un, string id, string smsg) {
        StringBuilder sResponse = new StringBuilder();
        string yourClass = " float-left pad-left-sml pad-right-big";
        if (un.ToLower() != "you") {
            yourClass = " float-right pad-right-sml pad-left-big";
        }

        sResponse.Append("<div class='chatLineSep" + yourClass + "'>");
        sResponse.Append("<table cellpadding='0' cellspacing='0'><tr>");

        bool usesImg = false;
        if (!string.IsNullOrEmpty(member.AccountImage)) {
            usesImg = true;
        }

        if (un.ToLower() == "you") {
            sResponse.Append(BuildUserImagePart(member, true));
            sResponse.Append(BuildMessagePart(un, date, id, smsg, usesImg, false));
        }
        else {
            sResponse.Append(BuildMessagePart(un, date, id, smsg, usesImg, true));
            sResponse.Append(BuildUserImagePart(member, false));
        }

        sResponse.Append("</tr></table></div>");

        return sResponse.ToString();
    }
    private string BuildUserImagePart(MemberDatabase member, bool alignImgLeft) {
        string img = string.Empty;
        string imgClass = string.Empty;

        string acctImg = member.AccountImage;

        if (!string.IsNullOrEmpty(acctImg)) {
            string imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + member.UserId + "/" + acctImg);
            if (acctImg.ToLower().Contains("http") || acctImg.ToLower().Contains("www.")) {
                imgLoc = acctImg;
            }

            img = "<img alt='' src='" + imgLoc + "' />";
            imgClass = " class='chat-acctImage'";
        }

        string colorAlign = "left: 0;'";
        if (!alignImgLeft) {
            colorAlign = "right: 0;'";
        }

        return "<td valign='top' align='center'" + imgClass + "><div class='sch_ColorCode' style='background-color: #" + member.UserColor + ";" + colorAlign + "'></div>" + img + "</td>";
    }
    private string BuildMessagePart(string un, DateTime date, string id, string smsg, bool usesImg, bool flipDateName) {
        StringBuilder sResponse = new StringBuilder();

        if (usesImg) {
            sResponse.Append("<td valign='top'><div>");
        }
        else {
            if (un.ToLower() == "you") {
                sResponse.Append("<td valign='top'><div class='pad-left-big'>");
            }
            else {
                sResponse.Append("<td valign='top'><div class='pad-right-big'>");
            }
        }

        if (!flipDateName) {
            sResponse.Append("<h3>" + un + "</h3>");
            sResponse.Append("<span class='date-chat-line margin-left-big'>" + date.ToShortTimeString() + ServerSettings.StringDelimiter + TimeZone.CurrentTimeZone.GetUtcOffset(date).Hours + "</span>");
            sResponse.Append("<div class='clear'></div>");
            sResponse.Append("<span id='ch_" + id + "' class='date-nodisplay'></span>");
            sResponse.Append("<span class='smsg-text'>" + smsg + "</span>");
            sResponse.Append("</div>");
        }
        else {
            sResponse.Append("<h3 style='float: right!important;'>" + un + "</h3>");
            sResponse.Append("<span class='date-chat-line margin-right-big' style='float: left!important;'>" + date.ToShortTimeString() + ServerSettings.StringDelimiter + TimeZone.CurrentTimeZone.GetUtcOffset(date).Hours + "</span>");
            sResponse.Append("<div class='clear'></div>");
            sResponse.Append("<span id='ch_" + id + "' class='date-nodisplay'></span>");
            sResponse.Append("<span class='smsg-text'>" + smsg + "</span>");
            sResponse.Append("</div>");
        }

        sResponse.Append("</td>");

        return sResponse.ToString();
    }


    private static string ConvertUrlsToLinks(string msg)
    {
        if ((msg.Contains("http://www.youtube.com/watch")) || (msg.Contains("www.youtube.com/watch")))
            return ConvertToObjectEmbeded(msg);
        else if (msg.Contains("chat-message-image"))
            return msg;

        const string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
        var r = new Regex(regex, RegexOptions.IgnoreCase);
        return r.Replace(msg, "<a href=\"$1\" title=\"Click to open in a new window or tab\" style=\"text-decoration: underline; color: #0077CC;\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
    }

    private static string ConvertUrlsToLinks(string msg, int width, int height)
    {
        if ((msg.Contains("<img alt=''")) && (msg.Contains("chat-message-image")))
            return msg;

        if ((msg.Contains("http://www.youtube.com/watch")) || (msg.Contains("www.youtube.com/watch")))
            return ConvertToObjectEmbeded(msg, width, height);

        const string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
        var r = new Regex(regex, RegexOptions.IgnoreCase);
        return r.Replace(msg, "<a href=\"$1\" title=\"Click to open in a new window or tab\" style=\"text-decoration: underline; color: #0077CC;\" target=\"&#95;blank\">$1</a>")
             .Replace("href=\"www", "href=\"http://www");
    }

    private static string ConvertToObjectEmbeded(string msg)
    {
        string[] del = { "?v=", "&v=" };
        string[] vidid = msg.Split(del, StringSplitOptions.RemoveEmptyEntries);
        if (vidid.Length > 1)
        {
            var str = new StringBuilder();

            // EMBEDED VERSION
            str.Append("<object width='240' height='200'>");
            str.Append("<param name='movie' value='https://www.youtube.com/v/" + vidid[1] +
                       "?version=3&autoplay=0'></param>");
            str.Append("<param name='allowScriptAccess' value='always'></param>");
            str.Append("<embed src='https://www.youtube.com/v/" + vidid[1] + "?version=3&autoplay=0' ");
            str.Append("type='application/x-shockwave-flash' ");
            str.Append("allowscriptaccess='always' ");
            str.Append("width='240' height='200'></embed> ");
            str.Append("</object>");

            // IFRAME VERSION (RECOMMENDED)
            //str.Append("<iframe id='ytplayer' type='text/html' width='250' height='150' ");
            //str.Append("src='http://www.youtube.com/embed/"+ vidid[1] + "?autoplay=0' ");
            //str.Append("frameborder='0' />");

            msg = str.ToString();
        }
        return msg;
    }

    private static string ConvertToObjectEmbeded(string msg, int width, int height)
    {
        string[] del = { "?v=", "&v=" };
        string[] vidid = msg.Split(del, StringSplitOptions.RemoveEmptyEntries);
        if (vidid.Length > 1)
        {
            var str = new StringBuilder();

            // EMBEDED VERSION
            str.Append("<object width='" + width + "' height='" + height + "'>");
            str.Append("<param name='movie' value='https://www.youtube.com/v/" + vidid[1] +
                       "?version=3&autoplay=0'></param>");
            str.Append("<param name='allowScriptAccess' value='always'></param>");
            str.Append("<embed src='https://www.youtube.com/v/" + vidid[1] + "?version=3&autoplay=0' ");
            str.Append("type='application/x-shockwave-flash' ");
            str.Append("allowscriptaccess='always' ");
            str.Append("width='" + width + "' height='" + height + "'></embed> ");
            str.Append("</object>");

            // IFRAME VERSION (RECOMMENDED)
            //str.Append("<iframe id='ytplayer' type='text/html' width='250' height='150' ");
            //str.Append("src='http://www.youtube.com/embed/"+ vidid[1] + "?autoplay=0' ");
            //str.Append("frameborder='0' />");

            msg = str.ToString();
        }
        return msg;
    }

    #endregion

}