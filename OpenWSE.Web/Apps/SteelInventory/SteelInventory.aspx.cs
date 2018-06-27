using OpenWSE_Tools.Apps;
using OpenWSE_Tools.AutoUpdates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Apps_SteelInventory_SteelInventory : System.Web.UI.Page {

    #region Private Variables

    private AppInitializer _appInitializer;
    private const string app_id = "app-steelinventory";
    private ServerSettings ss = new ServerSettings();
    private readonly UserUpdateFlags uuf = new UserUpdateFlags();
    private readonly SteelInventory steelInv = new SteelInventory();
    private readonly App _apps = new App(string.Empty);
    private bool InEditMode {
        get {
            if (ViewState["InEditMode"] != null && !string.IsNullOrEmpty(ViewState["InEditMode"].ToString())) {
                HelperMethods.ConvertBitToBoolean(ViewState["InEditMode"].ToString());
            }

            return false;
        }
        set {
            ViewState["InEditMode"] = value;
        }
    }
    private bool CanEdit {
        get {
            if (HttpContext.Current.User.Identity.IsAuthenticated && !HelperMethods.ConvertBitToBoolean(Request.QueryString["viewonly"])) {
                return true;
            }

            return false;
        }
    }
    private bool HideQuantityColumn {
        get {
            if (HelperMethods.ConvertBitToBoolean(Request.QueryString["hidequantity"])) {
                return true;
            }

            return false;
        }
    }
    private string CurrentUserTheme {
        get {
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                return _appInitializer.memberDatabase.SiteTheme; 
            }
            return "Standard";
        }
    }

    #endregion


    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        _appInitializer = new AppInitializer(app_id, userId.Name, Page);
        if (CanEdit && _appInitializer.TryLoadPageEvent) {
            if (!IsPostBack) {
                InitializeItems();
                BuildInventoryImages();
                BuildSteelInventory();
            }
        }
        else {
            pnl_topbar.Visible = false;
            InitializeItems();
            BuildInventoryImages();
            BuildSteelInventory();
        }
    }

    private void InitializeItems() {
        string cl = _apps.GetAppName(app_id);
        lbl_Title.Text = cl;

        img_Title.Visible = true;
        string clImg = _apps.GetAppIconName(app_id);
        img_Title.ImageUrl = "~/" + clImg;

        _appInitializer.LoadScripts_JS(false);
        _appInitializer.LoadScripts_CSS();
        _appInitializer.LoadDefaultScripts();
        _appInitializer.LoadCustomFonts();

        AutoUpdateSystem aus = new AutoUpdateSystem(hf_UpdateAll.ClientID, app_id, this.Page);
        aus.StartAutoUpdates();
    }

    private void BuildInventoryImages() {
        string[] files = Directory.GetFiles(ServerSettings.GetServerMapLocation + "Apps\\SteelInventory\\Images");
        if (files.Length == 0) {
            pnl_InventoryImageSelector.Controls.Add(new LiteralControl("<small>No images uploaded.</small>"));
        }
        else {
            string fileList = string.Empty;
            foreach (string imageFile in files) {
                FileInfo fi = new FileInfo(imageFile);
                fileList += "<div class='steelInventory-image-select'><div class='delete-invimg' onclick=\"steelInventory.DeleteImage('" + fi.Name + "');\" title='Delete Image'></div><img alt='' src='" + ResolveUrl("~/Apps/SteelInventory/Images/" + fi.Name) + "' onclick=\"steelInventory.onInventoryImageClick('" + fi.Name + "');\" title='" + fi.Name + "' /></div>";
            }
            pnl_InventoryImageSelector.Controls.Add(new LiteralControl(fileList + "<div class='clear'></div>"));
        }
    }

    private void BuildSteelInventory() {
        if (!string.IsNullOrEmpty(Request.QueryString["type"])) {
            steelInv.BuildTable(Request.QueryString["type"], "Gauge ASC");
        }
        else {
            steelInv.BuildTable();
        }
        TableBuilder tableBuilder = new TableBuilder(Page, true, CanEdit, 2, "MainInventory");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("Image", "70", false, false));
        headerColumns.Add(new TableBuilderHeaderColumns("Type", "100", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Gauge", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Grade", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("W\"", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("D\"", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("L\"", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Weight Per Sheet", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Pounds Per SQFT", "100", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Minimum Run", "125", false));
        if (!HideQuantityColumn) {
            headerColumns.Add(new TableBuilderHeaderColumns("QTY", "75", false));
        }
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body

        bool foundEditRow = false;
        foreach (Dictionary<string, string> item in steelInv.dtInv) {
            List<TableBuilderBodyColumnValues> bodyColumns = new List<TableBuilderBodyColumnValues>();

            if (item["ID"] == hf_EditItem.Value) {
                string imageLocation = "";
                if (string.IsNullOrEmpty(item["StockImage"])) {
                    imageLocation = GetDefaultImage(item["Type"]);
                }
                else if (File.Exists(ServerSettings.GetServerMapLocation + "Apps\\SteelInventory\\Images\\" + item["StockImage"])) {
                    imageLocation = string.Format("~/Apps/SteelInventory/Images/{0}", item["StockImage"]);
                    imageLocation = ResolveUrl(imageLocation);
                }
                else {
                    imageLocation = ResolveUrl( "~/App_Themes/" + CurrentUserTheme + "/Icons/add-image.png");
                }

                bodyColumns.Add(new TableBuilderBodyColumnValues("Image", "<img alt='' id='img_EditInventoryImg' data-name='Image' class='steel-inventory-image-edit' onclick=\"steelInventory.onInventorySelectClick();\" src='" + imageLocation + "' data-src='" + item["StockImage"] + "' title='Click to select new image' />", TableBuilderColumnAlignment.Center));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Type", "<input type='text' data-name='Type' class='textEntry-noWidth' value='" + item["Type"] + "' />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Gauge", "<input type='text' data-name='Gauge' class='textEntry-noWidth' value='" + item["Gauge"] + "' />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Grade", "<input type='text' data-name='Grade' class='textEntry-noWidth' value='" + item["Grade"] + "' />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("W\"", "<input type='text' data-name='Width' class='textEntry-noWidth' value='" + item["Width"] + "' />", TableBuilderColumnAlignment.Center));
                bodyColumns.Add(new TableBuilderBodyColumnValues("D\"", "<input type='text' data-name='Thickness' class='textEntry-noWidth' value='" + item["Thickness"] + "' />", TableBuilderColumnAlignment.Center));
                bodyColumns.Add(new TableBuilderBodyColumnValues("L\"", "<input type='text' data-name='Length' class='textEntry-noWidth' value='" + item["Length"] + "' />", TableBuilderColumnAlignment.Center));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Weight Per Sheet", "<input type='text' data-name='WeightPerSheet' class='textEntry-noWidth' value='" + item["WeightPerSheet"] + "' />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Pounds Per SQFT", "<input type='text' data-name='lBSperSQFT' class='textEntry-noWidth' value='" + item["lBSperSQFT"] + "' />", TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Minimum Run", "<input type='text' data-name='MinRun' class='textEntry-noWidth' value='" + item["MinRun"] + "' />", TableBuilderColumnAlignment.Left));
                int quantity = 0;
                if (!string.IsNullOrEmpty(item["Quantity"])) {
                    int.TryParse(item["Quantity"], out quantity);
                }

                if (!HideQuantityColumn) {
                    if (CanEdit) {
                        bodyColumns.Add(new TableBuilderBodyColumnValues("QTY", "<input type='number' data-name='Quantity' class='textEntry-noWidth' value='" + quantity.ToString() + "' onfocus=\"$(this).select();\" />", TableBuilderColumnAlignment.Left));
                    }
                    else {
                        bodyColumns.Add(new TableBuilderBodyColumnValues("QTY", quantity.ToString(), TableBuilderColumnAlignment.Left));
                    }
                }

                string updateBtn = "<a href='#' class='td-update-btn' title='Update' onclick=\"steelInventory.UpdateItem('" + item["ID"] + "');return false;\"></a>";
                string cancelBtn = "<a href='#' class='td-cancel-btn' title='Cancel' onclick=\"steelInventory.CancelItem();return false;\"></a>";

                tableBuilder.AddBodyRow(bodyColumns, updateBtn + cancelBtn, "data-id='" + item["ID"] + "'", "steelinventory-editrow");
                foundEditRow = true;
            }
            else {
                bodyColumns.Add(new TableBuilderBodyColumnValues("Image", GetInventoryImage(item["StockImage"], item["Type"]), TableBuilderColumnAlignment.Center));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Type", item["Type"], TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Gauge", item["Gauge"], TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Grade", item["Grade"], TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("W\"", item["Width"], TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("D\"", item["Thickness"], TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("L\"", item["Length"], TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Weight Per Sheet", item["WeightPerSheet"], TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Pounds Per SQFT", item["lBSperSQFT"], TableBuilderColumnAlignment.Left));
                bodyColumns.Add(new TableBuilderBodyColumnValues("Minimum Run", item["MinRun"], TableBuilderColumnAlignment.Left));

                int quantity = 0;
                if (!string.IsNullOrEmpty(item["Quantity"])) {
                    int.TryParse(item["Quantity"], out quantity);
                }

                if (!HideQuantityColumn) {
                    if (CanEdit) {
                        string disabledTextbox = string.Empty;
                        if (!string.IsNullOrEmpty(hf_EditItem.Value)) {
                            disabledTextbox = " disabled='disabled'";
                        }

                        bodyColumns.Add(new TableBuilderBodyColumnValues("QTY", "<input type='number' class='textEntry-noWidth'" + disabledTextbox + " data-id='" + item["ID"] + "' data-initvalue='" + quantity.ToString() + "' value='" + quantity.ToString() + "' onfocus=\"$(this).select();\" onblur=\"steelInventory.UpdateQuantity(this);\" onkeypress=\"steelInventory.OnQuantityKeyPress(this, event);\" />", TableBuilderColumnAlignment.Left));
                    }
                    else {
                        bodyColumns.Add(new TableBuilderBodyColumnValues("QTY", quantity.ToString(), TableBuilderColumnAlignment.Left));
                    }
                }

                string editBtn = "<a href='#' class='td-edit-btn' title='Edit' onclick=\"steelInventory.EditItem('" + item["ID"] + "');return false;\"></a>";
                string deleteBtn = "<a href='#' class='td-delete-btn' title='Delete' onclick=\"steelInventory.DeleteItem('" + item["ID"] + "');return false;\"></a>";

                tableBuilder.AddBodyRow(bodyColumns, editBtn + deleteBtn, "data-id='" + item["ID"] + "'");
            }
        }
        #endregion

        #region Insert Row

        if (CanEdit && !foundEditRow) {
            List<TableBuilderInsertColumnValues> insertColumns = new List<TableBuilderInsertColumnValues>();
            insertColumns.Add(new TableBuilderInsertColumnValues("Image", "img_Image_Insert", TableBuilderColumnAlignment.Center, TableBuilderInsertType.Image, ResolveUrl( "~/App_Themes/" + CurrentUserTheme + "/Icons/add-image.png")));
            insertColumns.Add(new TableBuilderInsertColumnValues("Type", "txt_Type_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Type"));
            insertColumns.Add(new TableBuilderInsertColumnValues("Gauge", "txt_Gauge_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Gauage"));
            insertColumns.Add(new TableBuilderInsertColumnValues("Grade", "txt_Grade_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Grade"));
            insertColumns.Add(new TableBuilderInsertColumnValues("W\"", "txt_Width_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Width"));
            insertColumns.Add(new TableBuilderInsertColumnValues("D\"", "txt_Depth_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Depth"));
            insertColumns.Add(new TableBuilderInsertColumnValues("L\"", "txt_Length_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Length"));
            insertColumns.Add(new TableBuilderInsertColumnValues("Weight Per Sheet", "txt_WeightPerSheet_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Weight Per Sheet"));
            insertColumns.Add(new TableBuilderInsertColumnValues("Pounds Per SQFT", "txt_PoundsPerSQFT_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Pounds Per SQFT"));
            insertColumns.Add(new TableBuilderInsertColumnValues("Minimum Run", "txt_MinimumRun_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text, "Minimum Run"));
            insertColumns.Add(new TableBuilderInsertColumnValues("QTY", "txt_QTY_Insert", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Number, "QTY"));
            tableBuilder.AddInsertRow(insertColumns, "steelInventory.AddItem()");
        }

        #endregion

        pnl_SteelInventory.Controls.Clear();
        pnl_SteelInventory.Controls.Add(tableBuilder.CompleteTableLiteralControl("No inventory found."));

        updatePnl_Table.Update();
    }

    private string GetInventoryImage(string stockImage, string itemType) {
        string image = "<img alt='' src='{0}' class='steel-inventory-image' />";
        if (string.IsNullOrEmpty(stockImage)) {
            return string.Format(image, ResolveUrl(GetDefaultImage(itemType)));
        }
        else if (File.Exists(ServerSettings.GetServerMapLocation + "Apps\\SteelInventory\\Images\\" + stockImage)) {
            string imageLocation = string.Format("~/Apps/SteelInventory/Images/{0}", stockImage);
            return string.Format(image, ResolveUrl(imageLocation));
        }

        return string.Empty;
    }

    private string GetDefaultImage(string itemType) {
        string stockImage = string.Empty;
        itemType = itemType.ToLower();

        if (itemType.Contains("laserflat")) {
            stockImage = "laserflat.jpg";
        }
        else if (itemType.StartsWith("cold rolled")) {
            stockImage = "coldrolled.jpg";
        }
        else if (itemType.StartsWith("galvanized")) {
            stockImage = "galvanized.jpg";
        }
        else if (itemType.StartsWith("galvanneal")) {
            stockImage = "galvanneal.jpg";
        }
        else if (itemType.StartsWith("hot rolled p&o") || itemType.StartsWith("hot rolled pickeled and oiled")) {
            stockImage = "hotrolled.jpg";
        }
        else if (itemType.StartsWith("hot rolled")) {
            stockImage = "hotrolledpickeledandoiled.jpg";
        }

        string imageLocation = string.Empty;
        if (!string.IsNullOrEmpty(stockImage) && File.Exists(ServerSettings.GetServerMapLocation + "Apps\\SteelInventory\\Images\\" + stockImage)) {
            imageLocation = string.Format("~/Apps/SteelInventory/Images/{0}", stockImage);
            imageLocation = ResolveUrl(imageLocation);
        }

        return imageLocation;
    }

    #region Postback Controls

    protected void hf_UpdateAll_ValueChanged(object sender, EventArgs e) {
        if (!InEditMode) {
            if (!string.IsNullOrEmpty(hf_UpdateAll.Value)) {
                string id = uuf.getFlag_AppID(hf_UpdateAll.Value);
                if (id == app_id) {
                    uuf.deleteFlag(hf_UpdateAll.Value);
                }
            }

            BuildInventoryImages();
            BuildSteelInventory();
            hf_UpdateAll.Value = string.Empty;
        }
    }

    protected void hf_UpdateQuantity_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_UpdateQuantityId.Value)) {
            int quantity = 0;
            if (!string.IsNullOrEmpty(hf_UpdateQuantity.Value)) {
                int.TryParse(hf_UpdateQuantity.Value, out quantity);
                steelInv.UpdateQuantity(hf_UpdateQuantityId.Value, quantity);
            }
        }

        BuildInventoryImages();
        BuildSteelInventory();
        hf_UpdateQuantityId.Value = string.Empty;
        hf_UpdateQuantity.Value = string.Empty;
    }
    protected void hf_EditItem_ValueChanged(object sender, EventArgs e) {
        if (hf_EditItem.Value == "CancelUpdate") {
            hf_EditItem.Value = string.Empty;
            InEditMode = false;
        }
        else {
            InEditMode = true;
        }

        BuildInventoryImages();
        BuildSteelInventory();
        hf_EditItem.Value = string.Empty;
    }
    protected void hf_DeleteItem_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_DeleteItem.Value)) {
            steelInv.DeleteRow(hf_DeleteItem.Value);
        }

        BuildInventoryImages();
        BuildSteelInventory();
        hf_DeleteItem.Value = string.Empty;
    }
    protected void hf_UpdateItem_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_UpdateItem.Value)) {
            try {
                JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
                string updateItemStr = HttpUtility.UrlDecode(hf_UpdateItem.Value);
                Dictionary<string, object> updateObj = (Dictionary<string, object>)js.Deserialize(updateItemStr, typeof(Dictionary<string, object>));

                int quantity = 0;
                if (!string.IsNullOrEmpty(updateObj["Quantity"].ToString())) {
                    int.TryParse(updateObj["Quantity"].ToString(), out quantity);
                }

                steelInv.UpdateRow(updateObj["ID"].ToString(), updateObj["Type"].ToString(), updateObj["Grade"].ToString(), updateObj["Gauge"].ToString(), updateObj["Thickness"].ToString(), updateObj["Width"].ToString(), updateObj["Length"].ToString(), updateObj["MinRun"].ToString(), updateObj["lBSperSQFT"].ToString(), updateObj["WeightPerSheet"].ToString(), quantity, updateObj["Image"].ToString());
            }
            catch (Exception ex) {
                AppLog.AddError(ex);
            }

            InEditMode = false;
        }

        BuildInventoryImages();
        BuildSteelInventory();
        hf_UpdateItem.Value = string.Empty;
    }

    protected void hf_AddItem_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_AddItem.Value)) {
            try {
                JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
                string insertItemStr = HttpUtility.UrlDecode(hf_AddItem.Value);
                Dictionary<string, object> updateObj = (Dictionary<string, object>)js.Deserialize(insertItemStr, typeof(Dictionary<string, object>));

                int quantity = 0;
                if (!string.IsNullOrEmpty(updateObj["QTY"].ToString())) {
                    int.TryParse(updateObj["QTY"].ToString(), out quantity);
                }

                steelInv.AddRow(updateObj["Type"].ToString(), updateObj["Grade"].ToString(), updateObj["Gauge"].ToString(), updateObj["D"].ToString(), updateObj["W"].ToString(), updateObj["L"].ToString(), updateObj["Minimum Run"].ToString(), updateObj["Pounds Per SQFT"].ToString(), updateObj["Weight Per Sheet"].ToString(), quantity, updateObj["Image"].ToString());
            }
            catch (Exception ex) {
                AppLog.AddError(ex);
            }

            InEditMode = false;
        }

        BuildInventoryImages();
        BuildSteelInventory();
        hf_AddItem.Value = string.Empty;
    }

    protected void hf_DeleteImage_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_DeleteImage.Value)) {
            if (File.Exists(ServerSettings.GetServerMapLocation + "Apps\\SteelInventory\\Images\\" + hf_DeleteImage.Value)) {
                try {
                    File.Delete(ServerSettings.GetServerMapLocation + "Apps\\SteelInventory\\Images\\" + hf_DeleteImage.Value);
                }
                catch (Exception ex) {
                    AppLog.AddError(ex);
                }
            }
        }

        hf_EditItem.Value = hf_DeleteImage_EditId.Value;

        BuildInventoryImages();
        BuildSteelInventory();
        hf_DeleteImage.Value = string.Empty;
        hf_EditItem.Value = string.Empty;
        hf_DeleteImage_EditId.Value = string.Empty;
    }
    protected void hf_RefreshImageList_ValueChanged(object sender, EventArgs e) {
        hf_EditItem.Value = hf_RefreshImageList.Value;
        if (hf_RefreshImageList.Value == "Refresh") {
            hf_EditItem.Value = string.Empty;
        }

        BuildInventoryImages();
        BuildSteelInventory();
        hf_EditItem.Value = string.Empty;
        hf_RefreshImageList.Value = string.Empty;
    }

    #endregion

}