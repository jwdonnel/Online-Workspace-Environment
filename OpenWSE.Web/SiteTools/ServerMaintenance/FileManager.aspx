<%@ Page Title="File Manager" Async="true" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="FileManager.aspx.cs" Inherits="SiteTools_FileManager" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
    <asp:Panel ID="pnl1" runat="server">
        <div class="table-settings-box">
            <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
                File Manager
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <b class="pad-right">View Mode</b>
                <asp:DropDownList ID="dd_viewtype" runat="server" AutoPostBack="true" CssClass="margin-right"
                    OnSelectedIndexChanged="dd_viewtype_SelectedIndexChanged" ClientIDMode="Static">
                    <asp:ListItem Text="All" Value="all"></asp:ListItem>
                    <asp:ListItem Text="Stylesheet" Value=".css"></asp:ListItem>
                    <asp:ListItem Text="Javascript" Value=".js"></asp:ListItem>
                </asp:DropDownList>
                <div class="clear-space">
                </div>
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <asp:Panel ID="pnl_filelist" runat="server">
                        </asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="dd_viewtype" />
                        <asp:AsyncPostBackTrigger ControlID="lbtn_save" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                The FileManager only loads files with extensions .js and .css
            </div>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnl2" runat="server" Enabled="false" Visible="false">
        <div class="table-settings-box no-margin">
            <div class="td-settings-title">
                File Editor/Viewer
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <asp:Label ID="lbl_currfile" runat="server" Text=""></asp:Label>
                <div class="clear-space">
                </div>
                <asp:Label ID="lbl_messageRead" runat="server" Text=""></asp:Label>
                <div class="clear-space">
                </div>
                <div id="editor" style="font-size: 14px; left: 0; position: relative; top: 0; width: 100%; display: none;">
                </div>
                <div class="clear-space"></div>
                <div class="clear-space"></div>
                <asp:HyperLink ID="lbtn_close" NavigateUrl="FileManager.aspx" CssClass="float-left input-buttons-create no-margin"
                    runat="server" ToolTip="Back">Back</asp:HyperLink>
                <asp:LinkButton ID="lbtn_save" runat="server" OnClientClick=" return ConfirmSaveFile(this); "
                    CssClass="float-right input-buttons-create no-margin" ToolTip="Save">Save</asp:LinkButton>
                <div class="clear-space"></div>
                <asp:HiddenField ID="hidden_editor" runat="server" ClientIDMode="Static" />
            </div>
            <div class="td-settings-desc">
                Do not refresh the browser or press the back button while in the File Editor/Viewer
            </div>
        </div>
    </asp:Panel>
    <script src='<%=ResolveUrl("~/Scripts/AceEditor/ace.js")%>' type="text/javascript" charset="utf-8"></script>
</asp:Content>
