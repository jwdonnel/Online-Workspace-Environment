using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

/// <summary>
/// Summary description for UserImageColorCreator
/// </summary>
public class UserImageColorCreator
{

    public UserImageColorCreator() { }

    /// <summary>
    /// Create the User's Account Image and Color for the GroupOrg, UserAccounts, UsersAndApps, and Apps
    /// </summary>
    /// <param name="acctImage"></param>
    /// <param name="usercolor"></param>
    /// <param name="userId"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static string CreateImgColor(string acctImage, string usercolor, string userId, int height) {
        string result = string.Empty;
        if (!string.IsNullOrEmpty(acctImage)) {
            string imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + userId + "/" + acctImage);
            if (acctImage.ToLower().Contains("http") || acctImage.ToLower().Contains("www.")) {
                imgLoc = acctImage;
            }
            string img = "<img alt='' src='" + imgLoc + "' class='float-left' style='height:" + height + "px;' />";
            result = "<div class='float-left'>" + img + "<div class='sch_ColorCode' style='background-color: #" + usercolor + "; top: 0px; height:" + height + "px;'></div></div>";
        }
        else {
            result = "<div class='float-left'><div class='sch_ColorCode rounded-corners-10' style='background-color: #" + usercolor + "; margin-top: 3px;'></div></div>";
        }

        return result;
    }

    /// <summary>
    /// Create the User's Account Image and Color for the SiteMasters and the AppRemote
    /// </summary>
    /// <param name="acctImage"></param>
    /// <param name="usercolor"></param>
    /// <param name="userId"></param>
    /// <param name="userFullName"></param>
    /// <param name="siteTheme"></param>
    /// <returns></returns>
    public static string CreateImgColorTopBar(string acctImage, string usercolor, string userId, string userFullName, string siteTheme) {
        string img = string.Empty;
        if (!string.IsNullOrEmpty(acctImage)) {
            string imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + userId + "/" + acctImage);
            if (acctImage.ToLower().Contains("http") || acctImage.ToLower().Contains("www.")) {
                imgLoc = acctImage;
            }
            img = "<img alt='' src='" + imgLoc + "' class='top-menu-acctImage'>";
        }

        return img + "<div class='sch_ColorCode' style='background-color: #" + usercolor + "'></div>" + userFullName;
    }

    /// <summary>
    /// Create the User's Account Image and Color for the Chat Client user list
    /// </summary>
    /// <param name="acctImage"></param>
    /// <param name="usercolor"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static string CreateImgColorChatList(string acctImage, string usercolor, string userId) {
        string uc = string.Empty;
        if (!string.IsNullOrEmpty(acctImage)) {
            string imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + userId + "/" + acctImage);
            if (acctImage.ToLower().Contains("http") || acctImage.ToLower().Contains("www.")) {
                imgLoc = acctImage;
            }
            uc += "<img alt='' src='" + imgLoc + "' class='chat-acctImage'>";
        }

        uc += "<div class='sch_ColorCode_chat' style='background-color: " + usercolor + "'></div>";
        return uc;
    }

}