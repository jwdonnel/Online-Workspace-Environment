<%@ Page Title="Database Viewer" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="dbViewer.aspx.cs" Inherits="SiteTools_dbViewer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="table-settings-box">
        <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
            Table Viewer
        </div>
        <div class="title-line"></div>
        <div class="td-settings-ctrl">
            <div class="input-settings-holder">
                <span class="font-bold">Update Interval</span>
                <div class="clear-space-two"></div>
                <asp:DropDownList ID="dd_interval" CssClass="margin-right" runat="server" ClientIDMode="Static">
                    <asp:ListItem Text="1 Seconds" Value="1000"></asp:ListItem>
                    <asp:ListItem Text="2 Seconds" Value="2000"></asp:ListItem>
                    <asp:ListItem Text="5 Seconds" Value="5000"></asp:ListItem>
                    <asp:ListItem Text="10 Seconds" Value="10000" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="15 Seconds" Value="15000"></asp:ListItem>
                </asp:DropDownList>
                <a href="#" id="a_turn_onoff_refresh" class="dbviewer-update-img">Turn off</a>
                <div class="clear"></div>
            </div>
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <div class="input-settings-holder">
                        <span class="font-bold">Table Name</span>
                        <div class="clear-space-two"></div>
                        <asp:DropDownList ID="dd_table" runat="server" CssClass="float-left margin-right-big"
                            ClientIDMode="Static" AutoPostBack="true" OnSelectedIndexChanged="dd_table_Changed">
                        </asp:DropDownList>
                        <div class="float-left">
                            <asp:LinkButton ID="lbtn_savetable" runat="server" CssClass="float-left dbviewer-update-img input-buttons"
                                OnClick="lbtn_savetable_Click"></asp:LinkButton>
                            <asp:Label ID="lbl_tablesaved_msg" runat="server" Text="Table has been backed up."
                                ForeColor="Green" Font-Size="11px" CssClass="pad-left-big float-left" Visible="false"
                                Enabled="false" Style="padding-top: 6px;"></asp:Label>
                        </div>
                        <div class="clear"></div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="td-settings-desc">
            Only customized tables are shown in the drop down menu. All ASP.Net tables are restricted and cannot be viewed by any user.
        </div>
    </div>
    <div class="clear-space"></div>
    <div id="tableViewerAccordion" class="custom-accordion" data-nocookie="false">
        <h3>Entries</h3>
        <div>
            <asp:Panel ID="pnl_rowsToSelect" runat="server" DefaultButton="btn_updateRowsToSelect" CssClass="float-left">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <span class="pad-right font-bold">Rows to Select</span>
                        <asp:TextBox ID="tb_rowsToSelect" runat="server" CssClass="textEntry margin-right-sml" TextMode="Number" Width="75px" min="1"></asp:TextBox>
                        <asp:Button ID="btn_updateRowsToSelect" runat="server" CssClass="input-buttons RandomActionBtns" Text="Update" OnClick="btn_updateRowsToSelect_Click" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
            <asp:LinkButton ID="btn_refresh" runat="server" CssClass="float-right dbviewer-update-img dbviewer-update" OnClick="lbtn_refresh_Click" Text="Refresh" />
            <div class="clear"></div>
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <asp:HiddenField ID="hf_updatetable" runat="server" OnValueChanged="hf_updatetable_Changed"
                        ClientIDMode="Static" />
                    <asp:Panel ID="pnl_gridviewholder" runat="server">
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btn_refresh" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
        <h3>Table Schema</h3>
        <div>
            <asp:UpdatePanel ID="UpdatePanel4" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnl_tableSchema" runat="server"></asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</asp:Content>
