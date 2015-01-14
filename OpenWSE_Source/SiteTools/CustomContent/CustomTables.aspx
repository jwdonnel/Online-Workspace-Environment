<%@ Page Title="Custom Tables" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="CustomTables.aspx.cs" Inherits="SiteTools_CustomTables" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .GridNormalRow
        {
            cursor: move;
        }

        .columnIDRow, .border-right
        {
            cursor: default !important;
        }

        #confirmPassword
        {
            float: left;
            padding-left: 10px;
            margin-top: -3px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding pad-top-big margin-top">
        <small><b class="pad-right-sml">Note:</b>All new tables automatically create a app that
        you will have to install for each user after creating. The Table Name will also
        be the name of the app created.</small>
        <div class="clear-space">
        </div>
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <asp:Panel ID="pnl_columnEditor" runat="server">
            <div id="Divcb_installHolder">
                <asp:CheckBox ID="cb_InstallAfterLoad" runat="server" Text="&nbsp;Install app for current user after creation"
                    Checked="true" />
                <div class="clear-space-five">
                </div>
                <div id="div_isPrivate">
                    <asp:CheckBox ID="cb_isPrivate" runat="server" Text="&nbsp;Make this app private (Only for me)"
                        ClientIDMode="Static" Checked="false" />
                    <div class="clear-space-five">
                    </div>
                </div>
                <asp:CheckBox ID="cb_showSidebar" runat="server" Text="&nbsp;Show sidebar with month selector"
                    ClientIDMode="Static" Checked="true" />
            </div>
            <div class="clear-space">
            </div>
            <table width="650px">
                <tr>
                    <td align="left" style="width: 100px;">
                        <span class="pad-right font-bold">Table Name</span>
                    </td>
                    <td style="width: 250px;">
                        <asp:TextBox ID="tb_tablename" runat="server" CssClass="textEntry margin-left float-left"
                            MaxLength="100" AutoCompleteType="Search" Width="175px" ClientIDMode="Static"></asp:TextBox>
                    </td>
                    <td align="right">
                        <div id="createBtns">
                            <a href="#" onclick="ClearControls();return false;" class="sb-links margin-right-big">Clear Entries</a>
                            <input type="button" id="btn_createtable" class="input-buttons margin-left" onclick="CreateTable()"
                                value="Create Table" />
                        </div>
                        <div id="editBtns" style="display: none;">
                            <input type="button" class="input-buttons margin-left" onclick="CancelUpdate()" value="Cancel" />
                            <input type="button" class="input-buttons margin-left" onclick="UpdateTable()" value="Update Table" />
                        </div>
                    </td>
                </tr>
            </table>
            <div class="clear">
            </div>
            <span id="successMessage"></span>
            <div class="clear" style="height: 30px;">
            </div>
            <h2>Column Editor</h2>
            <div class="clear-space">
            </div>
            <small><b class="pad-right-sml">Note:</b>A column ID row and TimeStamp row are already included
            in the list of columns. These rows cannot be deleted or edited.<br />
                Certain characters are not allowed for the column name. These include [|&;$%@"<>()+,],
            and spaces.</small>
            <div class="clear-space">
            </div>
            <table class="myHeaderStyle" cellpadding="5" cellspacing="0" style="width: 625px!important;">
                <tbody>
                    <tr>
                        <td width="45px"></td>
                        <td>Column Name
                        </td>
                        <td align="center" width="135px">Data Type
                        </td>
                        <td align="center" width="75px">Nullable
                        </td>
                        <td align="center" width="75px">Actions
                        </td>
                    </tr>
                </tbody>
            </table>
            <div class="clear">
            </div>
            <div id="sortContainer" class="float-left">
                <table id="table_columns" cellspacing="0" cellpadding="0" style="width: 625px!important; border-collapse: collapse;">
                </table>
            </div>
            <div class="clear">
            </div>
            <table cellspacing="0" cellpadding="5" style="width: 625px!important; border-collapse: collapse;">
                <tbody>
                    <tr class="GridNormalRow">
                        <td>
                            <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                <tbody>
                                    <tr>
                                        <td width="45px" align="center" class="GridViewNumRow"></td>
                                        <td class="border-right">
                                            <input type="text" id="tb_columnName" class="textEntry margin-left" onkeydown="AddNewRowTextBoxKeyDown(event)"
                                                maxlength="100" style="width: 220px;" />
                                        </td>
                                        <td class="border-right" align="center" width="135px">
                                            <select id="ddl_datatypes">
                                                <option value="nvarchar">nvarchar</option>
                                                <option value="DateTime">DateTime</option>
                                                <option value="Integer">Integer</option>
                                                <option value="Decimal">Decimal</option>
                                                <option value="Boolean">Boolean</option>
                                            </select>
                                        </td>
                                        <td class="border-right" align="center" width="75px">
                                            <input id="cb_nullable" type="checkbox" />
                                        </td>
                                        <td class="border-right" align="center" width="75px">
                                            <a href="#add" onclick="AddNewColumn();return false;" class="td-add-btn" title="Add Column"></a>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </td>
                    </tr>
                </tbody>
            </table>
        </asp:Panel>
        <div class="actions no-pad-left" style="margin-top: 40px;">
            <div class="clear-space">
            </div>
            <div class="editor_titles">
                <div class="title-line"></div>
                <h3>Custom Table List</h3>
            </div>
            <div class="clear-space">
            </div>
            <small><b class="pad-right-sml">Note:</b>If you are not an administrator you will not
            be able to see any of the created tables except your own.</small>
            <div class="clear-space-five">
            </div>
            <table class="myHeaderStyle" cellpadding="5" cellspacing="0" style="width: 750px!important;">
                <tbody>
                    <tr>
                        <td width="45px"></td>
                        <td>Table Name
                        </td>
                        <td align="center" width="180px">Created By
                        </td>
                        <td align="center" width="180px">Date Created
                        </td>
                        <td align="center" width="75px">Sidebar
                        </td>
                        <td align="center" width="75px">Actions
                        </td>
                    </tr>
                </tbody>
            </table>
            <div class="clear">
            </div>
            <asp:UpdatePanel ID="updatepnl_tableList" runat="server">
                <ContentTemplate>
                    <asp:HiddenField ID="hf_tableDeleteID" runat="server" ClientIDMode="Static" />
                    <asp:HiddenField ID="hf_tableUpdate" runat="server" OnValueChanged="hf_tableUpdate_Changed"
                        ClientIDMode="Static" />
                    <asp:HiddenField ID="hf_tableDelete" runat="server" OnValueChanged="hf_tableDelete_Changed"
                        ClientIDMode="Static" />
                    <asp:Panel ID="pnl_tableList" runat="server">
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="db_overlay" class="Modal-overlay" style="display: none;">
            <div class="password-element-align-db">
                <div id="db_modal" class="password-element-modal-db" style="display: none; min-height: 57px!important;">
                    <div id="confirmPassword">
                        <asp:UpdatePanel ID="updatepnl_passwordConfirm" runat="server">
                            <ContentTemplate>
                                <asp:Panel ID="pnl_passwordConfirm" runat="server" DefaultButton="btn_passwordConfirm">
                                    <b class="margin-right">Password:</b>
                                    <asp:TextBox ID="tb_passwordConfirm" runat="server" TextMode="Password" CssClass="TextBoxControls"></asp:TextBox>
                                    <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons margin-left"
                                        Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="openWSE.LoadingMessage1('Validating Password...');"
                                        Style="margin-top: -2px; margin-right: 5px!important" />
                                    <input type="button" class="input-buttons" value="Cancel" onclick="CancelDelete()"
                                        style="margin-top: -2px" />
                                </asp:Panel>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <div class="clear-space-five"></div>
                        <div class="float-left">
                            <small><b class="pad-right-sml">Note:</b>Enter in Created By user password.</small>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/customtables.js")%>'></script>
    </div>
</asp:Content>
