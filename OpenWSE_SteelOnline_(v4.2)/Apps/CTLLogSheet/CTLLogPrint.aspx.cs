#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

#endregion

public partial class Apps_CTLLogSheet_CTLLogPrint : System.Web.UI.Page
{
    private readonly IPWatch ipwatch = new IPWatch(true);
    private ServerSettings ss = new ServerSettings();
    private MemberDatabase member;

    protected void Page_Load(object sender, EventArgs e)
    {
        IIdentity userID = HttpContext.Current.User.Identity;
        NameValueCollection n = Request.ServerVariables;
        string ipaddress = n["REMOTE_ADDR"];
        if (ipaddress == "::1")
        {
            ipaddress = "127.0.0.1";
        }

        if ((ipwatch.CheckIfBlocked(ipaddress)) && (userID.Name.ToLower() != ServerSettings.AdminUserName.ToLower()))
        {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
        }
        else if ((ss.SiteOffLine) && (userID.Name.ToLower() != ServerSettings.AdminUserName.ToLower()))
        {
            Page.Response.Redirect("~/ErrorPages/Maintenance.html");
        }
        else
        {
            var listener = new IPListener(false);
            if (!listener.TableEmpty)
            {
                if (listener.CheckIfActive(ipaddress))
                {
                    StartUpPage(userID);
                }
                else
                {
                    if (userID.Name.ToLower() != ServerSettings.AdminUserName.ToLower())
                    {
                        Page.Response.Redirect("~/ErrorPages/Blocked.html");
                    }
                    else
                    {
                        StartUpPage(userID);
                    }
                }
            }
            else
            {
                StartUpPage(userID);
            }
        }
    }

    private void StartUpPage(IIdentity userID)
    {
        if (!userID.IsAuthenticated)
        {
            Page.Response.Redirect("~/Default.aspx");
        }
        else
        {
            var apps = new App();
            member = new MemberDatabase(userID.Name);
            if (member.UserHasApp("app-ctllogsheet"))
            {
                string date = Request.QueryString["date"];
                string line = Request.QueryString["line"];

                if (string.IsNullOrEmpty(date))
                    date = DateTime.Now.ToShortDateString();

                if (string.IsNullOrEmpty(line))
                    line = "1";

                lbl_date.Text = date;

                CTLReport ctlReport = new CTLReport(userID.Name);
                ctlReport.BuildEntriesByReportDateAndLine(date, line, "Sequence", "ASC");
                BuildTable(ctlReport.CTLReportCollection);
            }
            else
            {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    public void BuildTable(List<CTLReport_Coll> coll)
    {
        CTLLogs _ctlLogs = new CTLLogs(HttpContext.Current.User.Identity.Name);
        StringBuilder str1 = new StringBuilder();
        str1.Append("<table cellspacing='0' cellpadding='0' style='width:100%;border-collapse:collapse;'><tbody>");

        // Build Header
        str1.Append("<tr class='myHeaderStyle'>");
        str1.Append("<td>Coil #</td>");
        str1.Append("<td style='width: 125px;'>Material Used</td>");
        str1.Append("<td style='width: 125px;'>Mic/Heat #</td>");
        str1.Append("<td>Customer</td>");
        str1.Append("<td style='width: 75px;'>SO # <u>AND</u> Line #</td>");
        str1.Append("<td style='width: 115px;'>Size Produced</td>");
        str1.Append("<td style='width: 100px;'>Total Pieces</td>");
        str1.Append("<td>Coil Wt/Restock</td>");
        str1.Append("</tr>");

        string docNumber = "SF-07-05";
        string revision = "00";
        string approvedBy = "Not Assigned";

        string shift = "";

        int count = 0;

        // Build Data Rows
        foreach (var x in coll)
        {
            CTLLogs_Coll logEntry = _ctlLogs.GetEntry(x.ID);
            if (logEntry != null)
            {
                str1.Append("<tr class='myItemStyle GridNormalRow'>");
                str1.Append("<td class='table-cell-pad' style='border-left: 1px solid #555;'>" + x.CoilNumber + "</td>");
                str1.Append("<td class='table-cell-pad'><div class='clear-space'></div><b class='pad-right float-left'>PCS:</b><span class='float-right'>" + logEntry.MaterialUsed_PCS + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #777;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Size:</b><span class='float-right'>" + logEntry.MaterialUsed_Size + "</span><div class='clear-space'></div></td>");
                str1.Append("<td class='table-cell-pad'><div class='clear-space'></div><b class='pad-right float-left'>Mic:</b><span class='float-right'>" + logEntry.MicNumber + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #777;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Heat:</b><span class='float-right'>" + logEntry.HeatNumber + "</span><div class='clear-space'></div></td>");
                str1.Append("<td class='table-cell-pad'>" + x.Customer + "</td>");
                str1.Append("<td class='table-cell-pad'><div class='clear-space'></div><b class='pad-right float-left'>SO:</b><span class='float-right'>" + logEntry.SONumber + "</span><div class='clear-space-five'></div><div style='border-bottom: 1px solid #777;'></div><div class='clear-space-five'></div><b class='pad-right float-left'>Line:</b><span class='float-right'>" + logEntry.LineNumber + "</span><div class='clear-space'></div></td>");
                str1.Append("<td class='table-cell-pad'>" + logEntry.SizeProduced + "</td>");
                str1.Append("<td class='table-cell-pad'>" + logEntry.TotalPieces.ToString() + "</td>");
                str1.Append("<td class='table-cell-pad'>" + logEntry.CoilWeightRestock + "</td>");
                str1.Append("</tr>");

                docNumber = x.DocNumber;
                revision = x.Revision;

                MemberDatabase m = new MemberDatabase(x.ApprovedBy);
                string fullName = HelperMethods.MergeFMLNames(m);
                if (!string.IsNullOrEmpty(fullName))
                    approvedBy = fullName;
                else
                    approvedBy = x.ApprovedBy;

                if (string.IsNullOrEmpty(shift))
                    shift = logEntry.Shift;

                count++;
            }
        }

        str1.Append("</tbody></table>");

        if (count == 0)
            str1.Append("<div class='emptyGridView'>No Data Available</div>");

        lbl_shift.Text = shift;

        // Build Footer Info
        StringBuilder str2 = new StringBuilder();
        str2.Append("<div align='center'><b>The signature on this report identifies the individual performing the required inspections for these items, which serves as the record of inspection.</b></div>");
        str2.Append("<div class='clear-space'></div>");
        str2.Append("<table cellspacing='0' cellpadding='0' border='0' style='width: 100%;'><tbody>");
        str2.Append("<tr class='myItemStyle GridNormalRow'>");
        str2.Append("<td align='center'>Doc Number:  " + docNumber + "</td>");
        str2.Append("<td align='center'>REV:  " + revision + "</td>");
        str2.Append("<td align='center'>APPROVED BY:  " + approvedBy + "</td>");
        str2.Append("<td align='center'>Date:  " + DateTime.Now.ToShortDateString() + "</td>");
        str2.Append("</tr></tbody></table>");

        pnl_schedule.Controls.Add(new LiteralControl(str1.ToString() + "<div class='clear' style='height: 50px;'></div>" + str2.ToString()));
    }
}