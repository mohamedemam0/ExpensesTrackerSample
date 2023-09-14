using ExpensesTracker.Server.Data.Interfaces;
using ExpensesTracker.Server.Data.Models;
using ExpensesTracker.Server.Functions.Services;
using ExpensesTracker.Shared.DTOs;
using ExpensesTracker.Shared.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AKExpensesTracker.Server.Functions
{
    public class UpsertTransaction
    {
        private readonly ILogger<UpsertTransaction> _logger;
        private readonly IAttachmentRepo _attachmentsRepo;
        private readonly ITransactionRepo _transactionsRepo;
        private readonly IWalletsRepository _walletsRepo;
        private readonly IStorageService _storageService;
        private readonly IValidator<TransactionDto> _validator;

        public UpsertTransaction(ILogger<UpsertTransaction> log,
                                 IAttachmentRepo attachmentsRepo,
                                 ITransactionRepo transactionsRepo,
                                  IValidator<TransactionDto> validator,
                                 IWalletsRepository walletsRepo,
                                 IStorageService storageService)
        {
            _logger = log;
            _attachmentsRepo = attachmentsRepo;
            _transactionsRepo = transactionsRepo;
            _validator = validator;
            _walletsRepo = walletsRepo;
            _storageService = storageService;
        }

        [FunctionName("UpsertTransaction")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("UpsertTransaction function triggered");
            var userId = "userId";

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<TransactionDto>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            var isUpdate = !string.IsNullOrWhiteSpace(data.Id);

            var validationResult = _validator.Validate(data);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(new APIErrorResponse("Invalid data",
                                                                        validationResult
                                                                            .Errors
                                                                            .Select(e => e.ErrorMessage)));
            }

            if (isUpdate)
            {
                // TODO: Add the creation date to the TransactionDto
                var transaction = await _transactionsRepo.GetByIdAsync(data.Id, userId, 2023);
                if (transaction == null)
                    return new NotFoundResult(); // 404

                var wallet = await _walletsRepo.GetByIdAsync(transaction.WalletId, userId);
                if (wallet == null || wallet.Id != data.WalletId)
                    return new BadRequestObjectResult(new APIErrorResponse("Wallet not found"));

                // Detect and add new attachments
                IEnumerable<Attachment> newAttachments = Enumerable.Empty<Attachment>();
                if (data.Attachments != null && data.Attachments.Any())
                {
                    newAttachments = await _attachmentsRepo.GetByURLAsync(data.Attachments);
                    if (data.Attachments.Distinct().Count() != newAttachments.Count())
                        return new BadRequestObjectResult(new APIErrorResponse("Invalid attachments"));
                }

                // Detect and delete attachments that are not in the new list
                IEnumerable<string> urlsToDelete = Enumerable.Empty<string>();
                if (transaction.Attachments != null && transaction.Attachments.Any())
                {
                    urlsToDelete = transaction.Attachments.Except(data.Attachments);
                }

                // Execute the insert 
                if (newAttachments.Any())
                {
                    await _attachmentsRepo.DeleteBatchAsync(newAttachments);
                }

                // Execute the delete attachments
                if ((urlsToDelete.Any()))
                {
                    foreach (var item in urlsToDelete)
                    {
                        await _storageService.DeleteFileAsync(item);
                    }
                }

                // Update the balance 
                var existingAmount = transaction.IsIncome ? transaction.Amount : -transaction.Amount; 
                var newAmount = data.IsIncome ? data.Amount : -data.Amount;
                var amountToAdd = newAmount - existingAmount; 

                if (amountToAdd != 0)
                    await _walletsRepo.UpdateBalanceAsync(wallet.Id, userId, amountToAdd);

                transaction.Update(data.IsIncome,
                                   data.Amount,
                                   data.Category,
                                   data.Description,
                data.Tags,
                data.Attachments);

                await _transactionsRepo.UpdateAsync(transaction);

                return new OkObjectResult(new APISuccessResponse<TransactionDto>("Transaction updated", data));
            }
            else
            {
                var wallet = await _walletsRepo.GetByIdAsync(data.WalletId, userId);
                if (wallet == null)
                {
                    return new BadRequestObjectResult(new APIErrorResponse("Wallet not found"));
                }

                IEnumerable<Attachment> attachments = null;
                if (data.Attachments != null && data.Attachments.Any())
                {
                    attachments = await _attachmentsRepo.GetByURLAsync(data.Attachments);
                    if (data.Attachments.Distinct().Count() != attachments.Count())
                        return new BadRequestObjectResult(new APIErrorResponse("Invalid attachments"));
                    await _attachmentsRepo.DeleteBatchAsync(attachments);
                }

                var transaction = Transaction.Create(wallet.Id,
                                                     userId,
                                                     data.Amount,
                                                     data.IsIncome,
                                                     data.Category,
                                                     data.Description,
                                                     data.Tags,
                                                     attachments?.Select(a => a.Url).ToArray());

                await _transactionsRepo.CreateAsync(transaction);

                var amountToAdd = data.IsIncome ? data.Amount : -data.Amount;
                await _walletsRepo.UpdateBalanceAsync(wallet.Id, userId, amountToAdd);

                data.Id = transaction.Id;

                return new OkObjectResult(new APISuccessResponse<TransactionDto>("Transaction created", data));
            }

        }
    }
}