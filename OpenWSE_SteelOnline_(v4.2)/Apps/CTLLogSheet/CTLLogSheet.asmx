<%@ WebService Language="C#" Class="CTLLogSheet" %>
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
public class CTLLogSheet : WebService 
{
    private const string AppId = "app-ctllogsheet";
    private readonly AppLog _applog = new AppLog(false);
    private readonly IIdentity _userId;
    private CTLReport _ctlReport;
    private CTLLogs _ctlLogs;
    private readonly App _apps = new App();

    private string _username;
    private string _message;
    private string _group;

    public CTLLogSheet()
    {
        _userId = HttpContext.Current.User.Identity;
        _username = _userId.Name;
        _ctlReport = new CTLReport(_userId.Name);
        _ctlLogs = new CTLLogs(_userId.Name);
    }

    [WebMethod]
    public object[] LoadReport(string date, string line)
    {
        List<object> objectHolderMain = new List<object>();
        List<object> objectHolder1 = new List<object>();
        List<object> objectHolder2 = new List<object>();
        List<object> objectHolder3 = new List<object>();
        
        string employee = string.Empty;
        string shift = string.Empty;

        _ctlReport.BuildEntriesByReportDateAndLine(date, line, "Sequence", "ASC");
        foreach (var x in _ctlReport.CTLReportCollection)
        {
            List<object> _obj = new List<object>();
            _obj.Add(x.ID);
            _obj.Add(x.CoilNumber);
            _obj.Add(x.Customer);

            CTLLogs_Coll coll = _ctlLogs.GetEntry(x.ID);
            if (coll != null)
            {
                _obj.Add(coll.ID);
                _obj.Add(coll.MaterialUsed_PCS);
                _obj.Add(coll.MaterialUsed_Size);
                _obj.Add(coll.MicNumber);
                _obj.Add(coll.HeatNumber);
                _obj.Add(coll.SONumber);
                _obj.Add(coll.SizeProduced);
                _obj.Add(coll.TotalPieces.ToString());
                _obj.Add(coll.CoilWeightRestock);

                if (string.IsNullOrEmpty(employee))
                {
                    employee = coll.Employee;
                    MemberDatabase mEmployee = new MemberDatabase(employee);
                    string fullName = HelperMethods.MergeFMLNames(mEmployee);
                    if (!string.IsNullOrEmpty(fullName))
                        employee = fullName;
                }

                if (string.IsNullOrEmpty(shift))
                    shift = coll.Shift;
            }
            
            objectHolder1.Add(_obj.ToArray());
        }

        object[] header = _ctlReport.GetHeaderInfo(date, line);
        if (header.Length == 3)
        {
            if (header[0] == null)
                objectHolder2.Add("SF-07-05");
            else
                objectHolder2.Add(header[0].ToString());

            if (header[1] == null)
                objectHolder2.Add("00");
            else
                objectHolder2.Add(header[1].ToString());

            if (header[2] == null)
                objectHolder2.Add("Not Assigned");
            else
            {
                MemberDatabase m = new MemberDatabase(header[2].ToString());
                string fullName = HelperMethods.MergeFMLNames(m);
                if (!string.IsNullOrEmpty(fullName))
                    objectHolder2.Add(fullName);
                else
                    objectHolder2.Add(header[2].ToString());
            }
        }
        else
        {
            objectHolder2.Add("SF-07-05");
            objectHolder2.Add("00");
            objectHolder2.Add("Not Assigned");
        }

        objectHolder3.Add(employee);
        objectHolder3.Add(shift);
        
        objectHolderMain.Add(objectHolder1);
        objectHolderMain.Add(objectHolder2);
        objectHolderMain.Add(objectHolder3);
        return objectHolderMain.ToArray();
    }

    [WebMethod]
    public string SaveRow(string id, string line, string materialUsed_PCS, string materialUsed_Size, string micNumber, string heatNumber, string soNumber, string sizeProduced, string totalPieces, string restockWeight, string employee, string shift, string date)
    {
        employee = ConvertEmployeeName(employee);
        shift = HttpUtility.UrlDecode(shift);

        if ((!string.IsNullOrEmpty(employee)) && (!string.IsNullOrEmpty(shift)) && (employee.ToLower() != "employee name") && (shift.ToLower() != "shift"))
        {
            id = HttpUtility.UrlDecode(id);
            line = HttpUtility.UrlDecode(line);
            materialUsed_PCS = HttpUtility.UrlDecode(materialUsed_PCS);
            materialUsed_Size = HttpUtility.UrlDecode(materialUsed_Size);
            micNumber = HttpUtility.UrlDecode(micNumber);
            heatNumber = HttpUtility.UrlDecode(heatNumber);
            soNumber = HttpUtility.UrlDecode(soNumber);
            sizeProduced = HttpUtility.UrlDecode(sizeProduced);
            totalPieces = HttpUtility.UrlDecode(totalPieces);
            restockWeight = HttpUtility.UrlDecode(restockWeight);
            date = HttpUtility.UrlDecode(date);

            _ctlLogs.AddItem(id, materialUsed_PCS, materialUsed_Size, micNumber, heatNumber, soNumber, line, sizeProduced, totalPieces, restockWeight, employee, shift, date);
            return "";
        }

        return "false";
    }

