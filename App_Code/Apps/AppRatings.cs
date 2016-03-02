using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[Serializable]
public class AppRatings_Coll {
    private string _id = string.Empty;
    private string _appId = string.Empty;
    private string _userName = string.Empty;
    private string _rating = string.Empty;
    private string _description = string.Empty;
    private string _dateRated = string.Empty;

    public AppRatings_Coll() { }
    public AppRatings_Coll(string id, string appId, string userName, string rating, string description, string dateRated) {
        _id = id;
        _appId = appId;
        _userName = userName;
        _rating = rating;
        _description = description;
        _dateRated = dateRated;
    }

    public string ID {
        get { return _id; }
    }

    public string AppID {
        get { return _appId; }
    }

    public string UserName {
        get { return _userName; }
    }

    public string Rating {
        get { return _rating; }
    }

    public string Description {
        get { return _description; }
    }

    public string DateRated {
        get { return _dateRated; }
    }
}

public class AppRatings
{
    private string _userName = string.Empty;
    private const string _tableName = "AppRatings";
    private readonly DatabaseCall dbCall = new DatabaseCall();

    public AppRatings() { }
    public AppRatings(string userName) {
        _userName = userName;
    }

    public void AddRating(string appId, string rating, string description) {
        // Delete any previous rating by the current user
        DeleteRating(appId);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("AppID", appId));
        query.Add(new DatabaseQuery("UserName", _userName));
        query.Add(new DatabaseQuery("Rating", rating));
        query.Add(new DatabaseQuery("Description", description));
        query.Add(new DatabaseQuery("DateRated", ServerSettings.ServerDateTime.ToString()));

        dbCall.CallInsert(_tableName, query);
    }
    public void UpdateRating(string appId, string rating, string description) {
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Rating", rating));
        updateQuery.Add(new DatabaseQuery("Description", description));
        updateQuery.Add(new DatabaseQuery("DateRated", ServerSettings.ServerDateTime.ToString()));

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", appId));
        query.Add(new DatabaseQuery("UserName", _userName));

        dbCall.CallUpdate(_tableName, updateQuery, query);
    }
    public void DeleteRating(string appId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", appId));
        query.Add(new DatabaseQuery("UserName", _userName));

        dbCall.CallDelete(_tableName, query);
    }

    public List<AppRatings_Coll> GetAppRatings(string appId) {
        List<AppRatings_Coll> returnColl = new List<AppRatings_Coll>();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(_tableName, "", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appId), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> dicVal in dbSelect) {
            string id = dicVal["ID"];
            string userName = dicVal["UserName"];
            string rating = dicVal["Rating"];
            string description = dicVal["Description"];
            string dateRated = dicVal["DateRated"];
            returnColl.Add(new AppRatings_Coll(id, appId, userName, rating, description, dateRated));
        }

        return returnColl;
    }
    public List<AppRatings_Coll> GetAllUserAppRatings(string userName) {
        List<AppRatings_Coll> returnColl = new List<AppRatings_Coll>();

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(_tableName, "", new List<DatabaseQuery>() { new DatabaseQuery("UserName", userName), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });
        foreach (Dictionary<string, string> dicVal in dbSelect) {
            string id = dicVal["ID"];
            string appId = dicVal["AppID"];
            string rating = dicVal["Rating"];
            string description = dicVal["Description"];
            string dateRated = dicVal["DateRated"];
            returnColl.Add(new AppRatings_Coll(id, appId, userName, rating, description, dateRated));
        }

        return returnColl;
    }

    public string GetAverageRating(string appId) {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(_tableName, "Rating", new List<DatabaseQuery>() { new DatabaseQuery("AppID", appId), new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID) });

        if (dbSelect.Count > 0) {
            List<int> ratings = new List<int>();
            foreach (Dictionary<string, string> dicVal in dbSelect) {
                int rating = 0;
                if (int.TryParse(dicVal["Rating"], out rating)) {
                    ratings.Add(rating);
                }
            }

            double avgRating = Math.Round(ratings.ToArray().Average(), 2);
            return avgRating.ToString();
        }

        return "0";
    }
    public AppRatings_Coll GetUserRating(string appId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ApplicationId", ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("AppID", appId));
        query.Add(new DatabaseQuery("UserName", _userName));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(_tableName, "", query);

        AppRatings_Coll userRating = new AppRatings_Coll();
        foreach (Dictionary<string, string> dicVal in dbSelect) {
            string id = dicVal["ID"];
            string rating = dicVal["Rating"];
            string description = dicVal["Description"];
            string dateRated = dicVal["DateRated"];

            userRating = new AppRatings_Coll(id, appId, _userName, rating, description, dateRated);
            break;
        }

        return userRating;
    }

}