<%@ page title="Network Log" async="true" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_NetworkLog, App_Web_eye1hras" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .averagerequests
        {
            padding: 40px 5px 25px 5px;
        }

            .averagerequests span
            {
                font-size: 17px;
                font-weight: normal;
                color: #555;
            }

        .ui-autocomplete
        {
            max-height: 300px;
            min-width: 385px;
            max-width: 50%;
        }

        .eventlog-datecol
        {
            font-weight: bold;
            text-align: right;
            padding-right: 10px;
            width: 115px;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <asp:Panel ID="pnlLinkBtns" runat="server">
        </asp:Panel>
        <div class="clear-space">
        </div>
        <div id="activity" class="pnl-section" style="display: none">
            If charts are not updating every given time
            interval, switch to another browser. (Works best in Google Chrome and Internet Explorer
            8+). Averages are calculated for 1 minute.
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="clear-space">
                    </div>
                    <div class="float-left pad-right-big">
                        <b class="pad-right">Update Interval</b>
                        <asp:DropDownList ID="dd_interval" CssClass="margin-left" runat="server" ClientIDMode="Static">
                            <asp:ListItem Text="1 Second" Value="1000"></asp:ListItem>
                            <asp:ListItem Text="2 Seconds" Value="2000"></asp:ListItem>
                            <asp:ListItem Text="5 Seconds" Value="5000" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="10 Seconds" Value="10000"></asp:ListItem>
                            <asp:ListItem Text="15 Seconds" Value="15000"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <a href="#" id="imgPausePlay" class="margin-left-sml margin-top-sml margin-right-sml float-left cursor-pointer img-pause"
                        title="Pause/Play"></a>
                    <div class="float-left pad-left-big">
                        <b class="pad-right">Time to Track</b>
                        <asp:DropDownList ID="dd_track" CssClass="margin-left" runat="server" ClientIDMode="Static">
                            <asp:ListItem Text="5 Second" Value="5"></asp:ListItem>
                            <asp:ListItem Text="10 Seconds" Value="10"></asp:ListItem>
                            <asp:ListItem Text="15 Seconds" Value="15"></asp:ListItem>
                            <asp:ListItem Text="20 Seconds" Value="20"></asp:ListItem>
                            <asp:ListItem Text="25 Seconds" Value="25"></asp:ListItem>
                            <asp:ListItem Text="30 Seconds" Value="30" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="1 Minute" Value="60"></asp:ListItem>
                            <asp:ListItem Text="2 Minute" Value="120"></asp:ListItem>
                            <asp:ListItem Text="3 Minute" Value="180"></asp:ListItem>
                            <asp:ListItem Text="4 Minute" Value="240"></asp:ListItem>
                            <asp:ListItem Text="5 Minute" Value="300"></asp:ListItem>
                            <asp:ListItem Text="1 hour" Value="3600"></asp:ListItem>
                            <asp:ListItem Text="12 hour" Value="7200"></asp:ListItem>
                            <asp:ListItem Text="1 Day" Value="86400"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="clear-space">
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <table width="100%" cellpadding="10" cellspacing="10" border="0">
                <tr>
                    <td valign="top" style="width: 250px;">
                        <div id="network-info-div" class="pad-right-big">
                            <h3 align="center" class="font-bold">Network Information</h3>
                            <div class="clear-space">
                            </div>
                            <div id="pnl_NetworkInfoHolder">
                            </div>
                        </div>
                    </td>
                    <td valign="top">
                        <asp:Panel ID="pnl_interactivityholder" runat="server">
                            <table width="100%">
                                <tbody>
                                    <tr>
                                        <td valign="top">
                                            <div id="ChartRequests">
                                                <h3>Loading Google Chart. Please Wait...</h3>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td valign="top">
                                            <div id="statholder">
                                            </div>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </asp:Panel>
                    </td>
                </tr>
            </table>
        </div>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div id="search_box">
                    <div id="searchwrapper" style="min-width: 385px; width: 50%">
                        <asp:Panel ID="Panel1_applog" runat="server" DefaultButton="imgbtn_search">
                            <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                                onfocus="if(this.value=='Search Events')this.value=''" onblur="if(this.value=='')this.value='Search Events'"
                                Text="Search Events"></asp:TextBox>
                            <a href="#" onclick="$('#MainContent_tb_search').val('Search Events');return false;"
                                class="searchbox_clear"></a>
                            <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                OnClick="imgbtn_search_Click" />
                        </asp:Panel>
                    </div>
                    <span class="float-right margin-left-big">Leave search textbox blank to search
                    all events</span>
                </div>
                <asp:HiddenField ID="hf_searchreset" runat="server" ClientIDMode="Static" OnValueChanged="hf_searchreset_Changed" />
                <div class="clear-space">
                </div>
                <asp:HiddenField ID="hf_updateIgnore" runat="server" OnValueChanged="hf_updateIgnore_Changed" />
                <asp:HiddenField ID="hf_updateRefreshOnError" runat="server" OnValueChanged="hf_updateRefreshOnError_Changed" />
                <asp:HiddenField ID="hf_updateAllow" runat="server" OnValueChanged="hf_updateAllow_Changed" />
                <asp:HiddenField ID="hf_deleteError" runat="server" OnValueChanged="hf_deleteError_Changed" />
                <asp:HiddenField ID="hf_resetHitCount" runat="server" OnValueChanged="hf_resetHitCount_Changed" />
                <asp:HiddenField ID="hf_DeleteLoginEvent" runat="server" OnValueChanged="hf_DeleteLoginEvent_Changed" />
                <asp:HiddenField ID="hf_AllowBlockLoginEvent" runat="server" OnValueChanged="hf_AllowBlockLoginEvent_Changed" />
                <div id="log" class="pnl-section" style="display: none;">
                    <div class="float-left">
                        <asp:Panel ID="pnl_rtk_E" runat="server" DefaultButton="btn_Update_rtk_E" CssClass="float-left pad-right">
                            <b class="pad-right">Events To Keep</b>
                            <asp:TextBox ID="tb_Records_to_keep_E" runat="server" CssClass="textEntry margin-right"
                                Width="50px"></asp:TextBox><asp:Button ID="btn_Update_rtk_E" runat="server" CssClass="input-buttons RandomActionBtns"
                                    Text="Update" OnClick="btn_Update_rtk_E_Clicked" />
                        </asp:Panel>
                        <div class="float-left pad-top">
                            <div class="float-left pad-right pad-left-big">
                                <span class="img-allow float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Allowed</b>
                            </div>
                            <div class="float-left pad-left">
                                <span class="img-ignore float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Ignored</b>
                            </div>
                        </div>
                        <div class="float-left pad-left-big margin-left-big pad-top">
                            <asp:CheckBox ID="cb_ViewErrorsOnly" ClientIDMode="Static" runat="server" Text="&nbsp;View Errors Only" AutoPostBack="true" OnCheckedChanged="cb_ViewErrorsOnly_CheckedChanged" />
                        </div>
                    </div>
                    <div class="float-right">
                        <asp:LinkButton ID="lbtn_refresherrors" runat="server" OnClick="lbtn_refresherrors_Click" ToolTip="Refresh" CssClass="RandomActionBtns float-right margin-left img-refresh"></asp:LinkButton>
                        <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('SiteTools/iframes/LogFolder.aspx', this);return false;" class="margin-right-big">View Logging Folder</a>
                        <asp:LinkButton ID="LinkButton2" runat="server" CssClass="margin-right-big" OnClientClick="return ConfirmClearLogFolder(this);">Delete All Log Files</asp:LinkButton>
                        <asp:LinkButton ID="LinkButton1" runat="server" OnClientClick="return ConfirmClearLogAll(this);">Clear All Events</asp:LinkButton>
                    </div>
                    <div class="clear-space">
                    </div>
                    <asp:GridView ID="GV_Requests" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                        Width="100%" GridLines="None" ShowHeaderWhenEmpty="True">
                        <EmptyDataRowStyle ForeColor="Black" />
                        <RowStyle CssClass="GridNormalRow" />
                        <EmptyDataTemplate>
                            <div class="emptyGridView">
                                No Events Available
                            </div>
                        </EmptyDataTemplate>
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                        <tr>
                                            <td style="width: 40px;"></td>
                                            <td>Event Information
                                            </td>
                                            <td style="width: 75px; text-align: center;">Actions
                                            </td>
                                        </tr>
                                    </table>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                        <tbody>
                                            <td class="GridViewNumRow" valign="middle" style="width: 40px; text-align: center;">
                                                <%#Container.DataItemIndex + 1 %>
                                            </td>
                                            <td class="border-right" valign="top">
                                                <table>
                                                    <tr>
                                                        <td class="eventlog-datecol">Date</td>
                                                        <td><%#Eval("date") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Event Name</td>
                                                        <td><%#Eval("name") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Event Message</td>
                                                        <td><%#Eval("comment") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Stack Trace</td>
                                                        <td><%#Eval("stacktrace") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Exception Type</td>
                                                        <td><%#Eval("exceptiontype") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Machine Name</td>
                                                        <td><%#Eval("machinename") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Request Url</td>
                                                        <td><%#Eval("requesturl") %></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="eventlog-datecol">Username</td>
                                                        <td><%#Eval("username") %></td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td class='border-right' style="width: 75px; text-align: center;">
                                                <%#Eval("ignore")%>
                                                <%#Eval("refresh")%>
                                                <%#Eval("delete")%>
                                            </td>
                                        </tbody>
                                    </table>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div id="ignore" class="pnl-section" style="display: none">
                    <div class="float-left">
                        Sorted from newest to oldest Ignore certain requests from being recorded if it contains or equals a certain event momment.
                    </div>
                    <div class="float-right">
                        <asp:LinkButton ID="lbtn_refreshignore" runat="server" OnClick="lbtn_refreshignore_Click" ToolTip="Refresh" CssClass="RandomActionBtns float-right margin-left img-refresh"></asp:LinkButton>
                        <asp:LinkButton ID="lbtnClearAllIgnored" runat="server" OnClientClick="return ConfirmClearAllIgnored(this);">Clear All Ignored</asp:LinkButton>
                    </div>
                    <div class="clear-space">
                    </div>
                    <asp:GridView ID="gv_Ignore" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                        Width="100%" GridLines="None" ShowHeaderWhenEmpty="True">
                        <EmptyDataRowStyle ForeColor="Black" />
                        <RowStyle CssClass="GridNormalRow" />
                        <EmptyDataTemplate>
                            <div class="emptyGridView">
                                No Ignored Events Available
                            </div>
                        </EmptyDataTemplate>
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                        <tr>
                                            <td style="width: 41px;"></td>
                                            <td style="width: 135px;">Date
                                            </td>
                                            <td style="width: 65px;">Hits
                                            </td>
                                            <td>Event Message
                                            </td>
                                            <td style="width: 100px; text-align: center;">Actions
                                            </td>
                                        </tr>
                                    </table>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                        <tbody>
                                            <td class="GridViewNumRow" style="width: 40px; text-align: center;">
                                                <%#Container.DataItemIndex + 1 %>
                                            </td>
                                            <td class="border-right" style="width: 136px;">
                                                <%#Eval("date") %>
                                            </td>
                                            <td class="border-right" style="width: 65px; text-align: center;">
                                                <%#Eval("timesHit") %>
                                            </td>
                                            <td class="border-right" valign="middle">
                                                <%#Eval("comment") %>
                                            </td>
                                            <td class='border-right' style="width: 101px; text-align: center;">
                                                <%#Eval("refresh") %>
                                                <%#Eval("ignore") %>
                                            </td>
                                        </tbody>
                                    </table>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div id="loginActivity" class="pnl-section" style="display: none;">
                    <div class="float-left">
                        This list is partially populated from the IP Watchlist. Blocking or allowing an IP Address on this activity log will update the IP Watchlist. Only records within the past 5 days will be displayed. Records past 5 days will be deleted. Sorted from newest to oldest.
                    </div>
                    <div class="clear-space">
                    </div>
                    <div class="float-left pad-right">
                        <span class="img-allow float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Allowed</b>
                    </div>
                    <div class="float-left pad-left">
                        <span class="img-ignore float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Blocked</b>
                    </div>
                    <a id="clearloginsearch" href="#" class="float-left pad-left-big margin-left-big" onclick="ClearIpSearch();return false;" style="display: none;">Clear Search
                    </a>
                    <div class="float-left pad-left-big margin-left-big">
                        <span id="loginactError"></span>
                    </div>
                    <div class="float-right">
                        <asp:LinkButton ID="lbtn_refreshLoginactivity" runat="server" OnClick="lbtn_refreshLoginactivity_Click" ToolTip="Refresh" CssClass="RandomActionBtns float-right margin-left img-refresh"></asp:LinkButton>
                        <asp:LinkButton ID="lbtnClearAllLogin" runat="server" OnClientClick="return ConfirmClearAllLoginActivity(this);">Clear All Login Activity</asp:LinkButton>
                    </div>
                    <div class="clear-space">
                    </div>
                    <div class="clear-space">
                    </div>
                    <asp:GridView ID="gv_LoginActivity" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                        Width="100%" GridLines="None" ShowHeaderWhenEmpty="True">
                        <EmptyDataRowStyle ForeColor="Black" />
                        <RowStyle CssClass="GridNormalRow" />
                        <EmptyDataTemplate>
                            <div class="emptyGridView">
                                No Login Activity Available
                            </div>
                        </EmptyDataTemplate>
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                        <tr>
                                            <td style="width: 41px;"></td>
                                            <td style="width: 100px;">IP Address
                                            </td>
                                            <td style="width: 205px;">Name Used
                                            </td>
                                            <td>Event Message
                                            </td>
                                            <td style="width: 65px;">Type
                                            </td>
                                            <td style="width: 50px;">Valid
                                            </td>
                                            <td style="width: 165px;">Date
                                            </td>
                                            <td style="width: 75px; text-align: center;">Actions
                                            </td>
                                        </tr>
                                    </table>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <table class="myItemStyle ipsearchrow" cellpadding="5" cellspacing="0">
                                        <tbody>
                                            <td class="GridViewNumRow" style="width: 40px; text-align: center;">
                                                <%#Container.DataItemIndex + 1 %>
                                            </td>
                                            <td class="border-right" style="width: 100px; text-align: center;">
                                                <span class="ipsearchval" style="display: none;"><%#Eval("IpAddressNoLink") %></span>
                                                <%#Eval("IpAddress") %>
                                            </td>
                                            <td class="border-right" style="width: 205px; text-align: center;">
                                                <%#Eval("UserName") %>
                                            </td>
                                            <td class="border-right" valign="middle">
                                                <%#Eval("Message") %>
                                            </td>
                                            <td class="border-right" style="width: 65px; text-align: center;">
                                                <%#Eval("ActType") %>
                                            </td>
                                            <td class="border-right" style="width: 50px; text-align: center;">
                                                <%#Eval("Success") %>
                                            </td>
                                            <td class="border-right" style="width: 165px; text-align: center;">
                                                <%#Eval("Date") %>
                                            </td>
                                            <td class='border-right' style="width: 75px; text-align: center;">
                                                <%#Eval("AllowBlock") %>
                                                <%#Eval("Delete") %>
                                            </td>
                                        </tbody>
                                    </table>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <div id="loginactivitysearch" class="emptyGridView" style="display: none;">
                        No Login Activity Found
                    </div>
                </div>
                <div id="ipwatchlist" class="pnl-section" style="display: none;">
                    <asp:Literal ID="ltl_lockedipwatchlist" runat="server"></asp:Literal>
                    Blocking an IP Address will redirect every request to a default blocked page for
                    that IP. That (blocked) IP will not be able to access the site until the IP is released from the watch list.
                    <div class="clear-space">
                    </div>
                    <asp:Panel ID="pnl_ipAutoBlock" runat="server">
                        <div class="table-settings-box">
                            <div class="td-settings-title">
                                Auto Block IP
                            </div>
                            <div class="title-line"></div>
                            <div class="td-settings-ctrl">
                                <div class="field switch inline-block">
                                    <asp:RadioButton ID="rb_AutoIPBlock_on" runat="server" Text="True" CssClass="RandomActionBtns cb-enable"
                                        OnCheckedChanged="rb_AutoIPBlock_on_CheckedChanged" AutoPostBack="True" />
                                    <asp:RadioButton ID="rb_AutoIPBlock_off" runat="server" Text="False" CssClass="RandomActionBtns cb-disable"
                                        OnCheckedChanged="rb_AutoIPBlock_off_CheckedChanged" AutoPostBack="True" />
                                </div>
                            </div>
                            <div class="td-settings-desc">
                                Set to True if you want the site to automatically block any IP after a certain amount of attempts.
                            </div>
                        </div>
                        <asp:Panel ID="pnl_attemptsBeforeBlock" runat="server">
                            <div class="table-settings-box">
                                <div class="td-settings-title">
                                    Attempts Before Blocked
                                </div>
                                <div class="title-line"></div>
                                <div class="td-settings-ctrl">
                                    <asp:Panel ID="pnl_autoblock_count" runat="server" DefaultButton="btn_autoblock_count">
                                        <asp:TextBox ID="tb_autoblock_count" runat="server" CssClass="textEntry margin-right-big"
                                            Width="50px"></asp:TextBox><asp:Button ID="btn_autoblock_count" runat="server" CssClass="input-buttons RandomActionBtns"
                                                Text="Update" OnClick="btn_autoblock_count_Clicked" />
                                    </asp:Panel>
                                </div>
                                <div class="td-settings-desc">
                                    Set the number of attempts before IP is blocked. (Must be greater than 0)
                                </div>
                            </div>
                        </asp:Panel>
                    </asp:Panel>
                    <div class="clear-space"></div>
                    <div class="float-left pad-right">
                        <span class="img-allow float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Allowed</b>
                    </div>
                    <div class="float-left pad-left">
                        <span class="img-ignore float-left" style="margin-top: -1px; margin-right: 2px;"></span><b>Blocked</b>
                    </div>
                    <asp:LinkButton ID="lbtn_refreshIPWatchList" runat="server" OnClick="lbtn_refreshIPWatchList_Click" ToolTip="Refresh" CssClass="RandomActionBtns float-left margin-left img-refresh" Style="margin-left: 408px;"></asp:LinkButton>
                    <div class="clear-space">
                    </div>
                    <div>
                        <asp:UpdatePanel ID="updatepnl_list" runat="server">
                            <ContentTemplate>
                                <asp:GridView ID="GV_WatchList" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                                    Width="575px" GridLines="None" OnRowCommand="GV_WatchList_RowCommand" AllowPaging="false"
                                    ShowHeaderWhenEmpty="True" OnRowDeleting="GV_WatchList_RowDeleting" OnRowDataBound="GV_WatchList_RowDataBound">
                                    <AlternatingRowStyle CssClass="GridAlternate" />
                                    <EmptyDataRowStyle ForeColor="Black" />
                                    <RowStyle CssClass="GridNormalRow" />
                                    <EmptyDataTemplate>
                                        <div class="emptyGridView">
                                            There are no IP Addresses being watched
                                        </div>
                                    </EmptyDataTemplate>
                                    <Columns>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                                    <tr>
                                                        <td style="width: 41px;"></td>
                                                        <td>IP Address
                                                        </td>
                                                        <td width="60px">Attempts
                                                        </td>
                                                        <td width="150px">Last Attempt
                                                        </td>
                                                        <td width="75px">Actions
                                                        </td>
                                                    </tr>
                                                </table>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                                    <tr>
                                                        <td class="GridViewNumRow" style="width: 40px; text-align: center;">
                                                            <%#Container.DataItemIndex + 1 %>
                                                        </td>
                                                        <td class="border-right">
                                                            <div class="pad-left">
                                                                <font style="font-size: 14px; font-weight: bold;">
                                                                <%#Eval("ip")%></font><small>-
                                                                    <%#Eval("blockMessage") %></small>
                                                            </div>
                                                        </td>
                                                        <td width="60px" align="center" class="border-right">
                                                            <%#Eval("attempts")%>
                                                        </td>
                                                        <td width="150px" align="center" class="border-right">
                                                            <%#Eval("LastAttempt")%>
                                                        </td>
                                                        <td width="75px" align="center" class="border-right">
                                                            <asp:LinkButton ID="lb_block" runat="server" CommandName="block" CommandArgument='<%#Eval("ip") %>'
                                                                CssClass='<%#Eval("blocked") %>' ToolTip="Block/Allow IP"></asp:LinkButton>
                                                            <asp:LinkButton ID="lb_delete" runat="server" CommandName="delete" CommandArgument='<%#Eval("ip") %>'
                                                                ToolTip="Delete from IP List" CssClass="td-delete-btn RandomActionBtns"></asp:LinkButton>
                                                            <asp:Label ID="lb_blockActions_na" runat="server" Text="-" Enabled="false" Visible="false"></asp:Label>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                                <div class="clear" style="height: 35px">
                                </div>
                                <asp:Panel ID="pnl_addip" runat="server" DefaultButton="btn_createnew_listener">
                                    <table cellpadding="10" cellspacing="10">
                                        <tbody>
                                            <tr>
                                                <td align="right" style="width: 185px;">
                                                    <span class="pad-right font-bold">Add IP Address</span>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="tb_createnew_listener"
                                                        runat="server" CssClass="textEntry margin-right-big" Width="150px"></asp:TextBox>
                                                    <asp:Button ID="btn_createnew_listener" runat="server" Text="Add IP Address" CssClass="input-buttons margin-right-big RandomActionBtns"
                                                        OnClick="btn_createnew_listener_Click" />
                                                    <div class="clear-space-five"></div>
                                                    <small>Enter a new ip address for the site to block.</small>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                    <div class="clear-space-two">
                                    </div>
                                    <div id="ipwatch_postmessage" runat="server" style="margin-left: 107px;">
                                    </div>
                                    <div class="clear" style="height: 20px;">
                                    </div>
                                </asp:Panel>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
                <div id="iplistener" class="pnl-section" style="display: none;">
                    <asp:Literal ID="ltl_lockediplistener" runat="server"></asp:Literal>
                    The IP Listener can be used to set up an <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('http://en.wikipedia.org/wiki/Intranet', this);return false;">internal</a> only version of the site and/or only allowing certain ip address to
                    access the site. (This works best for companies with a static IP Address)
                    <div class="clear">
                    </div>
                    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                        <ContentTemplate>
                            <div class="clear"></div>
                            If any IP address is active to listen for,
                                    all other IP addresses will be blocked (Only the active ip address(s) will be available
                                    to access). You may have multiple IP addresses active. Turn off or delete all IP
                                    address to allow for everyone to access the site.
                                <div class="clear-space"></div>
                            <asp:Panel ID="pnl_iplistener_holder" runat="server">
                            </asp:Panel>
                            <div id="listener_postmessage" style="margin-left: 205px; margin-top: -10px;">
                            </div>
                            <asp:HiddenField ID="hf_UpdateIPListener" ClientIDMode="Static" runat="server" OnValueChanged="hf_UpdateIPListener_ValueChanged" />
                            <div class="clear" style="height: 20px;">
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/networklog.js")%>'></script>
    </div>
</asp:Content>
