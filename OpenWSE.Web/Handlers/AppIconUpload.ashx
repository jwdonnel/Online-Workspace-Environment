<%@ WebHandler Language="C#" Class="AppIconUpload" %>

using System;
using System.Web;
using System.Security.Principal;
using System.IO;

public class AppIconUpload : IHttpHandler {
    
    private HttpContext Context;
    private HttpRequest Request;
    private HttpResponse Response;
    private readonly ImageConverter _imageConverter = new ImageConverter();
    
    public void ProcessRequest (HttpContext context) {
        GetSiteRequests.AddRequest();

        Context = context;
        Request = context.Request;
        Response = context.Response;
        
        IIdentity userID = HttpContext.Current.User.Identity;
        if (userID.IsAuthenticated && Request.Form != null && Request.Form["TableID"] != null && Request.Form["TableType"] != null) {
            UploadFile(Request.Form["TableID"], Request.Form["TableType"], userID.Name);
        }

        Response.Write("");
        Response.Flush();
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

    private void UploadFile(string tableId, string tableType, string username) {
        try {
            HttpPostedFile fileData = Request.Files["Filedata"];
            if (fileData != null && fileData.ContentLength > 0) {
                if (IsFileTypeOk(fileData.FileName)) {
                    string appId = string.Empty;
                    if (tableType == "CustomTables") {
                        CustomTableViewer ctv = new CustomTableViewer(username);
                        appId = ctv.GetAppIdByTableID(tableId);
                    }
                    else if (tableType == "TableImporter") {
                        DBImporter dbImporter = new DBImporter();
                        DBImporter_Coll coll = dbImporter.GetImportTableByTableId(tableId);
                        if (!string.IsNullOrEmpty(coll.TableID)) {
                            appId = "app-" + coll.TableID;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(appId)) {
                        App apps = new App(username);
                        Apps_Coll appItem = apps.GetAppInformation(appId);
                        if (!string.IsNullOrEmpty(appItem.ID)) {
                            string currentIcon = appItem.Icon;
                            if (currentIcon.ToLower() == App.DefaultAppIconLocation.ToLower() + App.DefaultAppIcon.ToLower() || currentIcon.ToLower() == App.DefaultAppIconLocation.ToLower() + DBImporter.DefaultDatabaseIcon.ToLower()) {
                                FileInfo fi = new FileInfo(fileData.FileName);
                                currentIcon = App.CreateFullAppIconPath(appItem.filename, appItem.AppId.Replace("app-", string.Empty) + fi.Extension);
                            }

                            string appIconName = App.GetAppIconNameOnly(currentIcon);
                            string filePath = ServerSettings.GetServerMapLocation + "\\" + currentIcon.Replace("/", "\\");
                            if (_imageConverter.SaveNewImg(fileData, filePath)) {
                                apps.UpdateAppImage(appId, appIconName);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
    }
    
    private bool IsFileTypeOk(string fileName) {
        FileInfo fi = new FileInfo(fileName);
        string extension = fi.Extension.ToLower();
        if ((extension == ".png") || (extension == ".jpeg") || (extension == ".jpg") || (extension == ".gif")) {
            return true;
        }
        return false;
    }

}
