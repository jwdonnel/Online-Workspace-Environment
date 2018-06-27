using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using System.Text;
using System.Web.Security;
using System.Net;
using System.Net.Mail;
using OpenWSE_Tools.Notifications;

public partial class Integrated_Pages_eRequest : System.Web.UI.Page {
    private List<string> listTypes = new List<string>();
    private List<string> listGrades = new List<string>();
    private List<string> listGauges = new List<string>();
    private List<string> listWidth = new List<string>();
    private List<string> listLength = new List<string>();

    private string currentSessionId;
    private ConnectionStringSettings connString;
    private SqlConnection connection;
    private readonly Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("~/");

    protected void Page_Load(object sender, EventArgs e) {
        connString = rootWebConfig.ConnectionStrings.ConnectionStrings["ApplicationServices"];
        connection = new SqlConnection(connString.ConnectionString);

        currentSessionId = Request.QueryString["sessionId"];
        if (GetSaved()) {
            btn_ViewCurrent.Visible = true;
            Dictionary<string, string> ci = GetContactInfo();
            if (ci.Count > 0) {
                tb_Name.Text = ci["Name"];
                tb_Email.Text = ci["Email"];
                tb_Company.Text = ci["Company"];
                tb_PNAreaCode.Text = ci["PhoneNumber1"];
                tb_PN1.Text = ci["PhoneNumber2"];
                tb_PN2.Text = ci["PhoneNumber3"];
                tb_Comment.Text = ci["Comment"];
                string message = GetDraft();
                GetCountRefresh(message);
            }
        }
        else {
            lbl_Total.Text = "<b class='pad-right-sml'>Current Inquires:</b>0";
            btn_ViewCurrent.Visible = false;
        }

        if (!IsPostBack) {
            CheckOld();
            RePopulateAll();
        }
    }

    private void RePopulateAll() {
        GetTypes();
        PopulateTypes();

        string type = ddl_Type.SelectedValue;
        GetGrades(type);
        PopulateGrades();

        string grade = ddl_Grade.SelectedValue;
        GetGauges(type, grade);
        PopulateGauges();

        string gauge = ddl_Gauge.SelectedValue;
        GetThickness(type, grade, gauge);
        GetWidths(type, grade, gauge);
        PopulateWidths();

        string width = ddl_Width.SelectedValue;
        GetLengths(type, grade, gauge, width);
        PopulateLengths();

        string length = ddl_Length.SelectedValue;
        GetWeightPerSheet(type, grade, gauge, width, length);

        lbl_totalWeight.Text = string.Empty;
    }


    #region Dropdown Changes
    protected void ddl_Type_Changed(object sender, EventArgs e) {
        string type = ddl_Type.SelectedValue;
        GetGrades(type);
        PopulateGrades();

        string grade = ddl_Grade.SelectedValue;
        GetGauges(type, grade);
        PopulateGauges();

        string gauge = ddl_Gauge.SelectedValue;
        GetThickness(type, grade, gauge);
        GetWidths(type, grade, gauge);
        PopulateWidths();

        string width = ddl_Width.SelectedValue;
        GetLengths(type, grade, gauge, width);
        PopulateLengths();

        string length = ddl_Length.SelectedValue;
        GetWeightPerSheet(type, grade, gauge, width, length);
    }

    protected void ddl_Grade_Changed(object sender, EventArgs e) {
        string type = ddl_Type.SelectedValue;
        string grade = ddl_Grade.SelectedValue;
        GetGauges(type, grade);
        PopulateGauges();

        string gauge = ddl_Gauge.SelectedValue;
        GetThickness(type, grade, gauge);
        GetWidths(type, grade, gauge);
        PopulateWidths();

        string width = ddl_Width.SelectedValue;
        GetLengths(type, grade, gauge, width);
        PopulateLengths();

        string length = ddl_Length.SelectedValue;
        GetWeightPerSheet(type, grade, gauge, width, length);
    }

