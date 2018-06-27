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

public partial class SiteTools_AppCategory : BasePage {

    private App CurrentAppObject = new App(string.Empty);
    private AppCategory _category;
    private bool ViewUserOverrides {
        get {
            if (Session["ViewUserCategoryOverrides"] != null) {
                return HelperMethods.ConvertBitToBoolean(Session["ViewUserCategoryOverrides"]);
            }

            if (!string.IsNullOrEmpty(CurrentUsername) && !IsUserInAdminRole()) {
                if (ServerSettings.AdminPagesCheck("acctsettings_aspx", CurrentUsername)) {
                    return true;
                }
            }

            return false;
        }
        set {
            Session["ViewUserCategoryOverrides"] = value;
        }
    }

    protected void Page_Load(object sender, EventArgs e) {
        if (IsUserNameEqualToAdmin() || !ServerSettings.AdminPagesCheck("acctsettings_aspx", CurrentUsername)) {
            ViewUserOverrides = false;
            cb_ShowUserOverrides.Enabled = false;
            cb_ShowUserOverrides.Visible = false;
        }

        _category = new AppCategory(false);
        if (!PostbackControlContainsString("cb_ShowUserOverrides")) {
            if (ViewUserOverrides) {
                cb_ShowUserOverrides.Checked = true;
                CurrentAppObject = new App(CurrentUsername, true);
            }
            else {
                CurrentAppObject = new App(CurrentUsername, false);
            }
        }

        if (!IsPostBack || PostbackControlIsUpdateAll()) {
            BuildCategoryList();
            BuildAppList();
        }
    }

