using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;

/// <summary>
/// Summary description for AutomaticAppCreator
/// </summary>
public class dbImportAppCreator
{
    #region Private Variables
    private readonly App _app;
    private readonly StartupScripts _startupScripts = new StartupScripts(true);
    private string _username;
    private string _allowEdit = "true";
    #endregion

    public dbImportAppCreator(string username, bool allowEdit = true) 
    {
        _username = username;
        _app = new App(_username);
        _allowEdit = allowEdit.ToString().ToLower();
    }

    public string CreateApp(string appName, string categoryid, string description, string picname, bool isPrivate, bool notifyUsers, bool insertIntoAppList = true, string fileName = "")
    {
        if (string.IsNullOrEmpty(fileName))
            fileName = HelperMethods.RandomString(10);

        string appId = "app-" + fileName;

        if (string.IsNullOrEmpty(picname)) {
            picname = DBImporter.DefaultDatabaseIcon;
        }

        StringBuilder appStr = new StringBuilder();


        #region Code Builder
        appStr.AppendLine("<%@ Control Language=\"C#\" AutoEventWireup=\"true\" ClassName=\"" + fileName + "\" ClientIDMode=\"Static\" Inherits=\"DBImporterAppLoader\" %>");
        appStr.AppendLine("");

        // HTML Code
        appStr.AppendLine("<div class=\"dbImport-table-timermarker\">");
        #region dbImport-table-tableView-holder
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
        appStr.AppendLine("                         <input id=\"tb_search_" + fileName + "\" type=\"text\" class=\"searchbox\" placeholder=\"Search table...\" onkeypress=\"dbImport.KeyPressSearch(event, '" + fileName + "')\" />");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                     <a href=\"#\" class=\"searchbox_submit\" onclick=\"dbImport.Search('" + fileName + "');return false;\"></a>");
        appStr.AppendLine("                     <a href=\"#\" onclick=\"$('#tb_search_" + fileName + "').val('');dbImport.Refresh('" + fileName + "');return false;\" class=\"searchbox_clear\"></a>");
        appStr.AppendLine("                 </div>");
        appStr.AppendLine("             </div>");
        appStr.AppendLine("             <div class=\"clear\"></div>");
        appStr.AppendLine("             <div id=\"appdesc_" + fileName + "\" runat=\"server\" class=\"custom-table-appdescription\" visible=\"false\"></div>");
        appStr.AppendLine("         </div>");
        appStr.AppendLine("         <div class=\"img-appmenu app-menu-btn pad-all\" onclick=\"dbImport.MenuClick('" + fileName + "');\" title=\"View Menu\"></div>");
        appStr.AppendLine("         <div class=\"clear\"></div>");
        appStr.AppendLine("         <asp:HiddenField ID=\"hf_" + fileName + "_customizations\" runat=\"server\" Value=\"\" />");
        appStr.AppendLine("         <asp:HiddenField ID=\"hf_" + fileName + "_summaryData\" runat=\"server\" Value=\"\" />");
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
        appStr.AppendLine("             <div class=\"img-close-dark app-menu-btn\" onclick=\"dbImport.MenuClick('" + fileName + "');\" title=\"Close Menu\"></div>");
        appStr.AppendLine("             <a href=\"#\" class=\"float-right img-refresh\" onclick=\"dbImport.Refresh('" + fileName + "');return false;\" title=\"Refresh Table\"></a>");
        appStr.AppendLine("             <div class=\"clear-space\"></div>");
        appStr.AppendLine("             <ul>");
        appStr.AppendLine("                 <li id=\"" + fileName + "-sidebar-menu-viewallrecords\">");
        appStr.AppendLine("                     <div class=\"section-pad\" style=\"padding-top: 5px !important;\">");
        appStr.AppendLine("                         <a id=\"btn_" + fileName + "_addRecord\" runat=\"server\" class=\"sidebar-menu-buttons\" onclick=\"dbImport.AddRecord_MenuClick('" + fileName + "');return false;\"><span class=\"td-add-btn sidebar-menu-img\"></span>Create New Record</a>");
        appStr.AppendLine("                         <div class=\"clear\"></div>");
        appStr.AppendLine("                         <a id=\"btn_" + fileName + "_viewRecords\" class=\"sidebar-menu-buttons\" onclick=\"dbImport.ViewRecords_MenuClick('" + fileName + "');return false;\"><span class=\"td-details-btn sidebar-menu-img\"></span>View Record List</a>");
        appStr.AppendLine("                         <div class=\"clear\"></div>");
        appStr.AppendLine("                         <a id=\"btn_" + fileName + "_viewSummary\" class=\"sidebar-menu-buttons\" onclick=\"dbImport.ViewSummary_MenuClick('" + fileName + "');return false;\"><span class=\"td-view-btn sidebar-menu-img\"></span>View Summary</a>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li id=\"li_" + fileName + "_chartType\" runat=\"server\">");
        appStr.AppendLine("                     <asp:Panel ID=\"pnl_" + fileName + "_chartType\" CssClass=\"custom-table-view-chart section-pad\" runat=\"server\">");
        appStr.AppendLine("                         <a href=\"#\" onclick=\"dbImport.ViewChart('" + fileName + "', this);return false;\">");
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
        appStr.AppendLine("                         <div class=\"desktop-img sidebar-customtables-img\" title=\"Desktop\" onclick=\"dbImport.ChangeViewMode('" + fileName + "', 'desktop')\"></div>");
        appStr.AppendLine("                         <div class=\"mobile-img sidebar-customtables-img\" title=\"Mobile\" onclick=\"dbImport.ChangeViewMode('" + fileName + "', 'mobile')\"></div>");
        appStr.AppendLine("                         <div class=\"clear\"></div>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li>");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3 class=\"section-pad-title\">Font Size</h3>");
        appStr.AppendLine("                         <div class=\"font-larger-img sidebar-customtables-img\" title=\"Increase Font Size\" onclick=\"dbImport.ChangeFont('" + fileName + "', this)\"></div>");
        appStr.AppendLine("                         <div class=\"font-smaller-img sidebar-customtables-img\" title=\"Descrease Font Size\" onclick=\"dbImport.ChangeFont('" + fileName + "', this)\"></div>");
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
        appStr.AppendLine("                         <select id=\"RecordstoSelect_" + fileName + "\" class=\"float-right\" onchange=\"dbImport.RecordstoSelect('" + fileName + "')\">");
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
        appStr.AppendLine("                         <div class=\"custom-table-excelexport-btns\">");
        appStr.AppendLine("                             <input id=\"btnExportAll-" + fileName + "\" type=\"button\" class=\"input-buttons no-margin\" onclick=\"dbImport.ExportToExcelAll('" + fileName + "')\" title=\"Export all data to Excel file\" value=\"Export to Excel\" />");
        appStr.AppendLine("                             <div class=\"clear\"></div>");
        appStr.AppendLine("                         </div>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li id=\"li_cblist_" + fileName + "\" style=\"border-bottom: none !important;\">");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3>Columns to Show</h3>");
        appStr.AppendLine("                         <div class=\"clear-space-five\"></div>");
        appStr.AppendLine("                         <div id=\"cblist_" + fileName + "_holder\"></div>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("             </ul>");
        appStr.AppendLine("         </div>");
        appStr.AppendLine("     </div>");
        #endregion
        appStr.AppendLine("</div>");
        appStr.AppendLine("<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Apps/Database_Imports/dbimports.js' />");
        appStr.AppendLine("<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Scripts/jquery/jquery.fileDownload.js' />");

        #endregion


        string filePath = ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\" + fileName + ".ascx";
        if (!Directory.Exists(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\"))
            Directory.CreateDirectory(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\");

        using (var file = new StreamWriter(filePath))
        {
            file.Write(appStr.ToString());
        }

        if ((insertIntoAppList) && (!string.IsNullOrEmpty(appName))) {
            string siteName = ServerSettings.SiteName;
            if (!string.IsNullOrEmpty(siteName)) {
                siteName += " | ";
            }

            _app.CreateItem(appId, appName, "Database_Imports/" + fileName + ".ascx", picname, "1", "1", siteName + ServerSettings.ServerDateTime.Year, description, "app-main", categoryid, "450", "500", false, true, "~/ExternalAppHolder.aspx?appId=" + appId, "", false, "1", isPrivate, true, string.Empty, "#FFFFFF", notifyUsers);
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