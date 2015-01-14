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

public partial class SiteTools_AppPackages : Page {
    private NewUserDefaults _demoCustomizations;
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();
    private AppPackages _packs;
    private string _username;
    private bool AssociateWithGroups = false;
    private MemberDatabase _member;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                AssociateWithGroups = _ss.AssociateWithGroups;

                _username = userId.Name;
                _member = new MemberDatabase(_username);

                _packs = new AppPackages(false);

                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    pnl_app_installer.Enabled = false;
                    pnl_app_installer.Visible = false;
                    pnl_demo_installer.Enabled = false;
                    pnl_demo_installer.Visible = false;
                }

                if ((!_ss.NoLoginRequired) && (!_ss.ShowPreviewButtonLogin)) {
                    pnl_demo_installer.Enabled = false;
                    pnl_demo_installer.Visible = false;
                }

                if (_username.ToLower() != ServerSettings.AdminUserName.ToLower()) {
                    pnl_demo_installer.Enabled = false;
                    pnl_demo_installer.Visible = false;
                }

                using (var sm = ScriptManager.GetCurrent(Page)) {
                    string ctlId = null;
                    if (sm != null) ctlId = sm.AsyncPostBackSourceElementID;

                    if ((!IsPostBack) || (ctlId == "hf_UpdateAll")) {
                        BuildPackageList();
                    }
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void BuildPackageList() {
        dd_appinstaller.Items.Clear();
        dd_appdemo.Items.Clear();
        pnl_packageholder.Controls.Clear();
        var str = new StringBuilder();
        int count = 0;
        _packs = new AppPackages(true);
        foreach (Dictionary<string, string> dr in _packs.listdt) {
            bool cancontinue = false;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if ((dr["CreatedBy"].ToLower() == _username.ToLower()) ||
                    (HasAppinPackage(dr["AppList"]))) {
                    cancontinue = true;
                }
            }
            else {
                cancontinue = true;
            }

            if (cancontinue) {
                string[] list = _packs.GetAppList(dr["ID"]);
                str.Append(
                    "<div class='contact-card-main contact-card-main-category-packages'>");

                bool candelete = false;
                if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                    if (dr["CreatedBy"].ToLower() == _username.ToLower()) {
                        candelete = true;
                    }
                }
                else {
                    candelete = true;
                }

                if (candelete)
                    str.Append("<a href='#delete' class='float-right td-delete-btn' onclick='DeletePackageCategory(\"" + dr["ID"] + "\", \"Package\");return false;'></a>");

                str.Append("<a href='#edit' class='float-right td-edit-btn margin-right' onclick='EditPackage(\"" + dr["ID"] + "\");return false;'></a>");

                str.Append("<div class='float-left'><h2>" + dr["PackageName"] +
                           "</h2></div><div class='clear-space'></div>");
                str.Append("<div class='clear'></div>");
                str.Append("<div class='clear-margin package-contents pad-top pad-bottom'>" + LoadAppIcons(list) + "</div>");
                str.Append("<div class='clear-space-five'></div><div style='height: 45px;'><p class='float-right'><b>Total Apps</b>" + CountAppIcons(list) + "</p>");
                str.Append("<p class='float-left'><b>Updated By</b>" + dr["UpdatedBy"].ToString() + "</p><div class='clear-space-five'></div>");
                str.Append("<p class='float-left'><b>Last Updated</b>" + dr["Date"].ToString() + "</p></div></div>");

                dd_appdemo.Items.Add(new ListItem(dr["PackageName"], dr["ID"]));
                dd_appinstaller.Items.Add(new ListItem(dr["PackageName"], dr["ID"]));

                count++;
            }
        }
        lbl_packagecount.Text = count.ToString(CultureInfo.InvariantCulture);
        pnl_packageholder.Controls.Add(new LiteralControl(str.ToString()));
        updatepnl_header.Update();

        _demoCustomizations = new NewUserDefaults("DemoNoLogin");

        RegisterPostbackScripts.RegisterStartupScript(this, "$('#MainContent_dd_appinstaller').val('" + _ss.AppInstallerPackage + "');$('#MainContent_dd_appdemo').val('" + _demoCustomizations.GetDemoAppPackage + "');");
    }

    private bool HasAppinPackage(string list) {
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            return true;
        }
        var tempapps = new App(_username);
        tempapps.GetAllApps();
        string[] del = { "," };
        string[] mylist = list.Split(del, StringSplitOptions.RemoveEmptyEntries);
        return
            mylist.Select(tempapps.GetAppCreatedBy)
                  .Any(createdby => createdby.ToLower() == _username.ToLower());
    }

    private int CountAppIcons(IEnumerable<string> list) {
        int count = 0;
        var apps = new App();
        foreach (
            string w in from w in list where apps.IconExists(w) select w) {
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                string createdby = apps.GetAppCreatedBy(w);
                if ((createdby.ToLower() == _username.ToLower()) || (HasAppinPackage(w))) {
                    count++;
                }
            }
            else {
                count++;
            }
        }

        return count;
    }

    private string LoadAppIcons(IEnumerable<string> list) {
        var apps = new App();
        var appScript = new StringBuilder();
        foreach (string w in list) {
            if (apps.IconExists(w)) {
                if (HasAppinPackage(w)) {
                    Apps_Coll coll = apps.GetAppInformation(w);

                    if (AssociateWithGroups) {
                        if (!ServerSettings.CheckAppGroupAssociation(coll, _member)) {
                            continue;
                        }
                    }

                    if ((_username.ToLower() != coll.CreatedBy.ToLower()) && (coll.IsPrivate) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                        continue;
                    }

                    string image = "<img alt='icon' src='../../Standard_Images/App_Icons/" + coll.Icon +
                                   "' style='height: 25px; padding-right: 7px;' />";
                    if ((string.IsNullOrEmpty(coll.Icon)) || (_ss.HideAllAppIcons)) {
                        image = string.Empty;
                    }
                    appScript.Append(
                        "<div class='app-icon-admin inline-block' style='padding: 0 !important;'>" +
                        image);
                    appScript.Append(
                        "<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 182px;'>" +
                        coll.AppName + "</span></div>");
                }
            }
        }

        if (!string.IsNullOrEmpty(appScript.ToString())) {
            return appScript.ToString();
        }
        return "<i class='font-color-gray'>No apps in package</i><div class='clear-space-five'></div>";
    }

    private void LoadAppIcons_Edit(string id) {
        var apps = new App();
        apps.GetAllApps();
        pnl_w.Controls.Clear();
        var strApps = new StringBuilder();
        var strApps2 = new StringBuilder();
        string[] list = _packs.GetAppList(id);

        _packs = new AppPackages(true);
        foreach (
            var dr in _packs.listdt.Cast<Dictionary<string, string>>().Where(dr => dr["ID"].ToLower() == id.ToLower())) {
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (dr["CreatedBy"].ToLower() != _username.ToLower()) {
                    pnl_edit.Enabled = false;
                    pnl_edit.Visible = false;
                }
                else {
                    pnl_edit.Enabled = true;
                    pnl_edit.Visible = true;
                    tb_edit_name.Text = dr["PackageName"];
                    hf_edit_name.Value = dr["PackageName"];
                }
            }
            else {
                pnl_edit.Enabled = true;
                pnl_edit.Visible = true;
                tb_edit_name.Text = dr["PackageName"];
                hf_edit_name.Value = dr["PackageName"];
            }
            break;
        }

        foreach (Apps_Coll dr in apps.AppList) {

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(dr, _member)) {
                    continue;
                }
            }

            if ((_username.ToLower() != dr.CreatedBy.ToLower()) && (dr.IsPrivate) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                continue;
            }

            bool cancontinue = false;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (dr.CreatedBy.ToLower() == _username.ToLower()) {
                    cancontinue = true;
                }
            }
            else {
                cancontinue = true;
            }

            if (cancontinue) {
                if (apps.IconExists(dr.AppId)) {
                    string image = "<img alt='icon' src='../../Standard_Images/App_Icons/" + dr.Icon +
                                   "' style='height: 25px; padding-right: 7px;' />";
                    if ((string.IsNullOrEmpty(dr.Icon)) || (_ss.HideAllAppIcons)) {
                        image = string.Empty;
                    }

                    string _appId = dr.AppId;
                    if (list.Contains(_appId)) {
                        strApps.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin inline-block' style='padding: 0 !important;'>" + image);
                        strApps.Append("<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 190px;'>" + dr.AppName);
                        strApps.Append("<a href='#' onclick=\"RemoveApp(this, '" + _appId + "');return false;\" title='Remove " + dr.AppName + "'>");
                        strApps.Append("<div title='Remove' class='img-collapse-sml cursor-pointer float-left'></div></a></span></div>");
                    }
                    else {
                        strApps2.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin inline-block' style='padding: 0 !important;'>" + image);
                        strApps2.Append("<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 190px;'>" + dr.AppName);
                        strApps2.Append("<a href='#' onclick=\"AddApp(this, '" + _appId + "');return false;\" title='Add " + dr.AppName + "'>");
                        strApps2.Append("<div title='Add' class='img-expand-sml cursor-pointer float-left'></div></a></span></div>");
                    }
                }
            }
        }

        string strAdded = "<div id='package-added'>" + strApps + "</div>";
        string strRemoved = "<div id='package-removed'>" + strApps2 + "</div>";
        pnl_w.Controls.Add(new LiteralControl(strAdded + "<div class='clear' style='height: 30px;'></div>" + strRemoved + "</div>"));
        updatepnl_viewapps.Update();
        BuildPackageList();
        updatepnl_header.Update();
    }

    protected void btn_finish_add_Click(object sender, EventArgs e) {
        if ((string.IsNullOrEmpty(tb_packagename.Text)) || (tb_packagename.Text.Trim().ToLower() == "package name")) {
            lbl_error.Text = "Must have a package name";
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
                _packs.addItem(tb_packagename.Text.Trim(), string.Empty, _username);
                BuildPackageList();
                tb_packagename.Text = "Package Name";
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'NewPackage-element', '');");
            }
            else {
                lbl_error.Text = "Package already exists";
                lbl_error.Enabled = true;
                lbl_error.Visible = true;
            }
        }
    }

    protected void btn_edit_name_Click(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(tb_edit_name.Text)) {
            lbl_error_edit.Text = "Must have a<br />package name";
            lbl_error_edit.Enabled = true;
            lbl_error_edit.Visible = true;
        }
        else {
            string id = string.Empty;
            lbl_error_edit.Enabled = false;
            lbl_error_edit.Visible = false;
            bool cancontinue = true;
            _packs = new AppPackages(true);
            foreach (Dictionary<string, string> dr in _packs.listdt) {
                if (dr["PackageName"].ToLower() == hf_edit_name.Value.ToLower()) {
                    id = dr["ID"];
                }

                if (dr["PackageName"].ToLower() == tb_edit_name.Text.ToLower()) {
                    cancontinue = false;
                    break;
                }
            }

            if (cancontinue) {
                if (!string.IsNullOrEmpty(id)) {
                    _packs.updatePackageName(id, tb_edit_name.Text.Trim(), _username);
                    BuildPackageList();
                }
            }
            else {
                lbl_error_edit.Text = "Package name<br />already exists";
                lbl_error_edit.Enabled = true;
                lbl_error_edit.Visible = true;
            }
        }
    }

    protected void btn_cancel_add_Click(object sender, EventArgs e) {
        lbl_error.Enabled = false;
        lbl_error.Visible = false;
        tb_packagename.Text = "Package Name";
        BuildPackageList();
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'NewPackage-element', '');");
    }

    protected void hf_edit_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_edit.Value)) {
            LoadAppIcons_Edit(hf_edit.Value);
            hf_edit.Value = string.Empty;
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'PackageEdit-element', 'Edit Package');");
        }
    }

    protected void hf_delete_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_delete.Value)) {
            _packs.deletePackage(hf_delete.Value);
            hf_delete.Value = string.Empty;
            BuildPackageList();
        }
    }

    protected void hf_addapp_ValueChanged(object sender, EventArgs e) {
        string id = hf_currPackage.Value;
        string appId = hf_addapp.Value;
        if ((!string.IsNullOrEmpty(id)) && (!string.IsNullOrEmpty(appId))) {
            string list = _packs.GetAppList_nonarray(id);
            list += appId + ",";
            _packs.updateAppList(id, list, _username);
        }

        hf_addapp.Value = string.Empty;
        hf_currPackage.Value = string.Empty;
    }

    protected void hf_removeapp_ValueChanged(object sender, EventArgs e) {
        string id = hf_currPackage.Value;
        string appId = hf_removeapp.Value;
        if ((!string.IsNullOrEmpty(id)) && (!string.IsNullOrEmpty(appId))) {
            string list = string.Empty;
            string[] array = _packs.GetAppList(id);
            list = array.Where(w => w.ToLower() != appId.ToLower())
                        .Aggregate(list, (current, w) => current + (w + ","));
            _packs.updateAppList(id, list, _username);
        }

        hf_removeapp.Value = string.Empty;
        hf_currPackage.Value = string.Empty;
    }

    protected void hf_refreshList_ValueChanged(object sender, EventArgs e) {
        BuildPackageList();
        hf_refreshList.Value = string.Empty;
    }

    protected void btn_updateinstaller_Click(object sender, EventArgs e) {
        ServerSettings.update_AppInstallerPackage(dd_appinstaller.SelectedValue);
        BuildPackageList();
    }

    protected void btn_updatedemo_Click(object sender, EventArgs e) {
        _demoCustomizations = new NewUserDefaults("DemoNoLogin");
        _demoCustomizations.UpdateDefaults("AppPackage", dd_appdemo.SelectedValue);
        BuildPackageList();
    }
}