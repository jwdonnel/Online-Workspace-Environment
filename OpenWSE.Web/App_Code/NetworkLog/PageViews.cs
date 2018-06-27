using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


[Serializable]
public struct PageViews_Coll {
    private readonly string _id;
    private readonly string _ipAddress;
    private readonly string _username;
    private readonly string _pageName;
    private readonly string _date;

    public PageViews_Coll(string id, string ipAddress, string username, string pageName, string date) {
        _id = id;
        _ipAddress = ipAddress;
        _username = username;
        _pageName = pageName;
        _date = date;
    }

    public string ID {
        get { return _id; }
    }

    public string IPAddress {
        get { return _ipAddress; }
    }

    public string Username {
        get { return _username; }
    }

    public string PageName {
        get { return _pageName; }
    }

    public string DateAdded {
        get { return _date; }
    }
}


[Serializable]
public class PageViewsCount_Coll {
    private string _pageName;
    private string _pageUrl;
    private int _count;

    public PageViewsCount_Coll(string pageName, string pageUrl, int count) {
        _pageName = pageName;
        _pageUrl = pageUrl;
        _count = count;
    }

    public string PageName {
        get { return _pageName; }
    }

    public string PageUrl {
        get { return _pageUrl; }
    }

    public int Count {
        get { return _count; }
        set { _count = value; }
    }
}


/// <summary>
/// Summary description for PageViews
/// </summary>
public class PageViews {

    #region Private Variables
    private const string TableName = "aspnet_PageViews";
    private readonly DatabaseCall dbCall = new DatabaseCall();
    #endregion


    public PageViews() {

    }

    public void AddItem(string ipAddress, string username, string pageName) {
        if (!PageViewsUsersToIgnore.UsernameIsIgnored(username)) {
            pageName = GetItemPageName(pageName);
            if (!string.IsNullOrEmpty(pageName)) {
                List<DatabaseQuery> query = new List<DatabaseQuery>();
                query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
                query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
                query.Add(new DatabaseQuery("IPAddress", ipAddress));
                query.Add(new DatabaseQuery("Username", username));
                query.Add(new DatabaseQuery("PageName", pageName.ToLower()));
                query.Add(new DatabaseQuery("DateAdded", ServerSettings.ServerDateTime.ToString()));

                dbCall.CallInsert(TableName, query);
            }
        }
    }

    public List<PageViews_Coll> GetPageViewDetails(string pageName) {
        List<PageViews_Coll> coll = new List<PageViews_Coll>();
        pageName = GetItemPageName(pageName);
        if (!string.IsNullOrEmpty(pageName)) {
            List<DatabaseQuery> query = new List<DatabaseQuery>();
            query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
            query.Add(new DatabaseQuery("PageName", pageName));
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query, "DateAdded DESC");
            foreach (Dictionary<string, string> row in dbSelect) {
                string id = row["ID"];
                string ipAddress = row["IPAddress"];
                string username = row["Username"];
                string pn = row["PageName"];
                string dateAdded = row["DateAdded"];

                coll.Add(new PageViews_Coll(id, ipAddress, username, pn, dateAdded));
            }
        }

        return coll;
    }

    public Dictionary<string, PageViewsCount_Coll> GetCountForEachPage() {
        Dictionary<string, PageViewsCount_Coll> pageCounts = new Dictionary<string, PageViewsCount_Coll>();

        #region Initialize Page List
        try {
            SiteMapNodeCollection siteNodes = SiteMap.RootNode.ChildNodes;
            foreach (SiteMapNode node in siteNodes) {
                if (node != null && !string.IsNullOrEmpty(node.Title) && !string.IsNullOrEmpty(node.Url)) {
                    if (!pageCounts.ContainsKey(node.Title.ToLower())) {
                        pageCounts.Add(node.Title.ToLower(), new PageViewsCount_Coll(node.Title, node.Url, 0));
                    }
                }
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        #endregion

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "PageName", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            if (!string.IsNullOrEmpty(row["PageName"])) {
                string pageName = row["PageName"].ToLower();
                if (!pageCounts.ContainsKey(pageName)) {
                    pageCounts.Add(pageName, new PageViewsCount_Coll(pageName, "Not Available", 0));
                }
                else {
                    pageCounts[pageName].Count++;
                }
            }
        }

        return pageCounts;
    }

    public string GetItemPageName(string pageName) {
        if (!string.IsNullOrEmpty(pageName)) {
            pageName = pageName.ToLower();
            pageName = pageName.Replace("asp.", string.Empty);
            string[] pageParts = pageName.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            if (pageParts.Length > 1) {
                pageName = pageParts[pageParts.Length - 2] + "." + pageParts[pageParts.Length - 1];
            }

            try {
                SiteMapNodeCollection siteNodes = SiteMap.RootNode.ChildNodes;
                foreach (SiteMapNode node in siteNodes) {
                    if (node != null && !string.IsNullOrEmpty(node.Title) && !string.IsNullOrEmpty(node.Url)) {
                        if (node.Url.ToLower().Contains(pageName) || node.Title.ToLower().Contains(pageName)) {
                            return node.Title;
                        }
                    }
                }
            }
            catch {}
        }

        return string.Empty;
    }

    public void DeleteItemByID(string id) {
        dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("ID", id), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

    public void DeleteItemsByPageName(string pageName) {
        dbCall.CallDelete(TableName, new List<DatabaseQuery>() { new DatabaseQuery("PageName", pageName.ToLower()), new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID) });
    }

}