#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Configuration;
using OpenWSE_Tools.AutoUpdates;

#endregion

[Serializable]
public struct plt_coll {
    private readonly string _burntable;
    private readonly string _clt_line1;
    private readonly string _clt_line2;
    private readonly string _dateupdated;
    private readonly string _half;
    private readonly Guid _id;
    private readonly string _quart;
    private readonly string _updatedby;

    public plt_coll(Guid id, string line1, string line2, string quart, string half, string burntable, string dateupdated,
                    string updatedby) {
        _id = id;
        _clt_line1 = line1;
        _clt_line2 = line2;
        _quart = quart;
        _half = half;
        _burntable = burntable;
        _dateupdated = dateupdated;
        _updatedby = updatedby;
    }

    public Guid ID {
        get { return _id; }
    }

    public string CTL_Line1 {
        get { return _clt_line1; }
    }

    public string CTL_Line2 {
        get { return _clt_line2; }
    }

    public string Quarter_Shear {
        get { return _quart; }
    }

    public string Half_Shear {
        get { return _half; }
    }

    public string BurnTable {
        get { return _burntable; }
    }

    public string DateUpdated {
        get { return _dateupdated; }
    }

    public string UpdatedBy {
        get { return _updatedby; }
    }
}

/// <summary>
///     Summary description for plt
/// </summary>
[Serializable]
public class plt {
    private readonly List<plt_coll> _plt_coll = new List<plt_coll>();
    private readonly AppLog applog = new AppLog(false);
    private readonly DatabaseCall dbCall = new DatabaseCall();
    private readonly string username;
    private const string TableName = "ProdLeadTime";
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private MemberDatabase member = new MemberDatabase();

    public plt(bool getvalues) {
        IIdentity userID = HttpContext.Current.User.Identity;
        username = userID.Name;
        if (getvalues) {
            _plt_coll.Clear();
            List<Dictionary<string, string>> dbSelect = dbCall.CallSelect(TableName, "", null);
            foreach (Dictionary<string, string> row in dbSelect) {
                var id = Guid.Parse(row["ID"]);
                string line1 = row["CTLLine1"];
                string line2 = row["CTLLine2"];
                string quart = row["Quart"];
                string half = row["Half"];
                string burntable = row["BurnTable"];
                string last_updated = row["DateUpdated"];
                string updatedby = row["UpdatedBy"];
                plt_coll coll = new plt_coll(id, line1, line2, quart, half, burntable, last_updated, updatedby);
                updateSlots(coll);
            }
        }
    }

    public List<plt_coll> pltcoll {
        get { return _plt_coll; }
    }

    public void addItem(string ctl1, string ctl2, string quart, string half, string burntable, string updated, string updated_by) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", Guid.NewGuid().ToString()));
        query.Add(new DatabaseQuery("CTLLine1", ctl1));
        query.Add(new DatabaseQuery("CTLLine2", ctl2));
        query.Add(new DatabaseQuery("Quart", quart));
        query.Add(new DatabaseQuery("Half", half));
        query.Add(new DatabaseQuery("BurnTable", burntable));
        query.Add(new DatabaseQuery("DateUpdated", updated));
        query.Add(new DatabaseQuery("UpdatedBy", updated_by));

