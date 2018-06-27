using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;

public class TableBuilder {

    #region Private Variables
    private readonly ServerSettings _ss = new ServerSettings();
    private string _headerStr = string.Empty;
    private StringBuilder _bodyStr = new StringBuilder();
    private string _insertStr  = string.Empty;
    private Page _page;
    private string _cookieName = string.Empty;
    private bool _allowPaging = false;
    private bool _showRowCount = false;
    private bool _useAlternateRowClass = false;
    private bool _addEditControls = false;
    private int _editControlCount = 0;
    private int _rowCount = 0;
    private int _columnCount = 0;
    private string _initialSortColumn = string.Empty;
    private string _initialSortDir = string.Empty;
    private string _pageSize = string.Empty;
    private string _tableId = string.Empty;
    private const string _emptyRowText = "No data found";
    #endregion

    #region Public Variables
    public const int DefaultPageSize = 30;
    public static bool ShowRowCountGridViewTable {
        get {
            try {
                if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated) {
                    MemberDatabase member = new MemberDatabase(HttpContext.Current.User.Identity.Name);
                    return member.ShowRowCountGridViewTable;
                }
                else {
                    NewUserDefaults demoCustomizations = new NewUserDefaults("DemoNoLogin");
                    demoCustomizations.GetDefaults();
                    if (demoCustomizations.DefaultTable != null && demoCustomizations.DefaultTable.ContainsKey("ShowRowCountGridViewTable")) {
                        return HelperMethods.ConvertBitToBoolean(demoCustomizations.DefaultTable["ShowRowCountGridViewTable"]);
                    }
                }
            }
            catch { }
            return false;
        }
    }
    public static bool UseAlternateGridviewRows {
        get {
            try {
                if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated) {
                    MemberDatabase member = new MemberDatabase(HttpContext.Current.User.Identity.Name);
                    return member.UseAlternateGridviewRows;
                }
                else {
                    NewUserDefaults demoCustomizations = new NewUserDefaults("DemoNoLogin");
                    demoCustomizations.GetDefaults();
                    if (demoCustomizations.DefaultTable != null && demoCustomizations.DefaultTable.ContainsKey("UseAlternateGridviewRows")) {
                        return HelperMethods.ConvertBitToBoolean(demoCustomizations.DefaultTable["UseAlternateGridviewRows"]);
                    }
                }
            }
            catch { }
            return false;
        }
    }
    #endregion

    #region Constructor

    public TableBuilder(Page page, bool allowPaging, bool addEditControls, int editControlCount, string tableId = "") {
        _page = page;
        _allowPaging = allowPaging;
        _showRowCount = TableBuilder.ShowRowCountGridViewTable;
        _useAlternateRowClass = TableBuilder.UseAlternateGridviewRows;
        _addEditControls = addEditControls;
        _editControlCount = editControlCount;

        if (string.IsNullOrEmpty(tableId)) {
            _tableId = _page.ToString() + "Gridview";
            _cookieName = _page.ToString() + "GridviewPageSize";
        }
        else {
            _tableId = tableId;
            _cookieName = _tableId + "GridviewPageSize";
        }

        _pageSize = DefaultPageSize.ToString();
        object cookieValue = new SaveControls().GetCookie(_cookieName);
        if (cookieValue != null && !string.IsNullOrEmpty(cookieValue.ToString())) {
            _pageSize = cookieValue.ToString();
        }
    }
    public TableBuilder(Page page, bool allowPaging, bool addEditControls, int editControlCount, bool showRowCount, string tableId = "") {
        _page = page;
        _allowPaging = allowPaging;
        _showRowCount = showRowCount;
        _useAlternateRowClass = TableBuilder.UseAlternateGridviewRows;
        _addEditControls = addEditControls;
        _editControlCount = editControlCount;

        if (string.IsNullOrEmpty(tableId)) {
            _tableId = _page.ToString() + "Gridview";
            _cookieName = _page.ToString() + "GridviewPageSize";
        }
        else {
            _tableId = tableId;
            _cookieName = _tableId + "GridviewPageSize";
        }

        _pageSize = DefaultPageSize.ToString();
        object cookieValue = new SaveControls().GetCookie(_cookieName);
        if (cookieValue != null && !string.IsNullOrEmpty(cookieValue.ToString())) {
            _pageSize = cookieValue.ToString();
        }
    }

    #endregion

    #region Add Table Parts

    /// <summary> Add a header row to table
    /// </summary>
    /// <param name="headerColumns">Header Column Collection</param>
    /// <param name="allowSorting">Set to true to allow sorting of your table</param>
    /// <param name="initialSortColumn"></param>
    /// <param name="initialSortDir"></param>
    public void AddHeaderRow(List<TableBuilderHeaderColumns> headerColumns, bool allowSorting, string initialSortColumn = "", string initialSortDir = "ASC", string additionalRowAttributes = "", string additionalRowClasses = "") {
        if (headerColumns.Count > 0) {
            _initialSortColumn = initialSortColumn;
            _initialSortDir = initialSortDir;
            if (string.IsNullOrEmpty(_initialSortDir)) {
                _initialSortDir = "ASC";
            }

            if (!string.IsNullOrEmpty(additionalRowAttributes)) {
                additionalRowAttributes = " " + additionalRowAttributes;
            }

            if (!string.IsNullOrEmpty(additionalRowClasses)) {
                additionalRowClasses = " " + additionalRowClasses;
            }

            StringBuilder str = new StringBuilder();
            str.AppendFormat("<tr class=\"myHeaderStyle{0}\"{1}>", additionalRowClasses, additionalRowAttributes);

            if (_showRowCount) {
                str.Append("<td width=\"45px\" style=\"padding-left: 10px;\">#</td>");
                _columnCount++;
            }

            int currentColumnNumber = 0;
            foreach (TableBuilderHeaderColumns column in headerColumns) {
                string widthAttr = string.Empty;
                string sortTitleAttr = string.Empty;
                string sortDataColumnAttr = string.Empty;
                string sortClassAttr = string.Empty;
                string sortClickEventAttr = string.Empty;

                #region Set Width
                if (!string.IsNullOrEmpty(column.columnWidth)) {
                    string columnWidth = column.columnWidth;
                    if (!columnWidth.EndsWith("px") && !columnWidth.EndsWith("%")) {
                        columnWidth += "px";
                    }

                    if (column.setAsMinWidth) {
                        widthAttr = string.Format(" style=\"min-width: {0};\"", columnWidth);
                    }
                    else {
                        widthAttr = string.Format(" width=\"{0}\"", columnWidth);
                    }
                }
                #endregion

                #region Set Sort Controls
                if (allowSorting && !string.IsNullOrEmpty(column.columnName) && column.AllowSorting) {
                    if (string.IsNullOrEmpty(_initialSortColumn)) {
                        _initialSortColumn = column.columnName;
                    }

                    sortTitleAttr = string.Format(" title=\"Sort by {0}\"", column.columnName);
                    sortDataColumnAttr = string.Format(" data-columnname=\"{0}\"", column.columnName);
                    sortClassAttr = " class=\"td-sort-click\"";
                    sortClickEventAttr = " onclick=\"openWSE.GridViewMethods.SortColumn_Click(this);\"";
                }
                #endregion

                str.AppendFormat("<td{0}{1}{2}{3}{4}>{5}</td>", widthAttr, sortTitleAttr, sortDataColumnAttr, sortClassAttr, sortClickEventAttr, column.columnName);
                _columnCount++;
                currentColumnNumber++;
            }

            if (_addEditControls && _editControlCount > 0) {
                str.Append("<td class=\"edit-column-" + _editControlCount.ToString() + "-items\"></td>");
                _columnCount++;
            }

            str.Append("</tr>");

            _headerStr = str.ToString();
        }
    }

    /// <summary> Add a new row to the body of your table
    /// </summary>
    /// <param name="bodyColumnValues">Body Column Value Collection</param>
    /// <param name="editControls">The edit controls if needed</param>
    /// <param name="additionalRowAttributes"></param>
    /// <param name="additionalRowClasses"></param>
    public void AddBodyRow(List<TableBuilderBodyColumnValues> bodyColumnValues, string editControls = "", string additionalRowAttributes = "", string additionalRowClasses = "") {
        if (bodyColumnValues.Count > 0) {
            StringBuilder str = new StringBuilder();

            string altRowClass = " GridNormalRow";
            if (_useAlternateRowClass && _rowCount % 2 == 0) {
                altRowClass = " GridAlternate";
            }

            if (_allowPaging && _pageSize != "all") {
                int pageSizeInt = DefaultPageSize;
                int.TryParse(_pageSize, out pageSizeInt);
                if (_rowCount < 0 || _rowCount >= pageSizeInt) {
                    altRowClass += " hide-table-row";
                }
            }

            if (!string.IsNullOrEmpty(additionalRowAttributes)) {
                additionalRowAttributes = " " + additionalRowAttributes;
            }

            if (!string.IsNullOrEmpty(additionalRowClasses)) {
                altRowClass += " " + additionalRowClasses;
            }

            str.AppendFormat("<tr class=\"myItemStyle{0}\"{1}>", altRowClass, additionalRowAttributes);

            bool addColumnCount = false;

            if (_showRowCount) {
                str.AppendFormat("<td class=\"GridViewNumRow\">{0}</td>", (_rowCount + 1).ToString());
                if (_columnCount == 0) {
                    addColumnCount = true;
                    _columnCount++;
                }
            }

            int currentColumnNumber = 0;
            foreach (TableBuilderBodyColumnValues columnValue in bodyColumnValues) {
                string dataColumnAttr = string.Empty;
                string alignmentAttr = string.Empty;

                if (!string.IsNullOrEmpty(columnValue.columnName)) {
                    dataColumnAttr = string.Format(" data-columnname=\"{0}\"", columnValue.columnName);
                }

                if (!string.IsNullOrEmpty(columnValue.columnAlignment.ToString())) {
                    alignmentAttr = string.Format(" align=\"{0}\"", columnValue.columnAlignment.ToString().ToLower());
                }

                string value = columnValue.columnValue;
                if (!_addEditControls || _editControlCount == 0) {
                    value = string.Format("<div class=\"pad-top-sml pad-bottom-sml\">{0}</div>", value);
                }

                string additionalAttributes = columnValue.AdditionalAttributes;
                if (!string.IsNullOrEmpty(additionalAttributes)) {
                    additionalAttributes = " " + additionalAttributes;
                }

                string additionalClasses = columnValue.AdditionalClasses;
                if (!string.IsNullOrEmpty(additionalClasses)) {
                    additionalClasses = " " + additionalClasses;
                }

                str.AppendFormat("<td class=\"{0}\"{1}{2}{3}>{4}</td>", additionalClasses, dataColumnAttr, alignmentAttr, additionalAttributes, value);
                if (addColumnCount) {
                    _columnCount++;
                }

                currentColumnNumber++;
            }

            if (_addEditControls && _editControlCount > 0) {
                if (string.IsNullOrEmpty(editControls)) {
                    editControls = "<div class=\"td-empty-btn\"></div>";
                }

                str.AppendFormat("<td class=\"myItemStyle-action-btns\" align=\"center\">{0}</td>", editControls);
                if (addColumnCount) {
                    _columnCount++;
                }
            }

            str.Append("</tr>");
            _rowCount++;

            _bodyStr.Append(str.ToString());
        }
    }

    /// <summary> Add an insert row to your table
    /// </summary>
    /// <param name="insertColumns">Insert Column Value Collection</param>
    /// <param name="addBtnClickEvent">Add event used on the javascript side</param>
    /// <param name="additionalRowAttributes"></param>
    /// <param name="additionalRowClasses"></param>
    public void AddInsertRow(List<TableBuilderInsertColumnValues> insertColumns, string addBtnClickEvent, string additionalRowAttributes = "", string additionalRowClasses = "") {
        if (insertColumns.Count > 0 && _addEditControls && _editControlCount > 0) {
            StringBuilder str = new StringBuilder();

            string altRowClass = " GridNormalRow";
            if (_useAlternateRowClass && _rowCount % 2 == 0) {
                altRowClass = " GridAlternate";
            }

            if (!string.IsNullOrEmpty(additionalRowAttributes)) {
                additionalRowAttributes = " " + additionalRowAttributes;
            }

            if (!string.IsNullOrEmpty(additionalRowClasses)) {
                altRowClass += " " + additionalRowClasses;
            }

            str.AppendFormat("<tr class=\"addItemRow{0}\"{1}>", altRowClass, additionalRowAttributes);

            bool addColumnCount = false;

            if (_showRowCount) {
                str.Append("<td class=\"GridViewNumRow\"></td>");
                if (_columnCount == 0) {
                    addColumnCount = true;
                    _columnCount++;
                }
            }

            addBtnClickEvent = addBtnClickEvent.Trim();
            if (!addBtnClickEvent.EndsWith(";")) {
                addBtnClickEvent += ";";
            }

            addBtnClickEvent = HttpUtility.UrlEncode(addBtnClickEvent.Replace(" ", "&nbsp;"));

            foreach (TableBuilderInsertColumnValues columnValue in insertColumns) {
                string dataColumnAttr = string.Empty;
                string alignmentAttr = string.Empty;

                if (!string.IsNullOrEmpty(columnValue.columnName)) {
                    dataColumnAttr = string.Format(" data-columnname=\"{0}\"", columnValue.columnName);
                }

                if (!string.IsNullOrEmpty(columnValue.columnAlignment.ToString())) {
                    alignmentAttr = string.Format(" align=\"{0}\"", columnValue.columnAlignment.ToString().ToLower());
                }

                string additionalClasses = columnValue.AdditionalClasses;
                if (!string.IsNullOrEmpty(additionalClasses)) {
                    additionalClasses = " " + additionalClasses;
                }

                string inputStr = string.Empty;
                if (!string.IsNullOrEmpty(columnValue.InputId) && columnValue.InputType != TableBuilderInsertType.None) {
                    if (columnValue.InputType == TableBuilderInsertType.Image) {
                        inputStr = string.Format("<img id=\"{0}\" src=\"{1}\" alt='' />", columnValue.InputId, columnValue.PlaceholderText);
                    }
                    else {
                        string inputClass = "textEntry-noWidth";
                        string placeholderTextAttr = string.Empty;
                        if (columnValue.InputType == TableBuilderInsertType.Button) {
                            placeholderTextAttr = string.Format(" value=\"{0}\"", columnValue.PlaceholderText);
                            inputClass = "input-buttons";
                        }
                        else if (!string.IsNullOrEmpty(columnValue.PlaceholderText)) {
                            placeholderTextAttr = string.Format(" placeholder=\"{0}\"", columnValue.PlaceholderText);
                        }

                        inputStr = string.Format("<input type=\"{0}\" id=\"{1}\" class=\"{2}\" onkeypress=\"openWSE.GridViewMethods.OnInsertRow_KeyPress(event,'{3}','{4}');\"{5} />", columnValue.InputType.ToString().ToLower(), columnValue.InputId, inputClass, _tableId, addBtnClickEvent, placeholderTextAttr);
                    }
                }
                else if (!string.IsNullOrEmpty(columnValue.InnerHtml)) {
                    inputStr = columnValue.InnerHtml;
                }
                else {
                    additionalClasses += " addrow-emptycell";
                }

                string additionalAttributes = columnValue.AdditionalAttributes;
                if (!string.IsNullOrEmpty(additionalAttributes)) {
                    additionalAttributes = " " + additionalAttributes;
                }

                str.AppendFormat("<td class=\"{0}\"{1}{2}{3}>{4}</td>", additionalClasses, dataColumnAttr, alignmentAttr, additionalAttributes, inputStr);
                if (addColumnCount) {
                    _columnCount++;
                }
            }

            str.AppendFormat("<td align=\"center\"><a class=\"td-add-btn cursor-pointer\" onclick=\"openWSE.GridViewMethods.OnInsertRow('{0}','{1}');\" title=\"Add\"></a></td>", _tableId, addBtnClickEvent);
            if (addColumnCount) {
                _columnCount++;
            }

            str.Append("</tr>");
            _insertStr = str.ToString();
        }
    }

    #endregion

    #region Complete Table Build

    public string CompleteTableString(string emptyRowText, bool putInsertRowAtTop) {
        StringBuilder str = new StringBuilder();

        str.Append("<div class=\"gridview-table-holder\">");

        string altRowClass = string.Empty;
        if (_useAlternateRowClass) {
            altRowClass = "GridAlternate";
        }

        str.AppendFormat("<table data-tableid=\"{0}\" data-columnspan=\"{1}\" data-cookiename=\"{2}\" data-allowpaging=\"{3}\" data-initalsortcolumn=\"{4}\" data-initalsortdir=\"{5}\" data-altrowclass=\"{6}\" data-putinsertattop=\"{7}\" class=\"gridview-table\"><tbody>", _tableId, _columnCount, _cookieName, _allowPaging.ToString().ToLower(), _initialSortColumn, _initialSortDir, altRowClass, putInsertRowAtTop.ToString().ToLower());

        str.Append(_headerStr);
        str.Append(_bodyStr.ToString());
        str.Append(_insertStr);

        if (_rowCount == 0 && string.IsNullOrEmpty(_insertStr)) {
            if (string.IsNullOrEmpty(emptyRowText)) {
                emptyRowText = _emptyRowText;
            }

            str.AppendFormat("<tr><td class=\"emptyGridView-td\" colspan=\"{0}\"><div class=\"emptyGridView\">{1}</div></td></tr>", _columnCount.ToString(), emptyRowText);
        }

        #region Build Footer
        str.Append("<tr class=\"GridViewPager\">");

        StringBuilder searchHolder = new StringBuilder();
        searchHolder.Append("<div class=\"gridview-search-holder\">");
        searchHolder.AppendFormat("<span class=\"search-btn\" title=\"Search table\" onclick=\"openWSE.GridViewMethods.SearchTable('{0}');\"></span>", _tableId);
        searchHolder.AppendFormat("<span class=\"clear-search-btn\" title=\"Clear search\" onclick=\"openWSE.GridViewMethods.ClearSearch('{0}', true);\"></span>", _tableId);
        searchHolder.Append("<div class=\"clear\"></div>");
        searchHolder.Append("</div>");

        StringBuilder pagesizeHolder = new StringBuilder();
        if (_allowPaging) {
            pagesizeHolder.Append("<div class=\"table-pagesize-selector\">");
            pagesizeHolder.AppendFormat("<select onchange=\"openWSE.GridViewMethods.PageSizeChange(this);\">");
            #region Page size is 10
            if (_pageSize == "10") {
                pagesizeHolder.Append("<option value=\"10\" selected=\"selected\">10</option>");
            }
            else {
                pagesizeHolder.Append("<option value=\"10\">10</option>");
            }
            #endregion
            #region Page size is 20
            if (_pageSize == "20") {
                pagesizeHolder.Append("<option value=\"20\" selected=\"selected\">20</option>");
            }
            else {
                pagesizeHolder.Append("<option value=\"20\">20</option>");
            }
            #endregion
            #region Page size is 30
            if (_pageSize == "30") {
                pagesizeHolder.Append("<option value=\"30\" selected=\"selected\">30</option>");
            }
            else {
                pagesizeHolder.Append("<option value=\"30\">30</option>");
            }
            #endregion
            #region Page size is 40
            if (_pageSize == "40") {
                pagesizeHolder.Append("<option value=\"40\" selected=\"selected\">40</option>");
            }
            else {
                pagesizeHolder.Append("<option value=\"40\">40</option>");
            }
            #endregion
            #region Page size is 50
            if (_pageSize == "50") {
                pagesizeHolder.Append("<option value=\"50\" selected=\"selected\">50</option>");
            }
            else {
                pagesizeHolder.Append("<option value=\"50\">50</option>");
            }
            #endregion
            #region Page size is All
            if (_pageSize == "all") {
                pagesizeHolder.Append("<option value=\"all\" selected=\"selected\">All</option>");
            }
            else {
                pagesizeHolder.Append("<option value=\"all\">All</option>");
            }
            #endregion
            pagesizeHolder.Append("</select>");
            pagesizeHolder.Append("<div class=\"clear\"></div>");
            pagesizeHolder.Append("</div>");
        }

        str.AppendFormat("<td colspan=\"{0}\">{1}{2}<div class=\"table-pagesize-outof\"></div><div class=\"gridview-pager-holder\"></div><div class=\"clear\"></div></td>", _columnCount.ToString(), searchHolder.ToString(), pagesizeHolder.ToString());
        str.Append("</tr>");
        #endregion

        str.Append("</tbody></table></div>");

        RegisterPostbackScripts.RegisterStartupScript(_page, "openWSE.GridViewMethods.InitializeTable('" + _tableId + "');");

        return str.ToString();
    }
    public LiteralControl CompleteTableLiteralControl(string emptyRowText = "") {
        return new LiteralControl(CompleteTableString(emptyRowText, false));
    }
    public LiteralControl CompleteTableLiteralControl(string emptyRowText, bool putInsertRowAtTop) {
        return new LiteralControl(CompleteTableString(emptyRowText, putInsertRowAtTop));
    }

    #endregion

}

