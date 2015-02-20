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
            lines.Add("Disallow: /App_Data/");
            lines.Add("Disallow: /Backups/");
            lines.Add("Disallow: /bin/");
            lines.Add("Disallow: /CloudFiles/");

            if (File.Exists(ServerSettings.GetServerMapLocation + CreateSitemap.SiteMapFileName)) {
                string sitePath = ServerSettings.GetSitePath(_request);
                if (sitePath[sitePath.Length - 1] != '/') {
                    sitePath += "/";
                }

                string path = _request.Url.Scheme + ":" + sitePath;
                lines.Add("Sitemap: " + path + CreateSitemap.SiteMapFileName);
            }

            File.WriteAllLines(ServerSettings.GetServerMapLocation + RobotsFileName, lines.ToArray());
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
    }
}