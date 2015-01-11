<%@ WebService Language="C#" Class="AutoComplete_Custom" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections.Generic;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class AutoComplete_Custom  : System.Web.Services.WebService {

    public AutoComplete_Custom() {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }
    
    public string Strip(string text) {
        //Regex regex = new Regex(@"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", RegexOptions.Singleline);
        string temp = System.Text.RegularExpressions.Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
        string temp2 = temp.Replace("&nbsp;", "");
        string temp3 = temp2.Replace("\n", "");
        string temp4 = temp3.Replace("\t", "");
        return temp4;
    }

    [WebMethod]
    public string[] GetListOfSMWUnits(string prefixText, int count) {
        var temp = new List<string>();
        var ts = new TruckSchedule();
        if (string.IsNullOrEmpty(prefixText)) {
            foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
                if (!temp.Contains(t.Unit)) {
                    temp.Add(t.Unit);
                }
            }
        }
        else {
            foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
                if (!t.Unit.ToLower().Contains(prefixText.ToLower())) continue;
                if (!temp.Contains(t.Unit)) {
                    temp.Add(t.Unit);
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }

    
    [WebMethod]
    public string[] GetListOfSMWDrivers(string prefixText, int count) {
        var temp = new List<string>();
        var ts = new TruckSchedule();
        if (string.IsNullOrEmpty(prefixText)) {
            foreach (string t in ts.drivers_coll) {
                string driver = t;
                try {
                    driver = driver.Replace("_", " ");
                }
                catch {
                }
                if (!temp.Contains(driver)) {
                    temp.Add(driver);
                }
            }
        }
        else {
            foreach (string t in ts.drivers_coll) {
                string driver = t;
                try {
                    driver = driver.Replace("_", " ");
                }
                catch {
                }
                if (driver.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(driver)) {
                        temp.Add(driver);
                    }
                }
            }
        }

        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetTruckLinesCC(string prefixText, int count) {
        var temp = new List<string>();
        var ts = new TruckSchedule(true);
        if (!string.IsNullOrEmpty(prefixText)) {
            foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
                if (!t.TruckLine.ToLower().Contains(prefixText.ToLower())) continue;
                if (!temp.Contains(t.TruckLine)) {
                    temp.Add(t.TruckLine);
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetTruckSchedule(string prefixText, int count) {
        var temp = new List<string>();
        var ts = new TruckSchedule();
        if (!string.IsNullOrEmpty(prefixText)) {
            foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
                if (t.City.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.City)) {
                        temp.Add(t.City);
                    }
                }
                else if (t.CustomerName.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.CustomerName)) {
                        temp.Add(t.CustomerName);
                    }
                }
                else if (t.Date.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.Date)) {
                        temp.Add(t.Date);
                    }
                }
                else if (t.DriverName.Replace("_", " ").ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.DriverName.Replace("_", " "))) {
                        temp.Add(t.DriverName.Replace("_", " "));
                    }
                }
                else if (t.DriverName.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.DriverName)) {
                        temp.Add(t.DriverName.Replace("_", " "));
                    }
                }
                else if (t.GeneralDirection.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.GeneralDirection)) {
                        temp.Add(t.GeneralDirection);
                    }
                }
                else if (t.Unit.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.Unit)) {
                        temp.Add(t.Unit);
                    }
                }
                else if (t.Weight.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.Weight)) {
                        temp.Add(t.Weight);
                    }
                }
                else if (t.OrderNumber.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.OrderNumber)) {
                        temp.Add(t.OrderNumber);
                    }
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetListCustomersTS(string prefixText, int count) {
        var temp = new List<string>();
        var ts = new TruckSchedule();
        if (string.IsNullOrEmpty(prefixText)) {
            foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
                if (!temp.Contains(t.CustomerName)) {
                    temp.Add(t.CustomerName);
                }
            }
        }
        else {
            foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
                if (t.CustomerName.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.CustomerName)) {
                        temp.Add(t.CustomerName);
                    }
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }

    
    [WebMethod]
    public string[] GetListCityTS(string prefixText, int count) {
        var temp = new List<string>();
        var ts = new TruckSchedule();
        if (string.IsNullOrEmpty(prefixText)) {
            foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
                if (!temp.Contains(t.City)) {
                    temp.Add(t.City);
                }
            }
        }
        else {
            foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
                if (t.City.ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(t.City)) {
                        temp.Add(t.City);
                    }
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetSchedule(string prefixText, int count) {
        var schedule = new Scheduler();
        var temp = new List<string>();
        int cc = 0;
        foreach (var x in schedule.scheduler_coll) {
            if (x.Company.ToLower().Contains(prefixText.ToLower())) {
                if (!temp.Contains(x.Company)) {
                    temp.Add(x.Company);
                    cc++;
                }
            }
            else if (x.DeliveryFrom.ToLower().Contains(prefixText.ToLower())) {
                if (!temp.Contains(x.DeliveryFrom)) {
                    temp.Add(x.DeliveryFrom);
                    cc++;
                }
            }
            else if (x.Email.ToLower().Contains(prefixText.ToLower())) {
                if (!temp.Contains(x.Email)) {
                    temp.Add(x.Email);
                    cc++;
                }
            }
            else if (x.PhoneNumber.ToLower().Contains(prefixText.ToLower())) {
                if (!temp.Contains(x.PhoneNumber)) {
                    temp.Add(x.PhoneNumber);
                    cc++;
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetGauges(string prefixText, int count) {
        var temp = new List<string>();
        SteelInventory inv = new SteelInventory();
        if (!string.IsNullOrEmpty(prefixText)) {
            inv.BuildTable();
            for (int i = 0; i < inv.dtInv.Count; i++) {
                Dictionary<string, string> row = inv.dtInv[i];
                if (row["Gauge"].ToString().ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(row["Gauge"].ToString())) {
                        temp.Add(row["Gauge"].ToString());
                    }
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetWidths(string prefixText, int count) {
        var temp = new List<string>();
        SteelInventory inv = new SteelInventory();
        if (!string.IsNullOrEmpty(prefixText)) {
            inv.BuildTable();
            for (int i = 0; i < inv.dtInv.Count; i++) {
                Dictionary<string, string> row = inv.dtInv[i];
                if (row["Width"].ToString().ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(row["Width"].ToString())) {
                        temp.Add(row["Width"].ToString());
                    }
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetTypes(string prefixText, int count) {
        var temp = new List<string>();
        SteelInventory inv = new SteelInventory();
        if (!string.IsNullOrEmpty(prefixText)) {
            inv.BuildTable();
            for (int i = 0; i < inv.dtInv.Count; i++) {
                Dictionary<string, string> row = inv.dtInv[i];
                if (row["Type"].ToString().ToLower().Contains(prefixText.ToLower())) {
                    if (!temp.Contains(row["Type"].ToString())) {
                        temp.Add(row["Type"].ToString());
                    }
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }


    [WebMethod]
    public string[] GetCTLReports(string prefixText, int count) {
        var ctlReports = new CTLReport(HttpContext.Current.User.Identity.Name.ToLower());
        ctlReports.BuildEntriesAll();
        var temp = new List<string>();
        var temp2 = new List<string>();
        int cc = 0;
        foreach (var x in ctlReports.CTLReportCollection) {
            if (x.Line.ToLower().Contains(prefixText.ToLower())) {
                if (!temp2.Contains(x.Line)) {
                    temp.Add(x.ReportDate.ToShortDateString() + ": Line " + x.Line);
                    cc++;
                }
            }
            else if (x.Customer.ToLower().Contains(prefixText.ToLower())) {
                if (!temp2.Contains(x.Customer)) {
                    temp.Add(x.ReportDate.ToShortDateString() + ": Customer - " + x.Customer);
                    cc++;
                }
            }
            else if (x.CoilNumber.ToLower().Contains(prefixText.ToLower())) {
                if (!temp2.Contains(x.CoilNumber)) {
                    temp.Add(x.ReportDate.ToShortDateString() + ": Coil Number - " + x.CoilNumber);
                    cc++;
                }
            }
            else if (x.Material.ToLower().Contains(prefixText.ToLower())) {
                if (!temp2.Contains(x.Material)) {
                    temp.Add(x.ReportDate.ToShortDateString() + ": Material - " + x.Material);
                    cc++;
                }
            }
            else if (x.OrderNumber.ToLower().Contains(prefixText.ToLower())) {
                if (!temp2.Contains(x.OrderNumber)) {
                    temp.Add(x.ReportDate.ToShortDateString() + ": Order Number - " + x.OrderNumber);
                    cc++;
                }
            }
            else if (x.OrderDate.ToShortDateString().ToLower().Contains(prefixText.ToLower())) {
                if (!temp2.Contains(x.OrderDate.ToShortDateString())) {
                    temp.Add(x.ReportDate.ToShortDateString() + ": Order Date - " + x.OrderDate);
                    cc++;
                }
            }
            else if (x.Gauge.ToLower().Contains(prefixText.ToLower())) {
                if (!temp2.Contains(x.Gauge)) {
                    temp.Add(x.ReportDate.ToShortDateString() + ": Gauge - " + x.Gauge);
                    cc++;
                }
            }
            else if (x.ReportDate.ToShortDateString().ToLower().Contains(prefixText.ToLower())) {
                if (!temp2.Contains(x.ReportDate.ToShortDateString())) {
                    temp.Add("Report Date - " + x.ReportDate.ToShortDateString());
                    cc++;
                }
            }
        }
        temp.Sort();
        return temp.ToArray();
    }
}