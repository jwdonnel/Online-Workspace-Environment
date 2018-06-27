<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ProductLeadTime.aspx.cs"
    Inherits="Apps_ProdLeadTimes_ProductLeadTime" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Production Lead Times</title>
    <style type="text/css">
        #GV_PLTHistory_ProductLeadTime_PnlPager_ProductLeadTime { width: 560px; height: 40px; overflow: auto; }
        .lead-time-holder { float: left; padding: 0 20px; }
            .lead-time-holder .lead-time-name { clear: both; font-size: 13px; padding-bottom: 5px; font-weight: bold; }
    </style>
</head>
<body>
    <div id="plt-load" class="main-div-app-bg">
        <form id="form_mydocuments" runat="server" enctype="multipart/form-data" method="post">
            <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True"
                EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
            <asp:UpdatePanel ID="UpdatePanel4_ProductLeadTime" runat="server">
                <ContentTemplate>
                    <asp:HiddenField ID="hf_UpdateAll" runat="server" OnValueChanged="hf_UpdateAll_ValueChanged" />
                    <div class="pad-all app-title-bg-color">
                        <div class="float-left">
                            <asp:Image ID="img_Title" runat="server" CssClass="app-img-titlebar" />
                            <asp:Label ID="lbl_Title" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
                        </div>
                        <div align="right" class="float-right" style="font-size: 15px">
                            <div class="pad-right">
                                <b>Time Updated: </b>
                                <asp:Label ID="lbl_lastupdated_ProductLeadTime" runat="server" Text="N/A"></asp:Label>
                                <div class="clear-space-five">
                                </div>
                                <b>Updated by: </b>
                                <asp:Label ID="lbl_updatedby_ProductLeadTime" runat="server" Text="N/A"></asp:Label>
                            </div>
                        </div>
                        <div class="clear"></div>
                    </div>
                    <div class="pad-all">
                        <div class="float-left">
                            <span style="color: #618ebf">After noon please add 1 day to lead time<br />
                                If material needs to go from CTL to shear please add 1 day to lead time</span>
                        </div>
                        <div class="float-right">
                            Please note there is a 25,000 Lb max load
                        </div>
                    </div>
                    <div class="clear-space">
                    </div>


                    <div class="lead-time-holder">
                        <div class="lead-time-name">CTL Line 1 (Small)</div>
                        <div class="lead-time-editor">
                            <asp:TextBox runat="server" ID="tb_Date1_ProductLeadTime" autocomplete="off" OnTextChanged="tb_Date1_TextChanged"
                                CssClass="textEntry date-picker-tb" Style="width: 125px;" /><br />
                            <div class="clear-margin">
                                <asp:CheckBox ID="CheckBox1_ProductLeadTime" runat="server" Text="&nbsp;Out of Service"
                                    OnCheckedChanged="CheckBox1_CheckedChanged" CssClass="plt-update" AutoPostBack="True"
                                    Enabled="false" Visible="false" />
                                <div class="clear" style="height: 5px;">
                                </div>
                                <asp:CheckBox ID="CheckBox1_1_ProductLeadTime" runat="server" Text="&nbsp;Allow Override"
                                    OnCheckedChanged="CheckBox1_1CheckedChanged" AutoPostBack="True" CssClass="plt-update" />
                            </div>
                        </div>
                    </div>
                    <div class="lead-time-holder">
                        <div class="lead-time-name">CTL Line 2 (Large)</div>
                        <div class="lead-time-editor">
                            <asp:TextBox runat="server" ID="tb_Date2_ProductLeadTime" autocomplete="off" OnTextChanged="tb_Date2_TextChanged"
                                CssClass="textEntry date-picker-tb" Style="width: 125px;" /><br />
                            <div class="clear-margin">
                                <asp:CheckBox ID="CheckBox2_ProductLeadTime" runat="server" Text="&nbsp;Out of Service"
                                    OnCheckedChanged="CheckBox2_CheckedChanged" CssClass="plt-update" Enabled="false"
                                    Visible="false" AutoPostBack="True" />
                                <div class="clear" style="height: 5px;">
                                </div>
                                <asp:CheckBox ID="CheckBox2_2_ProductLeadTime" runat="server" Text="&nbsp;Allow Override"
                                    OnCheckedChanged="CheckBox2_2CheckedChanged" AutoPostBack="True" CssClass="plt-update" />
                            </div>
                        </div>
                    </div>
                    <div class="lead-time-holder">
                        <div class="lead-time-name">1/4" Shear</div>
                        <div class="lead-time-editor">
                            <asp:TextBox runat="server" ID="tb_Date3_ProductLeadTime" autocomplete="off" OnTextChanged="tb_Date3_TextChanged"
                                CssClass="textEntry date-picker-tb" Style="width: 125px;" /><br />
                            <div class="clear-margin">
                                <asp:CheckBox ID="CheckBox3_ProductLeadTime" runat="server" Text="&nbsp;Out of Service"
                                    OnCheckedChanged="CheckBox3_CheckedChanged" AutoPostBack="True" CssClass="plt-update"
                                    Enabled="false" Visible="false" />
                                <div class="clear" style="height: 5px;">
                                </div>
                                <asp:CheckBox ID="CheckBox3_3_ProductLeadTime" runat="server" Text="&nbsp;Allow Override"
                                    OnCheckedChanged="CheckBox3_3CheckedChanged" AutoPostBack="True" CssClass="plt-update" />
                            </div>
                        </div>
                    </div>
                    <div class="lead-time-holder">
                        <div class="lead-time-name">1/2" Shear</div>
                        <div class="lead-time-editor">
                            <asp:TextBox runat="server" ID="tb_Date4_ProductLeadTime" autocomplete="off" OnTextChanged="tb_Date4_TextChanged"
                                CssClass="textEntry date-picker-tb" Style="width: 125px;" /><br />
                            <div class="clear-margin">
                                <asp:CheckBox ID="CheckBox4_ProductLeadTime" runat="server" Text="&nbsp;Out of Service"
                                    OnCheckedChanged="CheckBox4_CheckedChanged" CssClass="plt-update" Enabled="false"
                                    Visible="false" AutoPostBack="True" />
                                <div class="clear" style="height: 5px;">
                                </div>
                                <asp:CheckBox ID="CheckBox4_4_ProductLeadTime" runat="server" Text="&nbsp;Allow Override"
                                    OnCheckedChanged="CheckBox4_4CheckedChanged" AutoPostBack="True" CssClass="plt-update" />
                            </div>
                        </div>
                    </div>
                    <div class="lead-time-holder">
                        <div class="lead-time-name">60x120 Laser</div>
                        <div class="lead-time-editor">
                            <asp:TextBox runat="server" ID="tb_Date6_ProductLeadTime" autocomplete="off" OnTextChanged="tb_Date6_TextChanged"
                                CssClass="textEntry date-picker-tb" Style="width: 125px;" /><br />
                            <div class="clear-margin">
                                <asp:CheckBox ID="CheckBox6_ProductLeadTime" runat="server" Text="&nbsp;Out of Service"
                                    OnCheckedChanged="CheckBox6_CheckedChanged" CssClass="plt-update" Enabled="false"
                                    Visible="false" AutoPostBack="True" />
                                <div class="clear" style="height: 5px;">
                                </div>
                                <asp:CheckBox ID="CheckBox6_6_ProductLeadTime" runat="server" Text="&nbsp;Allow Override"
                                    OnCheckedChanged="CheckBox6_6CheckedChanged" AutoPostBack="True" CssClass="plt-update" />
                            </div>
                        </div>
                    </div>
                    <div class="lead-time-holder">
                        <div class="lead-time-name">Burn Table</div>
                        <div class="lead-time-editor">
                            <asp:TextBox runat="server" ID="tb_Date5_ProductLeadTime" autocomplete="off" OnTextChanged="tb_Date5_TextChanged"
                                CssClass="textEntry date-picker-tb" Style="width: 125px;" /><br />
                            <div class="clear-margin">
                                <asp:CheckBox ID="CheckBox5_ProductLeadTime" runat="server" Text="&nbsp;Out of Service"
                                    OnCheckedChanged="CheckBox5_CheckedChanged" CssClass="plt-update" Enabled="false"
                                    Visible="false" AutoPostBack="True" />
                                <div class="clear" style="height: 5px;">
                                </div>
                                <asp:CheckBox ID="CheckBox5_5_ProductLeadTime" runat="server" Text="&nbsp;Allow Override"
                                    OnCheckedChanged="CheckBox5_5CheckedChanged" AutoPostBack="True" CssClass="plt-update" />
                            </div>
                        </div>
                    </div>
                    <div class="clear-space"></div>
                    <asp:Button ID="btn_update_ProductLeadTime" runat="server" CssClass="input-buttons plt-update float-left margin-left-big" Text="Update" OnClick="btn_update_Click" />
                    <div class="float-right pad-left-big pad-right-big">
                        <asp:Label ID="lbl_error_ProductLeadTime" runat="server" Style="color: Red" Text="* Please make sure all lead times are set. (No empty boxes)"
                            Enabled="false" Visible="false"></asp:Label>
                        <div class="clear-space">
                        </div>
                        <asp:Label ID="lbl_error2_ProductLeadTime" runat="server" Style="color: Red" Text="* Dates must be same day before noon or future dates."
                            Enabled="false" Visible="false"></asp:Label>
                    </div>

                    <div class="clear-space">
                    </div>
                    <div class="clear-space">
                    </div>
                    <div class="float-left pad-left-big">
                        <h2>Production Lead Time History</h2>
                    </div>
                    <div class="float-right">
                        <small>
                            <asp:LinkButton ID="lbtn_selectAll_ProductLeadTime" runat="server" CssClass="margin-top-sml margin-right-big plt-update"
                                OnClick="lbtn_selectAll_Click">Select All</asp:LinkButton></small>
                    </div>
                    <div class="clear-space">
                    </div>
                    <asp:HiddenField ID="HiddenField1_ProductLeadTime" runat="server" />
                    <div class="pad-left-big pad-right-big">
                        <asp:GridView ID="GV_PLTHistory_ProductLeadTime" runat="server" CellPadding="0" CellSpacing="0"
                            AutoGenerateColumns="False" GridLines="None" ShowHeaderWhenEmpty="True" PageSize="7"
                            Width="100%" AllowPaging="true" OnRowCommand="GV_PLTHistory_RowCommand" OnRowDataBound="GV_PLTHistory_RowDataBound"
                            OnPageIndexChanging="GV_PLTHistory_PageIndexChanging" OnRowCreated="GV_PLTHistory_RowCreated">
                            <EmptyDataRowStyle ForeColor="Black" />
                            <RowStyle CssClass="GridNormalRow" />
                            <EmptyDataTemplate>
                                <div class="emptyGridView">
                                    History not available
                                </div>
                            </EmptyDataTemplate>
                            <PagerSettings Position="Bottom" />
                            <PagerTemplate>
                                <div class="GridViewPager">
                                    <table width="100%">
                                        <tbody>
                                            <tr>
                                                <td align="right" valign="middle" width="50">
                                                    <asp:LinkButton ID="btnFirst_ProductLeadTime" runat="server" CommandName="Page" CommandArgument="First"
                                                        CssClass="pg-first-btn plt-update" ToolTip="Goto First Page" />
                                                    <asp:LinkButton ID="btnPrevious_ProductLeadTime" runat="server" CommandName="Page"
                                                        CommandArgument="Prev" CssClass="pg-prev-btn plt-update" ToolTip="Previous Page" />
                                                </td>
                                                <td align="center" valign="middle" width="150">
                                                    <asp:Panel ID="Panel1_ProductLeadTime" runat="server">
                                                        Page:
                                                <asp:TextBox ID="tb_pageManual_ProductLeadTime" runat="server" Width="22" OnTextChanged="tb_pageManual_TextChanged"
                                                    AutoPostBack="True"></asp:TextBox>
                                                        of
                                                <asp:Label ID="pglbl2_ProductLeadTime" runat="server" Text=""></asp:Label>
                                                    </asp:Panel>
                                                </td>
                                                <td align="left" valign="middle" width="50">
                                                    <asp:LinkButton ID="btnNext_ProductLeadTime" runat="server" CommandName="Page" CommandArgument="Next"
                                                        CssClass="pg-next-btn plt-update" ToolTip="Next Page" />
                                                    <asp:LinkButton ID="btnLast_ProductLeadTime" runat="server" CommandName="Page" CommandArgument="Last"
                                                        CssClass="pg-last-btn plt-update" ToolTip="Goto Last Page" />
                                                </td>
                                                <td align="center" valign="top">
                                                    <asp:Panel ID="PnlPager_ProductLeadTime" runat="server">
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
                                        <table class="myHeaderStyle" cellpadding="5" cellspacing="0" style="min-width: 925px;">
                                            <tr>
                                                <td width="20px" align="center">
                                                    <asp:LinkButton ID="imgbtn_del_ProductLeadTime" runat="server" ToolTip="Delete selected items"
                                                        OnClick="imgbtn_del_Click" CssClass="td-delete-btn" OnClientClick="return window.confirm('Are you sure you want to delete the selected files?');"
                                                        Style="padding: 0px!important;"></asp:LinkButton>
                                                </td>
                                                <td style="min-width: 150px;">Date Updated
                                                </td>
                                                <td style="width: 115px;">CTL 1 (Light)
                                                </td>
                                                <td style="width: 115px;">CTL 2 (Heavy)
                                                </td>
                                                <td style="width: 115px;">1/4" Shear
                                                </td>
                                                <td style="width: 115px;">1/2" Shear
                                                </td>
                                                <td style="width: 115px;">Laser Cutter
                                                </td>
                                                <td style="width: 115px;">Plasma Table
                                                </td>
                                                <td style="min-width: 120px;">Updated By
                                                </td>
                                            </tr>
                                        </table>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <table class="myItemStyle" cellpadding="5" cellspacing="0" style="min-width: 925px;">
                                            <tbody>
                                                <td width="22px" align="center" class="GridViewNumRow">
                                                    <asp:CheckBox ID="CheckBoxIndv_ProductLeadTime" runat="server" OnCheckedChanged="CheckBoxIndv_CheckChanged"
                                                        Text='<%#Eval("ID") %>' CssClass="HiddenText" AutoPostBack="true" />
                                                </td>
                                                <td class="border-right" style="min-width: 147px;">
                                                    <span style="font-size: 12px">
                                                        <%#Eval("date") %></span>
                                                </td>
                                                <td class="border-right" style="width: 114px;">
                                                    <%#Eval("ctl1") %>
                                                </td>
                                                <td class="border-right" style="width: 115px;">
                                                    <%#Eval("ctl2") %>
                                                </td>
                                                <td class="border-right" style="width: 115px;">
                                                    <%#Eval("shearquart") %>
                                                </td>
                                                <td class="border-right" style="width: 115px;">
                                                    <%#Eval("shearhalf") %>
                                                </td>
                                                <td class="border-right" style="width: 115px;">
                                                    <%#Eval("lasertable") %>
                                                </td>
                                                <td class="border-right" style="width: 115px;">
                                                    <%#Eval("burntable") %>
                                                </td>
                                                <td style="min-width: 121px;">
                                                    <%#Eval("updatedby") %>
                                                </td>
                                            </tbody>
                                        </table>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </form>
    </div>
    <script type="text/javascript" src="prodleadtimes.js"></script>
</body>
</html>
