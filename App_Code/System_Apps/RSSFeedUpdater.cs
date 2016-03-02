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
            Thread ServiceThread = new Thread(threadStart);
            ServiceThread.Start();
        }
        public void StopService() { }

        #endregion


        #region Current Service Methods/Variables

        private int _forceUpdateInterval = 15;

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
                LoadAppParms();

                BackgroundServices _backgroundServices = new BackgroundServices();
                BackgroundServiceLog _serviceLog = new BackgroundServiceLog();

                bool logInfo = false;
                string message = string.Empty;

                while (CurrentState == BackgroundStates.Running && _forceUpdateInterval > 0) {
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

                    for (int i = 0; i < (_forceUpdateInterval * 2); i++) {
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

                    LoadAppParms();
                }

                CurrentState = BackgroundStates.Stopped;
            }
        }
        private void LoadAppParms() {
            AppParams appParams = new AppParams(false);
            appParams.GetAllParameters_ForApp("app-rssfeed");
            foreach (Dictionary<string, string> dr in appParams.listdt) {
                try {
                    string param = dr["Parameter"];
                    int indexOf = param.IndexOf("=") + 1;
                    string subParam = param.Substring(indexOf);
                    if (param.Replace("=" + subParam, string.Empty) == "OnlyUpdateInteveral") {
                        int tempOut = 15;
                        if (int.TryParse(subParam, out tempOut) && tempOut >= 0) {
                            _forceUpdateInterval = tempOut;
                        }
                    }
                }
                catch { }
            }
        }

        #endregion

    }

}