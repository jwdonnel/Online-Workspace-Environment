#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion


[Serializable]
public class StartupScriptsSheets_Coll {
    private string _id;
    private string _scriptPath;
    private int _seq;
    private string _applyTo;
    private string _updatedBy;
    private DateTime _dateUpdated = new DateTime();
    private string _theme;

    public StartupScriptsSheets_Coll(string id, string scriptPath, string seq, string applyTo, string updatedBy, string dateUpdated, string theme) {
        _id = id;
        _scriptPath = scriptPath;
        int.TryParse(seq, out _seq);
        _applyTo = applyTo;
        _updatedBy = updatedBy;
        DateTime.TryParse(dateUpdated, out _dateUpdated);
        _theme = theme;
    }

    public string ID {
        get { return _id; }
    }

    public string ScriptPath {
        get { return _scriptPath; }
    }

    public int Sequence {
        get { return _seq; }
    }

    public string ApplyTo {
        get { return _applyTo; }
    }

    public string UpdatedBy {
        get { return _updatedBy; }
    }

    public DateTime DateUpdated {
        get { return _dateUpdated; }
    }

    public string Theme {
        get {
            if (string.IsNullOrEmpty(_theme)) {
                return "Standard";
            }
            return _theme; 
        }
    }
}


/// <summary>
///     Summary description for StartupStyleSheets
/// </summary>
public class StartupStyleSheets {
    private List<StartupScriptsSheets_Coll> _StartupscriptsList = new List<StartupScriptsSheets_Coll>();
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private bool _skipTimeStampAppend = false;

    public StartupStyleSheets(bool getvalues, bool skipTimeStampAppend = false) {
        _skipTimeStampAppend = skipTimeStampAppend;
        if (getvalues) {
            GetScripts();
        }
    }

    public List<StartupScriptsSheets_Coll> StartupScriptsSheetsList {
        get {
            return _StartupscriptsList;
        }
    }

