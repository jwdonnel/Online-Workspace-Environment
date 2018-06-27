<%@ Page Title="App Editor" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="AppManager.aspx.cs" Inherits="SiteTools_AppManager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            App Manager
        </div>
        <div class="title-line"></div>
    </div>
    <asp:Panel ID="pnlLinkBtns" runat="server" data-overwriteevent="true">
    </asp:Panel>
    <asp:HiddenField ID="hf_isParams" runat="server" ClientIDMode="Static" Value="0" />
    <asp:Panel ID="pnl_app_params" runat="server" Enabled="false" Visible="false">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
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
                                    <asp:HiddenField ID="hf_appchange_paramsdesc_update" runat="server" ClientIDMode="Static" />
                                    <asp:HiddenField ID="hf_appchange_params_cancel" runat="server" ClientIDMode="Static"
                                        OnValueChanged="hf_appchange_params_cancel_Changed" />
                                    <asp:Label ID="lbl_params_tip" runat="server" CssClass="pad-left"
                                        Text="Select a app to view/add/edit parameters"></asp:Label>
                                    <asp:Panel ID="pnl_params_holder" runat="server" Enabled="false" Visible="false">
                                        <div class="clear-margin">
                                            <asp:LinkButton ID="lbtn_close_params" CssClass="rbbuttons float-right"
                                                OnClick="lbtn_close_params_Click" runat="server" ToolTip="Close">Close Parameters</asp:LinkButton>
                                            <asp:Literal ID="ltl_app_params" runat="server"></asp:Literal>
                                        </div>
                                        <div class="clear">
                                        </div>
                                        <asp:HiddenField ID="hf_btnapp_addParms" runat="server" ClientIDMode="Static" OnValueChanged="btn_app_params_Click" />
                                        <asp:HiddenField ID="hf_app_params" runat="server" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hf_app_params_description" runat="server" ClientIDMode="Static" />
                                        <asp:Label ID="lbl_param_error" runat="server" ForeColor="Red" Text=""></asp:Label>
                                        <asp:Panel ID="pnl_app_params_holder" runat="server">
                                        </asp:Panel>
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnl_app_EditList" runat="server" ClientIDMode="Static">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
                <div class="clear"></div>
                <div class="float-left">
                    <b class="pad-right">Category</b>
                    <asp:DropDownList ID="ddl_categories" runat="server" ClientIDMode="Static" AutoPostBack="true" OnSelectedIndexChanged="ddl_categories_Changed">
                    </asp:DropDownList>
                </div>
                <asp:Panel ID="pnl_backupAllApps" runat="server" CssClass="float-right">
                    <iframe src="../iframes/AppDownloadBtn.aspx?backup=true" frameborder="0" height="31px"
                        width="190px" scrolling="no"></iframe>
                </asp:Panel>
                <div class="clear"></div>
                <asp:UpdatePanel ID="updatePnl_AppList" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_AppList" runat="server" ClientIDMode="Static" Style="border-top: none!important;"></asp:Panel>
                        <div class="clear-space"></div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="ddl_categories" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                Select an app above to view details. Apps without a delete button are standard apps that can only be deleted by the Administrator.
            </div>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnl_app_information" runat="server" Enabled="false" Visible="false" ClientIDMode="Static">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
                <a href="#" id="app_templatebtn" runat="server" visible="false" class="float-right" onclick="openWSE.LoadModalWindow(true, 'DownloadAppTemplate-element', 'Select Type'); return false;">App Templates</a>
                <div class="float-left">
                    <asp:Button ID="btn_create_easy" runat="server" Text="Create App" CssClass="input-buttons-create margin-right float-left"
                        Enabled="false" Visible="false" OnClick="btn_createEasy_Click" />
                    <asp:Button ID="btn_uploadnew" runat="server" Text="Upload App" CssClass="input-buttons-create margin-right float-left"
                        Enabled="false" Visible="false" OnClick="btn_uploadnew_Click" OnClientClick="if (!ValidateForm()){return false;}" />
                    <asp:Button ID="btn_clear_controls" runat="server" OnClick="btn_clear_controls_Click"
                        Text="Clear Controls" CssClass="input-buttons-create margin-right float-left"
                        OnClientClick="loadingPopup.Message('Clearing Controls. Please Wait...');" Style="margin-right: 20px!important;" />
                    <div class="clear-space">
                    </div>
                    <span id="lbl_ErrorUpload"></span>
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
                <div class="input-settings-holder">
                    <span class="font-bold">App Name</span>
                    <div class="clear-space-two"></div>
                    <asp:TextBox ID="tb_appname" CssClass="textEntry" runat="server" Width="210px" MaxLength="150"></asp:TextBox>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Description</span>
                    <div class="clear-space-two"></div>
                    <asp:TextBox ID="tb_description_create" runat="server" CssClass="textEntry" AutoPostBack="False" Width="100%"></asp:TextBox>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">About</span>
                    <div class="clear-space-two"></div>
                    <asp:TextBox ID="tb_about_create" runat="server" CssClass="textEntry" AutoPostBack="False" Width="100%"></asp:TextBox>
                </div>
                <asp:Panel ID="pnl_filename" runat="server" Style="display: none;">
                    <div class="input-settings-holder">
                        <span class="font-bold">Filename Extension</span>
                        <div class="clear-space-two"></div>
                        <asp:TextBox ID="tb_filename_create" CssClass="textEntry" runat="server" Width="210px"
                            Enabled="false" MaxLength="150" BackColor="#EFEFEF" data-allowallchars="true"></asp:TextBox>
                        <asp:Label ID="lbl_dotHtml" runat="server" Text=".html" Enabled="false" Visible="false"></asp:Label>
                        <div class="clear-space-two"></div>
                        <small>Filenames are automatically generated. File extension cannot be changed</small>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_apphtml" runat="server" Enabled="false" Visible="false">
                    <div class="input-settings-holder">
                        <span class="font-bold">Html Link</span>
                        <div class="clear-space-two"></div>
                        <asp:TextBox ID="tb_html_create" runat="server" CssClass="textEntry" AutoPostBack="False" Width="100%" data-allowallchars="true"></asp:TextBox>
                        <div class="clear-space-two"></div>
                        <small>Icon will be downloaded from html link automatically if available</small>
                    </div>
                </asp:Panel>
                <div id="newupload" class="input-settings-holder" style="display: none;">
                    <span class="font-bold">Upload File</span>
                    <div class="clear-space-two"></div>
                    <asp:FileUpload ID="fu_uploadnew" runat="server" />
                    <div class="clear-space-two"></div>
                    <small>.zip, .html, .dll, .htm, .txt, .aspx, .ascx, .pdf, Word files, and Excel files are only allowed</small>
                </div>
                <div id="zipfileLoadname" style="display: none">
                    <div class="input-settings-holder">
                        <span class="font-bold">App Load File</span>
                        <div class="clear-space-two"></div>
                        <asp:FileUpload ID="FileUpload1" runat="server" />
                        <div class="clear-space-two"></div>
                        <small>You will need to specify the filename that will be used to load the app. (e.g. AppFile.html) A dialog box will display allowing you to choose which file to use after uploading app</small>
                    </div>
                </div>
                <asp:Panel ID="pnl_appicon" runat="server">
                    <div class="input-settings-holder">
                        <span class="font-bold"></span>
                        <div class="clear-space-two"></div>
                        <a href="#" id="urlIcon-tab" onclick="ChangeIconUploadType(0);return false;">Click here to use Url Image</a>
                        <a href="#" id="uploadIcon-tab" onclick="ChangeIconUploadType(1);return false;" style="display: none;">Click here to Upload Icon</a>
                        <div class="clear"></div>
                        <div id="uploadIcon" class="pad-top">
                            <asp:FileUpload ID="fu_image_create" runat="server" />
                        </div>
                        <div id="urlIcon" class="pad-top" style="display: none">
                            <asp:TextBox ID="tb_imageurl" runat="server" CssClass="textEntry" Width="100%"></asp:TextBox>
                        </div>
                        <div class="clear-space-two"></div>
                        <small>.png, .jpeg, and .gif only allowed</small>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnl_new_AssociatedOverlay" runat="server">
                    <div class="input-settings-holder">
                        <span class="font-bold">Associated Overlays</span>
                        <div class="clear-space-two"></div>
                        <asp:CheckBoxList ID="cc_associatedOverlayNew" runat="server">
                        </asp:CheckBoxList>
                        <div class="clear"></div>
                    </div>
                </asp:Panel>
                <div class="input-settings-holder">
                    <span class="font-bold">Category</span>
                    <div class="clear-space-two"></div>
                    <asp:CheckBoxList ID="dd_category" runat="server">
                    </asp:CheckBoxList>
                    <div class="clear-space-two"></div>
                    <small>Put the app into a category to help organize the apps</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Minimum Width</span>
                    <div class="clear-space-two"></div>
                    <asp:TextBox ID="tb_minwidth_create" runat="server" CssClass="textEntry-noWidth" Width="65px" MaxLength="4" Text="500" TextMode="Number"></asp:TextBox><span class="pad-left">px</span>
                    <div class="clear-space-two"></div>
                    <small>Set the minimum width of the app</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Minimum Height</span>
                    <div class="clear-space-two"></div>
                    <asp:TextBox ID="tb_minheight_create" runat="server" CssClass="textEntry-noWidth" Width="65px" MaxLength="4" Text="400" TextMode="Number"></asp:TextBox><span class="pad-left">px</span>
                    <div class="clear-space-two"></div>
                    <small>Set the minimum height of the app</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Icon Background Color</span>
                    <div class="clear-space-two"></div>
                    <asp:TextBox ID="tb_iconColor_create" runat="server" CssClass="textEntry-noWidth float-left margin-right" TextMode="Color" Width="75px" MaxLength="7" Text="#FFFFFF"></asp:TextBox>
                    <div class="div_usedefaultcolor float-left margin-top">
                        <asp:CheckBox ID="cb_iconColor_create_default" runat="server" Checked="true" />
                        <asp:Label ID="lbl_iconColor_create_default" runat="server" AssociatedControlID="cb_iconColor_create_default" Text="Use Inherit"></asp:Label>
                    </div>
                    <div class="clear-space-two"></div>
                    <small>Set the icon background color that can be used for select App Selector settings</small>
                </div>
                <asp:Panel ID="pnl_AppPackage" runat="server">
                    <div class="input-settings-holder">
                        <span class="font-bold">App Package</span>
                        <div class="clear-space-two"></div>
                        <asp:CheckBoxList ID="dd_package" runat="server">
                        </asp:CheckBoxList>
                        <div class="clear-space-two"></div>
                        <small>Select a app package that you want the created app to be in</small>
                    </div>
                </asp:Panel>
                <div class="input-settings-holder">
                    <span class="font-bold">Allow Pop Out</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_allowpopout_create" runat="server">
                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                        <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>You can set a popout location for an app that allows users to open the app in a seperate window outside the workspace</small>
                </div>
                <div id="popoutlocdiv" class="input-settings-holder" style="display: none;">
                    <span class="font-bold">Pop Out Location</span>
                    <div class="clear-space-two"></div>
                    <asp:TextBox ID="tb_popoutLoc_create" runat="server" CssClass="textEntry" Width="100%" data-allowallchars="true"></asp:TextBox>
                    <div class="clear-space-two"></div>
                    <small>Set the location of where you want the popup window to take the user to</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Background</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_enablebg_create" runat="server" Style="width: 105px">
                        <asp:ListItem Text="Visible" Value="app-main"></asp:ListItem>
                        <asp:ListItem Text="Hidden" Value="app-main-nobg"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>
                        <b>Visible: </b>Will show the app background and controls<br />
                        <b>Hidden: </b>Will hide the background and controls. Controls will appear when hovering over the app
                    </small>
                </div>
                <div id="backgroundcolorholder_create" class="input-settings-holder">
                    <span class="font-bold">Background Color</span>
                    <div class="clear-space-two"></div>
                    <asp:TextBox ID="tb_backgroundColor_create" runat="server" CssClass="textEntry-noWidth float-left margin-right" TextMode="Color" Width="75px" MaxLength="7" Text="#FFFFFF"></asp:TextBox>
                    <div class="div_usedefaultcolor float-left margin-top">
                        <asp:CheckBox ID="cb_backgroundColor_create_default" runat="server" Checked="true" />
                        <asp:Label ID="lbl_backgroundColor_create_default" runat="server" AssociatedControlID="cb_backgroundColor_create_default" Text="Use Inherit"></asp:Label>
                    </div>
                    <div class="clear-space-two"></div>
                    <small>Set the app background color</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Max on Load</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_maxonload_create" runat="server" Style="width: 75px;">
                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                        <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>Set to True to force the app to expand to a full screen every time you load it</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Auto Open</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_autoOpen_create" runat="server" Style="width: 75px;">
                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                        <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>Set to True to automatically open this app when loading the workspace. App can be closed but will not be saved</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Allow Overrides</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_allowUserOverrides" runat="server" Style="width: 75px;">
                        <asp:ListItem Text="True" Value="1" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="False" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>Set to True to allow users to override certain settings for this app</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Allow Resize</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_allowresize_create" runat="server" Style="width: 75px;">
                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                        <asp:ListItem Text="False" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>Set to True to allow for the app to be resized</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Allow Maximize</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_allowmax_create" runat="server" Style="width: 75px;">
                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                        <asp:ListItem Text="False" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>Set to True to allow for the app to maximize. Setting this to false will hide the maximize button in the app header</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Allow Params</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_allow_params" runat="server" Style="width: 75px;">
                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                        <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>Set this to True to allow for parameters to be setup in the App Params page. Please note that the app must be coded to allow for the parameters to work</small>
                </div>
                <div id="div_AllowNotifications_Create" class="input-settings-holder" runat="server">
                    <span class="font-bold">Allow Notifications</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_allowNotifications_create" runat="server" Style="width: 75px;">
                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                        <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>Set this to True to allow for notifications. Please note that the app must be coded to allow for the notifications to work</small>
                </div>
                <div class="input-settings-holder">
                    <span class="font-bold">Default Workspace</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_defaultworkspace_create" runat="server" Style="width: 75px;">
                    </asp:DropDownList>
                    <div class="clear-space-two"></div>
                    <small>Select the workspace you want the app to load to by default</small>
                </div>
                <div class="input-settings-holder" style="display: none">
                    <span id="span-autocreate" class="font-bold">Auto Create</span>
                    <div class="clear-space-two"></div>
                    <asp:DropDownList ID="dd_autocreate_create" runat="server" ClientIDMode="Static" Style="width: 75px;">
                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                        <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
        </div>
        <div class="clear"></div>
    </asp:Panel>
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
                <asp:Button ID="lbtn_close" CssClass="input-buttons-create float-left margin-right RandomActionBtns" OnClick="lbtn_close_Click" runat="server" ToolTip="Close" Text="Back"></asp:Button>
                <input type="button" id="lbtn_save" class="input-buttons-create margin-left float-left" onclick="SaveApp_Click();" title="Save/Overwrite" value="Save" />
            </asp:Panel>
            <asp:Panel ID="pnl_htmleditor" runat="server" ClientIDMode="Static" Style="display: none; margin-top: 15px">
                <div id="btn_backProp" runat="server" class="float-left">
                    <input type="button" class="input-buttons-create" value="Back" onclick="ViewCode();" />
                </div>
                <div class="clear-space">
                </div>
                <div id="JAVASCRIPTCODE">
                    <asp:Literal ID="links_externalCode" runat="server"></asp:Literal>
                    <div class="clear-space">
                    </div>
                    <div id="editor">
                    </div>
                    <asp:HiddenField ID="hidden_editor" runat="server" ClientIDMode="Static" />
                    <asp:HiddenField ID="hf_saveapp" runat="server" ClientIDMode="Static" OnValueChanged="hf_saveapp_Changed" />
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
                                                    runat="server" CssClass="textEntry"></asp:TextBox>
                                                <div class="clear-space">
                                                </div>
                                                <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                                                    <ContentTemplate>
                                                        <b class='float-left pad-top-sml pad-right'>Category</b>
                                                        <asp:CheckBoxList ID="dd_category_edit" runat="server" RepeatDirection="Vertical" RepeatColumns="3">
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
                                            <asp:TextBox ID="tb_description_edit" runat="server" CssClass="textEntry-noWidth" Width="99%"></asp:TextBox>
                                            <div class="clear" style="height: 15px;">
                                            </div>
                                            <b>About</b><div class="clear-space-five">
                                            </div>
                                            <asp:TextBox ID="tb_about_edit" runat="server" CssClass="textEntry-noWidth" Width="99%"></asp:TextBox>
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
                                                <asp:TextBox ID="tb_minwidth_edit" runat="server" CssClass="textEntry-noWidth" Width="75px" TextMode="Number"></asp:TextBox><span
                                                    class="pad-left">px</span>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Min Height
                                                </div>
                                                <asp:TextBox ID="tb_minheight_edit" runat="server" CssClass="textEntry-noWidth" Width="75px" TextMode="Number"></asp:TextBox><span
                                                    class="pad-left">px</span>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Allow Maximize
                                                </div>
                                                <asp:DropDownList ID="dd_allowmax_edit" runat="server" Style="width: 75px;">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Allow Resize
                                                </div>
                                                <asp:DropDownList ID="dd_allowresize_edit" runat="server" Style="width: 75px;">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div class="clear-space">
                                                </div>
                                                <div class="float-left">
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Default Workspace
                                                    </div>
                                                    <asp:DropDownList ID="dd_defaultworkspace_edit" runat="server" Style="width: 75px;">
                                                    </asp:DropDownList>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Background
                                                </div>
                                                <asp:DropDownList ID="dd_enablebg_edit" runat="server" Style="width: 75px;">
                                                    <asp:ListItem Text="Visible" Value="app-main"></asp:ListItem>
                                                    <asp:ListItem Text="Hidden" Value="app-main-nobg"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div id="backgroundcolorholder_edit">
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                        Background Color
                                                    </div>
                                                    <asp:TextBox ID="tb_backgroundColor_edit" runat="server" CssClass="textEntry-noWidth" TextMode="Color" Width="75px" MaxLength="7"></asp:TextBox>
                                                    <div class="clear-space-five"></div>
                                                    <div class="div_usedefaultcolor" style="padding-left: 144px;">
                                                        <asp:CheckBox ID="cb_backgroundColor_edit_default" runat="server" />
                                                        <asp:Label ID="lbl_backgroundColor_edit_default" runat="server" AssociatedControlID="cb_backgroundColor_edit_default" Text="Use Inherit"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Allow Pop Out
                                                </div>
                                                <asp:DropDownList ID="dd_allowpopout_edit" runat="server" Style="width: 75px;">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0" Selected="True"></asp:ListItem>
                                                </asp:DropDownList>
                                            </div>
                                            <div class="float-left" style="padding-left: 75px;">
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 125px;">
                                                    Icon Color
                                                </div>
                                                <asp:TextBox ID="tb_iconColor_edit" runat="server" CssClass="textEntry-noWidth" TextMode="Color" Width="75px" MaxLength="7"></asp:TextBox>
                                                <div class="clear-space-five"></div>
                                                <div class="div_usedefaultcolor" style="padding-left: 140px;">
                                                    <asp:CheckBox ID="cb_iconColor_edit_default" runat="server" />
                                                    <asp:Label ID="lbl_iconColor_edit_default" runat="server" AssociatedControlID="cb_iconColor_edit_default" Text="Use Inherit"></asp:Label>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 125px;">
                                                    Max on Load
                                                </div>
                                                <asp:DropDownList ID="dd_maxonload_edit" runat="server" Style="width: 75px;">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div class="clear-space">
                                                </div>
                                                <div class="float-left">
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 125px;">
                                                        Allow Params
                                                    </div>
                                                    <asp:DropDownList ID="dd_allow_params_edit" runat="server" Style="width: 75px;">
                                                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                        <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                    </asp:DropDownList>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 125px;">
                                                    Auto Open
                                                </div>
                                                <asp:DropDownList ID="dd_autoOpen_edit" runat="server" Style="width: 75px;">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div class="clear-space">
                                                </div>
                                                <div id="div_allowNotifications_edit" class="float-left" runat="server" style="padding-bottom: 10px;">
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 125px;">
                                                        Allow Notifications
                                                    </div>
                                                    <asp:DropDownList ID="dd_allowNotifications_edit" runat="server" Style="width: 75px;">
                                                        <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                        <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                    </asp:DropDownList>
                                                </div>
                                                <div class="clear"></div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 125px;">
                                                    Allow Overrides
                                                </div>
                                                <asp:DropDownList ID="dd_allowUserOverrides_edit" runat="server" Style="width: 75px;">
                                                    <asp:ListItem Text="True" Value="1"></asp:ListItem>
                                                    <asp:ListItem Text="False" Value="0"></asp:ListItem>
                                                </asp:DropDownList>
                                                <div id="div_isprivate_edit" runat="server">
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="inline-block font-bold pad-right" align="right" style="width: 125px;">
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
                                                        <div class="inline-block font-bold pad-right" align="right" style="width: 125px;" title="Automatically create the app on the page initialization instead of dynamically">
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
                                            <asp:TextBox ID="tb_allowpopout_edit" runat="server" CssClass="textEntry-noWidth" Width="345px" data-allowallchars="true"></asp:TextBox>
                                            <div id="changeLoadFile_holder" runat="server">
                                                <div class="clear-space"></div>
                                                <div class="inline-block font-bold pad-right" align="right" style="width: 130px;">
                                                    Location
                                                </div>
                                                <asp:TextBox ID="tb_filename_edit" runat="server" CssClass="textEntry-noWidth margin-top" data-allowallchars="true" Enabled="False" Visible="false" Width="345px"></asp:TextBox>
                                                <span id="changeLoadFile" runat="server">
                                                    <small><a href="#" onclick="LoadDefaultPageSelector();return false;"
                                                        style="color: Blue">Change Load File</a></small>
                                                </span>
                                            </div>
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
                                <asp:Button ID="btn_save" runat="server" Text="Save" OnClick="btn_save_Click" CssClass="input-buttons modal-ok-btn" Enabled="false" Visible="false" OnClientClick="loadingPopup.Message('Loading. Please Wait...');" />
                                <asp:Button ID="btn_save_2" runat="server" Text="Save" OnClick="btn_save_Click"
                                    CssClass="input-buttons modal-ok-btn" Enabled="false" Visible="false" OnClientClick="loadingPopup.Message('Loading. Please Wait...');"
                                    Style="display: none" />
                                <asp:Button ID="btn_edit" runat="server" Text="Edit" OnClick="btn_edit_Click" CssClass="input-buttons modal-ok-btn" Enabled="false" Visible="false" OnClientClick="loadingPopup.Message('Loading. Please Wait...');" />
                                <asp:Button ID="btn_delete" runat="server" Text="Delete" Visible="false" Enabled="false"
                                    CssClass="input-buttons modal-ok-btn" OnClientClick="OnDelete();" />
                                <input type="button" class="input-buttons modal-cancel-btn" onclick="openWSE.LoadModalWindow(false, 'App-element', ''); $('#wlmd_editor_holder').hide(); $('#MainContent_tb_title_edit').val('');"
                                    value="Close" />
                                <asp:Button ID="btn_cancel" runat="server" Text="Cancel" OnClick="btn_cancel_Click" CssClass="input-buttons modal-cancel-btn" Enabled="false" Visible="false" OnClientClick="loadingPopup.Message('Loading. Please Wait...');" />
                                <div class="clear-space">
                                </div>
                                <asp:HiddenField ID="hf_appchange" runat="server" OnValueChanged="hf_appchange_ValueChanged"
                                    Value="" />
                                <asp:LinkButton ID="lb_editsource" runat="server" OnClick="lb_editsource_Click" Enabled="false"
                                    Visible="false" CssClass="float-left margin-right-sml RandomActionBtns margin-right">Edit Source Code</asp:LinkButton>
                                <iframe id="iframe-appDownloader" frameborder="0" height="31px" width="150px"
                                    scrolling="no"></iframe>
                                <div class="clear"></div>
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
                <div class="Modal-element-modal" data-setwidth="400">
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
                                        <asp:TextBox ID="tb_passwordConfirm" runat="server" TextMode="Password" CssClass="textEntry-noWidth"></asp:TextBox>
                                        <div class="clear-space"></div>
                                        <div class="clear-space"></div>
                                        <input type="button" class="input-buttons no-margin float-right" value="Cancel" onclick="CancelRequest()" />
                                        <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons float-right"
                                            Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="loadingPopup.Message('Validating Password...');" />
                                        <div class="clear"></div>
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
    <div id="DownloadAppTemplate-element" class="Modal-element" style="display: none;">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class='Modal-element-modal' data-setwidth="350">
                    <div class='ModalHeader'>
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#close" onclick="openWSE.LoadModalWindow(false, 'DownloadAppTemplate-element', '');return false;" class="ModalExitButton"></a>
                            </div>
                            <span class='Modal-title'></span>
                        </div>
                    </div>
                    <div class="ModalScrollContent">
                        <div class="ModalPadContent">
                            <h4>Select the template type you wish to download. There will be some minor differences between the templates and how they may function on the workspace.</h4>
                            <div class="clear-space"></div>
                            <div class="clear-space"></div>
                            <asp:UpdatePanel ID="updatepnl_templateDownload" runat="server">
                                <ContentTemplate>
                                    <asp:LinkButton ID="lbtn_aspxTemplate" runat="server" OnClick="lbtn_aspxTemplate_Click" PostBackUrl="~/SiteTools/AppMaintenance/AppManager.aspx" Text="Aspx (Asp Page) Template" OnClientClick="openWSE.LoadModalWindow(false, 'DownloadAppTemplate-element', '');"></asp:LinkButton>
                                    <div class="clear-space"></div>
                                    <asp:LinkButton ID="lbtn_ascxTemplate" runat="server" OnClick="lbtn_ascxTemplate_Click" PostBackUrl="~/SiteTools/AppMaintenance/AppManager.aspx" Text="Ascx (User Control) Template" OnClientClick="openWSE.LoadModalWindow(false, 'DownloadAppTemplate-element', '');"></asp:LinkButton>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <input id="hidden_temp_script" type="hidden" value="" />
    <script src='<%=ResolveUrl("~/Scripts/AceEditor/ace.js")%>' type="text/javascript" charset="utf-8"></script>
</asp:Content>
