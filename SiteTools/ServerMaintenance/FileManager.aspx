<%@ page title="File Manager" async="true" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_FileManager, App_Web_et3auwnc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <link type="text/css" rel="stylesheet" href='<%=ResolveUrl("~/Scripts/SyntaxHighlighter/Styles/shCore.css")%>' />
    <link type="text/css" rel="stylesheet" href='<%=ResolveUrl("~/Scripts/SyntaxHighlighter/Styles/shThemeDefault.css")%>' />
    <style type="text/css">
        #editor
        {
            font-size: 14px;
            left: 0;
            position: relative;
            top: 0;
            width: 100%;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div>
                    <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
                    <asp:Panel ID="pnl1" runat="server">
                        <div class="clear-space"></div>
                        <div class="float-left" style="width: 475px;">
                            <div id="searchwrapper" style="width: 444px;">
                                <asp:Panel ID="Panel1_FileManager" runat="server" DefaultButton="imgbtn_search">
                                    <asp:TextBox ID="tb_search" runat="server" CssClass="searchbox" Font-Size="Small"
                                        onfocus="if(this.value=='Search Files')this.value=''" onblur="if(this.value=='')this.value='Search Files'"
                                        Text="Search Files"></asp:TextBox>
                                    <a href="#" onclick="$('#MainContent_tb_search').val('Search Files');return false;"
                                        class="searchbox_clear" title="Clear search"></a>
                                    <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                        OnClick="imgbtn_search_Click" />
                                </asp:Panel>
                            </div>
                            <div class="clear-space">
                            </div>
                        </div>
                        <div class="float-right">
                            <div class="float-right pad-right-sml">
                                The FileManager only loads files with extensions: .js and .css
                            </div>
                            <div class="clear-space">
                            </div>
                            <div class="float-right">
                                <b class="pad-right-sml">View Mode:</b>
                                <asp:DropDownList ID="dd_viewtype" runat="server" AutoPostBack="true" CssClass="margin-right"
                                    OnSelectedIndexChanged="dd_viewtype_SelectedIndexChanged" ClientIDMode="Static">
                                    <asp:ListItem Text="All" Value="all"></asp:ListItem>
                                    <asp:ListItem Text="css Only" Value=".css"></asp:ListItem>
                                    <asp:ListItem Text="js Only" Value=".js"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="clear-space">
                        </div>
                        <asp:GridView ID="GV_Script" runat="server" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
                            Width="100%" GridLines="None" OnRowCommand="GV_Script_RowCommand" ShowHeaderWhenEmpty="True">
                            <EmptyDataRowStyle ForeColor="Black" />
                            <RowStyle CssClass="GridNormalRow" />
                            <EmptyDataTemplate>
                                <div class="emptyGridView">
                                    There are no Scripts available.
                                </div>
                            </EmptyDataTemplate>
                            <Columns>
                                <asp:TemplateField>
                                    <HeaderTemplate>
                                        <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                                            <tr>
                                                <td align="center">Filename
                                                </td>
                                                <td width="65px" align="center">Ext
                                                </td>
                                                <td width="84px" align="center">Size
                                                </td>
                                                <td width="114px" align="center">Last Accessed
                                                </td>
                                                <td width="100px" align="center">Actions
                                                </td>
                                            </tr>
                                        </table>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Panel ID="pnl_FileMain" class="panelcontainer" runat="server">
                                            <table cellpadding="5" cellspacing="0" class="myItemStyle">
                                                <tr>
                                                    <td align="left" class="border-right border-left" title="<%# String.Format("View options for {0}", Eval("Title")) %>">
                                                        <div style="padding: 3px 0 4px 0;">
                                                            <%#Eval("Title") %>
                                                        </div>
                                                    </td>
                                                    <td width="66px" align="left" class="border-right">
                                                        <%#Eval("Type") %>
                                                    </td>
                                                    <td width="85px" align="left" class="border-right">
                                                        <%#Eval("Size") %>
                                                    </td>
                                                    <td width="115px" align="left" class="border-right">
                                                        <%#Eval("UploadDate") %>
                                                    </td>
                                                    <td width="100px" align="center" class='border-right'>
                                                        <asp:LinkButton ID="LinkButton3" CssClass='<%#Eval("DownloadClass") %>' runat="server"
                                                            ToolTip="Download" CommandName="DownloadScript" CommandArgument='<%#Eval("Title") %>'
                                                            PostBackUrl="FileManager.aspx"></asp:LinkButton>
                                                        <a href='FileManager.aspx?edit=true&file=<%#Eval("Path") %>' class='<%#Eval("EditClass") %>'
                                                            title="Edit"></a><a href='FileManager.aspx?edit=false&file=<%#Eval("Path") %>' class="<%#Eval("PreviewClass") %>"
                                                                title="View"></a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:Panel>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:Panel>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="dd_viewtype" />
                <asp:AsyncPostBackTrigger ControlID="lbtn_save" />
            </Triggers>
        </asp:UpdatePanel>
        <asp:Panel ID="pnl2" runat="server" Enabled="false" Visible="false">
            <h3 class="float-left pad-top-sml">File Editor/Viewer&nbsp;&nbsp;-</h3>
            <asp:Label ID="lbl_currfile" runat="server" CssClass="float-left pad-top margin-left"
                Text=""></asp:Label>
            <asp:LinkButton ID="lbtn_save" runat="server" OnClientClick=" return ConfirmSaveFile(this); "
                CssClass="float-right input-buttons no-margin" ToolTip="Save" style="margin-left: 16px !important;">
                <span class="img-backup float-left margin-right-sml"></span>Save</asp:LinkButton>
            <asp:HyperLink ID="lbtn_close" NavigateUrl="FileManager.aspx" CssClass="float-right input-buttons no-margin"
                runat="server" ToolTip="Back">
                <span class="pg-prev-btn float-left margin-right-sml" style="padding: 0px!important;"></span>Back
            </asp:HyperLink>
            <div class="clear-space">
            </div>
            <div class="clear-margin">
                    <asp:Label ID="lbl_messageRead" runat="server" CssClass="float-right bold" Text=""></asp:Label>
                    Do not refresh the browser or press the back button while in the File Editor/Viewer
            </div>
            <div class="clear-space">
            </div>
            <asp:Literal ID="ltl_code" runat="server"></asp:Literal>
            <div id="editor" style="display: none;">
            </div>
            <asp:HiddenField ID="hidden_editor" runat="server" ClientIDMode="Static" />
        </asp:Panel>
        <script src="//d1n0x3qji82z53.cloudfront.net/src-min-noconflict/ace.js" type="text/javascript"
            charset="utf-8"> </script>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/filemanager.js")%>'> </script>
        <script type="text/javascript" language="javascript" src='<%=ResolveUrl("~/Scripts/SyntaxHighlighter/Scripts/shCore.js")%>'> </script>
        <script type="text/javascript" language="javascript" src='<%=ResolveUrl("~/Scripts/SyntaxHighlighter/Scripts/shBrushJScript.js")%>'> </script>
        <script type="text/javascript" language="javascript" src='<%=ResolveUrl("~/Scripts/SyntaxHighlighter/Scripts/shBrushXml.js")%>'> </script>
        <script type="text/javascript">
            $(document).ready(function () {
                $(document).tooltip({ disabled: true });
                SyntaxHighlighter.all()
                $(window).resize();
            });

            $(window).resize(function () {
                try
                {
                    var h = $(window).height();
                    $("#editor, #ace_scroller").css("height", h - 245);
                    $("#editor, #ace_scroller").css("width", $("#MainContent_pnl2").outerWith());
                }
                catch (evt) { }
            });

            $(document.body).on("change", "#dd_viewtype", function () {
                openWSE.LoadingMessage1("Updating. Please Wait...");
            });
        </script>
    </div>
</asp:Content>
