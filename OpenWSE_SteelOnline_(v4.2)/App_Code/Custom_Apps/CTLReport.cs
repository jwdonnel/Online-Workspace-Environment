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
public class CTLReport_Coll {
    private string _id;
    private int _sequence = 1;
    private string _line;
    private string _gauge;
    private string _material;
    private string _width;
    private string _coilNumber;
    private string _coilWeight;
    private string _customer;
    private DateTime _orderDate = new DateTime();
    private string _weight;
    private string _orderNumber;
    private DateTime _reportDate = new DateTime();
    private DateTime _dateEntered = new DateTime();
    private string _docNumber;
    private string _revision;
    private string _approvedBy;

    public CTLReport_Coll(string id, string sequence, string line, string gauge, string material, string width, string coilNumber, string coilWeight, string customer, string orderDate, string weight, string orderNumber, string reportDate, string dateEntered, string docNumber, string revision, string approvedBy) {
        _id = id;
        int.TryParse(sequence, out _sequence);
        _line = line;
        _gauge = gauge;
        _material = material;
        _width = width;
        _coilNumber = coilNumber;
        _coilWeight = coilWeight;
        _customer = customer;
        DateTime.TryParse(orderDate, out _orderDate);
        _weight = weight;
        _orderNumber = orderNumber;
        DateTime.TryParse(reportDate, out _reportDate);
        DateTime.TryParse(dateEntered, out _dateEntered);
        _docNumber = docNumber;
        _revision = revision;
        _approvedBy = approvedBy;
    }

    public string ID {
        get { return _id; }
    }

    public int Sequence {
        get { return _sequence; }
    }

    public string Line {
        get { return _line; }
    }

    public string Gauge {
        get { return _gauge; }
    }

    public string Material {
        get { return _material; }
    }

    public string Width {
        get { return _width; }
    }

    public string CoilNumber {
        get { return _coilNumber; }
    }

    public string CoilWeight {
        get { return _coilWeight; }
    }

    public string Customer {
        get { return _customer; }
    }

    public DateTime OrderDate {
        get { return _orderDate; }
    }

    public string Weight {
        get { return _weight; }
    }

    public string OrderNumber {
        get { return _orderNumber; }
    }

    public DateTime ReportDate {
        get { return _reportDate; }
    }

    public DateTime DateEntered {
        get { return _dateEntered; }
    }

    public string DocNumber {
        get { return _docNumber; }
    }

    public string Revision {
        get { return _revision; }
    }

    public string ApprovedBy {
        get { return _approvedBy; }
    }
}


public class CTLReport {
    private const string AppId = "app-ctloverview";
    private readonly AppLog _applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private List<CTLReport_Coll> _ctlColl = new List<CTLReport_Coll>();
    private const string TableName = "CTLReport";
    private string _userName;

    public CTLReport(string userName) {
        _ctlColl.Clear();
        _userName = userName;
    }

    public void AddItem(string line, string sequence, string gauge, string material, string width, string coilNumber, string coilWeight, string customer, string orderDate, string weight, string orderNumber, string reportDate, string docNumber, string revision, string approvedBy) {
        DateTime _orderDate = DateTime.Now;
        DateTime.TryParse(orderDate, out _orderDate);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("Sequence", sequence));
        query.Add(new DatabaseQuery("Line", line));
        query.Add(new DatabaseQuery("Gauge", gauge));
        query.Add(new DatabaseQuery("Material", material));
        query.Add(new DatabaseQuery("Width", width));
        query.Add(new DatabaseQuery("CoilNumber", coilNumber));
        query.Add(new DatabaseQuery("CoilWeight", coilWeight));
        query.Add(new DatabaseQuery("Customer", customer));
        query.Add(new DatabaseQuery("OrderDate", _orderDate.ToShortDateString()));
        query.Add(new DatabaseQuery("Weight", weight));
        query.Add(new DatabaseQuery("OrderNumber", orderNumber));
        query.Add(new DatabaseQuery("ReportDate", reportDate));
        query.Add(new DatabaseQuery("DateEntered", DateTime.Now.ToString()));
        query.Add(new DatabaseQuery("DocNumber", docNumber));
        query.Add(new DatabaseQuery("Revision", revision));
        query.Add(new DatabaseQuery("ApprovedBy", approvedBy));

