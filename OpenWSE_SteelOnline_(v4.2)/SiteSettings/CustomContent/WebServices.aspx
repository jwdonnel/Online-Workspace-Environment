<%@ Page Title="Web Services" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="WebServices.aspx.cs" Inherits="SiteSettings_WebServices" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <div class="clear-margin">
            <div class="pad-bottom-sml float-left pad-top-sml">
                <small><b class="pad-right-sml">Note:</b>These are open webservices which means anyone
            can access them on the web.</small>
            </div>
        </div>
        <div class="clear-space">
        </div>
        <div class="clear-space">
        </div>
        <asp:UpdatePanel ID="updatepnl1" runat="server">
            <ContentTemplate>
                <h3>Web Services Available
                </h3>
                <div class="clear-space">
                </div>
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
        <div class="clear" style="height: 35px;">
        </div>
        <div class="border-bottom">
        </div>
        <div class="clear" style="height: 30px;">
        </div>
        <h3 class="font-bold float-left">Upload Web Service File</h3>
        <div class="clear-space-five">
        </div>
        <small><span class="font-bold pad-right-sml">Note:</span>Only file extensions with .wsdl
    or .asmx are allowed.</small>
        <div class="clear-space">
        </div>
        <asp:Panel ID="pnl_uploadNew" runat="server" DefaultButton="btn_uploadFile">
            <asp:FileUpload ID="fu_newWebServiceFile" runat="server" />
            <div class="clear-space-five">
            </div>
            <a href="#" onclick="if ($('#div-uploadbyurl').css('display') == 'none') { $('#div-uploadbyurl').fadeIn(150); }else { $('#div-uploadbyurl').fadeOut(150); }return false;">Click Here</a> to Show/Hide Upload by Url
        <div class="clear-space-five">
        </div>
            <div id="div-uploadbyurl" style="display: none;">
                <div class="clear-space">
                </div>
                <asp:Label ID="lbl_webserviceName" runat="server" Text="Name:" CssClass="font-bold pad-right-sml pad-top-sml float-left"
                    AssociatedControlID="tb_wsdlName" Width="85px"></asp:Label>
                <asp:TextBox ID="tb_wsdlName" runat="server" CssClass="textEntry margin-right" Text=""
                    Width="400px" MaxLength="100"></asp:TextBox>
                <div class="clear-space">
                </div>
                <asp:Label ID="lbl_webserviceUrl" runat="server" Text="Wsdl Url:" CssClass="font-bold pad-right-sml pad-top-sml float-left"
                    AssociatedControlID="tb_wsdlUrl" Width="85px"></asp:Label>
                <asp:TextBox ID="tb_wsdlUrl" runat="server" CssClass="textEntry" Text="" Width="400px"></asp:TextBox>
            </div>
            <div class="clear-space">
            </div>
            <asp:Label ID="lbl_descriptionUpload" runat="server" Text="Description:" CssClass="font-bold pad-right-sml pad-top-sml float-left"
                AssociatedControlID="tb_descriptionUpload" Width="85px"></asp:Label>
            <asp:TextBox ID="tb_descriptionUpload" runat="server" CssClass="textEntry margin-right-big"
                Text="" Width="400px" TextMode="MultiLine" Height="75px" Style="border: 1px solid #DFDFDF; border-right: 1px solid #CCC; border-bottom: 1px solid #CCC;"></asp:TextBox>
            <asp:Button ID="btn_uploadFile" runat="server" CssClass="input-buttons RandomActionBtns"
                OnClick="btn_uploadFile_Clicked" Text="Upload" />
            <div class="clear-space">
            </div>
            <asp:Label ID="lbl_Error" runat="server" Text="" ForeColor="Red"></asp:Label>
            <div class="clear-space">
            </div>
        </asp:Panel>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/webservices.js")%>'></script>
    </div>
</asp:Content>
