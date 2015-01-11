#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Web;
using System.Web.Configuration;

#endregion

/// <summary>
///     Summary description for GeneralDirection
/// </summary>
[Serializable]
public class AppCategory {
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private List<Dictionary<string, string>> dataTable;

    public AppCategory(bool getvalues) {
        if (getvalues) {
            dataTable = dbCall.CallSelect("AppCategory", "", null, "Category ASC");
        }
    }

    public List<Dictionary<string, string>> category_dt {
        get { return dataTable; }
    }

    public void addItem(string category) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("Category", category));
        query.Add(new DatabaseQuery("DateAdded", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("CreatedBy", HttpContext.Current.User.Identity.Name));

        dbCall.CallInsert("AppCategory", query);
    }

    public void addItem(string category, string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Category", category));
        query.Add(new DatabaseQuery("DateAdded", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("CreatedBy", HttpContext.Current.User.Identity.Name));

        dbCall.CallInsert("AppCategory", query);
    }

    public string GetCategoryName(string id) {
        string temp = "";
        if (string.IsNullOrEmpty(id)) {
            temp = "Uncategorized";
        }
        else {
            DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppCategory", "Category", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
            string x = dbSelect.Value;
            if (string.IsNullOrEmpty(x)) {
                temp = "Uncategorized";
            }
            else {
                temp = x;
            }
        }

        if (string.IsNullOrEmpty(temp)) {
            temp = "Uncategorized";
        }

        return temp;
    }

    public void updateItem(string category, string id) {
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Category", category));

        dbCall.CallUpdate("AppCategory", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
        updateItemDate(id);
    }

    private void updateItemDate(string id) {
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DateAdded", DateTime.Now.ToString()));

        dbCall.CallUpdate("AppCategory", updateQuery, new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }

    public bool deleteItem(string id) {
        return dbCall.CallDelete("AppCategory", new List<DatabaseQuery>() { new DatabaseQuery("ID", id) });
    }
}