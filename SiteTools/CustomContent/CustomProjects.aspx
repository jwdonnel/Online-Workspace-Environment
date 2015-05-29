<%@ page title="Custom Projects" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_CustomProjects, App_Web_qakpaghm" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:UpdatePanel ID="updatepnl1" runat="server">
            <ContentTemplate>
                <div class="table-settings-box">
                    <div class="td-settings-ctrl">
                        <a href="#iframecontent" id="createcustompage" class="margin-right-big input-buttons-create" runat="server" onclick="openWSE.LoadIFrameContent('SiteTools/iframes/ProjectExplorer.aspx', this);return false;">Create Project</a>
                        <asp:Label ID="lbl_TotalWebServices" runat="server"></asp:Label>
                        <div class="clear-space"></div>
                        <asp:LinkButton ID="lbtn_refresh" runat="server" Text="" CssClass="img-refresh float-right RandomActionBtns"
                            OnClick="lbtn_refresh_Clicked" ToolTip="Refresh"></asp:LinkButton>
                        <div class="clear-space-two">
                        </div>
                        <asp:Panel ID="pnl_CustomPageList" runat="server" ClientIDMode="Static">
                        </asp:Panel>
                        <asp:HiddenField ID="hf_DeleteCP" runat="server" ClientIDMode="Static" OnValueChanged="hf_DeleteCP_Changed" />
                        <asp:HiddenField ID="hf_DescriptionEdit" runat="server" ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_nameEdit" runat="server" ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_CancelCP" runat="server" ClientIDMode="Static" OnValueChanged="hf_CancelCP_Changed" />
                        <asp:HiddenField ID="hf_refreshList" runat="server" ClientIDMode="Static" OnValueChanged="lbtn_refresh_Clicked" />
                        <asp:HiddenField ID="hf_download" runat="server" ClientIDMode="Static" />
                    </div>
                    <div class="td-settings-desc">
                        FTP pages will require a username and password upon edit. No credentials are stored on this site.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:Button ID="btn_download_hidden" runat="server" ClientIDMode="Static" OnClick="DownloadProject" Style="display: none;"></asp:Button>
        <asp:Panel ID="pnl_uploadNew" runat="server">
            <div class="table-settings-box">
                <div class="td-settings-title">
                    Upload Project Files
                </div>
                <div class="title-line"></div>
                <div class="td-settings-ctrl">
                    <div id="div_ftpUpload" class="float-left" style="display: none;">
                        <span class="settings-name-column float-left">FTP Address</span>
                        <input id="tb_ftpLocation" type="text" class="textEntry" style="width: 305px;" />
                        <a href="#" class="margin-left-big" onclick="UploadFiles();return false;">Upload Files</a>
                        <div class="clear-space"></div>
                        <span class="settings-name-column float-left">Username</span>
                        <input id="tb_ftpUsername" type="text" class="textEntry" style="width: 200px;" />
                        <span class="margin-left-big"><small>Use FTP login information</small></span>
                        <div class="clear-space"></div>
                        <span class="settings-name-column float-left">Password</span>
                        <input id="tb_ftpPassword" type="password" class="textEntry" style="width: 200px;" />
                    </div>
                    <span class="settings-name-column float-left"></span>
                    <div id="div_fileUpload" class="float-left">
                        <asp:FileUpload ID="fu_newWebServiceFile" runat="server" Width="280px" />
                        <a href="#" class="margin-left-big" onclick="ConnectFTP();return false;">Connect via FTP</a>
                        <div class="clear-space"></div>
                    </div>
                    <div class="clear-space"></div>
                    <asp:Label ID="lbl_UploadName" runat="server" Text="Name" CssClass="settings-name-column float-left"
                        AssociatedControlID="txt_UploadName"></asp:Label>
                    <asp:TextBox ID="txt_UploadName" runat="server" CssClass="textEntry margin-right-big"
                        Text="" Width="400px" MaxLength="150"></asp:TextBox>
                    <div class="clear-space"></div>
                    <asp:Label ID="lbl_descriptionUpload" runat="server" Text="Description" CssClass="settings-name-column float-left"
                        AssociatedControlID="tb_descriptionUpload"></asp:Label>
                    <asp:TextBox ID="tb_descriptionUpload" runat="server" CssClass="textEntry margin-right-big"
                        Text="" Width="400px" MaxLength="500" TextMode="MultiLine" Height="50px" Style="border: 1px solid #DFDFDF; border-right: 1px solid #CCC; border-bottom: 1px solid #CCC;"></asp:TextBox>
                    <div class="clear-space"></div>
                    <span class="settings-name-column float-left"></span>
                    <input id="btn_ftpUpload" type="button" class="input-buttons" value="Connect" onclick="TryFTPConnect();" style="display: none;" />
                    <asp:Button ID="btn_uploadFile" runat="server" CssClass="input-buttons-create RandomActionBtns"
                        OnClick="btn_uploadFile_Clicked" Text="Upload" />
                    <div class="clear-space">
                    </div>
                    <asp:Label ID="lbl_Error" runat="server" Text="" ForeColor="Red"></asp:Label>
                </div>
                <div class="td-settings-desc">
                    Any code related files such as .dll will be added with the page. You will not be able to modify or view these files.<br />
                    When connecting to an FTP location, make sure you specify the correct folder you are wanted to connect to by adding it in the FTP Address.
                </div>
            </div>
        </asp:Panel>
        <div id="LoaderApp-element" class="Modal-element" style="display: none;">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class='Modal-element-modal'>
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
                                <asp:UpdatePanel ID="updatepnl_DefaultPage" runat="server">
                                    <ContentTemplate>
                                        <asp:RadioButtonList ID="radioButton_FileList" runat="server">
                                        </asp:RadioButtonList>
                                        <div class="clear-space">
                                        </div>
                                        <asp:HiddenField ID="hf_defaultProjectID" runat="server" ClientIDMode="Static" />
                                        <asp:Button ID="btn_updateDefaultFile" runat="server" CssClass="input-buttons RandomActionBtns"
                                            Text="Set Page" OnClick="btn_updateDefaultFile_Clicked" />
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/customprojects.js")%>'></script>
    </div>
</asp:Content>

