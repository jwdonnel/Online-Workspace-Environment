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
using System.Web.Script.Serialization;

public partial class SiteTools_CustomTables : BasePage {

    private CustomTableViewer ctv;

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
        ctv = new CustomTableViewer(CurrentUsername);
        HelperMethods.SetIsSocialUserForDeleteItems(Page, CurrentUsername);

        BuildChartTypeList();
        BuildUsersAllowedToEdit();

        if (MainServerSettings.LockCustomTables) {
            ltl_locked.Text = HelperMethods.GetLockedByMessage();
            pnl_columnEditor.Enabled = false;
            pnl_columnEditor.Visible = false;
            btn_customTable_create.Visible = false;
            IsReadOnly = true;

            if (!IsPostBack) {
                BuildTableList();
            }
        }
        else {
            IsReadOnly = false;

            if (!IsPostBack) {
                BuildTableList();
            }

            if (IsUserNameEqualToAdmin()) {
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

        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(CurrentUsername)) {
            cb_InstallAfterLoad.Enabled = false;
            cb_InstallAfterLoad.Visible = false;
            cb_isPrivate.Enabled = false;
            cb_isPrivate.Visible = false;
            cb_InstallAfterLoad.Checked = false;
            cb_isPrivate.Checked = false;
        }
    }

