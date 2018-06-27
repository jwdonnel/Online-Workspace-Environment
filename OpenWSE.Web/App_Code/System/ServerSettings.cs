#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Xml;
using OpenWSE_Tools.GroupOrganizer;
using OpenWSE.Core;
using System.Web.Profile;
using OpenWSE_Library.Core.BackgroundServices;
using OpenWSE_Tools.AppServices;
using OpenWSE_Tools.BackgroundServiceDatabaseCalls;
using System.Web.Script.Serialization;
using System.Threading;
using System.Data;

#endregion

[Serializable]
public class ServerSettings {

    private static DatabaseCall dbCall = new DatabaseCall();
    private static Dictionary<string, string> SettingsTable = new Dictionary<string, string>();
    private const string dbTable = "aspnet_ServerSettings";

    public const string RobotsMetaTag = "INDEX, FOLLOW";

    public const string AdminUserName = "Administrator";
    public const string GuestUsername = "Guest User";

    public const string BackupFileExt = ".backup";
    public const string SavedDataFilesExt = ".data";

    // Changing this will also change the value on the javascript page
    public const string TimestampQuery = "vertimestamp=";

    public static string ApplicationID {
        get {
            string applicationId = WebConfigurationManager.AppSettings[DatabaseCall.ApplicationIdString];

            if (!string.IsNullOrEmpty(applicationId)) {
                if (Membership.ApplicationName != applicationId) {
                    Membership.ApplicationName = applicationId;
                }

                if (ProfileManager.ApplicationName != applicationId) {
                    ProfileManager.ApplicationName = applicationId;
                }

                if (Roles.ApplicationName != applicationId) {
                    Roles.ApplicationName = applicationId;
                }
            }

            return applicationId;
        }
    }

    #region Delimiters
    public const string StringDelimiter = ";";
    public static string[] StringDelimiter_Array = { ";" };
    #endregion


    /// <summary> Start the Background Services on Application Load
    /// </summary>
    private static void StartBackgroundServices() {
        BackgroundServices backgroundServices = new BackgroundServices();
        List<BackgroundServices_Coll> serviceColl = backgroundServices.GetBackgroundServicesWithDefaultStates();
        foreach (BackgroundServices_Coll service in serviceColl) {
            backgroundServices.UpdateState(service.ID, service.Namespace, service.State, service.LogInformation, ServerSettings.AdminUserName.ToLower());
        }
    }

    public static void SetRememberMeOnLogin(Login loginCtrl, HttpResponse response) {
        if (response != null) {
            if (loginCtrl != null && loginCtrl.RememberMeSet) {
                //clear any other tickets that are already in the response
                response.Cookies.Clear();

                //set the new expiry date - to thirty days from now
                DateTime expiryDate = ServerSettings.ServerDateTime.AddDays(30);

                //create a new forms auth ticket
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(2, loginCtrl.UserName, ServerSettings.ServerDateTime, expiryDate, true, String.Empty);

                //encrypt the ticket
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                //create a new authentication cookie - and set its expiration date
                HttpCookie authenticationCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                authenticationCookie.Expires = ticket.Expiration;

                //add the cookie to the response.
                response.Cookies.Add(authenticationCookie);
            }
        }
    }

    public static string SystemFilePathPrefix {
        get {
            // DirectoryInfo di = new DirectoryInfo(ServerSettings.GetServerMapLocation);
            // return di.Root.Name;

            return "C:\\";
        }
    }

    private static Configuration rootWebConfig;
    public static Configuration GetRootWebConfig {
        get {
            if (rootWebConfig == null) {
                try {
                    var configFile = new FileInfo(ServerSettings.GetServerMapLocation + "Web.config");
                    var vdm = new VirtualDirectoryMapping(configFile.DirectoryName, true, configFile.Name);
                    var wcfm = new WebConfigurationFileMap();
                    wcfm.VirtualDirectories.Add("/", vdm);
                    rootWebConfig = WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/");
                }
                catch {
                    rootWebConfig = WebConfigurationManager.OpenWebConfiguration("~/");
                }
            }

            return rootWebConfig;
        }
    }

    private static string _serverMapLocation = null;
    public static string GetServerMapLocation {
        get {
            if (_serverMapLocation == null && HttpContext.Current != null && HttpContext.Current.Server != null) {
                string serverLoc = HttpContext.Current.Server.MapPath("~");
                if (serverLoc[serverLoc.Length - 1] != '\\') {
                    serverLoc += "\\";
                }

                _serverMapLocation = serverLoc;
            }
            else if (_serverMapLocation == null) {
                return string.Empty;
            }

            return _serverMapLocation;
        }
    }
    private static void SetServerMapLocation() {
        if (HttpContext.Current != null && HttpContext.Current.Server != null) {
            string serverLoc = HttpContext.Current.Server.MapPath("~");
            if (serverLoc[serverLoc.Length - 1] != '\\') {
                serverLoc += "\\";
            }

            _serverMapLocation = serverLoc;
        }
    }

    public const string DefaultConnectionStringName = "ApplicationServices";

    private static List<DatabaseQuery> QueryList {
        get {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

            return query;
        }
    }

    public ServerSettings() {
        CheckAndBuildServerSettingsTable();
    }

    private static object _lockServerSettingsTableObj = new object();
    private static void CheckAndBuildServerSettingsTable() {
        if (SettingsTable.Count == 0) {
            lock (_lockServerSettingsTableObj) {
                if (dbCall.ConnectionString == null) {
                    return;
                }

                List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", QueryList); 
                if (dbSelect.Count > 0) {
                    SettingsTable = dbSelect[0];
                }
            }
        }
    }

