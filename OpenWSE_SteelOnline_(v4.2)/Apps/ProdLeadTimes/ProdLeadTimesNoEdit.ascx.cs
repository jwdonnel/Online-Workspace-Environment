#region

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web.Security;
using System.Web.UI;
using System.Data;
using System.Text;
using System.Web;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_ProdLeadTimesNoEdit : UserControl
{
    #region private variables

    private plt _plt;
    private MembershipUserCollection coll;
    private string ctrlname;
    private MemberDatabase md;
    private AppInitializer _appInitializer;

    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        ScriptManager sm = ScriptManager.GetCurrent(Page);
        var apps = new App();
        string name = apps.GetAppName("app-pltnoedit");
        lbl_Title.Text = name;

        // Initialize all the scripts and style sheets
        _appInitializer = new AppInitializer("app-pltnoedit", Page.User.Identity.Name, Page);
        _appInitializer.LoadScripts_JS(true);

        lbl_lastrefreshed_pltviewer.Text = "<b class='pad-right'>Last Refresh</b>" + DateTime.Now.ToString();

        ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), BuildJS, true);
    }

    protected void btn_update_pltviewer_Clicked(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        MemberDatabase _member = new MemberDatabase(userId.Name);

        int totalweight = 0;
        var str = new System.Text.StringBuilder();

        string line1 = string.Empty;
        string line2 = string.Empty;
        string line3 = string.Empty;
        string line4 = string.Empty;
        string line5 = string.Empty;
        string timeUpdated = string.Empty;
        string updatedBy = string.Empty;

        plt _prodLeadTime = new plt(true);
        if (_prodLeadTime.pltcoll.Count > 0) {
            _prodLeadTime.pltcoll.Sort(
                (x, y) => Convert.ToDateTime(y.DateUpdated).Ticks.CompareTo(Convert.ToDateTime(x.DateUpdated).Ticks));
            System.Collections.Generic.List<string> dates = _prodLeadTime.CalculatePLT();
            const int i = 0;
            DateTime dateupdate = new DateTime();
            DateTime.TryParse(_prodLeadTime.pltcoll[i].DateUpdated, out dateupdate);
            if ((dates != null) && (dates.Count > 0)) {
                timeUpdated = dateupdate.ToString();
                updatedBy = _prodLeadTime.pltcoll[i].UpdatedBy;
                line1 = _prodLeadTime.GetOverride("Override1")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line1)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line1, dates[0]);


                line2 = _prodLeadTime.GetOverride("Override2")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line2)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].CTL_Line2, dates[1]);


                line3 = _prodLeadTime.GetOverride("Override3")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].Quarter_Shear)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].Quarter_Shear, dates[2]);


                line4 = _prodLeadTime.GetOverride("Override4")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].Half_Shear)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].Half_Shear, dates[3]);


                line5 = _prodLeadTime.GetOverride("Override5")
                            ? Checkifoutofservice(_prodLeadTime.pltcoll[i].BurnTable)
                            : Checkifoutofservice(_prodLeadTime.pltcoll[i].BurnTable, dates[4]);
            }
        }
        else {
            line1 = "N/A";
            line2 = "N/A";
            line3 = "N/A";
            line4 = "N/A";
            line5 = "N/A";
            timeUpdated = "N/A";
            updatedBy = "N/A";
        }

        Label1_pltnoedit.Text = line1.Trim();
        Label2_pltnoedit.Text = line2.Trim();
        Label3_pltnoedit.Text = line3.Trim();
        Label4_pltnoedit.Text = line4.Trim();
        Label5_pltnoedit.Text = line5.Trim();
        lbl_timeupdated_pltnoedit.Text = timeUpdated.Trim();

        MemberDatabase m = new MemberDatabase(updatedBy.Trim());
        lbl_updatedby_pltnoedit.Text = HelperMethods.MergeFMLNames(m);
        lbl_lastrefreshed_pltviewer.Text = "<b class='pad-right'>Last Refresh</b>" + DateTime.Now.ToString();
    }

    private static string Checkifoutofservice(string a) {
        return a.ToLower() == "out of service" ? "Out of Service" : a;
    }

    private static string Checkifoutofservice(string a, string b) {
        return a.ToLower() == "out of service" ? "Out of Service" : b;
    }

    private string BuildJS
    {
        get
        {
            StringBuilder str = new StringBuilder();
            str.Append("Sys.Application.add_load(function () {");
            str.Append("$.ajax({");
            str.Append("url: 'Apps/ProdLeadTimes/GetPLT.asmx/GetTimes_ForViewer',");
            str.Append("type: 'POST',");
            str.Append("data: '{ }',");
            str.Append("contentType: 'application/json; charset=utf-8',");
            str.Append("success: function (data) {");
            str.Append("if (data.d.length > 0) {");
            str.Append("$('#Label1_pltnoedit').html(data.d[0]);");
            str.Append("$('#Label2_pltnoedit').html(data.d[1]);");
            str.Append("$('#Label3_pltnoedit').html(data.d[2]);");
            str.Append("$('#Label4_pltnoedit').html(data.d[3]);");
            str.Append("$('#Label5_pltnoedit').html(data.d[4]);");
            str.Append("$('#lbl_timeupdated_pltnoedit').html(data.d[5]);");
            str.Append("$('#lbl_updatedby_pltnoedit').html(data.d[6]);");
            str.Append("} } }); });");
            return str.ToString();
        }
    }


    /// <summary>
    ///     These methods must be on every user control that is a app (.ascx)
    /// </summary>
    /// <param name="control"></param>
    /// <param name="index"></param>
    //#region Register Controls
    //protected override void AddedControl(Control control, int index)
    //{
    //    foreach (Control c in control.Controls)
    //    {
    //        if (ss.CanRegisterControl(c))
    //        {
    //            ScriptManager.GetCurrent(Page).RegisterAsyncPostBackControl(c);
    //        }

    //        if (c.HasControls())
    //        {
    //            RegisterAsynControls(c.Controls);
    //        }
    //    }
    //    base.AddedControl(control, index);
    //}

    //private void RegisterAsynControls(ControlCollection page)
    //{
    //    foreach (Control c in page)
    //    {
    //        if (ss.CanRegisterControl(c))
    //        {
    //            ScriptManager.GetCurrent(Page).RegisterAsyncPostBackControl(c);
    //        }

    //        if (c.HasControls())
    //        {
    //            RegisterAsynControls(c.Controls);
    //        }
    //    }
    //}

    //protected override void Render(HtmlTextWriter writer)
    //{
    //    RegisterEventValidation(Page.Controls);
    //    base.RenderChildren(writer);
    //}

    //private void RegisterEventValidation(ControlCollection controls)
    //{
    //    foreach (Control c in controls)
    //    {
    //        if (ss.CanRegisterControl(c))
    //        {
    //            Page.ClientScript.RegisterForEventValidation(c.UniqueID);
    //        }

    //        if (c.HasControls())
    //        {
    //            RegisterEventValidation(c.Controls);
    //        }
    //    }
    //}

    //#endregion
}