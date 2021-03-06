﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AppDownloadBtn.aspx.cs"
    Inherits="SiteTools_AppDownloadBtn" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>App Downloader</title>
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <style type="text/css">
        body
        {
            font-family: "Helvetica Neue",Helvetica,Arial,sans-serif;
            font-size: 13px;
        }

        .float-left
        {
            float: left;
        }

        .float-right
        {
            float: right;
        }

        .pad-right-sml
        {
            padding-right: 3px;
        }

        img
        {
            border: 0 solid rgba(0, 0, 0, 0);
        }

        *
        {
            margin: 0;
            padding: 0;
        }

        a, a:link, a:visited
        {
            text-decoration: none;
            color: #6884b7;
            cursor: pointer;
        }

        a:hover
        {
            text-decoration: underline;
        }

        .margin-right-sml
        {
            margin-right: 3px;
        }

        .clear-space-five
        {
            clear: both;
            height: 5px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" EnablePartialRendering="True"
            EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
        <asp:HiddenField ID="hf_appID" runat="server" />
        <asp:LinkButton ID="lb_downloadApp" runat="server" OnClick="lb_downloadApp_Click"
            PostBackUrl="~/SiteTools/iframes/AppDownloadBtn.aspx" CssClass="float-right margin-right-sml"></asp:LinkButton>
    </form>
</body>
</html>