        if (dbCall.CallInsert(TableName, query)) {
            uuf.addFlag("workspace", "");
        }
    }

    public void DeleteHistory() {
        dbCall.CallDelete(TableName, null);
    }

    public void DeleteHistory(string id) {
        List<DatabaseQuery> query = new List<DatabaseQuery>();
        query.Add(new DatabaseQuery("ID", id));

        dbCall.CallDelete(TableName, query);
    }

    public List<string> CalculatePLT() {
        try {
            var line1 = new List<int>();
            var line2 = new List<int>();
            var line3 = new List<int>();
            var line4 = new List<int>();
            var line5 = new List<int>();

            var dl1 = new DateTime();
            var dl2 = new DateTime();
            var dl3 = new DateTime();
            var dl4 = new DateTime();
            var dl5 = new DateTime();

            var final = new List<string>();
            foreach (var x in pltcoll) {
                DateTime dateuploaded = Convert.ToDateTime(x.DateUpdated);
                if (DateTime.TryParse(x.CTL_Line1, out dl1)) {
                    line1.Add(Math.Abs(dl1.Subtract(dateuploaded).Days));
                }

                if (DateTime.TryParse(x.CTL_Line2, out dl2)) {
                    line2.Add(Math.Abs(dl2.Subtract(dateuploaded).Days));
                }

                if (DateTime.TryParse(x.Quarter_Shear, out dl3)) {
                    line3.Add(Math.Abs(dl3.Subtract(dateuploaded).Days));
                }

                if (DateTime.TryParse(x.Half_Shear, out dl4)) {
                    line4.Add(Math.Abs(dl4.Subtract(dateuploaded).Days));
                }

                if (DateTime.TryParse(x.BurnTable, out dl5)) {
                    line5.Add(Math.Abs(dl5.Subtract(dateuploaded).Days));
                }
            }
            if (line1.Count > 0) {
                final.Add(DateTime.Now.AddDays(line1.Average()).ToShortDateString());
            }
            else {
                final.Add("Out of Service");
            }

            if (line2.Count > 0) {
                final.Add(DateTime.Now.AddDays(line2.Average()).ToShortDateString());
            }
            else {
                final.Add("Out of Service");
            }

            if (line3.Count > 0) {
                final.Add(DateTime.Now.AddDays(line3.Average()).ToShortDateString());
            }
            else {
                final.Add("Out of Service");
            }

            if (line4.Count > 0) {
                final.Add(DateTime.Now.AddDays(line4.Average()).ToShortDateString());
            }
            else {
                final.Add("Out of Service");
            }

            if (line5.Count > 0) {
                final.Add(DateTime.Now.AddDays(line5.Average()).ToShortDateString());
            }
            else {
                final.Add("Out of Service");
            }

            return final;
        }
        catch {
            return null;
        }
    }

    public void UpdateOverride1(bool canchange) {
        string x = "0";
        if (canchange) {
            x = "1";
        }

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Override1", x));

        dbCall.CallUpdate("aspnet_ServerSettings", updateQuery, null);
    }

    public void UpdateOverride2(bool canchange) {
        string x = "0";
        if (canchange) {
            x = "1";
        }

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Override2", x));

        dbCall.CallUpdate("aspnet_ServerSettings", updateQuery, null);
    }

    public void UpdateOverride3(bool canchange) {
        string x = "0";
        if (canchange) {
            x = "1";
        }

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Override3", x));

        dbCall.CallUpdate("aspnet_ServerSettings", updateQuery, null);
    }

    public void UpdateOverride4(bool canchange) {
        string x = "0";
        if (canchange) {
            x = "1";
        }

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Override4", x));

        dbCall.CallUpdate("aspnet_ServerSettings", updateQuery, null);
    }

    public void UpdateOverride5(bool canchange) {
        string x = "0";
        if (canchange) {
            x = "1";
        }

        List<DatabaseQuery> updateQuery = new List<DatabaseQuery>();
        updateQuery.Add(new DatabaseQuery("Override5", x));

        dbCall.CallUpdate("aspnet_ServerSettings", updateQuery, null);
    }

    public bool GetOverride(string or_number) {
        bool canchange = false;

        List<Dictionary<string, string>> dbSelect = dbCall.CallSelect("aspnet_ServerSettings", "", null);
        foreach (Dictionary<string, string> row in dbSelect) {
            if (HelperMethods.ConvertBitToBoolean(row[or_number])) {
                return true;
            }
        }

        return canchange;
    }

    private void updateSlots(plt_coll coll) {
        _plt_coll.Add(coll);
    }
}