using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;

/// <summary>
/// Summary description for AutomaticAppCreator
/// </summary>
public class CustomTableAppCreator {
    #region Private Variables
    private readonly App _app;
    private readonly StartupScripts _startupScripts = new StartupScripts(true);
    private string _username;
    #endregion

    public CustomTableAppCreator(string username) {
        _username = username;
        _app = new App(_username);
    }

    public string CreateApp(string appName, string categoryid, string description, string picname, bool insertIntoAppList, bool isPrivate, bool notifyUsers, string fileName = "") {
        StringBuilder appStr = new StringBuilder();
        if (string.IsNullOrEmpty(fileName))
            fileName = HelperMethods.RandomString(10);

        string appId = "app-" + fileName;

        #region Code Builder
        appStr.AppendLine("<%@ Control Language=\"C#\" AutoEventWireup=\"true\" ClassName=\"" + fileName + "\" ClientIDMode=\"Static\" Inherits=\"CustomTablesAppLoader\" %>");
        appStr.AppendLine("");

        // HTML Code
        appStr.AppendLine("<div class=\"custom-table-timermarker\">");
        #region custom-table-tableView-holder
        appStr.AppendLine("    <div id=\"pnl_" + fileName + "_tableView\" class=\"custom-table-tableView-holder\" data-viewmode=\"\">");
        appStr.AppendLine("         <div id=\"app_title_bgcolor_" + fileName + "\" runat=\"server\" class=\"pad-all app-title-bg-color\">");
        appStr.AppendLine("             <div class=\"float-left\">");
        appStr.AppendLine("                 <asp:Image ID=\"img_Title_" + fileName + "\" runat=\"server\" CssClass=\"app-img-titlebar\" />");
        appStr.AppendLine("                 <asp:Label ID=\"lbl_Title_" + fileName + "\" runat=\"server\" Text=\"\" CssClass=\"app-text-titlebar\"></asp:Label>");
        appStr.AppendLine("             </div>");
        appStr.AppendLine("             <div id=\"search_" + fileName + "_divider\" class=\"search-customtable-divider\" style=\"display: none;\"></div>");
        appStr.AppendLine("             <div id=\"search_" + fileName + "_holder\" class=\"float-right\">");
        appStr.AppendLine("                 <div class=\"searchwrapper\">");
        appStr.AppendLine("                     <div class=\"searchboxholder\">");
        appStr.AppendLine("                         <input id=\"tb_search_" + fileName + "\" type=\"text\" placeholder=\"Search table...\" class=\"searchbox\" onkeypress=\"customTables.KeyPressSearch(event, '" + fileName + "')\" />");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                     <a href=\"#\" class=\"searchbox_submit\" onclick=\"customTables.Search('" + fileName + "');return false;\"></a>");
        appStr.AppendLine("                     <a href=\"#\" onclick=\"$('#tb_search_" + fileName + "').val('');customTables.Refresh('" + fileName + "');return false;\" class=\"searchbox_clear\"></a>");
        appStr.AppendLine("                 </div>");
        appStr.AppendLine("             </div>");
        appStr.AppendLine("             <div class=\"clear\"></div>");
        appStr.AppendLine("             <div id=\"appdesc_" + fileName + "\" runat=\"server\" class=\"custom-table-appdescription\" visible=\"false\"></div>");
        appStr.AppendLine("         </div>");
        appStr.AppendLine("         <div class=\"img-appmenu app-menu-btn pad-all\" onclick=\"customTables.MenuClick('" + fileName + "');\" title=\"View Menu\"></div>");
        appStr.AppendLine("         <div class=\"td-delete-btn app-delete-month pad-all\" onclick=\"customTables.DeleteMonth('" + fileName + "');\" title=\"Delete Month\"></div>");
        appStr.AppendLine("         <div class=\"img-copy app-copy-data pad-all\" onclick=\"customTables.CopyData('" + fileName + "');\" title=\"Copy Data\"></div>");
        appStr.AppendLine("         <asp:HiddenField ID=\"hf_" + fileName + "_customizations\" runat=\"server\" Value=\"\" />");
        appStr.AppendLine("         <asp:HiddenField ID=\"hf_" + fileName + "_summaryData\" runat=\"server\" Value=\"\" />");
        appStr.AppendLine("         <asp:HiddenField ID=\"hf_" + fileName + "_monthSelector\" runat=\"server\" Value=\"\" />");
        appStr.AppendLine("         <asp:HiddenField ID=\"hf_" + fileName + "_tableview\" runat=\"server\" Value=\"default\" />");
        appStr.AppendLine("         <div id=\"data-holder-" + fileName + "\" class=\"data-holder-customtable-holder\"></div>");
        appStr.AppendLine("     </div>");
        #endregion

