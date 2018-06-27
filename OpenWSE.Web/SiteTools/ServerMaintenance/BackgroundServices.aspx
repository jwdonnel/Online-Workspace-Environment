<%@ Page Title="Background Services" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="BackgroundServices.aspx.cs" Inherits="SiteTools_BackgroundServices" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
    <asp:Panel ID="pnl_UploadBackgroundService" runat="server">
        <div id="UploadService-element" class="Modal-element" style="display: none;">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="700">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'UploadService-element', '');return false;" class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                You can upload your own custom background service to this site. Zip files are allowed as long as the zip file has the valid dll service. In order for this service to upload and run properly, you must compile your service in C# .Net 4.6.1. Also, you will need to include the OpenWSE_Library dll in your solution as you will need to inherit some classes from the OpenWSE_Library.Core.BackgroundServices. Failure to to do this will result in the upload being rejected. Click <a href="#" onclick="openWSE.LoadIFrameContent('HelpPages/backgroundservices/HowToCreateService.html');return false;" class="font-bold">here</a> to see how to create your own Background Service in Visual Studio.
                                <div class="clear-space"></div>
                                <div class="input-settings-holder">
                                    <span class="font-bold">Service Name</span><div class="clear-space-two"></div>
                                    <asp:TextBox ID="txt_uploadDllName" runat="server" CssClass="textEntry" MaxLength="250" Width="100%"></asp:TextBox>
                                </div>
                                <div class="input-settings-holder">
                                    <span class="font-bold">Description</span><div class="clear-space-two"></div>
                                    <asp:TextBox ID="txt_uploadDllDescription" runat="server" CssClass="textEntry" MaxLength="1000" Width="100%"></asp:TextBox>
                                </div>
                                <asp:CheckBox ID="cb_autoStartService" runat="server" Text="&nbsp;Auto start service on load" Checked="true" CssClass="radiobutton-style" />
                                <div class="clear-space"></div>
                                <asp:CheckBox ID="cb_logService" runat="server" Text="&nbsp;Log service states" Checked="false" CssClass="radiobutton-style" />
                                <div class="clear-space"></div>
                                <div class="clear-space"></div>
                                <asp:FileUpload ID="fu_newDllFile" runat="server" />
                                <div class="clear-space">
                                </div>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:Button ID="btn_uploadFile" runat="server" CssClass="input-buttons modal-ok-btn" OnClick="btn_uploadFile_Click" Text="Upload" />
                            <input type="button" value="Close" class="input-buttons modal-cancel-btn" onclick="openWSE.LoadModalWindow(false, 'UploadService-element', '');" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>
    <asp:UpdatePanel ID="updatepnl1" runat="server">
        <ContentTemplate>
            <div id="div_backgroundserviceTable" runat="server" class="table-settings-box">
                <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
                    Background Services
                </div>
                <div class="title-line"></div>
                <div class="td-settings-ctrl">
                    <a class="input-buttons-create" onclick="openWSE.LoadModalWindow(true, 'UploadService-element', 'Background Service Upload');return false;">Service Upload</a>
                    <div class="clear-space"></div>
                    <asp:Panel ID="pnl_BackgroundServiceList" runat="server" ClientIDMode="Static">
                    </asp:Panel>
                    <asp:HiddenField ID="hf_DeleteService" runat="server" ClientIDMode="Static" OnValueChanged="hf_DeleteService_ValueChanged" />
                    <asp:HiddenField ID="hf_StartService" runat="server" ClientIDMode="Static" OnValueChanged="hf_StartService_ValueChanged" />
                    <asp:HiddenField ID="hf_RestartService" runat="server" ClientIDMode="Static" OnValueChanged="hf_RestartService_ValueChanged" />
                    <asp:HiddenField ID="hf_StopService" runat="server" ClientIDMode="Static" OnValueChanged="hf_StopService_ValueChanged" />
                    <asp:HiddenField ID="hf_EditService" runat="server" ClientIDMode="Static" OnValueChanged="hf_EditService_ValueChanged" />
                    <asp:HiddenField ID="hf_UpdateService" runat="server" ClientIDMode="Static" OnValueChanged="hf_UpdateService_ValueChanged" />
                    <asp:HiddenField ID="hf_CancelService" runat="server" ClientIDMode="Static" OnValueChanged="hf_CancelService_ValueChanged" />
                    <asp:HiddenField ID="hf_ServiceNameEdit" runat="server" ClientIDMode="Static" />
                    <asp:HiddenField ID="hf_ServiceDescriptionEdit" runat="server" ClientIDMode="Static" />
                    <asp:HiddenField ID="hf_ServiceLogEdit" runat="server" ClientIDMode="Static" />
                    <asp:HiddenField ID="hf_ServiceAutoStartEdit" runat="server" ClientIDMode="Static" />
                </div>
                <div class="td-settings-desc">
                    Background Services will run until an error occurs or manually stopped. Services running in the background uses more memory on the server and may slow down the site. Stopping a background service may result in other features to stop working. Deleting a service might not delete the actual bin folder location. You might have to do this manually since the site cannot unload that assembly completely, it can only stop it from running.
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div id="ServiceStatus-element" class="Modal-element" style="display: none;">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="700">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#" onclick="CloseServiceSettings();return false;" class="ModalExitButton"></a>
                            </div>
                            <span class="Modal-title"></span>
                        </div>
                    </div>
                    <div class="ModalScrollContent">
                        <div class="ModalPadContent">
                            <div class="clear-margin">
                                <div id="servicesettings_holder"></div>
                            </div>
                        </div>
                    </div>
                    <div class="ModalButtonHolder">
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
