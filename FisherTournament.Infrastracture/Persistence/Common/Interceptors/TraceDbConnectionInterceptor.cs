using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FisherTournament.Infrastracture.Persistence.Common.Interceptors;

public class TraceDbConnectionInterceptor : IDbConnectionInterceptor
{
    public const string ActivitySourceName = "MyDiagnostics.EFCore";

    private ActivitySource MyEFActivitySource = new ActivitySource(ActivitySourceName);

    private readonly ILogger<TraceDbConnectionInterceptor> _logger;

    public TraceDbConnectionInterceptor(ILogger<TraceDbConnectionInterceptor> logger)
    {
        _logger = logger;
    }

    public void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        // var activity = MyEFActivitySource.StartActivity("ConnectionOpened", ActivityKind.Client);

        // if (activity == null)
        // {
        //     return;
        // }

        // activity.AddEvent(new ActivityEvent("ConnectionOpened"));
    }

    public async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        // await Task.CompletedTask;

        // var activity = MyEFActivitySource.StartActivity("ConnectionOpened", ActivityKind.Client);

        // if (activity == null)
        // {
        //     return;
        // }

        // activity.AddEvent(new ActivityEvent("ConnectionOpened"));
    }

    public void ConnectionClosing(DbConnection connection, ConnectionEventData eventData)
    {
        // var activity = Activity.Current;


        // if (activity == null)
        // {
        //     _logger.LogWarning("[MyEFCoreDiagnostics] Activity.Current is null");
        //     return;
        // }

        // if (activity.Source != MyEFActivitySource)
        // {
        //     _logger.LogWarning("[MyEFCoreDiagnostics] Activity.Current.Source is not MyEFActivitySource");
        //     return;
        // }

        // activity.AddEvent(new ActivityEvent("ConnectionClosing"));

        // activity.Stop();
    }

    public async Task ConnectionClosingAsync(DbConnection connection, ConnectionEventData eventData, CancellationToken cancellationToken = default)
    {
        // var activity = Activity.Current;

        // if (activity == null)
        // {
        //     _logger.LogWarning("[MyEFCoreDiagnostics] Activity.Current is null");
        //     return;
        // }

        // if (activity.Source != MyEFActivitySource)
        // {
        //     _logger.LogWarning("[MyEFCoreDiagnostics] Activity.Current.Source is not MyEFActivitySource");
        //     return;
        // }

        // activity.Stop();

        // await Task.CompletedTask;
    }
}