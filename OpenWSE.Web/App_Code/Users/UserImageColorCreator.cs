using System;
using System.Collections.Generic;
using System.IO;
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
    /// Creates the User's Account Image and Color for the GroupOrg, UserAccounts, UsersAndApps, and Apps
    /// </summary>
    /// <param name="acctImage"></param>
    /// <param name="usercolor"></param>
    /// <param name="userId"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static string CreateImgColor(string acctImage, string usercolor, string userId, int height, string siteTheme) {
        int imageHeight = height - 2;
        const string imgFormat = "<img alt='account-image' src='{0}' style='height: {1}px;' />";
        string imgTag = string.Empty;

        if (!string.IsNullOrEmpty(acctImage)) {
            string imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + userId + "/" + acctImage);
            if (acctImage.ToLower().Contains("http") || acctImage.StartsWith("//") || acctImage.ToLower().Contains("www.")) {
                imgLoc = acctImage;
            }
            else if (!File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + userId + "/" + acctImage)) {
                imgLoc = ServerSettings.ResolveUrl("~/App_Themes/" + siteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
            }

            imgTag = string.Format(imgFormat, imgLoc, imageHeight);
        }
        else {
            imgTag = string.Format(imgFormat, ServerSettings.ResolveUrl("~/App_Themes/" + siteTheme + "/Icons/SiteMaster/EmptyUserImg.png"), imageHeight);
        }

        if (!usercolor.StartsWith("#")) {
            usercolor = "#" + usercolor;
        }

        return string.Format("<div class='sch_ColorCode' style='background-color: {0}; height: {1}px; width: {1}px;'>{2}</div>", usercolor, height, imgTag);
    }

    /// <summary>
    /// Creates the User's Account Image and Color for the SiteMasters and the AppRemote
    /// </summary>
    /// <param name="acctImage"></param>
    /// <param name="usercolor"></param>
    /// <param name="userId"></param>
    /// <param name="userFullName"></param>
    /// <param name="siteTheme"></param>
    /// <returns></returns>
    public static string CreateImgColorTopBar(string acctImage, string usercolor, string userId, string userFullName, string siteTheme, MemberDatabase.UserProfileLinkStyle linkStyle) {
        if (!usercolor.StartsWith("#")) {
            usercolor = "#" + usercolor;
        }

        const string imgFormat = "<img alt='account-image' src='{0}' class='top-menu-acctImage' style='{1}' />";
        const string sch_ColorCodeFormat = "<div class='sch_ColorCode' style='background-color: {0};{1}'>{2}</div>{3}";
        string imgLoc = ServerSettings.ResolveUrl("~/App_Themes/" + siteTheme + "/Icons/SiteMaster/EmptyUserImg.png");

        if (!string.IsNullOrEmpty(acctImage)) {
            imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + userId + "/" + acctImage);
            if (acctImage.ToLower().Contains("http") || acctImage.StartsWith("//") || acctImage.ToLower().Contains("www.")) {
                imgLoc = acctImage;
            }
            else if (!File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + userId + "/" + acctImage)) {
                imgLoc = ServerSettings.ResolveUrl("~/App_Themes/" + siteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
            }
        }

        string[] firstName = userFullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        if (firstName.Length > 0) {
            userFullName = firstName[0];
        }

        string imgTag = string.Empty;
        switch (linkStyle) {
            case MemberDatabase.UserProfileLinkStyle.Name_And_Color:
                return string.Format(sch_ColorCodeFormat, usercolor, " height: 12px!important; width: 12px!important; margin-top: 10px!important;", string.Empty, userFullName);

            case MemberDatabase.UserProfileLinkStyle.Image_Plus_Name_And_Color_Cover:
            case MemberDatabase.UserProfileLinkStyle.Image_And_Name:
                imgTag = string.Format(imgFormat, imgLoc, "margin-top: 4px!important; float: left!important; margin-right: 7px;");
                return imgTag + userFullName;

            case MemberDatabase.UserProfileLinkStyle.Image_And_Color:
                imgTag = string.Format(imgFormat, imgLoc, string.Empty);
                return string.Format(sch_ColorCodeFormat, usercolor, " margin-right: 0!important;", imgTag, string.Empty);

            case MemberDatabase.UserProfileLinkStyle.Image_Only:
                return string.Format(imgFormat, imgLoc, "margin-top: 4px!important;");

            case MemberDatabase.UserProfileLinkStyle.Name_And_Color_Cover:
            case MemberDatabase.UserProfileLinkStyle.Name_Only:
                return userFullName;

            default:
            case MemberDatabase.UserProfileLinkStyle.Default:
                imgTag = string.Format(imgFormat, imgLoc, string.Empty);
                return string.Format(sch_ColorCodeFormat, usercolor, string.Empty, imgTag, userFullName);
        }
    }

    /// <summary>
    /// Apply the Profile Link Style to the current page
    /// </summary>
    /// <param name="linkStyle"></param>
    /// <param name="usercolor"></param>
    /// <param name="ctrl"></param>
    public static void ApplyProfileLinkStyle(MemberDatabase.UserProfileLinkStyle linkStyle, string usercolor, Page page) {
        string lbl_UserName = "lbl_UserName";
        string parentJqueryCall = ".closest('.top-bar-userinfo-button')";
        if (page.ToString() == "ASP.appremote_aspx" || page.ToString() == "ASP.grouplogin_aspx") {
            parentJqueryCall = string.Empty;
        }

        string jsCode = "$('#" + lbl_UserName + "').css('color', '');$('#" + lbl_UserName + "')" + parentJqueryCall + ".css('background-color', '');";

        if (linkStyle == MemberDatabase.UserProfileLinkStyle.Name_And_Color_Cover || linkStyle == MemberDatabase.UserProfileLinkStyle.Image_Plus_Name_And_Color_Cover) {
            if (!string.IsNullOrEmpty(usercolor)) {
                if (!usercolor.StartsWith("#")) {
                    usercolor = "#" + usercolor;
                }

                string fontColor = "#FFF";
                if (HelperMethods.UseDarkTextColorWithBackground(usercolor)) {
                    fontColor = "#252525";
                }

                jsCode = "$('#" + lbl_UserName + "').css('color', '" + fontColor + "');$('#" + lbl_UserName + "')" + parentJqueryCall + ".css('background-color', '" + usercolor + "');";
            }
        }
        else if (linkStyle == MemberDatabase.UserProfileLinkStyle.Name_Only) {
            jsCode = "$('#" + lbl_UserName + "').css('color', '');$('#" + lbl_UserName + "')" + parentJqueryCall + ".css('background-color', '');";
        }
        else if (linkStyle == MemberDatabase.UserProfileLinkStyle.Image_Only || linkStyle == MemberDatabase.UserProfileLinkStyle.Image_And_Color) {
            jsCode = "$('#" + lbl_UserName + "').css('color', '');$('#" + lbl_UserName + "')" + parentJqueryCall + ".css('background-color', '');";
        }

        RegisterPostbackScripts.RegisterStartupScript(page, jsCode);
    }

    /// <summary>
    /// Create the User's Account Image and Color for the Chat Client user list
    /// </summary>
    /// <param name="acctImage"></param>
    /// <param name="usercolor"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static string CreateImgColorChatList(string acctImage, string usercolor, string userId, string siteTheme) {
        const string imgFormat = "<img alt='chat-account-image' src='{0}' class='top-menu-acctImage'>";
        string imgTag = string.Empty;

        if (!usercolor.StartsWith("#")) {
            usercolor = "#" + usercolor;
        }

        if (!string.IsNullOrEmpty(acctImage)) {
            string imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + userId + "/" + acctImage);
            if (acctImage.ToLower().Contains("http") || acctImage.StartsWith("//") || acctImage.ToLower().Contains("www.")) {
                imgLoc = acctImage;
            }
            else if (!File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + userId + "/" + acctImage)) {
                imgLoc = ServerSettings.ResolveUrl("~/App_Themes/" + siteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
            }

            imgTag = string.Format(imgFormat, imgLoc);
        }
        else {
            imgTag = string.Format(imgFormat, ServerSettings.ResolveUrl("~/App_Themes/" + siteTheme + "/Icons/SiteMaster/EmptyUserImg.png"));
        }

        return string.Format("<div class='sch_ColorCode_chat' style='background-color: {0};'>{1}</div>", usercolor, imgTag);
    }

}