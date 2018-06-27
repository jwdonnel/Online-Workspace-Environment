<%@ WebService Language="C#" Class="GetChatSettings" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Text;
using System.Collections.Generic;
using System.Web.Security;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class GetChatSettings : System.Web.Services.WebService
{
    private bool _canContinue = false;
    private string _userName;
    private MemberDatabase _member;
    
    public GetChatSettings()
    {
        System.Security.Principal.IIdentity userId = HttpContext.Current.User.Identity;
        if (userId.IsAuthenticated)
        {
            _canContinue = true;
            _userName = userId.Name;
            _member = new MemberDatabase(_userName);
        }
    }
    
    [WebMethod]
    public string LoadMessage(string type, string messageId)
    {
        StringBuilder str = new StringBuilder();
        if (_canContinue)
        {
            if (type == "chat")
            {
                var chat = new Chat(false);
                string id = messageId.Replace("!chat", string.Empty);
                str.Append("<div style='float: right;font-family: Arial; font-size: 1.0em;'>" + chat.GetDate(id).ToShortDateString() + "</div>");
                str.Append("<div style='float: left;font-family: Arial; font-size: 1.3em; font-width: bold'>Chat Log for " +
                           chat.GetsUser(id) + " and " + chat.GettUser(id) + "</div>");
                str.Append(
                    "<div style='clear: both; height:5px;'></div><hr /><div style='clear: both; height:3px;'></div>");
                str.Append(chat.GetAllMessages(chat.GetChatLog(chat.GetDate(id).ToShortDateString()), chat.GettUser(id), chat.GetsUser(id)));
            }
        }

        return str.ToString();
    }

    [WebMethod]
    public string DeleteLog(string messageDate)
    {
        if (_canContinue)
        {
            ChatLogsDeleted cld = new ChatLogsDeleted(_userName);
            cld.AddEntry(messageDate);
        }

        return "";
    }

    [WebMethod]
    public string GetChatTimeout()
    {
        string str = string.Empty;
        if (_canContinue)
            str = _member.ChatTimeout.ToString();

        return str;
    }

    [WebMethod]
    public string UpdateSettingSound(string allow)
    {
        if (_canContinue)
            _member.UpdateChatSoundNoti(HelperMethods.ConvertBitToBoolean(allow));

        return "";
    }

    [WebMethod]
    public string UpdateSettingTimeout(string time)
    {
        if (_canContinue)
        {
            int timeout = 10;
            if (!string.IsNullOrEmpty(time))
            {
                int.TryParse(time, out timeout);
                _member.UpdateChatTimeout(timeout.ToString());
                Chat chat = new Chat(false);
                chat.Load_usertimeout_list();
            }
        }
        
        return "";
    }

    [WebMethod]
    public string LoadBlockedUsers()
    {
        StringBuilder str = new StringBuilder();
        if (_canContinue)
            str.Append(BuildBlockedUsers);

        return str.ToString();
    }

    [WebMethod]
    public string BlockUser(string block, string userName)
    {
        if (_canContinue)
        {
            bool blockUser = HelperMethods.ConvertBitToBoolean(block);
            BlockedChats blockedChats = new BlockedChats(_userName);
            if (blockUser)
                blockedChats.AddItem(userName);
            else
                blockedChats.DeleteEntryByChatUser(userName);
        }
        
        return "";
    }
    
    private string BuildBlockedUsers
    {
        get
        {
            StringBuilder str = new StringBuilder();

            List<string>  newChatList = new List<string>();
            var mdata = new MemberDatabase(_member.Username);
            List<string> dataTable = mdata.ChattableUserList(_member.Username);
            BlockedChats blockedChats = new BlockedChats(_member.Username);
            blockedChats.BuildEntries();
            foreach (string dr in dataTable)
            {
                mdata = new MemberDatabase(dr);
                if (!HelperMethods.CompareUserGroups(_member, mdata)) continue;
                MembershipUser u = Membership.GetUser(dr);
                str = BuildUserList(mdata, u, _member.Username, str, blockedChats);
            }

            return str.ToString();
        }
    }

    private StringBuilder BuildUserList(MemberDatabase mdata, MembershipUser u, string currUser, StringBuilder str, BlockedChats blockedChats)
    {
        string userstatus = string.Empty;
        string cl = getStatusClass(mdata.ChatStatus);
        if (!u.IsOnline)
            cl = "statusUserOffline";

        if ((mdata.ChatStatus == "Away") && (u.IsOnline))
            userstatus = "Away";

        if ((mdata.ChatStatus == "Busy") && (u.IsOnline))
            userstatus = "Busy";

        string acctImg = mdata.AccountImage;
        string uc = UserImageColorCreator.CreateImgColor(acctImg, mdata.UserColor, mdata.UserId, 35, mdata.SiteTheme);
        if (string.IsNullOrEmpty(acctImg)) {
            uc = "<div class='float-left'>" + uc + "</div>";
        }
        str.Append("<div class='float-left margin-right-big' style='width: 210px; height: 37px;'>" + uc + "<div class='float-left margin-left-sml' style='padding-top: 9px;'>" + HelperMethods.MergeFMLNames(mdata) + "</div>");

        string isChecked = "";
        if (blockedChats.CheckIfBlocked(u.UserName))
            isChecked = " checked='checked'";

        str.Append("<input id='chatblock_" + u.UserName.Replace(" ", "_") + "' type='checkbox' class='float-left chatblock-checked'" + isChecked + " style='margin-left: 10px; margin-top: 11px;' /></div>");
        return str;
    }

    private string getStatusClass(string status)
    {
        string c;
        switch (status)
        {
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
            default:
                c = "statusUserOnline";
                break;
        }
        return c;
    }

    private static String RgbConverter(System.Drawing.Color c)
    {
        return "rgb(" + c.R.ToString() + ", " + c.G.ToString() + ", " + c.B.ToString() + ")";
    }
    
}