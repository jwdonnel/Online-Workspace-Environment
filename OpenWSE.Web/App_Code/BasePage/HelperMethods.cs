using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

public class HelperMethods
{
    public HelperMethods() { }

    /// <summary>
    /// Converts a given string or integer to a boolean value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool ConvertBitToBoolean(object value) {
        bool returnVal = false;
        if (value != null) {
            string _value = value.ToString().Trim().ToLower();

            if (_value != "true" && _value != "false" && _value != "0" && _value != "1" && !string.IsNullOrEmpty(_value)) {
                return true;
            }

            if (_value == "1") {
                return true;
            }
            else if (_value == "0") {
                return false;
            }
            else {
                if (!bool.TryParse(_value, out returnVal)) {
                    if (_value == "true") {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
        }

        return returnVal;
    }

    public static bool DoesPageContainStr(string str, Page page = null) {
        if (page != null && page.Request != null) {
            if (page.Request.RawUrl.ToLower().Contains(str.ToLower())) {
                return true;
            }
            if (str.ToLower() == "default.aspx" && (!page.Request.RawUrl.ToLower().Contains(".aspx") || page.Request.RawUrl == "/" || page.Request.RawUrl.EndsWith("/"))) {
                return true;
            }
        }
        if (HttpContext.Current != null && HttpContext.Current.Request != null) {
            if (HttpContext.Current.Request.RawUrl.ToLower().Contains(str.ToLower())) {
                return true;
            }
            if (str.ToLower() == "default.aspx" && (!HttpContext.Current.Request.RawUrl.ToLower().Contains(".aspx") || HttpContext.Current.Request.RawUrl == "/" || HttpContext.Current.Request.RawUrl.EndsWith("/"))) {
                return true;
            }
        }
        return false;
    }

    public static bool IsValidAppFileType(string filename) {
        filename = filename.ToLower();
        if (!filename.Contains(".exe") && !filename.Contains(".com") && !filename.Contains(".pif") && !filename.Contains(".bat") && !filename.Contains(".scr")) {
            return true;
        }
        else if (filename.StartsWith("//") || filename.Contains("http://") || filename.Contains("https://") || filename.Contains("www.")) {
            return true;
        }

        return false;
    }
    public static bool IsValidHttpBasedAppType(string filename) {
        filename = filename.ToLower();
        if (filename.StartsWith("//") || filename.Contains("http://") || filename.Contains("https://") || filename.Contains("www.")) {
            return true;
        }

        return false;
    }
    public static bool IsValidAscxOrDllFile(string filename) {
        filename = filename.ToLower();
        if (filename.Length > 5 && filename.Substring(filename.Length - 5) == ".ascx") {
            return true;
        }
        else if (IsValidDllFile(filename)) {
            return true;
        }

        return false;
    }
    public static bool IsValidDllFile(string filename) {
        filename = filename.ToLower();
        if (filename.Length > 5 && filename.Substring(filename.Length - 4) == ".dll") {
            return true;
        }

        return false;
    }
    public static bool IsValidAspxFile(string filename) {
        filename = filename.ToLower();
        if (filename.Length > 5 && filename.Substring(filename.Length - 5) == ".aspx") {
            return true;
        }

        return false;
    }

    private const int ImageMinimumBytes = 512;
    public static bool IsImage(object file) {
        if (file.GetType() == typeof(HttpPostedFile)) {
            HttpPostedFile postedFile = (HttpPostedFile)file;

            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png") {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg") {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try {
                if (!postedFile.InputStream.CanRead) {
                    return false;
                }

                if (postedFile.ContentLength < ImageMinimumBytes) {
                    return false;
                }

                byte[] buffer = new byte[512];
                postedFile.InputStream.Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline)) {
                    return false;
                }
            }
            catch (Exception) {
                return false;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------

            try {
                using (var bitmap = new System.Drawing.Bitmap(postedFile.InputStream)) {
                }
            }
            catch (Exception) {
                return false;
            }

            return true;
        }
        else if (file.GetType() == typeof(HttpWebResponse)) {
            HttpWebResponse postedFile = (HttpWebResponse)file;

            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png") {
                return false;
            }

            return true;
        }

        return false;
    }

    public static string GetFormatedTime(DateTime postDate) {
        DateTime now = ServerSettings.ServerDateTime;
        TimeSpan final = now.Subtract(postDate);
        string time = string.Empty;
        if (final.Days >= 1) {
            time = postDate.ToShortDateString();
        }
        else {
            if (final.Hours == 0) {
                time = final.Minutes.ToString() + " minute(s) ago";
            }
            else {
                time = final.Hours.ToString() + " hour(s) ago";
            }
        }
        return time;
    }

    public static string MergeFMLNames(MemberDatabase member) {
        string name = (string.IsNullOrEmpty(member.LastName)) || (member.LastName.ToLower() == "n/a")
                          ? member.FirstName
                          : member.FirstName + " " + member.LastName;

        if ((name.ToLower() == "n/a n/a") || (name.ToLower() == "n/a") || (string.IsNullOrEmpty(name))) {
            name = member.Username;
        }

        if (string.IsNullOrEmpty(name)) {
            name = "n/a";
        }

        return name;
    }

