using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Apps_StockViewer_StockViewer : System.Web.UI.UserControl {

    private const string _appId = "app-stockviewer";
    private App _apps = new App(string.Empty);

    protected void Page_Load(object sender, EventArgs e) {
        string cl = _apps.GetAppName(_appId);
        lbl_Title.Text = cl;

        img_Title.Visible = true;
        string clImg = _apps.GetAppIconName(_appId);
        img_Title.ImageUrl = "~/" + clImg;
    }

}