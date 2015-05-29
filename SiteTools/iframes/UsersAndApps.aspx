<%@ page language="C#" autoeventwireup="true" inherits="SiteTools_UsersAndApps, App_Web_1sq1bxip" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Users and Apps</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="../../Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="../../Standard_Images/favicon.ico"
        type="image/ico" />
    <link href="../../App_Themes/Standard/site_desktop.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="../../App_Themes/Standard/jqueryUI.css" />
    <style type="text/css">
        .item-column
        {
            padding: 5px 10px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:UpdatePanel ID="updatepnl_apps" runat="server">
            <ContentTemplate>
                <asp:ScriptManager ID="ScriptManager" runat="server" EnablePartialRendering="True"
                    EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
                <div id="app_title_bg" runat="server" class="app-Settings-title-bg-color-main">
                    <div class="pad-all">
                        <div class="app-Settings-title-user-info">
                            <div class="float-left">
                                <asp:Label ID="lbl_title" runat="server" CssClass="page-title"></asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="pad-all-big margin-left-big margin-right-big">
                    <div class="table-settings-box">
                        <div class="td-settings-ctrl">
                            <asp:Panel ID="pnl_List" runat="server">
                            </asp:Panel>
                            <div class="clear-space"></div>
                        </div>
                        <div class="td-settings-desc">
                            This page will show the current installed user apps and plugins. If viewing individual users, you can add and remove apps and plugins or install an app package.
                        </div>
                    </div>
                </div>
                <asp:HiddenField ID="hf_addApp" runat="server" OnValueChanged="hf_addApp_Changed" />
                <asp:HiddenField ID="hf_removeApp" runat="server" OnValueChanged="hf_removeApp_Changed" />
                <asp:HiddenField ID="hf_removeAllApp" runat="server" OnValueChanged="hf_removeAllApp_Changed" />
                <asp:HiddenField ID="hf_appPackage" runat="server" OnValueChanged="hf_appPackage_Changed" />
                <asp:HiddenField ID="hf_addPlugin" runat="server" OnValueChanged="hf_addPlugin_ValueChanged" />
                <asp:HiddenField ID="hf_removePlugin" runat="server" OnValueChanged="hf_removePlugin_ValueChanged" />
                <asp:HiddenField ID="hf_removeAllPlugins" runat="server" OnValueChanged="hf_removeAllPlugins_ValueChanged" />
            </ContentTemplate>
        </asp:UpdatePanel>
        <script type="text/javascript" src="<%=ResolveUrl("~/Scripts/SiteCalls/Min/openwse.min.js") %>"></script>
        <script type="text/javascript">
            $(document).ready(function () {
                openWSE_Config.siteRootFolder = "<%=ResolveUrl("~/").Replace("/", "") %>";
            });

            function RemovePlugin(id) {
                openWSE.LoadingMessage1("Removing Plugin...");
                document.getElementById("hf_removePlugin").value = id;
                __doPostBack("hf_removePlugin", "");
            }

            function AddPlugin(id) {
                openWSE.LoadingMessage1("Adding Plugin...");
                document.getElementById("hf_addPlugin").value = id;
                __doPostBack("hf_addPlugin", "");
            }

            function AddApp(id) {
                openWSE.LoadingMessage1("Adding App...");
                document.getElementById("hf_addApp").value = id;
                __doPostBack("hf_addApp", "");
            }

            function RemoveApp(id) {
                openWSE.LoadingMessage1("Uninstalling App...");
                document.getElementById("hf_removeApp").value = id;
                __doPostBack("hf_removeApp", "");
            }

            function RemoveAllPlugins() {
                openWSE.LoadingMessage1("Uninstalling All Plugins...");
                document.getElementById("hf_removeAllPlugins").value = new Date().toString();
                __doPostBack("hf_removeAllPlugins", "");
            }

            function RemoveAllApp() {
                openWSE.LoadingMessage1("Uninstalling All Apps...");
                document.getElementById("hf_removeAllApp").value = new Date().toString();
                __doPostBack("hf_removeAllApp", "");
            }

            function AppPackageInstall() {
                openWSE.LoadingMessage1("Installing App Package...");
                var id = document.getElementById("ddl_appPackages").value;
                document.getElementById("hf_appPackage").value = id;
                __doPostBack("hf_appPackage", "");
            }
        </script>
    </form>
</body>
</html>
