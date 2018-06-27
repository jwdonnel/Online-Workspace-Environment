using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SocialSignIn
/// </summary>
/// 

namespace SocialSignInApis {
    public class SocialSignIn {

        #region Constant Query Redirect Strings

        public const string GoogleRedirectQuery = "?SocialSignIn=Google";
        public const string TwitterRedirectQuery = "?SocialSignIn=Twitter";
        public const string FacebookRedirectQuery = "?SocialSignIn=Facebook";

        #endregion

        #region Attempt Login

        public static void GoogleSignIn() {
            Google.GoogleSignIn.Authorize();
        }
        public static void TwitterSignIn() {
            Twitter.TwitterSignIn.Authorize();
        }
        public static void FacebookSignIn() {
            Facebook.FacebookSignIn.Authorize();
        }

        #endregion

        public static void CheckSocialSignIn() {
            HttpRequest Request = HttpContext.Current.Request;

            if (Request != null) {
                switch (Request.QueryString["SocialSignIn"]) {
                    case "Google":
                        Google.GoogleSignIn.FinishSignIn();
                        break;

                    case "Twitter":
                        Twitter.TwitterSignIn.FinishSignIn();
                        break;

                    case "Facebook":
                        Facebook.FacebookSignIn.FinishSignIn();
                        break;
                }

                if (!string.IsNullOrEmpty(Request.QueryString["SocialSignIn"])) {
                    SetGroupLogin();
                    if (HttpContext.Current.Session != null && HttpContext.Current.Session["RSSFeedLogin"] != null && HttpContext.Current.Session["RSSFeedLogin"].ToString() == "true") {
                        HttpContext.Current.Session["RSSFeedLogin"] = null;
                        HelperMethods.PageRedirect("~/Apps/RSSFeed/RSSFeed.aspx");
                    }
                    else if (Request.QueryString.Count > 0) {
                        HelperMethods.PageRedirect(Request.Url.AbsoluteUri.Split('?')[0]);
                    }
                }
            }
        }

        private static void SetGroupLogin() {
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                string username = HttpContext.Current.User.Identity.Name;
                MemberDatabase _member = new MemberDatabase(username);
                if (!string.IsNullOrEmpty(_member.DefaultLoginGroup) && !GroupSessions.DoesUserHaveGroupLoginSessionKey(username)) {
                    GroupSessions.AddOrSetNewGroupLoginSession(username, _member.DefaultLoginGroup);
                }
            }
        }
    }
}