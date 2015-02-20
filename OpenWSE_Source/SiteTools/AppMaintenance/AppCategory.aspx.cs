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

#endregion

public partial class SiteTools_AppCategory : Page {
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();
    private AppCategory _category;
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

                _category = new AppCategory(false);

                ScriptManager sm = ScriptManager.GetCurrent(Page);
                if (sm != null) {
                    string ctlId = sm.AsyncPostBackSourceElementID;

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
        pnl_packageholder.Controls.Clear();
        var str = new StringBuilder();

        str.Append(BuildUncategorized());

        int count = 0;
        _category = new AppCategory(true);
        foreach (Dictionary<string, string> dr in _category.category_dt) {
            bool cancontinue = false;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if ((dr["CreatedBy"].ToLower() == _username.ToLower()) ||
                    (HasAppinCategory(dr["ID"]))) {
                    cancontinue = true;
                }
            }
            else {
                cancontinue = true;
            }

            if (!cancontinue) continue;
            str.Append("<div class='table-settings-box contact-card-main'>");

            bool candelete = true;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (dr["CreatedBy"].ToLower() != _username.ToLower())
                    candelete = false;
            }

            str.Append("<div class='td-settings-title'>" + dr["Category"]);
            if (candelete)
                str.Append("<a href='#delete' class='float-right td-delete-btn' onclick='DeletePackageCategory(\"" + dr["ID"] + "\", \"Category\");return false;'></a>");

