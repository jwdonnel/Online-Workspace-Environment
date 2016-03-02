<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UserImageUpload.aspx.cs" Inherits="SiteTools_iframes_UserImageUpload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>User Image Upload</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="../../Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="../../Standard_Images/favicon.ico"
        type="image/ico" />
    <link href="../../App_Themes/Standard/site_desktop.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="../../App_Themes/Standard/jqueryUI.css" />
</head>
<body style="background: transparent!important;">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" EnablePartialRendering="True"
            EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
        <asp:FileUpload ID="fileupload_userimages" runat="server" AllowMultiple="true" />
        <asp:Button ID="btn_UploadImages" runat="server" Text="Upload Images" CssClass="input-buttons RandomActionBtns" OnClick="btn_UploadImages_Click" />
        <div class="pad-top pad-bottom">
            <small>Images uploaded will only be viewable for this user. Images will be uploaded into your user directory.<br />
                Only .jpg, .png, .gif files are allowed.</small>
        </div>
        <script type="text/javascript" src="//code.jquery.com/jquery-1.11.2.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>
        <script type="text/javascript" src="//code.jquery.com/ui/1.11.4/jquery-ui.min.js"></script>
        <script type="text/javascript">
            var rootDir = "";
            function GetSiteRoot() {
                return window.location.protocol + "//" + window.location.host + "/" + rootDir + "/";
            }

            function BackgroundSelector() {
                var doc = window.top.document;
                if (doc != null) {
                    $.ajax({
                        url: GetSiteRoot() + "WebServices/AcctSettings.asmx/GetServerImageList",
                        type: "POST",
                        data: '{ "_workspace": "' + Getworkspace() + '", "folder": "user" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            if (data.d.length == 6) {
                                $(doc).find("#BackgroundSelector-element").find(".img-list-selector").html(data.d[1]);
                            }

                            window.location.href = window.location.href;
                        }
                    });
                }
            }

            function Getworkspace() {
                var doc = window.top.document;
                if (doc != null) {
                    var $this = $(doc).find('#workspace_holder').find(".workspace-holder");
                    var len = $this.length;
                    for (var i = 0; i < len; i++) {
                        if ($this.eq(i).css("visibility") == "visible") {
                            var id = $this.eq(i).attr("id");
                            return "workspace_" + id.substring(id.lastIndexOf("_") + 1);
                        }
                    }
                }

                return "workspace_1";
            }
        </script>
    </form>
</body>
</html>
