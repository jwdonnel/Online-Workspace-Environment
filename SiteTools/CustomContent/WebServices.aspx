<%@ page title="Web Services" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_WebServices, App_Web_5emrkdig" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .settings-name-column
        {
            width: 90px !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <div class="clear-space"></div>
        <a href="#" class="margin-right-big input-buttons-create float-left" onclick="openWSE.LoadModalWindow(true, 'UploadService-element', 'Web Service Upload');return false;">Service Upload</a>
        <div class="searchwrapper float-left" style="width: 350px; margin-top: 3px;">
            <asp:UpdatePanel ID="updatepnl_search" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnl_Search" runat="server" DefaultButton="imgbtn_search">
                        <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                            onfocus="if(this.value=='Search Web Services')this.value=''" onblur="if(this.value=='')this.value='Search Web Services'"
                            Text="Search Web Services" data-defaultvalue="Search Web Services"></asp:TextBox>
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
                                <asp:Panel ID="pnl_uploadNew" runat="server" DefaultButton="btn_uploadFile">
                                    Only file extensions with .wsdl or .asmx are allowed.
                                    <div class="clear-space"></div>
                                    <div class="clear-space"></div>
                                    <asp:FileUpload ID="fu_newWebServiceFile" runat="server" />
                                    <div class="clear-space">
                                    </div>
                                    <a href="#" onclick="if ($('#div-uploadbyurl').css('display') == 'none') { $('#div-uploadbyurl').fadeIn(150); }else { $('#div-uploadbyurl').fadeOut(150); }return false;">Click Here</a> to Show/Hide Upload by Url
                                    <div class="clear-space">
                                    </div>
                                    <div id="div-uploadbyurl" style="display: none;">
                                        <div class="clear-space">
                                        </div>
                                        <asp:TextBox ID="tb_wsdlName" runat="server" CssClass="textEntry margin-right" Text=""
                                            Width="400px" MaxLength="100" placeholder="Name"></asp:TextBox>
                                        <div class="clear-space">
                                        </div>
                                        <asp:TextBox ID="tb_wsdlUrl" runat="server" CssClass="textEntry" Text="" Width="400px" placeholder="Wsdl Url"></asp:TextBox>
                                    </div>
                                    <div class="clear-space">
                                    </div>
                                    <asp:TextBox ID="tb_descriptionUpload" runat="server" CssClass="textEntry"
                                        Text="" Width="400px" MaxLength="1000" placeholder="Description"></asp:TextBox>
                                </asp:Panel>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <asp:Button ID="btn_uploadFile" runat="server" CssClass="input-buttons float-left no-margin RandomActionBtns" OnClick="btn_uploadFile_Clicked" Text="Upload" />
                            <input type="button" value="Close" class="input-buttons float-right no-margin" onclick="openWSE.LoadModalWindow(false, 'UploadService-element', '');" />
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="table-settings-box no-border" style="margin-top: 0px!important;">
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="updatepnl1" runat="server">
                    <ContentTemplate>
                        <asp:Label ID="lbl_TotalWebServices" runat="server" CssClass="float-left"></asp:Label>
                        <div class="clear-space"></div>
                        <div class="clear-space"></div>
                        <div class="clear-space"></div>
                        <div class="float-left pad-right-big">
                            <asp:CheckBox ID="cb_hideStandardServices" runat="server" ClientIDMode="Static" Enabled="false" Visible="false" OnCheckedChanged="cb_hideStandardServices_CheckedChanged" AutoPostBack="true" Text="&nbsp;Hide Standard Web Services" Checked="true" />
                        </div>
                        <asp:LinkButton ID="lbtn_refresh" runat="server" Text="" CssClass="img-refresh float-right RandomActionBtns"
                            OnClick="lbtn_refresh_Clicked" ToolTip="Refresh"></asp:LinkButton>
                        <div class="clear-space-two">
                        </div>
                        <asp:Panel ID="pnl_WebServiceList" runat="server" ClientIDMode="Static">
                        </asp:Panel>
                        <asp:HiddenField ID="hf_DeleteWS" runat="server" ClientIDMode="Static" OnValueChanged="hf_DeleteWS_Changed" />
                        <asp:HiddenField ID="hf_EditWS" runat="server" ClientIDMode="Static" OnValueChanged="hf_EditWS_Changed" />
                        <asp:HiddenField ID="hf_UpdateWS" runat="server" ClientIDMode="Static" OnValueChanged="hf_UpdateWS_Changed" />
                        <asp:HiddenField ID="hf_FilenameEdit" runat="server" ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_DescriptionEdit" runat="server" ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_CancelWS" runat="server" ClientIDMode="Static" OnValueChanged="hf_CancelWS_Changed" />
                        <asp:HiddenField ID="hf_wsId" runat="server" ClientIDMode="Static" />
                    </ContentTemplate>
                </asp:UpdatePanel>
                <asp:Button ID="btn_download_hidden" runat="server" ClientIDMode="Static" OnClick="DownloadWebservice" Style="display: none;"></asp:Button>
            </div>
            <div class="td-settings-desc">
                These are open webservices which means anyone can access them on the web. The purpose of this page is to allow users to upload or save wsdl files or asmx files to be used either on other sites or within this one.
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/webservices.js")%>'></script>
    </div>
</asp:Content>
