using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using OpenWSE_Library.Core.BackgroundServices;

namespace OpenWSE_Tools.BackgroundServiceDatabaseCalls {

    [Serializable]
    public class BackgroundServices_Coll : BackgroundServiceCalls {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private BackgroundStates _state = BackgroundStates.Stopped;
        private string _dllFolder = string.Empty;
        private string _namespace = string.Empty;
        private bool _logInformation = false;
        private string _updatedBy = string.Empty;
        private DateTime _dateUpdated = new DateTime();

        public BackgroundServices_Coll() { }
        public BackgroundServices_Coll(string id, string name, string description, string dllFolder, string nameSpace, string logInformation, string updatedBy, string dateUpdated) {
            _id = id;
            _name = name;
            _description = description;
            _state = GetCurrentStateOfItem(nameSpace);
            _dllFolder = dllFolder;
            _namespace = nameSpace;
            _logInformation = HelperMethods.ConvertBitToBoolean(logInformation);
            _updatedBy = updatedBy;
            DateTime.TryParse(dateUpdated, out _dateUpdated);
        }
        public BackgroundServices_Coll(string id, string name, string description, string dllFolder, string defaultState, string nameSpace, string logInformation, string updatedBy, string dateUpdated) {
            _id = id;
            _name = name;
            _description = description;
            Enum.TryParse<BackgroundStates>(defaultState, out _state);
            _dllFolder = dllFolder;
            _namespace = nameSpace;
            _logInformation = HelperMethods.ConvertBitToBoolean(logInformation);
            _updatedBy = updatedBy;
            DateTime.TryParse(dateUpdated, out _dateUpdated);
        }

        public string ID {
            get { return _id; }
        }

        public string Name {
            get { return _name; }
        }

        public string Description {
            get { return _description; }
        }

        public BackgroundStates State {
            get { return _state; }
        }

        public string DLL_Location {
            get { return _dllFolder; }
        }

        public string Namespace {
            get { return _namespace; }
        }

        public bool LogInformation {
            get { return _logInformation; }
        }

        public string UpdatedBy {
            get { return _updatedBy; }
        }

        public DateTime DateUpdated {
            get { return _dateUpdated; }
        }
    }

    public class BackgroundServices : BackgroundServiceCalls {

        private readonly DatabaseCall dbCall = new DatabaseCall();
        private const string TableName = "aspnet_BackgroundServices";

        public BackgroundServices() { }

