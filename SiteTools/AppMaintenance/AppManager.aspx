<%@ page title="App Editor" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_AppManager, App_Web_tg1onwhl" %>

<%@ Register TagPrefix="cc" Namespace="TextEditor" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        #editor
        {
            position: relative;
            top: 0;
            height: 595px;
            width: 100%;
            left: 0;
            font-size: 14px;
        }

        #MainContent_cb_associatedOverlay td, #MainContent_cc_associatedOverlayNew td
        {
            padding-bottom: 5px;
        }

        #MainContent_dd_category td, #MainContent_dd_category_edit td
        {
            padding-bottom: 3px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <asp:Panel ID="pnlLinkBtns" runat="server">
        </asp:Panel>
        <div class="clear-space"></div>
        <asp:HiddenField ID="hf_isParams" runat="server" ClientIDMode="Static" Value="0" />
        <div class="actions-bg action-margin-fix">
            <asp:Panel ID="pnl_app_params" runat="server" Enabled="false" Visible="false">
                <table cellpadding="0" cellspacing="0" border="0" width="100%">
                    <tr>
                        <td class="pad-right-big" valign="top" style="min-width: 230px; width: 230px;">
                            <asp:UpdatePanel ID="updatepnl_ect_holder1" runat="server">
                                <ContentTemplate>
                                    <asp:Panel ID="pnl_appList1" runat="server">
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </td>
                        <td class="pad-left-big" valign="top">
                            <asp:UpdatePanel ID="Updatepnl_params" runat="server">
                                <ContentTemplate>
                                    <asp:HiddenField ID="hf_appchange_params" runat="server" OnValueChanged="hf_appchange_params_Changed" />
                                    <asp:HiddenField ID="hf_appchange_params_edit" runat="server" ClientIDMode="Static"
                                        OnValueChanged="hf_appchange_params_edit_Changed" />
                                    <asp:HiddenField ID="hf_appchange_params_delete" runat="server" ClientIDMode="Static"
                                        OnValueChanged="hf_appchange_params_delete_Changed" />
                                    <asp:HiddenField ID="hf_appchange_params_update" runat="server" ClientIDMode="Static"
                                        OnValueChanged="hf_appchange_params_update_Changed" />
                                    <asp:HiddenField ID="hf_appchange_params_cancel" runat="server" ClientIDMode="Static"
                                        OnValueChanged="hf_appchange_params_cancel_Changed" />
                                    <asp:Label ID="lbl_params_tip" runat="server" CssClass="pad-left"
                                        Text="Select a app to view/add/edit parameters"></asp:Label>
                                    <asp:Panel ID="pnl_params_holder" runat="server" Enabled="false" Visible="false">
                                        <div class="clear-margin" style="width: 612px">
                                            <asp:LinkButton ID="lbtn_close_params" CssClass="rbbuttons float-right"
                                                OnClick="lbtn_close_params_Click" runat="server" ToolTip="Close">Close Parameters</asp:LinkButton>
                                            <asp:Literal ID="ltl_app_params" runat="server"></asp:Literal>
                                        </div>
                                        <asp:Panel ID="Panel1" runat="server" DefaultButton="btn_app_params">
                                            <asp:TextBox ID="txt_app_params" runat="server" CssClass="TextBoxControls margin-right"
                                                Width="500px" onfocus="if(this.value=='New App Parameter')this.value=''" onblur="if(this.value=='')this.value='New App Parameter'"
                                                Text="New App Parameter"></asp:TextBox>
                                            <asp:Button ID="btn_app_params" runat="server" Text="Add Param" CssClass="input-buttons rbbuttons"
                                                OnClick="btn_app_params_Click" />
                                            <div class="clear-space-five">
                                            </div>
                                            <asp:TextBox ID="txt_app_params_description" runat="server" CssClass="TextBoxControls margin-right"
                                                Width="500px" onfocus="if(this.value=='Parameter Description')this.value=''"
                                                onblur="if(this.value=='')this.value='Parameter Description'" Text="Parameter Description"></asp:TextBox>
                                            <div class="clear-margin">
                                                <asp:Label ID="lbl_param_error" runat="server" ForeColor="Red" Text=""></asp:Label>
                                            </div>
                                        </asp:Panel>
                                        <div class="clear-space">
                                        </div>
                                        <asp:Panel ID="pnl_app_params_holder" runat="server">
                                        </asp:Panel>
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Panel ID="pnl_app_EditList" runat="server" ClientIDMode="Static">
                <div class="table-settings-box">
                    <div class="td-settings-title">
                        <div id="searchwrapper" style="width: 300px;">
                            <asp:Panel ID="Panel1_Installer" runat="server" DefaultButton="imgbtn_search">
                                <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                                    onfocus="if(this.value=='Search Apps')this.value=''" onblur="if(this.value=='')this.value='Search Apps'"
                                    Text="Search Apps"></asp:TextBox>
                                <a href="#" onclick="$('#MainContent_tb_search').val('Search Apps');return false;"
                                    class="searchbox_clear" title="Clear search"></a>
                                <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                    OnClick="imgbtn_search_Click" />
                            </asp:Panel>
                        </div>
                        <div class="clear-space">
                        </div>
                        <asp:UpdatePanel ID="updatePnl_AppTotals" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Label ID="lbl_AppsAvailable" runat="server" ClientIDMode="Static"></asp:Label>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <div class="float-right" style="margin-top: -20px;">
                            <b class="pad-right">Category</b>
                            <asp:DropDownList ID="ddl_categories" runat="server" ClientIDMode="Static">
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <asp:UpdatePanel ID="updatePnl_AppList" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Panel ID="pnl_AppList" runat="server" ClientIDMode="Static" Style="border-top: none!important;"></asp:Panel>
                                <div id="noItemsCategory" class="emptyGridView" style="display: none;">No items in category</div>
                                <div class="clear-space"></div>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                    <div class="td-settings-desc">
                        Select an app above to view details. Apps without a delete button are standard apps that can only be deleted by the Administrator.
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnl_app_information" runat="server" Enabled="false" Visible="false" ClientIDMode="Static">
                <table cellpadding="0" cellspacing="0" border="0" width="100%">
                    <tr>
                        <td class="pad-left-big" valign="top">
                            <div id="div_tip_cleanappbtn">
                                Use the controls below to create a custom app. The apps will be coded with
                                html and javascript.
                                    <asp:Panel ID="pnl_backupAllApps" runat="server" CssClass="float-right">
                                        <iframe src="../iframes/AppDownloadBtn.aspx?backup=true" frameborder="0" height="31px"
                                            width="178px" scrolling="no"></iframe>
                                    </asp:Panel>
                                <asp:LinkButton ID="btn_performCleanup" runat="server" OnClientClick="PerformAppCleanUp();return false;"
                                    CssClass="float-right"><span class="img-refresh float-left margin-right-sml"></span>Perform App List Clean Up</asp:LinkButton>
                                <asp:HiddenField ID="hf_performCleanup" ClientIDMode="Static" runat="server" OnValueChanged="btn_performCleanup_Click" />
                            </div>
                            <div class="clear-space" style="height: 20px;">
                            </div>
                            <div class="float-left">
                                <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                                    <ContentTemplate>
                                        <asp:HiddenField ID="hf_createapp" runat="server" ClientIDMode="Static" OnValueChanged="hf_createapp_Changed" />
                                        <asp:HiddenField ID="hf_saveapp" runat="server" ClientIDMode="Static" OnValueChanged="hf_saveapp_Changed" />
                                        <asp:Button ID="btn_create_easy" runat="server" Text="Create App" CssClass="input-buttons margin-right float-left"
                                            Enabled="false" Visible="false" OnClick="btn_createEasy_Click" />
                                        <asp:Panel ID="btn_create" runat="server" CssClass="float-left">
                                            <input type="button" id="btn_createapp" class="input-buttons margin-right"
                                                value="Create App" onclick="CreateApp_Click();" />
                                        </asp:Panel>
                                        <asp:Button ID="btn_uploadnew" runat="server" Text="Upload App" CssClass="input-buttons margin-right float-left"
                                            Enabled="false" Visible="false" OnClick="btn_uploadnew_Click" OnClientClick="if (!ValidateForm()){return false;}" />
                                        <asp:Button ID="btn_clear_controls" runat="server" OnClick="btn_clear_controls_Click"
                                            Text="Clear Controls" CssClass="input-buttons margin-right margin-left float-left"
                                            OnClientClick="openWSE.LoadingMessage1('Clearing Controls. Please Wait...');" />
                                        <div class="clear-space">
                                        </div>
                                        <span id="lbl_ErrorUpload"></span>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btn_performCleanup" />
                                        <asp:AsyncPostBackTrigger ControlID="hf_performCleanup" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                            <div id="btn_viewCode" runat="server" class="float-right">
                                <a href="#viewcode" onclick="ViewCode();return false;">View Source Code</a>
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="clear-margin">
                                <asp:CheckBox ID="cb_InstallAfterLoad" runat="server" Text="&nbsp;Install app for current user on create"
                                    Checked="true" />
                                <div class="clear-space-two"></div>
                                <asp:CheckBox ID="cb_wrapIntoIFrame" runat="server" Text="&nbsp;Wrap this app into an iframe"
                                    Checked="false" />
                                <div id="div_isPrivate">
                                    <div class="clear-space-two"></div>
                                    <asp:CheckBox ID="cb_isPrivate" runat="server" Text="&nbsp;Make this app private (Only for me)"
                                        ClientIDMode="Static" Checked="false" />
                                </div>
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="clear-space">
                            </div>
                            <table class="float-left pad-right-big" cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">App Name
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tb_appname" CssClass="textEntry" runat="server" Width="210px"
                                            MaxLength="150"></asp:TextBox>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear">
                            </div>
                            <asp:Panel ID="pnl_filename" runat="server">
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="settings-name-column">Filename
                                        </td>
                                        <td>
                                            <asp:TextBox ID="tb_filename_create" CssClass="textEntry" runat="server" Width="210px"
                                                Enabled="false" MaxLength="150" BackColor="#EFEFEF"></asp:TextBox>
                                            <asp:DropDownList ID="dd_filename_ext" runat="server" ClientIDMode="Static" CssClass="margin-left">
                                                <asp:ListItem Text=".html (Standard Extention)" Value=".html"></asp:ListItem>
                                                <asp:ListItem Text=".ascx (ASP.Net Extention)" Value=".ascx"></asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:Label ID="lbl_dotHtml" runat="server" Text=".html" Enabled="false" Visible="false"></asp:Label>
                                            <div class="clear-space-five">
                                            </div>
                                            <small>Filenames are automatically generated</small><br />
                                            <small>File extension cannot be changed</small><span id="ascxNote" style="padding-left: 52px; display: none"><small>This file extention has limited editing capabilities</small></span>
                                        </td>
                                    </tr>
                                </table>
                            </asp:Panel>
                            <div id="newupload" style="display: none;">
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="settings-name-column">Upload File
                                        </td>
                                        <td>
                                            <asp:FileUpload ID="fu_uploadnew" runat="server" />
                                            <div class="clear-space-five">
                                            </div>
                                            <small><b>.zip</b>, <b>.html</b>, <b>.htm</b>, <b>.txt</b>, <b>.aspx</b>, <b>.ascx</b>,
                                                    <b>.pdf</b>, <b>Word files</b>,<br />
                                                and <b>Excel files</b> are only allowed</small>
                                        </td>
                                    </tr>
                                </table>
                                <div id="zipfileLoadname" style="display: none">
                                    <table class="float-left pad-right-big" cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="settings-name-column">App Load File
                                            </td>
                                            <td>
                                                <small>You will need to specify the filename that will be used to load the app. (e.g.
                                                        AppFile.html)<br />
                                                    A dialog box will display allowing you to choose which file to use after uploading
                                                        app.</small>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                            <div class="clear">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Description
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tb_description_create" runat="server" CssClass="textEntry" AutoPostBack="False"
                                            TextMode="MultiLine" Height="65px" Font-Names='"Arial"' BorderColor="#D9D9D9"
                                            Width="375px" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;" ForeColor="#353535"></asp:TextBox>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">About
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tb_about_create" runat="server" CssClass="textEntry" AutoPostBack="False"
                                            TextMode="MultiLine" Height="65px" Font-Names='"Arial"' BorderColor="#D9D9D9"
                                            Width="375px" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;" ForeColor="#353535"></asp:TextBox>
                                    </td>
                                </tr>
                            </table>
                            <asp:Panel ID="pnl_apphtml" runat="server" Enabled="false" Visible="false">
                                <table cellpadding="10" cellspacing="10" width="100%">
                                    <tr>
                                        <td class="settings-name-column">HTML Link
                                        </td>
                                        <td>
                                            <asp:TextBox ID="tb_html_create" runat="server" CssClass="textEntry" AutoPostBack="False"
                                                TextMode="MultiLine" Height="50px" Font-Names='"Arial"' BorderColor="#D9D9D9"
                                                Width="375px" BorderStyle="Solid" BorderWidth="1px" Style="padding: 4px;" ForeColor="#353535"></asp:TextBox>
                                        </td>
                                    </tr>
                                </table>
                                <div class="clear-space-five">
                                </div>
                                <small><b class="pad-right-sml pad-left">Note:</b>Icon will be downloaded from html link
                                        automatically if available.</small>
                            </asp:Panel>
                            <asp:Panel ID="pnl_appicon" runat="server">
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="settings-name-column">App Icon
                                        </td>
                                        <td>
                                            <div class="float-left">
                                                <a href="#" id="urlIcon-tab" onclick="ChangeIconUploadType(0);return false;">Click here to use Url Image</a>
                                                <a href="#" id="uploadIcon-tab" onclick="ChangeIconUploadType(1);return false;" style="display: none;">Click here to Upload Icon</a>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                                <div id="uploadIcon">
                                    <table cellpadding="10" cellspacing="10">
                                        <tr>
                                            <td class="settings-name-column">Upload Icon
                                            </td>
                                            <td>
                                                <asp:FileUpload ID="fu_image_create" runat="server" />
                                                <div class="clear-space-five">
                                                </div>
                                                <small><b>.png</b> <b>.jpeg</b> and <b>.gif</b> only allowed</small>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                                <div id="urlIcon" style="display: none">
                                    <table cellpadding="10" cellspacing="10" style="width: 100%">
                                        <tr>
                                            <td class="settings-name-column">Icon Url
                                            </td>
                                            <td>
                                                <asp:TextBox ID="tb_imageurl" runat="server" CssClass="textEntry" Width="375px"></asp:TextBox>
                                                <div class="clear-space-five">
                                                </div>
                                                <small><b>.png</b> <b>.jpeg</b> and <b>.gif</b> only allowed</small>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </asp:Panel>
                            <div class="clear-space-five">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Allow Pop Out
                                    </td>
                                    <td>
                                        <asp:DropDownList ID="dd_allowpopout_create" runat="server" Style="width: 75px; margin-top: 2px;">
                                            <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">
                                        <span id="span1">Pop Out Location</span>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tb_popoutLoc_create" runat="server" CssClass="textEntry" Width="375px"></asp:TextBox>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <asp:Panel ID="pnl_new_AssociatedOverlay" runat="server">
                                <table cellpadding="10" cellspacing="10">
                                    <tr>
                                        <td class="settings-name-column">Associated Overlays
                                        </td>
                                        <td>
                                            <asp:CheckBoxList ID="cc_associatedOverlayNew" runat="server">
                                            </asp:CheckBoxList>
                                        </td>
                                    </tr>
                                </table>
                            </asp:Panel>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Category
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:CheckBoxList ID="dd_category" runat="server">
                                        </asp:CheckBoxList>
                                    </td>
                                    <td><small>Put the app into a category to help organize the apps.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Min-Width
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:TextBox ID="tb_minwidth_create" runat="server" CssClass="TextBoxEdit" Width="50px"
                                            MaxLength="4" Text="500"></asp:TextBox><span class="pad-left">px</span>
                                    </td>
                                    <td><small>Set the minimum width of the app.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Min-Height
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:TextBox ID="tb_minheight_create" runat="server" CssClass="TextBoxEdit" Width="50px"
                                            MaxLength="4" Text="400"></asp:TextBox><span class="pad-left">px</span>
                                    </td>
                                    <td><small>Set the minimum height of the app.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">App Package
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:DropDownList ID="dd_package" runat="server">
                                        </asp:DropDownList>
                                    </td>
                                    <td><small>Select a app package that you want the created app to be in.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Background
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:DropDownList ID="dd_enablebg_create" runat="server" Style="width: 105px">
                                            <asp:ListItem Text="Visible" Value="app-main"></asp:ListItem>
                                            <asp:ListItem Text="Hidden" Value="app-main-nobg"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td><small>
                                        <b>Visible: </b>Will show the app background and controls.<br />
                                        <b>Hidden: </b>Will hide the background and controls. Controls will appear when
                                                    hovering over the app.
                                    </small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Max on Load
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:DropDownList ID="dd_maxonload_create" runat="server" Style="width: 75px; margin-top: 2px;">
                                            <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td><small>Set to True to force the app to expand to a full screen every time you load it.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Auto Open
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:DropDownList ID="dd_autoOpen_create" runat="server" Style="width: 75px; margin-top: 2px;">
                                            <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td><small>Set to True to automatically open this app when loading the workspace.<br />
                                        App can be closed but will not be saved.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Allow Resize
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:DropDownList ID="dd_allowresize_create" runat="server" Style="width: 75px; margin-top: 2px;">
                                            <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td><small>Set to True to allow for the app to be resized.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Allow Maximize
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:DropDownList ID="dd_allowmax_create" runat="server" Style="width: 75px; margin-top: 2px;">
                                            <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td><small>Set to True to allow for the app to maximize.<br />
                                        Setting this to false will hide the maximize button in the app header.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Allow Params
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:DropDownList ID="dd_allow_params" runat="server" Style="width: 75px; margin-top: 2px;">
                                            <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td><small>Set this to True to allow for parameters to be setup in the App Params page.<br />
                                        Please note that the app must be coded to allow for the parameters to work.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">Default Workspace
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:DropDownList ID="dd_defaultworkspace_create" runat="server" Style="width: 75px; margin-top: 2px;">
                                            <asp:ListItem Text="1" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="2" Value="2"></asp:ListItem>
                                            <asp:ListItem Text="3" Value="3"></asp:ListItem>
                                            <asp:ListItem Text="4" Value="4"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td><small>Select the workspace you want the app to load to by default.</small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space-two">
                            </div>
                            <table cellpadding="10" cellspacing="10">
                                <tr>
                                    <td class="settings-name-column">
                                        <span id="span-autocreate" style="display: none">Auto Create</span>
                                    </td>
                                    <td style="width: 200px;">
                                        <asp:DropDownList ID="dd_autocreate_create" runat="server" ClientIDMode="Static"
                                            Style="width: 75px; margin-top: 2px; display: none">
                                            <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
        </div>
        <asp:UpdatePanel ID="updatepnl_editor" runat="server">
            <ContentTemplate>
                <asp:Panel ID="Edit_Controls" runat="server" Enabled="false" Visible="false" Width="100%">
                    <div class="clear" style="height: 20px;">
                    </div>
                    <h2 class="float-left font-bold">App Source Editor -</h2>
                    <asp:Label ID="lbl_currfile" runat="server" CssClass="float-left pad-top margin-left"
                        Text=""></asp:Label>
                    <div class="clear" style="height: 20px">
                    </div>
                    <a id="lbtn_save" href="#save" class="input-buttons no-margin float-right" onclick="SaveApp_Click();return false;"
                        title="Save/Overwrite"><span class="img-backup float-left margin-right-sml"></span>
                        Save</a>
                    <asp:LinkButton ID="lbtn_close" CssClass="input-buttons float-right RandomActionBtns"
                        OnClick="lbtn_close_Click" runat="server" ToolTip="Close">
                    <span class="pg-prev-btn float-left margin-right-sml" style="padding: 0px!important;"></span>Back</asp:LinkButton>
                </asp:Panel>
                <asp:Panel ID="pnl_htmleditor" runat="server" ClientIDMode="Static" Style="display: none; margin-top: 15px">
                    <div id="btn_backProp" runat="server" class="float-right">
                        <a href="#" onclick="ViewCode();return false;">Back to Properties</a>
                    </div>
                    <div class="clear-space">
                    </div>
                    <div id="HTMLCODE">
                        <div class="editor_titles">
                            <div class="title-line"></div>
                            <h3>HTML File Editor</h3>
                        </div>
                        <div class="clear-space" style="height: 20px;">
                        </div>
                        <cc:AppEditor runat="server" ID="htmlEditor" ClientIDMode="Static" Mode="Full"
                            Height="500px" />
                        <div class="clear-space" style="height: 40px;">
                        </div>
                    </div>
                    <div id="JAVASCRIPTCODE">
                        <div class="editor_titles">
                            <div class="title-line"></div>
                            <h3><span id="javascriptcode_Title">Javascript File Editor</span></h3>
                        </div>
                        <asp:Literal ID="links_externalCode" runat="server"></asp:Literal>
                        <div class="clear" style="height: 20px">
                        </div>
                        <div class="clear-margin pad-all">
                            <div style="background: #EBEBEB; padding-left: 42px">
                                <div id="script_frameworks" class="pad-top-big pad-bottom-sml" style="color: #888; border-bottom: 1px solid #9F9F9F;">
                                    <b class="pad-right">Frameworks Available</b><span class="pad-left pad-right"><a
                                        href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://jquery.com', this);return false;">jQuery</a></span><span
                                            class="pad-left pad-right"><a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://jqueryui.com', this);return false;">jQuery
                                            UI</a></span>
                                </div>
                            </div>
                            <div id="editor">
                            </div>
                            <div style="background: #EBEBEB; padding-left: 42px">
                                <div class="pad-top-sml pad-bottom-big" style="color: #888; border-top: 1px solid #9F9F9F;">
                                    <b class="pad-right">Code References</b> <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://jscompress.com', this);return false;"
                                        class="margin-right margin-left">Minify Javascript</a> <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://refresh-sf.com/yui', this);return false;"
                                            class="margin-right margin-left">Minify CSS</a> <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://www.colorzilla.com/gradient-editor', this);return false;"
                                                class="margin-right margin-left">Gradient Generator</a> <a href="#iframecontent"
                                                    onclick="openWSE.LoadIFrameContent('http://css3gen.com/box-shadow', this);return false;"
                                                    class="margin-right margin-left">Box Shadow Generator</a>
                                </div>
                            </div>
                        </div>
                        <asp:HiddenField ID="hidden_editor" runat="server" ClientIDMode="Static" />
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div id="App-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="700">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'App-element', '');$('#wlmd_editor_holder').hide();$('#MainContent_tb_title_edit').val('');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:Panel ID="pnl_appeditor" runat="server" DefaultButton="btn_save">
                                    <asp:UpdatePanel ID="updatepnl_apps" runat="server">
                                        <ContentTemplate>
                                            <asp:Panel ID="wlmd_holder" runat="server">
                                            </asp:Panel>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <div id="wlmd_editor_holder" style="display: none;">
                                        <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                            <ContentTemplate>
                                                <asp:Image ID="img_edit" ImageUrl="" runat="server" CssClass='pad-right-big float-left'
                                                    Style='height: 50px;' />
                                                <div class="float-left">
                                                    <b class='float-left pad-top pad-right'>Title</b><asp:TextBox ID="tb_title_edit"
                                                        runat="server" CssClass="TextBoxEdit margin-top"></asp:TextBox>
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                                                        <ContentTemplate>
                                                            <b class='float-left pad-top-sml pad-right'>Category</b>
                                                            <asp:CheckBoxList ID="dd_category_edit" runat="server">
                                                            </asp:CheckBoxList>
                                                        </ContentTemplate>
                                                    </asp:UpdatePanel>
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <div class='clear-space'>
                                        </div>
                                        <div class='clear-space'>
                                        </div>
                                        <asp:Panel ID="pnl_appIconEdit" runat="server">
                                            <div class="float-left" style="width: 260px">
                                                <b class="pad-right float-left pad-top-sml">App Icon</b>
                                                <div class="clear-space-five">
                                                </div>
                                                <div class="float-left">
                                                    <a href="#" id="urlIcon-tab-edit" onclick="ChangeIconUploadTypeEdit(0);return false;">Click here to use Url Image</a>
                                                    <a href="#" id="uploadIcon-tab-edit" onclick="ChangeIconUploadTypeEdit(1);return false;" style="display: none;">Click here to Upload Icon</a>
                                                </div>
                                                <div class="clear"></div>
                                            </div>
                                            <div class="float-left">
                                                <div id="uploadIcon-edit">
                                                    <b class="pad-right float-left pad-top-sml">Upload Icon</b>
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <asp:FileUpload ID="fu_image_edit" runat="server" CssClass="float-left" />
                                                </div>
                                                <div id="urlIcon-edit" style="display: none">
                                                    <b class="pad-right float-left pad-top-sml">URL Image</b>
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <asp:TextBox ID="tb_imageurl_edit" runat="server" CssClass="textEntry" Width="230px"></asp:TextBox>
                                                    <br />
                                                    <small><b>.png</b> <b>.jpeg</b> and <b>.gif</b> only allowed</small>
                                                </div>
                                            </div>
                                            <div class="clear" style="height: 25px">
                                            </div>
                                        </asp:Panel>
                                        <div class="clear-space-five">
                                        </div>
                                        <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                                            <ContentTemplate>
                                                <b>Description</b><div class="clear-space-five">
                                                </div>
                                                <asp:TextBox ID="tb_description_edit" runat="server" TextMode="MultiLine" CssClass="TextBoxEdit pad-all-sml"
                                                    Height="75px" Width="500px" Font-Names="Arial" BorderColor="#D9D9D9"></asp:TextBox>
                                                <div class="clear" style="height: 15px;">
                                                </div>
                                                <b>About</b><div class="clear-space-five">
                                                </div>
                                                <asp:TextBox ID="tb_about_edit" runat="server" TextMode="MultiLine" CssClass="TextBoxEdit pad-all-sml"
                                                    Height="75px" Width="500px" Font-Names="Arial" BorderColor="#D9D9D9"></asp:TextBox>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <div class="clear-space">
                                        </div>
                                        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                            <ContentTemplate>
                                                <div class="clear-margin">
                                                    <asp:Panel ID="pnl_edit_AssociatedOverlay" runat="server">
                                                        <b class="pad-right">Associated Overlays</b>
                                                        <div class="clear-space-two">
                                                        </div>
                                                        <asp:CheckBoxList ID="cb_associatedOverlay" runat="server">
                                                        </asp:CheckBoxList>
                                                        <asp:HiddenField ID="hf_AppOverlay" runat="server" />
                                                        <div class="clear-space-five">
                                                        </div>
                                                    </asp:Panel>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <div class="float-left">
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Min Width
                                                    </div>
                                                    <asp:TextBox ID="tb_minwidth_edit" runat="server" CssClass="TextBoxEdit" Width="50px"></asp:TextBox><span
                                                        class="pad-left">px</span>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Min Height
                                                    </div>
                                                    <asp:TextBox ID="tb_minheight_edit" runat="server" CssClass="TextBoxEdit" Width="50px"></asp:TextBox><span
                                                        class="pad-left">px</span>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Allow Maximize
                                                    </div>
                                                    <asp:DropDownList ID="dd_allowmax_edit" runat="server">
                                                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                        <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Allow Resize
                                                    </div>
                                                    <asp:DropDownList ID="dd_allowresize_edit" runat="server">
                                                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                        <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="float-left">
                                                        <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                            Default Workspace
                                                        </div>
                                                        <asp:DropDownList ID="dd_defaultworkspace_edit" runat="server" Style="width: 65px;">
                                                            <asp:ListItem Text="1" Value="1"></asp:ListItem>
                                                            <asp:ListItem Text="2" Value="2"></asp:ListItem>
                                                            <asp:ListItem Text="3" Value="3"></asp:ListItem>
                                                            <asp:ListItem Text="4" Value="4"></asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Background
                                                    </div>
                                                    <asp:DropDownList ID="dd_enablebg_edit" runat="server">
                                                        <asp:ListItem Text="Visible" Value="app-main"></asp:ListItem>
                                                        <asp:ListItem Text="Hidden" Value="app-main-nobg"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Allow Pop Out
                                                    </div>
                                                    <asp:DropDownList ID="dd_allowpopout_edit" runat="server">
                                                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                        <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                                                    </asp:DropDownList>
                                                </div>
                                                <div class="float-left" style="padding-left: 75px;">
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 115px;">
                                                        Max on Load
                                                    </div>
                                                    <asp:DropDownList ID="dd_maxonload_edit" runat="server" Style="width: 75px;">
                                                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                        <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="float-left">
                                                        <div class="inline-block font-bold pad-right" align="right" style="width: 115px;">
                                                            Allow Params
                                                        </div>
                                                        <asp:DropDownList ID="dd_allow_params_edit" runat="server">
                                                            <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                            <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 115px;">
                                                        Auto Open
                                                    </div>
                                                    <asp:DropDownList ID="dd_autoOpen_edit" runat="server" Style="width: 75px;">
                                                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                        <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <div id="div_isprivate_edit" runat="server">
                                                        <div class="clear-space">
                                                        </div>
                                                        <div class="inline-block font-bold pad-right" align="right" style="width: 115px;">
                                                            Is Private
                                                        </div>
                                                        <asp:DropDownList ID="dd_isPrivate_Edit" runat="server" Style="width: 75px;">
                                                            <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                            <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                    <asp:Panel ID="pnl_autocreate_edit" runat="server">
                                                        <div class="clear-space">
                                                        </div>
                                                        <div class="float-left">
                                                            <div class="inline-block font-bold pad-right" align="right" style="width: 115px;" title="Automatically create the app on the page initialization instead of dynamically">
                                                                Auto Create
                                                            </div>
                                                            <asp:DropDownList ID="dd_AutoLoad_edit" runat="server" Style="width: 75px;">
                                                                <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                                <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                            </asp:DropDownList>
                                                        </div>
                                                    </asp:Panel>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Pop Out Location
                                                </div>
                                                <asp:TextBox ID="tb_allowpopout_edit" runat="server" CssClass="TextBoxEdit" Width="345px"></asp:TextBox>
                                                <div class="clear-space"></div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Filename
                                                </div>
                                                <asp:TextBox ID="tb_filename_edit" runat="server" CssClass="TextBoxEdit margin-top" Enabled="False" Visible="false"></asp:TextBox>
                                                <span id="changeLoadFile" runat="server">
                                                    <small><a href="#" onclick="LoadDefaultPageSelector();return false;"
                                                        style="color: Blue">Change Load File</a></small>
                                                </span>
                                                <div class="clear-space">
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </div>
                                    <div class="clear-space">
                                    </div>
                                    <div class="clear-space">
                                    </div>

                                </asp:Panel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:UpdatePanel ID="updatepnl_btnedit" runat="server">
                                <ContentTemplate>
                                    <asp:Button ID="btn_save" Width="75px" runat="server" Text="Save" OnClick="btn_save_Click" CssClass="input-buttons" Enabled="false" Visible="false" OnClientClick="openWSE.LoadingMessage1('Loading. Please Wait...');" />
                                    <asp:Button ID="btn_save_2" Width="75px" runat="server" Text="Save" OnClick="btn_save_Click"
                                        CssClass="input-buttons" Enabled="false" Visible="false" OnClientClick="openWSE.LoadingMessage1('Loading. Please Wait...');"
                                        Style="display: none" />
                                    <asp:Button ID="btn_cancel" Width="75px" runat="server" Text="Cancel" OnClick="btn_cancel_Click" CssClass="input-buttons" Enabled="false" Visible="false" OnClientClick="openWSE.LoadingMessage1('Loading. Please Wait...');" />
                                    <asp:Button ID="btn_delete" Width="75px" runat="server" Text="Delete" Visible="false" Enabled="false"
                                        CssClass="input-buttons" OnClientClick="OnDelete();" />
                                    <asp:Button ID="btn_edit" Width="75px" runat="server" Text="Edit" OnClick="btn_edit_Click" CssClass="input-buttons" Enabled="false" Visible="false" OnClientClick="openWSE.LoadingMessage1('Loading. Please Wait...');" />
                                    <input type="button" class="input-buttons" onclick="openWSE.LoadModalWindow(false, 'App-element', ''); $('#wlmd_editor_holder').hide(); $('#MainContent_tb_title_edit').val('');"
                                        value="Close" style="width: 75px; margin-right: 5px!important;" />
                                    <div class="clear-space">
                                    </div>
                                    <asp:HiddenField ID="hf_appchange" runat="server" OnValueChanged="hf_appchange_ValueChanged"
                                        Value="" />
                                    <asp:LinkButton ID="lb_editsource" runat="server" OnClick="lb_editsource_Click" Enabled="false"
                                        Visible="false" CssClass="float-left margin-right-sml RandomActionBtns margin-right"><span class="td-edit-btn float-left margin-right-sml" style="padding: 0px!important;"></span>Edit Source Code</asp:LinkButton>
                                    <iframe id="iframe-appDownloader" frameborder="0" height="31px" width="150px"
                                        scrolling="no"></iframe>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="password-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="490">
                        <div class='ModalHeader'>
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#close" onclick="CancelRequest();return false;" class="ModalExitButton"></a>
                                </div>
                                <span class='Modal-title'></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:HiddenField ID="hf_appdeleteid" runat="server" ClientIDMode="Static" />
                                <asp:UpdatePanel ID="updatepnl_passwordConfirm" runat="server">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_passwordConfirm" runat="server" DefaultButton="btn_passwordConfirm">
                                            <span class="password-hint">Enter the password of the user who created the app.</span>
                                            <div class="clear-space"></div>
                                            <b class="pad-right">Password</b>
                                            <asp:TextBox ID="tb_passwordConfirm" runat="server" TextMode="Password" CssClass="TextBoxControls"></asp:TextBox>
                                            <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons margin-left"
                                                Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="openWSE.LoadingMessage1('Validating Password...');"
                                                Style="margin-top: -2px; margin-right: 5px!important" />
                                            <input type="button" class="input-buttons" value="Cancel" onclick="CancelRequest()"
                                                style="margin-top: -2px" />
                                            <div class="clear-space"></div>
                                        </asp:Panel>
                                        <asp:HiddenField ID="hf_StartDelete" runat="server" OnValueChanged="hf_StartDelete_Changed"
                                            ClientIDMode="Static" />
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="LoaderApp-element" class="Modal-element" style="display: none;">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class='Modal-element-modal' data-setwidth="400">
                        <div class='ModalHeader'>
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#close" onclick="openWSE.LoadModalWindow(false, 'LoaderApp-element', '');return false;" class="ModalExitButton"></a>
                                </div>
                                <span class='Modal-title'></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="updatepnl_LoaderApp" runat="server">
                                    <ContentTemplate>
                                        <asp:RadioButtonList ID="radioButton_FileList" runat="server">
                                        </asp:RadioButtonList>
                                        <div class="clear-space">
                                        </div>
                                        <asp:Button ID="btn_updateLoaderFile" runat="server" CssClass="input-buttons RandomActionBtns"
                                            Text="Update Filename" OnClick="btn_updateLoaderFile_Clicked" />
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="LoaderAppNew-element" class="Modal-element" style="display: none;">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class='Modal-element-modal' data-setwidth="400">
                        <div class='ModalHeader'>
                            <div>
                                <span class='Modal-title'></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:RadioButtonList ID="radioButton_FileList_New" runat="server">
                                </asp:RadioButtonList>
                                <div class="clear-space">
                                </div>
                                <asp:UpdatePanel ID="updatePnl_LoaderFileBtns" runat="server">
                                    <ContentTemplate>
                                        <asp:Button ID="btn_updateLoaderFileNew" runat="server" CssClass="input-buttons RandomActionBtns"
                                            Text="Save" OnClick="btn_updateLoaderFileNew_Clicked" />
                                        <asp:Button ID="btn_updateLoaderFileCancel" runat="server" CssClass="input-buttons" ClientIDMode="Static"
                                            Text="Cancel" OnClientClick="return ConfirmLoaderFileCancel(this);" />
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <input id="hidden_temp_script" type="hidden" value="$(document).ready(function () { });" />
        <input id="hidden_temp_html" type="hidden" value="" />
        <script src='<%=ResolveUrl("~/Scripts/SiteTools/appmanager.js")%>' type="text/javascript"></script>
        <script src='<%=ResolveUrl("~/Scripts/AceEditor/ace.js")%>' type="text/javascript" charset="utf-8"></script>
        <script type="text/javascript">
            var canContinue = false;
            $(window).unload(function () {
                $("#pnl_htmleditor,#hidden_editor,#hf_saveapp,#hf_createapp").remove();
            });

            var tempId = "";
            function appchange(id) {
                openWSE.LoadingMessage1("Loading...");
                var inner = $.trim($("#MainContent_wlmd_holder").html());
                if ((inner != "") && ($("#App-element").css("display") == "block")) {
                    setTimeout(function () {
                        openWSE.RemoveUpdateModal();
                    }, 500);
                    return false;
                }
                if ($("#MainContent_tb_title_edit").val() == "") {
                    if (id == "reset") {
                        id = tempId;
                    }

                    if (document.getElementById('<%=hf_appchange.ClientID%>').value != id) {
                        if (document.getElementById('<%=hf_isParams.ClientID%>').value == "0") {
                            document.getElementById('<%=wlmd_holder.ClientID%>').innerHTML = "";
                            document.getElementById('<%=hf_appchange.ClientID %>').value = id;
                            __doPostBack('<%=hf_appchange.ClientID%>', "");
                        }
                        else {
                            if (document.getElementById('<%=hf_appchange_params.ClientID%>').value != id) {
                                document.getElementById('<%=hf_appchange_params.ClientID %>').value = id;
                                __doPostBack('<%=hf_appchange_params.ClientID %>', "");
                            }
                            else {
                                setTimeout(function () { openWSE.RemoveUpdateModal(); }, 500);
                            }
                        }
                    }
                    else {
                        tempId = id;
                        document.getElementById('<%=hf_appchange.ClientID %>').value = "reset";
                        __doPostBack('<%=hf_appchange.ClientID%>', "");
                    }
                }
            }
        </script>
    </div>
</asp:Content>
