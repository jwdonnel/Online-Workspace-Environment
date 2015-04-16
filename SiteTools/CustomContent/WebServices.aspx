<%@ page title="Web Services" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_WebServices, App_Web_kmou1qyu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <div class="table-settings-box">
            <div class="td-settings-title">
                Web Services Available
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <asp:UpdatePanel ID="updatepnl1" runat="server">
                    <ContentTemplate>
                        <asp:Label ID="lbl_TotalWebServices" runat="server" CssClass="float-left"></asp:Label>
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
                These are open webservices which means anyone
            can access them on the web.
            </div>
        </div>
        <div class="table-settings-box">
            <div class="td-settings-title">
                Upload Web Service File
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <asp:Panel ID="pnl_uploadNew" runat="server" DefaultButton="btn_uploadFile">
                    <span class="settings-name-column float-left"></span>
                    <asp:FileUpload ID="fu_newWebServiceFile" runat="server" />
                    <div class="clear-space">
                    </div>
                    <span class="settings-name-column float-left"></span><a href="#" onclick="if ($('#div-uploadbyurl').css('display') == 'none') { $('#div-uploadbyurl').fadeIn(150); }else { $('#div-uploadbyurl').fadeOut(150); }return false;">Click Here</a> to Show/Hide Upload by Url
                    <div class="clear-space">
                    </div>
                    <div id="div-uploadbyurl" style="display: none;">
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="lbl_webserviceName" runat="server" Text="Name" CssClass="settings-name-column float-left"
                            AssociatedControlID="tb_wsdlName"></asp:Label>
                        <asp:TextBox ID="tb_wsdlName" runat="server" CssClass="textEntry margin-right" Text=""
                            Width="400px" MaxLength="100"></asp:TextBox>
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="lbl_webserviceUrl" runat="server" Text="Wsdl Url" CssClass="settings-name-column float-left"
                            AssociatedControlID="tb_wsdlUrl"></asp:Label>
                        <asp:TextBox ID="tb_wsdlUrl" runat="server" CssClass="textEntry" Text="" Width="400px"></asp:TextBox>
                    </div>
                    <div class="clear-space">
                    </div>
                    <asp:Label ID="lbl_descriptionUpload" runat="server" Text="Description" CssClass="settings-name-column float-left"
                        AssociatedControlID="tb_descriptionUpload"></asp:Label>
                    <asp:TextBox ID="tb_descriptionUpload" runat="server" CssClass="textEntry margin-right-big"
                        Text="" Width="400px" TextMode="MultiLine" Height="75px" Style="border: 1px solid #DFDFDF; border-right: 1px solid #CCC; border-bottom: 1px solid #CCC;"></asp:TextBox>
                    <div class="clear-space">
                    </div>
                    <span class="settings-name-column float-left"></span>
                    <asp:Button ID="btn_uploadFile" runat="server" CssClass="input-buttons RandomActionBtns"
                        OnClick="btn_uploadFile_Clicked" Text="Upload" />
                    <div class="clear-space">
                    </div>
                    <asp:Label ID="lbl_Error" runat="server" Text="" ForeColor="Red"></asp:Label>
                    <div class="clear-space">
                    </div>
                </asp:Panel>
            </div>
            <div class="td-settings-desc">
                Only file extensions with .wsdl or .asmx are allowed.
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/webservices.js")%>'></script>
    </div>
</asp:Content>
