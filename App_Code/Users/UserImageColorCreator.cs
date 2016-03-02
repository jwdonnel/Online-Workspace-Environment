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
    /// Create the User's Account Image and Color for the GroupOrg, UserAccounts, UsersAndApps, and Apps
    /// </summary>
    /// <param name="acctImage"></param>
    /// <param name="usercolor"></param>
    /// <param name="userId"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static string CreateImgColor(string acctImage, string usercolor, string userId, int height) {
        string img = "<img alt='' src='" + ServerSettings.ResolveUrl("~/Standard_Images/EmptyUserImg.png") + "' class='float-left' style='height:" + height + "px;' />";
        if (!string.IsNullOrEmpty(acctImage)) {
            string imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + userId + "/" + acctImage);
            if (acctImage.ToLower().Contains("http") || acctImage.ToLower().Contains("www.")) {
                imgLoc = acctImage;
            }
            else if (!File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + userId + "/" + acctImage)) {
                imgLoc = ServerSettings.ResolveUrl("~/Standard_Images/EmptyUserImg.png");
            }
            img = "<img alt='' src='" + imgLoc + "' class='float-left' style='height:" + height + "px;' />";
        }

        return "<div class='float-left'>" + img + "<div class='sch_ColorCode' style='background-color: #" + usercolor + "; top: 0px; height:" + height + "px;'></div></div>";
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
    public static string CreateImgColorTopBar(string acctImage, string usercolor, string userId, string userFullName, string siteTheme, MemberDatabase.UserProfileLinkStyle linkStyle) {
        string imagePadding = string.Empty;
        if (linkStyle == MemberDatabase.UserProfileLinkStyle.Image_Plus_Name_And_Color_Cover || linkStyle == MemberDatabase.UserProfileLinkStyle.Image_And_Name) {
            imagePadding = " top-menu-acctImage-padding";
        }

        string img = "<img alt='' src='" + ServerSettings.ResolveUrl("~/Standard_Images/EmptyUserImg.png") + "' class='top-menu-acctImage" + imagePadding + "'>";
        if (!string.IsNullOrEmpty(acctImage)) {
            string imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + userId + "/" + acctImage);
            if (acctImage.ToLower().Contains("http") || acctImage.ToLower().Contains("www.")) {
                imgLoc = acctImage;
            }
            else if (!File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + userId + "/" + acctImage)) {
                imgLoc = ServerSettings.ResolveUrl("~/Standard_Images/EmptyUserImg.png");
            }
            img = "<img alt='' src='" + imgLoc + "' class='top-menu-acctImage" + imagePadding + "'>";
        }

        string[] firstName = userFullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        if (firstName.Length > 0) {
            userFullName = firstName[0];
        }

        switch (linkStyle) {
            case MemberDatabase.UserProfileLinkStyle.Name_And_Color:
                return "<div class='sch_ColorCode' style='background-color: #" + usercolor + "'></div>" + userFullName;

            case MemberDatabase.UserProfileLinkStyle.Image_Plus_Name_And_Color_Cover:
            case MemberDatabase.UserProfileLinkStyle.Image_And_Name:
                return img + userFullName;

            case MemberDatabase.UserProfileLinkStyle.Image_And_Color:
                return img + "<div class='sch_ColorCode' style='background-color: #" + usercolor + "; margin-right: 0px!important;'></div>";

            case MemberDatabase.UserProfileLinkStyle.Image_Only:
                return img;

            case MemberDatabase.UserProfileLinkStyle.Name_And_Color_Cover:
            case MemberDatabase.UserProfileLinkStyle.Name_Only:
                return userFullName;

            default:
            case MemberDatabase.UserProfileLinkStyle.Default:
                return img + "<div class='sch_ColorCode' style='background-color: #" + usercolor + "'></div>" + userFullName;
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
        string parentJqueryCall = ".parent()";
        if (page.ToString() == "ASP.appremote_aspx" || page.ToString() == "ASP.grouplogin_aspx") {
            parentJqueryCall = string.Empty;
        }

        string jsCode = "$('#" + lbl_UserName + "').css('color', '');$('#" + lbl_UserName + "')" + parentJqueryCall + ".css('background-color', '');$('#" + lbl_UserName + "').css('padding-left', '');$('#" + lbl_UserName + "').css('padding-right', '');";

        if (linkStyle == MemberDatabase.UserProfileLinkStyle.Name_And_Color_Cover || linkStyle == MemberDatabase.UserProfileLinkStyle.Image_Plus_Name_And_Color_Cover) {
            if (!string.IsNullOrEmpty(usercolor)) {
                if (!usercolor.StartsWith("#")) {
                    usercolor = "#" + usercolor;
                }

                string fontColor = "#FFF";
                if (HelperMethods.UseDarkTextColorWithBackground(usercolor)) {
                    fontColor = "#252525";
                }

                jsCode = "$('#" + lbl_UserName + "').css('color', '" + fontColor + "');$('#" + lbl_UserName + "')" + parentJqueryCall + ".css('background-color', '" + usercolor + "');$('#" + lbl_UserName + "').css('padding-right', '');";
                if (linkStyle == MemberDatabase.UserProfileLinkStyle.Name_And_Color_Cover) {
                    jsCode += "$('#" + lbl_UserName + "').css('padding-left', '10px');";
                }
                else {
                    jsCode += "$('#" + lbl_UserName + "').css('padding-left', '');";
                }
            }
        }
        else if (linkStyle == MemberDatabase.UserProfileLinkStyle.Name_Only) {
            jsCode = "$('#" + lbl_UserName + "').css('color', '');$('#" + lbl_UserName + "')" + parentJqueryCall + ".css('background-color', '');$('#" + lbl_UserName + "').css('padding-left', '10px');$('#" + lbl_UserName + "').css('padding-right', '');";
        }
        else if (linkStyle == MemberDatabase.UserProfileLinkStyle.Image_Only || linkStyle == MemberDatabase.UserProfileLinkStyle.Image_And_Color) {
            jsCode = "$('#" + lbl_UserName + "').css('color', '');$('#" + lbl_UserName + "')" + parentJqueryCall + ".css('background-color', '');$('#" + lbl_UserName + "').css('padding-left', '');$('#" + lbl_UserName + "').css('padding-right', '0');";
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
    public static string CreateImgColorChatList(string acctImage, string usercolor, string userId) {
        string uc = "<img alt='' src='" + ServerSettings.ResolveUrl("~/Standard_Images/EmptyUserImg.png") + "' class='top-menu-acctImage'>";
        if (!string.IsNullOrEmpty(acctImage)) {
            string imgLoc = ServerSettings.ResolveUrl(ServerSettings.AccountImageLoc + userId + "/" + acctImage);
            if (acctImage.ToLower().Contains("http") || acctImage.ToLower().Contains("www.")) {
                imgLoc = acctImage;
            }
            else if (!File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + userId + "/" + acctImage)) {
                imgLoc = ServerSettings.ResolveUrl("~/Standard_Images/EmptyUserImg.png");
            }
            uc = "<img alt='' src='" + imgLoc + "' class='chat-acctImage'>";
        }

        uc += "<div class='sch_ColorCode_chat' style='background-color: " + usercolor + "'></div>";
        return uc;
    }

}