    [WebMethod]
    public string UpdateRow(string id, string schId, string line, string materialUsed_PCS, string materialUsed_Size, string micNumber, string heatNumber, string soNumber, string sizeProduced, string totalPieces, string restockWeight, string employee, string shift, string date)
    {
        employee = ConvertEmployeeName(employee);
        shift = HttpUtility.UrlDecode(shift);

        if ((!string.IsNullOrEmpty(employee)) && (!string.IsNullOrEmpty(shift)) && (employee.ToLower() != "employee name") && (shift.ToLower() != "shift"))
        {
            id = HttpUtility.UrlDecode(id);
            schId = HttpUtility.UrlDecode(schId);
            line = HttpUtility.UrlDecode(line);
            materialUsed_PCS = HttpUtility.UrlDecode(materialUsed_PCS);
            materialUsed_Size = HttpUtility.UrlDecode(materialUsed_Size);
            micNumber = HttpUtility.UrlDecode(micNumber);
            heatNumber = HttpUtility.UrlDecode(heatNumber);
            soNumber = HttpUtility.UrlDecode(soNumber);
            sizeProduced = HttpUtility.UrlDecode(sizeProduced);
            totalPieces = HttpUtility.UrlDecode(totalPieces);
            restockWeight = HttpUtility.UrlDecode(restockWeight);
            date = HttpUtility.UrlDecode(date);

            _ctlLogs.UpdateRow(id, schId, materialUsed_PCS, materialUsed_Size, micNumber, heatNumber, soNumber, line, sizeProduced, totalPieces, restockWeight, employee, shift, date);
            return "";
        }

        return "false";
    }

    [WebMethod]
    public string UpdateHeader(string date, string line, string employee, string shift)
    {
        employee = ConvertEmployeeName(employee);
        shift = HttpUtility.UrlDecode(shift);

        if ((!string.IsNullOrEmpty(employee)) && (!string.IsNullOrEmpty(shift)) && (employee.ToLower() != "employee name") && (shift.ToLower() != "shift"))
        {
            line = HttpUtility.UrlDecode(line);
            date = HttpUtility.UrlDecode(date);

            _ctlLogs.UpdateRowHeader(date, line, employee, shift);
            return "";
        }

        return "false";
    }
    
    [WebMethod]
    public string ExportToExcel(string _date, string _line)
    {
        string _Path = "CTLLog_Line_" + _line + "-" + DateTime.Now.ToShortDateString().Replace("/", "_") + ".xls";
        string directory = ServerSettings.GetServerMapLocation + "Apps\\CTLLogSheet\\Exports";
        string p = Path.Combine(directory, _Path);
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }
        
        var temp = new DataTable();
        try
        {
            _ctlReport.BuildEntriesByReportDateAndLine(_date, _line, "Sequence", "ASC");
            if (_ctlReport.CTLReportCollection.Count == 0)
            {
                return "";
            }
            else
            {
                temp.Columns.Add(new DataColumn("Coil Number"));
                temp.Columns.Add(new DataColumn("Line Number"));
                temp.Columns.Add(new DataColumn("Material Type"));
                temp.Columns.Add(new DataColumn("PCS"));
                temp.Columns.Add(new DataColumn("Material Size"));
                temp.Columns.Add(new DataColumn("Mic Number"));
                temp.Columns.Add(new DataColumn("Heat Number"));
                temp.Columns.Add(new DataColumn("Customer"));
                temp.Columns.Add(new DataColumn("Order Number"));
                temp.Columns.Add(new DataColumn("SO Number"));
                temp.Columns.Add(new DataColumn("Size Produced"));
                temp.Columns.Add(new DataColumn("Total Pieces"));
                temp.Columns.Add(new DataColumn("Employee"));
                temp.Columns.Add(new DataColumn("Shift"));
                temp.Columns.Add(new DataColumn("Report Date"));

                foreach (CTLReport_Coll report in _ctlReport.CTLReportCollection)
                {
                    CTLLogs_Coll coll = _ctlLogs.GetEntry(report.ID);
                    if (coll != null)
                    {
                        DataRow drsch = temp.NewRow();
                        drsch["Coil Number"] = report.CoilNumber;
                        drsch["Line Number"] = report.Line;
                        drsch["Material Type"] = report.Material;
                        drsch["PCS"] = coll.MaterialUsed_PCS;
                        drsch["Material Size"] = coll.MaterialUsed_Size;
                        drsch["Mic Number"] = coll.MicNumber;
                        drsch["Heat Number"] = coll.HeatNumber;
                        drsch["Customer"] = report.Customer;
                        drsch["Order Number"] = report.OrderNumber;
                        drsch["SO Number"] = coll.SONumber;
                        drsch["Size Produced"] = coll.SizeProduced;
                        drsch["Total Pieces"] = coll.TotalPieces.ToString();
                        
                        MemberDatabase m = new MemberDatabase(coll.Employee);
                        string fullName = HelperMethods.MergeFMLNames(m);
                        if (!string.IsNullOrEmpty(fullName))
                            drsch["Employee"] = fullName;
                        else
                            drsch["Employee"] = coll.Employee;

                        drsch["Shift"] = coll.Shift;
                        drsch["Report Date"] = coll.Date.ToShortDateString();
                        temp.Rows.Add(drsch);
                    }
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

            return "Apps/CTLLogSheet/Exports/" + _Path;
        }
        catch (Exception ex)
        {
            return "";
        }

        return "";
    }

    private string ConvertEmployeeName(string employee)
    {
        employee = HttpUtility.UrlDecode(employee);
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll)
        {
            if (HelperMethods.MergeFMLNames(new MemberDatabase(u.UserName)).ToLower() == employee.ToLower())
            {
                employee = u.UserName;
                break;
            }
        }

        return employee;
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