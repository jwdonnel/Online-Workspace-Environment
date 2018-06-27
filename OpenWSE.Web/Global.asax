<%@ Application Language="C#" %>
<%@ Import Namespace="System.Globalization" %>
<script RunAt="server">

    private void Application_Start(object sender, EventArgs e) {
        ServerSettings.RunStartServerApplication = true;
    }

    private void Application_End(object sender, EventArgs e) {
        GetSiteRequests.ClearRequests();
    }

    private void Application_BeginRequest(object sender, EventArgs e) {
        try {
            if (Context != null && Context.Request != null && Context.Request.Url != null && !Context.Request.Url.OriginalString.ToLower().Contains("getgraphs")) {
                GetSiteRequests.AddRequest();
            }
        }
        catch { }
    }

    private void Application_Error(object sender, EventArgs e) {
        try {
            if (Context != null && Context.Error != null) {
                AppLog.AddError(Context.Error);
            }
        }
        catch { }
    }

    private void Session_Start(object sender, EventArgs e) {
        try {
            if (Context != null && Context.User != null && Context.User.Identity != null && Context.User.Identity.IsAuthenticated) {
                MemberDatabase.AddUserSessionIds(Context.User.Identity.Name);
            }
        }
        catch { }
    }

    private void Session_End(object sender, EventArgs e) {
        try {
            if (Context != null && Context.User != null && Context.User.Identity != null && Context.User.Identity.IsAuthenticated) {
                MemberDatabase.DeleteUserSessionId(Context.User.Identity.Name);
            }
        }
        catch { }
    }
    
</script>
