using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Text;
using System.IO;
using System.Data;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;

public partial class Apps_CTLOverview_CTLOverview : System.Web.UI.Page
{
    #region Private Variables

    private const string AppId = "app-ctloverview";
    private readonly IPWatch _ipwatch = new IPWatch(true);
    private ServerSettings _ss = new ServerSettings();
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private AppInitializer _appInitializer;
    private CTLReport _ctlReport;
    private CTLLogs _ctlLogs;
    private MemberDatabase _member;
    private string _username;

    #endregion


    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/Default.aspx");

        _appInitializer = new AppInitializer(AppId, userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent)
        {
            _username = userId.Name;
            _member = _appInitializer.memberDatabase;
            _ctlReport = new CTLReport(userId.Name);
            _ctlLogs = new CTLLogs(userId.Name);
            if (!IsPostBack)
            {
                // Initialize all the scripts and style sheets
                _appInitializer.SetHeaderLabelImage(lbl_Title, img_Title);
                _appInitializer.LoadScripts_JS(false);
                _appInitializer.LoadScripts_CSS();

                AutoUpdateSystem aus = new AutoUpdateSystem(hf_DataUpdate_ctloverview.ClientID, AppId, this);
                aus.StartAutoUpdates();

                tb_date_ctloverview.Text = DateTime.Now.ToShortDateString();

                StartBuild();
            }
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }
    protected void hf_DataUpdate_ctloverview_Changed(object sender, EventArgs e)
    {
        bool cancontinue = false;
        if (!string.IsNullOrEmpty(hf_DataUpdate_ctloverview.Value))
        {
            string id = _uuf.getFlag_AppID(hf_DataUpdate_ctloverview.Value);
            if (id == AppId)
            {
                _uuf.deleteFlag(hf_DataUpdate_ctloverview.Value);
                cancontinue = true;
            }
        }

        if (cancontinue)
            StartBuild();

        hf_DataUpdate_ctloverview.Value = "";
    }
    protected void hf_refresh_ctloverview_Changed(object sender, EventArgs e)
    {
        StartBuild();
        hf_refresh_ctloverview.Value = "";
    }


    #region Build Table

