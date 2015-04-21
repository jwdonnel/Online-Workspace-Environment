<%@ page title="Custom Tables" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_CustomTables, App_Web_muevkmyy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .GridNormalRow
        {
            cursor: move;
        }

        .entryIDRow, .border-right
        {
            cursor: default !important;
        }

        .checkbox-click
        {
            cursor: default;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <asp:Panel ID="pnl_columnEditor" runat="server">
            <div class="table-settings-box no-border">
                <div class="td-settings-ctrl">
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
                        <div class="clear-space-five">
                        </div>
                        <asp:CheckBox ID="cb_addChart" runat="server" Text="&nbsp;Enable Data Chart"
                            ClientIDMode="Static" Checked="true" />
                        <div class="clear-space-five">
                        </div>
                        <asp:CheckBox ID="cb_allowNotifi" runat="server" Text="&nbsp;Enable Notifications - Notify users upon change"
                            ClientIDMode="Static" Checked="true" />
                        <div class="clear-space">
                        </div>
                        <div id="chart_selector">
                            <table>
                                <tr>
                                    <td class="settings-name-column">Chart Type
                                    </td>
                                    <td>
                                        <asp:DropDownList ID="ddl_ChartType" runat="server" CssClass="margin-left margin-right" ClientIDMode="Static">
                                        </asp:DropDownList>
                                    </td>
                                    <td>
                                        <img id="img_charttype" alt="charttype" src="../../Standard_Images/ChartTypes/area.png" class="margin-left-big margin-right" style="max-height: 80px;" />

                                    </td>
                                    <td>
                                        <small>This chart image will be used as the main App Icon.<br />
                                            Click <a id="lnk_chartTypeSetup" href="https://google-developers.appspot.com/chart/interactive/docs/gallery/areachart" target="_blank">HERE</a> to see how the data should be setup for this chart type.
                                        </small>
                                    </td>
                                </tr>
                            </table>
                            <div class="clear-space">
                            </div>
                        </div>
                    </div>
                    <table>
                        <tr id="tr-chart-title">
                            <td class="settings-name-column">Chart Title
                        <div class="clear-space"></div>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_chartTitle" runat="server" CssClass="textEntry margin-left float-left"
                                    MaxLength="250" Width="400px" ClientIDMode="Static"></asp:TextBox>
                                <div class="clear-space"></div>
                            </td>
                        </tr>
                        <tr>
                            <td class="settings-name-column">Table Name
                        <div class="clear-space"></div>
                            </td>
                            <td>
                                <asp:TextBox ID="tb_tablename" runat="server" CssClass="textEntry margin-left float-left"
                                    MaxLength="100" AutoCompleteType="Search" Width="175px" ClientIDMode="Static"></asp:TextBox>
                                <div class="clear-space"></div>
                            </td>
                        </tr>
                        <tr id="tr-usersallowed">
                            <td class="settings-name-column">Users Allowed To Edit
                                <div class="clear-space"></div>
                            </td>
                            <td>
                                <a href="#" class="margin-left input-buttons" onclick="openWSE.LoadModalWindow(true, 'UsersAllowed-element', 'Users Allowed To Edit');return false;"><span class="img-users float-left margin-right-sml"></span>Select Users</a>
                                <div id="UsersAllowed-element" class="Modal-element">
                                    <div class="Modal-overlay">
                                        <div class="Modal-element-align">
                                            <div class="Modal-element-modal" data-setwidth="650">
                                                <div class="ModalHeader">
                                                    <div>
                                                        <div class="app-head-button-holder-admin">
                                                            <a href="#" onclick="openWSE.LoadModalWindow(false, 'UsersAllowed-element', '');return false;"
                                                                class="ModalExitButton"></a>
                                                        </div>
                                                        <span class="Modal-title"></span>
                                                    </div>
                                                </div>
                                                <div class="ModalScrollContent">
                                                    <div class="ModalPadContent">
                                                        <asp:Panel ID="pnl_usersAllowedToEdit" runat="server">
                                                        </asp:Panel>
                                                        <div align="right">
                                                            <input type="button" class="input-buttons no-margin" value="Done" onclick="openWSE.LoadModalWindow(false, 'UsersAllowed-element', '');" />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="clear-space"></div>
                            </td>
                        </tr>
                        <tr style="display: none;">
                            <td class="settings-name-column">Import Excel File
                            </td>
                            <td>
                                <div id="div-excelFileUpload" class="margin-left">
                                    <input type="file" id="excelFileUpload" class="margin-right" /><input type="button" class="input-buttons" onclick="ImportExcelSpreadSheet()" value="Import" />
                                </div>
                                <div class="clear-space-two"></div>
                                <small class="margin-left">Only file extensions .xlsx and .xls are allowed</small>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
            <div class="table-settings-box">
                <div class="td-settings-title">
                    Column Editor
                </div>
                <div class="title-line"></div>
                <div class="td-settings-ctrl">
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
                                                        <option value="DateTime">Date</option>
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
                    <div class="clear" style="height: 20px;"></div>
                    <div id="createBtns" style="width: 625px;">
                        <a href="#" onclick="ClearControls();return false;" class="float-left">Clear Entries</a>
                        <input type="button" id="btn_createtable" class="input-buttons float-right no-margin" onclick="CreateTable()"
                            value="Create Table" />
                    </div>
                    <div id="editBtns" style="width: 625px; display: none;">
                        <input type="button" class="input-buttons" onclick="CancelUpdate()" value="Cancel" />
                        <input type="button" class="input-buttons float-right no-margin" onclick="UpdateTable()" value="Update Table" />
                    </div>
                    <div class="clear-space"></div>
                </div>
                <div class="td-settings-desc">
                    Entry ID row and TimeStamp row are already included
            in the list of columns. These rows cannot be deleted or edited. Certain characters are not allowed for the column name. These include [|&;$%@"<>()+,],
            and spaces. All new tables automatically create a app that
        you will have to install for each user after creating. The Table Name will also
        be the name of the app created.
                </div>
            </div>
        </asp:Panel>
        <div class="table-settings-box">
            <div class="td-settings-title">
                Custom Table List
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <table class="myHeaderStyle" cellpadding="5" cellspacing="0">
                    <tbody>
                        <tr>
                            <td width="45px"></td>
                            <td width="150px">Table Name
                            </td>
                            <td>Chart Title
                            </td>
                            <td align="center" width="180px">Created By
                            </td>
                            <td align="center" width="180px">Date Created
                            </td>
                            <td align="center" width="60px">Sidebar
                            </td>
                            <td align="center" width="60px">Notify
                            </td>
                            <td align="center" width="125px">Chart Type
                            </td>
                            <td align="center" width="100px">Users Allowed
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
            <div class="td-settings-desc">
                If you are not an administrator you will not be able to see any of the created tables except your own.
            </div>
        </div>
        <div id="password-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="490">
                        <div class='ModalHeader'>
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#close" onclick="CancelDelete();return false;" class="ModalExitButton"></a>
                                </div>
                                <span class='Modal-title'></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <asp:UpdatePanel ID="updatepnl_passwordConfirm" runat="server">
                                    <ContentTemplate>
                                        <asp:Panel ID="pnl_passwordConfirm" runat="server" DefaultButton="btn_passwordConfirm">
                                            <span class="password-hint">Enter the password of the user who created the custom table.</span>
                                            <div class="clear-space"></div>
                                            <b class="pad-right">Password</b>
                                            <asp:TextBox ID="tb_passwordConfirm" runat="server" TextMode="Password" CssClass="TextBoxControls"></asp:TextBox>
                                            <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons margin-left"
                                                Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="openWSE.LoadingMessage1('Validating Password...');"
                                                Style="margin-top: -2px; margin-right: 5px!important" />
                                            <input type="button" class="input-buttons" value="Cancel" onclick="CancelDelete()"
                                                style="margin-top: -2px" />
                                            <div class="clear-space"></div>
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="UsersAllowedEdit-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="650">
                        <div class="ModalHeader">
                            <div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/customtables.js")%>'></script>
    </div>
</asp:Content>
