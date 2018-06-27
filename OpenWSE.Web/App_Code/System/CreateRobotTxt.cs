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
    private HttpContext _context;

    public CreateRobotTxt() {
        _xmlDoc = new XmlDocument();
        _context = HttpContext.Current;
    }

    public void Create() {
        if (_context == null || _context.Request == null) {
            return;
        }

        try {
            List<string> lines = new List<string>();

            lines.Add("User-Agent: *");
            lines.Add("Disallow: /App_Data/");
            lines.Add("Disallow: /Backups/");
            lines.Add("Disallow: /bin/");
            lines.Add("Disallow: /CloudFiles/");
            lines.Add("Disallow: /ScriptResource.axd");
            lines.Add("Disallow: /WebResource.axd");

            if (File.Exists(ServerSettings.GetServerMapLocation + CreateSitemap.SiteMapFileName)) {
                string sitePath = ServerSettings.GetSitePath(_context.Request);
                if (sitePath[sitePath.Length - 1] != '/') {
                    sitePath += "/";
                }

                string path = _context.Request.Url.Scheme + ":" + sitePath;
                lines.Add("Sitemap: " + path + CreateSitemap.SiteMapFileName);
            }

            File.WriteAllLines(ServerSettings.GetServerMapLocation + RobotsFileName, lines.ToArray());
        }
        catch (Exception e) {
            AppLog.AddError(e);
        }
    }
}