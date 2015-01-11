using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Configuration;
using System.Data;
using OpenWSE_Tools.AutoUpdates;


[Serializable]
public class CTLLogs_Coll {
    private string _id;
    private string _schId;
    private string _material_pcs;
    private string _material_size;
    private string _micNumber;
    private string _heatNumber;
    private string _soNumber;
    private string _lineNumber;
    private string _sizeProduced;
    private string _totalPieces;
    private string _coilWeightRestock;
    private string _employee;
    private string _shift;
    private DateTime _date = new DateTime();
    private DateTime _dateEntered = new DateTime();

    public CTLLogs_Coll(string id, string schId, string material_pcs, string material_size, string micNumber, string heatNumber, string soNumber, string lineNumber, string sizeProduced, string totalPieces, string coilWeightRestock, string employee, string shift, string date, string dateEntered) {
        _id = id;
        _schId = schId;
        _material_pcs = material_pcs;
        _material_size = material_size;
        _micNumber = micNumber;
        _heatNumber = heatNumber;
        _soNumber = soNumber;
        _lineNumber = lineNumber;
        _sizeProduced = sizeProduced;
        _totalPieces = totalPieces;
        _coilWeightRestock = coilWeightRestock;
        _employee = employee;
        _shift = shift;
        DateTime.TryParse(date, out _date);
        DateTime.TryParse(dateEntered, out _dateEntered);
    }

    public string ID {
        get { return _id; }
    }

    public string ScheduleID {
        get { return _schId; }
    }

    public string MaterialUsed_PCS {
        get { return _material_pcs; }
    }

    public string MaterialUsed_Size {
        get { return _material_size; }
    }

    public string MicNumber {
        get { return _micNumber; }
    }

    public string HeatNumber {
        get { return _heatNumber; }
    }

    public string SONumber {
        get { return _soNumber; }
    }

    public string LineNumber {
        get { return _lineNumber; }
    }

    public string SizeProduced {
        get { return _sizeProduced; }
    }

    public int TotalPieces {
        get {
            int outInt = 0;
            int.TryParse(_totalPieces, out outInt);
            return outInt;
        }
    }

    public string CoilWeightRestock {
        get { return _coilWeightRestock; }
    }

    public string Employee {
        get { return _employee; }
    }

    public string Shift {
        get { return _shift; }
    }

    public DateTime Date {
        get { return _date; }
    }

    public DateTime DateEntered {
        get { return _dateEntered; }
    }
}


public class CTLLogs {
    private readonly AppLog _applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private List<CTLLogs_Coll> _ctlColl = new List<CTLLogs_Coll>();
    private string _userName;
    private const string TableName = "CTLLogSheet";

    public CTLLogs(string userName) {
        _ctlColl.Clear();
        _userName = userName;
    }

    public void AddItem(string schId, string materialUsed_PCS, string materialUsed_Size, string micNumber, string heatNumber, string soNumber, string lineNumber, string sizeProduced, string totalPieces, string coilWeightRestock, string employee, string shift, string date) {
        DateTime _date = DateTime.Now;
        DateTime.TryParse(date, out _date);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("ScheduleID", schId));
        query.Add(new DatabaseQuery("MaterialUsed_PCS", materialUsed_PCS));
        query.Add(new DatabaseQuery("MaterialUsed_Size", materialUsed_Size));
        query.Add(new DatabaseQuery("MicNumber", micNumber));
        query.Add(new DatabaseQuery("HeatNumber", heatNumber));
        query.Add(new DatabaseQuery("SONumber", soNumber));
        query.Add(new DatabaseQuery("LineNumber", lineNumber));
        query.Add(new DatabaseQuery("SizeProduced", sizeProduced));
        query.Add(new DatabaseQuery("TotalPieces", totalPieces));
        query.Add(new DatabaseQuery("CoilWeightRestock", coilWeightRestock));
        query.Add(new DatabaseQuery("Employee", employee));
        query.Add(new DatabaseQuery("Shift", shift));
        query.Add(new DatabaseQuery("Date", _date.ToShortDateString()));
        query.Add(new DatabaseQuery("DateEntered", DateTime.Now.ToString()));

