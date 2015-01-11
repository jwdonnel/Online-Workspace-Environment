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
using AjaxControlToolkit;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Xml;
using OpenWSE_Tools.GroupOrganizer;
using System.Data.SqlServerCe;

#endregion

[Serializable]
public class ServerSettings {

    private static DatabaseCall dbCall = new DatabaseCall();
    private static Dictionary<string, string> SettingsTable = new Dictionary<string, string>();
    private const string dbTable = "aspnet_ServerSettings";

    public const string AdminUserName = "Administrator";
    public const string OverrideMobileSessionString = "OverrideMobile";

    public const string LicenseSite = "http://openwse.info/";

    public const string BackupFileExt = ".backup";
    public const string SavedDataFilesExt = ".data";

    #region Delimiters
    public const string StringDelimiter = ";";
    public static string[] StringDelimiter_Array = { ";" };
    #endregion


    private static string _siteName = string.Empty;

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

    public static string GetServerMapLocation {
        get {
            string serverLoc = HttpContext.Current.Server.MapPath("~");
            if (serverLoc[serverLoc.Length - 1] != '\\') {
                serverLoc += "\\";
            }

            return serverLoc;
        }
    }

    public const string DefaultConnectionStringName = "ApplicationServices";

    public ServerSettings() {
        CheckAndBuildServerSettingsTable();
    }
    private void CheckAndBuildServerSettingsTable() {
        if (SettingsTable.Count == 0) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", null);
            if (dbSelect.Count == 1) {
                SettingsTable = dbSelect[0];
            }
            else if (dbSelect.Count == 0) {
                #region Try to rebuild aspnet_ServerSettings Table
                try {
                    DataTable dt = dbCall.CallGetDataTable(dbTable);
                    if (dt == null) return;

                    List<DatabaseQuery> query = new List<DatabaseQuery>();
                    foreach (DataColumn column in dt.Columns) {
                        if (column.ColumnName == "ApplicationId") {
                            string appId = Guid.NewGuid().ToString();
                            if (dbCall.DataProvider == "System.Data.SqlClient") {
                                DatabaseQuery dbSelectAppId = dbCall.CallSelectSingle("aspnet_Applications", "ApplicationId", new List<DatabaseQuery>() { new DatabaseQuery("ApplicationName", "/") });
                                if (!string.IsNullOrEmpty(dbSelectAppId.Value)) {
                                    appId = dbSelectAppId.Value;
                                }
                            }
                            else if (dbCall.DataProvider == "System.Data.SqlServerCe.4.0") {
                                DatabaseQuery dbSelectAppId = dbCall.CallSelectSingle("Applications", "ApplicationId", new List<DatabaseQuery>() { new DatabaseQuery("ApplicationName", "/") });
                                if (!string.IsNullOrEmpty(dbSelectAppId.Value)) {
                                    appId = dbSelectAppId.Value;
                                }
                            }

                            query.Add(new DatabaseQuery(column.ColumnName, appId));
                        }
                        else {
                            query.Add(new DatabaseQuery(column.ColumnName, string.Empty));
                        }
                    }
                    dbCall.CallInsert(dbTable, query);
                }
                catch { }
                #endregion
            }
        }
    }

    public static void ResetServerSettings() {
        SettingsTable = new Dictionary<string, string>();
        SettingsTable.Clear();
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
               
                string[] roles = Roles.GetAllRoles();
                if (roles != null && roles.Length > 0) {
                    AutoBackupSystem abs = new AutoBackupSystem(ServerSettings.GetServerMapLocation);
                    ServerSettings ss = new ServerSettings();
                    if (ss.AutoBackupSystem) {
                        abs.StartBackupSystem();
                    }
                }

                // Build default tables if needed
                DefaultDBTables.BuildDefaults();

                CreateDefaultRoles();
                CreateAdminUser();
            }
            else {
                HttpContext.Current.Response.Redirect("~/DatabaseSelection.aspx");
            }
        }
        catch { }
    }
    private static void CreateDefaultRoles() {
        List<string> roles = Roles.GetAllRoles().ToList();
        List<string> defaultRoles = new List<string>();
        defaultRoles.Add(ServerSettings.AdminUserName);
        defaultRoles.Add("Standard");
        foreach (string r in defaultRoles) {
            if (!roles.Contains(r)) {
                Roles.CreateRole(r);
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
            else if (context.Session["LoginGroup"] != null) {
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
        string pageQuery = page.Request.QueryString["toolView"];
        if (string.IsNullOrEmpty(pageQuery)) {
            pageQuery = "false";
        }

        if (HelperMethods.ConvertBitToBoolean(pageQuery.ToLower())) {
            string toolView = "?toolView=true";
            if (url.Contains("?")) {
                toolView = "&toolView=true";
            }
            page.Response.Redirect(url + toolView);
        }

        page.Response.Redirect(url);
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
            MailSettingsDeserialize();
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
                    new AppLog(false).AddError(e);
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
        string createdBy = tableColl.CreatedBy;
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
    public static bool CheckCustomProjectsGroupAssociation(CustomProjects_Coll cpColl, MemberDatabase member) {
        string createdBy = cpColl.UploadedBy;
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

    public static void update_EmailHeader(string header) {
        string id = Guid.NewGuid().ToString();
        string user = HttpContext.Current.User.Identity.Name;
        string smtpServer = string.Empty;
        string port = string.Empty;
        bool ssl = false;
        string email = string.Empty;
        string password = string.Empty;
        string dateTime = DateTime.Now.ToString();
        string footer = string.Empty;

        MailSettingsDeserialize();
        if (MailSettings_Coll.Count > 0) {
            List<MailSettings> tempColl = MailSettings_Coll;
            id = tempColl[0].ID;
            smtpServer = tempColl[0].SMTP_Address;
            port = tempColl[0].PortNumber;
            ssl = tempColl[0].SSL;
            email = tempColl[0].EmailAddress;
            password = tempColl[0].Password;
            footer = tempColl[0].Footer;
        }

        var mailsettings = new MailSettings(id, user, smtpServer, port, ssl, email, password, header.Trim(), footer, dateTime);
        var coll = new List<MailSettings> { mailsettings };
        MailSettingsSerialize(coll);
    }

    public static void update_EmailFooter(string footer) {
        string id = Guid.NewGuid().ToString();
        string user = HttpContext.Current.User.Identity.Name;
        string smtpServer = string.Empty;
        string port = string.Empty;
        bool ssl = false;
        string email = string.Empty;
        string password = string.Empty;
        string dateTime = DateTime.Now.ToString();
        string header = string.Empty;

        MailSettingsDeserialize();
        if (MailSettings_Coll.Count > 0) {
            List<MailSettings> tempColl = MailSettings_Coll;
            id = tempColl[0].ID;
            smtpServer = tempColl[0].SMTP_Address;
            port = tempColl[0].PortNumber;
            ssl = tempColl[0].SSL;
            email = tempColl[0].EmailAddress;
            password = tempColl[0].Password;
            header = tempColl[0].Header;
        }

        var mailsettings = new MailSettings(id, user, smtpServer, port, ssl, email, password, header, footer.Trim(), dateTime);
        var coll = new List<MailSettings> { mailsettings };
        MailSettingsSerialize(coll);
    }

    public static string EmailHeader {
        get {
            string temp = string.Empty;

            try {
                MailSettingsDeserialize();
                if (MailSettings_Coll.Count > 0) {
                    temp = MailSettings_Coll[0].Header.Trim();
                }
            }
            catch { }

            return temp;
        }
    }

    public static string EmailFooter {
        get {
            string temp = string.Empty;

            try {
                MailSettingsDeserialize();
                if (MailSettings_Coll.Count > 0) {
                    temp = MailSettings_Coll[0].Footer.Trim();
                }
            }
            catch { }

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
            new AppLog(false).AddError(e);
        }
        return absolutePath;
    }

    private static void CheckAndCreateServerSettings() {
        DatabaseCall dbCall = new DatabaseCall();
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", null);
        if (dbSelect.Count == 0) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();

            List<string> columnList = DefaultDBTables.GetDatabaseTableDefault(dbTable, dbCall);
            foreach (string column in columnList) {
                if (column == "ApplicationId") {
                    query.Add(new DatabaseQuery(column, "/"));
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
            return SettingsTable["DocumentsFolder"];
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
            return HelperMethods.ConvertBitToBoolean(SettingsTable["SignUpConfirmationEmail"]);
        }
    }

    public string ShowUpdatesPopupDate {
        get {
            CheckAndBuildServerSettingsTable();
            string value = SettingsTable["ShowUpdatesPopupDate"];
            if (string.IsNullOrEmpty(value)) {
                return "N/A";
            }
            else {
                return value;
            }
        }
    }

    public bool AllowAppRating {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["AllowAppRating"]);
        }
    }

    public string LoginMessage {
        get {
            CheckAndBuildServerSettingsTable();
            string value = SettingsTable["LoginMessage"];
            if (string.IsNullOrEmpty(value)) {
                return string.Empty;
            }
            else {
                return value;
            }
        }
    }

    public string LoginMessageDate {
        get {
            CheckAndBuildServerSettingsTable();
            string value = SettingsTable["LoginMessageDate"];
            if (string.IsNullOrEmpty(value)) {
                return "N/A";
            }
            else {
                return value;
            }
        }
    }

    public string TwitterAccessToken {
        get {
            CheckAndBuildServerSettingsTable();
            return SettingsTable["TwitterAccessToken"];
        }
    }

    public string TwitterAccessTokenSecret {
        get {
            CheckAndBuildServerSettingsTable();
            return SettingsTable["TwitterAccessTokenSecret"];
        }
    }

    public string TwitterConsumerKey {
        get {
            CheckAndBuildServerSettingsTable();
            return SettingsTable["TwitterConsumerKey"];
        }
    }

    public string TwitterConsumerSecret {
        get {
            CheckAndBuildServerSettingsTable();
            return SettingsTable["TwitterConsumerSecret"];
        }
    }

    public bool RecordLoginActivity {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["RecordLoginActivity"]);
        }
    }

    public int TotalWorkspacesAllowed {
        get {
            CheckAndBuildServerSettingsTable();
            int defaultWorkspaces = 4;
            string value = SettingsTable["TotalWorkspacesAllowed"];
            int.TryParse(value, out defaultWorkspaces);
            if (defaultWorkspaces == 0) {
                defaultWorkspaces = 4;
            }

            return defaultWorkspaces;
        }
    }

    public bool AutoBackupSystem {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["AutoBackupSystem"]);
        }
    }

    public bool AssociateWithGroups {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["AssociateWithGroups"]);
        }
    }

    public bool HideAllAppIcons {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["HideAllAppIcons"]);
        }
    }

    public bool EmailOnRegister {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["EmailOnRegister"]);
        }
    }

    public bool LockASCXEdit {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["LockASCXEdit"]);
        }
    }

    public bool LockCustomTables {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["LockCustomTables"]);
        }
    }

    public bool NoLoginRequired {
        get {
            CheckAndBuildServerSettingsTable();
            bool returnBool = false;
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

            return returnBool;
        }
    }

    public bool ShowPreviewButtonLogin {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["ShowPreviewButtonLogin"]);
        }
    }

    public bool SitePluginsLocked {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["SitePluginsLocked"]);
        }
    }

    public bool NotificationsLocked {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["NotificationsLocked"]);
        }
    }

    public bool OverlaysLocked {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["OverlaysLocked"]);
        }
    }

    public bool AllowPrivacy {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["AllowPrivacy"]);
        }
    }

    public bool EmailSystemStatus {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["EmailSystemStatus"]);
        }
    }

    public bool RecordActivity {
        get {
            CheckAndBuildServerSettingsTable();
            try {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["RecordActivity"]);
            }
            catch { }

            return false;
        }
    }

    public bool SiteOffLine {
        get {
            CheckAndBuildServerSettingsTable();
            try {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["SiteOffLine"]);
            }
            catch { }

            return false;
        }
    }

    public bool EmailActivity {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["EmailActivity"]);
        }
    }

    public bool AutoBlockIP {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["AutoBlockIP"]);
        }
    }

    public bool AutoUpdates {
        get {
            CheckAndBuildServerSettingsTable();
            if (SettingsTable.ContainsKey("AutoUpdates")) {
                return HelperMethods.ConvertBitToBoolean(SettingsTable["AutoUpdates"]);
            }

            return false;
        }
    }

    public int AutoBlockIPCount {
        get {
            CheckAndBuildServerSettingsTable();

            int temp = 10;

            string value = SettingsTable["AutoBlockIPCount"];
            int.TryParse(value, out temp);
            if (temp == 0) {
                temp = 10;
            }

            return temp;
        }
    }

    public int WebEventsToKeep {
        get {
            CheckAndBuildServerSettingsTable();
            int temp = 0;
            int.TryParse(SettingsTable["WebEventsToKeep"], out temp);
            return temp;
        }
    }

    public bool ChatEnabled {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["ChatEnabled"]);
        }
    }

    public bool CacheHomePage {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["CacheHomePage"]);
        }
    }

    public bool LockFileEditor {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["LockFileEditor"]);
        }
    }

    public bool SSLRedirect {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["SSLRedirect"]);
        }
    }

    public bool URLValidation {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["URLValidation"]);
        }
    }

    public bool LockStartupScripts {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["LockStartupScripts"]);
        }
    }

    public bool LockIPListener {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["LockIPListener"]);
        }
    }

    public bool CustomizationsLocked {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["CustomizationsLocked"]);
        }
    }

    public bool AllowUserSignUp {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["AllowUserSignUp"]);
        }
    }

    public static string SiteName {
        get {
            if (string.IsNullOrEmpty(_siteName)) {
                if (!CheckLicense.IsTrial && !CheckLicense.IsExpired) {
                    CheckLicense.ValidateLicense();
                }
            }
            return _siteName;
        }
    }

    public string LogoOpacity {
        get {
            CheckAndBuildServerSettingsTable();
            string value = SettingsTable["LogoOpacity"];
            if (!string.IsNullOrEmpty(value)) {
                return value;
            }

            return "1.0";
        }
    }

    public string UserSignUpRole {
        get {
            CheckAndBuildServerSettingsTable();
            string defaultName = "Standard";
            string value = SettingsTable["UserSignUpRole"];
            defaultName = value;
            if (string.IsNullOrEmpty(defaultName)) {
                defaultName = "Standard";
            }

            return defaultName;
        }
    }

    public string AdminNote {
        get {
            CheckAndBuildServerSettingsTable();
            return SettingsTable["AdminNote"];
        }
    }

    public string LoginScreenTheme {
        get {
            CheckAndBuildServerSettingsTable();
            string defaultName = "Login_Standard_Light";
            try {
                string value = SettingsTable["LoginScreenTheme"];
                defaultName = value;
                if (string.IsNullOrEmpty(defaultName)) {
                    defaultName = "Login_Standard_Light";
                }
            }
            catch { }
            return defaultName;
        }
    }

    public string AdminNoteBy {
        get {
            CheckAndBuildServerSettingsTable();
            DatabaseQuery dbSelect = dbCall.CallSelectSingle(dbTable, "AdminNoteBy", null);
            return SettingsTable["AdminNoteBy"];
        }
    }

    public string AppInstallerPackage {
        get {
            CheckAndBuildServerSettingsTable();
            return SettingsTable["AppInstallerPackage"];
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
            DateTime _start = DateTime.Now;

            DateTime _end = DateTime.Now;
            TimeSpan _duration = (_end - _start);

            double timediff = (512 / _duration.TotalSeconds) / 100;
            double speed = Math.Round(timediff, 3);

            return speed;
        }
    }

    public bool UserSignUpEmailChecker {
        get {
            CheckAndBuildServerSettingsTable();
            return HelperMethods.ConvertBitToBoolean(SettingsTable["UserSignUpEmailChecker"]);
        }
    }

    public string UserSignUpEmailAssociation {
        get {
            CheckAndBuildServerSettingsTable();
            return SettingsTable["UserSignUpEmailAssociation"];
        }
    }

    // Google Sign In
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
            return SettingsTable["GoogleClientId"];
        }
    }
    public string GoogleClientSecret {
        get {
            CheckAndBuildServerSettingsTable();
            return SettingsTable["GoogleClientSecret"];
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
            return SettingsTable["FacebookAppId"];
        }
    }
    public string FacebookAppSecret {
        get {
            CheckAndBuildServerSettingsTable();
            return SettingsTable["FacebookAppSecret"];
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
        bool canUpdateDate = dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LoginMessage", message) }, null);
        if (canUpdateDate) {
            dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LoginMessageDate", date) }, null);
        }
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
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AllowAppRating", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_SignUpConfirmationEmail(bool sendEmail) {
        int temp = 0;
        if (sendEmail) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignUpConfirmationEmail", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterAccessToken(string TwitterAccessToken) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterAccessToken", TwitterAccessToken) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterAccessTokenSecret(string TwitterAccessTokenSecret) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterAccessTokenSecret", TwitterAccessTokenSecret) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterConsumerKey(string TwitterConsumerKey) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterConsumerKey", TwitterConsumerKey) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_TwitterConsumerSecret(string TwitterConsumerSecret) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TwitterConsumerSecret", TwitterConsumerSecret) }, null);
        UpdateServerSettingsTable();
    }

    public static void Update_LogoOpacity(string opacity) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LogoOpacity", opacity) }, null);
        UpdateServerSettingsTable();
    }

    public static void Update_ShowUpdatesPopupDate(string date) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ShowUpdatesPopupDate", date) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_docFolder(string path) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("DocumentsFolder", path) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_AppInstallerPackage(string package) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AppInstallerPackage", package) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_AutoBackupSystem(bool systemOn) {
        int temp = 0;
        if (systemOn) {
            temp = 1;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AutoBackupSystem", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_RecordLoginActivity(bool record) {
        int temp = 0;
        if (record) {
            temp = 1;
        }

        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordLoginActivity", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_TotalWorkspacesAllowed(int count) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("TotalWorkspacesAllowed", count.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_AssociateWithGroups(bool enabled) {
        int temp = 0;
        if (enabled) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AssociateWithGroups", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_EmailOnRegister(bool email) {
        int temp = 0;
        if (email) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("EmailOnRegister", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_NoLoginRequired(bool needLogin) {
        int temp = 0;
        if (needLogin) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("NoLoginRequired", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_HideAllAppIcons(bool hideAll) {
        int temp = 0;
        if (hideAll) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("HideAllAppIcons", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_ShowPreviewButtonLogin(bool show) {
        int temp = 0;
        if (show) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ShowPreviewButtonLogin", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_RecordActivity(bool record) {
        int temp = 0;
        if (record) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("RecordActivity", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_AllowPrivacy(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AllowPrivacy", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_EmailSystemStatus(bool isOn) {
        int temp = 0;
        if (isOn) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("EmailSystemStatus", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_SitePluginsLocked(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SitePluginsLocked", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_NotificationsLocked(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("NotificationsLocked", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_OverlaysLocked(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("OverlaysLocked", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_CacheHomePage(bool cache) {
        int temp = 0;
        if (cache) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("CacheHomePage", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_SSLRedirect(bool redirect) {
        int temp = 0;
        if (redirect) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SSLRedirect", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_URLValidation(bool validate) {
        int temp = 0;
        if (validate) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("URLValidation", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_LockFileEditor(bool _lock) {
        int temp = 0;
        if (_lock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockFileEditor", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_LockStartupScripts(bool _lock) {
        int temp = 0;
        if (_lock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockStartupScripts", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_LockIPListener(bool _lock) {
        int temp = 0;
        if (_lock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockIPListener", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_LockASCXEdit(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockASCXEdit", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_LockCustomTables(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LockCustomTables", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_LoginScreenTheme(string theme) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LoginScreenTheme", theme) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_EmailActivity(bool record) {
        int temp = 0;
        if (record) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("EmailActivity", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_AutoBlockIP(bool autoblock) {
        int temp = 0;
        if (autoblock) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AutoBlockIP", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_SiteOffLine(bool offline) {
        int temp = 0;
        if (offline) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SiteOffLine", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_ChatEnabled(bool chat) {
        int temp = 0;
        if (chat) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("ChatEnabled", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_CustomizationsLocked(bool locked) {
        int temp = 0;
        if (locked) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("CustomizationsLocked", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_SiteName(string name) {
        _siteName = name;
    }

    public static void update_AutoUpdates(bool isOn) {
        int temp = 0;
        if (isOn) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AutoUpdates", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_AutoBlockIPCount(int count) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AutoBlockIPCount", count.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_AdminNote(string note) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AdminNote", note) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_AdminNoteBy(string username) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AdminNoteBy", username) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_AllowUserSignUp(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("AllowUserSignUp", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_UserSignUpRole(string role) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("UserSignUpRole", role) }, null);
        UpdateServerSettingsTable();
    }

    public static string update_LastCacheClearDate() {
        string date = DateTime.Now.ToString();
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("LastCacheDate", date) }, null);
        UpdateServerSettingsTable();
        return date;
    }

    public static void update_WebEventsToKeep(int number) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("WebEventsToKeep", number.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_SignInWithGoogle(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignInWithGoogle", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }
    public static void update_GoogleClientId(string clientId) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("GoogleClientId", clientId) }, null);
        UpdateServerSettingsTable();
    }
    public static void update_GoogleClientSecret(string clientSecret) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("GoogleClientSecret", clientSecret) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_SignInWithTwitter(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignInWithTwitter", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }

    public static void update_SignInWithFacebook(bool allow) {
        int temp = 0;
        if (allow) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("SignInWithFacebook", temp.ToString()) }, null);
        UpdateServerSettingsTable();
    }
    public static void update_FacebookAppId(string appId) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("FacebookAppId", appId) }, null);
        UpdateServerSettingsTable();
    }
    public static void update_FacebookAppSecret(string appSecret) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("FacebookAppSecret", appSecret) }, null);
        UpdateServerSettingsTable();
    }

    private static void UpdateServerSettingsTable() {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(dbTable, "", null);
        if (dbSelect.Count == 1) {
            SettingsTable = dbSelect[0];
        }
    }

    #endregion


    #region User Registration

    public static void update_UserSignUpEmailChecker(bool checker) {
        int temp = 0;
        if (checker) {
            temp = 1;
        }
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("UserSignUpEmailChecker", temp.ToString()) }, null);
    }
    public static void update_UserSignUpEmailAssociation(string email) {
        dbCall.CallUpdate(dbTable, new List<DatabaseQuery>() { new DatabaseQuery("UserSignUpEmailAssociation", email) }, null);
    }

    #endregion


    #region Mail Settings

    private static string _loc = ServerSettings.GetServerMapLocation + "App_Data\\MailSettings" + ServerSettings.SavedDataFilesExt;
    private static string encryptloc = ServerSettings.GetServerMapLocation + "App_Data\\MailSettings_Encrypted" + ServerSettings.SavedDataFilesExt;
    private static List<MailSettings> _coll = new List<MailSettings>();

    public static List<MailSettings> MailSettings_Coll {
        get { return _coll; }
    }
    public static void MailSettingsSerialize(List<MailSettings> list) {
        try {
            var d = new DataEncryption();
            d.SerializeFile(list, _loc);
            d.EncryptFile(_loc, encryptloc, ServerBackup.EncryptPassword);
        }
        catch (Exception e) {
            new AppLog(false).AddError(e);
        }
    }
    public static void MailSettingsDeserialize() {
        try {
            if (File.Exists(encryptloc)) {
                var d = new DataEncryption();
                MemoryStream a = d.DecryptFile(encryptloc, ServerBackup.EncryptPassword);
                using (var str = new BinaryReader(a)) {
                    var bf = new BinaryFormatter();
                    str.BaseStream.Position = 0;
                    _coll = (List<MailSettings>)bf.Deserialize(str.BaseStream);
                    a.Close();
                    str.Close();
                }
            }
        }
        catch (Exception e) {
            new AppLog(false).AddError(e);
        }
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