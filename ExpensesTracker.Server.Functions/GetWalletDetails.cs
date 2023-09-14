using ExpensesTracker.Server.Data.Interfaces;
using ExpensesTracker.Shared.DTOs;
using ExpensesTracker.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading.Tasks;

namespace ExpensesTracker.Server.Functions
{
    public class GetWalletDetails
    {
        private readonly ILogger<GetWalletDetails> _logger;
        private readonly IWalletsRepository _walletRepo;

        public GetWalletDetails(ILogger<GetWalletDetails> log, IWalletsRepository walletRepo)
        {
            _logger = log;
            _walletRepo = walletRepo;
        }

        [FunctionName("GetWalletDetails")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            // TODO: Fetch the user id fromt eh access token.
            var userId = "userId";
            var walletId = req.Query["id"];
            _logger.LogInformation($"Retrieve the wallet with Id {walletId} for the user {userId}");
            if (string.IsNullOrWhiteSpace(walletId))
                return new BadRequestObjectResult(new APIErrorResponse("Wallet Id is required.", null));

            var wallet = await _walletRepo.GetByIdAsync(walletId, userId);
            if (wallet == null)
                return new NotFoundResult(); //404 response Code
            else
            {
                return new OkObjectResult(new WalletDto
                {
                    Id = walletId,
                    Name = wallet.Name,
                    AccountType = wallet.AccountType,
                    Balance = wallet.Balance,
                    Currency = wallet.Currency, 
                    BankName = wallet.BankName,
                    CreationDate = wallet.CreationDate,
                    Iban   = wallet.Iban,
                    Swift = wallet.Swift,
                    Type = wallet.Type.Value,
                    Username = wallet.Username
                    }); // 200 with response body
            }
        }
    }
}

