using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;

/// <summary>
/// Summary description for AutomaticAppCreator
/// </summary>
public class CustomTableAppCreator
{
    #region Private Variables
    private readonly App _app = new App();
    private readonly StartupScripts _startupScripts = new StartupScripts(true);
    private string _username;
    #endregion

    public CustomTableAppCreator(string username)
    {
        _username = username;
    }

    public string CreateApp(bool showSideBar, string appName, string categoryid, string description, string picname, bool insertIntoAppList, bool isPrivate, string fileName = "")
    {
        StringBuilder appStr = new StringBuilder();
        if (string.IsNullOrEmpty(fileName))
            fileName = HelperMethods.RandomString(10);

        string appId = "app-" + fileName;
        string applyTo = "Custom Tables";

        string appClass = "main-div-app-bg";

        #region Code Builder
        appStr.Append("<%@ Control Language=\"C#\" ClassName=\"" + fileName + "\" ClientIDMode=\"Static\" %>");

        // C# Code
        appStr.Append("<script runat=\"server\">");
        appStr.Append("private ServerSettings _ss = new ServerSettings();");
        appStr.Append("private readonly App _apps = new App();");
        appStr.Append("private AppInitializer _appInitializer;");
        appStr.Append("private const string app_id = \"" + appId + "\";");
        appStr.Append("protected void Page_Load(object sender, EventArgs e) {");
        appStr.Append("System.Security.Principal.IIdentity userId = Page.User.Identity;");
        appStr.Append("string cl = \"\";");
        appStr.Append("CustomTableViewer ctv = new CustomTableViewer(userId.Name);");
        appStr.Append("cl = ctv.GetTableNameByAppID(app_id);");
        appStr.Append("lbl_Title_" + fileName + ".Text = cl;");
        appStr.Append("if (!_ss.HideAllAppIcons) {");
        appStr.Append("img_Title_" + fileName + ".Visible = true;");
        appStr.Append("string clImg = _apps.GetAppIconName(app_id);");
        appStr.Append("img_Title_" + fileName + ".ImageUrl = \"~/Standard_Images/App_Icons/\" + clImg; }");
        appStr.Append("else {");
        appStr.Append("img_Title_" + fileName + ".Visible = false; }");
        appStr.Append("_appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page, \"" + applyTo + "\");");
        appStr.Append("_appInitializer.LoadScripts_JS(true, \"Load_CustomTables('" + fileName + "');\"); }");
        appStr.Append("</script>");


        // HTML Code
        appStr.Append("<div id=\"" + fileName + "-load\" class=\"" + appClass + " custom-table-timermarker\">");

        appStr.Append("<div class=\"pad-all app-title-bg-color\" style=\"height: 40px\">");
        appStr.Append("<div class=\"float-left\">");
        appStr.Append("<asp:Image ID=\"img_Title_" + fileName + "\" runat=\"server\" CssClass=\"float-left pad-right\" Height=\"38px\" />");
        appStr.Append("<asp:Label ID=\"lbl_Title_" + fileName + "\" runat=\"server\" Text=\"\" Font-Size=\"30px\"></asp:Label>");
        appStr.Append("</div>");
        appStr.Append("<div class=\"float-right pad-top-sml\" style=\"font-size: 15px\">");
        appStr.Append("<div class=\"float-right\">");
        appStr.Append("<div id=\"searchwrapper\" style=\"width: 375px;\">");
        appStr.Append("<input id=\"tb_search_" + fileName + "\" type=\"text\" class=\"searchbox\" onfocus=\"if(this.value=='Search this table')this.value=''\" onblur=\"if(this.value=='')this.value='Search this table'\" onkeypress=\"KeyPressSearch_CustomTables(event, '" + fileName + "')\" value=\"Search this table\" />");
        appStr.Append("<a href=\"#\" onclick=\"$('#tb_search_" + fileName + "').val('Search this table');Refresh_CustomTables('" + fileName + "');return false;\" class=\"searchbox_clear\"></a>");
        appStr.Append("<a href=\"#\" class=\"searchbox_submit\" onclick=\"Search_CustomTables('" + fileName + "');return false;\"></a></div>");
        appStr.Append("</div></div></div>");

        appStr.Append("<table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">");
        appStr.Append("<tr>");


        if (showSideBar) {
            appStr.Append("<td valign=\"top\" class=\"td-sidebar pad-right\">");
            appStr.Append("<div id=\"sidebar-items-" + fileName + "\" class=\"sidebar-items-fixed\">");
            appStr.Append("<div class=\"clear-margin\">");
            appStr.Append("<div class=\"pad-left pad-top-big\"><h3>Month Selector</h3></div>");
            appStr.Append("<div class=\"clear-space\"></div>");
            appStr.Append("<div class=\"pad-left pad-right\"><small><b class=\"pad-right-sml\">Note:</b>These dates are taken from the timestamp of each created item.</small></div>");
            appStr.Append("<div class=\"clear-space\"></div>");
            appStr.Append("<div id=\"month-selector-" + fileName + "\" class=\"pad-left\"></div>");
            appStr.Append("</div>");
            appStr.Append("</div></td>");
        }


        appStr.Append("<td valign=\"top\">");
        appStr.Append("<div class=\"pad-all\">");
        appStr.Append("<div class=\"pad-top\">");
        appStr.Append("<a href=\"#\" class=\"float-right margin-right margin-left img-refresh\" onclick=\"Refresh_CustomTables('" + fileName + "');return false;\" title=\"Refresh List\"></a>");
        appStr.Append("<select id=\"font-size-selector-" + fileName + "\" class=\"custom-table-font-selector float-right margin-left\">");
        appStr.Append("<option value=\"x-small\">Font Size: x-Small</option>");
        appStr.Append("<option value=\"small\" selected=\"selected\">Font Size: Small</option>");
        appStr.Append("<option value=\"medium\">Font Size: Medium</option>");
        appStr.Append("<option value=\"large\">Font Size: Large</option>");
        appStr.Append("<option value=\"x-large\">Font Size: x-Large</option>");
        appStr.Append("</select>");
        appStr.Append("<div class=\"float-right margin-right\">");
        appStr.Append("<span class=\"font-bold pad-right\">Records to Pull</span>");

        appStr.Append("<select id=\"RecordstoSelect_" + fileName + "\" onchange=\"RecordstoSelect_CustomTables('" + fileName + "')\">");
        appStr.Append("<option value=\"5\">5</option>");
        appStr.Append("<option value=\"10\">10</option>");
        appStr.Append("<option value=\"25\">25</option>");
        appStr.Append("<option value=\"50\" selected=\"selected\">50</option>");
        appStr.Append("<option value=\"75\">75</option>");
        appStr.Append("<option value=\"100\">100</option>");
        appStr.Append("<option value=\"200\">200</option>");
        appStr.Append("<option value=\"all\">All</option>");
        appStr.Append("</select>");
        appStr.Append("</div>");

        appStr.Append("<div class=\"float-left pad-left\">");
        appStr.Append("<table cellpadding=\"0\" cellspacing=\"0\">");
        appStr.Append("<tr>");
        appStr.Append("<td>");
        appStr.Append("<b class=\"pad-right\">Export to Spreadsheet From dates</b>");
        appStr.Append("<input id=\"tb_exportDateFrom_" + fileName + "\" type=\"text\" class=\"textEntry\" style=\"width: 85px;\" />");
        appStr.Append("<b class=\"pad-left-sml pad-right\">To</b>");
        appStr.Append("<input id=\"tb_exportDateTo_" + fileName + "\" type=\"text\" class=\"textEntry\" style=\"width: 85px;\" />");
        appStr.Append("</td>");
        appStr.Append("<td>");
        appStr.Append("<input id=\"btnExport-" + fileName + "\" type=\"button\" class=\"input-buttons margin-left\" onclick=\"ExportToExcelAll_CustomTables('" + fileName + "')\" title=\"Select a date to export to an Excel file\" value=\"Export\" />");
        appStr.Append("<span id=\"exportingNow_" + fileName + "\" class=\"margin-left margin-top margin-bottom\" style=\"display: none;\">Exporting...</span>");
        appStr.Append("</td>");
        appStr.Append("</tr>");
        appStr.Append("</table>");
        appStr.Append("</div>");

        appStr.Append("<div class=\"clear-space-five\"></div>");
        appStr.Append("<div class=\"float-right pad-right\" style=\"font-size: 12px;\">Data refreshed every minute</div>");
        appStr.Append("</div></div>");


        appStr.Append("<div class=\"clear-space\"></div>");
        appStr.Append("<div class=\"clear-margin\">");
        appStr.Append("<div class=\"pad-left-big pad-right-big\">");
        appStr.Append("<div id=\"data-holder-" + fileName + "\"></div>");
        appStr.Append("</div>");
        appStr.Append("</div>");
        appStr.Append("</td></tr></table>");
        appStr.Append("</div>");
        #endregion

        string filePath = ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\" + fileName + ".ascx";
        if (!Directory.Exists(ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\"))
            Directory.CreateDirectory(ServerSettings.GetServerMapLocation + "Apps\\Custom_Tables\\");

        using (var file = new StreamWriter(filePath))
        {
            file.Write(appStr.ToString());
        }

        if (insertIntoAppList)
            _app.CreateItem(appId, appName, "Custom_Tables/" + fileName + ".ascx", picname, "1", "1", ServerSettings.SiteName + " | " + DateTime.Now.Year, description, "app-main", categoryid, "700", "900", false, true, "~/ExternalAppHolder.aspx?appId=" + appId, "", "", false, true, false, "1", isPrivate);

        return fileName;
    }
}