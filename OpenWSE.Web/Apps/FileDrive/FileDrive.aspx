<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FileDrive.aspx.cs" Inherits="Apps_FileDrive" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>File Drive</title>
    <link type="text/css" rel="Stylesheet" href="filedrive.css" />
</head>
<body>
    <div id="mydocuments-load" class="main-div-app-bg">
        <form id="form_mydocuments" runat="server" enctype="multipart/form-data" method="post">
            <asp:ScriptManager ID="ScriptManager_filedrive" runat="server" AsyncPostBackTimeout="360000">
            </asp:ScriptManager>
            <div class="content-main" style="position: relative; z-index: 0;">
                <table width="100%" cellspacing="0" cellpadding="0">
                    <tbody>
                        <tr>
                            <td class="td-content-inner">
                                <div class="content-overflow-app">
                                    <div id="audioPlayer">
                                    </div>
                                    <asp:UpdatePanel ID="UpdatePanel1_documents" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <div id="top-bar-foldergroup-holder">
                                                <asp:HiddenField ID="hf_UpdateAll" runat="server" OnValueChanged="hf_UpdateAll_ValueChanged" />
                                                <div id="pnl_folderlist" class="float-left">
                                                    <h3>Folder List</h3>
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <asp:DropDownList ID="dd_folders" runat="server" CssClass="margin-right margin-bottom" OnSelectedIndexChanged="dd_folders_Changed" AutoPostBack="true">
                                                    </asp:DropDownList>
                                                    <asp:LinkButton ID="lbtn_editfolders" runat="server" Text="Edit Folders" CssClass="RandomActionBtns" OnClick="lbtn_editfolders_Click"></asp:LinkButton>
                                                    <div class="clear"></div>
                                                </div>
                                                <asp:Panel ID="pnl_grouplist" runat="server" CssClass="float-right">
                                                    <h3>Available Groups</h3>
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <asp:DropDownList ID="dd_groups" runat="server" CssClass="float-right margin-bottom">
                                                    </asp:DropDownList>
                                                    <asp:HiddenField ID="hf_groupsChange" runat="server" OnValueChanged="hf_groupsChange_ValueChanged" />
                                                    <div class="clear"></div>
                                                </asp:Panel>
                                                <div class="clear">
                                                </div>
                                            </div>
                                        </ContentTemplate>
                                        <Triggers>
                                            <asp:AsyncPostBackTrigger ControlID="hf_UpdateAll" />
                                            <asp:AsyncPostBackTrigger ControlID="hf_groupsChange" />
                                            <asp:AsyncPostBackTrigger ControlID="dd_folders" />
                                            <asp:AsyncPostBackTrigger ControlID="lbtn_editfolders" />
                                        </Triggers>
                                    </asp:UpdatePanel>
                                    <div class="clear"></div>
                                    <asp:UpdatePanel ID="UpdatePanel2_documents" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <div class="pad-left-big pad-right-big">
                                                <asp:Label ID="lbl_fileNoti_documents" runat="server" Font-Italic="true" Font-Size="12px" Text=""></asp:Label>
                                                <a class="input-buttons float-left" onclick="openWSE.LoadModalWindow(true, 'FileUpload-element', 'File Upload');return false;">Add Files</a>
                                                <asp:LinkButton ID="refresh_Click_documents" runat="server" CssClass="float-right img-refresh pad-all RandomActionBtns" OnClick="btn_refresh_Click" Style="outline: none;" />
                                                <div class="clear-space-two"></div>
                                                <div id="movetofolder-holder" class="float-left" style="display: none;">
                                                    Move selected file(s)
                                                    <div class="clear-space-five"></div>
                                                    <asp:Panel ID="pnl_moveToGroup" runat="server" CssClass="float-left margin-right-big">
                                                        <span class="font-bold">Group</span>
                                                        <div class="clear-space-two"></div>
                                                        <asp:DropDownList ID="dd_moveGroup_documents" runat="server">
                                                        </asp:DropDownList>
                                                        <asp:HiddenField ID="hf_moveGroupChange" runat="server" OnValueChanged="hf_moveGroupChange_ValueChanged" />
                                                    </asp:Panel>
                                                    <div class="float-left margin-right-big">
                                                        <span class="font-bold">Folder</span>
                                                        <div class="clear-space-two"></div>
                                                        <asp:DropDownList ID="dd_moveFolder_documents" runat="server">
                                                        </asp:DropDownList>
                                                    </div>
                                                    <asp:Button ID="btn_moveFile_documents" runat="server" CssClass="input-buttons float-left" OnClick="btn_moveFile_Click" Text="Move Selected" Style="margin-top: 15px;" />
                                                    <asp:HiddenField ID="hf_moveFiles_documents" runat="server" ClientIDMode="Static" />
                                                    <div class="clear-space">
                                                    </div>
                                                    <a onclick="DeleteSelectedFiles();return false;" style="font-size: 11px;">Delete Files</a>
                                                    <div class="clear-space">
                                                    </div>
                                                </div>
                                                <div class="clear">
                                                </div>
                                                <asp:HiddenField ID="hf_fileEdit" runat="server" OnValueChanged="hf_fileEdit_ValueChanged" />
                                                <asp:HiddenField ID="hf_fileDelete" runat="server" OnValueChanged="hf_fileDelete_ValueChanged" />
                                                <asp:HiddenField ID="hf_fileDeleteSelected" runat="server" OnValueChanged="hf_fileDeleteSelected_ValueChanged" />
                                                <asp:HiddenField ID="hf_fileUpdate" runat="server" OnValueChanged="hf_fileUpdate_ValueChanged" />
                                                <asp:HiddenField ID="hf_fileUpdateName" runat="server" />
                                                <asp:HiddenField ID="hf_fileUpdateExt" runat="server" />
                                                <asp:HiddenField ID="hf_fileUpdateFolder" runat="server" />
                                                <asp:Button ID="btn_DownloadFile" runat="server" PostBackUrl="~/Apps/FileDrive/FileDrive.aspx" OnClick="btn_DownloadFile_Click" Style="display: none;" />
                                                <asp:HiddenField ID="hf_fileDownload" runat="server" />
                                                <asp:Panel ID="pnl_FilesDocuments" runat="server" ClientIDMode="Static"></asp:Panel>
                                            </div>
                                        </ContentTemplate>
                                        <Triggers>
                                            <asp:AsyncPostBackTrigger ControlID="refresh_Click_documents" />
                                            <asp:AsyncPostBackTrigger ControlID="btn_moveFile_documents" />
                                            <asp:AsyncPostBackTrigger ControlID="hf_fileEdit" />
                                            <asp:AsyncPostBackTrigger ControlID="hf_fileDelete" />
                                            <asp:AsyncPostBackTrigger ControlID="hf_fileDeleteSelected" />
                                            <asp:PostBackTrigger ControlID="btn_DownloadFile" />
                                            <asp:AsyncPostBackTrigger ControlID="hf_fileUpdate" />
                                            <asp:AsyncPostBackTrigger ControlID="hf_moveGroupChange" />
                                        </Triggers>
                                    </asp:UpdatePanel>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div id="FileUpload-element" class="Modal-element">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal">
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
                                <div id="fileuploader" class="clear-margin" style="width: 300px;">
                                    Select file(s) and press the Upload Files button
                                <div class="clear-space-five"></div>
                                    <asp:FileUpload ID="FileUploadControl" runat="server" AllowMultiple="true" />
                                    <div class="clear-space"></div>
                                    <asp:Button ID="btnFileUpload" runat="server" Text="Upload Files" Width="95px" CssClass="RandomActionBtns input-buttons float-left" OnClick="btnFileUpload_OnClick" />
                                    <input type="button" id="btn_close_uploader" class="input-buttons float-right" value="Close" onclick="openWSE.LoadModalWindow(false, 'FileUpload-element', '');" style="margin-right: 0px;" />
                                    <div class="clear"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div id="FolderEdit-element" class="Modal-element">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="500">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'FolderEdit-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="updatePnl_NewFolder" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_newFolder" runat="server" DefaultButton="btn_newFolder">
                                            <h3>Create New Folder</h3>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="tb_newFolder" runat="server" CssClass="textEntry margin-right" placeholder="Folder name"></asp:TextBox>
                                            <asp:Button ID="btn_newFolder" runat="server" OnClick="btn_newFolder_Click" CssClass="input-buttons RandomActionBtns" Text="Create" />
                                        </asp:Panel>
                                        <div class="clear-space"></div>
                                        <div class="clear-space"></div>
                                        <asp:Panel ID="pnl_folderEditList" runat="server">
                                            <h3>Folder List</h3>
                                            <div class="clear-space-two"></div>
                                            <asp:DropDownList ID="dd_folderEditList" runat="server" OnSelectedIndexChanged="dd_folderEditList_SelectedIndexChanged" AutoPostBack="true" CssClass="margin-right">
                                            </asp:DropDownList>
                                            <asp:LinkButton ID="lbtn_EditFolder" runat="server" CssClass="margin-right RandomActionBtns" Text="Edit" OnClick="lbtn_EditFolder_Click"></asp:LinkButton>
                                            <a onclick="DeleteFolder();return false;">Delete</a>
                                            <asp:HiddenField ID="hf_DeleteFolder" runat="server" OnValueChanged="hf_DeleteFolder_ValueChanged"></asp:HiddenField>
                                            <asp:Panel ID="pnl_EditFolder" runat="server" DefaultButton="btn_UpdateFolder" Enabled="false" Visible="false">
                                                <div class="clear-space"></div>
                                                <asp:TextBox ID="tb_editFolder" runat="server" CssClass="textEntry margin-right" placeholder="Folder name"></asp:TextBox>
                                                <asp:Button ID="btn_UpdateFolder" runat="server" OnClick="btn_UpdateFolder_Click" CssClass="input-buttons RandomActionBtns" Text="Update" />
                                                <asp:Button ID="btn_cancelEditFolder" runat="server" OnClick="btn_cancelEditFolder_Click" CssClass="input-buttons RandomActionBtns" Text="Cancel" />
                                            </asp:Panel>
                                        </asp:Panel>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btn_newFolder" />
                                        <asp:AsyncPostBackTrigger ControlID="dd_folderEditList" />
                                        <asp:AsyncPostBackTrigger ControlID="lbtn_EditFolder" />
                                        <asp:AsyncPostBackTrigger ControlID="hf_DeleteFolder" />
                                        <asp:AsyncPostBackTrigger ControlID="btn_UpdateFolder" />
                                        <asp:AsyncPostBackTrigger ControlID="btn_cancelEditFolder" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" value="Close" onclick="openWSE.LoadModalWindow(false, 'FolderEdit-element', '');" class="input-buttons modal-cancel-btn" />
                        </div>
                    </div>
                </div>
            </div>
            <input type="hidden" id="continue_play_hidden" value="true" />
            <input type="hidden" id="shuffle_play_hidden" value="false" />
            <input type="hidden" id="repeat_play_hidden" value="false" />
        </form>
    </div>
    <script type="text/javascript" src="filedrive.js"></script>
</body>
</html>
