<%@ Page Title="Overlay Manager" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="OverlayManager.aspx.cs" Inherits="SiteTools_OverlayManager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
    <asp:Panel ID="pnl_OverlayList" ClientIDMode="Static" runat="server">
        <asp:UpdatePanel ID="updatepnl_overlays" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="table-settings-box">
                    <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
                        Overlay Manager
                    </div>
                    <div class="title-line"></div>
                    <div class="td-settings-ctrl">
                        <a id="aAddNewOverlay" runat="server" class="margin-right-big input-buttons-create float-left" onclick="openWSE.LoadModalWindow(true, 'AddOverlay-element', 'Add New Overlay');$('#MainContent_txt_uploadNotifiName').focus();return false;">Upload Overlay</a>
                        <div class="clear-space"></div>
                        <asp:Panel ID="pnl_overlays" runat="server">
                        </asp:Panel>
                    </div>
                    <div class="td-settings-desc">
                        Overlays can be uploaded and configured here. Only ASP.Net user control files (.ascx) and .zip that contains the .dll and the .ascx are allowed at the moment. Users will be able to enable these overlays their Account Settings. Overlays must be associated with an installed app in order to view them in the user overlay settings.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
    <div id="AddOverlay-element" class="Modal-element">
        <div class="Modal-overlay">
            <div class="Modal-element-align">
                <div class="Modal-element-modal" data-setwidth="650">
                    <div class="ModalHeader">
                        <div>
                            <div class="app-head-button-holder-admin">
                                <a href="#" onclick="openWSE.LoadModalWindow(false, 'AddOverlay-element', '');RefreshList();return false;" class="ModalExitButton"></a>
                            </div>
                            <span class="Modal-title"></span>
                        </div>
                    </div>
                    <div class="ModalScrollContent">
                        <div class="ModalPadContent">
                            <asp:Panel ID="pnl_AddControls" runat="server">
                                You can add a custom overlay for a app. Overlays must be a UserControl (.ascx) file with precompiled code.
                                <div class="clear-space">
                                </div>
                                <div class="input-settings-holder">
                                    <span class="font-bold">Name</span>
                                    <div class="clear-space-two"></div>
                                    <asp:TextBox ID="txt_uploadOverlayName" runat="server" CssClass="textEntry" Width="100%" MaxLength="250"></asp:TextBox>
                                </div>
                                <div class="input-settings-holder">
                                    <span class="font-bold">Description</span>
                                    <div class="clear-space-two"></div>
                                    <asp:TextBox ID="txt_uploadOverlayDesc" runat="server" CssClass="textEntry" Width="100%" MaxLength="500"></asp:TextBox>
                                </div>
                                <div class="input-settings-holder">
                                    <span class="font-bold">Display Type
                                    </span>
                                    <div class="clear-space-two"></div>
                                    <asp:DropDownList ID="dd_displayTypeNew" runat="server">
                                        <asp:ListItem Value="workspace-overlays" Text="Solid Background"></asp:ListItem>
                                        <asp:ListItem Value="workspace-overlays-nobg" Text="Transparent"></asp:ListItem>
                                        <asp:ListItem Value="workspace-overlays-custom" Text="Custom / No Header"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_associatedOverlays" runat="server" ClientIDMode="Static">
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <div class="clear-space">
                                </div>
                                <div class="input-settings-holder">
                                    <span class="font-bold">File Upload
                                    </span>
                                    <div class="clear-space-two"></div>
                                    <asp:FileUpload ID="fileupload_Overlay" runat="server" />
                                    <div class="clear-margin">
                                        <asp:Label ID="lbl_uploadMessage" runat="server" ClientIDMode="Static"></asp:Label>
                                    </div>
                                </div>
                                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                    <ContentTemplate>
                                        <asp:HiddenField ID="hf_DeleteOverlay" runat="server" ClientIDMode="Static" OnValueChanged="hf_DeleteOverlay_Changed" />
                                        <asp:HiddenField ID="hf_EditOverlay" runat="server" ClientIDMode="Static" OnValueChanged="hf_EditOverlay_Changed" />
                                        <asp:HiddenField ID="hf_UpdateOverlayName" runat="server" ClientIDMode="Static" OnValueChanged="hf_UpdateOverlayName_Changed" />
                                        <asp:HiddenField ID="hf_OverlayID" runat="server" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hf_UpdateDescOverlay" runat="server" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hf_displayType" runat="server" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hf_refreshList" runat="server" OnValueChanged="hf_refreshList_ValueChanged"
                                            Value="" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hf_appAssocationList_added" runat="server" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hf_appAssocationList_removed" runat="server" ClientIDMode="Static" />
                                        <asp:HiddenField ID="hf_newAppAssocationList_Checked" runat="server" ClientIDMode="Static" />
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </asp:Panel>
                        </div>
                    </div>
                    <div class="ModalButtonHolder">
                        <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'AddOverlay-element', ''); RefreshList();" />
                        <asp:Button ID="btn_uploadOverlay" runat="server" Text="Upload" CssClass="input-buttons modal-ok-btn Upload-Button-Action"
                            disabled="disabled" OnClick="btn_uploadOverlay_Clicked" />
                    </div>
                </div>
            </div>
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
                                    <asp:Panel ID="pnl_editControls" runat="server"></asp:Panel>
                                    <div class="clear-space">
                                    </div>
                                    <asp:Panel ID="Typelist" runat="server" ClientIDMode="Static">
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                    <div class="ModalButtonHolder">
                        <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'App-element', ''); RefreshList();" />
                        <input id="btn_saveeditnoti" type="button" class="input-buttons modal-ok-btn" value="Save" onclick="openWSE.LoadModalWindow(false, 'App-element', ''); UpdateOverlay();" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
