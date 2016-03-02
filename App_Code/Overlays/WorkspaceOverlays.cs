using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Web.UI;

namespace OpenWSE_Tools.Overlays {

    [Serializable]
    public struct WorkspaceOverlay_Coll {
        private readonly string _id;
        private readonly string _overlayName;
        private readonly string _displayType;
        private readonly string _fileLoc;
        private readonly string _username;
        private readonly string _description;

        public WorkspaceOverlay_Coll(string id, string overlayName, string displayType, string fileLoc, string username, string description) {
            _id = id;
            _overlayName = overlayName;
            _displayType = displayType;
            _fileLoc = fileLoc;
            _username = username;
            _description = description;
        }

        public string ID {
            get { return _id; }
        }

        public string OverlayName {
            get { return _overlayName; }
        }

        public string DisplayType {
            get { return _displayType; }
        }

        public string FileLocation {
            get { return _fileLoc; }
        }

        public string UserName {
            get { return _username; }
        }

        public string Description {
            get { return _description; }
        }

    }

    [Serializable]
    public struct UserOverlay_Coll {
        private readonly string _overlayId;

        public UserOverlay_Coll(string overlayId) {
            _overlayId = overlayId;
        }

        public string OverlayID {
            get { return _overlayId; }
        }
    }

    /// <summary>
    /// Summary description for WorkspaceOverlays
    /// </summary>
    public class WorkspaceOverlays {

        #region Private Variables

        private List<WorkspaceOverlay_Coll> _overlaysList = new List<WorkspaceOverlay_Coll>();
        private List<UserOverlay_Coll> _userOverlays = new List<UserOverlay_Coll>();
        private readonly DatabaseCall dbCall = new DatabaseCall();

        #endregion


        public const string ComplexPanelId = "pnl_OverlaysAll";
        public const string SimplePanelId = "workspace_holder";

        public static string GetOverlayPanelId(Page page, string workspaceMode) {
            bool isComplexMode = MemberDatabase.IsComplexWorkspaceMode(workspaceMode);
            string pnlId = string.Empty;

            if (!isComplexMode && HelperMethods.DoesPageContainStr("workspace.aspx")) {
                pnlId = WorkspaceOverlays.SimplePanelId;
                if (!string.IsNullOrEmpty(page.Request.QueryString["AppPage"])) {
                    pnlId = string.Empty;
                }
            }
            else if (isComplexMode) {
                pnlId = WorkspaceOverlays.ComplexPanelId;
            }

            return pnlId;
        }


        #region Constructor

        public WorkspaceOverlays() { }

        #endregion


        #region Main Overlays

