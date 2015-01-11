using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Summary description for SteelInventory
/// </summary>
public class SteelInventory {
    private readonly AppLog applog = new AppLog(false);
    private List<Dictionary<string, string>> dataTable;
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private const string TableName = "SteelInventory";

    public SteelInventory() { }

    public void BuildTable() {
        dataTable = dbCall.CallSelect(TableName, "", null, "Gauge ASC");
    }

    public void BuildTable(string type, string sortBy) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("Type", type));

        if (string.IsNullOrEmpty(sortBy)) {
            sortBy = "Type ASC, Grade ASC, Gauge ASC";
        }

        dataTable = dbCall.CallSelect(TableName, "", query, sortBy);
    }

    public void AddRow(string type, string grade, string gauge, string thickness, string width, string length, string minrun, string lbspersqft, string weightpersheet) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("Type", type.Trim()));
        query.Add(new DatabaseQuery("Grade", grade.Trim()));
        query.Add(new DatabaseQuery("Gauge", gauge.Trim()));
        query.Add(new DatabaseQuery("Thickness", thickness.Trim()));
        query.Add(new DatabaseQuery("Width", width.Trim()));
        query.Add(new DatabaseQuery("Length", length.Trim()));
        query.Add(new DatabaseQuery("MinRun", minrun.Trim()));
        query.Add(new DatabaseQuery("lBSperSQFT", lbspersqft.Trim()));
        query.Add(new DatabaseQuery("WeightPerSheet", weightpersheet.Trim()));

        dbCall.CallInsert(TableName, query);
    }

    public void UpdateRow(string id, string type, string grade, string gauge, string thickness, string width, string length, string minrun, string lbspersqft, string weightpersheet) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Type", type.Trim()));
        updateQuery.Add(new DatabaseQuery("Grade", grade.Trim()));
        updateQuery.Add(new DatabaseQuery("Gauge", gauge.Trim()));
        updateQuery.Add(new DatabaseQuery("Thickness", thickness.Trim()));
        updateQuery.Add(new DatabaseQuery("Width", width.Trim()));
        updateQuery.Add(new DatabaseQuery("Length", length.Trim()));
        updateQuery.Add(new DatabaseQuery("MinRun", minrun.Trim()));
        updateQuery.Add(new DatabaseQuery("lBSperSQFT", lbspersqft.Trim()));
        updateQuery.Add(new DatabaseQuery("WeightPerSheet", weightpersheet.Trim()));

        dbCall.CallUpdate(TableName, updateQuery, query);
    }

    public void DeleteRow(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        dbCall.CallDelete(TableName, query);
    }

    public List<Dictionary<string, string>> dtInv {
        get { return dataTable; }
    }
}