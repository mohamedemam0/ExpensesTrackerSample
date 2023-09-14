using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExpensesTracker.Server.Data.Interfaces;
using ExpensesTracker.Server.Functions.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ExpensesTracker.Server.Functions
{
    public class DeleteUnusedAttachments
    {
        private readonly IStorageService _storageService;
        private readonly IAttachmentRepo _attachmentRepo;

        public DeleteUnusedAttachments(IStorageService storageService, IAttachmentRepo attachmentRepo)
        {
            _storageService = storageService;
            _attachmentRepo = attachmentRepo;
        }

        [FunctionName("DeleteUnusedAttachments")]
        public async Task Run([TimerTrigger("0 0 */6 * 1 *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Delete Unused Attachments triggered at: {DateTime.Now}");
            int deletedCount = 0;

            var attachments = await _attachmentRepo.GetUnsedAttachmentsAsync(12);
            if(attachments.Any())
            {
                log.LogInformation($"{attachments.Count()} attachments have been found to be deleted.");

                foreach (var attachment in attachments)
                {
                    var fileName = Path.GetFileName(attachment.Url);
                    try
                    {
                        await _storageService.DeleteFileAsync(fileName);
                        await _attachmentRepo.DeleteAsync(attachment.Id, attachment.UploadedByUserId);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Error in deleting the file: {fileName}", ex);
                    }
                }
                log.LogInformation($"{deletedCount}/{attachments.Count()} attachments have been deleted successfully!");
            }
            else
            {
                log.LogInformation($"No attachments to be deleted have benn found.");
            }

        }
    }
}
