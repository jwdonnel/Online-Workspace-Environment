<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CTLSchedulePrint.aspx.cs"
    Inherits="Apps_CTLReport_CTLSchedulePrint" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Print CTL Daily Schedule</title>
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <style type="text/css">
        body
        {
            color: #353535;
            font-family: arial, sans-serif;
            font-size: .75em;
            width: 100%;
        }
        
        .clear
        {
            clear: both;
        }
        
        .clear-space
        {
            clear: both;
            height: 15px;
        }
        
        .table-cell-pad
        {
            border-right: 1px solid #555;
            text-align: center;
            vertical-align: middle;
        }
        
        
        /* GridView Properties
        ----------------------------------*/
        .GridViewNumRow
        {
            background-color: #F1F1F1;
            border-right: 1px solid #555;
            border-left: 1px solid #555;
            color: #333;
            text-align: center;
        }
        .GridNormalRow
        {
            background: #FFF;
            border-bottom: 1px solid #555;
        }
        .emptyGridView
        {
            color: #353535;
            margin-top: 5px;
            padding: 7px;
        }
        .myHeaderStyle
        {
            height: 35px;
            margin-bottom: 0px;
            text-align: center;
            width: 100%;
            border: 1px solid #555;
        }
        .myHeaderStyle td, .myHeaderStyle th
        {
            border-right: 1px solid #555;
            font-weight: 400;
            padding: 0 5px;
            vertical-align: middle;
            font-weight: bold;
        }
        .myItemStyle
        {
            width: 100%;
        }
        .myItemStyle b
        {
            padding-right: 5px;
        }
        .myItemStyle td
        {
            padding: 5px;
        }
        .myItemStyle .sb-links
        {
            opacity: 0.4;
            filter: alpha(opacity=40);
        }
        .myItemStyle:hover .sb-links
        {
            opacity: 1.0;
            filter: alpha(opacity=100);
        }
    </style>
</head>
<body onload=" window.print(); ">
    <form id="form1" runat="server" enctype="multipart/form-data" method="post">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True"
        EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
    <div class="clear-space">
    </div>
    <div>
        <asp:Image ID="img_logo" runat="server" ImageUrl="~/Standard_Images/logoPrint.png"
            CssClass="floatleft" Style="float: left; height: 70px;" />
        <div style="float: right; padding: 4px 15px; margin-top: 15px; margin-right: 10px; border: 1px solid #777;">
            <h2 style="padding: 0; margin: 0;">
                CTL DAILY SCHEDULE</h2>
        </div>
        <div class="clear-space">
        </div>
        <hr />
        <div class="clear">
        </div>
        <div style="float: left; padding: 0 15px; border: 1px solid #777;">
            <h3>
                <strong style="padding-right: 5px;">Date:</strong><asp:Label ID="lbl_date" runat="server"></asp:Label></h3>
        </div>
        <div class="clear-space">
        </div>
        <div style="margin: 0 auto; width: 97%;">
            <asp:Panel ID="pnl_schedule" runat="server">
            </asp:Panel>
        </div>
    </div>
    </form>
</body>
</html>
