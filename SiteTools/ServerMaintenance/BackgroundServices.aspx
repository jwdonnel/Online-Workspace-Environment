<%@ Page Title="Background Services" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="BackgroundServices.aspx.cs" Inherits="SiteTools_BackgroundServices" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <asp:Panel ID="pnl_UploadBackgroundService" runat="server">
            <a href="#" class="margin-right-big input-buttons-create float-left" onclick="openWSE.LoadModalWindow(true, 'UploadService-element', 'Background Service Upload');return false;">Service Upload</a>
            <div class="searchwrapper float-left" style="width: 350px; margin-top: 3px;">
                <asp:UpdatePanel ID="updatepnl_search" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_Search" runat="server" DefaultButton="imgbtn_search">
                            <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                                onfocus="if(this.value=='Search Background Services')this.value=''" onblur="if(this.value=='')this.value='Search Background Services'"
                                Text="Search Background Services" data-defaultvalue="Search Background Services"></asp:TextBox>
                            <asp:LinkButton ID="imgbtn_clearsearch" runat="server" ToolTip="Clear search" CssClass="searchbox_clear RandomActionBtns"
                                OnClick="imgbtn_clearsearch_Click" />
                            <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                OnClick="imgbtn_search_Click" />
                        </asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="imgbtn_clearsearch" />
                        <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="clear"></div>
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
                                    You can upload your own custom background service to this site. Zip files are allowed as long as the zip file has the valid dll service. In order for this service to upload and run properly, you must compile your service in C# .Net 4.5. Also, you will need to include the OpenWSE_Library dll in your solution as you will need to inherit some classes from the OpenWSE_Library.Core.BackgroundServices. Failure to to do this will result in the upload being rejected.<br />
                                    <a href="#" onclick="openWSE.LoadIFrameContent('HelpPages/backgroundservices/HowToCreateService.html', this);return false;" class="font-bold">Click Here</a> to see how to create your own Background Service in Visual Studio.
                                    <div class="clear-space"></div>
                                    <div class="clear-space"></div>
                                    <asp:TextBox ID="txt_uploadDllName" runat="server" placeholder="Service Name" CssClass="textEntry" MaxLength="250"></asp:TextBox>
                                    <div class="clear-space"></div>
                                    <asp:TextBox ID="txt_uploadDllDescription" runat="server" placeholder="Description" CssClass="textEntry" MaxLength="1000" Width="640px"></asp:TextBox>
                                    <div class="clear-space"></div>
                                    <asp:CheckBox ID="cb_autoStartService" runat="server" Text="&nbsp;Auto start service on load" Checked="true" />
                                    <div class="clear-space"></div>
                                    <asp:CheckBox ID="cb_logService" runat="server" Text="&nbsp;Log service states" Checked="false" />
                                    <div class="clear-space"></div>
                                    <div class="clear-space"></div>
                                    <asp:FileUpload ID="fu_newDllFile" runat="server" />
                                    <div class="clear-space">
                                    </div>
                                </div>
                            </div>
                            <div class="ModalButtonHolder">
                                <asp:Button ID="btn_uploadFile" runat="server" CssClass="input-buttons float-left no-margin" OnClick="btn_uploadFile_Click" Text="Upload" />
                                <input type="button" value="Close" class="input-buttons float-right no-margin" onclick="openWSE.LoadModalWindow(false, 'UploadService-element', '');" />
                                <div class="clear"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
        <asp:UpdatePanel ID="updatepnl1" runat="server">
            <ContentTemplate>
                <div class="clear-space"></div>
                <asp:Label ID="lbl_TotalServices" runat="server"></asp:Label>
                <div id="div_backgroundserviceTable" runat="server" class="table-settings-box no-border">
                    <div class="td-settings-ctrl">
                        <asp:LinkButton ID="lbtn_refresh" runat="server" Text="" CssClass="img-refresh float-right RandomActionBtns"
                            OnClick="lbtn_refresh_Click" ToolTip="Refresh"></asp:LinkButton>
                        <div class="clear-space-two">
                        </div>
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
                    </div>
                    <div class="td-settings-desc">
                        Background Services will run until an error occurs or manually stopped. Services running in the background uses more memory on the server and may slow down the site. Stopping a background service may result in other features to stop working. Deleting a service might not delete the actual bin folder location. You might have to do this manually since the site cannot unload that assembly completely, it can only stop it from running.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
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
                </div>
            </div>
        </div>
    </div>
</asp:Content>
