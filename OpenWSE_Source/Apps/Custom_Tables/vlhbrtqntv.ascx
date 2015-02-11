<%@ Control Language="C#" ClassName="vlhbrtqntv" ClientIDMode="Static" %>

<script runat="server">
     private ServerSettings _ss = new ServerSettings();
     private readonly App _apps = new App();
     private OpenWSE_Tools.Apps.AppInitializer _appInitializer;
     private const string app_id = "app-vlhbrtqntv";

     protected void Page_Load(object sender, EventArgs e) {
         System.Security.Principal.IIdentity userId = Page.User.Identity;
         CustomTableViewer ctv = new CustomTableViewer(userId.Name);
         CustomTable_Coll coll = ctv.GetTableInfoByAppId(app_id);
         lbl_Title_vlhbrtqntv.Text = coll.TableName;

         if (!_ss.HideAllAppIcons) {
             img_Title_vlhbrtqntv.Visible = true;
             string clImg = _apps.GetAppIconName(app_id);
             img_Title_vlhbrtqntv.ImageUrl = "~/Standard_Images/App_Icons/" + clImg;
         }
         else {
             img_Title_vlhbrtqntv.Visible = false;
         }

         td_vlhbrtqntv_sidebar.Visible = coll.Sidebar;

         if (!string.IsNullOrEmpty(coll.Chart_Type.ToString()) && coll.Chart_Type != ChartType.None) {
             pnl_vlhbrtqntv_chartType.Enabled = true;
             pnl_vlhbrtqntv_chartType.Visible = true;
             img_vlhbrtqntv_chartType.ImageUrl = "~/Standard_Images/ChartTypes/" + coll.Chart_Type.ToString().ToLower() + ".png";
             hf_vlhbrtqntv_chartType.Value = coll.Chart_Type.ToString();
             string chartTitle = coll.ChartTitle;
             if (string.IsNullOrEmpty(chartTitle)) {
                 chartTitle = coll.TableName;
             }
             hf_vlhbrtqntv_chartTitle.Value = chartTitle;
         }
         else {
             pnl_vlhbrtqntv_chartType.Enabled = false;
             pnl_vlhbrtqntv_chartType.Visible = false;
         }

         _appInitializer = new OpenWSE_Tools.Apps.AppInitializer(app_id, Page.User.Identity.Name, Page, "Custom Tables");
         _appInitializer.LoadScripts_JS(true, "customTables.Load('vlhbrtqntv');");
     }
