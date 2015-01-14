using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using OpenWSE.Core.Licensing;

public partial class DatabaseSelection : System.Web.UI.Page {

    private string customErrorsMode = "Off";
    private string customErrorsDefaultRedirect = @"ErrorPages\Error.html";
    private string compilationDebug = "false";

    protected void Page_Load(object sender, EventArgs e) {
        CheckLicense.ValidateLicense();

        if (!CheckLicense.LicenseValid) {
            HttpContext.Current.Response.Redirect("~/SiteTools/ServerMaintenance/LicenseManager.aspx");
        }

        if (!IsPostBack && CheckLicense.TrialActivated && CheckLicense.LicenseValid) {
            string trialScript = "SetTrialText('" + CheckLicense.DaysLeftBeforeExpired + "');";
            RegisterPostbackScripts.RegisterStartupScript(this, trialScript);
        }
        
        if (ServerSettings.CheckWebConfigFile()) {
            ServerSettings.StartServerApplication();
            HttpContext.Current.Response.Redirect("~/Default.aspx");
        }
    }

    protected void lbtn_compact_Click(object sender, EventArgs e) {
        string filePath = ServerSettings.GetServerMapLocation + "App_Data\\DatabaseWebConfigs\\Web_SQLCe.config";
        try {
            GetCurrentWebConfigFile();

            if (File.Exists(filePath)) {
                string[] allLines = File.ReadAllLines(filePath);
                for (int i = 0; i < allLines.Length; i++) {
                    switch (allLines[i].Trim()) {
                        case "<CUSTOMERRORSHERE/>":
                            allLines[i] = string.Format("<customErrors mode=\"{0}\" defaultRedirect=\"{1}\" />", customErrorsMode, customErrorsDefaultRedirect);
                            break;

                        case "<COMPILATIONHERE>":
                            allLines[i] = string.Format("<compilation debug=\"{0}\" targetFramework=\"4.5\">", compilationDebug);
                            break;

                        case "</COMPILATIONHERE>":
                            allLines[i] = "</compilation>";
                            break;
                    }
                }

                string webConfigPath = ServerSettings.GetServerMapLocation + "Web.config";
                if (File.Exists(webConfigPath)) {
                    File.WriteAllLines(webConfigPath, allLines);

                    ServerSettings.StartServerApplication();
                    Response.Redirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
                }
            }
        }
        catch { }
    }

    protected void lbtn_express_Click(object sender, EventArgs e) {
        if (string.IsNullOrEmpty(txt_connectionstring.Text.Trim())) {
            lbl_errorMessage.Enabled = true;
            lbl_errorMessage.Visible = true;
        }
        else {
            lbl_errorMessage.Enabled = false;
            lbl_errorMessage.Visible = false;

            string filePath = ServerSettings.GetServerMapLocation + "App_Data\\DatabaseWebConfigs\\Web_SQLExpress.config";
            try {
                GetCurrentWebConfigFile();

                if (File.Exists(filePath)) {
                    string[] allLines = File.ReadAllLines(filePath);
                    for (int i = 0; i < allLines.Length; i++) {
                        switch (allLines[i].Trim()) {
                            case "<CONNECTIONSTRINGHERE/>":
                                allLines[i] = string.Format("<add name=\"ApplicationServices\" connectionString=\"{0}\" providerName=\"System.Data.SqlClient\" />", txt_connectionstring.Text.Trim());
                                break;

                            case "<CUSTOMERRORSHERE/>":
                                allLines[i] = string.Format("<customErrors mode=\"{0}\" defaultRedirect=\"{1}\" />", customErrorsMode, customErrorsDefaultRedirect);
                                break;

                            case "<COMPILATIONHERE>":
                                allLines[i] = string.Format("<compilation debug=\"{0}\" targetFramework=\"4.5\">", compilationDebug);
                                break;

                            case "</COMPILATIONHERE>":
                                allLines[i] = "</compilation>";
                                break;
                        }
                    }

                    string webConfigPath = ServerSettings.GetServerMapLocation + "Web.config";
                    if (File.Exists(webConfigPath)) {
                        File.WriteAllLines(webConfigPath, allLines);

                        ServerSettings.StartServerApplication();
                        HttpContext.Current.Response.Redirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
                    }
                }
            }
            catch { }
        }
    }

    private void GetCurrentWebConfigFile() {
        string path = HttpContext.Current.Server.MapPath("~/Web.Config");
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        XmlNode node = doc.DocumentElement.FirstChild.SelectSingleNode("customErrors");
        if (node != null) {
            if (node.Attributes["mode"] != null && !string.IsNullOrEmpty(node.Attributes["mode"].Value)) {
                customErrorsMode = node.Attributes["mode"].Value;
            }
        }

        node = doc.DocumentElement.FirstChild.SelectSingleNode("compilation");
        if (node != null) {
            if (node.Attributes["debug"] != null && !string.IsNullOrEmpty(node.Attributes["debug"].Value)) {
                compilationDebug = node.Attributes["debug"].Value;
            }
        }
    }

    protected void lbtn_expressContinue_Click(object sender, EventArgs e) {
        lbl_errorMessage.Enabled = false;
        lbl_errorMessage.Visible = false;
        pnl1.Enabled = false;
        pnl1.Visible = false;
        pnl2.Enabled = true;
        pnl2.Visible = true;
        txt_connectionstring.Text = string.Empty;
    }

    protected void lbtn_Cancel_Click(object sender, EventArgs e) {
        lbl_errorMessage.Enabled = false;
        lbl_errorMessage.Visible = false;
        pnl2.Enabled = false;
        pnl2.Visible = false;
        pnl1.Enabled = true;
        pnl1.Visible = true;
    }

    protected void lbtn_useDefaultConnectionString_Click(object sender, EventArgs e) {
        txt_connectionstring.Text = @"data source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\localdatabase.mdf;User Instance=true";
    }

    protected void lbtn_clearConnectionString_Click(object sender, EventArgs e) {
        txt_connectionstring.Text = string.Empty;
    }

}