    private void BuildUsersAllowedToEdit() {
        pnl_usersAllowedToEdit.Controls.Clear();

        StringBuilder str = new StringBuilder();
        string checkboxInput = "<div class='checkbox-click float-left pad-right-big pad-top pad-bottom' style='min-width: 150px;'><input type='checkbox' class='checkbox-usersallowed float-left margin-right-sml' {0} value='{1}' style='margin-top: {2};' />&nbsp;{3}</div>";

        MembershipUserCollection userColl = Membership.GetAllUsers();
        foreach (MembershipUser membershipUser in userColl) {
            string user = membershipUser.UserName;
            if (IsUserNameEqualToAdmin(user)) {
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

            string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30, tempMember.SiteTheme);
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
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, true, 4, "MainTableList_Gridview");

        StringBuilder detailModals = new StringBuilder();

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns(string.Empty, "30px", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Name", "250px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Description", "400px", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Table ID", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Notify Users", "100px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Updated By", "150px", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Updated", "150px", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        if (IsUserInAdminRole()) {
            ctv.BuildEntriesAll();
        }
        else {
            ctv.BuildEntriesForUser();
        }

        App apps = new App(string.Empty);
        foreach (CustomTable_Coll row in ctv.CustomTableList) {
            MemberDatabase tempMember = new MemberDatabase(row.UpdatedBy);

            if (MainServerSettings.AssociateWithGroups) {
                if (!ServerSettings.CheckCustomTablesGroupAssociation(row, CurrentUserMemberDatabase)) {
                    continue;
                }
            }

            List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();

            #region Detail Modal
            detailModals.Append(CreateDetailsModal(row));
            #endregion

            #region App Icon
            Apps_Coll appColl = apps.GetAppInformation(row.AppID);
            string appIcon = ResolveUrl("~/" + appColl.Icon);
            string querySeperator = "?";
            if (appIcon.Contains("?")) {
                querySeperator = "&";
            }

            appIcon += string.Format("{0}{1}{2}", querySeperator, ServerSettings.TimestampQuery, HelperMethods.GetTimestamp());
            appIcon = "<img alt='' src='" + appIcon + "' style='width: 22px; height: 22px;' />";

            bodyColumns.Add(new TableBuilderBodyColumnValues(string.Empty, appIcon, TableBuilderColumnAlignment.Left));
            #endregion

            #region Table Name
            bodyColumns.Add(new TableBuilderBodyColumnValues("Name", row.TableName, TableBuilderColumnAlignment.Left));
            #endregion

            #region Description
            string description = "No description available";
            if (!string.IsNullOrEmpty(row.Description)) {
                description = row.Description;
            }

            bodyColumns.Add(new TableBuilderBodyColumnValues("Description", description, TableBuilderColumnAlignment.Left));
            #endregion

            #region Table ID
            bodyColumns.Add(new TableBuilderBodyColumnValues("Table ID", row.TableID, TableBuilderColumnAlignment.Left));
            #endregion

            #region Notify Users
            string _checkedNotifyUsers = string.Empty;
            if (row.NotifyUsers) {
                _checkedNotifyUsers = "checked='checked'";
            }
            bodyColumns.Add(new TableBuilderBodyColumnValues("Notify Users", "<input type='checkbox' disabled='disabled' " + _checkedNotifyUsers + " />", TableBuilderColumnAlignment.Left));
            #endregion

            #region Updated By
            bodyColumns.Add(new TableBuilderBodyColumnValues("Updated By", HelperMethods.MergeFMLNames(tempMember), TableBuilderColumnAlignment.Left));
            #endregion

            #region Date Updated
            bodyColumns.Add(new TableBuilderBodyColumnValues("Date Updated", row.DateUpdated.ToString(), TableBuilderColumnAlignment.Left));
            #endregion

            #region Action Buttons
            bool canAddEditButtons = true;
            if (!IsUserInAdminRole()) {
                if (CurrentUsername.ToLower() != row.UpdatedBy.ToLower()) {
                    canAddEditButtons = false;
                }
            }

            StringBuilder str = new StringBuilder();
            if (canAddEditButtons && !MainServerSettings.LockCustomTables) {
                str.Append("<a class='td-edit-btn' title='Edit' onclick=\"EditEntry('" + row.TableID + "');return false;\"></a>");
                str.Append("<a class='td-details-btn' title='More Details' onclick=\"ShowHideTableDetails('" + row.ID + "', '" + row.TableName + "');return false;\"></a>");
                if (AppExists(row.AppID)) {
                    str.Append("<a class='td-delete-btn' title='Delete' onclick=\"DeleteEntry('" + row.ID + "', '" + row.TableName + "');return false;\"></a>");
                }
                else {
                    str.Append("<a class='td-delete-btn' title='Delete' onclick=\"DeleteEntry('" + row.ID + "', '" + row.TableName + "');return false;\"></a>");
                    str.Append("<a class='td-restore-btn' title='Recreate App' onclick=\"RecreateApp('" + row.AppID + "', '" + row.TableID + "', '" + row.TableName + "', '" + row.Description + "');return false;\"></a>");
                }
            }
            else if (canAddEditButtons && MainServerSettings.LockCustomTables) {
                str.Append("<a class='td-details-btn' title='More Details' onclick=\"ShowHideTableDetails('" + row.ID + "', '" + row.TableName + "');return false;\"></a>");
            }
            #endregion

            tableBuilder.AddBodyRow(bodyColumns, str.ToString(), "data-id='" + row.TableID + "'");
        }
        #endregion

        pnl_tableList.Controls.Clear();
        pnl_tableList.Controls.Add(tableBuilder.CompleteTableLiteralControl("No custom tables found"));

        pnl_tableDetailList.Controls.Clear();
        pnl_tableDetailList.Controls.Add(new LiteralControl(detailModals.ToString()));

        updatepnl_tableList.Update();
    }
    private string CreateDetailsModal(CustomTable_Coll row) {
        StringBuilder str = new StringBuilder();

        str.Append("<div id=\"" + row.ID + "Modal-element\" class=\"Modal-element\">");
        str.Append("<div class=\"Modal-overlay\">");
        str.Append("<div class=\"Modal-element-align\">");
        str.Append("<div class=\"Modal-element-modal\" data-setwidth=\"800\">");

        str.Append("<div class=\"ModalHeader\"><div><div class=\"app-head-button-holder-admin\"><a href=\"#close\" onclick=\"CloseTableDetailsModal('" + row.ID + "');return false;\" class=\"ModalExitButton\"></a></div><span class=\"Modal-title\"></span></div></div>");

        str.Append("<div class=\"ModalScrollContent\">");
        str.Append("<div class=\"ModalPadContent\">");

        str.Append("<h2><span style='font-weight: normal!important;'>" + row.TableName + "</span></h2><div class='clear-space'></div><div class='clear-space'></div>");

        str.Append("<div data-id='" + row.TableID + "' class='import-entry'>");
        str.Append("<div class='import-entry-table'>");

        #region Month Selector
        string monthSelector = GetCustomizations("MonthSelector", row);
        if (!string.IsNullOrEmpty(monthSelector)) {
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Month Selector", monthSelector);
        }
        #endregion

        #region Show Row Counts
        string showRowCounts = GetCustomizations("ShowRowCounts", row);
        if (!string.IsNullOrEmpty(showRowCounts)) {
            string _checkedShowRowCounts = "";
            if (HelperMethods.ConvertBitToBoolean(showRowCounts)) {
                _checkedShowRowCounts = "checked='checked'";
            }
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Show Row Counts", "<input type='checkbox' disabled='disabled' " + _checkedShowRowCounts + " />");
        }
        #endregion

        #region Show Description
        string showDescriptionOnApp = GetCustomizations("ShowDescriptionOnApp", row);
        if (!string.IsNullOrEmpty(showDescriptionOnApp)) {
            string _checkedShowDescriptionOnApp = "";
            if (HelperMethods.ConvertBitToBoolean(showDescriptionOnApp)) {
                _checkedShowDescriptionOnApp = "checked='checked'";
            }
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Show Description", "<input type='checkbox' disabled='disabled' " + _checkedShowDescriptionOnApp + " />");
        }
        #endregion

        #region Table View Style
        string tableViewStyle = GetCustomizations("TableViewStyle", row);
        if (!string.IsNullOrEmpty(tableViewStyle)) {
            switch (tableViewStyle) {
                case "excel":
                    tableViewStyle = "Excel Spreadsheet";
                    break;
                default:
                    tableViewStyle = "Default";
                    break;
            }

            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Table View Style", tableViewStyle);
        }
        #endregion

        #region Title Color
        string appStyleTitleColor = GetCustomizations("AppStyleTitleColor", row);
        if (!string.IsNullOrEmpty(appStyleTitleColor)) {
            if (!appStyleTitleColor.StartsWith("#")) {
                appStyleTitleColor = "#" + appStyleTitleColor;
            }

            string titleColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + appStyleTitleColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Title Color", titleColorEle + appStyleTitleColor);
        }
        #endregion

        #region Background Color
        string appStyleBackgroundColor = GetCustomizations("AppStyleBackgroundColor", row);
        if (!string.IsNullOrEmpty(appStyleBackgroundColor)) {
            if (!appStyleBackgroundColor.StartsWith("#")) {
                appStyleBackgroundColor = "#" + appStyleBackgroundColor;
            }

            string bgColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + appStyleBackgroundColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Header Color", bgColorEle + appStyleBackgroundColor);
        }
        #endregion

        #region Background Image
        string appStyleBackgroundImage = GetCustomizations("AppStyleBackgroundImage", row);
        if (!string.IsNullOrEmpty(appStyleBackgroundImage)) {
            string imgEle = "<img alt='' src='" + appStyleBackgroundImage + "' style='max-height: 100px; max-width: 100px; />";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "' style='clear: both;'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Header Image", imgEle);
        }
        #endregion

        #region Table Header Color
        string headerColor = GetCustomizations("HeaderColor", row);
        if (!string.IsNullOrEmpty(headerColor)) {
            if (!headerColor.StartsWith("#")) {
                headerColor = "#" + headerColor;
            }

            string titleColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + headerColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Table Header Color", titleColorEle + headerColor);
        }
        #endregion

        #region Table Primary Row Color
        string primaryRowColor = GetCustomizations("PrimaryRowColor", row);
        if (!string.IsNullOrEmpty(primaryRowColor)) {
            if (!primaryRowColor.StartsWith("#")) {
                primaryRowColor = "#" + primaryRowColor;
            }

            string titleColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + primaryRowColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Table Primary Row Color", titleColorEle + primaryRowColor);
        }
        #endregion

        #region Table Alternative Row Color
        string alternativeRowColor = GetCustomizations("AlternativeRowColor", row);
        if (!string.IsNullOrEmpty(alternativeRowColor)) {
            if (!alternativeRowColor.StartsWith("#")) {
                alternativeRowColor = "#" + alternativeRowColor;
            }

            string titleColorEle = "<span class='float-left margin-right-sml' style='height: 14px; width: 14px; border: 1px solid #CCC; background-color: " + alternativeRowColor + ";'></span>";
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Table Alternative Row Color", titleColorEle + alternativeRowColor);
        }
        #endregion

        #region Font Family
        string fontFamily = GetCustomizations("FontFamily", row);
        if (!string.IsNullOrEmpty(fontFamily)) {
            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Font Family", fontFamily);
        }
        #endregion

        #region Chart Information
        string chartType = ctv.GetChartTypeFromCustomizations(row.TableCustomizations).ToString();
        string chartImg = string.Empty;
        if (chartType != ChartType.None.ToString()) {
            chartImg = "<img src='" + ResolveUrl("~/Standard_Images/ChartTypes/" + chartType.ToLower() + ".png") + "' class='float-left pad-right' style='height: 16px;' />";
        }

        str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Chart Type", chartImg + chartType);
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
            chartColumnStr += "</ul><div class='clear'></div>";

            str.AppendFormat("<div class='import-entry-row' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Chart Title", chartTitle);
            str.AppendFormat("<div class='import-entry-row import-entry-istablelist' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Chart Columns", chartColumnStr);
        }
        #endregion