            str.Append("<a href='#edit' class='float-right td-edit-btn margin-right' onclick='EditPackage(\"" + dr["ID"] + "\");return false;'></a>");
            str.Append("</div>");
            str.Append("<div class='title-line'></div>");
            str.Append("<div class='td-settings-ctrl'>" + LoadAppIcons(dr["ID"]) + "</div>");
            str.Append("<div class='td-settings-desc'>");
            str.Append("<b class='pad-right-sml'>Last Updated:</b>" + dr["DateAdded"]);
            str.Append("<div class='float-right'><b class='pad-right-sml'>Total Apps:</b>" + CountAppIcons(dr["ID"]) + "</div>");
            str.Append("</div></div>");
            count++;
        }
        lbl_packagecount.Text = count.ToString(CultureInfo.InvariantCulture);
        pnl_packageholder.Controls.Add(new LiteralControl(str.ToString()));
        updatepnl_header.Update();
    }

    private string BuildUncategorized() {
        StringBuilder str = new StringBuilder();
        str.Append("<div class='table-settings-box contact-card-main'>");
        str.Append("<div class='td-settings-title'>Uncategorized Apps</div>");
        str.Append("<div class='title-line'></div>");
        str.Append("<div class='td-settings-ctrl'>" + LoadAppIcons("") + "</div>");
        str.Append("<div class='td-settings-desc' style='background: none;'><div align='right'><b class='pad-right-sml'>Total Apps:</b>" + CountAppIcons("") + "</div></div>");
        str.Append("</div>");

        return str.ToString();
    }

    private int CountAppIcons(string id) {
        if (string.IsNullOrEmpty(id)) {
            return CountAppIcons_Uncategorized();
        }
        else {
            var apps = new App();

            return
                (from Apps_Coll dr in apps.GetApps_byCategory(id) select dr.AppId).Count(apps.IconExists);
        }
    }
    private int CountAppIcons_Uncategorized() {
        int count = 0;
        var w = new App();

        w.GetAllApps();
        List<Apps_Coll> appColl = w.AppList;

        AppCategory app_category = new AppCategory(false);

        foreach (Apps_Coll dr in appColl) {
            string[] categoryList = dr.Category.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

            foreach (string cn in categoryList) {
                string categoryname = app_category.GetCategoryName(cn);
                if (categoryname == "Uncategorized") {
                    count++;
                }
            }
        }
        return count;
    }

    private string LoadAppIcons(string id) {
        var apps = new App();
        var appScript = new StringBuilder();
        List<Apps_Coll> coll = new List<Apps_Coll>();
        if (string.IsNullOrEmpty(id)) {
            return LoadUncategorizedIcons();
        }
        else {
            coll = apps.GetApps_byCategory(id);
        }

        foreach (Apps_Coll dr in coll) {
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
                if (dr.CreatedBy.ToLower() == _username.ToLower())
                    cancontinue = true;
            }
            else
                cancontinue = true;

            if (!cancontinue) continue;
            if (apps.IconExists(dr.AppId)) {
                string name = dr.AppName;
                string icon = dr.Icon;
                string image = "<img alt='icon' src='../../Standard_Images/App_Icons/" + icon + "' style='height: 25px; padding-right: 7px;' />";
                if ((string.IsNullOrEmpty(icon)) || (_ss.HideAllAppIcons))
                    image = string.Empty;

                appScript.Append("<div class='app-icon-admin inline-block' style='padding: 0 !important;'>" + image);
                appScript.Append("<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 182px;'>" + name + "</span></div>");
            }
        }

        if (!string.IsNullOrEmpty(appScript.ToString())) {
            return appScript.ToString();
        }
        return "<i class='font-color-gray'>No apps in category</i><div class='clear-space-five'></div>";
    }

    private string LoadUncategorizedIcons() {
        var apps = new App();
        var appScript = new StringBuilder();

        AppCategory app_category = new AppCategory(false);

        apps.GetAllApps();
        List<Apps_Coll> coll = apps.AppList;
        foreach (Apps_Coll dr in apps.AppList) {

            string[] categoryList = dr.Category.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

            foreach (string cn in categoryList) {
                string categoryname = app_category.GetCategoryName(cn);
                if (categoryname == "Uncategorized") {
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
                        if (dr.CreatedBy.ToLower() == _username.ToLower())
                            cancontinue = true;
                    }
                    else
                        cancontinue = true;

                    if (!cancontinue) continue;
                    if (apps.IconExists(dr.AppId)) {
                        string name = dr.AppName;
                        string icon = dr.Icon;
                        string image = "<img alt='icon' src='../../Standard_Images/App_Icons/" + icon + "' style='height: 25px; padding-right: 7px;' />";
                        if ((string.IsNullOrEmpty(icon)) || (_ss.HideAllAppIcons))
                            image = string.Empty;

                        appScript.Append("<div class='app-icon-admin inline-block' style='padding: 0 !important;'>" +
                                            image);
                        appScript.Append(
                            "<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 182px;'>" +
                            name + "</span></div>");
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(appScript.ToString())) {
            return appScript.ToString();
        }
        return "<i class='font-color-gray'>No apps in category</i><div class='clear-space-five'></div>";
    }

    private void LoadAppIcons_Edit(string id) {
        var apps = new App();
        apps.GetAllApps();
        pnl_w.Controls.Clear();
        var strApps = new StringBuilder();
        var strApps2 = new StringBuilder();

        _category = new AppCategory(true);
        foreach (
            var dr in
                _category.category_dt.Cast<Dictionary<string, string>>()
                         .Where(dr => dr["ID"].ToLower() == id.ToLower())) {
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (dr["CreatedBy"].ToLower() != _username.ToLower()) {
                    pnl_editName.Enabled = false;
                    pnl_editName.Visible = false;
                }
                else {
                    pnl_editName.Enabled = true;
                    pnl_editName.Visible = true;
                    tb_edit_name.Text = dr["Category"];
                    hf_edit_name.Value = dr["Category"];
                }
            }
            else {
                pnl_editName.Enabled = true;
                pnl_editName.Visible = true;
                tb_edit_name.Text = dr["Category"];
                hf_edit_name.Value = dr["Category"];
            }
            break;
        }

        var list =
            apps.GetApps_byCategory(id)
                   .Cast<Apps_Coll>()
                   .Select(dr => dr.AppId)
                   .Where(apps.IconExists)
                   .ToList();

        foreach (Apps_Coll dr in apps.AppList) {
            if (AssociateWithGroups) {
                if (!ServerSettings.CheckAppGroupAssociation(dr, _member)) {
                    continue;
                }
            }

            if ((_username.ToLower() != dr.CreatedBy.ToLower()) && (dr.IsPrivate) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                continue;
            }

            if (!apps.IconExists(dr.AppId)) continue;
            bool cancontinue = false;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (dr.CreatedBy.ToLower() == _username.ToLower()) {
                    cancontinue = true;
                }
            }
            else {
                cancontinue = true;
            }

            if (!cancontinue) continue;
            string image = "<img alt='icon' src='../../Standard_Images/App_Icons/" + dr.Icon + "' style='height: 25px; padding-right: 7px;' />";
            if ((string.IsNullOrEmpty(dr.Icon)) || (_ss.HideAllAppIcons)) {
                image = string.Empty;
            }

            string _appId = dr.AppId;
            if (list.Contains(_appId)) {
                strApps.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin inline-block' style='padding: 0 !important;'>" + image);
                strApps.Append("<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 190px;'>" + dr.AppName);
                strApps.Append("<a href='#' onclick=\"RemoveApp(this, '" + _appId + "');return false;\" title='Remove " + dr.AppName + "' class='float-left'>");
                strApps.Append("<div title='Remove' class='img-collapse-sml cursor-pointer float-left'></div></a></span></div>");
            }
            else {
                strApps2.Append("<div id='app-icon-" + _appId + "' class='app-icon-admin inline-block' style='padding: 0 !important;'>" + image);
                strApps2.Append("<span class='app-span-modify' style='text-align: left; padding: 11px 0 0 0 !important; line-height: 4px !important; font-size: 12px; width: 190px;'>" + dr.AppName);
                strApps2.Append("<a href='#' onclick=\"AddApp(this, '" + _appId + "');return false;\" title='Add " + dr.AppName + "' class='float-left'>");
                strApps2.Append("<div title='Add' class='img-expand-sml cursor-pointer float-left'></div></a></span></div>");
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
        if ((string.IsNullOrEmpty(tb_categoryname.Text)) ||
            (tb_categoryname.Text.Trim().ToLower() == "category name")) {
            lbl_error.Text = "Must have a category name";
            lbl_error.Enabled = true;
            lbl_error.Visible = true;
        }
        else {
            lbl_error.Enabled = false;
            lbl_error.Visible = false;
            _category = new AppCategory(true);
            bool cancontinue =
                _category.category_dt.Cast<Dictionary<string, string>>()
                         .All(dr => dr["Category"].ToLower() != tb_categoryname.Text.ToLower());

            if (cancontinue) {
                _category.addItem(tb_categoryname.Text.Trim());
                BuildPackageList();
                tb_categoryname.Text = "Category Name";
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'NewCategory-element', '');");
            }
            else {
                lbl_error.Text = "Category already exists";
                lbl_error.Enabled = true;
                lbl_error.Visible = true;
            }
        }
    }

    protected void btn_edit_name_Click(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(tb_edit_name.Text)) {
            lbl_error_edit.Text = "Must have a<br />category name";
            lbl_error_edit.Enabled = true;
            lbl_error_edit.Visible = true;
        }
        else {
            string id = string.Empty;
            lbl_error_edit.Enabled = false;
            lbl_error_edit.Visible = false;
            bool cancontinue = true;
            _category = new AppCategory(true);
            foreach (Dictionary<string, string> dr in _category.category_dt) {
                if (dr["Category"].ToLower() == hf_edit_name.Value.ToLower()) {
                    id = dr["ID"];
                }

                if (dr["Category"].ToLower() == tb_edit_name.Text.ToLower()) {
                    cancontinue = false;
                    break;
                }
            }

            if (cancontinue) {
                if (!string.IsNullOrEmpty(id)) {
                    _category.updateItem(tb_edit_name.Text.Trim(), id);
                    BuildPackageList();
                }
            }
            else {
                lbl_error_edit.Text = "Category already exists";
                lbl_error_edit.Enabled = true;
                lbl_error_edit.Visible = true;
            }
        }
    }

    protected void btn_cancel_add_Click(object sender, EventArgs e) {
        lbl_error.Enabled = false;
        lbl_error.Visible = false;
        tb_categoryname.Text = "Category Name";
        BuildPackageList();
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'NewCategory-element', '');");
    }

    protected void hf_edit_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_edit.Value)) {
            LoadAppIcons_Edit(hf_edit.Value);
            hf_edit.Value = string.Empty;
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'CategoryEdit-element', 'Edit Category');");
        }
    }

    protected void hf_delete_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_delete.Value)) {
            _category.deleteItem(hf_delete.Value);
            hf_delete.Value = string.Empty;
            BuildPackageList();
        }
    }

    protected void hf_addapp_ValueChanged(object sender, EventArgs e) {
        var apps = new App();
        string id = hf_currPackage.Value;
        string appId = hf_addapp.Value;
        if ((!string.IsNullOrEmpty(id)) && (!string.IsNullOrEmpty(appId))) {
            List<string> categories = apps.GetCategoriesForApp(appId);
            if (!categories.Contains(id)) {
                string categoryStr = string.Join(ServerSettings.StringDelimiter, categories.ToArray());
                if (!string.IsNullOrEmpty(categoryStr) && categoryStr[categoryStr.Length - 1] != ';') {
                    categoryStr += ";";
                }

                categoryStr += id;
                apps.UpdateCategory(appId, categoryStr);
            }
        }
        hf_addapp.Value = string.Empty;
        hf_currPackage.Value = string.Empty;
    }

    protected void hf_removeapp_ValueChanged(object sender, EventArgs e) {
        var apps = new App();
        string id = hf_currPackage.Value;
        string appId = hf_removeapp.Value;
        if ((!string.IsNullOrEmpty(id)) && (!string.IsNullOrEmpty(appId))) {
            List<string> categories = apps.GetCategoriesForApp(appId);
            if (categories.Contains(id)) {
                categories.Remove(id);
            }
            string categoryStr = string.Join(ServerSettings.StringDelimiter, categories.ToArray());
            apps.UpdateCategory(appId, categoryStr);
        }

        hf_removeapp.Value = string.Empty;
        hf_currPackage.Value = string.Empty;
    }

    protected void hf_refreshList_ValueChanged(object sender, EventArgs e) {
        BuildPackageList();
        hf_refreshList.Value = string.Empty;
    }

    private bool HasAppinCategory(string id) {
        var tempapps = new App(_username);
        tempapps.GetAllApps();
        return
            tempapps.AppList.Cast<Apps_Coll>()
                       .Any(
                           dr =>
                           (dr.CreatedBy.ToLower() == _username.ToLower()) &&
                           (id == dr.Category));
    }
}