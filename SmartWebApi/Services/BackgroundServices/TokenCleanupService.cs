using SmartWebApi.Data.UnitOfWork;

namespace SmartWebApi.Services.BackgroundServices;

public class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Run daily

    public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokensAsync();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token cleanup");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Retry after 30 minutes on error
            }
        }
    }

    private async Task CleanupExpiredTokensAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            await unitOfWork.RefreshTokens.CleanupExpiredTokensAsync();
            var deletedCount = await unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} expired refresh tokens", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired tokens");
        }
    }
}