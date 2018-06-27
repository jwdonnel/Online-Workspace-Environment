<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SchedulePrint.aspx.cs" Inherits="steel_online_auth_SchedulePrint" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Print Truck Schedule</title>
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <style type="text/css">
        body { color: Black; font-family: arial, sans-serif; font-size: .80em; width: 100%; }

        .GridDateHeader { -moz-box-shadow: -1px 1px 1px rgba(0,0,0,.1); -webkit-box-shadow: 0 2px 4px rgba(0,0,0,.1); box-shadow: 0 2px 4px rgba(0,0,0,.1); font-family: arial,helvetica; font-size: 14px; font-weight: 700; margin-bottom: 5px; text-align: center; width: 100%; -moz-border-radius: 3px; -webkit-border-radius: 3px; border-radius: 3px; }
        .GridDateHeader2 { -moz-box-shadow: -1px 1px 1px rgba(0,0,0,.1); -webkit-box-shadow: 0 2px 4px rgba(0,0,0,.1); border: 1px solid #CCC; box-shadow: 0 2px 4px rgba(0,0,0,.1); font-size: 14px; min-width: 680px; -moz-border-radius: 3px; -webkit-border-radius: 3px; border-radius: 3px; background: #EEE; border-bottom: 1px solid #AAA; border-top: 1px solid #AAA; text-align: center; width: 100%; }

        .clear { clear: both; }

        .clearSpace { clear: both; height: 15px; }

        .clearMargin { clear: both; padding: 5px 0; }

        .PadAll { padding: 10px; }

        .PadRight { padding-right: 10px; }

        .PadRightBig { padding-right: 20px; }

        .PadLeft { padding-left: 10px; }

        .PadLeftSml { padding-left: 7px; }

        .PadLeftBig { padding-left: 20px; }

        .PadRightSml { margin-right: 3px; }

        .MarginLeft { margin-left: 10px; }

        .MarginLeftSml { margin-left: 3px; }

        .MarginRight { margin-right: 10px; }

        .MarginRightSml { margin-right: 3px; }

        .MarginRightBig { margin-right: 20px; }

        .MarginLeftBig { margin-left: 20px; }

        .MarginTop { margin-top: 10px; }

        .MarginAll { margin: 10px; }

        .padTopBottom { padding: 5px 0; }

        .padBottom { padding-bottom: 5px; }

        .padTop { padding-top: 5px; }

        .imgNoPad { border: 0px; margin: 0 10px 0 0; padding: 0; }

        .floatleft { float: left; }

        .floatright { float: right; }

        .dot-BottomBorder { border-bottom: 1px dotted #B7B7B7; }

        .myItemStyle { width: 100%; }

            .myItemStyle td { padding: 7px; }

            .myItemStyle a { text-decoration: none; }

            .myItemStyle b { padding-right: 5px; }

            .myItemStyle span { line-height: 20px; padding: 0 10px 0 0; }

            .myItemStyle a:hover { text-decoration: none; }

        .signature-verify-holder { margin-top: 50px; margin-bottom: 10px; text-align: right; }
            .signature-verify-holder span { font-weight: bold; padding-right: 5px; }
    </style>
</head>
<body onload="window.print();">
    <form id="form1" runat="server" enctype="multipart/form-data" method="post">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True"
            EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
        <div class="clearSpace">
        </div>
        <div>
            <asp:Image ID="img_logo" runat="server" ImageUrl="~/Apps/SteelTrucks/logoPrint.png"
                CssClass="floatleft" Style="height: 90px; padding-left: 50px;" />
            <div style="float: right; padding: 28px 50px 0 0; margin-top: -10px;">
                <div style="text-align: center; font-size: 18px;">
                    <asp:Label ID="lbl_scheduleNumber" runat="server"></asp:Label></div>
                <div id="div_clearScheduleNumber" runat="server" class="clear" style="height: 10px;"></div>
                <b>****************** DRIVE SAFELY ******************</b>
                <div class="clear" style="height: 5px;">
                </div>
                <div style="background-color: #F4F4F4; border: 1px solid #555; padding: 4px 2px;">
                    <div class="PadLeft PadRight">
                        <b class="PadRight">TOTAL WEIGHT</b><asp:Label ID="lbl_smwtruckweight" runat="server"
                            Text=""></asp:Label>
                    </div>
                </div>
            </div>
            <div class="clearSpace">
            </div>
            <div class="clearSpace">
            </div>
            <div style="margin: 0 auto; width: 90%;">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <div id="pnl_schholder">
                            <div class="GridDateHeader2">
                                <div style="padding: 0.4em 0;">
                                    <table style="width: 100%;" cellpadding="3" cellspacing="3">
                                        <tbody>
                                            <tr>
                                                <td style="text-align: left; width: 25%;">
                                                    <b><u class="PadRight">Driver</u></b><div class="clear" style="height: 5px;">
                                                    </div>
                                                    <asp:Label ID="lbl_driver" runat="server" Text="" ForeColor="Black"></asp:Label>
                                                </td>
                                                <td style="text-align: left; width: 25%;">
                                                    <b><u class="PadRight">Date</u></b><div class="clear" style="height: 5px;">
                                                    </div>
                                                    <asp:Label ID="lbl_date" runat="server" Text="" ForeColor="Black"></asp:Label>
                                                </td>
                                                <td style="text-align: left; width: 25%;">
                                                    <b><u class="PadRight">Unit</u></b><div class="clear" style="height: 5px;">
                                                    </div>
                                                    <asp:Label ID="lbl_unit" runat="server" Text="" ForeColor="Black"></asp:Label>
                                                </td>
                                                <td style="text-align: left; width: 25%;">
                                                    <b><u class="PadRight">Unit</u></b><div class="clear" style="height: 5px;">
                                                    </div>
                                                    <asp:Label ID="lbl_gd" runat="server" Text="" ForeColor="Black"></asp:Label>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                            <div class="clearSpace">
                            </div>
                            <asp:GridView ID="GV_Schedule" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                                Width="100%" GridLines="Both" AllowPaging="false" ShowHeaderWhenEmpty="True"
                                BorderColor="#CCCCCC">
                                <EmptyDataRowStyle ForeColor="Black" />
                                <RowStyle CssClass="GridNormalRow" />
                                <AlternatingRowStyle CssClass="GridAlternate" />
                                <EmptyDataTemplate>
                                    <div class="emptyGridView">
                                        No Data Available
                                    </div>
                                </EmptyDataTemplate>
                                <Columns>
                                    <asp:TemplateField>
                                        <HeaderTemplate>
                                            <table class="myItemStyle" style="border-bottom: 1px solid #CCC; text-align: left;"
                                                cellpadding="0" cellspacing="0">
                                                <tbody>
                                                    <tr>
                                                        <td style="width: 31px;"></td>
                                                        <td style="width: 166px;">
                                                            <b>Customer Name</b>
                                                        </td>
                                                        <td>
                                                            <b>City</b>
                                                        </td>
                                                        <td style="width: 115px;">
                                                            <b>Order #</b>
                                                        </td>
                                                        <td style="width: 90px;">
                                                            <b>Weight (lbs)</b>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hf_id" runat="server" Value='<%#Eval("ID") %>' />
                                            <asp:HiddenField ID="hf_sequence" runat="server" Value='<%#Eval("Sequence") %>' />
                                            <table class="myItemStyle" cellpadding="0" cellspacing="0">
                                                <tr>
                                                    <td style="border-right: 1px solid #CCC; width: 30px;">
                                                        <b>
                                                            <%# string.Format("{0}", Container.DataItemIndex + 1 + ".") %></b>
                                                    </td>
                                                    <td style="border-right: 1px solid #CCC; width: 165px;">
                                                        <%#Eval("CustomerName") %>
                                                    </td>
                                                    <td style="border-right: 1px solid #CCC;">
                                                        <%#Eval("City") %>
                                                    </td>
                                                    <td style="border-right: 1px solid #CCC; width: 115px;">
                                                        <%#Eval("OrderNumber") %>
                                                    </td>
                                                    <td style="border-right: 1px solid #CCC; width: 89px;">
                                                        <%#Eval("Weight") %>
                                                    </td>
                                                </tr>
                                            </table>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div class="signature-verify-holder">
                    <span>Load securement verified by:</span>_______________________________________
                </div>
            </div>
        </div>
    </form>
</body>
</html>