    public static void ResetServerSettings() {
        if (dbCall.ConnectionString != null) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", QueryList);

            if (dbSelect.Count > 0) {
                SettingsTable = dbSelect[0];
            }
        }
    }

    public static string AccountImageServerLoc {
        get {
            return ServerSettings.GetServerMapLocation + "Standard_Images\\AcctImages\\";
        }
    }
    public const string AccountImageLoc = "~/Standard_Images/AcctImages/";

    public static bool RunStartServerApplication = false;
    public static void StartServerApplication(bool isPostback) {
        if (!isPostback && RunStartServerApplication) {
            try {
                if (CheckWebConfigFile) {
                    if (dbCall.ConnectionString == null) {
                        return;
                    }

                    DefaultDBTables.CopyCorrectDatabase();

                    // Build default tables if needed
                    DefaultDBTables.BuildDefaults();

                    System.Threading.Thread.Sleep(500);

                    DBViewer dbViewer = new DBViewer(true);
                    foreach (DataRow row in dbViewer.dt.Rows) {
                        DefaultDBTables.InsertDefaultDataIntoTable(row["TABLE_NAME"].ToString(), false);
                    }

                    SetServerMapLocation();
                    SetupApplicationId();
                    StartBackgroundServices();
                    StartCpuUsageMonitor();
                    CreateDefaultRoles();
                    CreateAdminUser();
                    RunStartServerApplication = false;
                }
                else {
                    HelperMethods.PageRedirect("~/Installer.aspx");
                }
            }
            catch { }
        }
    }
    public static void SetupApplicationId() {
        DatabaseCall dbCall = new DatabaseCall();
        string tableName = "Applications";
        if (dbCall.DataProvider == "System.Data.SqlClient") {
            tableName = "aspnet_Applications";
        }

        List<Dictionary<string, string>> dbSelectAppId = dbCall.CallSelect(tableName, "", QueryList);

        if (dbSelectAppId.Count == 0) {
            string applicationId = ServerSettings.ApplicationID;
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, applicationId));
            query.Add(new DatabaseQuery("ApplicationName", applicationId));
            if (dbCall.DataProvider == "System.Data.SqlClient") {
                query.Add(new DatabaseQuery("LoweredApplicationName", applicationId.ToLower()));
            }

            query.Add(new DatabaseQuery("Description", string.Empty));
            dbCall.CallInsert(tableName, query);
        }
    }
    private static void CreateDefaultRoles() {
        List<string> roles = Roles.GetAllRoles().ToList();
        List<string> defaultRoles = new List<string>();
        defaultRoles.Add(ServerSettings.AdminUserName);
        defaultRoles.Add("Standard");
        foreach (string r in defaultRoles) {
            if (!roles.Contains(r)) {
                MemberDatabase.CreateRole(r);
            }
        }
    }
    private static void CreateAdminUser() {
        MembershipUser user = Membership.GetUser(ServerSettings.AdminUserName);
        if (user == null) {
            MembershipCreateStatus status = new MembershipCreateStatus();

            string passwordVal = "";
            for (int i = 0; i <= Membership.MinRequiredPasswordLength; i++) {
                passwordVal += i.ToString();
            }

            if (string.IsNullOrEmpty(passwordVal)) {
                passwordVal = "password";
            }

            Membership.CreateUser(ServerSettings.AdminUserName, passwordVal, "N/A", "?", "None", true, out status);
            Roles.AddUserToRole(ServerSettings.AdminUserName, ServerSettings.AdminUserName);
            MemberDatabase memberDatabase = new MemberDatabase(ServerSettings.AdminUserName);
            memberDatabase.AddUserCustomizationRow(ServerSettings.AdminUserName);
            memberDatabase.UpdateColor("DDDDDD");
            memberDatabase.UpdateIsNewMember(true);
        }
    }

    private static string _checkWebConfigFile = null;
    public static bool CheckWebConfigFile {
        get {
            try {
                if (string.IsNullOrEmpty(_checkWebConfigFile) && HttpContext.Current != null && HttpContext.Current.Request != null) {
                    Configuration cfg = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
                    if (cfg != null) {
                        SystemWebSectionGroup swsg = (SystemWebSectionGroup)cfg.SectionGroups["system.web"];
                        CustomErrorsSection customErrorSection = swsg.CustomErrors;
                        if (customErrorSection != null) {
                            _checkWebConfigFile = (!(customErrorSection.DefaultRedirect == "Installer.aspx")).ToString().ToLower();
                        }
                    }
                }
            }
            catch { }

            return _checkWebConfigFile == "true";
        }
    }

    public static bool NeedToLoadAdminNewMemberPage {
        get {
            try {
                MembershipUserCollection userColl = Membership.GetAllUsers();
                if (userColl.Count == 1) {
                    foreach (MembershipUser u in userColl) {
                        if (u.UserName.ToLower() == AdminUserName.ToLower() && new MemberDatabase(AdminUserName).IsNewMember) {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
            return false;
        }
    }


    #region Cpu Usage Monitor

    private static object _cpuUsageMonitorLockObject = new object();
    private static bool _cpuUsageThreadIsRunning = false;
    public static double CurrentCpuUsageValue = 0.0f;
    public static long CurrentMemoryUsageValue = 0;
    private static void StartCpuUsageMonitor() {
        Thread threadWorker = new Thread(CpuUsageThread);
        threadWorker.Name = "CpuUsageThread";
        threadWorker.Start();
    }
    private static void CpuUsageThread() {
        lock (_cpuUsageMonitorLockObject) {
            ServerSettings ss = new ServerSettings();
            _cpuUsageThreadIsRunning = true;
            int totalLoops = 0;
            bool cpuAlertSent = false;
            bool memoryAlertSent = false;

            DateTime lastTime = DateTime.Now;
            TimeSpan lastTotalProcessorTime = Process.GetCurrentProcess().TotalProcessorTime;
            
            #region Start loop
            while (true) {
                try {
                    if (!ss.MonitorCpuUsage) {
                        break;
                    }

                    var currentProcess = Process.GetCurrentProcess();

                    DateTime currTime = DateTime.Now;
                    TimeSpan currTotalProcessorTime = currentProcess.TotalProcessorTime;

                    double CPUUsage = (currTotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds) / currTime.Subtract(lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);
                    CurrentCpuUsageValue = CPUUsage * 100;
                    if (CurrentCpuUsageValue > 100.0d) {
                        CurrentCpuUsageValue = 100.0d;
                    }
                    else if (CurrentCpuUsageValue < 0.0d || CurrentCpuUsageValue.ToString() == "NaN") {
                        CurrentCpuUsageValue = 0.0d;
                    }

                    lastTime = currTime;
                    lastTotalProcessorTime = currTotalProcessorTime;

                    if (CurrentCpuUsageValue > ss.MonitorCpuUsagePercentAlert && !cpuAlertSent) {
                        AppLog.AddEvent("CPU Usage Alert", "The server's CPU usage for this site has reached " + Math.Round(CurrentCpuUsageValue, 1).ToString() + "%.<br />To change when this message is shown, adjust the 'CPU MONITOR ALERT VALUE' located under Server Timezone and Monitoring Settings on the Site Settings page. The current value is " + Math.Round(ss.MonitorCpuUsagePercentAlert, 1).ToString() + "%.<span style='display: none;'>error</span>");
                        cpuAlertSent = true;
                    }

                    long num = 1024;
                    CurrentMemoryUsageValue = currentProcess.WorkingSet64 / num;

                    if (CurrentMemoryUsageValue > ss.MonitorMemoryUsageAlert && !memoryAlertSent) {
                        AppLog.AddEvent("Memory Usage Alert", "The server's Memory usage for this site has reached " + string.Format("{0:n0}", CurrentMemoryUsageValue) + " K.<br />To change when this message is shown, adjust the 'MEMORY MONITOR ALERT VALUE' located under Server Timezone and Monitoring Settings on the Site Settings page. The current value is " + string.Format("{0:n0}", ss.MonitorMemoryUsageAlert) + " K.<span style='display: none;'>error</span>");
                        memoryAlertSent = true;
                    }

                    BackgroundServiceCalls.UpdateCurrentCpuUsage();

                    Thread.Sleep(4000);
                    totalLoops++;

                    if (totalLoops > 30) {
                        totalLoops = 0;
                        cpuAlertSent = false;
                        memoryAlertSent = false;
                    }
                }
                catch (Exception e) {
                    if (e != null && e.GetType() == typeof(System.PlatformNotSupportedException)) {
                        ServerSettings.update_MonitorCpuUsage(false);
                        break;
                    }
                }
            }
            #endregion

            _cpuUsageThreadIsRunning = false;
        }
    }

    #endregion


    private static int _maxJsonLength = -1;
    public static int GetMaxJsonLength() {
        if (_maxJsonLength == -1) {
            try {
                string path = HttpContext.Current.Server.MapPath("~/Web.Config");
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode node = doc.DocumentElement.SelectSingleNode("system.web.extensions/scripting/webServices/jsonSerialization");
                if (node != null && node.Attributes["maxJsonLength"] != null) {
                    string maxJsonLength = node.Attributes["maxJsonLength"].Value;
                    int maxValue = int.MaxValue;
                    int.TryParse(maxJsonLength, out maxValue);
                    _maxJsonLength = maxValue;
                }
            }
            catch {
                _maxJsonLength = int.MaxValue;
            }
        }

        return _maxJsonLength;
    }

    public static JavaScriptSerializer CreateJavaScriptSerializer() {
        JavaScriptSerializer js = new JavaScriptSerializer();
        js.MaxJsonLength = GetMaxJsonLength();
        return js;
    }

    public static void AddMetaTagsToPage(Page page) {
        // Set the Page title on every postback
        SetPageTitle(page);

        if (!page.IsPostBack) {
            ServerSettings _ss = new ServerSettings();
            if (string.IsNullOrEmpty(page.MetaDescription)) {
                page.MetaDescription = _ss.MetaTagDescription;
            }

            if (string.IsNullOrEmpty(page.MetaKeywords)) {
                page.MetaKeywords = _ss.MetaTagKeywords;
            }

            if (page.Header != null) {
                if (!string.IsNullOrEmpty(ServerSettings.RobotsMetaTag)) {
                    HtmlMeta meta = new HtmlMeta();
                    meta.Name = "robots";
                    meta.Content = ServerSettings.RobotsMetaTag;
                    page.Header.Controls.Add(meta);
                }
            }
        }
    }
    private static void SetPageTitle(Page page) {
        if (!string.IsNullOrEmpty(ServerSettings.SiteName) && !string.IsNullOrEmpty(page.Title)) {
            page.Title = page.Title + " / " + ServerSettings.SiteName;
        }
        else if (!string.IsNullOrEmpty(page.Title)) {
            page.Title = page.Title;
        }
        else {
            page.Title = ServerSettings.SiteName;
        }
    }

    public void AutoUpdateDatabaseCheck() {
        if (CheckWebConfigFile && AutoFixDBIssues) {
            if (dbCall.ConnectionString == null) {
                return;
            }

            try {
                DefaultDBTables.UpdateDefaults();
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }
    }

    public static bool CanLoginToGroup(string username, string requestGroup, HttpContext context) {
        if (!string.IsNullOrEmpty(requestGroup)) {
            bool groupFound = false;
            Groups group = new Groups(username);
            List<string> groupList = group.GetEntryList();
            List<string> memberGroups = new MemberDatabase(username).GroupList;
            foreach (string g in groupList) {
                if (g.ToLower() == requestGroup.ToLower()) {
                    groupFound = true;
                    break;
                }
            }

            if (groupFound) {
                if (!group.IsApartOfGroup(memberGroups, requestGroup)) {
                    return false;
                }
            }
            else if (GroupSessions.DoesUserHaveGroupLoginSessionKey(username)) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Redirect the page with the proper query request for the tool view mode
    /// </summary>
    /// <param name="page">Current Page control</param>
    /// <param name="url">Redirect Url</param>
    public static void PageIFrameRedirect(Page page, string url) {
        if (page != null && page.Request != null && page.Response != null) {
            if (!string.IsNullOrEmpty(url)) {
                string query = string.Empty;
                if (url.IndexOf("?") > 0) {
                    query = url.Substring(url.IndexOf("?"));
                }

                string mainRedirectPath = url;
                if (!string.IsNullOrEmpty(query)) {
                    mainRedirectPath = url.Replace(query, string.Empty);
                    if (query.StartsWith("&") || query.StartsWith("?")) {
                        query = query.Substring(1);
                    }
                }

                if (HelperMethods.ConvertBitToBoolean(page.Request.QueryString["iframeMode"])) {
                    if (!string.IsNullOrEmpty(query)) {
                        query = "iframeMode=true&" + query;
                    }
                    else {
                        query = "iframeMode=true";
                    }
                }

                if (HelperMethods.ConvertBitToBoolean(page.Request.QueryString["mobileMode"])) {
                    if (!string.IsNullOrEmpty(query)) {
                        query = "mobileMode=true&" + query;
                    }
                    else {
                        query = "mobileMode=true";
                    }
                }

                if (HelperMethods.ConvertBitToBoolean(page.Request.QueryString["fromAppRemote"])) {
                    if (!string.IsNullOrEmpty(query)) {
                        query = "fromAppRemote=true&" + query;
                    }
                    else {
                        query = "fromAppRemote=true";
                    }
                }

                if (!string.IsNullOrEmpty(query)) {
                    query = "?" + query;
                }

                HelperMethods.PageRedirect(mainRedirectPath + query);
            }
            else {
                HelperMethods.PageRedirect("~/ErrorPages/Error.html");
            }
        }
    }


    /// <summary>
    /// Refreshes the page using the same url link
    /// </summary>
    /// <param name="page"></param>
    public static void RefreshPage(Page page, string query) {
        if (page != null && page.Request != null && page.Response != null) {
            string url = page.Request.RawUrl;
            if (!string.IsNullOrEmpty(query)) {
                if (url.Contains("?") && !query.Contains("&")) {
                    query = "&" + query;
                }
                else if (url.Contains("&") && !query.Contains("?")) {
                    query = "?" + query;
                }
            }

            HelperMethods.PageRedirect(url + query);
        }
    }


    /// <summary>
    /// Returns a site relative HTTP path from a partial path starting out with a ~.
    /// Same syntax that ASP.Net internally supports but this method can be used
    /// outside of the Page framework.
    /// 
    /// Works like Control.ResolveUrl including support for ~ syntax
    /// but returns an absolute URL.
    /// </summary>
    /// <param name="originalUrl">Any Url including those starting with ~</param>
    /// <returns>relative url</returns>
    public static string ResolveUrl(string originalUrl) {
        if (originalUrl == null)
            return null;

        originalUrl = System.Web.VirtualPathUtility.ToAbsolute(originalUrl);

        return originalUrl;
    }


    /// <summary>
    /// This method returns a fully qualified absolute server Url which includes
    /// the protocol, server, port in addition to the server relative Url.
    /// 
    /// Works like Control.ResolveUrl including support for ~ syntax
    /// but returns an absolute URL.
    /// </summary>
    /// <param name="ServerUrl">Any Url, either App relative or fully qualified</param>
    /// <param name="forceHttps">if true forces the url to use https</param>
    /// <returns></returns>
    public static string ResolveServerUrl(string serverUrl, bool forceHttps) {
        // *** Is it already an absolute Url?
        if (serverUrl.IndexOf("://") > -1)
            return serverUrl;

        // *** Start by fixing up the Url an Application relative Url
        string newUrl = ResolveUrl(serverUrl);

        Uri originalUri = HttpContext.Current.Request.Url;
        newUrl = (forceHttps ? "https" : originalUri.Scheme) +
                 "://" + originalUri.Authority + newUrl;

        return newUrl;
    }


    /// <summary>
    /// Adds an assembly path from the runtime node
    /// </summary>
    /// <param name="name">Assembly path</param>
    public static void AddRuntimeAssemblyBinding(string name) {
        try {
            string path = HttpContext.Current.Server.MapPath("~/Web.Config");
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode node = doc.DocumentElement.SelectSingleNode("runtime");
            if (node == null) {
                return;
            }

            XmlNode probing = node.FirstChild.FirstChild;
            if (probing == null) {
                return;
            }

            string privatePath = probing.Attributes["privatePath"].Value;

            if (privatePath[privatePath.Length - 1] != ServerSettings.StringDelimiter[0]) {
                privatePath += ServerSettings.StringDelimiter;
            }
            privatePath += name;
            probing.Attributes["privatePath"].Value = privatePath;

            doc.Save(path);
        }
        catch { }
    }


    /// <summary>
    /// Removes an assembly path from the runtime node
    /// </summary>
    /// <param name="name">Assembly path</param>
    public static void RemoveRuntimeAssemblyBinding(string name) {
        try {
            string path = HttpContext.Current.Server.MapPath("~/Web.Config");
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode node = doc.DocumentElement.SelectSingleNode("runtime");
            if (node == null) {
                return;
            }

            XmlNode probing = node.FirstChild.FirstChild;
            if (probing == null) {
                return;
            }

            string privatePath = probing.Attributes["privatePath"].Value;

            string[] values = privatePath.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
            string newValues = string.Empty;

            for (int i = 0; i < values.Length; i++) {
                if (values[i] != name) {
                    newValues += values[i] + ServerSettings.StringDelimiter;
                }
            }

            if (newValues[newValues.Length - 1] == ServerSettings.StringDelimiter[0]) {
                newValues = newValues.Remove(newValues.Length - 1);
            }

            probing.Attributes["privatePath"].Value = newValues;

            doc.Save(path);
        }
        catch { }
    }


    /// <summary>
    /// Deletes all temp backup files for a full database backup
    /// </summary>
    public static void DeleteBackupTempFolderFiles() {
        string[] files = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Backups\\Temp");
        foreach (string file in files) {
            try {
                if (File.Exists(file)) {
                    File.Delete(file);
                }
            }
            catch { }
        }
    }


    #region SMTP Mail Setup
    public static void SendNewEmail(MailMessage mailMessage, string emailHeaderTitle, string _subject, string _messageBody) {
        ServerSettings _ss = new ServerSettings();
        if (_ss.EmailSystemStatus) {
            GetMailSettingList();
            if (MailSettings_Coll.Count > 0) {
                try {
                    int index = MailSettings_Coll.Count - 1;
                    MailSettings mailsettings = MailSettings_Coll[index];
                    string from = mailsettings.EmailAddress;
                    string password = mailsettings.Password;
                    string address = mailsettings.SMTP_Address;
                    int portnumber = 587;
                    int.TryParse(mailsettings.PortNumber, out portnumber);
                    bool ssl = mailsettings.SSL;
                    mailMessage.From = new MailAddress(from);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Subject = _subject;

                    // Build email body
                    StringBuilder bodyStr = new StringBuilder();
                    bool needTitle = true;
                    if (!string.IsNullOrEmpty(EmailHeader)) {
                        if (EmailHeader.Contains("[MESSAGE_TITLE]")) {
                            emailHeaderTitle = StripTagsRegex(emailHeaderTitle);
                            string tempEmailHeader = EmailHeader.Replace("[MESSAGE_TITLE]", emailHeaderTitle);
                            bodyStr.Append(tempEmailHeader);
                            needTitle = false;
                        }
                        else
                            bodyStr.Append(EmailHeader);

                        bodyStr.Append("<div style='clear:both;height:5px'></div>");
                    }
                    if (needTitle) {
                        bodyStr.Append(emailHeaderTitle);
                        bodyStr.Append("<div style='clear:both;height:5px'></div>");
                    }
                    bodyStr.Append(_messageBody);
                    if (!string.IsNullOrEmpty(EmailFooter)) {
                        bodyStr.Append("<div style='clear:both;height:15px'></div>");
                        if (EmailFooter.Contains("[MESSAGE_TITLE]")) {
                            string tempEmailFooter = EmailFooter.Replace("[MESSAGE_TITLE]", emailHeaderTitle);
                            bodyStr.Append(tempEmailFooter);
                        }
                        else
                            bodyStr.Append(EmailFooter);
                    }

                    mailMessage.Body = bodyStr.ToString();

                    var cred = new NetworkCredential(from, password);
                    var emailClient = new SmtpClient(address, portnumber);
                    emailClient.EnableSsl = ssl;
                    emailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    emailClient.UseDefaultCredentials = false;
                    emailClient.Timeout = 10000;
                    emailClient.Credentials = cred;
                    emailClient.Send(mailMessage);
                    emailClient.Dispose();
                }
                catch (Exception e) {
                    AppLog.AddError(e);
                }
            }
        }
    }


    public static bool CheckAppGroupAssociation(Apps_Coll appColl, MemberDatabase member) {
        string createdBy = appColl.CreatedBy;
        if ((!string.IsNullOrEmpty(createdBy)) && (createdBy.ToLower() != ServerSettings.AdminUserName.ToLower()) && (HttpContext.Current.User.Identity.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
            var m = new MemberDatabase(createdBy);
            if (!HelperMethods.CompareUserGroups(member, m)) {
                return false;
            }
        }

        return true;
    }
    public static bool CheckPluginGroupAssociation(SitePlugins_Coll pluginColl, MemberDatabase member) {
        string createdBy = pluginColl.CreatedBy;
        if ((!string.IsNullOrEmpty(createdBy)) && (createdBy.ToLower() != ServerSettings.AdminUserName.ToLower()) && (HttpContext.Current.User.Identity.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
            var m = new MemberDatabase(createdBy);
            if (!HelperMethods.CompareUserGroups(member, m)) {
                return false;
            }
        }

        return true;
    }
    public static bool CheckCustomTablesGroupAssociation(CustomTable_Coll tableColl, MemberDatabase member) {
        string createdBy = tableColl.UpdatedBy;
        if ((!string.IsNullOrEmpty(createdBy)) && (createdBy.ToLower() != ServerSettings.AdminUserName.ToLower()) && (HttpContext.Current.User.Identity.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
            var m = new MemberDatabase(createdBy);
            if (!HelperMethods.CompareUserGroups(member, m)) {
                return false;
            }
        }

        return true;
    }
    public static bool CheckDBImportsGroupAssociation(DBImporter_Coll tableColl, MemberDatabase member) {
        string createdBy = tableColl.ImportedBy;
        if ((!string.IsNullOrEmpty(createdBy)) && (createdBy.ToLower() != ServerSettings.AdminUserName.ToLower()) && (HttpContext.Current.User.Identity.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
            var m = new MemberDatabase(createdBy);
            if (!HelperMethods.CompareUserGroups(member, m)) {
                return false;
            }
        }

        return true;
    }


    private static string StripTagsRegex(string source) {
        return string.IsNullOrEmpty(source) ? source : Regex.Replace(source, "<.*?>", string.Empty);
    }

    public static string EmailHeader {
        get {
            string temp = string.Empty;
            GetMailSettingList();
            if (_coll.Count == 1) {
                temp = _coll[0].Header;
            }

            return temp;
        }
    }

    public static string EmailFooter {
        get {
            string temp = string.Empty;

            GetMailSettingList();
            if (_coll.Count == 1) {
                temp = _coll[0].Footer;
            }

            return temp;
        }
    }
    #endregion


    #region Get Methods

    public static string GetSitePath(HttpRequest request) {
        string absolutePath = string.Empty;
        try {
            string servername = HttpUtility.UrlEncode(request.ServerVariables["SERVER_NAME"]);
            absolutePath = "//" + HttpUtility.UrlEncode(request.ServerVariables["SERVER_NAME"]);
            if (servername == "localhost")
                absolutePath += ":" + HttpUtility.UrlEncode(request.ServerVariables["SERVER_PORT"]) + request.ApplicationPath;

            else
                absolutePath = "//" + HttpUtility.UrlEncode(request.ServerVariables["SERVER_NAME"]) + request.ApplicationPath;
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        return absolutePath;
    }

    private static void CheckAndCreateServerSettings() {
        DatabaseCall dbCall = new DatabaseCall();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", QueryList);
        if (dbSelect.Count == 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();

            List<string> columnList = DefaultDBTables.GetDatabaseTableDefault(dbTable, dbCall);
            foreach (string column in columnList) {
                if (column == DatabaseCall.ApplicationIdString) {
                    query.Add(new DatabaseQuery(column, ServerSettings.ApplicationID));
                }
                else {
                    query.Add(new DatabaseQuery(column, null));
                }
            }

            dbCall.CallInsert(dbTable, query);
        }
    }

    public bool SignUpConfirmationEmail {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("SignUpConfirmationEmail")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SignUpConfirmationEmail"]);
            }

            return false;
        }
    }

    public string ShowUpdatesPopupDate {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("ShowUpdatesPopupDate")) {
                string value = SettingsTable["ShowUpdatesPopupDate"];
                if (string.IsNullOrEmpty(value)) {
                    return "N/A";
                }
                else {
                    return value;
                }
            }

            return "N/A";
        }
    }

    public bool AllowAppRating {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AllowAppRating")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["AllowAppRating"]);
            }

            return false;
        }
    }

    public bool AppendTimestampOnScripts {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AppendTimestampOnScripts")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["AppendTimestampOnScripts"]);
            }

            return false;
        }
    }

    public bool SaveCookiesAsSessions {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("SaveCookiesAsSessions")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SaveCookiesAsSessions"]);
            }

            return false;
        }
    }

    public bool DeleteOldLoginActivity {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("DeleteOldLoginActivity")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["DeleteOldLoginActivity"]);
            }

            return false;
        }
    }

    public int LoginActivityToKeepInDays {
        get {
            int daysToKeep = 5;
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LoginActivityToKeepInDays")) {
                string value = SettingsTable["LoginActivityToKeepInDays"];
                if (!string.IsNullOrEmpty(value)) {
                    int.TryParse(value, out daysToKeep);
                    if (daysToKeep <= 0) {
                        daysToKeep = 5;
                    }

                    return daysToKeep;
                }
            }

            return daysToKeep;
        }
    }

    public string LoginMessage {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LoginMessage")) {
                string value = SettingsTable["LoginMessage"];
                if (string.IsNullOrEmpty(value)) {
                    return string.Empty;
                }
                else {
                    return value;
                }
            }

            return string.Empty;
        }
    }

    public string LoginMessageDate {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LoginMessageDate")) {
                string value = SettingsTable["LoginMessageDate"];
                if (string.IsNullOrEmpty(value)) {
                    return "N/A";
                }
                else {
                    return value;
                }
            }

            return "N/A";
        }
    }

    public string TwitterAccessToken {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("TwitterAccessToken")) {
                return SettingsTable["TwitterAccessToken"];
            }

            return string.Empty;
        }
    }

    public string TwitterAccessTokenSecret {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("TwitterAccessTokenSecret")) {
                return SettingsTable["TwitterAccessTokenSecret"];
            }

            return string.Empty;
        }
    }

    public string TwitterConsumerKey {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("TwitterConsumerKey")) {
                return SettingsTable["TwitterConsumerKey"];
            }

            return string.Empty;
        }
    }

    public string TwitterConsumerSecret {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("TwitterConsumerSecret")) {
                return SettingsTable["TwitterConsumerSecret"];
            }

            return string.Empty;
        }
    }

    public bool RecordLoginActivity {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("RecordLoginActivity")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["RecordLoginActivity"]);
            }

            return false;
        }
    }

    public int TotalWorkspacesAllowed {
        get {
            CheckAndBuildServerSettingsTable();
            int defaultWorkspaces = 4;
            if (SettingsTable.ContainsKey("TotalWorkspacesAllowed")) {
                string value = SettingsTable["TotalWorkspacesAllowed"];
                int.TryParse(value, out defaultWorkspaces);
                if (defaultWorkspaces == 0) {
                    defaultWorkspaces = 4;
                }
            }
            return defaultWorkspaces;
        }
    }

    public bool AssociateWithGroups {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AssociateWithGroups")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["AssociateWithGroups"]);
            }

            return false;
        }
    }

    public bool ForceGroupLogin {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("ForceGroupLogin")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["ForceGroupLogin"]);
            }

            return false;
        }
    }

    public bool ShowLoginModalOnDemoMode {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("ShowLoginModalOnDemoMode")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["ShowLoginModalOnDemoMode"]);
            }

            return false;
        }
    }

    public bool RecordPageViews {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("RecordPageViews")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["RecordPageViews"]);
            }

            return false;
        }
    }

    public bool RecordSiteRequests {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("RecordSiteRequests")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["RecordSiteRequests"]);
            }

            return false;
        }
    }

    public int MaxRequestRecordSize {
        get {
            CheckAndBuildServerSettingsTable();
            int defaultVal = 4000;
            if (SettingsTable.ContainsKey("MaxRequestRecordSize")) {
                string value = SettingsTable["MaxRequestRecordSize"];
                int.TryParse(value, out defaultVal);
                if (defaultVal == 0) {
                    defaultVal = 4000;
                }
            }
            return defaultVal;
        }
    }

    public bool EmailOnRegister {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("EmailOnRegister")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["EmailOnRegister"]);
            }

            return false;
        }
    }

    public bool LockCustomTables {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LockCustomTables")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["LockCustomTables"]);
            }

            return false;
        }
    }

    public bool NoLoginRequired {
        get {
            CheckAndBuildServerSettingsTable();
            bool returnBool = false;
            if (SettingsTable.ContainsKey("NoLoginRequired")) {
                try {
                    string value = SettingsTable["NoLoginRequired"];
                    if (HelperMethods.ConvertBitToBoolean(value)) {
                        returnBool = true;
                    }

                    if (HttpContext.Current.Session != null && HttpContext.Current.Session["DemoMode"] != null && HelperMethods.ConvertBitToBoolean(HttpContext.Current.Session["DemoMode"].ToString())) {
                        return true;
                    }
                }
                catch { }
            }

            return returnBool;
        }
    }

    public bool ShowPreviewButtonLogin {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("ShowPreviewButtonLogin")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["ShowPreviewButtonLogin"]);
            }

            return false;
        }
    }

    public bool SitePluginsLocked {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("SitePluginsLocked")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SitePluginsLocked"]);
            }

            return false;
        }
    }

    public bool OverlaysLocked {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("OverlaysLocked")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["OverlaysLocked"]);
            }

            return false;
        }
    }

    public bool AllowPrivacy {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AllowPrivacy")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["AllowPrivacy"]);
            }

            return false;
        }
    }

    public bool EmailSystemStatus {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("EmailSystemStatus")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["EmailSystemStatus"]);
            }

            return false;
        }
    }

    public bool RecordActivity {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("RecordActivity")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["RecordActivity"]);
            }

            return false;
        }
    }

    public bool RecordActivityToLogFile {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("RecordActivityToLogFile")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["RecordActivityToLogFile"]);
            }

            return false;
        }
    }

    public bool RecordErrorsOnly {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("RecordErrorsOnly")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["RecordErrorsOnly"]);
            }

            return false;
        }
    }

    public bool SiteOffLine {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("SiteOffLine")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SiteOffLine"]);
            }

            return false;
        }
    }

    public bool EmailActivity {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("EmailActivity")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["EmailActivity"]);
            }

            return false;
        }
    }

    public bool AutoBlockIP {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AutoBlockIP")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["AutoBlockIP"]);
            }

            return false;
        }
    }

    public bool AutoUpdates {
        get {
            BackgroundServices bs = new BackgroundServices();
            BackgroundStates state = bs.GetCurrentStateFromNamespace(OpenWSE_Tools.AutoUpdates.AutoUpdateSystem.AutoUpdateServiceNamespace);
            if (state == BackgroundStates.Running) {
                return true;
            }

            return false;
        }
    }

    public int AutoBlockIPCount {
        get {
            CheckAndBuildServerSettingsTable();

            int temp = 10;

            if (SettingsTable.ContainsKey("AutoBlockIPCount")) {
                string value = SettingsTable["AutoBlockIPCount"];
                int.TryParse(value, out temp);
                if (temp == 0) {
                    temp = 10;
                }
            }

            return temp;
        }
    }

    public int WebEventsToKeep {
        get {
            CheckAndBuildServerSettingsTable();
            int temp = 0;
            if (SettingsTable.ContainsKey("WebEventsToKeep")) {
                int.TryParse(SettingsTable["WebEventsToKeep"], out temp);
            }
            return temp;
        }
    }

    public bool ChatEnabled {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("ChatEnabled")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["ChatEnabled"]);
            }

            return false;
        }
    }

    public bool CacheHomePage {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("CacheHomePage")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["CacheHomePage"]);
            }

            return false;
        }
    }

    public bool LockFileManager {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LockFileManager")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["LockFileManager"]);
            }

            return false;
        }
    }

    public bool SSLRedirect {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("SSLRedirect")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SSLRedirect"]);
            }

            return false;
        }
    }

    public bool URLValidation {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("URLValidation")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["URLValidation"]);
            }

            return false;
        }
    }

    public bool LockStartupScripts {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LockStartupScripts")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["LockStartupScripts"]);
            }

            return false;
        }
    }

    public bool LockIPListenerWatchlist {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LockIPListenerWatchlist")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["LockIPListenerWatchlist"]);
            }

            return false;
        }
    }

    public bool CustomizationsLocked {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("CustomizationsLocked")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["CustomizationsLocked"]);
            }

            return false;
        }
    }

    public bool AllowUserSignUp {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AllowUserSignUp")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["AllowUserSignUp"]);
            }

            return false;
        }
    }

    public string UserSignUpRole {
        get {
            CheckAndBuildServerSettingsTable();
            string defaultName = "Standard";
            if (SettingsTable.ContainsKey("UserSignUpRole")) {
                string value = SettingsTable["UserSignUpRole"];
                defaultName = value;
                if (string.IsNullOrEmpty(defaultName)) {
                    defaultName = "Standard";
                }
            }

            return defaultName;
        }
    }

    public static string SiteName {
        get {
            try {
                if (SettingsTable.Count > 0 && SettingsTable.ContainsKey("SiteName") && !string.IsNullOrEmpty(SettingsTable["SiteName"])) {
                    return SettingsTable["SiteName"];
                }
                else {
                    DatabaseQuery dbQuery = dbCall.CallSelectSingle(dbTable, "SiteName", QueryList);
                    if (dbQuery != null && !string.IsNullOrEmpty(dbQuery.Value)) {
                        return dbQuery.Value;
                    }
                }
            }
            catch { }

            return "My Site Name";
        }
    }

    public string AdminNote {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AdminNote")) {
                return SettingsTable["AdminNote"];
            }

            return string.Empty;
        }
    }

    public string AdminNoteBy {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AdminNoteBy")) {
                return SettingsTable["AdminNoteBy"];
            }

            return string.Empty;
        }
    }

    public string AppInstallerPackage {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AppInstallerPackage")) {
                return SettingsTable["AppInstallerPackage"];
            }

            return string.Empty;
        }
    }

    public static string DefaultStartupPage {
        get {
            try {
                string defaultPage = WebConfigurationManager.AppSettings["DefaultLoginDocument"];
                if (string.IsNullOrEmpty(defaultPage))
                    return "Login.aspx";

                return defaultPage;
            }
            catch { }

            return "Login.aspx";
        }
    }

    public DateTime LastCacheClearDate {
        get {
            CheckAndBuildServerSettingsTable();
            var t = new DateTime();
            if (SettingsTable.ContainsKey("LastCacheDate")) {
                string value = SettingsTable["LastCacheDate"];
                DateTime temp;
                if (DateTime.TryParse(value, out temp)) {
                    t = temp;
                }
            }
            return t;
        }
    }

    public static int getTotalBlockedIP {
        get {
            var ipwatch = new IPWatch(true);
            int count = 0;
            for (var i = 0; i < ipwatch.ipwatchdt.Count; i++) {
                if (HelperMethods.ConvertBitToBoolean(ipwatch.ipwatchdt[i]["Blocked"])) {
                    count++;
                }
            }

            return count;
        }
    }

    public static int getTotalIPWatch {
        get {
            int count = 0;
            var ipwatch = new IPWatch(true);
            for (var i = 0; i < ipwatch.ipwatchdt.Count; i++) {
                Dictionary<string, string> row = ipwatch.ipwatchdt[i];
                if (HelperMethods.ConvertBitToBoolean(row["Blocked"])) {
                    count++;
                }
            }

            return count;
        }
    }

    public static string GetIpListener {
        get {
            var listener = new IPListener(true);
            return listener.TotalActive > 0 ? "On" : "Off";
        }
    }

    public static double CalculateSpeed {
        get {
            DateTime _start = ServerSettings.ServerDateTime;

            DateTime _end = ServerSettings.ServerDateTime;
            TimeSpan _duration = (_end - _start);

            double timediff = (512 / _duration.TotalSeconds) / 100;
            double speed = Math.Round(timediff, 3);

            return speed;
        }
    }

    public bool UserSignUpEmailChecker {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("UserSignUpEmailChecker")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["UserSignUpEmailChecker"]);
            }

            return false;
        }
    }

    public bool DisableJavascriptErrorAlerts {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("DisableJavascriptErrorAlerts")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["DisableJavascriptErrorAlerts"]);
            }

            return false;
        }
    }

    public string DefaultBodyFontFamily {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("DefaultBodyFontFamily")) {
                return SettingsTable["DefaultBodyFontFamily"];
            }

            if (File.Exists(ServerSettings.GetServerMapLocation + "App_Data\\DatabaseDefaultValues.xml")) {
                try {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(ServerSettings.GetServerMapLocation + "App_Data\\DatabaseDefaultValues.xml");
                    if (xmlDoc != null && xmlDoc.DocumentElement != null) {
                        XmlNodeList tableList = xmlDoc.DocumentElement.SelectNodes("Table[@name='aspnet_ServerSettings']");
                        if (tableList.Count == 1 && tableList[0].FirstChild != null && tableList[0].FirstChild.Attributes["DefaultBodyFontFamily"] != null) {
                            return tableList[0].FirstChild.Attributes["DefaultBodyFontFamily"].Value;
                        }
                    }
                }
                catch { }
            }

            return string.Empty;
        }
    }

    public string DefaultBodyFontSize {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("DefaultBodyFontSize")) {
                return SettingsTable["DefaultBodyFontSize"];
            }

            return string.Empty;
        }
    }

    public string DefaultBodyFontColor {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("DefaultBodyFontColor")) {
                string fontColor = SettingsTable["DefaultBodyFontColor"];
                if (!string.IsNullOrEmpty(fontColor)) {
                    if (!fontColor.StartsWith("#")) {
                        fontColor = "#" + fontColor;
                    }

                    return fontColor;
                }
            }

            return string.Empty;
        }
    }

    public string UserSignUpEmailAssociation {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("UserSignUpEmailAssociation")) {
                return SettingsTable["UserSignUpEmailAssociation"];
            }

            return string.Empty;
        }
    }

    public string GoogleMapsApiKey {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("GoogleMapsApiKey")) {
                return SettingsTable["GoogleMapsApiKey"];
            }

            return string.Empty;
        }
    }

    public bool SignInWithGoogle {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("SignInWithGoogle")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SignInWithGoogle"]);
            }

            return false;
        }
    }
    public string GoogleClientId {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("GoogleClientId")) {
                return SettingsTable["GoogleClientId"];
            }

            return string.Empty;
        }
    }
    public string GoogleClientSecret {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("GoogleClientSecret")) {
                return SettingsTable["GoogleClientSecret"];
            }

            return string.Empty;
        }
    }

    public bool SignInWithTwitter {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("SignInWithTwitter")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SignInWithTwitter"]);
            }

            return false;
        }
    }

    public bool SignInWithFacebook {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("SignInWithFacebook")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SignInWithFacebook"]);
            }

            return false;
        }
    }
    public string FacebookAppId {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("FacebookAppId")) {
                return SettingsTable["FacebookAppId"];
            }

            return string.Empty;
        }
    }
    public string FacebookAppSecret {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("FacebookAppSecret")) {
                return SettingsTable["FacebookAppSecret"];
            }

            return string.Empty;
        }
    }

    public string MetaTagDescription {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("MetaTagDescription")) {
                return SettingsTable["MetaTagDescription"];
            }

            return string.Empty;
        }
    }

    public string MetaTagKeywords {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("MetaTagKeywords")) {
                return SettingsTable["MetaTagKeywords"];
            }

            return string.Empty;
        }
    }

    public bool AutoFixDBIssues {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AutoFixDBIssues")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["AutoFixDBIssues"]);
            }

            return false;
        }
    }

    public double ServerTimezone {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("ServerTimezone")) {
                double timezone = 0.0d;
                double.TryParse(SettingsTable["ServerTimezone"], out timezone);
                return timezone;
            }

            return 0.0d;
        }
    }

    public static DateTime ServerDateTime {
        get {
            if (SettingsTable.ContainsKey("ServerTimezone")) {
                double timezone = 0.0d;
                double.TryParse(SettingsTable["ServerTimezone"], out timezone);
                timezone = GetTimezoneDaylightSavingsOffset(timezone);

                DateTime serverDatetime = DateTime.Now.ToUniversalTime().AddHours(timezone);
                return serverDatetime;
            }

            return DateTime.Now;
        }
    }
    public static double GetTimezoneDaylightSavingsOffset(double timezone) {
        if (DateTime.Now.IsDaylightSavingTime()) {
            if (timezone == -5.0d || timezone == -6.0d || timezone == -7.0d || timezone == -8.0d) {
                timezone = timezone + 1.0d;
            }
        }

        return timezone;
    }

    public bool LockAppCreator {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LockAppCreator")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["LockAppCreator"]);
            }

            return false;
        }
    }

    public bool LockBackgroundServices {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LockBackgroundServices")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["LockBackgroundServices"]);
            }

            return false;
        }
    }

    public bool MonitorCpuUsage {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("MonitorCpuUsage")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["MonitorCpuUsage"]);
            }

            return false;
        }
    }
    public double MonitorCpuUsagePercentAlert {
        get {
            double value = 75.0d;
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("MonitorCpuUsagePercentAlert")) {
                string monitorCpuUsagePercentAlert = SettingsTable["MonitorCpuUsagePercentAlert"];
                if (string.IsNullOrEmpty(monitorCpuUsagePercentAlert)) {
                    monitorCpuUsagePercentAlert = "75";
                }

                monitorCpuUsagePercentAlert = monitorCpuUsagePercentAlert.Replace("%", string.Empty);
                double.TryParse(monitorCpuUsagePercentAlert, out value);
            }

            return value;
        }
    }
    public long MonitorMemoryUsageAlert {
        get {
            long value = 150000;
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("MonitorMemoryUsageAlert")) {
                string monitorMemoryUsageAlert = SettingsTable["MonitorMemoryUsageAlert"];
                if (string.IsNullOrEmpty(monitorMemoryUsageAlert)) {
                    monitorMemoryUsageAlert = "150000";
                }

                long.TryParse(monitorMemoryUsageAlert, out value);
            }

            return value;
        }
    }

    #endregion


    #region Update Methods

    public static bool AdminPagesCheck(string page, string username) {
        bool cancontinue = false;

        if (Roles.IsUserInRole(username, ServerSettings.AdminUserName)) {
            cancontinue = true;
        }
        else {
            page = page.ToLower().Replace("_aspx", "").Replace("asp.", "").Replace(".aspx", "");
            page = page.Substring(page.LastIndexOf('_') + 1);
            var member = new MemberDatabase(username);
            if (member.AdminPagesList.Any(p => page == p.ToLower())) {
                cancontinue = true;
            }
        }

        return cancontinue;
    }

    private static bool CheckIfCanUpdateServerSettings(bool checkAdminPages = false) {
        if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated) {
            if (checkAdminPages) {
                return AdminPagesCheck("sitesettings", HttpContext.Current.User.Identity.Name);
            }

            return Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName);
        }

        return false;
    }

    private static bool CheckIfUserIsAdminForUpdate() {
        if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated) {
            return BasePage.IsUserNameEqualToAdmin(HttpContext.Current.User.Identity.Name);
        }

        return false;
    }

    public static void UpdateLoginMessage(string message, string date) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        bool canUpdateDate = dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LoginMessage", message) }, QueryList);
        if (canUpdateDate) {
            dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LoginMessageDate", date) }, QueryList);
        }
        UpdateServerSettingsTable();
    }

    public static List<string> AdminPages() {
        var returnList = new List<string>();
        SiteMapNodeCollection siteNodes = SiteMap.RootNode.ChildNodes;
        AdminPages(siteNodes, ref returnList);
        return returnList;
    }
    private static void AdminPages(SiteMapNodeCollection siteNodes, ref List<string> returnList) {
        foreach (SiteMapNode node in siteNodes) {
            try {
                bool adminOnly = false;
                foreach (var role in node.Roles) {
                    if (role.ToString().ToLower() == ServerSettings.AdminUserName.ToLower()) {
                        adminOnly = true;
                        break;
                    }
                }

                if ((adminOnly) && (!Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName))) {
                    continue;
                }

                if (!node.Url.Contains("iframecontent=")) {
                    string url = node.Url.Substring(node.Url.LastIndexOf('/') + 1);
                    string[] urlSplit = url.Split('.');
                    string filename = string.Empty;
                    if (urlSplit.Length > 0) {
                        filename = urlSplit[0];
                    }

                    if (!returnList.Contains(filename)) {
                        returnList.Add(filename);
                    }

                    if (node.HasChildNodes) {
                        AdminPages(node.ChildNodes, ref returnList);
                    }
                }
            }
            catch { }
        }
    }

    public static string[] GetAdminPageLink(string page) {
        var returnList = new List<string>();
        SiteMapNodeCollection siteNodes = SiteMap.RootNode.ChildNodes;
        GetAdminPage(page, siteNodes, ref returnList);
        return returnList.ToArray();
    }
    private static void GetAdminPage(string page, SiteMapNodeCollection siteNodes, ref List<string> returnList) {
        foreach (SiteMapNode node in siteNodes) {
            try {
                bool adminOnly = false;
                foreach (var role in node.Roles) {
                    if (role.ToString().ToLower() == ServerSettings.AdminUserName.ToLower()) {
                        adminOnly = true;
                        break;
                    }
                }

                if ((adminOnly) && (!Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, ServerSettings.AdminUserName))) {
                    continue;
                }

                string url = node.Url.Substring(node.Url.LastIndexOf('/') + 1);
                string[] urlSplit = url.Split('.');
                string filename = string.Empty;
                if (urlSplit.Length > 0) {
                    filename = urlSplit[0];
                }

                if (filename == page) {
                    if (!returnList.Contains(node.Url)) {
                        returnList.Add(node.Url);
                    }

                    if (!returnList.Contains(node.Title)) {
                        returnList.Add(node.Title);
                    }

                    break;
                }
            }
            catch { }
        }
    }

    public static void update_AllowAppRating(bool allow) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (allow) {
            temp = 1;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AllowAppRating", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SaveCookiesAsSessions(bool allow) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SaveCookiesAsSessions", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DeleteOldLoginActivity(bool allow) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DeleteOldLoginActivity", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void Update_LoginActivityToKeepInDays(int daysToKeep) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        if (daysToKeep <= 0) {
            daysToKeep = 1;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LoginActivityToKeepInDays", daysToKeep.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SignUpConfirmationEmail(bool sendEmail) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (sendEmail) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignUpConfirmationEmail", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterAccessToken(string TwitterAccessToken) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterAccessToken", TwitterAccessToken) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterAccessTokenSecret(string TwitterAccessTokenSecret) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterAccessTokenSecret", TwitterAccessTokenSecret) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterConsumerKey(string TwitterConsumerKey) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterConsumerKey", TwitterConsumerKey) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterConsumerSecret(string TwitterConsumerSecret) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterConsumerSecret", TwitterConsumerSecret) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void Update_ShowUpdatesPopupDate(string date) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ShowUpdatesPopupDate", date) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AppInstallerPackage(string package) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AppInstallerPackage", package) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordLoginActivity(bool record) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (record) {
            temp = 1;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordLoginActivity", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TotalWorkspacesAllowed(int count) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TotalWorkspacesAllowed", count.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AssociateWithGroups(bool enabled) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (enabled) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AssociateWithGroups", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_EmailOnRegister(bool email) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (email) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("EmailOnRegister", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_NoLoginRequired(bool needLogin) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (needLogin) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("NoLoginRequired", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_ForceGroupLogin(bool enable) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (enable) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ForceGroupLogin", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_ShowPreviewButtonLogin(bool show) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (show) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ShowPreviewButtonLogin", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordActivity(bool record) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (record) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordActivity", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordActivityToLogFile(bool saveFile) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (saveFile) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordActivityToLogFile", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordErrorsOnly(bool errorsOnly) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (errorsOnly) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordErrorsOnly", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AllowPrivacy(bool allow) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AllowPrivacy", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_EmailSystemStatus(bool isOn) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        int temp = 0;
        if (isOn) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("EmailSystemStatus", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SitePluginsLocked(bool locked) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SitePluginsLocked", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_OverlaysLocked(bool locked) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("OverlaysLocked", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_CacheHomePage(bool cache) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        int temp = 0;
        if (cache) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("CacheHomePage", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SSLRedirect(bool redirect) {
        if (!CheckIfUserIsAdminForUpdate ()) {
            return;
        }

        int temp = 0;
        if (redirect) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SSLRedirect", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_URLValidation(bool validate) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (validate) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("URLValidation", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockFileManager(bool _lock) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (_lock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockFileManager", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockStartupScripts(bool _lock) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (_lock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockStartupScripts", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockIPListenerWatchlist(bool _lock) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (_lock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockIPListenerWatchlist", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockCustomTables(bool locked) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockCustomTables", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockAppCreator(bool locked) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockAppCreator", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockBackgroundServices(bool locked) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockBackgroundServices", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_EmailActivity(bool record) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (record) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("EmailActivity", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AutoBlockIP(bool autoblock) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (autoblock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AutoBlockIP", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AppendTimestampOnScripts(bool appendTimestamp) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (appendTimestamp) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AppendTimestampOnScripts", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SiteOffLine(bool offline) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (offline) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SiteOffLine", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_ChatEnabled(bool chat) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (chat) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ChatEnabled", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_CustomizationsLocked(bool locked) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("CustomizationsLocked", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AutoBlockIPCount(int count) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AutoBlockIPCount", count.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SiteName(string name) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SiteName", name) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AdminNote(string note) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AdminNote", note) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AdminNoteBy(string username) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AdminNoteBy", username) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AllowUserSignUp(bool allow) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AllowUserSignUp", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_ShowLoginModalOnDemoMode(bool showLoginModal) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (showLoginModal) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ShowLoginModalOnDemoMode", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordPageViews(bool record) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (record) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordPageViews", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordSiteRequests(bool record) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (record) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordSiteRequests", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_MaxRequestRecordSize(int count) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("MaxRequestRecordSize", count.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DisableJavascriptErrorAlerts(bool disable) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (disable) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DisableJavascriptErrorAlerts", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DefaultBodyFontFamily(string fontFamily) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DefaultBodyFontFamily", fontFamily) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DefaultBodyFontSize(string fontSize) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DefaultBodyFontSize", fontSize) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DefaultBodyFontColor(string fontColor) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DefaultBodyFontColor", fontColor) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_UserSignUpRole(string role) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("UserSignUpRole", role) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static string update_LastCacheClearDate() {
        string date = ServerSettings.ServerDateTime.ToString();
        if (!CheckIfCanUpdateServerSettings()) {
            return date;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LastCacheDate", date) }, QueryList);
        UpdateServerSettingsTable();
        return date;
    }

    public static void update_WebEventsToKeep(int number) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("WebEventsToKeep", number.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SignInWithGoogle(bool allow) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignInWithGoogle", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_GoogleClientId(string clientId) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("GoogleClientId", clientId) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_GoogleClientSecret(string clientSecret) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("GoogleClientSecret", clientSecret) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SignInWithTwitter(bool allow) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignInWithTwitter", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SignInWithFacebook(bool allow) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignInWithFacebook", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_FacebookAppId(string appId) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("FacebookAppId", appId) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_FacebookAppSecret(string appSecret) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("FacebookAppSecret", appSecret) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_MetaTagDescription(string description) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("MetaTagDescription", description) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_MetaTagKeywords(string keywords) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("MetaTagKeywords", keywords) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AutoFixDBIssues(bool autoFix) {
        if (!CheckIfCanUpdateServerSettings()) {
            return;
        }

        int temp = 0;
        if (autoFix) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AutoFixDBIssues", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    private static void UpdateServerSettingsTable() {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", QueryList);
        if (dbSelect.Count > 0) {
            SettingsTable = dbSelect[0];
        }
    }

    public static void update_ServerTimezone(string timezone) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ServerTimezone", timezone) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_GoogleMapsApiKey(string apiKey) {
        if (!CheckIfUserIsAdminForUpdate()) {
            return;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("GoogleMapsApiKey", apiKey) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_MonitorCpuUsage(bool monitor) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        int temp = 0;
        if (monitor) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("MonitorCpuUsage", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();

        if (monitor) {
            StartCpuUsageMonitor();
        }
        else {
            int totalLoops = 0;
            while (_cpuUsageThreadIsRunning) {
                if (totalLoops > 30) {
                    break;
                }

                Thread.Sleep(1000);
                totalLoops++;
            }
        }
    }
    public static void update_MonitorCpuUsagePercentAlert(double cpuUsage) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        if (cpuUsage <= 0.0f || cpuUsage > 100.0f) {
            cpuUsage = 75.0f;
        }

        string newVal = string.Format("{0:N0}", cpuUsage);
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("MonitorCpuUsagePercentAlert", newVal) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_MonitorMemoryUsageAlert(long memoryUsage) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        if (memoryUsage <= 0.0f) {
            memoryUsage = 150000;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("MonitorMemoryUsageAlert", memoryUsage.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    #endregion


    #region User Registration

    public static void update_UserSignUpEmailChecker(bool checker) {
        int temp = 0;
        if (checker) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("UserSignUpEmailChecker", temp.ToString()) }, QueryList);
    }
    public static void update_UserSignUpEmailAssociation(string email) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("UserSignUpEmailAssociation", email) }, QueryList);
    }

    #endregion


    #region Mail Settings

    private const string MailSettingsTableName = "aspnet_MailSettings";
    private static List<MailSettings> _coll = new List<MailSettings>();

    public static List<MailSettings> MailSettings_Coll {
        get { return _coll; }
    }
    public static void GetMailSettingList() {
        _coll = new List<MailSettings>();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(MailSettingsTableName, "", QueryList);

        foreach (Dictionary<string, string> entry in dbSelect) {
            string id = entry["ID"];
            string username = entry["Username"];
            string smtpAddress = entry["SMTP_Address"];
            string portNumber = entry["PortNumber"];
            bool ssl = HelperMethods.ConvertBitToBoolean(entry["SSL"]);
            string emailAddress = entry["EmailAddress"];
            string password = StringEncryption.Decrypt(entry["Password"], "@" + id.Replace("-", "").Substring(0, 15));
            string headerString = entry["HeaderString"];
            string footerString = entry["FooterString"];
            string date = entry["Date"];

            MailSettings coll = new MailSettings(id, username, smtpAddress, portNumber, ssl, emailAddress, password, headerString, footerString, date);
            _coll.Add(coll);
        }
    }

    public static void UpdateMailSettings(string username, string address, string portnumber, string email, string password, bool ssl) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        GetMailSettingList();
        string headerString = string.Empty;
        string footerString = string.Empty;

        if (_coll.Count == 1) {
            headerString = _coll[0].Header;
            footerString = _coll[0].Footer;
        }

        dbCall.CallDelete(MailSettingsTableName, QueryList);

        string _ssl = "0";
        if (ssl) {
            _ssl = "1";
        }

        string id = Guid.NewGuid().ToString();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Username", username));
        query.Add(new DatabaseQuery("SMTP_Address", address));
        query.Add(new DatabaseQuery("PortNumber", portnumber));
        query.Add(new DatabaseQuery("SSL", _ssl));
        query.Add(new DatabaseQuery("EmailAddress", email));
        query.Add(new DatabaseQuery("Password", StringEncryption.Encrypt(password, "@" + id.Replace("-", "").Substring(0, 15))));
        query.Add(new DatabaseQuery("HeaderString", headerString));
        query.Add(new DatabaseQuery("FooterString", footerString));
        query.Add(new DatabaseQuery("Date", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert(MailSettingsTableName, query);
    }

    public static void UpdateHeaderStringMailSettings(string headerString) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        GetMailSettingList();
        string id = Guid.NewGuid().ToString();
        string username = string.Empty;
        string address = string.Empty;
        string portnumber = string.Empty;
        bool ssl = false;
        string email = "";
        string password = "";
        string footerString = string.Empty;
        string date = ServerSettings.ServerDateTime.ToString();

        if (_coll.Count == 1) {
            id = _coll[0].ID;
            username = _coll[0].User;
            address = _coll[0].SMTP_Address;
            portnumber = _coll[0].PortNumber;
            ssl = _coll[0].SSL;
            email = _coll[0].EmailAddress;
            password = _coll[0].Password;
            footerString = _coll[0].Footer;
            date = _coll[0].Date;
        }

        dbCall.CallDelete(MailSettingsTableName, QueryList);

        string _ssl = "0";
        if (ssl) {
            _ssl = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Username", username));
        query.Add(new DatabaseQuery("SMTP_Address", address));
        query.Add(new DatabaseQuery("PortNumber", portnumber));
        query.Add(new DatabaseQuery("SSL", _ssl));
        query.Add(new DatabaseQuery("EmailAddress", email));
        if (!string.IsNullOrEmpty(password)) {
            query.Add(new DatabaseQuery("Password", StringEncryption.Encrypt(password, "@" + id.Replace("-", "").Substring(0, 15))));
        }
        else {
            query.Add(new DatabaseQuery("Password", password));
        }
        query.Add(new DatabaseQuery("HeaderString", headerString));
        query.Add(new DatabaseQuery("FooterString", footerString));
        query.Add(new DatabaseQuery("Date", date));

        dbCall.CallInsert(MailSettingsTableName, query);
    }
    public static void UpdateFooterStringMailSettings(string footerString) {
        if (!CheckIfCanUpdateServerSettings(true)) {
            return;
        }

        GetMailSettingList();
        string id = Guid.NewGuid().ToString();
        string username = string.Empty;
        string address = string.Empty;
        string portnumber = string.Empty;
        bool ssl = false;
        string email = "";
        string password = "";
        string headerString = string.Empty;
        string date = ServerSettings.ServerDateTime.ToString();

        if (_coll.Count == 1) {
            id = _coll[0].ID;
            username = _coll[0].User;
            address = _coll[0].SMTP_Address;
            portnumber = _coll[0].PortNumber;
            ssl = _coll[0].SSL;
            email = _coll[0].EmailAddress;
            password = _coll[0].Password;
            headerString = _coll[0].Header;
            date = _coll[0].Date;
        }

        dbCall.CallDelete(MailSettingsTableName, QueryList);

        string _ssl = "0";
        if (ssl) {
            _ssl = "1";
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Username", username));
        query.Add(new DatabaseQuery("SMTP_Address", address));
        query.Add(new DatabaseQuery("PortNumber", portnumber));
        query.Add(new DatabaseQuery("SSL", _ssl));
        query.Add(new DatabaseQuery("EmailAddress", email));
        if (!string.IsNullOrEmpty(password)) {
            query.Add(new DatabaseQuery("Password", StringEncryption.Encrypt(password, "@" + id.Replace("-", "").Substring(0, 15))));
        }
        else {
            query.Add(new DatabaseQuery("Password", password));
        }
        query.Add(new DatabaseQuery("HeaderString", headerString));
        query.Add(new DatabaseQuery("FooterString", footerString));
        query.Add(new DatabaseQuery("Date", date));

        dbCall.CallInsert(MailSettingsTableName, query);
    }

    #endregion

}

[Serializable]
public class MailSettings {
    private string _address;
    private string _date;
    private string _email;
    private string _id;
    private string _password;
    private string _port;
    private bool _ssl;
    private string _user;
    private string _header;
    private string _footer;

    public MailSettings(string id, string user, string address, string port, bool ssl, string email, string password, string header, string footer, string date) {
        _id = id;
        _user = user;
        _address = address;
        _port = port;
        _ssl = ssl;
        _email = email;
        _password = password;
        _header = header;
        _footer = footer;
        _date = date;
    }

    public string ID {
        get { return _id; }
        set { _id = value; }
    }

    public string User {
        get { return _user; }
        set { _user = value; }
    }

    public string SMTP_Address {
        get { return _address; }
        set { _address = value; }
    }

    public string PortNumber {
        get { return _port; }
        set { _port = value; }
    }

    public bool SSL {
        get { return _ssl; }
        set { _ssl = value; }
    }

    public string EmailAddress {
        get { return _email; }
        set { _email = value; }
    }

    public string Password {
        get { return _password; }
        set { _password = value; }
    }

    public string Header {
        get { return _header; }
        set { _header = value; }
    }

    public string Footer {
        get { return _footer; }
        set { _footer = value; }
    }

    public string Date {
        get { return _date; }
        set { _date = value; }
    }
}