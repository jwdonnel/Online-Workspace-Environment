<%@ page language="C#" autoeventwireup="true" inherits="DatabaseSelection, App_Web_etymirm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Database Selection</title>
    <meta name="viewport" content="width=device-width, user-scalable=no" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="Standard_Images/favicon.ico"
        type="image/ico" />
    <style type="text/css">
        .container
        {
            font-family: Arial;
            font-size: 15px;
            height: 420px;
            left: 50%;
            margin-left: -440px;
            margin-top: -220px;
            padding: 15px 15px 5px 15px;
            position: fixed;
            text-align: center;
            text-shadow: 0 1px rgba(0, 0, 0, 0.15);
            top: 50%;
            width: 850px;
            color: #FFF;
            background: rgba(0,0,0,0.7);
            -webkit-border-radius: 40px;
            -moz-border-radius: 40px;
            border-radius: 40px;
            -moz-box-shadow: 5px 5px 10px #101010;
            -webkit-box-shadow: 5px 5px 10px #101010;
            box-shadow: 5px 5px 10px #101010;
            overflow: auto;
        }

        a
        {
            color: #EFEFEF;
            cursor: pointer;
            padding: 3px;
            text-decoration: underline;
            transition: all .2s ease-in-out;
        }

        body
        {
            font-size: .80em;
            font-family: Arial,Helvetica,sans-serif;
            color: #555555;
            -webkit-font-smoothing: antialiased;
            font-smooth: always;
            overflow: hidden;
            margin: 0!important;
        }

        .body-bg
        {
            position: fixed;
            left: 0;
            right: 0;
            top: 0;
            bottom: 0;
            z-index: 0;
            background-image: url('App_Themes/Standard/Body/default-bg.jpg');
            background-repeat: no-repeat;
            background-position: bottom center;
            background-attachment: fixed;
            background-color: #2D2D2D;
        }

        h3
        {
            font-size: 1.2em;
            color: #555656;
            font-weight: 200;
            margin-top: 3px;
            margin-bottom: 0;
            text-transform: none;
        }

        .Modal-overlay .input-buttons
        {
            background: #4D90FE;
            border: 1px solid #3079ED;
            font-weight: bold;
            color: #FFF;
        }

            .Modal-overlay .input-buttons:hover
            {
                background: #357AE8;
                border: 1px solid #2F5BB7;
            }

            .Modal-overlay .input-buttons:active
            {
                background: #2E70C1;
                border: 1px solid #2B53A5;
            }

        .logo
        {
            padding-top: 10px;
            max-height: 45px;
        }

        #update-element
        {
            display: none;
        }

        .update-element-overlay
        {
            height: 100%;
            left: 0;
            position: fixed;
            text-align: center;
            top: 0;
            width: 100%;
            z-index: 12000;
        }

        .update-element-modal
        {
            display: block;
            text-align: center;
            position: fixed;
            background-color: rgba(15,15,15,0.8);
            padding: 20px 40px 17px 40px;
            left: 50%;
            top: 50%;
            min-height: 20px;
            margin-left: -50px;
            margin-top: -50px;
            -moz-box-shadow: -3px 3px 3px rgba(0,0,0,.25);
            -webkit-box-shadow: 0 5px 7px rgba(0,0,0,.25);
            box-shadow: 0 5px 7px rgba(0,0,0,.25);
            z-index: 12000;
            -moz-border-radius: 5px;
            -webkit-border-radius: 5px;
            border-radius: 5px;
        }

            .update-element-modal .loading-icon-sml
            {
                margin-top: -3px;
            }

            .update-element-modal h3
            {
                color: #FFF;
                font-weight: bold;
                float: left;
                margin-top: 1px;
                margin-left: 10px;
                font-size: 13px;
            }

            .update-element-modal .progress
            {
                width: 15px;
                padding-left: 2px;
                text-align: left;
            }

        .update-element-align
        {
            position: fixed;
            text-align: left;
            z-index: 5000;
        }

        .loading-icon-sml
        {
            width: 22px;
            height: 22px;
            float: left;
            background-image: url(App_Themes/Standard/Body/loading.gif);
            background-repeat: no-repeat;
            background-position: center center;
            display: inline-block;
            zoom: 1;
            *display: inline;
        }

        .inline-block
        {
            display: inline-block;
        }

        .trial-version-text
        {
            padding: 7px 10px;
            background: rgba(0,0,0,0.5);
            color: white;
            font-weight: bold;
            position: absolute;
            z-index: 1;
            left: 0;
            top: 0;
            right: 0;
            text-align: left;
        }

        .float-right
        {
            float: right;
        }

        #ConfirmWindow-element .Modal-element-modal
        {
            min-height: 115px;
            min-width: 370px;
            max-width: 370px;
            z-index: 20000;
        }

        #ConfirmWindow-element .message-text
        {
            color: #444;
            clear: both;
            word-wrap: normal;
            padding-bottom: 10px;
        }

        #ConfirmWindow-element img
        {
            float: left;
            height: 28px;
            margin-right: 10px;
            margin-bottom: 5px;
        }

        #ConfirmWindow-element .button-holder
        {
            clear: both;
            padding-top: 5px;
            padding-bottom: 5px;
            text-align: right;
        }

        #ConfirmWindow-element .confirm-cancel-button
        {
            margin-right: 0px;
        }

        .ModalPadContent
        {
            padding: 15px 25px 10px 25px;
        }

        .ModalHeader
        {
            display: block;
            background: url(App_Themes/Standard/Body/app-header.png) repeat-x top;
            border-bottom: 1px solid #CCC;
        }

        .ModalHeader
        {
            -moz-border-top-right-radius: 5px;
            -webkit-border-top-right-radius: 5px;
            border-top-right-radius: 5px;
            -moz-border-top-left-radius: 5px;
            -webkit-border-top-left-radius: 5px;
            border-top-left-radius: 5px;
        }

            .ModalHeader div
            {
                cursor: move;
                padding: 7px 15px 5px 15px;
                color: #5F5F5F;
                font-weight: bold;
            }

        .Modal-element
        {
            display: none;
            visibility: hidden;
        }

        .Modal-element-modal
        {
            background: #F9F9F9;
            max-width: 650px;
            min-width: 500px;
            border: 1px solid #B7B7B7;
            border-right: 1px solid #CCC;
            border-top: 1px solid #DDD;
            -moz-box-shadow: 0 5px 10px rgba(0,0,0,.4);
            -webkit-box-shadow: 0 5px 10px rgba(0,0,0,.4);
            box-shadow: 0 5px 10px rgba(0,0,0,.4);
            margin: 0 auto;
            z-index: 10000;
            -moz-border-top-left-radius: 7px;
            -webkit-border-top-left-radius: 7px;
            border-top-left-radius: 7px;
            -moz-border-top-right-radius: 7px;
            -webkit-border-top-right-radius: 7px;
            border-top-right-radius: 7px;
        }

        .Modal-element-align
        {
            position: fixed;
            text-align: left;
            z-index: 10000;
            top: 50%;
            left: 50%;
        }

        .Modal-overlay
        {
            height: 100%;
            left: 0;
            position: fixed;
            text-align: center;
            top: 0;
            width: 100%;
            z-index: 10000;
        }

        .ModalExitButton
        {
            margin-top: 2px;
            margin-right: -12px;
            float: right;
            height: 12px;
            width: 22px;
            background: url(App_Themes/Standard/App/headerbar-btns.png) no-repeat -44px 0;
            overflow: hidden;
        }

            .ModalExitButton:hover
            {
                background: url(App_Themes/Standard/App/headerbar-btns.png) no-repeat -44px -13px;
            }

        .app-head-button-holder-admin
        {
            float: right;
            height: 16px;
            margin-top: -7px;
            margin-right: -10px;
            width: 2px;
        }

        input[type="text"], input[type="password"], textarea
        {
            -moz-border-radius: 5px;
            -webkit-border-radius: 5px;
            border-radius: 5px;
        }

        .input-buttons
        {
            color: #333;
            background: #F8F8F8;
            border: 1px solid #D3D3D3;
            -moz-border-radius: 4px;
            -webkit-border-radius: 4px;
            border-radius: 4px;
            text-decoration: none!important;
            cursor: pointer;
            text-align: center;
            padding: 5px 9px;
            font-size: 12px;
            line-height: normal;
            margin-right: 16px;
            -moz-transition: all .2s ease-in-out;
            -webkit-transition: all .2s ease-in-out;
            transition: all .2s ease-in-out;
        }

            .input-buttons:hover
            {
                background: #EFEFEF;
                cursor: pointer;
            }

            .input-buttons:active
            {
                background: #E0E0E0;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div class="body-bg">
        </div>
        <div class="container">
            <asp:UpdatePanel ID="updatePnl1" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="pnl1" runat="server">
                        <h2><b><u>Select a Database</u></b></h2>
                        <div style="clear: both; height: 15px;">
                        </div>
                        <table border="0" cellpadding="20" cellspacing="20" width="100%">
                            <tbody>
                                <tr>
                                    <td valign="top" width="50%">
                                        <b>SQL Server Compact 4</b>
                                        <br />
                                        <br />
                                        A small and simple database, lightweight installation, connecting to a database file. SQL Server Compact 4 features an easier setup compared to SQL Server Express. This should work on just about any system without any setup.
                                    </td>
                                    <td valign="top">
                                        <b>SQL Server Express 2008+</b>
                                        <br />
                                        <br />
                                        A larger and more complex database that allows for larger data and Table Views. SQL Server Express should only be used if the server running this site has SQL Server Express 2008 or higher.
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <a href="#" onclick="ConfirmCompact();return false;">Create SQL Server Compact 4 Database</a>
                                        <asp:HiddenField ID="lbtn_compact" runat="server" OnValueChanged="lbtn_compact_Click" />
                                    </td>
                                    <td>
                                        <asp:LinkButton ID="lbtn_expressContinue" runat="server" OnClick="lbtn_expressContinue_Click" Text="Create SQL Server Express Database"></asp:LinkButton>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </asp:Panel>
                    <asp:Panel ID="pnl2" runat="server" Enabled="false" Visible="false">
                        <h2><b><u>Setup SQL Express Database</u></b></h2>
                        <div style="clear: both; height: 15px;">
                        </div>
                        In order to create/connect to the SQL Express Server, you need to enter in some information.
                        <table border="0" cellpadding="10" cellspacing="10" width="100%">
                            <tbody>
                                <tr>
                                    <td valign="top" width="50%">
                                        <b>Connection String</b> -
                                        <asp:LinkButton ID="lbtn_useDefaultConnectionString" runat="server" Text="Use Default" Font-Size="X-Small" OnClick="lbtn_useDefaultConnectionString_Click" Style="margin-right: 5px;"></asp:LinkButton>
                                        <asp:LinkButton ID="lbtn_clearConnectionString" runat="server" Text="Clear" Font-Size="X-Small" OnClick="lbtn_clearConnectionString_Click"></asp:LinkButton>
                                        <br />
                                        <br />
                                        <asp:TextBox ID="txt_connectionstring" runat="server" Width="700px" Height="90px" Font-Names="Arial" Font-Size="14px" BorderStyle="Solid" BorderColor="#CCCCCC" BorderWidth="1px" TextMode="MultiLine" Style="padding: 3px;"></asp:TextBox>
                                        <br />
                                        <asp:Label ID="lbl_errorMessage" runat="server" ForeColor="Red" Text="Connection string cannot be blank" Enabled="false" Visible="false"></asp:Label>
                                        <div style="clear: both; height: 15px;">
                                        </div>
                                        <a href="#" onclick="ConfirmExpress();return false;">Create SQL Server Express Database</a>
                                        <asp:HiddenField ID="lbtn_express" runat="server" OnValueChanged="lbtn_express_Click" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <asp:LinkButton ID="lbtn_Cancel" runat="server" Text="<- Go Back" OnClick="lbtn_Cancel_Click"></asp:LinkButton>
                        <div style="clear: both; height: 15px;">
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <img alt="logo" src="Standard_Images/About Logos/openwse_alt.png" class="logo" />
        </div>
        <script type="text/javascript" src="//code.jquery.com/jquery-1.11.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/ui/1.11.1/jquery-ui.min.js"></script>
        <script type="text/javascript">
            Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function (sender, args) {
                LoadingMessage1("One Moment...");
            });

            $(document).ready(function () {
                $("body").css("height", $(window).height());
            });
            $(window).resize(function () {
                $("body").css("height", $(window).height());
            });

            var intervalCount = 0;
            var messageLoadInterval;
            function LoadingMessage1(message) {
                RemoveUpdateModal();

                intervalCount = 0;
                if (message.indexOf("...") != -1) {
                    message = message.replace("...", "");
                }

                message = message + "<span class='progress inline-block'></span>";
                var x = "<div id='update-element'><div class='update-element-overlay'><div class='update-element-align'>";
                x += "<div class='update-element-modal'><h3 class='inline-block'>" + message + "</h3></div></div></div></div>";
                $("body").append(x);

                StartMessageTickInterval(message, $("#update-element").find(".progress")[0]);
                $("#update-element").show();

                var $modalWindow = $("#update-element").find(".update-element-modal");
                var currUpdateWidth = -($modalWindow.outerWidth() / 2);
                var currUpdateHeight = -($modalWindow.outerHeight() / 2);
                $modalWindow.css({
                    marginLeft: currUpdateWidth,
                    marginTop: currUpdateHeight
                });
            }
            function StartMessageTickInterval(message, elem) {
                messageLoadInterval = setInterval(function () {
                    var messageWithTrail = "";
                    switch (intervalCount) {
                        case 0:
                            messageWithTrail = ".";
                            intervalCount++;
                            break;
                        case 1:
                            messageWithTrail = "..";
                            intervalCount++;
                            break;
                        case 2:
                            messageWithTrail = "...";
                            intervalCount++;
                            break;
                        default:
                            messageWithTrail = "";
                            intervalCount = 0;
                            break;
                    }
                    $(elem).html(messageWithTrail);
                }, 400);
            }
            function RemoveUpdateModal() {
                if (messageLoadInterval != null) {
                    clearInterval(messageLoadInterval);
                }
                $("#update-element").remove();
            }

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                RemoveUpdateModal();
            });

            /* Set Trial Text */
            function SetTrialText(exp) {
                var text = "<div class='trial-version-text'><span>Trial Version</span><span class='float-right'>Expires in " + exp + "</span></div>";
                $("body").prepend(text);
            }

            function ConfirmCompact() {
                ConfirmWindow("Are you sure you want to create an SQL Server Compact 4 Database? You will not be able to change this later on.", function () {
                    $("#lbtn_compact").val(new Date().toString());
                    __doPostBack("lbtn_compact", "");
                }, null);
            }

            function ConfirmExpress() {
                ConfirmWindow("Are you sure you want to create an SQL Server Express Database? You will not be able to change this later on.", function () {
                    $("#lbtn_express").val(new Date().toString());
                    __doPostBack("lbtn_express", "");
                }, null);
            }

            /* Confirm Window */
            function ConfirmWindow(message, okCallback, cancelCallback) {
                if (message == "" || message == null) {
                    message = "Are you sure you want to continue?";
                }

                var ele = "<div id='ConfirmWindow-element' class='Modal-element' style='display: none;'>";
                ele += "<div class='Modal-overlay'>";
                ele += "<div class='Modal-element-align'>";
                ele += "<div class='Modal-element-modal'>";

                // Header
                ele += "<div class='ModalHeader'><div>";
                ele += "<span class='Modal-title'></span></div></div>";

                // Body
                var okButton = "<input class='input-buttons confirm-ok-button' type='button' value='Ok' style='width: 60px;' />";
                var cancelButton = "<input class='input-buttons confirm-cancel-button' type='button' value='Cancel' />";

                var img = "<img alt='confirm' src='App_Themes/Standard/Icons/confirm.png' />";
                ele += "<div class='ModalPadContent'><div class='message-text'>" + img + message + "</div><div class='button-holder'>" + okButton + cancelButton + "</div></div></div>";
                ele += "</div></div></div></div>";

                $("body").append(ele);

                $("#ConfirmWindow-element").find(".confirm-ok-button").one("click", function () {
                    CloseConfirmWindow();
                    if (okCallback != null) {
                        okCallback();
                    }
                });
                $("#ConfirmWindow-element").find(".confirm-cancel-button, .confirm-cancel-button-header").one("click", function () {
                    CloseConfirmWindow();
                    if (cancelCallback != null) {
                        cancelCallback();
                    }
                });

                LoadModalWindow(true, "ConfirmWindow-element", "Confirmation");
                $("#ConfirmWindow-element").find(".confirm-ok-button").focus();
            }
            function CloseConfirmWindow() {
                if ($("#ConfirmWindow-element").length > 0) {
                    $("#ConfirmWindow-element").hide();
                    $("#ConfirmWindow-element").remove();
                }
            }
            function LoadModalWindow(open, element, title) {
                var $thisElement = $("#" + element);
                if (open) {
                    $thisElement.show();

                    var $modalElement = $thisElement.find(".Modal-element-modal");
                    if ($modalElement.outerWidth() > $(window).width()) {
                        $modalElement.css({
                            minWidth: 50,
                            width: $(window).width()
                        });
                        $modalElement.find(".ModalPadContent").css("overflow", "auto");
                    }

                    $thisElement.find(".Modal-element-align").css({
                        marginTop: -($thisElement.find(".Modal-element-modal").height() / 2),
                        marginLeft: -($thisElement.find(".Modal-element-modal").width() / 2)
                    });

                    var container = "#container";
                    if (($("#container").length == 0) || (($thisElement.hasClass("outside-main-app-div")) && ($(".workspace-holder").length > 0))) {
                        if ($("#maincontent_overflow").length > 0) {
                            container = "#maincontent_overflow";
                        }
                        else {
                            container = "body";
                        }
                    }

                    $modalElement.draggable({
                        containment: container,
                        cancel: '.ModalPadContent, .ModalExitButton, #MainContent_pwreset_overlay',
                        drag: function (event, ui) {
                            var $this = $(this);
                            $this.css("opacity", "0.6");
                            $this.css("filter", "alpha(opacity=60)");

                            // Apply an overlay over app
                            // This fixes the issues when dragging iframes
                            if ($this.find("iframe").length > 0) {
                                var $_id = $this.find(".ModalPadContent");
                                $wo = $_id.find(".app-overlay-fix");
                                if ($wo.length == 0) {
                                    if ($_id.length == 1) {
                                        $_id.append("<div class='app-overlay-fix'></div>");
                                    }
                                }
                            }
                        },
                        stop: function (event, ui) {
                            var $this = $(this);
                            $this.css("opacity", "1.0");
                            $this.css("filter", "alpha(opacity=100)");
                            $wo = $(this).find(".app-overlay-fix");
                            if ($wo.length == 1) {
                                $wo.remove();
                            }
                        }
                    });

                    if (title != "") {
                        $thisElement.find(".Modal-title").html(title);
                    }

                    $thisElement.css("visibility", "visible");
                }
                else {
                    $thisElement.hide();
                    $thisElement.css("visibility", "hidden");
                    $thisElement.find(".Modal-title").html("");
                    $thisElement.find(".Modal-element-modal").find(".ModalPadContent").css("overflow", "");
                }
            }
        </script>
    </form>
</body>
</html>