        #region chart view
        appStr.AppendLine("     <div id=\"pnl_" + fileName + "_chartView\" style=\"display: none;\"></div>");
        #endregion

        #region customtable-sidebar-menu
        appStr.AppendLine("     <div id=\"" + fileName + "-sidebar-menu\" class=\"customtable-sidebar-menu\">");
        appStr.AppendLine("         <div class=\"pad-all customtable-sidebar-innercontent\">");
        appStr.AppendLine("             <div class=\"img-close-dark app-menu-btn\" onclick=\"customTables.MenuClick('" + fileName + "');\" title=\"Close Menu\"></div>");
        appStr.AppendLine("             <a href=\"#\" class=\"float-right img-refresh\" onclick=\"customTables.Refresh('" + fileName + "');return false;\" title=\"Refresh Table\"></a>");
        appStr.AppendLine("             <div class=\"clear-space\"></div>");
        appStr.AppendLine("             <ul>");
        appStr.AppendLine("                 <li id=\"" + fileName + "-sidebar-menu-viewallrecords\">");
        appStr.AppendLine("                     <div class=\"section-pad\" style=\"padding-top: 5px !important;\">");
        appStr.AppendLine("                         <a id=\"btn_" + fileName + "_addRecord\" runat=\"server\" class=\"sidebar-menu-buttons\" onclick=\"customTables.AddRecord_MenuClick('" + fileName + "');return false;\"><span class=\"td-add-btn sidebar-menu-img\"></span>Create New Record</a>");
        appStr.AppendLine("                         <div class=\"clear\"></div>");
        appStr.AppendLine("                         <a id=\"btn_" + fileName + "_viewRecords\" class=\"sidebar-menu-buttons\" onclick=\"customTables.ViewRecords_MenuClick('" + fileName + "');return false;\"><span class=\"td-details-btn sidebar-menu-img\"></span>View Record List</a>");
        appStr.AppendLine("                         <div class=\"clear\"></div>");
        appStr.AppendLine("                         <a id=\"btn_" + fileName + "_viewSummary\" class=\"sidebar-menu-buttons\" onclick=\"customTables.ViewSummary_MenuClick('" + fileName + "');return false;\"><span class=\"td-view-btn sidebar-menu-img\"></span>View Summary</a>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li id=\"li_" + fileName + "_chartType\" runat=\"server\">");
        appStr.AppendLine("                     <asp:Panel ID=\"pnl_" + fileName + "_chartType\" CssClass=\"custom-table-view-chart section-pad\" runat=\"server\">");
        appStr.AppendLine("                         <a href=\"#\" onclick=\"customTables.ViewChart('" + fileName + "', this);return false;\">");
        appStr.AppendLine("                             <asp:HiddenField ID=\"hf_" + fileName + "_chartType\" runat=\"server\" />");
        appStr.AppendLine("                             <asp:HiddenField ID=\"hf_" + fileName + "_chartTitle\" runat=\"server\" />");
        appStr.AppendLine("                             <asp:HiddenField ID=\"hf_" + fileName + "_chartColumns\" runat=\"server\" />");
        appStr.AppendLine("                             <asp:Image ID=\"img_" + fileName + "_chartType\" runat=\"server\" />");
        appStr.AppendLine("                             <span>View Data in Chart</span>");
        appStr.AppendLine("                         </a>");
        appStr.AppendLine("                         <div class=\"clear\"></div>");
        appStr.AppendLine("                     </asp:Panel>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li id=\"li_" + fileName + "_viewMode\" runat=\"server\">");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3 class=\"section-pad-title\">View Mode</h3>");
        appStr.AppendLine("                         <div class=\"desktop-img sidebar-customtables-img\" title=\"Desktop\" onclick=\"customTables.ChangeViewMode('" + fileName + "', 'desktop')\"></div>");
        appStr.AppendLine("                         <div class=\"mobile-img sidebar-customtables-img\" title=\"Mobile\" onclick=\"customTables.ChangeViewMode('" + fileName + "', 'mobile')\"></div>");
        appStr.AppendLine("                         <div class=\"clear\"></div>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li>");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3 class=\"section-pad-title\">Font Size</h3>");
        appStr.AppendLine("                         <div class=\"font-larger-img sidebar-customtables-img\" title=\"Increase Font Size\" onclick=\"customTables.ChangeFont('" + fileName + "', this)\"></div>");
        appStr.AppendLine("                         <div class=\"font-smaller-img sidebar-customtables-img\" title=\"Descrease Font Size\" onclick=\"customTables.ChangeFont('" + fileName + "', this)\"></div>");
        appStr.AppendLine("                         <div class=\"clear\"></div>");
        appStr.AppendLine("                         <select id=\"font-size-selector-" + fileName + "\" style=\"display: none;\">");
        appStr.AppendLine("                             <option value=\"x-small\">X-Small</option>");
        appStr.AppendLine("                             <option value=\"small\" selected=\"selected\">Small</option>");
        appStr.AppendLine("                             <option value=\"medium\">Medium</option>");
        appStr.AppendLine("                             <option value=\"large\">Large</option>");
        appStr.AppendLine("                             <option value=\"x-large\">X-Large</option>");
        appStr.AppendLine("                         </select>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li>");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3 class=\"section-pad-title\" style=\"padding-top: 3px !important;\">Records to Pull</h3>");
        appStr.AppendLine("                         <select id=\"RecordstoSelect_" + fileName + "\" class=\"float-right\" onchange=\"customTables.RecordstoSelect('" + fileName + "')\">");
        appStr.AppendLine("                             <option value=\"5\">5</option>");
        appStr.AppendLine("                             <option value=\"10\">10</option>");
        appStr.AppendLine("                             <option value=\"25\">25</option>");
        appStr.AppendLine("                             <option value=\"50\">50</option>");
        appStr.AppendLine("                             <option value=\"75\">75</option>");
        appStr.AppendLine("                             <option value=\"100\">100</option>");
        appStr.AppendLine("                             <option value=\"200\">200</option>");
        appStr.AppendLine("                             <option value=\"all\" selected=\"selected\">All</option>");
        appStr.AppendLine("                         </select>");
        appStr.AppendLine("                         <div class=\"clear\"></div>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li>");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3>Export to Excel</h3>");
        appStr.AppendLine("                         <div class=\"clear-space-five\"></div>");
        appStr.AppendLine("                         <div class=\"custom-table-excelexport-btns\">");
        appStr.AppendLine("                             <input id=\"btnExport-" + fileName + "\" type=\"button\" class=\"input-buttons\" onclick=\"customTables.OpenExcelExportModal('" + fileName + "')\" title=\"Select a date to export to an Excel file\" value=\"Export Dates\" />");
        appStr.AppendLine("                             <input id=\"btnExportAll-" + fileName + "\" type=\"button\" class=\"input-buttons no-margin\" onclick=\"customTables.ExportToExcelAll('" + fileName + "')\" title=\"Export all data to Excel file\" value=\"Export All\" />");
        appStr.AppendLine("                             <div class=\"clear\"></div>");
        appStr.AppendLine("                         </div>");
        appStr.AppendLine("                         <div id=\"" + fileName + "-ExcelExport-element\" class=\"Modal-element\">");
        appStr.AppendLine("                             <div class=\"Modal-element-align\">");
        appStr.AppendLine("                                 <div class=\"Modal-element-modal ui-draggable ui-draggable-handle\" data-setwidth=\"350\">");
        appStr.AppendLine("                                     <div class=\"ModalHeader\">");
        appStr.AppendLine("                                         <div>");
        appStr.AppendLine("                                             <div class=\"app-head-button-holder-admin\"><a href=\"#\" onclick=\"customTables.CloseExcelExportModal('" + fileName + "');return false;\" class=\"ModalExitButton\"></a></div>");
        appStr.AppendLine("                                             <span class=\"Modal-title\"></span>");
        appStr.AppendLine("                                         </div>");
        appStr.AppendLine("                                     </div>");
        appStr.AppendLine("                                     <div class=\"ModalPadContent\">");
        appStr.AppendLine("                                         Select the dates you wish to export. The dates are based off of the Month Selector column.");
        appStr.AppendLine("                                         <div class=\"clear-space\"></div>");
        appStr.AppendLine("                                         <div align=\"center\">");
        appStr.AppendLine("                                             <input id=\"tb_exportDateFrom_" + fileName + "\" type=\"text\" class=\"textEntry\" style=\"width: 95px;\" />");
        appStr.AppendLine("                                             <span class=\"font-bold pad-left-sml pad-right-sml\">To</span>");
        appStr.AppendLine("                                             <input id=\"tb_exportDateTo_" + fileName + "\" type=\"text\" class=\"textEntry\" style=\"width: 95px;\" />");
        appStr.AppendLine("                                             <div class=\"clear\"></div>");
        appStr.AppendLine("                                         </div>");
        appStr.AppendLine("                                         <div class=\"clear-space\"></div>");
        appStr.AppendLine("                                         <div class=\"clear-space\"></div>");
        appStr.AppendLine("                                         <input class=\"input-buttons float-left\" type=\"button\" value=\"Export\" onclick=\"customTables.ExportToExcelDates('" + fileName + "');\" />");
        appStr.AppendLine("                                         <input class=\"input-buttons float-right no-margin\" type=\"button\" value=\"Cancel\" onclick=\"customTables.CloseExcelExportModal('" + fileName + "');\" />");
        appStr.AppendLine("                                         <div class=\"clear\"></div>");
        appStr.AppendLine("                                     </div>");
        appStr.AppendLine("                                 </div>");
        appStr.AppendLine("                             </div>");
        appStr.AppendLine("                         </div>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li id=\"li_cblist_" + fileName + "\">");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3>Columns to Show</h3>");
        appStr.AppendLine("                         <div class=\"clear-space-five\"></div>");
        appStr.AppendLine("                         <div id=\"cblist_" + fileName + "_holder\"></div>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li id=\"td_" + fileName + "_sidebar\" style=\"border-bottom: none !important;\">");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <div id=\"sidebar-items-" + fileName + "\">");
        appStr.AppendLine("                             <h3>Month Selector</h3>");
        appStr.AppendLine("                             <div id=\"month-selector-text-" + fileName + "\" class=\"pad-top-sml pad-bottom\"><small>These dates are taken from the timestamp of each item created.</small></div>");
        appStr.AppendLine("                             <div id=\"month-selector-" + fileName + "\"></div>");
        appStr.AppendLine("                         </div>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("             </ul>");
        appStr.AppendLine("         </div>");
        appStr.AppendLine("     </div>");
        #endregion
        appStr.AppendLine("</div>");
        appStr.AppendLine("<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Apps/Custom_Tables/customtables.js' />");
        appStr.AppendLine("<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Scripts/jquery/jquery.fileDownload.js' />");

        #endregion

        string filePath = ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\" + fileName + ".ascx";
        if (!Directory.Exists(ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\"))
            Directory.CreateDirectory(ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\");

        using (var file = new StreamWriter(filePath)) {
            file.Write(appStr.ToString());
        }

        if (insertIntoAppList) {
            string siteName = ServerSettings.SiteName;
            if (!string.IsNullOrEmpty(siteName)) {
                siteName += " | ";
            }

            _app.CreateItem(appId, appName, "Custom_Tables/" + fileName + ".ascx", picname, "1", "1", siteName + ServerSettings.ServerDateTime.Year, description, "app-main", categoryid, "450", "500", false, true, "~/ExternalAppHolder.aspx?appId=" + appId, "", false, "1", isPrivate, true, string.Empty, "#FFFFFF", notifyUsers);
        }

        if (GroupSessions.DoesUserHaveGroupLoginSessionKey(_username)) {
            string groupId = GroupSessions.GetUserGroupSessionName(_username);
            NewUserDefaults defaults = new NewUserDefaults(groupId);
            string packageId = defaults.GetDefault("AppPackage");
            AppPackages packages = new AppPackages(false);
            List<string> appList = packages.GetAppList(packageId).ToList();
            appList.Add(appId);
            string appListStr = string.Join(",", appList.ToArray());
            if (!string.IsNullOrEmpty(appListStr)) {
                appListStr += ",";
            }
            packages.updateAppList(packageId, appListStr, _username);
        }

        return fileName;
    }

}