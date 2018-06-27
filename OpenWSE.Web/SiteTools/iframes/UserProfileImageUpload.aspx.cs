using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SiteTools_iframes_UserProfileImageUpload : Page
{
    private ServerSettings _ss = new ServerSettings();

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (userId.IsAuthenticated) {
            MemberDatabase member = new MemberDatabase(userId.Name);
            if (!IsPostBack) {
                if (Request.QueryString["uploadComplete"] == "true") {
                    RegisterPostbackScripts.RegisterStartupScript(this, "rootDir='" + Page.ResolveUrl("~/").Replace("/", "") + "';UpdateModal_UploadFinished('" + member.UserId + "', '" + member.AccountImage + "');");
                }
                else if (Request.QueryString["uploadComplete"] == "false") {
                    lbl_error.Enabled = true;
                    lbl_error.Visible = true;
                }

                BaseMaster baseMaster = new BaseMaster();
                baseMaster.LoadAllDefaultScriptsStyleSheets(Page, member);
            }

            if (!string.IsNullOrEmpty(member.AccountImage)) {
                if (member.AccountImage.ToLower().Contains("http") || member.AccountImage.StartsWith("//") || member.AccountImage.ToLower().Contains("www.")) {
                    img_UserImage.ImageUrl = member.AccountImage;
                }
                else {
                    if (File.Exists(ServerSettings.GetServerMapLocation + ServerSettings.AccountImageLoc.Replace("~/", string.Empty) + member.UserId + "/" + member.AccountImage)) {
                        img_UserImage.ImageUrl = ServerSettings.AccountImageLoc + member.UserId + "/" + member.AccountImage;
                    }
                    else {
                        img_UserImage.ImageUrl = ResolveUrl("~/App_Themes/" + member.SiteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
                    }
                }
            }
            else {
                img_UserImage.ImageUrl = ResolveUrl("~/App_Themes/" + member.SiteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
            }

            lbl_message.Text = string.Empty;
            lbl_message.Enabled = false;
            lbl_message.Visible = false;
        }
        else {
            HelperMethods.PageRedirect("~/ErrorPages/Blocked.html");
        }
    }

    protected void btn_UploadImages_Click(object sender, EventArgs e) {
        if (fileUpload_acctImage.HasFile) {
            try {
                if (HelperMethods.IsImage(fileUpload_acctImage.PostedFile)) {
                    MemberDatabase member = new MemberDatabase(HttpContext.Current.User.Identity.Name);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileUpload_acctImage.FileName).ToLower();
                    string imgPath = ServerSettings.AccountImageServerLoc + member.UserId;
                    if (!Directory.Exists(imgPath)) {
                        Directory.CreateDirectory(imgPath);
                    }
                    else {
                        string[] files = Directory.GetFiles(imgPath);
                        foreach (string file in files) {
                            try {
                                File.Delete(file);
                            }
                            catch { }
                        }
                    }

                    ImageConverter ic = new ImageConverter();
                    ic.NeedRoundCorners = false;
                    ic.SaveNewImg(fileUpload_acctImage.PostedFile, imgPath + "\\" + fileName);
                    member.UpdateAcctImage(fileName);

                    HelperMethods.PageRedirect("~/SiteTools/iframes/UserProfileImageUpload.aspx?uploadComplete=true", null);
                }
            }
            catch {
                // AppLog.AddError(ex);
            }
        }

        HelperMethods.PageRedirect("~/SiteTools/iframes/UserProfileImageUpload.aspx?uploadComplete=false");
    }

    protected void lnkBtn_clearImage_Click(object sender, EventArgs e) {
        MemberDatabase member = new MemberDatabase(HttpContext.Current.User.Identity.Name);
        try {
            string imgPath = ServerSettings.AccountImageServerLoc + member.UserId;
            if (Directory.Exists(imgPath)) {
                string[] fileList = Directory.GetFiles(imgPath);
                foreach (string file in fileList) {
                    File.Delete(file);
                }
            }

            member.UpdateAcctImage(string.Empty);
            if (member.IsSocialAccount) {
                lbl_message.Text = "Log out and log back in to get social network profile image.";
                lbl_message.Enabled = true;
                lbl_message.Visible = true;
            }

            string defaultImg = ResolveUrl("~/App_Themes/" + member.SiteTheme + "/Icons/SiteMaster/EmptyUserImg.png");

            img_UserImage.ImageUrl = ResolveUrl("~/App_Themes/" + member.SiteTheme + "/Icons/SiteMaster/EmptyUserImg.png");
            RegisterPostbackScripts.RegisterStartupScript(this, "UpdateModal('" + defaultImg + "');");
            lbl_error.Enabled = false;
            lbl_error.Visible = false;
        }
        catch (Exception ex) {
            AppLog.AddError(ex);
            lbl_error.Enabled = true;
            lbl_error.Visible = true;
        }
    }
}