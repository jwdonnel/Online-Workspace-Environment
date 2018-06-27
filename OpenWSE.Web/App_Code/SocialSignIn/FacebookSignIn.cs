using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using OpenWSE.API.FaceBookLogin;

/// <summary>
/// Summary description for TwitterSignIn
/// </summary>
namespace SocialSignInApis.Facebook {
    public class FacebookSignIn {

        public static void Authorize() {
            HttpRequest Request = HttpContext.Current.Request;

            if (Request != null) {
                ServerSettings _ss = new ServerSettings();

                FaceBookConnect.API_Key = _ss.FacebookAppId;
                FaceBookConnect.API_Secret = _ss.FacebookAppSecret;

                FaceBookConnect facebookConnect = new FaceBookConnect(SocialRedirectUrl.GetRedirectUrl(SocialSignIn.FacebookRedirectQuery));
                facebookConnect.Authorize("email");
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

                    FaceBookConnect.API_Key = _ss.FacebookAppId;
                    FaceBookConnect.API_Secret = _ss.FacebookAppSecret;

                    FaceBookConnect facebookConnect = new FaceBookConnect(Request.Url.AbsoluteUri.Split('?')[0] + SocialSignIn.FacebookRedirectQuery);
                    string json = facebookConnect.Fetch(code, "me");

                    FaceBookUser faceBookUser = ServerSettings.CreateJavaScriptSerializer().Deserialize<FaceBookUser>(json);

                    if (faceBookUser != null) {
                        string username = faceBookUser.name.Replace(" ", "_") + "@Facebook";

                        MembershipUser mUser = Membership.GetUser(username);
                        if (mUser != null) {
                            MemberDatabase _member = new MemberDatabase(mUser.UserName);
                            if (_member.IsSocialAccount) {
                                Membership.ValidateUser(mUser.UserName, mUser.ResetPassword());
                                FormsAuthentication.SetAuthCookie(mUser.UserName, true);
                                new LoginActivity().AddItem(mUser.UserName, true, ActivityType.Social);

                                if (string.IsNullOrEmpty(_member.AccountImage)) {
                                    _member.UpdateAcctImage(string.Format("https://graph.facebook.com/{0}/picture", faceBookUser.id));
                                }
                            }
                        }
                        else {
                            // Create new user
                            string profileImg = string.Format("https://graph.facebook.com/{0}/picture", faceBookUser.id);
                            CreateAccount.CreateSocialUser.CreateNewUser(username, faceBookUser.first_name, faceBookUser.last_name, faceBookUser.email, profileImg);
                        }

                        MemberDatabase.AddUserSessionIds(username);
                    }
                }
                catch (Exception e) {
                    AppLog.AddError(e);
                }
            }
        }

    }
}