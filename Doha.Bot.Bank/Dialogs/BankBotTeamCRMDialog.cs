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
    public class BankBotTeamCRMDialog : IDialog<object>
    {
        private string userName;
        private string password;
        private string UserLoggedInName;
        private int currentQ = 0;
        private int NextQ = 0;
        private string InputListTitle = string.Empty;
        private string InputDesc = string.Empty;
        private string InputAttachmentPath = string.Empty;
        private string InputQuestionType = string.Empty;
        private bool InputUsertype = true;
        private string InputSubmittedBy = string.Empty;


        private int NextQYes = 0;
        private int NextQNo = 0;

        //   private int AnswerRecordID = -1;
        private DateTime msgReceivedDate;
        protected int count = 1;
        string InputSelectedOption = "";


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


            string UserLoggedInName = userName; //Common.CRM.checkAuthorizedUser(userName, password);

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
                PromptDialog.Text(
                    context: context,
                    resume: ResomeLoadDesc,
                    prompt: "What is the Idea Title?"
                    );
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



        public virtual async Task ResomeLoadDesc(IDialogContext context, IAwaitable<string> answer)
        {
            string response = await answer;
            InputListTitle = response;


            PromptDialog.Text(
                          context: context,
                          resume: ResomeLoadAttachments,
                          prompt: "What is the description?"
                          );
        }



        public virtual async Task ResomeLoadAttachments(IDialogContext context, IAwaitable<string> answer)
        {
            string response = await answer;
            InputDesc = response;

            PromptDialog.Confirm(
                context: context,
                resume: ResumeAfterConfirmationAttachment,
                prompt: "Do you want to Upload Attachment ?"
                );
        }

        private async Task ResumeAfterConfirmationAttachment(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmation = await result;

            if (confirmation == true)
            {
                NextQ = NextQYes;
                PromptDialog.Text(
                    context: context,
                    resume: ResomeLoadMoreAttachments,
                    prompt: "Please add the file path");
            }
            else
            {
                ResumeLoadUserInfo(context);
            }
        }

        private void ResumeLoadUserInfo(IDialogContext context)
        {
           

            PromptDialog.Confirm(
                context: context,
                resume: ResumeAfterConfirmationUserInfo,
                prompt: "Do you want to submit the idea as anonymous ?"
                );
        }

        public virtual async Task ResomeLoadMoreAttachments(IDialogContext context, IAwaitable<string> answer)
        {
            string response = await answer;
            InputAttachmentPath = InputAttachmentPath + response + ",";

            PromptDialog.Confirm(
                context: context,
                resume: ResumeAfterConfirmationAttachment,
                prompt: "Do you want to Upload more Attachment ?"
                );
        } 

        private async Task ResumeAfterConfirmationUserInfo(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmation = await result;
           // InputAttachmentPath = InputAttachmentPath + ",";

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

            Common.CRM.SaveNewAnswer(InputSelectedOption, InputListTitle, InputDesc, InputUsertype, InputSubmittedBy, InputAttachmentPath);
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