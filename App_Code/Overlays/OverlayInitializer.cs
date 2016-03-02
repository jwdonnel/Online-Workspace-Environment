using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace OpenWSE_Tools.Overlays {

    /// <summary>
    /// Summary description for OverlayInitializer
    /// </summary>
    public class OverlayInitializer {
        private readonly WorkspaceOverlays _overlays = new WorkspaceOverlays();
        public string _overlayId = "";

        public OverlayInitializer(string userName, string fileloc) {
            WorkspaceOverlay_Coll tempColl = _overlays.GetWorkspaceOverlayByFileLoc(fileloc);
            if (!string.IsNullOrEmpty(tempColl.ID)) {
                _overlayId = tempColl.ID;
            }

            userName = GroupSessions.GetUserGroupSessionName(userName);
            _overlays.GetUserOverlays(userName);
        }

        public bool TryLoadOverlay {
            get {
                foreach (UserOverlay_Coll coll in _overlays.UserOverlays) {
                    if (coll.OverlayID == _overlayId) {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}