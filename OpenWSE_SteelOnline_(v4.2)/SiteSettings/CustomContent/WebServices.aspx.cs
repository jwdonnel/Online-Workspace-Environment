using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;
using System.Web.Security;

public partial class SiteSettings_WebServices : System.Web.UI.Page {
    #region private variables

    private readonly AppLog _applog = new AppLog(false);
    private readonly WebServicesAvailable wsa = new WebServicesAvailable();
    private string _username;

    private const string WSDLNamespace = "http://schemas.xmlsoap.org/wsdl/";
    private const string XSDNamespace = "http://www.w3.org/2001/XMLSchema";

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated) {
            Page.Response.Redirect("~/Default.aspx");
        }
        else {
            if (ServerSettings.AdminPagesCheck(Page.ToString(), userId.Name)) {
                _username = userId.Name;
                if (Request.QueryString["error"] == "invalidformat") {
                    lbl_Error.Text = "Invalid file format!";
                    RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function () { $('#" + lbl_Error.ClientID + "').html(''); }, 3000);");
                }
                else if (Request.QueryString["error"] == "couldnotupload") {
                    lbl_Error.Text = "Error. Could not upload web service file!";
                    RegisterPostbackScripts.RegisterStartupScript(this, "setTimeout(function () { $('#" + lbl_Error.ClientID + "').html(''); }, 3000);");
                }

                BuildTable();
            }
            else {
                Page.Response.Redirect("~/ErrorPages/Blocked.html");
            }
        }
    }

    private List<WebService_Coll> GetDefaultWebservices() {
        List<WebService_Coll> coll = new List<WebService_Coll>();

        if (Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            string[] fileList = Directory.GetFiles(ServerSettings.GetServerMapLocation + "WebServices");
            foreach (string file in fileList) {
                FileInfo fi = new FileInfo(file);
                if (fi.Extension.ToLower() == ".asmx") {
                    Guid tempGuid = new Guid();
                    if (!Guid.TryParse(fi.Name.Replace(fi.Extension, string.Empty), out tempGuid)) {
                        WebService_Coll defaultWs = new WebService_Coll(Guid.NewGuid().ToString(), fi.Name, "This is a standard web service for OpenWSE", "-", "-");
                        coll.Add(defaultWs);
                    }
                }
            }
        }

        return coll;
    }

    private void BuildTable() {
        wsa.GetWebServices();
        List<WebService_Coll> defaultColl = GetDefaultWebservices();

        pnl_WebServiceList.Controls.Clear();
        var str = new StringBuilder();

        // Build Header
        str.Append("<div class='margin-top-sml'>");
        str.Append("<table cellpadding='5' cellspacing='0' style='min-width: 100%'><tbody>");
        str.Append("<tr class='myHeaderStyle'><td width='45px'></td><td width='250px'>Name</td>");
        str.Append("<td>Description</td>");
        str.Append("<td width='150px'>Upload Date</td>");
        str.Append("<td width='150px'>Uploaded By</td>");
        str.Append("<td width='105px'>Actions</td></tr>");

        int fileCount = 0;

        foreach (WebService_Coll ws in wsa.WS_Collection) {
            string fileExt = new FileInfo(ws.Filename).Extension;
            if (string.IsNullOrEmpty(fileExt)) {
                fileExt = ".wsdl";
            }

            string downloadBtn = "<a href= '#download' class='td-download-btn margin-right-sml' onclick='DownloadWS(\"" + ws.ID + "\"); return false;', title='Download'></a>";
            string deleteBtn = "<a href='#delete' class='td-delete-btn' onclick='DeleteWebService(\"" + ws.ID + "\", \"" + ws.Filename + "\");return false;' title='Delete'></a>";
            string editBtn = "<a href='#edit' class='td-edit-btn margin-right-sml' onclick='EditWebService(\"" + ws.ID + "\");return false;' title='Edit'></a>";
            string updateBtn = "<a href='#update' class='td-update-btn margin-right-sml' onclick='UpdateWebService(\"" + ws.ID + "\");return false;' title='Update'></a>";
            string cancelBtn = "<a href='#cancel' class='td-cancel-btn' onclick='CancelWebService();return false;' title='Cancel'></a>";
            string linkBtn = "<a href='" + ResolveUrl("~/WebServices/" + ws.ID + fileExt) + "' target='_blank' class='td-view-btn margin-right-sml' title='View'></a>";

            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                if (ws.UploadedBy.ToLower() != _username.ToLower()) {
                    downloadBtn = string.Empty;
                    editBtn = string.Empty;
                    deleteBtn = string.Empty;
                    updateBtn = string.Empty;
                    cancelBtn = string.Empty;
                }
            }

            MemberDatabase tempMember = new MemberDatabase(ws.UploadedBy);

            if (hf_EditWS.Value == ws.ID) {
                str.Append("<tr class='myItemStyle GridNormalRow'>");
                str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + (fileCount + 1) + "</td>");
                str.Append("<td align='left' class='border-right border-bottom'><input id='tb_filenameEdit' type='text' class='textEntry' style='width: 98%!important;' value='" + ws.Filename + "' /></td>");
                str.Append("<td align='left' class='border-right border-bottom'><input id='tb_descriptionEdit' type='text' class='textEntry' style='width: 98%!important;' value='" + ws.Description + "' /></td>");
                str.Append("<td align='center' class='border-right border-bottom'>" + ws.UploadDate + "</td>");
                str.Append("<td align='center' class='border-right border-bottom'>" + HelperMethods.MergeFMLNames(tempMember) + "</td>");
                str.Append("<td align='center' class='border-right border-bottom'>" + updateBtn + cancelBtn + "</td></tr>");
            }
            else {
                string desc = ws.Description;
                if (string.IsNullOrEmpty(desc)) {
                    desc = "No description given";
                }
                str.Append("<tr class='myItemStyle GridNormalRow'>");
                str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + (fileCount + 1) + "</td>");
                str.Append("<td class='border-right border-bottom'>" + ws.Filename + "</td>");
                str.Append("<td align='left' class='border-right border-bottom'>" + desc + "</td>");
                str.Append("<td align='center' class='border-right border-bottom'>" + ws.UploadDate + "</td>");
                str.Append("<td align='center' class='border-right border-bottom'>" + HelperMethods.MergeFMLNames(tempMember) + "</td>");
                str.Append("<td align='center' class='border-right border-bottom'>" + linkBtn + downloadBtn + editBtn + deleteBtn + "</td></tr>");
            }

            fileCount++;
        }

        foreach (WebService_Coll ws in defaultColl) {
            string linkBtn = "<a href='" + ResolveUrl("~/WebServices/" + ws.Filename) + "' target='_blank' class='td-view-btn margin-right-sml' title='View'></a>";
            string desc = ws.Description;
            if (string.IsNullOrEmpty(desc)) {
                desc = "No description given";
            }
            str.Append("<tr class='myItemStyle GridNormalRow'>");
            str.Append("<td class='GridViewNumRow border-bottom' style='text-align: center'>" + (fileCount + 1) + "</td>");
            str.Append("<td class='border-right border-bottom'>" + ws.Filename + "</td>");
            str.Append("<td align='left' class='border-right border-bottom'>" + desc + "</td>");
            str.Append("<td align='center' class='border-right border-bottom'>" + ws.UploadDate + "</td>");
            str.Append("<td align='center' class='border-right border-bottom'>" + ws.UploadedBy + "</td>");
            str.Append("<td align='center' class='border-right border-bottom'>" + linkBtn + "</td></tr>");

            fileCount++;
        }

        str.Append("</tbody></table></div>");
        if (fileCount == 0) {
            str.Append("<div class='emptyGridView'>No Web Services Found</div>");
        }

        lbl_TotalWebServices.Text = "<span class='font-bold pad-right-sml'>Total Files:</span>" + fileCount.ToString();
        pnl_WebServiceList.Controls.Add(new LiteralControl(str.ToString()));
    }

    protected void lbtn_refresh_Clicked(object sender, EventArgs e) {
        BuildTable();
    }


    #region Edit Buttons

    protected void hf_DeleteWS_Changed(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_DeleteWS.Value)) {

            bool canDelete = true;
            if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
                WebService_Coll wsColl = wsa.GetWebService(hf_DeleteWS.Value);
                if (wsColl.UploadedBy.ToLower() != _username.ToLower()) {
                    canDelete = false;
                }
            }

            if (canDelete) {
                string fileExt = wsa.GetFileExt(hf_DeleteWS.Value);
                if (string.IsNullOrEmpty(fileExt)) {
                    fileExt += ".wsdl";
                }
                string filename = hf_DeleteWS.Value + fileExt;

                // Make sure we dont delete any standard web services

                Guid tempGuid = new Guid();
                if (Guid.TryParse(hf_DeleteWS.Value, out tempGuid)) {
                    string webServiceFolder = ServerSettings.GetServerMapLocation + "WebServices\\" + filename;
                    try {
                        if (File.Exists(webServiceFolder)) {
                            File.Delete(webServiceFolder);
                        }
                    }
                    catch { }

                    wsa.DeleteItem(hf_DeleteWS.Value);
                }
            }
        }
        BuildTable();
        hf_DeleteWS.Value = string.Empty;
    }
    protected void hf_EditWS_Changed(object sender, EventArgs e) {
        BuildTable();
        hf_EditWS.Value = string.Empty;
        RegisterPostbackScripts.RegisterStartupScript(this, "SaveTableHtml();");
    }
    protected void hf_UpdateWS_Changed(object sender, EventArgs e) {
        bool canUpdate = true;
        if (!Roles.IsUserInRole(_username, ServerSettings.AdminUserName)) {
            WebService_Coll wsColl = wsa.GetWebService(hf_UpdateWS.Value);
            if (wsColl.UploadedBy.ToLower() != _username.ToLower()) {
                canUpdate = false;
            }
        }

        if (canUpdate) {
            if ((!string.IsNullOrEmpty(hf_UpdateWS.Value)) && (!string.IsNullOrEmpty(hf_FilenameEdit.Value.Trim()))) {
                wsa.UpdateItem(hf_UpdateWS.Value, hf_FilenameEdit.Value.Trim(), hf_DescriptionEdit.Value.Trim());
            }
        }

        BuildTable();
        hf_UpdateWS.Value = string.Empty;
        hf_FilenameEdit.Value = string.Empty;
        hf_DescriptionEdit.Value = string.Empty;
    }
    protected void hf_CancelWS_Changed(object sender, EventArgs e) {
        hf_CancelWS.Value = string.Empty;
        BuildTable();
    }

    #endregion

    protected void DownloadWebservice(object sender, EventArgs e) {
        WebService_Coll ws = wsa.GetWebService(hf_wsId.Value);
        if (!string.IsNullOrEmpty(ws.Filename)) {
            string drive = new DirectoryInfo(ServerSettings.GetServerMapLocation).Root.Name.Replace("\\", "");

            string fileExt = wsa.GetFileExt(ws.ID);
            if (string.IsNullOrEmpty(fileExt)) {
                fileExt += ".wsdl";
            }

            string filePath = ServerSettings.GetServerMapLocation + "WebServices\\" + ws.ID + fileExt;
            try {
                if (File.Exists(filePath)) {
                    Response.ClearContent();
                    Response.Clear();
                    Response.ContentType = "application/octet-stream";
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + ws.Filename.Replace(" ", "_") + fileExt);
                    Response.WriteFile(filePath);
                    Response.Flush();
                    Response.End();
                }
            }
            catch { }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('There was an error downloading the web service.');");
        }

        hf_wsId.Value = string.Empty;
    }

    protected void btn_uploadFile_Clicked(object sender, EventArgs e) {
        string webServiceFolder = ServerSettings.GetServerMapLocation + "WebServices";
        if (!Directory.Exists(webServiceFolder)) {
            try {
                Directory.CreateDirectory(webServiceFolder);
            }
            catch { }
        }

        string fileName = Guid.NewGuid().ToString();
        if (fu_newWebServiceFile.HasFile) {
            FileInfo fi = new FileInfo(fu_newWebServiceFile.FileName);
            if ((fi.Extension.ToLower() == ".wsdl") || (fi.Extension.ToLower() == ".asmx")) {
                try {
                    fu_newWebServiceFile.SaveAs(webServiceFolder + "\\" + fileName + fi.Extension);
                    wsa.AddItem(fileName, fu_newWebServiceFile.FileName, tb_descriptionUpload.Text.Trim());
                }
                catch { 
                }
            }
            else {
                ServerSettings.PageToolViewRedirect(this.Page, "WebServices.aspx?error=invalidformat");
            }

            ServerSettings.PageToolViewRedirect(this.Page, "WebServices.aspx");
        }
        else if ((tb_wsdlUrl.Text.Trim() != "") && (tb_wsdlName.Text.Trim() != "")) {
            try {
                WebClient wc = new WebClient();
                string wsdlText = wc.DownloadString(tb_wsdlUrl.Text.Trim());

                // load WSDL
                XmlDocument wsdl = new XmlDocument();
                wsdl.LoadXml(wsdlText);

                // Need to replace any instance of xs and replace it was xsd
                string innerXml = wsdl.InnerXml;
                innerXml = innerXml.Replace("<xs:", "<xsd:");
                innerXml = innerXml.Replace("</xs:", "</xsd:");
                innerXml = innerXml.Replace("xmlns:xs=", "xmlns:xsd=");
                wsdl.InnerXml = innerXml;

                // add standard namespaces
                XmlNamespaceManager manager = PrepareNamespaceManager(wsdl);

                // verify it is a WSDL file
                if (!VerifyWSDL(wsdl, manager)) {
                    ServerSettings.PageToolViewRedirect(this.Page, "WebServices.aspx?error=invalidformat");
                }

                wsa.AddItem(fileName, tb_wsdlName.Text.Trim(), tb_descriptionUpload.Text.Trim());
                wsdl.Save(webServiceFolder + "\\" + fileName + ".wsdl");
                
                ServerSettings.PageToolViewRedirect(this.Page, "WebServices.aspx?date=" + DateTime.Now.Ticks.ToString());
            }
            catch { }
        }

        ServerSettings.PageToolViewRedirect(this.Page, "WebServices.aspx?date=" + DateTime.Now.Ticks.ToString());
    }

    private bool VerifyWSDL(XmlDocument wsdl, XmlNamespaceManager manager) {
        XmlNode node = wsdl.SelectSingleNode("/wsdl:definitions", manager);
        if (node == null) {
            return false;
        }
        else {
            return true;
        }
    }
    private XmlNamespaceManager PrepareNamespaceManager(XmlDocument wsdl) {
        XmlNamespaceManager manager = new XmlNamespaceManager(wsdl.NameTable);
        manager.AddNamespace("wsdl", WSDLNamespace);
        manager.AddNamespace("xsd", XSDNamespace);
        return manager;
    }
}