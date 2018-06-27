<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ExternalAppHolder.aspx.cs"
    Inherits="WebControls_ExternalAppHolder" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>External App Viewer</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <style type="text/css">
        .main-div-app-bg, .iFrame-apps { -moz-border-radius: 0px !important; -webkit-border-radius: 0px !important; border-radius: 0px !important; }
        #main_container_external { overflow: auto; position: absolute; top: 0; bottom: 0; left: 0; right: 0; width: auto!important; height: auto!important; }
        .external-title-with-image img { float: left; height: 22px; margin-top: 8px; margin-right: 8px; }
    </style>
</head>
<body id="site_mainbody" runat="server">
    <!--[if lt IE 9]>
        <div class="lt-ie9-bg">
            <p class="browsehappy">You are using an <strong>outdated</strong> browser.</p>
            <p>Please <a href="http://browsehappy.com/">upgrade your browser</a> to improve your experience.</p>
        </div>
    <![endif]-->
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <div id="top_bar_iframe" runat="server" class="iframe-top-bar">
            <div id="iframe_title_logo" runat="server" class="iframe-title-logo"></div>
            <span class="iframe-title-top-bar">
                <asp:Label ID="lbl_appName" runat="server"></asp:Label>
            </span>
            <a onclick="return openWSE.CloseWindow();" class="close-iframe" title="Close"></a>
            <a onclick="window.location.href=window.location.href;" class="refresh-iframe" title="Refresh"></a>
            <a id="btn_ExternalOpen" runat="server" visible="false" onclick="OpenApp_External();return false;" class="openexternal-iframe" title="Pop app back into workspace"></a>
            <div class="clear"></div>
        </div>
        <asp:UpdatePanel ID="updatepnl_topbar" runat="server" ClientIDMode="Static" EnableViewState="False" ViewStateMode="Disabled">
            <ContentTemplate>
                <asp:HiddenField ID="hf_UpdateAll" runat="server" OnValueChanged="hf_UpdateAll_ValueChanged" />
            </ContentTemplate>
        </asp:UpdatePanel>
        <div id="div_loadingbackground_holder" data-usespinner="true" runat="server" class="loading-background-holder" style="z-index: 10000;"><div></div></div>
        <asp:PlaceHolder ID="PlaceHolder1" runat="server" EnableViewState="false" ViewStateMode="Disabled"></asp:PlaceHolder>
        <asp:HiddenField ID="hf_appId" runat="server" Value="" />
        <div class="clear"></div>
        <script type="text/javascript">
            $(document).ready(function () {
                openWSE_Config.workspaceMode = "simple";
                openWSE.LoadCSSFilesInApp($("#hf_appId").val());
                $(window).resize();
            });

            $(document.body).on("click", "#btn_refresh", function () {
                $(".loading-background-holder").show();
            });

            $(window).resize(function () {
                var alwaysVisibleHt = $("#top_bar_iframe").outerHeight();
                if ($("#top_bar_iframe").css("display") == "none") {
                    alwaysVisibleHt = 0;
                }

                $("#main_container_external").css("top", alwaysVisibleHt);
                if ($(".iFrame-apps").length > 0) {
                    $(".iFrame-apps").height($(".app-main-holder").outerHeight());
                }
            });

            var performingAction = false;
            function OpenApp_External() {
                if (!performingAction) {
                    $(".iframe-top-bar").find("a").hide();

                    $(".loading-background-holder").attr("data-usespinner", "true");
                    $(".loading-background-holder").css("background-image", "");
                    $(".loading-background-holder").show();

                    performingAction = true;
                    var windowMode = "normal";

                    var width = $(window).width();
                    var height = $(window).height();

                    if (screen.width == width) {
                        windowMode = "maximize";
                    }

                    var options = "0;" + windowMode + ";" + 0 + ";" + 0 + ";" + width + ";" + height + ";";

                    openWSE.AjaxCall("WebServices/AcctSettings.asmx/UpdateAppRemote", '{ "id": "' + $("#hf_appId").val() + '","options": "' + escape(options) + '" }', null, function (data) {
                        performingAction = false;
                        if (openWSE.ConvertBitToBoolean(data.d)) {
                            openWSE.CloseWindow();
                        }
                        else {
                            openWSE.AlertWindow("Could not load! Try again.");
                        }
                        $(".iframe-top-bar").find("a").show();
                        $(".loading-background-holder").hide();
                    }, function () {
                        performingAction = false;
                        openWSE.AlertWindow("Could not load! Try again.");
                        $(".iframe-top-bar").find("a").show();
                        $(".loading-background-holder").hide();
                    });
                }
            }
        </script>
    </form>
</body>
</html>
