using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.SharePoint.Client;
using Microsoft.Azure;
using System.IO;
using System.Security.Cryptography;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Linq;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Doha.Bot.Bank.Dialogs
{
    [Serializable]
    public class BankBotTeamDialog : IDialog<object>
    {
        private string userName;
        private string password;
        private string UserLoggedInName;
        private int currentQ = 0;
        private int NextQ = 0;
        private string InputTitle = string.Empty;
        private string InputDesc = string.Empty;
        private string InputAttachmentPath = string.Empty;
        private string InputQuestionType = string.Empty;
        private int NextQYes = 0;
        private int NextQNo = 0;

        private int AnswerRecordID = -1;
        private DateTime msgReceivedDate;
        protected int count = 1;
        string selectedOption = "";


        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            string response;
            if (context.UserData.TryGetValue<string>("UserLoggedInName", out UserLoggedInName))
            {
                if (message.Text.Equals("hi"))
                {
                    PromptDialog.Choice(context, this.AfterSelectOption, new string[] { "Idea", "Suggestion", "Complaint", "Incident" , "Exit" }, "Hello, " + UserLoggedInName + " How can I help you today? Do you want to submit:");
                } 
                else if(message.Text.Equals("bye"))
                {

                    if (this.msgReceivedDate.ToString("tt") == "AM")
                    {
                        response = $"Good bye, {userName}.. Have a nice day. :)";
                    }
                    else
                    {
                        response = $"b'bye {userName}, Take care.";
                    }
                    context.UserData.Clear();
                    await context.PostAsync(response);
                    context.Wait(MessageReceivedAsync);
           }
            }
            else
            {
                PromptDialog.Text(
                    context: context,
                    resume: ResumeGetPassword,
                    prompt: "Dear , May I know your user name?",
                    retry: "Sorry, I didn't understand that. Please try again."
                );
            }

        }

        public virtual async Task ResumeGetPassword(IDialogContext context, IAwaitable<string> UserEmail)
        {
            string response = await UserEmail;
            userName = response; ;

            PromptDialog.Text(
                context: context,
                resume: SignUpComplete,
                prompt: "Please share your password",
                retry: "Sorry, I didn't understand that. Please try again."
            );
        }
        public virtual async Task SignUpComplete(IDialogContext context, IAwaitable<string> pass)
        {
            string response = await pass;
            password = response;


            string UserLoggedInName = Common.Sharepoint.checkAuthorizedUser(userName, password);

            if (UserLoggedInName != string.Empty)
            {
                context.UserData.SetValue("UserName", userName);
                context.UserData.SetValue("Password", password);
                context.UserData.SetValue("UserLoggedInName", UserLoggedInName);
                var message = $"You are currently Logged In. Please Enjoy Using our App. **{UserLoggedInName}**.";
                await context.PostAsync(message);
                PromptDialog.Choice(context, this.AfterSelectOption, new string[] { "Idea", "Suggestion", "Complaint", "Incident" , "Exit" }, "Hello, How can I help you today?Do you want to submit:");
            }
            else
            {
                PromptDialog.Confirm(context, ResumeAfterConfirmation, "The User Don't have permission , do you want to try another cridentials?");

            }
        }

        private async Task ResumeAfterConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmation = await result;
            if (confirmation == true)
            {
                PromptDialog.Text(
                    context: context,
                    resume: ResumeGetPassword,
                    prompt: "Dear , May I know your user name?",
                    retry: "Sorry, I didn't understand that. Please try again."
                );
            }
            else
            {
                string response = string.Empty;

                if (this.msgReceivedDate.ToString("tt") == "AM")
                {
                    response = $"Good bye, {userName}.. Have a nice day. :)";
                }
                else
                {
                    response = $"b'bye {userName}, Take care.";
                }

                context.UserData.Clear();
                await context.PostAsync(response);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterSelectOption(IDialogContext context, IAwaitable<string> result)
        {
            string response;
            if ((await result) == "Idea")
                selectedOption = "Idea";
            else if ((await result) == "Suggestion")
                selectedOption = "Suggestion";
            else if ((await result) == "Complaint")
                selectedOption = "Complaint";
            else if ((await result) == "Incident")
                selectedOption = "Incident";
            else if ((await result) == "Exit")
                selectedOption = "Exit";

            if (selectedOption != "Exit")
            {
                ListItemCollection collListItem = Common.Sharepoint.GetSelectedTypeQuestions(selectedOption);
                if (collListItem.Count > 0)
                {
                    var Question1 = collListItem[0]["Title"].ToString();
                    string strNextQuestionID = string.Empty;
                    if (collListItem[0]["NextQuestionID"] != null)
                    {
                        if (collListItem[0]["NextQuestionID"].ToString() != string.Empty)
                        {
                            if (collListItem[0]["NextQuestionID"].ToString().Contains(","))
                            {
                                string[] strsplit = collListItem[0]["NextQuestionID"].ToString().Split(',');
                                NextQYes = int.Parse(strsplit[0]);
                                NextQNo = int.Parse(strsplit[1]);
                            }
                            else
                            {
                                strNextQuestionID = collListItem[0]["NextQuestionID"].ToString();
                                NextQ = int.Parse(strNextQuestionID);
                            }
                        }
                    }

                    if (collListItem[0]["Question_x0020_Type"].ToString() == "Text")//Options
                    {

                        PromptDialog.Text(
                            context: context,
                            resume: ResomeLoadAnswers,
                            prompt: Question1
                            );
                    }

                }
            }
            else
            {
                if (this.msgReceivedDate.ToString("tt") == "AM")
                {
                    response = $"Good bye, {userName}.. Have a nice day. :)";
                }
                else
                {
                    response = $"b'bye {userName}, Take care.";
                }
                context.UserData.Clear();
                await context.PostAsync(response);
                context.Wait(MessageReceivedAsync);
            }
        }

        public virtual async Task ResomeLoadAnswers(IDialogContext context, IAwaitable<string> answer)
        {
            string response = await answer;
            InputTitle = response;

            if (currentQ == 0)
            {
                //save
                AnswerRecordID = Common.Sharepoint.SaveNewAnswer(selectedOption, response);
            }
            else
            {
                //update
                if (InputQuestionType == "Text")
                    Common.Sharepoint.UpdateAnswer(AnswerRecordID, selectedOption, response, "", "", "");
                else if (InputQuestionType == "Attachment")
                {
                    string filename = UploadFiles(response);
                    await context.PostAsync(filename);
                    //if (filename != "")
                    //      Common.Sharepoint.addAttachmentToListItem(AnswerRecordID, filename);

                    //Microsoft.Bot.Connector.Attachment attachment = new HeroCard
                    //{
                    //    Title = "Click to download Report",
                    //    Buttons = new List<CardAction>()
                    //        {
                    //            new CardAction()
                    //            {
                    //                Title = "Get Started",
                    //                Type = ActionTypes.OpenUrl,
                    //                Value = "C:\\Alaa\\New Text Document.txt"
                    //            }
                    //         }
                    //}.ToAttachment();

                    //var reply = context.MakeMessage();
                    //reply.Attachments.Add(attachment);
                    //await context.PostAsync(reply);

                    //StackTraceGathered(context);
                    //var replymes = context.MakeMessage();

                    //Microsoft.Bot.Connector.Attachment attachmentsend = null;
                    //attachmentsend = await GetUploadedAttachmentAsync(replymes.ServiceUrl, replymes.Conversation.Id);

                    //replymes.Attachments.Add(attachment);

                    //await context.PostAsync(replymes);

                }
            }

            currentQ = NextQ;
            ListItem question = Common.Sharepoint.GetQuestion(currentQ);


            if (question != null)
            {
                var Question1 = question["Title"].ToString();
                string strNextQuestionID = string.Empty;
                InputQuestionType = question["Question_x0020_Type"].ToString();
                if (question["NextQuestionID"] != null)
                {
                    if (question["NextQuestionID"].ToString().Contains(","))
                    {
                        string[] strsplit = question["NextQuestionID"].ToString().Split(',');
                        NextQYes = int.Parse(strsplit[0]);
                        NextQNo = int.Parse(strsplit[1]);
                    }
                    else
                    {
                        strNextQuestionID = question["NextQuestionID"].ToString();
                        NextQ = int.Parse(strNextQuestionID);
                    }
                }
                if (question["Question_x0020_Type"].ToString() == "Text")//Options
                {
                    PromptDialog.Text(
                     context: context,
                     resume: ResomeLoadAnswers,
                     prompt: Question1);
                }
                else if (question["Question_x0020_Type"].ToString() == "Attachment")//Options
                {

                    PromptDialog.Confirm(context, ResumeAfterConfirmationAttachment, Question1);

                }

                else if (question["Question_x0020_Type"].ToString() == "UserInfo")//Options
                {

                    PromptDialog.Confirm(context, ResumeAfterConfirmationUserInfo, Question1);

                }
            }
        }


        public static string UploadFiles(string Attchpath)
        {
            string StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            string SourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
            string destContainer = ConfigurationManager.AppSettings["destContainer"];

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = cloudBlobClient.GetContainerReference(destContainer);
            blobContainer.CreateIfNotExists();
          //  string[] fileEntries = Directory.GetFiles(SourceFolder);
           // foreach(string filepath in fileEntries)
           // {
                //string key = Path.GetFileName(Attchpath);
                //CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(key);
                //using (var fs = System.IO.File.Open(Attchpath, FileMode.Open, FileAccess.Read, FileShare.None))
                //{
                //    blockBlob.UploadFromStream(fs);
                //}
           // }

            // int iUploadedCnt = 0;
            string fileName = "";
           // //string sourcePath = @"C:\Users\Bijin\Desktop\Images\";
           // //string targetPath = System.Web.Hosting.HostingEnvironment.MapPath("~/UploadedFiles/");

           // //string targetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AttachmentFiles");

           //string targetPath = HttpContext.Current.Server.MapPath("~/AttachmentFiles/");

           // ////--this is the local path we want to take to upload(try with your local path data)
           // //// string Attchpath = ("C:\\Users\\Bijin\\Desktop\\Images\\delete.png,C:\\Users\\Bijin\\Desktop\\Images\\edit.jpg,C:\\Users\\Bijin\\Desktop\\Images\\Refernce links.txt");
           // //// string Attchpath = ("C:\\Users\\Bijin\\Desktop\\Images\\delete.pn");

           // ////ProcessedFiles = Server.MapPath(@"~\godurian\sth100\ProcessedFiles");
           // ////string ProcessedFiles = Directory.GetFiles("\\Archive\\*.zip"); //Server.MapPath(@"~\ProcessedFiles");

           // //string[] AttchList = Attchpath.Split(',');

           // // foreach (string file in AttchList)
           // // {
           // //  string sourceFile = System.IO.Path.Combine(Attchpath, fileName);

           // fileName = "ss.doc";

           //     //fileName = System.IO.Path.GetFileName(Attchpath);
           //     string destFile = System.IO.Path.Combine(targetPath, fileName);

           // //destFile = System.IO.Path.Combine(targetPath, fileName);
           //     System.IO.File.Copy(Attchpath, destFile, true);
           // //iUploadedCnt = iUploadedCnt + 1;

           // // }
           // //// RETURN A MESSAGE.
           // ////if (iUploadedCnt > 0)
           // ////{
           // ////    return iUploadedCnt + " Files Uploaded Successfully";
           // ////}
           // ////else
           // ////{
           // ////    return "Upload Failed";
           // ////}


            return fileName;
        }

        //private static async Task<Microsoft.Bot.Connector.Attachment> GetUploadedAttachmentAsync(string serviceUrl, string conversationId)
        //{
        //    var imagePath = System.Web.HttpContext.Current.Server.MapPath("~/AttachmentFiles/small-image.png");

        //    using (var connector = new ConnectorClient(new Uri(serviceUrl)))
        //    {
        //        var attachments = new Attachments(connector);
        //        var response = await attachments.Client.Conversations.UploadAttachmentAsync(
        //            conversationId,
        //            new AttachmentData
        //            {
        //                Name = "Results.csv",
        //                OriginalBase64 = System.IO.File.ReadAllBytes(imagePath),
        //                Type = "text/csv"
        //            });

        //        var attachmentUri = attachments.GetAttachmentUri(response.Id);

        //        return new Microsoft.Bot.Connector.Attachment
        //        {
        //            Name = "Results.csv",
        //            ContentType = "text/csv",
        //            ContentUrl = attachmentUri
        //        };
        //    }
        //}
        //public virtual async Task StackTraceGathered(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    var message = await argument;
        //  //  FileName = message.Attachments[0].Name;
        //    HttpPostedFileBase file = (HttpPostedFileBase)message.Attachments[0].Content;

        //    string filePath = HttpContext.Current.Server.MapPath("~/AttachmentFiles/" + file.FileName);
        //    file.SaveAs(filePath);

        //    if (message.Attachments != null && message.Attachments.Any())
        //    {
        //        var attachment = message.Attachments.First();
        //        using (HttpClient httpClient = new HttpClient())
        //        {
        //            // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
        //            if ((message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) || message.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase))
        //                && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
        //            {
        //                var token = await new MicrosoftAppCredentials().GetTokenAsync();
        //                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //            }

        //            var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

        //            var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

        //            await context.PostAsync($"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.");
        //        }
        //    }
        //    else
        //    {
        //        await context.PostAsync("Hi there! I'm a bot created to show you how I can receive message attachments, but no attachment was sent to me. Please, try again sending a new message including an attachment.");
        //    }
        //    PromptDialog.Text(context, ProblemStartDuration, "How long has this been an issue? (Provide answer in days, if issue has been occurring for less than one day put 1).");

        //    context.Wait(this.StackTraceGathered);
        //}

        private async Task ResumeAfterConfirmationAttachment(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmation = await result;
           
            if (confirmation == true)
            {
                NextQ = NextQYes;
                PromptDialog.Text(
                    context: context,
                    resume: ResomeLoadAnswers,
                    prompt: "Please add the file path");
            }
            else
            {
                currentQ = NextQNo;
                ResomeLoadAnswers2(context);            
            }
        }


        //public virtual async Task ResumeAfterConfirmationAttachmentName(IDialogContext context, IAwaitable<string> answer)
        //{

        //    var filename = await answer;

        //    InputAttachmentPath = filename;

        //    PromptDialog.Text(
        //            context: context,
        //            resume: ResomeLoadAnswers,
        //            prompt: "Please add the file name");
        //}

        private void ResomeLoadAnswers2(IDialogContext context)
        {
            string response = string.Empty;
            ListItem question = Common.Sharepoint.GetQuestion(currentQ);
            if (question != null)
            {
                var Question1 = question["Title"].ToString();
                string strNextQuestionID = string.Empty;
                InputQuestionType = question["Question_x0020_Type"].ToString();
                if (question["NextQuestionID"] != null)
                {
                    if (question["NextQuestionID"].ToString().Contains(","))
                    {
                        string[] strsplit = question["NextQuestionID"].ToString().Split(',');
                        NextQYes = int.Parse(strsplit[0]);
                        NextQNo = int.Parse(strsplit[1]);
                    }
                    else
                    {
                        strNextQuestionID = question["NextQuestionID"].ToString();
                        NextQ = int.Parse(strNextQuestionID);
                    }
                }
                if (question["Question_x0020_Type"].ToString() == "Text")//Options
                {
                    PromptDialog.Text(
                     context: context,
                     resume: ResomeLoadAnswers,
                     prompt: Question1);
                }
                else if (question["Question_x0020_Type"].ToString() == "Attachment")//Options
                {

                    PromptDialog.Confirm(context, ResumeAfterConfirmationAttachment, Question1);

                }

                else if (question["Question_x0020_Type"].ToString() == "UserInfo")//Options
                {

                    PromptDialog.Confirm(context, ResumeAfterConfirmationUserInfo, Question1);

                }
            }

        }

        private async Task ResumeAfterConfirmationUserInfo(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmation = await result;
            if (confirmation == true)
            {
                if (context.UserData.TryGetValue<string>("UserName", out userName))
                {
                    string EncrebtedUsernme = Encrypt(userName);
                    Common.Sharepoint.UpdateAnswer(AnswerRecordID, selectedOption, "", "", "Yes", EncrebtedUsernme);
                }
            }
            else
            {
                if (context.UserData.TryGetValue<string>("UserName", out userName))
                {
                    Common.Sharepoint.UpdateAnswer(AnswerRecordID, selectedOption, "", "", "No", userName);
                }
            }

            await context.PostAsync("Your "+selectedOption+" has Been Submitted");

        }

        protected string Encrypt(string p_sClearText)
        {
            string EncryptionKey = "20180517";
            byte[] clearBytes = Encoding.Unicode.GetBytes(p_sClearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    p_sClearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return p_sClearText;
        }

        protected string Decrypt(string p_sCipherText)
        {
            string EncryptionKey = "20180517";
            byte[] cipherBytes = Convert.FromBase64String(p_sCipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    p_sCipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return p_sCipherText;
        }

        
    }
}