    protected void ddl_Gauge_Changed(object sender, EventArgs e) {
        string type = ddl_Type.SelectedValue;
        string grade = ddl_Grade.SelectedValue;
        string gauge = ddl_Gauge.SelectedValue;
        GetThickness(type, grade, gauge);
        GetWidths(type, grade, gauge);
        PopulateWidths();

        string width = ddl_Width.SelectedValue;
        GetLengths(type, grade, gauge, width);
        PopulateLengths();

        string length = ddl_Length.SelectedValue;
        GetWeightPerSheet(type, grade, gauge, width, length);
    }

    protected void ddl_Width_Changed(object sender, EventArgs e) {
        string type = ddl_Type.SelectedValue;
        string grade = ddl_Grade.SelectedValue;
        string gauge = ddl_Gauge.SelectedValue;
        string width = ddl_Width.SelectedValue;
        GetLengths(type, grade, gauge, width);
        PopulateLengths();

        string length = ddl_Length.SelectedValue;
        GetWeightPerSheet(type, grade, gauge, width, length);
    }
    #endregion


    #region SQL Calls
    private void GetTypes() {
        listTypes.Clear();
        string cmdText = "SELECT Type FROM SteelInventory";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    string temp = myReader["Type"].ToString().Trim();
                    if (!listTypes.Contains(temp))
                        listTypes.Add(temp);
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch { }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
    }

