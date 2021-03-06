﻿using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.SharePoint.Client;
using System.IO;
using System.Net.Http;
using System.Security;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text;
using System.Security.Cryptography;

namespace Doha.Bot.Bank.Dialogs
{
    [Serializable]
    public class BankBotTeamDialog : IDialog<object>
    {

        private static string _userName;
        private static string _userPassword;
        private static string UserLoggedInName;


        private static string _serverURL = ConfigurationManager.AppSettings["ServerURL"];

        private static string _userNameAdmin = ConfigurationManager.AppSettings["DomainAdmin"];
        private static string _userPasswordAdmin = ConfigurationManager.AppSettings["DomainAdminPassword"];

        private string userName;
        private string password;
        // private string UserLoggedInName;
        private int currentQ = 0;
        private int NextQ = 0;
        private string InputListTitle = string.Empty;
        private string InputTit = string.Empty;
        private string InputQuestion = string.Empty;

        private string InputDesc = string.Empty;
        private string InputAttachmentPath = string.Empty;
        private string InputQuestionType = string.Empty;
        private bool InputUsertype = true;
        private string InputSubmittedBy = string.Empty;


        private int NextQYes = 0;
        private int NextQNo = 0;

        private int AnswerRecordID = -1;
        private DateTime msgReceivedDate;
        protected int count = 1;
        string InputSelectedOption = "";


        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {


            var message = await result as Activity;

            if (message.Attachments != null)
            {
                var attachment = message.Attachments[0];
                using (HttpClient httpClient = new HttpClient())
                {
                  //  Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.

                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;
                    string filename = attachment.Name;
                    string dir = AppDomain.CurrentDomain.BaseDirectory; // System.IO.Directory.GetCurrentDirectory();

                    string file = dir + "Uploads";

                    if (!Directory.Exists(file))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(file);
                        //return;
                    }

                    // Try to create the directory.

                    string file1 = dir + "Uploads" + "\\" + filename;

                    FileStream fs = new FileStream(file1, FileMode.Create, FileAccess.Write, FileShare.None);
                    //  FileStream fs = new FileStream(file1, FileMode.Open);
                    //  SaveAttchments(fs, filename);
                    await responseMessage.Content.CopyToAsync(fs).ContinueWith(
                        (copyTask) =>
                        {
                            fs.Close();

                        });


                    string StorageConnectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
                    //string SourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
                    string destContainer = ConfigurationManager.AppSettings["destContainer"];

                    CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(StorageConnectionString);
                    Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                    CloudBlobContainer blobContainer = cloudBlobClient.GetContainerReference(destContainer);
                    blobContainer.CreateIfNotExists();
                    string key = Path.GetFileName(file1);
                    CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(key);
                    using (var fis = System.IO.File.Open(file1, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        blockBlob.UploadFromStream(fis);
                    }


                    await context.PostAsync($"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.");
                    await context.PostAsync($"Attachment of {attachment.ContentType} type and size of bytes received.");
                }
            }
            else
            {
                await context.PostAsync("Hi there! I'm a bot created to show you how I can receive message attachments, but no attachment was sent to me. Please, try again sending a new message including an attachment.");
            }

            context.Wait(this.MessageReceivedAsync);

        }

