<%@ Page Title="Group Login" Language="C#" AutoEventWireup="true" CodeFile="GroupLogin.aspx.cs" Inherits="GroupLogin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta name="author" content="John Donnelly" />
    <meta name="revisit-after" content="10 days" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta name="viewport" content="user-scalable = yes" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <style type="text/css">
        .group-selection-entry { width: 100%!important; max-width: 100%!important; }
    </style>
</head>
<body id="site_mainbody" runat="server" class="container-main-bg-simple">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="True" AsyncPostBackTimeout="360000" />
        <h1 style="display: none;">Group Login</h1>
        <div id="top_bar" runat="server">
            <div id="top-logo-holder">
                <a id="lnk_BackToWorkspace" href="~/Default.aspx" runat="server" title="Back to Workspace"></a>
            </div>
            <div id="top-button-holder">
                <div id="settings_tab" runat="server" class="top-bar-menu-button">
                    <div class="active-div"></div>
                    <ul>
                        <li class="a settings-menu-toggle" title="Options/Settings"></li>
                        <li class="b">
                            <div class="li-header">
                                Options<a id="hyp_AccountCustomizations" runat="server" class="float-right pad-left-big margin-left-big" visible="false" title="Customize your account" onclick="openWSE.CloseTopDropDowns();">Customizations</a>
                            </div>
                            <div class="li-pnl-tab">
                                <div class="clear-space"></div>
                                <h3 class="pad-left pad-top">Layout Style
                                </h3>
                                <div class="clear-space-two"></div>
                                <div class="pad-left pad-top pad-bottom pad-right">
                                    <div class="float-left pad-right-big">
                                        <asp:RadioButton ID="rb_BoxedLayout" runat="server" Text="&nbsp;Boxed" CssClass="radiobutton-style" GroupName="layoutstyles" />
                                    </div>
                                    <div class="float-left pad-right-big">
                                        <asp:RadioButton ID="rb_WideLayout" runat="server" Text="&nbsp;Wide" CssClass="radiobutton-style" GroupName="layoutstyles" />
                                    </div>
                                    <div class="clear"></div>
                                </div>
                                <div class="clear-space"></div>
                                <div class="border-top"></div>
                                <div class="clear-space"></div>
                                <h3 class="pad-left pad-top">Color Option
                                </h3>
                                <div class="clear-space-two"></div>
                                <div id="div_ColorOptionsHolder" runat="server" class="pad-all-sml">
                                </div>
                                <div class="clear-space"></div>
                            </div>
                        </li>
                    </ul>
                </div>
            </div>
            <div id="user_profile_tab" runat="server" class="top-bar-userinfo-button">
                <div class="active-div"></div>
                <ul>
                    <li class="a">
                        <asp:Label ID="lbl_UserName" runat="server" CssClass="username-top-info" ToolTip="Account Options"></asp:Label></li>
                    <li class="b">
                        <div class="li-header">
                            My Account
                        </div>
                        <div class="pad-all">
                            <div class="pad-all">
                                <div class="float-left margin-right profile-tab-acctimg">
                                    <asp:Image ID="img_Profile" runat="server" CssClass="acct-image"></asp:Image>
                                </div>
                                <div class="float-left pad-left">
                                    <asp:Label ID="lbl_UserFullName" runat="server" Text="" CssClass="title-dd-name"></asp:Label>
                                    <div class="clear-space-two">
                                    </div>
                                    <asp:Label ID="lbl_UserEmail" runat="server" Text=""></asp:Label>
                                </div>
                                <div class="clear">
                                </div>
                            </div>
                        </div>
                        <div id="profile_tab_Buttons" class="li-footer">
                            <asp:LinkButton ID="lbtn_signoff" runat="server" OnClick="lbtn_signoff_Click">Log Out</asp:LinkButton>
                            <div class="clear"></div>
                        </div>
                    </li>
                </ul>
            </div>
            <div class="clear"></div>
        </div>
        <div class="fixed-container-border-left"></div>
        <div class="fixed-container-border-right"></div>
        <div class="fixed-container-holder-background">
            <div class="fixed-container-holder">
                <div id="main_container" runat="server">
                    <div class="table-settings-box">
                    <div class="td-settings-title">Select a Group</div>
                    <div class="td-settings-ctrl">
                        <div id="group-list"></div>
                    </div>
                    <div class="td-settings-desc">
                        Each group has its own apps and default settings that cannot be modified. Once logged in, all default settings for that group will be applied to your account. These settings will be removed once you log out of the group. Your Apps, Overlays, and Notifications will be overridden with the group's settings.
                    </div>
                </div>
                </div>
                <div id="footer_container" runat="server">
                    <div id="copyright-footer" class="float-left"><asp:Literal ID="ltl_footercopyright" runat="server"></asp:Literal></div>
                    <div class="help-icon" title="Need help?">
                    </div>
                    <div id="footer-signdate" class="float-right">
                        <a href="#" title="Open Mobile Workspace" class="my-app-remote-link" onclick="return openWSE.OpenMobileWorkspace();">Mobile</a><a href="#iframecontent" onclick="openWSE.LoadIFrameContent('About.aspx');return false;">About</a><a href="#iframecontent" onclick="openWSE.LoadIFrameContent('About.aspx?a=termsofuse');return false;">Terms</a>
                    </div>
                </div>
            </div>
        </div>
        <div class="fixed-footer-container-left"></div>
        <div class="fixed-footer-container-right"></div>
        <script type="text/javascript">
            $(document).ready(function () {
                GroupLoginModal();
            });

            /* Group Login Modal */
            function GroupLoginModal() {
                loadingPopup.Message("Loading Groups...");
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/GetUserGroups", '{ }', null, function (data) {
                    var x = "";
                    try {
                        for (var i = 0; i < data.d[0].length; i++) {
                            var groupId = data.d[0][i][0];
                            var groupName = data.d[0][i][1];
                            var image = data.d[0][i][2];
                            var owner = data.d[0][i][3];
                            var background = data.d[0][i][4];
                            var groupApps = data.d[0][i][5];

                            var styleBackground = "style=\"background: url('" + background + "'); background-size: cover;\"";

                            x += "<div class='group-selection-entry' onclick='openWSE.LoginAsGroup(\"" + groupId + "\")' title='Click to login' " + styleBackground + ">";
                            x += "<div class='overlay'></div>";
                            x += "<div class='group-selection-info'>";
                            x += "<img class='group-img' alt='Group Logo' src='" + image + "' />";
                            x += "<div class='group-name-info'>";
                            x += "<span class='group-name'>" + groupName + "</span><div class='clear-space-five'></div>";
                            x += "<span class='group-info'><b class='pad-right-sml'>Owner:</b>" + owner + "</span>";
                            x += "</div><div class='clear'></div>";

                            var appList = "<div class='group-selection-applist'>";
                            for (var j = 0; j < groupApps.length; j++) {
                                appList += "<div class='group-applist-item'>";
                                appList += "<img alt='' src='" + openWSE.siteRoot() + groupApps[j].Icon + "' />";
                                appList += "<span>" + groupApps[j].AppName + "</span>";
                                appList += "<div class='clear'></div></div>";
                            }
                            appList += "</div><div class='clear'></div>";

                            x += appList;
                            x += "</div></div>";
                        }

                        if (openWSE.ConvertBitToBoolean(data.d[1])) {
                            x += "<div class='logingroupselector-logout' onclick='LoginAsGroup(\"\")'>";
                            x += "<div>Log Out of Group</div>";
                            x += "</div>";
                        }

                        x += "<div class='clear'></div>";
                    }
                    catch (evt) { }

                    $("#group-list").html(x);
                    loadingPopup.RemoveMessage();
                });
            }
            function LoginAsGroup(id) {
                loadingPopup.Message("Loading...");
                openWSE.AjaxCall("WebServices/AcctSettings.asmx/LoginUnderGroup", '{ "id": "' + id + '" }', null, function (data) {
                    if (data.d == "true") {
                        window.location = "Default.aspx";
                    }
                    else {
                        location.reload();
                    }
                }, function (e) {
                    loadingPopup.RemoveMessage();
                });
            }
        </script>
    </form>
</body>
</html>
