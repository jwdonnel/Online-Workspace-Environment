using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Drawing;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE_Tools.Overlays;

/// <summary>
/// Summary description for UserRegistration
/// </summary>
public class UserRegistration {
    private List<string> LoadedList1 = new List<string>();
    private string _userName;
    private string _role;
    private MemberDatabase _member;
    private NewUserDefaults defaults;
    private Dictionary<string, string> _drDefaults;
    private readonly UserNotifications _notifications = new UserNotifications();
    private readonly WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();

    public UserRegistration(string username, string role) {
        _userName = username;
        _role = role;

        defaults = new NewUserDefaults(role);
        defaults.GetDefaults();
        _drDefaults = defaults.DefaultTable;

        _member = new MemberDatabase(username);
    }

    public void RegisterNewUser(string firstname, string lastname, string email, string color) {
        try {
            _member.UpdateFirstName(firstname);
            _member.UpdateLastName(lastname);

            MembershipUser msu = Membership.GetUser(_userName);
            msu.Email = email;
            Membership.UpdateUser(msu);

            Roles.AddUserToRole(_userName, _role);

            _member.AddUserCustomizationRow(_userName);

            if (string.IsNullOrEmpty(color))
                _member.UpdateColor(RandomColor());
            else
                _member.UpdateColor(GetColor(color));

            _member.UpdateIsNewMember(true);
            SignOffNewUser();
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
    }

    public void RegisterDefaults() {
        try {
            InstallAdminPages();
            InstallApps();
            AddToGroups();
            InstallCustomizations();
            _member.UpdatePrivateAccount(false);
        }
        catch { }
    }

    private void InstallCustomizations() {
        #region Notifications and Overlays
        EnableNotifications(_member.Username);
        EnableOverlays(_member.Username);
        #endregion

        #region Theme
        string userTheme = _drDefaults["Theme"];
        if (string.IsNullOrEmpty(userTheme))
            userTheme = "Standard";
        _member.UpdateTheme(userTheme);
        #endregion

        #region Chat Client
        // Chat Enabled
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["EnableChat"]))
            _member.UpdateEnableChat(true);
        else
            _member.UpdateEnableChat(false);

        // Chat Timeout
        if (!string.IsNullOrEmpty(_drDefaults["ChatTimeout"]))
            _member.UpdateChatTimeout(_drDefaults["ChatTimeout"]);

