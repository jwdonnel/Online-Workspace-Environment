using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using OpenWSE_Library.Core.BackgroundServices;

namespace OpenWSE_Tools.AutoUpdates {

    /// <summary>
    /// Summary description for AutoUpdateSystem
    /// </summary>
    public class AutoUpdateSystem : IBackgroundServiceState {

        #region Variables
        public const string AutoUpdateServiceNamespace = "OpenWSE_Tools.AutoUpdates.AutoUpdateSystem";
        private string _handler;
        private string _controlId;
        private string _app;
        private Control _control;
        public static bool EnableDetailedLogging {
            get {
                try {
                    if (WebConfigurationManager.AppSettings["EnableDetailedLoggingAutoUpdateSystem"] != null) {
                        return HelperMethods.ConvertBitToBoolean(WebConfigurationManager.AppSettings["EnableDetailedLoggingAutoUpdateSystem"]);
                    }
                }
                catch { }

                return true;
            }
        }
        #endregion

        #region Required Variables

        /// <summary> Get or Set the current state of the object
        /// </summary>
        protected static BackgroundStates CurrentState {
            get;
            set;
        }

        /// <summary> Get or Set the current Thread Id of the service
        /// </summary>
        protected static int CurrentThreadId {
            get;
            set;
        }

        #endregion

        #region Required Interface Methods

        public void StartService() { }
        public void StopService() { }

        #endregion

        public AutoUpdateSystem(string controlId, string app, Control control) {
            _handler = ServerSettings.GetSitePath(HttpContext.Current.Request) + "/WebServices/AutoUpdate.asmx/StartAUSystem";
            _controlId = controlId;
            _app = app;
            _control = control;
        }

        public void StartAutoUpdates() {
            try {
                if (CurrentState == BackgroundStates.Running) {
                    string autoupdate = "openWSE.autoupdate('" + _controlId + "','" + _handler + "','" + _app + "');";
                    RegisterPostbackScripts.RegisterStartupScript(_control, autoupdate);
                }
            }
            catch (Exception e) {
                AppLog.AddError(e);
            }
        }

    }

}