<%@ page title="Group Organizer" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_GroupOrg, App_Web_thajkrpw" %>

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
                            <a class="margin-right-big input-buttons" onclick="CreateGroup();return false;">
                                <span class="td-add-btn float-left margin-right-sml" style="padding: 0!important;"></span>Create a Group</a>
                            <b class="pad-right">Group Count</b><asp:Label ID="lbl_companycount" runat="server" Text="0"></asp:Label>
                            <div class="clear-space">
                            </div>
                        </asp:Panel>
                        <div class="clear-space-five"></div>
                        <div class="float-left">
                            <div id="searchwrapper">
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
                        </div>
                        <div class="clear-space"></div>
                    </div>
                    <div class="td-settings-desc">
                        The groups created are used to determine how to pull data from each app. Each user created MUST be associated with a group. When sending a user an invite, that user must have their group notification enabled. Otherwise, you will not see the user in the invite list.<br />
                    </div>
                </div>
                <div class="clear-space">
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
                                    <a href="#" onclick="openWSE.LoadModalWindow(false, 'NewEdit-Group-element', '');ResetControls();return false;"
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
                                                <span style="color: #555;">Group Name</span>
                                                <br />
                                                <asp:TextBox ID="tb_companyname" runat="server" CssClass="textEntryReg" Width="172px"
                                                    MaxLength="500"></asp:TextBox>
                                                <asp:Label ID="lbl_error" runat="server" Text="" Font-Size="X-Small" ForeColor="Red"></asp:Label>
                                                <asp:Label ID="lbl_tempcompanyname" runat="server" Text="" Visible="false" Enabled="false"></asp:Label>
                                                <div class="clear-space">
                                                </div>
                                                <span style="color: #555;">Phone Number</span>
                                                <br />
                                                <input id="company_phone1" runat="server" type="text" class="TextBoxControls" style="width: 35px;"
                                                    maxlength="3" />
                                                -
                                        <input id="company_phone2" runat="server" type="text" class="TextBoxControls" style="width: 35px;"
                                            maxlength="3" />
                                                -
                                        <input id="company_phone3" runat="server" type="text" class="TextBoxControls" style="width: 45px;"
                                            maxlength="4" />
                                                <div class="clear-space">
                                                </div>
                                                <span style="color: #555;">Upload a Logo</span>
                                                <br />
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <asp:FileUpload ID="fu_image_create" runat="server" />
                                        <div class="clear-space-five">
                                        </div>
                                        <small><i><b>.png</b> <b>.jpeg</b> and <b>.gif</b> are allowed</i></small>
                                        <div class="clear-space">
                                        </div>
                                    </div>
                                </div>
                                <asp:UpdatePanel ID="updatepnl_editmode_2" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <div style="height: 180px;" class="inline-block float-right pad-left-big">
                                            <div class="clear-margin" style="height: 87px;">
                                                <span style="color: #555;">Address</span>
                                                <br />
                                                <asp:TextBox ID="tb_address" runat="server" CssClass="textEntryReg" Width="235px"></asp:TextBox>
                                                <div class="clear-space">
                                                </div>
                                                <div class="float-left pad-right">
                                                    <span style="color: #555;">City</span>
                                                    <br />
                                                    <asp:TextBox ID="tb_city" runat="server" CssClass="textEntryReg" Width="90px" MaxLength="250"></asp:TextBox>
                                                </div>
                                                <div class="float-left pad-right">
                                                    <span style="color: #555;">State</span>
                                                    <br />
                                                    <select id="dd_state" runat="server">
                                                        <option value="AL">AL</option>
                                                        <option value="AK">AK</option>
                                                        <option value="AZ">AZ</option>
                                                        <option value="AR">AR</option>
                                                        <option value="CA">CA</option>
                                                        <option value="CO">CO</option>
                                                        <option value="CT">CT</option>
                                                        <option value="DE">DE</option>
                                                        <option value="DC">DC</option>
                                                        <option value="FL">FL</option>
                                                        <option value="GA">GA</option>
                                                        <option value="HI">HI</option>
                                                        <option value="ID">ID</option>
                                                        <option value="IL">IL</option>
                                                        <option value="IN">IN</option>
                                                        <option value="IA">IA</option>
                                                        <option value="KS" selected="selected">KS</option>
                                                        <option value="KY">KY</option>
                                                        <option value="LA">LA</option>
                                                        <option value="ME">ME</option>
                                                        <option value="MD">MD</option>
                                                        <option value="MA">MA</option>
                                                        <option value="MI">MI</option>
                                                        <option value="MN">MN</option>
                                                        <option value="MS">MS</option>
                                                        <option value="MO">MO</option>
                                                        <option value="MT">MT</option>
                                                        <option value="NE">NE</option>
                                                        <option value="NV">NV</option>
                                                        <option value="NH">NH</option>
                                                        <option value="NJ">NJ</option>
                                                        <option value="NM">NM</option>
                                                        <option value="NY">NY</option>
                                                        <option value="NC">NC</option>
                                                        <option value="ND">ND</option>
                                                        <option value="OH">OH</option>
                                                        <option value="OK">OK</option>
                                                        <option value="OR">OR</option>
                                                        <option value="PA">PA</option>
                                                        <option value="RI">RI</option>
                                                        <option value="SC">SC</option>
                                                        <option value="SD">SD</option>
                                                        <option value="TN">TN</option>
                                                        <option value="TX">TX</option>
                                                        <option value="UT">UT</option>
                                                        <option value="VT">VT</option>
                                                        <option value="VA">VA</option>
                                                        <option value="WA">WA</option>
                                                        <option value="WV">WV</option>
                                                        <option value="WI">WI</option>
                                                        <option value="WY">WY</option>
                                                    </select>
                                                </div>
                                                <div class="float-left pad-right">
                                                    <span style="color: #555;">Postal Code</span>
                                                    <br />
                                                    <asp:TextBox ID="tb_postalcode" runat="server" CssClass="textEntryReg" Width="55px"
                                                        MaxLength="10"></asp:TextBox>
                                                </div>
                                            </div>
                                            <div class="clear-margin" style="width: 270px;">
                                                <div class="" style="padding-top: 15px;">
                                                    <p class="font-color-light-black">
                                                        If you have a URL that you would like to use for your logo, enter it in the textbox
                                                below.
                                                    </p>
                                                    <div class="clear-space">
                                                    </div>
                                                    <span class="pad-right" style="color: #555;">Image Url</span><br />
                                                    <asp:TextBox ID="tb_imageurl" runat="server" CssClass="textEntry" onfocus="if(this.value=='Link to image')this.value=''"
                                                        onblur="if(this.value=='')this.value='Link to image'" Text="Link to image" Width="235px"></asp:TextBox>
                                                    <div class="clear-space">
                                                    </div>
                                                    <asp:CheckBox ID="cb_isprivate" runat="server" Text="&nbsp;Is Private" Style="color: #555;" />
                                                    <div class="clear-space">
                                                    </div>
                                                    <a href="#" id="group-userdefaults-button" title="Change the user default settings for users that log into this group.">Change Default User Settings</a>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="clear-space">
                                        </div>
                                        <div class="inline-block float-left pad-right-big" style="margin-top: -10px;">
                                            <asp:Image ID="img_logo" runat="server" Style="display: none; max-height: 40px; max-width: 175px;" />
                                        </div>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <div class="clear-space">
                                </div>
                                <asp:Button ID="btn_finish_add" runat="server" Text="Save Group" CssClass="input-buttons"
                                    OnClick="btn_finish_add_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="GroupEdit-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="600">
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
                    </div>
                </div>
            </div>
        </div>
        <div id="GroupInviteUser-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="600">
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
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/grouporg.js")%>'></script>
    </div>
</asp:Content>
