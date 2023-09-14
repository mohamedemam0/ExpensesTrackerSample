using ExpensesTracker.Server.Data.Interfaces;
using ExpensesTracker.Server.Data.Models;
using ExpensesTracker.Shared.DTOs;
using ExpensesTracker.Shared.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ExpensesTracker.Server.Functions
{
    public class UpsertWallet
    {
        private readonly ILogger<UpsertWallet> _logger;
        private readonly IWalletsRepository _walletRepo;
        private readonly IValidator<WalletDto> _walletValidator;
        public UpsertWallet(ILogger<UpsertWallet> log, IWalletsRepository walletsRepo, IValidator<WalletDto> walletValidator)
        {
            _logger = log;
            _walletRepo = walletsRepo;
            _walletValidator = walletValidator;
        }

        [FunctionName("UpsertWallet")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Upsert wallet started.");
            var userId = "userId";

            //Read the string from the body (JSON)
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //Deserialize the string to WalletDto object
            var data = JsonConvert.DeserializeObject<WalletDto>(requestBody);

            // Input Validations
            var validationResult = _walletValidator.Validate(data);
            if (!validationResult.IsValid)
            {
                _logger.LogError("ERROR: Upsert wallet - invalid input.");
                return new BadRequestObjectResult(new APIErrorResponse("Wallet inputs are not valid.",
                    validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            //TODO: Validate the number of wallets based on the subscription type
            var isAdd = string.IsNullOrWhiteSpace(data.Id);
            Wallet wallet = null;

            // Add Operation:
            if (isAdd)
            {
                wallet = new()
                {
                    CreationDate = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString()
                };
            }
            // Update Operation
            else
            {
                wallet = await _walletRepo.GetByIdAsync(data.Id, userId);
                if (wallet == null)
                {
                    _logger.LogError("ERROR - Wallet not found.");
                    return new NotFoundObjectResult(new APIErrorResponse("Wallet not found", null));
                }
            }

            wallet.Name = data.Name;
            wallet.CreationDate = DateTime.UtcNow;
            wallet.AccountType = data.AccountType;
            wallet.Swift = data.Swift;
            wallet.BankName = data.BankName;
            wallet.Currency = data.Currency;
            wallet.Iban = data.Iban;
            wallet.ModificationDate = DateTime.UtcNow;
            wallet.UserId = userId;
            wallet.Username = data.Username;            
            wallet.WalletType = data.Type.ToString();

            if (isAdd)
                await _walletRepo.CreateAsync(wallet);
            else
                await _walletRepo.UpdateAsync(wallet);


            //Set the auto generated properties
            data.Id = wallet.Id;
            data.CreationDate = wallet.CreationDate;

            return new OkObjectResult(new APISuccessResponse<WalletDto>($"Wallet {(isAdd ? "inserted" : "updated")} successfully.", data));
        }
    }
}

