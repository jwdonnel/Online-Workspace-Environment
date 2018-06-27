<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ColdRolled.aspx.cs" Inherits="Integrated_Pages_StockList_ColdRolled" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Cold Rolled</title>
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <link id="Link4" rel="stylesheet" type="text/css" runat="server" href="~/App_Themes/Standard/StyleSheets/Main/main.css" />
    <link id="Link5" rel="stylesheet" type="text/css" runat="server" href="~/App_Themes/Standard/StyleSheets/Main/sitemaster.css" />
    <link id="Link3" rel="stylesheet" type="text/css" runat="server" href="~/CustomProjects/SteelOnline/StockList/StockListStyles.css" />
</head>
<body style="min-width: 200px!important; background-color: transparent!important;">
    <form id="form1" runat="server">
    <asp:HiddenField ID="hfSortBy" runat="server" Value="Type ASC, Grade ASC, Gauge ASC" />
    <div class="pageSize">
        <table id="productTable_Sheet" runat="server" cellpadding="0" cellspacing="0" style="width: 100%">
        </table>
        <div id="spacing" runat="server" class="clear" style="height: 40px">
        </div>
        <table id="productTable_Coil" runat="server" cellpadding="0" cellspacing="0" style="width: 100%">
        </table>
    </div>
    </form>
</body>
</html>
