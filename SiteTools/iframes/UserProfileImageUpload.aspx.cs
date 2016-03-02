using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SiteTools_iframes_UserProfileImageUpload : System.Web.UI.Page
{
    private ServerSettings _ss = new ServerSettings();

    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated) {
            MemberDatabase member = new MemberDatabase(userID.Name);
            if (!IsPostBack) {
                if (Request.QueryString["uploadComplete"] == "true") {
                    RegisterPostbackScripts.RegisterStartupScript(this, "rootDir='" + Page.ResolveUrl("~/").Replace("/", "") + "';UpdateModal_UploadFinished('" + member.UserId + "', '" + member.AccountImage + "');");
                }
                else if (Request.QueryString["uploadComplete"] == "false") {
                    lbl_error.Enabled = true;
                    lbl_error.Visible = true;
                }

                CustomFonts.SetCustomValues(this.Page, member);
            }

            if (!string.IsNullOrEmpty(member.AccountImage)) {
                img_UserImage.ImageUrl = ResolveUrl("~/Standard_Images/AcctImages/" + member.UserId + "/" + member.AccountImage);
            }
            else {
                img_UserImage.ImageUrl = ResolveUrl("~/Standard_Images/EmptyUserImg.png");
            }
        }
        else {
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
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

                    Response.Redirect("~/SiteTools/iframes/UserProfileImageUpload.aspx?uploadComplete=true");
                }
            }
            catch (Exception ex) {
                // AppLog.AddError(ex);
            }
        }

        Response.Redirect("~/SiteTools/iframes/UserProfileImageUpload.aspx?uploadComplete=false");
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
            string defaultImg = ResolveUrl("~/Standard_Images/EmptyUserImg.png");

            img_UserImage.ImageUrl = ResolveUrl("~/Standard_Images/EmptyUserImg.png");
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