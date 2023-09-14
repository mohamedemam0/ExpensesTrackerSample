using Microsoft.Extensions.DependencyInjection;

namespace ExpensesTracker.Server.Data
{
    public static class DependencyInjectionExtensions
    {

        public static void AddCosmosDbClient(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton(sp => new CosmosClient(connectionString, new CosmosClientOptions
            {
                AllowBulkExecution = true,
            }));
        }

        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IWalletsRepository, CosmosWalletsRepo>();
            services.AddScoped<IAttachmentRepo, CosmosAttachmentRepo>();
            services.AddScoped<ITransactionRepo, CosmosTransactionRepo>();
        }
    }
}
