using ExpensesTracker.Shared.DTOs;
using ExpensesTracker.Shared.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ExpensesTracker.Shared
{
    public static  class DependencyInjectionExtensions
    {
        public static void AddValidators(this IServiceCollection services)
        {
            // Automatic Way
            services.AddValidatorsFromAssemblyContaining<WalletDtoValidator>();

            //Manual Way
            //services.AddScoped<IValidator<WalletDto>,WalletDtoValidator>();
        }
    }
}
