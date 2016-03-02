using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SiteTools_PluginInstaller : System.Web.UI.Page
{
    private ServerSettings _ss = new ServerSettings();
    private MemberDatabase _member;
    private string _username;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name) && (userId.Name.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                _username = userId.Name;
                _member = new MemberDatabase(_username);
                BuildPluginList();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void BuildPluginList() {
        pnl_PluginList.Controls.Clear();

        StringBuilder str = new StringBuilder();
        StringBuilder strJavascript = new StringBuilder();


        SitePlugins _plugins = new SitePlugins(_username);
        _plugins.BuildSitePlugins(true);
        _plugins.BuildSitePluginsForUser();

        int count = 0;
        bool AssociateWithGroups = _ss.AssociateWithGroups;
        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            if (CanAddPluginInList(coll)) {
                if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                    if (AssociateWithGroups) {
                        if (!ServerSettings.CheckPluginGroupAssociation(coll, _member)) {
                            continue;
                        }
                    }

                    string userPluginId = string.Empty;
                    bool isInstalled = false;
                    foreach (UserPlugins_Coll userColl in _plugins.userplugins_dt) {
                        if (userColl.PluginID == coll.ID) {
                            userPluginId = userColl.ID;
                            isInstalled = true;
                            break;
                        }
                    }

                    str.AppendFormat("<div class='table-settings-box contact-card-main app-item-installer'>");
                    str.AppendFormat("<div class='app-name-holder' style='padding: 0px!important;'><span class='app-name'>{0}</span></div>", coll.PluginName);

                    if (isInstalled) {
                        str.AppendFormat("<a href='#' title='Uninstall Plugin' class='install-btn td-subtract-btn' onclick=\"UninstallPlugin('{0}', '{1}');return false;\"></a>", userPluginId, coll.PluginName);
                    }
                    else {
                        str.AppendFormat("<a href='#' title='Install Plugin' class='install-btn td-add-btn' onclick=\"InstallPlugin('{0}');return false;\"></a>", coll.ID);
                    }

                    str.AppendFormat("<div class='app-description'>{0}</div>", coll.Description);
                    if (isInstalled) {
                        str.AppendFormat("<span class='installed'>Installed</span>");
                    }

                    str.AppendFormat("</div>");
                    count++;
                }
            }
        }

        lbl_PluginsAvailable.InnerHtml = "<b class='pad-right'>Plugins Available</b>" + count.ToString();

        if (!string.IsNullOrEmpty(strJavascript.ToString())) {
            RegisterPostbackScripts.RegisterStartupScript(this, strJavascript.ToString());
        }

        if (!string.IsNullOrEmpty(str.ToString())) {
            pnl_PluginList.Controls.Add(new LiteralControl(str.ToString()));
        }
        else {
            pnl_PluginList.Controls.Add(new LiteralControl("<h3 class='pad-top-big pad-bottom-big'>No Plugins Found</h3>"));
        }

        updatePnl_PluginTotals.Update();
        updatePnl_PluginList.Update();
    }
    private bool CanAddPluginInList(SitePlugins_Coll coll) {
        string search = tb_search.Text.Trim().ToLower();

        if (!string.IsNullOrEmpty(coll.ID)) {
            if (!string.IsNullOrEmpty(search) && search != "search plugins" && !coll.AssociatedWith.ToLower().Contains(search) && !coll.PluginName.ToLower().Contains(search) && !coll.Description.ToLower().Contains(search)) {
                return false;
            }

            return true;
        }

        return false;
    }

    protected void hf_UninstallPlugin_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_UninstallPlugin.Value)) {
            SitePlugins plugins = new SitePlugins(_username);
            plugins.deleteItemForUser(hf_UninstallPlugin.Value);
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.ReloadPage();");
        }

        hf_UninstallPlugin.Value = string.Empty;
    }
    protected void hf_InstallPlugin_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_InstallPlugin.Value)) {
            SitePlugins plugins = new SitePlugins(_username);
            plugins.addItemForUser(hf_InstallPlugin.Value);
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.ReloadPage();");
        }

        hf_InstallPlugin.Value = string.Empty;
    }

    protected void imgbtn_search_Click(object sender, EventArgs e) {
        BuildPluginList();
    }
}