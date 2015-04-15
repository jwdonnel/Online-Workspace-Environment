<%@ page language="C#" autoeventwireup="true" inherits="Apps_FileDrive, App_Web_exjr3opz" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>File Drive</title>
    <style type="text/css">
        .td-sort-click a
        {
            text-decoration: none !important;
        }

        #AjaxFileUpload1
        {
            overflow: auto;
            max-height: 300px;
        }

        .hidden-img-lnk
        {
            opacity: 1.0!important;
            filter: alpha(opacity=100)!important;
        }
    </style>
</head>
<body style="background: #F5F5F5 !important">
    <div id="mydocuments-load" class="main-div-app-bg">
        <form id="form_mydocuments" runat="server" enctype="multipart/form-data" method="post">
            <asp:ScriptManager ID="ScriptManager_deliverypickups" runat="server" AsyncPostBackTimeout="360000">
            </asp:ScriptManager>
            <input id="hf_editing" type="hidden" value="false" />
            <input id="hf_currexp" type="hidden" value="expand_View_All_Documents" />
            <div class="pad-all app-title-bg-color" style="height: 40px">
                <div class="float-left">
                    <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
                    <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
                </div>
                <div class="pad-left pad-right pad-top float-right">
                    <div id="searchwrapper" style="width: 350px;">
                        <asp:UpdatePanel ID="updatePanel2_documents" runat="server">
                            <ContentTemplate>
                                <asp:Panel ID="Panel1_documents" runat="server" DefaultButton="imgbtn_search_documents">
                                    <asp:HiddenField ID="hf_searchFiles_documents" runat="server" OnValueChanged="hf_searchFiles_ValueChanged" />
                                    <asp:TextBox ID="tb_search_documents" runat="server" CssClass="searchbox" Font-Size="Small"
                                        onfocus="if(this.value=='Search for documents')this.value=''" onblur="if(this.value=='')this.value='Search for documents'"
                                        Text="Search for documents"></asp:TextBox>
                                    <a href="#" title="Clear search" class="searchbox_clear" onclick="onSearchClearFiles('<%=tb_search_documents.ClientID %>'); return false;"></a>
                                    <asp:LinkButton ID="imgbtn_search_documents" runat="server" ToolTip="Start search"
                                        CssClass="searchbox_submit" />
                                </asp:Panel>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
            <table class="content-table" cellpadding="0" cellspacing="0">
                <tbody>
                    <tr>
                        <td class="td-sidebar">
                            <div id="sidebar-padding-docs" class="sidebar-padding">
                                <div class="sidebar-scroll-app">
                                    <asp:Panel ID="pnl_ect_documents" runat="server" Width="230px">
                                        <div class="clear-margin">
                                            <h3>Groups & Folders</h3>
                                            <div class="clear-space">
                                            </div>
                                            <asp:UpdatePanel ID="updatepnl_groups" runat="server">
                                                <ContentTemplate>
                                                    <asp:HiddenField ID="hf_UpdateAll" runat="server" OnValueChanged="hf_UpdateAll_ValueChanged" />
                                                    <asp:HiddenField ID="hf_AutoUpdateInterval_Main" runat="server" />
                                                    <div class="clear-margin">
                                                        <asp:HiddenField ID="hf_ddgroups" runat="server" ClientIDMode="Static" />
                                                        <asp:DropDownList ID="dd_groups" runat="server" CssClass="textEntry margin-right" ClientIDMode="Static">
                                                        </asp:DropDownList>
                                                        <asp:Button ID="btn_groups" runat="server" CssClass="input-buttons RandomActionBtns" OnClick="dd_groups_Changed" Text="Update" />
                                                    </div>
                                                </ContentTemplate>
                                                <Triggers>
                                                    <asp:AsyncPostBackTrigger ControlID="btn_groups" />
                                                </Triggers>
                                            </asp:UpdatePanel>
                                            <div class="clear-space">
                                            </div>
                                            <div class="sidebar-divider">
                                            </div>
                                            <div class="clear-space">
                                            </div>
                                            <a class="ajaxCall_Modal_documents input-buttons margin-top" href="#Contact_Modal_documents">
                                                <span class="float-left margin-right-sml td-add-btn" style="padding: 0px 0px 0px 5px!important;"></span><span class="pad-right-sml float-left" style="margin-top: 2px;">Add Files</span></a>
                                            <div class="clear-space">
                                            </div>
                                            <asp:UpdatePanel ID="updatepnl_ect_documents" runat="server">
                                                <ContentTemplate>
                                                    <asp:Literal ID="ltl_ect_documents" runat="server"></asp:Literal>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </div>
                        </td>
                        <td class="td-content">
                            <div class="content-main" style="min-width: 700px;">
                                <table width="100%" cellspacing="0" cellpadding="0">
                                    <tbody>
                                        <tr>
                                            <td class="td-content-inner">
                                                <div class="content-overflow-app" style="/*overflow: visible!important; */">
                                                    <div id="audioPlayer">
                                                    </div>
                                                    <asp:UpdatePanel ID="UpdatePanel1_documents" runat="server">
                                                        <ContentTemplate>
                                                            <asp:HiddenField ID="hf_folderchange_documents" runat="server" OnValueChanged="hf_folderchange_ValueChanged"
                                                                Value="" />
                                                            <asp:HiddenField ID="hf_category_documents" runat="server" />
                                                            <div class="pad-top-big pad-left">
                                                                <asp:Label ID="lbl_currFolderName" runat="server" Style="color: #555; font-size: 22px">Root Directory</asp:Label>
                                                                <div class="float-right">
                                                                    <asp:LinkButton ID="lbtn_HideImages" runat="server" Text="Hide Images" CssClass="RandomActionBtns margin-right-big" OnClick="lbtn_HideImages_Click"></asp:LinkButton>
                                                                </div>
                                                            </div>
                                                            <div class="pad-left pad-right">
                                                                <div class="float-left">
                                                                    <asp:Label ID="lbl_movetofolder_documents" runat="server" CssClass="bold" Text="Move Selected Files to a new Folder:"
                                                                        Visible="False" Enabled="False"></asp:Label>
                                                                    <asp:DropDownList ID="dd_myAlbums_documents" runat="server" Enabled="False"
                                                                        Visible="False">
                                                                    </asp:DropDownList>
                                                                    <asp:LinkButton ID="lbtn_moveFile_documents" runat="server" Visible="False" Enabled="False"
                                                                        CssClass="margin-left RandomActionBtns-docs margin-right-big" OnClick="lbtn_moveFile_Click">Move Selected</asp:LinkButton>
                                                                </div>
                                                                <asp:LinkButton ID="lbtn_selectAll_documents" runat="server" CssClass="RandomActionBtns-docs float-left"
                                                                    OnClick="lbtn_selectAll_Click" Style="margin-top: 1px">Select All</asp:LinkButton>
                                                                <asp:LinkButton ID="refresh_Click_documents" runat="server" CssClass="float-right margin-left margin-top-sml RandomActionBtns-docs margin-right img-refresh"
                                                                    ToolTip="Refresh Files" OnClick="btn_refresh_Click" />
                                                                <asp:Label ID="lbl_fileNoti_documents" runat="server" CssClass="float-right pad-top"
                                                                    Font-Size="12px" Text=""></asp:Label>
                                                            </div>
                                                            <div class="clear-space">
                                                            </div>
                                                            <asp:HiddenField ID="HiddenField1_documents" runat="server" />
                                                            <asp:HiddenField ID="hf_sortcol_documents" runat="server" />
                                                            <asp:GridView ID="GV_Files_documents" runat="server" CellPadding="0" CellSpacing="0"
                                                                AutoGenerateColumns="False" Width="100%" GridLines="None" OnRowCommand="GV_Files_RowCommand"
                                                                OnRowDataBound="GV_Files_RowDataBound" OnRowEditing="GV_Files_RowEdit" OnRowCancelingEdit="GV_Files_CancelEdit"
                                                                OnRowUpdating="GV_Files_RowUpdate" ShowHeaderWhenEmpty="True" OnRowCreated="GV_Files_RowCreated">
                                                                <EmptyDataRowStyle ForeColor="Black" />
                                                                <RowStyle CssClass="GridNormalRow" />
                                                                <AlternatingRowStyle CssClass="GridNormalRow" />
                                                                <%--<AlternatingRowStyle CssClass="GridAlternate" />--%>
                                                                <EmptyDataTemplate>
                                                                    <div class="emptyGridView">
                                                                        There are no documents in folder.
                                                                    </div>
                                                                </EmptyDataTemplate>
                                                                <Columns>
                                                                    <asp:TemplateField>
                                                                        <HeaderTemplate>
                                                                            <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                                                                <tr>
                                                                                    <td width="24px" align="center">
                                                                                        <asp:LinkButton ID="imgbtn_del" runat="server" ToolTip="Delete selected items" OnClick="imgbtn_del_Click"
                                                                                            CssClass="td-delete-light-btn" OnClientClick="return window.confirm('Are you sure you want to delete the selected files?');"></asp:LinkButton>
                                                                                    </td>
                                                                                    <td id="td_filename" runat="server" align="center" class="td-sort-click" style="min-width: 150px;">
                                                                                        <asp:LinkButton ID="lbtn_filename" runat="server" OnClick="lbtn_filename_Click" CssClass="RandomActionBtns-docs">Filename</asp:LinkButton>
                                                                                    </td>
                                                                                    <td id="td_ext" runat="server" width="115px" class="td-sort-click" align="center">
                                                                                        <asp:LinkButton ID="lbtn_ext" runat="server" OnClick="lbtn_ext_Click" CssClass="RandomActionBtns-docs">Ext</asp:LinkButton>
                                                                                    </td>
                                                                                    <td id="td_size" runat="server" width="100px" class="td-sort-click" align="center">
                                                                                        <asp:LinkButton ID="lbtn_size" runat="server" OnClick="lbtn_size_Click" CssClass="RandomActionBtns-docs">Size</asp:LinkButton>
                                                                                    </td>
                                                                                    <td id="td_date" runat="server" width="115px" class="td-sort-click" align="center">
                                                                                        <asp:LinkButton ID="lbtn_date" runat="server" OnClick="lbtn_date_Click" CssClass="RandomActionBtns-docs">Upload Date</asp:LinkButton>
                                                                                    </td>
                                                                                    <td id="td_album" runat="server" width="150px" class="td-sort-click" align="center">
                                                                                        <asp:LinkButton ID="lbtn_album" runat="server" OnClick="lbtn_album_Click" CssClass="RandomActionBtns-docs">Folder</asp:LinkButton>
                                                                                    </td>
                                                                                    <td width="110px" align="center">Actions
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </HeaderTemplate>
                                                                        <ItemTemplate>
                                                                            <asp:Panel ID="pnl_FileMain" class="panelcontainer" runat="server">
                                                                                <table cellpadding="5" cellspacing="0" class="myItemStyle" style="font-size: 1.0em;">
                                                                                    <tr>
                                                                                        <td width="24px" align="center" class="GridViewNumRow">
                                                                                            <asp:CheckBox ID="CheckBoxIndv" runat="server" OnCheckedChanged="CheckBoxIndv_CheckChanged"
                                                                                                Text='<%#Eval("ID") %>' CssClass="HiddenText" AutoPostBack="true" />
                                                                                        </td>
                                                                                        <td align="left" style="min-width: 150px;" class="border-right">
                                                                                            <div class='pad-top pad-bottom'>
                                                                                                <%#Eval("Title") %>
                                                                                            </div>
                                                                                        </td>
                                                                                        <td width="115px" align="left" class="border-right">
                                                                                            <%#Eval("Type") %>
                                                                                        </td>
                                                                                        <td width="100px" align="left" class="border-right">
                                                                                            <%#Eval("Size")%>
                                                                                        </td>
                                                                                        <td width="115px" align="left" class="border-right">
                                                                                            <%#Eval("UploadDate")%>
                                                                                        </td>
                                                                                        <td width="150px" align="left" class="border-right">
                                                                                            <%#Eval("Album")%>
                                                                                        </td>
                                                                                        <td width="110px" align="center" class="border-right">
                                                                                            <asp:LinkButton ID="LinkButton1" CssClass="td-edit-btn margin-right-sml margin-left-sml RandomActionBtns-docs"
                                                                                                runat="server" CommandName="Edit" ToolTip="Edit"></asp:LinkButton>
                                                                                            <asp:LinkButton ID="LinkButton3" CssClass="img-download pad-all-sml margin-right-sml margin-left-sml"
                                                                                                runat="server" CommandName="downloadFile" CommandArgument='<%#Eval("ID") %>'
                                                                                                PostBackUrl="~/Apps/FileDrive/FileDrive.aspx" ToolTip="Download"></asp:LinkButton>
                                                                                            <asp:LinkButton ID="LinkButton2" CssClass="td-delete-btn margin-right-sml margin-left-sml"
                                                                                                runat="server" CommandName="deleteFile" CommandArgument='<%#Eval("ID") %>' OnClientClick="var ret = confirm('Are you sure you want to delete this file?');if (ret == true) {DeleteConfirmation();}else{return false;}"
                                                                                                ToolTip="Delete"></asp:LinkButton>
                                                                                        </td>
                                                                                    </tr>
                                                                                </table>
                                                                            </asp:Panel>
                                                                        </ItemTemplate>
                                                                        <EditItemTemplate>
                                                                            <asp:HiddenField ID="hf_editID" runat="server" Value='<%#Eval("ID") %>' />
                                                                            <asp:HiddenField ID="hf_editFileName" runat="server" Value='<%#Eval("TitleEdit") %>' />
                                                                            <asp:HiddenField ID="hf_editFolderName" runat="server" Value='<%#Eval("Album") %>' />
                                                                            <asp:HiddenField ID="hf_editExt" runat="server" Value='<%#Eval("Type") %>' />
                                                                            <table cellpadding="5" cellspacing="0" class="myItemStyle" style="font-size: 0.95em;">
                                                                                <tr>
                                                                                    <td width="24px" align="center" class="GridViewNumRow"></td>
                                                                                    <td align="center" class="border-right">
                                                                                        <asp:TextBox ToolTip='<%# String.Format("Edit {0}",Eval("TitleEdit")) %>' CssClass="textEntry"
                                                                                            runat="server" ID="tb_editFileName" Text='<%#Eval("TitleEdit") %>' Width="95%"
                                                                                            MaxLength="100"></asp:TextBox>
                                                                                    </td>
                                                                                    <td width="115px" align="left" class="border-right">
                                                                                        <%#Eval("Type") %>
                                                                                    </td>
                                                                                    <td width="100px" align="left" class="border-right">
                                                                                        <%#Eval("Size")%>
                                                                                    </td>
                                                                                    <td width="115px" align="left" class="border-right">
                                                                                        <%#Eval("UploadDate")%>
                                                                                    </td>
                                                                                    <td width="150px" align="center" class="border-right">
                                                                                        <asp:DropDownList ID="dd_editFolderName" runat="server" Width="95%" Height="25px">
                                                                                        </asp:DropDownList>
                                                                                    </td>
                                                                                    <td width="110px" align="center" class="border-right">
                                                                                        <asp:LinkButton ID="lbtn_update" CssClass="td-update-btn margin-right-sml margin-left-sml RandomActionBtns-docs"
                                                                                            runat="server" CausesValidation="True" CommandName="Update" ToolTip="Update"></asp:LinkButton>
                                                                                        <asp:LinkButton ID="lbtn_cancel" CssClass="td-cancel-btn margin-right-sml margin-left-sml RandomActionBtns-docs"
                                                                                            runat="server" CausesValidation="False" CommandName="Cancel" ToolTip="Cancel"></asp:LinkButton>
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </EditItemTemplate>
                                                                    </asp:TemplateField>
                                                                </Columns>
                                                            </asp:GridView>
                                                        </ContentTemplate>
                                                    </asp:UpdatePanel>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
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
                        <div class="ModalPadContent">
                            <div id="fileuploader" class="clear-margin">
                                Select file(s) and press the Upload Files button
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
        </form>
    </div>
    <noscript>
        <meta http-equiv="refresh" content="0; URL=../browserchecker_nojs.html" />
    </noscript>
    <script type="text/javascript">
        //$get('mf_View_All').style.fontWeight = "bold";
        function folderChange(x) {
            var hf = document.getElementById("hf_currexp");
            if (hf.value != "") {
                $("#" + hf.value).removeClass("tsactive");
            }
            var foldertemp = "View_All_Documents";
            if (x == 1) {
                foldertemp = "PersonalFolder_Documents";
            }
            hf.value = "expand_" + foldertemp;
            $("#expand_" + foldertemp).addClass("tsactive");
            enableCurrSelected();
            document.getElementById('<%=hf_folderchange_documents.ClientID %>').value = x;
            document.getElementById("hf_category_documents").value = "";
            __doPostBack('<%=hf_folderchange_documents.ClientID %>', "");
        }
    </script>
</body>
</html>
