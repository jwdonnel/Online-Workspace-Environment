<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DailyOverview.aspx.cs" Inherits="Apps_DailyOverview_DailyOverview"
    ClientIDMode="Static" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Daily Overview</title>
    <link type="text/css" rel="Stylesheet" href="dailyoverview.css" />
</head>
<body>
    <div id="dailyoverview-load" class="main-div-app-bg">
        <form id="formDailyOverview" runat="server">
            <asp:ScriptManager ID="ScriptManager_SiteMonitor" runat="server" AsyncPostBackTimeout="360000">
            </asp:ScriptManager>
            <asp:UpdatePanel ID="updatepnl_SearchTitleBar" runat="server">
                <ContentTemplate>
                    <div class="pad-all app-title-bg-color">
                        <div class="float-left">
                            <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
                            <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
                        </div>
                        <div class="float-right">
                            <div class="clear-space-five">
                            </div>
                            <div class="searchwrapper">
                                <asp:Panel ID="Panel1_dailyoverview" runat="server" DefaultButton="imgbtn_search_dailyoverview">
                                    <asp:TextBox ID="tb_search_dailyoverview" runat="server" CssClass="searchbox" Font-Size="Small"
                                        placeholder="Search for Truck Schedule" Width="160px"></asp:TextBox>
                                    <a href="#" onclick=" $('#<%= tb_search_dailyoverview.ClientID %>').val('');return false;"
                                        class="searchbox_clear" title="Clear search"></a>
                                    <asp:LinkButton ID="imgbtn_search_dailyoverview" runat="server" ToolTip="Start search"
                                        CssClass="searchbox_submit dailyoverview-update-img" OnClick="imgbtn_search_dailyoverview_click" />
                                </asp:Panel>
                            </div>
                        </div>
                        <div class="float-right pad-right-sml">
                            <div class="clear-space-five">
                            </div>
                            <asp:Button ID="btn_showallmonth_dailyoverview" runat="server" Text="Show Entire Month"
                                OnClick="btn_showallmonth_dailyoverview_Click" CssClass="input-buttons dailyoverview-update-img" />
                        </div>
                        <div class="clear"></div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <table class="content-table" cellpadding="0" cellspacing="0">
                <tbody>
                    <tr>
                        <td class="td-sidebar">
                            <div id="sidebar-padding-overview" class="sidebar-padding">
                                <div class="sidebar-scroll-app">
                                    <span id="btn_pauseScrollPage" class="img-pause cursor-pointer margin-left-sml" title="Pause/Play AutoScroll" onclick="PausePageScroll(this);"></span>
                                    <div class="clear-space"></div>
                                    <asp:UpdatePanel ID="updatepnl_Apps_dailyoverview" runat="server">
                                        <ContentTemplate>
                                            <asp:HiddenField ID="hf_UpdateAll" runat="server" OnValueChanged="hf_UpdateAll_ValueChanged" />
                                            <asp:HiddenField ID="hf_AutoUpdateInterval_Main" runat="server" />
                                            <div class="clear">
                                            </div>
                                            <div id="Calendardiv_dailyoverview">
                                                <asp:Calendar ID="Calendar1_dailyoverview" runat="server" CellSpacing="1" Font-Size="9pt"
                                                    BorderWidth="0" ForeColor="Black" Height="215px" Width="240px" SelectionMode="Day"
                                                    UseAccessibleHeader="false" NextPrevFormat="ShortMonth" OnSelectionChanged="Calendar1_Changed"
                                                    OnVisibleMonthChanged="Calendar1_Month_Changed" CssClass="calendar-delivery-pickups">
                                                    <SelectedDayStyle BackColor="#595959" ForeColor="White" />
                                                    <DayStyle BackColor="#FFFFFF" Font-Bold="True" ForeColor="#2D2D2D" CssClass="day-picker-hover" />
                                                    <NextPrevStyle Font-Bold="True" Font-Size="8pt" ForeColor="White" />
                                                    <DayHeaderStyle Font-Bold="True" Font-Size="8pt" ForeColor="#333" Height="8pt" />
                                                    <TitleStyle BackColor="#555555" BorderStyle="None" Font-Bold="True" Font-Size="12pt"
                                                        ForeColor="White" Height="28px" VerticalAlign="Middle" CssClass="calendar-delivery-pickups-title boxshadow"
                                                        Wrap="False" Width="100%" />
                                                    <OtherMonthDayStyle BackColor="Transparent" Font-Bold="False" ForeColor="#777777"
                                                        CssClass="day-picker-alt-hover" />
                                                </asp:Calendar>
                                            </div>
                                            <div class="clear-space">
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <asp:UpdatePanel ID="updatepnl_dates_dailyoverview" runat="server" OnLoad="updb"
                                        UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <div class="clear-space">
                                            </div>
                                            <div class="clear-margin">
                                                <small><i>Based off of selected Day/Month/Year</i></small>
                                            </div>
                                            <asp:Panel ID="pnl_overviewholder_dailyoverview" runat="server">
                                            </asp:Panel>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                        </td>
                        <td class="td-content">
                            <div class="content-main">
                                <table width="100%" cellspacing="0" cellpadding="0">
                                    <tbody>
                                        <tr>
                                            <td class="td-content-inner">
                                                <div id="autoscroll-content-dailyoverview" class="content-overflow-app">
                                                    <asp:UpdatePanel ID="updatepnl_schDriver_dailyoverview" runat="server">
                                                        <ContentTemplate>
                                                            <a href="#" id="hyp_hidecontrols" class="float-right margin-top margin-bottom margin-right"
                                                                onclick="HideControls();return false;">Hide Controls</a>
                                                            <div class="float-left pad-left-big margin-top">
                                                                <asp:Label ID="lbl_current" runat="server" ForeColor="#555555" Font-Size="22px" Text=""></asp:Label>
                                                            </div>
                                                            <div id="min_Controls" class="float-right pad-right-big margin-top">
                                                                <asp:LinkButton ID="btn_refresh_dailyoverview" runat="server" CssClass="float-right dailyoverview-update-img img-refresh deliverypickup-update margin-top-sml margin-left"
                                                                    ToolTip="Refresh Table" OnClick="lbtn_multisort_Click" />
                                                                <span class="float-right pad-right">
                                                                    <asp:DropDownList ID="dd_display_dailyoverview" runat="server" Width="60px" AutoPostBack="True"
                                                                        OnSelectedIndexChanged="dd_display_dailyoverview_SelectedIndexChanged">
                                                                        <asp:ListItem Value="10">10</asp:ListItem>
                                                                        <asp:ListItem Value="20">20</asp:ListItem>
                                                                        <asp:ListItem Value="30">30</asp:ListItem>
                                                                        <asp:ListItem Value="40" Selected="True">40</asp:ListItem>
                                                                        <asp:ListItem Value="1">All</asp:ListItem>
                                                                    </asp:DropDownList>
                                                                </span><span class="float-right pad-right">
                                                                    <select id="font-size-selector">
                                                                        <option value="x-small">Font Size: x-Small</option>
                                                                        <option value="small">Font Size: Small</option>
                                                                        <option value="medium">Font Size: Medium</option>
                                                                        <option value="large">Font Size: Large</option>
                                                                        <option value="x-large">Font Size: x-Large</option>
                                                                    </select>
                                                                </span>
                                                                <div class="clear-space-five">
                                                                </div>
                                                            </div>
                                                            <asp:Panel ID="pnl_multisort_dailyoverview" runat="server" CssClass="pnl-multisort">
                                                            </asp:Panel>
                                                        </ContentTemplate>
                                                    </asp:UpdatePanel>
                                                    <asp:UpdatePanel ID="updatepnl_schDriver_gridview_dailyoverview" runat="server" UpdateMode="Conditional"
                                                        OnLoad="updb">
                                                        <ContentTemplate>
                                                            <div style="font-size: .97em!important">
                                                                <asp:GridView ID="GV_dbimport_dailyoverview" runat="server" CellPadding="0" CellSpacing="0"
                                                                    AutoGenerateColumns="True" Width="100%" GridLines="Both" ShowHeaderWhenEmpty="True"
                                                                    AllowPaging="True" AllowSorting="True" PageSize="40" BorderColor="#CCCCCC" OnPageIndexChanging="GV_dbimport_dailyoverview_PageIndexChanging"
                                                                    OnRowCreated="GV_dbimport_dailyoverview_RowCreated" OnSorting="GV_dbimport_dailyoverview_Sorting"
                                                                    OnRowDataBound="GV_dbimport_dailyoverview_OnRowDataBound">
                                                                    <PagerSettings Position="Bottom" />
                                                                    <PagerStyle CssClass="GridViewPager dailyoverview-update" />
                                                                    <EmptyDataRowStyle ForeColor="Black" />
                                                                    <RowStyle CssClass="GridNormalRow" />
                                                                    <AlternatingRowStyle CssClass="GridAlternate" />
                                                                    <EmptyDataTemplate>
                                                                        <div class="emptyGridView">
                                                                            No data available
                                                                        </div>
                                                                    </EmptyDataTemplate>
                                                                    <HeaderStyle CssClass="myHeaderStyle" />
                                                                </asp:GridView>
                                                            </div>
                                                        </ContentTemplate>
                                                    </asp:UpdatePanel>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </form>
        <script type="text/javascript" src="dailyoverview.js"></script>
    </div>
</body>
</html>