#region Collection Classes

public class TableBuilderHeaderColumns {

    private string _columnName = string.Empty;
    private string _columnWidth = string.Empty;
    private bool _setAsMinWidth = false;
    private bool _allowSorting = false;

    public TableBuilderHeaderColumns(string columnName, object columnWidth, bool setAsMinWidth, bool allowSorting = true) {
        _columnName = columnName;
        _columnWidth = columnWidth.ToString();
        _setAsMinWidth = setAsMinWidth;
        _allowSorting = allowSorting;
    }

    public string columnName {
        get {
            if (string.IsNullOrEmpty(_columnName)) {
                return "&nbsp;";
            }
            return _columnName;
        }
    }
    public string columnWidth {
        get {
            return _columnWidth;
        }
    }
    public bool setAsMinWidth {
        get {
            return _setAsMinWidth;
        }
    }
    public bool AllowSorting {
        get {
            return _allowSorting;
        }
    }

}
public class TableBuilderBodyColumnValues {

    private string _columnName = string.Empty;
    private string _columnValue = string.Empty;
    private TableBuilderColumnAlignment _columnAlignment;
    private string _additionalAttributes = string.Empty;
    private string _additionalClasses = string.Empty;

    public TableBuilderBodyColumnValues(string columnName, string columnValue, TableBuilderColumnAlignment columnAlignment, string additionalAttributes = "", string additionalClasses = "") {
        _columnName = columnName;
        _columnValue = columnValue;
        _columnAlignment = columnAlignment;
        _additionalAttributes = additionalAttributes;
        _additionalClasses = additionalClasses;
    }

