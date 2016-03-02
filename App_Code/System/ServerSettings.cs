#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
using HtmlAgilityPack;
using System.Xml;
using OpenWSE_Tools.GroupOrganizer;
using System.Data.SqlServerCe;
using OpenWSE.Core.Licensing;
using OpenWSE.Core;
using System.Web.Profile;
using System.Web.Providers;
using OpenWSE_Library.Core.BackgroundServices;
using OpenWSE_Tools.AppServices;
using OpenWSE_Tools.BackgroundServiceDatabaseCalls;

#endregion

[Serializable]
public class ServerSettings {

    private static DatabaseCall dbCall = new DatabaseCall();
    private static Dictionary<string, string> SettingsTable = new Dictionary<string, string>();
    private const string dbTable = "aspnet_ServerSettings";

    public const string RobotsMetaTag = "INDEX, FOLLOW";

    public const string AdminUserName = "Administrator";
    public const string OverrideMobileSessionString = "OverrideMobile";

    public const string BackupFileExt = ".backup";
    public const string SavedDataFilesExt = ".data";

    // Changing this will also change the value on the javascript page
    public const string TimestampQuery = "vertimestamp=";

    public static string ApplicationID {
        get {
            string licenseId = WebConfigurationManager.AppSettings["ApplicationId"];

            if (!string.IsNullOrEmpty(licenseId)) {
                if (Membership.ApplicationName != licenseId) {
                    Membership.ApplicationName = licenseId;
                }

                if (ProfileManager.ApplicationName != licenseId) {
                    ProfileManager.ApplicationName = licenseId;
                }

                if (Roles.ApplicationName != licenseId) {
                    Roles.ApplicationName = licenseId;
                }
            }

            return licenseId;
        }
    }


    #region Delimiters
    public const string StringDelimiter = ";";
    public static string[] StringDelimiter_Array = { ";" };
    #endregion

    /// <summary> Start the Background Services on Application Load
    /// </summary>
    private static void StartBackgroundServices() {
        string currUsername = ServerSettings.AdminUserName;
        if (HttpContext.Current != null && HttpContext.Current.User != null 
            && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated) {
                currUsername = HttpContext.Current.User.Identity.Name;
        }

        BackgroundServices backgroundServices = new BackgroundServices();
        List<BackgroundServices_Coll> serviceColl = backgroundServices.GetBackgroundServicesWithDefaultStates();
        foreach (BackgroundServices_Coll service in serviceColl) {
            backgroundServices.UpdateState(service.ID, service.Namespace, service.State, service.LogInformation, currUsername);
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
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));

