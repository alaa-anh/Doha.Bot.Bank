using Microsoft.SharePoint.Client;
using System;
using System.Collections;
using System.Configuration;
using System.IO;
using SP = Microsoft.SharePoint.Client;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.SharePoint.Client.Utilities;
using System.Web;

namespace Common
{
    public class Sharepoint
    {
        private static string _userName;
        private static string _userPassword;
        private static string UserLoggedInName;

        
        private static string _serverURL = ConfigurationManager.AppSettings["ServerURL"];

        private static string _userNameAdmin = ConfigurationManager.AppSettings["DomainAdmin"];
        private static string _userPasswordAdmin = ConfigurationManager.AppSettings["DomainAdminPassword"];


        public static string checkAuthorizedUser(string name, string upassword)
        {
          

            try
            {
                using (ClientContext ctx = new ClientContext(_serverURL))
                {

                    SecureString passWord = new SecureString();
                    foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                    ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);


                    var user = ctx.Web.EnsureUser(name);
                    ctx.Load(user);
                    ctx.ExecuteQuery();

                    if (user != null)
                    {
                        UserLoggedInName = user.Title;
                    }
                }
            }
            catch (Exception ex)
            {
                UserLoggedInName = string.Empty;
            }
            return UserLoggedInName;
        }

        public static ListItemCollection GetSelectedTypeQuestions( string selectedFlowType)
        {

            using (ClientContext ctx = new ClientContext(_serverURL))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);


                List oList = ctx.Web.Lists.GetByTitle("Question Flow");
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = @"<Query>
                                   <Where>
                                      <Eq>
                                         <FieldRef Name = 'Flow_x0020_Type'/> 
                                          <Value Type = 'Choice'> "+ selectedFlowType + @" </Value>  
                                        </Eq>  
                                     </Where>
                                  </Query>";
                ListItemCollection collListItem = oList.GetItems(camlQuery);
                ctx.Load(collListItem);

                ctx.ExecuteQuery();

                return collListItem;
            }

        }

        public static ListItem GetQuestion(int QuestionID)
        {

            using (ClientContext ctx = new ClientContext(_serverURL))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);


                List oList = ctx.Web.Lists.GetByTitle("Question Flow");
               
               ListItem collListItem = oList.GetItemById(QuestionID);
                ctx.Load(collListItem);

                ctx.ExecuteQuery();

                    return collListItem;
            }

        }

        public static void SaveNewAnswer(string selectedFlowType , string NewTitle , string Desc , bool Usertype , string SubmittedBy , string filename)
        {
           // int AnswerRecordID = 0;
            using (ClientContext ctx = new ClientContext(_serverURL))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);
                List oList = ctx.Web.Lists.GetByTitle("Submitted Data");
                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                ListItem oListItem = oList.AddItem(itemCreateInfo);
                oListItem["New_x0020_Title"] = NewTitle;
                oListItem["Type_x0020_of_x0020_Submition"] = selectedFlowType;
                oListItem["Source"] = "Bot";
                oListItem["Description"] = Desc;
                oListItem["Anonymous"] = Usertype;
                oListItem["Submitted_x0020_By"] = SubmittedBy;

                if (filename != "")
                {
                    //string[] multifiles = filename.Split(',');
                    //foreach (string file in multifiles)
                    //{
                    //    var imagePath = HttpContext.Current.Server.MapPath("~/AttachmentFiles/" + file);
                    //    FileStream fs = new FileStream(imagePath, FileMode.Open);

                    //    AttachmentCreationInformation attInfo = new AttachmentCreationInformation();
                    //    attInfo.FileName = fs.Name;
                    //    attInfo.ContentStream = fs;
                    //    oListItem.AttachmentFiles.Add(attInfo);
                    //}
                }
                oListItem.Update();
                ctx.ExecuteQuery();
            }
        }


      


        //public static void addAttachmentToListItem(int itemID, string filename)
        //{

        //  // string strFileName = System.IO.Path.GetFileName(filePath);
        //  //  MyFile.PostedFile.SaveAs(Server.MapPath("servername/files") + strFileName);


        //    var imagePath = HttpContext.Current.Server.MapPath("~/AttachmentFiles/" + filename);
        //    FileStream fs = new FileStream(imagePath, FileMode.Open);
        //    SecureString passWord = new SecureString();
        //    foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);

        //    ClientContext Context = new ClientContext(_serverURL);
        //    Context.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);
        //    var list = Context.Web.Lists.GetByTitle("Submitted Data");
        //    Context.Load(list);
        //    Context.ExecuteQuery();
        //    ListItem oListItem = list.GetItemById(itemID);
        //    Context.Load(oListItem);

        //    AttachmentCreationInformation attInfo = new AttachmentCreationInformation();
        //    attInfo.FileName = fs.Name;
        //    attInfo.ContentStream = fs;
        //    oListItem.AttachmentFiles.Add(attInfo);
        //    oListItem.Update();
        //    Context.ExecuteQuery();
        //}

        //public static void addAttachmentToListItem(int itemID, string filePath , string filename)
        //{
        //    string MainContent = string.Empty;
        //    WebClient webClient = new WebClient();

        //    SecureString passWord = new SecureString();
        //    foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
        //    webClient.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);

        //    webClient.Headers.Add("Accept: text/html, application/xhtml+xml, application/pdf, */*");
        //    webClient.Headers.Add("User-Agent: Other");
        //    webClient.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
        //    webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json;odata=verbose");
        //    webClient.Headers.Add(HttpRequestHeader.Accept, "application/json;odata=verbose");
        //    webClient.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);
        //    MainContent = webClient.DownloadString(filePath);

        //    ClientContext Context = new ClientContext(_serverURL);
        //    Context.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);
        //    var list = Context.Web.Lists.GetByTitle("Submitted Data");
        //    Context.Load(list);
        //    Context.ExecuteQuery();

        //    ListItem itm = list.GetItemById(itemID);
        //    Context.Load(itm);

        //    //    newAtt.FileName = filename;// "myAttachment.txt";
        //    string fileContent = MainContent;// "This file is was ubloaded by client object meodel ";
        //    System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        //    byte[] buffer = enc.GetBytes(fileContent);

        //    AttachmentCreationInformation newAtt = new AttachmentCreationInformation();
        //    newAtt.ContentStream = new MemoryStream(buffer);
        //    newAtt.FileName = filename;   
        //    Attachment attachment = itm.AttachmentFiles.Add(newAtt);
        //    //Context.Load(attachment);
        //    //Context.ExecuteQuery();
        //    list.Update();
        //    Context.ExecuteQuery();


        //}


    }
}