    public static string GetCopyrightFooterText() {
        string name = ServerSettings.SiteName;
        if (string.IsNullOrEmpty(name)) {
            name = "My Site Name";
        }

        return "&copy; " + DateTime.Now.Year + " " + name;
    }

    public static string FormatBytes(float fileInBytes) {
        string strSize = "00";
        if (fileInBytes < 1024)
            strSize = fileInBytes + " B"; //Byte
        else if (fileInBytes > 1024 & fileInBytes < 1048576)
            strSize = Math.Round((fileInBytes / 1024), 2) + " KB"; //Kilobyte
        else if (fileInBytes > 1048576 & fileInBytes < 107341824)
            strSize = Math.Round((fileInBytes / 1024) / 1024, 2) + " MB"; //Megabyte
        else if (fileInBytes > 107341824 & fileInBytes < 1099511627776)
            strSize = Math.Round(((fileInBytes / 1024) / 1024) / 1024, 2) + " GB"; //Gigabyte
        else
            strSize = Math.Round((((fileInBytes / 1024) / 1024) / 1024) / 1024, 2) + " TB"; //Terabyte
        return strSize;
    }

    public static string RandomString(int size, bool upperCase = false) {
        Random Random = new Random((int)ServerSettings.ServerDateTime.Ticks);
        var builder = new StringBuilder();
        for (var i = 0; i < size; i++) {
            char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
            builder.Append(ch);
        }

        return upperCase ? builder.ToString().ToUpper() : builder.ToString().ToLower();
    }

    public static bool CompareUserGroups(MemberDatabase member1, MemberDatabase member2) {
        List<string> group1 = member1.GroupList;
        List<string> group2 = member2.GroupList;

        return (from g1 in group1 from g2 in group2 where g2 == g1 select g1).Any();
    }

    public static String GetTimestamp() {
        return DateTime.Now.ToString("yyyyMMddHHmmssffff");
    }

    /// <summary>
    /// This method will check a url to see that it does not return server or protocol errors
    /// </summary>
    /// <param name="url">The path to check</param>
    /// <returns></returns>
    public static bool UrlIsValid(string url, string username) {
        return new IpMethods().UrlIsValid(url, username);
    }

    public static string GetWebPageTitle(string url) {
        return new IpMethods().GetWebPageTitle(url);
    }

    public static string GetWebPageImage(string url) {
        return new IpMethods().GetWebPageImage(url);
    }

    public static bool IsMobileDevice {
        get {
            HttpRequest request = HttpContext.Current.Request;
            HttpResponse response = HttpContext.Current.Response;
            if (request != null) {
                try {
                    if ((response.Cookies["MobileDevice"] != null && response.Cookies["MobileDevice"].Value == "IgnoreMobileDevice") || HelperMethods.ConvertBitToBoolean(request.QueryString["overrideMobile"])) {
                        return false;
                    }

                    if ((request.Browser != null && HelperMethods.ConvertBitToBoolean(request.Browser["IsMobileDevice"])) || HelperMethods.ConvertBitToBoolean(request.QueryString["mobileMode"])) {
                        return true;
                    }
                    
                    //string userAgent = request.UserAgent;
                    //if (!string.IsNullOrEmpty(userAgent)) {
                    //    userAgent = userAgent.ToLower();

                    //    // Mobile Device Array
                    //    string[] MobileDevices = new[] { "iphone", "ipad", "ipod", "webos", "ppc", "windows ce", "blackberry", "opera mini", "mobile", "palm", "portable", "opera mobi", "android", "windows phone" };
                    //    return MobileDevices.Any(x => userAgent.Contains(x));
                    //}
                }
                catch { }
            }

            return false;
        }
    }

    public static TimeSpan UpTime {
        get {
            try {
                using (var uptime = new PerformanceCounter("System", "System Up Time")) {
                    uptime.NextValue(); //Call this an extra time before reading its value
                    return TimeSpan.FromSeconds(uptime.NextValue());
                }
            }
            catch { }

            return TimeSpan.Zero;
        }
    }

    public static IpCityState GetCityStateFromIP(string ip) {
        return new IpMethods().GetCityStateFromIp(ip);
    }

