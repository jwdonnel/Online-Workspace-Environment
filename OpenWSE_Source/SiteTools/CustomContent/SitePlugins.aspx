<%@ Page Title="Site Plugins" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="SitePlugins.aspx.cs" Inherits="SiteTools_SitePlugins" %>

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
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <asp:Panel ID="pnl_AddControls" runat="server" ClientIDMode="Static">
            <div class="table-settings-box">
                <div class="td-settings-ctrl">
                    <div class="float-left settings-name-column"></div>
                    <a id="aUploadFile" href="#upload" onclick="UploadFileSelect();return false;" style="display: none;">Click here to Upload File</a>
                    <a href="#mappath" id="aMapPath" onclick="MapPathSelect();return false;">Click here to Map Path</a>
                    <div class="clear-space">
                    </div>
                    <div id="AddPlugin" style="display: none">
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
                                <asp:Panel ID="pnl_uploadUrl" runat="server" DefaultButton="btn_addNewPlugin">
                                    <div class="float-left settings-name-column">
                                        Add Plugin
                                    </div>
                                    <asp:TextBox ID="txt_PluginName" runat="server" CssClass="textEntry margin-right"
                                        Text="Plugin Name" Width="175px" onfocus="if(this.value=='Plugin Name')this.value=''"
                                        onblur="if(this.value=='')this.value='Plugin Name'" MaxLength="250"></asp:TextBox>
                                    <asp:TextBox ID="txt_PluginLoc" runat="server" CssClass="textEntry margin-right"
                                        Text="Plugin Location/URL" Width="375px" onfocus="if(this.value=='Plugin Location/URL')this.value=''"
                                        onblur="if(this.value=='')this.value='Plugin Location/URL'"></asp:TextBox>
                                    <asp:CheckBox ID="cb_enabledPlugin" runat="server" Text="&nbsp;Enable Plugin" CssClass="margin-right-big" />
                                    <asp:Button ID="btn_addNewPlugin" runat="server" Text="Add Plugin" CssClass="input-buttons Button-Action"
                                        OnClick="btn_addNewPlugin_Clicked" />
                                </asp:Panel>
                                <div class="clear-space-five">
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                    <div id="UploadPlugin">
                        <div class="float-left settings-name-column">
                            Upload Plugin
                        </div>
                        <asp:TextBox ID="txt_uploadPluginName" runat="server" CssClass="textEntry margin-right"
                            Text="Plugin Name" Width="150px" onfocus="if(this.value=='Plugin Name')this.value=''"
                            onblur="if(this.value=='')this.value='Plugin Name'" MaxLength="250"></asp:TextBox>
                        <asp:CheckBox ID="cb_enableUpload" runat="server" Text="&nbsp;Enable Plugin" CssClass="margin-right-big" />
                        <asp:FileUpload ID="fileupload_Plugin" runat="server" CssClass="margin-right-big" Width="270px" />
                        <asp:Button ID="btn_uploadPlugin" runat="server" Text="Upload/Add" CssClass="input-buttons Upload-Button-Action"
                            OnClick="btn_uploadPlugin_Clicked" />
                        <div class="clear-space-five">
                        </div>
                    </div>
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
                    <div id="cb_installerAfter_div">
                        <div class="float-left settings-name-column"></div>
                        <asp:CheckBox ID="cb_installAfter" runat="server" Text="&nbsp;Install after upload" />
                    </div>
                    <div class="clear">
                    </div>
                    <div class="clear-margin">
                        <asp:Label ID="lbl_uploadMessage" runat="server" ClientIDMode="Static" Style="padding-left: 155px"></asp:Label>
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
                        <b class="pad-right">Total (No Association)</b><asp:Label ID="lbl_TotalPlugins"
                            runat="server"></asp:Label>
                    </div>
                    <div class="float-left pad-left-big">
                        <b class="pad-right">Enabled</b><asp:Label ID="lbl_TotalEnabledPlugins" runat="server"></asp:Label>
                    </div>
                </div>
                <asp:Panel ID="pnl_siteplugins" runat="server" CssClass="clear-margin">
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class="clear" style="height: 20px">
        </div>
        <script src='<%=ResolveUrl("~/Scripts/AceEditor/ace.js")%>' type="text/javascript" charset="utf-8"></script>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/siteplugins.js")%>'></script>
    </div>
</asp:Content>
