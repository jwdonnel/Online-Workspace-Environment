using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using OpenWSE_Tools.AutoUpdates;
using OpenWSE_Tools.Apps;
using System.Web.Script.Serialization;

public partial class Apps_SteelTrucks_SteelTrucks : System.Web.UI.UserControl {

    private readonly App _apps = new App(string.Empty);
    private AppInitializer _appInitializer;
    private const string app_id = "app-steeltrucks";

    protected void Page_Load(object sender, EventArgs e) {
        string cl = _apps.GetAppName(app_id);
        lbl_Title.Text = cl;

        img_Title.Visible = true;
        string clImg = _apps.GetAppIconName(app_id);
        img_Title.ImageUrl = "~/" + clImg;

        _appInitializer = new AppInitializer(app_id, Page.User.Identity.Name, Page);
        _appInitializer.LoadScripts_JS(true);

        JavaScriptSerializer js = ServerSettings.CreateJavaScriptSerializer();
        string jsVal = js.Serialize(GetCommonCarrierAutoCompleteLists());
        jsVal = jsVal.Replace(" ", "~");
        hf_AutoCompleteList_SteelTrucks.Value = HttpUtility.UrlEncode(jsVal);
    }


    #region AutoCompleteList

    private object[] GetCommonCarrierAutoCompleteLists() {
        List<object> returnObj = new List<object>();

        var ts = new TruckSchedule();

        returnObj.Add(GetListOfSMWDrivers(ts));
        returnObj.Add(GetListOfSMWUnits(ts));
        returnObj.Add(GetListCustomersTS(ts));
        returnObj.Add(GetListCityTS(ts));
        return returnObj.ToArray();
    }
    private string[] GetListOfSMWDrivers(TruckSchedule ts) {
        var temp = new List<string>();
        foreach (string t in ts.drivers_coll) {
            string driver = t;
            try {
                driver = driver.Replace("_", " ");
            }
            catch {
            }
            if (!temp.Contains(driver)) {
                temp.Add(driver);
            }
        }

        temp.Sort();
        return temp.ToArray();
    }
    private string[] GetListOfSMWUnits(TruckSchedule ts) {
        var temp = new List<string>();
        foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
            if (!temp.Contains(t.Unit) && !string.IsNullOrEmpty(t.Unit)) {
                temp.Add(t.Unit);
            }
        }
        temp.Sort();
        return temp.ToArray();
    }
    private string[] GetListCustomersTS(TruckSchedule ts) {
        var temp = new List<string>();
        foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
            if (!temp.Contains(t.CustomerName) && !string.IsNullOrEmpty(t.CustomerName)) {
                temp.Add(t.CustomerName);
            }
        }
        temp.Sort();
        return temp.ToArray();
    }
    private string[] GetListCityTS(TruckSchedule ts) {
        var temp = new List<string>();
        foreach (TruckSchedule_Coll t in ts.scheduler_coll) {
            if (!temp.Contains(t.City)) {
                temp.Add(t.City);
            }
        }
        temp.Sort();
        return temp.ToArray();
    }

    #endregion

}