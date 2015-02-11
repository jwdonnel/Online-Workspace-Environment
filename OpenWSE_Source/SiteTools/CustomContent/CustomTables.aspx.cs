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
using OpenWSE_Tools.GroupOrganizer;

public partial class SiteTools_CustomTables : System.Web.UI.Page {
    private readonly AppLog _applog = new AppLog(false);
    private ServerSettings _ss = new ServerSettings();
    private string _username;
    private string _siteTheme = "Standard";
    private CustomTableViewer ctv;
    private bool AssociateWithGroups = false;
    private MemberDatabase _member;

    private bool IsReadOnly {
        get {
            if (ViewState["IsReadOnly"] != null) {
                return HelperMethods.ConvertBitToBoolean(ViewState["IsReadOnly"].ToString());
            }

            return false;
        }
        set {
            ViewState["IsReadOnly"] = value;
        }
    }

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

                BuildChartTypeList();
                BuildUsersAllowedToEdit();

                if ((_username.ToLower() != ServerSettings.AdminUserName.ToLower()) && (_ss.LockCustomTables)) {
                    ltl_locked.Text = HelperMethods.GetLockedByMessage();
                    pnl_columnEditor.Enabled = false;
                    pnl_columnEditor.Visible = false;
                    IsReadOnly = true;
                    BuildTableList();
                }
                else {
                    IsReadOnly = false;
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

    private void BuildUsersAllowedToEdit() {
        pnl_usersAllowedToEdit.Controls.Clear();

        StringBuilder str = new StringBuilder();
        List<string> groupList = _member.GroupList;
        string checkboxInput = "<div class='checkbox-click float-left pad-right-big pad-bottom-big' style='min-width: 150px;'><input type='checkbox' class='checkbox-usersallowed float-left margin-right-sml' {0} value='{1}' style='margin-top: {2};' />&nbsp;{3}</div>";
        Groups groups = new Groups(HttpContext.Current.User.Identity.Name);

        foreach (string group in groupList) {
            List<string> users = groups.GetMembers_of_Group(group);
            string groupImg = groups.GetGroupImg_byID(group);

            if (groupImg.StartsWith("~/")) {
                groupImg = ResolveUrl(groupImg);
            }

            string groupImgHtmlCtrl = "<img alt='' src='" + groupImg + "' class='float-left margin-right' style='max-height: 24px;' />";
            str.Append("<h3 class='pad-bottom'>" + groupImgHtmlCtrl + groups.GetGroupName_byID(group) + "</h3><div class='clear-space'></div><div class='clear-space'></div>");
            foreach (string user in users) {
                string isChecked = string.Empty;
                if (user.ToLower() == HttpContext.Current.User.Identity.Name.ToLower()) {
                    isChecked = "checked='checked'";
                }
                MemberDatabase tempMember = new MemberDatabase(user);

                string un = HelperMethods.MergeFMLNames(tempMember);
                if ((user.Length > 15) && (!string.IsNullOrEmpty(tempMember.LastName)))
                    un = tempMember.FirstName + " " + tempMember.LastName[0].ToString() + ".";

                if (un.ToLower() == "n/a")
                    un = user;

                string marginTop = "3px";
                string userNameTitle = "<h4>" + un + "</h4>";
                string acctImage = tempMember.AccountImage;
                if (!string.IsNullOrEmpty(acctImage)) {
                    userNameTitle = "<h4 class='float-left pad-top pad-left-sml'>" + un + "</h4>";
                    marginTop = "8px";
                }

                string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30);
                str.AppendFormat(checkboxInput, isChecked, user, marginTop, userImageAndName + userNameTitle);
            }
            str.Append("<div class='clear-space'></div><div class='clear-space'></div><div class='clear-space'></div>");
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Append("<h4 class='pad-all'>There are no usrs to select from</h4>");
        }

        pnl_usersAllowedToEdit.Controls.Add(new LiteralControl(str.ToString()));
    }

    private void BuildChartTypeList() {
        ddl_ChartType.Items.Clear();
        Array chartTypes = Enum.GetValues(typeof(ChartType));
        foreach (ChartType type in chartTypes) {
            if (type == ChartType.None) {
                continue;
            }

            ddl_ChartType.Items.Add(new ListItem(type.ToString(), type.ToString()));
        }
    }

    private string BuildDropDownChartType(CustomTable_Coll coll) {
        string ddlReturn = "<select id='" + coll.ID + "-charttype-select' onchange='UpdateSidebarChartType(\"" + coll.ID + "\");'>";
        Array chartTypes = Enum.GetValues(typeof(ChartType));
        foreach (ChartType type in chartTypes) {
            string selected = string.Empty;
            if (coll.Chart_Type.ToString().ToLower() == type.ToString().ToLower()) {
                selected = " selected";
            }
            ddlReturn += "<option" + selected + ">" + type.ToString() + "</option>";
        }
        ddlReturn += "</select>";
        return ddlReturn;
    }

