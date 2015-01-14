using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Web.Security;
using System.Text;
using System.IO;

public partial class SiteTools_CustomTables : System.Web.UI.Page {
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();
    private string _username;
    private string _siteTheme = "Standard";
    private CustomTableViewer ctv;
    private bool AssociateWithGroups = false;
    private MemberDatabase _member;

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        bool cont = false;

        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            _username = userId.Name;
            if (ServerSettings.AdminPagesCheck(Page.ToString(), _username)) {
                ctv = new CustomTableViewer(_username);

                AssociateWithGroups = _ss.AssociateWithGroups;

                _member = new MemberDatabase(_username);
                _siteTheme = _member.SiteTheme;

                if ((_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && (_ss.LockCustomTables)) {
                    ltl_locked.Text = HelperMethods.GetLockedByMessage();
                    pnl_columnEditor.Enabled = false;
                    pnl_columnEditor.Visible = false;
                    BuildLockedTableList();
                }
                else {
                    BuildTableList();
                    if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                        cb_InstallAfterLoad.Visible = false;
                        cb_InstallAfterLoad.Enabled = false;
                        cb_isPrivate.Visible = false;
                        cb_isPrivate.Enabled = false;
                    }
                    else {
                        cb_InstallAfterLoad.Visible = true;
                        cb_InstallAfterLoad.Enabled = true;
                        cb_isPrivate.Visible = true;
                        cb_isPrivate.Enabled = true;
                    }
                }
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private void BuildTableList() {
        pnl_tableList.Controls.Clear();
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
            ctv.BuildEntriesAll();
        else
            ctv.BuildEntriesForUser();

        StringBuilder str = new StringBuilder();

        App apps = new App();

        int count = 1;
        foreach (CustomTable_Coll coll in ctv.CustomTableList) {
            MemberDatabase tempMember = new MemberDatabase(coll.CreatedBy);

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckCustomTablesGroupAssociation(coll, _member)) {
                    continue;
                }
            }

            string column = "<table cellspacing='0' cellpadding='0' style='width: 750px!important; border-collapse: collapse;'>";
            column += "<tr class='GridNormalRow columnIDRow'><td><table class='myItemStyle' cellpadding='5' cellspacing='0'><tr>";
            column += "<td width='45px' align='center' class='GridViewNumRow'>" + count + "</td>";
            column += "<td class='border-right'><span class='pad-left'>" + coll.TableName + "</span></td>";
            column += "<td class='border-right' align='center' width='180px'>" + HelperMethods.MergeFMLNames(tempMember) + "</td>";
            column += "<td class='border-right' align='center' width='180px'>" + coll.DateCreated + "</td>";

            string deleteBtn = "<a href='#delete' onclick='DeleteTable(\"" + coll.ID + "\", \"" + coll.TableName + "\");return false;' class='td-delete-btn table-action-btns' title='Delete'></a>";
            string editBtn = "<a href='#edit' onclick='EditTable(\"" + coll.TableID + "\", \"" + coll.TableName + "\");return false;' class='td-edit-btn table-action-btns margin-right' title='Edit'></a>";

            string _check = string.Empty;
            if (coll.Sidebar)
                _check = " checked=\"checked\"";

            string recreateAppBtn = "";
            if (!AppExists(coll.AppID))
                recreateAppBtn = "<div class='clear-space-two'></div><a href='#createapp' onclick='RecreateApp(\"" + coll.AppID.Replace("app-", "") + "\", \"" + coll.TableName + "\", " + coll.Sidebar.ToString().ToLower() + ");return false;' class='sb-links table-action-btns' title='Create App'>Create</a>";
            else {
                if ((_username.ToLower() != coll.CreatedBy.ToLower()) && (apps.GetIsPrivate(coll.AppID)) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                    continue;
                }
            }

            string shouldCheck = "true";
            if (coll.Sidebar)
                shouldCheck = "false";

            column += "<td class='border-right' align='center' width='75px'><input type=\"checkbox\" class=\"table-action-cb\" onchange='RecreateApp(\"" + coll.AppID.Replace("app-", "") + "\", \"" + coll.TableName + "\", " + shouldCheck + ");'" + _check + " /></td>";
            column += "<td class='border-right' align='center' width='75px'>" + editBtn + deleteBtn + recreateAppBtn + "</td>";
            column += "</tr></table></td></tr></table>";
            str.Append(column);

            count++;
        }

