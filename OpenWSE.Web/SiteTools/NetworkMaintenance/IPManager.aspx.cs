using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenWSE_Tools.AutoUpdates;

public partial class SiteTools_IPManager : BasePage {

    #region Private Variables

    private IPWatch _ipwatch = new IPWatch(true);
    private LoginActivity _la = new LoginActivity();

    #endregion

    protected void Page_Load(object sender, EventArgs e) {
        if (!IsPostBack) {
            BaseMaster.BuildLinks(pnlLinkBtns, CurrentUsername, this.Page);
            if (!MainServerSettings.LockIPListenerWatchlist) {
                pnl_ipAutoBlock.Enabled = true;
                pnl_ipAutoBlock.Visible = true;
            }
            else {
                pnl_ipAutoBlock.Enabled = false;
                pnl_ipAutoBlock.Visible = false;
                ltl_lockedipwatchlist.Text = HelperMethods.GetLockedByMessage();
            }

            tb_autoblock_count.Text = MainServerSettings.AutoBlockIPCount.ToString();
        }

        if (!IsUserInAdminRole()) {
            rb_AutoIPBlock_on.Enabled = false;
            rb_AutoIPBlock_off.Enabled = false;
            pnl_ipAutoBlock.Enabled = false;
            pnl_ipAutoBlock.Visible = false;
            networkSettings.Visible = false;
        }

        if (MainServerSettings.AutoBlockIP) {
            rb_AutoIPBlock_on.Checked = true;
            rb_AutoIPBlock_off.Checked = false;
            pnl_attemptsBeforeBlock.Enabled = true;
            pnl_attemptsBeforeBlock.Visible = true;
        }
        else {
            rb_AutoIPBlock_on.Checked = false;
            rb_AutoIPBlock_off.Checked = true;
            pnl_attemptsBeforeBlock.Enabled = false;
            pnl_attemptsBeforeBlock.Visible = false;
        }

        BuildIpAddresses();
        LoadIpAddresses();

        RegisterPostbackScripts.RegisterStartupScript(this, "SetToCurrentIP('" + CurrentIpAddress + "');");
    }

    #region IP Watchlist

    public void LoadIpAddresses() {
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, IsUserInAdminRole() && !MainServerSettings.LockIPListenerWatchlist, 2, "IPWatchlist_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("IP Address", "200", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Attempts", "60", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Last Attempt", "160", false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        foreach (Dictionary<string, string> row in _ipwatch.ipwatchdt) {
            string ipAddress = row["IPAddress"];

            string blockedClass = "td-allow-btn RandomActionBtns";
            string blockButtonTitleAttr = string.Format("Block {0}", ipAddress);
            string blockedMessage = "Allowed";

            if (HelperMethods.ConvertBitToBoolean(row["Blocked"])) {
                blockedClass = "td-ignore-btn RandomActionBtns";
                blockButtonTitleAttr = string.Format("Allow {0}", ipAddress);
                blockedMessage = "Blocked";
            }

            string lastAttempt = row["LastAttempt"];
            if (string.IsNullOrEmpty(lastAttempt)) {
                lastAttempt = "N/A";
            }

            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("IP Address", ipAddress + "<small> - " + blockedMessage + "</small>", TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Attempts", row["Attempts"], TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Last Attempt", lastAttempt, TableBuilderColumnAlignment.Left));

            string allowBlockBtn = string.Format("<a href=\"#block-allow\" class=\"{0}\" title=\"{1}\" onclick=\"blockAllowIpAddress('{2}');return false;\"></a>", blockedClass, blockButtonTitleAttr, ipAddress);
            string deleteBtn = string.Format("<a href=\"#delete\" class=\"td-delete-btn\" title=\"Delete {0} from IP List\" onclick=\"confirmDeleteWatchlistIP('{0}');return false;\"></a>", ipAddress);

