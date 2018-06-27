using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

/// <summary>
/// Summary description for CustomFonts
/// </summary>
public class CustomFonts {
    private const string DefaultCustomFontsFolder = "CustomFonts";

    public static void SetCustomValues(Page page) {
        if (!PageSupported(page)) {
            return;
        }

        StringBuilder str = new StringBuilder();
        SetCustomFontFamily(page);
        str.Append(SetCustomFontSize());
        str.Append(SetCustomFontColor());

        if (!string.IsNullOrEmpty(str.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(page, str.ToString());
        }
    }
    public static void SetCustomValues(Page page, MemberDatabase member) {
        if (!PageSupported(page)) {
            return;
        }

        StringBuilder str = new StringBuilder();
        if (member != null) {
            SetCustomFontFamily(page, member.DefaultBodyFontFamily);
            str.Append(SetCustomFontSize(member.DefaultBodyFontSize));
            str.Append(SetCustomFontColor(member.DefaultBodyFontColor));
        }
        else {
            SetCustomFontFamily(page);
            str.Append(SetCustomFontSize());
            str.Append(SetCustomFontColor());
        }

        if (!string.IsNullOrEmpty(str.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(page, str.ToString());
        }
    }
    public static void SetCustomValues(Page page, Dictionary<string, string> demoMemberDatabase) {
        if (!PageSupported(page)) {
            return;
        }

        StringBuilder str = new StringBuilder();

        string defaultBodyFontFamily = string.Empty;
        string defaultBodyFontSize = string.Empty;
        string defaultBodyFontColor = string.Empty;

        if (demoMemberDatabase != null && demoMemberDatabase.Count > 0) {
            if (demoMemberDatabase.ContainsKey("DefaultBodyFontFamily")) {
                defaultBodyFontFamily = demoMemberDatabase["DefaultBodyFontFamily"];
            }

            if (demoMemberDatabase.ContainsKey("DefaultBodyFontSize")) {
                defaultBodyFontSize = demoMemberDatabase["DefaultBodyFontSize"];
            }

            if (demoMemberDatabase.ContainsKey("DefaultBodyFontColor")) {
                defaultBodyFontColor = demoMemberDatabase["DefaultBodyFontColor"];
            }
        }

        SetCustomFontFamily(page, defaultBodyFontFamily);
        str.Append(SetCustomFontSize(defaultBodyFontSize));
        str.Append(SetCustomFontColor(defaultBodyFontColor));

        if (!string.IsNullOrEmpty(str.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(page, str.ToString());
        }
    }

    private static bool PageSupported(Page page) {
        if (page == null) {
            return false;
        }

        return true;
    }

    #region Set Methods

    private static void SetCustomFontFamily(Page page, string defaultVal = "") {
        ServerSettings _ss = new ServerSettings();
        StartupStyleSheets cssStartup = new StartupStyleSheets(false);

        string cssPath = ServerSettings.GetServerMapLocation + DefaultCustomFontsFolder + "\\";
        string defaultFontFamily = _ss.DefaultBodyFontFamily;

        if (!string.IsNullOrEmpty(defaultVal)) {
            defaultFontFamily = defaultVal;
            if (defaultFontFamily.ToLower() == "inherit") {
                return;
            }
        }

        if (!string.IsNullOrEmpty(defaultFontFamily) && !defaultFontFamily.EndsWith(".css")) {
            defaultFontFamily += ".css";
        }

        if (!string.IsNullOrEmpty(defaultFontFamily) && File.Exists(cssPath + defaultFontFamily)) {
            var scriptPath = page.ResolveUrl("~/" + DefaultCustomFontsFolder + "/" + defaultFontFamily);
            if (_ss.AppendTimestampOnScripts) {
                string querySeperator = "?";
                if (scriptPath.Contains("?")) {
                    querySeperator = "&";
                }

                scriptPath += string.Format("{0}{1}{2}", querySeperator, ServerSettings.TimestampQuery, HelperMethods.GetTimestamp());
            }

            cssStartup.AddCssToPage(scriptPath, page);
        }
    }
    private static string SetCustomFontSize(string defaultVal = "") {
        ServerSettings _ss = new ServerSettings();
        string fontSize = _ss.DefaultBodyFontSize;

        if (!string.IsNullOrEmpty(defaultVal)) {
            fontSize = defaultVal;
        }

        if (!string.IsNullOrEmpty(fontSize)) {
            int tempSize = 0;
            if (int.TryParse(fontSize, out tempSize) && tempSize > 0) {
                return "$('body').css('font-size', '" + fontSize + "px');";
            }
        }

        return string.Empty;
    }
    private static string SetCustomFontColor(string defaultVal = "") {
        ServerSettings _ss = new ServerSettings();
        string fontColor = _ss.DefaultBodyFontColor;

        if (!string.IsNullOrEmpty(defaultVal)) {
            fontColor = defaultVal;
        }

        if (!string.IsNullOrEmpty(fontColor)) {
            return "$('body').css('color', '" + fontColor + "');";
        }

        return string.Empty;
    }

    #endregion

    #region Get Methods

    public static string GetCustomFontFamily(MemberDatabase member) {
        ServerSettings _ss = new ServerSettings();
        StartupStyleSheets cssStartup = new StartupStyleSheets(false);

        string cssPath = ServerSettings.GetServerMapLocation + DefaultCustomFontsFolder + "\\";
        string defaultFontFamily = _ss.DefaultBodyFontFamily;

        string defaultVal = member.DefaultBodyFontFamily;
        if (!string.IsNullOrEmpty(defaultVal)) {
            defaultFontFamily = defaultVal;
            if (defaultFontFamily.ToLower() == "inherit") {
                return string.Empty;
            }
        }

        if (!string.IsNullOrEmpty(defaultFontFamily) && !defaultFontFamily.EndsWith(".css")) {
            defaultFontFamily += ".css";
        }

        return ServerSettings.ResolveUrl("~/" + DefaultCustomFontsFolder + "/" + defaultFontFamily);
    }
    public static string GetCustomFontSize(MemberDatabase member) {
        ServerSettings _ss = new ServerSettings();
        string fontSize = _ss.DefaultBodyFontSize;

        string defaultVal = member.DefaultBodyFontSize;
        if (!string.IsNullOrEmpty(defaultVal)) {
            fontSize = defaultVal;
        }

        if (!string.IsNullOrEmpty(fontSize)) {
            int tempSize = 0;
            if (int.TryParse(fontSize, out tempSize) && tempSize > 0) {
                return fontSize + "px";
            }
        }

        return string.Empty;
    }
    public static string GetCustomFontColor(MemberDatabase member) {
        ServerSettings _ss = new ServerSettings();
        string fontColor = _ss.DefaultBodyFontColor;

        string defaultVal = member.DefaultBodyFontColor;
        if (!string.IsNullOrEmpty(defaultVal)) {
            fontColor = defaultVal;
        }

        if (!string.IsNullOrEmpty(fontColor)) {
            return fontColor;
        }

        return string.Empty;
    }

    #endregion

}