        if (dbCall.CallInsert(TableName, query)) {
            _uuf.addFlag("workspace", "");
        }
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
            string sequence = row["Sequence"];
            string line = row["Line"];
            string gauge = row["Gauge"];
            string material = row["Material"];
            string width = row["Width"];
            string coilNumber = row["CoilNumber"];
            string coilWeight = row["CoilWeight"];
            string customer = row["Customer"];
            string orderDate = row["OrderDate"];
            string weight = row["Weight"];
            string orderNumber = row["OrderNumber"];
            string reportDate = row["ReportDate"];
            string dateEntered = row["DateEntered"];
            string docNumber = row["DocNumber"];
            string revision = row["Revision"];
            string approvedBy = row["ApprovedBy"];
            var coll = new CTLReport_Coll(id, sequence, line, gauge, material, width, coilNumber, coilWeight, customer, orderDate, weight, orderNumber, reportDate, dateEntered, docNumber, revision, approvedBy);
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
        query.Add(new DatabaseQuery("Line", _line));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query, orderBy);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string sequence = row["Sequence"];
            string line = row["Line"];
            string gauge = row["Gauge"];
            string material = row["Material"];
            string width = row["Width"];
            string coilNumber = row["CoilNumber"];
            string coilWeight = row["CoilWeight"];
            string customer = row["Customer"];
            string orderDate = row["OrderDate"];
            string weight = row["Weight"];
            string orderNumber = row["OrderNumber"];
            string reportDate = row["ReportDate"];
            string dateEntered = row["DateEntered"];
            string docNumber = row["DocNumber"];
            string revision = row["Revision"];
            string approvedBy = row["ApprovedBy"];
            var coll = new CTLReport_Coll(id, sequence, line, gauge, material, width, coilNumber, coilWeight, customer, orderDate, weight, orderNumber, reportDate, dateEntered, docNumber, revision, approvedBy);
            _ctlColl.Add(coll);
        }
    }

    public void BuildEntriesByReportDate(string _reportDate, string sortCol = "", string sortDir = "") {
        _ctlColl.Clear();

        string orderBy = "Customer ASC";
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "Date";
            if (sortDir == "undefined")
                sortDir = "DESC";

            orderBy = sortCol + " " + sortDir;
        }

        if ((string.IsNullOrEmpty(_reportDate)) || (_reportDate == "undefined"))
            _reportDate = DateTime.Now.ToShortDateString();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ReportDate", _reportDate));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query, orderBy);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string sequence = row["Sequence"];
            string line = row["Line"];
            string gauge = row["Gauge"];
            string material = row["Material"];
            string width = row["Width"];
            string coilNumber = row["CoilNumber"];
            string coilWeight = row["CoilWeight"];
            string customer = row["Customer"];
            string orderDate = row["OrderDate"];
            string weight = row["Weight"];
            string orderNumber = row["OrderNumber"];
            string reportDate = row["ReportDate"];
            string dateEntered = row["DateEntered"];
            string docNumber = row["DocNumber"];
            string revision = row["Revision"];
            string approvedBy = row["ApprovedBy"];
            var coll = new CTLReport_Coll(id, sequence, line, gauge, material, width, coilNumber, coilWeight, customer, orderDate, weight, orderNumber, reportDate, dateEntered, docNumber, revision, approvedBy);
            _ctlColl.Add(coll);
        }
    }

    public void BuildEntriesByReportDateAndLine(string _reportDate, string _line, string sortCol = "", string sortDir = "") {
        _ctlColl.Clear();

        string orderBy = "Customer ASC";
        if ((sortCol != "") && (sortDir != "")) {
            if (sortCol == "undefined")
                sortCol = "Date";
            if (sortDir == "undefined")
                sortDir = "DESC";

            orderBy = sortCol + " " + sortDir;
        }

        if ((string.IsNullOrEmpty(_reportDate)) || (_reportDate == "undefined"))
            _reportDate = DateTime.Now.ToShortDateString();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ReportDate", _reportDate));
        query.Add(new DatabaseQuery("Line", _line));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", query, orderBy);
        foreach (Dictionary<string, string> row in dbSelect) {
            string id = row["ID"];
            string sequence = row["Sequence"];
            string line = row["Line"];
            string gauge = row["Gauge"];
            string material = row["Material"];
            string width = row["Width"];
            string coilNumber = row["CoilNumber"];
            string coilWeight = row["CoilWeight"];
            string customer = row["Customer"];
            string orderDate = row["OrderDate"];
            string weight = row["Weight"];
            string orderNumber = row["OrderNumber"];
            string reportDate = row["ReportDate"];
            string dateEntered = row["DateEntered"];
            string docNumber = row["DocNumber"];
            string revision = row["Revision"];
            string approvedBy = row["ApprovedBy"];
            var coll = new CTLReport_Coll(id, sequence, line, gauge, material, width, coilNumber, coilWeight, customer, orderDate, weight, orderNumber, reportDate, dateEntered, docNumber, revision, approvedBy);
            _ctlColl.Add(coll);
        }
    }

    public object[] GetHeaderInfo(string _reportDate, string _line) {
        object[] obj = new object[3];

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ReportDate", _reportDate));
        query.Add(new DatabaseQuery("Line", _line));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "DocNumber, Revision, ApprovedBy", query);
        foreach (Dictionary<string, string> row in dbSelect) {
            string docNumber = row["DocNumber"];
            string revision = row["Revision"];
            string approvedBy = row["ApprovedBy"];
            if (!string.IsNullOrEmpty(approvedBy)) {
                obj[0] = docNumber;
                obj[1] = revision;
                obj[2] = approvedBy;
                break;
            }
        }

        return obj;
    }

    public void UpdateRow(string id, string line, string gauge, string material, string width, string coilNumber, string coilWeight, string customer, string orderDate, string weight, string orderNumber, string reportDate, string approvedBy) {
        DateTime _orderDate = DateTime.Now;
        DateTime.TryParse(orderDate, out _orderDate);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Line", line));
        updateQuery.Add(new DatabaseQuery("Gauge", gauge));
        updateQuery.Add(new DatabaseQuery("Material", material));
        updateQuery.Add(new DatabaseQuery("Width", width));
        updateQuery.Add(new DatabaseQuery("CoilNumber", coilNumber));
        updateQuery.Add(new DatabaseQuery("CoilWeight", coilWeight));
        updateQuery.Add(new DatabaseQuery("Customer", customer));
        updateQuery.Add(new DatabaseQuery("OrderDate", _orderDate.ToShortDateString()));
        updateQuery.Add(new DatabaseQuery("Weight", weight));
        updateQuery.Add(new DatabaseQuery("OrderNumber", orderNumber));
        updateQuery.Add(new DatabaseQuery("ReportDate", reportDate));
        updateQuery.Add(new DatabaseQuery("ApprovedBy", approvedBy));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("workspace", "");
        }
    }

    public void UpdateRow(string id, string line, string gauge, string material, string width, string coilNumber, string coilWeight, string customer, string orderDate, string weight, string orderNumber, string reportDate, string docNumber, string revision, string approvedBy) {
        DateTime _orderDate = DateTime.Now;
        DateTime.TryParse(orderDate, out _orderDate);

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Line", line));
        updateQuery.Add(new DatabaseQuery("Gauge", gauge));
        updateQuery.Add(new DatabaseQuery("Material", material));
        updateQuery.Add(new DatabaseQuery("Width", width));
        updateQuery.Add(new DatabaseQuery("CoilNumber", coilNumber));
        updateQuery.Add(new DatabaseQuery("CoilWeight", coilWeight));
        updateQuery.Add(new DatabaseQuery("Customer", customer));
        updateQuery.Add(new DatabaseQuery("OrderDate", _orderDate.ToShortDateString()));
        updateQuery.Add(new DatabaseQuery("Weight", weight));
        updateQuery.Add(new DatabaseQuery("OrderNumber", orderNumber));
        updateQuery.Add(new DatabaseQuery("ReportDate", reportDate));
        updateQuery.Add(new DatabaseQuery("DocNumber", docNumber));
        updateQuery.Add(new DatabaseQuery("Revision", revision));
        updateQuery.Add(new DatabaseQuery("ApprovedBy", approvedBy));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("workspace", "");
        }
    }

    public bool UpdateSequence(string id, int sequence) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Sequence", sequence.ToString()));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("workspace", "");
            return true;
        }

        return false;
    }

    public void UpdateHeader(string date, string line, string docNumber, string revision, string approvedBy) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ReportDate", date));
        query.Add(new DatabaseQuery("Line", line));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("DocNumber", docNumber));
        updateQuery.Add(new DatabaseQuery("Revision", revision));
        updateQuery.Add(new DatabaseQuery("ApprovedBy", approvedBy));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            _uuf.addFlag("workspace", "");
        }
    }

    public void DeleteRow(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        if (dbCall.CallDelete(TableName, query)) {
            _uuf.addFlag("workspace", "");
            _uuf.addFlag("app-ctloverview", "");
        }

        CTLLogs ctlLogs = new CTLLogs(_userName);
        ctlLogs.DeleteRowByScheduleID(id);
    }

    public List<CTLReport_Coll> CTLReportCollection {
        get { return _ctlColl; }
    }
}