            return query;
        }
    }

    public ServerSettings() {
        CheckAndBuildServerSettingsTable();
    }
    private void CheckAndBuildServerSettingsTable() {
        if (SettingsTable.Count == 0) {
            if (dbCall.ConnectionString == null) {
                return;
            }

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", QueryList);

            if (dbSelect.Count > 0) {
                SettingsTable = dbSelect[0];
            }
            else {
                DefaultDBTables.InsertDefaultDataIntoTable(dbTable, true);
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

    public static void StartServerApplication() {
        try {
            if (CheckWebConfigFile()) {
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
                CreateDefaultRoles();
                CreateAdminUser();
                StartBackgroundServices();
            }
            else {
                HttpContext.Current.Response.Redirect("~/DatabaseSelection.aspx");
            }
        }
        catch { }
    }
    public static void SetupApplicationId() {
        DatabaseCall dbCall = new DatabaseCall();
        string tableName = "Applications";
        if (dbCall.DataProvider == "System.Data.SqlClient") {
            tableName = "aspnet_Applications";
        }

        List<Dictionary<string, string>> dbSelectAppId = dbCall.CallSelect(tableName, "", QueryList);

        if (dbSelectAppId.Count == 0) {
            string licenseId = ServerSettings.ApplicationID;
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", licenseId));
            query.Add(new DatabaseQuery("ApplicationName", licenseId));
            if (dbCall.DataProvider == "System.Data.SqlClient") {
                query.Add(new DatabaseQuery("LoweredApplicationName", licenseId.ToLower()));
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
            memberDatabase.UpdateAutoHideMode(false);
        }
    }
    public static bool CheckWebConfigFile() {
        string path = HttpContext.Current.Server.MapPath("~/Web.Config");
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        XmlNode node = doc.DocumentElement.SelectSingleNode("connectionStrings");
        if (node == null) {
            return false;
        }

        return true;
    }
    public static bool NeedToLoadAdminNewMemberPage {
        get {
            MembershipUserCollection userColl = Membership.GetAllUsers();
            if (userColl.Count == 1) {
                foreach (MembershipUser u in userColl) {
                    if (u.UserName.ToLower() == AdminUserName.ToLower() && new MemberDatabase(AdminUserName).IsNewMember) {
                        return true;
                    }
                }
            }
            return false;
        }
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

            if (!string.IsNullOrEmpty(ServerSettings.RobotsMetaTag)) {
                HtmlMeta meta = new HtmlMeta();
                meta.Name = "robots";
                meta.Content = ServerSettings.RobotsMetaTag;
                page.Header.Controls.Add(meta);
            }
        }
    }
    private static void SetPageTitle(Page page) {
        if (!string.IsNullOrEmpty(page.Title) && !string.IsNullOrEmpty(CheckLicense.SiteName)) {
            page.Title = CheckLicense.SiteName + " - " + page.Title;
        }
        else if (!string.IsNullOrEmpty(page.Title)) {
            page.Title = page.Title;
        }
        else {
            page.Title = CheckLicense.SiteName;
        }
    }

    public void AutoUpdateDatabaseCheck() {
        if (CheckWebConfigFile() && AutoFixDBIssues) {
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
    public static void PageToolViewRedirect(Page page, string url) {
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

                if (HelperMethods.ConvertBitToBoolean(page.Request.QueryString["toolView"])) {
                    if (!string.IsNullOrEmpty(query)) {
                        query = "toolView=true&" + query;
                    }
                    else {
                        query = "toolView=true";
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

                if (!string.IsNullOrEmpty(query)) {
                    query = "?" + query;
                }

                page.Response.Redirect(mainRedirectPath + query);
            }
            else {
                page.Response.Redirect("~/ErrorPages/Error.html");
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

            page.Response.Redirect(url + query);
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
                if (column == "ApplicationId") {
                    query.Add(new DatabaseQuery(column, ServerSettings.ApplicationID));
                }
                else {
                    query.Add(new DatabaseQuery(column, null));
                }
            }

            dbCall.CallInsert(dbTable, query);
        }
    }

    public string DocumentsFolder {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("DocumentsFolder")) {
                return SettingsTable["DocumentsFolder"];
            }

            return string.Empty;
        }
    }
    public string ResolvedDocumentFolder {
        get {
            string temp = DocumentsFolder;
            if (temp.Length > 0 && temp.StartsWith("~/")) {
                temp = ServerSettings.GetServerMapLocation + temp.Replace("~/", "");
            }
            else if (string.IsNullOrEmpty(temp)) {
                temp = ServerSettings.GetServerMapLocation + "CloudFiles";
            }

            return temp;
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

    public bool HideAllAppIcons {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("HideAllAppIcons")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["HideAllAppIcons"]);
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

    public string GetSidebarCategoryIcon(string category) {
        CheckAndBuildServerSettingsTable();
        if (SettingsTable.ContainsKey("SidebarToolCategoryIcons")) {
            string[] iconList = SettingsTable["SidebarToolCategoryIcons"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

            foreach (string val in iconList) {
                string[] splitVal = val.Split('=');
                if (splitVal.Length == 2 && splitVal[0].Replace("_", " ") == category) {
                    return splitVal[1];
                }
            }
        }

        return string.Empty;
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

                    if (HttpContext.Current.Session != null) {
                        if (HttpContext.Current.Session["DemoMode"] != null) {
                            if (HelperMethods.ConvertBitToBoolean(HttpContext.Current.Session["DemoMode"].ToString()))
                                return true;
                        }
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

    public bool NotificationsLocked {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("NotificationsLocked")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["NotificationsLocked"]);
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

    public string LogoBackgroundColor_NonTranslated {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LogoBackgroundColor")) {
                string value = SettingsTable["LogoBackgroundColor"];
                if (!string.IsNullOrEmpty(value)) {
                    return value.Replace("#", string.Empty);
                }
            }

            return "000000";
        }
    }

    public string LogoBackgroundColor {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LogoBackgroundColor")) {
                string value = SettingsTable["LogoBackgroundColor"];
                if (!string.IsNullOrEmpty(value)) {
                    if (!value.StartsWith("#")) {
                        value = "#" + value;
                    }

                    try {
                        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(value);
                        return color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString();
                    }
                    catch { }
                }
            }

            return "0,0,0";
        }
    }

    public string LogoOpacity {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("LogoOpacity")) {
                string value = SettingsTable["LogoOpacity"];
                if (!string.IsNullOrEmpty(value)) {
                    return value;
                }
            }

            return "1.0";
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
                string defaultPage = WebConfigurationManager.AppSettings["DefaultDocument"];
                if (string.IsNullOrEmpty(defaultPage))
                    return "Default.aspx";

                return defaultPage;
            }
            catch { }

            return "Default.aspx";
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

    public bool SignInWithGoogle {
        get {
            CheckAndBuildServerSettingsTable();
            if (!string.IsNullOrEmpty(GoogleClientId) && !string.IsNullOrEmpty(GoogleClientSecret) && SettingsTable.ContainsKey("SignInWithGoogle")) {
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
            if (!string.IsNullOrEmpty(TwitterConsumerKey) && !string.IsNullOrEmpty(TwitterConsumerSecret) && SettingsTable.ContainsKey("SignInWithTwitter")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SignInWithTwitter"]);
            }

            return false;
        }
    }

    public bool SignInWithFacebook {
        get {
            CheckAndBuildServerSettingsTable();
            if (!string.IsNullOrEmpty(FacebookAppId) && !string.IsNullOrEmpty(FacebookAppSecret) && SettingsTable.ContainsKey("SignInWithFacebook")) {
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

    public bool AddBackgroundToLogo {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AddBackgroundToLogo")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["AddBackgroundToLogo"]);
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

    public static void UpdateLoginMessage(string message, string date) {
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
        int temp = 0;
        if (allow) {
            temp = 1;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AllowAppRating", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SaveCookiesAsSessions(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SaveCookiesAsSessions", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DeleteOldLoginActivity(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DeleteOldLoginActivity", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void Update_LoginActivityToKeepInDays(int daysToKeep) {
        if (daysToKeep <= 0) {
            daysToKeep = 1;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LoginActivityToKeepInDays", daysToKeep.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SignUpConfirmationEmail(bool sendEmail) {
        int temp = 0;
        if (sendEmail) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignUpConfirmationEmail", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterAccessToken(string TwitterAccessToken) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterAccessToken", TwitterAccessToken) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterAccessTokenSecret(string TwitterAccessTokenSecret) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterAccessTokenSecret", TwitterAccessTokenSecret) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterConsumerKey(string TwitterConsumerKey) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterConsumerKey", TwitterConsumerKey) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterConsumerSecret(string TwitterConsumerSecret) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterConsumerSecret", TwitterConsumerSecret) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SidebarToolCategoryIcons(string category, string value) {
        if (SettingsTable.ContainsKey("SidebarToolCategoryIcons")) {
            string newList = string.Empty;
            string[] iconList = SettingsTable["SidebarToolCategoryIcons"].Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

            bool found = false;
            foreach (string val in iconList) {
                string[] splitVal = val.Split('=');
                if (splitVal.Length == 2 && splitVal[0] == category) {
                    newList += splitVal[0] + "=" + value + ServerSettings.StringDelimiter;
                    found = true;
                }
                else {
                    newList += val + ServerSettings.StringDelimiter;
                }
            }

            if (!found) {
                newList += category + "=" + value + ServerSettings.StringDelimiter;
            }

            dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SidebarToolCategoryIcons", newList) }, QueryList);
            UpdateServerSettingsTable();
        }
    }

    public static void Update_LogoOpacity(string opacity) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LogoOpacity", opacity) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void Update_ShowUpdatesPopupDate(string date) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ShowUpdatesPopupDate", date) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_docFolder(string path) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DocumentsFolder", path) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AppInstallerPackage(string package) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AppInstallerPackage", package) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordLoginActivity(bool record) {
        int temp = 0;
        if (record) {
            temp = 1;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordLoginActivity", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_TotalWorkspacesAllowed(int count) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TotalWorkspacesAllowed", count.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AssociateWithGroups(bool enabled) {
        int temp = 0;
        if (enabled) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AssociateWithGroups", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_EmailOnRegister(bool email) {
        int temp = 0;
        if (email) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("EmailOnRegister", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_NoLoginRequired(bool needLogin) {
        int temp = 0;
        if (needLogin) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("NoLoginRequired", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_HideAllAppIcons(bool hideAll) {
        int temp = 0;
        if (hideAll) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("HideAllAppIcons", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_ForceGroupLogin(bool enable) {
        int temp = 0;
        if (enable) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ForceGroupLogin", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_ShowPreviewButtonLogin(bool show) {
        int temp = 0;
        if (show) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ShowPreviewButtonLogin", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordActivity(bool record) {
        int temp = 0;
        if (record) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordActivity", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordActivityToLogFile(bool saveFile) {
        int temp = 0;
        if (saveFile) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordActivityToLogFile", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_RecordErrorsOnly(bool errorsOnly) {
        int temp = 0;
        if (errorsOnly) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordErrorsOnly", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AllowPrivacy(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AllowPrivacy", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_EmailSystemStatus(bool isOn) {
        int temp = 0;
        if (isOn) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("EmailSystemStatus", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SitePluginsLocked(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SitePluginsLocked", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_NotificationsLocked(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("NotificationsLocked", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_OverlaysLocked(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("OverlaysLocked", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_CacheHomePage(bool cache) {
        int temp = 0;
        if (cache) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("CacheHomePage", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AddBackgroundToLogo(bool add) {
        int temp = 0;
        if (add) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AddBackgroundToLogo", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SSLRedirect(bool redirect) {
        int temp = 0;
        if (redirect) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SSLRedirect", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_URLValidation(bool validate) {
        int temp = 0;
        if (validate) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("URLValidation", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockFileManager(bool _lock) {
        int temp = 0;
        if (_lock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockFileManager", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockStartupScripts(bool _lock) {
        int temp = 0;
        if (_lock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockStartupScripts", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockIPListenerWatchlist(bool _lock) {
        int temp = 0;
        if (_lock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockIPListenerWatchlist", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockCustomTables(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockCustomTables", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_EmailActivity(bool record) {
        int temp = 0;
        if (record) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("EmailActivity", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AutoBlockIP(bool autoblock) {
        int temp = 0;
        if (autoblock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AutoBlockIP", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AppendTimestampOnScripts(bool appendTimestamp) {
        int temp = 0;
        if (appendTimestamp) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AppendTimestampOnScripts", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SiteOffLine(bool offline) {
        int temp = 0;
        if (offline) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SiteOffLine", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_ChatEnabled(bool chat) {
        int temp = 0;
        if (chat) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ChatEnabled", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_CustomizationsLocked(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("CustomizationsLocked", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AutoBlockIPCount(int count) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AutoBlockIPCount", count.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AdminNote(string note) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AdminNote", note) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AdminNoteBy(string username) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AdminNoteBy", username) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AllowUserSignUp(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AllowUserSignUp", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_ShowLoginModalOnDemoMode(bool showLoginModal) {
        int temp = 0;
        if (showLoginModal) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ShowLoginModalOnDemoMode", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DisableJavascriptErrorAlerts(bool disable) {
        int temp = 0;
        if (disable) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DisableJavascriptErrorAlerts", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DefaultBodyFontFamily(string fontFamily) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DefaultBodyFontFamily", fontFamily) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DefaultBodyFontSize(string fontSize) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DefaultBodyFontSize", fontSize) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_DefaultBodyFontColor(string fontColor) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DefaultBodyFontColor", fontColor) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_UserSignUpRole(string role) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("UserSignUpRole", role) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LogoBackgroundColor(string color) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LogoBackgroundColor", color) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static string update_LastCacheClearDate() {
        string date = ServerSettings.ServerDateTime.ToString();
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LastCacheDate", date) }, QueryList);
        UpdateServerSettingsTable();
        return date;
    }

    public static void update_WebEventsToKeep(int number) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("WebEventsToKeep", number.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SignInWithGoogle(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignInWithGoogle", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_GoogleClientId(string clientId) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("GoogleClientId", clientId) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_GoogleClientSecret(string clientSecret) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("GoogleClientSecret", clientSecret) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SignInWithTwitter(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignInWithTwitter", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_SignInWithFacebook(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignInWithFacebook", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_FacebookAppId(string appId) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("FacebookAppId", appId) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_FacebookAppSecret(string appSecret) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("FacebookAppSecret", appSecret) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_MetaTagDescription(string description) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("MetaTagDescription", description) }, QueryList);
        UpdateServerSettingsTable();
    }
    public static void update_MetaTagKeywords(string keywords) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("MetaTagKeywords", keywords) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_AutoFixDBIssues(bool autoFix) {
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
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ServerTimezone", timezone) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockAppCreator(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockAppCreator", temp.ToString()) }, QueryList);
        UpdateServerSettingsTable();
    }

    public static void update_LockBackgroundServices(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockBackgroundServices", temp.ToString()) }, QueryList);
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
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
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
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
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
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
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