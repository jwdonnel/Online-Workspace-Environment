using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenWSE_Library.Core.BackgroundServices;
using System.Threading;
using OpenWSE_Tools.Logging;
using OpenWSE_Tools.BackgroundServiceDatabaseCalls;
using System.Web.Script.Serialization;
using System.IO;
using System.Diagnostics;
using System.Web.Security;
using System.Net.Mail;
using OpenWSE_Tools.Notifications;
using System.Text.RegularExpressions;

namespace OpenWSE_Tools.AppServices {

    /// <summary> Background Service for the RSS Feed App that automatically updates the feeds and stores them in memory
    /// </summary>
    public class RSSFeedUpdater : IBackgroundServiceState {

        private const string ServiceNamespace = "OpenWSE_Tools.AppServices.RSSFeedUpdater";

        #region Private Static Variables

        /// <summary> Needed to lock the current thread while its running
        /// </summary>
        private static object LockObject = new object();

        #endregion


        #region Required Variables

        /// <summary> Get or Set the current state of the object
        /// </summary>
        protected static BackgroundStates CurrentState {
            get;
            set;
        }

        #endregion


        #region Interface Methods

        public void StartService() {
            ThreadStart threadStart = new ThreadStart(RunThread);
            BackgroundServiceCalls.CreateServiceThread(threadStart, ServiceNamespace).Start();
        }
        public void StopService() { }

        #endregion


        #region Current Service Methods/Variables

        public const int ForceUpdateInterval = 15;

        public static BackgroundStates GetCurrentState {
            get {
                return CurrentState;
            }
        }

        public RSSFeedUpdater() { }

        /// <summary> Start running the Thread for the Background Service
        /// </summary>
        private void RunThread() {
            lock (LockObject) {
                BackgroundServiceCalls.UpdateThreadInformation();

                BackgroundServices _backgroundServices = new BackgroundServices();
                BackgroundServiceLog _serviceLog = new BackgroundServiceLog();

                bool logInfo = false;
                string message = string.Empty;

                while (CurrentState == BackgroundStates.Running && ForceUpdateInterval > 0) {
                    logInfo = _backgroundServices.DoesLogInformation(ServiceNamespace);
                    List<string[]> feedUrls = RSSFeeds.GetFeedLinksFromCategory(string.Empty, string.Empty);

                    RSSFeeds.FeedListDateUpdated = ServerSettings.ServerDateTime;
                    List<RSSItem> nodeList = new List<RSSItem>();
                    foreach (string[] strItem in feedUrls) {
                        if (CurrentState == BackgroundStates.Running) {
                            RSSFeeds.GetNewFeeds(strItem[0], strItem[1], strItem[2], nodeList, string.Empty);
                            if (logInfo) {
                                BackgroundServiceCalls.AttemptLogMessage(ServiceNamespace, string.Format("RSS feeds for {0} has been added to memory.", strItem[0]));
                            }
                        }
                        else {
                            break;
                        }
                    }

                    RSSFeeds.SaveOutLoadedList();

                    if (CurrentState == BackgroundStates.Error || CurrentState == BackgroundStates.Stopping || CurrentState == BackgroundStates.Stopped) {
                        break;
                    }

                    CurrentState = BackgroundStates.Sleeping;
                    _backgroundServices.UpdateState_DateOnly(ServiceNamespace);

                    if (logInfo) {
                        message = string.Format("Background State updated to <span class='state-{0}'>{0}</span>", CurrentState.ToString());
                        _serviceLog.AddItem(ServiceNamespace, message);
                    }

                    for (int i = 0; i < (ForceUpdateInterval * 2); i++) {
                        if (CurrentState == BackgroundStates.Stopped || CurrentState == BackgroundStates.Stopping || CurrentState == BackgroundStates.Error) {
                            break;
                        }

                        Thread.Sleep(30 * 1000);
                    }

                    if (CurrentState == BackgroundStates.Stopped || CurrentState == BackgroundStates.Stopping || CurrentState == BackgroundStates.Error) {
                        break;
                    }

                    CurrentState = BackgroundStates.Running;
                    _backgroundServices.UpdateState_DateOnly(ServiceNamespace);

                    if (logInfo) {
                        message = string.Format("Background State updated to <span class='state-{0}'>{0}</span>", CurrentState.ToString());
                        _serviceLog.AddItem(ServiceNamespace, message);
                    }
                }

                CurrentState = BackgroundStates.Stopped;
            }
        }

        private static Regex _htmlAlertRegex = new Regex("<.*?>", RegexOptions.Compiled);
        private static object _sendAlertMessagesLockObject = new object();
        public static void SendAlertMessages(RSSItem item) {
            Thread threadWorker = new Thread(() => {
                lock (_sendAlertMessagesLockObject) {
                    RunSendAlertMessagesThread(item);
                }
            });
            threadWorker.Name = "RSSAlertThread";
            threadWorker.Start();
        }
        private static void RunSendAlertMessagesThread(RSSItem item) {
            MembershipUserCollection userlist = Membership.GetAllUsers();
            string rssAppId = "app-rssfeed";
            Apps_Coll appInfo = new App(string.Empty).GetAppInformation(rssAppId);
            if (appInfo.AllowNotifications) {
                foreach (MembershipUser u in userlist) {
                    if (u.UserName.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                        MemberDatabase member = new MemberDatabase(u.UserName);
                        if (member.UserHasApp(rssAppId)) {
                            RSSAlerts alerts = new RSSAlerts(u.UserName);
                            List<string> keywords = alerts.GetKeywords_List();
                            foreach (string keyword in keywords) {
                                if (KeywordFoundInItem(item, keyword)) {
                                    string content = item.Content + item.Summary;
                                    content = _htmlAlertRegex.Replace(content, string.Empty);
                                    if (content.Length > 300) {
                                        content = content.Substring(0, 300) + "...";
                                    }
                                    content = content.Trim();

                                    string title = "<b>" + item.Title + "</b>";
                                    if (!string.IsNullOrEmpty(item.Link)) {
                                        title = "<b><a href='" + item.Link + "' target='_blank'>" + item.Title + "</a></b>";
                                    }

                                    string messagebody = "<h2>" + keyword + "</h2><div style='clear: both; height: 5px;'></div>" + title + "<br /><small class='rss-feed-alert-content'>" + content + "</small>";
                                    var message = new MailMessage();

                                    bool alreadyExists = false;
                                    UserNotificationMessages un = new UserNotificationMessages(u.UserName);
                                    List<string> messageList = un.getMessagesByNotificationID(rssAppId);
                                    foreach (string m in messageList) {
                                        if (m == messagebody) {
                                            alreadyExists = true;
                                            break;
                                        }
                                    }

                                    if (!alreadyExists) {
                                        string email = un.attemptAdd(rssAppId, messagebody, true);
                                        if (!string.IsNullOrEmpty(email)) {
                                            message.To.Add(email);
                                        }

                                        UserNotificationMessages.finishAdd(message, rssAppId, messagebody);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static bool KeywordFoundInItem(RSSItem item, string keyword) {
            keyword = keyword.ToLower();
            if (!string.IsNullOrEmpty(item.Content) && item.Content.ToLower().Contains(keyword)) {
                return true;
            }

            if (!string.IsNullOrEmpty(item.Creator) && item.Creator.ToLower().Contains(keyword)) {
                return true;
            }

            if (!string.IsNullOrEmpty(item.Summary) && item.Summary.ToLower().Contains(keyword)) {
                return true;
            }

            if (!string.IsNullOrEmpty(item.Title) && item.Title.ToLower().Contains(keyword)) {
                return true;
            }

            return false;
        }

        #endregion

    }

}