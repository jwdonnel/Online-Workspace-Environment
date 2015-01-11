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

public partial class Apps_CTLReport_CTLSchedulePrint : System.Web.UI.Page
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
            if (member.UserHasApp("app-ctlreport"))
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
        StringBuilder str1 = new StringBuilder();
        str1.Append("<table cellspacing='0' cellpadding='0' style='width:100%;border-collapse:collapse;'><tbody>");

        // Build Header
        str1.Append("<tr class='myHeaderStyle'>");
        str1.Append("<td style='width: 50px;'>SEQ</td>");
        str1.Append("<td>GAUGE</td>");
        str1.Append("<td>MATERIAL</td>");
        str1.Append("<td>WIDTH</td>");
        str1.Append("<td>COIL #</td>");
        str1.Append("<td>COIL WT.</td>");
        str1.Append("<td>CUSTOMER</td>");
        str1.Append("<td>ORDER DATE</td>");
        str1.Append("<td>WEIGHT</td>");
        str1.Append("<td>ORDER #</td>");
        str1.Append("</tr>");

        string docNumber = "SF-07-05";
        string revision = "00";
        string approvedBy = "Not Assigned";

        // Build Data Rows
        foreach (var x in coll)
        {
            str1.Append("<tr class='myItemStyle GridNormalRow'>");
            str1.Append("<td class='GridViewNumRow' style='width: 50px;'>" + x.Sequence.ToString() + "</td>");
            str1.Append("<td class='table-cell-pad'>" + x.Gauge + "</td>");
            str1.Append("<td class='table-cell-pad'>" + x.Material + "</td>");
            str1.Append("<td class='table-cell-pad'>" + x.Width + "</td>");
            str1.Append("<td class='table-cell-pad'>" + x.CoilNumber + "</td>");
            str1.Append("<td class='table-cell-pad'>" + x.CoilWeight + "</td>");
            str1.Append("<td class='table-cell-pad'>" + x.Customer + "</td>");
            str1.Append("<td class='table-cell-pad'>" + x.OrderDate.ToShortDateString() + "</td>");
            str1.Append("<td class='table-cell-pad'>" + x.Weight + "</td>");
            str1.Append("<td class='table-cell-pad'>" + x.OrderNumber + "</td>");
            str1.Append("</tr>");

            docNumber = x.DocNumber;
            revision = x.Revision;

            MemberDatabase m = new MemberDatabase(x.ApprovedBy);
            string fullName = HelperMethods.MergeFMLNames(m);
            if (!string.IsNullOrEmpty(fullName))
                approvedBy = fullName;
            else
                approvedBy = x.ApprovedBy;
        }

        str1.Append("</tbody></table>");

        if (coll.Count == 0)
            str1.Append("<div class='emptyGridView'>No Data Available</div>");


        // Build Footer Info
        StringBuilder str2 = new StringBuilder();
        str2.Append("<table cellspacing='0' cellpadding='0' style='width:100%;border-collapse:collapse;'><tbody>");
        str2.Append("<tr class='myHeaderStyle'>");
        str2.Append("<td>DOC NUMBER</td>");
        str2.Append("<td>REVISION</td>");
        str2.Append("<td>REVIEWED AND APPROVED BY</td>");
        str2.Append("<td>DATE</td>");
        str2.Append("</tr>");
        str2.Append("<tr class='myItemStyle GridNormalRow'>");
        str2.Append("<td class='table-cell-pad' style='border-left: 1px solid #555;'>" + docNumber + "</td>");
        str2.Append("<td class='table-cell-pad'>" + revision + "</td>");
        str2.Append("<td class='table-cell-pad'>" + approvedBy + "</td>");
        str2.Append("<td class='table-cell-pad'>" + DateTime.Now.ToShortDateString() + "</td>");
        str2.Append("</tr></tbody></table>");

        pnl_schedule.Controls.Add(new LiteralControl(str1.ToString() + "<div class='clear' style='height: 50px;'></div>" + str2.ToString()));
    }
}