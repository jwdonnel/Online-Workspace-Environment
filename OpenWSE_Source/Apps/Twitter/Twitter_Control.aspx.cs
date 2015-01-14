#region

using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using AjaxControlToolkit;
using System.Configuration;
using System.Collections.Generic;
using OpenWSE_Tools.Apps;

#endregion

public partial class Apps_Twitter_Twitter_Control : Page {
    private readonly IPWatch ipwatch = new IPWatch(true);
    private ServerSettings ss = new ServerSettings();
    private MemberDatabase member;
    private TwitterFeeds tf;
    private string username;
    private AppInitializer _appInitializer;


    protected void Page_Load(object sender, EventArgs e) {
        IIdentity userId = HttpContext.Current.User.Identity;
        if (!userId.IsAuthenticated)
            Page.Response.Redirect("~/Default.aspx");

        _appInitializer = new AppInitializer("app-twitter", userId.Name, Page);
        if (_appInitializer.TryLoadPageEvent) {
            username = _appInitializer.UserName;
            member = _appInitializer.memberDatabase;

            // Initialize all the scripts and style sheets
            _appInitializer.SetHeaderLabelImage(lbl_Title, img_Title);
            _appInitializer.LoadScripts_JS(false);
            _appInitializer.LoadScripts_CSS();

            ConfigurationManager.AppSettings.Set("act:TwitterAccessToken", ss.TwitterAccessToken);
            ConfigurationManager.AppSettings.Set("act:TwitterAccessTokenSecret", ss.TwitterAccessTokenSecret);
            ConfigurationManager.AppSettings.Set("act:TwitterConsumerKey", ss.TwitterConsumerKey);
            ConfigurationManager.AppSettings.Set("act:TwitterConsumerSecret", ss.TwitterConsumerSecret);

            GetFeeds();
        }
        else
            Page.Response.Redirect("~/ErrorPages/Blocked.html");
    }

    private void GetFeeds() {
        tf = new TwitterFeeds(username, true);
        pnl_twitter_holder.Controls.Clear();
        if (tf.twitter_list.Count > 0) {
            foreach (Dictionary<string, string> dr in tf.twitter_list) {
                try {
                    var twitter = new Twitter();
                    int displaycount = Convert.ToInt16(dr["Display"]);
                    twitter.CssClass = "ajax_twitter";

                    string id = dr["ID"];

                    if (string.IsNullOrEmpty(dr["TwitterSearch"]))
                        tf.deleteFeed(id);
                    else {
                        twitter.ID = id;
                        twitter.Title = dr["Title"];
                        twitter.Caption = dr["Caption"];
                        twitter.Count = displaycount;

                        switch (dr["Type"]) {
                            case "Search":
                                twitter.Mode = TwitterMode.Search;
                                twitter.Search = dr["TwitterSearch"];
                                break;
                            default:
                                twitter.Mode = TwitterMode.Profile;
                                twitter.ScreenName = dr["TwitterSearch"];
                                break;
                        }

                        string theme = member.SiteTheme;

                        string clearFeed = "<a href='#delete' class='td-cancel-btn RandomActionBtns' onclick='DeleteFeed(\"" + id + "\");return false;' title='Delete Feed'></a>";
                        string editFeed = "<a href='#edit' class='td-edit-btn margin-right RandomActionBtns' onclick='EditFeed(\"" + id + "\");return false;' title='Edit'></a>";


                        pnl_twitter_holder.Controls.Add(new LiteralControl("<div class='float-right pad-right-big pad-top-big' style='position: relative; z-index: 1000'>"));
                        pnl_twitter_holder.Controls.Add(new LiteralControl(editFeed + clearFeed + "</div>"));
                        pnl_twitter_holder.Controls.Add(twitter);
                    }
                }
                catch { }
            }
        }
        else {
            pnl_twitter_holder.Controls.Add(new LiteralControl("<div class='float-left pad-right-big pad-left-big pad-top-big'><h3>No Twitter Feeds Available. Click the 'Add Feed' button at the top right.</h3></div>"));
        }

        RegisterPostbackScripts.RegisterStartupScript(this, "AddHrefTarget_TwitterFeeds();");
    }

