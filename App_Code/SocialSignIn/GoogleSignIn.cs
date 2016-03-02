using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.SessionState;
using OpenWSE.API.GoogleLogin;

/// <summary>
/// Summary description for GoogleSignIn
/// </summary>
namespace SocialSignInApis.Google {

    public class GoogleSignIn {

        public static void Authorize() {
            HttpRequest Request = HttpContext.Current.Request;

            if (Request != null) {
                ServerSettings _ss = new ServerSettings();

                GoogleConnect.ClientId = _ss.GoogleClientId;
                GoogleConnect.ClientSecret = _ss.GoogleClientSecret;

                GoogleConnect googleConnect = new GoogleConnect(SocialRedirectUrl.GetRedirectUrl(SocialSignIn.GoogleRedirectQuery));
                googleConnect.Authorize("profile,email");
            }
        }
        public static void FinishSignIn() {
            HttpRequest Request = HttpContext.Current.Request;

            if (Request != null) {
                try {
                    string code = Request.QueryString["code"];
                    if (string.IsNullOrEmpty(code)) {
                        return;
                    }

                    ServerSettings _ss = new ServerSettings();

                    GoogleConnect.ClientId = _ss.GoogleClientId;
                    GoogleConnect.ClientSecret = _ss.GoogleClientSecret;

                    GoogleConnect googleConnect = new GoogleConnect(Request.Url.AbsoluteUri.Split('?')[0] + SocialSignIn.GoogleRedirectQuery);
                    string json = googleConnect.Fetch(code, "me");

                    GoogleProfile profile = new JavaScriptSerializer().Deserialize<GoogleProfile>(json);
                    if (profile != null && profile.Emails.Count > 0) {
                        foreach (Email email in profile.Emails) {
                            string username = email.Value.Replace(email.Value.Substring(email.Value.IndexOf("@")), "").Replace(" ", "_") + "@Google";

                            MembershipUser mUser = Membership.GetUser(username);
                            if (mUser != null) {
                                MemberDatabase _member = new MemberDatabase(mUser.UserName);
                                if (_member.IsSocialAccount) {
                                    FormsAuthentication.Authenticate(mUser.UserName, mUser.ResetPassword());
                                    FormsAuthentication.SetAuthCookie(mUser.UserName, true);
                                    new LoginActivity().AddItem(mUser.UserName, true, ActivityType.Social);
                                    MemberDatabase.AddUserSessionIds(username);
                                }
                            }
                            else {
                                // Create new user
                                string firstName = profile.DisplayName;
                                string lastName = string.Empty;
                                string[] names = profile.DisplayName.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                if (names.Length == 2) {
                                    firstName = names[0];
                                    lastName = names[1];
                                }
                                CreateAccount.CreateSocialUser.CreateNewUser(username, firstName, lastName, email.Value, profile.Image.Url);
                                MemberDatabase.AddUserSessionIds(username);
                                break;
                            }
                        }
                    }
                }
                catch (Exception e) {
                    AppLog.AddError(e);
                }
            }
        }

    }

}