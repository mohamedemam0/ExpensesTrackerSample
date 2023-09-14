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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ExpensesTracker.Server.Functions
{
    public class DeleteTransaction
    {
        private readonly ILogger<DeleteTransaction> _logger;
        private readonly ITransactionRepo _transactionRepo;
        private readonly IStorageService _storageService;
        private readonly IWalletsRepository _walletRepo;
        public DeleteTransaction(ILogger<DeleteTransaction> log, ITransactionRepo transactionRepo, IStorageService storageService, IWalletsRepository walletRepo)
        {
            _logger = log;
            _transactionRepo = transactionRepo;
            _storageService = storageService;
            _walletRepo = walletRepo;
        }

        [FunctionName("DeleteTransaction")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("DeleteTransaction triggered.");
            var userId = "UserId";
            var id = req.Query["id"];
            var year = req.Query["year"];
            if (string.IsNullOrWhiteSpace(id))
                return new BadRequestObjectResult(new APIErrorResponse("Id is required", null));
            if (string.IsNullOrWhiteSpace(year))
                return new BadRequestObjectResult(new APIErrorResponse("Year is required", null));
            if (!int.TryParse(year, out var yearAsInt))
                return new BadRequestObjectResult(new APIErrorResponse("Year is invalid", null));

            //Get Transaction
            var transaction = await _transactionRepo.GetByIdAsync(id, userId, yearAsInt);
            if (transaction == null)
                return new NotFoundResult();

            //Delete attachment
            if (transaction.Attachments != null && transaction.Attachments.Any())
            {
                foreach (var url in transaction.Attachments)
                {
                    await _storageService.DeleteFileAsync(url);
                }
            }

            await _transactionRepo.DeleteAsync(transaction);

            var amountToAdd = transaction.IsIncome ? -transaction.Amount : transaction.Amount;
            await _walletRepo.UpdateBalanceAsync(transaction.WalletId, userId, amountToAdd);
            return new OkObjectResult(new APIResponse()
            { Message = "Transaction has been deleted successfully." });
        }
    }
}

