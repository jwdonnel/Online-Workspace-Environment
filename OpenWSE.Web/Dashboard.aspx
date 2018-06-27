<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Dashboard.aspx.cs" Inherits="Dashboard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta name="author" content="John Donnelly" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="mobile-web-app-capable" content="yes" />
    <meta name="revisit-after" content="10 days" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
</head>
<body onload="pageLoad();">
    <form id="form1" runat="server">
        <div>
            This page has moved. Navigating to correct page, one moment...
        <br />
            <br />
            If page does not load, <a href="Default.aspx">click here</a>.
        </div>
        <script type="text/javascript">
            setTimeout(function () {
                window.location.href = "Default.aspx";
            }, 1000);
        </script>
    </form>
</body>
</html>
