<%@ Page Language="C#" Title="Home Workspace" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeFile="Workspace.aspx.cs" Inherits="Workspace" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <asp:Image ID="container_logo" runat="server" ImageUrl="~/Standard_Images/logo.png" ClientIDMode="Static" />
    <asp:Panel ID="workspace_holder" runat="server" ClientIDMode="Static">
    </asp:Panel>
    <asp:Panel ID="pnl_adminnote" runat="server" class="administrator-workspace-note" Enabled="False" Visible="False">
        <asp:Label ID="lbl_adminnote" runat="server" Text=""></asp:Label>
        <div class="clear-space">
        </div>
        <div style="text-align: right;">
            <asp:Label ID="lbl_adminnoteby" runat="server" CssClass="font-bold" Text=""></asp:Label>
        </div>
    </asp:Panel>
    <asp:HiddenField ID="hf_appContainer" runat="server" Value="" ClientIDMode="Static" />
</asp:Content>
