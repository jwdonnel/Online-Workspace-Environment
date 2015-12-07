<%@ page title="Custom Projects" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_CustomProjects, App_Web_5emrkdig" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:Panel ID="pnl_uploadNew" runat="server">
            <div class="clear-space"></div>
            <a href="#" class="input-buttons-create float-left margin-right-big" onclick="openWSE.LoadModalWindow(true, 'UploadService-element', 'Custom Project Upload');return false;">Upload Project</a>
            <a href="#iframecontent" id="createcustompage" class="margin-right-big input-buttons-create float-left" runat="server" onclick="openWSE.LoadIFrameContent('SiteTools/iframes/ProjectExplorer.aspx', this);return false;">Create Project</a>
            <div class="searchwrapper float-left" style="width: 350px; margin-top: 3px;">
                <asp:UpdatePanel ID="updatepnl_search" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_Search" runat="server" DefaultButton="imgbtn_search">
                            <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                                onfocus="if(this.value=='Search Projects')this.value=''" onblur="if(this.value=='')this.value='Search Projects'"
                                Text="Search Projects" data-defaultvalue="Search Projects"></asp:TextBox>
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
            <div class="clear"></div>
            <asp:Label ID="lbl_Error" runat="server" Text="" ForeColor="Red" CssClass="float-right pad-right"></asp:Label>
            <div id="UploadService-element" class="Modal-element" style="display: none;">
                <div class="Modal-overlay">
                    <div class="Modal-element-align">
                        <div class="Modal-element-modal" data-setwidth="500">
                            <div class="ModalHeader">
                                <div>
                                    <div class="app-head-button-holder-admin">
                                        <a href="#" onclick="openWSE.LoadModalWindow(false, 'UploadService-element', '');return false;" class="ModalExitButton"></a>
                                    </div>
                                    <span class="Modal-title"></span>
                                </div>
                            </div>
                            <div class="ModalScrollContent">
                                <div class="ModalPadContent">
                                    Any code related files such as .dll will be added with the page. You will not be able to modify or view these files. When connecting to an FTP location, make sure you specify the correct folder you are wanted to connect to by adding it in the FTP Address.
                                    <div class="clear-space"></div>
                                    <div class="clear-space"></div>
                                    <div id="div_ftpUpload" class="float-left" style="display: none;">
                                        <input id="tb_ftpLocation" type="text" class="textEntry" style="width: 305px;" placeholder="FTP Address" />
                                        <a href="#" class="margin-left-big" onclick="UploadFiles();return false;">Upload Files</a>
                                        <div class="clear-space"></div>
                                        <input id="tb_ftpUsername" type="text" class="textEntry" style="width: 200px;" placeholder="Username" />
                                        <span class="margin-left-big"><small>Use FTP login information</small></span>
                                        <div class="clear-space"></div>
                                        <input id="tb_ftpPassword" type="password" class="textEntry" style="width: 200px;" placeholder="Password" />
                                    </div>
                                    <div id="div_fileUpload" class="float-left">
                                        <asp:FileUpload ID="fu_newWebServiceFile" runat="server" Width="280px" />
                                        <a href="#" class="margin-left-big" onclick="ConnectFTP();return false;">Connect via FTP</a>
                                        <div class="clear-space"></div>
                                    </div>
                                    <div class="clear-space"></div>
                                    <asp:TextBox ID="txt_UploadName" runat="server" CssClass="textEntry margin-right-big"
                                        Text="" Width="400px" MaxLength="150" placeholder="Name"></asp:TextBox>
                                    <div class="clear-space"></div>
                                    <asp:TextBox ID="tb_descriptionUpload" runat="server" CssClass="textEntry"
                                        Text="" Width="400px" MaxLength="500" placeholder="Description"></asp:TextBox>
                                    <div class="clear-space"></div>
                                    <input id="btn_ftpUpload" type="button" class="input-buttons" value="Connect" onclick="TryFTPConnect();" style="display: none;" />
                                    <div class="clear-space">
                                    </div>
                                </div>
                            </div>
                            <div class="ModalButtonHolder">
                                <asp:Button ID="btn_uploadFile" runat="server" CssClass="input-buttons float-left no-margin RandomActionBtns"
                                    OnClick="btn_uploadFile_Clicked" Text="Upload" />
                                <input type="button" value="Close" class="input-buttons float-right no-margin" onclick="openWSE.LoadModalWindow(false, 'UploadService-element', '');" />
                                <div class="clear"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
        <div class="clear"></div>
        <asp:UpdatePanel ID="updatepnl1" runat="server">
            <ContentTemplate>
                <div class="table-settings-box no-border" style="margin-top: 0px !important;">
                    <div class="td-settings-ctrl">
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

