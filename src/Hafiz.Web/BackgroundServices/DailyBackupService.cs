using Hafiz.Application.Interfaces.Services;

namespace Hafiz.Web.BackgroundServices
{
    /// <summary>
    /// خدمة خلفية تُنشئ نسخة احتياطية لقاعدة البيانات وترفعها إلى Google Drive
    /// مرة واحدة يوميًا في الوقت المحدد عبر الإعداد "Backup:DailyTime" (الافتراضي 02:00 صباحًا).
    /// </summary>
    public class DailyBackupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyBackupService> _logger;
        private readonly TimeSpan _runAt;

        public DailyBackupService(
            IServiceScopeFactory scopeFactory,
            ILogger<DailyBackupService> logger,
            IConfiguration config
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _runAt = TimeSpan.TryParse(config["Backup:DailyTime"], out var t)
                ? t
                : new TimeSpan(2, 0, 0); // 02:00 صباحًا افتراضيًا
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetDelayUntilNextRun();
                _logger.LogInformation(
                    "النسخ الاحتياطي اليومي القادم بعد {Delay} (الساعة {Time})",
                    delay,
                    _runAt
                );

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break; // الإيقاف أثناء الانتظار
                }

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var backupService =
                        scope.ServiceProvider.GetRequiredService<IBackupService>();
                    var fileName = await backupService.RunBackupAsync(stoppingToken);
                    _logger.LogInformation(
                        "اكتمل النسخ الاحتياطي اليومي المجدول: {File}",
                        fileName
                    );
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    // لا نُسقط الخدمة عند فشل دورة واحدة — نسجّل الخطأ وننتظر اليوم التالي.
                    _logger.LogError(ex, "فشل النسخ الاحتياطي اليومي المجدول");
                }
            }
        }

        private TimeSpan GetDelayUntilNextRun()
        {
            var now = DateTime.Now;
            var nextRun = now.Date.Add(_runAt);
            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);
            return nextRun - now;
        }
    }
}
