<%@ Page Title="File Manager" Async="true" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="FileManager.aspx.cs" Inherits="SiteTools_FileManager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <asp:Panel ID="pnl1" runat="server">
            The FileManager only loads files with extensions: .js and .css
                        <div class="clear-space"></div>
            <div class="searchwrapper" style="width: 444px;">
                <asp:Panel ID="Panel1_FileManager" runat="server" DefaultButton="imgbtn_search">
                    <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                        onfocus="if(this.value=='Search Files')this.value=''" onblur="if(this.value=='')this.value='Search Files'"
                        Text="Search Files"></asp:TextBox>
                    <a href="#" onclick="$('#MainContent_tb_search').val('Search Files');return false;"
                        class="searchbox_clear" title="Clear search"></a>
                    <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                        OnClick="imgbtn_search_Click" />
                </asp:Panel>
            </div>
            <div class="clear-space">
            </div>
            <b class="pad-right-sml">View Mode:</b>
            <asp:DropDownList ID="dd_viewtype" runat="server" AutoPostBack="true" CssClass="margin-right"
                OnSelectedIndexChanged="dd_viewtype_SelectedIndexChanged" ClientIDMode="Static">
                <asp:ListItem Text="All" Value="all"></asp:ListItem>
                <asp:ListItem Text="css Only" Value=".css"></asp:ListItem>
                <asp:ListItem Text="js Only" Value=".js"></asp:ListItem>
            </asp:DropDownList>
            <div class="clear-space">
            </div>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="pnl_filelist" runat="server">
                    </asp:Panel>
                    <asp:HiddenField ID="hf_downloadFile" runat="server" ClientIDMode="Static" OnValueChanged="hf_downloadFile_ValueChanged" />
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="dd_viewtype" />
                    <asp:AsyncPostBackTrigger ControlID="lbtn_save" />
                    <asp:PostBackTrigger ControlID="hf_downloadFile" />
                    <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>
        <asp:Panel ID="pnl2" runat="server" Enabled="false" Visible="false">
            <h3 class="float-left pad-top-sml">File Editor/Viewer&nbsp;&nbsp;-</h3>
            <asp:Label ID="lbl_currfile" runat="server" CssClass="float-left pad-top margin-left"
                Text=""></asp:Label>
            <div class="clear-space"></div>
            <asp:HyperLink ID="lbtn_close" NavigateUrl="FileManager.aspx" CssClass="float-left input-buttons-create margin-right"
                runat="server" ToolTip="Back">Back</asp:HyperLink>
            <asp:LinkButton ID="lbtn_save" runat="server" OnClientClick=" return ConfirmSaveFile(this); "
                CssClass="float-left input-buttons-create margin-left" ToolTip="Save">Save</asp:LinkButton>
            <div class="clear-space">
            </div>
            <div class="clear-margin">
                <asp:Label ID="lbl_messageRead" runat="server" CssClass="float-right bold" Text=""></asp:Label>
                Do not refresh the browser or press the back button while in the File Editor/Viewer
            </div>
            <div class="clear-space">
            </div>
            <div id="editor" style="display: none;">
            </div>
            <asp:HiddenField ID="hidden_editor" runat="server" ClientIDMode="Static" />
        </asp:Panel>
        <script src='<%=ResolveUrl("~/Scripts/AceEditor/ace.js")%>' type="text/javascript" charset="utf-8"></script>
    </div>
</asp:Content>
