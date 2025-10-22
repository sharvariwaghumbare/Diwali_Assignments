using DinkToPdf;
using DinkToPdf.Contracts;
using ECommerce.Application.Services.Abstract;
using ECommerce.Infrastructure.BackgroundServices;
using ECommerce.Infrastructure.Services.Pdf;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.DI
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddScoped<IInvoiceService, InvoiceService>();

            // Register background services
            services.AddHostedService<CouponExpiryChecker>();

            return services;
        }
    }
}
