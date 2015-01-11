using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using System.Globalization;
using System.Security.Principal;
using System.Collections.Specialized;
using System.Text;
using System.Net.Mail;
using System.Net;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Notifications;
using OpenWSE_Tools.Apps;

public partial class Apps_ProdLeadTimes_ProductLeadTime : System.Web.UI.Page {
    #region private variables
    private MemberDatabase _member;
    private MembershipUserCollection coll;
    private plt _plt;
    private App apps = new App();
    private string ctrlname;
    private ScriptManager sm;
    private readonly IPWatch ipwatch = new IPWatch(true);
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private const string app_id = "app-productleadtime";
    private string _sitetheme;
    private AppInitializer _appInitializer;

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/Default.aspx");

        _appInitializer = new AppInitializer(app_id, userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent) {
            coll = Membership.GetAllUsers();

            _member = _appInitializer.memberDatabase;
            _sitetheme = _appInitializer.siteTheme;

            _plt = new plt(true);
            if (!IsPostBack) {
                // Initialize all the scripts and style sheets
                _appInitializer.SetHeaderLabelImage(lbl_Title, img_Title);
                _appInitializer.LoadScripts_JS(false);
                _appInitializer.LoadScripts_CSS();

                AutoUpdateSystem aus = new AutoUpdateSystem(hf_UpdateAll.ClientID, app_id, this);
                aus.StartAutoUpdates();

                resetSelected();

                getPLT();
                LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
            }
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }


    protected void hf_UpdateAll_ValueChanged(object sender, EventArgs e) {
        bool cancontinue = false;
        if (!string.IsNullOrEmpty(hf_UpdateAll.Value)) {
            string id = uuf.getFlag_AppID(hf_UpdateAll.Value);
            if (id == app_id) {
                uuf.deleteFlag(hf_UpdateAll.Value);
                cancontinue = true;
            }
        }

        if (cancontinue) {
            _plt = new plt(true);
            getPLT();
            LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
        }
        hf_UpdateAll.Value = "";
    }


