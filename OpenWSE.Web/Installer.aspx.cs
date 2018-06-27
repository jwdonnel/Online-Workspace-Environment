using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using System.Web.UI;

public partial class Installer : Page {

    private string compilationDebug = "false";

    protected void Page_Load(object sender, EventArgs e) {
        if (ServerSettings.CheckWebConfigFile) {
            HelperMethods.PageRedirect("~/" + ServerSettings.DefaultStartupPage);
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "openWSE_Config.siteRootFolder='" + ResolveUrl("~/").Replace("/", "") + "';");
        BaseMaster baseMaster = new BaseMaster();
        baseMaster.LoadAllDefaultScriptsStyleSheets(this.Page);
    }

    protected void lbtn_compact_Click(object sender, EventArgs e) {
        string filePath = ServerSettings.GetServerMapLocation + "App_Data\\DatabaseWebConfigs\\Web_SQLCe.config";
        try {
            GetCurrentWebConfigFile();

            if (File.Exists(filePath)) {
                string appId = Guid.NewGuid().ToString();

                string[] allLines = File.ReadAllLines(filePath);
                for (int i = 0; i < allLines.Length; i++) {
                    if (allLines[i].Trim().Contains("APPLICATIONIDHERE")) {
                        allLines[i] = allLines[i].Replace("APPLICATIONIDHERE", appId);
                    }
                    else {
                        switch (allLines[i].Trim()) {
                            case "<MACHINEKEYHERE/>":
                                string vKey = CreateKey(64);
                                string dKey = CreateKey(24);
                                allLines[i] = string.Format("        <machineKey validationKey=\"{0}\" decryptionKey=\"{1}\" validation=\"SHA1\" />", vKey, dKey);
                                break;

                            case "<COMPILATIONHERE>":
                                allLines[i] = string.Format("        <compilation debug=\"{0}\" targetFramework=\"4.6.1\">", compilationDebug);
                                break;

                            case "</COMPILATIONHERE>":
                                allLines[i] = "        </compilation>";
                                break;

                            case "<APPLICATIONID/>":
                                allLines[i] = string.Format("        <add key=\"ApplicationId\" value=\"{0}\" />", appId);
                                break;
                        }
                    }
                }

                string webConfigPath = ServerSettings.GetServerMapLocation + "Web.config";
                if (File.Exists(webConfigPath)) {
                    File.WriteAllLines(webConfigPath, allLines);

                    ServerSettings.StartServerApplication(IsPostBack);
                    HelperMethods.PageRedirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
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
                    string appId = Guid.NewGuid().ToString();

                    string[] allLines = File.ReadAllLines(filePath);
                    for (int i = 0; i < allLines.Length; i++) {
                        if (allLines[i].Trim().Contains("APPLICATIONIDHERE")) {
                            allLines[i] = allLines[i].Replace("APPLICATIONIDHERE", appId);
                        }
                        else {
                            switch (allLines[i].Trim()) {
                                case "<MACHINEKEYHERE/>":
                                    string vKey = CreateKey(64);
                                    string dKey = CreateKey(24);
                                    allLines[i] = string.Format("        <machineKey validationKey=\"{0}\" decryptionKey=\"{1}\" validation=\"SHA1\" />", vKey, dKey);
                                    break;

                                case "<CONNECTIONSTRINGHERE/>":
                                    allLines[i] = string.Format("        <add name=\"ApplicationServices\" connectionString=\"{0}\" providerName=\"System.Data.SqlClient\" />", txt_connectionstring.Text.Trim());
                                    break;

                                case "<COMPILATIONHERE>":
                                    allLines[i] = string.Format("        <compilation debug=\"{0}\" targetFramework=\"4.6.1\">", compilationDebug);
                                    break;

                                case "</COMPILATIONHERE>":
                                    allLines[i] = "        </compilation>";
                                    break;

                                case "<APPLICATIONID/>":
                                    allLines[i] = string.Format("        <add key=\"ApplicationId\" value=\"{0}\" />", appId);
                                    break;
                            }
                        }
                    }

                    string webConfigPath = ServerSettings.GetServerMapLocation + "Web.config";
                    if (File.Exists(webConfigPath)) {
                        File.WriteAllLines(webConfigPath, allLines);

                        ServerSettings.StartServerApplication(IsPostBack);
                        HelperMethods.PageRedirect("~/SiteTools/ServerMaintenance/SiteSettings.aspx");
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
        XmlNode node = doc.DocumentElement.FirstChild.SelectSingleNode("compilation");
        if (node != null) {
            if (node.Attributes["debug"] != null && !string.IsNullOrEmpty(node.Attributes["debug"].Value)) {
                compilationDebug = node.Attributes["debug"].Value;
            }
        }
    }

    private const string DefaultSqlExpressStr = @"data source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\localdatabase.mdf;User Instance=true";

    protected void lbtn_expressContinue_Click(object sender, EventArgs e) {
        lbl_errorMessage.Enabled = false;
        lbl_errorMessage.Visible = false;
        pnl1.Enabled = false;
        pnl1.Visible = false;
        pnl2.Enabled = true;
        pnl2.Visible = true;
        txt_connectionstring.Text = DefaultSqlExpressStr;
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
        txt_connectionstring.Text = DefaultSqlExpressStr;
    }

    protected void lbtn_clearConnectionString_Click(object sender, EventArgs e) {
        txt_connectionstring.Text = string.Empty;
    }

    private static String CreateKey(int numBytes) {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] buff = new byte[numBytes];

        rng.GetBytes(buff);
        return BytesToHexString(buff);
    }

    private static String BytesToHexString(byte[] bytes) {
        StringBuilder hexString = new StringBuilder(64);

        for (int counter = 0; counter < bytes.Length; counter++) {
            hexString.Append(String.Format("{0:X2}", bytes[counter]));
        }
        return hexString.ToString();
    }

}