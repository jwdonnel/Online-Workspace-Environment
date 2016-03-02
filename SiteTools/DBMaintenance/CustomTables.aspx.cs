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
            Page.Response.Redirect("~/" + ServerSettings.DefaultStartupPage);
        }
        else {
            _username = userId.Name;
            if (ServerSettings.AdminPagesCheck(Page.ToString(), _username)) {
                ctv = new CustomTableViewer(_username);
                HelperMethods.SetIsSocialUserForDeleteItems(Page, _username);
                AssociateWithGroups = _ss.AssociateWithGroups;

                _member = new MemberDatabase(_username);
                _siteTheme = _member.SiteTheme;

                BuildChartTypeList();
                BuildUsersAllowedToEdit();

                if (_ss.LockCustomTables) {
                    ltl_locked.Text = HelperMethods.GetLockedByMessage();
                    pnl_columnEditor.Enabled = false;
                    pnl_columnEditor.Visible = false;
                    btn_customTable_create.Visible = false;
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

                if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
                    cb_InstallAfterLoad.Enabled = false;
                    cb_InstallAfterLoad.Visible = false;
                    cb_isPrivate.Enabled = false;
                    cb_isPrivate.Visible = false;
                    cb_InstallAfterLoad.Checked = false;
                    cb_isPrivate.Checked = false;
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
        string checkboxInput = "<div class='checkbox-click float-left pad-right-big pad-top pad-bottom' style='min-width: 150px;'><input type='checkbox' class='checkbox-usersallowed float-left margin-right-sml' {0} value='{1}' style='margin-top: {2};' />&nbsp;{3}</div>";

        MembershipUserCollection userColl = Membership.GetAllUsers();
        foreach (MembershipUser membershipUser in userColl) {
            string user = membershipUser.UserName;
            if (user.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                continue;
            }

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

            string marginTop = "8px";
            string userNameTitle = "<h4 class='float-left pad-top pad-left-sml'>" + un + "</h4>";
            string acctImage = tempMember.AccountImage;

            string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30);
            str.AppendFormat(checkboxInput, isChecked, user, marginTop, userImageAndName + userNameTitle);
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Append("<h4 class='pad-top-big'>There are no usrs to select from</h4>");
        }

        pnl_usersAllowedToEdit.Controls.Add(new LiteralControl(str.ToString()));
    }

    private void BuildChartTypeList() {
        string chartList = "availableCharts='";
        Array chartTypes = Enum.GetValues(typeof(ChartType));
        foreach (ChartType type in chartTypes) {
            chartList += type.ToString() + ";";
        }
        chartList += "';";
        RegisterPostbackScripts.RegisterStartupScript(this, chartList);

        ddl_ChartType.Items.Clear();
        foreach (ChartType type in chartTypes) {
            ddl_ChartType.Items.Add(new ListItem(type.ToString(), type.ToString()));
        }
    }

    private void BuildTableList() {
        pnl_tableList.Controls.Clear();
        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName))
            ctv.BuildEntriesAll();
        else
            ctv.BuildEntriesForUser();

        App apps = new App(string.Empty);

        StringBuilder str = new StringBuilder();
        int count = 1;
        foreach (CustomTable_Coll row in ctv.CustomTableList) {
            MemberDatabase tempMember = new MemberDatabase(row.UpdatedBy);

            if (AssociateWithGroups) {
                if (!ServerSettings.CheckCustomTablesGroupAssociation(row, _member)) {
                    continue;
                }
            }

            string search = tb_searchcustomtables.Text.Trim().ToLower();
            if (search != "search custom tables" && !string.IsNullOrEmpty(search)) {
                if (!search.Contains(row.Description.ToLower()) && !search.Contains(row.UpdatedBy.ToLower()) && !search.Contains(row.TableName.ToLower())) {
                    continue;
                }
            }

            str.Append("<div data-id='" + row.TableID + "' class='import-entry'>");
            str.Append("<table class='import-entry-table'>");

            #region Non-editable selection

            string description = "No description available";
            if (!string.IsNullOrEmpty(row.Description)) {
                description = row.Description;
            }

            str.AppendFormat("<tr><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Name/Description", row.TableName + " - " + description);
            str.AppendFormat("<tr><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Updated By", HelperMethods.MergeFMLNames(tempMember) + " on " + row.DateUpdated.ToString());
            str.AppendFormat("<tr><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "", "<a href='#' class='showhidedetails' data-rowid='" + row.ID + "' onclick=\"ShowHideTableDetails('" + row.ID + "');return false;\">Show Details</a>");

            str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Table ID", row.TableID);

            string _checkedNotifyUsers = string.Empty;
            if (row.NotifyUsers) {
                _checkedNotifyUsers = "checked='checked'";
            }

            string chartType = ctv.GetChartTypeFromCustomizations(row.TableCustomizations).ToString();
            str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Notify Users", "<input type='checkbox' disabled='disabled' " + _checkedNotifyUsers + " />");

            string chartImg = string.Empty;
            if (chartType != ChartType.None.ToString()) {
                chartImg = "<img src='" + ResolveUrl("~/Standard_Images/ChartTypes/" + chartType.ToLower() + ".png") + "' class='float-left pad-right' style='height: 16px;' />";
            }

            str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Chart Type", chartImg + chartType);
            if (chartType != ChartType.None.ToString()) {
                string chartTitle = ctv.GetChartTitleFromCustomizations(row.TableCustomizations);
                if (string.IsNullOrEmpty(chartTitle)) {
                    chartTitle = "<i>(No title available)</i>";
                }

                string[] chartColumns = ctv.GetChartColumnsFromCustomizations(row.TableCustomizations).Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries);
                string chartColumnStr = "<ul class='table-columns-forchart'>";
                foreach (string cc in chartColumns) {
                    chartColumnStr += "<li>" + cc + "</li>";
                }
                chartColumnStr += "</ul>";

                str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}<div class='clear'></div></td></tr>", "Chart Title", chartTitle);
                str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}<div class='clear'></div></td></tr>", "Chart Columns", chartColumnStr);
            }

            #region Customizations
            string fontFamily = GetCustomizations("FontFamily", row);
            if (!string.IsNullOrEmpty(fontFamily)) {
                fontFamily = "font-family: " + fontFamily.Replace("'", "\"") + "!important;";
            }
            #endregion

            string overrideColumns = "<table cellspacing='0' cellpadding='0' style='width: 850px; border-collapse: collapse; " + fontFamily + "'><tbody>";
            overrideColumns += AddHeaderTableViewer(row);
            for (int i = 0; i < row.ColumnData.Count; i++) {
                overrideColumns += AddItemTableViewer(i + 1, row.ColumnData[i], row);
            }
            overrideColumns += "</tbody></table>";

            str.AppendFormat("<tr class='more-table-details' data-id='" + row.ID + "'><td class='import-column-title'>{0}</td><td class='import-column-value'>{1}</td></tr>", "Column Data", overrideColumns);
            #endregion

            str.Append("</table>");

            bool canAddEditButtons = true;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (_username.ToLower() != row.UpdatedBy.ToLower()) {
                    canAddEditButtons = false;
                }
            }

            if (canAddEditButtons) {
                str.Append("<div class='import-entry-editbtns'>");
                str.Append("<input type='button' class='input-buttons import-entry-editbtn margin-bottom' value='Edit' onclick=\"EditEntry('" + row.TableID + "');\" />");
                if (AppExists(row.AppID)) {
                    str.Append("<input type='button' class='input-buttons import-entry-editbtn margin-bottom' value='Delete' onclick=\"DeleteEntry('" + row.ID + "', '" + row.TableName + "');\" />");
                }
                else {
                    str.Append("<input type='button' class='input-buttons import-entry-editbtn margin-bottom' value='Delete' onclick=\"DeleteEntry('" + row.ID + "', '" + row.TableName + "');\" />");
                    str.Append("<input type='button' class='input-buttons import-entry-editbtn margin-bottom' value='Create App' onclick=\"RecreateApp('" + row.AppID + "', '" + row.TableID + "', '" + row.TableName + "', '" + row.Description + "');\" />");
                }
                str.Append("</div>");
            }

            str.Append("</div>");
            count++;
        }

        if (!string.IsNullOrEmpty(str.ToString()) && count > 1) {
            pnl_tableList.Controls.Add(new LiteralControl(str.ToString()));
        }
        else {
            pnl_tableList.Controls.Add(new LiteralControl("<div class='emptyGridView'>No custom tables found.</div>"));
        }
    }

    private static string GetCustomizations(string name, CustomTable_Coll row) {
        foreach (CustomTableCustomizations ctc in row.TableCustomizations) {
            if (ctc.customizeName == name) {
                return ctc.customizeValue;
            }
        }

        return string.Empty;
    }
    private static string AddHeaderTableViewer(CustomTable_Coll row) {
        string column = "<tr class='myHeaderStyle'>";

        string headerColor = GetCustomizations("HeaderColor", row);
        if (!string.IsNullOrEmpty(headerColor)) {
            string color = "customization-light-color";
            if (HelperMethods.UseDarkTextColorWithBackground(headerColor)) {
                color = "customization-dark-color";
            }
            headerColor = "background: " + headerColor + "!important;";
            column = "<tr class='myHeaderStyle " + color + "' style='" + headerColor + "'>";
        }

        column += "<td width='45px'></td>";
        column += "<td>Real Name</td>";
        column += "<td width='235px'>Shown Name</td>";
        column += "<td width='100px'>Data Type</td>";
        column += "<td width='75px'>Length</td>";
        column += "<td width='75px'>Nullable</td>";
        column += "</tr>";

        return column;
    }
    private static string AddItemTableViewer(int index, CustomTableColumnData data, CustomTable_Coll row) {
        string color = string.Empty;
        string alternativeRowColor = GetCustomizations("AlternativeRowColor", row);
        if (!string.IsNullOrEmpty(alternativeRowColor)) {
            color = "customization-light-color";
            if (HelperMethods.UseDarkTextColorWithBackground(alternativeRowColor)) {
                color = "customization-dark-color";
            }
            alternativeRowColor = "background: " + alternativeRowColor + "!important;";
        }

        string column = "<tr class='GridNormalRow myItemStyle " + color + "' style='" + alternativeRowColor + "'>";
        if ((index - 1) % 2 == 0) {
            string primaryRowColor = GetCustomizations("PrimaryRowColor", row);
            if (!string.IsNullOrEmpty(primaryRowColor)) {
                color = "customization-light-color";
                if (HelperMethods.UseDarkTextColorWithBackground(primaryRowColor)) {
                    color = "customization-dark-color";
                }
                primaryRowColor = "background: " + primaryRowColor + "!important;";
            }

            column = "<tr class='GridNormalRow myItemStyle " + color + "' style='" + primaryRowColor + "'>";
        }

        column += "<td align='center' class='GridViewNumRow cursor-default'>" + index.ToString() + "</td>";
        column += "<td class='border-right'><span class='float-left pad-top-sml pad-bottom-sml'>" + data.realName + "</span></td>";
        column += "<td class='border-right'>" + data.shownName + "</td>";
        column += "<td align='center' class='border-right'>" + data.dataType + "</td>";

        string lengthStr = data.dataLength.ToString();
        if (data.dataType != "nvarchar") {
            lengthStr = "-";
        }

        string _nullableChecked = "";
        if (data.nullable) {
            _nullableChecked = " checked='checked'";
        }

        column += "<td align='center' class='border-right'>" + lengthStr + "</td>";
        column += "<td align='center' class='border-right'><input type='checkbox' disabled='disabled' " + _nullableChecked + " /></td>";
        column += "</tr>";

        return column;
    }

    protected void hf_tableUpdate_Changed(object sender, EventArgs e) {
        BuildTableList();

        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            AppIconBuilder aib = new AppIconBuilder(Page, _member);
            aib.BuildAppsForUser();
        }

        hf_tableUpdate.Value = string.Empty;
    }
    protected void hf_tableDelete_Changed(object sender, EventArgs e) {
        string id = hf_tableDelete.Value.Trim();

        string appID = ctv.GetAppIDByID(id);
        ctv.DeleteRowByID(id, appID);
        BuildTableList();

        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            AppIconBuilder aib = new AppIconBuilder(Page, _member);
            aib.BuildAppsForUser();
        }
     
        hf_tableUpdate.Value = string.Empty;
        hf_tableDeleteID.Value = string.Empty;
        hf_tableDelete.Value = string.Empty;
    }

    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_tableDeleteID.Value)) {
            string createdBy = ctv.GetUpdatedByByID(hf_tableDeleteID.Value);

            bool isGood = false;

            MemberDatabase tempMember = new MemberDatabase(_username);
            if (tempMember.IsSocialAccount && createdBy.ToLower() == _username.ToLower()) {
                isGood = true;
            }
            else {
                if (_username.ToLower() == ServerSettings.AdminUserName.ToLower()) {
                    bool userLockedOut = MemberDatabase.CheckIfUserIsLockedOut(ServerSettings.AdminUserName);
                    isGood = Membership.ValidateUser(ServerSettings.AdminUserName, tb_passwordConfirm.Text);
                    MemberDatabase.UnlockUserIfNeeded(userLockedOut, ServerSettings.AdminUserName);
                }
                else if (!string.IsNullOrEmpty(createdBy) && createdBy.ToLower() == _username.ToLower()) {
                    bool userLockedOut = MemberDatabase.CheckIfUserIsLockedOut(createdBy);
                    isGood = Membership.ValidateUser(createdBy, tb_passwordConfirm.Text);
                    MemberDatabase.UnlockUserIfNeeded(userLockedOut, createdBy);
                }
            }

            if (isGood) {
                RegisterPostbackScripts.RegisterStartupScript(this, "PerformDelete('" + hf_tableDeleteID.Value + "');");
            }
            else {
                if (tempMember.IsSocialAccount) {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('You are not authorized to delete this table.');");
                }
                else {
                    tb_passwordConfirm.Text = "";
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Password is invalid');");
                }
            }
        }
    }
    private bool AppExists(string appId) {
        FileInfo fi = new FileInfo(ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\" + appId.Replace("app-", "") + ".ascx");
        if (fi.Exists) {
            return true;
        }

        return false;
    }
    private static string BuildTextboxForSelectEdit(string name, string value) {
        return "<input type='text' data-name='" + name + "' class='column-namechange-edit textEntry margin-bottom' maxlength='100' value='" + value + "' style='width: 150px; margin-top: -5px;' />";
    }



    #region Search Methods

    protected void imgbtn_clearsearch_Click(object sender, EventArgs e) {
    }
    protected void imgbtn_search_Click(object sender, EventArgs e) {
    }

    protected void imgbtn_clearsearchsummary_Click(object sender, EventArgs e) {
    }
    protected void imgbtn_searchsummary_Click(object sender, EventArgs e) {
    }

    #endregion

}