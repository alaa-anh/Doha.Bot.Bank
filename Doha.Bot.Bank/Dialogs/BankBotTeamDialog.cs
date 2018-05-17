﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.SharePoint.Client;

using System.IO;
using System.Security.Cryptography;

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
                PromptDialog.Choice(context, this.AfterSelectOption, new string[] { "Idea", "Suggestion", "Complaint", "Incident" }, "Hello, How can I help you today?Do you want to submit:");
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
                    Common.Sharepoint.UpdateAnswer(AnswerRecordID, selectedOption, response , "" , "" , "");
                else if (InputQuestionType == "Attachment")
                    Common.Sharepoint.UpdateAnswer(AnswerRecordID, selectedOption,"", response , "" , "");
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
                Common.Sharepoint.UpdateAnswer(AnswerRecordID, selectedOption, "", "", "Yes", "Anonymous");
            }
            else
            {


                if (context.UserData.TryGetValue<string>("UserName", out userName))
                {
                   string EncrebtedUsernme= Encrypt(userName);
                    Common.Sharepoint.UpdateAnswer(AnswerRecordID, selectedOption, "", "", "No", EncrebtedUsernme);
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

        //public async Task ProcessSelectedOptionAsync(IDialogContext context, IAwaitable<string> argument)
        //{

        //    var message = await argument;

        //    var replyMessage = context.MakeMessage();

        //    Microsoft.Bot.Connector.Attachment attachment = null;

        //    switch (message)
        //    {
        //        case "1":
        //            attachment = GetLocalAttachment();
        //            replyMessage.Text = "Attach Image from Local Machine";
        //            break;
        //            //    case "2":
        //            //        attachment = GetInternetAttachment();
        //            //        replyMessage.Text = "Attach Image from Internet";
        //            //        break;
        //            //    case "3":
        //            //        attachment = GetinternetFileAttachment();
        //            //        replyMessage.Text = "Click Link for navigate PDF internet location";
        //            //        break;
        //            //    case "4":
        //            //        attachment = GetLocalFileAttachment();
        //            //        replyMessage.Text = "Click Link for navigate PDF local location";
        //            //        break;
        //            //    case "5":
        //            //        attachment = GetinternetVideoAttachment();
        //            //        replyMessage.Text = "Click on play button ";
        //            //        break;
        //            //    case "6":
        //            //        attachment = GetinternetYoutubeAttachment();
        //            //        replyMessage.Text = "Showing video from Youtube ";
        //            //        break;
        //            //    case "7":
        //            //        attachment = GetinternetMP3Attachment();
        //            //        replyMessage.Text = "Showing MP3 from internet ";
        //            //        break;

        //            }
        //            replyMessage.Attachments = new List<Microsoft.Bot.Connector.Attachment> { attachment };

        //    await context.PostAsync(replyMessage);

        //    await this.DisplayOptionsAsync(context);
        //}



        // public async Task DisplayOptionsAsync(IDialogContext context)
        // {
        //     PromptDialog.Choice<string>(
        //context,
        //this.ProcessSelectedOptionAsync,
        //this.options.Keys,
        //"What Demo / Sample option would you like to see?",
        //"Please select Valid option 1 to 6",
        //6,
        //PromptStyle.PerLine,
        //this.options.Values);
        // }

        //private static Microsoft.Bot.Connector.Attachment GetLocalAttachment()
        //{
        //    var imagePath = HttpContext.Current.Server.MapPath("~/images/demo.gif");

        //    var imageData = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath));

        //    return new Microsoft.Bot.Connector.Attachment
        //    {
        //        Name = "demo.gif",
        //        ContentType = "image/gif",
        //        ContentUrl = $"data:image/gif;base64,{imageData}"
        //    };
        //}
    }
}