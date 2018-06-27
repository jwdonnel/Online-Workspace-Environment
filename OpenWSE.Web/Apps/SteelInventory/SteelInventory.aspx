<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SteelInventory.aspx.cs" Inherits="Apps_SteelInventory_SteelInventory" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Steel Inventory</title>
    <link href="steelinventory-styles.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager_SteelInventory" runat="server" AsyncPostBackTimeout="360000"></asp:ScriptManager>
        <div id="pnl_topbar" runat="server" class="pad-all app-title-bg-color">
            <div class="float-left">
                <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
                <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
            </div>
            <div class="clear"></div>
        </div>
        <div id="pnl_InventoryList" class="pad-all">
            <div id="InventoryUpload-element" class="Modal-element">
                <div class="Modal-overlay">
                    <div class="Modal-element-align">
                        <div class="Modal-element-modal" data-setwidth="550">
                            <div class="ModalHeader">
                                <div>
                                    <div class="app-head-button-holder-admin">
                                        <a href="#" onclick="openWSE.LoadModalWindow(false, 'InventoryUpload-element', '');return false;"
                                            class="ModalExitButton"></a>
                                    </div>
                                    <span class="Modal-title"></span>
                                </div>
                            </div>
                            <div class="ModalScrollContent">
                                <div class="ModalPadContent">
                                    <div id="fileuploader">
                                        Select file and press the Upload Files button.
                                        <div class="clear-space-five"></div>
                                        <input type="file" id="FileUploadControl" allowmulitple="false" />
                                        <div class="clear-space"></div>
                                        Select the image you wish to use
                                        <div class="clear-space"></div>
                                        <asp:UpdatePanel ID="updatePnl_ImageSelect" runat="server">
                                            <ContentTemplate>
                                                <asp:Panel ID="pnl_InventoryImageSelector" runat="server">
                                                </asp:Panel>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <div class="clear"></div>
                                    </div>
                                </div>
                            </div>
                            <div class="ModalButtonHolder">
                                <input type="button" class="input-buttons modal-ok-btn" value="Upload Files" onclick="steelInventory.UploadImage();" />
                                <input type="button" class="input-buttons modal-cancel-btn" value="Close" onclick="openWSE.LoadModalWindow(false, 'InventoryUpload-element', '');" />
                                <div class="clear"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <asp:UpdatePanel ID="updatePnl_Table" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:HiddenField ID="hf_UpdateAll" runat="server" OnValueChanged="hf_UpdateAll_ValueChanged" />
                    <asp:HiddenField ID="hf_UpdateQuantityId" runat="server" />
                    <asp:HiddenField ID="hf_UpdateQuantity" runat="server" OnValueChanged="hf_UpdateQuantity_ValueChanged" />
                    <asp:HiddenField ID="hf_EditItem" runat="server" OnValueChanged="hf_EditItem_ValueChanged" />
                    <asp:HiddenField ID="hf_DeleteItem" runat="server" OnValueChanged="hf_DeleteItem_ValueChanged" />
                    <asp:HiddenField ID="hf_UpdateItem" runat="server" OnValueChanged="hf_UpdateItem_ValueChanged" />
                    <asp:HiddenField ID="hf_AddItem" runat="server" OnValueChanged="hf_AddItem_ValueChanged" />
                    <asp:HiddenField ID="hf_DeleteImage" runat="server" OnValueChanged="hf_DeleteImage_ValueChanged" />
                    <asp:HiddenField ID="hf_DeleteImage_EditId" runat="server" />
                    <asp:HiddenField ID="hf_RefreshImageList" runat="server" OnValueChanged="hf_RefreshImageList_ValueChanged" />
                    <asp:Panel ID="pnl_SteelInventory" runat="server"></asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="hf_UpdateAll" />
                    <asp:AsyncPostBackTrigger ControlID="hf_UpdateQuantity" />
                    <asp:AsyncPostBackTrigger ControlID="hf_EditItem" />
                    <asp:AsyncPostBackTrigger ControlID="hf_DeleteItem" />
                    <asp:AsyncPostBackTrigger ControlID="hf_UpdateItem" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
        <div class="clear"></div>
    </form>
    <script type="text/javascript" src="steelInventory.js"></script>
</body>
</html>
