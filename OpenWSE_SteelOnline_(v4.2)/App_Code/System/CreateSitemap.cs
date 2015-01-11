using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for CreateSitemap
/// </summary>
public class CreateSitemap
{
    public const string SiteMapFileName = "sitemap.xml";
    private XmlDocument _xmlDoc;
    private HttpRequest _request;

    public CreateSitemap() {
        _xmlDoc = new XmlDocument();
        _request = HttpContext.Current.Request;
    }

    public void Create() {
        if (_request == null) {
            return;
        }

        try {
            XmlDeclaration declaration = _xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", string.Empty);
            _xmlDoc.AppendChild(declaration);

            XmlNode rootNode = _xmlDoc.CreateNode(XmlNodeType.Element, "urlset", "");
            XmlAttribute rootAttr = _xmlDoc.CreateAttribute("xmlns");
            rootAttr.Value = "http://www.sitemaps.org/schemas/sitemap/0.9";
            rootNode.Attributes.Append(rootAttr);

            string sitePath = _request.Url.Scheme + "://" + _request.Url.Authority;

            SiteMapNodeCollection siteNodes = SiteMap.RootNode.ChildNodes;
            foreach (SiteMapNode node in siteNodes) {
                XmlNode urlNode = _xmlDoc.CreateNode(XmlNodeType.Element, "url", "");

                XmlNode locNode = _xmlDoc.CreateNode(XmlNodeType.Element, "loc", "");
                locNode.InnerText = sitePath + node.Url;

                XmlNode priorityNode = _xmlDoc.CreateNode(XmlNodeType.Element, "priority", "");
                priorityNode.InnerText = "0.5";

                XmlNode lastmodNode = _xmlDoc.CreateNode(XmlNodeType.Element, "lastmod", "");
                string filePath = ServerSettings.GetServerMapLocation + node.Url.Replace(_request.ApplicationPath + "/", string.Empty).Replace("/", "\\");
                if (File.Exists(filePath)) {
                    lastmodNode.InnerText = File.GetLastWriteTime(filePath).ToShortDateString();
                }

                urlNode.AppendChild(locNode);
                urlNode.AppendChild(priorityNode);
                urlNode.AppendChild(lastmodNode);

                rootNode.AppendChild(urlNode);
            }

            _xmlDoc.AppendChild(rootNode);
            _xmlDoc.Save(ServerSettings.GetServerMapLocation + SiteMapFileName);
        }
        catch (Exception e) {
            new AppLog(false).AddError(e);
        }
    }

}