using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using ICSharpCode.SharpZipLib.Zip;
using OpenWSE_Library.Core.BackgroundServices;
using OpenWSE_Tools.BackgroundServiceDatabaseCalls;

public partial class SiteTools_BackgroundServices : BasePage {

    private readonly BackgroundServices _backgroundServices = new BackgroundServices();
    private string EditServiceID {
        get {
            if (ViewState["EditServiceID"] != null) {
                return ViewState["EditServiceID"].ToString();
            }

            return string.Empty;
        }
        set {
            ViewState["EditServiceID"] = value;
        }
    }

    protected void Page_Load(object sender, EventArgs e) {
        if (MainServerSettings.LockBackgroundServices) {
            pnl_UploadBackgroundService.Enabled = false;
            pnl_UploadBackgroundService.Visible = false;
            ltl_locked.Text = HelperMethods.GetLockedByMessage();
            div_backgroundserviceTable.Style.Add("border-bottom", "none !important");
            RegisterPostbackScripts.RegisterStartupScript(this, "backgroundServicesLocked=true;");
        }

        if (!IsPostBack || string.IsNullOrEmpty(AsyncPostBackSourceElementID) || PostbackControlIsUpdateAll()) {
            BuildTable();
        }

        if (!IsPostBack && Request.QueryString["error"] == "true") {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('There was an error uploading the service. Check to make sure your service inherits the correct BackgroundService library from the OpenWSE_Library.dll.');");
        }
    }

    private void BuildTable() {
        List<BackgroundServices_Coll> serviceColl = _backgroundServices.GetBackgroundServices();

        pnl_BackgroundServiceList.Controls.Clear();
        var str = new StringBuilder();

        bool lockBackgroundServices = MainServerSettings.LockBackgroundServices;

        TableBuilder tableBuilder = new TableBuilder(this.Page, true, !lockBackgroundServices && IsUserInAdminRole(), 3);

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Service Name", "175", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Description", "200", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Log", "45", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Auto Start", "45", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Updated", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Updated By", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("State", "90", false));
        if (MainServerSettings.MonitorCpuUsage) {
            headerColumns.Add(new TableBuilderHeaderColumns("CPU", "80", false));
        }
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        int fileCount = 0;
        foreach (BackgroundServices_Coll bs in serviceColl) {
            MemberDatabase tempMember = new MemberDatabase(bs.UpdatedBy);

            string actionBtn = "<span class='state-update-btn' data-id='" + bs.ID + "'>" + GetActionBtn(bs.ID, bs.State, lockBackgroundServices) + "</span>";
            string viewBtn = "<a href='#view' class='td-view-btn' onclick=\"LoadServiceSettings('" + bs.ID + "', true);return false;\" title='View'></a>";
            string editBtn = "<a href='#edit' class='td-edit-btn' onclick=\"EditService('" + bs.ID + "');return false;\" title='Edit'></a>";
            if (lockBackgroundServices) {
                viewBtn = "<a href='#view' class='td-view-btn' onclick=\"LoadServiceSettings('" + bs.ID + "', true);return false;\" title='View'></a>";
                editBtn = string.Empty;
            }

            string deleteBtn = "<a href='#delete' class='td-delete-btn' onclick=\"DeleteService('" + bs.ID + "');return false;\" title='Delete'></a>";
            if (string.IsNullOrEmpty(bs.DLL_Location)) {
                deleteBtn = string.Empty;
            }

            string updateBtn = string.Empty;
            string cancelBtn = string.Empty;
            if (EditServiceID == bs.ID) {
                updateBtn = "<a href='#update' class='td-update-btn' onclick=\"UpdateService('" + bs.ID + "');return false;\" title='Update'></a>";
                cancelBtn = "<a href='#cancel' class='td-cancel-btn' onclick=\"CancelEditService();return false;\" title='Cancel'></a>";
            }
            else if (!string.IsNullOrEmpty(EditServiceID)) {
                actionBtn = string.Empty;
                viewBtn = string.Empty;
                editBtn = string.Empty;
                deleteBtn = string.Empty;
            }

            string logInfoChecked = string.Empty;
            if (bs.LogInformation) {
                logInfoChecked = "checked='checked'";
            }

            string runOnStartChecked = string.Empty;
            if (_backgroundServices.StartServiceOnDefault(bs.Namespace)) {
                runOnStartChecked = "checked='checked'";
            }

            string newNameInput = bs.Name;
            string newDescriptionInput = bs.Description;
            string logInfo = "<input type='checkbox' disabled='disabled' " + logInfoChecked + " />";
            string autoStart = "<input type='checkbox' disabled='disabled' " + runOnStartChecked + " />";
            if (!string.IsNullOrEmpty(updateBtn)) {
                actionBtn = string.Empty;
                newNameInput = "<input type='text' id='tb_newNameEdit' class='textEntry' onkeypress=\"UpdateKeyDownEvent(event, '" + bs.ID + "');\" style='width: 98%;' /><span id='span_newNameEdit' style='display: none;'>" + bs.Name + "</span>";
                newDescriptionInput = "<input type='text' id='tb_newDescriptionEdit' class='textEntry' onkeypress=\"UpdateKeyDownEvent(event, '" + bs.ID + "');\" style='width: 98%;' /><span id='span_newDescriptionEdit' style='display: none;'>" + bs.Description + "</span>";
                logInfo = "<input type='checkbox' id='tb_logInfoEdit' " + logInfoChecked + " />";
                autoStart = "<input type='checkbox' id='tb_autoStartEdit' " + runOnStartChecked + " />";
                viewBtn = string.Empty;
                editBtn = string.Empty;
                deleteBtn = string.Empty;

                RegisterPostbackScripts.RegisterStartupScript(this, "LoadEditControlValues();");
            }

            string state = "<span class='state-" + bs.State.ToString() + "'>" + bs.State.ToString() + "</span>";
            if (string.IsNullOrEmpty(newDescriptionInput)) {
                newDescriptionInput = "No description given";
            }

            fileCount++;
            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();

            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Service Name", newNameInput, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Description", newDescriptionInput, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Log", logInfo, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Auto Start", autoStart, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Date Updated", "<span class='date-holder' data-id='" + bs.ID + "'>" + bs.DateUpdated.ToString() + "</span>", TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Updated By", "<span class='user-holder' data-id='" + bs.ID + "'>" + HelperMethods.MergeFMLNames(tempMember) + "</span>", TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("State", "<span class='state-holder' data-id='" + bs.ID + "'>" + state + "</span>", TableBuilderColumnAlignment.Left));

