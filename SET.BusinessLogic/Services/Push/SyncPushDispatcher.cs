using System.Threading;
using System.Threading.Channels;

using Microsoft.Extensions.Hosting;

namespace BusinessLogic;

/// <summary>
/// Collects <see cref="SyncPushRequest"/>s per user for a short window, then sends one
/// silent push to each of the user's other devices. Batching keeps bursts (autosave,
/// bulk edits) from tripping APNs background-push throttling.
/// </summary>
public sealed class SyncPushDispatcher : BackgroundService, ISyncPushService
{
    public static readonly TimeSpan DefaultDebounce = TimeSpan.FromSeconds( 2 );

    /// <summary>FCM data payloads are capped at 4 KB; peers still sync the full delta.</summary>
    internal const int MaxIdsPerPush = 200;

    private readonly Channel<SyncPushRequest> m_channel =
        Channel.CreateUnbounded<SyncPushRequest>( new UnboundedChannelOptions { SingleReader = true } );

    private readonly IServiceScopeFactory m_scopeFactory;
    private readonly TimeSpan m_debounce;

    public SyncPushDispatcher( IServiceScopeFactory scopeFactory )
        : this( scopeFactory, DefaultDebounce )
    {
    }

    internal SyncPushDispatcher( IServiceScopeFactory scopeFactory, TimeSpan debounce )
    {
        m_scopeFactory = scopeFactory;
        m_debounce = debounce;
    }

    public void Enqueue( SyncPushRequest request )
    {
        if (request.UserId > 0)
        {
            m_channel.Writer.TryWrite( request );
        }
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        Dictionary<long, PendingSyncPush> pending = new();
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime? nextDue = pending.Count == 0 ? null : pending.Values.Min( p => p.DueAt );
            try
            {
                await WaitForWorkAsync( nextDue, stoppingToken ).ConfigureAwait( false );
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            while (m_channel.Reader.TryRead( out SyncPushRequest? request ))
            {
                if (!pending.TryGetValue( request.UserId, out PendingSyncPush? entry ))
                {
                    entry = new PendingSyncPush( DateTime.UtcNow + m_debounce );
                    pending[request.UserId] = entry;
                }

                entry.Add( request );
            }

            DateTime now = DateTime.UtcNow;
            foreach (long userId in pending.Where( p => p.Value.DueAt <= now ).Select( p => p.Key ).ToList())
            {
                PendingSyncPush due = pending[userId];
                pending.Remove( userId );
                try
                {
                    await FlushAsync( userId, due, stoppingToken ).ConfigureAwait( false );
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Log.Error( ex, "Sync push for user {UserId} failed", userId );
                }
            }
        }
    }

    private async Task WaitForWorkAsync( DateTime? nextDue, CancellationToken stoppingToken )
    {
        if (nextDue is null)
        {
            await m_channel.Reader.WaitToReadAsync( stoppingToken ).ConfigureAwait( false );
            return;
        }

        TimeSpan delay = nextDue.Value - DateTime.UtcNow;
        if (delay <= TimeSpan.Zero)
        {
            return;
        }

        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource( stoppingToken );
        timeout.CancelAfter( delay );
        try
        {
            await m_channel.Reader.WaitToReadAsync( timeout.Token ).ConfigureAwait( false );
        }
        catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
        {
            // Debounce window elapsed.
        }
    }

    internal async Task FlushAsync( long userId, PendingSyncPush push, CancellationToken cancellationToken )
    {
        using IServiceScope scope = m_scopeFactory.CreateScope();
        IPushSender sender = scope.ServiceProvider.GetRequiredService<IPushSender>();
        if (!sender.IsConfigured)
        {
            return;
        }

        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        string? excludedDeviceId = push.ExcludedDeviceId;
        List<UserDevice> devices = await dbContext.UserDevices
            .Where( d => d.UserId == userId && d.DeviceId != excludedDeviceId )
            .ToListAsync( cancellationToken )
            .ConfigureAwait( false );
        if (devices.Count == 0)
        {
            return;
        }

        Dictionary<string, string> data = push.ToData( MaxIdsPerPush );
        List<UserDevice> invalid = new();
        foreach (UserDevice device in devices)
        {
            PushSendResult result = await sender
                .SendDataAsync( device.PushToken, data, cancellationToken )
                .ConfigureAwait( false );
            if (result == PushSendResult.InvalidToken)
            {
                invalid.Add( device );
            }
        }

        if (invalid.Count > 0)
        {
            dbContext.UserDevices.RemoveRange( invalid );
            await dbContext.SaveChangesAsync( cancellationToken ).ConfigureAwait( false );
        }
    }
}

internal sealed class PendingSyncPush
{
    private readonly HashSet<string?> m_origins = new();

    public PendingSyncPush( DateTime dueAt )
    {
        DueAt = dueAt;
    }

    public DateTime DueAt { get; }
    public HashSet<long> DeletedTaskIds { get; } = new();
    public HashSet<long> DeletedHabitIds { get; } = new();

    /// <summary>
    /// The origin is skipped only when one device made every change in the window;
    /// otherwise each device needs the others' edits.
    /// </summary>
    public string? ExcludedDeviceId => m_origins.Count == 1 ? m_origins.First() : null;

    public void Add( SyncPushRequest request )
    {
        DeletedTaskIds.UnionWith( request.DeletedTaskIds );
        DeletedHabitIds.UnionWith( request.DeletedHabitIds );
        m_origins.Add( request.OriginDeviceId );
    }

    public Dictionary<string, string> ToData( int maxIds ) => new()
    {
        ["type"] = "sync",
        ["deletedTaskIds"] = string.Join( ',', DeletedTaskIds.Take( maxIds ) ),
        ["deletedHabitIds"] = string.Join( ',', DeletedHabitIds.Take( maxIds ) ),
    };
}
