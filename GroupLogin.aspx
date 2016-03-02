<%@ Page Title="Group Login" Language="C#" AutoEventWireup="true" CodeFile="GroupLogin.aspx.cs" Inherits="GroupLogin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta name="author" content="John Donnelly" />
    <meta name="revisit-after" content="10 days" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta name="viewport" content="user-scalable = yes" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <link rel="stylesheet" href="App_Themes/Standard/jqueryUI.css" />
    <style type="text/css">
        .group-selection-entry
        {
            clear: none!important;
            float: left;
            width: 48%;
            margin-right: 5px;
            margin-left: 5px;
        }

        #lbl_UserName
        {
            cursor: default!important;
        }

            #lbl_UserName:hover, #lbl_UserName:active
            {
                background-color: inherit!important;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True" AsyncPostBackTimeout="360000" />
        <div id="always-visible">
            <div id="workspace-selector">
                <a id="lnk_BackToWorkspace" href="#">Close</a>
                <asp:LinkButton ID="lbtn_Logoff" runat="server" Text="Log Off" ToolTip="Return to login page" OnClick="lbtn_Logoff_Click"></asp:LinkButton>
            </div>
            <asp:Label ID="lbl_UserName" runat="server" CssClass="username-top-info float-right"></asp:Label>
        </div>
        <div id="container">
            <div class="pad-left-big pad-right-big">
                <div class="table-settings-box no-border">
                    <div class="td-settings-title">Select a Group</div>
                    <div class="td-settings-ctrl">
                        <div id="group-list"></div>
                    </div>
                    <div class="td-settings-desc">
                        Each group has its own apps and default settings that cannot be modified. Once logged in, all default settings for that group will be applied to your account. These settings will be removed once you log out of the group. Your Apps, Overlays, and Notifications will be overridden with the group's settings.
                    </div>
                </div>
            </div>
        </div>
        <div id="container-footer" class="footer">
            <div class="footer-padding">
                <div id="copyright-footer" class="float-left">&copy; 2016 OpenWSE</div>
                <div id="footer-signdate" class="float-right">
                    <a href="AppRemote.aspx" title="Open Mobile Workspace">Mobile</a> | 
                    <a href="#" onclick="OpeniframePage('About.aspx?iframeName=About&iframeFullScreen=false');return false;">About</a> | 
                    <a href="#iframecontent" onclick="openWSE.LoadIFrameContent('About.aspx?a=termsofuse', this);return false;">Terms</a>
                </div>
            </div>
        </div>
        <script type="text/javascript" src="//code.jquery.com/jquery-1.11.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/ui/1.11.1/jquery-ui.min.js"></script>
        <script type="text/javascript" src="Scripts/jquery/combined-scripts.min.js"></script>
        <script type="text/javascript" src="WebControls/jscolor/jscolor.js"></script>
        <script type="text/javascript">
            var siteName = "";
            var animationSpeed = 150;

            $(function () {
                $(window).hashchange(function () {
                    HashChange();
                });
            });

            $(window).resize(function () {
                UpdateContainerHeight();

                if ($("#iframe-content-src").length > 0) {
                    var fullScreen = GetUrlParameterByName("iframeFullScreen");
                    var iframeHeight = $(window).height();

                    if (fullScreen == "false") {
                        iframeHeight -= ($("#always-visible").height() + $("#container-footer").height());
                    }
                    $("#iframe-content-src").css("height", iframeHeight);
                }

            });

            $(document).ready(function () {
                UpdateContainerHeight();
                GroupLoginModal();
                HashChange();
            });

            function UpdateContainerHeight() {
                var ht = $(window).height() - ($("#always-visible").outerHeight() + $("#container-footer").outerHeight());
                $("#container").css("height", ht);
            }

            function OpeniframePage(url) {
                CloseIFrameContent();
                var fullUrl = "";
                var tempUrl = window.location.hash;
                if ((tempUrl.indexOf("#") == -1) && (window.location.href.charAt(window.location.href.length - 1) != "#")) {
                    fullUrl = "#?";
                }
                else if (tempUrl != "") {
                    fullUrl = "&";
                }
                else {
                    fullUrl = "?";
                }

                fullUrl += "iframecontent=" + url; // + "&contentName=" + $.trim($(_this).text());

                window.location += fullUrl;
            }
            function CloseIFrameContent() {
                $("#iframe-container-helper").remove();
                $("#container").show();

                try {
                    var url = window.location.href;
                    if (url.indexOf("?iframecontent=") != -1) {
                        var loc = url.split("?iframecontent=");
                        if (loc.length > 1) {
                            var fullLoc = "?iframecontent=" + loc[1];
                            url = url.replace(fullLoc, "");
                            window.location = url;
                        }
                    }
                    else if (url.indexOf("&iframecontent=") != -1) {
                        var loc = url.split("&iframecontent=");
                        if (loc.length > 1) {
                            var fullLoc = "&iframecontent=" + loc[1];
                            url = url.replace(fullLoc, "");
                            window.location = url;
                        }
                    }
                }
                catch (evt) { }
            }
            function FinishIframeLoad(url, name, fullScreen) {
                var iframeHeight = $(window).height();
                var topPos = 0;
                var bottomPos = 0;

                if (fullScreen == "false") {
                    iframeHeight -= ($("#always-visible").height() + $("#container-footer").height());
                    topPos = $("#always-visible").height();
                    bottomPos = $("#container-footer").height();
                    $("#lbtn_Logoff").hide();
                    $("#lnk_BackToWorkspace").show();
                }

                var iframe = "<iframe id='iframe-content-src' src='" + url + "' width='100%' frameborder='0' style='height: " + iframeHeight + "px;'></iframe>";
                var holder = "<div id='iframe-container-helper' style='top: " + topPos + "px; bottom: " + bottomPos + "px; z-index: 10002;'>" + iframe + "<div class='loading-background-holder'></div></div>";
                $("#container").hide();

                if (name != null && name != "") {
                    name = "Close " + name;
                }
                else {
                    name = "Close";
                }

                $("#lnk_BackToWorkspace").html(name);
                $("body").append(holder);
                $("#iframe-container-helper").fadeIn(animationSpeed);

                document.getElementById("iframe-content-src").onload = function () {
                    $(document).ready(function () {
                        setTimeout(function () {
                            $("#iframe-container-helper").find(".loading-background-holder").remove();
                        }, animationSpeed);
                    });
                };
            }

            function HashChange() {
                var url = location.hash;
                $("#lnk_BackToWorkspace").hide();
                $("#lbtn_Logoff").show();

                var content = GetUrlParameterByName("iframecontent");
                if (content != null && content != "") {
                    var name = GetUrlParameterByName("iframeName");
                    var fullScreen = GetUrlParameterByName("iframeFullScreen");
                    FinishIframeLoad(content, name, fullScreen);
                }
                else {
                    $("#iframe-container-helper").remove();
                    $("#container").show();
                }

                $(window).resize();
            }

            function GetUrlParameterByName(e) {
                e = e.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
                var t = "[\\?&]" + e + "=([^&#]*)";
                var n = new RegExp(t);
                var url = window.location.href.replace("#", "");
                var r = n.exec(url);
                if (r == null)
                    return "";
                else
                    return decodeURIComponent(r[1].replace(/\+/g, " "));
            }


            /* Group Login Modal */
            function GroupLoginModal() {
                LoadingMessage1("Loading Groups...");
                $.ajax({
                    url: "WebServices/AcctSettings.asmx/GetUserGroups",
                    type: "POST",
                    data: '{ }',
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        var x = "";
                        try {
                            for (var i = 0; i < data.d[0].length; i++) {
                                var groupId = data.d[0][i][0];
                                var groupName = data.d[0][i][1];
                                var image = data.d[0][i][2];
                                var owner = data.d[0][i][3];
                                var background = data.d[0][i][4];

                                var styleBackground = "style=\"background: url('" + background + "') 100% 50% repeat;\"";

                                x += "<div class='group-selection-entry' onclick='LoginAsGroup(\"" + groupId + "\")' title='Click to login' " + styleBackground + ">";
                                x += "<div class='overlay'></div>";
                                x += "<div class='group-selection-info'>";
                                x += "<div class='group-name-info'>";
                                x += "<span class='group-name'>" + groupName + "</span><div class='clear-space-five'></div>";
                                x += "<span class='group-info'><b class='pad-right-sml'>Owner:</b>" + owner + "</span>";
                                x += "</div>";
                                x += "<img class='group-img' alt='Group Logo' src='" + image + "' />";
                                x += "</div></div>";
                            }

                            if (ConvertBitToBoolean(data.d[1])) {
                                x += "<div class='logingroupselector-logout' onclick='LoginAsGroup(\"\")'>";
                                x += "<div>Log Out of Group</div>";
                                x += "</div>";
                            }

                            x += "<div class='clear-space'></div>";
                        }
                        catch (evt) { }

                        $("#group-list").html(x);
                        RemoveUpdateModal();
                    }
                });
            }
            function LoginAsGroup(id) {
                LoadingMessage1("Loading...");
                $.ajax({
                    url: "WebServices/AcctSettings.asmx/LoginUnderGroup",
                    type: "POST",
                    data: '{ "id": "' + id + '" }',
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        if (data.d == "true") {
                            window.location = "Workspace.aspx";
                        }
                        else {
                            location.reload();
                        }
                    },
                    error: function (e) {
                        RemoveUpdateModal();
                    }
                });
            }

            function ConvertBitToBoolean(value) {
                if (value != null && value != undefined) {
                    var _value = $.trim(value.toString().toLowerCase());

                    if (_value != "true" && _value != "false" && _value != "0" && _value != "1" && _value != "") {
                        return true;
                    }

                    if (_value == "1" || _value == "true" || _value == "") {
                        return true;
                    }
                    else if (_value == "0" || _value == "false") {
                        return false;
                    }
                }

                return false;
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
