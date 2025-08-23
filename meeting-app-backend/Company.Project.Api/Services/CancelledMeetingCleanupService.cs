using Company.Project.Context;
using Microsoft.EntityFrameworkCore;

namespace Company.Project.Api.Services;

public class CancelledMeetingCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CancelledMeetingCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Her 24 saatte bir çalışacak

    public CancelledMeetingCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<CancelledMeetingCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("İptal Edilen Toplantıları Temizleme Servisi başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // İptal edilen toplantıları temizle
                await CleanupCancelledMeetingsAsync();

                // Bir sonraki çalışma zamanına kadar bekle
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "İptal edilen toplantılar temizlenirken bir hata oluştu.");
                
                // Hata durumunda 5 dakika bekle ve tekrar dene
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task CleanupCancelledMeetingsAsync()
    {
        // İptal edilmiş ve 30 günden daha eski toplantıları temizle
        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cancelledMeetings = await dbContext.Meetings
            .Where(m => m.IsCancelled && m.CancelledAt < cutoffDate)
            .Include(m => m.Documents)
            .ToListAsync();

        if (cancelledMeetings.Any())
        {
            _logger.LogInformation("{Count} adet iptal edilmiş toplantı temizleniyor.", cancelledMeetings.Count);
            
            dbContext.Meetings.RemoveRange(cancelledMeetings);
            await dbContext.SaveChangesAsync();
            
            _logger.LogInformation("{Count} adet iptal edilmiş toplantı başarıyla temizlendi.", cancelledMeetings.Count);
        }
        else
        {
            _logger.LogInformation("Temizlenecek iptal edilmiş toplantı bulunamadı.");
        }
    }
}
