using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using OpenWSE_Tools.GroupOrganizer;

/// <summary>
/// Summary description for DatabaseImportCreator
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class DatabaseImportCreator : System.Web.Services.WebService {

    public DatabaseImportCreator () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string EditUsersAllowedToEditForDatabaseImport(string id) {
        StringBuilder str = new StringBuilder();

        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            DBImporter db = new DBImporter();
            db.BinaryDeserialize();
            foreach (DBImporter_Coll coll in db.DBColl) {
                if (coll.ID == id) {
                    MemberDatabase importedByMember = new MemberDatabase(coll.ImportedBy);
                    List<string> groupList = importedByMember.GroupList;
                    string checkboxInput = "<div class='checkbox-edit-click float-left pad-right-big pad-bottom-big' style='min-width: 150px;'><input type='checkbox' class='checkbox-usersallowed float-left margin-right-sml' {0} value='{1}' style='margin-top: {2};' />&nbsp;{3}</div>";
                    Groups groups = new Groups(coll.ImportedBy);

                    foreach (string group in groupList) {
                        List<string> users = groups.GetMembers_of_Group(group);
                        string groupImg = groups.GetGroupImg_byID(group);

                        if (groupImg.StartsWith("~/")) {
                            groupImg = ServerSettings.ResolveUrl(groupImg);
                        }

                        string groupImgHtmlCtrl = "<img alt='' src='" + groupImg + "' class='float-left margin-right' style='max-height: 24px;' />";
                        str.Append("<h3 class='pad-bottom'>" + groupImgHtmlCtrl + groups.GetGroupName_byID(group) + "</h3><div class='clear-space'></div><div class='clear-space'></div>");
                        foreach (string user in users) {
                            string isChecked = string.Empty;
                            bool foundUser = !string.IsNullOrEmpty(coll.UsersAllowedToEdit.Find(_x => _x.ToLower() == user.ToLower()));
                            if (foundUser) {
                                isChecked = "checked='checked'";
                            }
                            MemberDatabase tempMember = new MemberDatabase(user);

                            string un = HelperMethods.MergeFMLNames(tempMember);
                            if ((user.Length > 15) && (!string.IsNullOrEmpty(tempMember.LastName)))
                                un = tempMember.FirstName + " " + tempMember.LastName[0].ToString() + ".";

                            if (un.ToLower() == "n/a")
                                un = user;

                            string marginTop = "3px";
                            string userNameTitle = "<h4>" + un + "</h4>";
                            string acctImage = tempMember.AccountImage;
                            if (!string.IsNullOrEmpty(acctImage)) {
                                userNameTitle = "<h4 class='float-left pad-top pad-left-sml'>" + un + "</h4>";
                                marginTop = "8px";
                            }

                            string userImageAndName = UserImageColorCreator.CreateImgColor(acctImage, tempMember.UserColor, tempMember.UserId, 30);
                            str.AppendFormat(checkboxInput, isChecked, user, marginTop, userImageAndName + userNameTitle);
                        }
                        str.Append("<div class='clear-space'></div><div class='clear-space'></div><div class='clear-space'></div>");
                    }

                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(str.ToString())) {
            str.Append("<h4 class='pad-all'>There are no usrs to select from</h4>");
        }

        return str.ToString();
    }

    [WebMethod]
    public void UpdateUsersAllowedToEditForDatabaseImport(string id, string usersAllowed) {
        if (HttpContext.Current.User.Identity.IsAuthenticated) {
            DBImporter dbImporter = new DBImporter();
            dbImporter.UpdateUsersAllowedToEdit(id, usersAllowed);
        }
    }

}
