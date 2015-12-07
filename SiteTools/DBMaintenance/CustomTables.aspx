<%@ page title="Custom Tables" language="C#" masterpagefile="~/Site.master" autoeventwireup="true" inherits="SiteTools_CustomTables, App_Web_h3zobxng" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
    <style type="text/css">
        .GridNormalRow
        {
            cursor: move;
        }

        .entryIDRow
        {
            display: none;
        }

        .entryIDRow, .border-right
        {
            cursor: default !important;
        }

        .checkbox-click
        {
            cursor: default;
        }

        .settings-name-column
        {
            width: 100px!important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="maincontent-padding margin-top">
        <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
        <asp:Panel ID="pnl_columnEditor" runat="server">
            <div id="ImportWizard-element" class="Modal-element">
                <div class="Modal-overlay">
                    <div class="Modal-element-align">
                        <div class="Modal-element-modal" data-setwidth="800">
                            <div class='ModalHeader'>
                                <div>
                                    <div class="app-head-button-holder-admin">
                                        <a href="#close" onclick="ConfirmCancelWizard();return false;" class="ModalExitButton"></a>
                                    </div>
                                    <span class='Modal-title'></span>
                                </div>
                            </div>
                            <div class="ModalScrollContent">
                                <div class="ModalPadContent">
                                    <ul id="create-wizard-steps" class="import-steps">
                                        <li class="steps step1">
                                            <div class="step-container">
                                                <div class="wizard-text-hint">Give your new table a custom name. This name will be applied for both the App and the table. Since these tables have a unique id that the user cannot see, you can name this table anything. The app name can be change at any time by going to the App Management->App Manager.</div>
                                                <asp:TextBox ID="tb_tablename" runat="server" CssClass="textEntry" MaxLength="150" AutoCompleteType="Search" ClientIDMode="Static" placeholder="Table Name"></asp:TextBox>
                                                <div class="clear-space"></div>
                                                <asp:TextBox ID="tb_description" runat="server" CssClass="textEntry" Width="98%" AutoCompleteType="Search" ClientIDMode="Static" placeholder="Description"></asp:TextBox>
                                            </div>
                                        </li>
                                        <li class="steps step2">
                                            <div class="step-container">
                                                <div class="wizard-text-hint">
                                                    All custom tables automatically create an app that you will have to install for each user after creating.<br />
                                                    Select the options you wish to include in this Custom Table.
                                                </div>
                                                <div id="div_InstallAfterLoad">
                                                    <asp:CheckBox ID="cb_InstallAfterLoad" runat="server" Text="&nbsp;Install app for current user after creation"
                                                        Checked="true" />
                                                    <div class="clear-space-five">
                                                    </div>
                                                </div>
                                                <div id="div_isPrivate">
                                                    <asp:CheckBox ID="cb_isPrivate" runat="server" Text="&nbsp;Make this app private (Only for me)"
                                                        ClientIDMode="Static" Checked="false" />
                                                    <div class="clear-space-five">
                                                    </div>
                                                </div>
                                                <asp:CheckBox ID="cb_allowNotifi" runat="server" Text="&nbsp;Enable Notifications - Notify users upon change"
                                                    ClientIDMode="Static" Checked="true" />
                                                <div class="clear-space">
                                                </div>
                                                <table>
                                                    <tr id="tr-usersallowed">
                                                        <td class="settings-name-column" valign="top" style="width: 150px!important;">
                                                            <div class="pad-top-big">Users Allowed To Edit</div>
                                                        </td>
                                                        <td>
                                                            <asp:Panel ID="pnl_usersAllowedToEdit" runat="server">
                                                            </asp:Panel>
                                                        </td>
                                                    </tr>
                                                    <tr id="tr-usersallowedEdit" style="display: none;">
                                                        <td class="settings-name-column" valign="top" style="width: 150px!important;">
                                                            <div class="pad-top-big">Users Allowed To Edit</div>
                                                        </td>
                                                        <td id="UsersAllowedEdit-element"></td>
                                                    </tr>
                                                    <tr style="display: none;">
                                                        <td class="settings-name-column" style="width: 150px!important;">Import Excel File
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
                                        </li>
                                        <li class="steps step3">
                                            <div class="step-container">
                                                <div class="wizard-text-hint">
                                                    You can customize the look of your table by adjusting the controls below.
                                                </div>
                                                <table cellpadding="10" cellspacing="10">
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;">Table Header Color
                                                        </td>
                                                        <td style="width: 105px;">
                                                            <asp:TextBox ID="tb_tableheadercolor" runat="server" CssClass="TextBoxEdit color" Width="65px"
                                                                MaxLength="6" ClientIDMode="Static"></asp:TextBox>
                                                        </td>
                                                        <td style="width: 110px;">
                                                            <asp:CheckBox ID="cb_usedefaultheadercolor" runat="server" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultheadercolor', 'tb_tableheadercolor');" /></td>
                                                        <td><small>Set the color of your header's background. The forecolor will be adjusted to a black/white depending on the color chosen.</small></td>
                                                    </tr>
                                                </table>
                                                <div class="clear"></div>
                                                <table cellpadding="10" cellspacing="10">
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;">Primary Row Color
                                                        </td>
                                                        <td style="width: 105px;">
                                                            <asp:TextBox ID="tb_primaryrowcolor" runat="server" CssClass="TextBoxEdit color" Width="65px"
                                                                MaxLength="6" ClientIDMode="Static"></asp:TextBox>
                                                        </td>
                                                        <td style="width: 110px;">
                                                            <asp:CheckBox ID="cb_usedefaultprimarycolor" runat="server" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultprimarycolor', 'tb_primaryrowcolor');" /></td>
                                                        <td><small>Set your primary row's color that will alternate every other row starting from the first row.</small></td>
                                                    </tr>
                                                </table>
                                                <div class="clear"></div>
                                                <table cellpadding="10" cellspacing="10">
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;">Alternative Row Color
                                                        </td>
                                                        <td style="width: 105px;">
                                                            <asp:TextBox ID="tb_alternativerowcolor" runat="server" CssClass="TextBoxEdit color" Width="65px"
                                                                MaxLength="6" ClientIDMode="Static"></asp:TextBox>
                                                        </td>
                                                        <td style="width: 110px;">
                                                            <asp:CheckBox ID="cb_usedefaultalternativecolor" runat="server" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultalternativecolor', 'tb_alternativerowcolor');" /></td>
                                                        <td><small>Set your alternative row's color that will alternate every other row starting from the second row.</small></td>
                                                    </tr>
                                                </table>
                                                <div class="clear"></div>
                                                <table cellpadding="10" cellspacing="10">
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;">Default Font Family
                                                        </td>
                                                        <td>
                                                            <select id="dd_fontfamilycustomization" onchange="fontFamilyChange();">
                                                                <option value="">Use Default</option>
                                                                <option value="Georgia, serif">Georgia, serif</option>
                                                                <option value="'Palatino Linotype', 'Book Antiqua', Palatino, serif">"Palatino Linotype", "Book Antiqua", Palatino, serif</option>
                                                                <option value="'Times New Roman', Times, serif">"Times New Roman", Times, serif</option>
                                                                <option value="Arial, Helvetica, sans-serif">Arial, Helvetica, sans-serif</option>
                                                                <option value="'Arial Black', Gadget, sans-serif">"Arial Black", Gadget, sans-serif</option>
                                                                <option value="'Comic Sans MS', cursive, sans-serif">"Comic Sans MS", cursive, sans-serif</option>
                                                                <option value="Impact, Charcoal, sans-serif">Impact, Charcoal, sans-serif</option>
                                                                <option value="'Lucida Sans Unicode', 'Lucida Grande', sans-serif">"Lucida Sans Unicode", "Lucida Grande", sans-serif</option>
                                                                <option value="Tahoma, Geneva, sans-serif">Tahoma, Geneva, sans-serif</option>
                                                                <option value="'Trebuchet MS', Helvetica, sans-serif">"Trebuchet MS", Helvetica, sans-serif</option>
                                                                <option value="Verdana, Geneva, sans-serif">Verdana, Geneva, sans-serif</option>
                                                                <option value="'Courier New', Courier, monospace">"Courier New", Courier, monospace</option>
                                                                <option value="'Lucida Console', Monaco, monospace">"Lucida Console", Monaco, monospace</option>
                                                            </select>
                                                            <div class="clear-space-five"></div>
                                                            <span id="span_fontfamilypreview" style="font-size: 15px;">This is the font preview</span>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="settings-name-column" style="width: 150px!important;"></td>
                                                        <td><small>Set your table's main font family that you want to display for the entire table.</small></td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </li>
                                        <li class="steps step4">
                                            <div class="step-container">
                                                <h3 class="font-bold">Main Table Editor</h3>
                                                <div class="clear-space"></div>
                                                <div class="wizard-text-hint">
                                                    ApplicationID, EntryID and TimeStamp rows are already included in the list of columns. These rows cannot be deleted or edited. Column lengths cannot exceed 3999 for an nvarchar type.
                                                </div>
                                                <div id="sortContainer">
                                                    <table id="table_columns" cellspacing="0" cellpadding="0" style="width: 100%; border-collapse: collapse;">
                                                        <tbody>
                                                        </tbody>
                                                    </table>
                                                </div>
                                                <div class="clear">
                                                </div>
                                                <table id="table_addcolumn_Controls" cellspacing="0" cellpadding="5" style="width: 100%; border-collapse: collapse;">
                                                    <tbody>
                                                        <tr class="GridNormalRow">
                                                            <td>
                                                                <table class="myItemStyle" cellpadding="5" cellspacing="0">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td width="45px" align="center" class="GridViewNumRow" style="cursor: default!important;"></td>
                                                                            <td class="border-right">
                                                                                <input type="text" id="tb_columnName" class="textEntry margin-left" onkeydown="AddNewRowTextBoxKeyDown(event)"
                                                                                    maxlength="100" style="width: 90%;" />
                                                                            </td>
                                                                            <td class="border-right" align="center" width="135px">
                                                                                <select id="ddl_datatypes">
                                                                                    <option value="nvarchar">nvarchar</option>
                                                                                    <option value="DateTime">Date</option>
                                                                                    <option value="DateTime">DateTime</option>
                                                                                    <option value="Integer">Integer</option>
                                                                                    <option value="Money">Money</option>
                                                                                    <option value="Decimal">Decimal</option>
                                                                                    <option value="Boolean">Boolean</option>
                                                                                </select>
                                                                            </td>
                                                                            <td class="border-right" align="center" width="75px">
                                                                                <input type="text" class="textEntry" id="tb_length" maxlength="4" value="100" onkeydown="AddNewRowTextBoxKeyDown(event)" style="width: 55px;" />
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
                                            </div>
                                        </li>
                                        <li class="steps step5">
                                            <div class="step-container">
                                                <div class="wizard-text-hint">You can enable the table data chart below. Select which chart type you want to show along with which columns to associate the chart with.</div>
                                                <div id="chart_selector">
                                                    <table width="100%">
                                                        <tr>
                                                            <td class="settings-name-column">Chart Type
                                                            </td>
                                                            <td>
                                                                <asp:DropDownList ID="ddl_ChartType" runat="server" CssClass="margin-right-big" ClientIDMode="Static" onchange="ChartTypeChangeEvent();">
                                                                </asp:DropDownList>
                                                            </td>
                                                            <td id="img_charttype_holder">
                                                                <img id="img_charttype" alt="charttype" src="../../Standard_Images/ChartTypes/area.png" class="margin-right-big" style="max-height: 50px;" />
                                                            </td>
                                                            <td>
                                                                <small>This chart image will be used as the main App Icon.<br />
                                                                    Click <a id="lnk_chartTypeSetup" href="https://google-developers.appspot.com/chart/interactive/docs/gallery/areachart" target="_blank">HERE</a> to see how the data should be setup for this chart type.</small>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    <div id="chart-title-holder">
                                                        <div class="clear-space"></div>
                                                        <table id="tr-chart-title">
                                                            <tr>
                                                                <td class="settings-name-column">Chart Title
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tb_chartTitle" runat="server" CssClass="textEntry" AutoCompleteType="Search" MaxLength="150" ClientIDMode="Static"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="chart-column-selector">
                                                        <div class="clear-space"></div>
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="settings-name-column">Data Columns
                                                                </td>
                                                                <td width="240px">
                                                                    <div id="chart-column-selector-holder"></div>
                                                                </td>
                                                                <td>
                                                                    <small>Select the columns you wish to add to use on the chart.</small>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </div>
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                            <div class="ModalButtonHolder">
                                <input type="button" class="input-buttons float-right no-margin" value="Cancel" onclick="ConfirmCancelWizard();" />
                                <input type="button" class="input-buttons prev-step float-left" value="Previous" onclick="CreatePrevStep();" style="display: none;" />
                                <input type="button" class="input-buttons next-step float-left no-margin" value="Next" onclick="CreateNextStep();" style="" />
                                <input id="btn_finishImportWizard" type="button" class="input-buttons float-left no-margin" value="Create" onclick="CreateUpdateTable();" style="display: none;" />
                                <input id="btn_finishSummaryWizard" type="button" class="input-buttons float-left no-margin" value="Create" onclick="CreateUpdateSummary();" style="display: none;" />
                                <div class="clear"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnl_TableEntries" runat="server" CssClass="pnl-section pad-top-big margin-top" data-title="Custom Tables">
            <div class="clear"></div>
            <a href="#" id="btn_customTable_create" runat="server" class="input-buttons-create margin-right-big margin-bottom float-left" onclick="StartCreateWizard();return false;">Table Wizard</a>
            <asp:UpdatePanel ID="updatepnl_tableList" runat="server">
                <ContentTemplate>
                    <div class="searchwrapper float-left" style="width: 350px; margin-top: 3px;">
                        <asp:Panel ID="Panel1_customtables" runat="server" DefaultButton="imgbtn_search">
                            <asp:TextBox ID="tb_searchcustomtables" runat="server" CssClass="searchbox" Font-Size="Small"
                                onfocus="if(this.value=='Search Custom Tables')this.value=''" onblur="if(this.value=='')this.value='Search Custom Tables'"
                                Text="Search Custom Tables"></asp:TextBox>
                            <asp:LinkButton ID="imgbtn_clearsearch" runat="server" ToolTip="Clear search" CssClass="searchbox_clear RandomActionBtns"
                                OnClick="imgbtn_clearsearch_Click" />
                            <asp:LinkButton ID="imgbtn_search" runat="server" ToolTip="Start search" CssClass="searchbox_submit RandomActionBtns"
                                OnClick="imgbtn_search_Click" />
                        </asp:Panel>
                    </div>
                    <div class="clear-space"></div>
                    <asp:HiddenField ID="hf_tableDeleteID" runat="server" ClientIDMode="Static" />
                    <asp:HiddenField ID="hf_tableUpdate" runat="server" OnValueChanged="hf_tableUpdate_Changed"
                        ClientIDMode="Static" />
                    <asp:HiddenField ID="hf_tableDelete" runat="server" OnValueChanged="hf_tableDelete_Changed"
                        ClientIDMode="Static" />
                    <asp:Panel ID="pnl_tableList" runat="server">
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>
        <div class="table-settings-box no-border">
            <div class="td-settings-desc">
                If you are not an administrator you will not be able to see any of the created tables except your own.
            </div>
        </div>
        <div id="password-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="400">
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
                                            <div class="clear-space"></div>
                                            <div class="clear-space"></div>
                                            <input type="button" class="input-buttons no-margin float-right" value="Cancel" onclick="CancelDelete()" />
                                            <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons float-left"
                                                Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="openWSE.LoadingMessage1('Validating Password...');" />
                                            <div class="clear"></div>
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript" src='<%=ResolveUrl("~/WebControls/jscolor/jscolor.js")%>'></script>
        <script type="text/javascript" src='<%=ResolveUrl("~/Scripts/SiteTools/customtables.js")%>'></script>
    </div>
</asp:Content>