    private void BuildCategoryList() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 2);

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Category Name", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Apps", "200", true, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Last Updated", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Updated By", "150", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        if (CountAppIcons_Uncategorized() > 0) {
            List<TableBuilderBodyColumnValues> uncategorizedBodyColumnValues = new List<TableBuilderBodyColumnValues>();

            uncategorizedBodyColumnValues.Add(new TableBuilderBodyColumnValues("Category Name", "Uncategorized Apps", TableBuilderColumnAlignment.Left));
            uncategorizedBodyColumnValues.Add(new TableBuilderBodyColumnValues("Apps", "<div class='clear-space-five'></div>" + LoadAppIcons(""), TableBuilderColumnAlignment.Left));
            uncategorizedBodyColumnValues.Add(new TableBuilderBodyColumnValues("Last Updated", "-", TableBuilderColumnAlignment.Left));
            uncategorizedBodyColumnValues.Add(new TableBuilderBodyColumnValues("Updated By", "-", TableBuilderColumnAlignment.Left));

            tableBuilder.AddBodyRow(uncategorizedBodyColumnValues);
        }

        _category = new AppCategory(true);
        foreach (Dictionary<string, string> dr in _category.category_dt) {
            bool cancontinue = false;
            if (!Roles.IsUserInRole(CurrentUsername, ServerSettings.AdminUserName)) {
                if ((dr["CreatedBy"].ToLower() == CurrentUsername.ToLower()) || (HasAppinCategory(dr["ID"])) || (ViewUserOverrides)) {
                    cancontinue = true;
                }
            }
            else {
                cancontinue = true;
            }

            if (!cancontinue) continue;

            StringBuilder editControls = new StringBuilder();
            #region Build Edit Controls
            bool candelete = true;
            if (!Roles.IsUserInRole(CurrentUsername, ServerSettings.AdminUserName)) {
                if (dr["CreatedBy"].ToLower() != CurrentUsername.ToLower()) {
                    candelete = false;
                }
            }

            editControls.Append("<a class='td-edit-btn' onclick='EditPackage(\"" + dr["ID"] + "\");return false;' title='Edit'></a>");
            if (candelete) {
                editControls.Append("<a class='td-delete-btn' onclick='DeletePackageCategory(\"" + dr["ID"] + "\", \"Category\");return false;' title='Delete'></a>");
            }
            #endregion

            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();

            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Category Name", dr["Category"], TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Apps", "<div class='clear-space-five'></div>" + LoadAppIcons(dr["ID"]), TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Last Updated", dr["DateUpdated"], TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Updated By", dr["UpdatedBy"], TableBuilderColumnAlignment.Left));

            tableBuilder.AddBodyRow(bodyColumnValues, editControls.ToString());
        }
        #endregion

        pnl_categoryholder.Controls.Clear();
        pnl_categoryholder.Controls.Add(tableBuilder.CompleteTableLiteralControl("No categories found"));

        updatepnl_header.Update();
    }

    private int CountAppIcons_Uncategorized() {
        int count = 0;

        CurrentAppObject.GetAllApps();
        List<Apps_Coll> appColl = CurrentAppObject.AppList;

        AppCategory app_category = new AppCategory(false);

        foreach (Apps_Coll dr in appColl) {
            Dictionary<string, string> categoryList = app_category.BuildCategoryDictionary(dr.Category);
            foreach (KeyValuePair<string, string> cn in categoryList) {
                if (cn.Key == AppCategory.Uncategorized_Name) {
                    count++;
                }
            }
        }

        return count;
    }

    private string LoadAppIcons(string id) {
        var appScript = new StringBuilder();
        List<Apps_Coll> coll = new List<Apps_Coll>();
        if (string.IsNullOrEmpty(id)) {
            return LoadUncategorizedIcons();
        }
        else {
            coll = CurrentAppObject.GetApps_byCategory(id);
        }

        foreach (Apps_Coll dr in coll) {
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

            if (ViewUserOverrides && CurrentUserMemberDatabase.EnabledApps.Contains(dr.AppId)) {
                cancontinue = true;
            }
            else {
                if (!IsUserInAdminRole()) {
                    if (dr.CreatedBy.ToLower() == CurrentUsername.ToLower())
                        cancontinue = true;
                }
                else
                    cancontinue = true;
            }

            if (!cancontinue) continue;
            string name = dr.AppName;
            string icon = dr.Icon;
            string image = "<img alt='icon' src='" + ResolveUrl("~/" + icon) + "' class='app-icon-admin-icon' />";
            appScript.Append("<div class='app-icon-admin'>" + image);
            appScript.Append("<span class='app-span-modify'>" + name + "</span><div class='clear'></div></div>");
        }

        if (!string.IsNullOrEmpty(appScript.ToString())) {
            return appScript.ToString();
        }
        return "No apps in category<div class='clear-space-five'></div>";
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
            if (IsUserInAdminRole() || enabledApps.Contains(id)) {
                string name = dr.AppName;
                string icon = dr.Icon;
                string image = "<img alt='icon' src='" + ResolveUrl("~/" + icon) + "' class='app-icon-admin-icon' />";
                if (string.IsNullOrEmpty(icon))
                    image = string.Empty;

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

        pnl_appsInCategory.Controls.Clear();
        string table = HelperMethods.TableAddRemove(removeScript.ToString(), string.Empty, "Apps Available to Add", "Apps in Category", false);
        pnl_appsInCategory.Controls.Add(new LiteralControl(table));
    }

    private string LoadUncategorizedIcons() {
        var appScript = new StringBuilder();

        AppCategory app_category = new AppCategory(false);

        CurrentAppObject.GetAllApps();
        List<Apps_Coll> coll = CurrentAppObject.AppList;
        foreach (Apps_Coll dr in CurrentAppObject.AppList) {
            Dictionary<string, string> categoryList = app_category.BuildCategoryDictionary(dr.Category);
            foreach (KeyValuePair<string, string> cn in categoryList) {
                if (cn.Key == AppCategory.Uncategorized_Name) {
                    string temp = BuildUncategorizedRow(dr);
                    if (!string.IsNullOrEmpty(temp)) {
                        appScript.Append(temp);
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(appScript.ToString())) {
            return appScript.ToString();
        }
        return "No apps in category<div class='clear-space-five'></div>";
    }
    private string BuildUncategorizedRow(Apps_Coll dr) {
        StringBuilder appScript = new StringBuilder();
        if (MainServerSettings.AssociateWithGroups) {
            if (!ServerSettings.CheckAppGroupAssociation(dr, CurrentUserMemberDatabase)) {
                return string.Empty;
            }
        }

        if (string.IsNullOrEmpty(dr.ID)) {
            return string.Empty;
        }

        if (CurrentUsername.ToLower() != dr.CreatedBy.ToLower() && dr.IsPrivate && !IsUserNameEqualToAdmin()) {
            return string.Empty;
        }

        bool cancontinue = false;

        if (ViewUserOverrides && CurrentUserMemberDatabase.EnabledApps.Contains(dr.AppId)) {
            cancontinue = true;
        }
        else {
            if (!IsUserInAdminRole()) {
                if (dr.CreatedBy.ToLower() == CurrentUsername.ToLower())
                    cancontinue = true;
            }
            else
                cancontinue = true;
        }

        if (!cancontinue) return string.Empty;
        string name = dr.AppName;
        string icon = dr.Icon;
        string image = "<img alt='icon' src='" + ResolveUrl("~/" + icon) + "' class='app-icon-admin-icon' />";
        if (string.IsNullOrEmpty(icon))
            image = string.Empty;

        appScript.Append("<div class='app-icon-admin'>" + image);
        appScript.Append("<span class='app-span-modify'>" + name + "</span><div class='clear'></div></div>");

        return appScript.ToString();
    }

    private void LoadAppIcons_Edit(string id) {
        CurrentAppObject.GetAllApps();
        pnl_w.Controls.Clear();
        var strApps = new StringBuilder();
        var strApps2 = new StringBuilder();

        _category = new AppCategory(true);
        foreach (
            var dr in
                _category.category_dt.Cast<Dictionary<string, string>>()
                         .Where(dr => dr["ID"].ToLower() == id.ToLower())) {
            if (!IsUserInAdminRole()) {
                if (dr["CreatedBy"].ToLower() != CurrentUsername.ToLower()) {
                    pnl_editName.Enabled = false;
                    pnl_editName.Visible = false;
                }
                else {
                    pnl_editName.Enabled = true;
                    pnl_editName.Visible = true;
                    tb_edit_name.Text = dr["Category"];
                }
            }
            else {
                pnl_editName.Enabled = true;
                pnl_editName.Visible = true;
                tb_edit_name.Text = dr["Category"];
            }

            lbl_typeEdit_Name.Text = "<h2><span style='font-weight: normal!important;'>" + dr["Category"] + "</span></h2>";
            break;
        }

        var list =
            CurrentAppObject.GetApps_byCategory(id)
                   .Cast<Apps_Coll>()
                   .Select(dr => dr.AppId)
                   .ToList();

        foreach (Apps_Coll dr in CurrentAppObject.AppList) {
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

            if (ViewUserOverrides && CurrentUserMemberDatabase.EnabledApps.Contains(dr.AppId)) {
                cancontinue = true;
            }
            else {
                if (!IsUserInAdminRole()) {
                    if (dr.CreatedBy.ToLower() == CurrentUsername.ToLower()) {
                        cancontinue = true;
                    }
                }
                else {
                    cancontinue = true;
                }
            }

            if (!cancontinue) continue;
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
                strApps2.Append("<a onclick=\"AddApp(this, '" + _appId + "');return false;\" title='Add " + dr.AppName + "' class='float-left img-expand-sml cursor-pointer '></a>");
                strApps2.Append(image + "<span class='app-span-modify'>" + dr.AppName + "</span>");
                strApps2.Append("<div class='clear'></div></div>");
                strApps2.Append("<div class='clear'></div>");
            }
        }

        string table = HelperMethods.TableAddRemove(strApps2.ToString(), strApps.ToString(), "Apps Available to Add", "Apps in Category", false);
        pnl_w.Controls.Add(new LiteralControl(table));
        updatepnl_viewapps.Update();
        BuildCategoryList();
        updatepnl_header.Update();
    }

    protected void btn_finish_add_Click(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(tb_categoryname.Text)) {
            lbl_error.Text = "<div class='clear-space'></div>Must have a category name";
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
                string categoryId = Guid.NewGuid().ToString();
                _category.addItem(tb_categoryname.Text.Trim(), categoryId);

                string tempAppAssociationList = HttpUtility.UrlDecode(hf_newAppAssocationList_Checked.Value);
                string[] cb_associated = tempAppAssociationList.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);

                foreach (string appAssociation in cb_associated) {
                    Dictionary<string, string> categories = CurrentAppObject.GetCategoriesForApp(appAssociation);
                    if (!categories.ContainsKey(categoryId)) {
                        string categoryStr = string.Empty;
                        foreach (KeyValuePair<string, string> categoryPair in categories) {
                            categoryStr += categoryPair.Key + ServerSettings.StringDelimiter;
                        }

                        if (!string.IsNullOrEmpty(categoryStr) && categoryStr[categoryStr.Length - 1].ToString() != ServerSettings.StringDelimiter) {
                            categoryStr += ServerSettings.StringDelimiter;
                        }

                        categoryStr += categoryId;
                        CurrentAppObject.UpdateCategory(appAssociation, categoryStr);
                    }
                }

                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'NewCategory-element', '');");

                hf_newAppAssocationList_Checked.Value = string.Empty;
                BuildCategoryList();
                ClearNewDialogControls();
            }
            else {
                lbl_error.Text = "<div class='clear-space'></div>Category already exists";
                lbl_error.Enabled = true;
                lbl_error.Visible = true;
            }
        }
    }

    protected void btn_edit_name_Click(object sender, EventArgs e) {
        ClearNewDialogControls();

        if (string.IsNullOrEmpty(tb_edit_name.Text)) {
            lbl_error_edit.Text = "<div class='clear-space'></div>Must have a category name";
            lbl_error_edit.Enabled = true;
            lbl_error_edit.Visible = true;
        }
        else {
            string id = hf_edit.Value;
            lbl_error_edit.Enabled = false;
            lbl_error_edit.Visible = false;
            _category = new AppCategory(false);
            if (!string.IsNullOrEmpty(id)) {
                _category.updateItem(tb_edit_name.Text.Trim(), id);

                #region Update Added To
                string[] appSlit_added = hf_appAssocationList_added.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string appId in appSlit_added) {
                    if ((!string.IsNullOrEmpty(id)) && (!string.IsNullOrEmpty(appId))) {
                        Dictionary<string, string> categories = CurrentAppObject.GetCategoriesForApp(appId);
                        if (!categories.ContainsKey(id)) {
                            string categoryStr = string.Empty;
                            foreach (KeyValuePair<string, string> categoryPair in categories) {
                                categoryStr += categoryPair.Key + ServerSettings.StringDelimiter;
                            }

                            if (!string.IsNullOrEmpty(categoryStr) && categoryStr[categoryStr.Length - 1].ToString() != ServerSettings.StringDelimiter) {
                                categoryStr += ServerSettings.StringDelimiter;
                            }

                            categoryStr += id;
                            if (ViewUserOverrides) {
                                UserAppSettings appSettings = new UserAppSettings(CurrentUsername, true);
                                appSettings.UpdateCategorySetting(appId, categoryStr);
                            }
                            else {
                                CurrentAppObject.UpdateCategory(appId, categoryStr);
                            }
                        }
                    }
                }
                #endregion

                #region Update Removed From
                string[] appSlit_removed = hf_appAssocationList_removed.Value.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                foreach (string appId in appSlit_removed) {
                    if ((!string.IsNullOrEmpty(id)) && (!string.IsNullOrEmpty(appId))) {
                        Dictionary<string, string> categories = CurrentAppObject.GetCategoriesForApp(appId);

                        if (categories.ContainsKey(id)) {
                            categories.Remove(id);
                        }

                        string categoryStr = string.Empty;
                        foreach (KeyValuePair<string, string> categoryPair in categories) {
                            categoryStr += categoryPair.Key + ServerSettings.StringDelimiter;
                        }

                        if (ViewUserOverrides) {
                            UserAppSettings appSettings = new UserAppSettings(CurrentUsername, true);
                            appSettings.UpdateCategorySetting(appId, categoryStr);
                        }
                        else {
                            CurrentAppObject.UpdateCategory(appId, categoryStr);
                        }
                    }
                }
                #endregion
            }

            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
            aib.BuildAppsForUser();

            BuildCategoryList();

            hf_appAssocationList_added.Value = string.Empty;
            hf_appAssocationList_removed.Value = string.Empty;
            hf_edit.Value = string.Empty;

            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'CategoryEdit-element', ''); currPackage = '';");
        }
    }

    protected void hf_edit_ValueChanged(object sender, EventArgs e) {
        ClearNewDialogControls();

        if (!string.IsNullOrEmpty(hf_edit.Value)) {
            LoadAppIcons_Edit(hf_edit.Value);
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(true, 'CategoryEdit-element', 'Edit Category');");
        }
    }

    protected void hf_delete_ValueChanged(object sender, EventArgs e) {
        ClearNewDialogControls();

        if (!string.IsNullOrEmpty(hf_delete.Value)) {
            _category.deleteItem(hf_delete.Value);
            hf_delete.Value = string.Empty;
            BuildCategoryList();
        }
    }

    protected void hf_refreshList_ValueChanged(object sender, EventArgs e) {
        ClearNewDialogControls();
        BuildCategoryList();
        hf_refreshList.Value = string.Empty;

        AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
        aib.BuildAppsForUser();

        hf_edit.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'CategoryEdit-element', ''); currPackage = '';");
    }

    private bool HasAppinCategory(string id) {
        CurrentAppObject.GetAllApps();
        return
            CurrentAppObject.AppList.Cast<Apps_Coll>()
                       .Any(
                           dr =>
                           (dr.CreatedBy.ToLower() == CurrentUsername.ToLower()) &&
                           (id == dr.Category));
    }

    protected void cb_ShowUserOverrides_CheckedChanged(object sender, EventArgs e) {
        ClearNewDialogControls();

        ViewUserOverrides = cb_ShowUserOverrides.Checked;

        if (cb_ShowUserOverrides.Checked) {
            CurrentAppObject = new App(CurrentUsername, true);
        }
        else {
            CurrentAppObject = new App(CurrentUsername, false);
        }

        BuildCategoryList();
    }

    private void ClearNewDialogControls() {
        tb_categoryname.Text = string.Empty;
        lbl_error.Text = string.Empty;
        lbl_error.Enabled = false;
        lbl_error.Visible = false;

        BuildAppList();
    }

}