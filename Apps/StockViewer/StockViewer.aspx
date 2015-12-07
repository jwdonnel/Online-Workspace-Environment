<%@ page language="C#" autoeventwireup="true" inherits="Apps_StockViewer_StockViewer, App_Web_y0innsij" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Stock Viewer</title>
    <style type="text/css">
        *
        {
            padding: 0;
            margin: 0;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- TradingView App BEGIN -->
        <script type="text/javascript" src="https://s3.amazonaws.com/tradingview/tv.js"></script>
        <script type="text/javascript">
            new TradingView.widget({
                "width": 986,
                "height": 500,
                "symbol": "GOOG",
                "interval": "D",
                "toolbar_bg": "#E4E8EB",
                "allow_symbol_change": true
            });
        </script>
        <!-- TradingView App END -->
    </form>
</body>
</html>
