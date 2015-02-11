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
    private readonly App _app = new App();
    private readonly StartupScripts _startupScripts = new StartupScripts(true);
    private string _username;
    private string _allowEdit = "true";
    #endregion

    public dbImportAppCreator(string username, bool allowEdit = true) 
    {
        _username = username;
        _allowEdit = allowEdit.ToString().ToLower();
    }

    public string CreateApp(string appName, string categoryid, string description, string picname, bool isPrivate, bool insertIntoAppList = true, string fileName = "")
    {
        if (string.IsNullOrEmpty(fileName))
            fileName = HelperMethods.RandomString(10);

        string appId = "app-" + fileName;
        string applyTo = "Database Import";
        string appClass = "main-div-app-bg";

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
        appStr.AppendLine("     private readonly App _apps = new App();");
        appStr.AppendLine("     private OpenWSE_Tools.Apps.AppInitializer _appInitializer;");
        appStr.AppendLine("     private const string app_id = \"" + appId + "\";");
        appStr.AppendLine("");
        appStr.AppendLine("     protected void Page_Load(object sender, EventArgs e) {");
        appStr.AppendLine("         System.Security.Principal.IIdentity userId = Page.User.Identity;");
        appStr.AppendLine("         DBImporter_Coll coll = dbImportservice.GetDataBase_Coll(\"" + fileName + "\");");
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
        appStr.AppendLine("         if (!string.IsNullOrEmpty(coll.Chart_Type.ToString()) && coll.Chart_Type != ChartType.None) {");
        appStr.AppendLine("             pnl_" + fileName + "_chartType.Enabled = true;");
        appStr.AppendLine("             pnl_" + fileName + "_chartType.Visible = true;");
        appStr.AppendLine("             img_" + fileName + "_chartType.ImageUrl = \"~/Standard_Images/ChartTypes/\" + coll.Chart_Type.ToString().ToLower() + \".png\";");
        appStr.AppendLine("             hf_" + fileName + "_chartType.Value = coll.Chart_Type.ToString();");
        appStr.AppendLine("             string chartTitle = coll.ChartTitle;");
        appStr.AppendLine("             if (string.IsNullOrEmpty(chartTitle)) {");
        appStr.AppendLine("                 chartTitle = coll.TableName;");
        appStr.AppendLine("             }");
        appStr.AppendLine("             hf_" + fileName + "_chartTitle.Value = chartTitle;");
        appStr.AppendLine("         }");
        appStr.AppendLine("         else {");
        appStr.AppendLine("             pnl_" + fileName + "_chartType.Enabled = false;");
        appStr.AppendLine("             pnl_" + fileName + "_chartType.Visible = false;");
        appStr.AppendLine("         }");
        appStr.AppendLine("");
        appStr.AppendLine("         _appInitializer = new OpenWSE_Tools.Apps.AppInitializer(app_id, Page.User.Identity.Name, Page, \"" + applyTo + "\");");
        appStr.AppendLine("         _appInitializer.LoadScripts_JS(true, \"dbImport.Load('" + fileName + "');\");");
        appStr.AppendLine("     }");
        appStr.AppendLine("</script>");

        // HTML Code
        appStr.AppendLine("<div id=\"" + fileName + "-load\" class=\"" + appClass + " dbImport-table-timermarker\">");
        appStr.AppendLine("     <div id=\"pnl_" + fileName + "_tableView\" class=\"custom-table-tableView-holder\">");
        appStr.AppendLine("         <div class=\"pad-all app-title-bg-color\" style=\"height: 40px\">");
        appStr.AppendLine("             <div class=\"float-left\">");
        appStr.AppendLine("                 <asp:Image ID=\"img_Title_" + fileName + "\" runat=\"server\" CssClass=\"float-left pad-right\" Height=\"38px\" />");
        appStr.AppendLine("                 <asp:Label ID=\"lbl_Title_" + fileName + "\" runat=\"server\" Text=\"\" Font-Size=\"30px\"></asp:Label>");
        appStr.AppendLine("             </div>");
        appStr.AppendLine("             <div id=\"search_" + fileName + "_holder\" class=\"float-right pad-top-sml\" style=\"font-size: 15px\">");
        appStr.AppendLine("                 <div class=\"float-right\">");
        appStr.AppendLine("                     <div id=\"searchwrapper\" style=\"width: 375px;\">");
        appStr.AppendLine("                         <input id=\"tb_search_" + fileName + "\" type=\"text\" class=\"searchbox\" onfocus=\"if(this.value=='Search this table')this.value=''\" onblur=\"if(this.value=='')this.value='Search this table'\" onkeypress=\"dbImport.KeyPressSearch(event, '" + fileName + "')\" value=\"Search this table\" />");
        appStr.AppendLine("                         <a href=\"#\" onclick=\"$('#tb_search_" + fileName + "').val('Search this table');dbImport.Refresh('" + fileName + "');return false;\" class=\"searchbox_clear\"></a>");
        appStr.AppendLine("                         <a href=\"#\" class=\"searchbox_submit\" onclick=\"dbImport.Search('" + fileName + "');return false;\"></a>");
        appStr.AppendLine("                     </div>");
        appStr.AppendLine("                 </div>");
        appStr.AppendLine("             </div>");
        appStr.AppendLine("         </div>");
        appStr.AppendLine("         <asp:Panel ID=\"pnl_" + fileName + "_chartType\" CssClass=\"custom-table-view-chart\" runat=\"server\">");
        appStr.AppendLine("             <a href=\"#\" onclick=\"dbImport.ViewChart('" + fileName + "', this);return false;\">");
        appStr.AppendLine("                 <asp:HiddenField ID=\"hf_" + fileName + "_chartType\" runat=\"server\" />");
        appStr.AppendLine("                 <asp:HiddenField ID=\"hf_" + fileName + "_chartTitle\" runat=\"server\" />");
        appStr.AppendLine("                 <asp:Image ID=\"img_" + fileName + "_chartType\" runat=\"server\" />");
        appStr.AppendLine("                 <span>View Data in Chart</span>");
        appStr.AppendLine("             </a>");
        appStr.AppendLine("         </asp:Panel>");
        appStr.AppendLine("         <div class=\"float-right pad-top\">");
        appStr.AppendLine("             <a href=\"#\" class=\"float-right margin-right margin-left img-refresh\" onclick=\"dbImport.Refresh('" + fileName + "');return false;\" title=\"Refresh List\"></a>");
        appStr.AppendLine("             <select id=\"font-size-selector-" + fileName + "\" class=\"custom-table-font-selector float-right margin-left\">");
        appStr.AppendLine("                 <option value=\"x-small\">Font Size: x-Small</option>");
        appStr.AppendLine("                 <option value=\"small\" selected=\"selected\">Font Size: Small</option>");
        appStr.AppendLine("                 <option value=\"medium\">Font Size: Medium</option>");
        appStr.AppendLine("                 <option value=\"large\">Font Size: Large</option>");
        appStr.AppendLine("                 <option value=\"x-large\">Font Size: x-Large</option>");
        appStr.AppendLine("             </select>");
        appStr.AppendLine("             <div class=\"float-right margin-right\">");
        appStr.AppendLine("                 <span class=\"font-bold pad-right\">Records to Pull</span>");
        appStr.AppendLine("                 <select id=\"RecordstoSelect_" + fileName + "\" onchange=\"dbImport.RecordstoSelect('" + fileName + "')\">");
        appStr.AppendLine("                     <option value=\"5\">5</option>");
        appStr.AppendLine("                     <option value=\"10\">10</option>");
        appStr.AppendLine("                     <option value=\"25\">25</option>");
        appStr.AppendLine("                     <option value=\"50\" selected=\"selected\">50</option>");
        appStr.AppendLine("                     <option value=\"75\">75</option>");
        appStr.AppendLine("                     <option value=\"100\">100</option>");
        appStr.AppendLine("                     <option value=\"200\">200</option>");
        appStr.AppendLine("                     <option value=\"all\">All</option>");
        appStr.AppendLine("                 </select>");
        appStr.AppendLine("             </div>");
        appStr.AppendLine("             <div class=\"clear-space\"></div>");
        appStr.AppendLine("             <table cellpadding=\"0\" cellspacing=\"0\">");
        appStr.AppendLine("                 <tr>");
        appStr.AppendLine("                     <td>");
        appStr.AppendLine("                         <b class=\"pad-right\">Export to Spreadsheet From dates</b>");
        appStr.AppendLine("                         <input id=\"tb_exportDateFrom_" + fileName + "\" type=\"text\" class=\"textEntry\" style=\"width: 85px;\" />");
        appStr.AppendLine("                         <b class=\"pad-left-sml pad-right-sml\">To</b>");
        appStr.AppendLine("                         <input id=\"tb_exportDateTo_" + fileName + "\" type=\"text\" class=\"textEntry\" style=\"width: 85px;\" />");
        appStr.AppendLine("                     </td>");
        appStr.AppendLine("                     <td>");
        appStr.AppendLine("                         <input id=\"btnExport-" + fileName + "\" type=\"button\" class=\"input-buttons margin-left\" onclick=\"dbImport.ExportToExcelAll('" + fileName + "')\" title=\"Select a date to export to an Excel file\" value=\"Export\" />");
        appStr.AppendLine("                         <span id=\"exportingNow_" + fileName + "\" class=\"margin-left margin-top margin-bottom\" style=\"display: none;\">Exporting...</span>");
        appStr.AppendLine("                     </td>");
        appStr.AppendLine("                 </tr>");
        appStr.AppendLine("             </table>");
        appStr.AppendLine("         </div>");
        appStr.AppendLine("         <div class=\"clear-space-five\"></div>");
        appStr.AppendLine("         <div class=\"float-left pad-left\" style=\"font-size: 11px;\">Data refreshed every minute</div>");
        appStr.AppendLine("         <div class=\"clear\"></div>");
        appStr.AppendLine("         <div id=\"data-holder-" + fileName + "\" class=\"pad-left pad-right\"></div>");
        appStr.AppendLine("     </div>");
        appStr.AppendLine("     <div id=\"pnl_" + fileName + "_chartView\" class=\"pad-left pad-right\" style=\"display: none;\"></div>");
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
            _app.CreateItem(appId, appName, "Database_Imports/" + fileName + ".ascx", picname, "1", "1", OpenWSE.Core.Licensing.CheckLicense.SiteName + " | " + DateTime.Now.Year, description, "app-main", categoryid, "250", "300", false, true, "~/ExternalAppHolder.aspx?appId=" + appId, "", "", false, "1", isPrivate);

        return fileName;
    }
}