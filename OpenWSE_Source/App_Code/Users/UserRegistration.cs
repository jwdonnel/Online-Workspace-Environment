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
    private readonly AppLog _appLog = new AppLog(false);
    private readonly Notifications _notifications = new Notifications();
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
            _appLog.AddError(e);
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
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["ReceiveAll"]))
            _member.UpdateReceiveAll(true);
        else
            _member.UpdateReceiveAll(false);

        EnableNotifications(_member.Username);
        EnableOverlays(_member.Username);
        #endregion

        #region Theme
        string userTheme = _drDefaults["Theme"];
        if (string.IsNullOrEmpty(userTheme))
            userTheme = "Standard";
        _member.UpdateTheme(userTheme);
        #endregion

        #region Panels
        // Show Datetime
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["ShowDateTime"]))
            _member.UpdateShowDateTime(true);
        else
            _member.UpdateShowDateTime(false);

        // Load Links Blank Page
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["LoadLinksBlankPage"]))
            _member.UpdateLoadLinksBlankPage(true);
        else
            _member.UpdateLoadLinksBlankPage(false);

        // Lock Apps
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["LockAppIcons"]))
            _member.UpdateLockAppIcons(true);
        else
            _member.UpdateLockAppIcons(false);

        // Group Icons
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["GroupIcons"]))
            _member.UpdateGroupIcons(true);
        else
            _member.UpdateGroupIcons(false);

        // Icon Category Count
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["IconCategoryCount"]))
            _member.UpdateShowCategoryCount(true);
        else
            _member.UpdateShowCategoryCount(false);

        // Tool tips
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["ToolTips"]))
            _member.UpdateShowToolTip(true);
        else
            _member.UpdateShowToolTip(false);

        // Taskbar Show All
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["TaskBarShowAll"]))
            _member.UpdateTaskBarShowAll(true);
        else
            _member.UpdateTaskBarShowAll(false);

        // Show Minimized Preview
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["ShowMinimizedPreview"]))
            _member.UpdateShowMinimizedPreview(true);
        else
            _member.UpdateShowMinimizedPreview(false);

        // Hide App Icon
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["HideAppIcon"]))
            _member.UpdateHideAppIcon(true);
        else
            _member.UpdateHideAppIcon(false);

        // Show workspace Num in App Icon
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["ShowWorkspaceNumApp"]))
            _member.UpdateShowWorkspaceNumApp(true);
        else
            _member.UpdateShowWorkspaceNumApp(false);

        // Show Workspace Hover Preview
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["HoverPreviewWorkspace"]) || string.IsNullOrEmpty(_drDefaults["HoverPreviewWorkspace"]))
            _member.UpdateHoverPreviewWorkspace(true);
        else
            _member.UpdateHoverPreviewWorkspace(false);
        #endregion

        #region Container / Apps
        // Workspace Rotate
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["WorkspaceRotate"]))
            _member.UpdateAutoRotateWorkspace(true);
        else
            _member.UpdateAutoRotateWorkspace(false);

        // Workspace Rotate Auto Refresh
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["RotateAutoRefresh"]))
            _member.UpdateRotateAutoRefresh(true);
        else
            _member.UpdateRotateAutoRefresh(false);

        // Workspace Rotate Interval
        if (!string.IsNullOrEmpty(_drDefaults["WorkspaceRotateInterval"]))
            _member.UpdateAutoRotateWorkspaceInterval(_drDefaults["WorkspaceRotateInterval"]);

        // Workspace Rotate Screens
        if (!string.IsNullOrEmpty(_drDefaults["WorkspaceRotateScreens"]))
            _member.UpdateWorkspaceRotateScreens(_drDefaults["WorkspaceRotateScreens"]);

        // Show App Title
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["ShowAppTitle"]))
            _member.UpdateShowAppTitle(true);
        else
            _member.UpdateShowAppTitle(false);

        // Show App Image
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["AppHeaderIcon"]))
            _member.UpdateAppHeaderIcon(true);
        else
            _member.UpdateAppHeaderIcon(false);

        // Clear Properties on Sign Off
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["ClearPropOnSignOff"]))
            _member.UpdateClearPropOnSignOff(true);
        else
            _member.UpdateClearPropOnSignOff(false);

        // App Container
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["AppContainer"]))
            _member.UpdateAppContainer(true);
        else
            _member.UpdateAppContainer(false);

        // Snap To Grid
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["SnapToGrid"]))
            _member.UpdateAppSnapToGrid(true);
        else
            _member.UpdateAppSnapToGrid(false);

        // Animation Speed
        if (!string.IsNullOrEmpty(_drDefaults["AnimationSpeed"])) {
            int tempOut = 200;
            int.TryParse(_drDefaults["AnimationSpeed"], out tempOut);
            _member.UpdateAnimationSpeed(tempOut);
        }
        else
            _member.UpdateAnimationSpeed(200);

        // AutoHide Mode
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["AutoHideMode"]))
            _member.UpdateAutoHideMode(true);
        else
            _member.UpdateAutoHideMode(false);

        // Presentation Mode
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["PresentationMode"]))
            _member.UpdatePresentationMode(true);
        else
            _member.UpdatePresentationMode(false);

        // Total Workspaces
        int totalWorkspaces = 4;
        int.TryParse(_drDefaults["TotalWorkspaces"], out totalWorkspaces);
        _member.UpdateTotalWorkspaces(totalWorkspaces);

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

        #region Background
        // Background Image 1
        if (!string.IsNullOrEmpty(_drDefaults["BackgroundImgs"])) {
            _member.UpdateBackgroundImg(_drDefaults["BackgroundImgs"]);
        }

        // Enable Backgrounds
        if (HelperMethods.ConvertBitToBoolean(_drDefaults["EnableBackgrounds"]))
            _member.UpdateEnableBackgrounds(true);
        else
            _member.UpdateEnableBackgrounds(false);

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
        App wid = new App();
        AppPackages wp = new AppPackages(false);
        string packageid = _drDefaults["AppPackage"];
        string[] apps = wp.GetAppList(packageid);
        foreach (string w in apps) {
            applist += w + ",";
        }

        _member.UpdateEnabledApps_NewUser(applist);
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
        int hour = DateTime.Now.Hour;
        int min = DateTime.Now.Minute;
        int seconds = DateTime.Now.Second;

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
        int month = DateTime.Now.Month;
        int day = DateTime.Now.Day - 1;
        if (day == 0) {
            day = 1;
        }

        if (DateTime.Now.Day == 1) {
            month = DateTime.Now.Month - 1;
            if (month == 0) {
                month = 12;
            }
            day = 25;
        }
        var newDate = new DateTime(DateTime.Now.Year, month, day, hour, min, seconds);
        MembershipUser membershipuser = Membership.GetUser(_userName);

        membershipuser.LastActivityDate = newDate;
        Membership.UpdateUser(membershipuser);
    }

    public void EnableNotifications(string _username) {
        var apps = new App(_username);
        _notifications.GetUserNotifications(_username);
        List<string> userApps = _member.EnabledApps;
        LoadedList1.Clear();
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
            _notifications.AddUserNotification(_username, "236a9dc9-c92a-437f-8825-27809af36a3f", true);

        if (_member.ReceiveAll)
            _notifications.AddUserNotification(_username, "1159aca6-2449-4aff-bacb-5f29e479e2d7", true);

        foreach (string w in userApps) {
            var table = apps.GetAppInformation(w);
            if (table != null) {
                string notifiId = table.NotificationID;
                string[] notifiList = notifiId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string nId in notifiList) {
                    if ((!string.IsNullOrEmpty(nId)) && (!LoadedList1.Contains(nId))) {
                        LoadedList1.Add(nId);
                        _notifications.AddUserNotification(_username, nId, true);
                    }
                }
            }
        }
    }

    private void EnableOverlays(string _username) {
        var apps = new App(_username);
        _workspaceOverlays.GetUserOverlays(_username);
        List<string> userApps = _member.EnabledApps;
        LoadedList1.Clear();
        foreach (string w in userApps) {
            var table = apps.GetAppInformation(w);
            if (table != null) {
                string overlayId = table.OverlayID;
                if (!string.IsNullOrEmpty(overlayId)) {
                    string[] overlayList = overlayId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string oId in overlayList) {
                        WorkspaceOverlay_Coll coll = _workspaceOverlays.GetWorkspaceOverlay(oId);
                        if ((!string.IsNullOrEmpty(coll.OverlayName)) && (!LoadedList1.Contains(oId))) {
                            LoadedList1.Add(oId);
                            _workspaceOverlays.AddUserOverlay(_username, oId);
                        }
                    }
                }
            }
        }
    }

    #region Get Random User Color
    private string GetColor(string colorCode) {
        try {
            Color c = System.Drawing.ColorTranslator.FromHtml(colorCode);
            if ((c.IsKnownColor) || (c.IsNamedColor) || (c.IsSystemColor)) {
                return colorCode;
            }
            return RandomColor();
        }
        catch {
            return colorCode;
        }
    }

    public string RandomColor() {
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