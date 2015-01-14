using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for CreateRobotTxt
/// </summary>
public class CreateRobotTxt
{
    public const string RobotsFileName = "robots.txt";
    private XmlDocument _xmlDoc;
    private HttpRequest _request;

    public CreateRobotTxt() {
        _xmlDoc = new XmlDocument();
        _request = HttpContext.Current.Request;
    }

    public void Create() {
        if (_request == null) {
            return;
        }

        try {
            List<string> lines = new List<string>();

            lines.Add("User-Agent: *");
            lines.Add("Disallow: ");
            lines.Add("Allow: /");

            if (File.Exists(ServerSettings.GetServerMapLocation + CreateSitemap.SiteMapFileName)) {
                string sitePath = ServerSettings.GetSitePath(_request);
                if (sitePath[sitePath.Length - 1] != '/') {
                    sitePath += "/";
                }

                string path = _request.Url.Scheme + ":" + sitePath;

                lines.Add("Allow: /SiteTools/CustomContent/");
                lines.Add("Allow: /SiteTools/DBMaintenance/");
                lines.Add("Allow: /SiteTools/ServerMaintenance/");
                lines.Add("Allow: /SiteTools/UserMaintenance/");
                lines.Add("Allow: /SiteTools/AppMaintenance/");
                lines.Add("");

                lines.Add("Sitemap: " + path + CreateSitemap.SiteMapFileName);
            }

            File.WriteAllLines(ServerSettings.GetServerMapLocation + RobotsFileName, lines.ToArray());
        }
        catch (Exception e) {
            new AppLog(false).AddError(e);
        }
    }
}