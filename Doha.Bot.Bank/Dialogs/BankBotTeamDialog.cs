using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Doha.Bot.Bank.FormFlow;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Doha.Bot.Bank.Dialogs
{
    [Serializable]
    public class BankBotTeamDialog : IDialog<object>
    {
        private string userName;
        private string password;
        private string UserLoggedInName;

        private DateTime msgReceivedDate;
        protected int count = 1;



        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            StringBuilder response = new StringBuilder();
            if (context.UserData.TryGetValue<string>("UserLoggedInName", out UserLoggedInName))
            {
                //if (this.msgReceivedDate.ToString("tt") == "AM")
                //{
                //    response.Append($"Good morning team, {UserLoggedInName}.. :)");
                //}
                //else
                //{
                //    response.Append($"Hey {UserLoggedInName}.. :)");
                //}
                //await context.PostAsync(response.ToString());
                //context.Wait(MessageReceivedAsync);
                ////ShowOptions(context);
                PromptDialog.Choice(context, this.AfterSelectOption, new string[] { "Idea", "Suggestion","Complaint","Incident" }, "Hello, How can I help you today?Do you want to submit:");

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

            if ((await result) == "Idea")
            {
                await context.PostAsync("Great, back to the original conversation!");
                context.Done(String.Empty); //Finish this dialog
            }
            else if ((await result) == "Suggestion")
            {
                await context.PostAsync("Great, back to the original conversation!");
                context.Done(String.Empty); //Finish this dialog
            }
            else if ((await result) == "Complaint")
            {
                await context.PostAsync("Great, back to the original conversation!");
                context.Done(String.Empty); //Finish this dialog
            }
            else if ((await result) == "Incident")
            {
                await context.PostAsync("Great, back to the original conversation!");
                context.Done(String.Empty); //Finish this dialog
            }
        }

       

    }
}