</script>
<div id="vlhbrtqntv-load" class="main-div-app-bg custom-table-timermarker">
     <div id="pnl_vlhbrtqntv_tableView" class="custom-table-tableView-holder">
         <div class="pad-all app-title-bg-color" style="height: 40px">
             <div class="float-left">
                 <asp:Image ID="img_Title_vlhbrtqntv" runat="server" CssClass="float-left pad-right" Height="38px" />
                 <asp:Label ID="lbl_Title_vlhbrtqntv" runat="server" Text="" Font-Size="30px"></asp:Label>
             </div>
             <div id="search_vlhbrtqntv_holder" class="float-right pad-top-sml" style="font-size: 15px">
                 <div class="float-right">
                     <div id="searchwrapper" style="width: 375px;">
                         <input id="tb_search_vlhbrtqntv" type="text" class="searchbox" onfocus="if(this.value=='Search this table')this.value=''" onblur="if(this.value=='')this.value='Search this table'" onkeypress="customTables.KeyPressSearch(event, 'vlhbrtqntv')" value="Search this table" />
                         <a href="#" onclick="$('#tb_search_vlhbrtqntv').val('Search this table');customTables.Refresh('vlhbrtqntv');return false;" class="searchbox_clear"></a>
                         <a href="#" class="searchbox_submit" onclick="customTables.Search('vlhbrtqntv');return false;"></a>
                     </div>
                 </div>
             </div>
         </div>
         <asp:Panel ID="pnl_vlhbrtqntv_chartType" CssClass="custom-table-view-chart" runat="server">
             <a href="#" onclick="customTables.ViewChart('vlhbrtqntv', this);return false;">
                 <asp:HiddenField ID="hf_vlhbrtqntv_chartType" runat="server" />
                 <asp:HiddenField ID="hf_vlhbrtqntv_chartTitle" runat="server" />
                 <asp:Image ID="img_vlhbrtqntv_chartType" runat="server" />
                 <span>View Data in Chart</span>
             </a>
         </asp:Panel>
         <div class="float-right pad-top">
             <a href="#" class="float-right margin-right margin-left img-refresh" onclick="customTables.Refresh('vlhbrtqntv');return false;" title="Refresh List"></a>
             <select id="font-size-selector-vlhbrtqntv" class="custom-table-font-selector float-right margin-left">
                 <option value="x-small">Font Size: x-Small</option>
                 <option value="small" selected="selected">Font Size: Small</option>
                 <option value="medium">Font Size: Medium</option>
                 <option value="large">Font Size: Large</option>
                 <option value="x-large">Font Size: x-Large</option>
             </select>
             <div class="float-right margin-right">
                 <span class="font-bold pad-right">Records to Pull</span>
                 <select id="RecordstoSelect_vlhbrtqntv" onchange="customTables.RecordstoSelect('vlhbrtqntv')">
                     <option value="5">5</option>
                     <option value="10">10</option>
                     <option value="25">25</option>
                     <option value="50" selected="selected">50</option>
                     <option value="75">75</option>
                     <option value="100">100</option>
                     <option value="200">200</option>
                     <option value="all">All</option>
                 </select>
             </div>
             <div class="clear-space"></div>
             <table cellpadding="0" cellspacing="0">
                 <tr>
                     <td>
                         <b class="pad-right">Export to Spreadsheet From dates</b>
                         <input id="tb_exportDateFrom_vlhbrtqntv" type="text" class="textEntry" style="width: 85px;" />
                         <b class="pad-left-sml pad-right-sml">To</b>
                         <input id="tb_exportDateTo_vlhbrtqntv" type="text" class="textEntry" style="width: 85px;" />
                     </td>
                     <td>
                         <input id="btnExport-vlhbrtqntv" type="button" class="input-buttons margin-left" onclick="customTables.ExportToExcelAll('vlhbrtqntv')" title="Select a date to export to an Excel file" value="Export" />
                         <span id="exportingNow_vlhbrtqntv" class="margin-left margin-top margin-bottom" style="display: none;">Exporting...</span>
                     </td>
                 </tr>
             </table>
         </div>
         <div class="clear-space-five"></div>
         <div id="btn_vlhbrtqntv_selectusers"></div>
         <div class="clear-space-five"></div>
         <div class="float-left pad-left" style="font-size: 11px;">Data refreshed every minute</div>
         <div class="clear"></div>
         <table cellpadding="0" cellspacing="0" width="100%">
             <tr>
                 <td id="td_vlhbrtqntv_sidebar" runat="server" valign="top" class="td-sidebar pad-right-big">
                     <div id="sidebar-items-vlhbrtqntv" class="sidebar-items-fixed">
                         <div class="pad-top-big"><h3>Month Selector</h3></div>
                         <div class="clear-space"></div>
                         <div class="pad-right"><small><b class="pad-right-sml">Note:</b>These dates are taken from the timestamp of each created item.</small></div>
                         <div class="clear-space"></div>
                         <div id="month-selector-vlhbrtqntv"></div>
                     </div>
                 </td>
                 <td valign="top">
                     <div id="data-holder-vlhbrtqntv" class="pad-left pad-right"></div>
                 </td>
             </tr>
         </table>
     </div>
     <div id="pnl_vlhbrtqntv_chartInfo" class="pad-left pad-right" style="display: none;"></div>
</div>