        public void GetWorkspaceOverlays() {
            _overlaysList.Clear();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));

            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("WorkspaceOverlays", "", query, "OverlayName ASC");
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string on = row["OverlayName"];
                string fl = row["FileLoc"];
                string un = row["UserName"];
                string des = row["Description"];
                string displayType = row["DisplayType"];
                var coll = new WorkspaceOverlay_Coll(id, on, displayType, fl, un, des);
                _overlaysList.Add(coll);
            }
        }

        public string GetWorkspaceOverlayID(string fileLoc) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("WorkspaceOverlays", "ID", new List<DatabaseQuery>() { new DatabaseQuery("FileLoc", fileLoc), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
            return dbSelect.Value;
        }

        public string GetWorkspaceFileLoc(string id) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("WorkspaceOverlays", "FileLoc", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
            return dbSelect.Value;
        }

        public string GetWorkspaceOverlayDefaultPosition(string id) {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("WorkspaceOverlays", "Position", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
            return dbSelect.Value;
        }

        public WorkspaceOverlay_Coll GetWorkspaceOverlay(string _id) {
            WorkspaceOverlay_Coll coll = new WorkspaceOverlay_Coll();

            if (!string.IsNullOrEmpty(_id)) {
                List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("WorkspaceOverlays", "", new List<DatabaseQuery>() { new DatabaseQuery("ID", _id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
                foreach (Dictionary<string, string> row in dbSelect) {
                    string id = row["ID"];
                    string on = row["OverlayName"];
                    string fl = row["FileLoc"];
                    string un = row["UserName"];
                    string des = row["Description"];
                    string displayType = row["DisplayType"];
                    coll = new WorkspaceOverlay_Coll(id, on, displayType, fl, un, des);
                    break;
                }
            }

            return coll;
        }
        public WorkspaceOverlay_Coll GetWorkspaceOverlayByFileLoc(string fileLoc) {
            WorkspaceOverlay_Coll coll = new WorkspaceOverlay_Coll();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("WorkspaceOverlays", "", new List<DatabaseQuery>() { new DatabaseQuery("FileLoc", fileLoc), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string on = row["OverlayName"];
                string fl = row["FileLoc"];
                string un = row["UserName"];
                string des = row["Description"];
                string displayType = row["DisplayType"];
                coll = new WorkspaceOverlay_Coll(id, on, displayType, fl, un, des);
                break;
            }

            return coll;
        }

        public List<WorkspaceOverlay_Coll> OverlayList {
            get { return _overlaysList; }
        }

        public string AddOverlay(string username, string overlayname, string fileloc, string desc, string displayType) {
            string id = Guid.NewGuid().ToString();

            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));
            query.Add(new DatabaseQuery("OverlayName", overlayname));
            query.Add(new DatabaseQuery("FileLoc", fileloc));
            query.Add(new DatabaseQuery("UserName", username.ToLower()));
            query.Add(new DatabaseQuery("Description", desc));
            query.Add(new DatabaseQuery("DisplayType", displayType));

            dbCall.CallInsert("WorkspaceOverlays", query);
            return id;
        }

        public void UpdateOverlayName(string id, string overlayname) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("OverlayName", overlayname));

            dbCall.CallUpdate("WorkspaceOverlays", updateQuery, query);
        }

        public void UpdateOverlayDescription(string id, string desc) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("Description", desc));

            dbCall.CallUpdate("WorkspaceOverlays", updateQuery, query);
        }

        public void UpdateOverlayDisplayType(string id, string displayType) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("DisplayType", displayType));

            dbCall.CallUpdate("WorkspaceOverlays", updateQuery, query);
        }

        public void UpdateOverlayFileLoc(string id, string fileloc) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("ID", id));

            List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
            updateQuery.Add(new DatabaseQuery("FileLoc", fileloc));

            dbCall.CallUpdate("WorkspaceOverlays", updateQuery, query);
        }

        public void DeleteOverlay(string id) {
            dbCall.CallDelete("WorkspaceOverlays", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        }

        #endregion


        #region User Overlays

        public void GetUserOverlays(string userName) {
            _userOverlays.Clear();
            if (!string.IsNullOrEmpty(userName)) {
                List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserOverlays", "OverlayID", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName.ToLower()), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
                foreach (Dictionary<string, string> row in dbSelect) {
                    string ol = row["OverlayID"];
                    var coll = new UserOverlay_Coll(ol);
                    if (!_userOverlays.Contains(coll))
                        _userOverlays.Add(coll);
                }
            }
        }

        public void AddUserOverlay(string username, string overlayId) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("UserName", username.ToLower()));
            query.Add(new DatabaseQuery("OverlayID", overlayId));

            dbCall.CallInsert("aspnet_UserOverlays", query);
        }

        public bool IsUserOverlayEnabled(string username, string overlayId) {
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_UserOverlays", "", new List<DatabaseQuery>() { new DatabaseQuery("OverlayID", overlayId), new DatabaseQuery("UserName", username.ToLower()), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
            if (dbSelect.Count > 0) {
                return true;
            }

            return false;
        }

        public void DeleteUserOverlay(string username, string overlayId) {
            dbCall.CallDelete("aspnet_UserOverlays", new List<DatabaseQuery>() { new DatabaseQuery("OverlayID", overlayId), new DatabaseQuery("UserName", username.ToLower()), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        }

        public void DeleteAllUserOverlays(string username) {
            dbCall.CallDelete("aspnet_UserOverlays", new List<DatabaseQuery>() { new DatabaseQuery("UserName", username.ToLower()), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        }

        public List<UserOverlay_Coll> UserOverlays {
            get { return _userOverlays; }
        }

        #endregion

    }
}