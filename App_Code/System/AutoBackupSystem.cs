using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using System.Data;
using System.Threading;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SqlServerCe;
using OpenWSE_Library.Core.BackgroundServices;
using OpenWSE_Tools.BackgroundServiceDatabaseCalls;

namespace OpenWSE_Tools.AppServices {

    [Serializable]
    public struct AutoBackupSystem_Coll {
        private readonly string _id;
        private readonly string _backupDay;
        private readonly string _backupTime;
        private readonly string _backupType;

        public AutoBackupSystem_Coll(string id, string backupDay, string backupTime, string backupType) {
            _id = id;
            _backupDay = backupDay;
            _backupTime = backupTime;
            _backupType = backupType;
        }

        public string ID {
            get { return _id; }
        }

        public string BackupDay {
            get { return _backupDay; }
        }

        public string BackupTime {
            get { return _backupTime; }
        }

        public string BackupType {
            get { return _backupType; }
        }
    }


    /// <summary>
    /// Summary description for AutoBackupSystem
    /// </summary>
    public class AutoBackupSystem : IBackgroundServiceState {

        #region Required Variables

        /// <summary> Get or Set the current state of the object
        /// </summary>
        protected static BackgroundStates CurrentState {
            get;
            set;
        }

        #endregion

        public const string AutoBackupSystemNamespace = "OpenWSE_Tools.AppServices.AutoBackupSystem";
        private readonly DatabaseCall dbCall = new DatabaseCall();
        private static List<AutoBackupSystem_Coll> dataTable = null;
        private static string _serverPath = "";

        public static BackgroundStates GetCurrentState {
            get {
                return CurrentState;
            }
        }

        public AutoBackupSystem() { }
        public AutoBackupSystem(string serverPath) {
            _serverPath = serverPath;
        }

        public void GetEntries() {
            if (dataTable == null) {
                dataTable = new List<AutoBackupSystem_Coll>();
                dataTable.Clear();

                List<DatabaseQuery> query = new List<DatabaseQuery>();
                query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));

