using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SiteTools_PluginInstaller : BasePage {

    protected void Page_Load(object sender, EventArgs e) {
        if (!IsUserNameEqualToAdmin()) {
            BuildPluginList();
        }
        else {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }
    }

    private void BuildPluginList() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, false, 0);

        #region Build Body

        SitePlugins _plugins = new SitePlugins(CurrentUsername);
        _plugins.BuildSitePlugins(true);
        _plugins.BuildSitePluginsForUser();

        foreach (SitePlugins_Coll coll in _plugins.siteplugins_dt) {
            if (string.IsNullOrEmpty(coll.AssociatedWith)) {
                if (MainServerSettings.AssociateWithGroups) {
                    if (!ServerSettings.CheckPluginGroupAssociation(coll, CurrentUserMemberDatabase)) {
                        continue;
                    }
                }

                List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();

                StringBuilder str = new StringBuilder();
                string userPluginId = string.Empty;
                bool isInstalled = false;
                foreach (UserPlugins_Coll userColl in _plugins.userplugins_dt) {
                    if (userColl.PluginID == coll.ID) {
                        userPluginId = userColl.ID;
                        isInstalled = true;
                        break;
                    }
                }

                str.AppendFormat("<div class='app-name-holder' style='padding: 0px!important;'><span class='app-name'>{0}</span></div>", coll.PluginName);

                if (isInstalled) {
                    str.AppendFormat("<a title='Uninstall Plugin' class='install-btn td-subtract-btn' onclick=\"UninstallPlugin('{0}', '{1}');return false;\"></a>", userPluginId, coll.PluginName);
                }
                else {
                    str.AppendFormat("<a title='Install Plugin' class='install-btn td-add-btn' onclick=\"InstallPlugin('{0}');return false;\"></a>", coll.ID);
                }

                str.AppendFormat("<div class='app-description'>{0}</div>", coll.Description);
                if (isInstalled) {
                    str.AppendFormat("<span class='installed'>Installed</span>");
                }
                str.AppendFormat("<div class='clear'></div>");

                bodyColumnValues.Add(new TableBuilderBodyColumnValues("", str.ToString(), TableBuilderColumnAlignment.Left));
                tableBuilder.AddBodyRow(bodyColumnValues, string.Empty, string.Empty, "app-item-installer");
            }
        }

        #endregion

        pnl_PluginList.Controls.Clear();
        pnl_PluginList.Controls.Add(tableBuilder.CompleteTableLiteralControl("No plugins found"));
        updatePnl_PluginList.Update();
    }

    protected void hf_UninstallPlugin_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_UninstallPlugin.Value)) {
            SitePlugins plugins = new SitePlugins(CurrentUsername);
            plugins.deleteItemForUser(hf_UninstallPlugin.Value);
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.ReloadPage();");
        }

        hf_UninstallPlugin.Value = string.Empty;
    }
    protected void hf_InstallPlugin_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_InstallPlugin.Value)) {
            SitePlugins plugins = new SitePlugins(CurrentUsername);
            plugins.addItemForUser(hf_InstallPlugin.Value);
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.ReloadPage();");
        }

        hf_InstallPlugin.Value = string.Empty;
    }

}
