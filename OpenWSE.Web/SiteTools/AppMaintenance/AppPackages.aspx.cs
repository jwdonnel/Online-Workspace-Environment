#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

public partial class SiteTools_AppPackages : BasePage {

    private AppPackages _packs;

    protected void Page_Load(object sender, EventArgs e) {
        BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);
        _packs = new AppPackages(false);

        if (!IsUserInAdminRole()) {
            pnl_packagesettings.Enabled = false;
            pnl_packagesettings.Visible = false;
            pnl_app_installer.Enabled = false;
            pnl_app_installer.Visible = false;
            pnl_demo_installer.Enabled = false;
            pnl_demo_installer.Visible = false;
        }

        if (!MainServerSettings.NoLoginRequired && !MainServerSettings.ShowPreviewButtonLogin) {
            pnl_demo_installer.Enabled = false;
            pnl_demo_installer.Visible = false;
        }

        if (!IsPostBack || PostbackControlIsUpdateAll()) {
            BuildPackageList();
            BuildAppList();
        }
    }

    private void BuildPackageList() {
        dd_appinstaller.Items.Clear();
        dd_appdemo.Items.Clear();


        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 2);

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Package Name", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Apps", "200", true, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Last Updated", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Updated By", "150", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        _packs = new AppPackages(true);

        foreach (Dictionary<string, string> dr in _packs.listdt) {
            bool cancontinue = false;
            if (!IsUserInAdminRole()) {
                if ((dr["CreatedBy"].ToLower() == CurrentUsername.ToLower()) ||
                    (HasAppinPackage(dr["AppList"]))) {
                    cancontinue = true;
                }
            }
            else {
                cancontinue = true;
            }

            if (cancontinue) {
                if (!dd_appdemo.Items.Contains(new ListItem(dr["PackageName"], dr["ID"]))) {
                    dd_appdemo.Items.Add(new ListItem(dr["PackageName"], dr["ID"]));
                }

                if (!dd_appinstaller.Items.Contains(new ListItem(dr["PackageName"], dr["ID"]))) {
                    dd_appinstaller.Items.Add(new ListItem(dr["PackageName"], dr["ID"]));
                }

                string[] list = _packs.GetAppList(dr["ID"]);

                StringBuilder editControls = new StringBuilder();

                #region Build Edit Controls
                bool candelete = false;
                if (!Roles.IsUserInRole(CurrentUsername, ServerSettings.AdminUserName)) {
                    if (dr["CreatedBy"].ToLower() == CurrentUsername.ToLower()) {
                        candelete = true;
                    }
                }
                else {
                    candelete = true;
                }

                editControls.Append("<a class='td-edit-btn' onclick='EditPackage(\"" + dr["ID"] + "\");return false;' title=\"Edit\"></a>");
                if (candelete) {
                    editControls.Append("<a class='td-delete-btn' onclick='DeletePackageCategory(\"" + dr["ID"] + "\", \"Package\");return false;' title=\"Delete\"></a>");
                }
                #endregion

                List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();

                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Package Name", dr["PackageName"], TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Apps", "<div class='clear-space-five'></div>" + LoadAppIcons(list), TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Last Updated", dr["DateUpdated"], TableBuilderColumnAlignment.Left));
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Updated By", dr["UpdatedBy"], TableBuilderColumnAlignment.Left));

                tableBuilder.AddBodyRow(bodyColumnValues, editControls.ToString());
            }
        }
        #endregion

        pnl_packageholder.Controls.Clear();
        pnl_packageholder.Controls.Add(tableBuilder.CompleteTableLiteralControl("No packages found"));

        updatepnl_header.Update();
        updatepnl_settings.Update();
        NewUserDefaults tempDemoCustomizations = new NewUserDefaults("DemoNoLogin");
        RegisterPostbackScripts.RegisterStartupScript(this, "$('#MainContent_dd_appinstaller').val('" + MainServerSettings.AppInstallerPackage + "');$('#MainContent_dd_appdemo').val('" + tempDemoCustomizations.GetDemoAppPackage + "');");
    }

    private bool HasAppinPackage(string list) {
        if (Roles.IsUserInRole(CurrentUsername, ServerSettings.AdminUserName)) {
            return true;
        }
        var tempapps = new App(CurrentUsername);
        tempapps.GetAllApps();
        string[] del = { "," };
        string[] mylist = list.Split(del, StringSplitOptions.RemoveEmptyEntries);
        return
            mylist.Select(tempapps.GetAppCreatedBy)
                  .Any(createdby => createdby.ToLower() == CurrentUsername.ToLower());
    }

    private string LoadAppIcons(IEnumerable<string> list) {
        var apps = new App(string.Empty);
        var appScript = new StringBuilder();
        foreach (string w in list) {
            if (HasAppinPackage(w)) {
                Apps_Coll coll = apps.GetAppInformation(w);

                if (MainServerSettings.AssociateWithGroups) {
                    if (!ServerSettings.CheckAppGroupAssociation(coll, CurrentUserMemberDatabase)) {
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(coll.ID)) {
                    continue;
                }

                if (CurrentUsername.ToLower() != coll.CreatedBy.ToLower() && coll.IsPrivate && !IsUserNameEqualToAdmin()) {
                    continue;
                }

                string image = "<img alt='icon' src='" + ResolveUrl("~/" + coll.Icon) + "' class='app-icon-admin-icon' />";
                if (string.IsNullOrEmpty(coll.Icon)) {
                    image = string.Empty;
                }
                appScript.Append("<div class='app-icon-admin'>" + image);
                appScript.Append("<span class='app-span-modify'>" + coll.AppName + "</span><div class='clear'></div></div>");
            }
        }

        if (!string.IsNullOrEmpty(appScript.ToString())) {
            return appScript.ToString();
        }
        return "No apps in package<div class='clear-space-five'></div>";
    }

    private void LoadAppIcons_Edit(string id) {
        var apps = new App(string.Empty);
        _packs = new AppPackages(true);

        apps.GetAllApps();
        pnl_w.Controls.Clear();
        var strApps = new StringBuilder();
        var strApps2 = new StringBuilder();
        string[] list = _packs.GetAppList(id);

        foreach (
            var dr in _packs.listdt.Cast<Dictionary<string, string>>().Where(dr => dr["ID"].ToLower() == id.ToLower())) {
            if (!Roles.IsUserInRole(CurrentUsername, ServerSettings.AdminUserName)) {
                if (dr["CreatedBy"].ToLower() != CurrentUsername.ToLower()) {
                    pnl_edit.Enabled = false;
                    pnl_edit.Visible = false;
                }
                else {
                    pnl_edit.Enabled = true;
                    pnl_edit.Visible = true;
                    tb_edit_name.Text = dr["PackageName"];
                }
            }
            else {
                pnl_edit.Enabled = true;
                pnl_edit.Visible = true;
                tb_edit_name.Text = dr["PackageName"];
            }

            lbl_typeEdit_Name.Text = "<h2><span style='font-weight: normal!important;'>" + dr["PackageName"] + "</span></h2>";
            break;
        }

        foreach (Apps_Coll dr in apps.AppList) {

            if (MainServerSettings.AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(dr, CurrentUserMemberDatabase)) {
                    continue;
                }
            }

            if (string.IsNullOrEmpty(dr.ID)) {
                continue;
            }

            if (CurrentUsername.ToLower() != dr.CreatedBy.ToLower() && dr.IsPrivate && !IsUserNameEqualToAdmin()) {
                continue;
            }

            bool cancontinue = false;
            if (!IsUserInAdminRole()) {
                if (dr.CreatedBy.ToLower() == CurrentUsername.ToLower()) {
                    cancontinue = true;
                }
            }
            else {
                cancontinue = true;
            }

            if (cancontinue) {
                string image = "<img alt='icon' src='" + ResolveUrl("~/" + dr.Icon) + "' class='app-icon-admin-icon' />";
                if (string.IsNullOrEmpty(dr.Icon)) {
                    image = string.Empty;
                }

                string _appId = dr.AppId;
                if (list.Contains(_appId)) {
                    strApps.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin'>");
                    strApps.Append("<a onclick=\"RemoveApp(this, '" + _appId + "');return false;\" title='Remove " + dr.AppName + "' class='float-left img-collapse-sml cursor-pointer'></a>");
                    strApps.Append(image + "<span class='app-span-modify'>" + dr.AppName + "</span>");
                    strApps.Append("<div class='clear'></div></div>");
                    strApps.Append("<div class='clear'></div>");
                }
                else {
                    strApps2.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin'>");
                    strApps2.Append("<a onclick=\"AddApp(this, '" + _appId + "');return false;\" title='Add " + dr.AppName + "' class='float-left img-expand-sml cursor-pointer'></a>");
                    strApps2.Append(image + "<span class='app-span-modify'>" + dr.AppName + "</span>");
                    strApps2.Append("<div class='clear'></div></div>");
                    strApps2.Append("<div class='clear'></div>");
                }
            }
        }

        string table = HelperMethods.TableAddRemove(strApps2.ToString(), strApps.ToString(), "Apps Available to Add", "Apps in Package", false);
        pnl_w.Controls.Add(new LiteralControl(table));
        updatepnl_viewapps.Update();
        BuildPackageList();
        updatepnl_header.Update();
        updatepnl_settings.Update();
    }

    private void BuildAppList() {
        Dictionary<string, string> removeTemp = new Dictionary<string, string>();

        var apps = new App(CurrentUsername);
        apps.GetAllApps();
        List<string> enabledApps = CurrentUserMemberDatabase.EnabledApps;
        foreach (Apps_Coll dr in apps.AppList) {
            if (MainServerSettings.AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(dr, CurrentUserMemberDatabase)) {
                    continue;
                }
            }

            string id = dr.AppId;
            if ((Roles.IsUserInRole(CurrentUsername, ServerSettings.AdminUserName)) || ((enabledApps.Contains(id)))) {
                string name = dr.AppName;
                string image = "<img alt='icon' src='" + ResolveUrl("~/" + dr.Icon) + "' class='app-icon-admin-icon' />";
                if (string.IsNullOrEmpty(dr.Icon)) {
                    image = string.Empty;
                }

                if (!removeTemp.ContainsKey(id)) {
                    StringBuilder appScript = new StringBuilder();
                    appScript.Append("<div id='app-icon-" + id + "' class='app-icon-admin'>");
                    appScript.Append("<a onclick=\"AddAssociation_New(this, '" + dr.AppId + "');return false;\" title='Add " + dr.AppName + "' class='float-left img-expand-sml cursor-pointer'></a>");
                    appScript.Append(image + "<span class='app-span-modify'>" + dr.AppName + "</span>");
                    appScript.Append("<div class='clear'></div></div>");
                    appScript.Append("<div class='clear'></div>");
                    removeTemp.Add(id, appScript.ToString());
                }
            }
        }

        StringBuilder removeScript = new StringBuilder();

        foreach (KeyValuePair<string, string> kvp in removeTemp) {
            removeScript.Append(kvp.Value);
        }

        pnl_appsInPackage.Controls.Clear();
        string table = HelperMethods.TableAddRemove(removeScript.ToString(), string.Empty, "Apps Available to Add", "Apps in Category", false);
        pnl_appsInPackage.Controls.Add(new LiteralControl(table));
    }

    protected void btn_finish_add_Click(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(tb_packagename.Text)) {
            lbl_error.Text = "<div class='clear-space'></div>Must have a package name";
            lbl_error.Enabled = true;
            lbl_error.Visible = true;
        }
        else {
            lbl_error.Enabled = false;
            lbl_error.Visible = false;
            _packs = new AppPackages(true);
            bool cancontinue =
                _packs.listdt.Cast<Dictionary<string, string>>()
                      .All(dr => dr["PackageName"].ToLower() != tb_packagename.Text.ToLower());

            if (cancontinue) {
                string tempAppAssociationList = HttpUtility.UrlDecode(hf_newAppAssocationList_Checked.Value);
                _packs.addItem(tb_packagename.Text.Trim(), tempAppAssociationList, CurrentUsername);
                BuildPackageList();
                ClearNewDialogControls();
                hf_newAppAssocationList_Checked.Value = string.Empty;
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'NewPackage-element', '');");
            }
            else {
                lbl_error.Text = "<div class='clear-space'></div>Package already exists";
                lbl_error.Enabled = true;
                lbl_error.Visible = true;
            }
        }
    }

    protected void btn_edit_name_Click(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(tb_edit_name.Text)) {
            lbl_error_edit.Text = "<div class='clear-space'></div>Must have a package name";
            lbl_error_edit.Enabled = true;
            lbl_error_edit.Visible = true;
        }
        else {
            string id = hf_edit.Value;
            lbl_error_edit.Enabled = false;
            lbl_error_edit.Visible = false;

            _packs = new AppPackages(true);
            if (!string.IsNullOrEmpty(id)) {
                _packs.updatePackageName(id, tb_edit_name.Text.Trim(), CurrentUsername);

                #region Update Added To
                string[] appSlit_added = hf_appAssocationList_added.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string appId in appSlit_added) {
                    if ((!string.IsNullOrEmpty(id)) && (!string.IsNullOrEmpty(appId))) {
                        string list = _packs.GetAppList_nonarray(id);
                        list += appId + ",";
                        _packs.updateAppList(id, list, CurrentUsername);
                    }
                }
                #endregion

                #region Update Remove From
                string[] appSlit_removed = hf_appAssocationList_removed.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string appId in appSlit_removed) {
                    if ((!string.IsNullOrEmpty(id)) && (!string.IsNullOrEmpty(appId))) {
                        string list = string.Empty;
                        string[] array = _packs.GetAppList(id);
                        list = array.Where(w => w.ToLower() != appId.ToLower())
                                    .Aggregate(list, (current, w) => current + (w + ","));
                        _packs.updateAppList(id, list, CurrentUsername);
                    }
                }
                #endregion

                BuildPackageList();
            }

            hf_appAssocationList_added.Value = string.Empty;
            hf_appAssocationList_removed.Value = string.Empty;
            hf_edit.Value = string.Empty;

            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'PackageEdit-element', ''); currPackage = '';");
        }
    }

    protected void hf_edit_ValueChanged(object sender, EventArgs e) {
        ClearNewDialogControls();

        if (!string.IsNullOrEmpty(hf_edit.Value)) {
            LoadAppIcons_Edit(hf_edit.Value);
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'PackageEdit-element', 'Edit Package');");
        }
    }

    protected void hf_delete_ValueChanged(object sender, EventArgs e) {
        ClearNewDialogControls();

        if (!string.IsNullOrEmpty(hf_delete.Value)) {
            _packs.deletePackage(hf_delete.Value);
            hf_delete.Value = string.Empty;
            BuildPackageList();
        }
    }

    protected void hf_refreshList_ValueChanged(object sender, EventArgs e) {
        ClearNewDialogControls();

        BuildPackageList();
        hf_refreshList.Value = string.Empty;

        hf_edit.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'PackageEdit-element', ''); currPackage = '';");
    }

    protected void btn_updateinstaller_Click(object sender, EventArgs e) {
        ClearNewDialogControls();
        ServerSettings.update_AppInstallerPackage(dd_appinstaller.SelectedValue);
        BuildPackageList();
    }

    protected void btn_updatedemo_Click(object sender, EventArgs e) {
        ClearNewDialogControls();
        NewUserDefaults tempDemoCustomizations = new NewUserDefaults("DemoNoLogin");
        tempDemoCustomizations.UpdateDefaults("AppPackage", dd_appdemo.SelectedValue);
        BuildPackageList();
    }

    private void ClearNewDialogControls() {
        tb_packagename.Text = string.Empty;
        lbl_error.Text = string.Empty;
        lbl_error.Enabled = false;
        lbl_error.Visible = false;

        BuildAppList();
    }

}