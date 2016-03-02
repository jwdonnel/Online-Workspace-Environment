<%@ Page Title="Database Viewer" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="dbViewer.aspx.cs" Inherits="SiteTools_dbViewer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <div class="table-settings-box">
            <div class="td-settings-ctrl">
                <span class="settings-name-column float-left" style="padding-top: 0px!important;">Update Interval</span>
                <asp:DropDownList ID="dd_interval" CssClass="margin-right" runat="server" ClientIDMode="Static">
                    <asp:ListItem Text="1 Seconds" Value="1000"></asp:ListItem>
                    <asp:ListItem Text="2 Seconds" Value="2000"></asp:ListItem>
                    <asp:ListItem Text="5 Seconds" Value="5000"></asp:ListItem>
                    <asp:ListItem Text="10 Seconds" Value="10000" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="15 Seconds" Value="15000"></asp:ListItem>
                </asp:DropDownList>
                <a href="#" id="a_turn_onoff_refresh" class="dbviewer-update-img">Turn off</a>
                <div class="clear-space">
                </div>
                <span class="settings-name-column float-left margin-top" style="padding-top: 3px!important;">Table Name</span>
                <asp:DropDownList ID="dd_table" runat="server" CssClass="float-left margin-right margin-top"
                    ClientIDMode="Static" AutoPostBack="true" OnSelectedIndexChanged="dd_table_Changed">
                </asp:DropDownList>
                <div class="float-left margin-top">
                    <asp:UpdatePanel ID="UpdatePanel3" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:LinkButton ID="lbtn_savetable" runat="server" CssClass="margin-left float-left dbviewer-update-img input-buttons"
                                OnClick="lbtn_savetable_Click"></asp:LinkButton>
                            <asp:Label ID="lbl_tablesaved_msg" runat="server" Text="Table has been backed up."
                                ForeColor="Green" Font-Size="11px" CssClass="pad-left-big float-left" Visible="false"
                                Enabled="false" Style="padding-top: 6px;"></asp:Label>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbtn_savetable" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
                <div class="clear-space">
                </div>
                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Label ID="lbl_rowCount" runat="server" Text=""></asp:Label>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                Only customized tables are shown in the drop down
        menu. All ASP.Net tables are restricted and cannot be viewed by any user.
            </div>
        </div>
        <asp:UpdatePanel ID="UpdatePanel4" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel ID="pnl_tableSchema" runat="server"></asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class='float-right'>
            <asp:LinkButton ID="btn_refresh" runat="server" CssClass="float-right img-refresh dbviewer-update-img dbviewer-update"
                ToolTip="Refresh Table" OnClick="lbtn_refresh_Click" />
            <div class='pad-right-big float-right'>
                <asp:DropDownList ID="dd_display" runat="server" ClientIDMode="Static" Width="60px"
                    AutoPostBack="True" OnSelectedIndexChanged="dd_dbviewer_SelectedIndexChanged">
                    <asp:ListItem Value="10">10</asp:ListItem>
                    <asp:ListItem Value="20" Selected="True">20</asp:ListItem>
                    <asp:ListItem Value="30">30</asp:ListItem>
                    <asp:ListItem Value="40">40</asp:ListItem>
                    <asp:ListItem Value="1">All</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class='clear-space'>
        </div>
        <div class='clear-space-five'>
        </div>
        <div id="search_box">
            <div class="searchwrapper" style="width: 400px">
                <asp:Panel ID="Panel1_dbviewer" runat="server" DefaultButton="imgbtn_search">
                    <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                        onfocus="if(this.value=='Search Database Table')this.value=''" onblur="if(this.value=='')this.value='Search Database Table'"
                        Text="Search Database Table"></asp:TextBox>
                    <asp:LinkButton ID="imgbtn_clearsearch" runat="server" ToolTip="Clear search" OnClientClick="$('#MainContent_tb_search').val('Search Database Table');" CssClass="searchbox_clear RandomActionBtns"
                        OnClick="imgbtn_clearsearch_Click" />
                    <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                        OnClick="imgbtn_search_Click" />
                </asp:Panel>
            </div>
            <span class="float-right margin-left-big"><small>Leave search textbox blank to search
            all events</small></span>
        </div>
        <div class="clear-space">
        </div>
        <div class="clear-space-five">
        </div>
        <asp:UpdatePanel ID="UpdatePanel2" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hf_current" runat="server" />
                <asp:HiddenField ID="hf_updatetable" runat="server" OnValueChanged="hf_updatetable_Changed"
                    ClientIDMode="Static" />
                <asp:GridView ID="GV_dbviewer" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="True"
                    Width="100%" ShowHeaderWhenEmpty="True" AllowPaging="True" AllowSorting="True" BorderStyle="None"
                    PageSize="20" OnPageIndexChanging="GV_dbviewer_PageIndexChanging" OnRowCreated="GV_dbviewer_RowCreated"
                    OnSorting="GV_dbviewer_Sorting">
                    <PagerSettings Position="Bottom" />
                    <PagerStyle CssClass="GridViewPager dbviewer-update" />
                    <EmptyDataRowStyle ForeColor="Black" />
                    <RowStyle CssClass="GridNormalRow" />
                    <EmptyDataTemplate>
                        <div class="emptyGridView">
                            No data available
                        </div>
                    </EmptyDataTemplate>
                    <HeaderStyle CssClass="myHeaderStyle" BorderColor="#B7B7B7" />
                </asp:GridView>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="dd_display" />
                <asp:AsyncPostBackTrigger ControlID="dd_table" />
                <asp:AsyncPostBackTrigger ControlID="btn_refresh" />
                <asp:AsyncPostBackTrigger ControlID="imgbtn_clearsearch" />
                <asp:AsyncPostBackTrigger ControlID="imgbtn_search" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
