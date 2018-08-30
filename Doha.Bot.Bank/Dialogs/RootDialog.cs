using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net.Http;
using System.Linq;

namespace Doha.Bot.Bank.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
            public Task StartAsync(IDialogContext context)
            {
                context.Wait(MessageReceivedAsync);

                return Task.CompletedTask;
            }

            private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
            {


                var message = await result as Activity;

                //if (message.Attachments != null && message.Attachments.Any())
                //{
                //    var attachment = message.Attachments[0];
                //    using (HttpClient httpClient = new HttpClient())
                //    {
                //        // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.

                //        var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);
                //        var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;
                //        string filename = attachment.Name;
                //        string dir = AppDomain.CurrentDomain.BaseDirectory; // System.IO.Directory.GetCurrentDirectory();

                //        string file = dir + "Uploads";

                //        if (!Directory.Exists(file))
                //        {
                //            DirectoryInfo di = Directory.CreateDirectory(file);
                //            //return;
                //        }

                //        // Try to create the directory.

                //        string file1 = dir + "Uploads" + "\\" + filename;

                //        FileStream fs = new FileStream(file1, FileMode.Create, FileAccess.Write, FileShare.None);

                //        await responseMessage.Content.CopyToAsync(fs).ContinueWith(
                //            (copyTask) =>
                //            {
                //                fs.Close();

                //            });

                //        string StorageConnectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
                //        //string SourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
                //        string destContainer = ConfigurationManager.AppSettings["destContainer"];

                //        CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(StorageConnectionString);
                //        Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                //        CloudBlobContainer blobContainer = cloudBlobClient.GetContainerReference(destContainer);
                //        blobContainer.CreateIfNotExists();
                //        string key = Path.GetFileName(file1);
                //        CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(key);
                //        using (var fis = System.IO.File.Open(file1, FileMode.Open, FileAccess.Read, FileShare.None))
                //        {
                //            blockBlob.UploadFromStream(fis);
                //        }


                //        await context.PostAsync($"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.");
                //    }
                //}
                //else
                //{
                    await context.PostAsync("Hi there! I'm a bot created to show you how I can receive message attachments, but no attachment was sent to me. Please, try again sending a new message including an attachment.");
                //}

                context.Wait(this.MessageReceivedAsync);

            }
        }
}