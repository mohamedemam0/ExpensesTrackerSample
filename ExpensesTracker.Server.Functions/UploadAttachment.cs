using ExpensesTracker.Server.Data.Interfaces;
using ExpensesTracker.Server.Functions.Services;
using ExpensesTracker.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ExpensesTracker.Server.Functions
{
    public class UploadAttachment
    {
        private readonly ILogger<UploadAttachment> _logger;
        private readonly IStorageService _storageService;
        private readonly IAttachmentRepo _attachmentRepo;

        public UploadAttachment(ILogger<UploadAttachment> log, IStorageService storageService, IAttachmentRepo attachmentRepo)
        {
            _logger = log;
            _storageService = storageService;
            _attachmentRepo = attachmentRepo;
        }

        [FunctionName("UploadAttachment")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("UploadAttachment has been triggered.");
            
            var userId = "userId";
            
            var file = req.Form.Files["File"];
            if (file == null)
                return new BadRequestObjectResult(new APIErrorResponse("File is required", null));

            string url = string.Empty;
            try
            {
                url = await _storageService.SaveFileAsync(file.OpenReadStream(), file.FileName);
            }
            catch (NotSupportedException)
            {

                return new BadRequestObjectResult(new APIErrorResponse("File is required", null));
            }

            await _attachmentRepo.AddAsync(new Data.Models.Attachment
            {
                Id = Guid.NewGuid().ToString(),
                UploadedByUserId = userId,
                UploadingDate = DateTime.UtcNow,
                Url = url
            });

            return new OkObjectResult(new APISuccessResponse<string>($"Attachment has been uploaded at: {url}"));
        }
    }
}

