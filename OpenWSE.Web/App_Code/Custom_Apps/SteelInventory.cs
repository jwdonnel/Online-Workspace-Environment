using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;
using OpenWSE_Tools.AutoUpdates;

/// <summary>
/// Summary description for SteelInventory
/// </summary>
public class SteelInventory {

    private const string app_id = "app-steelinventory";
    private readonly AppLog applog = new AppLog(false);
    private List<Dictionary<string, string>> dataTable;
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private const string TableName = "SteelInventory";

    public SteelInventory() { }

    public void BuildTable() {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        dataTable = dbCall.CallSelect(TableName, "", query, "Type ASC");
    }

    public List<string> GetTypeList() {
        List<string> typeList = new List<string>();

        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "Type", query, "Type ASC");
        foreach (Dictionary<string, string> item in dbSelect) {
            if (!typeList.Contains(item["Type"])) {
                typeList.Add(item["Type"]);
            }
        }

        return typeList;
    }

    public void BuildTable(string type, string sortBy) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("Type", type));

        if (string.IsNullOrEmpty(sortBy)) {
            sortBy = "Type ASC";
        }

        dataTable = dbCall.CallSelect(TableName, "", query, sortBy);
    }

    public void AddRow(string type, string grade, string gauge, string thickness, string width, string length, string minrun, string lbspersqft, string weightpersheet, int quantity, string stockImage) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
        query.Add(new DatabaseQuery("Quantity", quantity.ToString()));
        query.Add(new DatabaseQuery("StockImage", stockImage.Trim()));

        if (dbCall.CallInsert(TableName, query)) {
            uuf.addFlag(app_id, "", false);
        }
    }

    public void UpdateRow(string id, string grade, string gauge, string thickness, string width, string length, string minrun, string lbspersqft, string weightpersheet, int quantity, string stockImage) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Grade", grade.Trim()));
        updateQuery.Add(new DatabaseQuery("Gauge", gauge.Trim()));
        updateQuery.Add(new DatabaseQuery("Thickness", thickness.Trim()));
        updateQuery.Add(new DatabaseQuery("Width", width.Trim()));
        updateQuery.Add(new DatabaseQuery("Length", length.Trim()));
        updateQuery.Add(new DatabaseQuery("MinRun", minrun.Trim()));
        updateQuery.Add(new DatabaseQuery("lBSperSQFT", lbspersqft.Trim()));
        updateQuery.Add(new DatabaseQuery("WeightPerSheet", weightpersheet.Trim()));
        updateQuery.Add(new DatabaseQuery("Quantity", quantity.ToString()));
        updateQuery.Add(new DatabaseQuery("StockImage", stockImage.Trim()));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            uuf.addFlag(app_id, "", false);
        }
    }
    public void UpdateRow(string id, string type, string grade, string gauge, string thickness, string width, string length, string minrun, string lbspersqft, string weightpersheet, int quantity, string stockImage) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
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
        updateQuery.Add(new DatabaseQuery("Quantity", quantity.ToString()));
        updateQuery.Add(new DatabaseQuery("StockImage", stockImage.Trim()));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            uuf.addFlag(app_id, "", false);
        }
    }

    public void UpdateQuantity(string id, int quantity) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Quantity", quantity.ToString()));

        if (dbCall.CallUpdate(TableName, updateQuery, query)) {
            uuf.addFlag(app_id, "", false);
        }
    }

    public void DeleteRow(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery(DatabaseCall.ApplicationIdString, ServerSettings.ApplicationID));
        query.Add(new DatabaseQuery("ID", id));

        if (dbCall.CallDelete(TableName, query)) {
            uuf.addFlag(app_id, "", false);
        }
    }

    public List<Dictionary<string, string>> dtInv {
        get { return dataTable; }
    }
}