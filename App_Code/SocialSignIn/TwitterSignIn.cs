using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using OpenWSE.API.TwitterLogin;

/// <summary>
/// Summary description for TwitterSignIn
/// </summary>
/// 

namespace SocialSignInApis.Twitter {
    public class TwitterSignIn {

        public static void Authorize() {
            HttpRequest Request = HttpContext.Current.Request;

            if (Request != null) {
                ServerSettings _ss = new ServerSettings();
                TwitterConnect.API_Key = _ss.TwitterConsumerKey;
                TwitterConnect.API_Secret = _ss.TwitterConsumerSecret;

                if (!TwitterConnect.IsAuthorized) {
                    TwitterConnect twitter = new TwitterConnect();
                    twitter.Authorize(SocialRedirectUrl.GetRedirectUrl(SocialSignIn.TwitterRedirectQuery));
                }
            }
        }
        public static void FinishSignIn() {
            HttpRequest Request = HttpContext.Current.Request;

            if (Request != null) {
                try {
                    ServerSettings _ss = new ServerSettings();
                    TwitterConnect.API_Key = _ss.TwitterConsumerKey;
                    TwitterConnect.API_Secret = _ss.TwitterConsumerSecret;

                    if (TwitterConnect.IsAuthorized) {
                        TwitterConnect twitter = new TwitterConnect();
                        string json = twitter.FetchProfile();
                        TwitterUser twitterUser = new JavaScriptSerializer().Deserialize<TwitterUser>(json);

                        if (twitterUser != null) {
                            string userName = twitterUser.screen_name + "@Twitter";
                            if (string.IsNullOrEmpty(userName)) {
                                userName = twitterUser.name.Replace(" ", "_") + "@Twitter";
                            }

                            MembershipUser mUser = Membership.GetUser(userName);
                            if (mUser != null) {
                                MemberDatabase _member = new MemberDatabase(mUser.UserName);
                                if (_member.IsSocialAccount) {
                                    FormsAuthentication.Authenticate(mUser.UserName, mUser.ResetPassword());
                                    FormsAuthentication.SetAuthCookie(mUser.UserName, true);
                                    new LoginActivity().AddItem(mUser.UserName, true, ActivityType.Social);
                                }
                            }
                            else {
                                // Create new user
                                string firstName = twitterUser.name;
                                string lastName = string.Empty;
                                string[] names = twitterUser.name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                if (names.Length == 2) {
                                    firstName = names[0];
                                    lastName = names[1];
                                }
                                CreateAccount.CreateSocialUser.CreateNewUser(userName, firstName, lastName, string.Empty, twitterUser.profile_image_url);
                            }

                            MemberDatabase.AddUserSessionIds(userName);
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