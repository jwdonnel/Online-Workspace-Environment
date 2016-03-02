<%@ Page Title="Startup Scripts" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="StartupScripts.aspx.cs" Inherits="SiteTools_StartupScripts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <asp:Panel ID="pnlLinkBtns" runat="server">
        </asp:Panel>
        <div id="FindReplace-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="510">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'FindReplace-element', '');return false;" class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_findreplace" runat="server" Enabled="false" Visible="false">
                                            <h3>Find and Replace</h3>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="tb_findvalue" runat="server" CssClass="textEntry margin-right" onfocus="if(this.value=='Find value')this.value=''"
                                                onblur="if(this.value=='')this.value='Find value'" Text="Find value" Width="350px"></asp:TextBox>
                                            <div class="clear-space-five"></div>
                                            <asp:TextBox ID="tb_replacevalue" runat="server" CssClass="textEntry margin-right"
                                                onfocus="if(this.value=='Replace with value')this.value=''" onblur="if(this.value=='')this.value='Replace with value'"
                                                Text="Replace with value" Width="350px">
                                            </asp:TextBox>
                                            <div class="clear"></div>
                                        </asp:Panel>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btn_findreplace" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:Button ID="btn_findreplace" runat="server" CssClass="input-buttons RandomActionBtns no-margin float-left" Text="Replace" OnClick="btn_findreplace_Clicked" />
                            <input type="button" value="Cancel" class="input-buttons no-margin float-right" onclick="openWSE.LoadModalWindow(false, 'FindReplace-element', ''); $('#MainContent_tb_findvalue').val('Find value'); $('#MainContent_tb_replacevalue').val('Replace with value'); return false;" />
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div id="javascripts" class="pnl-section">
            <div class="clear"></div>
            <a href="#" id="aFindReplace" runat="server" onclick="openWSE.LoadModalWindow(true, 'FindReplace-element', 'Find and Replace');return false;" class="float-right input-buttons" title="Find and replace all script paths." style="position: relative; z-index: 1; margin-right: 0; margin-left: 10px; margin-bottom: 10px;"><span class="float-left img-replace margin-right-sml"></span>Find and Replace</a>
            <div class="pad-bottom">
                Add/Update/Edit/Delete the startup scripts. Functionality will depend on the arrangment of the scripts. To edit a script, go to the File Manager page.
            </div>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="clear-margin">
                        <asp:Panel ID="pnl_startupscripts" runat="server">
                        </asp:Panel>
                        <div class="clear-space">
                        </div>
                    </div>
                    <asp:HiddenField ID="hf_UpdateStartupScripts" ClientIDMode="Static" runat="server"
                        OnValueChanged="hf_UpdateStartupScripts_ValueChanged" />
                    <asp:HiddenField ID="hf_EditStartupScripts" ClientIDMode="Static" runat="server"
                        OnValueChanged="hf_EditStartupScripts_ValueChanged" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="stylesheets" class="pnl-section" style="display: none">
            <div class="clear"></div>
            <a href="#" id="aFindReplace_css" runat="server" onclick="openWSE.LoadModalWindow(true, 'FindReplace-element', 'Find and Replace');return false;" class="float-right input-buttons" title="Find and replace all script paths." style="position: relative; z-index: 1; margin-right: 0; margin-left: 10px; margin-bottom: 10px;"><span class="float-left img-replace margin-right-sml"></span>Find and Replace</a>
            <div class="pad-bottom">
                Add/Update/Edit/Delete the startup scripts. Functionality will depend on the arrangment of the scripts. To edit a script, go to the File Manager page.
            </div>
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <div class="clear-margin">
                        <asp:Panel ID="pnl_startupscripts_css" runat="server">
                        </asp:Panel>
                        <div class="clear-space">
                        </div>
                    </div>
                    <asp:HiddenField ID="hf_UpdateStartupScripts_css" ClientIDMode="Static" runat="server"
                        OnValueChanged="hf_UpdateStartupScripts_CSS_ValueChanged" />
                    <asp:HiddenField ID="hf_EditStartupScripts_css" ClientIDMode="Static" runat="server"
                        OnValueChanged="hf_EditStartupScripts_CSS_ValueChanged" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="settings" runat="server" clientidmode="Static" class="pnl-section" style="display: none;">
            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                <ContentTemplate>
                    <div class="table-settings-box no-border">
                        <div class="td-settings-title">
                            Append Timestamp on Load
                        </div>
                        <div class="title-line"></div>
                        <div class="td-settings-ctrl">
                            <div class="field switch inline-block">
                                <asp:RadioButton ID="rb_appendTimestamp_on" runat="server" Text="Yes" CssClass="RandomActionBtns cb-enable"
                                    OnCheckedChanged="rb_appendTimestamp_on_CheckedChanged" AutoPostBack="True" />
                                <asp:RadioButton ID="rb_appendTimestamp_off" runat="server" Text="No" CssClass="RandomActionBtns cb-disable"
                                    OnCheckedChanged="rb_appendTimestamp_off_CheckedChanged" AutoPostBack="True" />
                            </div>
                        </div>
                        <div class="td-settings-desc">
                            Set to Yes if you want to append a timestamp to the end of the scripth path to ensure that you get the latest version.
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="rb_appendTimestamp_on" />
                    <asp:AsyncPostBackTrigger ControlID="rb_appendTimestamp_off" />
                </Triggers>
            </asp:UpdatePanel>
            <div class="clear-space">
            </div>
        </div>
    </div>
</asp:Content>