    public static bool IsValidFileFolderFormat(string fileExt) {
        fileExt = fileExt.ToLower();
        if ((fileExt == ".aspx") || (fileExt == ".html") || (fileExt == ".php") || (fileExt == ".asp") ||
            (fileExt == ".htm") || (fileExt == ".xhtml") || (fileExt == ".jhtml") || (fileExt == ".php4") ||
            (fileExt == ".php3") || (fileExt == ".phtml") || (fileExt == ".xml") || (fileExt == ".rss") ||
            (fileExt == ".css") || (fileExt == ".js") || (fileExt == ".log") || (fileExt == ".conf") ||
            (fileExt == ".config") || (fileExt == ".master") || (fileExt == ".pdf") || (fileExt == ".txt")) {
            return true;
        }

        return false;
    }
    public static bool IsImageFileType(string extension) {
        extension = extension.ToLower();
        var ok = (extension == ".png") || (extension == ".bmp") || (extension == ".jpg")
                 || (extension == ".jpeg") || (extension == ".jpe") || (extension == ".jfif")
                 || (extension == ".tif") || (extension == ".tiff") || (extension == ".gif")
                 || (extension == ".tga");
        return ok;
    }
    public static bool IsValidDefaultPage(string fileExt) {
        fileExt = fileExt.ToLower();
        if ((fileExt == ".aspx") || (fileExt == ".html") || (fileExt == ".php") || (fileExt == ".asp") ||
            (fileExt == ".htm") || (fileExt == ".xhtml") || (fileExt == ".jhtml") || (fileExt == ".php4") ||
            (fileExt == ".php3") || (fileExt == ".phtml")) {
            return true;
        }

        return false;
    }

    public static string GetLockedByMessage() {
        string message = "<div class='clear-space-five'></div>";
        message += "<span class='td-lock-btn float-left margin-right-sml'></span>";
        message += "<h3>Locked by " + ServerSettings.AdminUserName + "</h3>";
        message += "<div class='clear-space'></div>";
        return message;
    }

    public static string TableAddRemove(string itemsRemoved, string itemsAdded, string titleRemoved, string titleAdded, bool addExtraTopPadding, bool addBoxShadow = false) {
        string classBoxShadow = string.Empty;
        if (addBoxShadow) {
            classBoxShadow = " boxshadow";
        }

        string classPadTop = string.Empty;
        if (addExtraTopPadding) {
            classPadTop = " pad-top-big pad-bottom";
        }

        string strRemoved = "<div class='package-float-box first-box'><span class='title-column'>" + titleRemoved + "</span><div id='package-removed' class='item-column" + classPadTop + classBoxShadow + "'>" + itemsRemoved + "</div><div class='clear'></div></div>";
        string strAdded = "<div class='package-float-box'><span class='title-column'>" + titleAdded + "</span><div id='package-added' class='item-column" + classPadTop + classBoxShadow + "'>" + itemsAdded + "</div><div class='clear'></div></div>";
        return "<div class='add-remove-columntable'>" + strRemoved + strAdded + "<div class='clear'></div></div>";
    }

    /// <summary>
    /// Creates a javascript variable called UserIsSocialAccount if the user has a Social Network account
    /// </summary>
    /// <param name="page"></param>
    /// <param name="userName"></param>
    public static void SetIsSocialUserForDeleteItems(Page page, string userName) {
        MemberDatabase member = new MemberDatabase(userName);
        if (page != null) {
            RegisterPostbackScripts.RegisterStartupScript(page, "UserIsSocialAccount=" + member.IsSocialAccount.ToString().ToLower() + ";");
        }
    }

    public static string GetPrettyDate(DateTime d) {
        // 1.
        // Get time span elapsed since the date.
        TimeSpan s = ServerSettings.ServerDateTime.Subtract(d);

        // 2.
        // Get total number of days elapsed.
        int dayDiff = (int)s.TotalDays;

        // 3.
        // Get total number of seconds elapsed.
        int secDiff = (int)s.TotalSeconds;

        // 4.
        // Don't allow out of range values.
        if (dayDiff < 0 || dayDiff >= 31) {
            return d.ToShortDateString();
        }

        // 5.
        // Handle same-day times.
        if (dayDiff == 0) {
            // A.
            // Less than one minute ago.
            if (secDiff < 60) {
                return "just now";
            }
            // B.
            // Less than 2 minutes ago.
            if (secDiff < 120) {
                return "1 minute ago";
            }
            // C.
            // Less than one hour ago.
            if (secDiff < 3600) {
                return string.Format("{0} minutes ago",
                    Math.Floor((double)secDiff / 60));
            }
            // D.
            // Less than 2 hours ago.
            if (secDiff < 7200) {
                return "1 hour ago";
            }
            // E.
            // Less than one day ago.
            if (secDiff < 86400) {
                return string.Format("{0} hours ago",
                    Math.Floor((double)secDiff / 3600));
            }
        }
        // 6.
        // Handle previous days.
        if (dayDiff == 1) {
            return "yesterday";
        }
        if (dayDiff < 7) {
            return string.Format("{0} days ago",
            dayDiff);
        }
        if (dayDiff < 31) {
            return string.Format("{0} weeks ago",
            Math.Ceiling((double)dayDiff / 7));
        }

        return string.Empty;
    }

