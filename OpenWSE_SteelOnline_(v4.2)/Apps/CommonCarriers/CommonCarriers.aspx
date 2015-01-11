<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CommonCarriers.aspx.cs" Inherits="Apps_CommonCarriers" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Common Carriers</title>
    <style type="text/css">
        .td-sort-click a
        {
            text-decoration: none !important;
        }
    </style>
</head>
<body style="background: #F5F5F5 !important;">
    <form id="form_steeltrucks" runat="server">
        <div>
            <ajaxToolkit:ToolkitScriptManager ID="ScriptManager_steeltrucks" runat="server" EnablePartialRendering="true" AsyncPostBackTimeout="360000">
            </ajaxToolkit:ToolkitScriptManager>
            <div id="steeltrucks-load" class="main-div-app-bg">
                <div class="pad-all app-title-bg-color" style="height: 40px">
                    <asp:UpdatePanel ID="UpdatePanel1_steeltrucks" runat="server">
                        <ContentTemplate>
                            <div class="float-left">
                                <asp:Image ID="img_Title" runat="server" CssClass="float-left pad-right" Height="38px" />
                                <asp:Label ID="lbl_Title" runat="server" Text="" Font-Size="30px"></asp:Label>
                            </div>
                            <div class="float-right">
                                <div class="clear-space-five">
                                </div>
                                <div id="searchwrapper" style="width: 375px;">
                                    <asp:Panel ID="Panel1_steeltrucks" runat="server" DefaultButton="imgbtn_search_steeltrucks">
                                        <asp:TextBox ID="tb_search_steeltrucks" runat="server" CssClass="searchbox" Font-Size="Small"
                                            onfocus="if(this.value=='Search Common Carriers')this.value=''" onblur="if(this.value=='')this.value='Search Common Carriers'"
                                            Text="Search Common Carriers"></asp:TextBox>
                                        <a href="#" onclick="$('#<%=tb_search_steeltrucks.ClientID %>').val('Search Common Carriers');return false;"
                                            class="searchbox_clear"></a>
                                        <asp:LinkButton ID="imgbtn_search_steeltrucks" runat="server" ToolTip="Start search"
                                            CssClass="searchbox_submit RandomActionBtns" OnClick="imgbtn_search_Click" />
                                    </asp:Panel>
                                </div>
                            </div>
                            <div class="float-right pad-right-sml">
                                <div class="clear-space-five">
                                </div>
                                <asp:Button ID="btn_export" runat="server" Text="Export to Excel" CssClass="input-buttons"
                                    OnClick="btnExportToExcel_Click" />
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <table class="content-table" cellpadding="0" cellspacing="0">
                    <tbody>
                        <tr>
                            <td class="td-sidebar">
                                <div id="sidebar-padding-trucksh" class="sidebar-padding">
                                    <div class="sidebar-scroll-app">
                                        <asp:Panel ID="pnl_ect" runat="server" Width="230px">
                                            <div class="clear-margin">
                                                <asp:UpdatePanel ID="updatepnl_ect_steeltrucks" runat="server">
                                                    <ContentTemplate>
                                                        <asp:Literal ID="ltl_ect" runat="server"></asp:Literal>
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
                                                        <input id="hf_currDriverexp" type="hidden" value="" />
                                                        <div class="pad-top-big margin-top">
                                                            <asp:UpdatePanel ID="updatepnl_schDriver_steeltrucks" runat="server">
                                                                <ContentTemplate>
                                                                    <div class="pad-left pad-right pad-bottom-big">
                                                                        <div class="pad-left-sml float-left pad-top-sml" style="color: #555555; font-size: 22px;">
                                                                            <asp:Label ID="lbl_schDriverUser_steeltrucks" runat="server" Text=""></asp:Label>
                                                                        </div>
                                                                        <asp:HiddenField ID="HiddenField1_steeltrucks" runat="server" />
                                                                        <asp:HiddenField ID="hf_sortcol_steeltrucks" runat="server" />
                                                                        <asp:HiddenField ID="hf_dateselected_steeltrucks" runat="server" OnValueChanged="hf_dateselected_steeltrucks_Changed"
                                                                            ClientIDMode="Static" />
                                                                        <asp:LinkButton ID="refresh_steeltrucks_Click" runat="server" CssClass="float-right margin-right margin-left RandomActionBtns img-refresh"
                                                                            ToolTip="Refresh Files" OnClick="btn_refresh_Click" />
                                                                        <div class="float-right pad-right">
                                                                            <asp:DropDownList ID="dd_display_steeltrucks" runat="server" ToolTip="Change display amount"
                                                                                Width="60px" AutoPostBack="True" OnSelectedIndexChanged="dd_display_SelectedIndexChanged">
                                                                                <asp:ListItem Value="10">10</asp:ListItem>
                                                                                <asp:ListItem Value="20">20</asp:ListItem>
                                                                                <asp:ListItem Value="30">30</asp:ListItem>
                                                                                <asp:ListItem Value="40">40</asp:ListItem>
                                                                                <asp:ListItem Value="2000">All</asp:ListItem>
                                                                            </asp:DropDownList>
                                                                        </div>
                                                                        <div class="float-right pad-right-big">
                                                                            <select id="font-size-selector" class="float-right margin-right-sml">
                                                                                <option value="x-small">Font Size: x-Small</option>
                                                                                <option value="small">Font Size: Small</option>
                                                                                <option value="medium">Font Size: Medium</option>
                                                                                <option value="large">Font Size: Large</option>
                                                                                <option value="x-large">Font Size: x-Large</option>
                                                                            </select>
                                                                        </div>
                                                                        <div class="clear-space">
                                                                        </div>
                                                                        <div class="float-left">
                                                                            <small>
                                                                                <asp:LinkButton ID="lbtn_selectAll_steeltrucks" runat="server" CssClass="sb-links margin-top-sml RandomActionBtns"
                                                                                    OnClick="lbtn_selectAll_Click">Select All</asp:LinkButton></small>
                                                                        </div>
                                                                        <div class="float-right borderShadow2" style="padding: 4px 2px; background-color: #FFF; color: #555!important">
                                                                            <div class="float-right pad-left pad-right">
                                                                                <b class="pad-right">TOTAL MONTHLY WEIGHT</b><asp:Label ID="lbl_smwtruckweight_steeltrucks"
                                                                                    runat="server" Text=""></asp:Label>
                                                                            </div>
                                                                        </div>
                                                                        <div class="clear-space-five">
                                                                        </div>
                                                                    </div>
                                                                    <asp:GridView ID="GV_Header_steeltrucks" runat="server" CellPadding="0" CellSpacing="0"
                                                                        AutoGenerateColumns="False" GridLines="None" OnRowCommand="GV_Header_RowCommand"
                                                                        OnRowDataBound="GV_Header_RowDataBound" AllowPaging="True" OnPageIndexChanging="GV_Header_PageIndexChanging"
                                                                        OnRowEditing="GV_Header_RowEdit" OnRowCancelingEdit="GV_Header_CancelEdit" OnRowUpdating="GV_Header_RowUpdate"
                                                                        ShowHeaderWhenEmpty="True" OnRowCreated="GV_Header_RowCreated" Width="100%">
                                                                        <PagerSettings Position="Bottom" />
                                                                        <PagerTemplate>
                                                                            <div class="GridViewPager">
                                                                                <table width="100%">
                                                                                    <tbody>
                                                                                        <tr>
                                                                                            <td align="right" valign="middle" width="50">
                                                                                                <asp:LinkButton ID="btnFirst" runat="server" CommandName="Page" CommandArgument="First"
                                                                                                    CssClass="pg-first-btn RandomActionBtns" ToolTip="Goto First Page" />
                                                                                                <asp:LinkButton ID="btnPrevious" runat="server" CommandName="Page" CommandArgument="Prev"
                                                                                                    CssClass="pg-prev-btn RandomActionBtns" ToolTip="Previous Page" />
                                                                                            </td>
                                                                                            <td align="center" valign="middle" width="150">
                                                                                                <asp:Panel ID="Panel1_steeltrucks" runat="server">
                                                                                                    Page:
                                                                                                <asp:TextBox ID="tb_pageManual_steeltrucks" runat="server" Width="22" OnTextChanged="tb_pageManual_TextChanged"
                                                                                                    AutoPostBack="True"></asp:TextBox>
                                                                                                    of
                                                                                                <asp:Label ID="pglbl2_steeltrucks" runat="server" Text=""></asp:Label>
                                                                                                    <asp:Button ID="btn_hidden_steeltrucks" runat="server" Text="" Visible="False" />
                                                                                                </asp:Panel>
                                                                                            </td>
                                                                                            <td align="left" valign="middle" width="50">
                                                                                                <asp:LinkButton ID="btnNext" runat="server" CommandName="Page" CommandArgument="Next"
                                                                                                    CssClass="pg-next-btn RandomActionBtns" ToolTip="Next Page" />
                                                                                                <asp:LinkButton ID="btnLast" runat="server" CommandName="Page" CommandArgument="Last"
                                                                                                    CssClass="pg-last-btn RandomActionBtns" ToolTip="Goto Last Page" />
                                                                                            </td>
                                                                                            <td align="center" valign="top">
                                                                                                <asp:Panel ID="PnlPager_steeltrucks" runat="server">
                                                                                                </asp:Panel>
                                                                                            </td>
                                                                                        </tr>
                                                                                    </tbody>
                                                                                </table>
                                                                            </div>
                                                                        </PagerTemplate>
                                                                        <EmptyDataRowStyle ForeColor="Black" />
                                                                        <RowStyle CssClass="GridNormalRow" />
                                                                        <AlternatingRowStyle CssClass="GridAlternate" />
                                                                        <EmptyDataTemplate>
                                                                            <div class="emptyGridView">
                                                                                No Common Carrier logs for selected month
                                                                            </div>
                                                                        </EmptyDataTemplate>
                                                                        <Columns>
                                                                            <asp:TemplateField>
                                                                                <HeaderTemplate>
                                                                                    <table class="myHeaderStyle" cellpadding="5" cellspacing="0" style="font-size: 0.90em; min-width: 1000px">
                                                                                        <tr>
                                                                                            <td width="24px" align="center">
                                                                                                <asp:LinkButton ID="imgbtn_del_steeltrucks" runat="server" ToolTip="Delete selected items"
                                                                                                    OnClick="imgbtn_del_Click" CssClass="td-delete-light-btn" OnClientClick="setTimeout(function () { $('#steeltrucks-load').unblock(); }, 500);return window.confirm('Are you sure you want to delete the selected files?')"></asp:LinkButton>
                                                                                            </td>
                                                                                            <td id="td_date" runat="server" width="90px" class="td-sort-click">
                                                                                                <asp:LinkButton ID="lbtn_date_steeltrucks" runat="server" OnClick="lbtn_date_Click"
                                                                                                    CssClass="RandomActionBtns">Date</asp:LinkButton>
                                                                                            </td>
                                                                                            <td id="td_truckline" runat="server" class="td-sort-click" style="min-width: 100px">
                                                                                                <asp:LinkButton ID="lbtn_truckline_steeltrucks" runat="server" OnClick="lbtn_truckline_Click"
                                                                                                    CssClass="RandomActionBtns">Truck Line</asp:LinkButton>
                                                                                            </td>
                                                                                            <td id="td_trucknumber" runat="server" class="td-sort-click" width="150px">
                                                                                                <asp:LinkButton ID="lbtn_trucknumber_steeltrucks" runat="server" OnClick="lbtn_unit_Click"
                                                                                                    CssClass="RandomActionBtns">Truck #</asp:LinkButton>
                                                                                            </td>
                                                                                            <td id="td_customer" runat="server" class="td-sort-click" width="150px">
                                                                                                <asp:LinkButton ID="lbtn_name_steeltrucks" runat="server" OnClick="lbtn_name_Click"
                                                                                                    CssClass="RandomActionBtns">Customer</asp:LinkButton>
                                                                                            </td>
                                                                                            <td id="td_direction" runat="server" class="td-sort-click" width="150px">
                                                                                                <asp:LinkButton ID="lbtn_direction_steeltrucks" runat="server" OnClick="lbtn_direction_Click"
                                                                                                    CssClass="RandomActionBtns">Driection</asp:LinkButton>
                                                                                            </td>
                                                                                            <td id="td_ordernumber" runat="server" class="td-sort-click" width="75px">
                                                                                                <asp:LinkButton ID="lbtn_ordernumber_steeltrucks" runat="server" OnClick="lbtn_ordernumber_Click"
                                                                                                    CssClass="RandomActionBtns">Order #</asp:LinkButton>
                                                                                            </td>
                                                                                            <td id="td_weight" runat="server" class="td-sort-click" width="75px">
                                                                                                <asp:LinkButton ID="lbtn_weight_steeltrucks" runat="server" OnClick="lbtn_weight_Click"
                                                                                                    CssClass="RandomActionBtns">Weight</asp:LinkButton>
                                                                                            </td>
                                                                                            <td align="center" width="75px">Actions
                                                                                            </td>
                                                                                        </tr>
                                                                                    </table>
                                                                                </HeaderTemplate>
                                                                                <ItemTemplate>
                                                                                    <table cellpadding="5" cellspacing="0" class='<%#Eval("RowClass") %>' style="font-size: 0.90em; min-width: 1000px">
                                                                                        <tr>
                                                                                            <td width="24px" align="center" class="GridViewNumRow">
                                                                                                <asp:CheckBox ID="CheckBoxIndv_steeltrucks" runat="server" OnCheckedChanged="CheckBoxIndv_CheckChanged"
                                                                                                    Text='<%#Eval("ID") %>' CssClass="HiddenText" AutoPostBack="true" />
                                                                                            </td>
                                                                                            <td class="border-right" width="90px">
                                                                                                <%#Eval("Date") %>
                                                                                            </td>
                                                                                            <td class="border-right" style="min-width: 100px">
                                                                                                <%#Eval("TruckLine")%>
                                                                                            </td>
                                                                                            <td class="border-right" width="150px">
                                                                                                <%#Eval("TruckNumber")%>
                                                                                            </td>
                                                                                            <td class="border-right" width="150px">
                                                                                                <%#Eval("Customer")%>
                                                                                            </td>
                                                                                            <td class="border-right" width="150px">
                                                                                                <%#Eval("GeneralDirection")%>
                                                                                            </td>
                                                                                            <td class="border-right" width="75px">
                                                                                                <%#Eval("SalesOrder")%>
                                                                                            </td>
                                                                                            <td class="border-right" width="75px">
                                                                                                <%#Eval("Weight")%>
                                                                                            </td>
                                                                                            <td align="center" width="75px">
                                                                                                <asp:LinkButton ID="lb_edit_steeltrucks" runat="server" CssClass="td-edit-btn RandomActionBtns margin-right"
                                                                                                    CommandName="edit" CommandArgument='<%#Eval("ID")%>' ToolTip="Edit"></asp:LinkButton>
                                                                                                <asp:LinkButton ID="lb_delete_steeltrucks" runat="server" CssClass="td-delete-btn"
                                                                                                    CommandName="deleteSlot" ToolTip="Delete" CommandArgument='<%#Eval("ID")%>' OnClientClick="var ret = confirm('Are you sure you want to delete this line?');if (ret == true) {DeleteConfirmation();}else{return false;}"></asp:LinkButton>
                                                                                            </td>
                                                                                        </tr>
                                                                                    </table>
                                                                                    <asp:Panel ID="pnl_defaultbutton1" runat="server" DefaultButton="lb_add">
                                                                                        <table cellpadding="0" cellspacing="0" class='<%#Eval("RowClassAdd") %>' style="font-size: 0.95em; text-align: center; min-width: 1000px">
                                                                                            <tr>
                                                                                                <td width="24px" align="center" class="GridViewNumRow">
                                                                                                    <asp:CheckBox ID="CheckBox1" runat="server" CssClass="HiddenText" Style="visibility: hidden" />
                                                                                                </td>
                                                                                                <td class="border-right" width="90px">
                                                                                                    <asp:TextBox ID="tb_date" runat="server" CssClass="textEntry" Width="97%" Text='<%#Eval("Date") %>'
                                                                                                        onfocus="if(this.value=='Date')this.value=''" onblur="if(this.value=='')this.value='Date'"></asp:TextBox>
                                                                                                    <ajaxToolkit:CalendarExtender ID="defaultCalendarExtender" runat="server" TargetControlID="tb_date"
                                                                                                        ClearTime="True" DefaultView="Days" />
                                                                                                </td>
                                                                                                <td class="border-right" style="min-width: 100px">
                                                                                                    <asp:TextBox ID="tb_truckline" runat="server" CssClass="textEntry truckline-tb-autosearch"
                                                                                                        Width="97%" Text='<%#Eval("TruckLine") %>' onfocus="if(this.value=='Truckline')this.value=''"
                                                                                                        onblur="if(this.value=='')this.value='Truckline'"></asp:TextBox>
                                                                                                </td>
                                                                                                <td class="border-right" width="150px">
                                                                                                    <asp:TextBox ID="tb_trucknumber" runat="server" CssClass="textEntry trucknum-tb-autosearch"
                                                                                                        Width="97%" Text='<%#Eval("TruckNumber") %>' onfocus="if(this.value=='Truck #')this.value=''"
                                                                                                        onblur="if(this.value=='')this.value='Truck #'"></asp:TextBox>
                                                                                                </td>
                                                                                                <td class="border-right" width="150px">
                                                                                                    <asp:TextBox ID="tb_customer" runat="server" CssClass="textEntry customername-tb-autosearch"
                                                                                                        Width="97%" Text='<%#Eval("Customer") %>' onfocus="if(this.value=='Customer')this.value=''"
                                                                                                        onblur="if(this.value=='')this.value='Customer'"></asp:TextBox>
                                                                                                </td>
                                                                                                <td class="border-right" width="150px">
                                                                                                    <asp:DropDownList ID="dd_gd" Width="140px" runat="server">
                                                                                                    </asp:DropDownList>
                                                                                                </td>
                                                                                                <td class="border-right" width="75px">
                                                                                                    <asp:TextBox ID="tb_salesorder" runat="server" CssClass="textEntry" Width="97%" Text='<%#Eval("SalesOrder") %>'
                                                                                                        onfocus="if(this.value=='Order #')this.value=''" onblur="if(this.value=='')this.value='Order #'"></asp:TextBox>
                                                                                                </td>
                                                                                                <td class="border-right" width="75px">
                                                                                                    <asp:TextBox ID="tb_weight" runat="server" CssClass="textEntry" Width="97%" Text='<%#Eval("Weight") %>'
                                                                                                        onfocus="if(this.value=='Weight')this.value=''" onblur="if(this.value=='')this.value='Weight'"></asp:TextBox>
                                                                                                </td>
                                                                                                <td align="center" width="75px">
                                                                                                    <asp:LinkButton ID="lb_add" runat="server" CssClass="RandomActionBtns td-add-btn"
                                                                                                        CommandName="Add" CommandArgument='<%# string.Format("{0}",Container.DataItemIndex + 1) %>'
                                                                                                        ToolTip="Add"></asp:LinkButton>
                                                                                                </td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </asp:Panel>
                                                                                </ItemTemplate>
                                                                                <EditItemTemplate>
                                                                                    <asp:HiddenField ID="hf_date_edit" runat="server" Value='<%#Eval("Date") %>' />
                                                                                    <asp:HiddenField ID="hf_id_edit" runat="server" Value='<%#Eval("ID") %>' />
                                                                                    <asp:HiddenField ID="hf_generaldirection_edit" runat="server" Value='<%#Eval("GeneralDirection") %>' />
                                                                                    <asp:Panel ID="pnl_defaultbutton2" runat="server" DefaultButton="lb_update">
                                                                                        <table cellpadding="0" cellspacing="0" class="myItemStyle" style="font-size: 0.95em; text-align: center; min-width: 1000px">
                                                                                            <tr>
                                                                                                <td width="24px" align="center" class="GridViewNumRow"></td>
                                                                                                <td class="border-right" width="90px">
                                                                                                    <asp:TextBox ID="tb_date_edit" runat="server" CssClass="textEntry" Width="97%" Text='<%#Eval("Date") %>'></asp:TextBox>
                                                                                                    <ajaxToolkit:CalendarExtender ID="defaultCalendarExtender" runat="server" TargetControlID="tb_date_edit"
                                                                                                        ClearTime="True" DefaultView="Days" />
                                                                                                </td>
                                                                                                <td class="border-right" style="min-width: 100px">
                                                                                                    <asp:TextBox ID="tb_truckline_edit" runat="server" CssClass="textEntry truckline-tb-autosearch"
                                                                                                        Width="97%" Text='<%#Eval("TruckLine") %>'></asp:TextBox>
                                                                                                </td>
                                                                                                <td class="border-right" width="150px">
                                                                                                    <asp:TextBox ID="tb_trucknumber_edit" runat="server" CssClass="textEntry trucknum-tb-autosearch"
                                                                                                        Width="97%" Text='<%#Eval("TruckNumber") %>'></asp:TextBox>
                                                                                                </td>
                                                                                                <td class="border-right" width="150px">
                                                                                                    <asp:TextBox ID="tb_customer_edit" runat="server" CssClass="textEntry customername-tb-autosearch"
                                                                                                        Width="97%" Text='<%#Eval("Customer") %>'></asp:TextBox>
                                                                                                </td>
                                                                                                <td class="border-right" width="150px">
                                                                                                    <asp:DropDownList ID="dd_gd_edit" Width="140px" runat="server">
                                                                                                    </asp:DropDownList>
                                                                                                </td>
                                                                                                <td class="border-right" width="75px">
                                                                                                    <asp:TextBox ID="tb_salesorder_edit" runat="server" CssClass="textEntry" Width="97%"
                                                                                                        Text='<%#Eval("SalesOrder") %>'></asp:TextBox>
                                                                                                </td>
                                                                                                <td class="border-right" width="75px">
                                                                                                    <asp:TextBox ID="tb_weight_edit" runat="server" CssClass="textEntry" Width="97%"
                                                                                                        Text='<%#Eval("Weight") %>'></asp:TextBox>
                                                                                                </td>
                                                                                                <td align="center" width="75px">
                                                                                                    <asp:LinkButton ID="lb_update" runat="server" CssClass="RandomActionBtns td-update-btn margin-right"
                                                                                                        CommandName="Update" CausesValidation="True"></asp:LinkButton>
                                                                                                    <asp:LinkButton ID="lb_cancel" runat="server" CssClass="RandomActionBtns td-cancel-btn"
                                                                                                        CommandName="Cancel" CausesValidation="False" ToolTip="Cancel"></asp:LinkButton>
                                                                                                </td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </asp:Panel>
                                                                                </EditItemTemplate>
                                                                            </asp:TemplateField>
                                                                        </Columns>
                                                                    </asp:GridView>
                                                                </ContentTemplate>
                                                            </asp:UpdatePanel>
                                                        </div>
                                                        <div id="DirEdit-element" class="Modal-element">
                                                            <div class="Modal-overlay">
                                                                <div class="Modal-element-align">
                                                                    <div class="Modal-element-modal">
                                                                        <div class="ModalHeader">
                                                                            <div>
                                                                                <div class="app-head-button-holder-admin">
                                                                                    <a href="#" onclick="LoadGenDirEditor();return false;" class="ModalExitButton"></a>
                                                                                </div>
                                                                                <span class="Modal-title"></span>
                                                                            </div>
                                                                        </div>
                                                                        <div class="ModalPadContent">
                                                                            <div id="GenDirList" style="max-height: 325px; overflow: auto;">
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
                <script type="text/javascript">
                    Sys.Application.add_load(function () {

                        var fontsize = cookie.get("commoncarriers-fontsize");
                        if ((fontsize != null) && (fontsize != "")) {
                            $(".GridNormalRow, .GridAlternate").css("font-size", fontsize);
                            $("#font-size-selector option").each(function () {
                                if ($(this).val() == fontsize) {
                                    $(this).attr('selected', 'selected');
                                }
                                else {
                                    $(this).removeAttr('selected');
                                }
                            });
                        }
                        else {
                            $("#font-size-selector option").each(function () {
                                if ($(this).val() == "small") {
                                    $(this).attr('selected', 'selected');
                                }
                                else {
                                    $(this).removeAttr('selected');
                                }
                            });
                        }

                        reloadCurr();
                        $("#<%=tb_search_steeltrucks.ClientID %>").autocomplete({
                        minLength: 1,
                        autoFocus: true,
                        source: function (request, response) {
                            $.ajax({
                                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetTruckSchedule",
                                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                dataFilter: function (data) { return data; },
                                success: function (data) {
                                    response($.map(data.d, function (item) {
                                        return {
                                            label: item,
                                            value: item
                                        }
                                    }))
                                }
                            });
                        }
                    }).focus(function () {
                        $(this).autocomplete("search", "");
                    });
                    $(".customername-tb-autosearch").autocomplete({
                        minLength: 1,
                        autoFocus: true,
                        source: function (request, response) {
                            $.ajax({
                                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListCustomersTS",
                                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                dataFilter: function (data) { return data; },
                                success: function (data) {
                                    response($.map(data.d, function (item) {
                                        return {
                                            label: item,
                                            value: item
                                        }
                                    }))
                                }
                            });
                        }
                    }).focus(function () {
                        $(this).autocomplete("search", "");
                    });
                    $(".truckline-tb-autosearch").autocomplete({
                        minLength: 1,
                        autoFocus: true,
                        source: function (request, response) {
                            $.ajax({
                                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetTruckLinesCC",
                                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                dataFilter: function (data) { return data; },
                                success: function (data) {
                                    response($.map(data.d, function (item) {
                                        return {
                                            label: item,
                                            value: item
                                        }
                                    }))
                                }
                            });
                        }
                    }).focus(function () {
                        $(this).autocomplete("search", "");
                    });
                    $(".trucknum-tb-autosearch").autocomplete({
                        minLength: 1,
                        autoFocus: true,
                        source: function (request, response) {
                            $.ajax({
                                url: openWSE.siteRoot() + "WebServices/AutoComplete_Custom.asmx/GetListOfSMWUnits",
                                data: "{ 'prefixText': '" + request.term + "', 'count': '10' }",
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                dataFilter: function (data) { return data; },
                                success: function (data) {
                                    response($.map(data.d, function (item) {
                                        return {
                                            label: item,
                                            value: item
                                        }
                                    }))
                                }
                            });
                        }
                    }).focus(function () {
                        $(this).autocomplete("search", "");
                    });

                    ResizeSideBar();
                });
                </script>
            </div>
        </div>
    </form>
</body>
</html>
