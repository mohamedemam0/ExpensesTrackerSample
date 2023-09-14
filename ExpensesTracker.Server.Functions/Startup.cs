using ExpensesTracker.Server.Data;
using ExpensesTracker.Server.Functions.Services;
using ExpensesTracker.Shared;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ExpensesTracker.Server.Functions.Startup))]
namespace ExpensesTracker.Server.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.GetContext().Configuration;
            builder.Services.AddCosmosDbClient(config["CosmosDbConnectionString"]);
            builder.Services.AddRepositories();
            builder.Services.AddValidators();
            builder.Services.AddScoped<IStorageService>(sp => new AzureBlobStorageService(config["AzureWebJobsStorage"]));
        }
    }
}