            tableBuilder.AddBodyRow(bodyColumnValues, allowBlockBtn + deleteBtn);
        }
        #endregion

        #region Build Insert
        List<TableBuilderInsertColumnValues> insertColumns = new List<TableBuilderInsertColumnValues>();
        insertColumns.Add(new TableBuilderInsertColumnValues("IP Address", "txt_ipaddress_watchlist", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text));
        insertColumns.Add(new TableBuilderInsertColumnValues("Attempts", string.Empty, TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
        insertColumns.Add(new TableBuilderInsertColumnValues("Last Attempt", string.Empty, TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
        tableBuilder.AddInsertRow(insertColumns, "AddIPWatchlist()");
        #endregion

        pnl_watchlist.Controls.Clear();
        pnl_watchlist.Controls.Add(tableBuilder.CompleteTableLiteralControl("There are no IP Addresses being watched"));
    }
    protected void rb_AutoIPBlock_on_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_AutoIPBlock_on.Checked = true;
            rb_AutoIPBlock_off.Checked = false;
            pnl_attemptsBeforeBlock.Enabled = true;
            pnl_attemptsBeforeBlock.Visible = true;
            ServerSettings.update_AutoBlockIP(true);
        }
    }
    protected void rb_AutoIPBlock_off_CheckedChanged(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            rb_AutoIPBlock_on.Checked = false;
            rb_AutoIPBlock_off.Checked = true;
            pnl_attemptsBeforeBlock.Enabled = false;
            pnl_attemptsBeforeBlock.Visible = false;
            ServerSettings.update_AutoBlockIP(false);
        }
    }
    protected void btn_autoblock_count_Clicked(object sender, EventArgs e) {
        if (IsUserInAdminRole()) {
            int tempOut = 0;
            if (int.TryParse(tb_autoblock_count.Text, out tempOut)) {
                if (tempOut > 0) {
                    ServerSettings.update_AutoBlockIPCount(tempOut);
                }
                else {
                    tb_autoblock_count.Text = MainServerSettings.AutoBlockIPCount.ToString();
                }
            }
            else {
                tb_autoblock_count.Text = MainServerSettings.AutoBlockIPCount.ToString();
            }
        }
    }
    protected void hf_AddIPAddress_Watchlist_ValueChanged(object sender, EventArgs e) {
        string ipAddress_Value = HttpUtility.UrlDecode(hf_AddIPAddress_Watchlist.Value);

        if (!string.IsNullOrEmpty(ipAddress_Value)) {
            if (Parse(ipAddress_Value)) {
                if (!_ipwatch.CheckIfExists(ipAddress_Value)) {
                    if (ipAddress_Value == CurrentIpAddress && !IsUserNameEqualToAdmin()) {
                        _ipwatch.addItem(ipAddress_Value, 0, false);
                    }
                    else {
                        _ipwatch.addItem(ipAddress_Value, 0, true);
                    }
                    _ipwatch = new IPWatch(true);
                    LoadIpAddresses();
                }
                else {
                    RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('IP already exists');");
                }
            }
            else {
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('IP address invalid');");
            }
        }
        else {
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('IP address invalid');");
        }

        hf_AddIPAddress_Watchlist.Value = string.Empty;
    }
    protected void hf_BlockAllowIPAddress_Watchlist_ValueChanged(object sender, EventArgs e) {
        string ipAddress_CommandArgument = hf_BlockAllowIPAddress_Watchlist.Value;

        bool blocked = true;
        if (_ipwatch.CheckIfBlocked(ipAddress_CommandArgument)) {
            blocked = false;
        }
        else {
            if (ipAddress_CommandArgument == CurrentIpAddress && !IsUserNameEqualToAdmin()) {
                blocked = false;
                RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.AlertWindow('Cannot block current IP');");
            }
        }
        _ipwatch.updateBlocked(ipAddress_CommandArgument, blocked);

        if (blocked) {
            MembershipUserCollection coll = Membership.GetAllUsers();
            foreach (MembershipUser uColl in coll) {
                MemberDatabase mb = new MemberDatabase(uColl.UserName);
                if ((mb.IpAddress == ipAddress_CommandArgument) && (uColl.IsOnline) && (uColl.UserName.ToLower() != CurrentUsername.ToLower())) {
                    UserUpdateFlags uFlags = new UserUpdateFlags();
                    uFlags.addFlag(uColl.UserName, "", "");
                }
            }
        }

        _ipwatch = new IPWatch(true);
        LoadIpAddresses();

        hf_BlockAllowIPAddress_Watchlist.Value = string.Empty;
    }
    protected void hf_DeleteWatchlistIP_ValueChanged(object sender, EventArgs e) {
        if (!string.IsNullOrEmpty(hf_DeleteWatchlistIP.Value)) {
            _ipwatch.deleteIP(hf_DeleteWatchlistIP.Value);
            _ipwatch = new IPWatch(true);
            LoadIpAddresses();
        }

        hf_DeleteWatchlistIP.Value = string.Empty;
    }

    #endregion


    #region IP Listener

    private void BuildIpAddresses() {
        var listener = new IPListener(true);
        TableBuilder tableBuilder = new TableBuilder(this.Page, true, IsUserInAdminRole() && !MainServerSettings.LockIPListenerWatchlist, 1, "IPListener_Gridview");

        #region Build Header
        List<TableBuilderHeaderColumns> headerColumns = new List<TableBuilderHeaderColumns>();
        headerColumns.Add(new TableBuilderHeaderColumns("IP Address", "200", true));
        headerColumns.Add(new TableBuilderHeaderColumns("Updated By", "150", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Date Updated", "160", false));
        headerColumns.Add(new TableBuilderHeaderColumns("Status", "90", false, false));
        tableBuilder.AddHeaderRow(headerColumns, true);
        #endregion

        #region Build Body
        bool hasOneActive = false;
        foreach (Dictionary<string, string> dr in listener.iplistener_table) {
            if (HelperMethods.ConvertBitToBoolean(dr["Active"])) {
                hasOneActive = true;
                break;
            }
        }

        int count = 0;
        foreach (Dictionary<string, string> row in listener.iplistener_table) {
            string ipAddress = row["IPAddress"];
            var m = new MemberDatabase(row["UpdatedBy"]);
            bool active = HelperMethods.ConvertBitToBoolean(row["Active"]);

            List<TableBuilderBodyColumnValues> bodyColumnValues = new List<TableBuilderBodyColumnValues>();
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("IP Address", ipAddress, TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Updated By", HelperMethods.MergeFMLNames(m), TableBuilderColumnAlignment.Left));
            bodyColumnValues.Add(new TableBuilderBodyColumnValues("Date Updated", row["Date"], TableBuilderColumnAlignment.Left));
            if (IsUserInAdminRole() && !MainServerSettings.LockIPListenerWatchlist) {
                string statusBtn = CreateRadioButtons_Listener(active, count, ipAddress, hasOneActive);
                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Status", statusBtn, TableBuilderColumnAlignment.Left));
            }
            else {
                string activeText = "Allowed";
                if (hasOneActive && !active) {
                    activeText = "Blocked";
                }
                else if (hasOneActive && active) {
                    activeText = "Listening";
                }

                bodyColumnValues.Add(new TableBuilderBodyColumnValues("Status", activeText, TableBuilderColumnAlignment.Left));
            }

            string deleteBtn = string.Format("<a href=\"#delete\" class=\"td-delete-btn\" onclick=\"DeleteIP('{0}');return false;\" title=\"Delete {0} from IP Listener\"></a>", ipAddress);

            tableBuilder.AddBodyRow(bodyColumnValues, deleteBtn);
            count++;
        }
        #endregion

        #region Build Insert
        List<TableBuilderInsertColumnValues> insertColumns = new List<TableBuilderInsertColumnValues>();
        insertColumns.Add(new TableBuilderInsertColumnValues("IP Address", "tb_createnew_listener", TableBuilderColumnAlignment.Left, TableBuilderInsertType.Text));
        insertColumns.Add(new TableBuilderInsertColumnValues("Updated By", string.Empty, TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
        insertColumns.Add(new TableBuilderInsertColumnValues("Date Updated", string.Empty, TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
        insertColumns.Add(new TableBuilderInsertColumnValues("Status", string.Empty, TableBuilderColumnAlignment.Left, TableBuilderInsertType.None));
        tableBuilder.AddInsertRow(insertColumns, "AddIPToListener()");
        #endregion

        pnl_iplistener_holder.Controls.Clear();
        pnl_iplistener_holder.Controls.Add(tableBuilder.CompleteTableLiteralControl("No data available"));
    }
    private string CreateRadioButtons_Listener(bool active, int count, string ip, bool hasOneActive) {
        var str = new StringBuilder();
        str.Append("<div class='field switch'>");
        string enabledclass = "RandomActionBtns cb-enable";
        string disabledclass = "cb-disable selected";
        string onclickEnable = "onclick='UpdateActive(\"" + ip + "\", 1, this)'";
        string onclickDisable = "";
        if (active) {
            enabledclass = "cb-enable selected";
            disabledclass = "RandomActionBtns cb-disable";
            onclickEnable = "";
            onclickDisable = "onclick='UpdateActive(\"" + ip + "\", 0, this)'";
        }

        string listenText = "Allowed";
        if (hasOneActive) {
            listenText = "Blocked";
        }

        str.Append("<span class='" + enabledclass + "'><input id='rb_listener_active_" +
                   count.ToString(CultureInfo.InvariantCulture) +
                   "' type='radio' value='active' " + onclickEnable + " /><label for='rb_listener_active_" +
                   count.ToString(CultureInfo.InvariantCulture) + "'>Listen</label></span>");
        str.Append("<span class='" + disabledclass + "'><input id='rb_listener_deactive_" +
                   count.ToString(CultureInfo.InvariantCulture) +
                   "' type='radio' value='deactive' " + onclickDisable + " /><label for='rb_listener_deactive_" +
                   count.ToString(CultureInfo.InvariantCulture) + "'>" + listenText + "</label></span>");
        str.Append("</div>");
        return str.ToString();
    }
    protected void hf_UpdateIPListener_ValueChanged(object sender, EventArgs e) {
        BuildIpAddresses();
    }

    #endregion

    private static bool Parse(string ipAddress) {
        try {
            var address = IPAddress.Parse(ipAddress);
        }
        catch {
            return false;
        }

        return true;
    }

}