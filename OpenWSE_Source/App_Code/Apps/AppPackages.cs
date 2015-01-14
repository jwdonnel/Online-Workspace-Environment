#region

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Data.SqlServerCe;

#endregion

/// <summary>
///     Summary description for AppPackages
/// </summary>
[Serializable]
public class AppPackages {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<Dictionary<string, string>> dataTable;

    public AppPackages(bool getvalues) {
        if (getvalues) {
            dataTable = dbCall.CallSelect("AppPackages", "", null, "PackageName ASC");
        }
    }

    public List<Dictionary<string, string>> listdt {
        get { return dataTable; }
    }

    public void addItem(string name, string list, string user) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("PackageName", name));
        query.Add(new DatabaseQuery("AppList", list));
        query.Add(new DatabaseQuery("UpdatedBy", user));
        query.Add(new DatabaseQuery("Date", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("CreatedBy", user));

        dbCall.CallInsert("AppPackages", query);
    }

    public void SelectPackage(string id) {
        dataTable = dbCall.CallSelect("AppPackages", "", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) }, "PackageName ASC");
    }

    public string[] GetAppList(string id) {
        if (!string.IsNullOrEmpty(id) && id != "0") {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppPackages", "AppList", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });

            string[] delim = { "," };
            string[] array = dbSelect.Value.Split(delim, StringSplitOptions.RemoveEmptyEntries);
            List<string> tempArray = new List<string>();
            App wl = new App();

            foreach (string w in array) {
                if (!tempArray.Contains(w)) {
                    if (wl.IconExists(w))
                        tempArray.Add(w);
                }
            }

            if (array.Length != tempArray.Count) {
                string newList = string.Empty;
                foreach (string w in tempArray) {
                    newList += w + ",";
                }
                updateAppList(id, newList, "System Correction");
            }

            array = tempArray.ToArray();
            return array;
        }
        return new string[] { };
    }

    public string GetAppList_nonarray(string id) {
        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppPackages", "AppList", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        return dbSelect.Value;
    }

    public void updatePackageName(string id, string name, string user) {
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("PackageName", name));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", user));
        updateQuery.Add(new DatabaseQuery("Date", DateTime.Now.ToString()));

        dbCall.CallUpdate("AppPackages", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }

    public void updateAppList(string id, string list, string user) {
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("AppList", list));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", user));
        updateQuery.Add(new DatabaseQuery("Date", DateTime.Now.ToString()));

        dbCall.CallUpdate("AppPackages", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }

    public void deletePackage(string id) {
        dbCall.CallDelete("AppPackages", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }
}