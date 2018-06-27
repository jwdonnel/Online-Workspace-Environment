#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using OpenWSE_Tools.AutoUpdates;

#endregion

[Serializable]
public class Scheduler_Coll {
    private readonly string _SchDate;
    private readonly string _cn;
    private readonly string _comment;
    private readonly string _company;
    private readonly bool _complete;
    private readonly string _email;
    private readonly string _from;
    private readonly Guid _id;
    private readonly int _items;
    private readonly string _phonenumber;
    private readonly string _schType;
    private readonly string _timeSch;
    private readonly string _trucknum;

    public Scheduler_Coll(Guid id, string c, string t, string d, int i, string p, string e, string schType,
                          string timeSch, string SchDate, string comment, string cn, bool complete) {
        _id = id;
        _company = c;
        _trucknum = t;
        _from = d;
        _items = i;
        _phonenumber = p;
        _email = e;
        _schType = schType;
        _timeSch = timeSch;
        _SchDate = SchDate;
        _comment = comment;
        _cn = cn;
        _complete = complete;
    }

    public Guid ID {
        get { return _id; }
    }

    public string Company {
        get { return _company; }
    }

    public string TruckNum {
        get { return _trucknum; }
    }

    public string DeliveryFrom {
        get { return _from; }
    }

    public int Items {
        get { return _items; }
    }

    public string PhoneNumber {
        get { return _phonenumber; }
    }

    public string Email {
        get { return _email; }
    }

    public string ScheduleType {
        get { return _schType; }
    }

    public string TimeScheduled {
        get { return _timeSch; }
    }

    public string ScheduleDate {
        get { return _SchDate; }
    }

    public string Comment {
        get { return _comment; }
    }

    public string ConfirmationNum {
        get { return _cn; }
    }

    public bool Complete {
        get { return _complete; }
    }
}

/// <summary>
///     Summary description for Scheduler
/// </summary>
[Serializable]
public class Scheduler {
    private readonly AppLog _applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private List<Scheduler_Coll> _schedulerColl = new List<Scheduler_Coll>();
    private const string TableName = "Scheduler";

