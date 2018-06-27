<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DeliveryPickups.aspx.cs"
    Inherits="Apps_iFrames_DeliveryPickups" ClientIDMode="Static" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Delivery and Pickups</title>
    <link type="text/css" rel="Stylesheet" href="deliverypickups.css" />
</head>
<body style="background: #F5F5F5 !important;">
    <form id="form1_deliverypickups" runat="server">
        <asp:ScriptManager ID="ScriptManager_deliverypickups" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <div id="deliverypickup-load" class="main-div-app-bg">
            <div class="stylefour">
                <div class="float-left">
                    <h3 class="font-bold pad-right-big">
                        <asp:Label ID="lbl_title" runat="server" Text=""></asp:Label></h3>
                </div>
                <div class="float-right">
                    <asp:UpdatePanel ID="updatepnl_schedulemenu_deliverypickups" runat="server">
                        <ContentTemplate>
                            <ul id="ScheduleMenu">
                                <li class='<%=class1 %>'><a href="#Delivery" class="catButtons">
                                    <div class="sch_ColorCode rounded-corners-10" style="color: transparent; background-color: #7FB6FF; margin-top: 11px;">
                                    </div>
                                    Delivery</a></li>
                                <li class='<%=class2 %>'><a href="#Pickup" class="catButtons">
                                    <div class="sch_ColorCode rounded-corners-10" style="color: transparent; background-color: #FFD256; margin-top: 11px;">
                                    </div>
                                    Pickup</a></li>
                                <li class='<%=class3 %>'><a href="#Completed" class="catButtons">Complete</a></li>
                                <li class='<%=class4 %>'><a href="#Opened" class="catButtons">Open</a></li>
                                <li class='<%=class5 %>'><a href="#All" class="catButtons">View All</a></li>
                            </ul>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="cb_viewslots_deliverypickups" EventName="CheckedChanged" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
            <table class="content-table" cellpadding="0" cellspacing="0">
                <tbody>
                    <tr>
                        <td class="td-sidebar">
                            <div id="sidebar-padding-deliverypickups" class="sidebar-padding">
                                <div class="sidebar-scroll-app">
                                    <asp:Panel ID="pnl_schedulerNextEvt_deliverypickups" runat="server">
                                        <div class="statusTicker">
                                            <div class="clear-margin">
                                                <div class="clear-space-two">
                                                </div>
                                                <h3><u>Next Delivery/Pickup</u></h3>
                                                <div class="clear-space-two"></div>
                                                <asp:UpdatePanel ID="updatepnl_pnl_nextApp_deliverypickups" runat="server">
                                                    <ContentTemplate>
                                                        <asp:HiddenField ID="hf_nextApp_deliverypickups" runat="server" />
                                                        <div class="clear-space-five">
                                                        </div>
                                                        <asp:Panel ID="pnl_nextApp_deliverypickups" runat="server">
                                                        </asp:Panel>
                                                    </ContentTemplate>
                                                </asp:UpdatePanel>
                                            </div>
                                        </div>
                                    </asp:Panel>
                                    <div class="sidebar-divider">
                                    </div>
                                    <asp:Panel ID="pnl_scheduler_deliverypickups" runat="server">
                                        <div class="clear-margin">
                                            <asp:UpdatePanel ID="updatepnl_Apps_deliverypickups" runat="server">
                                                <ContentTemplate>
                                                    <asp:HiddenField ID="hf_refreshTimer_deliverypickups" runat="server" OnValueChanged="hf_refreshTimer_ValueChanged" />
                                                    <asp:HiddenField ID="hf_category_deliverypickups" runat="server" OnValueChanged="hf_category_ValueChanged" />
                                                    <asp:HiddenField ID="hf_dateselected_deliverypickups" runat="server" Value="" OnValueChanged="hf_dateselected_ValueChanged" />
                                                    <asp:HiddenField ID="hf_viewTimes_deliverypickups" runat="server" Value="All" />
                                                    <div class="clear-space">
                                                    </div>
                                                    <div id="ScheduleCal">
                                                        <asp:Calendar ID="Calendar1_deliverypickups" runat="server" CellSpacing="1" Font-Size="9pt"
                                                            BorderWidth="0" ForeColor="Black" Height="215px" Width="240px" SelectionMode="Day"
                                                            UseAccessibleHeader="false" NextPrevFormat="ShortMonth" OnSelectionChanged="Calendar1_Changed"
                                                            CssClass="calendar-delivery-pickups">
                                                            <SelectedDayStyle BackColor="#595959" ForeColor="White" />
                                                            <DayStyle BackColor="#FFFFFF" Font-Bold="True" ForeColor="#2D2D2D" CssClass="day-picker-hover deliverypickup-update" />
                                                            <NextPrevStyle Font-Bold="True" Font-Size="8pt" ForeColor="White" />
                                                            <DayHeaderStyle Font-Bold="True" Font-Size="8pt" ForeColor="#333" Height="8pt" CssClass="deliverypickup-update" />
                                                            <TitleStyle BackColor="#555555" BorderStyle="None" Font-Bold="True" Font-Size="12pt"
                                                                ForeColor="White" Height="28px" VerticalAlign="Middle" CssClass="calendar-delivery-pickups-title boxshadow deliverypickup-update"
                                                                Wrap="False" Width="100%" />
                                                            <OtherMonthDayStyle BackColor="Transparent" Font-Bold="False" ForeColor="#777777"
                                                                CssClass="day-picker-alt-hover" />
                                                        </asp:Calendar>
                                                    </div>
                                                    <div class="sidebar-divider">
                                                    </div>
                                                    <div class="clear-space">
                                                    </div>
                                                    <span class="pad-right"><b style="color: #707070">Total Deliveries:</b></span><asp:Label
                                                        ID="lbl_ttlAppsDel_deliverypickups" runat="server" Text=""></asp:Label><br />
                                                    <span class="pad-right"><b style="color: #707070">Total Pickups:</b></span><asp:Label
                                                        ID="lbl_ttlAppsPU_deliverypickups" runat="server" Text=""></asp:Label><br />
                                                    <span class="pad-right"><b style="color: #707070">Marked Complete:</b></span><asp:Label
                                                        ID="lbl_ttlComplete_deliverypickups" runat="server" Text=""></asp:Label><br />
                                                    <small><i>(For selected date & category)</i></small>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </div>
                        </td>
                        <td class="td-content">
                            <div class="content-main">
                                <table width="100%" cellspacing="0" cellpadding="0">
                                    <tbody>
                                        <tr>
                                            <td class="td-content-inner">
                                                <div class="content-overflow-app">
                                                    <input id="hf_dateMonth" type="hidden" value="" />
                                                    <input id="hf_dateYear" type="hidden" value="" />
                                                    <table width="100%" cellspacing="0" cellpadding="0">
                                                        <tr>
                                                            <td>
                                                                <div class="float-right pad-top-big pad-left-big pad-right-sml">
                                                                    <asp:CheckBox ID="cb_viewslots_deliverypickups" runat="server" CssClass="margin-right deliverypickup-update-img"
                                                                        Text="&nbsp;Filled Slots Only" AutoPostBack="true" OnCheckedChanged="cb_viewslots_Changed"
                                                                        ToolTip="View only scheduled time slots" />
                                                                </div>
                                                                <div class="float-right pad-right pad-top">
                                                                    <select id="font-size-selector">
                                                                        <option value="x-small">Font Size: x-Small</option>
                                                                        <option value="small">Font Size: Small</option>
                                                                        <option value="medium">Font Size: Medium</option>
                                                                        <option value="large">Font Size: Large</option>
                                                                        <option value="x-large">Font Size: x-Large</option>
                                                                    </select>
                                                                </div>
                                                                <div class="float-left margin-left pad-top-big">
                                                                    <a class="ajaxCall_Modal float-left margin-right" href="#Contact_Modal_deliverypickups"
                                                                        title="Create new schedule"><span class="margin-right-sml float-left td-add-btn"
                                                                            style="padding: 0px!important;"></span>Create New</a>
                                                                </div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    <div class="clear-space">
                                                    </div>
                                                    <div class="clear-margin">
                                                        <div class="float-left margin-left" style="min-width: 450px;">
                                                            <div class="searchwrapper" style="width: 95%;">
                                                                <asp:Panel ID="Panel1_deliverypickups" runat="server" DefaultButton="imgbtn_search_deliverypickups">
                                                                    <asp:TextBox ID="tb_search_deliverypickups" runat="server" CssClass="serach-deliverypickups searchbox"
                                                                        Font-Size="Small" onfocus="if(this.value=='Search for Schedule')this.value=''"
                                                                        onblur="if(this.value=='')this.value='Search for Schedule'" Text="Search for Schedule"></asp:TextBox>
                                                                    <a href="#" class="searchbox_clear" onclick="$('#tb_search_deliverypickups').val('Search for Schedule');return false;"></a>
                                                                    <asp:LinkButton ID="imgbtn_search_deliverypickups" runat="server" ToolTip="Start search"
                                                                        CssClass="searchbox_submit deliverypickup-update-img" OnClick="imgbtn_search_Click" />
                                                                </asp:Panel>
                                                            </div>
                                                        </div>
                                                        <div class="float-right">
                                                            <asp:LinkButton ID="btn_refresh_deliverypickups" runat="server" CssClass="img-refresh float-right deliverypickup-update-img margin-right"
                                                                ToolTip="Refresh schedule" OnClick="btn_refresh_Click" />
                                                            <div class="pad-right-big float-right">
                                                                <asp:DropDownList ID="dd_display_deliverypickups" runat="server" ToolTip="Change display amount"
                                                                    Width="60px" AutoPostBack="True" OnSelectedIndexChanged="dd_display_SelectedIndexChanged">
                                                                    <asp:ListItem Value="10">10</asp:ListItem>
                                                                    <asp:ListItem Value="20">20</asp:ListItem>
                                                                    <asp:ListItem Value="30">30</asp:ListItem>
                                                                    <asp:ListItem Value="40">40</asp:ListItem>
                                                                    <asp:ListItem Value="1" Selected="True">All</asp:ListItem>
                                                                </asp:DropDownList>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="clear-space">
                                                    </div>
                                                    <asp:UpdatePanel ID="UpdatepnlMain_deliverypickups" runat="server">
                                                        <ContentTemplate>
                                                            <div id="containerScheduler">
                                                                <div class="GridDateHeader td-bg-color">
                                                                    <div class="GridDatePrev" title="Previous day">
                                                                        &larr;
                                                                    </div>
                                                                    <div class="GridDateNext" title="Next day">
                                                                        &rarr;
                                                                    </div>
                                                                    <asp:Label ID="lbl_currDate_deliverypickups" runat="server" Text="" Visible="false"></asp:Label>
                                                                    <div style="padding: 0.4em 0;">
                                                                        <asp:Label ID="lbl_currDate2_deliverypickups" runat="server" Text=""></asp:Label>
                                                                    </div>
                                                                </div>
                                                                <asp:GridView ID="GV_Schedule_deliverypickups" runat="server" CellPadding="0" CellSpacing="0"
                                                                    AutoGenerateColumns="False" Width="100%" GridLines="None" OnRowCommand="GV_Schedule_RowCommand"
                                                                    OnRowDataBound="GV_Schedule_RowDataBound" OnRowEditing="GV_Schedule_RowEdit"
                                                                    OnRowCancelingEdit="GV_Schedule_CancelEdit" OnRowUpdating="GV_Schedule_RowUpdate"
                                                                    AllowPaging="True" OnPageIndexChanging="GV_Schedule_PageIndexChanging" ShowHeaderWhenEmpty="True"
                                                                    OnRowCreated="GV_Schedule_RowCreated">
                                                                    <EmptyDataRowStyle ForeColor="Black" />
                                                                    <RowStyle CssClass="GridNormalRow" />
                                                                    <EmptyDataTemplate>
                                                                        <div class="emptyGridView">
                                                                            There are no scheduled deliveries or pickups today
                                                                        </div>
                                                                    </EmptyDataTemplate>
                                                                    <PagerSettings Position="Bottom" />
                                                                    <PagerTemplate>
                                                                        <div class="GridViewPager">
                                                                            <table width="100%">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td align="right" valign="middle" width="50">
                                                                                            <asp:LinkButton ID="btnFirst_deliverypickups" runat="server" CommandName="Page" CommandArgument="First"
                                                                                                CssClass="pg-first-btn RandomActionBtns" ToolTip="Goto First Page" />
                                                                                            <asp:LinkButton ID="btnPrevious_deliverypickups" runat="server" CommandName="Page"
                                                                                                CommandArgument="Prev" CssClass="pg-prev-btn RandomActionBtns" ToolTip="Previous Page" />
                                                                                        </td>
                                                                                        <td align="center" valign="middle" width="150">
                                                                                            <asp:Panel ID="Panel1_deliverypickups" runat="server">
                                                                                                Page:
                                                                                            <asp:TextBox ID="tb_pageManual_deliverypickups" runat="server" Width="22" OnTextChanged="tb_pageManual_TextChanged"
                                                                                                AutoPostBack="True"></asp:TextBox>
                                                                                                of
                                                                                            <asp:Label ID="pglbl2_deliverypickups" runat="server" Text=""></asp:Label>
                                                                                                <asp:Button ID="btn_hidden_deliverypickups" runat="server" Text="" Visible="False" />
                                                                                            </asp:Panel>
                                                                                        </td>
                                                                                        <td align="left" valign="middle" width="50">
                                                                                            <asp:LinkButton ID="btnNext_deliverypickups" runat="server" CommandName="Page" CommandArgument="Next"
                                                                                                CssClass="pg-next-btn RandomActionBtns" ToolTip="Next Page" />
                                                                                            <asp:LinkButton ID="btnLast_deliverypickups" runat="server" CommandName="Page" CommandArgument="Last"
                                                                                                CssClass="pg-last-btn RandomActionBtns" ToolTip="Goto Last Page" />
                                                                                        </td>
                                                                                        <td align="center" valign="top">
                                                                                            <asp:Panel ID="PnlPager_deliverypickups" runat="server">
                                                                                            </asp:Panel>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                    </PagerTemplate>
                                                                    <Columns>
                                                                        <asp:TemplateField>
                                                                            <HeaderTemplate>
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <table cellpadding="5" cellspacing="0" class="myItemStyle">
                                                                                    <tr>
                                                                                        <td width="75px" align="center" class='<%#Eval("MarkedComp") %>'>
                                                                                            <%#Eval("TimeSlot") %>
                                                                                        </td>
                                                                                        <td class='<%#Eval("Type") %>'>
                                                                                            <div class='<%#Eval("NewSlot") %>'>
                                                                                                <div class="float-left">
                                                                                                    <asp:LinkButton ID="lbnewapp" runat="server" CssClass="deliverypickup-update"
                                                                                                        CommandName="newApp" CommandArgument='<%#Eval("Scheduled") %>' Text="Create a new Appointment" />
                                                                                                </div>
                                                                                            </div>
                                                                                            <asp:Panel ID="schDiv" CssClass='<%#Eval("SlotFilled") %>' runat="server">
                                                                                                <dl class="myItemStyleDP float-left" style="width: 300px;">
                                                                                                    <dt>Truck Line</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("Company") %>
                                                                                                    </dd>
                                                                                                    <dt>Truck Number</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("TruckNum")%>
                                                                                                    </dd>
                                                                                                </dl>
                                                                                                <dl class="myItemStyleDP float-left" style="width: 300px;">
                                                                                                    <dt>Mill/Processor</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("DeliveryFrom")%></dd>
                                                                                                    <dt>Email Address</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("Email")%></dd>
                                                                                                </dl>
                                                                                                <dl class="myItemStyleDP float-left" style="width: 300px;">
                                                                                                    <dt>Confirmation</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("ConfirmationNum")%>
                                                                                                    </dd>
                                                                                                    <dt>Phone Number</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("PhoneNumber")%>
                                                                                                    </dd>
                                                                                                </dl>
                                                                                                <dl class="myItemStyleDP float-left" style="width: 300px;">
                                                                                                    <dt>Number of Items</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("Items")%></dd>
                                                                                                    <dt>Date Created</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("Created") %></dd>
                                                                                                </dl>
                                                                                                <div class="clear">
                                                                                                    <div class="float-left pad-top" style="width: 350px;">
                                                                                                        <span style="color: #707070;" class="pad-right-big">Comments</span><%#Eval("Comment") %>
                                                                                                    </div>
                                                                                                    <div class="float-right" style="margin-top: 2px;">
                                                                                                        <span id="imgcomplete" runat="server" title="Event Complete" class="margin-left-sml margin-right-sml img-approve"
                                                                                                            visible="false" />
                                                                                                        <asp:LinkButton ID="lbComplete" runat="server" CssClass="margin-right-sml td-update-btn deliverypickup-update"
                                                                                                            CommandName="markComplete" ToolTip="Complete Event" CommandArgument='<%#Eval("Scheduled")%>'
                                                                                                            OnClientClick="var c = confirm(&apos;Are you sure you want to mark this event as complete? You will not be able to edit this after complete.&apos;);if(c == false){setTimeout(function(){loadingPopup.RemoveMessage();},500);return false;}else{return true;}"></asp:LinkButton>
                                                                                                        <asp:LinkButton ID="lbedit" runat="server" CssClass="margin-right-sml td-edit-btn deliverypickup-update"
                                                                                                            CommandName="edit" CommandArgument='<%#Eval("Scheduled")%>' ToolTip="Edit"></asp:LinkButton>
                                                                                                        <asp:LinkButton ID="lbdelete" runat="server" CssClass="margin-right-sml td-delete-btn deliverypickup-update"
                                                                                                            CommandName="deleteSlot" CommandArgument='<%#Eval("Scheduled")%>' ToolTip="Delete"
                                                                                                            OnClientClick="var c = confirm(&apos;Are you sure you want to delete this event?&apos;);if(c == false){setTimeout(function(){loadingPopup.RemoveMessage();},500); return false;}else{return true;}"></asp:LinkButton>
                                                                                                    </div>
                                                                                                </div>
                                                                                            </asp:Panel>
                                                                                        </td>
                                                                                    </tr>
                                                                                </table>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:HiddenField ID="hfEdit_Date" runat="server" Value='<%#Eval("Scheduled") %>' />
                                                                                <asp:HiddenField ID="hfEdit_Type" runat="server" Value='<%#Eval("SchType") %>' />
                                                                                <asp:Panel ID="Panel9" runat="server">
                                                                                    <table cellpadding="5" cellspacing="0" class="myItemStyle">
                                                                                        <tr>
                                                                                            <td width="75px" align="center" class="GridViewNumRow">
                                                                                                <%#Eval("TimeSlot") %>
                                                                                            </td>
                                                                                            <td>
                                                                                                <dl class="myItemStyleDP float-left" style="width: 350px;">
                                                                                                    <dt>Truck Line</dt>
                                                                                                    <dd>
                                                                                                        <asp:TextBox runat="server" ID="tb_company" CssClass="textEntry-noWidth" Text='<%#Eval("Company") %>'
                                                                                                            AutoCompleteType="Search" AutoPostBack="True"></asp:TextBox>
                                                                                                    </dd>
                                                                                                    <dt>Truck Number</dt>
                                                                                                    <dd>
                                                                                                        <asp:TextBox runat="server" ID="tb_trucknum" CssClass="textEntry-noWidth" Text='<%#Eval("TruckNum")%>'
                                                                                                            AutoCompleteType="Search" AutoPostBack="True"></asp:TextBox>
                                                                                                    </dd>
                                                                                                </dl>
                                                                                                <dl class="myItemStyleDP float-left" style="width: 350px;">
                                                                                                    <dt>Mill/Processor</dt>
                                                                                                    <dd>
                                                                                                        <asp:TextBox runat="server" ID="tb_from" CssClass="textEntry-noWidth" Text='<%#Eval("DeliveryFrom") %>'
                                                                                                            AutoCompleteType="Search" AutoPostBack="True"></asp:TextBox></dd>
                                                                                                    <dt>Email Address</dt>
                                                                                                    <dd>
                                                                                                        <asp:TextBox runat="server" ID="tb_email" CssClass="textEntry-noWidth" Text='<%#Eval("Email") %>'
                                                                                                            AutoCompleteType="Search" AutoPostBack="True"></asp:TextBox></dd>
                                                                                                </dl>
                                                                                                <dl class="myItemStyleDP float-left" style="width: 300px;">
                                                                                                    <dt>Confirmation</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("ConfirmationNum")%>
                                                                                                    </dd>
                                                                                                    <dt>Phone Number</dt>
                                                                                                    <dd>
                                                                                                        <asp:TextBox runat="server" ID="tb_phonenumber" CssClass="textEntry-noWidth" Text='<%#Eval("PhoneNumber") %>'
                                                                                                            Width="100px" AutoCompleteType="Search" AutoPostBack="True"></asp:TextBox>
                                                                                                    </dd>
                                                                                                </dl>
                                                                                                <dl class="myItemStyleDP float-left" style="width: 300px;">
                                                                                                    <dt>Number of Items</dt>
                                                                                                    <dd>
                                                                                                        <asp:TextBox runat="server" ID="tb_items" CssClass="textEntry-noWidth" Text='<%#Eval("Items") %>'
                                                                                                            Width="20px" AutoCompleteType="Search" AutoPostBack="True"></asp:TextBox></dd>
                                                                                                    <dt>Date Created</dt>
                                                                                                    <dd>
                                                                                                        <%#Eval("Created") %></dd>
                                                                                                </dl>
                                                                                                <div class="clear">
                                                                                                    <div class="float-left" style="width: 350px;">
                                                                                                        <span style="color: #707070; font-weight: bold;" class="PadRightBig">Comments</span><asp:TextBox
                                                                                                            runat="server" ID="tb_comment" CssClass="textEntry-noWidth" Text='<%#Eval("Comment") %>'
                                                                                                            Width="200px" AutoCompleteType="Search" AutoPostBack="True"></asp:TextBox>
                                                                                                    </div>
                                                                                                    <div class="float-right" style="margin-top: 2px;">
                                                                                                        <asp:LinkButton ID="LinkButton1" CssClass="update margin-right-sml td-update-btn deliverypickup-update"
                                                                                                            runat="server" CausesValidation="True" CommandName="Update" ToolTip="Update"></asp:LinkButton>
                                                                                                        <asp:LinkButton ID="LinkButton2" CssClass="cancel margin-right-sml td-cancel-btn deliverypickup-update"
                                                                                                            runat="server" CausesValidation="False" CommandName="Cancel" ToolTip="Cancel"></asp:LinkButton>
                                                                                                    </div>
                                                                                                    <asp:DropDownList ID="dd_scheduleType" runat="server" AutoPostBack="true">
                                                                                                        <asp:ListItem Value="Delivery" Text="Delivery"></asp:ListItem>
                                                                                                        <asp:ListItem Value="Pickup" Text="Pickup"></asp:ListItem>
                                                                                                    </asp:DropDownList>
                                                                                                </div>
                                                                                            </td>
                                                                                        </tr>
                                                                                    </table>
                                                                                </asp:Panel>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </div>
                                                        </ContentTemplate>
                                                        <Triggers>
                                                            <asp:AsyncPostBackTrigger ControlID="dd_display_deliverypickups" />
                                                            <asp:AsyncPostBackTrigger ControlID="imgbtn_search_deliverypickups" />
                                                            <asp:AsyncPostBackTrigger ControlID="btn_refresh_deliverypickups" />
                                                        </Triggers>
                                                    </asp:UpdatePanel>
                                                    <div id="NewDelPickup_element" class="Modal-element">
                                                        <div class="Modal-overlay">
                                                            <div class="Modal-element-align">
                                                                <div class="Modal-element-modal" data-setwidth="625">
                                                                    <div class="ModalHeader">
                                                                        <div>
                                                                            <div class="app-head-button-holder-admin">
                                                                                <asp:LinkButton ID="btn_modalClose_deliverypickups" runat="server" CssClass="ModalExitButton deliverypickup-update"
                                                                                    OnClick="btn_modalClose_Click"></asp:LinkButton>
                                                                            </div>
                                                                            <span class="Modal-title">Schedule New Delivery/Pickup</span>
                                                                        </div>
                                                                    </div>
                                                                    <div class="ModalScrollContent">
                                                                        <div class="ModalPadContent">
                                                                            <asp:UpdatePanel ID="updatepnl_newApp_deliverypickups" runat="server">
                                                                                <ContentTemplate>
                                                                                    <div id="loading_Sch" class="ajaxLoading" style="display: none;">
                                                                                        <h2 class="pad-top-big">Updating modal. Please wait.</h2>
                                                                                    </div>
                                                                                    <div id="container_Sch">
                                                                                        <asp:Panel ID="Step1_deliverypickups" runat="server" CssClass="inline-block" Style="width: 260px;">
                                                                                            <div class="myItemStyle" style="height: 260px;">
                                                                                                <div class="float-left">
                                                                                                    <table cellpadding="10" cellspacing="10" width="290px">
                                                                                                        <tbody>
                                                                                                            <tr>
                                                                                                                <td class="contactFormInput">
                                                                                                                    <div class="float-left">
                                                                                                                        Delivery / Pickup<br />
                                                                                                                        <select id="spd_deliverypickups" onchange="onTypeChange(this)" runat="server" style="width: 100px;">
                                                                                                                            <option>Delivery</option>
                                                                                                                            <option>Pickup</option>
                                                                                                                        </select>
                                                                                                                    </div>
                                                                                                                    <div class="float-right PadRightBig">
                                                                                                                        Items<br />
                                                                                                                        <input id="schedule_items_deliverypickups" type="text" class="textEntry-noWidth" style="width: 30px;"
                                                                                                                            maxlength="2" runat="server" />
                                                                                                                    </div>
                                                                                                                </td>
                                                                                                            </tr>
                                                                                                            <tr>
                                                                                                                <td class="contactFormInput">Truck Line<br />
                                                                                                                    <input id="schedule_name_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                                                                                                        style="width: 225px;" />
                                                                                                                    <span class="float-right" style="color: Red; padding-left: 5px;"><small>*</small></span>
                                                                                                                </td>
                                                                                                            </tr>
                                                                                                            <tr>
                                                                                                                <td class="contactFormInput">Mill/Processor Name:<br />
                                                                                                                    <input id="schedule_from_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                                                                                                        style="width: 225px;" />
                                                                                                                    <span class="float-right" style="color: Red; padding-left: 5px;"><small>*</small></span>
                                                                                                                </td>
                                                                                                            </tr>
                                                                                                            <tr>
                                                                                                                <td class="contactFormInput">E-mail<br />
                                                                                                                    <input id="schedule_email_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                                                                                                        style="width: 225px; margin-bottom: 5px;" />
                                                                                                                    <span id="emailmarker" class="float-right" style="display: none; color: Red; padding-left: 5px;">
                                                                                                                        <small>*</small></span><br />
                                                                                                                    <span>Send Confirmation Email </span>
                                                                                                                    <input id="cb_sendEmail_deliverypickups" type="checkbox" runat="server" onchange="sendEmailChecked()" />
                                                                                                                </td>
                                                                                                            </tr>
                                                                                                        </tbody>
                                                                                                    </table>
                                                                                                </div>
                                                                                                <div class="float-right contactFormInput" style="width: 250px; padding: 10px 0;">
                                                                                                    Phone Number<br />
                                                                                                    <input id="schedule_phone1_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                                                                                        style="width: 35px;" maxlength="3" />
                                                                                                    -
                                                                                            <input id="schedule_phone2_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                                                                                style="width: 35px;" maxlength="3" />
                                                                                                    -
                                                                                            <input id="schedule_phone3_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                                                                                style="width: 45px;" maxlength="4" />
                                                                                                    <div class="clear-space">
                                                                                                    </div>
                                                                                                    Truck Number<br />
                                                                                                    <input id="schedule_trucknum_deliverypickups" runat="server" type="text" class="textEntry-noWidth"
                                                                                                        style="width: 200px; margin-bottom: 5px;" />
                                                                                                    <div class="clear-space">
                                                                                                    </div>
                                                                                                    Comments (optional)<br />
                                                                                                    <textarea id="schedule_comment_deliverypickups" runat="server" class="TextBoxComment"
                                                                                                        rows="2" style="width: 200px"></textarea>
                                                                                                    <div class="clear-margin">
                                                                                                        <i>Fields with a <b style="color: Red;">*</b> next them are required.</i>
                                                                                                        <div id="failMessage_deliverypickups" runat="server" class="clear">
                                                                                                        </div>
                                                                                                        <div class="clear PadAll">
                                                                                                            <input id="btn_step1_deliverypickups" type="button" class="input-buttons" value="Select a Date"
                                                                                                                onclick="btnStepOne()" style="margin: 5px 0 0 45px;" />
                                                                                                        </div>
                                                                                                    </div>
                                                                                                </div>
                                                                                            </div>
                                                                                        </asp:Panel>
                                                                                        <asp:Panel ID="Step2_deliverypickups" runat="server" CssClass="inline-block" Style="width: 260px;">
                                                                                            <div class="float-left">
                                                                                                <strong class="font-color-black">Date</strong> Selection
                                                                                        <div class="clear pad-all">
                                                                                            <asp:TextBox runat="server" ID="tb_Date1_deliverypickups" autocomplete="off" OnTextChanged="tb_Date1_TextChanged"
                                                                                                CssClass="textEntry" Style="width: 150px;" AutoPostBack="True" /><br />
                                                                                            <small><i>Click in the box above to select a date</i></small>
                                                                                            <br />
                                                                                            <asp:LinkButton ID="lb_clearDate_deliverypickups" runat="server" CssClass="deliverypickup-update-modal"
                                                                                                OnClick="lb_clearDate_Click">Clear Date</asp:LinkButton>
                                                                                        </div>
                                                                                                <div class="clear-margin">
                                                                                                    <input id="btn_stepBack" type="button" class="input-buttons" value="Go Back" onclick="btnStepBack()" />
                                                                                                </div>
                                                                                            </div>
                                                                                            <div class="float-right" style="width: 315px;">
                                                                                                <h3>
                                                                                                    <strong class="font-color-black">Schedule</strong> Time</h3>
                                                                                                <div class="clear-margin pad-all">
                                                                                                    Time slots not listed indicates another appointment<br />
                                                                                                    (Hours: 6:30 am - 10:00 pm, Monday - Friday)
                                                                                                </div>
                                                                                                <asp:DropDownList ID="dd_schTimeSlot_deliverypickups" Style='width: 95px;' runat="server"
                                                                                                    Enabled="False" Visible="False">
                                                                                                </asp:DropDownList>
                                                                                                <asp:Button ID="btn_selectTime_deliverypickups" runat="server" CssClass="input-buttons margin-left-big deliverypickup-update-modal"
                                                                                                    Text="Select Time" Enabled="False" Visible="False" OnClick="btn_selectTime_Click" /><div
                                                                                                        class="clear-space">
                                                                                                    </div>
                                                                                                <asp:Label ID="lbl_slotsopen_deliverypickups" runat="server" Enabled="False" Visible="False"></asp:Label>
                                                                                                <div class="clear-margin">
                                                                                                    <br />
                                                                                                    <asp:Label ID="lbl_SelectedDate_deliverypickups" runat="server" Text="" Enabled="false"
                                                                                                        Visible="false"></asp:Label>
                                                                                                    <div class="clear-space">
                                                                                                    </div>
                                                                                                    <asp:Button ID="btn_finish_deliverypickups" runat="server" Text="" CssClass="input-buttons deliverypickup-update-modal"
                                                                                                        Enabled="false" Visible="false" OnClick="btn_finish_Click" />
                                                                                                </div>
                                                                                            </div>
                                                                                        </asp:Panel>
                                                                                        <asp:Panel ID="Step3_deliverypickups" runat="server" CssClass="inline-block" Style="width: 260px;">
                                                                                            <div id="CompleteSch_deliverypickups" runat="server">
                                                                                            </div>
                                                                                            <div class="clear-space">
                                                                                            </div>
                                                                                            Click
                                                                                    <asp:LinkButton ID="btn_newApp_deliverypickups" OnClick="btn_newApp_Click" runat="server">HERE</asp:LinkButton>
                                                                                            to schedule a new appointment or click the close button at the top right of this
                                                                                    popup.
                                                                                        </asp:Panel>
                                                                                    </div>
                                                                                </ContentTemplate>
                                                                                <Triggers>
                                                                                    <asp:AsyncPostBackTrigger ControlID="btn_modalClose_deliverypickups" />
                                                                                </Triggers>
                                                                            </asp:UpdatePanel>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
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
            <script type="text/javascript" src="deliverypickups.js"></script>
            <script type="text/javascript">
                onSchedulerLoad('<%=a %>', '<%=b %>', '<%=c %>', '<%=d %>', '<%=e %>', '<%=s1 %>', '<%=s2 %>');
            </script>
        </div>
    </form>
</body>
</html>
