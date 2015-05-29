<%@ page title="Group Organizer" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_GroupOrg, App_Web_rrukwxwl" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:UpdatePanel ID="updatepnl_refresh" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hf_edit" runat="server" OnValueChanged="hf_edit_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_delete" runat="server" OnValueChanged="hf_delete_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_reset" runat="server" OnValueChanged="hf_reset_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_viewusers" runat="server" OnValueChanged="hf_viewusers_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_viewusers_Standard" runat="server" OnValueChanged="hf_viewusers_Standard_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_removeuser" runat="server" OnValueChanged="hf_removeuser_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_adduser" runat="server" OnValueChanged="hf_adduser_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_inviteUser" runat="server" OnValueChanged="hf_inviteUser_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_inviteUserList" runat="server" Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_finishInviteUser" runat="server" OnValueChanged="hf_finishInviteUser_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_refreshList" runat="server" OnValueChanged="hf_refreshList_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_logoutGroup" runat="server" OnValueChanged="hf_logoutGroup_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_loginGroup" runat="server" OnValueChanged="hf_loginGroup_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_groupNetwork" runat="server" OnValueChanged="hf_groupNetwork_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_groupNetwork_Update" runat="server" OnValueChanged="hf_groupNetwork_Update_ValueChanged"
                    Value="" ClientIDMode="Static" />
                <asp:HiddenField ID="hf_groupNetwork_Delete" runat="server" OnValueChanged="hf_groupNetwork_Delete_ValueChanged"
                    Value="" ClientIDMode="Static" />
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="updatepnl_header" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="table-settings-box">
                    <div class="td-settings-ctrl">
                        <asp:Panel ID="pnl_addgroupbtn" runat="server">
                            <a class="margin-right-big input-buttons-create" onclick="ResetControls();return false;">Create Group</a>
                            <div class="float-right">
                                <b class="pad-right">Group Count</b><asp:Label ID="lbl_companycount" runat="server" Text="0"></asp:Label>
                            </div>
                            <div class="clear-space">
                            </div>
                        </asp:Panel>
                        <div class="clear-space-five"></div>
                    </div>
                    <div class="td-settings-desc">
                        The groups created are used to determine how to pull data from each app. Each user created MUST be associated with a group. When sending a user an invite, that user must have their group notification enabled. Otherwise, you will not see the user in the invite list.<br />
                    </div>
                </div>
                <div class="clear-space">
                </div>
                <div class="searchwrapper">
                    <asp:HiddenField ID="hf_clearsearch" runat="server" ClientIDMode="Static" OnValueChanged="hf_clearsearch_Changed" />
                    <asp:Panel ID="Panel1_groupsearch" runat="server" DefaultButton="imgbtn_search">
                        <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                            onfocus="if(this.value=='Search Groups')this.value=''" onblur="if(this.value=='')this.value='Search Groups'"
                            Text="Search Groups"></asp:TextBox>
                        <a href="#" onclick="return false;" class="searchbox_clear" title="Clear search"></a>
                        <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                            OnClick="imgbtn_search_Click" />
                    </asp:Panel>
                </div>
                <asp:Panel ID="pnl_companyholder" runat="server">
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div id="NewEdit-Group-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="580">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'NewEdit-Group-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent" style="height: 300px;">
                            <div class="ModalPadContent">
                                <div style="width: 220px;" class="inline-block float-left">
                                    <div class="clear-margin">
                                        <asp:UpdatePanel ID="updatepnl_editmode_1" runat="server" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <span>Group Name</span>
                                                <br />
                                                <asp:TextBox ID="tb_companyname" runat="server" CssClass="textEntryReg" Width="225px"
                                                    MaxLength="500"></asp:TextBox>
                                                <asp:Label ID="lbl_error" runat="server" Text="" Font-Size="X-Small" ForeColor="Red"></asp:Label>
                                                <asp:Label ID="lbl_tempcompanyname" runat="server" Text="" Visible="false" Enabled="false"></asp:Label>
                                                <div class="clear-space">
                                                </div>
                                                <div style="padding-top: 15px;">
                                                    <p class="font-color-light-black">
                                                        If you have a URL that you would like to use for your logo, enter it in the textbox
                                                below.
                                                    </p>
                                                    <div class="clear-space">
                                                    </div>
                                                    <span>Image Url</span><br />
                                                    <asp:TextBox ID="tb_imageurl" runat="server" CssClass="textEntry" onfocus="if(this.value=='Link to image')this.value=''"
                                                        onblur="if(this.value=='')this.value='Link to image'" Text="Link to image" Width="225px"></asp:TextBox>
                                                </div>
                                                <div class="clear-space">
                                                </div>
                                                <span>Upload a Logo</span>
                                                <br />
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <asp:FileUpload ID="fu_image_create" runat="server" />
                                        <div class="clear-space-five">
                                        </div>
                                        <small><i><b>.png</b> <b>.jpeg</b> and <b>.gif</b> are allowed</i></small>
                                    </div>
                                </div>
                                <asp:UpdatePanel ID="updatepnl_editmode_2" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <div class="inline-block float-right pad-left-big">
                                            <div class="clear-margin">
                                                <span>Description</span>
                                                <br />
                                                <asp:TextBox ID="tb_description" runat="server" CssClass="textEntryReg" Width="235px" Height="70px" Font-Names="Arial" TextMode="MultiLine" MaxLength="500"></asp:TextBox>
                                                <div class="clear-space">
                                                </div>
                                                <asp:CheckBox ID="cb_isprivate" runat="server" Text="&nbsp;Is Private" Style="color: #555;" />
                                                <div class="clear-space">
                                                </div>
                                                <a href="#" id="group-userdefaults-button" title="Change the user default settings for users that log into this group." class="margin-bottom">Change Default User Settings</a>
                                            </div>
                                            <asp:Image ID="img_logo" runat="server" Style="display: none; max-height: 90px; max-width: 175px;" />
                                        </div>
                                        <div class="clear-space">
                                        </div>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:Button ID="btn_finish_add" runat="server" Text="Save" CssClass="input-buttons"
                                OnClick="btn_finish_add_Click" CausesValidation="false" Width="60px" />
                            <input type="button" class="input-buttons no-margin" onclick="openWSE.LoadModalWindow(false, 'NewEdit-Group-element', '');" value="Cancel" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="GroupEdit-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="650">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'GroupEdit-element', '');RefreshList();return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="updatepnl_viewusers" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_modalTitle" runat="server">
                                        </asp:Panel>
                                        <asp:Panel ID="pnl_users" runat="server">
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons" value="Close" onclick="openWSE.LoadModalWindow(false, 'GroupEdit-element', ''); RefreshList();" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="GroupInviteUser-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="650">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'GroupInviteUser-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <div id="invite-innermodal">
                                </div>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons" value="Close" onclick="openWSE.LoadModalWindow(false, 'GroupInviteUser-element', '');" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="GroupNetwork-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="700">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#" onclick="$('#MainContent_ipMessage').html('');$('#tb_ipAdd').val('');openWSE.LoadModalWindow(false, 'GroupNetwork-element', '');return false;"
                                        class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="updatepnl_groupNetwork" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_groupNetworkAdd" runat="server" DefaultButton="btn_addIp">
                                            <asp:Image ID="imgGroupNetwork" runat="server" CssClass="float-left margin-right" Style="max-height: 30px;" />
                                            <asp:Label ID="lblGroupNetworkName" runat="server" Font-Size="Large"></asp:Label>
                                            <div class="clear-space"></div>
                                            <small><b class="pad-right-sml">Note:</b>Adding an Ip address and activating it will cause any user that is not on that Ip address to be blocked. This will only apply if the user is attempting to login to the group.</small>
                                            <div class="clear-space"></div>
                                            <span class="pad-right"><b>Ip Address to listen on:</b></span>
                                            <asp:TextBox ID="tb_createnew_listener" runat="server" ClientIDMode="Static" CssClass="textEntry margin-right" Width="150px"></asp:TextBox>
                                            <asp:Button ID="btn_addIp" runat="server" CssClass="input-buttons" Text="Add Ip" OnClick="btn_addIp_Click" OnClientClick="if ($.trim($('#tb_createnew_listener').val()) == '') { return false; } else { openWSE.LoadingMessage1('Updating. Please Wait...'); }" />
                                            <span id="ipMessage" runat="server"></span>
                                            <div class="clear-space"></div>
                                        </asp:Panel>
                                        <asp:Panel ID="pnl_groupNetwork" runat="server">
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons" value="Close" onclick="$('#MainContent_ipMessage').html(''); $('#tb_ipAdd').val(''); openWSE.LoadModalWindow(false, 'GroupNetwork-element', '');" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/grouporg.js")%>'></script>
    </div>
</asp:Content>
