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

    public string CreateApp(string appName, string categoryid, string description, string picname, bool isPrivate, bool insertIntoAppList = true, string fileName = "")
    {
        if (string.IsNullOrEmpty(fileName))
            fileName = HelperMethods.RandomString(10);

        string appId = "app-" + fileName;
        string applyTo = "Table Imports";

        if (string.IsNullOrEmpty(picname)) {
            picname = "database.png";
        }

        StringBuilder appStr = new StringBuilder();


        #region Code Builder
        appStr.AppendLine("<%@ Control Language=\"C#\" ClassName=\"" + fileName + "\" ClientIDMode=\"Static\" %>");
        appStr.AppendLine("");

        // C# Code
        appStr.AppendLine("<script runat=\"server\">");
        appStr.AppendLine("     private ServerSettings _ss = new ServerSettings();");
        appStr.AppendLine("     private App _apps;");
        appStr.AppendLine("     private OpenWSE_Tools.Apps.AppInitializer _appInitializer;");
        appStr.AppendLine("     private const string app_id = \"" + appId + "\";");
        appStr.AppendLine("");
        appStr.AppendLine("     protected void Page_Load(object sender, EventArgs e) {");
        appStr.AppendLine("         System.Security.Principal.IIdentity userId = Page.User.Identity;");
        appStr.AppendLine("         _apps = new App(userId.Name);");
        appStr.AppendLine("         DBImporter_Coll coll = dbImportservice.GetDataBase_Coll(\"" + fileName + "\");");
        appStr.AppendLine("         DBImporter dbImporter = new DBImporter();");
        appStr.AppendLine("         string cl = _apps.GetAppName(app_id);");
        appStr.AppendLine("         lbl_Title_" + fileName + ".Text = cl;");
        appStr.AppendLine("");
        appStr.AppendLine("         if (!_ss.HideAllAppIcons) {");
        appStr.AppendLine("             img_Title_" + fileName + ".Visible = true;");
        appStr.AppendLine("             string clImg = _apps.GetAppIconName(app_id);");
        appStr.AppendLine("             img_Title_" + fileName + ".ImageUrl = \"~/Standard_Images/App_Icons/\" + clImg;");
        appStr.AppendLine("         }");
        appStr.AppendLine("         else {");
        appStr.AppendLine("             img_Title_" + fileName + ".Visible = false;");
        appStr.AppendLine("         }");
        appStr.AppendLine("");
        appStr.AppendLine("         if (coll.AllowEdit) {");
        appStr.AppendLine("             btn_" + fileName + "_addRecord.Visible = coll.UsersAllowedToEdit.Contains(userId.Name.ToLower());");
        appStr.AppendLine("         }");
        appStr.AppendLine("         else {");
        appStr.AppendLine("             btn_" + fileName + "_addRecord.Visible = false;");
        appStr.AppendLine("         }");
        appStr.AppendLine("");
        appStr.AppendLine("         hf_" + fileName + "_customizations.Value = string.Empty;");
        appStr.AppendLine("         System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();");
        appStr.AppendLine("         try {");
        appStr.AppendLine("             string tableCustomizations = HttpUtility.UrlEncode(serializer.Serialize(coll.TableCustomizations));");
        appStr.AppendLine("             hf_" + fileName + "_customizations.Value = tableCustomizations;");
        appStr.AppendLine("         } catch { }");
        appStr.AppendLine("");
        appStr.AppendLine("         string headerColor = string.Empty;");
        appStr.AppendLine("         string alternativeColor = string.Empty;");
        appStr.AppendLine("         foreach (CustomTableCustomizations customizations in coll.TableCustomizations) {");
        appStr.AppendLine("             switch (customizations.customizeName) {");
        appStr.AppendLine("                 case \"HeaderColor\":");
        appStr.AppendLine("                     headerColor = customizations.customizeValue;");
        appStr.AppendLine("                     break;");
        appStr.AppendLine("                 case \"AlternativeRowColor\":");
        appStr.AppendLine("                 case \"PrimaryRowColor\":");
        appStr.AppendLine("                     if (string.IsNullOrEmpty(alternativeColor)) {");
        appStr.AppendLine("                         alternativeColor = customizations.customizeValue;");
        appStr.AppendLine("                     }");
        appStr.AppendLine("                     break;");
        appStr.AppendLine("             }");
        appStr.AppendLine("         }");
        appStr.AppendLine("");
        appStr.AppendLine("         if (!string.IsNullOrEmpty(headerColor) && !string.IsNullOrEmpty(alternativeColor)) {");
        appStr.AppendLine("             string cssSytles = HelperMethods.GetCSSGradientStyles(alternativeColor, headerColor);");
        appStr.AppendLine("             app_title_bgcolor_" + fileName + ".Attributes[\"style\"] = cssSytles;");
        appStr.AppendLine("             if (!HelperMethods.UseDarkTextColorWithBackground(alternativeColor)) {");
        appStr.AppendLine("                 lbl_Title_" + fileName + ".Style[\"color\"] = \"#FFFFFF\";");
        appStr.AppendLine("             }");
        appStr.AppendLine("             else {");
        appStr.AppendLine("                 lbl_Title_" + fileName + ".Style[\"color\"] = \"#353535\";");
        appStr.AppendLine("             }");
        appStr.AppendLine("         }");
        appStr.AppendLine("");
        appStr.AppendLine("         ChartType chart_Type = dbImporter.GetChartTypeFromCustomizations(coll.TableCustomizations);");
        appStr.AppendLine("         string chartColumns = dbImporter.GetChartColumnsFromCustomizations(coll.TableCustomizations);");
        appStr.AppendLine("         if (chart_Type != ChartType.None && chartColumns.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).Length > 0) {");
        appStr.AppendLine("             string chartTitle = dbImporter.GetChartTitleFromCustomizations(coll.TableCustomizations);");
        appStr.AppendLine("             pnl_" + fileName + "_chartType.Enabled = true;");
        appStr.AppendLine("             pnl_" + fileName + "_chartType.Visible = true;");
        appStr.AppendLine("             li_" + fileName + "_chartType.Visible = true;");
        appStr.AppendLine("             img_" + fileName + "_chartType.ImageUrl = \"~/Standard_Images/ChartTypes/\" + chart_Type.ToString().ToLower() + \".png\";");
        appStr.AppendLine("             hf_" + fileName + "_chartType.Value = chart_Type.ToString();");
        appStr.AppendLine("             if (string.IsNullOrEmpty(chartTitle)) {");
        appStr.AppendLine("                 chartTitle = coll.TableName;");
        appStr.AppendLine("             }");
        appStr.AppendLine("             hf_" + fileName + "_chartTitle.Value = chartTitle;");
        appStr.AppendLine("             hf_" + fileName + "_chartColumns.Value = chartColumns;");
        appStr.AppendLine("         }");
        appStr.AppendLine("         else {");
        appStr.AppendLine("             pnl_" + fileName + "_chartType.Enabled = false;");
        appStr.AppendLine("             pnl_" + fileName + "_chartType.Visible = false;");
        appStr.AppendLine("             li_" + fileName + "_chartType.Visible = false;");
        appStr.AppendLine("         }");
        appStr.AppendLine("");
        appStr.AppendLine("         _appInitializer = new OpenWSE_Tools.Apps.AppInitializer(app_id, Page.User.Identity.Name, Page, \"" + applyTo + "\");");
        appStr.AppendLine("         _appInitializer.LoadScripts_JS(true, \"dbImport.Load('" + fileName + "');\");");
        appStr.AppendLine("     }");
        appStr.AppendLine("</script>");

        // HTML Code
        appStr.AppendLine("<div class=\"dbImport-table-timermarker\">");
        #region dbImport-table-tableView-holder
        appStr.AppendLine("    <div id=\"pnl_" + fileName + "_tableView\" class=\"custom-table-tableView-holder\" data-viewmode=\"\">");
        appStr.AppendLine("         <div id=\"app_title_bgcolor_" + fileName + "\" runat=\"server\" class=\"pad-all app-title-bg-color\">");
        appStr.AppendLine("             <div class=\"float-left\">");
        appStr.AppendLine("                 <asp:Image ID=\"img_Title_" + fileName + "\" runat=\"server\" CssClass=\"app-img-titlebar\" />");
        appStr.AppendLine("                 <asp:Label ID=\"lbl_Title_" + fileName + "\" runat=\"server\" Text=\"\" CssClass=\"app-text-titlebar\"></asp:Label>");
        appStr.AppendLine("             </div>");
        appStr.AppendLine("             <div id=\"search_" + fileName + "_divider\" class=\"clear-space\" style=\"display: none;\"></div>");
        appStr.AppendLine("             <div id=\"search_" + fileName + "_holder\" class=\"float-right pad-top-sml\">");
        appStr.AppendLine("                 <div class=\"searchwrapper\" style=\"width: 375px;\">");
        appStr.AppendLine("                     <input id=\"tb_search_" + fileName + "\" type=\"text\" class=\"searchbox\" onfocus=\"if(this.value=='Search this table')this.value=''\" onblur=\"if(this.value=='')this.value='Search this table'\" onkeypress=\"dbImport.KeyPressSearch(event, '" + fileName + "')\" value=\"Search this table\" />");
        appStr.AppendLine("                     <a href=\"#\" onclick=\"$('#tb_search_" + fileName + "').val('Search this table');dbImport.Refresh('" + fileName + "');return false;\" class=\"searchbox_clear\"></a>");
        appStr.AppendLine("                     <a href=\"#\" class=\"searchbox_submit\" onclick=\"dbImport.Search('" + fileName + "');return false;\"></a>");
        appStr.AppendLine("                 </div>");
        appStr.AppendLine("             </div>");
        appStr.AppendLine("             <div class=\"clear\"></div>");
        appStr.AppendLine("         </div>");
        appStr.AppendLine("         <div class=\"img-appmenu app-menu-btn pad-all\" onclick=\"dbImport.MenuClick('" + fileName + "');\" title=\"View Menu\"></div>");
        appStr.AppendLine("         <div class=\"clear\"></div>");
        appStr.AppendLine("         <asp:HiddenField ID=\"hf_" + fileName + "_customizations\" runat=\"server\" Value=\"\" />");
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
        appStr.AppendLine("                     <div class=\"section-pad\" align=\"center\">");
        appStr.AppendLine("                         <input id=\"btn_" + fileName + "_viewRecords\" type=\"button\" class=\"input-buttons-create no-margin\" value=\"View Records\" onclick=\"dbImport.ViewRecords_MenuClick('" + fileName + "');\" />");
        appStr.AppendLine("                         <div class=\"clear-space\"></div>");
        appStr.AppendLine("                         <input id=\"btn_" + fileName + "_addRecord\" runat=\"server\" type=\"button\" class=\"input-buttons-create no-margin\" value=\"Add Record\" onclick=\"dbImport.AddRecord_MenuClick('" + fileName + "');\" />");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li>");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3>View Mode</h3>");
        appStr.AppendLine("                         <div class=\"clear-space-five\"></div>");
        appStr.AppendLine("                         <select id=\"viewmode-selector-" + fileName + "\" onchange=\"dbImport.ChangeViewMode('" + fileName + "')\">");
        appStr.AppendLine("                             <option value=\"desktop\">Desktop</option>");
        appStr.AppendLine("                             <option value=\"mobile\">Mobile</option>");
        appStr.AppendLine("                         </select>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li id=\"li_" + fileName + "_chartType\" runat=\"server\">");
        appStr.AppendLine("                     <asp:Panel ID=\"pnl_" + fileName + "_chartType\" CssClass=\"custom-table-view-chart section-pad\" runat=\"server\">");
        appStr.AppendLine("                         <div class=\"clear-space\"></div>");
        appStr.AppendLine("                         <a href=\"#\" onclick=\"dbImport.ViewChart('" + fileName + "', this);return false;\">");
        appStr.AppendLine("                             <asp:HiddenField ID=\"hf_" + fileName + "_chartType\" runat=\"server\" />");
        appStr.AppendLine("                             <asp:HiddenField ID=\"hf_" + fileName + "_chartTitle\" runat=\"server\" />");
        appStr.AppendLine("                             <asp:HiddenField ID=\"hf_" + fileName + "_chartColumns\" runat=\"server\" />");
        appStr.AppendLine("                             <asp:Image ID=\"img_" + fileName + "_chartType\" runat=\"server\" />");
        appStr.AppendLine("                             <span>View Data in Chart</span>");
        appStr.AppendLine("                         </a>");
        appStr.AppendLine("                     </asp:Panel>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li>");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3>Font Size</h3>");
        appStr.AppendLine("                         <div class=\"clear-space-five\"></div>");
        appStr.AppendLine("                         <select id=\"font-size-selector-" + fileName + "\" onchange=\"dbImport.ChangeFont('" + fileName + "')\">");
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
        appStr.AppendLine("                         <h3>Records to Pull</h3>");
        appStr.AppendLine("                         <div class=\"clear-space-five\"></div>");
        appStr.AppendLine("                         <select id=\"RecordstoSelect_" + fileName + "\" onchange=\"dbImport.RecordstoSelect('" + fileName + "')\">");
        appStr.AppendLine("                             <option value=\"5\">5</option>");
        appStr.AppendLine("                             <option value=\"10\">10</option>");
        appStr.AppendLine("                             <option value=\"25\" selected=\"selected\">25</option>");
        appStr.AppendLine("                             <option value=\"50\">50</option>");
        appStr.AppendLine("                             <option value=\"75\">75</option>");
        appStr.AppendLine("                             <option value=\"100\">100</option>");
        appStr.AppendLine("                             <option value=\"200\">200</option>");
        appStr.AppendLine("                             <option value=\"all\">All</option>");
        appStr.AppendLine("                         </select>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li>");
        appStr.AppendLine("                     <div class=\"section-pad\">");
        appStr.AppendLine("                         <h3>Export to Spreadsheet</h3>");
        appStr.AppendLine("                         <div class=\"clear-space-five\"></div>");
        appStr.AppendLine("                         <input id=\"tb_exportDateFrom_" + fileName + "\" type=\"text\" class=\"textEntry\" style=\"width: 85px;\" />");
        appStr.AppendLine("                         <span class=\"font-bold pad-left-sml pad-right-sml\">To</span>");
        appStr.AppendLine("                         <input id=\"tb_exportDateTo_" + fileName + "\" type=\"text\" class=\"textEntry\" style=\"width: 85px;\" />");
        appStr.AppendLine("                         <div class=\"clear-space\"></div>");
        appStr.AppendLine("                         <input id=\"btnExport-" + fileName + "\" type=\"button\" class=\"input-buttons\" onclick=\"dbImport.ExportToExcelAll('" + fileName + "')\" title=\"Select a date to export to an Excel file\" value=\"Export\" />");
        appStr.AppendLine("                         <span id=\"exportingNow_" + fileName + "\" class=\"margin-top\" style=\"display: none;\">Exporting...</span>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </li>");
        appStr.AppendLine("                 <li id=\"li_cblist_" + fileName + "\">");
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

        #endregion


        string filePath = ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\" + fileName + ".ascx";
        if (!Directory.Exists(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\"))
            Directory.CreateDirectory(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\");

        using (var file = new StreamWriter(filePath))
        {
            file.Write(appStr.ToString());
        }

        if ((insertIntoAppList) && (!string.IsNullOrEmpty(appName)))
            _app.CreateItem(appId, appName, "Database_Imports/" + fileName + ".ascx", picname, "1", "1", OpenWSE.Core.Licensing.CheckLicense.SiteName + " | " + ServerSettings.ServerDateTime.Year, description, "app-main", categoryid, "450", "500", false, true, "~/ExternalAppHolder.aspx?appId=" + appId, "", "", false, "1", isPrivate, true, string.Empty, "#FFFFFF");

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