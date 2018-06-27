<%@ Control Language="C#" AutoEventWireup="true" ClassName="kepfxabrbq" ClientIDMode="Static" Inherits="CustomTablesAppLoader" %>

<div class="custom-table-timermarker">
    <div id="pnl_kepfxabrbq_tableView" class="custom-table-tableView-holder" data-viewmode="">
         <div id="app_title_bgcolor_kepfxabrbq" runat="server" class="pad-all app-title-bg-color">
             <div class="float-left">
                 <asp:Image ID="img_Title_kepfxabrbq" runat="server" CssClass="app-img-titlebar" />
                 <asp:Label ID="lbl_Title_kepfxabrbq" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
             </div>
             <div id="search_kepfxabrbq_divider" class="search-customtable-divider" style="display: none;"></div>
             <div id="search_kepfxabrbq_holder" class="float-right">
                 <div class="searchwrapper">
                     <div class="searchboxholder">
                         <input id="tb_search_kepfxabrbq" type="text" placeholder="Search table..." class="searchbox" onkeypress="customTables.KeyPressSearch(event, 'kepfxabrbq')" />
                     </div>
                     <a href="#" class="searchbox_submit" onclick="customTables.Search('kepfxabrbq');return false;"></a>
                     <a href="#" onclick="$('#tb_search_kepfxabrbq').val('');customTables.Refresh('kepfxabrbq');return false;" class="searchbox_clear"></a>
                 </div>
             </div>
             <div class="clear"></div>
             <div id="appdesc_kepfxabrbq" runat="server" class="custom-table-appdescription" visible="false"></div>
         </div>
         <div class="img-appmenu app-menu-btn pad-all" onclick="customTables.MenuClick('kepfxabrbq');" title="View Menu"></div>
         <div class="td-delete-btn app-delete-month pad-all" onclick="customTables.DeleteMonth('kepfxabrbq');" title="Delete Month"></div>
         <div class="img-copy app-copy-data pad-all" onclick="customTables.CopyData('kepfxabrbq');" title="Copy Data"></div>
         <asp:HiddenField ID="hf_kepfxabrbq_customizations" runat="server" Value="" />
         <asp:HiddenField ID="hf_kepfxabrbq_summaryData" runat="server" Value="" />
         <asp:HiddenField ID="hf_kepfxabrbq_monthSelector" runat="server" Value="" />
         <asp:HiddenField ID="hf_kepfxabrbq_tableview" runat="server" Value="default" />
         <div id="data-holder-kepfxabrbq" class="data-holder-customtable-holder"></div>
     </div>
     <div id="pnl_kepfxabrbq_chartView" style="display: none;"></div>
     <div id="kepfxabrbq-sidebar-menu" class="customtable-sidebar-menu">
         <div class="pad-all customtable-sidebar-innercontent">
             <div class="img-close-dark app-menu-btn" onclick="customTables.MenuClick('kepfxabrbq');" title="Close Menu"></div>
             <a href="#" class="float-right img-refresh" onclick="customTables.Refresh('kepfxabrbq');return false;" title="Refresh Table"></a>
             <div class="clear-space"></div>
             <ul>
                 <li id="kepfxabrbq-sidebar-menu-viewallrecords">
                     <div class="section-pad" style="padding-top: 5px !important;">
                         <a id="btn_kepfxabrbq_addRecord" runat="server" class="sidebar-menu-buttons" onclick="customTables.AddRecord_MenuClick('kepfxabrbq');return false;"><span class="td-add-btn sidebar-menu-img"></span>Create New Record</a>
                         <div class="clear"></div>
                         <a id="btn_kepfxabrbq_viewRecords" class="sidebar-menu-buttons" onclick="customTables.ViewRecords_MenuClick('kepfxabrbq');return false;"><span class="td-details-btn sidebar-menu-img"></span>View Record List</a>
                         <div class="clear"></div>
                         <a id="btn_kepfxabrbq_viewSummary" class="sidebar-menu-buttons" onclick="customTables.ViewSummary_MenuClick('kepfxabrbq');return false;"><span class="td-view-btn sidebar-menu-img"></span>View Summary</a>
                     </div>
                 </li>
                 <li id="li_kepfxabrbq_chartType" runat="server">
                     <asp:Panel ID="pnl_kepfxabrbq_chartType" CssClass="custom-table-view-chart section-pad" runat="server">
                         <a href="#" onclick="customTables.ViewChart('kepfxabrbq', this);return false;">
                             <asp:HiddenField ID="hf_kepfxabrbq_chartType" runat="server" />
                             <asp:HiddenField ID="hf_kepfxabrbq_chartTitle" runat="server" />
                             <asp:HiddenField ID="hf_kepfxabrbq_chartColumns" runat="server" />
                             <asp:Image ID="img_kepfxabrbq_chartType" runat="server" />
                             <span>View Data in Chart</span>
                         </a>
                         <div class="clear"></div>
                     </asp:Panel>
                 </li>
                 <li id="li_kepfxabrbq_viewMode" runat="server">
                     <div class="section-pad">
                         <h3 class="section-pad-title">View Mode</h3>
                         <div class="desktop-img sidebar-customtables-img" title="Desktop" onclick="customTables.ChangeViewMode('kepfxabrbq', 'desktop')"></div>
                         <div class="mobile-img sidebar-customtables-img" title="Mobile" onclick="customTables.ChangeViewMode('kepfxabrbq', 'mobile')"></div>
                         <div class="clear"></div>
                     </div>
                 </li>
                 <li>
                     <div class="section-pad">
                         <h3 class="section-pad-title">Font Size</h3>
                         <div class="font-larger-img sidebar-customtables-img" title="Increase Font Size" onclick="customTables.ChangeFont('kepfxabrbq', this)"></div>
                         <div class="font-smaller-img sidebar-customtables-img" title="Descrease Font Size" onclick="customTables.ChangeFont('kepfxabrbq', this)"></div>
                         <div class="clear"></div>
                         <select id="font-size-selector-kepfxabrbq" style="display: none;">
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
                         <select id="RecordstoSelect_kepfxabrbq" class="float-right" onchange="customTables.RecordstoSelect('kepfxabrbq')">
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
                             <input id="btnExport-kepfxabrbq" type="button" class="input-buttons" onclick="customTables.OpenExcelExportModal('kepfxabrbq')" title="Select a date to export to an Excel file" value="Export Dates" />
                             <input id="btnExportAll-kepfxabrbq" type="button" class="input-buttons no-margin" onclick="customTables.ExportToExcelAll('kepfxabrbq')" title="Export all data to Excel file" value="Export All" />
                             <div class="clear"></div>
                         </div>
                         <div id="kepfxabrbq-ExcelExport-element" class="Modal-element">
                             <div class="Modal-element-align">
                                 <div class="Modal-element-modal ui-draggable ui-draggable-handle" data-setwidth="350">
                                     <div class="ModalHeader">
                                         <div>
                                             <div class="app-head-button-holder-admin"><a href="#" onclick="customTables.CloseExcelExportModal('kepfxabrbq');return false;" class="ModalExitButton"></a></div>
                                             <span class="Modal-title"></span>
                                         </div>
                                     </div>
                                     <div class="ModalPadContent">
                                         Select the dates you wish to export. The dates are based off of the Month Selector column.
                                         <div class="clear-space"></div>
                                         <div align="center">
                                             <input id="tb_exportDateFrom_kepfxabrbq" type="text" class="textEntry" style="width: 95px;" />
                                             <span class="font-bold pad-left-sml pad-right-sml">To</span>
                                             <input id="tb_exportDateTo_kepfxabrbq" type="text" class="textEntry" style="width: 95px;" />
                                             <div class="clear"></div>
                                         </div>
                                         <div class="clear-space"></div>
                                         <div class="clear-space"></div>
                                         <input class="input-buttons float-left" type="button" value="Export" onclick="customTables.ExportToExcelDates('kepfxabrbq');" />
                                         <input class="input-buttons float-right no-margin" type="button" value="Cancel" onclick="customTables.CloseExcelExportModal('kepfxabrbq');" />
                                         <div class="clear"></div>
                                     </div>
                                 </div>
                             </div>
                         </div>
                     </div>
                 </li>
                 <li id="li_cblist_kepfxabrbq">
                     <div class="section-pad">
                         <h3>Columns to Show</h3>
                         <div class="clear-space-five"></div>
                         <div id="cblist_kepfxabrbq_holder"></div>
                     </div>
                 </li>
                 <li id="td_kepfxabrbq_sidebar" style="border-bottom: none !important;">
                     <div class="section-pad">
                         <div id="sidebar-items-kepfxabrbq">
                             <h3>Month Selector</h3>
                             <div id="month-selector-text-kepfxabrbq" class="pad-top-sml pad-bottom"><small>These dates are taken from the timestamp of each item created.</small></div>
                             <div id="month-selector-kepfxabrbq"></div>
                         </div>
                     </div>
                 </li>
             </ul>
         </div>
     </div>
</div>
<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Apps/Custom_Tables/customtables.js' />
<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Scripts/jquery/jquery.fileDownload.js' />
