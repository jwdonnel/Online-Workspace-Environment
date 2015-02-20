using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace OpenWSE_Tools.AutoUpdates {

    /// <summary>
    /// Summary description for AutoUpdateSystem
    /// </summary>
    public class AutoUpdateSystem {
        #region Private Variables
        private string _handler;
        private string _controlId;
        private string _app;
        private Control _control;
        #endregion

        public AutoUpdateSystem(string controlId, string app, Control control) {
            _handler = ServerSettings.GetSitePath(HttpContext.Current.Request) + "/WebServices/AutoUpdate.asmx/StartAUSystem";
            _controlId = controlId;
            _app = app;
            _control = control;
        }

        public void StartAutoUpdates() {
            try {
                ServerSettings ss = new ServerSettings();
                if (ss.AutoUpdates) {
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