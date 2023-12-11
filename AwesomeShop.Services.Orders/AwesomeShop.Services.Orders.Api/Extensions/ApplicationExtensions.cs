using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeShop.Services.Orders.Api.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            services.AddMediatR(typeof(ApplicationExtensions));

            return services;
        }
    }
}
