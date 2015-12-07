<%@ page language="C#" autoeventwireup="true" inherits="SiteTools_ProjectExplorer, App_Web_fcqplbhq" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Project Explorer</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="../../Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="../../Standard_Images/favicon.ico"
        type="image/ico" />
    <link href="../../App_Themes/Standard/site_desktop.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="../../App_Themes/Standard/jqueryUI.css" />
    <style type="text/css">
        .page-list
        {
            list-style: none;
        }

            .page-list li
            {
                padding: 5px 10px;
                border-bottom: 1px solid #E5E5E5;
                line-height: 24px;
            }

                .page-list li:hover
                {
                    background: #F4F4F4;
                }

            .page-list a
            {
                color: #353535;
            }

            .page-list .selectedPage
            {
                font-weight: bold;
                font-size: 15px;
            }

        #editor
        {
            font-size: 14px;
            left: 0;
            position: relative;
            top: 0;
            width: 100%;
        }

        .ace_editor
        {
            position: relative!important;
        }

        .td-cancel-btn, .td-update-btn
        {
            display: block !important;
        }

        .bg-drag-select
        {
            background: #FFF;
            z-index: 5000;
            opacity: 0.7;
            filter: alpha(opacity=70);
        }

        .over-draggable
        {
            background: #FFE884;
        }

        .ui-helper-hidden-accessible
        {
            display: none;
        }

        #AjaxFileUpload1
        {
            overflow: auto;
            max-height: 300px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager_deliverypickups" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <div id="app_title_bg" runat="server" class="app-Settings-title-bg-color-main">
            <div class="pad-all">
                <div class="app-Settings-title-user-info">
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                            <div class="float-left">
                                <asp:Label ID="Label1" runat="server" CssClass="page-title" Text="Project Explorer"></asp:Label>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
        <table cellpadding="0" cellspacing="0" width="100%">
            <tr>
                <td valign="top" style="border-right: 1px solid #DDD; width: 405px;">
                    <asp:UpdatePanel ID="updatepnl1" runat="server">
                        <ContentTemplate>
                            <div id="projectInfo" class="pad-all" style="background: #EFEFEF; border-bottom: 1px solid #DDD;">
                                <asp:Panel ID="NewControlsPnl" runat="server" DefaultButton="btn_saveProject">
                                    <div class="clear-space"></div>
                                    <div class="font-bold pad-right float-left pad-top-sml" style="width: 95px;">Project Name:</div>
                                    <asp:TextBox ID="tb_projectName" runat="server" MaxLength="250" CssClass="TextBoxControls margin-right-big" Width="150px"></asp:TextBox>
                                    <asp:Button ID="btn_saveProject" runat="server" CssClass="input-buttons-create RandomActionBtns no-margin" Text="Save" Width="80px" OnClick="btn_saveProject_Click" />
                                    <div class="clear-space"></div>
                                    <div class="font-bold pad-right float-left pad-top-sml" style="width: 95px;">Description:</div>
                                    <asp:TextBox ID="tb_description" runat="server" CssClass="TextBoxControls margin-right-big" Width="240px"></asp:TextBox>
                                    <div class="clear-space"></div>
                                    <asp:Panel ID="pnl_defaultPage" runat="server">
                                        <div class="font-bold pad-right float-left pad-top-sml" style="width: 95px;">Default Page:</div>
                                        <asp:DropDownList ID="ddl_defaultPage" runat="server" Width="255px"></asp:DropDownList>
                                        <asp:Literal ID="ltl_previewButton" runat="server"></asp:Literal>
                                    </asp:Panel>
                                </asp:Panel>
                            </div>
                            <asp:Panel ID="pnl_pageControls" runat="server">
                                <div id="projectPages" class="pad-all" style="overflow: auto;">
                                    <asp:LinkButton ID="lbtn_addNewPage" runat="server" CssClass="RandomActionBtns" OnClick="lbtn_addNewPage_Click"><span class="td-add-btn float-left margin-right-sml" style="padding:0px; margin-top: 1px;"></span>Add New Page</asp:LinkButton>
                                    <a href="#" class="float-right" onclick="openWSE.LoadModalWindow(true, 'FileUpload-element', 'Upload Files');return false;"><span class="img-upload float-left margin-right-sml"></span>Upload Files</a>
                                    <div class="clear-space"></div>
                                    <asp:LinkButton ID="lbtn_addNewFolder" runat="server" CssClass="RandomActionBtns" OnClick="lbtn_addNewFolder_Click"><span class="td-add-btn float-left margin-right-sml" style="padding:0px; margin-top: 1px;"></span>Add New Folder</asp:LinkButton>
                                    <a href="#" class="float-right" onclick="ClearSelected();return false;" style="font-size: 11px; color: #999;">Clear Selected</a>
                                    <div class="clear-space"></div>
                                    <div class="pad-top-sml pad-bottom-sml">
                                        <small id="current-path-selected"></small>
                                    </div>
                                    <div class="clear-space"></div>
                                    <asp:Panel ID="pnl_pages" runat="server">
                                    </asp:Panel>
                                    <asp:HiddenField ID="hf_LoadSavedPage" runat="server" OnValueChanged="hf_LoadSavedPage_ValueChanged" />
                                    <asp:HiddenField ID="hf_currFolder" runat="server" />
                                    <asp:HiddenField ID="hf_editorID" runat="server" />
                                    <asp:HiddenField ID="hidden_editor" runat="server" OnValueChanged="hidden_editor_ValueChanged" />
                                    <asp:HiddenField ID="hf_deleteFile" runat="server" OnValueChanged="hf_deleteFile_ValueChanged" />
                                    <asp:HiddenField ID="hf_updatePageName" runat="server" OnValueChanged="hf_updatePageName_ValueChanged" />
                                    <asp:HiddenField ID="hf_deleteFolder" runat="server" OnValueChanged="hf_deleteFolder_ValueChanged" />
                                    <asp:HiddenField ID="hf_updateFolderName" runat="server" OnValueChanged="hf_updateFolderName_ValueChanged" />
                                    <asp:HiddenField ID="hf_moveFile" runat="server" OnValueChanged="hf_moveFile_ValueChanged" />
                                    <asp:HiddenField ID="hf_moveFolder" runat="server" OnValueChanged="hf_moveFolder_ValueChanged" />
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnl_nosavedproject" runat="server" Visible="false" Enabled="false" CssClass="pad-all" Style="overflow: auto;">
                                <asp:Label ID="lbl_nosavedproject" runat="server" Text="<h3>Must start a new project</h3><br /><small>Give your project a new name and press 'Save Info'</small>"></asp:Label>
                            </asp:Panel>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
                <td valign="top">
                    <div class="pad-all">
                        <h3 id="selectpagehint">Select an item in the page list</h3>
                        <span id="currFileShowing"></span>
                        <div class="clear-space"></div>
                        <div id="htmlEditor" style="display: none;">
                            <div id="editor">
                            </div>
                            <div class="clear-space"></div>
                            <a id="lbtn_savePage" href="#" class="float-left margin-bottom-big margin-top margin-left margin-right" onclick="SavePage();return false;"><span class="img-backup float-left margin-right-sml"></span>Save Page</a>
                            <a id="lbtn_cancelPage" href="#" class="float-left margin-bottom-big margin-top margin-left margin-right" onclick="Reload_Cancel();return false;"><span class="td-cancel-btn float-left margin-right-sml" style="padding: 0px;"></span>Cancel Changes</a>
                            <a id="lbtn_previewPage" href="#" target="_blank" class="float-left margin-bottom-big margin-top margin-left margin-right"><span class="td-view-btn float-left margin-right-sml" style="padding: 0px;"></span>Preview</a>
                        </div>
                        <img id="imgEditor" alt="" src="" style="display: none; max-width: 800px;" />
                    </div>
                </td>
            </tr>
        </table>
        <div id="FtpProject-element" class="Modal-element" style="display: none;">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class='Modal-element-modal' data-setwidth="400">
                        <div class='ModalHeader'>
                            <div><span class='Modal-title'></span></div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <h4 class="title"></h4>
                                <div class="clear-space"></div>
                                <div class="clear-space"></div>
                                <div class="inline-block" style="margin: auto 0; text-align: left;">
                                    <asp:UpdatePanel ID="updatepnl_FtpLogin" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <asp:Panel ID="pnl_ftpLogin" runat="server" DefaultButton="btn_ftpLogin">
                                                <span class="font-bold">Username</span><br />
                                                <asp:TextBox ID="tb_ftpUsername" runat="server" CssClass="textEntry margin-right" Style="width: 200px;" />
                                                <asp:Button ID="btn_ftpLogin" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="btn_ftpLogin_Click" Text="Ok" Style="width: 65px;" />
                                                <div class="clear-space"></div>
                                                <span class="font-bold">Password</span><br />
                                                <asp:TextBox ID="tb_ftpPassword" runat="server" TextMode="Password" CssClass="textEntry margin-right" Style="width: 200px;" />
                                                <input type="button" class="input-buttons" value="Cancel" onclick="CancelLogin();" style="width: 65px;" />
                                                <div class="clear-space"></div>
                                                <asp:Literal ID="ltl_ftpLogin" runat="server"></asp:Literal>
                                            </asp:Panel>
                                        </ContentTemplate>
                                        <Triggers>
                                            <asp:AsyncPostBackTrigger ControlID="btn_ftpLogin" />
                                        </Triggers>
                                    </asp:UpdatePanel>
                                </div>
                                <div class="clear-space"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="FileUpload-element" class="Modal-element">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="510">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'FileUpload-element', '');return false;"
                                    class="ModalExitButton"></a>
                            </div>
                            <span class="Modal-title"></span>
                        </div>
                    </div>
                    <div class="ModalScrollContent">
                        <div class="ModalPadContent">
                            <div id="fileuploader" class="clear-margin">
                                Select file(s) and press the Upload Files button. Your uploaded files will be<br />
                                placed into the currently selected folder.
                                <div class="clear-space-five"></div>
                                <asp:FileUpload ID="FileUploadControl" runat="server" AllowMultiple="true" />
                                <div class="clear-space"></div>
                                <div align="right">
                                    <asp:Button ID="btnFileUpload" runat="server" Text="Upload Files" CssClass="RandomActionBtns input-buttons float-left" OnClick="btnFileUpload_OnClick" />
                                    <input type="button" id="btn_close_uploader" class="input-buttons" value="Close"
                                        onclick="openWSE.LoadModalWindow(false, 'FileUpload-element', '');"
                                        style="margin-right: 0px" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src="<%=ResolveUrl("~/Scripts/SiteCalls/Min/openwse.min.js") %>"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/Scripts/AceEditor/ace.js") %>" charset="utf-8"></script>
        <script type="text/javascript" src="<%=ResolveUrl("~/Scripts/SiteTools/projectexplorer.js") %>"></script>
        <script type="text/javascript">
            $(document).ready(function () {
                openWSE_Config.siteRootFolder = "<%=ResolveUrl("~/").Replace("/", "") %>";
            });
        </script>
    </form>
</body>
</html>
