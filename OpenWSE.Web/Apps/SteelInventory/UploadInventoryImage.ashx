<%@ WebHandler Language="C#" Class="UploadInventoryImage" %>

using System;
using System.Web;
using System.IO;

public class UploadInventoryImage : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
        string result = "false";
        if (context.User.Identity.IsAuthenticated && context.Request.Files.Count > 0) {
            for (int i = 0; i < context.Request.Files.Count; i++) {
                HttpPostedFile postedFile = context.Request.Files[i];
                if (!Directory.Exists(ServerSettings.GetServerMapLocation + "Apps\\SteelInventory\\Images")) {
                    Directory.CreateDirectory(ServerSettings.GetServerMapLocation + "Apps\\SteelInventory\\Images");
                }

                FileInfo fi = new FileInfo(postedFile.FileName);
                if (HelperMethods.IsImageFileType(fi.Extension)) {
                    try {
                        postedFile.SaveAs(ServerSettings.GetServerMapLocation + "Apps\\SteelInventory\\Images\\" + fi.Name);
                        result = "true";
                    }
                    catch (Exception e) {
                        AppLog.AddError(e);
                    }
                }
            }
        }

        context.Response.Write(result);
        context.ApplicationInstance.CompleteRequest();
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}