    private void StartBuild()
    {
        ctl_logs_holder.Controls.Clear();
        string search = hf_search_ctloverview.Value.Trim();
        string result = string.Empty;

        if ((!string.IsNullOrEmpty(search)) && (search.ToLower() != "search for ctl logs"))
            result = BuildSearchTable(search);
        else
            result = BuildMainTable();

        ctl_logs_holder.Controls.Add(new LiteralControl(result));
        updatepnl_ctloverview.Update();
    }
    private string BuildMainTable()
    {
        string date = tb_date_ctloverview.Text.Trim();
        StringBuilder str = new StringBuilder();

        _ctlReport.BuildEntriesByReportDate(date, "Sequence", "ASC");
        str.Append("<table cellspacing='0' cellpadding='0' style='width:100%;border-collapse:collapse;'><tbody>");

        // Build Header
        str.Append("<tr class='myHeaderStyle'>");
        str.Append("<td>Coil #</td>");
        str.Append("<td style='width: 125px;'>Material Used</td>");
        str.Append("<td style='width: 125px;'>Mic/Heat #</td>");
        str.Append("<td>Customer</td>");
        str.Append("<td style='width: 75px;'>SO # <u>AND</u> Line #</td>");
        str.Append("<td style='width: 115px;'>Size Produced</td>");
        str.Append("<td style='width: 100px;'>Total Pieces</td>");
        str.Append("<td>Coil Wt/Restock</td>");
        str.Append("</tr>");

        header_ctloverview.Enabled = true;
        header_ctloverview.Visible = true;

        string docNumber = "SF-07-05";
        string revision = "00";
        string approvedBy = "Not Assigned";

        int count = 0;

        // Build Data Rows
        foreach (var x in _ctlReport.CTLReportCollection)
        {
            CTLLogs_Coll logEntry = _ctlLogs.GetEntry(x.ID);
            if (logEntry != null)
            {
                str.Append("<tr class='myItemStyle GridNormalRow'>");
                str.Append("<td class='border-right' align='center' style='border-left: 1px solid #DFDFDF;'>" + x.CoilNumber + "</td>");
                str.Append("<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left'>PCS:</b><span class='float-right'>" + logEntry.MaterialUsed_PCS + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #AAA;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Size:</b><span class='float-right'>" + logEntry.MaterialUsed_Size + "</span><div class='clear-space'></div></td>");
                str.Append("<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left'>Mic:</b><span class='float-right'>" + logEntry.MicNumber + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #AAA;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Heat:</b><span class='float-right'>" + logEntry.HeatNumber + "</span><div class='clear-space'></div></td>");
                str.Append("<td class='border-right' align='center'>" + x.Customer + "</td>");
                str.Append("<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left'>SO:</b><span class='float-right'>" + logEntry.SONumber + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #AAA;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Line:</b><span class='float-right'>" + logEntry.LineNumber + "</span><div class='clear-space'></div></td>");
                str.Append("<td class='border-right' align='center'>" + logEntry.SizeProduced + "</td>");
                str.Append("<td class='border-right' align='center'>" + logEntry.TotalPieces.ToString() + "</td>");
                str.Append("<td class='border-right' align='center'>" + logEntry.CoilWeightRestock + "</td>");
                str.Append("</tr>");

                docNumber = x.DocNumber;
                revision = x.Revision;

                MemberDatabase m = new MemberDatabase(x.ApprovedBy);
                string fullName = HelperMethods.MergeFMLNames(m);
                if (!string.IsNullOrEmpty(fullName))
                    approvedBy = fullName;
                else
                    approvedBy = x.ApprovedBy;

                count++;
            }
        }

        str.Append("</tbody></table>");

        if (count == 0)
            str.Append("<div class='emptyGridView'>No Data Available</div>");

        docNumber_ctllogsheet.InnerHtml = docNumber;
        revision_ctllogsheet.InnerHtml = revision;
        approvedBy_ctllogsheet.InnerHtml = approvedBy;

        ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "$('#ctloverview-search-topcontrols').hide();$('#ctloverview-topcontrols').show();", true);

        return str.ToString();
    }
    private string BuildSearchTable(string search)
    {
        StringBuilder str = new StringBuilder();

        int count = 0;
        _ctlReport.BuildEntriesAll("Sequence", "ASC");
        if (search.Contains(": "))
            search = search.Substring(search.IndexOf(": ") + (": ").Length);
        if (search.Contains("- "))
            search = search.Substring(search.IndexOf("- ") + ("- ").Length);

        str.Append("<table cellspacing='0' cellpadding='0' style='width:100%;border-collapse:collapse;'><tbody>");

        // Build Header
        str.Append("<tr class='myHeaderStyle'>");
        str.Append("<td>Date</td>");
        str.Append("<td>Coil #</td>");
        str.Append("<td style='width: 125px;'>Material Used</td>");
        str.Append("<td style='width: 125px;'>Mic/Heat #</td>");
        str.Append("<td>Customer</td>");
        str.Append("<td style='width: 75px;'>SO # <u>AND</u> Line #</td>");
        str.Append("<td style='width: 115px;'>Size Produced</td>");
        str.Append("<td style='width: 100px;'>Total Pieces</td>");
        str.Append("<td>Coil Wt/Restock</td>");
        str.Append("</tr>");

        header_ctloverview.Enabled = false;
        header_ctloverview.Visible = false;

        // Build Data Rows
        foreach (var x in _ctlReport.CTLReportCollection)
        {
            bool canContinue = false;
            if (x.Line.ToLower().Contains(search.ToLower()))
                canContinue = true;
            else if (x.Customer.ToLower().Contains(search.ToLower()))
                canContinue = true;
            else if (x.CoilNumber.ToLower().Contains(search.ToLower()))
                canContinue = true;
            else if (x.Material.ToLower().Contains(search.ToLower()))
                canContinue = true;
            else if (x.OrderNumber.ToLower().Contains(search.ToLower()))
                canContinue = true;
            else if (x.OrderDate.ToShortDateString().ToLower().Contains(search.ToLower()))
                canContinue = true;
            else if (x.Gauge.ToLower().Contains(search.ToLower()))
                canContinue = true;
            else if (x.ReportDate.ToShortDateString().ToLower().Contains(search.ToLower()))
                canContinue = true;

            if (canContinue == true)
            {
                CTLLogs_Coll logEntry = _ctlLogs.GetEntry(x.ID);
                if (logEntry != null)
                {
                    str.Append("<tr class='myItemStyle GridNormalRow'>");
                    str.Append("<td class='border-right' align='center' style='border-left: 1px solid #DFDFDF;'>" + x.ReportDate.ToShortDateString() + "</td>");
                    str.Append("<td class='border-right' align='center'>" + x.CoilNumber + "</td>");
                    str.Append("<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left'>PCS:</b><span class='float-right'>" + logEntry.MaterialUsed_PCS + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #AAA;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Size:</b><span class='float-right'>" + logEntry.MaterialUsed_Size + "</span><div class='clear-space'></div></td>");
                    str.Append("<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left'>Mic:</b><span class='float-right'>" + logEntry.MicNumber + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #AAA;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Heat:</b><span class='float-right'>" + logEntry.HeatNumber + "</span><div class='clear-space'></div></td>");
                    str.Append("<td class='border-right' align='center'>" + x.Customer + "</td>");
                    str.Append("<td class='border-right' align='center'><div class='clear-space'></div><b class='pad-right float-left'>SO:</b><span class='float-right'>" + logEntry.SONumber + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #AAA;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Line:</b><span class='float-right'>" + logEntry.LineNumber + "</span><div class='clear-space'></div></td>");
                    str.Append("<td class='border-right' align='center'>" + logEntry.SizeProduced + "</td>");
                    str.Append("<td class='border-right' align='center'>" + logEntry.TotalPieces.ToString() + "</td>");
                    str.Append("<td class='border-right' align='center'>" + logEntry.CoilWeightRestock + "</td>");
                    str.Append("</tr>");
                    count++;
                }
            }
        }

        str.Append("</tbody></table>");

        if ((_ctlReport.CTLReportCollection.Count == 0) || (count == 0))
            str.Append("<div class='emptyGridView'>No Data Available</div>");

        ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "$('#ctloverview-topcontrols').hide();$('#ctloverview-search-topcontrols').show();", true);

        return str.ToString();
    }

    #endregion


    #region Export to Excel

    protected void hf_export_ctloverview_Changed(object sender, EventArgs e)
    {
        string search = hf_search_ctloverview.Value.Trim();

        if ((!string.IsNullOrEmpty(search)) && (search.ToLower() != "search for ctl logs"))
            ExportToExcel_Search(search);
        else
            ExportToExcel();

        StartBuild();
        hf_export_ctloverview.Value = "";
    }
    private void ExportToExcel()
    {
        string _date = tb_date_ctloverview.Text.Trim();
        string _Path = "CTLOverview-" + _date.Replace("/", "_") + ".xls";
        string directory = ServerSettings.GetServerMapLocation + "Apps\\CTLOverview\\Exports";
        string p = Path.Combine(directory, _Path);
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        var temp = new DataTable();
        try
        {
            _ctlReport.BuildEntriesByReportDate(_date, "Sequence", "ASC");
            if (_ctlReport.CTLReportCollection.Count == 0)
                ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "openWSE.AlertWindow('No records to export for current date.');", true);
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

            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "$.fileDownload('../../" + "Apps/CTLOverview/Exports" + "/" + _Path + "');", true);
        }
        catch { }
    }
    private void ExportToExcel_Search(string search)
    {
        string _Path = "CTLOverview_Search-" + DateTime.Now.ToShortDateString().Replace("/", "_") + ".xls";
        string directory = _ss.DocumentsFolder;
        string p = Path.Combine(directory, _Path);
        var temp = new DataTable();
        try
        {
            int count = 0;
            _ctlReport.BuildEntriesAll("Sequence", "ASC");
            if (search.Contains(": "))
                search = search.Substring(search.IndexOf(": ") + (": ").Length);
            if (search.Contains("- "))
                search = search.Substring(search.IndexOf("- ") + ("- ").Length);

            if (_ctlReport.CTLReportCollection.Count == 0)
                ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "openWSE.AlertWindow('No records to export for current date.');", true);
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

                foreach (CTLReport_Coll x in _ctlReport.CTLReportCollection)
                {
                    bool canContinue = false;
                    if (x.Line.ToLower().Contains(search.ToLower()))
                        canContinue = true;
                    else if (x.Customer.ToLower().Contains(search.ToLower()))
                        canContinue = true;
                    else if (x.CoilNumber.ToLower().Contains(search.ToLower()))
                        canContinue = true;
                    else if (x.Material.ToLower().Contains(search.ToLower()))
                        canContinue = true;
                    else if (x.OrderNumber.ToLower().Contains(search.ToLower()))
                        canContinue = true;
                    else if (x.OrderDate.ToShortDateString().ToLower().Contains(search.ToLower()))
                        canContinue = true;
                    else if (x.Gauge.ToLower().Contains(search.ToLower()))
                        canContinue = true;
                    else if (x.ReportDate.ToShortDateString().ToLower().Contains(search.ToLower()))
                        canContinue = true;

                    if (canContinue == true)
                    {
                        CTLLogs_Coll coll = _ctlLogs.GetEntry(x.ID);
                        if (coll != null)
                        {
                            DataRow drsch = temp.NewRow();
                            drsch["Coil Number"] = x.CoilNumber;
                            drsch["Line Number"] = x.Line;
                            drsch["Material Type"] = x.Material;
                            drsch["PCS"] = coll.MaterialUsed_PCS;
                            drsch["Material Size"] = coll.MaterialUsed_Size;
                            drsch["Mic Number"] = coll.MicNumber;
                            drsch["Heat Number"] = coll.HeatNumber;
                            drsch["Customer"] = x.Customer;
                            drsch["Order Number"] = x.OrderNumber;
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
                            count++;
                        }
                    }
                }
            }

            if (count == 0)
                ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "openWSE.AlertWindow('No records to export for current search.');", true);
            else
            {
                var FI = new FileInfo(p);
                var stringWriter = new StringWriter();
                var htmlWrite = new System.Web.UI.HtmlTextWriter(stringWriter);
                var DataGrd = new DataGrid();
                DataGrd.DataSource = temp;
                DataGrd.DataBind();

                DataGrd.RenderControl(htmlWrite);

                bool fileExisted = false;
                try
                {
                    if (File.Exists(p))
                    {
                        File.Delete(p);
                        fileExisted = true;
                    }
                }
                catch { }

                var vw = new StreamWriter(p, true);
                stringWriter.ToString().Normalize();
                vw.Write(stringWriter.ToString());
                vw.Flush();
                vw.Close();
                var info = new FileInfo(p);
                var filesql = new FileDrive(_username);
                MemberDatabase _member = new MemberDatabase(_username);
                if (!fileExisted)
                {
                    foreach (string group in _member.GroupList)
                    {
                        filesql.addFile(Guid.NewGuid().ToString(), _Path, info.Extension, HelperMethods.FormatBytes(info.Length), directory, string.Empty, "-", group);
                    }
                }
                DirectoryInfo di = new DirectoryInfo(directory);
                ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "$.fileDownload('../../" + di.Name + "/" + _Path + "');", true);
            }
        }
        catch { }
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

    #endregion
}