        public static void SaveAttchments(FileStream fsAttachment, string NewTitle)
        {
            using (ClientContext ctx = new ClientContext(_serverURL))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);
                List oList = ctx.Web.Lists.GetByTitle("BotTestAttachments");
                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                ListItem oListItem = oList.AddItem(itemCreateInfo);
                oListItem["Title"] = NewTitle;
                oListItem.Update();
                ctx.ExecuteQuery();
                ListItem item = oList.GetItemById(oListItem.Id);
                AttachmentCreationInformation attInfo = new AttachmentCreationInformation();
                attInfo.FileName = fsAttachment.Name;
                attInfo.ContentStream = fsAttachment;
                item.AttachmentFiles.Add(attInfo);
                item.Update();
                ctx.ExecuteQuery();
            }
        }


        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            string response;
            if (context.UserData.TryGetValue<string>("UserLoggedInName", out UserLoggedInName))
            {
                if (message.Text.Equals("hi"))
                {
                    PromptDialog.Choice(context, this.AfterSelectOption, new string[] { "Idea", "Suggestion", "Complaint", "Incident", "Exit" }, "Hello, " + UserLoggedInName + " How can I help you today? Do you want to submit:");
                }
                else if (message.Text.Equals("bye"))
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
                PromptDialog.Choice(context, this.AfterSelectOption, new string[] { "Idea", "Suggestion", "Complaint", "Incident", "Exit" }, "Hello, How can I help you today?Do you want to submit:");
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
                InputSelectedOption = "Idea";
            else if ((await result) == "Suggestion")
                InputSelectedOption = "Suggestion";
            else if ((await result) == "Complaint")
                InputSelectedOption = "Complaint";
            else if ((await result) == "Incident")
                InputSelectedOption = "Incident";
            else if ((await result) == "Exit")
                InputSelectedOption = "Exit";

            if (InputSelectedOption != "Exit")
            {
                ListItemCollection collListItem = Common.Sharepoint.GetSelectedTypeQuestions(InputSelectedOption);
                if (collListItem.Count > 0)
                {
                    var Question1 = collListItem[0]["Title"].ToString();
                    InputQuestion = Question1;
                    string strNextQuestionID = string.Empty;
                    InputQuestionType = collListItem[0]["Question_x0020_Type"].ToString();
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
            if (InputQuestionType == "Text")
            {
                if (InputQuestion.Contains("Title?"))
                    InputListTitle = response;
                else if (InputQuestion.Contains("description?"))
                    InputDesc = response;
            }
            else if (InputQuestionType == "Attachment")
            {
                InputAttachmentPath = InputAttachmentPath + "," + response;
                UploadFiles(response, context);
                // //context.Wait(AttachmentReceivedAsync);
            }
            currentQ = NextQ;
            ListItem question = Common.Sharepoint.GetQuestion(currentQ);
            if (question != null)
            {
                var Question1 = question["Title"].ToString();
                string strNextQuestionID = string.Empty;
                InputQuestionType = question["Question_x0020_Type"].ToString();
                InputQuestion = Question1;
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

        //private async Task AttachmentReceivedAsync(IDialogContext context, IAwaitable<object> result)
        //{


        //    var message = await result as Activity;

        //    if (message.Attachments != null)
        //    {
        //        var attachment = message.Attachments[0];
        //        using (HttpClient httpClient = new HttpClient())
        //        {
        //            // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.

        //            var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

        //            var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;
        //            string filename = attachment.Name;
        //            string dir = AppDomain.CurrentDomain.BaseDirectory; // System.IO.Directory.GetCurrentDirectory();

        //            string file = dir + "Uploads";

        //            if (!Directory.Exists(file))
        //            {
        //                DirectoryInfo di = Directory.CreateDirectory(file);
        //                //return;
        //            }

        //            // Try to create the directory.

        //            string file1 = dir + "Uploads" + "\\" + filename;

        //            FileStream fs = new FileStream(file1, FileMode.Create, FileAccess.Write, FileShare.None);

        //            await responseMessage.Content.CopyToAsync(fs).ContinueWith(
        //                (copyTask) =>
        //                {
        //                    fs.Close();

        //                });

        //            string StorageConnectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
        //            //string SourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
        //            string destContainer = ConfigurationManager.AppSettings["destContainer"];

        //            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(StorageConnectionString);
        //            Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        //            CloudBlobContainer blobContainer = cloudBlobClient.GetContainerReference(destContainer);
        //            blobContainer.CreateIfNotExists();
        //            string key = Path.GetFileName(file1);
        //            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(key);
        //            using (var fis = System.IO.File.Open(file1, FileMode.Open, FileAccess.Read, FileShare.None))
        //            {
        //                blockBlob.UploadFromStream(fis);
        //            }


        //            await context.PostAsync($"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.");
        //        }
        //    }
        //    else
        //    {
        //        await context.PostAsync("Hi there! I'm a bot created to show you how I can receive message attachments, but no attachment was sent to me. Please, try again sending a new message including an attachment.");
        //    }

        //    context.Wait(this.MessageReceivedAsync);

        //}

        public static void UploadFiles(string Attchpath, IDialogContext context)
        {


            Attchpath = @"C:\Alaa\ReadMe.txt";
            string StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            string SourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
            string destContainer = ConfigurationManager.AppSettings["destContainer"];

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = cloudBlobClient.GetContainerReference(destContainer);
            blobContainer.CreateIfNotExists();
            string key = Path.GetFileName(Attchpath);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(key);
            using (var fs = System.IO.File.Open(Attchpath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                blockBlob.UploadFromStream(fs);
            }
        }

        private static async Task<Microsoft.Bot.Connector.Attachment> GetUploadedAttachmentAsync(string serviceUrl, string conversationId)
        {
            var imagePath = System.Web.HttpContext.Current.Server.MapPath(@"~\AttachmentFiles\small-image.png");

            using (var connector = new ConnectorClient(new Uri(serviceUrl)))
            {
                var attachments = new Attachments(connector);
                var response = await attachments.Client.Conversations.UploadAttachmentAsync(
                    conversationId,
                    new AttachmentData
                    {
                        Name = "small-image.png",
                        OriginalBase64 = System.IO.File.ReadAllBytes(imagePath),
                        Type = "text/csv"
                    });

                var attachmentUri = attachments.GetAttachmentUri(response.Id);

                return new Microsoft.Bot.Connector.Attachment
                {
                    Name = "small-image.png",
                    ContentType = "text/csv",
                    ContentUrl = attachmentUri
                };
            }
        }
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
                    InputUsertype = true;
                    InputSubmittedBy = EncrebtedUsernme;
                }
            }
            else
            {
                if (context.UserData.TryGetValue<string>("UserName", out userName))
                {
                    InputUsertype = false;
                    InputSubmittedBy = userName;
                }
            }

            Common.Sharepoint.SaveNewAnswer(InputSelectedOption, InputListTitle, InputTit, InputDesc, InputUsertype, InputSubmittedBy, InputAttachmentPath);
            await context.PostAsync("Your " + InputSelectedOption + " has Been Submitted");

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