        public void AddItem(string name, string description, BackgroundStates state, string serviceNamespace, string dllFolder, bool logInfo, string username) {
            string logInfoStr = "0";
            if (logInfo) {
                logInfoStr = "1";
            }

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
            query.Add(new DatabaseQuery("Name", name));
            query.Add(new DatabaseQuery("Description", description));
            query.Add(new DatabaseQuery("DLL_Location", dllFolder));
            query.Add(new DatabaseQuery("DefaultState", state.ToString()));
            query.Add(new DatabaseQuery("Namespace", serviceNamespace));
            query.Add(new DatabaseQuery("LogInformation", logInfoStr));
            query.Add(new DatabaseQuery("UpdatedBy", username));
            query.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));
            dbCall.CallInsert(TableName, query);
        }

        public BackgroundServices_Coll GetBackgroundService(string id) {
            BackgroundServices_Coll returnColl = new BackgroundServices_Coll();

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            foreach (Dictionary<string, string> dicVal in dbSelect) {
                string name = dicVal["Name"];
                string description = dicVal["Description"];
                string dllFolder = dicVal["DLL_Location"];
                string nameSpace = dicVal["Namespace"];
                string logInformation = dicVal["LogInformation"];
                string updatedBy = dicVal["UpdatedBy"];
                string dateUpdated = dicVal["DateUpdated"];

                returnColl = new BackgroundServices_Coll(id, name, description, dllFolder, nameSpace, logInformation, updatedBy, dateUpdated);
                break;
            }

            return returnColl;
        }

        public BackgroundServices_Coll GetBackgroundServiceFromNamespace(string id) {
            BackgroundServices_Coll returnColl = new BackgroundServices_Coll();

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            foreach (Dictionary<string, string> dicVal in dbSelect) {
                string name = dicVal["Name"];
                string description = dicVal["Description"];
                string dllFolder = dicVal["DLL_Location"];
                string nameSpace = dicVal["Namespace"];
                string logInformation = dicVal["LogInformation"];
                string updatedBy = dicVal["UpdatedBy"];
                string dateUpdated = dicVal["DateUpdated"];

                returnColl = new BackgroundServices_Coll(id, name, description, dllFolder, nameSpace, logInformation, updatedBy, dateUpdated);
                break;
            }

            return returnColl;
        }

        public List<BackgroundServices_Coll> GetBackgroundServices() {
            List<BackgroundServices_Coll> returnColl = new List<BackgroundServices_Coll>();

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Name ASC");
            foreach (Dictionary<string, string> dicVal in dbSelect) {
                string id = dicVal["ID"];
                string name = dicVal["Name"];
                string description = dicVal["Description"];
                string dllFolder = dicVal["DLL_Location"];
                string nameSpace = dicVal["Namespace"];
                string logInformation = dicVal["LogInformation"];
                string updatedBy = dicVal["UpdatedBy"];
                string dateUpdated = dicVal["DateUpdated"];
                returnColl.Add(new BackgroundServices_Coll(id, name, description, dllFolder, nameSpace, logInformation, updatedBy, dateUpdated));
            }

            return returnColl;
        }

        public List<BackgroundServices_Coll> GetBackgroundServicesWithDefaultStates() {
            List<BackgroundServices_Coll> returnColl = new List<BackgroundServices_Coll>();

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Name ASC");
            foreach (Dictionary<string, string> dicVal in dbSelect) {
                string id = dicVal["ID"];
                string name = dicVal["Name"];
                string description = dicVal["Description"];
                string dllFolder = dicVal["DLL_Location"];
                string defaultState = dicVal["DefaultState"];
                string nameSpace = dicVal["Namespace"];
                string logInformation = dicVal["LogInformation"];
                string updatedBy = dicVal["UpdatedBy"];
                string dateUpdated = dicVal["DateUpdated"];
                returnColl.Add(new BackgroundServices_Coll(id, name, description, dllFolder, defaultState, nameSpace, logInformation, updatedBy, dateUpdated));
            }

            return returnColl;
        }

        public bool StartServiceOnDefault(string serviceNamespace) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "DefaultState", new List<DatabaseQuery>() { new DatabaseQuery("Namespace", serviceNamespace), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            return dbSelect.Value == BackgroundStates.Running.ToString();
        }

        public bool DoesLogInformation(string serviceNamespace) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "LogInformation", new List<DatabaseQuery>() { new DatabaseQuery("Namespace", serviceNamespace), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            return HelperMethods.ConvertBitToBoolean(dbSelect.Value);
        }

        public string GetBinFolder(string serviceNamespace) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle(TableName, "DLL_Location", new List<DatabaseQuery>() { new DatabaseQuery("Namespace", serviceNamespace), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
            return ServerSettings.GetServerMapLocation + "Bin\\" + dbSelect.Value;
        }

        public void UpdateItem(string id, string name, string description, bool logInformation, string defaultState, string username) {
            string logInfo = "0";
            if (logInformation) {
                logInfo = "1";
            }

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Name", name));
            updateQuery.Add(new DatabaseQuery("Description", description));
            updateQuery.Add(new DatabaseQuery("LogInformation", logInfo));
            updateQuery.Add(new DatabaseQuery("UpdatedBy", username));
            updateQuery.Add(new DatabaseQuery("DefaultState", defaultState));
            updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));

            dbCall.CallUpdate(TableName, updateQuery, query);
        }

        public void UpdateState(string id, string fullNamespace, BackgroundStates state, bool logInfo, string username) {
            if (SetCurrentStateOfItem(fullNamespace, state, logInfo)) {
                List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
                updateQuery.Add(new DatabaseQuery("UpdatedBy", username));
                updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

                List<DatabaseQuery> query = new List<DatabaseQuery>();
                query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
                query.Add(new DatabaseQuery("ID", id));

                dbCall.CallUpdate(TableName, updateQuery, query);
            }
        }

        public void UpdateState_DateOnly(string fullNamespace) {
            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("Namespace", fullNamespace));

            dbCall.CallUpdate(TableName, updateQuery, query);
        }

        public BackgroundStates GetCurrentStateFromNamespace(string fullNamespace) {
            return GetCurrentStateOfItem(fullNamespace);
        }

        public double GetCurrentCpuUsageFromNamespace(string fullNamespace) {
            return GetCurrentCpuUsage(fullNamespace);
        }

        public void DeleteItem(string id) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));
            dbCall.CallDelete(TableName, query);
        }

    }

}