    private void BuildTableList() {
        pnl_tableList.Controls.Clear();
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
            ctv.BuildEntriesAll();
        else
            ctv.BuildEntriesForUser();

        App apps = new App();

        StringBuilder str = new StringBuilder();
        str.Append("<table cellpadding='5' cellspacing='0' style='width: 100%;'>");

        int count = 1;
        foreach (CustomTable_Coll coll in ctv.CustomTableList) {
            MemberDatabase tempMember = new MemberDatabase(coll.CreatedBy);

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckCustomTablesGroupAssociation(coll, _member)) {
                    continue;
                }
            }

            string column = "<tr class='myItemStyle entryIDRow GridNormalRow'>";
            column += "<td width='45px' align='center' class='GridViewNumRow border-bottom'>" + count + "</td>";
            column += "<td class='border-right border-bottom' width='150px'>" + coll.TableName + "</td>";
            column += "<td class='border-right border-bottom'>" + coll.ChartTitle + "</td>";
            column += "<td class='border-right border-bottom' align='center' width='180px'>" + HelperMethods.MergeFMLNames(tempMember) + "</td>";
            column += "<td class='border-right border-bottom' align='center' width='180px'>" + coll.DateCreated + "</td>";

            string deleteBtn = "<a href='#delete' onclick='DeleteTable(\"" + coll.ID + "\", \"" + coll.TableName + "\");return false;' class='td-delete-btn table-action-btns' title='Delete'></a>";
            string editBtn = "<a href='#edit' onclick='EditTable(\"" + coll.TableID + "\", \"" + coll.TableName + "\");return false;' class='td-edit-btn table-action-btns margin-right' title='Edit'></a>";

            string _check = string.Empty;
            if (coll.Sidebar)
                _check = " checked=\"checked\"";

            string _check2 = string.Empty;
            if (coll.NotifyUsers)
                _check2 = " checked=\"checked\"";

            string recreateAppBtn = "";
            if (!AppExists(coll.AppID))
                recreateAppBtn = "<div class='clear-space-two'></div><a href='#createapp' onclick='RecreateApp(\"" + coll.AppID.Replace("app-", "") + "\", \"" + coll.TableName + "\", \"" + coll.Sidebar.ToString().ToLower() + "\", \"" + coll.Chart_Type.ToString() + "\");return false;' class='sb-links table-action-btns' title='Create App'>Create</a>";
            else {
                if ((_username.ToLower() != coll.CreatedBy.ToLower()) && (apps.GetIsPrivate(coll.AppID)) && (_username.ToLower() != ServerSettings.AdminUserName.ToLower())) {
                    continue;
                }
            }

            string shouldCheck = "true";
            if (coll.Sidebar)
                shouldCheck = "false";

            string sidebarCheckbox = "<input type=\"checkbox\" id=\"" + coll.ID + "-sidebar-cb\" onchange='UpdateSidebarChartType(\"" + coll.ID + "\");'" + _check + " title='Display a sidebar with the month selector on the app' />";
            string notfiCheckbox = "<input type=\"checkbox\" id=\"" + coll.ID + "-notifi-cb\" onchange='UpdateSidebarChartType(\"" + coll.ID + "\");'" + _check2 + " title='Notify Users when a change is made to the table' />";
            string dropDownChartType = BuildDropDownChartType(coll);
            string usersAllowedBtn = "<a href='#' onclick=\"EditUsersAllowed('" + coll.AppID + "', '" + coll.ID + "', '" + coll.TableName + "');return false;\">Click to Edit</a>";

            if (IsReadOnly) {
                editBtn = "<div class='pad-bottom' style='padding-top: 20px;'></div>";
                deleteBtn = string.Empty;
                recreateAppBtn = string.Empty;

                sidebarCheckbox = "<input type=\"checkbox\" id=\"" + coll.ID + "-sidebar-cb\"" + _check + " title='Display a sidebar with the month selector on the app' disabled='disabled' />";
                notfiCheckbox = "<input type=\"checkbox\" id=\"" + coll.ID + "-notifi-cb\"" + _check2 + " title='Notify Users when a change is made to the table' disabled='disabled' />";
                dropDownChartType = coll.Chart_Type.ToString();
                usersAllowedBtn = "N/A";
            }

            column += "<td class='border-right border-bottom' align='center' width='60px'>" + sidebarCheckbox + "</td>";
            column += "<td class='border-right border-bottom' align='center' width='60px'>" + notfiCheckbox + "</td>";
            column += "<td class='border-right border-bottom' align='center' width='125px'>" + dropDownChartType + "</td>";
            column += "<td class='border-right border-bottom' align='center' width='100px'>" + usersAllowedBtn + "</td>";
            column += "<td class='border-right border-bottom' align='center' width='75px'>" + editBtn + deleteBtn + recreateAppBtn + "</td>";
            column += "</tr>";
            str.Append(column);

            count++;
        }

        str.Append("</table>");

        if (!string.IsNullOrEmpty(str.ToString()) && count > 1)
            pnl_tableList.Controls.Add(new LiteralControl(str.ToString()));
        else
            pnl_tableList.Controls.Add(new LiteralControl("<div class='emptyGridView'>No custom tables found.</div>"));
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