    private void GetGrades(string type) {
        listGrades.Clear();
        string cmdText = "SELECT Grade FROM SteelInventory WHERE Type=@Type";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("Type", type));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    string temp = myReader["Grade"].ToString().Trim();
                    if (!listGrades.Contains(temp))
                        listGrades.Add(temp);
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch { }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
    }

    private void GetGauges(string type, string grade) {
        listGauges.Clear();
        string cmdText = "SELECT Gauge FROM SteelInventory WHERE Type=@Type AND Grade=@Grade";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("Type", type));
                myCommand.Parameters.Add(new SqlParameter("Grade", grade));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    string temp = myReader["Gauge"].ToString().Trim();
                    if (!listGauges.Contains(temp))
                        listGauges.Add(temp);
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch { }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
    }

    private void GetThickness(string type, string grade, string gauge) {
        string thickness = "N/A";
        string cmdText = "SELECT Thickness FROM SteelInventory WHERE Type=@Type AND Grade=@Grade AND Gauge=@Gauge";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("Type", type));
                myCommand.Parameters.Add(new SqlParameter("Grade", grade));
                myCommand.Parameters.Add(new SqlParameter("Gauge", gauge));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    thickness = myReader["Thickness"].ToString().Trim();
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch { }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }

        lbl_Thickness.Text = thickness + " Inches";
    }

    private void GetWidths(string type, string grade, string gauge) {
        listWidth.Clear();
        string cmdText = "SELECT Width FROM SteelInventory WHERE Type=@Type AND Grade=@Grade AND Gauge=@Gauge";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("Type", type));
                myCommand.Parameters.Add(new SqlParameter("Grade", grade));
                myCommand.Parameters.Add(new SqlParameter("Gauge", gauge));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    string temp = myReader["Width"].ToString().Trim();
                    if (!listWidth.Contains(temp))
                        listWidth.Add(temp);
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch { }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
    }

    private void GetLengths(string type, string grade, string gauge, string width) {
        bool ctlHint = false;
        listLength.Clear();
        string cmdText = "SELECT Length FROM SteelInventory WHERE Type=@Type AND Grade=@Grade AND Gauge=@Gauge AND Width=@Width";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("Type", type));
                myCommand.Parameters.Add(new SqlParameter("Grade", grade));
                myCommand.Parameters.Add(new SqlParameter("Gauge", gauge));
                myCommand.Parameters.Add(new SqlParameter("Width", width));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    string temp = myReader["Length"].ToString().Trim();
                    if (temp.ToLower() == "ctl")
                        ctlHint = true;

                    if (!listLength.Contains(temp))
                        listLength.Add(temp);
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch { }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }

        if (ctlHint)
            lbl_ctlHint.Text = "<i>Note: CTL = Cut To Length</i><br />";
        else
            lbl_ctlHint.Text = string.Empty;
    }

    private void GetWeightPerSheet(string type, string grade, string gauge, string width, string length) {
        string weight = "";
        string cmdText = "SELECT WeightPerSheet FROM SteelInventory WHERE Type=@Type AND Grade=@Grade AND Gauge=@Gauge AND Width=@Width AND Length=@Length";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("Type", type));
                myCommand.Parameters.Add(new SqlParameter("Grade", grade));
                myCommand.Parameters.Add(new SqlParameter("Gauge", gauge));
                myCommand.Parameters.Add(new SqlParameter("Width", width));
                myCommand.Parameters.Add(new SqlParameter("Length", length));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    weight = myReader["WeightPerSheet"].ToString().Trim();
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch { }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }

        if (!string.IsNullOrEmpty(weight)) {
            string str = "<b class='pad-right'>Weight Per Sheet:</b><span id='weightPer'>" + weight + "</span> lbs";
            str += GetLBSPerSqft(type, grade, gauge, width, length);
            lbl_weightPerSheet.Text = str;
        }
        else {
            GetMinRun(type, grade, gauge, width, length);
        }
    }

    private string GetLBSPerSqft(string type, string grade, string gauge, string width, string length) {
        string perSqft = "";
        string cmdText = "SELECT lBSperSQFT FROM SteelInventory WHERE Type=@Type AND Grade=@Grade AND Gauge=@Gauge AND Width=@Width AND Length=@Length";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("Type", type));
                myCommand.Parameters.Add(new SqlParameter("Grade", grade));
                myCommand.Parameters.Add(new SqlParameter("Gauge", gauge));
                myCommand.Parameters.Add(new SqlParameter("Width", width));
                myCommand.Parameters.Add(new SqlParameter("Length", length));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    perSqft = myReader["lBSperSQFT"].ToString().Trim();
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch { }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }

        if (!string.IsNullOrEmpty(perSqft)) {
            return "<br /><b class='pad-right'>lbs Per Sq. ft:</b>" + perSqft + " lbs";
        }
        else
            return "";
    }

    private void GetMinRun(string type, string grade, string gauge, string width, string length) {
        string minRun = "";
        string cmdText = "SELECT MinRun FROM SteelInventory WHERE Type=@Type AND Grade=@Grade AND Gauge=@Gauge AND Width=@Width AND Length=@Length";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("Type", type));
                myCommand.Parameters.Add(new SqlParameter("Grade", grade));
                myCommand.Parameters.Add(new SqlParameter("Gauge", gauge));
                myCommand.Parameters.Add(new SqlParameter("Width", width));
                myCommand.Parameters.Add(new SqlParameter("Length", length));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    minRun = myReader["MinRun"].ToString().Trim();
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch { }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }

        if (!string.IsNullOrEmpty(minRun)) {
            lbl_weightPerSheet.Text = "<b class='pad-right'>Minimum Weight:</b>" + minRun;
            lbl_totalWeight.Text = string.Empty;
        }
        else
            lbl_weightPerSheet.Text = string.Empty;
    }
    #endregion


    #region Populate Dropdown Lists
    private void PopulateTypes() {
        ddl_Type.Items.Clear();
        foreach (string type in listTypes) {
            ListItem item = new ListItem(type, type);
            if (!ddl_Type.Items.Contains(item))
                ddl_Type.Items.Add(item);
        }
    }

    private void PopulateGrades() {
        ddl_Grade.Items.Clear();
        foreach (string grade in listGrades) {
            ListItem item = new ListItem(grade, grade);
            if (!ddl_Grade.Items.Contains(item))
                ddl_Grade.Items.Add(item);
        }
    }

    private void PopulateGauges() {
        ddl_Gauge.Items.Clear();
        foreach (string gauge in listGauges) {
            ListItem item = new ListItem(gauge, gauge);
            if (!ddl_Gauge.Items.Contains(item))
                ddl_Gauge.Items.Add(item);
        }
    }

    private void PopulateWidths() {
        ddl_Width.Items.Clear();
        foreach (string width in listWidth) {
            ListItem item = new ListItem(width, width);
            if (!ddl_Width.Items.Contains(item))
                ddl_Width.Items.Add(item);
        }
    }

    private void PopulateLengths() {
        ddl_Length.Items.Clear();
        foreach (string length in listLength) {
            ListItem item = new ListItem(length, length);
            if (!ddl_Length.Items.Contains(item))
                ddl_Length.Items.Add(item);
        }
    }
    #endregion


    #region Send/Clear Form
    protected void btn_Add_Clicked(object sender, EventArgs e) {
        string company = tb_Company.Text.Trim();
        string name = tb_Name.Text.Trim();
        string email = tb_Email.Text.Trim();
        string pnAreaCode = tb_PNAreaCode.Text.Trim();
        string phonenumber1 = tb_PN1.Text.Trim();
        string phonenumber2 = tb_PN2.Text.Trim();
        string comment = tb_Comment.Text.Trim();

        if ((string.IsNullOrEmpty(company)) || (string.IsNullOrEmpty(name)) || (string.IsNullOrEmpty(email))
            || (string.IsNullOrEmpty(pnAreaCode)) || (string.IsNullOrEmpty(phonenumber1)) || (string.IsNullOrEmpty(phonenumber2))) {
            Submit_Message.Text = "<span id='returnMessage' style='color:Red;margin-left:-12px'>Please fill out Contact Information</span>";
        }
        else {
            Submit_Message.Text = "";

            string type = ddl_Type.SelectedValue;
            string grade = ddl_Grade.SelectedValue;
            string gauge = ddl_Gauge.SelectedValue;
            string width = ddl_Width.SelectedValue;
            if (!string.IsNullOrEmpty(tb_WidthCustom.Text.Trim()))
                width = tb_WidthCustom.Text.Trim();

            string length = ddl_Length.SelectedValue;
            if (!string.IsNullOrEmpty(tb_LengthCustom.Text.Trim()))
                length = tb_LengthCustom.Text.Trim();

            string qty = tb_Quantity.Text.Trim();
            if (string.IsNullOrEmpty(qty))
                qty = "N/A";

            StringBuilder message = new StringBuilder();
            message.Append("<br /><b>Type: </b>" + type + "<br />");
            message.Append("<b>Grade: </b>" + grade + "<br />");
            message.Append("<b>Gauge: </b>" + gauge + "<br />");
            message.Append("<b>Size: </b>" + width + " x " + length + "<br />");
            message.Append("<b>Quantity: </b>" + qty + "<br />");

            if (GetSaved()) {
                string saved = GetDraft();
                updateDraft(saved + message.ToString(), name, email, company, pnAreaCode, phonenumber1, phonenumber2, comment);
            }
            else {
                addDraft(message.ToString(), name, email, company, pnAreaCode, phonenumber1, phonenumber2, comment);
                lbl_Total.Text = "<b class='pad-right-sml'>Current Inquires:</b>1";
            }
            btn_ViewCurrent.Visible = true;
            RePopulateAll();
        }
    }

    protected void btn_Send_Clicked(object sender, EventArgs e) {
        if (GetSaved())
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "AnyMoreRequests();", true);
        else
            Submit_Message.Text = "<span id='returnMessage' style='color:Red;margin-left:-12px'>Please add an inquire first</span>";
    }

    protected void btn_View_Clicked(object sender, EventArgs e) {
        pnl_entryHolder.Controls.Clear();
        string requests = GetDraft();
        GetCountRefresh(requests);
        string[] delim = { "<br /><br />" };
        string[] reqList = requests.Split(delim, StringSplitOptions.RemoveEmptyEntries);

        if (reqList.Length > 0) {
            int index = 0;
            foreach (string x in reqList) {
                string delete = "<div class='float-left pad-right' style='padding-top:15px'><img alt='delete' src='../App_Themes/Standard/Icons/Tables/delete.png' ";
                delete += "onclick='deleteEntry(\"" + index + "\");' style='cursor:pointer' /></div>";
                string message = "<div class='float-left' style='padding-right:75px'>" + x + "</div>";
                pnl_entryHolder.Controls.Add(new LiteralControl(delete + message));
                index++;
            }
        }
        else
            pnl_entryHolder.Controls.Add(new LiteralControl("There are no inquires available."));

        pnl_entry.Visible = false;
        pnl_view.Visible = true;
    }

    protected void btn_ViewEntry_Clicked(object sender, EventArgs e) {
        pnl_entryHolder.Controls.Clear();
        pnl_entry.Visible = true;
        pnl_view.Visible = false;
    }

    protected void btn_Reset_Clicked(object sender, EventArgs e) {
        if (GetSaved())
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "ResetConfirmation();", true);
        else
            ClearForm();
    }

    private void ClearForm() {
        tb_Company.Text = string.Empty;
        tb_Name.Text = string.Empty;
        tb_Email.Text = string.Empty;
        tb_PNAreaCode.Text = string.Empty;
        tb_PN1.Text = string.Empty;
        tb_PN2.Text = string.Empty;
        tb_Comment.Text = string.Empty;
        Submit_Message.Text = string.Empty;

        btn_ViewCurrent.Visible = false;
        lbl_Total.Text = "<b class='pad-right-sml'>Current Inquires:</b>0";
        tb_Quantity.Text = string.Empty;
        RePopulateAll();
    }

    protected void hf_SendRequest_Changed(object sender, EventArgs e) {
        Dictionary<string, string> ci = GetContactInfo();

        if (ci.Count > 0) {
            StringBuilder contactInfo = new StringBuilder();
            string phone = "(" + ci["PhoneNumber1"] + ")" + "-" + ci["PhoneNumber2"] + "-" + ci["PhoneNumber3"];
            contactInfo.Append("<div style='float:left;padding-right:100px'><h3><u>Contact Information</u></h3><br />");
            contactInfo.Append("<div><b>Name: </b>" + ci["Name"] + "<br /><b>Company: </b>" + ci["Company"]);
            contactInfo.Append("<br /><b>Phone: </b>" + phone + "<br /><b>E-mail: </b>" + ci["Email"] + "</div>");
            contactInfo.Append("<br />" + ci["Comment"] + "</div><div style='float:left'><h3><u>Inquire List</u></h3>");

            string requests = GetDraft();
            string finalMessage = contactInfo.ToString() + requests + "</div>";
            sendMessage("Inquire Request from " + ci["Name"] + " at " + ci["Company"], finalMessage, ServerSettings.ServerDateTime.ToString());

            deleteDraft();
            ClearForm();

            Submit_Message.Text = "<span id='returnMessage' style='color:Green;margin-left:-12px'>Your inquires have been submitted. Thank you.</span>";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "setTimeout(function(){RemoveMessage();},3000);", true);
        }

        hf_SendRequest.Value = string.Empty;
    }

    protected void hf_ResetRequest_Changed(object sender, EventArgs e) {
        if (hf_ResetRequest.Value == "all") {
            deleteDraft();
            ClearForm();
        }
        else {
            RePopulateAll();
        }

        hf_ResetRequest.Value = string.Empty;
    }

    protected void hf_DeleteRequest_Changed(object sender, EventArgs e) {
        pnl_entryHolder.Controls.Clear();
        string requests = GetDraft();
        StringBuilder rebuiltList = new StringBuilder();
        GetCountRefresh(requests);
        string[] delim = { "<br /><br />" };
        string[] reqList = requests.Split(delim, StringSplitOptions.RemoveEmptyEntries);

        if (reqList.Length > 0) {
            int index = 0;
            foreach (string x in reqList) {
                if (index.ToString() != hf_DeleteRequest.Value) {
                    string delete = "<div class='float-left pad-right' style='padding-top:15px'><img alt='delete' src='../App_Themes/Standard/Icons/Tables/delete.png' ";
                    delete += "onclick='deleteEntry(\"" + index + "\");' style='cursor:pointer' /></div>";
                    string message = "<div class='float-left' style='padding-right:75px'>" + x + "</div>";
                    rebuiltList.Append(x);
                    pnl_entryHolder.Controls.Add(new LiteralControl(delete + message));
                }
                index++;
            }

            updateDraft(rebuiltList.ToString());
        }

        requests = GetDraft();
        GetCountRefresh(requests);
        reqList = requests.Split(delim, StringSplitOptions.RemoveEmptyEntries);

        if (reqList.Length == 0)
            pnl_entryHolder.Controls.Add(new LiteralControl("There are no inquires available."));

        hf_DeleteRequest.Value = string.Empty;
    }

    private void sendMessage(string _subject, string _messagebody, string date) {
        var mailTo = new MailMessage();
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll) {
            if (!string.IsNullOrEmpty(u.Email) && (u.Email == "bdonnelly@steel-mfg.com" || u.Email == "ldonnelly@steel-mfg.com")) {
                mailTo.To.Add(u.Email);
            }
        }

        if (!string.IsNullOrEmpty(tb_Email.Text.Trim()) && tb_Email.Text.Contains("@")) {
            mailTo.To.Add(tb_Email.Text.Trim());
        }

        UserNotificationMessages.finishAdd(mailTo, "eRequest", _messagebody, _subject);
    }
    #endregion


    #region Save/Update eRequestDraft
    public void addDraft(string message, string name, string email, string company, string pn1, string pn2, string pn3, string comment) {
        try {
            if (string.IsNullOrEmpty(currentSessionId))
                currentSessionId = Guid.NewGuid().ToString();

            connection.Open();
            using (var command = new SqlCommand(
                "INSERT INTO eRequestDrafts VALUES(@ID, @Message, @Name, @Email, @Company, @PhoneNumber1, @PhoneNumber2, @PhoneNumber3, @Comment, @Date)", connection)) {
                command.Parameters.Add(new SqlParameter("ID", currentSessionId));
                command.Parameters.Add(new SqlParameter("Message", message));
                command.Parameters.Add(new SqlParameter("Name", name));
                command.Parameters.Add(new SqlParameter("Email", email));
                command.Parameters.Add(new SqlParameter("Company", company));
                command.Parameters.Add(new SqlParameter("PhoneNumber1", pn1));
                command.Parameters.Add(new SqlParameter("PhoneNumber2", pn2));
                command.Parameters.Add(new SqlParameter("PhoneNumber3", pn3));
                command.Parameters.Add(new SqlParameter("Comment", comment));
                command.Parameters.Add(new SqlParameter("Date", ServerSettings.ServerDateTime.ToString()));
                command.ExecuteNonQuery();
                command.Parameters.Clear();
                command.Dispose();
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
    }

    private string GetDraft() {
        string message = string.Empty;
        string cmdText = "SELECT Message FROM eRequestDrafts WHERE ID=@ID";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("ID", currentSessionId));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    message = myReader["Message"].ToString().Trim();
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
        GetCount(message);
        return message;
    }

    private void GetCount(string message) {
        string[] delim = { "<br /><br />" };
        string[] count = message.Split(delim, StringSplitOptions.RemoveEmptyEntries);
        lbl_Total.Text = "<b class='pad-right-sml'>Current Inquires:</b>" + (count.Length + 1).ToString();
    }

    private void GetCountRefresh(string message) {
        string[] delim = { "<br /><br />" };
        string[] count = message.Split(delim, StringSplitOptions.RemoveEmptyEntries);
        lbl_Total.Text = "<b class='pad-right-sml'>Current Inquires:</b>" + count.Length.ToString();
    }

    private Dictionary<string, string> GetContactInfo() {
        Dictionary<string, string> contactInfo = new Dictionary<string, string>();
        string cmdText = "SELECT Name, Email, Company, PhoneNumber1, PhoneNumber2, PhoneNumber3, Comment FROM eRequestDrafts WHERE ID=@ID";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("ID", currentSessionId));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    string name = myReader["Name"].ToString().Trim();
                    string email = myReader["Email"].ToString().Trim();
                    string company = myReader["Company"].ToString().Trim();
                    string pn1 = myReader["PhoneNumber1"].ToString().Trim();
                    string pn2 = myReader["PhoneNumber2"].ToString().Trim();
                    string pn3 = myReader["PhoneNumber3"].ToString().Trim();
                    string comment = myReader["Comment"].ToString().Trim();

                    contactInfo.Add("Name", name);
                    contactInfo.Add("Email", email);
                    contactInfo.Add("Company", company);
                    contactInfo.Add("PhoneNumber1", pn1);
                    contactInfo.Add("PhoneNumber2", pn2);
                    contactInfo.Add("PhoneNumber3", pn3);
                    contactInfo.Add("Comment", comment);
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }

        return contactInfo;
    }

    private bool GetSaved() {
        bool isSaved = false;

        if (!string.IsNullOrEmpty(currentSessionId)) {
            string cmdText = "SELECT ID FROM eRequestDrafts WHERE ID=@ID";
            try {
                SqlDataReader myReader = null;
                using (var myCommand = new SqlCommand(cmdText, connection)) {
                    connection.Open();
                    myCommand.Parameters.Add(new SqlParameter("ID", currentSessionId));
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read()) {
                        string id = myReader["ID"].ToString().Trim();
                        if (!string.IsNullOrEmpty(id))
                            isSaved = true;
                    }
                }
                myReader.Close();
                myReader.Dispose();
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
            finally {
                if (connection.State == ConnectionState.Open) {
                    connection.Close();
                }
            }
        }

        return isSaved;
    }

    public void updateDraft(string message) {
        string cmdText = "UPDATE eRequestDrafts SET Message=@Message WHERE ID=@ID";
        try {
            connection.Open();
            using (var command = new SqlCommand(cmdText, connection)) {
                command.Parameters.Add(new SqlParameter("Message", message));
                command.Parameters.Add(new SqlParameter("ID", currentSessionId));
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
    }

    public void updateDraft(string message, string name, string email, string company, string pn1, string pn2, string pn3, string comment) {
        string cmdText = "UPDATE eRequestDrafts SET Message=@Message, Name=@Name, Email=@Email, Company=@Company, PhoneNumber1=@PhoneNumber1, ";
        cmdText += "PhoneNumber2=@PhoneNumber2, PhoneNumber3=@PhoneNumber3, Comment=@Comment WHERE ID=@ID";
        try {
            connection.Open();
            using (var command = new SqlCommand(cmdText, connection)) {
                command.Parameters.Add(new SqlParameter("Message", message));
                command.Parameters.Add(new SqlParameter("Name", name));
                command.Parameters.Add(new SqlParameter("Email", email));
                command.Parameters.Add(new SqlParameter("Company", company));
                command.Parameters.Add(new SqlParameter("PhoneNumber1", pn1));
                command.Parameters.Add(new SqlParameter("PhoneNumber2", pn2));
                command.Parameters.Add(new SqlParameter("PhoneNumber3", pn3));
                command.Parameters.Add(new SqlParameter("Comment", comment));
                command.Parameters.Add(new SqlParameter("ID", currentSessionId));
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
    }

    public void deleteDraft() {
        string cmdText = "DELETE FROM eRequestDrafts WHERE ID=@ID";
        try {
            connection.Open();
            using (var command = new SqlCommand(cmdText, connection)) {
                command.Parameters.Add(new SqlParameter("ID", currentSessionId));
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
    }

    public void deleteDraft(string id) {
        string cmdText = "DELETE FROM eRequestDrafts WHERE ID=@ID";
        try {
            connection.Open();
            using (var command = new SqlCommand(cmdText, connection)) {
                command.Parameters.Add(new SqlParameter("ID", id));
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }
    }

    private void CheckOld() {
        List<string> idList = new List<string>();
        DateTime now = ServerSettings.ServerDateTime;

        string cmdText = "SELECT ID, Date FROM eRequestDrafts";
        try {
            SqlDataReader myReader = null;
            using (var myCommand = new SqlCommand(cmdText, connection)) {
                connection.Open();
                myCommand.Parameters.Add(new SqlParameter("ID", currentSessionId));
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    string id = myReader["ID"].ToString().Trim();
                    string date = myReader["Date"].ToString().Trim();
                    DateTime outTime = new DateTime();
                    if (DateTime.TryParse(date, out outTime)) {
                        TimeSpan final = now.Subtract(outTime);
                        if (final.Days >= 1)
                            idList.Add(id);
                    }
                }
            }
            myReader.Close();
            myReader.Dispose();
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
        finally {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
            }
        }

        if (idList.Count > 0) {
            foreach (string id in idList) {
                deleteDraft(id);
            }
        }
    }
    #endregion
}
