using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Security;


/// <summary>
/// Summary description for CreateSocialUser
/// </summary>
/// 

namespace SocialSignInApis.CreateAccount {
    public class CreateSocialUser {

        public static void CreateNewUser(string username, string firstName, string lastName, string email, string img) {
            ServerSettings _ss = new ServerSettings();

            string role = _ss.UserSignUpRole;

            string password = HelperMethods.RandomString(20);
            Membership.CreateUser(username, password, email);

            UserRegistration ur = new UserRegistration(username, role);
            ur.RegisterNewUser(firstName, lastName, email, string.Empty);
            ur.RegisterDefaults();

            MembershipUser newuser = Membership.GetUser(username);
            if (newuser != null) {
                newuser.IsApproved = true;
                Membership.UpdateUser(newuser);
            }

            MemberDatabase member = new MemberDatabase(username);
            member.UpdateActivationCode(Guid.NewGuid().ToString());
            member.UpdateIsSocialAccount(true);
            if (!string.IsNullOrEmpty(img)) {
                member.UpdateAcctImage(img);
            }

            FormsAuthentication.Authenticate(username, password);
            FormsAuthentication.SetAuthCookie(username, true);
            new LoginActivity().AddItem(username, true, ActivityType.Social);

            if (_ss.EmailOnRegister) {
                MembershipUser adminUser = Membership.GetUser(ServerSettings.AdminUserName);
                if (!string.IsNullOrEmpty(adminUser.Email)) {
                    var message = new MailMessage();
                    message.To.Add(adminUser.Email);
                    StringBuilder newUserText = new StringBuilder();
                    newUserText.Append("The user " + newuser.UserName + " has been created. Username created by " + firstName + " " + lastName + ".");
                    newUserText.Append("<br /><b>User email:</b> " + email + "<br /><b>Date/Time Created:</b> " + ServerSettings.ServerDateTime.ToString());
                    newUserText.Append("<br /><b><span style='float:left;'>User Color: </span></b>");
                    newUserText.Append("<div style='float:left;height:12px;width:12px;background:#" + member.UserColor + ";margin-left:5px;margin-top:2px;-webkit-border-radius:50%;-moz-border-radius:50%;border-radius:50%;'></div>");
                    newUserText.Append("<br /><b>User Role:</b> " + role);
                    ServerSettings.SendNewEmail(message, "<h1 style='color:#555'>New User Created</h1>", OpenWSE.Core.Licensing.CheckLicense.SiteName + " : New Account Created", newUserText.ToString());
                }
            }
        }

    }
}