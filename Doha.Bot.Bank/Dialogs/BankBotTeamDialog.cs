using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Linq;

namespace Doha.Bot.Bank.Dialogs
{
    [Serializable]
    public class BankBotTeamDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();
                using (HttpClient httpClient = new HttpClient())
                {
                    // Skype attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                   

                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                    await context.PostAsync($"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.");
                }
            }
            else
            {
                await context.PostAsync("Hi there! I'm a bot created to show you how I can receive message attachments, but no attachment was sent to me. Please, try again sending a new message including an attachment.");
            }

            context.Wait(this.MessageReceivedAsync);
        }

     


    }
}