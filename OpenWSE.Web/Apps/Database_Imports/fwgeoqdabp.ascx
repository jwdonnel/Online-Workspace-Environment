<%@ Control Language="C#" AutoEventWireup="true" ClassName="fwgeoqdabp" ClientIDMode="Static" Inherits="DBImporterAppLoader" %>

<div class="dbImport-table-timermarker">
    <div id="pnl_fwgeoqdabp_tableView" class="custom-table-tableView-holder" data-viewmode="">
         <div id="app_title_bgcolor_fwgeoqdabp" runat="server" class="pad-all app-title-bg-color">
             <div class="float-left">
                 <asp:Image ID="img_Title_fwgeoqdabp" runat="server" CssClass="app-img-titlebar" />
                 <asp:Label ID="lbl_Title_fwgeoqdabp" runat="server" Text="" CssClass="app-text-titlebar"></asp:Label>
             </div>
             <div id="search_fwgeoqdabp_divider" class="search-customtable-divider" style="display: none;"></div>
             <div id="search_fwgeoqdabp_holder" class="float-right">
                 <div class="searchwrapper">
                     <div class="searchboxholder">
                         <input id="tb_search_fwgeoqdabp" type="text" class="searchbox" placeholder="Search table..." onkeypress="dbImport.KeyPressSearch(event, 'fwgeoqdabp')" />
                     </div>
                     <a href="#" class="searchbox_submit" onclick="dbImport.Search('fwgeoqdabp');return false;"></a>
                     <a href="#" onclick="$('#tb_search_fwgeoqdabp').val('');dbImport.Refresh('fwgeoqdabp');return false;" class="searchbox_clear"></a>
                 </div>
             </div>
             <div class="clear"></div>
             <div id="appdesc_fwgeoqdabp" runat="server" class="custom-table-appdescription" visible="false"></div>
         </div>
         <div class="img-appmenu app-menu-btn pad-all" onclick="dbImport.MenuClick('fwgeoqdabp');" title="View Menu"></div>
         <div class="clear"></div>
         <asp:HiddenField ID="hf_fwgeoqdabp_customizations" runat="server" Value="" />
         <asp:HiddenField ID="hf_fwgeoqdabp_summaryData" runat="server" Value="" />
         <asp:HiddenField ID="hf_fwgeoqdabp_tableview" runat="server" Value="default" />
         <div id="data-holder-fwgeoqdabp" class="data-holder-customtable-holder"></div>
     </div>
     <div id="pnl_fwgeoqdabp_chartView" style="display: none;"></div>
     <div id="fwgeoqdabp-sidebar-menu" class="customtable-sidebar-menu">
         <div class="pad-all customtable-sidebar-innercontent">
             <div class="img-close-dark app-menu-btn" onclick="dbImport.MenuClick('fwgeoqdabp');" title="Close Menu"></div>
             <a href="#" class="float-right img-refresh" onclick="dbImport.Refresh('fwgeoqdabp');return false;" title="Refresh Table"></a>
             <div class="clear-space"></div>
             <ul>
                 <li id="fwgeoqdabp-sidebar-menu-viewallrecords">
                     <div class="section-pad" style="padding-top: 5px !important;">
                         <a id="btn_fwgeoqdabp_addRecord" runat="server" class="sidebar-menu-buttons" onclick="dbImport.AddRecord_MenuClick('fwgeoqdabp');return false;"><span class="td-add-btn sidebar-menu-img"></span>Create New Record</a>
                         <div class="clear"></div>
                         <a id="btn_fwgeoqdabp_viewRecords" class="sidebar-menu-buttons" onclick="dbImport.ViewRecords_MenuClick('fwgeoqdabp');return false;"><span class="td-details-btn sidebar-menu-img"></span>View Record List</a>
                         <div class="clear"></div>
                         <a id="btn_fwgeoqdabp_viewSummary" class="sidebar-menu-buttons" onclick="dbImport.ViewSummary_MenuClick('fwgeoqdabp');return false;"><span class="td-view-btn sidebar-menu-img"></span>View Summary</a>
                     </div>
                 </li>
                 <li id="li_fwgeoqdabp_chartType" runat="server">
                     <asp:Panel ID="pnl_fwgeoqdabp_chartType" CssClass="custom-table-view-chart section-pad" runat="server">
                         <a href="#" onclick="dbImport.ViewChart('fwgeoqdabp', this);return false;">
                             <asp:HiddenField ID="hf_fwgeoqdabp_chartType" runat="server" />
                             <asp:HiddenField ID="hf_fwgeoqdabp_chartTitle" runat="server" />
                             <asp:HiddenField ID="hf_fwgeoqdabp_chartColumns" runat="server" />
                             <asp:Image ID="img_fwgeoqdabp_chartType" runat="server" />
                             <span>View Data in Chart</span>
                         </a>
                         <div class="clear"></div>
                     </asp:Panel>
                 </li>
                 <li id="li_fwgeoqdabp_viewMode" runat="server">
                     <div class="section-pad">
                         <h3 class="section-pad-title">View Mode</h3>
                         <div class="desktop-img sidebar-customtables-img" title="Desktop" onclick="dbImport.ChangeViewMode('fwgeoqdabp', 'desktop')"></div>
                         <div class="mobile-img sidebar-customtables-img" title="Mobile" onclick="dbImport.ChangeViewMode('fwgeoqdabp', 'mobile')"></div>
                         <div class="clear"></div>
                     </div>
                 </li>
                 <li>
                     <div class="section-pad">
                         <h3 class="section-pad-title">Font Size</h3>
                         <div class="font-larger-img sidebar-customtables-img" title="Increase Font Size" onclick="dbImport.ChangeFont('fwgeoqdabp', this)"></div>
                         <div class="font-smaller-img sidebar-customtables-img" title="Descrease Font Size" onclick="dbImport.ChangeFont('fwgeoqdabp', this)"></div>
                         <div class="clear"></div>
                         <select id="font-size-selector-fwgeoqdabp" style="display: none;">
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
                         <select id="RecordstoSelect_fwgeoqdabp" class="float-right" onchange="dbImport.RecordstoSelect('fwgeoqdabp')">
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
                         <div class="custom-table-excelexport-btns">
                             <input id="btnExportAll-fwgeoqdabp" type="button" class="input-buttons no-margin" onclick="dbImport.ExportToExcelAll('fwgeoqdabp')" title="Export all data to Excel file" value="Export to Excel" />
                             <div class="clear"></div>
                         </div>
                     </div>
                 </li>
                 <li id="li_cblist_fwgeoqdabp" style="border-bottom: none !important;">
                     <div class="section-pad">
                         <h3>Columns to Show</h3>
                         <div class="clear-space-five"></div>
                         <div id="cblist_fwgeoqdabp_holder"></div>
                     </div>
                 </li>
             </ul>
         </div>
     </div>
</div>
<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Apps/Database_Imports/dbimports.js' />
<input type='hidden' data-scriptelement='true' data-tagname='script' data-tagtype='text/javascript' data-tagsrc='~/Scripts/jquery/jquery.fileDownload.js' />
