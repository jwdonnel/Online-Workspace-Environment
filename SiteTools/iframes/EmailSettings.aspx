<%@ Page Language="C#" AutoEventWireup="true" CodeFile="EmailSettings.aspx.cs" Inherits="SiteTools_EmailSettings" %>

<%@ Register TagPrefix="cc" Namespace="TextEditor" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Email Settings</title>
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <link id="Link1" runat="server" rel="shortcut icon" href="../../Standard_Images/favicon.ico"
        type="image/x-icon" />
    <link id="Link2" runat="server" rel="icon" href="../../Standard_Images/favicon.ico"
        type="image/ico" />
    <link href="../../App_Themes/Standard/site_desktop.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="../../App_Themes/Standard/jqueryUI.css" />
    <style type="text/css">
        .sitemenu-selection
        {
            margin-left: -20px!important;
            margin-right: -20px!important;
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
                        <asp:Label ID="Label1" runat="server" CssClass="page-title" Text="Outgoing Email Customizations"></asp:Label>
                    </div>
                </div>
            </div>
        </div>
        <div class="clear" style="height: 25px;">
        </div>
        <div class="pad-left-big pad-right-big">
            <asp:Panel ID="pnlLinkBtns" runat="server">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <ul class="sitemenu-selection">
                            <li class="active">
                                <asp:LinkButton ID="lbtn_header" runat="server" CssClass="RandomActionBtns"
                                    OnClick="lbtn_header_Checked" Text="Header Message" /></li>
                            <li>
                                <asp:LinkButton ID="lbtn_footer" runat="server" CssClass="RandomActionBtns"
                                    OnClick="lbtn_footer_Checked" Text="Footer Message" /></li>
                        </ul>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
        </div>
        <div class="clear-space">
        </div>
        <div class="clear-space">
        </div>
        <div style="padding: 15px 30px">
            <div class="clear-margin" style="margin-top: 10px">
                These settings will stylize all outgoing emails sent
            by the site. Edit the header and footer of the email to give it a custom look.<br />
                    If you wish to put a custom message title in the header, place [MESSAGE_TITLE] anywhere
            in the header/footer. Using the message title override will remove the default message
            title.
            </div>
            <script type="text/javascript" src="<%=ResolveUrl("~/Scripts/SiteCalls/Min/openwse.min.js") %>"></script>
            <script type="text/javascript">
                $(document).ready(function () {
                    openWSE_Config.siteRootFolder = "<%=ResolveUrl("~/").Replace("/", "") %>";
                });

                $(window).load(function () {
                    $(".sitemenu-selection").removeClass("mobile-mode");
                });

                function pageLoad() {
                    LoadTinyMCEControls_Simple("htmlEditor");
                    $(document).tooltip({ disabled: true });
                }

                /*TinyMCE*/
                function LoadTinyMCEControls_Simple(id) {
                    if (typeof tinyMCE != "undefined") {
                        if (document.getElementById(id) != null) {
                            window.tinymce.dom.Event.domLoaded = true;
                            tinymce.init({
                                selector: "#" + id,
                                theme: "modern",
                                remove_script_host: false,
                                relative_urls: false,
                                plugins: ["advlist autolink lists link image charmap print preview anchor", "searchreplace visualblocks code fullscreen"],
                                toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
                            });

                            canLoadEditorText = 1;
                        }
                    }
                    else {
                        setTimeout(function () { LoadTinyMCEControls_Simple(id); }, 1000);
                    }
                }

                function LoadTinyMCEControls_Full(id) {
                    if (typeof tinyMCE != "undefined") {
                        if (document.getElementById(id) != null) {
                            window.tinymce.dom.Event.domLoaded = true;
                            tinymce.init({
                                selector: "#" + id,
                                theme: "modern",
                                remove_script_host: false,
                                relative_urls: false,
                                plugins: ["advlist autolink lists link image charmap print preview hr anchor pagebreak", "searchreplace wordcount visualblocks visualchars code fullscreen", "insertdatetime media nonbreaking save table contextmenu directionality", "emoticons template paste"],
                                toolbar1: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image",
                                toolbar2: "print preview media | forecolor backcolor emoticons",
                                templates: [{ title: 'Test template 1', content: 'Test 1' }, { title: 'Test template 2', content: 'Test 2' }]
                            });

                            canLoadEditorText = 1;
                        }
                    }
                    else {
                        setTimeout(function () { LoadTinyMCEControls_Full(id); }, 1000);
                    }
                }

                $(document.body).on("click", "#btn1", function () {
                    var x = $.trim(tinymce.activeEditor.getContent());
                    if (x != null && x != "") {
                        openWSE.LoadingMessage1("Updating. Please Wait...");
                        if ($(this).val() == "Update Header") {
                            $("#hf_UpdateHeader").val(escape(x));
                            __doPostBack("hf_UpdateHeader", "");
                        }
                        else {
                            $("#hf_UpdateFooter").val(escape(x));
                            __doPostBack("hf_UpdateFooter", "");
                        }
                    }
                });

                function UnescapeCode(text) {
                    setTimeout(function () {
                        if ((tinymce != null) && (tinymce != undefined)) {
                            try {
                                tinymce.activeEditor.setContent(unescape(text));
                            }
                            catch (evt) {
                                UnescapeCode(text);
                            }
                        }
                        else {
                            UnescapeCode(text);
                        }
                    }, 50);
                }
            </script>
            <div class="clear-space">
            </div>
            <div class="clear-margin">
                <div class="float-right">
                    <input id="btn1" type="button" class="input-buttons-create" value="Update Header" />
                </div>
                <div class="clear-space">
                </div>
                <div class="clear-space">
                </div>
                <cc:AppEditor runat="server" ID="htmlEditorHeader" ClientIDMode="Static" Mode="Simple"
                    Height="400px" />
                <asp:UpdatePanel ID="updatepnl" runat="server">
                    <ContentTemplate>
                        <div class="clear-space">
                        </div>
                        <asp:LinkButton ID="lbtn_ClearHeader" runat="server" CssClass="float-right"
                            OnClick="lbtn_ClearHeader_Checked" OnClientClick=""
                            Text="Clear Header" />
                        <asp:LinkButton ID="lbtn_ClearFooter" runat="server" CssClass="float-right"
                            OnClick="lbtn_ClearFooter_Checked"
                            Text="Clear Footer" />
                        <div class="clear" style="height: 60px">
                        </div>
                        <asp:HiddenField ID="hf_UpdateHeader" runat="server" OnValueChanged="hf_UpdateHeader_Changed" />
                        <asp:HiddenField ID="hf_UpdateFooter" runat="server" OnValueChanged="hf_UpdateFooter_Changed" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </form>
</body>
</html>