        if (dbCall.CallInsert(TableName, query)) {
            _uuf.addFlag("app-ctloverview", "");
        }
    }

    public CTLLogs_Coll GetEntry(string scheduleID) {
        CTLLogs_Coll coll = null;

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ScheduleID", scheduleID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string schId = row["ScheduleID"];
            string materialUsed_PCS = row["MaterialUsed_PCS"];
            string materialUsed_Size = row["MaterialUsed_Size"];
            string micNumber = row["MicNumber"];
            string heatNumber = row["HeatNumber"];
            string soNumber = row["SONumber"];
            string lineNumber = row["LineNumber"];
            string sizeProduced = row["SizeProduced"];
            string totalPieces = row["TotalPieces"];
            string coilWeightRestock = row["CoilWeightRestock"];
            string employee = row["Employee"];
            string shift = row["Shift"];
            string date = row["Date"];
            string dateEntered = row["DateEntered"];
            coll = new CTLLogs_Coll(id, schId, materialUsed_PCS, materialUsed_Size, micNumber, heatNumber, soNumber, lineNumber, sizeProduced, totalPieces, coilWeightRestock, employee, shift, date, dateEntered);
            break;
        }

        return coll;
    }

    public void BuildEntriesAll(string sortCol = "", string sortDir = "") {
        _ctlColl.Clear();

        string orderBy = "Customer ASC";
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "Date";
            if (sortDir == "undefined")
                sortDir = "DESC";

            orderBy = sortCol + " " + sortDir;
        }

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", null, orderBy);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string schId = row["ScheduleID"];
            string materialUsed_PCS = row["MaterialUsed_PCS"];
            string materialUsed_Size = row["MaterialUsed_Size"];
            string micNumber = row["MicNumber"];
            string heatNumber = row["HeatNumber"];
            string soNumber = row["SONumber"];
            string lineNumber = row["LineNumber"];
            string sizeProduced = row["SizeProduced"];
            string totalPieces = row["TotalPieces"];
            string coilWeightRestock = row["CoilWeightRestock"];
            string employee = row["Employee"];
            string shift = row["Shift"];
            string date = row["Date"];
            string dateEntered = row["DateEntered"];
            var coll = new CTLLogs_Coll(id, schId, materialUsed_PCS, materialUsed_Size, micNumber, heatNumber, soNumber, lineNumber, sizeProduced, totalPieces, coilWeightRestock, employee, shift, date, dateEntered);
            _ctlColl.Add(coll);
        }
    }

    public void BuildEntriesByLine(string _line, string sortCol = "", string sortDir = "") {
        _ctlColl.Clear();

        string orderBy = "Customer ASC";
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "Date";
            if (sortDir == "undefined")
                sortDir = "DESC";

            orderBy = sortCol + " " + sortDir;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("LineNumber", _line));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query, orderBy);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string schId = row["ScheduleID"];
            string materialUsed_PCS = row["MaterialUsed_PCS"];
            string materialUsed_Size = row["MaterialUsed_Size"];
            string micNumber = row["MicNumber"];
            string heatNumber = row["HeatNumber"];
            string soNumber = row["SONumber"];
            string lineNumber = row["LineNumber"];
            string sizeProduced = row["SizeProduced"];
            string totalPieces = row["TotalPieces"];
            string coilWeightRestock = row["CoilWeightRestock"];
            string employee = row["Employee"];
            string shift = row["Shift"];
            string date = row["Date"];
            string dateEntered = row["DateEntered"];
            var coll = new CTLLogs_Coll(id, schId, materialUsed_PCS, materialUsed_Size, micNumber, heatNumber, soNumber, lineNumber, sizeProduced, totalPieces, coilWeightRestock, employee, shift, date, dateEntered);
            _ctlColl.Add(coll);
        }
    }

    public void BuildEntriesByEmployeeAndDate(string _employee, string _date, string sortCol = "", string sortDir = "") {
        _ctlColl.Clear();

        string orderBy = "Customer ASC";
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "Date";
            if (sortDir == "undefined")
                sortDir = "DESC";

            orderBy = sortCol + " " + sortDir;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("Employee", _employee));
        query.Add(new DatabaseQuery("Date", _date));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query, orderBy);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string schId = row["ScheduleID"];
            string materialUsed_PCS = row["MaterialUsed_PCS"];
            string materialUsed_Size = row["MaterialUsed_Size"];
            string micNumber = row["MicNumber"];
            string heatNumber = row["HeatNumber"];
            string soNumber = row["SONumber"];
            string lineNumber = row["LineNumber"];
            string sizeProduced = row["SizeProduced"];
            string totalPieces = row["TotalPieces"];
            string coilWeightRestock = row["CoilWeightRestock"];
            string employee = row["Employee"];
            string shift = row["Shift"];
            string date = row["Date"];
            string dateEntered = row["DateEntered"];
            var coll = new CTLLogs_Coll(id, schId, materialUsed_PCS, materialUsed_Size, micNumber, heatNumber, soNumber, lineNumber, sizeProduced, totalPieces, coilWeightRestock, employee, shift, date, dateEntered);
            _ctlColl.Add(coll);
        }
    }

    public void BuildEntriesByReportDateAndLine(string _Date, string _line, string sortCol = "", string sortDir = "") {
        _ctlColl.Clear();

        string orderBy = "Customer ASC";
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "Date";
            if (sortDir == "undefined")
                sortDir = "DESC";

            orderBy = sortCol + " " + sortDir;
        }

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("Date", _Date));
        query.Add(new DatabaseQuery("LineNumber", _line));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query, orderBy);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string schId = row["ScheduleID"];
            string materialUsed_PCS = row["MaterialUsed_PCS"];
            string materialUsed_Size = row["MaterialUsed_Size"];
            string micNumber = row["MicNumber"];
            string heatNumber = row["HeatNumber"];
            string soNumber = row["SONumber"];
            string lineNumber = row["LineNumber"];
            string sizeProduced = row["SizeProduced"];
            string totalPieces = row["TotalPieces"];
            string coilWeightRestock = row["CoilWeightRestock"];
            string employee = row["Employee"];
            string shift = row["Shift"];
            string date = row["Date"];
            string dateEntered = row["DateEntered"];
            var coll = new CTLLogs_Coll(id, schId, materialUsed_PCS, materialUsed_Size, micNumber, heatNumber, soNumber, lineNumber, sizeProduced, totalPieces, coilWeightRestock, employee, shift, date, dateEntered);
            _ctlColl.Add(coll);
        }
    }

    public void UpdateRow(string id, string scheduleId, string materialUsed_PCS, string materialUsed_Size, string micNumber, string heatNumber, string soNumber, string lineNumber, string sizeProduced, string totalPieces, string coilWeightRestock, string employee, string shift, string date) {
        DateTime _date = DateTime.Now;
        DateTime.TryParse(date, out _date);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("ScheduleID", scheduleId));
        updateQuery.Add(new DatabaseQuery("MaterialUsed_PCS", materialUsed_PCS));
        updateQuery.Add(new DatabaseQuery("MaterialUsed_Size", materialUsed_Size));
        updateQuery.Add(new DatabaseQuery("MicNumber", micNumber));
        updateQuery.Add(new DatabaseQuery("HeatNumber", heatNumber));
        updateQuery.Add(new DatabaseQuery("SONumber", soNumber));
        updateQuery.Add(new DatabaseQuery("LineNumber", lineNumber));
        updateQuery.Add(new DatabaseQuery("SizeProduced", sizeProduced));
        updateQuery.Add(new DatabaseQuery("TotalPieces", totalPieces));
        updateQuery.Add(new DatabaseQuery("CoilWeightRestock", coilWeightRestock));
        updateQuery.Add(new DatabaseQuery("Employee", employee));
        updateQuery.Add(new DatabaseQuery("Shift", shift));
        updateQuery.Add(new DatabaseQuery("Date", _date.ToShortDateString()));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-ctloverview", "");
        }
    }

    public void UpdateRowHeader(string date, string line, string employee, string shift) {
        DateTime _date = DateTime.Now;
        DateTime.TryParse(date, out _date);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("Date", _date.ToShortDateString()));
        query.Add(new DatabaseQuery("LineNumber", line));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Employee", employee));
        updateQuery.Add(new DatabaseQuery("Shift", shift));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("app-ctloverview", "");
        }
    }

    public void DeleteRow(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        if (dbCall.CallDelete(TableName, query)) {
            _uuf.addFlag("app-ctloverview", "");
        }
    }

    public void DeleteRowByScheduleID(string schId) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ScheduleID", schId));

        if (dbCall.CallDelete(TableName, query)) {
            _uuf.addFlag("app-ctloverview", "");
        }
    }

    public List<CTLLogs_Coll> CTLLogCollection {
        get { return _ctlColl; }
    }
}