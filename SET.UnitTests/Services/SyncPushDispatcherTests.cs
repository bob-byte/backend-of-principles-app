using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SET.DataAccess;
using SET.Shared.Models;

namespace SET.UnitTests.Services;

public class SyncPushDispatcherTests
{
    private sealed class RecordingSender : IPushSender
    {
        public bool IsConfigured { get; set; } = true;
        public HashSet<string> InvalidTokens { get; } = new();
        public ConcurrentQueue<(string Token, IReadOnlyDictionary<string, string> Data)> Sent { get; } = new();

        public Task<PushSendResult> SendDataAsync(
            string token,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default )
        {
            Sent.Enqueue( (token, data) );
            return Task.FromResult( InvalidTokens.Contains( token ) ? PushSendResult.InvalidToken : PushSendResult.Sent );
        }
    }

    private static (SyncPushDispatcher dispatcher, RecordingSender sender, IServiceProvider services) CreateSut(
        TimeSpan? debounce = null )
    {
        string databaseName = Guid.NewGuid().ToString();
        RecordingSender sender = new();
        ServiceProvider services = new ServiceCollection()
            .AddDbContext<AppDbContext>( options => options.UseInMemoryDatabase( databaseName ) )
            .AddSingleton<IPushSender>( sender )
            .BuildServiceProvider();
        SyncPushDispatcher dispatcher = new(
            services.GetRequiredService<IServiceScopeFactory>(),
            debounce ?? TimeSpan.FromMilliseconds( 50 ) );
        return (dispatcher, sender, services);
    }

    private static async Task SeedDevicesAsync( IServiceProvider services, params (long UserId, string DeviceId, string Token)[] devices )
    {
        using IServiceScope scope = services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        foreach ((long userId, string deviceId, string token) in devices)
        {
            db.UserDevices.Add( new UserDevice
            {
                UserId = userId,
                DeviceId = deviceId,
                PushToken = token,
                Platform = "ios",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            } );
        }

        await db.SaveChangesAsync();
    }

    private static PendingSyncPush Pending( params SyncPushRequest[] requests )
    {
        PendingSyncPush push = new( DateTime.UtcNow );
        foreach (SyncPushRequest request in requests)
        {
            push.Add( request );
        }

        return push;
    }

    private static SyncPushRequest Request( string? origin, long[]? tasks = null, long[]? habits = null ) =>
        new( 7, origin, tasks ?? Array.Empty<long>(), habits ?? Array.Empty<long>() );

    [Fact]
    public async Task FlushAsync_skips_the_origin_device_and_other_users()
    {
        (SyncPushDispatcher dispatcher, RecordingSender sender, IServiceProvider services) = CreateSut();
        await SeedDevicesAsync(
            services,
            (7, "mac", "token-mac"),
            (7, "phone", "token-phone"),
            (8, "other", "token-other") );

        await dispatcher.FlushAsync( 7, Pending( Request( "mac", tasks: new long[] { 42 } ) ), CancellationToken.None );

        (string token, IReadOnlyDictionary<string, string> data) = Assert.Single( sender.Sent );
        Assert.Equal( "token-phone", token );
        Assert.Equal( "sync", data["type"] );
        Assert.Equal( "42", data["deletedTaskIds"] );
        Assert.Equal( string.Empty, data["deletedHabitIds"] );
    }

    [Fact]
    public async Task FlushAsync_wakes_every_device_when_several_made_changes()
    {
        (SyncPushDispatcher dispatcher, RecordingSender sender, IServiceProvider services) = CreateSut();
        await SeedDevicesAsync( services, (7, "mac", "token-mac"), (7, "phone", "token-phone") );

        await dispatcher.FlushAsync( 7, Pending( Request( "mac" ), Request( "phone" ) ), CancellationToken.None );

        Assert.Equal(
            new[] { "token-mac", "token-phone" },
            sender.Sent.Select( s => s.Token ).OrderBy( t => t ) );
    }

    [Fact]
    public async Task FlushAsync_removes_devices_with_invalid_tokens()
    {
        (SyncPushDispatcher dispatcher, RecordingSender sender, IServiceProvider services) = CreateSut();
        await SeedDevicesAsync( services, (7, "old", "token-old"), (7, "phone", "token-phone") );
        sender.InvalidTokens.Add( "token-old" );

        await dispatcher.FlushAsync( 7, Pending( Request( null ) ), CancellationToken.None );

        using IServiceScope scope = services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal( new[] { "phone" }, await db.UserDevices.Select( d => d.DeviceId ).ToListAsync() );
    }

