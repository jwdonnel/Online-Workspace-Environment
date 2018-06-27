<%@ Control Language="C#" AutoEventWireup="true" ClassName="zbhxnuwopf" ClientIDMode="Static" Inherits="CustomTablesAppLoader" %>

<div class="custom-table-timermarker">
    <div id="pnl_zbhxnuwopf_tableView" class="custom-table-tableView-holder" data-viewmode="">
         <div id="app_title_bgcolor_zbhxnuwopf" runat="server" class="pad-all app-title-bg-color">
             <div class="float-left">
                 <asp:Image ID="img_Title_zbhxnuwopf" runat="server" CssClass="app-img-titlebar" />
                 <asp:Label ID="lbl_Title_zbhxnuwopf" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
             </div>
             <div id="search_zbhxnuwopf_divider" class="search-customtable-divider" style="display: none;"></div>
             <div id="search_zbhxnuwopf_holder" class="float-right">
                 <div class="searchwrapper">
                     <div class="searchboxholder">
                         <input id="tb_search_zbhxnuwopf" type="text" placeholder="Search table..." class="searchbox" onkeypress="customTables.KeyPressSearch(event, 'zbhxnuwopf')" />
                     </div>
                     <a href="#" class="searchbox_submit" onclick="customTables.Search('zbhxnuwopf');return false;"></a>
                     <a href="#" onclick="$('#tb_search_zbhxnuwopf').val('');customTables.Refresh('zbhxnuwopf');return false;" class="searchbox_clear"></a>
                 </div>
             </div>
             <div class="clear"></div>
             <div id="appdesc_zbhxnuwopf" runat="server" class="custom-table-appdescription" visible="false"></div>
         </div>
         <div class="img-appmenu app-menu-btn pad-all" onclick="customTables.MenuClick('zbhxnuwopf');" title="View Menu"></div>
         <div class="td-delete-btn app-delete-month pad-all" onclick="customTables.DeleteMonth('zbhxnuwopf');" title="Delete Month"></div>
         <div class="img-copy app-copy-data pad-all" onclick="customTables.CopyData('zbhxnuwopf');" title="Copy Data"></div>
         <asp:HiddenField ID="hf_zbhxnuwopf_customizations" runat="server" Value="" />
         <asp:HiddenField ID="hf_zbhxnuwopf_summaryData" runat="server" Value="" />
         <asp:HiddenField ID="hf_zbhxnuwopf_monthSelector" runat="server" Value="" />
         <asp:HiddenField ID="hf_zbhxnuwopf_tableview" runat="server" Value="default" />
         <div id="data-holder-zbhxnuwopf" class="data-holder-customtable-holder"></div>
     </div>
     <div id="pnl_zbhxnuwopf_chartView" style="display: none;"></div>
     <div id="zbhxnuwopf-sidebar-menu" class="customtable-sidebar-menu">
         <div class="pad-all customtable-sidebar-innercontent">
             <div class="img-close-dark app-menu-btn" onclick="customTables.MenuClick('zbhxnuwopf');" title="Close Menu"></div>
             <a href="#" class="float-right img-refresh" onclick="customTables.Refresh('zbhxnuwopf');return false;" title="Refresh Table"></a>
             <div class="clear-space"></div>
             <ul>
                 <li id="zbhxnuwopf-sidebar-menu-viewallrecords">
                     <div class="section-pad" style="padding-top: 5px !important;">
                         <a id="btn_zbhxnuwopf_addRecord" runat="server" class="sidebar-menu-buttons" onclick="customTables.AddRecord_MenuClick('zbhxnuwopf');return false;"><span class="td-add-btn sidebar-menu-img"></span>Create New Record</a>
                         <div class="clear"></div>
                         <a id="btn_zbhxnuwopf_viewRecords" class="sidebar-menu-buttons" onclick="customTables.ViewRecords_MenuClick('zbhxnuwopf');return false;"><span class="td-details-btn sidebar-menu-img"></span>View Record List</a>
                         <div class="clear"></div>
                         <a id="btn_zbhxnuwopf_viewSummary" class="sidebar-menu-buttons" onclick="customTables.ViewSummary_MenuClick('zbhxnuwopf');return false;"><span class="td-view-btn sidebar-menu-img"></span>View Summary</a>
                     </div>
                 </li>
                 <li id="li_zbhxnuwopf_chartType" runat="server">
                     <asp:Panel ID="pnl_zbhxnuwopf_chartType" CssClass="custom-table-view-chart section-pad" runat="server">
                         <a href="#" onclick="customTables.ViewChart('zbhxnuwopf', this);return false;">
                             <asp:HiddenField ID="hf_zbhxnuwopf_chartType" runat="server" />
                             <asp:HiddenField ID="hf_zbhxnuwopf_chartTitle" runat="server" />
                             <asp:HiddenField ID="hf_zbhxnuwopf_chartColumns" runat="server" />
                             <asp:Image ID="img_zbhxnuwopf_chartType" runat="server" />
                             <span>View Data in Chart</span>
                         </a>
                         <div class="clear"></div>
                     </asp:Panel>
                 </li>
                 <li id="li_zbhxnuwopf_viewMode" runat="server">
                     <div class="section-pad">
                         <h3 class="section-pad-title">View Mode</h3>
                         <div class="desktop-img sidebar-customtables-img" title="Desktop" onclick="customTables.ChangeViewMode('zbhxnuwopf', 'desktop')"></div>
                         <div class="mobile-img sidebar-customtables-img" title="Mobile" onclick="customTables.ChangeViewMode('zbhxnuwopf', 'mobile')"></div>
                         <div class="clear"></div>
                     </div>
                 </li>
                 <li>
                     <div class="section-pad">
                         <h3 class="section-pad-title">Font Size</h3>
                         <div class="font-larger-img sidebar-customtables-img" title="Increase Font Size" onclick="customTables.ChangeFont('zbhxnuwopf', this)"></div>
                         <div class="font-smaller-img sidebar-customtables-img" title="Descrease Font Size" onclick="customTables.ChangeFont('zbhxnuwopf', this)"></div>
                         <div class="clear"></div>
                         <select id="font-size-selector-zbhxnuwopf" style="display: none;">
                             <option value="x-small">X-Small</option>
                             <option value="small" selected="selected">Small</option>
                             <option value="medium">Medium</option>
                             <option value="large">Large</option>
                             <option value="x-large">X-Large</option>
                         </select>
                     </div>
                 </li>
                 <li>
                     <div class="section-pad">
                         <h3 class="section-pad-title" style="padding-top: 3px !important;">Records to Pull</h3>
                         <select id="RecordstoSelect_zbhxnuwopf" class="float-right" onchange="customTables.RecordstoSelect('zbhxnuwopf')">
                             <option value="5">5</option>
                             <option value="10">10</option>
                             <option value="25">25</option>
                             <option value="50">50</option>
                             <option value="75">75</option>
                             <option value="100">100</option>
                             <option value="200">200</option>
                             <option value="all" selected="selected">All</option>
                         </select>
                         <div class="clear"></div>
                     </div>
                 </li>
                 <li>
                     <div class="section-pad">
                         <h3>Export to Excel</h3>
                         <div class="clear-space-five"></div>
                         <div class="custom-table-excelexport-btns">
                             <input id="btnExport-zbhxnuwopf" type="button" class="input-buttons" onclick="customTables.OpenExcelExportModal('zbhxnuwopf')" title="Select a date to export to an Excel file" value="Export Dates" />
                             <input id="btnExportAll-zbhxnuwopf" type="button" class="input-buttons no-margin" onclick="customTables.ExportToExcelAll('zbhxnuwopf')" title="Export all data to Excel file" value="Export All" />
                             <div class="clear"></div>
                         </div>
                         <div id="zbhxnuwopf-ExcelExport-element" class="Modal-element">
                             <div class="Modal-element-align">
                                 <div class="Modal-element-modal ui-draggable ui-draggable-handle" data-setwidth="350">
                                     <div class="ModalHeader">
                                         <div>
                                             <div class="app-head-button-holder-admin"><a href="#" onclick="customTables.CloseExcelExportModal('zbhxnuwopf');return false;" class="ModalExitButton"></a></div>
                                             <span class="Modal-title"></span>
                                         </div>
                                     </div>
                                     <div class="ModalPadContent">
                                         Select the dates you wish to export. The dates are based off of the Month Selector column.
                                         <div class="clear-space"></div>
                                         <div align="center">
                                             <input id="tb_exportDateFrom_zbhxnuwopf" type="text" class="textEntry" style="width: 95px;" />
                                             <span class="font-bold pad-left-sml pad-right-sml">To</span>
                                             <input id="tb_exportDateTo_zbhxnuwopf" type="text" class="textEntry" style="width: 95px;" />
                                             <div class="clear"></div>
                                         </div>
                                         <div class="clear-space"></div>
                                         <div class="clear-space"></div>
                                         <input class="input-buttons float-left" type="button" value="Export" onclick="customTables.ExportToExcelDates('zbhxnuwopf');" />
                                         <input class="input-buttons float-right no-margin" type="button" value="Cancel" onclick="customTables.CloseExcelExportModal('zbhxnuwopf');" />
                                         <div class="clear"></div>
                                     </div>
                                 </div>
                             </div>
                         </div>
                     </div>
                 </li>
                 <li id="li_cblist_zbhxnuwopf">
                     <div class="section-pad">
                         <h3>Columns to Show</h3>
                         <div class="clear-space-five"></div>
                         <div id="cblist_zbhxnuwopf_holder"></div>
                     </div>
                 </li>
                 <li id="td_zbhxnuwopf_sidebar" style="border-bottom: none !important;">
                     <div class="section-pad">
                         <div id="sidebar-items-zbhxnuwopf">
                             <h3>Month Selector</h3>
                             <div id="month-selector-text-zbhxnuwopf" class="pad-top-sml pad-bottom"><small>These dates are taken from the timestamp of each item created.</small></div>
                             <div id="month-selector-zbhxnuwopf"></div>
                         </div>
                     </div>
                 </li>
             </ul>
         </div>
     </div>
</div>
<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Apps/Custom_Tables/customtables.js' />
<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Scripts/jquery/jquery.fileDownload.js' />