    public string columnName {
        get {
            if (string.IsNullOrEmpty(_columnName)) {
                return "&nbsp;";
            }
            return _columnName;
        }
    }
    public string columnValue {
        get {
            return _columnValue;
        }
    }
    public TableBuilderColumnAlignment columnAlignment {
        get {
            return _columnAlignment;
        }
    }
    public string AdditionalAttributes {
        get {
            return _additionalAttributes;
        }
    }
    public string AdditionalClasses {
        get {
            return _additionalClasses;
        }
    }

}
public class TableBuilderInsertColumnValues {

    private string _columnName = string.Empty;
    private string _inputId = string.Empty;
    private TableBuilderColumnAlignment _columnAlignment;
    private TableBuilderInsertType _inputType;
    private string _innerHtml = string.Empty;
    private string _placeholderText = string.Empty;
    private string _additionalAttributes = string.Empty;
    private string _additionalClasses = string.Empty;

    public TableBuilderInsertColumnValues(string columnName, string innerHtml, TableBuilderColumnAlignment columnAlignment, string additionalAttributes = "", string additionalClasses = "") {
        _columnName = columnName;
        _columnAlignment = columnAlignment;
        _innerHtml = innerHtml;
        _additionalAttributes = additionalAttributes;
        _additionalClasses = additionalClasses;
    }
    public TableBuilderInsertColumnValues(string columnName, string inputId, TableBuilderColumnAlignment columnAlignment, TableBuilderInsertType inputType, string placeholderText = "", string additionalAttributes = "", string additionalClasses = "") {
        _columnName = columnName;
        _inputId = inputId;
        _columnAlignment = columnAlignment;
        _inputType = inputType;
        _placeholderText = placeholderText;
        _additionalAttributes = additionalAttributes;
        _additionalClasses = additionalClasses;
    }

    public string columnName {
        get {
            if (string.IsNullOrEmpty(_columnName)) {
                return "&nbsp;";
            }
            return _columnName;
        }
    }
    public string InputId {
        get {
            return _inputId;
        }
    }
    public TableBuilderColumnAlignment columnAlignment {
        get {
            return _columnAlignment;
        }
    }
    public TableBuilderInsertType InputType {
        get {
            return _inputType;
        }
    }
    public string InnerHtml {
        get {
            return _innerHtml;
        }
    }
    public string PlaceholderText {
        get {
            return _placeholderText;
        }
    }
    public string AdditionalAttributes {
        get {
            return _additionalAttributes;
        }
    }
    public string AdditionalClasses {
        get {
            return _additionalClasses;
        }
    }

}
public enum TableBuilderColumnAlignment {
    Left,
    Center,
    Right
}
public enum TableBuilderInsertType {
    None,
    Text,
    Checkbox,
    Number,
    Button,
    Image
}

#endregion
