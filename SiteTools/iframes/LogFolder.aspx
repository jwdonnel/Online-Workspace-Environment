<%@ page language="C#" autoeventwireup="true" inherits="SiteTools_iframes_LogFolder, App_Web_e0mbdkse" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Log Folder Contents</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="../../Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="../../Standard_Images/favicon.ico"
        type="image/ico" />
    <link rel="stylesheet" href="../../App_Themes/Standard/site_desktop.css" />
    <link rel="stylesheet" href="../../App_Themes/Standard/jqueryUI.css" />
    <style type="text/css">
        .main-section
        {
            display: block;
            padding: 0 25px;
        }

        .header-section
        {
            padding-bottom: 5px;
            clear: both;
            margin-bottom: 20px;
        }

            .header-section .hint
            {
                clear: both;
                padding-bottom: 10px;
                font-size: 12px;
                font-style: italic;
            }

            .header-section .file-count
            {
                float: right;
                color: #555;
                font-size: 12px;
            }

        .file-section ol
        {
            margin-left: 20px;
        }

        .file-section li
        {
            padding: 5px 0;
            margin-bottom: 5px;
        }

            .file-section li > a
            {
                display: none;
            }

            .file-section li:hover > a
            {
                display: inline;
            }

        .file-section span
        {
            cursor: pointer;
        }

            .file-section span:hover
            {
                text-decoration: underline;
            }

        .file-content
        {
            padding: 15px;
            border: 1px solid #DDD;
            background: #F9F9F9;
            -moz-border-radius: 5px;
            -webkit-border-radius: 5px;
            border-radius: 5px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" EnablePartialRendering="True"
            EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
        <div id="app_title_bg" runat="server" class="app-Settings-title-bg-color-main">
            <div class="pad-all">
                <div class="app-Settings-title-user-info">
                    <div class="float-left">
                        <asp:Label ID="Label1" runat="server" CssClass="page-title" Text="Logging Folder"></asp:Label>
                    </div>
                </div>
            </div>
        </div>
        <div class="clear" style="height: 25px;">
        </div>
        <div class="main-section">
            <div class="header-section"><span class="hint">Click on a log file below to view the contents</span><span class="file-count"></span></div>
            <asp:UpdatePanel ID="updatepnl_FileContent" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnl_fileSection" runat="server" CssClass="file-section">
                    </asp:Panel>
                    <a href="#back" id="btn_Close" onclick="CloseFile();return false;" style="display: none;">Close</a>
                    <div class="clear-space"></div>
                    <asp:Panel ID="pnl_fileContent" runat="server" CssClass="file-content" Style="display: none;">
                    </asp:Panel>
                    <asp:HiddenField ID="hf_DeleteFile" runat="server" OnValueChanged="hf_DeleteFile_ValueChanged" />
                    <asp:HiddenField ID="hf_FileContent" runat="server" OnValueChanged="hf_FileContent_ValueChanged" />
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="hf_DeleteFile" />
                    <asp:AsyncPostBackTrigger ControlID="hf_FileContent" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
        <script type="text/javascript" src="//code.jquery.com/jquery-1.11.2.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/ui/1.11.3/jquery-ui.min.js"></script>
        <script type="text/javascript">
            function OnFileClick(file) {
                LoadingMessage1("Loading File...");
                $("#hf_FileContent").val(file);
                __doPostBack("hf_FileContent", "");
            }

            function FinishFileLoad() {
                $("#pnl_fileSection").hide();
                $("#pnl_fileContent, #btn_Close").show();
                RemoveUpdateModal();
            }

            function DeleteFile(file) {
                LoadingMessage1("Deleting File...");
                $("#hf_DeleteFile").val(file);
                __doPostBack("hf_DeleteFile", "");
            }

            function CloseFile() {
                $("#pnl_fileContent, #btn_Close").hide();
                $("#pnl_fileContent").html("");
                $("#pnl_fileSection").show();
            }

            /* Loading And Updating Modals */
            var intervalCount = 0;
            var messageLoadInterval;
            function LoadingMessage1(message) {
                RemoveUpdateModal();

                if (message.indexOf("...") != -1) {
                    message = message.replace("...", "");
                }

                message = message + "<span class='progress inline-block'></span>";

                var x = "<div id='update-element'><div class='update-element-overlay'><div class='update-element-align'>";
                x += "<div class='update-element-modal'><h3 class='inline-block'>" + message + "</h3></div></div></div></div>";
                $("body").append(x);

                StartMessageTickInterval($("#update-element").find(".progress")[0]);
                $("#update-element").show();

                var $modalWindow = $("#update-element").find(".update-element-modal");
                var currUpdateWidth = -($modalWindow.outerWidth() / 2);
                var currUpdateHeight = -($modalWindow.outerHeight() / 2);
                $modalWindow.css({
                    marginLeft: currUpdateWidth,
                    marginTop: currUpdateHeight
                });
            }
            function StartMessageTickInterval(elem) {
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
                intervalCount = 0;
                if (messageLoadInterval != null) {
                    clearInterval(messageLoadInterval);
                }
                $("#update-element").remove();
            }
        </script>
    </form>
</body>
</html>