        #region Column Data
        TableBuilder tableBuilder1 = new TableBuilder(this.Page, true, false, 0, "ColumnDataDetails_Gridview");

        List<TableBuilderHeaderColumns> headerColumns1 = new List<TableBuilderHeaderColumns>();
        headerColumns1.Add(new TableBuilderHeaderColumns("Real Name", string.Empty, false));
        headerColumns1.Add(new TableBuilderHeaderColumns("Shown Name", string.Empty, false));
        headerColumns1.Add(new TableBuilderHeaderColumns("Data Type", string.Empty, false));
        headerColumns1.Add(new TableBuilderHeaderColumns("Length", string.Empty, false));
        headerColumns1.Add(new TableBuilderHeaderColumns("Nullable", string.Empty, false));
        tableBuilder1.AddHeaderRow(headerColumns1, false);

        for (int i = 0; i < row.ColumnData.Count; i++) {
            CustomTableColumnData data = row.ColumnData[i];

            List<TableBuilderBodyColumnValues> bodyColumns1 = new List<TableBuilderBodyColumnValues>();
            bodyColumns1.Add(new TableBuilderBodyColumnValues("Real Name", data.realName, TableBuilderColumnAlignment.Left));
            bodyColumns1.Add(new TableBuilderBodyColumnValues("Shown Name", data.shownName, TableBuilderColumnAlignment.Left));
            bodyColumns1.Add(new TableBuilderBodyColumnValues("Data Type", data.dataType, TableBuilderColumnAlignment.Left));

            string lengthStr = data.dataLength.ToString();
            if (data.dataType != "nvarchar") {
                lengthStr = "-";
            }

            bodyColumns1.Add(new TableBuilderBodyColumnValues("Length", lengthStr, TableBuilderColumnAlignment.Left));

            string _nullableChecked = "";
            if (data.nullable) {
                _nullableChecked = " checked='checked'";
            }

            bodyColumns1.Add(new TableBuilderBodyColumnValues("Nullable", "<input type='checkbox' disabled='disabled' " + _nullableChecked + " />", TableBuilderColumnAlignment.Left));
            tableBuilder1.AddBodyRow(bodyColumns1);
        }

