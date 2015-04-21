<%@ page title="Overlay Manager" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_Overlays, App_Web_muevkmyy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .app-span-modify
        {
            color: #555 !important;
        }

        .app-icon-admin
        {
            margin-left: 0px !important;
            margin-right: 0px !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <div id="AddOverlay-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="510" data-setmaxheight="460">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'AddOverlay-element', '');return false;" class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:Panel ID="pnl_AddControls" runat="server">
                                    You can add a custom overlay for a app. Overlays must be a UserControl (.ascx) file with precompiled code.
                                <div class="clear" style="height: 20px;">
                                </div>
                                    <div class="font-bold pad-bottom-sml">
                                        Overlay Name
                                    </div>
                                    <asp:TextBox ID="txt_uploadOverlayName" runat="server" CssClass="textEntry" Width="325px"
                                        MaxLength="250"></asp:TextBox>
                                    <div class="clear-space">
                                    </div>
                                    <div class="font-bold pad-bottom-sml pad-top">
                                        Description
                                    </div>
                                    <asp:TextBox ID="txt_uploadOverlayDesc" runat="server" CssClass="pad-all-sml" Width="400px"
                                        Height="50px" MaxLength="500" TextMode="MultiLine" Font-Names="Arial" BorderColor="#DDDDDD"
                                        BorderWidth="1px"></asp:TextBox>
                                    <div class="clear-space">
                                    </div>
                                    <div class="font-bold pad-bottom-sml pad-top">
                                        Display Type
                                    </div>
                                    <asp:DropDownList ID="dd_displayTypeNew" runat="server">
                                        <asp:ListItem Value="workspace-overlays" Text="Solid Background"></asp:ListItem>
                                        <asp:ListItem Value="workspace-overlays-nobg" Text="Transparent"></asp:ListItem>
                                        <asp:ListItem Value="workspace-overlays-custom" Text="Custom / No Header"></asp:ListItem>
                                    </asp:DropDownList>
                                    <div class="clear-space">
                                    </div>
                                    <div class="font-bold pad-bottom-sml pad-top">
                                        Associated Apps
                                    </div>
                                    <span><a href="#" onclick="if ($('#cb_ShowHideList').css('display') == 'none') { $('#cb_ShowHideList').slideDown(openWSE_Config.animationSpeed); if ($.trim($('#cb_ShowHideList').html()) == '') { $('#cb_ShowHideList').html('<h4>&nbsp;&nbsp;Must create a app</h4>'); } }else { $('#cb_ShowHideList').slideUp(openWSE_Config.animationSpeed); }return false;">Show/Hide App List</a></span>
                                    <div class="clear">
                                    </div>
                                    <div id="cb_ShowHideList" style="display: none;">
                                        <asp:CheckBoxList ID="cb_associatedOverlay" runat="server" CssClass="margin-right">
                                        </asp:CheckBoxList>
                                    </div>
                                    <div class="clear-space">
                                    </div>
                                    <div class="font-bold pad-bottom-sml pad-top">
                                        File Upload
                                    </div>
                                    <asp:FileUpload ID="fileupload_Overlay" runat="server" />
                                    <div class="clear-margin">
                                        <asp:Label ID="lbl_uploadMessage" runat="server" ClientIDMode="Static"></asp:Label>
                                    </div>
                                    <div class="clear-space">
                                    </div>
                                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                        <ContentTemplate>
                                            <asp:HiddenField ID="hf_DeleteOverlay" runat="server" ClientIDMode="Static" OnValueChanged="hf_DeleteOverlay_Changed" />
                                            <asp:HiddenField ID="hf_EditOverlay" runat="server" ClientIDMode="Static" OnValueChanged="hf_EditOverlay_Changed" />
                                            <asp:HiddenField ID="hf_UpdateNameOverlay" runat="server" ClientIDMode="Static" OnValueChanged="hf_UpdateNameOverlay_Changed" />
                                            <asp:HiddenField ID="hf_AssociationOverlay" runat="server" ClientIDMode="Static"
                                                OnValueChanged="hf_AssociationOverlay_Changed" />
                                            <asp:HiddenField ID="hf_OverlayID" runat="server" ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_UpdateDescOverlay" runat="server" ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_displayType" runat="server" ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_removeapp" runat="server" OnValueChanged="hf_removeapp_ValueChanged"
                                                ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_addapp" runat="server" OnValueChanged="hf_addapp_ValueChanged"
                                                ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_refreshList" runat="server" OnValueChanged="hf_refreshList_ValueChanged"
                                                Value="" ClientIDMode="Static" />
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </asp:Panel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                                                                <asp:Button ID="btn_uploadOverlay" runat="server" Text="Upload" CssClass="input-buttons Upload-Button-Action"
                                        disabled="disabled" OnClick="btn_uploadOverlay_Clicked" />
                                    <input type="button" class="input-buttons no-margin" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'AddOverlay-element', '');" />
                            </div>
                    </div>
                </div>
            </div>
        </div>
        <asp:UpdatePanel ID="updatepnl_overlays" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="table-settings-box" style="margin-bottom: 0px!important;">
                    <div class="td-settings-ctrl">
                        <a id="aAddNewOverlay" runat="server" class="margin-right-big input-buttons" onclick="openWSE.LoadModalWindow(true, 'AddOverlay-element', 'Add New Overlay');$('#MainContent_txt_uploadNotifiName').focus();return false;"><span class="td-add-btn float-left margin-right-sml" style="padding: 0!important;"></span>Add New Overlay</a>
                        <span class="font-bold pad-right">Total Overlays</span><asp:Label ID="lbl_overlaysEnabled"
                            runat="server" Text="0"></asp:Label>
                        <div class="clear-space">
                        </div>
                        <asp:Panel ID="pnl_overlays" runat="server" CssClass="pad-top-big">
                        </asp:Panel>
                        <div class="clear-space">
                        </div>
                    </div>
                    <div class="td-settings-desc">
                        Overlays can be uploaded and configured here. Only ASP.Net user control files (.ascx) and .zip that contains the .dll and the .ascx are allowed at the moment. Users will be able to enable these overlays their Account Settings.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class="table-settings-box no-border" style="padding-top: 0px!important; margin-top: 0px!important;">
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="updatepnl_Associations" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_AppAssociation" runat="server">
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div class="clear">
                </div>
            </div>
            <div class="td-settings-desc">
                Overlays must be associated with an installed app in order to view them in the user overlay settings.
            </div>
        </div>
        <div id="App-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="650">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'App-element', '');RefreshList();return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="updatepnl_AssociationsEdit" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Label ID="lbl_typeEdit_Name" ClientIDMode="Static" runat="server"></asp:Label>
                                        <div class="clear-space">
                                        </div>
                                        Add the apps that associate with the current overlay. Adding a app already
                                    associated with another overlay will remove it from that overlay and add it to the
                                    current.
                                        <div class="clear" style="height: 20px">
                                        </div>
                                        <asp:Panel ID="Typelist" runat="server" ClientIDMode="Static">
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/overlays.js")%>'></script>
    </div>
</asp:Content>
