<%@ page title="Site Plugins" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_SitePlugins, App_Web_rytka2m4" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        #editor
        {
            height: 195px;
            width: 565px;
            font-size: 14px;
        }

        #updateEditor
        {
            height: 150px;
            width: 98%;
            font-size: 14px;
            position: relative;
        }

        #MainContent_radioButton_FileList td, #MainContent_checkbox_FileList td
        {
            padding: 3px 0;
        }

        #MainContent_radioButton_FileList input, #MainContent_checkbox_FileList input
        {
            float: left;
            margin-right: 2px;
            margin-top: 1px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <asp:Panel ID="pnl_AddControls" runat="server" ClientIDMode="Static">
            <div class="table-settings-box">
                <div class="td-settings-ctrl">
                    <div id="UploadPlugin">
                        <div class="float-left settings-name-column">
                            Upload Plugin
                        </div>
                        <asp:TextBox ID="txt_uploadPluginName" runat="server" CssClass="textEntry margin-right"
                            Text="Plugin Name" Width="150px" onfocus="if(this.value=='Plugin Name')this.value=''"
                            onblur="if(this.value=='')this.value='Plugin Name'" MaxLength="250"></asp:TextBox>
                        <asp:CheckBox ID="cb_enableUpload" runat="server" Text="&nbsp;Enable Plugin" CssClass="margin-right-big" />
                        <asp:CheckBox ID="cb_installAfter" ClientIDMode="Static" runat="server" Text="&nbsp;Install after upload" />
                        <asp:UpdatePanel ID="updatepnl_Add" runat="server">
                            <ContentTemplate>
                                <asp:HiddenField ID="hf_deletePlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_deletePlugin_Changed" />
                                <asp:HiddenField ID="hf_editPlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_editPlugin_Changed" />
                                <asp:HiddenField ID="hf_enablePlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_enablePlugin_Changed" />
                                <asp:HiddenField ID="hf_disablePlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_disablePlugin_Changed" />
                                <asp:HiddenField ID="hf_cancelPlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_cancelPlugin_Changed" />
                                <asp:HiddenField ID="hf_updatePlugin" runat="server" ClientIDMode="Static" OnValueChanged="hf_updatePlugin_Changed" />
                                <asp:HiddenField ID="hf_InitializeCode" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_updateName" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_updateLoc" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_updateDesc" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_updateInitializeCode" runat="server" ClientIDMode="Static" />
                                <asp:HiddenField ID="hf_updateaw" runat="server" ClientIDMode="Static" />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                    <div class="clear-space">
                    </div>
                    <div class="float-left settings-name-column">
                        Files To Upload
                    </div>
                    <asp:FileUpload ID="fileupload_Plugin" runat="server" CssClass="margin-right-big" Width="270px" />
                    <div class="clear-space">
                    </div>
                    <div class="float-left settings-name-column">
                        Plugin Description
                    </div>
                    <asp:TextBox ID="txt_Description" runat="server" ClientIDMode="Static" Width="565px" CssClass="textEntry"></asp:TextBox>
                    <div class="clear-space">
                    </div>
                    <div class="float-left settings-name-column">
                        Initialize Code<br />
                        <small style="font-weight: normal!important;">(Javascript Only)</small>
                    </div>
                    <div class="float-left" style="height: 200px">
                        <div id="editor">
                        </div>
                    </div>
                    <div class="clear-space">
                    </div>
                    <div class="float-left settings-name-column">
                        Associated With
                    </div>
                    <asp:UpdatePanel ID="updatePnl_aw" runat="server">
                        <ContentTemplate>
                            <asp:DropDownList ID="dd_aw" runat="server" CssClass="pad-right-sml">
                            </asp:DropDownList>
                            <span class="pad-left-big"><small>Select '--- Nothing ---' if plugin is main file.</small></span>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div class="clear-space">
                    </div>
                    <div class="clear-space">
                    </div>
                    <div class="float-left settings-name-column"></div>
                    <asp:Button ID="btn_uploadPlugin" runat="server" Text="Upload Plugin" CssClass="input-buttons Upload-Button-Action"
                        OnClick="btn_uploadPlugin_Clicked" />
                    <div class="pad-top-big">
                        <div class="float-left settings-name-column"></div>
                        <asp:Label ID="lbl_uploadMessage" runat="server" ClientIDMode="Static"></asp:Label>
                    </div>
                </div>
                <div class="td-settings-desc">
                    Site Plugins can be uploaded here. These plugins are used for site enhancements
        and customizations. Only javascript (.js) and style sheets (.css) based plugins
        are allowed at the moment. Future plugins will feature asp.net web services and
        more. Users will be able to install the plugins from the
        <asp:Label ID="lbl_appInstaller" runat="server"></asp:Label>.
                </div>
            </div>
        </asp:Panel>
        <asp:UpdatePanel ID="updatepnl_SitePlugins" runat="server">
            <ContentTemplate>
                <div class="clear-space"></div>
                <div class="clear-margin">
                    <div class="float-left pad-right-big">
                        <b class="pad-right">Total</b><asp:Label ID="lbl_TotalPlugins"
                            runat="server"></asp:Label>
                    </div>
                    <div class="float-left pad-left-big">
                        <b class="pad-right">Enabled</b><asp:Label ID="lbl_TotalEnabledPlugins" runat="server"></asp:Label>
                    </div>
                    <asp:LinkButton ID="lbtn_associationBack" runat="server" OnClick="lbtn_associationBack_Click" CssClass="float-right RandomActionBtns" Text="Back to Main Plugins" Enabled="false" Visible="false"></asp:LinkButton>
                </div>
                <asp:Panel ID="pnl_siteplugins" runat="server" CssClass="clear-margin">
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="updatePnl_association" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hf_BuildAssociatedTable" runat="server" ClientIDMode="Static" OnValueChanged="BuildAssociatedTable_ValueChanged" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="hf_BuildAssociatedTable" />
            </Triggers>
        </asp:UpdatePanel>
        <div id="LoaderApp-element" class="Modal-element" style="display: none;">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="700">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#close" onclick="openWSE.LoadModalWindow(false, 'LoaderApp-element', '');return false;" class="ModalExitButton"></a>
                                </div>
                                <span class='Modal-title'></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="updatepnl_DefaultPage" runat="server">
                                    <ContentTemplate>
                                        Select the main javascript file that needs to load. You can choose multiple files to assoicate with the main file.
                                        Pressing the Cancel button will delete all the contents of the plugin.
                                        <div class="table-settings-box no-border">
                                            <table cellpadding="10" cellspacing="10">
                                                <tr>
                                                    <td class="td-settings-title">Main Javascript File</td>
                                                    <td class="td-settings-title">Associated Files</td>
                                                </tr>
                                                <tr>
                                                    <td valign="top" class="border-right pad-right">
                                                        <asp:RadioButtonList ID="radioButton_FileList" runat="server">
                                                        </asp:RadioButtonList>
                                                        <asp:Label ID="mainfileEmpty" runat="server" Text="There are no files to select from" Enabled="false" Visible="false"></asp:Label>
                                                    </td>
                                                    <td valign="top">
                                                        <a href="#" id="a_selectall" data-state="off" onclick="SelectAllUpload();return false;">Select All</a>
                                                        <div class="clear-space-two"></div>
                                                        <asp:CheckBoxList ID="checkbox_FileList" runat="server">
                                                        </asp:CheckBoxList>
                                                        <asp:Label ID="associatedfileEmpty" runat="server" Text="There are no files to select from" Enabled="false" Visible="false"></asp:Label>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btn_updateDefaultFile" />
                                        <asp:AsyncPostBackTrigger ControlID="btn_cancelUpload" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:HiddenField ID="hf_defaultID" runat="server" ClientIDMode="Static" />
                            <asp:Button ID="btn_updateDefaultFile" runat="server" CssClass="input-buttons RandomActionBtns"
                                Text="Set Page" OnClick="btn_updateDefaultFile_Clicked" />
                            <asp:Button ID="btn_cancelUpload" runat="server" CssClass="input-buttons RandomActionBtns no-margin"
                                Text="Cancel" OnClick="btn_cancelUpload_Clicked" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script src='<%=ResolveUrl("~/Scripts/AceEditor/ace.js")%>' type="text/javascript" charset="utf-8"></script>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/siteplugins.js")%>'></script>
    </div>
</asp:Content>