        // Chat Notification Sound
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["ChatSoundNoti"]))
            _member.UpdateChatSoundNoti(true);
        else
            _member.UpdateChatSoundNoti(false);
        #endregion
    }

    private void InstallAdminPages() {
        string[] adminpages = _drDefaults["UserSignUpAdminPages"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        foreach (string p in adminpages) {
            _member.UpdateAdminPages(p, false);
        }
    }

    private void InstallApps() {
        string applist = string.Empty;
        App wid = new App(_role);
        AppPackages wp = new AppPackages(false);
        string packageid = _drDefaults["AppPackage"];
        string[] apps = wp.GetAppList(packageid);
        foreach (string w in apps) {
            applist += w + ",";
        }

        _member.UpdateEnabledApps_NewUser(applist);

        UserAppSettings userAppSettings = new UserAppSettings( NewUserDefaults.GetRoleID(_role), true);
        UserAppSettings newUserAppSettings = new UserAppSettings(_member.Username, true);
        userAppSettings.BuildUserAppSettings();
        foreach (Dictionary<string, string> appSetting in userAppSettings.UserAppSettingList) {
            string appId = appSetting["AppID"];
            bool allowResize = HelperMethods.ConvertBitToBoolean(appSetting["AllowResize"]);
            bool allowMaximize = HelperMethods.ConvertBitToBoolean(appSetting["AllowMaximize"]);
            string cssClass = appSetting["CssClass"];
            bool autoFullScreen = HelperMethods.ConvertBitToBoolean(appSetting["AutoFullScreen"]);
            string minHeight = appSetting["MinHeight"];
            string minWidth = appSetting["MinWidth"];
            bool allowPopOut = HelperMethods.ConvertBitToBoolean(appSetting["AllowPopOut"]);
            string popOutLoc = appSetting["PopOutLoc"];
            bool autoOpen = HelperMethods.ConvertBitToBoolean(appSetting["AutoOpen"]);
            string defaultWorkspace = appSetting["DefaultWorkspace"];
            string category = appSetting["Category"];
            string appBackgroundColor = appSetting["AppBackgroundColor"];
            string iconBackgroundColor = appSetting["IconBackgroundColor"];
            newUserAppSettings.AddNewAppSetting(appId, allowResize, allowMaximize, cssClass, autoFullScreen, minHeight, minWidth, allowPopOut, popOutLoc, autoOpen, defaultWorkspace, category, appBackgroundColor, iconBackgroundColor);
        }
    }

    private void AddToGroups() {
        string userGroups = string.Empty;
        string[] groupList = _drDefaults["Groups"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        Groups groups = new Groups();
        foreach (string g in groupList) {
            if (!string.IsNullOrEmpty(groups.GetGroupName_byID(g))) {
                userGroups += g + ServerSettings.StringDelimiter;
            }
        }

        // Update the group defaults

        defaults.UpdateDefaults("Groups", userGroups);

        _member.UpdateGroupName(userGroups);
    }

    private void SignOffNewUser() {
        int hour = ServerSettings.ServerDateTime.Hour;
        int min = ServerSettings.ServerDateTime.Minute;
        int seconds = ServerSettings.ServerDateTime.Second;

        if (min >= 20) {
            min = min - 20;
        }
        else {
            int tempmin = min - 20;
            min = 60 + tempmin;
            if (hour > 1) {
                hour = hour - 1;
            }
            else {
                hour = 24 - hour - 1;
            }
        }
        int month = ServerSettings.ServerDateTime.Month;
        int day = ServerSettings.ServerDateTime.Day - 1;
        if (day == 0) {
            day = 1;
        }

        if (ServerSettings.ServerDateTime.Day == 1) {
            month = ServerSettings.ServerDateTime.Month - 1;
            if (month == 0) {
                month = 12;
            }
            day = 25;
        }
        var newDate = new DateTime(ServerSettings.ServerDateTime.Year, month, day, hour, min, seconds);
        MembershipUser membershipuser = Membership.GetUser(_userName);

        membershipuser.LastActivityDate = newDate;
        Membership.UpdateUser(membershipuser);
    }

    public void EnableNotifications(string _username) {
        List<UserNotifications_Coll> coll = _notifications.GetUserNotifications(NewUserDefaults.GetRoleID(_role));
        foreach (UserNotifications_Coll x in coll) {
            _notifications.AddUserNotification(_username, x.NotificationID, x.Email);
        }
    }

    private void EnableOverlays(string _username) {
        _workspaceOverlays.GetUserOverlays(NewUserDefaults.GetRoleID(_role));
        foreach (UserOverlay_Coll coll in _workspaceOverlays.UserOverlays) {
            _workspaceOverlays.AddUserOverlay(_username, coll.OverlayID);
        }
    }

    #region Get Random User Color
    private string GetColor(string colorCode) {
        try {
            Color c = System.Drawing.ColorTranslator.FromHtml("#" + colorCode.Replace("#", string.Empty));
            if (!string.IsNullOrEmpty(c.Name) || c.IsKnownColor || c.IsNamedColor || c.IsSystemColor) {
                return colorCode;
            }
            return RandomColor();
        }
        catch {
            return colorCode;
        }
    }

    public static string RandomColor() {
        Random randomGen = new Random();
        KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
        KnownColor randomColorName = names[randomGen.Next(names.Length)];
        Color randomColor = Color.FromKnownColor(randomColorName);
        int ColorValue = Color.FromName(randomColor.Name).ToArgb();
        string ColorHex = string.Format("{0:x6}", ColorValue);

        if (ColorHex.Length > 6) {
            int start = ColorHex.Length - 6;
            ColorHex = ColorHex.Substring(start);
        }

        return ColorHex;
    }
    #endregion
}