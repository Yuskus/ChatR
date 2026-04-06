using ChatR.Services;

namespace ChatR.Hosted;

public sealed class CleanupService(
    IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Запускаем таймер: каждый день в 03:00
        var now = DateTime.Now;
        var nextRun = now.Date.AddHours(3); // 03:00
        if (now > nextRun)
            nextRun = nextRun.AddDays(1);

        var timeToFirstRun = nextRun - now;

        _timer = new Timer(DoCleanup, null, timeToFirstRun, TimeSpan.FromDays(1));

        Console.WriteLine($"CleanupService запущен. Следующий запуск: {nextRun}");
        return Task.CompletedTask;
    }

    private async void DoCleanup(object? state)
    {
        using var scope = _serviceProvider.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CleanupService>>();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var messageService = scope.ServiceProvider.GetRequiredService<MessageService>();
        var roomService = scope.ServiceProvider.GetRequiredService<RoomService>();

        var now = DateTime.Now;

        var twoWeeksAgo = now.AddDays(-14);
        var oneMonthAgo = now.AddMonths(-1);
        var threeMonthsAgo = now.AddMonths(-3);

        try
        {
            await messageService.DeleteOldMessages(olderThan: twoWeeksAgo);

            await roomService.DeleteInactiveRooms(olderThan: oneMonthAgo);

            await userService.DeleteInactiveUsers(olderThan: threeMonthsAgo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Cleanup] Ошибка: {Message}", ex.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