        str.AppendFormat("<div class='clear'></div><div class='import-entry-row import-entry-istablelist' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Column Data", tableBuilder1.CompleteTableString("No table data found", false));
        #endregion

        #region Summary Data
        if (row.SummaryData.Count > 0) {
            TableBuilder tableBuilder2 = new TableBuilder(this.Page, true, false, 0, "SummaryDataDetails_Gridview");

            List<TableBuilderHeaderColumns> headerColumns2 = new List<TableBuilderHeaderColumns>();
            headerColumns2.Add(new TableBuilderHeaderColumns("Summary Name", string.Empty, false));
            headerColumns2.Add(new TableBuilderHeaderColumns("Column Name", string.Empty, false));
            headerColumns2.Add(new TableBuilderHeaderColumns("Formula", string.Empty, false));
            tableBuilder2.AddHeaderRow(headerColumns2, false);

            for (int i = 0; i < row.SummaryData.Count; i++) {
                List<TableBuilderBodyColumnValues> bodyColumns2 = new List<TableBuilderBodyColumnValues>();
                bodyColumns2.Add(new TableBuilderBodyColumnValues("Summary Name", row.SummaryData[i].summaryName, TableBuilderColumnAlignment.Left));
                bodyColumns2.Add(new TableBuilderBodyColumnValues("Column Name", row.SummaryData[i].columnName, TableBuilderColumnAlignment.Left));
                bodyColumns2.Add(new TableBuilderBodyColumnValues("Formula", HttpUtility.UrlDecode(row.SummaryData[i].formulaType), TableBuilderColumnAlignment.Left));
                tableBuilder2.AddBodyRow(bodyColumns2);
            }

            str.AppendFormat("<div class='import-entry-row import-entry-istablelist' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Summary Data", tableBuilder2.CompleteTableString("No summary data found", false));
        }
        #endregion

