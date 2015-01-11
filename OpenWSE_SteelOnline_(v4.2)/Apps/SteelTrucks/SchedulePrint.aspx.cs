#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

public partial class steel_online_auth_SchedulePrint : Page
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
            if (member.UserHasApp("app-steeltrucks"))
            {
                string driver = Request.QueryString["user"];
                string date = Request.QueryString["date"];
                string unit = Request.QueryString["unit"];
                string gd = Request.QueryString["gd"];
                try
                {
                    driver = driver.Replace("_", " ");
                }
                catch
                {
                }
                lbl_driver.Text = driver;
                lbl_date.Text = date;
                lbl_unit.Text = unit;
                lbl_gd.Text = gd;
                var ts = new TruckSchedule(driver.Replace(" ", "_"), date, unit, gd);
                LoadSchedule(ref GV_Schedule, ts.scheduler_coll);
            }
            else
            {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    #region GridView Properties Methods

    public void LoadSchedule(ref GridView gv, List<TruckSchedule_Coll> ts)
    {
        DataView dvSch = GetTimeSlots(ts);
        dvSch.Sort = string.Format("{0} {1}", dvSch.Table.Columns[Convert.ToInt32("0")], "asc");
        gv.DataSource = dvSch;
        gv.DataBind();
    }

    public DataView GetTimeSlots(List<TruckSchedule_Coll> ts)
    {
        var dtsch = new DataTable();
        dtsch.Columns.Add(new DataColumn("Sequence"));
        dtsch.Columns.Add(new DataColumn("CustomerName"));
        dtsch.Columns.Add(new DataColumn("City"));
        dtsch.Columns.Add(new DataColumn("OrderNumber"));
        dtsch.Columns.Add(new DataColumn("Weight"));
        dtsch.Columns.Add(new DataColumn("ID"));
        int weight = 0;
        for (int i = 0; i < ts.Count; i++)
        {
            DataRow drsch = dtsch.NewRow();
            drsch["CustomerName"] = ts[i].CustomerName;
            drsch["City"] = ts[i].City;
            drsch["OrderNumber"] = addWhiteSpace(ts[i].OrderNumber);
            drsch["Sequence"] = ts[i].Sequence;
            drsch["ID"] = ts[i].ID;
            int tempWeight;
            if (Int32.TryParse(ts[i].Weight.Replace(",", ""), out tempWeight))
            {
                drsch["Weight"] = tempWeight.ToString("#,##0");
            }
            else
            {
                drsch["Weight"] = ts[i].Weight.Replace(",", "");
            }
            try
            {
                weight += Convert.ToInt32(ts[i].Weight.Replace(",", ""));
            }
            catch
            {
            }
            dtsch.Rows.Add(drsch);
        }
        if (string.IsNullOrEmpty(weight.ToString()))
        {
            lbl_smwtruckweight.Text = "N/A";
        }
        else
        {
            lbl_smwtruckweight.Text = weight.ToString("#,##0") + " lbs";
        }
        var dvsch = new DataView(dtsch);
        return dvsch;
    }

    private string addWhiteSpace(string x)
    {
        string ret = x.Replace(",", ", ");
        ret = ret.Replace(".", ", ");
        return ret;
    }

    #endregion
}