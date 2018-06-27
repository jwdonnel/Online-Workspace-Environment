using OpenWSE_Tools.Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

public class DBImporterAppLoader : UserControl {

    private ServerSettings _ss = new ServerSettings();
    private App _apps;
    private AppInitializer _appInitializer;
    private UserControl userControl;

    protected void Page_Load(object sender, EventArgs e) {
        string userName = GetUsername();
        _apps = new App(userName);

        userControl = (UserControl)sender;
        if (userControl != null) {
            string appClassName = userControl.GetType().Name;
            string app_id = "app-" + appClassName;

            DBImporter_Coll coll = dbImportservice.GetDataBase_Coll(appClassName);
            DBImporter dbImporter = new DBImporter();
            string cl = _apps.GetAppName(app_id);

            Label lbl_Title = GetLabelControl("lbl_Title_" + appClassName);
            if (lbl_Title != null) {
                lbl_Title.Text = cl;
            }

            Image img_Title = GetImageControl("img_Title_" + appClassName);
            if (img_Title != null) {
                img_Title.Visible = true;
                string clImg = _apps.GetAppIconName(app_id);
                img_Title.ImageUrl = "~/" + clImg;
            }

            HtmlAnchor btn_addRecord = GetHtmlAnchorControl("btn_" + appClassName + "_addRecord");
            if (btn_addRecord != null) {
                if (coll.AllowEdit) {
                    btn_addRecord.Visible = coll.UsersAllowedToEdit.Contains(userName.ToLower());
                }
                else {
                    btn_addRecord.Visible = false;
                }
            }

            JavaScriptSerializer serializer = ServerSettings.CreateJavaScriptSerializer();

            HiddenField hf_customizations = GetHiddenFieldControl("hf_" + appClassName + "_customizations");
            if (hf_customizations != null) {
                hf_customizations.Value = string.Empty;
                try {
                    string tableCustomizations = HttpUtility.UrlEncode(serializer.Serialize(coll.TableCustomizations));
                    hf_customizations.Value = tableCustomizations;
                }
                catch { }
            }

            HiddenField hf_summaryData = GetHiddenFieldControl("hf_" + appClassName + "_summaryData");
            if (hf_summaryData != null) {
                hf_summaryData.Value = string.Empty;
                try {
                    string summaryDataStr = serializer.Serialize(coll.SummaryData);
                    summaryDataStr = summaryDataStr.Replace(" ", "&nbsp;");
                    string tableSummary = HttpUtility.UrlEncode(summaryDataStr);
                    hf_summaryData.Value = tableSummary;
                }
                catch { }
            }

            HtmlGenericControl appdesc = GetHtmlGenericControl("appdesc_" + appClassName);
            HtmlGenericControl app_title_bgcolor = GetHtmlGenericControl("app_title_bgcolor_" + appClassName);

            foreach (CustomTableCustomizations customizations in coll.TableCustomizations) {
                switch (customizations.customizeName) {
                    case "ShowDescriptionOnApp":
                        if (HelperMethods.ConvertBitToBoolean(customizations.customizeValue) && !string.IsNullOrEmpty(coll.Description)) {
                            if (appdesc != null) {
                                appdesc.InnerHtml = coll.Description;
                                appdesc.Visible = true;
                            }
                        }
                        break;
                    case "AppStyleTitleColor":
                        if (!string.IsNullOrEmpty(customizations.customizeValue)) {
                            string titleColor = customizations.customizeValue;
                            if (!titleColor.StartsWith("#")) {
                                titleColor = "#" + titleColor;
                            }

                            if (lbl_Title != null) {
                                lbl_Title.Style["color"] = titleColor;
                            }

                            if (appdesc != null) {
                                appdesc.Style["color"] = titleColor;
                            }
                        }
                        break;
                    case "AppStyleBackgroundColor":
                        if (!string.IsNullOrEmpty(customizations.customizeValue)) {
                            string bgColor = customizations.customizeValue;
                            if (!bgColor.StartsWith("#")) {
                                bgColor = "#" + bgColor;
                            }

                            if (app_title_bgcolor != null) {
                                app_title_bgcolor.Style["background-color"] = bgColor;
                            }
                        }
                        break;
                    case "AppStyleBackgroundImage":
                        if (!string.IsNullOrEmpty(customizations.customizeValue)) {
                            if (app_title_bgcolor != null) {
                                app_title_bgcolor.Style["background-image"] = "url('" + customizations.customizeValue + "')";
                                app_title_bgcolor.Style["background-repeat"] = "repeat";
                                app_title_bgcolor.Style["background-position"] = "center center";
                            }
                        }
                        break;
                    case "TableViewStyle":
                        HiddenField hf_tableview = GetHiddenFieldControl("hf_" + appClassName + "_tableview");
                        if (hf_tableview != null) {
                            hf_tableview.Value = customizations.customizeValue;
                        }

                        if (customizations.customizeValue == "excel") {
                            HtmlGenericControl li_viewMode = GetHtmlGenericControl("li_" + appClassName + "_viewMode");
                            if (li_viewMode != null) {
                                li_viewMode.Visible = false;
                            }
                        }
                        break;
                }
            }

            ChartType chart_Type = dbImporter.GetChartTypeFromCustomizations(coll.TableCustomizations);
            string chartColumns = dbImporter.GetChartColumnsFromCustomizations(coll.TableCustomizations);
            Panel pnl_chartType = GetPanelControl("pnl_" + appClassName + "_chartType");
            HtmlGenericControl li_chartType = GetHtmlGenericControl("li_" + appClassName + "_chartType");

            if (chart_Type != ChartType.None && chartColumns.Split(ServerSettings.StringDelimiter_Array, StringSplitOptions.RemoveEmptyEntries).Length > 0) {
                string chartTitle = dbImporter.GetChartTitleFromCustomizations(coll.TableCustomizations);
                if (pnl_chartType != null) {
                    pnl_chartType.Enabled = true;
                    pnl_chartType.Visible = true;
                }

                if (li_chartType != null) {
                    li_chartType.Visible = true;
                }

                Image img_chartType = GetImageControl("img_" + appClassName + "_chartType");
                if (img_chartType != null) {
                    img_chartType.ImageUrl = "~/Standard_Images/ChartTypes/" + chart_Type.ToString().ToLower() + ".png";
                }

                HiddenField hf_chartType = GetHiddenFieldControl("hf_" + appClassName + "_chartType");
                if (hf_chartType != null) {
                    hf_chartType.Value = chart_Type.ToString();
                }

                if (string.IsNullOrEmpty(chartTitle)) {
                    chartTitle = coll.TableName;
                }

                HiddenField hf_chartTitle = GetHiddenFieldControl("hf_" + appClassName + "_chartTitle");
                if (hf_chartTitle != null) {
                    hf_chartTitle.Value = chartTitle;
                }

                HiddenField hf_chartColumns = GetHiddenFieldControl("hf_" + appClassName + "_chartColumns");
                if (hf_chartColumns != null) {
                    hf_chartColumns.Value = chartColumns;
                }
            }
            else {
                if (pnl_chartType != null) {
                    pnl_chartType.Enabled = false;
                    pnl_chartType.Visible = false;
                }

                if (li_chartType != null) {
                    li_chartType.Visible = false;
                }
            }

            _appInitializer = new OpenWSE_Tools.Apps.AppInitializer(app_id, userName, Page, "Table Imports");
            _appInitializer.LoadScripts_JS(true, "dbImport.Load('" + appClassName + "');");
        }
    }

    private string GetUsername() {
        if (Page != null && Page.User != null && Page.User.Identity != null && Page.User.Identity.IsAuthenticated) {
            return Page.User.Identity.Name;
        }
        else if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated) {
            return HttpContext.Current.User.Identity.Name;
        }

        return string.Empty;
    }

    #region Get Control Methods

    private Label GetLabelControl(string name) {
        return (Label)userControl.FindControl(name);
    }
    private Image GetImageControl(string name) {
        return (Image)userControl.FindControl(name);
    }
    private HtmlAnchor GetHtmlAnchorControl(string name) {
        return (HtmlAnchor)userControl.FindControl(name);
    }
    private HiddenField GetHiddenFieldControl(string name) {
        return (HiddenField)userControl.FindControl(name);
    }
    private HtmlGenericControl GetHtmlGenericControl(string name) {
        return (HtmlGenericControl)userControl.FindControl(name);
    }
    private Panel GetPanelControl(string name) {
        return (Panel)userControl.FindControl(name);
    }

    #endregion

}