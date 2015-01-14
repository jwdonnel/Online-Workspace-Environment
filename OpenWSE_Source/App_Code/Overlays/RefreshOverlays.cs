using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Collections;
using OpenWSE_Tools.Overlays;

/// <summary>
/// Summary description for RefreshOverlays
/// </summary>
public class RefreshOverlays : Page {
    private List<string> _loadedControls = new List<string>();
    private ServerSettings _ss = new ServerSettings();
    private string _username;
    private MemberDatabase _member;
    private App _apps;
    private string _serverPath;
    private Page _page;
    private string _panelId;

    public RefreshOverlays(string username, string serverPath, Page page, string panelId) {
        if (!string.IsNullOrEmpty(username)) {
            _username = username;
            _member = new MemberDatabase(username);
            _apps = new App(_username);
        }
        else
            _apps = new App();

        _serverPath = serverPath;
        _page = page;
        _panelId = panelId;
    }

    public string DynamicReloadOverlays() {
        StringBuilder strReload = new StringBuilder();

        PlaceHolder PlaceHolder1 = (PlaceHolder)_page.Master.FindControl("PlaceHolder1");
        if (PlaceHolder1 != null) {
            PlaceHolder1.Controls.Clear();

            WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
            bool AssociateWithGroups = _ss.AssociateWithGroups;
            _workspaceOverlays.GetUserOverlays(_username);
            List<string> userApps = _member.EnabledApps;

            List<string> LoadedList = new List<string>();
            _loadedControls.Clear();

            string idsToLoad = string.Empty;

            foreach (string w in userApps) {
                var table = _apps.GetAppInformation(w);
                if (table != null) {
                    string overlayId = table.OverlayID;
                    if (!string.IsNullOrEmpty(overlayId)) {

                        if (AssociateWithGroups) {
                            if (!ServerSettings.CheckAppGroupAssociation(table, _member)) {
                                continue;
                            }
                        }

                        string[] oIds = overlayId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string oId in oIds) {
                            WorkspaceOverlay_Coll coll = _workspaceOverlays.GetWorkspaceOverlay(oId);
                            if ((!string.IsNullOrEmpty(coll.OverlayName)) && (!LoadedList.Contains(oId))) {
                                LoadedList.Add(oId);
                                if (!string.IsNullOrEmpty(coll.FileLocation)) {
                                    string filePath = _serverPath + coll.FileLocation.Replace("/", "\\");
                                    if (File.Exists(filePath)) {
                                        foreach (UserOverlay_Coll userOverlays in _workspaceOverlays.UserOverlays) {
                                            if (userOverlays.OverlayID == coll.ID) {
                                                UserControl uc = (UserControl)LoadControl("~/" + coll.FileLocation);
                                                if (uc.Controls.Count > 0) {
                                                    string displayType = "workspace-overlays";
                                                    if (!string.IsNullOrEmpty(coll.DisplayType)) {
                                                        displayType = coll.DisplayType;
                                                    }

                                                    uc.Controls.AddAt(0, new LiteralControl("<div class='workspace-overlay-selector " + displayType + " " + coll.ID + "'>"));
                                                    if (displayType.ToLower() != "workspace-overlays-custom") {
                                                        uc.Controls.AddAt(1, new LiteralControl(BuildHeader(coll.OverlayName)));
                                                    }
                                                    uc.Controls.Add(new LiteralControl("</div>"));
                                                }
                                                PlaceHolder1.Controls.Add(uc);
                                                idsToLoad += coll.ID + ",";
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(idsToLoad)) {
                strReload.Append("openWSE.TryAddLoadOverlay('" + idsToLoad + "');");
            }

        }
        return strReload.ToString();
    }

    public void DynamicReloadOverlay(UserOverlay_Coll userOverlay) {
        WorkspaceOverlays _do = new WorkspaceOverlays();
        WorkspaceOverlay_Coll coll = _do.GetWorkspaceOverlay(userOverlay.OverlayID);
        if (!string.IsNullOrEmpty(coll.OverlayName)) {
            FindPanelControl(_page.Master.Controls, coll);
        }
    }

    public void DynamicReloadOverlay_NoLogin(WorkspaceOverlay_Coll userOverlay) {
        WorkspaceOverlays _do = new WorkspaceOverlays();
        if (!string.IsNullOrEmpty(userOverlay.OverlayName)) {
            FindPanelControl(_page.Master.Controls, userOverlay);
        }
    }

    public void GetWorkspaceOverlays(List<Apps_Coll> userApps) {
        WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
        _workspaceOverlays.GetUserOverlays(_username);
        List<string> LoadedList = new List<string>();
        _loadedControls.Clear();

        foreach (Apps_Coll dr in userApps) {
            string overlayId = dr.OverlayID;
            if (!string.IsNullOrEmpty(overlayId)) {
                string[] oIds = overlayId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string oId in oIds) {
                    WorkspaceOverlay_Coll coll = _workspaceOverlays.GetWorkspaceOverlay(oId);
                    if ((!string.IsNullOrEmpty(coll.OverlayName)) && (!LoadedList.Contains(oId))) {
                        LoadedList.Add(oId);
                        if (!string.IsNullOrEmpty(coll.FileLocation)) {
                            string filePath = _serverPath + coll.FileLocation.Replace("/", "\\");
                            if (File.Exists(filePath)) {
                                foreach (UserOverlay_Coll userOverlays in _workspaceOverlays.UserOverlays) {
                                    if (userOverlays.OverlayID == coll.ID) {
                                        FindPanelControl(_page.Master.Controls, coll);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void GetWorkspaceOverlays_NoLogin(List<Apps_Coll> userApps) {
        WorkspaceOverlays _workspaceOverlays = new WorkspaceOverlays();
        NewUserDefaults _dc = new NewUserDefaults("DemoNoLogin");

        List<string> LoadedList = new List<string>();

        _loadedControls.Clear();

        foreach (Apps_Coll dr in userApps) {
            string overlayId = dr.OverlayID;
            if (!string.IsNullOrEmpty(overlayId)) {
                string[] oIds = overlayId.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string oId in oIds) {
                    WorkspaceOverlay_Coll coll = _workspaceOverlays.GetWorkspaceOverlay(oId);
                    if ((!string.IsNullOrEmpty(coll.OverlayName)) && (!LoadedList.Contains(oId))) {
                        LoadedList.Add(oId);
                        if (!string.IsNullOrEmpty(coll.FileLocation)) {
                            string filePath = _serverPath + coll.FileLocation.Replace("/", "\\");
                            if (File.Exists(filePath)) {
                                FindPanelControl(_page.Master.Controls, coll);
                            }
                        }
                    }
                }
            }
        }
    }

    private void FindPanelControl(ControlCollection page, WorkspaceOverlay_Coll coll) {
        foreach (Control c in page) {
            if (_loadedControls.Contains(coll.ID))
                return;
            else {
                if (c is Panel) {
                    Panel pnl = (Panel)c.FindControl(_panelId);
                    if (pnl != null) {
                        UserControl uc = (UserControl)LoadControl("~/" + coll.FileLocation);
                        if (uc.Controls.Count > 0) {
                            string displayType = "workspace-overlays";
                            if (!string.IsNullOrEmpty(coll.DisplayType)) {
                                displayType = coll.DisplayType;
                            }

                            uc.Controls.AddAt(0, new LiteralControl("<div class='workspace-overlay-selector " + displayType + " " + coll.ID + "'>"));
                            if (displayType.ToLower() != "workspace-overlays-custom") {
                                uc.Controls.AddAt(1, new LiteralControl(BuildHeader(coll.OverlayName)));
                            }
                            uc.Controls.Add(new LiteralControl("</div>"));
                            _loadedControls.Add(coll.ID);
                        }
                        pnl.Controls.Add(uc);
                        break;
                    }
                }
                if (c.HasControls())
                    FindPanelControl(c.Controls, coll);
            }
        }
    }
    private string BuildHeader(string name) {
        StringBuilder str = new StringBuilder();

        str.Append("<div class=\"overlay-header\">");
        str.Append("<span class=\"overlay-title\">" + name + "</span>");
        str.Append("<a href=\"#\" class=\"overlay-menu exit-button-app\" onclick=\"openWSE.OverlayDisable(this);return false;\" title=\"Close Overlay\"></a>");
        str.Append("</div>");

        return str.ToString();
    }
}