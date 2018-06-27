using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;

public partial class Integrated_Pages_StockList_Galvannealed : System.Web.UI.Page
{
    private SteelInventory inventory = new SteelInventory();

    protected void Page_Load(object sender, EventArgs e)
    {
        #region Build Sheets Table
        inventory.BuildTable("Galvanneal Sheet", hfSortBy.Value);
        productTable_Sheet.Rows.Clear();
        if (inventory.dtInv.Count > 0)
        {
            BuildHeaderSheet();
            foreach (Dictionary<string, string> dr in inventory.dtInv)
            {
                productTable_Sheet.Rows.Add(BuildRowsSheet(dr));
            }
        }
        else
            spacing.Visible = false;
        #endregion

        #region Build Coil Table
        inventory.BuildTable("Galvanneal Coil", hfSortBy.Value);
        productTable_Coil.Rows.Clear();
        if (inventory.dtInv.Count > 0)
        {
            BuildHeaderCoil();
            foreach (Dictionary<string, string> dr in inventory.dtInv)
            {
                productTable_Coil.Rows.Add(BuildRowsCoil(dr));
            }
        }
        #endregion
    }

    private void BuildHeaderSheet()
    {
        HtmlTableRow row = new HtmlTableRow();
        row.Attributes["class"] = "productTableHeader-tr";

        HtmlTableCell cell = new HtmlTableCell();
        cell.InnerHtml = "Type";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Grade";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Gauge";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Thickness Inches";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Width";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Length";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "LBS per SQFT";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Weight Per Sheet";
        row.Cells.Add(cell);

        productTable_Sheet.Rows.Add(row);
    }

    private void BuildHeaderCoil()
    {
        HtmlTableRow row = new HtmlTableRow();
        row.Attributes["class"] = "productTableHeader-tr";

        HtmlTableCell cell = new HtmlTableCell();
        cell.InnerHtml = "Type";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Grade";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Gauge";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Thickness Inches";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Width";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Length";
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = "Minimum Run";
        row.Cells.Add(cell);

        productTable_Coil.Rows.Add(row);
    }

    private HtmlTableRow BuildRowsSheet(Dictionary<string, string> dr)
    {
        HtmlTableRow row = new HtmlTableRow();
        row.Attributes["class"] = "productTable-tr";

        HtmlTableCell cell = new HtmlTableCell();
        cell.InnerHtml = dr["Type"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Grade"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Gauge"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Thickness"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Width"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Length"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["lBSperSQFT"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["WeightPerSheet"].ToString();
        row.Cells.Add(cell);

        return row;
    }

    private HtmlTableRow BuildRowsCoil(Dictionary<string, string> dr)
    {
        HtmlTableRow row = new HtmlTableRow();
        row.Attributes["class"] = "productTable-tr";

        HtmlTableCell cell = new HtmlTableCell();
        cell.InnerHtml = dr["Type"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Grade"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Gauge"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Thickness"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Width"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["Length"].ToString();
        row.Cells.Add(cell);

        cell = new HtmlTableCell();
        cell.InnerHtml = dr["MinRun"].ToString();
        row.Cells.Add(cell);

        return row;
    }
}