    public Scheduler() {
        _schedulerColl.Clear();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string c = row["Company"];
            string t = row["TruckNum"];
            string d = row["DeliverFrom"];
            int i = Convert.ToInt32(row["Items"]);
            string p = row["PhoneNumber"];
            string e = row["Email"];
            string schType = row["ScheduleType"];
            string timeSch = row["TimeScheduled"];
            string schDate = row["ScheduleDate"];
            string comment = row["Comment"];
            string cn = row["ConfirmationNumber"];
            bool complete = HelperMethods.ConvertBitToBoolean(row["Complete"]);
            var id = Guid.Parse(row["ID"]);
            var coll = new Scheduler_Coll(id, c, t, d, i, p, e, schType, timeSch, schDate, comment, cn, complete);
            updateSlots(coll);
        }
    }

    public Scheduler(string date) {
        _schedulerColl.Clear();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string c = row["Company"];
            string t = row["TruckNum"];
            string d = row["DeliverFrom"];
            int i = Convert.ToInt32(row["Items"]);
            string p = row["PhoneNumber"];
            string e = row["Email"];
            string schType = row["ScheduleType"];
            string timeSch = row["TimeScheduled"];
            string schDate = row["ScheduleDate"];
            string comment = row["Comment"];
            string cn = row["ConfirmationNumber"];
            bool complete = HelperMethods.ConvertBitToBoolean(row["Complete"]);
            var id = Guid.Parse(row["ID"]);
            var coll = new Scheduler_Coll(id, c, t, d, i, p, e, schType, timeSch, schDate, comment, cn, complete);
            updateSlots(coll);
        }
    }

    public Scheduler(string date, string type) {
        _schedulerColl.Clear();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));
        query.Add(new DatabaseQuery("ScheduleType", type));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string c = row["Company"];
            string t = row["TruckNum"];
            string d = row["DeliverFrom"];
            int i = Convert.ToInt32(row["Items"]);
            string p = row["PhoneNumber"];
            string e = row["Email"];
            string schType = row["ScheduleType"];
            string timeSch = row["TimeScheduled"];
            string schDate = row["ScheduleDate"];
            string comment = row["Comment"];
            string cn = row["ConfirmationNumber"];
            bool complete = HelperMethods.ConvertBitToBoolean(row["Complete"]);
            var id = Guid.Parse(row["ID"]);
            var coll = new Scheduler_Coll(id, c, t, d, i, p, e, schType, timeSch, schDate, comment, cn, complete);
            updateSlots(coll);
        }
    }

    public Scheduler(string date, bool opencomp) {
        _schedulerColl.Clear();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));
        query.Add(new DatabaseQuery("Complete", opencomp.ToString()));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string c = row["Company"];
            string t = row["TruckNum"];
            string d = row["DeliverFrom"];
            int i = Convert.ToInt32(row["Items"]);
            string p = row["PhoneNumber"];
            string e = row["Email"];
            string schType = row["ScheduleType"];
            string timeSch = row["TimeScheduled"];
            string schDate = row["ScheduleDate"];
            string comment = row["Comment"];
            string cn = row["ConfirmationNumber"];
            bool complete = HelperMethods.ConvertBitToBoolean(row["Complete"]);
            var id = Guid.Parse(row["ID"]);
            var coll = new Scheduler_Coll(id, c, t, d, i, p, e, schType, timeSch, schDate, comment, cn, complete);
            updateSlots(coll);
        }
    }

    public List<Scheduler_Coll> scheduler_coll {
        get { return _schedulerColl; }
    }

    private Guid UpdateIDFromNull(string d) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", d));

        string id = Guid.NewGuid().ToString();
        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ID", id));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }

        return Guid.Parse(id); ;
    }

    public void addItem(string company, string trucknum, string from, string items, string phonenumber, string email,
                        string type, string timesch, string schdate, string comment, string cn) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("Company", company));
        query.Add(new DatabaseQuery("TruckNum", trucknum));
        query.Add(new DatabaseQuery("DeliverFrom", from));
        query.Add(new DatabaseQuery("Items", items));
        query.Add(new DatabaseQuery("PhoneNumber", phonenumber));
        query.Add(new DatabaseQuery("Email", email));
        query.Add(new DatabaseQuery("ScheduleType", type));
        query.Add(new DatabaseQuery("TimeScheduled", timesch));
        query.Add(new DatabaseQuery("ScheduleDate", schdate));
        query.Add(new DatabaseQuery("Comment", comment));
        query.Add(new DatabaseQuery("ConfirmationNumber", cn));
        query.Add(new DatabaseQuery("Complete", "false"));

        if (dbCall.CallInsert(TableName, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public int getTotalDelPu(string date, string type) {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("SELECT * FROM Scheduler WHERE ScheduleDate like '" + date + "%' AND ScheduleType='" + type + "' AND ApplicationId='" + ServerSettings.ApplicationID + "'");
        return dbSelect.Count;
    }

    public int getTotalComplete() {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("SELECT * FROM Scheduler WHERE Complete='true' AND ApplicationId='" + ServerSettings.ApplicationID + "'");
        return dbSelect.Count;
    }

    public int getTotalComplete(string date) {
        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("SELECT * FROM Scheduler WHERE ScheduleDate like '" + date + "%' AND Complete='true' AND ApplicationId='" + ServerSettings.ApplicationID + "'");
        return dbSelect.Count;
    }

    public void updateTimeSlot(string company, string newDate, string oldDate) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("Company", company));
        query.Add(new DatabaseQuery("ScheduleDate", oldDate));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ScheduleDate", newDate));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void updateCompany(string newComp, string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Company", newComp));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void updateTruckNum(string newtn, string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("TruckNum", newtn));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void updateDriver(string newDriver, string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DeliverFrom", newDriver));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void updateItems(string newItems, string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Items", newItems));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void updatePhoneNumber(string newPN, string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("PhoneNumber", newPN));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void updateEmail(string newEmail, string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Email", newEmail));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void updateType(string newType, string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ScheduleType", newType));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void updateComment(string newComment, string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Comment", newComment));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void updateComplete(string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("Complete", "false"));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Comment", "true"));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public void deleteSlot(string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        if (dbCall.CallDelete(TableName, query)) {
            _uuf.addFlag("app-deliverypickups", "");
            _uuf.addFlag("workspace", "");
        }
    }

    public bool checkTimeSlot(string date) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query);
        return dbSelect.Count > 0;
    }

    public bool checkTimeSlot(string date, string type) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ScheduleType", type));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query);
        return dbSelect.Count > 0;
    }

    public bool checkTimeSlot(string date, bool completed) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("Complete", completed.ToString().ToLower()));
        query.Add(new DatabaseQuery("ScheduleDate", date));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query);
        return dbSelect.Count > 0;
    }

    private void updateSlots(Scheduler_Coll coll) {
        _schedulerColl.Add(coll);
    }
}