    /******************************************
     * Production Lead Time
     ******************************************/
    #region Production Lead Time
    private void getPLT() {
        if (_plt.pltcoll.Count > 0) {
            _plt.pltcoll.Sort((x, y) => Convert.ToDateTime(y.DateUpdated).Ticks.CompareTo(Convert.ToDateTime(x.DateUpdated).Ticks));
            List<string> dates = _plt.CalculatePLT();
            const int i = 0;
            lbl_lastupdated_ProductLeadTime.Text = _plt.pltcoll[i].DateUpdated.ToString();
            var tempmember = new MemberDatabase(_plt.pltcoll[i].UpdatedBy);
            lbl_updatedby_ProductLeadTime.Text = HelperMethods.MergeFMLNames(tempmember);
            CheckForOverrides();
            if ((dates != null) && (dates.Count > 0)) {
                if (CheckBox1_1_ProductLeadTime.Checked) {
                    tb_Date1_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].CTL_Line1, CheckBox1_ProductLeadTime, tb_Date1_ProductLeadTime);
                    CheckBox1_ProductLeadTime.Enabled = true;
                    CheckBox1_ProductLeadTime.Visible = true;
                    tb_Date1_ProductLeadTime.Enabled = true;
                }
                else {
                    tb_Date1_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].CTL_Line1, dates[0], CheckBox1_ProductLeadTime, tb_Date1_ProductLeadTime);
                    CheckBox1_ProductLeadTime.Enabled = false;
                    CheckBox1_ProductLeadTime.Visible = false;
                    tb_Date1_ProductLeadTime.Enabled = false;
                }


                if (CheckBox2_2_ProductLeadTime.Checked) {
                    tb_Date2_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].CTL_Line2, CheckBox2_ProductLeadTime, tb_Date2_ProductLeadTime);
                    CheckBox2_ProductLeadTime.Enabled = true;
                    CheckBox2_ProductLeadTime.Visible = true;
                    tb_Date2_ProductLeadTime.Enabled = true;
                }
                else {
                    tb_Date2_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].CTL_Line2, dates[1], CheckBox2_ProductLeadTime, tb_Date2_ProductLeadTime);
                    CheckBox2_ProductLeadTime.Enabled = false;
                    CheckBox2_ProductLeadTime.Visible = false;
                    tb_Date2_ProductLeadTime.Enabled = false;
                }


                if (CheckBox3_3_ProductLeadTime.Checked) {
                    tb_Date3_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].Quarter_Shear, CheckBox3_ProductLeadTime, tb_Date3_ProductLeadTime);
                    CheckBox3_ProductLeadTime.Enabled = true;
                    CheckBox3_ProductLeadTime.Visible = true;
                    tb_Date3_ProductLeadTime.Enabled = true;
                }
                else {
                    tb_Date3_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].Quarter_Shear, dates[2], CheckBox3_ProductLeadTime, tb_Date3_ProductLeadTime);
                    CheckBox3_ProductLeadTime.Enabled = false;
                    CheckBox3_ProductLeadTime.Visible = false;
                    tb_Date3_ProductLeadTime.Enabled = false;
                }


                if (CheckBox4_4_ProductLeadTime.Checked) {
                    tb_Date4_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].Half_Shear, CheckBox4_ProductLeadTime, tb_Date4_ProductLeadTime);
                    CheckBox4_ProductLeadTime.Enabled = true;
                    CheckBox4_ProductLeadTime.Visible = true;
                    tb_Date4_ProductLeadTime.Enabled = true;
                }
                else {
                    tb_Date4_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].Half_Shear, dates[3], CheckBox4_ProductLeadTime, tb_Date4_ProductLeadTime);
                    CheckBox4_ProductLeadTime.Enabled = false;
                    CheckBox4_ProductLeadTime.Visible = false;
                    tb_Date4_ProductLeadTime.Enabled = false;
                }


                if (CheckBox5_5_ProductLeadTime.Checked) {
                    tb_Date5_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].BurnTable, CheckBox5_ProductLeadTime, tb_Date5_ProductLeadTime);
                    CheckBox5_ProductLeadTime.Enabled = true;
                    CheckBox5_ProductLeadTime.Visible = true;
                    tb_Date5_ProductLeadTime.Enabled = true;
                }
                else {
                    tb_Date5_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].BurnTable, dates[4], CheckBox5_ProductLeadTime, tb_Date5_ProductLeadTime);
                    CheckBox5_ProductLeadTime.Enabled = false;
                    CheckBox5_ProductLeadTime.Visible = false;
                    tb_Date5_ProductLeadTime.Enabled = false;
                }
            }
            else {
                CheckBox1_ProductLeadTime.Enabled = true;
                CheckBox2_ProductLeadTime.Enabled = true;
                CheckBox3_ProductLeadTime.Enabled = true;
                CheckBox4_ProductLeadTime.Enabled = true;
                CheckBox5_ProductLeadTime.Enabled = true;
                tb_Date1_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].CTL_Line1, CheckBox1_ProductLeadTime, tb_Date1_ProductLeadTime);
                tb_Date2_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].CTL_Line2, CheckBox2_ProductLeadTime, tb_Date2_ProductLeadTime);
                tb_Date3_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].Quarter_Shear, CheckBox3_ProductLeadTime, tb_Date3_ProductLeadTime);
                tb_Date4_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].Half_Shear, CheckBox4_ProductLeadTime, tb_Date4_ProductLeadTime);
                tb_Date5_ProductLeadTime.Text = checkifoutofservice(_plt.pltcoll[i].BurnTable, CheckBox5_ProductLeadTime, tb_Date5_ProductLeadTime);
            }
        }
        else {
            lbl_lastupdated_ProductLeadTime.Text = "N/A";
            lbl_updatedby_ProductLeadTime.Text = "N/A";
            tb_Date1_ProductLeadTime.Text = "N/A";
            tb_Date2_ProductLeadTime.Text = "N/A";
            tb_Date3_ProductLeadTime.Text = "N/A";
            tb_Date4_ProductLeadTime.Text = "N/A";
            tb_Date5_ProductLeadTime.Text = "N/A";
        }
        resetSelected();
        CheckDisableUpdate();
    }

    private void CheckForOverrides() {
        CheckBox1_1_ProductLeadTime.Checked = _plt.GetOverride("Override1");
        CheckBox2_2_ProductLeadTime.Checked = _plt.GetOverride("Override2");
        CheckBox3_3_ProductLeadTime.Checked = _plt.GetOverride("Override3");
        CheckBox4_4_ProductLeadTime.Checked = _plt.GetOverride("Override4");
        CheckBox5_5_ProductLeadTime.Checked = _plt.GetOverride("Override5");
    }

    private void CheckDisableUpdate() {
        //if ((!CheckBox1_1.Checked) && (!CheckBox2_2.Checked)
        //    && (!CheckBox3_3.Checked) && (!CheckBox4_4.Checked)
        //    && (!CheckBox5_5.Checked))
        //{
        //    btn_update.Enabled = false;
        //    btn_update.Visible = false;
        //}
        //else
        //{
        //    btn_update.Enabled = true;
        //    btn_update.Visible = true;
        //}
    }

    private string checkifoutofservice(string a, CheckBox checkbox, TextBox tb) {
        if (a.ToLower() == "out of service") {
            tb.Enabled = false;
            checkbox.Checked = true;
            return a;
        }
        else {
            tb.Enabled = true;
            checkbox.Checked = false;
            return a;
        }
    }

    private string checkifoutofservice(string a, string b, CheckBox checkbox, TextBox tb) {
        if (a.ToLower() == "out of service") {
            tb.Enabled = false;
            checkbox.Checked = true;
            return a;
        }
        else {
            tb.Enabled = true;
            checkbox.Checked = false;
            return b;
        }
    }

    private void checkforerrors(TextBox tb) {
        try {
            DateTime now = DateTime.Now;
            TimeSpan final = now.Subtract(Convert.ToDateTime(tb.Text));
            if ((final.TotalHours > 12) && (final.Ticks <= 0)) {
                tb.Text = string.Empty;
                lbl_error2_ProductLeadTime.Enabled = true;
                lbl_error2_ProductLeadTime.Visible = true;
            }
            else if ((final.Ticks > 0) && (final.Days != 0)) {
                tb.Text = string.Empty;
                lbl_error2_ProductLeadTime.Enabled = true;
                lbl_error2_ProductLeadTime.Visible = true;
            }
            else {
                lbl_error2_ProductLeadTime.Enabled = false;
                lbl_error2_ProductLeadTime.Visible = false;
            }
        }
        catch { }
    }

    protected void tb_Date1_TextChanged(object sender, EventArgs e) {
        checkforerrors(tb_Date1_ProductLeadTime);
    }

    protected void tb_Date2_TextChanged(object sender, EventArgs e) {
        checkforerrors(tb_Date2_ProductLeadTime);
    }

    protected void tb_Date3_TextChanged(object sender, EventArgs e) {
        checkforerrors(tb_Date3_ProductLeadTime);
    }

    protected void tb_Date4_TextChanged(object sender, EventArgs e) {
        checkforerrors(tb_Date4_ProductLeadTime);
    }

    protected void tb_Date5_TextChanged(object sender, EventArgs e) {
        checkforerrors(tb_Date5_ProductLeadTime);
    }

    protected void CheckBox1_CheckedChanged(object sender, EventArgs e) {
        if (CheckBox1_ProductLeadTime.Checked) {
            tb_Date1_ProductLeadTime.Text = "Out of Service";
            tb_Date1_ProductLeadTime.Enabled = false;
        }
        else {
            tb_Date1_ProductLeadTime.Text = string.Empty;
            tb_Date1_ProductLeadTime.Enabled = true;
        }
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBox1_1CheckedChanged(object sender, EventArgs e) {
        if (CheckBox1_1_ProductLeadTime.Checked) {
            _plt.UpdateOverride1(true);
            CheckBox1_ProductLeadTime.Enabled = true;
            CheckBox1_ProductLeadTime.Visible = true;
        }
        else {
            _plt.UpdateOverride1(false);
            CheckBox1_ProductLeadTime.Enabled = false;
            CheckBox1_ProductLeadTime.Visible = false;
        }
        getPLT();
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBox2_CheckedChanged(object sender, EventArgs e) {
        if (CheckBox2_ProductLeadTime.Checked) {
            tb_Date2_ProductLeadTime.Text = "Out of Service";
            tb_Date2_ProductLeadTime.Enabled = false;
        }
        else {
            tb_Date2_ProductLeadTime.Text = string.Empty;
            tb_Date2_ProductLeadTime.Enabled = true;
        }
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBox2_2CheckedChanged(object sender, EventArgs e) {
        if (CheckBox2_2_ProductLeadTime.Checked) {
            _plt.UpdateOverride2(true);
            CheckBox2_ProductLeadTime.Enabled = true;
            CheckBox2_ProductLeadTime.Visible = true;
        }
        else {
            _plt.UpdateOverride2(false);
            CheckBox2_ProductLeadTime.Enabled = false;
            CheckBox2_ProductLeadTime.Visible = false;
        }
        getPLT();
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBox3_CheckedChanged(object sender, EventArgs e) {
        if (CheckBox3_ProductLeadTime.Checked) {
            tb_Date3_ProductLeadTime.Text = "Out of Service";
            tb_Date3_ProductLeadTime.Enabled = false;
        }
        else {
            tb_Date3_ProductLeadTime.Text = string.Empty;
            tb_Date3_ProductLeadTime.Enabled = true;
        }
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBox3_3CheckedChanged(object sender, EventArgs e) {
        if (CheckBox3_3_ProductLeadTime.Checked) {
            _plt.UpdateOverride3(true);
            CheckBox3_ProductLeadTime.Enabled = true;
            CheckBox3_ProductLeadTime.Visible = true;
        }
        else {
            _plt.UpdateOverride3(false);
            CheckBox3_ProductLeadTime.Enabled = false;
            CheckBox3_ProductLeadTime.Visible = false;
        }
        getPLT();
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBox4_CheckedChanged(object sender, EventArgs e) {
        if (CheckBox4_ProductLeadTime.Checked) {
            tb_Date4_ProductLeadTime.Text = "Out of Service";
            tb_Date4_ProductLeadTime.Enabled = false;
        }
        else {
            tb_Date4_ProductLeadTime.Text = string.Empty;
            tb_Date4_ProductLeadTime.Enabled = true;
        }
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBox4_4CheckedChanged(object sender, EventArgs e) {
        if (CheckBox4_4_ProductLeadTime.Checked) {
            _plt.UpdateOverride4(true);
            CheckBox4_ProductLeadTime.Enabled = true;
            CheckBox4_ProductLeadTime.Visible = true;
        }
        else {
            _plt.UpdateOverride4(false);
            CheckBox4_ProductLeadTime.Enabled = false;
            CheckBox4_ProductLeadTime.Visible = false;
        }
        getPLT();
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBox5_CheckedChanged(object sender, EventArgs e) {
        if (CheckBox5_ProductLeadTime.Checked) {
            tb_Date5_ProductLeadTime.Text = "Out of Service";
            tb_Date5_ProductLeadTime.Enabled = false;
        }
        else {
            tb_Date5_ProductLeadTime.Text = string.Empty;
            tb_Date5_ProductLeadTime.Enabled = true;
        }
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBox5_5CheckedChanged(object sender, EventArgs e) {
        if (CheckBox5_5_ProductLeadTime.Checked) {
            _plt.UpdateOverride5(true);
            CheckBox5_ProductLeadTime.Enabled = true;
            CheckBox5_ProductLeadTime.Visible = true;
        }
        else {
            _plt.UpdateOverride5(false);
            CheckBox5_ProductLeadTime.Enabled = false;
            CheckBox5_ProductLeadTime.Visible = false;
        }
        getPLT();
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void btn_update_Click(object sender, EventArgs e) {
        if ((string.IsNullOrEmpty(tb_Date1_ProductLeadTime.Text)) || (string.IsNullOrEmpty(tb_Date2_ProductLeadTime.Text))
            || (string.IsNullOrEmpty(tb_Date3_ProductLeadTime.Text)) || (string.IsNullOrEmpty(tb_Date4_ProductLeadTime.Text))
            || (string.IsNullOrEmpty(tb_Date5_ProductLeadTime.Text))) {
            lbl_error_ProductLeadTime.Enabled = true;
            lbl_error_ProductLeadTime.Visible = true;
        }
        else {
            try {
                string DateTimeNow = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                _plt.addItem(tb_Date1_ProductLeadTime.Text, tb_Date2_ProductLeadTime.Text, tb_Date3_ProductLeadTime.Text, tb_Date4_ProductLeadTime.Text, tb_Date5_ProductLeadTime.Text, DateTimeNow, HttpContext.Current.User.Identity.Name);
                if (_plt.pltcoll.Count > 0) {
                    lbl_lastupdated_ProductLeadTime.Text = DateTimeNow;
                    var tempmember = new MemberDatabase(HttpContext.Current.User.Identity.Name);
                    lbl_updatedby_ProductLeadTime.Text = HelperMethods.MergeFMLNames(tempmember);
                }
                AddNotifications(tb_Date1_ProductLeadTime.Text, tb_Date2_ProductLeadTime.Text, tb_Date3_ProductLeadTime.Text, tb_Date4_ProductLeadTime.Text, tb_Date5_ProductLeadTime.Text, DateTimeNow, HttpContext.Current.User.Identity.Name);
                _plt = new plt(true);
                getPLT();
                LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
            }
            catch { }
        }
    }

    private void AddNotifications(string date1, string date2, string date3, string date4, string date5, string updated, string by) {
        MembershipUserCollection userlist = Membership.GetAllUsers();


        string messagebody = string.Empty;
        messagebody += "<h2>Production Lead Times</h2><br /><b>Time Updated: </b>" + updated + "<br /><b>Updated by: </b>" + by;
        messagebody += "<table border='0' cellpadding='15' cellspacing='15' style='width: 500px'><tbody><tr><td><b><u>CTL 1 (Little)</u></b></td><td>";
        messagebody += "<b><u>CTL 2 (Big)</u></b></td><td><b><u>1/4'' Shear</u></b></td><td><b><u>1/2'' Shear</u></b></td>";
        messagebody += "<td><b><u>Burn Table</u></b></td></tr><tr><td>" + date1 + "</td><td>" + date2 + "</td><td>" + date3 + "</td>";
        messagebody += "<td>" + date4 + "</td><td>" + date5 + "</td></tr></tbody></table>";
        var message = new MailMessage();

        foreach (MembershipUser u in userlist) {
            if ((u.UserName.ToLower() != Page.User.Identity.Name.ToLower()) && (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                MemberDatabase member = new MemberDatabase(u.UserName);
                if ((member.UserHasApp(app_id)) || (member.UserHasApp("app-pltnoedit"))) {
                    UserNotificationMessages un = new UserNotificationMessages(u.UserName);
                    string email = "";
                    if (member.UserHasApp(app_id))
                        email = un.attemptAdd(app_id, messagebody.ToString(), true);
                    else
                        email = un.attemptAdd("app-pltnoedit", messagebody.ToString(), true);

                    if (!string.IsNullOrEmpty(email))
                        message.To.Add(email);
                }
            }
        }

        UserNotificationMessages.finishAdd(message, app_id, messagebody.ToString());
    }

    protected void GV_PLTHistory_RowCreated(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.Pager) {
            Panel PnlPager = (Panel)e.Row.FindControl("PnlPager_ProductLeadTime");
            for (int i = 0; i < GV_PLTHistory_ProductLeadTime.PageCount; i++) {
                LinkButton lbtn_page = new LinkButton();
                lbtn_page.CommandArgument = i.ToString();
                lbtn_page.CommandName = "PageNo";
                if (GV_PLTHistory_ProductLeadTime.PageIndex == i) {
                    lbtn_page.CssClass = "GVPagerNumActive plt-update";
                }
                else {
                    lbtn_page.CssClass = "GVPagerNum plt-update";
                }
                lbtn_page.Text = (i + 1).ToString();
                PnlPager.Controls.Add(lbtn_page);
            }
        }
    }

    protected void GV_PLTHistory_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.Pager) {
            Label lbl2 = (Label)e.Row.FindControl("pglbl2_ProductLeadTime");
            LinkButton btnFirst = (LinkButton)e.Row.FindControl("btnFirst_ProductLeadTime");
            LinkButton btnPrevious = (LinkButton)e.Row.FindControl("btnPrevious_ProductLeadTime");
            LinkButton btnNext = (LinkButton)e.Row.FindControl("btnNext_ProductLeadTime");
            LinkButton btnLast = (LinkButton)e.Row.FindControl("btnLast_ProductLeadTime");
            TextBox tb_page = (TextBox)e.Row.FindControl("tb_pageManual_ProductLeadTime");

            lbl2.Text = GV_PLTHistory_ProductLeadTime.PageCount.ToString();
            tb_page.Text = (GV_PLTHistory_ProductLeadTime.PageIndex + 1).ToString();

            if ((GV_PLTHistory_ProductLeadTime.PageIndex + 1) == 1) {
                btnFirst.Visible = false;
                btnPrevious.Visible = false;
            }
            if ((GV_PLTHistory_ProductLeadTime.PageIndex + 1) == GV_PLTHistory_ProductLeadTime.PageCount) {
                btnNext.Visible = false;
                btnLast.Visible = false;
            }
        }
    }

    protected void GV_PLTHistory_RowCommand(object sender, GridViewCommandEventArgs e) {
        switch (e.CommandName) {
            case "PageNo":
                LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
                GV_PLTHistory_ProductLeadTime.PageIndex = Convert.ToInt32(e.CommandArgument.ToString());
                GV_PLTHistory_ProductLeadTime.DataBind();
                break;
        }
    }

    protected void GV_PLTHistory_PageIndexChanging(object sender, GridViewPageEventArgs e) {
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
        GV_PLTHistory_ProductLeadTime.PageIndex = e.NewPageIndex;
        GV_PLTHistory_ProductLeadTime.DataBind();
    }

    protected void tb_pageManual_TextChanged(object sender, EventArgs e) {
        if (_plt.pltcoll.Count > 0) {
            int page = 0;
            try {
                page = Convert.ToInt32(((TextBox)sender).Text);
            }
            catch { }
            if (page > GV_PLTHistory_ProductLeadTime.PageCount) {
                page = GV_PLTHistory_ProductLeadTime.PageCount;
            }
            else if (page <= 0) {
                page = 1;
            }

            GV_PLTHistory_ProductLeadTime.PageIndex = page - 1;
            GV_PLTHistory_ProductLeadTime.DataBind();
        }
    }

    public void LoadPLTHistory(ref GridView gv, string sortExp, string sortDir) {
        DataView dvlist = GetList();
        if (dvlist.Count > 0) {
            if (sortExp != string.Empty) {
                dvlist.Sort = string.Format("{0} {1}", dvlist.Table.Columns[Convert.ToInt16(sortExp)], sortDir);
            }

        }
        gv.DataSource = dvlist;
        gv.DataBind();
    }

    public DataView GetList() {
        DataTable dtlist = new System.Data.DataTable();
        dtlist.Columns.Add(new System.Data.DataColumn("dateLong"));
        dtlist.Columns.Add(new System.Data.DataColumn("date"));
        dtlist.Columns.Add(new System.Data.DataColumn("ctl1"));
        dtlist.Columns.Add(new System.Data.DataColumn("ctl2"));
        dtlist.Columns.Add(new System.Data.DataColumn("shearhalf"));
        dtlist.Columns.Add(new System.Data.DataColumn("shearquart"));
        dtlist.Columns.Add(new System.Data.DataColumn("burntable"));
        dtlist.Columns.Add(new System.Data.DataColumn("updatedby"));
        dtlist.Columns.Add(new System.Data.DataColumn("ID"));
        foreach (plt_coll row in _plt.pltcoll) {
            DataRow drlist = dtlist.NewRow();
            MemberDatabase md = new MemberDatabase(row.UpdatedBy);
            drlist["dateLong"] = Convert.ToDateTime(row.DateUpdated).Ticks;
            drlist["date"] = row.DateUpdated;
            drlist["ctl1"] = row.CTL_Line1;
            drlist["ctl2"] = row.CTL_Line2;
            drlist["shearhalf"] = row.Half_Shear;
            drlist["shearquart"] = row.Quarter_Shear;
            drlist["burntable"] = row.BurnTable;
            drlist["updatedby"] = HelperMethods.MergeFMLNames(md);
            drlist["ID"] = row.ID.ToString();
            dtlist.Rows.Add(drlist);
        }
        DataView dvlist = new DataView(dtlist);
        return dvlist;
    }
    #endregion

    private string getCaseSensUN(string user) {
        string csun = string.Empty;
        MembershipUserCollection c = Membership.GetAllUsers();
        foreach (MembershipUser u in c) {
            if (u.UserName.ToLower() == user.ToLower()) {
                csun = u.UserName;
            }
        }
        return csun;
    }

    private string getTime(DateTime postDate) {
        DateTime now = DateTime.Now;
        TimeSpan final = now.Subtract(postDate);
        string time = string.Empty;
        if (final.Days > 2) {
            time = postDate.ToShortDateString();
        }
        else {
            if (final.Days == 0) {
                if (final.Hours == 0) {
                    time = final.Minutes.ToString() + " minute(s) ago";
                }
                else {
                    time = final.Hours.ToString() + " hour(s) ago";
                }
            }
            else {
                time = final.Days.ToString() + " day(s) ago";
            }
        }
        return time;
    }

    protected void imgbtn_del_Click(object sender, EventArgs e) {
        string[] list = HiddenField1_ProductLeadTime.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
        try {
            for (int i = 0; i < list.Length; i++) {
                _plt.DeleteHistory(list[i]);
            }
        }
        catch { }
        _plt = new plt(true);
        getPLT();
        LoadPLTHistory(ref GV_PLTHistory_ProductLeadTime, "0", "desc");
    }

    protected void CheckBoxIndv_CheckChanged(object sender, EventArgs e) {
        CheckBox chk = (CheckBox)sender;
        string filename = chk.Text;
        string[] filelist = HiddenField1_ProductLeadTime.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

        if (filelist.Contains(filename)) {
            List<string> templist = new List<string>();
            for (int i = 0; i < filelist.Length; i++) {
                if (!filelist[i].Contains(filename)) {
                    templist.Add(filelist[i]);
                }
            }
            HiddenField1_ProductLeadTime.Value = "";
            if (templist.Count > 0) {
                foreach (string file in templist) {
                    HiddenField1_ProductLeadTime.Value += file + ServerSettings.StringDelimiter;
                }
            }
        }
        else {
            HiddenField1_ProductLeadTime.Value += filename + ServerSettings.StringDelimiter;
        }
    }

    protected void lbtn_selectAll_Click(object sender, EventArgs e) {
        if (lbtn_selectAll_ProductLeadTime.Text == "Select All") {
            HiddenField1_ProductLeadTime.Value = string.Empty;
            foreach (GridViewRow r in GV_PLTHistory_ProductLeadTime.Rows) {
                CheckBox chk = (CheckBox)r.FindControl("CheckBoxIndv_ProductLeadTime");
                chk.Checked = true;
                string filename = chk.Text;
                string[] filelist = HiddenField1_ProductLeadTime.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                if (filelist.Contains(filename)) {
                    List<string> templist = new List<string>();
                    for (int i = 0; i < filelist.Length; i++) {
                        if (!filelist[i].Contains(filename)) {
                            templist.Add(filelist[i]);
                        }
                    }
                    HiddenField1_ProductLeadTime.Value = "";
                    if (templist.Count > 0) {
                        foreach (string file in templist) {
                            HiddenField1_ProductLeadTime.Value += file + ServerSettings.StringDelimiter;
                        }
                    }
                }
                else {
                    HiddenField1_ProductLeadTime.Value += filename + ServerSettings.StringDelimiter;
                }
            }
            lbtn_selectAll_ProductLeadTime.Text = "Deselect All";
        }

        else {
            foreach (GridViewRow r in GV_PLTHistory_ProductLeadTime.Rows) {
                CheckBox chk = (CheckBox)r.FindControl("CheckBoxIndv_ProductLeadTime");
                chk.Checked = false;
            }
            resetSelected();
        }
    }

    private void resetSelected() {
        HiddenField1_ProductLeadTime.Value = string.Empty;
        lbtn_selectAll_ProductLeadTime.Text = "Select All";
    }
}