        if (!string.IsNullOrEmpty(str.ToString()))
            pnl_tableList.Controls.Add(new LiteralControl(str.ToString()));
        else
            pnl_tableList.Controls.Add(new LiteralControl("<div class='emptyGridView float-left' style='width: 735px!important; border-collapse: collapse;'>No custom tables found.</div>"));
    }

    private void BuildLockedTableList() {
        pnl_tableList.Controls.Clear();
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
            ctv.BuildEntriesAll();
        else
            ctv.BuildEntriesForUser();

        StringBuilder str = new StringBuilder();

        App apps = new App();

        int count = 1;
        foreach (CustomTable_Coll coll in ctv.CustomTableList) {
            MemberDatabase tempMember = new MemberDatabase(coll.CreatedBy);

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckCustomTablesGroupAssociation(coll, _member)) {
                    continue;
                }
            }

            string recreateAppBtn = "";
            if (AppExists(coll.AppID)) {
                if ((_username.ToLower() != coll.CreatedBy.ToLower()) && (apps.GetIsPrivate(coll.AppID)) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                    continue;
                }
            }

            string column = "<table cellspacing='0' cellpadding='0' style='width: 750px!important; border-collapse: collapse;'>";
            column += "<tr class='GridNormalRow columnIDRow'><td><table class='myItemStyle' cellpadding='5' cellspacing='0'><tr>";
            column += "<td width='45px' align='center' class='GridViewNumRow'>" + count + "</td>";
            column += "<td class='border-right'><span class='pad-left'>" + coll.TableName + "</span></td>";
            column += "<td class='border-right' align='center' width='180px'>" + HelperMethods.MergeFMLNames(tempMember) + "</td>";
            column += "<td class='border-right' align='center' width='180px'>" + coll.DateCreated + "</td>";

            string _check = string.Empty;
            if (coll.Sidebar)
                _check = " checked=\"checked\"";

            string shouldCheck = "true";
            if (coll.Sidebar)
                shouldCheck = "false";

            column += "<td class='border-right' align='center' width='75px'><input type=\"checkbox\" class=\"table-action-cb\"" + _check + " disabled='disabled' /></td>";
            column += "<td class='border-right' align='center' width='75px'> - </td>";
            column += "</tr></table></td></tr></table>";
            str.Append(column);

            count++;
        }

        if (!string.IsNullOrEmpty(str.ToString()))
            pnl_tableList.Controls.Add(new LiteralControl(str.ToString()));
        else
            pnl_tableList.Controls.Add(new LiteralControl("<div class='emptyGridView float-left' style='width: 735px!important; border-collapse: collapse;'>No custom tables found.</div>"));
    }

    protected void hf_tableUpdate_Changed(object sender, EventArgs e) {
        BuildTableList();
        hf_tableUpdate.Value = string.Empty;
    }
    protected void hf_tableDelete_Changed(object sender, EventArgs e) {
        string id = hf_tableDelete.Value.Trim();

        string appID = ctv.GetAppIDByID(id);
        ctv.DeleteRowByID(id, appID);

        BuildTableList();
        hf_tableUpdate.Value = string.Empty;
        hf_tableDeleteID.Value = string.Empty;
    }

    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_tableDeleteID.Value)) {
            CustomTableViewer ctv = new CustomTableViewer(_username);
            string createdBy = ctv.GetCreatedByByID(hf_tableDeleteID.Value);

            bool isGood = false;
            if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                isGood = Membership.ValidateUser(ServerSettings.AdminUserName, tb_passwordConfirm.Text);
            }
            else {
                if (!string.IsNullOrEmpty(createdBy)) {
                    isGood = Membership.ValidateUser(createdBy, tb_passwordConfirm.Text);
                }
                else {
                    isGood = Membership.ValidateUser(ServerSettings.AdminUserName, tb_passwordConfirm.Text);
                }
            }

            if (isGood) {
                RegisterPostbackScripts.RegisterStartupScript(this, "PerformDelete('" + hf_tableDeleteID.Value + "');");
            }
            else {
                tb_passwordConfirm.Text = "";
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Password is invalid');");
            }
        }
    }
    private bool AppExists(string appId) {
        App apps = new App();
        string fileName = apps.GetAppFilename(appId);
        var fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\" + appId.Replace("app-", "") + ".ascx");

        if (!string.IsNullOrEmpty(fileName)) {
            return true;
        }
        else if (fi.Exists) {
            try {
                fi.Delete();
            }
            catch { }
        }

        return false;
    }

}