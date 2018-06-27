#region

using System;
using System.Collections.Generic;
using System.Configuration;
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
    public const string Uncategorized_Name = "Uncategorized";

    public AppCategory(bool getvalues) {
        if (getvalues) {
            dataTable = dbCall.CallSelect("AppCategory", "", new List<DatabaseQuery>() { new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) }, "Category ASC");
        }
    }

    public List<Dictionary<string, string>> category_dt {
        get { return dataTable; }
    }

    public void addItem(string category) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("Category", category));
        query.Add(new DatabaseQuery("UpdatedBy", HttpContext.Current.User.Identity.Name));
        query.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));
        query.Add(new DatabaseQuery("CreatedBy", HttpContext.Current.User.Identity.Name));

        dbCall.CallInsert("AppCategory", query);
    }

    public void addItem(string category, string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));
        query.Add(new DatabaseQuery("Category", category));
        query.Add(new DatabaseQuery("UpdatedBy", HttpContext.Current.User.Identity.Name));
        query.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));
        query.Add(new DatabaseQuery("CreatedBy", HttpContext.Current.User.Identity.Name));

        dbCall.CallInsert("AppCategory", query);
    }

    public string GetCategoryName(string id) {
        if (string.IsNullOrEmpty(id) || id == AppCategory.Uncategorized_Name) {
            return AppCategory.Uncategorized_Name;
        }

        DatabaseQuery dbSelect = dbCall.CallSelectSingle("AppCategory", "Category", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
        string x = dbSelect.Value;
        if (!string.IsNullOrEmpty(x)) {
            return x;
        }

        return string.Empty;
    }

    public void updateItem(string category, string id) {
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Category", category));
        updateQuery.Add(new DatabaseQuery("UpdatedBy", HttpContext.Current.User.Identity.Name));
        updateQuery.Add(new DatabaseQuery("DateUpdated", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallUpdate("AppCategory", updateQuery, new List<DatabaseQuery>() { 
            new DatabaseQuery("ID", id), 
            new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) 
        });
    }

    public bool deleteItem(string id) {
        return dbCall.CallDelete("AppCategory", new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public Dictionary<string, string> BuildCategoryDictionary(string categoryStr) {
        string[] categoryList = categoryStr.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, string> categoryDictionary = new Dictionary<string, string>();

        if (categoryList.Length > 0) {
            foreach (string cId in categoryList) {
                if (string.IsNullOrEmpty(cId) || cId == AppCategory.Uncategorized_Name) {
                    continue;
                }

                string categoryname = GetCategoryName(cId);
                if (string.IsNullOrEmpty(categoryname)) {
                    continue;
                }

                if (!categoryDictionary.ContainsKey(cId)) {
                    categoryDictionary.Add(cId, categoryname);
                }
            }
        }

        if (categoryDictionary.Count == 0) {
            categoryDictionary.Add(AppCategory.Uncategorized_Name, AppCategory.Uncategorized_Name);
        }

        return categoryDictionary;
    }
}