        #region Default Values
        string defaultValues = GetCustomizations("DefaultValues", row);
        if (!string.IsNullOrEmpty(defaultValues)) {
            JavaScriptSerializer columnsSerializer = ServerSettings.CreateJavaScriptSerializer();
            Dictionary<string, string>[] defaultValuesArray = columnsSerializer.Deserialize<Dictionary<string, string>[]>(defaultValues);

            if (defaultValuesArray.Length > 0) {
                TableBuilder tableBuilder3 = new TableBuilder(this.Page, true, false, 0, "DefaultValuesDetails_Gridview");

                List<TableBuilderHeaderColumns> headerColumns3 = new List<TableBuilderHeaderColumns>();
                headerColumns3.Add(new TableBuilderHeaderColumns("Column Name", string.Empty, false));
                headerColumns3.Add(new TableBuilderHeaderColumns("Default Value", string.Empty, false));
                tableBuilder3.AddHeaderRow(headerColumns3, false);

                for (int i = 0; i < defaultValuesArray.Length; i++) {
                    List<TableBuilderBodyColumnValues> bodyColumns3 = new List<TableBuilderBodyColumnValues>();
                    bodyColumns3.Add(new TableBuilderBodyColumnValues("Column Name", defaultValuesArray[i]["name"], TableBuilderColumnAlignment.Left));
                    bodyColumns3.Add(new TableBuilderBodyColumnValues("Default Value", defaultValuesArray[i]["value"], TableBuilderColumnAlignment.Left));
                    tableBuilder3.AddBodyRow(bodyColumns3);
                }

                str.AppendFormat("<div class='import-entry-row import-entry-istablelist' data-id='" + row.ID + "'><div class='import-column-title'>{0}</div><div class='import-column-value'>{1}</div><div class='clear'></div></div>", "Default Values", tableBuilder3.CompleteTableString("No default values found", false));
            }
        }
        #endregion

        str.Append("</div></div></div></div>");
        str.Append("<div class=\"ModalButtonHolder\"><input type=\"button\" class=\"input-buttons modal-cancel-btn\" value=\"Close\" onclick=\"CloseTableDetailsModal('" + row.ID + "');\" /><div class=\"clear\"></div></div>");
        str.Append("</div></div></div></div>");

        return str.ToString();
    }

    private static string GetCustomizations(string name, CustomTable_Coll row) {
        foreach (CustomTableCustomizations ctc in row.TableCustomizations) {
            if (ctc.customizeName == name) {
                return ctc.customizeValue;
            }
        }

        return string.Empty;
    }

    protected void hf_tableUpdate_Changed(object sender, EventArgs e) {
        BuildTableList();

        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
            aib.BuildAppsForUser(true);
        }

        hf_tableUpdate.Value = string.Empty;
    }
    protected void hf_tableDelete_Changed(object sender, EventArgs e) {
        string id = hf_tableDelete.Value.Trim();

        string appID = ctv.GetAppIDByID(id);
        ctv.DeleteRowByID(id, appID);
        BuildTableList();

        if (!HelperMethods.ConvertBitToBoolean(Request.QueryString["mobileMode"])) {
            AppIconBuilder aib = new AppIconBuilder(Page, CurrentUserMemberDatabase);
            aib.BuildAppsForUser(true);
        }
     
        hf_tableUpdate.Value = string.Empty;
        hf_tableDeleteID.Value = string.Empty;
        hf_tableDelete.Value = string.Empty;
    }

    protected void btn_passwordConfirm_Clicked(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_tableDeleteID.Value)) {
            string createdBy = ctv.GetUpdatedByByID(hf_tableDeleteID.Value);

            bool isGood = false;

            if (CurrentUserMemberDatabase.IsSocialAccount && createdBy.ToLower() == CurrentUsername.ToLower()) {
                isGood = true;
            }
            else {
                if (IsUserNameEqualToAdmin()) {
                    bool userLockedOut = MemberDatabase.CheckIfUserIsLockedOut(ServerSettings.AdminUserName);
                    isGood = Membership.ValidateUser(ServerSettings.AdminUserName, tb_passwordConfirm.Text);
                    MemberDatabase.UnlockUserIfNeeded(userLockedOut, ServerSettings.AdminUserName);
                }
                else if (!string.IsNullOrEmpty(createdBy) && createdBy.ToLower() == CurrentUsername.ToLower()) {
                    bool userLockedOut = MemberDatabase.CheckIfUserIsLockedOut(createdBy);
                    isGood = Membership.ValidateUser(createdBy, tb_passwordConfirm.Text);
                    MemberDatabase.UnlockUserIfNeeded(userLockedOut, createdBy);
                }
            }

            if (isGood) {
                RegisterPostbackScripts.RegisterStartupScript(this, "PerformDelete('" + hf_tableDeleteID.Value + "');");
            }
            else {
                if (CurrentUserMemberDatabase.IsSocialAccount) {
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

}