    public static string GetCSSGradientStyles(string primaryColor, string secondaryColor, bool reverse = false) {
        if (primaryColor.Length == 6 || (primaryColor.StartsWith("#") && primaryColor != "#" && primaryColor.Length == 7)) {
            if (!primaryColor.StartsWith("#")) {
                primaryColor = "#" + primaryColor;
            }

            if (primaryColor == "inherit" || primaryColor == "#inherit") {
                primaryColor = "#FFFFFF";
            }

            if (secondaryColor == "inherit" || secondaryColor == "#inherit") {
                secondaryColor = "#FFFFFF";
            }

            int colorDifference = 40;
            if (string.IsNullOrEmpty(secondaryColor)) {
                Color _color = ColorTranslator.FromHtml(primaryColor);
                int r = _color.R;
                int g = _color.G;
                int b = _color.B;
                if (r > colorDifference) {
                    r -= colorDifference;
                }
                else {
                    r = 0;
                }

                if (g > colorDifference) {
                    g -= colorDifference;
                }
                else {
                    g = 0;
                }

                if (b > colorDifference) {
                    b -= colorDifference;
                }
                else {
                    b = 0;
                }

                secondaryColor = ColorTranslator.ToHtml(Color.FromArgb(r, g, b));
            }

            if (!secondaryColor.StartsWith("#")) {
                secondaryColor = "#" + secondaryColor;
            }

            if (reverse) {
                string tempPrimaryColor = primaryColor;
                primaryColor = secondaryColor;
                secondaryColor = tempPrimaryColor;
            }

            string cssStyle = string.Format("background:{0};", primaryColor);
            cssStyle += string.Format("background:-moz-linear-gradient(-45deg, {0} 0%, {1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("background:-webkit-gradient(linear, left top, right bottom, color-stop(0%,{0}), color-stop(100%,{1}));", primaryColor, secondaryColor);
            cssStyle += string.Format("background:-webkit-linear-gradient(-45deg, {0} 0%,{1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("background:-o-linear-gradient(-45deg, {0} 0%,{1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("background:-ms-linear-gradient(-45deg, {0} 0%,{1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("background:linear-gradient(135deg, {0} 0%,{1} 100%);", primaryColor, secondaryColor);
            cssStyle += string.Format("filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='{0}', endColorstr='{1}',GradientType=1 );", primaryColor, secondaryColor);

            return cssStyle;
        }

        if (primaryColor == "#") {
            primaryColor = "#FFFFFF";
        }

        return primaryColor;
    }

    public static string GetAltColorFromHex(string hex, int colorDiff) {
        if (!hex.StartsWith("#")) {
            hex = "#" + hex;
        }

        if (hex != "inherit" && hex != "#inherit") {
            Color _color = ColorTranslator.FromHtml(hex);
            try {
                int newR = Convert.ToInt32(_color.R);
                int newG = Convert.ToInt32(_color.G);
                int newB = Convert.ToInt32(_color.B);

                if (newR > colorDiff) {
                    newR -= colorDiff;
                }
                else {
                    newR = 0;
                }

                if (newG > colorDiff) {
                    newG -= colorDiff;
                }
                else {
                    newG = 0;
                }

                if (newB > colorDiff) {
                    newB -= colorDiff;
                }
                else {
                    newB = 0;
                }
                string newHex = ColorTranslator.ToHtml(Color.FromArgb(newR, newG, newB));
                if (!newHex.StartsWith("#")) {
                    newHex = "#" + newHex;
                }

                return newHex.ToUpper();
            }
            catch { }
        }

        return hex.ToUpper();
    }

    public static bool UseDarkTextColorWithBackground(string backgroundColor) {
        bool useDarkColor = true;

        if (backgroundColor != "inherit" && backgroundColor != "#inherit") {
            Color _color = ColorTranslator.FromHtml(backgroundColor);
            if (_color.R + _color.G + _color.B < 475) {
                useDarkColor = false;
            }
        }

        return useDarkColor;
    }

    /// <summary>Save a session state for scrolling to an element before the postback happens 
    /// </summary>
    /// <param name="jqueryElement"></param>
    /// <param name="session"></param>
    public static void SaveSessionScrollToElement(string jqueryElement, HttpSessionState session) {
        if (session != null) {
            try {
                session["ScrollToJqueryElement"] = jqueryElement;
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }

    /// <summary>Load the ScrollToPostBack session state if available
    /// </summary>
    /// <param name="session"></param>
    /// <param name="showAnimation"></param>
    public static void LoadSavedScrollToElementSession(HttpSessionState session, Page page, bool showAnimation = false) {
        if (session != null && page != null) {
            try {
                if (session["ScrollToJqueryElement"] != null && !string.IsNullOrEmpty(session["ScrollToJqueryElement"].ToString())) {
                    RegisterPostbackScripts.RegisterStartupScript(page, "openWSE.ScrollToElement('" + session["ScrollToJqueryElement"].ToString() + "', " + showAnimation.ToString().ToLower() + ");");
                    session["ScrollToJqueryElement"] = null;
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }

    public static string BuildColorOptionList(string onclickFunction, string onColorUpateFunction, string onResetFunction, string siteColorOption, Page page) {
        StringBuilder str = new StringBuilder();

        string selectedIndex = "1";
        string[] siteOptionSplit = siteColorOption.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        if (siteOptionSplit.Length > 0) {
            string[] splitVal = siteOptionSplit[0].Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitVal.Length >= 1) {
                selectedIndex = splitVal[0];
            }
        }

        int selectedIndexInt = 1;
        int.TryParse(selectedIndex, out selectedIndexInt);
        if (string.IsNullOrEmpty(selectedIndex) || selectedIndexInt <= 0 || selectedIndexInt > 12) {
            selectedIndex = "1";
        }

        string currOptionNum = "1";
        string selectedClass = string.Empty;
        string selectedColor = string.Empty;
        string color = string.Empty;
        string backgroundColorStyle = string.Empty;

        #region Option 1
        for (var i = 0; i < siteOptionSplit.Length; i++) {
            string[] splitValTemp = siteOptionSplit[i].Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitValTemp[0] == currOptionNum && splitValTemp.Length > 1) {
                color = splitValTemp[1];
                break;
            }
        }
        if (color.Length > 6) {
            color = string.Empty;
        }
        if (selectedIndex == currOptionNum) {
            selectedClass = " selected";
            selectedColor = color;
        }
        if (!string.IsNullOrEmpty(color)) {
            backgroundColorStyle = string.Format(" style=\"background-color: {0}!important;\"", color);
            if (!color.StartsWith("#")) {
                backgroundColorStyle = string.Format(" style=\"background-color: #{0}!important;\"", color);
            }
        }
        str.AppendFormat("<div class=\"theme-color-option{0}\" data-option=\"{3}\" onclick=\"{1}\" data-color=\"{2}\">", selectedClass, onclickFunction, color, currOptionNum);
        str.AppendFormat("<div class=\"color-option-toplogo\"{0}></div>", backgroundColorStyle);
        str.AppendFormat("<div class=\"color-option-topbar\"{0}></div>", backgroundColorStyle);
        str.Append("<div class=\"color-option-sidebar\"></div>");
        str.Append("<div class=\"color-option-container\"></div>");
        str.Append("<div class=\"color-option-footer\"></div>");
        str.Append("</div>");
        #endregion

        selectedClass = string.Empty;
        color = string.Empty;
        backgroundColorStyle = string.Empty;

        #region Option 2
        currOptionNum = "2";
        for (var i = 0; i < siteOptionSplit.Length; i++) {
            string[] splitValTemp = siteOptionSplit[i].Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitValTemp[0] == currOptionNum && splitValTemp.Length > 1) {
                color = splitValTemp[1];
                break;
            }
        }
        if (color.Length > 6) {
            color = string.Empty;
        }
        if (selectedIndex == currOptionNum) {
            selectedClass = " selected";
            selectedColor = color;
        }
        if (!string.IsNullOrEmpty(color)) {
            backgroundColorStyle = string.Format(" style=\"background-color: {0}!important;\"", color);
            if (!color.StartsWith("#")) {
                backgroundColorStyle = string.Format(" style=\"background-color: #{0}!important;\"", color);
            }
        }
        str.AppendFormat("<div class=\"theme-color-option{0}\" data-option=\"{3}\" onclick=\"{1}\" data-color=\"{2}\">", selectedClass, onclickFunction, color, currOptionNum);
        str.AppendFormat("<div class=\"color-option-toplogo\"{0}></div>", backgroundColorStyle);
        str.Append("<div class=\"color-option-topbar\"></div>");
        str.Append("<div class=\"color-option-sidebar\"></div>");
        str.Append("<div class=\"color-option-container\"></div>");
        str.Append("<div class=\"color-option-footer\"></div>");
        str.Append("</div>");
        #endregion

        selectedClass = string.Empty;
        color = string.Empty;
        backgroundColorStyle = string.Empty;

        #region Option 3
        currOptionNum = "3";
        for (var i = 0; i < siteOptionSplit.Length; i++) {
            string[] splitValTemp = siteOptionSplit[i].Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitValTemp[0] == currOptionNum && splitValTemp.Length > 1) {
                color = splitValTemp[1];
                break;
            }
        }
        if (color.Length > 6) {
            color = string.Empty;
        }
        if (selectedIndex == currOptionNum) {
            selectedClass = " selected";
            selectedColor = color;
        }
        if (!string.IsNullOrEmpty(color)) {
            backgroundColorStyle = string.Format(" style=\"background-color: {0}!important;\"", color);
            if (!color.StartsWith("#")) {
                backgroundColorStyle = string.Format(" style=\"background-color: #{0}!important;\"", color);
            }
        }
        str.AppendFormat("<div class=\"theme-color-option{0}\" data-option=\"{3}\" onclick=\"{1}\" data-color=\"{2}\">", selectedClass, onclickFunction, color, currOptionNum);
        str.AppendFormat("<div class=\"color-option-toplogo\"{0}></div>", backgroundColorStyle);
        str.AppendFormat("<div class=\"color-option-topbar\"{0}></div>", backgroundColorStyle);
        str.Append("<div class=\"color-option-sidebar\"></div>");
        str.Append("<div class=\"color-option-container\"></div>");
        str.Append("<div class=\"color-option-footer\"></div>");
        str.Append("</div>");
        #endregion

        selectedClass = string.Empty;
        color = string.Empty;
        backgroundColorStyle = string.Empty;

        #region Option 4
        currOptionNum = "4";
        for (var i = 0; i < siteOptionSplit.Length; i++) {
            string[] splitValTemp = siteOptionSplit[i].Split(new string[] { "~" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitValTemp[0] == currOptionNum && splitValTemp.Length > 1) {
                color = splitValTemp[1];
                break;
            }
        }
        if (color.Length > 6) {
            color = string.Empty;
        }
        if (selectedIndex == currOptionNum) {
            selectedClass = " selected";
            selectedColor = color;
        }
        if (!string.IsNullOrEmpty(color)) {
            backgroundColorStyle = string.Format(" style=\"background-color: {0}!important;\"", color);
            if (!color.StartsWith("#")) {
                backgroundColorStyle = string.Format(" style=\"background-color: #{0}!important;\"", color);
            }
        }
        str.AppendFormat("<div class=\"theme-color-option{0}\" data-option=\"{3}\" onclick=\"{1}\" data-color=\"{2}\">", selectedClass, onclickFunction, color, currOptionNum);
        str.AppendFormat("<div class=\"color-option-toplogo\"{0}></div>", backgroundColorStyle);
        str.Append("<div class=\"color-option-topbar\"></div>");
        str.Append("<div class=\"color-option-sidebar\"></div>");
        str.Append("<div class=\"color-option-container\"></div>");
        str.Append("<div class=\"color-option-footer\"></div>");
        str.Append("</div>");
        #endregion

        if (!string.IsNullOrEmpty(selectedColor)) {
            str.AppendFormat("<div class=\"color-option-picker\"><input type=\"color\" class=\"color-option-input textEntry\" value=\"{0}\" style=\"width: 75px;\" />", CreateFormattedHexColor(selectedColor));
        }
        else {
            str.Append("<div class=\"color-option-picker\"><input type=\"color\" class=\"color-option-input textEntry\" style=\"width: 75px;\" />");
        }
        str.AppendFormat("<input type=\"button\" class=\"input-buttons\" value=\"Update\" onclick=\"{0}\" style=\"width: 75px;\" /><div class=\"clear\"></div></div>", onColorUpateFunction);

        str.AppendFormat("<div class=\"clear-space-five\"></div><span class=\"radiobutton-style\"><input type=\"checkbox\" id=\"cb_usedefaultthemecolor\" name=\"cb_usedefaultthemecolor\" onchange=\"{0}\" class=\"margin-left cb_usedefaultthemecolor_class\" /><label for=\"cb_usedefaultthemecolor\">&nbsp;Use default colors</label></span><div class=\"clear\"></div>", onResetFunction);

        string cssElements = "<style data-id=\"theme-coloroption-style\" type=\"text/css\">";
        if (HelperMethods.DoesPageContainStr("appremote")) {
            cssElements += "body[data-coloroption=\"1\"] .iframe-top-bar {{ border-bottom: 1px solid {6}!important; }}";
            cssElements += "body[data-coloroption=\"1\"] .sidebar-menu-toggle, body[data-coloroption=\"1\"] .close-iframe, body[data-coloroption=\"1\"] .minimize-iframe {{ filter: {5}; }}";
            cssElements += "body[data-coloroption=\"3\"] .iframe-top-bar {{ border-bottom: 1px solid {6}!important; }}";
            cssElements += "body[data-coloroption=\"3\"] .sidebar-menu-toggle, body[data-coloroption=\"3\"] .close-iframe, body[data-coloroption=\"3\"] .minimize-iframe {{ filter: {5}; }}";
        }
        else {
            cssElements += "body[data-coloroption=\"1\"] .fixed-container-border-left, body[data-coloroption=\"1\"] .fixed-container-border-right {{ background: {6}!important; }}";
            cssElements += "body[data-coloroption=\"3\"] .fixed-container-border-left, body[data-coloroption=\"3\"] .fixed-container-border-right {{ background: {6}!important; }}";
        }
        cssElements += "body[data-coloroption=\"1\"] #top_bar, body[data-coloroption=\"1\"] .iframe-top-bar, body[data-coloroption=\"1\"] #top_bar #lbl_notifications span, body[data-coloroption=\"1\"] .workspace-menu-toggle span {{ background-color: {1}!important; }}";
        cssElements += "body[data-coloroption=\"1\"] #top-logo-holder span.title-text, body[data-coloroption=\"1\"] .iframe-top-bar > .iframe-title-logo span.title-text, body[data-coloroption=\"1\"] .iframe-top-bar > .iframe-title-top-bar, body[data-coloroption=\"1\"] #top_bar #lbl_notifications span, body[data-coloroption=\"1\"] #top_bar li.a, body[data-coloroption=\"1\"] #top-bar-datetime, body[data-coloroption=\"1\"] .searchwrapper-tools-search input, body[data-coloroption=\"1\"] #top_bar_toolview_holder li.a {{ color: {2}!important; }}";
        cssElements += "body[data-coloroption=\"1\"] .searchwrapper-tools-search input::-webkit-input-placeholder, body[data-coloroption=\"3\"] .searchwrapper-tools-search input::-webkit-input-placeholder {{ color: {2}!important; }}";
        cssElements += "body[data-coloroption=\"1\"] #top-button-holder li.a, body[data-coloroption=\"1\"] #notifications_tab li.a .notifications-none, body[data-coloroption=\"1\"] .searchwrapper-tools-search a {{ filter: {5}; }}";
        cssElements += "body[data-coloroption=\"3\"] #top_bar, body[data-coloroption=\"3\"] .iframe-top-bar, body[data-coloroption=\"3\"] #top_bar #lbl_notifications span, body[data-coloroption=\"3\"] .workspace-menu-toggle span {{ background-color: {1}!important; }}";
        cssElements += "body[data-coloroption=\"1\"] #top_bar .notifications-none span, body[data-coloroption=\"3\"] #top_bar .notifications-none span, body[data-coloroption=\"1\"] .workspace-menu-toggle span, body[data-coloroption=\"3\"] .workspace-menu-toggle span {{ filter: {7}!important;  }}";
        cssElements += "body[data-coloroption=\"3\"] #top-logo-holder span.title-text, body[data-coloroption=\"3\"] .iframe-top-bar > .iframe-title-logo span.title-text, body[data-coloroption=\"3\"] .iframe-top-bar > .iframe-title-top-bar, body[data-coloroption=\"3\"] #top_bar #lbl_notifications span, body[data-coloroption=\"3\"] #top_bar li.a, body[data-coloroption=\"3\"] #top-bar-datetime, body[data-coloroption=\"3\"] .searchwrapper-tools-search input, body[data-coloroption=\"3\"] #top_bar_toolview_holder li.a {{ color: {2}!important; }}";
        cssElements += "body[data-coloroption=\"3\"] #top-button-holder li.a, body[data-coloroption=\"3\"] #notifications_tab li.a .notifications-none, body[data-coloroption=\"3\"] .searchwrapper-tools-search a {{ filter: {5}; }}";
        cssElements += "body[data-coloroption=\"2\"] #top_bar, body[data-coloroption=\"2\"] .iframe-top-bar {{ background-color: #F9F9F9!important; }}";
        cssElements += "body[data-coloroption=\"2\"] #top-logo-holder span.title-text, body[data-coloroption=\"2\"] .iframe-top-bar > .iframe-title-logo span.title-text {{ color: {2}!important; }}";
        cssElements += "body[data-coloroption=\"4\"] #top_bar, body[data-coloroption=\"4\"] .iframe-top-bar {{ background-color: #F9F9F9!important; }}";
        cssElements += "body[data-coloroption=\"4\"] #top-logo-holder span.title-text, body[data-coloroption=\"4\"] .iframe-top-bar > .iframe-title-logo span.title-text {{ color: {2}!important; }}";
        cssElements += "body[data-coloroption=\"{0}\"] #top-logo-holder, body[data-coloroption=\"{0}\"] .iframe-top-bar > .iframe-title-logo {{ background-color: {1}!important; }}";
        cssElements += "body[data-coloroption=\"{0}\"] .cb-enable label, body[data-coloroption=\"{0}\"] .keyword-split-array-item {{ background-color: {1}!important; color: {2}!important; }}";
        cssElements += "body[data-coloroption=\"{0}\"] .keyword-split-array-item:hover {{ background-color: {3}!important; }}";
        cssElements += "body[data-coloroption=\"{0}\"] .input-buttons-create, body[data-coloroption=\"{0}\"] .update-element-modal .update-progress-box.color-filled {{ background-color: {1}!important; border-color: {3}!important; color: {2}!important; }}";
        cssElements += "body[data-coloroption=\"{0}\"] .input-buttons-create:hover {{ background-color: {3}!important; border-color: {3}!important; }}";
        cssElements += "body[data-coloroption=\"{0}\"] .input-buttons-create:active {{ background-color: {4}!important; border-color: {3}!important; }}";
        cssElements += "body[data-coloroption=\"{0}\"] ul.sitemenu-selection li, body[data-coloroption=\"{0}\"] .modal-style1 .ModalHeader {{ background-color: {1}; }}";
        cssElements += "body[data-coloroption=\"{0}\"] ul.sitemenu-selection li:hover {{ background-color: {3}; }}";
        cssElements += "body[data-coloroption=\"{0}\"] ul.sitemenu-selection li:active {{ background-color: {4}; }}";
        cssElements += "body[data-coloroption=\"{0}\"] ul.sitemenu-selection li > a, body[data-coloroption=\"{0}\"] .modal-style1 .ModalHeader > div {{ color: {2}; }}";
        cssElements += "body[data-coloroption=\"{0}\"] ul.sitemenu-selection li.active {{ border-top-color: {1}!important; }}";
        cssElements += "body[data-coloroption=\"{0}\"] ul.mobile-mode li.active, body[data-coloroption=\"{0}\"] .site-tools-tablist .active, body[data-coloroption=\"{0}\"] .site-tools-tablist a:hover {{ border-left-color: {1}!important; }}";
        cssElements += "</style>";

        if (page != null) {
            const string defaultLightFontColor = "#FFFFFF";
            const string defaultDarkFontColor = "#5F5F5F";

            if (page.Header != null && !string.IsNullOrEmpty(selectedColor)) {
                if (!selectedColor.StartsWith("#")) {
                    selectedColor = "#" + selectedColor;
                }

                string altColor1 = GetAltColorFromHex(selectedColor, 40);
                string altColor2 = GetAltColorFromHex(selectedColor, 70);
                string fontColor = defaultLightFontColor;
                string defaultBrightnessFilter = "brightness(500%)";
                string defaultSpanIndicatorBrightnessFilter = "brightness(20%)";
                string topBorderColor = altColor1;
                if (UseDarkTextColorWithBackground(selectedColor)) {
                    fontColor = defaultDarkFontColor;
                    defaultBrightnessFilter = "brightness(100%)";
                    defaultSpanIndicatorBrightnessFilter = "brightness(100%)";
                    topBorderColor = "inherit";
                }

                // {0} - selectedIndex
                // {1} - selectedColor
                // {2} - fontColor
                // {3} - altColor1
                // {4} = altColor2
                // {5} = filter brightness
                // {6} = top border color
                // {7} = span indicator brightness

                string finalCssElements = string.Format(cssElements, selectedIndex, selectedColor, fontColor, altColor1, altColor2, defaultBrightnessFilter, topBorderColor, defaultSpanIndicatorBrightnessFilter);

                page.Header.Controls.Add(new LiteralControl(finalCssElements));
            }

            RegisterPostbackScripts.RegisterStartupScript(page, "openWSE.SetNewThemeColorOptionFormat('" + cssElements + "', '" + defaultLightFontColor + "', '" + defaultDarkFontColor + "');");
        }

        return str.ToString();
    }

    public static string CreateFormattedHexColor(string color) {
        if (string.IsNullOrEmpty(color)) {
            return string.Empty;
        }

        if (!color.StartsWith("#")) {
            return string.Format("#{0}", color);
        }

        return color;
    }

    public static UserControl CreateUserControl(Page page, string filePath) {
        try {
            string absolutePath = string.Empty;
            if (filePath.StartsWith("~/")) {
                absolutePath = ServerSettings.GetServerMapLocation + filePath.Substring(2).Replace("/", "\\");
            }
            else {
                absolutePath = ServerSettings.GetServerMapLocation + filePath.Replace("/", "\\");
            }

            if (HelperMethods.IsValidDllFile(filePath)) {
                if (File.Exists(absolutePath)) {
                    Assembly userControlDll = Assembly.LoadFrom(absolutePath);
                    Type type = HelperMethods.GetUserControlType(userControlDll, string.Empty);
                    if (type != null) {
                        UserControl uc = (UserControl)page.LoadControl(type, null);
                        return uc;
                    }
                }
            }
            else {
                if (File.Exists(absolutePath)) {
                    UserControl uc = (UserControl)page.LoadControl(filePath);
                    return uc;
                }
                else {
                    string userControlName = new FileInfo(absolutePath).Name.Replace(".ascx", "_ascx");
                    List<string> fileList = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Bin", "*.*", SearchOption.AllDirectories)
                        .Where(s => Path.GetExtension(s).ToLower().Contains("dll") && Path.GetFileName(s).ToLower().StartsWith("app_")).ToList();
                    foreach (string file in fileList) {
                        Assembly userControlDll = Assembly.LoadFrom(file);
                        Type type = HelperMethods.GetUserControlType(userControlDll, userControlName);
                        if (type != null) {
                            UserControl uc = (UserControl)page.LoadControl(type, null);
                            return uc;
                        }
                    }
                }
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }

        return null;
    }
    private static Type GetUserControlType(Assembly userControl, string name) {
        if (userControl != null) {
            Type[] types = userControl.GetTypes();
            foreach (Type t in types) {
                if (typeof(UserControl).IsAssignableFrom(t) && (string.IsNullOrEmpty(name) || (t.Name.ToLower().EndsWith(name.ToLower())))) {
                    return t;
                }
            }
        }

        return null;
    }

    public static void PageRedirect(string url, HttpContext currentContext = null) {
        if (currentContext == null) {
            currentContext = HttpContext.Current;
        }

        if (currentContext != null && currentContext.Response != null) {
            try {
                currentContext.Response.Redirect(url, true);
            }
            catch { 
                // Do nothing
            }
        }
    }

    public static string GetRandomColor() {
        Random random = new Random();
        return string.Format("#{0:X6}", random.Next(0x1000000));
    }

    public static string RemoveProtocolFromUrl(string url) {
        if (!string.IsNullOrEmpty(url)) {
            if (url.StartsWith("http://")) {
                url = url.Substring(5);
            }
            else if (url.StartsWith("https://")) {
                url = url.Substring(6);
            }

            if (url.StartsWith("//www.")) {
                url = url.Substring(2);
            }
        }

        return url;
    }

}