<%@ WebService Language="C#" Class="CTLReportService" %>
#region

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Data;
using System.Web;
using System.Web.Script.Services;
using System.Web.Security;
using System.Web.Services;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;

#endregion

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class CTLReportService : WebService 
{
    private const string AppId = "app-ctlreport";
    private readonly AppLog _applog = new AppLog(false);
    private readonly IIdentity _userId;
    private CTLReport _ctlReport;
    private readonly App _apps = new App();

    private string _username;
    private string _message;
    private string _group;

    public CTLReportService()
    {
        _userId = HttpContext.Current.User.Identity;
        _username = _userId.Name;
        _ctlReport = new CTLReport(_userId.Name);
    }

    [WebMethod]
    public object[] LoadReport(string date, string searchValue)
    {
        if ((string.IsNullOrEmpty(searchValue)) || (searchValue.ToLower() == "search for ctl report"))
            return BuildStandardList(date);
        else
            return SearchReports(searchValue);
    }

    [WebMethod]
    public object[] LoadReportEdit(string date)
    {
        return BuildStandardList(date);
    }

    [WebMethod]
    public string UpdateHeader(string date, string line, string docNumber, string revision, string approvedBy)
    {
        date = HttpUtility.UrlDecode(date);
        docNumber = HttpUtility.UrlDecode(docNumber);
        revision = HttpUtility.UrlDecode(revision);
        approvedBy = HttpUtility.UrlDecode(approvedBy);
        
        _ctlReport.UpdateHeader(date, line, docNumber, revision, approvedBy);
        return "";
    }
    
    [WebMethod]
    public string UpdateSequence(string ids)
    {
        string[] delim = { "," };
        string[] splitIds = ids.Split(delim, StringSplitOptions.RemoveEmptyEntries);

        int count = 1;
        foreach (string id in splitIds)
        {
            if (!_ctlReport.UpdateSequence(id, count))
                break;
            
            count++;
        }

        return "";
    }

    [WebMethod]
    public string AddNewLine(string line, string sequence, string gauge, string material, string width, string coilNumber, string coilWeight, string customer, string orderDate, string weight, string orderNumber, string date, string docNumber, string revision, string approvedBy)
    {
        gauge = HttpUtility.UrlDecode(gauge);
        material = HttpUtility.UrlDecode(material);
        width = HttpUtility.UrlDecode(width);
        coilNumber = HttpUtility.UrlDecode(coilNumber);
        coilWeight = HttpUtility.UrlDecode(coilWeight);
        customer = HttpUtility.UrlDecode(customer);
        orderDate = HttpUtility.UrlDecode(orderDate);
        weight = HttpUtility.UrlDecode(weight);
        orderNumber = HttpUtility.UrlDecode(orderNumber);
        date = HttpUtility.UrlDecode(date);
        docNumber = HttpUtility.UrlDecode(docNumber);
        revision = HttpUtility.UrlDecode(revision);
        approvedBy = HttpUtility.UrlDecode(approvedBy);

        if ((string.IsNullOrEmpty(approvedBy)) || (approvedBy.ToLower() == "approved by"))
            approvedBy = _username;
        else
        {
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser u in coll)
            {
                if (HelperMethods.MergeFMLNames(new MemberDatabase(u.UserName)).ToLower() == approvedBy.ToLower())
                {
                    approvedBy = u.UserName;
                    break;
                }
            }
        }
        
        _ctlReport.AddItem(line, sequence, gauge, material, width, coilNumber, coilWeight, customer, orderDate, weight, orderNumber, date, docNumber, revision, approvedBy);
        return "";
    }

    [WebMethod]
    public string UpdateRow(string id, string line, string gauge, string material, string width, string coilNumber, string coilWeight, string customer, string orderDate, string weight, string orderNumber, string date, string docNumber, string revision, string approvedBy)
    {
        id = HttpUtility.UrlDecode(id);
        gauge = HttpUtility.UrlDecode(gauge);
        material = HttpUtility.UrlDecode(material);
        width = HttpUtility.UrlDecode(width);
        coilNumber = HttpUtility.UrlDecode(coilNumber);
        coilWeight = HttpUtility.UrlDecode(coilWeight);
        customer = HttpUtility.UrlDecode(customer);
        orderDate = HttpUtility.UrlDecode(orderDate);
        weight = HttpUtility.UrlDecode(weight);
        orderNumber = HttpUtility.UrlDecode(orderNumber);
        date = HttpUtility.UrlDecode(date);
        docNumber = HttpUtility.UrlDecode(docNumber);
        revision = HttpUtility.UrlDecode(revision);
        approvedBy = HttpUtility.UrlDecode(approvedBy);

        if ((string.IsNullOrEmpty(approvedBy)) || (approvedBy.ToLower() == "approved by"))
            approvedBy = _username;
        else
        {
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser u in coll)
            {
                if (HelperMethods.MergeFMLNames(new MemberDatabase(u.UserName)).ToLower() == approvedBy.ToLower())
                {
                    approvedBy = u.UserName;
                    break;
                }
            }
        }
        
        _ctlReport.UpdateRow(id, line, gauge, material, width, coilNumber, coilWeight, customer, orderDate, weight, orderNumber, date, docNumber, revision, approvedBy); 
        return "";
    }

    [WebMethod]
    public string UpdateRowSearch(string id, string line, string gauge, string material, string width, string coilNumber, string coilWeight, string customer, string orderDate, string weight, string orderNumber, string date, string approvedBy)
    {
        id = HttpUtility.UrlDecode(id);
        gauge = HttpUtility.UrlDecode(gauge);
        material = HttpUtility.UrlDecode(material);
        width = HttpUtility.UrlDecode(width);
        coilNumber = HttpUtility.UrlDecode(coilNumber);
        coilWeight = HttpUtility.UrlDecode(coilWeight);
        customer = HttpUtility.UrlDecode(customer);
        orderDate = HttpUtility.UrlDecode(orderDate);
        weight = HttpUtility.UrlDecode(weight);
        orderNumber = HttpUtility.UrlDecode(orderNumber);
        date = HttpUtility.UrlDecode(date);
        approvedBy = HttpUtility.UrlDecode(approvedBy);

        if ((string.IsNullOrEmpty(approvedBy)) || (approvedBy.ToLower() == "approved by"))
            approvedBy = _username;
        else
        {
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser u in coll)
            {
                if (HelperMethods.MergeFMLNames(new MemberDatabase(u.UserName)).ToLower() == approvedBy.ToLower())
                {
                    approvedBy = u.UserName;
                    break;
                }
            }
        }

        _ctlReport.UpdateRow(id, line, gauge, material, width, coilNumber, coilWeight, customer, orderDate, weight, orderNumber, date, approvedBy);
        return "";
    }

    [WebMethod]
    public string DeleteRow(string id, string date, string line)
    {
        id = HttpUtility.UrlDecode(id);
        
        _ctlReport.DeleteRow(id);

        int count = 1;
        _ctlReport.BuildEntriesByReportDateAndLine(date, line, "Sequence", "ASC");
        foreach (var x in _ctlReport.CTLReportCollection)
        {
            if (!_ctlReport.UpdateSequence(x.ID, count))
                break;

            count++;
        }
        
        return "";
    }

    [WebMethod]
    public string ExportToExcel(string _date, string _line, string _exportAll)
    {
        string _Path = string.Empty;
        if (HelperMethods.ConvertBitToBoolean(_exportAll))
        {
            _Path = "CTLSchedule_Line-" + DateTime.Now.ToShortDateString().Replace("/", "_") + ".xls";
            _ctlReport.BuildEntriesByReportDate(_date, "Sequence", "ASC");
        }
        else
        {
            _Path = "CTLSchedule_Line_" + _line + "-" + DateTime.Now.ToShortDateString().Replace("/", "_") + ".xls";
            _ctlReport.BuildEntriesByReportDateAndLine(_date, _line, "DateEntered", "ASC");
        }

        string directory = ServerSettings.GetServerMapLocation + "Apps\\CTLReport\\Exports";
        string p = Path.Combine(directory, _Path);
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }
        
        var temp = new DataTable();
        try
        {
            if (_ctlReport.CTLReportCollection.Count == 0)
            {
                return "";
            }
            else
            {
                temp.Columns.Add(new DataColumn("Line"));
                temp.Columns.Add(new DataColumn("Gauge"));
                temp.Columns.Add(new DataColumn("Material"));
                temp.Columns.Add(new DataColumn("Width"));
                temp.Columns.Add(new DataColumn("Coil Number"));
                temp.Columns.Add(new DataColumn("Coil Weight"));
                temp.Columns.Add(new DataColumn("Customer"));
                temp.Columns.Add(new DataColumn("Order Date"));
                temp.Columns.Add(new DataColumn("Weight"));
                temp.Columns.Add(new DataColumn("Order Number"));
                temp.Columns.Add(new DataColumn("Report Date"));
                temp.Columns.Add(new DataColumn("Approved By"));

                foreach (CTLReport_Coll report in _ctlReport.CTLReportCollection)
                {
                    DataRow drsch = temp.NewRow();
                    drsch["Line"] = report.Line;
                    drsch["Gauge"] = report.Gauge;
                    drsch["Material"] = report.Material;
                    drsch["Width"] = report.Width;
                    drsch["Coil Number"] = report.CoilNumber;
                    drsch["Coil Weight"] = report.CoilWeight;
                    drsch["Customer"] = report.Customer;
                    drsch["Order Date"] = report.OrderDate.ToShortDateString();
                    drsch["Weight"] = report.Weight;
                    drsch["Order Number"] = report.OrderNumber;
                    drsch["Report Date"] = report.ReportDate.ToShortDateString();

                    MemberDatabase m = new MemberDatabase(report.ApprovedBy);
                    string fullName = HelperMethods.MergeFMLNames(m);
                    if (!string.IsNullOrEmpty(fullName))
                        drsch["Approved By"] = fullName;
                    else
                        drsch["Approved By"] = report.ApprovedBy;
                    

                    temp.Rows.Add(drsch);
                }
            }

            var stringWriter = new StringWriter();
            var htmlWrite = new System.Web.UI.HtmlTextWriter(stringWriter);
            var DataGrd = new DataGrid();
            DataGrd.DataSource = temp;
            DataGrd.DataBind();

            DataGrd.RenderControl(htmlWrite);

            try {
                if (File.Exists(p)) {
                    File.Delete(p);
                }
            }
            catch { }

            var vw = new StreamWriter(p, true);
            stringWriter.ToString().Normalize();
            vw.Write(stringWriter.ToString());
            vw.Flush();
            vw.Close();

            return "Apps/CTLReport/Exports/" + _Path;
        }
        catch (Exception ex)
        {
            return "";
        }

        return "";
    }
    
    private object[] BuildStandardList(string date)
    {
        List<object> objectHolderMain = new List<object>();

        _ctlReport.BuildEntriesByReportDate(date, "Sequence", "ASC");
        foreach (var x in _ctlReport.CTLReportCollection)
        {
            object[] _obj = new object[8];
            _obj[0] = x.ID;
            _obj[1] = x.Sequence;
            _obj[2] = x.CoilNumber;
            _obj[3] = x.Gauge;
            _obj[4] = x.Material;
            _obj[5] = x.Width;
            _obj[6] = x.Line;
            MemberDatabase m = new MemberDatabase(x.ApprovedBy);
            string fullName = HelperMethods.MergeFMLNames(m);
            if (!string.IsNullOrEmpty(fullName))
                _obj[7] = fullName;
            else
                _obj[7] = x.ApprovedBy;
            objectHolderMain.Add(_obj);
        }

        return objectHolderMain.ToArray();
    }

    private object[] SearchReports(string searchValue)
    {
        List<object> objectHolderMain = new List<object>();
        _ctlReport.BuildEntriesAll("ReportDate", "ASC");
        if (searchValue.Contains(": "))
            searchValue = searchValue.Substring(searchValue.IndexOf(": ") + (": ").Length);
        if (searchValue.Contains("- "))
            searchValue = searchValue.Substring(searchValue.IndexOf("- ") + ("- ").Length);
        foreach (var x in _ctlReport.CTLReportCollection)
        {
            bool canContinue = false;
            if (x.Line.ToLower().Contains(searchValue.ToLower()))
                canContinue = true;
            else if (x.Customer.ToLower().Contains(searchValue.ToLower()))
                canContinue = true;
            else if (x.CoilNumber.ToLower().Contains(searchValue.ToLower()))
                canContinue = true;
            else if (x.Material.ToLower().Contains(searchValue.ToLower()))
                canContinue = true;
            else if (x.OrderNumber.ToLower().Contains(searchValue.ToLower()))
                canContinue = true;
            else if (x.OrderDate.ToShortDateString().ToLower().Contains(searchValue.ToLower()))
                canContinue = true;
            else if (x.Gauge.ToLower().Contains(searchValue.ToLower()))
                canContinue = true;
            else if (x.ReportDate.ToShortDateString().ToLower().Contains(searchValue.ToLower()))
                canContinue = true;

            if (canContinue == true)
            {
                object[] _obj = new object[9];
                _obj[0] = x.ID;
                _obj[1] = x.Sequence;
                _obj[2] = x.CoilNumber;
                _obj[3] = x.Gauge;
                _obj[4] = x.Material;
                _obj[5] = x.Width;
                _obj[6] = x.Line;
                
                MemberDatabase m = new MemberDatabase(x.ApprovedBy);
                string fullName = HelperMethods.MergeFMLNames(m);
                if (!string.IsNullOrEmpty(fullName))
                    _obj[7] = fullName;
                else
                    _obj[7] = x.ApprovedBy;

                
                _obj[8] = x.ReportDate.ToShortDateString();

                objectHolderMain.Add(_obj);
            }
        }

        return objectHolderMain.ToArray();
    }

    private string removeRegex(string name)
    {
        string fnew1 = name.Replace("'", "");
        string fnew1_temp = fnew1;
        fnew1 = fnew1_temp.Replace("&", "and");
        string fnew2_temp = fnew1;
        fnew1 = fnew2_temp.Replace("%", "");
        string fnew3_temp = fnew1;
        fnew1 = fnew3_temp.Replace(">", "");
        string fnew4_temp = fnew1;
        fnew1 = fnew4_temp.Replace("<", "");
        string fnew5_temp = fnew1;
        fnew1 = fnew5_temp.Replace("/", "");
        string fnew6_temp = fnew1;
        fnew1 = fnew6_temp.Replace(" ", "_");
        string fnew7_temp = fnew1;
        fnew1 = System.Text.RegularExpressions.Regex.Replace(fnew7_temp, @"<(.|\n)*?>", string.Empty);
        return fnew1;
    }

    private string addWhiteSpace(string x)
    {
        string ret = x.Replace(",", ", ");
        ret = ret.Replace(".", ", ");
        return ret;
    }
}