<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UserBackgroundImageUpload.aspx.cs" Inherits="SiteTools_iframes_UserBackgroundImageUpload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>User Background Image Upload</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="~/Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="~/Standard_Images/favicon.ico" type="image/ico" />
</head>
<body style="background: transparent!important;">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" EnablePartialRendering="True"
            EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
        <asp:FileUpload ID="fileupload_userimages" runat="server" AllowMultiple="true" />
        <asp:Button ID="btn_UploadImages" runat="server" Text="Upload Images" CssClass="input-buttons RandomActionBtns" OnClick="btn_UploadImages_Click" />
        <script type="text/javascript">
            var rootDir = "";
            var multipleBackgrounds = false;

            function GetSiteRoot() {
                return window.location.protocol + "//" + window.location.host + "/" + rootDir + "/";
            }

            function BackgroundSelector() {
                var doc = window.top.document;
                if (doc != null) {
                    var workspaceNum = Getworkspace();
                    if (!multipleBackgrounds) {
                        workspaceNum = "workspace_1";
                    }

                    $.ajax({
                        url: GetSiteRoot() + "WebServices/AcctSettings.asmx/GetServerImageList",
                        type: "POST",
                        data: '{ "_workspace": "' + workspaceNum + '", "folder": "user" }',
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            if (data.d.length > 1) {
                                $(doc).find("#background-selector-holder").find(".img-list-selector").html(data.d[1]);
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
