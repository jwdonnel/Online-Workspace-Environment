using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Xml;
using System.Data;
using System.IO;
using System.Web.Security;
using OpenWSE_Tools.GroupOrganizer;


/// <summary>
/// Summary description for RSSCreator
/// </summary>
public class RSSCreator
{
    private string _serverLoc;
    private string _userName;

	public RSSCreator(string serverLoc, string userName)
	{
        _serverLoc = serverLoc;
        _userName = userName;
	}

    public void Create(string groupName)
    {
        string fileLoc = _serverLoc + "Apps\\MessageBoard\\RSS_Feeds\\" + groupName + ".rss";

        if (!Directory.Exists(_serverLoc + "Apps\\MessageBoard\\RSS_Feeds"))
        {
            try
            {
                Directory.CreateDirectory(_serverLoc + "Apps\\MessageBoard\\RSS_Feeds");
            }
            catch { }
        }

        try
        {
            if (File.Exists(fileLoc))
                File.Delete(fileLoc);
        }
        catch { }

        Groups groups = new Groups(_userName);
        string gn = groups.GetGroupName_byID(groupName);

        using (FileStream fs = File.Create(_serverLoc + "Apps\\MessageBoard\\RSS_Feeds\\" + groupName + ".rss"))
        {
            XmlTextWriter xtwFeed = new XmlTextWriter(fs, Encoding.UTF8);
            xtwFeed.WriteStartDocument();

            // The mandatory rss tag
            xtwFeed.WriteStartElement("rss");
            xtwFeed.WriteAttributeString("version", "2.0");

            SiteMessageBoard messageBoard = new SiteMessageBoard(_userName);

            // The channel tag contains RSS feed details
            xtwFeed.WriteStartElement("channel");
            xtwFeed.WriteElementString("title", gn + " Posts");
            xtwFeed.WriteElementString("link", HttpContext.Current.Request.Url.OriginalString);
            xtwFeed.WriteElementString("description", "The latest message board postings from the " + gn + " group.");
            xtwFeed.WriteElementString("copyright", "Copyright " + DateTime.Now.Year.ToString() + " " + OpenWSE.Core.Licensing.CheckLicense.SiteName + ". All rights reserved.");

            // Loop through the content of the database and add them to the RSS feed
            int count = 0;
            messageBoard.getEntriesByGroup(groupName);
            List<Dictionary<string, string>> dt = messageBoard.post_dt;
            foreach (Dictionary<string, string> row in dt)
            {
                if (count > 20)
                    break;

                xtwFeed.WriteStartElement("item");
                xtwFeed.WriteElementString("title", row["UserName"] + " posted:");
                xtwFeed.WriteElementString("description",  HttpUtility.UrlDecode(row["Post"]));
                xtwFeed.WriteElementString("pubDate", row["Date"]);
                xtwFeed.WriteElementString("author", row["UserName"]);
                xtwFeed.WriteEndElement();
                count++;
            }

            // Close all tags 
            xtwFeed.WriteEndElement();
            xtwFeed.WriteEndElement();
            xtwFeed.WriteEndDocument();
            xtwFeed.Flush();
            xtwFeed.Close();
        }

        string loc = ServerSettings.GetSitePath(HttpContext.Current.Request) + "/Apps/MessageBoard/RSS_Feeds/" + groupName + ".rss";

        MembershipUserCollection userColl = Membership.GetAllUsers();
        foreach (MembershipUser memberUser in userColl) {
            MemberDatabase _member = new MemberDatabase(memberUser.UserName);
            if ((_member.UserHasApp("app-rssfeed")) && (groups.IsApartOfGroup(_member.GroupList, groupName))) {
                RSSFeeds feeds = new RSSFeeds(memberUser.UserName.ToLower());
                feeds.AddItem(gn, loc, Guid.NewGuid().ToString(), true);
            }
        }
    }
}