                List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AutoBackupSystem", "", query, "BackupDay ASC");
                foreach (Dictionary<string, string> row in dbSelect) {
                    string id = row["ID"];
                    string day = row["BackupDay"];
                    string time = row["BackupTime"];
                    string type = row["BackupType"];
                    var coll = new AutoBackupSystem_Coll(id, day, time, type);
                    dataTable.Add(coll);
                }
            }
        }

        public AutoBackupSystem_Coll GetEntry(string _id) {
            AutoBackupSystem_Coll coll = new AutoBackupSystem_Coll();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("AutoBackupSystem", "", new List<DatabaseQuery>() { new DatabaseQuery("ID", _id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string day = row["BackupDay"];
                string time = row["BackupTime"];
                string type = row["BackupType"];
                coll = new AutoBackupSystem_Coll(id, day, time, type);
                break;
            }

            return coll;
        }

        public void addItem(string day, string time, string backupType) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("BackupDay", day));
            query.Add(new DatabaseQuery("BackupTime", time));
            query.Add(new DatabaseQuery("BackupType", backupType));

            dbCall.CallInsert("AutoBackupSystem", query);

            dataTable = null;
            GetEntries();
        }

        public void updateBackupDay(string id, string day) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("BackupDay", day));

            dbCall.CallUpdate("AutoBackupSystem", updateQuery, query);

            dataTable = null;
            GetEntries();
        }

        public void updateBackupTime(string id, string time) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("BackupTime", time));

            dbCall.CallUpdate("AutoBackupSystem", updateQuery, query);

            dataTable = null;
            GetEntries();
        }

        public void updateBackupType(string id, string backupType) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("BackupType", backupType));

            dbCall.CallUpdate("AutoBackupSystem", updateQuery, query);

            dataTable = null;
            GetEntries();
        }

        public void deleteItem(string id) {
            dbCall.CallDelete("AutoBackupSystem", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });

            dataTable = null;
            GetEntries();
        }

        public static List<AutoBackupSystem_Coll> AutoBackupColl {
            get { return dataTable; }
        }

        #region Interface Methods

        /// <summary> Used for locking the current thread running the service
        /// </summary>
        private static object LockObject = new object();

        public void StartService() {
            ThreadStart startThread = new ThreadStart(BackupSystemChecker);
            Thread serviceThread = new Thread(startThread);
            serviceThread.Start();
        }
        public void StopService() { }

        #endregion

        #region Run Auto Backup
        private static List<AutoBackupSystem_Coll> tempColl = new List<AutoBackupSystem_Coll>();

        private void BackupSystemChecker() {
            lock (LockObject) {
                string currDay = ServerSettings.ServerDateTime.DayOfWeek.ToString();
                while (CurrentState == BackgroundStates.Running) {
                    GetEntries();
                    string dayofweek = ServerSettings.ServerDateTime.DayOfWeek.ToString();
                    if (currDay != dayofweek) {
                        tempColl.Clear();
                    }

                    string timeofday = ServerSettings.ServerDateTime.ToShortTimeString().ToLower();
                    int seconds = ServerSettings.ServerDateTime.Second;
                    foreach (AutoBackupSystem_Coll coll in dataTable) {
                        if ((coll.BackupDay == dayofweek) && (coll.BackupTime == timeofday) && (!tempColl.Contains(coll))) {
                            tempColl.Add(coll);
                            StartRunningBackup(coll.BackupType);
                        }
                    }

                    Thread.Sleep(1000);
                }

                CurrentState = BackgroundStates.Stopped;
            }
        }

        private void StartRunningBackup(string backupType) {
            string serverPath = _serverPath;
            var dbviewer = new DBViewer(true);

            BackgroundServices _backgroundServices = new BackgroundServices();
            bool logInfo = _backgroundServices.DoesLogInformation(AutoBackupSystemNamespace);

            switch (backupType.ToLower()) {
                case "partial":
                    string backupfile = serverPath + "Backups\\BackupLog.xml";
                    string loc = serverPath + "Backups\\Database_" + ServerSettings.ServerDateTime.ToFileTime() + "Temp" + ServerSettings.BackupFileExt;
                    var sb = new ServerBackup(ServerSettings.AdminUserName, loc);
                    sb.BinarySerialize(dbviewer.dt);
                    Thread.Sleep(200);
                    string tDesc = "Auto Partial Backup";
                    WriteToXml(backupfile, loc.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), tDesc);
                    if (logInfo) {
                        BackgroundServiceCalls.AttemptLogMessage(AutoBackupSystemNamespace, string.Format("A partial backup has just ran and stored as {0}.", loc));
                    }
                    break;
                case "full":
                    try {
                        if (!Directory.Exists(serverPath + "Backups\\Temp")) {
                            Directory.CreateDirectory(serverPath + "Backups\\Temp");
                        }

                        foreach (string filename in Directory.GetFiles(serverPath + "Backups\\Temp")) {
                            if (File.Exists(filename)) {
                                File.Delete(filename);
                            }
                        }
                    }
                    catch {
                    }
                    string f = "DBFull_" + ServerSettings.ServerDateTime.ToFileTime();
                    string loc2 = serverPath + "Backups\\Temp\\" + f + "Temp" + ServerSettings.BackupFileExt;
                    var sb2 = new ServerBackup(ServerSettings.AdminUserName, loc2);

                    sb2.BinarySerialize_Current(dbviewer.dt);

                    string backupfile2 = serverPath + "Backups\\BackupLog.xml";
                    Thread.Sleep(200);
                    WriteToXml(backupfile2, loc2.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), "Auto Full Backup");

                    if (File.Exists(loc2.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt))) {
                        File.Copy(loc2.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt), serverPath + "Backups\\" + f + ServerSettings.BackupFileExt, true);
                    }

                    ServerSettings.DeleteBackupTempFolderFiles();
                    if (logInfo) {
                        BackgroundServiceCalls.AttemptLogMessage(AutoBackupSystemNamespace, string.Format("A full backup has just ran and stored as {0}.", loc2.Replace("Temp" + ServerSettings.BackupFileExt, ServerSettings.BackupFileExt)));
                    }
                    break;
            }
        }

        private void WriteToXml(string backupfile, string path, string desc) {
            try {
                if (File.Exists(backupfile)) {
                    var fi = new FileInfo(path);
                    var reader = new XmlTextReader(backupfile);
                    var doc = new XmlDocument();
                    doc.Load(reader);
                    reader.Close();

                    XmlElement root = doc.DocumentElement;
                    XmlElement newBackup = doc.CreateElement("Backup");
                    var mem = new MemberDatabase(ServerSettings.AdminUserName);
                    var str = new StringBuilder();
                    str.Append("<Filename>" + fi.Name + "</Filename>");
                    str.Append("<Description>" + desc + "</Description>");
                    str.Append("<BackupDate>" + fi.CreationTime.ToString(CultureInfo.InvariantCulture) + "</BackupDate>");
                    str.Append("<RestoreDate>N/A</RestoreDate>");
                    str.Append("<Size>" + HelperMethods.FormatBytes(fi.Length) + "</Size>");
                    str.Append("<User>" + HelperMethods.MergeFMLNames(mem) + "</User>");

                    newBackup.InnerXml = str.ToString();

                    if (root != null) root.PrependChild(newBackup);

                    //save the output to a file
                    doc.Save(backupfile);
                }
                else {
                    var doc = new XmlDocument();
                    doc.LoadXml("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><Backups></Backups>");
                    var writer = new XmlTextWriter(backupfile, null) { Formatting = Formatting.Indented };
                    doc.Save(writer);
                    writer.Close();
                    WriteToXml(backupfile, path, desc);
                }
            }
            catch {
            }
        }
        #endregion

    }

}