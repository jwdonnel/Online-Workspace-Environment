using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Web.Security;
using System.Text;
using OpenWSE_Tools.Notifications;

public partial class SiteSettings_CreateAccount : System.Web.UI.Page
{
    private readonly Notifications _notifications = new Notifications();
    private readonly AppLog _applog = new AppLog(false);
    private readonly IPWatch _ipwatch = new IPWatch(true);
    private ServerSettings _ss = new ServerSettings();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!_ss.AllowUserSignUp)
            Page.Response.Redirect("~/ErrorPages/Blocked.html");

        if (_ss.UserSignUpEmailChecker)
            lbl_email_assocation.Text = "Must have a valid <b>" + _ss.UserSignUpEmailAssociation + "</b> email address to register.";

        if (_ss.SignUpConfirmationEmail) {
            RegisterUser.CompleteSuccessText = "Your account has been successfully created. Please check your email to activate your account. If you do not recieve an email to activate your account, please click on the Forgot Password to left.";
        }

        if (!IsPostBack)
            GetStartupScripts_JS();
    }

    #region Register New User
    protected void RegisterUser_CreatedUser(object sender, EventArgs e)
    {
        string username = RegisterUser.UserName;
        string email = RegisterUser.Email;
        var tb_firstnamereg = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("tb_firstnamereg");
        var tb_lastnamereg = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("tb_lastnamereg");
        var color1 = (TextBox)RegisterUserWizardStep.ContentTemplateContainer.FindControl("color1");
        string role = _ss.UserSignUpRole;

        UserRegistration ur = new UserRegistration(username, role);
        ur.RegisterNewUser(tb_firstnamereg.Text, tb_lastnamereg.Text, email, color1.Text);
        ur.RegisterDefaults();

        MembershipUser newuser = Membership.GetUser(username);

        bool signUpConfirmEmail = _ss.SignUpConfirmationEmail;

        if (newuser != null)
        {
            if (signUpConfirmEmail) {
                newuser.IsApproved = false;
            }
            else {
                newuser.IsApproved = true;
            }
            Membership.UpdateUser(newuser);
        }

        MemberDatabase member = new MemberDatabase(username);
        string code = Guid.NewGuid().ToString();

        member.UpdateActivationCode(code);

        if (signUpConfirmEmail) {
            string url = ServerSettings.GetSitePath(Request) + "/";
            string logo = "<img alt='logo' src='" + url + "Standard_Images/logo.png' style='padding-right:5px;max-height:60px!important;height:60px!important;float:left!important' />";
            logo += "<h1 style='color:#555'>Registration Confirmation</h1><div style='clear:both;height:20px'></div>";

            string str = "<p><h2 style='color:#353535'>Thank you for registering an account with " + ServerSettings.SiteName + ". Please click on the link below to activate your account.</h2></p>";
            string link = "http:" + ServerSettings.GetSitePath(Request) + "/Default.aspx?ActivateUser=" + username + "&ActivationCode=" + code;
            string fakelink = "http:" + ServerSettings.GetSitePath(Request) + "/Default.aspx";
            str += "<p>Click on the link to activate account: <a href='" + link + "'>" + fakelink + "</a></p>";
            SendEmail_ActivateUser(newuser, str);
        }

        if (_ss.EmailOnRegister)
        {
            MembershipUser adminUser = Membership.GetUser(ServerSettings.AdminUserName.ToLower());
            if (!string.IsNullOrEmpty(adminUser.Email))
            {
                var message = new MailMessage();
                message.To.Add(adminUser.Email);
                StringBuilder newUserText = new StringBuilder();
                newUserText.Append("The user " + newuser.UserName + " has been created. Username created by " + tb_firstnamereg.Text + " " + tb_lastnamereg.Text + ".");
                newUserText.Append("<br /><b>User email:</b> " + email + "<br /><b>Date/Time Created:</b> " + DateTime.Now.ToString());
                newUserText.Append("<br /><b><span style='float:left;'>User Color: </span></b>");
                newUserText.Append("<div style='float:left;height:12px;width:12px;background:#" + color1.Text + ";margin-left:5px;margin-top:2px;-webkit-border-radius:50%;-moz-border-radius:50%;border-radius:50%;' title='" + color1.Text + "'></div>");
                newUserText.Append("<br /><b>User Role:</b> " + role);
                ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>New User Created</h1>", ServerSettings.SiteName + " : New Account Created", newUserText.ToString());
            }
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "register_Finish();");
    }

    protected void RegisterUser_CreatingUser(object sender, LoginCancelEventArgs e)
    {
        string script = "register();";
        if (_ss.AllowUserSignUp)
        {
            bool canContinue = true;
            if (_ss.UserSignUpEmailChecker)
            {
                string emailAssociation = _ss.UserSignUpEmailAssociation.ToLower();
                if (!RegisterUser.Email.ToLower().Contains(emailAssociation))
                {
                    e.Cancel = true;
                    script += "$('#failureMessage').html('User must have a valid " + emailAssociation + " email address');";
                    canContinue = false;
                }
            }

            if ((RegisterUser.UserName.ToLower() == "newuserdefaults") || (RegisterUser.UserName.ToLower() == "demouser")) {
                canContinue = false;
                e.Cancel = true;
                script += "$('#failureMessage').html('User is already registered');";
            }

            if (canContinue)
            {
                MembershipUser _user = Membership.GetUser(RegisterUser.UserName);
                if (_user != null)
                {
                    e.Cancel = true;
                    script += "$('#failureMessage').html('User is already registered');";
                }
                else
                {
                    e.Cancel = false;
                    script = "";
                }
            }
        }
        else
        {
            e.Cancel = true;
            script += "$('#failureMessage').html('Not allowed to register user.');";
        }

        if (!string.IsNullOrEmpty(script)) {
            RegisterPostbackScripts.RegisterStartupScript(this, script);
        }
    }

    protected void RegisterUser_Continue(object sender, EventArgs e)
    {
        Page.Response.Redirect("~/SiteSettings/iframes/CreateAccount.aspx");
    }

    private void SendEmail_ActivateUser(MembershipUser u, string text)
    {
        var message = new MailMessage();
        message.To.Add(u.Email);

        ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>Account Activation</h1>", ServerSettings.SiteName + " : Account Activation", text);
    }
    #endregion


    #region Dynamically Load Scripts

    private void GetStartupScripts_JS()
    {
        var startupscripts = new StartupScripts(true);
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        foreach (StartupScripts_Coll coll in startupscripts.StartupscriptsList)
        {
            if (coll.ApplyTo == "All Components")
            {
                var sref = new ScriptReference(coll.ScriptPath);
                if (sm != null)
                    sm.Scripts.Add(sref);
            }
        }
        if (sm != null) sm.ScriptMode = ScriptMode.Release;
    }

    #endregion

}