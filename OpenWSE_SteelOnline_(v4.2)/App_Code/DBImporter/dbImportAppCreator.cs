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

        if (string.IsNullOrEmpty(picname)) {
            picname = "database.png";
        }

        StringBuilder appStr = new StringBuilder();

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
        appStr.Append("cl = _apps.GetAppName(app_id);");
        appStr.Append("lbl_Title_" + fileName + ".Text = cl;");
        appStr.Append("if (!_ss.HideAllAppIcons) {");
        appStr.Append("img_Title_" + fileName + ".Visible = true;");
        appStr.Append("string clImg = _apps.GetAppIconName(app_id);");
        appStr.Append("img_Title_" + fileName + ".ImageUrl = \"~/Standard_Images/App_Icons/\" + clImg; }");
        appStr.Append("else {");
        appStr.Append("img_Title_" + fileName + ".Visible = false; }");
        appStr.Append("_appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page, \"" + applyTo + "\");");
        appStr.Append("_appInitializer.LoadScripts_JS(true, \"Load_dbImports('" + fileName + "', " + _allowEdit.ToString() + ");\"); }");
        appStr.Append("</script>");

        // HTML Code
        appStr.Append("<div id=\"" + fileName + "-load\" class=\"main-div-app-bg custom-table-timermarker\">");
        appStr.Append("<table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">");
        appStr.Append("<tr>");


        appStr.Append("<td valign=\"top\">");
        appStr.Append("<div class=\"pad-all app-title-bg-color\" style=\"height: 40px\">");
        appStr.Append("<div class=\"float-left\">");
        appStr.Append("<asp:Image ID=\"img_Title_" + fileName + "\" runat=\"server\" CssClass=\"float-left pad-right\" Height=\"38px\" />");
        appStr.Append("<asp:Label ID=\"lbl_Title_" + fileName + "\" runat=\"server\" Text=\"\" Font-Size=\"30px\"></asp:Label>");
        appStr.Append("</div>");
        appStr.Append("<div class=\"float-right pad-top-sml\" style=\"font-size: 15px\">");
        appStr.Append("<div class=\"float-right\">");
        appStr.Append("<div id=\"searchwrapper\" style=\"width: 375px;\">");
        appStr.Append("<input id=\"tb_search_" + fileName + "\" type=\"text\" class=\"searchbox\" onfocus=\"if(this.value=='Search this table')this.value=''\" onblur=\"if(this.value=='')this.value='Search this table'\" onkeypress=\"KeyPressSearch_dbImports(event, '" + fileName + "', " + _allowEdit.ToString() + ")\" value=\"Search this table\" />");
        appStr.Append("<a href=\"#\" onclick=\"$('#tb_search_" + fileName + "').val('Search this table');Refresh_dbImports('" + fileName + "', " + _allowEdit.ToString() + ");return false;\" title=\"Clear search\" class=\"searchbox_clear\"></a>");
        appStr.Append("<a href=\"#\" class=\"searchbox_submit\" onclick=\"Search_dbImports('" + fileName + "', " + _allowEdit.ToString() + ");return false;\"></a></div>");
        appStr.Append("</div></div></div>");


        appStr.Append("<div class=\"pad-all\">");
        appStr.Append("<div class=\"pad-top\">");
        appStr.Append("<a href=\"#\" class=\"float-right margin-right margin-left img-refresh\" onclick=\"Refresh_dbImports('" + fileName + "', " + _allowEdit.ToString() + ");return false;\" title=\"Refresh List\"></a>");
        appStr.Append("<select id=\"font-size-selector-" + fileName + "\" class=\"custom-table-font-selector float-right margin-left\">");
        appStr.Append("<option value=\"x-small\">Font Size: x-Small</option>");
        appStr.Append("<option value=\"small\" selected=\"selected\">Font Size: Small</option>");
        appStr.Append("<option value=\"medium\">Font Size: Medium</option>");
        appStr.Append("<option value=\"large\">Font Size: Large</option>");
        appStr.Append("<option value=\"x-large\">Font Size: x-Large</option>");
        appStr.Append("</select>");
        appStr.Append("<div class=\"float-right margin-right\">");
        appStr.Append("<span class=\"font-bold pad-right\">Records to Pull</span>");

        appStr.Append("<select id=\"RecordstoSelect_" + fileName + "\" onchange=\"RecordstoSelect_dbImports('" + fileName + "', " + _allowEdit.ToString() + ")\">");
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
        appStr.Append("<b class=\"pad-right-sml\">Export table to Spreadsheet</b>");
        appStr.Append("<input id=\"btnExport-" + fileName + "\" type=\"button\" class=\"input-buttons margin-left\" onclick=\"ExportToExcelAll_dbImports('" + fileName + "')\" title=\"Select a date to export to an Excel file\" value=\"Export\" />");
        appStr.Append("<span id=\"exportingNow_" + fileName + "\" class=\"margin-left margin-top margin-bottom\" style=\"display: none;\">Exporting...</span>");
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

        string filePath = ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\" + fileName + ".ascx";
        if (!Directory.Exists(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\"))
            Directory.CreateDirectory(ServerSettings.GetServerMapLocation + "Apps\\Database_Imports\\");

        using (var file = new StreamWriter(filePath))
        {
            file.Write(appStr.ToString());
        }

        if ((insertIntoAppList) && (!string.IsNullOrEmpty(appName)))
            _app.CreateItem(appId, appName, "Database_Imports/" + fileName + ".ascx", picname, "1", "1", ServerSettings.SiteName + " | " + DateTime.Now.Year, description, "app-main", categoryid, "500", "700", false, true, "~/ExternalAppHolder.aspx?appId=" + appId, "", "", false, true, false, "1", isPrivate);

        return fileName;
    }
}