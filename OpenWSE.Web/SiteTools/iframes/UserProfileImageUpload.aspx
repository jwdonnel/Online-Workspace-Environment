<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UserProfileImageUpload.aspx.cs" Inherits="SiteTools_iframes_UserProfileImageUpload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>User Profile Image Upload</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="../../Standard_Images/favicon.ico" type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="../../Standard_Images/favicon.ico" type="image/ico" />
</head>
<body style="background: transparent!important;">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" EnablePartialRendering="True"
            EnableHistory="true" EnableSecureHistoryState="False" AsyncPostBackTimeout="360000" />
        <table cellspacing="5" cellpadding="5" style="width: 100%;">
            <tr>
                <td valign="top">
                    <asp:UpdatePanel ID="updatepnl_img" runat="server">
                        <ContentTemplate>
                            <asp:Image ID="img_UserImage" runat="server" CssClass="acct-image margin-right" />
                            <div id="div_cancelupload" style="display: none;">
                                <div class="clear-space-two"></div>
                                <small><a href="#" class="margin-left" onclick="CancelUpload();return false;">Cancel</a></small>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
                <td valign="top">
                    <asp:Button ID="btn_UploadImages" runat="server" Text="Upload" CssClass="input-buttons float-right no-margin" OnClick="btn_UploadImages_Click" style="margin-top: 0!important;" />
                    <asp:FileUpload ID="fileUpload_acctImage" runat="server" AllowMultiple="false" />
                    <div class="clear-space-five"></div>
                    <div class="pad-top">
                        <asp:UpdatePanel ID="updatepnl_btns" runat="server">
                            <ContentTemplate>
                                <small class="float-left">Only .jpg, .png, .gif files are allowed.</small>
                                <asp:LinkButton ID="lnkBtn_clearImage" runat="server" OnClick="lnkBtn_clearImage_Click" Text="Clear" CssClass="float-right"></asp:LinkButton>
                                <div class="clear-space-two"></div>
                                <asp:Label ID="lbl_error" runat="server" ForeColor="Red" Text="Error uploading image. Please try again." Enabled="false" Visible="false"></asp:Label>
                                <asp:Label ID="lbl_message" runat="server" Enabled="false" Visible="false" Font-Size="12px"></asp:Label>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </td>
            </tr>
        </table>
        <script type="text/javascript">
            var rootDir = "";
            function GetSiteRoot() {
                return window.location.protocol + "//" + window.location.host + "/" + rootDir + "/";
            }

            var tempUserImg = "";
            function CancelUpload() {
                $("#div_cancelupload").hide();
                $("#img_UserImage").attr("src", tempUserImg);
                $("#img_UserImage").css({
                    width: "",
                    height: ""
                });
            }

            $(document.body).on("change", "#fileUpload_acctImage", function (e) {
                $("#div_cancelupload").hide();
                tempUserImg = $("#img_UserImage").attr("src");

                try {
                    var tmppath = URL.createObjectURL(e.target.files[0]);
                    if (tmppath != "") {
                        $("#img_UserImage").attr("src", tmppath);
                        $("#img_UserImage").css({
                            width: 57,
                            height: 57
                        });
                        $("#div_cancelupload").show();
                    }
                }
                catch (evt) { }
            });

            function UpdateModal(acctImgUrl) {
                var $topDoc = $(window.top.document);
                if ($topDoc.length > 0) {
                    $topDoc.find("#user_profile_tab").find(".top-menu-acctImage").attr("src", acctImgUrl);
                    $topDoc.find("#img_Profile").attr("src", acctImgUrl);
                    if ($topDoc.find("#imgAcctImage").length > 0) {
                        $topDoc.find("#imgAcctImage").attr("src", acctImgUrl);
                    }
                }
            }

            function UpdateModal_UploadFinished(userId, acctImg) {
                var $topDoc = $(window.top.document);
                if ($topDoc.length > 0) {
                    var acctImgUrl = GetSiteRoot() + "Standard_Images/AcctImages/" + userId + "/" + acctImg;
                    $topDoc.find("#user_profile_tab").find(".top-menu-acctImage").attr("src", acctImgUrl);
                    $topDoc.find("#img_Profile").attr("src", acctImgUrl);
                    if ($topDoc.find("#imgAcctImage").length > 0) {
                        $topDoc.find("#imgAcctImage").attr("src", acctImgUrl);
                    }
                }
            }
        </script>
    </form>
</body>
</html>
