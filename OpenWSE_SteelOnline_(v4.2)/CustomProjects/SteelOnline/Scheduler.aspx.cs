using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using System.Data;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Text;
using OpenWSE_Tools.AutoUpdates;

public partial class Integrated_Pages_Scheduler : System.Web.UI.Page
{
    #region public variables

    public string a;
    public string b;
    public string c;
    public string class1;
    public string class2;
    public string class3;
    public string class4;
    public string class5;
    public string d;
    public string e;
    public string hf_rt;
    public string s1;
    public string s2;

    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private AppLog _applog;
    private Scheduler _schedule;

    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        _applog = new AppLog(false);
        _schedule = new Scheduler();
    }

    #region Schedule Handler

    private readonly Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("~/");
    private ConnectionStringSettings connString;

    protected void tb_Date1_TextChanged(object sender, EventArgs e)
    {
        dd_schTimeSlot_deliverypickups.Items.Clear();
        lbl_slotsopen_deliverypickups.Text = string.Empty;
        dd_schTimeSlot_deliverypickups.Enabled = false;
        dd_schTimeSlot_deliverypickups.Visible = false;
        btn_selectTime_deliverypickups.Enabled = false;
        btn_selectTime_deliverypickups.Visible = false;
        lbl_slotsopen_deliverypickups.Enabled = false;
        lbl_slotsopen_deliverypickups.Visible = false;
        lbl_SelectedDate_deliverypickups.Enabled = false;
        lbl_SelectedDate_deliverypickups.Visible = false;
        btn_finish_deliverypickups.Enabled = false;
        btn_finish_deliverypickups.Visible = false;
        try
        {
            string[] temp = tb_Date1_deliverypickups.Text.Split('/');
            buildTimeSlot(Convert.ToInt32(temp[0]), Convert.ToInt32(temp[1]), Convert.ToInt32(temp[2]));
        }
        catch
        {
        }
        Step3_deliverypickups.Attributes["style"] = "display: none;";
    }

    protected void lb_clearDate_Click(object sender, EventArgs e)
    {
        disableControls();
        Step3_deliverypickups.Attributes["style"] = "display: none;";
    }

    protected void btn_selectTime_Click(object sender, EventArgs e)
    {
        try
        {
            lbl_SelectedDate_deliverypickups.Enabled = true;
            lbl_SelectedDate_deliverypickups.Visible = true;
            btn_finish_deliverypickups.Enabled = true;
            btn_finish_deliverypickups.Visible = true;
            lbl_SelectedDate_deliverypickups.Text = "<h3>You have selected a " + getSchType() + " for " +
                                                    Convert.ToDateTime(tb_Date1_deliverypickups.Text)
                                                           .Date.ToLongDateString() + " at " +
                                                    dd_schTimeSlot_deliverypickups.SelectedValue + "</h3>";
            btn_finish_deliverypickups.Text = "Schedule " + getSchType();
        }
        catch
        {
        }
        Step3_deliverypickups.Attributes["style"] = "display: none;";
    }

    protected void btn_finish_Click(object sender, EventArgs e)
    {
        failMessage_deliverypickups.InnerHtml = string.Empty;
        if ((string.IsNullOrEmpty(schedule_name_deliverypickups.Value)) ||
            (string.IsNullOrEmpty(schedule_from_deliverypickups.Value)))
        {
            failMessage_deliverypickups.InnerHtml = "<span style='color: #FF0000;'>Please enter all the required fields</span>";
            Step3_deliverypickups.Attributes["style"] = "display: none;";
        }
        else
        {
            string date = tb_Date1_deliverypickups.Text + " " + dd_schTimeSlot_deliverypickups.SelectedValue;
            string phonenum = schedule_phone1_deliverypickups.Value + "-" + schedule_phone2_deliverypickups.Value + "-" +
                              schedule_phone3_deliverypickups.Value;
            string cn = ConfirmationNumber();
            string date2 = Convert.ToDateTime(tb_Date1_deliverypickups.Text).Date.ToShortDateString();
            if (cb_sendEmail_deliverypickups.Checked)
            {
                add_sendEmail(schedule_name_deliverypickups.Value, schedule_trucknum_deliverypickups.Value,
                              schedule_from_deliverypickups.Value, schedule_items_deliverypickups.Value, phonenum,
                              schedule_email_deliverypickups.Value, getSchType(), date,
                              schedule_comment_deliverypickups.Value, cn);
                displaySuccess(date2, cn);
            }
            else
            {
                _schedule.addItem(schedule_name_deliverypickups.Value, schedule_trucknum_deliverypickups.Value,
                                 schedule_from_deliverypickups.Value, schedule_items_deliverypickups.Value, phonenum,
                                 schedule_email_deliverypickups.Value, getSchType(), DateTime.Now.ToString(), date,
                                 schedule_comment_deliverypickups.Value, cn);
                displaySuccess(date2, cn);
            }
        }
    }

    private void displaySuccess(string date, string cn)
    {
        string success =
            "<span style='padding-top: 5px;'>Your appointment for " +
            getSchType() + " has been booked for " + date + " at " + dd_schTimeSlot_deliverypickups.SelectedValue +
            "<br />Confirmation number: <strong>" + cn + "</strong></span>";
        disableControls();
        CompleteSch_deliverypickups.InnerHtml = success;
        Step3_deliverypickups.Attributes["style"] = "display: block;";
    }

    protected void btn_newApp_Click(object sender, EventArgs e)
    {
        schedule_name_deliverypickups.Value = string.Empty;
        schedule_items_deliverypickups.Value = string.Empty;
        schedule_phone1_deliverypickups.Value = string.Empty;
        schedule_phone2_deliverypickups.Value = string.Empty;
        schedule_phone3_deliverypickups.Value = string.Empty;
        schedule_from_deliverypickups.Value = string.Empty;
        schedule_email_deliverypickups.Value = string.Empty;
        schedule_comment_deliverypickups.Value = string.Empty;
        schedule_trucknum_deliverypickups.Value = string.Empty;
        spd_deliverypickups.SelectedIndex = 0;
        cb_sendEmail_deliverypickups.Checked = false;
        disableControls();
        Step3_deliverypickups.Attributes["style"] = "display: none;";
    }

    private string getSchType()
    {
        string type = "Pickup";
        if (spd_deliverypickups.SelectedIndex == 0)
            type = "Delivery";

        return type;
    }

    private void disableControls()
    {
        tb_Date1_deliverypickups.Text = string.Empty;
        dd_schTimeSlot_deliverypickups.Items.Clear();
        lbl_slotsopen_deliverypickups.Text = string.Empty;
        dd_schTimeSlot_deliverypickups.Enabled = false;
        dd_schTimeSlot_deliverypickups.Visible = false;
        btn_selectTime_deliverypickups.Enabled = false;
        btn_selectTime_deliverypickups.Visible = false;
        lbl_slotsopen_deliverypickups.Enabled = false;
        lbl_slotsopen_deliverypickups.Visible = false;
        lbl_SelectedDate_deliverypickups.Enabled = false;
        lbl_SelectedDate_deliverypickups.Visible = false;
        btn_finish_deliverypickups.Enabled = false;
        btn_finish_deliverypickups.Visible = false;
        failMessage_deliverypickups.InnerHtml = string.Empty;
        CompleteSch_deliverypickups.InnerHtml = string.Empty;
    }

    protected void btn_modalClose_Click(object sender, EventArgs e)
    {
        schedule_name_deliverypickups.Value = string.Empty;
        schedule_items_deliverypickups.Value = string.Empty;
        schedule_phone1_deliverypickups.Value = string.Empty;
        schedule_phone2_deliverypickups.Value = string.Empty;
        schedule_phone3_deliverypickups.Value = string.Empty;
        schedule_from_deliverypickups.Value = string.Empty;
        schedule_email_deliverypickups.Value = string.Empty;
        schedule_comment_deliverypickups.Value = string.Empty;
        schedule_trucknum_deliverypickups.Value = string.Empty;
        spd_deliverypickups.SelectedIndex = 0;
        cb_sendEmail_deliverypickups.Checked = false;
        disableControls();
        Step3_deliverypickups.Attributes["style"] = "display: none;";
    }

    private string ConfirmationNumber()
    {
        string cn = "SM";
        cn += RandomNumber(100, 999);
        cn += HelperMethods.RandomString(3);
        return cn;
    }

    private string RandomNumber(int min, int max)
    {
        var random = new Random();
        return random.Next(min, max).ToString(CultureInfo.InvariantCulture);
    }

    private void add_sendEmail(string company, string trucknum, string Delfrom, string items, string phonenumber,
                               string email, string type, string schdate, string comment, string cn)
    {
        try
        {
            try
            {
                var message = new MailMessage();
                message.To.Add(email);

                string messagebody = string.Empty;
                messagebody += "<h3 style='color: #555;'><u>Confirmation for " + type +
                               "</u></h3><p>You have scheduled a " + type + " for " + schdate +
                               "<br />Confirmation Number: <strong>" + cn + "</strong></p><br />";
                messagebody += "<p> Attn Drivers:<br /> Please DO NOT arrive no more than 15-30 minutes before you scheduled appointment time. If you can not meet the appointment time and date please contact 816-581-6222 as they will be able to reschedule.<br /> <br /> Incoming shipments are checked againsted ASN's.<br /> We ask that you do not block out more times then needed.<br /> <br /> Thank you. </p>";
                messagebody += "<br /><span style='color: #777777; font-size: 10px;'>Please contact the sales department if you have any questions or concerns regaring this email.";

                ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Delivery/Pickup Confirmation</h1>", "Steel-MFG: Confirmation Email for " + type, messagebody);
                _schedule.addItem(company, trucknum, Delfrom, items, phonenumber, email, type, DateTime.Now.ToString(CultureInfo.InvariantCulture),
                                 schdate, comment, cn);
            }
            catch (Exception e) {
                _applog.AddError(e);
            }
        }
        catch (Exception e) {
            _applog.AddError(e);
        }
    }

    private void buildTimeSlot(int m, int d, int y)
    {
        string tempDate = m.ToString(CultureInfo.InvariantCulture) + "/" + d.ToString(CultureInfo.InvariantCulture) + "/" + y.ToString(CultureInfo.InvariantCulture);
        DateTime checkWeekends = Convert.ToDateTime(tempDate);
        if ((((m == DateTime.Now.Month) && (d >= DateTime.Now.Day) && (y >= DateTime.Now.Year))
             || ((m > DateTime.Now.Month) && (y >= DateTime.Now.Year)))
            && ((checkWeekends.DayOfWeek != DayOfWeek.Saturday) && (checkWeekends.DayOfWeek != DayOfWeek.Sunday)))
        {
            int openSpots = 0;
            int hour = 6;
            int hl = 6;
            int min = 30;
            int minLong = 30;
            string pmam = "AM";
            while (hl <= 22)
            {
                if (hour >= 12)
                {
                    pmam = "PM";
                }
                if (hour >= 13)
                {
                    hour = hour - 12;
                }
                string tempmin = min == 0 ? "00" : min.ToString(CultureInfo.InvariantCulture);
                if (minLong == 60)
                {
                    minLong = 0;
                }

                string date = m.ToString(CultureInfo.InvariantCulture) + "/" + d.ToString(CultureInfo.InvariantCulture) + "/" + y.ToString(CultureInfo.InvariantCulture) + " " + hour.ToString(CultureInfo.InvariantCulture) + ":" +
                              tempmin + " " + pmam;
                if (checkTimeSlot(date))
                {
                    if ((m == DateTime.Now.Month) && (d == DateTime.Now.Day) && (y == DateTime.Now.Year))
                    {
                        if ((hl == DateTime.Now.Hour) && (minLong > DateTime.Now.Minute))
                        {
                            var item = new ListItem(hour + ":" + tempmin + " " + pmam, hour + ":" + tempmin + " " + pmam);
                            dd_schTimeSlot_deliverypickups.Items.Add(item);
                            openSpots++;
                        }
                        else
                        {
                            if (hl > DateTime.Now.Hour)
                            {
                                var item = new ListItem(hour + ":" + tempmin + " " + pmam,
                                                        hour + ":" + tempmin + " " + pmam);
                                dd_schTimeSlot_deliverypickups.Items.Add(item);
                                openSpots++;
                            }
                        }
                    }
                    else
                    {
                        var item = new ListItem(hour + ":" + tempmin + " " + pmam, hour + ":" + tempmin + " " + pmam);
                        dd_schTimeSlot_deliverypickups.Items.Add(item);
                        openSpots++;
                    }
                }
                if ((hour + ":" + tempmin + " " + pmam) == "10:00 PM")
                {
                    break;
                }
                else
                {
                    min = min + 15;
                    minLong = minLong + 15;
                    if (min == 60)
                    {
                        min = 0;
                        hour = hour + 1;
                        hl = hl + 1;
                        if (minLong != 60)
                        {
                            minLong = 60;
                        }
                    }
                }
            }
            lbl_slotsopen_deliverypickups.Text = "<i>There are currently " + openSpots +
                                                 " time slots open (Max: 63)</i>";
            dd_schTimeSlot_deliverypickups.Enabled = true;
            dd_schTimeSlot_deliverypickups.Visible = true;
            btn_selectTime_deliverypickups.Enabled = true;
            btn_selectTime_deliverypickups.Visible = true;
            lbl_slotsopen_deliverypickups.Enabled = true;
            lbl_slotsopen_deliverypickups.Visible = true;
        }
        else
        {
            lbl_slotsopen_deliverypickups.Text = "<i>The selected date is not a valid chose</i>";
            lbl_slotsopen_deliverypickups.Enabled = true;
            lbl_slotsopen_deliverypickups.Visible = true;
        }
    }

    private bool checkTimeSlot(string date)
    {
        connString = rootWebConfig.ConnectionStrings.ConnectionStrings["ApplicationServices"];
        var connection = new SqlConnection(connString.ConnectionString);
        bool canContinue = true;
        try
        {
            connection.Open();
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(
                "SELECT * FROM Scheduler WHERE ScheduleDate='" + date + "'", connection))
            {
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    canContinue = false;
                    break;
                }
            }
            myReader.Close();
        }
        catch (Exception e) {
            _applog.AddError(e);
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        return canContinue;
    }

    #endregion
}