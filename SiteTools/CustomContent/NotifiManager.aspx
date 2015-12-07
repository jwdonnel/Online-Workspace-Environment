<%@ page title="Notification Manager" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_NotifiManager, App_Web_5emrkdig" %>

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
    <div class="maincontent-padding pad-top-big margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <div class="clear-space-five"></div>
        <ul class="sitemenu-selection">
        </ul>
        <div class="clear-space"></div>
        <asp:Panel ID="pnl_NotificationList" runat="server" CssClass="pnl-section margin-top" data-title="Notification List">
            <asp:UpdatePanel ID="updatepnl_notifi" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="table-settings-box no-border" style="margin-bottom: 0px!important;">
                        <div class="td-settings-ctrl" style="padding-top: 0px!important;">
                            <a id="aAddNewNoti" runat="server" class="margin-right-big input-buttons-create float-left" onclick="openWSE.LoadModalWindow(true, 'AddNotification-element', 'Add New Notification');$('#MainContent_txt_uploadNotifiName').focus();return false;">Create Notification</a>
                            <div class="searchwrapper float-left" style="width: 350px; margin-top: 3px;">
                                <asp:UpdatePanel ID="updatepnl_search" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_Search" runat="server" DefaultButton="imgbtn_search">
                                            <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                                                onfocus="if(this.value=='Search Notifications')this.value=''" onblur="if(this.value=='')this.value='Search Notifications'"
                                                Text="Search Notifications" data-defaultvalue="Search Notifications"></asp:TextBox>
                                            <asp:LinkButton ID="imgbtn_clearsearch" runat="server" ToolTip="Clear search" CssClass="searchbox_clear RandomActionBtns"
                                                OnClick="imgbtn_clearsearch_Click" />
                                            <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                                OnClick="imgbtn_search_Click" />
                                        </asp:Panel>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="imgbtn_clearsearch" />
                                        <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                            <div class="clear-space"></div>
                            <span class="font-bold pad-right">Total Notifications</span><asp:Label ID="lbl_notifiEnabled"
                                runat="server" Text="0"></asp:Label>
                            <div class="clear-space">
                            </div>
                            <asp:LinkButton ID="lbtn_Refresh" runat="server" Text="Rebuild Notifications" CssClass="RandomActionBtns"
                                OnClick="lbtn_Refresh_Clicked"></asp:LinkButton>
                            <div class="clear-space">
                            </div>
                            <asp:Panel ID="pnl_notifi" runat="server">
                            </asp:Panel>
                        </div>
                        <div class="td-settings-desc">
                            Notifications can be added and configured here. Users will be able to enable these notifications in their Account Settings. Apps must be coded/setup in order to send notifications.
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>
        <asp:Panel ID="pnl_AssociatedApps" runat="server" CssClass="pnl-section pad-top-big" data-title="Associated Apps">
            <div class="table-settings-box no-border" style="padding-top: 0px!important; margin-top: 0px!important;">
                <div class="td-settings-ctrl">
                    <asp:UpdatePanel ID="updatepnl_Associations" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Panel ID="pnl_AppAssociation" runat="server">
                            </asp:Panel>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div class="clear-space">
                    </div>
                </div>
                <div class="td-settings-desc">
                    Notifications must be associated with an installed
            app in order to view them in the user notification settings. Some notifications
            are hidden because they are a standard notification within the site.
                </div>
            </div>
        </asp:Panel>
        <div id="AddNotification-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="510" data-setmaxheight="460">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'AddNotification-element', '');return false;" class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_AddControls" runat="server">
                                            Fill in the information below to create a new notification for users to enable. Notifications will only work if the app or workspace is configured to accept it.
                                        <div class="clear" style="height: 20px;">
                                        </div>
                                            <asp:TextBox ID="txt_uploadNotifiName" runat="server" CssClass="textEntry" Width="325px"
                                                MaxLength="250" placeholder="Name"></asp:TextBox>
                                            <div class="clear-space">
                                            </div>
                                            <asp:TextBox ID="txt_uploadNofiImg" runat="server" CssClass="textEntry" Width="325px"
                                                MaxLength="250" ClientIDMode="Static" placeholder="Image"></asp:TextBox>
                                            <div class="clear-space-two"></div>
                                            <asp:CheckBox ID="cb_UseAppImg" runat="server" Text="&nbsp;Use App Icon" ClientIDMode="Static" />
                                            <div class="clear-space">
                                            </div>
                                            <div class="font-bold pad-bottom-sml pad-top">
                                                Description
                                            </div>
                                            <asp:TextBox ID="txt_uploadNotifiDesc" runat="server" CssClass="pad-all-sml" Width="400px" MaxLength="500" placeholder="Description"></asp:TextBox>
                                            <div class="clear-space">
                                            </div>
                                            <div class="font-bold pad-bottom-sml pad-top">
                                                Associated Apps
                                            </div>
                                            <span><a href="#" onclick="if ($('#cb_ShowHideList').css('display') == 'none') { $('#cb_ShowHideList').slideDown(openWSE_Config.animationSpeed); if ($.trim($('#cb_ShowHideList').html()) == '') { $('#cb_ShowHideList').html('<h4>&nbsp;&nbsp;Must create a app</h4>'); } }else { $('#cb_ShowHideList').slideUp(openWSE_Config.animationSpeed); }return false;">Show/Hide App List</a></span>
                                            <div class="clear">
                                            </div>
                                            <div id="cb_ShowHideList" style="display: none;">
                                                <asp:CheckBoxList ID="cb_associatedNoti" runat="server" CssClass="margin-right">
                                                </asp:CheckBoxList>
                                            </div>
                                            <div class="clear-space">
                                            </div>
                                            <div class="clear-margin">
                                                <asp:Label ID="lbl_uploadMessage" runat="server" ClientIDMode="Static"></asp:Label>
                                            </div>
                                            <asp:HiddenField ID="hf_DeleteNotifi" runat="server" ClientIDMode="Static" OnValueChanged="hf_DeleteNotifi_Changed" />
                                            <asp:HiddenField ID="hf_EditNotifi" runat="server" ClientIDMode="Static" OnValueChanged="hf_EditNotifi_Changed" />
                                            <asp:HiddenField ID="hf_UpdateNameNotifi" runat="server" ClientIDMode="Static" OnValueChanged="hf_UpdateNameNotifi_Changed" />
                                            <asp:HiddenField ID="hf_AssociationNotifi" runat="server" ClientIDMode="Static" OnValueChanged="hf_AssociationNotifi_Changed" />
                                            <asp:HiddenField ID="hf_NotifiID" runat="server" ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_UpdateDescNotifi" runat="server" ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_UpdateImgNotifi" runat="server" ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_removeapp" runat="server" OnValueChanged="hf_removeapp_ValueChanged"
                                                ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_addapp" runat="server" OnValueChanged="hf_addapp_ValueChanged"
                                                ClientIDMode="Static" />
                                            <asp:HiddenField ID="hf_refreshList" runat="server" OnValueChanged="hf_refreshList_ValueChanged"
                                                Value="" ClientIDMode="Static" />
                                        </asp:Panel>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btn_uploadNotifi" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons float-right no-margin" value="Cancel" onclick="openWSE.LoadModalWindow(false, 'AddNotification-element', '');" />
                            <asp:Button ID="btn_uploadNotifi" runat="server" Text="Create" CssClass="input-buttons float-left no-margin Upload-Button-Action"
                                OnClick="btn_uploadNotifi_Clicked" />
                            <div class="clear"></div>
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
                                        Add the apps that associate with the current notification. Adding a app
                                    already associated with another notification will remove it from that overlay and
                                    add it to the current.
                                        <div class="clear" style="height: 20px">
                                        </div>
                                        <asp:Panel ID="Typelist" runat="server" ClientIDMode="Static">
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons no-margin" value="Close" onclick="openWSE.LoadModalWindow(false, 'App-element', ''); RefreshList();" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/notifiManager.js")%>'></script>
    </div>
</asp:Content>
