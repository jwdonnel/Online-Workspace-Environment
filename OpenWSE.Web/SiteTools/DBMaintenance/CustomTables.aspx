<%@ Page Title="Custom Tables" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeFile="CustomTables.aspx.cs" Inherits="SiteTools_CustomTables" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <asp:Literal ID="ltl_locked" runat="server"></asp:Literal>
    <asp:Panel ID="pnl_columnEditor" runat="server">
        <div id="ImportWizard-element" class="Modal-element">
            <div class="Modal-overlay">
                <div class="Modal-element-align">
                    <div class="Modal-element-modal" data-setwidth="800">
                        <div class="ModalHeader">
                            <div>
                                <div class="app-head-button-holder-admin">
                                    <a href="#close" onclick="ConfirmCancelWizard();return false;" class="ModalExitButton"></a>
                                </div>
                                <span class="Modal-title"></span>
                            </div>
                        </div>
                        <div class="ModalScrollContent">
                            <div class="ModalPadContent">
                                <ul id="create-wizard-steps" class="import-steps">
                                    <li class="steps step1">
                                        <div class="step-container">
                                            Give your table a name and a description. New tables will use the name for the App name. To change the app name of an existing table, you can be change it in the App Management->App Manager page.
                                            <div class="clear-space"></div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Name</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_tablename" runat="server" CssClass="textEntry" MaxLength="150" Width="100%" AutoCompleteType="None" ClientIDMode="Static"></asp:TextBox>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Description</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_description" runat="server" CssClass="textEntry" Width="100%" AutoCompleteType="None" ClientIDMode="Static"></asp:TextBox>
                                            </div>
                                        </div>
                                    </li>
                                    <li class="steps step2">
                                        <div class="step-container">
                                            <span class="font-bold font-underline">Main Table Editor</span>
                                            <div class="clear-space"></div>
                                            ApplicationID, EntryID and TimeStamp rows are already included in the list of columns. These rows cannot be deleted or edited. Column lengths cannot exceed 3999 for an nvarchar type.
                                            <div class="clear-space"></div>
                                            <div id="sortContainer">
                                                <table id="table_columns" cellspacing="0" cellpadding="0" style="width: 100%; border-collapse: collapse;">
                                                    <tbody>
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="clear">
                                            </div>
                                            <table id="table_addcolumn_Controls" cellspacing="0" cellpadding="0" style="width: 100%; border-collapse: collapse;">
                                                <tbody>
                                                    <tr class="GridNormalRow addItemRow">
                                                        <td width="35px" class="GridViewNumRow" style="cursor: default!important;"></td>
                                                        <td>
                                                            <input type="text" id="tb_columnName" class="textEntry" onkeydown="AddNewRowTextBoxKeyDown(event)"
                                                                maxlength="100" style="width: 95%;" />
                                                        </td>
                                                        <td align="left" width="119px">
                                                            <select id="ddl_datatypes">
                                                                <option value="nvarchar">nvarchar</option>
                                                                <option value="Date">Date</option>
                                                                <option value="DateTime">DateTime</option>
                                                                <option value="Integer">Integer</option>
                                                                <option value="Money">Money</option>
                                                                <option value="Decimal">Decimal</option>
                                                                <option value="Boolean">Boolean</option>
                                                            </select>
                                                        </td>
                                                        <td align="left" width="59px">
                                                            <input type="number" class="textEntry" id="tb_length" value="100" onkeydown="AddNewRowTextBoxKeyDown(event)" step="1" style="width: 95%;" />
                                                        </td>
                                                        <td align="left" width="59px">
                                                            <input id="cb_nullable" type="checkbox" />
                                                        </td>
                                                        <td align="center" width="64px">
                                                            <a href="#add" onclick="AddNewColumn();return false;" class="td-add-btn" title="Add Column"></a>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                            <div class="clear-space"></div>
                                        </div>
                                    </li>
                                    <li class="steps step3">
                                        <div class="step-container">
                                            <span class="font-bold font-underline">Table Default Values</span>
                                            <div class="clear-space"></div>
                                            Set the default values for each column. These values will appear when creating a new record only.
                                            <div class="clear-space"></div>
                                            <table id="table_table_defaultvalues" cellspacing="0" cellpadding="0" style="width: 100%; border-collapse: collapse;">
                                                <tbody>
                                                    <tr class="myHeaderStyle">
                                                        <td width="45px"></td>
                                                        <td>Column Name</td>
                                                        <td>Default Value</td>
                                                        <td class="edit-column-2-items"></td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                            <div class="clear-space"></div>
                                        </div>
                                    </li>
                                    <li class="steps step4">
                                        <div class="step-container">
                                            <span class="font-bold font-underline">Table Summaries</span>
                                            <div class="clear-space"></div>
                                            Create a summary that will displayed at the top of the App where you can use a set of predefined formulas.
                                            <div class="clear-space"></div>
                                            <div id="sortContainer_Summary">
                                                <table id="table_columns_summary" cellspacing="0" cellpadding="0" style="width: 100%; border-collapse: collapse;">
                                                    <tbody>
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="clear">
                                            </div>
                                            <table id="table_addcolumn_summary_Controls" cellspacing="0" cellpadding="0" style="width: 100%; border-collapse: collapse;">
                                                <tbody>
                                                    <tr class="GridNormalRow addItemRow">
                                                        <td width="35px" class="GridViewNumRow" style="cursor: default!important;"></td>
                                                        <td>
                                                            <input type="text" id="tb_summaryname" class="textEntry" onkeydown="AddNewSummaryRowTextBoxKeyDown(event)"
                                                                maxlength="100" style="width: 95%;" />
                                                        </td>
                                                        <td align="left" width="184px">
                                                            <select id="dd_columnSummary" style="width: 95%;">
                                                            </select>
                                                        </td>
                                                        <td align="left" width="119px">
                                                            <select id="dd_formulatype" onchange="onFormulaTypeChange(this);">
                                                                <option value="Sum">Sum</option>
                                                                <option value="Min">Min</option>
                                                                <option value="Max">Max</option>
                                                                <option value="Average">Average</option>
                                                                <option value="SumIf">SumIf</option>
                                                                <option value="MinIf">MinIf</option>
                                                                <option value="MaxIf">MaxIf</option>
                                                                <option value="AverageIf">AverageIf</option>
                                                            </select>
                                                        </td>
                                                        <td align="center" width="64px">
                                                            <a href="#add" onclick="AddNewSummaryColumn();return false;" class="td-add-btn" title="Add Column"></a>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                            <div class="clear-space"></div>
                                        </div>
                                    </li>
                                    <li class="steps step5">
                                        <div class="step-container">
                                            All custom tables automatically create an app that you will have to install for each user after creating. Select the options you wish to include in this Custom Table.
                                            <div class="clear-space"></div>
                                            <div id="div_InstallAfterLoad">
                                                <asp:CheckBox ID="cb_InstallAfterLoad" runat="server" CssClass="cb_style" Text="&nbsp;Install app for current user after creation" Checked="true" />
                                                <div class="clear-space"></div>
                                            </div>
                                            <div id="div_isPrivate">
                                                <asp:CheckBox ID="cb_isPrivate" runat="server" CssClass="cb_style" Text="&nbsp;Make this app private (Only for me)" ClientIDMode="Static" Checked="false" />
                                                <div class="clear-space"></div>
                                            </div>
                                            <asp:CheckBox ID="cb_allowNotifi" runat="server" CssClass="cb_style" Text="&nbsp;Enable Notifications - Notify users upon change" ClientIDMode="Static" Checked="true" />
                                            <div class="clear-space"></div>
                                            <div class="clear-space"></div>
                                            <div id="tr-usersallowed" class="input-settings-holder">
                                                <span class="font-bold">Users Allowed To Edit</span>
                                                <div class="clear-space-two"></div>
                                                <asp:Panel ID="pnl_usersAllowedToEdit" runat="server">
                                                </asp:Panel>
                                            </div>
                                            <div id="tr-usersallowedEdit" class="input-settings-holder" style="display: none;">
                                                <span class="font-bold">Users Allowed To Edit</span>
                                                <div class="clear-space-two"></div>
                                                <div id="UsersAllowedEdit-element"></div>
                                            </div>
                                            <div class="input-settings-holder" style="display: none;">
                                                <span class="font-bold">Import Excel File</span>
                                                <div class="clear-space-two"></div>
                                                <div id="div-excelFileUpload" class="margin-left">
                                                    <input type="file" id="excelFileUpload" class="margin-right" /><input type="button" class="input-buttons" onclick="ImportExcelSpreadSheet()" value="Import" />
                                                </div>
                                                <div class="clear-space-two"></div>
                                                <small class="margin-left">Only file extensions .xlsx and .xls are allowed</small>
                                            </div>
                                            <div class="clear"></div>
                                        </div>
                                    </li>
                                    <li class="steps step6">
                                        <div class="step-container">
                                            <span class="font-bold font-underline">Table Customizations</span>
                                            <div class="clear-space"></div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Table Header Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_tableheadercolor" runat="server" CssClass="textEntry-noWidth margin-right float-left" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_usedefaultheadercolor" runat="server" CssClass="cb_style inline-block" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultheadercolor', 'tb_tableheadercolor');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set the color of your header's background. The forecolor will be adjusted to a black/white depending on the color chosen.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Primary Row Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_primaryrowcolor" runat="server" CssClass="textEntry-noWidth margin-right float-left" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_usedefaultprimarycolor" runat="server" CssClass="cb_style inline-block" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultprimarycolor', 'tb_primaryrowcolor');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set your primary row's color that will alternate every other row starting from the first row.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Alternative Row Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_alternativerowcolor" runat="server" CssClass="textEntry-noWidth margin-right float-left" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_usedefaultalternativecolor" runat="server" CssClass="cb_style inline-block" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_usedefaultalternativecolor', 'tb_alternativerowcolor');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set your alternative row's color that will alternate every other row starting from the second row.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Show Row Counts</span>
                                                <div class="clear-space-two"></div>
                                                <div class="field switch inline-block">
                                                    <asp:RadioButton ID="rb_showrowcounts_on" ClientIDMode="Static" runat="server" Text="Yes" CssClass="cb-enable no-postback" Checked="true" />
                                                    <asp:RadioButton ID="rb_showrowcounts_off" ClientIDMode="Static" runat="server" Text="No" CssClass="cb-disable no-postback" />
                                                </div>
                                                <div class="clear-space-two"></div>
                                                <small>Set your alternative row's color that will alternate every other row starting from the second row.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Table View Style</span>
                                                <div class="clear-space-two"></div>
                                                <select id="dd_viewstyle">
                                                    <option value="default">Use Default</option>
                                                    <option value="excel">Excel Spreadsheet</option>
                                                </select>
                                                <div class="clear-space-two"></div>
                                                <small>Set your table's view style. You can select between the default style and an Excel Spreadsheet style. Styles will have there own way of editing an item.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Default Font Family</span>
                                                <div class="clear-space-two"></div>
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
                                                <div class="clear-space-two"></div>
                                                <small>Set your table's main font family that you want to display for the entire table.</small>
                                            </div>
                                            <div class="clear"></div>
                                        </div>
                                    </li>
                                    <li class="steps step7">
                                        <div class="step-container">
                                            <span class="font-bold font-underline">App Customizations</span>
                                            <div class="clear-space"></div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Month Selector</span>
                                                <div class="clear-space-two"></div>
                                                <select id="dd_monthselector" class="margin-right" style="max-width: 95%;"></select>
                                                <div class="clear-space-two"></div>
                                                <small>Set the month selector that is used for the sidebar selector on the app. By default, the month selector is the TimeStamp (Which is hidden).</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">App Icon</span>
                                                <div class="clear-space-two"></div>
                                                <div class="float-left">
                                                    <a href="#" id="urlIcon-tab" onclick="ChangeIconUploadType(0);return false;">Click here to use Url Image</a>
                                                    <a href="#" id="uploadIcon-tab" onclick="ChangeIconUploadType(1);return false;" style="display: none;">Click here to Upload Icon</a>
                                                </div>
                                                <div class="clear-space-five"></div>
                                                <div id="uploadIcon">
                                                    <asp:FileUpload ID="fu_image_create" runat="server" />
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <small><b>.png</b> <b>.jpeg</b> and <b>.gif</b> only allowed</small>
                                                </div>
                                                <div id="urlIcon" style="display: none">
                                                    <asp:TextBox ID="tb_imageurl" runat="server" CssClass="textEntry" Width="95%" placeholder="Image Url"></asp:TextBox>
                                                    <div class="clear-space-five">
                                                    </div>
                                                    <small><b>.png</b> <b>.jpeg</b> and <b>.gif</b> only allowed</small>
                                                </div>
                                                <div class="clear-space-two"></div>
                                                <small>Upload or copy and paste a url image to save as the App Icon.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Show Description</span>
                                                <div class="clear-space-two"></div>
                                                <div class="field switch inline-block">
                                                    <asp:RadioButton ID="rb_showdescription_on" ClientIDMode="Static" runat="server" Text="Yes" CssClass="cb-enable no-postback" Checked="true" />
                                                    <asp:RadioButton ID="rb_showdescription_off" ClientIDMode="Static" runat="server" Text="No" CssClass="cb-disable no-postback" />
                                                </div>
                                                <div class="clear-space-two"></div>
                                                <small>Set to Yes to show the description of your table below the title on the app.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Title Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_apptitle_color" runat="server" CssClass="textEntry-noWidth float-left margin-right" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_apptitle_color_default" runat="server" CssClass="cb_style inline-block" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_apptitle_color_default', 'tb_apptitle_color');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set the app title color or use the default.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Header Color</span>
                                                <div class="clear-space-two"></div>
                                                <asp:TextBox ID="tb_appbackground_color" runat="server" CssClass="textEntry-noWidth float-left margin-right" TextMode="Color" Width="75px" MaxLength="7" ClientIDMode="Static"></asp:TextBox>
                                                <asp:CheckBox ID="cb_appbackground_color_default" runat="server" CssClass="cb_style inline-block" Style="margin-top: 7px;" ClientIDMode="Static" Text="&nbsp;Use Default" onchange="UseDefaultChanged('cb_appbackground_color_default', 'tb_appbackground_color');" />
                                                <div class="clear-space-two"></div>
                                                <small>Set the app header background color or use the default.</small>
                                            </div>
                                            <div class="input-settings-holder">
                                                <span class="font-bold">Header Image</span>
                                                <div class="clear-space-two"></div>
                                                <input type="text" id="tb_appbackground_image" class="textEntry" placeholder="Image Url" style="width: 100%" />
                                                <div class="clear-space-two"></div>
                                                <small>Set the app header background image (if wanted). Leave blank if you do not want an image as the background.</small>
                                            </div>
                                            <div class="clear"></div>
                                        </div>
                                    </li>
                                    <li class="steps step8">
                                        <div class="step-container">
                                            You can enable the table data chart below. Select which chart type you want to show along with which columns to associate the chart with.
                                            <div class="clear-space"></div>
                                            <div id="chart_selector">
                                                <div class="input-settings-holder">
                                                    <span class="font-bold">Chart Type</span>
                                                    <div class="clear-space-two"></div>
                                                    <asp:DropDownList ID="ddl_ChartType" runat="server" ClientIDMode="Static" onchange="ChartTypeChangeEvent();"></asp:DropDownList>
                                                    <div id="img_charttype_holder" class="margin-top-sml">
                                                        <img id="img_charttype" alt="charttype" src="../../Standard_Images/ChartTypes/area.png" style="height: 50px;" />
                                                    </div>
                                                    <div class="clear-space-two"></div>
                                                    <small>This chart image will be used as the main App Icon. Click <a id="lnk_chartTypeSetup" href="https://google-developers.appspot.com/chart/interactive/docs/gallery/areachart" target="_blank">here</a> to see how the data should be setup for this chart type.</small>
                                                </div>
                                                <div id="chart-title-holder">
                                                    <div id="tr-chart-title">
                                                        <div class="input-settings-holder">
                                                            <span class="font-bold">Chart Title</span>
                                                            <div class="clear-space-two"></div>
                                                            <asp:TextBox ID="tb_chartTitle" runat="server" CssClass="textEntry" AutoCompleteType="None" MaxLength="150" Width="100%" ClientIDMode="Static"></asp:TextBox>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div id="chart-column-selector">
                                                    <div class="input-settings-holder">
                                                        <span class="font-bold">Data Columns</span>
                                                        <div class="clear-space-two"></div>
                                                        <div id="chart-column-selector-holder"></div>
                                                        <div class="clear-space-two"></div>
                                                        <small>Select the columns you wish to add to use on the chart.</small>
                                                    </div>
                                                </div>
                                                <div class="clear"></div>
                                            </div>
                                        </div>
                                    </li>
                                </ul>
                            </div>
                        </div>
                        <div class="ModalButtonHolder">
                            <input type="button" class="input-buttons modal-cancel-btn" value="Cancel" onclick="ConfirmCancelWizard();" />
                            <input type="button" class="input-buttons prev-step modal-ok-btn" value="Previous" onclick="CreatePrevStep();" style="display: none;" />
                            <input type="button" class="input-buttons next-step modal-ok-btn" value="Next" onclick="CreateNextStep();" style="" />
                            <input id="btn_finishImportWizard" type="button" class="input-buttons modal-ok-btn" value="Create" onclick="CreateUpdateTable();" style="display: none;" />
                            <input id="btn_finishSummaryWizard" type="button" class="input-buttons modal-ok-btn" value="Create" onclick="CreateUpdateSummary();" style="display: none;" />
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnl_TableEntries" runat="server">
        <div id="div_backgroundserviceTable" runat="server" class="table-settings-box">
            <div id="pageTdSettingsTitle" runat="server" class="td-settings-title">
                Custom Tables
            </div>
            <div class="title-line"></div>
            <div class="td-settings-ctrl">
                <a id="btn_customTable_create" runat="server" class="input-buttons-create" onclick="StartCreateWizard();return false;">Table Wizard</a>
                <div class="clear-space"></div>
                <asp:UpdatePanel ID="updatepnl_tableList" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:HiddenField ID="hf_tableDeleteID" runat="server" ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_tableUpdate" runat="server" OnValueChanged="hf_tableUpdate_Changed"
                            ClientIDMode="Static" />
                        <asp:HiddenField ID="hf_tableDelete" runat="server" OnValueChanged="hf_tableDelete_Changed"
                            ClientIDMode="Static" />
                        <asp:Panel ID="pnl_tableList" runat="server">
                        </asp:Panel>
                        <asp:Panel ID="pnl_tableDetailList" runat="server">
                        </asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="hf_tableUpdate" />
                        <asp:AsyncPostBackTrigger ControlID="hf_tableDelete" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="td-settings-desc">
                If you are not an administrator you will not be able to see any of the created tables except your own.
            </div>
        </div>
    </asp:Panel>
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
                                        Enter the password of the user who created the custom table.
                                        <div class="clear-space"></div>
                                        <div class="input-settings-holder">
                                            <span class="font-bold">Password</span>
                                            <div class="clear-space-two"></div>
                                            <asp:TextBox ID="tb_passwordConfirm" runat="server" TextMode="Password" CssClass="textEntry-noWidth" Width="99%"></asp:TextBox>
                                        </div>
                                        <div class="clear-space"></div>
                                        <input type="button" class="input-buttons no-margin float-right" value="Cancel" onclick="CancelDelete()" />
                                        <asp:Button ID="btn_passwordConfirm" runat="server" CssClass="input-buttons float-left"
                                            Text="Confirm" OnClick="btn_passwordConfirm_Clicked" OnClientClick="loadingPopup.Message('Validating Password...');" />
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
</asp:Content>