            if (MainServerSettings.MonitorCpuUsage) {
                string cpuUsageStr = "0%";
                if (bs.Namespace == OpenWSE_Tools.AutoUpdates.AutoUpdateSystem.AutoUpdateServiceNamespace || bs.State == BackgroundStates.Stopped || bs.State == BackgroundStates.Error) {
                    cpuUsageStr = "-"; 
                }
                else {
                    double cpuUsage = _backgroundServices.GetCurrentCpuUsageFromNamespace(bs.Namespace);
                    if (bs.State != BackgroundStates.Stopped && bs.State != BackgroundStates.Error) {
                        cpuUsageStr = Math.Round(cpuUsage, 1).ToString() + "%";
                    }
                }

                bodyColumnValues.Add(new TableBuilderBodyColumnValues("CPU", "<span class='cpu-holder' data-id='" + bs.ID + "'>" + cpuUsageStr + "</span>", TableBuilderColumnAlignment.Left));
            }

            string additionalRowAttributes = "data-namespace='" + bs.Namespace + "' data-serviceid='" + bs.ID + "'";
            string editControls = actionBtn + viewBtn + editBtn + deleteBtn + updateBtn + cancelBtn;

            tableBuilder.AddBodyRow(bodyColumnValues, editControls, additionalRowAttributes, "main-table-rows");
        }
        #endregion

        pnl_BackgroundServiceList.Controls.Clear();
        pnl_BackgroundServiceList.Controls.Add(tableBuilder.CompleteTableLiteralControl("No background services found"));
    }
    private string GetActionBtn(string id, BackgroundStates state, bool lockBackgroundServices) {
        if (!lockBackgroundServices) {
            switch (state) {
                case BackgroundStates.Running:
                case BackgroundStates.Sleeping:
                    return "<a href='#stop' class='state-update-btn img-stop' data-id='" + id + "' onclick=\"UpdateServiceState('" + id + "', '" + state.ToString() + "');return false;\" title='Stop Service' style='padding: 3px;'></a>";

                case BackgroundStates.Stopped:
                    return "<a href='#start' class='state-update-btn img-play' data-id='" + id + "' onclick=\"UpdateServiceState('" + id + "', '" + state.ToString() + "');return false;\" title='Start Service' style='padding: 3px;'></a>";

                case BackgroundStates.Error:
                    return "<a href='#restart' class='state-update-btn img-refresh' data-id='" + id + "' onclick=\"UpdateServiceState('" + id + "', '" + state.ToString() + "');return false;\" title='Restart Service' style='padding: 3px;'></a>";
            }
        }

        return string.Empty;
    }

    protected void hf_StartService_ValueChanged(object sender, EventArgs e) {
        BackgroundServices_Coll bs = _backgroundServices.GetBackgroundService(hf_StartService.Value);
        if (!string.IsNullOrEmpty(bs.ID) && !string.IsNullOrEmpty(bs.Namespace)) {
            // Start the service
            _backgroundServices.UpdateState(bs.ID, bs.Namespace, BackgroundStates.Running, bs.LogInformation, Page.User.Identity.Name.ToLower());

            // Get the background service again
            bs = _backgroundServices.GetBackgroundService(hf_StartService.Value);
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('There was an issue attempting to get the background service from the database. Please try again.');");
        }

        BuildTable();
        hf_StartService.Value = string.Empty;
    }
    protected void hf_RestartService_ValueChanged(object sender, EventArgs e) {
        BackgroundServices_Coll bs = _backgroundServices.GetBackgroundService(hf_RestartService.Value);
        if (!string.IsNullOrEmpty(bs.ID) && !string.IsNullOrEmpty(bs.Namespace)) {
            // Stop the service
            BackgroundStates newState = BackgroundStates.Stopping;
            if (bs.Namespace == "OpenWSE_Tools.AppServices.AutoBackupSystem") {
                newState = BackgroundStates.Stopped;
            }

            _backgroundServices.UpdateState(bs.ID, bs.Namespace, newState, bs.LogInformation, Page.User.Identity.Name.ToLower());

            if (newState == BackgroundStates.Stopping) {
                while (_backgroundServices.GetCurrentStateFromNamespace(bs.Namespace) == BackgroundStates.Stopping) {
                    Thread.Sleep(500);
                }
            }

            // Start the service
            _backgroundServices.UpdateState(bs.ID, bs.Namespace, BackgroundStates.Running, bs.LogInformation, Page.User.Identity.Name.ToLower());

            // Get the background service again
            bs = _backgroundServices.GetBackgroundService(hf_RestartService.Value);
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('There was an issue attempting to get the background service from the database. Please try again.');");
        }

        BuildTable();
        hf_RestartService.Value = string.Empty;
    }
    protected void hf_StopService_ValueChanged(object sender, EventArgs e) {
        BackgroundServices_Coll bs = _backgroundServices.GetBackgroundService(hf_StopService.Value);
        if (!string.IsNullOrEmpty(bs.ID) && !string.IsNullOrEmpty(bs.Namespace)) {
            // Stop the service
            BackgroundStates newState = BackgroundStates.Stopping;
            if (bs.Namespace == "OpenWSE_Tools.AppServices.AutoBackupSystem" || bs.Namespace == "OpenWSE_Tools.AutoUpdates.AutoUpdateSystem") {
                newState = BackgroundStates.Stopped;
            }

            _backgroundServices.UpdateState(bs.ID, bs.Namespace, newState, bs.LogInformation, Page.User.Identity.Name.ToLower());

            // Get the background service again
            bs = _backgroundServices.GetBackgroundService(hf_StopService.Value);
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('There was an issue attempting to get the background service from the database. Please try again.');");
        }

        BuildTable();
        hf_StopService.Value = string.Empty;
    }

    protected void hf_EditService_ValueChanged(object sender, EventArgs e) {
        EditServiceID = hf_EditService.Value;
        BuildTable();

        hf_EditService.Value = string.Empty;
    }
    protected void hf_DeleteService_ValueChanged(object sender, EventArgs e) {
        BackgroundServices_Coll bs = _backgroundServices.GetBackgroundService(hf_DeleteService.Value);
        if (!string.IsNullOrEmpty(bs.DLL_Location)) {
            // Stop the service
            if (bs.State == BackgroundStates.Running || bs.State == BackgroundStates.Sleeping) {
                _backgroundServices.UpdateState(bs.ID, bs.Namespace, BackgroundStates.Stopping, false, CurrentUsername);
                while (_backgroundServices.GetCurrentStateFromNamespace(bs.Namespace) == BackgroundStates.Stopping) {
                    Thread.Sleep(500);
                }
            }

            // Delete the background service dll
            string binFolder = ServerSettings.GetServerMapLocation + "Bin\\" + bs.DLL_Location;
            if (Directory.Exists(binFolder) && binFolder != ServerSettings.GetServerMapLocation + "Bin\\") {
                try {
                    GC.Collect(); // collects all unused memory
                    GC.WaitForPendingFinalizers(); // wait until GC has finished its work
                    GC.Collect();
                }
                catch { }

                try {
                    ServerSettings.RemoveRuntimeAssemblyBinding("Bin\\" + bs.DLL_Location);
                    Thread.Sleep(2000);
                    Directory.Delete(binFolder, true);
                }
                catch { }
            }

            _backgroundServices.DeleteItem(bs.ID);
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('This background service cannot be deleted.');");
        }

        BuildTable();
        hf_DeleteService.Value = string.Empty;
    }

    protected void hf_UpdateService_ValueChanged(object sender, EventArgs e) {
        if (EditServiceID == hf_UpdateService.Value) {
            string newName = hf_ServiceNameEdit.Value.Trim();
            string newDescription = hf_ServiceDescriptionEdit.Value.Trim();
            string logInfo = hf_ServiceLogEdit.Value.Trim();
            string autoStart = BackgroundStates.Stopped.ToString();
            if (HelperMethods.ConvertBitToBoolean(hf_ServiceAutoStartEdit.Value.Trim())) {
                autoStart = BackgroundStates.Running.ToString();

                // Start the service
                BackgroundServices_Coll bs = _backgroundServices.GetBackgroundService(hf_UpdateService.Value);
                if (!string.IsNullOrEmpty(bs.ID) && !string.IsNullOrEmpty(bs.Namespace) && bs.State == BackgroundStates.Stopped) {
                    // Start the service
                    _backgroundServices.UpdateState(bs.ID, bs.Namespace, BackgroundStates.Running, bs.LogInformation, Page.User.Identity.Name.ToLower());

                    // Get the background service again
                    bs = _backgroundServices.GetBackgroundService(hf_UpdateService.Value);
                }
            }

            if (!string.IsNullOrEmpty(newName)) {
                _backgroundServices.UpdateItem(EditServiceID, newName, newDescription, HelperMethods.ConvertBitToBoolean(logInfo), autoStart, Page.User.Identity.Name.ToLower());
            }
        }

        EditServiceID = string.Empty;
        BuildTable();

        hf_UpdateService.Value = string.Empty;
    }
    protected void hf_CancelService_ValueChanged(object sender, EventArgs e) {
        EditServiceID = string.Empty;
        BuildTable();

        hf_CancelService.Value = string.Empty;
    }

    protected void btn_uploadFile_Click(object sender, EventArgs e) {
        if (fu_newDllFile.HasFile) {
            string dllFolder = HelperMethods.RandomString(10);
            string filePath = ServerSettings.GetServerMapLocation + "Bin\\" + dllFolder + "\\";

            string serviceName = txt_uploadDllName.Text.Trim();
            string serviceDescription = txt_uploadDllDescription.Text.Trim();
            if (string.IsNullOrEmpty(serviceDescription)) {
                serviceDescription = "No description available";
            }

            BackgroundStates currentState = BackgroundStates.Stopped;
            if (cb_autoStartService.Checked) {
                currentState = BackgroundStates.Running;
            }

            string fileName = fu_newDllFile.PostedFile.FileName;
            FileInfo fi = new FileInfo(fileName);

            #region ZIP FILES
            if (fi.Extension.ToLower() == ".zip" && HasAtLeastOneValidFile(fu_newDllFile.FileContent)) {
                fu_newDllFile.FileContent.Position = 0;
                CreateBinDirectory(filePath, dllFolder);

                bool foundServiceNamespace = false;
                using (Stream fileStreamIn = fu_newDllFile.FileContent) {
                    using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn)) {
                        ZipEntry entry;
                        string tmpEntry = String.Empty;

                        try {
                            while ((entry = zipInStream.GetNextEntry()) != null) {
                                string fn = Path.GetFileName(entry.Name);
                                if (string.IsNullOrEmpty(fn) || fn.ToLower() == "openwse_library.dll") {
                                    continue;
                                }

                                string fileExt = new FileInfo(fn).Extension.ToLower();
                                if (fileExt == ".dll" || fileExt == ".compiled" || fileExt == ".ini" || fileExt == ".xml" || fileExt == ".txt") {

                                    string fullPath = filePath + new FileInfo(fn).Name;
                                    fullPath = fullPath.Replace("\\ ", "\\").Replace("/", "\\");
                                    FileStream streamWriter = File.Create(fullPath);

                                    int size = 2048;
                                    byte[] data = new byte[2048];
                                    while (true) {
                                        size = zipInStream.Read(data, 0, data.Length);
                                        if (size > 0)
                                            streamWriter.Write(data, 0, size);
                                        else
                                            break;
                                    }

                                    streamWriter.Close();

                                    if (fileExt == ".dll") {
                                        using (StreamReader streamReader = new StreamReader(fullPath)) {
                                            string serviceNamespace = BackgroundServiceCalls.GetServiceStreamNamespace(streamReader.BaseStream);
                                            if (!string.IsNullOrEmpty(serviceNamespace)) {
                                                if (string.IsNullOrEmpty(serviceName) && serviceNamespace.LastIndexOf('.') != -1) {
                                                    serviceName = serviceNamespace.Substring(serviceNamespace.LastIndexOf('.') + 1);
                                                }
                                                _backgroundServices.AddItem(serviceName, serviceDescription, currentState, serviceNamespace, dllFolder, cb_logService.Checked, CurrentUsername);
                                                foundServiceNamespace = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }

                if (!foundServiceNamespace) {
                    if (Directory.Exists(filePath) && fileName != ServerSettings.GetServerMapLocation + "Bin\\") {
                        ServerSettings.RemoveRuntimeAssemblyBinding("Bin\\" + dllFolder);
                        try {
                            Directory.Delete(filePath, true);
                        }
                        catch { }
                    }

                    ServerSettings.PageIFrameRedirect(Page, "BackgroundServices.aspx?error=true");
                }

                ServerSettings.PageIFrameRedirect(Page, "BackgroundServices.aspx?uploadSuccess=true");
            }
            #endregion

            #region DLL FILES
            else if (fi.Extension.ToLower() == ".dll") {
                string serviceNamespace = BackgroundServiceCalls.GetServiceStreamNamespace(fu_newDllFile.FileContent);
                if (!string.IsNullOrEmpty(serviceNamespace) && fu_newDllFile.FileContent.Length > 0) {
                    CreateBinDirectory(filePath, dllFolder);
                    fu_newDllFile.SaveAs(filePath + fu_newDllFile.FileName);
                    if (string.IsNullOrEmpty(serviceName) && serviceNamespace.LastIndexOf('.') != -1) {
                        serviceName = serviceNamespace.Substring(serviceNamespace.LastIndexOf('.') + 1);
                    }
                    _backgroundServices.AddItem(serviceName, serviceDescription, currentState, serviceNamespace, dllFolder, cb_logService.Checked, CurrentUsername);
                    ServerSettings.PageIFrameRedirect(Page, "BackgroundServices.aspx?uploadSuccess=true");
                }
            }
            #endregion

            ServerSettings.PageIFrameRedirect(Page, "BackgroundServices.aspx?error=true");
        }

        ServerSettings.PageIFrameRedirect(Page, "BackgroundServices.aspx");
    }

    private bool HasAtLeastOneValidFile(Stream str) {
        bool returnVal = false;
        ZipInputStream zipInStream = new ZipInputStream(str);
        ZipEntry entry;
        while ((entry = zipInStream.GetNextEntry()) != null) {
            string fn = Path.GetFileName(entry.Name);
            var tempfi = new FileInfo(fn);
            if (new FileInfo(fn).Extension.ToLower() == ".dll") {
                returnVal = true;
                break;
            }
        }

        return returnVal;
    }
    private void CreateBinDirectory(string filePath, string dllFolder) {
        if (!Directory.Exists(filePath)) {
            Directory.CreateDirectory(filePath);
            ServerSettings.AddRuntimeAssemblyBinding("Bin\\" + dllFolder);
        }
    }

}