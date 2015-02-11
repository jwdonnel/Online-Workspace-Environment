<%@ Application Language="C#" %>
<%@ Import Namespace="System.Globalization" %>
<script RunAt="server">

    private void Application_Start(object sender, EventArgs e) {
        ServerSettings.StartServerApplication();
    }

    private void Application_End(object sender, EventArgs e) { }

    private void Application_BeginRequest(object sender, EventArgs e) {
        if (!Context.Request.Url.OriginalString.ToLower().Contains("getgraphs")) {
            GetSiteRequests.AddRequest();
        }
    }
    
    private void Application_Error(object sender, EventArgs e) {
        var applog = new AppLog(false);
        applog.AddError(Context.Error);
    }

    private void Session_Start(object sender, EventArgs e) { }

    private void Session_End(object sender, EventArgs e) { }
    
</script>
