<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Installer.aspx.cs" Inherits="Installer" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Installer</title>
    <meta name="author" content="John Donnelly" />
    <meta name="revisit-after" content="10 days" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <meta name="viewport" content="width=device-width, user-scalable=no" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
    <style type="text/css">
        .container { font-size: 15px; height: 295px; left: 50%; margin-left: -440px; margin-top: -174px; padding: 25px 15px 25px 15px; position: fixed; z-index: 100; text-align: center; text-shadow: 0 1px rgba(0, 0, 0, 0.15); top: 50%; width: 850px; color: #FFF; background: rgba(25,25,25,0.7); border: 1px solid rgba(100,100,100,0.8); -webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px; -webkit-box-shadow: -1px 2px 40px 2px rgba(0,0,0,0.45); -moz-box-shadow: -1px 2px 40px 2px rgba(0,0,0,0.45); box-shadow: -1px 2px 40px 2px rgba(0,0,0,0.45); overflow: auto; }
            .container h1 { color: #FFF; font-weight: bold; }
            .container a { color: #CDE3EB; }
        .container-blur { height: 365px; left: 50%; margin-left: -440px; margin-top: -207px; padding: 25px 15px 25px 15px; position: fixed; z-index: 1; top: 50%; width: 850px; border: 1px solid rgba(100,100,100,0.0); -webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px; background-image: url('App_Themes/Standard/Body/default-bg.jpg'); background-repeat: no-repeat; background-position: center center; background-attachment: fixed; background-color: #2D2D2D; background-size: cover; -webkit-filter: blur(7px); -moz-filter: blur(7px); -o-filter: blur(7px); filter: blur(7px); }
        .body-bg { position: fixed; left: 0; right: 0; top: 0; bottom: 0; z-index: 0; background-image: url('App_Themes/Standard/Body/default-bg.jpg'); background-repeat: no-repeat; background-position: center center; background-attachment: fixed; background-color: #2D2D2D; background-size: cover; }
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
                        <h1>Select a Database</h1>
                        <div class="clear-space"></div>
                        <div class="clear-space"></div>
                        <div class="clear-space"></div>
                        <table border="0" cellpadding="18" cellspacing="18" width="100%">
                            <tbody>
                                <tr>
                                    <td valign="top" width="50%">
                                        <div class="pad-right-big">
                                            <b>SQL Server Compact 4</b>
                                            <br />
                                            <br />
                                            A small and simple database, lightweight installation, connecting to a database file. SQL Server Compact 4 features an easier setup compared to SQL Server Express. This should work on just about any system without any setup.
                                        </div>
                                    </td>
                                    <td valign="top">
                                        <div class="pad-left-big">
                                            <b>SQL Server Express 2008+</b>
                                            <br />
                                            <br />
                                            A larger and more complex database that allows for larger data and Table Views. SQL Server Express should only be used if the server running this site has SQL Server Express 2008 or higher.
                                        </div>
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
                        <h1>Setup SQL Express Database</h1>
                        <div class="clear-space"></div>
                        <div class="clear-space"></div>
                        <div class="clear-space"></div>
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
                                        <div class="clear-space"></div>
                                        <a href="#" onclick="ConfirmExpress();return false;">Create SQL Server Express Database</a>
                                        <asp:HiddenField ID="lbtn_express" runat="server" OnValueChanged="lbtn_express_Click" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <asp:LinkButton ID="lbtn_Cancel" runat="server" Text="<- Go Back" OnClick="lbtn_Cancel_Click"></asp:LinkButton>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div class="clear-space"></div>
        </div>
        <div class="container-blur"></div>
        <script type="text/javascript" src="//code.jquery.com/jquery-3.3.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/ui/1.12.1/jquery-ui.min.js"></script>
        <script type="text/javascript" src="Scripts/jquery/combined-scripts.min.js"></script>
        <script type="text/javascript" src="Scripts/SiteCalls/Min/openwse.min.js"></script>
        <script type="text/javascript">
            Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function (sender, args) {
                loadingPopup.Message("One Moment...");
            });

            $(document).ready(function () {
                $("body").css("height", $(window).height());
            });
            $(window).resize(function () {
                $("body").css("height", $(window).height());
            });

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                loadingPopup.RemoveMessage();
            });

            function ConfirmCompact() {
                openWSE.ConfirmWindow("Are you sure you want to create an SQL Server Compact 4 Database? You will not be able to change this later on without restarting the Installer.", function () {
                    $("#lbtn_compact").val(new Date().toString());
                    setTimeout(function () {
                        __doPostBack("lbtn_compact", "");
                    }, 1);
                }, null);
            }

            function ConfirmExpress() {
                openWSE.ConfirmWindow("Are you sure you want to create an SQL Server Express Database? You will not be able to change this later on without restarting the Installer.", function () {
                    $("#lbtn_express").val(new Date().toString());
                    setTimeout(function () {
                        __doPostBack("lbtn_express", "");
                    }, 1);
                }, null);
            }
        </script>
    </form>
</body>
</html>