    [Fact]
    public async Task FlushAsync_is_a_noop_when_fcm_is_not_configured()
    {
        (SyncPushDispatcher dispatcher, RecordingSender sender, IServiceProvider services) = CreateSut();
        await SeedDevicesAsync( services, (7, "phone", "token-phone") );
        sender.IsConfigured = false;

        await dispatcher.FlushAsync( 7, Pending( Request( null ) ), CancellationToken.None );

        Assert.Empty( sender.Sent );
    }

    [Fact]
    public async Task Burst_of_changes_is_sent_as_one_push_with_all_deleted_ids()
    {
        (SyncPushDispatcher dispatcher, RecordingSender sender, IServiceProvider services) =
            CreateSut( TimeSpan.FromMilliseconds( 150 ) );
        await SeedDevicesAsync( services, (7, "mac", "token-mac"), (7, "phone", "token-phone") );

        await dispatcher.StartAsync( CancellationToken.None );
        try
        {
            dispatcher.Enqueue( Request( "mac", tasks: new long[] { 1 } ) );
            dispatcher.Enqueue( Request( "mac", tasks: new long[] { 2 }, habits: new long[] { 3 } ) );
            dispatcher.Enqueue( Request( "mac" ) );

            for (int i = 0; i < 50 && sender.Sent.IsEmpty; i++)
            {
                await Task.Delay( 20 );
            }

            await Task.Delay( 300 );
        }
        finally
        {
            await dispatcher.StopAsync( CancellationToken.None );
        }

        (string token, IReadOnlyDictionary<string, string> data) = Assert.Single( sender.Sent );
        Assert.Equal( "token-phone", token );
        Assert.Equal( "1,2", data["deletedTaskIds"] );
        Assert.Equal( "3", data["deletedHabitIds"] );
    }

    [Fact]
    public void ToData_caps_the_number_of_ids()
    {
        PendingSyncPush push = Pending( Request( null, tasks: Enumerable.Range( 1, 500 ).Select( i => (long)i ).ToArray() ) );

        Dictionary<string, string> data = push.ToData( 3 );

        Assert.Equal( "1,2,3", data["deletedTaskIds"] );
    }

    [Theory]
    [InlineData( HttpStatusCode.NotFound, "{}", true )]
    [InlineData( HttpStatusCode.BadRequest, "{\"error\":{\"details\":[{\"errorCode\":\"UNREGISTERED\"}]}}", true )]
    [InlineData( HttpStatusCode.Forbidden, "{\"error\":{\"details\":[{\"errorCode\":\"SENDER_ID_MISMATCH\"}]}}", true )]
    [InlineData( HttpStatusCode.BadRequest, "{\"error\":{\"message\":\"The registration token is not a valid FCM registration token\"}}", true )]
    [InlineData( HttpStatusCode.BadRequest, "{\"error\":{\"message\":\"Invalid JSON payload\"}}", false )]
    [InlineData( HttpStatusCode.ServiceUnavailable, "{}", false )]
    public void IsInvalidToken_only_flags_dead_tokens( HttpStatusCode status, string body, bool expected )
    {
        Assert.Equal( expected, FcmPushSender.IsInvalidToken( status, body ) );
    }

    [Fact]
    public void BuildMessage_is_a_silent_background_push()
    {
        object message = FcmPushSender.BuildMessage(
            "token-phone",
            new Dictionary<string, string> { ["type"] = "sync" } );

        using JsonDocument json = JsonDocument.Parse( JsonSerializer.Serialize( message ) );
        JsonElement root = json.RootElement.GetProperty( "message" );
        Assert.Equal( "token-phone", root.GetProperty( "token" ).GetString() );
        Assert.Equal( "sync", root.GetProperty( "data" ).GetProperty( "type" ).GetString() );
        Assert.False( root.TryGetProperty( "notification", out _ ) );
        Assert.Equal( "HIGH", root.GetProperty( "android" ).GetProperty( "priority" ).GetString() );
        JsonElement apns = root.GetProperty( "apns" );
        Assert.Equal( "background", apns.GetProperty( "headers" ).GetProperty( "apns-push-type" ).GetString() );
        Assert.Equal( "5", apns.GetProperty( "headers" ).GetProperty( "apns-priority" ).GetString() );
        Assert.Equal(
            1,
            apns.GetProperty( "payload" ).GetProperty( "aps" ).GetProperty( "content-available" ).GetInt32() );
    }
}
