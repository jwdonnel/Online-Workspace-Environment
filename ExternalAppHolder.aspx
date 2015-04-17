<%@ page language="C#" autoeventwireup="true" inherits="WebControls_ExternalAppHolder, App_Web_2ux1dlqd" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>External App Viewer</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <style type="text/css">
        .main-div-app-bg
        {
            -moz-border-radius: 0px !important;
            -webkit-border-radius: 0px !important;
            border-radius: 0px !important;
        }

        #container
        {
            overflow: auto;
        }

        .external-title
        {
            font-size: 15px;
            color: #FFF;
            padding: 7px 10px;
            float: left;
        }

        .iFrame-apps
        {
            -moz-border-radius: 0px !important;
            -webkit-border-radius: 0px !important;
            border-radius: 0px !important;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout="360000">
        </asp:ScriptManager>
        <div id="always-visible">
            <div id="top-main-bar-top">
                <asp:UpdatePanel ID="updatepnl_topbar" runat="server" ClientIDMode="Static" EnableViewState="False" ViewStateMode="Disabled">
                    <ContentTemplate>
                        <div id="workspace-selector">
                        </div>
                        <asp:Label ID="lbl_appName" runat="server" CssClass="external-title"></asp:Label>
                        <table class="top-options" cellpadding="0" cellspacing="0">
                            <tbody>
                                <tr>
                                    <td id="btn_ExternalOpen" runat="server" visible="false">
                                        <li class="a" onclick="OpenApp_External();return false;">
                                            <a href="#" class="img-popin margin-right margin-left" title="Place on workspace" onclick="OpenApp_External();return false;"></a>
                                        </li>
                                    </td>
                                    <td>
                                        <li class="a" onclick="window.location.href=window.location.href;">
                                            <a href="#" class="img-refresh-alt margin-right margin-left" title="Refresh app" onclick="window.location.href=window.location.href;"></a>
                                        </li>
                                    </td>
                                    <td>
                                        <li class="a" onclick="window.close();return false;">
                                            <a href="#" class="img-close-alt margin-right margin-left" title="Close" onclick="window.close();return false;"></a>
                                        </li>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <asp:HiddenField ID="hf_UpdateAll" runat="server" OnValueChanged="hf_UpdateAll_ValueChanged" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="loading-background-holder" style="z-index: 10000;"></div>
        <asp:PlaceHolder ID="PlaceHolder1" runat="server" EnableViewState="false" ViewStateMode="Disabled"></asp:PlaceHolder>
        <asp:HiddenField ID="hf_appId" runat="server" Value="" />
        <script type="text/javascript">
            $(document).ready(function () {
                openWSE_Config.workspaceMode = "simple";
            });

            $(document.body).on("click", "#btn_refresh", function () {
                $(".loading-background-holder").show();
            });

            $(window).resize(function () {
                var alwaysVisibleHt = $("#always-visible").outerHeight();
                if ($("#always-visible").css("display") == "none") {
                    alwaysVisibleHt = 0;
                }

                $(".app-main-holder").css({
                    width: $(window).width(),
                    height: $(window).height() - alwaysVisibleHt
                });

                if ($(".iFrame-apps").length > 0) {
                    $(".iFrame-apps").height($(".app-main-holder").outerHeight());
                }
            });

            var performingAction = false;
            function OpenApp_External() {
                if (!performingAction) {
                    $(".top-options").hide();
                    $(".loading-background-holder").show();
                    performingAction = true;
                    var windowMode = "normal";

                    var width = $(window).width();
                    var height = $(window).height();

                    if (screen.width == width) {
                        windowMode = "maximize";
                    }

                    var options = "0;" + windowMode + ";" + 0 + ";" + 0 + ";" + width + ";" + height + ";";

                    $.ajax({
                        url: "WebServices/AcctSettings.asmx/UpdateAppRemote",
                        type: "POST",
                        data: '{ "id": "' + $("#hf_appId").val() + '","options": "' + escape(options) + '" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            performingAction = false;
                            if (openWSE.ConvertBitToBoolean(data.d)) {
                                window.close();
                            }
                            else {
                                openWSE.AlertWindow("Could not load! Try again.");
                            }
                            $(".top-options").show();
                            $(".loading-background-holder").hide();
                        },
                        error: function () {
                            performingAction = false;
                            openWSE.AlertWindow("Could not load! Try again.");
                            $(".top-options").show();
                            $(".loading-background-holder").hide();
                        }
                    });
                }
            }
        </script>
    </form>
</body>
</html>