    public void AddCssToPage(string path, Page page) {
        string _path = page.ResolveUrl(path);
        var cssFile = new Literal {
            Text = @"<link href=""" + _path + @""" type=""text/css"" rel=""stylesheet"" />"
        };
        page.Header.Controls.Add(cssFile);
    }

    public void addItem(string scriptpath, int sequence, string updatedby, string applyto, string theme) {
        scriptpath = scriptpath.Replace("\"", "");

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("ScriptPath", scriptpath.Trim()));
        query.Add(new DatabaseQuery("Sequence", sequence.ToString()));
        query.Add(new DatabaseQuery("ApplyTo", applyto.Trim()));
        query.Add(new DatabaseQuery("UpdatedBy", updatedby.Trim()));
        query.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));
        query.Add(new DatabaseQuery("Theme", theme));

        dbCall.CallInsert("StartupStyleSheets", query);

        GetScripts();
    }

    public void UpdateApplyTo_AppEditor(string oldname, string newname, string user) {
        foreach (var dr in _StartupscriptsList.Cast<StartupScriptsSheets_Coll>().Where(dr => dr.ApplyTo.ToLower() == oldname.ToLower())) {
            updateAppliesTo(dr.ID, newname, user);
        }

        GetScripts();
    }

    public int GetSequence(string script) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("StartupStyleSheets", "Sequence", new List<DatabaseQuery>() { new DatabaseQuery("ScriptPath", script), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        int count = 0;
        int.TryParse(dbSelect.Value, out count);
        return count;
    }

    public int GetSequence_ByID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("StartupStyleSheets", "Sequence", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        int count = 0;
        int.TryParse(dbSelect.Value, out count);
        return count;
    }

    public string GetScriptPathFromSequence(int sequence) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("StartupStyleSheets", "ScriptPath", new List<DatabaseQuery>() { new DatabaseQuery("Sequence", sequence.ToString()), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public string GetAppliesToFromID(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("StartupStyleSheets", "ApplyTo", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        return dbSelect.Value;
    }

    public List<string> GetScriptIDFromSequence(int sequence) {
        var script = new List<string>();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("StartupStyleSheets", "ID", new List<DatabaseQuery>() { new DatabaseQuery("Sequence", sequence.ToString()), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            script.Add(row["ID"]);
        }

        return script;
    }

    public bool CheckIfExists(string scriptPath) {
        bool ex = false;

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("StartupStyleSheets", "ScriptPath", new List<DatabaseQuery>() { new DatabaseQuery("ScriptPath", scriptPath), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string _ScriptPath = row["ScriptPath"];
            if ((!string.IsNullOrEmpty(_ScriptPath)) && (_ScriptPath.Trim() == scriptPath.Trim())) {
                ex = true;
            }
        }

        return ex;
    }

    public bool CheckIfExists(string scriptPath, string applyto) {
        bool ex = false;

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("StartupStyleSheets", "ScriptPath", new List<DatabaseQuery>() { new DatabaseQuery("ScriptPath", scriptPath), new DatabaseQuery("ApplyTo", applyto), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> row in dbSelect) {
            string sp = row["ScriptPath"];
            if ((!string.IsNullOrEmpty(sp)) && (sp.Trim() == scriptPath.Trim())) {
                ex = true;
            }
        }

        return ex;
    }

    public void updateScriptPath(string ID, string scriptpath, string user, bool getScripts = true) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", ID.Trim()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ScriptPath", scriptpath.Trim()));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", user));
        updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate("StartupStyleSheets", updateQuery, query);

        if (getScripts)
            GetScripts();
    }

    public void updateTheme(string ID, string theme, string user) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", ID.Trim()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Theme", theme.ToString()));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", user));
        updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate("StartupStyleSheets", updateQuery, query);

        GetScripts();
    }

    public void updateSequence(string ID, int sequence, string user) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", ID.Trim()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Sequence", sequence.ToString()));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", user));
        updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate("StartupStyleSheets", updateQuery, query);
    }

    public void updateSequence_List(string[] sequence, string user) {
        int count = 1;
        foreach (string id in sequence) {
            updateSequence(id, count, user);
            count++;
        }

        GetScripts();
    }

    public void updateAppliesTo(string id, string applyto, string user) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id.Trim()));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ApplyTo", applyto.Trim()));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", user));
        updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate("StartupStyleSheets", updateQuery, query);
        GetScripts();
    }

    public void deleteStartupScript(string ID) {
        dbCall.CallDelete("StartupStyleSheets", new List<DatabaseQuery>() { new DatabaseQuery("ID", ID), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        GetScripts();
        int count = 1;
        foreach (StartupScriptsSheets_Coll dr in _StartupscriptsList) {
            updateSequence(dr.ID, count, HttpContext.Current.User.Identity.Name);
            count++;
        }

        GetScripts();
    }

    public void GetScripts() {
        _StartupscriptsList.Clear();

        ServerSettings _ss = new ServerSettings();
        bool appendTimestamp = false;
        if (!_skipTimeStampAppend) {
            appendTimestamp = _ss.AppendTimestampOnScripts;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("StartupStyleSheets", "", query, "Sequence ASC");
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"].Trim();
            string scriptPath = row["ScriptPath"].Trim();

            if (!_skipTimeStampAppend && appendTimestamp) {
                string querySeperator = "?";
                if (scriptPath.Contains("?")) {
                    querySeperator = "&";
                }

                scriptPath += string.Format("{0}{1}{2}", querySeperator, ServerSettings.TimestampQuery, HelperMethods.GetTimestamp());
            }

            string seq = row["Sequence"].Trim();
            string applyTo = row["ApplyTo"].Trim();
            string updatedBy = row["UpdatedBy"].Trim();
            string dateUpdated = row["DateUpdated"].Trim();
            string theme = row["Theme"].Trim();
            var coll = new StartupScriptsSheets_Coll(id, scriptPath, seq, applyTo, updatedBy, dateUpdated, theme);
            _StartupscriptsList.Add(coll);
        }
    }
}