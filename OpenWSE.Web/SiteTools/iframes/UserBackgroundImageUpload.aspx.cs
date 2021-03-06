﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SiteTools_iframes_UserBackgroundImageUpload : Page
{
    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = User.Identity;
        if (!userId.IsAuthenticated || string.IsNullOrEmpty(Request.QueryString["id"])) {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }

        BaseMaster baseMaster = new BaseMaster();
        baseMaster.LoadAllDefaultScriptsStyleSheets(Page, new MemberDatabase(userId.Name));

        bool multiBackgrounds = new MemberDatabase(userId.Name).MultipleBackgrounds;
        RegisterPostbackScripts.RegisterStartupScript(this, "rootDir='" + Page.ResolveUrl("~/").Replace("/", "") + "';multipleBackgrounds=" + multiBackgrounds.ToString().ToLower() + ";");
    }

    protected void btn_UploadImages_Click(object sender, EventArgs e) {
        if (fileupload_userimages.HasFile || fileupload_userimages.HasFiles) {
            string fullPath = GetUploadedBackgroundDir;
            if (!Directory.Exists(fullPath)) {
                try {
                    Directory.CreateDirectory(fullPath);
                }
                catch {
                    ServerSettings.RefreshPage(Page, string.Empty);
                }
            }

            foreach (HttpPostedFile file in fileupload_userimages.PostedFiles) {
                string ext = Path.GetExtension(file.FileName);
                if (ext.ToLower() == ".jpg" || ext.ToLower() == ".jpeg" || ext.ToLower() == ".gif" || ext.ToLower() == ".png") {
                    file.SaveAs(fullPath + HelperMethods.RandomString(10) + ext);
                }
            }
        }

        if (Request.QueryString["javascriptpostback"] == "true") {
            RegisterPostbackScripts.RegisterStartupScript(this, "BackgroundSelector();");
        }
        else {
            ServerSettings.RefreshPage(Page, string.Empty);
        }
    }

    private string GetUploadedBackgroundDir {
        get {
            return ServerSettings.GetServerMapLocation + "Standard_Images\\AcctImages\\" + Request.QueryString["id"] + "\\UploadedBackgrounds\\";
        }
    }
}