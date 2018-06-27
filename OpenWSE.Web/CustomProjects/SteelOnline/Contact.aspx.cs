using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Web.Security;
using System.Net;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Notifications;

public partial class Integrated_Pages_Contact : System.Web.UI.Page {
    private readonly UserUpdateFlags _uuf = new UserUpdateFlags();
    private AppLog _applog;

    protected void Page_Load(object sender, EventArgs e) {
        _applog = new AppLog(false);
    }

    protected void btn_Send_Clicked(object sender, EventArgs e) {
        string company = tb_Company.Text.Trim();
        string address = tb_Address.Text.Trim();
        string city = tb_City.Text.Trim();
        string state = dd_State.Value;
        string postalCode = tb_PostalCode.Text.Trim();
        string name = tb_Name.Text.Trim();
        string email = tb_Email.Text.Trim();
        string pnAreaCode = tb_PNAreaCode.Text.Trim();
        string phonenumber1 = tb_PN1.Text.Trim();
        string phonenumber2 = tb_PN2.Text.Trim();
        string comment = tb_Comment.Text.Trim();

        if ((string.IsNullOrEmpty(company)) || (string.IsNullOrEmpty(address)) || (string.IsNullOrEmpty(city)) || (string.IsNullOrEmpty(postalCode))
            || (string.IsNullOrEmpty(name)) || (string.IsNullOrEmpty(email)) || (string.IsNullOrEmpty(pnAreaCode)) || (string.IsNullOrEmpty(phonenumber1))
            || (string.IsNullOrEmpty(phonenumber2)) || (string.IsNullOrEmpty(comment))) {
            Submit_Message.Text = "<span id='returnMessage' style='color:Red'>Please fill out entire form</span>";
        }
        else {
            string pn = "(" + pnAreaCode + ")" + " - " + phonenumber1 + " - " + phonenumber2;
            SendMessage(name, company, address, city, state, postalCode, pn, email, comment);
            ClearForm();
            Submit_Message.Text = "<span id='returnMessage' style='color:Green'>Your question/comment has been sent.</span>";
        }
        ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), "RemoveMessage();", true);
    }

    protected void btn_Reset_Clicked(object sender, EventArgs e) {
        ClearForm();
    }

    private void ClearForm() {
        tb_Company.Text = string.Empty;
        tb_Address.Text = string.Empty;
        tb_City.Text = string.Empty;
        dd_State.SelectedIndex = 0;
        tb_PostalCode.Text = string.Empty;
        tb_Name.Text = string.Empty;
        tb_Email.Text = string.Empty;
        tb_PNAreaCode.Text = string.Empty;
        tb_PN1.Text = string.Empty;
        tb_PN2.Text = string.Empty;
        tb_Comment.Text = string.Empty;
        Submit_Message.Text = string.Empty;
    }

    private void SendMessage(string name, string company, string add, string city, string state, string zip, string phone,
                               string email, string question) {
        try {
            string message = "<div><b>Name: </b>" + name + "<br /><b>Company: </b>" + company;
            message += "<br /><b>Phone: </b>" + phone + "<br /><b>E-mail: </b>" + email + "</div>";
            message += "<div style='clear:both'></div><br />" + question + "<br />";
            message += "<div style='clear:both;padding: 15px 0;'>" + add + "<br />" + city + ", " + state + " " + zip + "</div>";

            sendMessage("Question/Comment from " + name + " at " + company, message, ServerSettings.ServerDateTime.ToString());
        }
        catch {
        }
    }

    private void sendMessage(string _subject, string _messagebody, string date) {
        var mailTo = new MailMessage();
        MembershipUserCollection coll = Membership.GetAllUsers();
        foreach (MembershipUser u in coll) {
            if (!string.IsNullOrEmpty(u.Email) && (u.Email == "bdonnelly@steel-mfg.com" || u.Email == "ldonnelly@steel-mfg.com")) {
                mailTo.To.Add(u.Email);
            }
        }

        if (!string.IsNullOrEmpty(tb_Email.Text.Trim()) && tb_Email.Text.Contains("@"))
            mailTo.To.Add(tb_Email.Text.Trim());

        UserNotificationMessages.finishAdd(mailTo, "Contact Us", _messagebody, _subject);
    }
}