    protected void btn_add_Click(object sender, EventArgs e) {
        tf = new TwitterFeeds(username, false);
        lbl_errorTwitter.Text = string.Empty;
        string title = tb_title.Text;
        string caption = tb_caption.Text;
        string type = string.Empty;
        string search = tb_twitteraccount.Text;
        if (string.IsNullOrEmpty(search))
            lbl_errorTwitter.Text = "Must specify Account name or Search.";
        else {
            switch (dd_mode.SelectedValue) {
                case "1":
                    type = "Search";
                    break;
                default:
                    type = "Profile";
                    break;
            }

            if (string.IsNullOrEmpty(tb_twitteraccount.Text))
                type = "Search";

            if (string.IsNullOrEmpty(title)) {
                title = "No Title";
            }
            if (string.IsNullOrEmpty(caption)) {
                caption = string.Empty;
            }

            if (string.IsNullOrEmpty(hf_editID.Value))
                tf.addItem(Guid.NewGuid().ToString(), username, title, caption, search, dd_display_amount.SelectedValue, type);
            else
                tf.UpdateItem(hf_editID.Value, title, caption, search, dd_display_amount.SelectedValue, type);

            hf_editID.Value = string.Empty;
            tb_title.Text = string.Empty;
            tb_caption.Text = string.Empty;
            tb_twitteraccount.Text = string.Empty;
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'TwitterAdd-element', '');");
        }
        GetFeeds();
    }

    protected void btn_update_Click(object sender, EventArgs e) {
        tf = new TwitterFeeds(username, false);
        lbl_errorTwitter.Text = string.Empty;
        string title = tb_title.Text;
        string caption = tb_caption.Text;
        string type = string.Empty;
        string search = tb_twitteraccount.Text;
        if (string.IsNullOrEmpty(search))
            lbl_errorTwitter.Text = "Must specify Account name or Search.";
        else {
            switch (dd_mode.SelectedValue) {
                case "1":
                    type = "Search";
                    break;
                default:
                    type = "Profile";
                    break;
            }

            if (string.IsNullOrEmpty(tb_twitteraccount.Text))
                type = "Search";

            if (string.IsNullOrEmpty(title)) {
                title = "No Title";
            }
            if (string.IsNullOrEmpty(caption)) {
                caption = string.Empty;
            }

            if (string.IsNullOrEmpty(hf_editID.Value))
                tf.addItem(Guid.NewGuid().ToString(), username, title, caption, search, dd_display_amount.SelectedValue, type);
            else
                tf.UpdateItem(hf_editID.Value, title, caption, search, dd_display_amount.SelectedValue, type);

            hf_editID.Value = string.Empty;
            tb_title.Text = string.Empty;
            tb_caption.Text = string.Empty;
            tb_twitteraccount.Text = string.Empty;
            RegisterPostbackScripts.RegisterStartupScript(this, "openWSE.LoadModalWindow(false, 'TwitterAdd-element', '');");
        }
        GetFeeds();
    }

    protected void btn_refresh_Click(object sender, EventArgs e) {
        GetFeeds();
    }

    protected void hf_delete_TwitterFeed_Changed(object sender, EventArgs e) {
        string id = hf_delete_TwitterFeed.Value;
        tf = new TwitterFeeds(username, false);
        tf.deleteFeed(id);
        hf_delete_TwitterFeed.Value = string.Empty;
        GetFeeds();
    }

    protected void hf_edit_TwitterFeed_Changed(object sender, EventArgs e) {
        string id = hf_edit_TwitterFeed.Value;
        hf_editID.Value = id;
        lbl_errorTwitter.Text = string.Empty;
        tf = new TwitterFeeds(username, false);
        Dictionary<string, string> row = tf.GetRow(id);
        if (row != null) {
            tb_title.Text = row["Title"];
            tb_caption.Text = row["Caption"];
            tb_twitteraccount.Text = row["TwitterSearch"];

            switch (row["Type"]) {
                case "Search":
                    dd_mode.SelectedIndex = 0;
                    break;
                default:
                    dd_mode.SelectedIndex = 1;
                    break;
            }

            string display = row["Display"];
            for (int i = 0; i < dd_display_amount.Items.Count; i++) {
                if (dd_display_amount.Items[i].Value == display) {
                    dd_display_amount.SelectedIndex = i;
                    break;
                }
            }
        }

        hf_edit_TwitterFeed.Value = string.Empty;
        GetFeeds();
    }

    protected void hf_updateall_Changed(object sender, EventArgs e) {
        GetFeeds();
    }
}