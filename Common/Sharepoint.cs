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

        public static int SaveNewAnswer(string selectedFlowType , string Title)
        {
            int AnswerRecordID = 0;
            using (ClientContext ctx = new ClientContext(_serverURL))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);


                List oList = ctx.Web.Lists.GetByTitle("Submitted Data");

                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                ListItem oListItem = oList.AddItem(itemCreateInfo);
                oListItem["Title"] = Title;
                oListItem["Type_x0020_of_x0020_Submition"] = selectedFlowType;
                oListItem["Source"] = "Bot";


                oListItem.Update();

                ctx.ExecuteQuery();
                AnswerRecordID = oListItem.Id;
            }
            return AnswerRecordID;
        }


        public static void UpdateAnswer(int AnswerRecordID, string selectedFlowType , string Desc , string pdfPath , string Usertype , string SubmittedBy)
        {

            using (ClientContext ctx = new ClientContext(_serverURL))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);


                List oList = ctx.Web.Lists.GetByTitle("Submitted Data");

                ListItem oListItem = oList.GetItemById(AnswerRecordID);

                if(Desc != "")
                    oListItem["Description"] = Desc;

                oListItem["Anonymous"] = Usertype;
                oListItem["Submitted_x0020_By"] = SubmittedBy;




                //if (pdfPath != string.Empty)
                //{

                //  //  WebClient client = new WebClient();
                // //   Stream stream = client.OpenRead("https://teams.microsoft.com/_#/docx/viewer/recent/https%3A~2F~2Fm365x892385.sharepoint.com~2Fsites~2Fpwa~2FJCB%2520demo~2FShared%2520Documents~2FDocument.docx");
                // //  
                // //   StreamReader reader = new StreamReader(stream);
                    
                // //   String content = reader.ReadToEnd();

                //    var textFromFile = (new WebClient()).DownloadString("https://teams.microsoft.com/_#/docx/viewer/recent/https%3A~2F~2Fm365x892385.sharepoint.com~2Fsites~2Fpwa~2FJCB%2520demo~2FShared%2520Documents~2FDocument.docx");


                //    //   Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, "/Lists/Submitted Data/Attachments/" + AnswerRecordID + "/" + "Document.docx", stream, true);

                //    //  FileStream fs = new FileStream(pdfPath, FileMode.Open);

                //    //    // using (FileStream fs = new FileStream(pdfPath, FileMode.Open))
                //    //    // {
                //    AttachmentCreationInformation attInfo = new AttachmentCreationInformation();
                //        //attInfo.FileName = Path.GetFileName( .Name;
                //    //    attInfo.ContentStream = textFromFile;
                //    oListItem.AttachmentFiles.Add(attInfo);
                //    //   // oListItem.Update();
                //    //   // ctx.ExecuteQuery();
                //    //    //// }

                //    //    ////  if (pdfPath.IndexOf("\\") > 0)
                //    //    ////      pdfPath = pdfPath.Replace("\\" , @"\");
                //    //    ////   System.IO.StreamReader file =new System.IO.StreamReader(@"C:\Alaa\New Text Document.txt");
                //    //    ////                    byte[] bytes = System.IO.File.ReadAllBytes(pdfPath);
                //    //    ////MemoryStream mStream = new MemoryStream(bytes);
                //    //    ////AttachmentCreationInformation aci = new AttachmentCreationInformation();
                //    //    ////aci.ContentStream = mStream;
                //    //    ////aci.FileName = Path.GetFileName(pdfPath);
                //    //    ////Attachment attachment = oListItem.AttachmentFiles.Add(aci);
                //}

                    oListItem.Update();
                ctx.ExecuteQuery();
            }

        }


        public static void addAttachmentToListItem(int itemID, string filePath)
        {
            using (ClientContext Context = new ClientContext(_serverURL))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                Context.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);
                var list = Context.Web.Lists.GetByTitle("Submitted Data");
                Context.Load(list);
                Context.ExecuteQuery();

                ListItem item = list.GetItemById(itemID);
                Context.Load(item);
                Context.ExecuteQuery();
                if (item != null)
                {
                    FileStream fileStream = new FileStream(filePath, FileMode.Open);
                    AttachmentCreationInformation attInfo = new AttachmentCreationInformation();
                    attInfo.ContentStream = fileStream;
                    attInfo.FileName = fileStream.Name;
                    Attachment attachment = item.AttachmentFiles.Add(attInfo);
                    Context.Load(attachment);
                    Context.ExecuteQuery();
                    fileStream.Close();
                }
            }
        }

        ////public static void UploadAttachments(int AnswerRecordID, string pdfPath)
        ////{





        ////    String fileToUpload = @"C:\Alaa\New Text Document.txt";
        ////    // WORKS 
        ////    ClientContext context = new ClientContext(_serverURL);
        ////    //WORKS 
        ////    //ClientContext context = new ClientContext("http://ws.chi.com"); 
        ////    SecureString passWord = new SecureString();
        ////    foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
        ////    context.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);

        ////    Web currentWeb = context.Web;
        ////    context.Load(currentWeb);
        ////    context.ExecuteQuery();
        ////    using (FileStream fs = new FileStream(fileToUpload, FileMode.Open))
        ////    {
        ////        Microsoft.SharePoint.Client.File.SaveBinaryDirect(context, "/DemoDocs/New Text Document.txt", fs, true);
        ////    }

        ////    //using (ClientContext ctx = new ClientContext(_serverURL))
        ////    //{
        ////    //    SecureString passWord = new SecureString();
        ////    //    foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
        ////    //    ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);

        ////    //    using (FileStream fs = new FileStream(@"C:\Alaa\New Text Document.txt", FileMode.Open))
        ////    //    {
        ////    //        Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, "/Lists/Submitted Data/Attachments/" + AnswerRecordID + "/" + "New Text Document.txt", fs, true);
        ////    //    }

        ////    //    //using (FileStream strm = new FileInfo(@"C:\Alaa\New Text Document.txt").Open(FileMode.Open))
        ////    //    //{
        ////    //    //    Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, "/Lists/Submitted Data/Attachments/" + AnswerRecordID + "/" + "New Text Document.txt", strm, true);
        ////    //    //}

        ////    //    // List oList = ctx.Web.Lists.GetByTitle("Submitted Data");
        ////    //    // FileStream oFileStream = new FileStream(@"C:\Alaa\New Text Document.txt", FileMode.Open);
        ////    //    // string attachmentpath = "/Lists/Submitted Data/Attachments/"+ AnswerRecordID + "/New Text Document.txt";
        ////    //    //// ctx.Load(oList);
        ////    //    // //ctx.ExecuteQuery();
        ////    //    // Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, attachmentpath, oFileStream, true);
        ////    //}
        ////}
    }
}
