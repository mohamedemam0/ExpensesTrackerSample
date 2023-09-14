using AKExpensesTracker.Server.Functions.Mapper;
using ExpensesTracker.Server.Data.Interfaces;
using ExpensesTracker.Shared.DTOs;
using ExpensesTracker.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ExpensesTracker.Server.Functions
{
    public class ListTransactions
    {
        private readonly ILogger<ListTransactions> _logger;
        private readonly ITransactionRepo _transactionRepo;

        public ListTransactions(ILogger<ListTransactions> log)
        {
            _logger = log;
        }

        [FunctionName("ListTransactions")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("List transactions");
            var userId = "userId"; // TODO: Fetch from the access token

            string yearParameter = req.Query["year"];
            string minDateParameter = req.Query["minDate"];
            string maxDateParameter = req.Query["maxDate"];
            var walletIds = req.Query["walletIds"];

            if (string.IsNullOrWhiteSpace(yearParameter))
                return new BadRequestObjectResult(new APIErrorResponse("Year is required"));
            if (!walletIds.Any())
                return new BadRequestObjectResult(new APIErrorResponse("At least one wallet is required"));

            int year = 2023;
            DateTime? minDate = null;
            DateTime? maxDate = null;
            if (!int.TryParse(yearParameter, out year))
                return new BadRequestObjectResult(new APIErrorResponse("Year is invalid"));

            if (!string.IsNullOrWhiteSpace(minDateParameter) && DateTime.TryParse(minDateParameter, out var minDateValue))
                minDate = minDateValue;

            if (!string.IsNullOrWhiteSpace(maxDateParameter) && DateTime.TryParse(maxDateParameter, out var maxDateValue))
                maxDate = maxDateValue;

            var transactions = await _transactionRepo.ListByUserIdAsync(userId, year, walletIds, minDate, maxDate);

            return new OkObjectResult(new APISuccessResponse<IEnumerable<TransactionDto>>("Transactions retreived successfully",
                                                                                           transactions.Select(t => t.ToTransactionDto())));
        }
    }
}

