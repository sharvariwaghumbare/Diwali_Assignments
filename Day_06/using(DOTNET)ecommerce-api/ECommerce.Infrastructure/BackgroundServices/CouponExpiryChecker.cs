using ECommerce.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.BackgroundServices
{
    public class CouponExpiryChecker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public CouponExpiryChecker(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<CouponExpiryChecker>>();
                    try
                    {
                        logger.LogInformation("Checking for expired coupons...");
                        // Use stoppingToken in ToListAsync method to ensure that the operation can be cancelled if needed.
                        var expiredCoupons = await dbContext.Coupons
                            .Where(c => c.ExpiryDate <= DateTime.UtcNow && c.IsActive)
                            .ToListAsync(stoppingToken);
                        foreach (var coupon in expiredCoupons)
                        {
                            coupon.IsActive = false;
                            coupon.UpdatedAt = DateTime.UtcNow;
                        }
                        await dbContext.SaveChangesAsync(stoppingToken);
                        logger.LogInformation($"{expiredCoupons.Count} coupons deactivated due to expiry.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error checking for expired coupons.");
                    }
                    // Get the delay interval from configuration or set a default value
                    var delay = new TimeSpan(
                        _configuration.GetValue<int>("CouponExpiryCheck:Days", 0),
                        _configuration.GetValue<int>("CouponExpiryCheck:Hours", 0),
                        _configuration.GetValue<int>("CouponExpiryCheck:Minutes", 10),
                        _configuration.GetValue<int>("CouponExpiryCheck:Seconds", 0)
                    );
                    logger.LogInformation($"Next check will be in {delay}.");

                    // Wait for a specified time before checking again
                    await Task.Delay(delay, stoppingToken); // Check every 10 minutes
                }
            }
        }
    }
}
