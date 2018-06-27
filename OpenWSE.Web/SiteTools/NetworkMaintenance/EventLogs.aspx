<%@ Page Title="Event Logs" Async="true" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="EventLogs.aspx.cs" Inherits="SiteTools_EventLogs" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            Event Logs
        </div>
        <div class="title-line"></div>
    </div>
    <asp:Panel ID="pnlLinkBtns" runat="server">
    </asp:Panel>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hf_updateIgnore" runat="server" OnValueChanged="hf_updateIgnore_Changed" />
            <asp:HiddenField ID="hf_updateRefreshOnError" runat="server" OnValueChanged="hf_updateRefreshOnError_Changed" />
            <asp:HiddenField ID="hf_updateAllow" runat="server" OnValueChanged="hf_updateAllow_Changed" />
            <asp:HiddenField ID="hf_deleteError" runat="server" OnValueChanged="hf_deleteError_Changed" />
            <asp:HiddenField ID="hf_resetHitCount" runat="server" OnValueChanged="hf_resetHitCount_Changed" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <div id="log" class="pnl-section">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                    <ContentTemplate>
                        <div class="clear">
                        </div>
                        <div class="float-left">
                            <div class="float-left">
                                <div class="float-left pad-right">
                                    <span class="td-allow-btn float-left"></span><b style="line-height: 22px;">Allowed</b>
                                    <div class="clear"></div>
                                </div>
                                <div class="float-left pad-left">
                                    <span class="td-ignore-btn float-left"></span><b style="line-height: 22px;">Ignored</b>
                                    <div class="clear"></div>
                                </div>
                            </div>
                            <div class="float-left pad-left-big margin-left-big pad-top">
                                <asp:CheckBox ID="cb_ViewErrorsOnly" ClientIDMode="Static" runat="server" Text="&nbsp;View errors only" AutoPostBack="true" OnCheckedChanged="cb_ViewErrorsOnly_CheckedChanged" />
                            </div>
                            <div class="float-left pad-left-big margin-left-big pad-top">
                                <asp:CheckBox ID="cb_ViewMoreDetails" ClientIDMode="Static" runat="server" Text="&nbsp;View more details" AutoPostBack="true" OnCheckedChanged="cb_ViewMoreDetails_CheckedChanged" />
                            </div>
                        </div>
                        <div class="float-right pad-top">
                            <asp:LinkButton ID="lbtn_refresherrors" runat="server" OnClick="lbtn_refresherrors_Click" Text="Refresh" CssClass="RandomActionBtns margin-left-big"></asp:LinkButton>
                        </div>
                        <div class="float-right pad-top">
                            <a onclick="OpenLogFolderModal();return false;" class="margin-left-big">View Error Folder</a>
                            <asp:LinkButton ID="LinkButton1" runat="server" CssClass="margin-left-big" OnClientClick="return ConfirmClearLogAll(this);">Clear Events</asp:LinkButton>
                        </div>
                        <div class="clear-space">
                        </div>
                        <asp:Panel ID="pnl_gridviewrequests" runat="server"></asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbtn_refresherrors" />
                    </Triggers>
                </asp:UpdatePanel>
                <div id="logFolder-element" class="Modal-element">
                    <div class="Modal-overlay">
                        <div class="Modal-element-align">
                            <div class="Modal-element-modal" data-setwidth="450">
                                <div class="ModalHeader">
                                    <div>
                                        <div class="app-head-button-holder-admin">
                                            <a href="#close" onclick="openWSE.LoadModalWindow(false, 'logFolder-element', '');return false;" class="ModalExitButton"></a>
                                        </div>
                                        <span class="Modal-title"></span>
                                    </div>
                                </div>
                                <asp:UpdatePanel ID="updatepnl_logfolderHolder" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:HiddenField ID="hf_logfolder" runat="server" ClientIDMode="Static" OnValueChanged="hf_logfolder_ValueChanged" />
                                        <asp:HiddenField ID="hf_DeleteFile" runat="server" ClientIDMode="Static" OnValueChanged="hf_DeleteFile_ValueChanged" />
                                        <div class="ModalScrollContent">
                                            <div class="ModalPadContent">
                                                <asp:LinkButton ID="LinkButton2" runat="server" CssClass="float-right" OnClientClick="return ConfirmClearLogFolder(this);">Delete All Error Files</asp:LinkButton>
                                                <div class="clear-space"></div>
                                                <asp:Panel ID="pnl_logfolderHolder" runat="server" CssClass="file-section">
                                                </asp:Panel>
                                            </div>
                                        </div>
                                        <div class="ModalButtonHolder">
                                            <input type="button" value="Close" onclick="openWSE.LoadModalWindow(false, 'logFolder-element', '');" class="input-buttons modal-cancel-btn" />
                                        </div>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="LinkButton2" />
                                        <asp:AsyncPostBackTrigger ControlID="hf_logfolder" />
                                        <asp:AsyncPostBackTrigger ControlID="hf_DeleteFile" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
                <div id="logFile-element" class="Modal-element">
                    <div class="Modal-overlay">
                        <div class="Modal-element-align">
                            <div class="Modal-element-modal" data-setwidth="1000">
                                <div class="ModalHeader">
                                    <div>
                                        <div class="app-head-button-holder-admin">
                                            <a href="#close" onclick="openWSE.LoadModalWindow(false, 'logFile-element', '');openWSE.LoadModalWindow(true, 'logFolder-element', 'Error Folder');return false;" class="ModalExitButton"></a>
                                        </div>
                                        <span class="Modal-title"></span>
                                    </div>
                                </div>
                                <asp:UpdatePanel ID="updatepnl_FileContent" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:HiddenField ID="hf_FileContent" runat="server" ClientIDMode="Static" OnValueChanged="hf_FileContent_ValueChanged" />
                                        <div class="ModalScrollContent">
                                            <div class="ModalPadContent">
                                                <asp:Panel ID="pnl_fileContent" runat="server" CssClass="file-content">
                                                </asp:Panel>
                                            </div>
                                        </div>
                                        <div class="ModalButtonHolder">
                                            <input type="button" value="Close" onclick="openWSE.LoadModalWindow(false, 'logFile-element', ''); openWSE.LoadModalWindow(true, 'logFolder-element', 'Error Folder');" class="input-buttons modal-cancel-btn" />
                                        </div>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="hf_FileContent" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div id="ignore" class="pnl-section" style="display: none">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                    <ContentTemplate>
                        <div class="clear">
                        </div>
                        <asp:LinkButton ID="lbtn_refreshignore" runat="server" OnClick="lbtn_refreshignore_Click" Text="Refresh" CssClass="RandomActionBtns float-right margin-left-big"></asp:LinkButton>
                        <asp:LinkButton ID="lbtnClearAllIgnored" runat="server" CssClass="float-right" OnClientClick="return ConfirmClearAllIgnored(this);">Clear All Ignored</asp:LinkButton>
                        <div class="clear-space">
                        </div>
                        <asp:Panel ID="pnl_gridviewignore" runat="server"></asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbtn_refreshignore" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                Sorted from newest to oldest Ignore certain requests from being recorded if it contains or equals a certain event momment.
            </div>
        </div>
    </div>
    <div id="networkSettings" runat="server" clientidmode="Static" class="pnl-section" style="display: none;">
        <asp:UpdatePanel ID="UpdatePanel8" runat="server">
            <ContentTemplate>
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        Record Network Activity
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <div class="field switch inline-block">
                            <asp:RadioButton ID="cb_netactOn" runat="server" Text="On" CssClass="RandomActionBtns cb-enable"
                                OnCheckedChanged="cb_netactOn_CheckedChanged" AutoPostBack="True" />
                            <asp:RadioButton ID="cb_netactOff" runat="server" Text="Off" CssClass="RandomActionBtns cb-disable"
                                OnCheckedChanged="cb_netactOff_CheckedChanged" AutoPostBack="True" />
                        </div>
                    </div>
                    <div class="td-settings-desc">
                        Disable this feature if you dont want the website to track errors and Sql database changes.
                    </div>
                </div>
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        Recorded Events to Keep
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <asp:Panel ID="pnl_rtk_E" runat="server" DefaultButton="btn_Update_rtk_E">
                            <asp:TextBox ID="tb_Records_to_keep_E" runat="server" CssClass="textEntry margin-right"
                                Width="65px" TextMode="Number" min="0"></asp:TextBox><asp:Button ID="btn_Update_rtk_E" runat="server" CssClass="input-buttons RandomActionBtns"
                                    Text="Update" OnClick="btn_Update_rtk_E_Clicked" />
                        </asp:Panel>
                        <div class="clear"></div>
                    </div>
                    <div class="td-settings-desc">
                        Change the number of events to keep when events are recorded to the Activity log. Older events will be deleted first before adding new ones.
                    </div>
                </div>
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        Disable Javascript Error Alerts
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <div class="field switch inline-block">
                            <asp:RadioButton ID="rb_DisableJavascriptErrorAlerts_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                OnCheckedChanged="rb_DisableJavascriptErrorAlerts_on_CheckedChanged" AutoPostBack="True" />
                            <asp:RadioButton ID="rb_DisableJavascriptErrorAlerts_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                OnCheckedChanged="rb_DisableJavascriptErrorAlerts_off_CheckedChanged" AutoPostBack="True" />
                        </div>
                    </div>
                    <div class="td-settings-desc">
                        Set this to no to allow the javascript to notify errors to the user .
                    </div>
                </div>
                <asp:Panel ID="pnl_RecordLogFile" runat="server">
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Record Errors Only
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="field switch inline-block">
                                <asp:RadioButton ID="rb_RecordErrorsOnly_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                    OnCheckedChanged="rb_RecordErrorsOnly_on_CheckedChanged" AutoPostBack="True" />
                                <asp:RadioButton ID="rb_RecordErrorsOnly_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                    OnCheckedChanged="rb_RecordErrorsOnly_off_CheckedChanged" AutoPostBack="True" />
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Enable this feature if you only want to record errors.
                        </div>
                    </div>
                    <div class="table-settings-box">
                        <div class="td-settings-title">
                            Record Errors To Log File
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="field switch inline-block">
                                <asp:RadioButton ID="rb_recordLogFile_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                    OnCheckedChanged="rb_recordLogFile_on_CheckedChanged" AutoPostBack="True" />
                                <asp:RadioButton ID="rb_recordLogFile_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                    OnCheckedChanged="rb_recordLogFile_off_CheckedChanged" AutoPostBack="True" />
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Disable this feature if you dont want to save a physical log file to the Logging folder.
                        </div>
                    </div>
                </asp:Panel>
                <div id="tableEmailAct" runat="server" class="table-settings-box">
                    <div class="td-settings-title">
                        Allow Notification on Errors
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <div class="field switch inline-block">
                            <asp:RadioButton ID="cb_emailon" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                OnCheckedChanged="cb_emailon_CheckedChanged" AutoPostBack="True" />
                            <asp:RadioButton ID="cb_emailoff" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                OnCheckedChanged="cb_emailoff_CheckedChanged" AutoPostBack="True" />
                        </div>
                    </div>
                    <div class="td-settings-desc">
                        Disable this feature if you dont want users to have the ability to get notifications on errors. (Users will have to enable the Error Report notification in their account settings if set to Yes)
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
