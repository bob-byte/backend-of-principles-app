using BusinessLogic;
using BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using SET.DataAccess;
using SET.Shared.Models;
using SET.UnitTests.TestSupport;

namespace SET.UnitTests.Services;

public class AiConversationServiceTests
{
    private static (AiConversationService service, AppDbContext db) CreateSut()
    {
        AppDbContext db = TestDb.Create();
        return (new AiConversationService( db ), db);
    }

    private static async Task<AiConversation> AddConversationAsync(
        AppDbContext db,
        long userId,
        string clientId,
        DateTime? updatedAt = null )
    {
        AiConversation conversation = new()
        {
            UserId = userId,
            ClientId = clientId,
            Title = clientId,
            CreatedAt = new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ),
            UpdatedAt = updatedAt ?? new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ),
            Messages = new List<AiMessage>
            {
                new() { ClientId = "m1", Role = "user", Content = "hi", SortOrder = 0, CreatedAt = DateTime.UtcNow },
            },
        };
        db.AiConversations.Add( conversation );
        await db.SaveChangesAsync();
        return conversation;
    }

    private static AiConversationDto Dto( string clientId, string title = "Chat" ) => new()
    {
        ClientId = clientId,
        Title = title,
        Messages = new List<AiConversationMessageDto>
        {
            new() { Id = "a", Role = "user", Content = "question" },
            new() { Id = "b", Role = "assistant", Content = "answer" },
        },
    };

    [Fact]
    public async Task GetAllAsync_returns_users_conversations_newest_first()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();
        await AddConversationAsync( db, 1, "old", new DateTime( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc ) );
        await AddConversationAsync( db, 1, "new", new DateTime( 2026, 3, 1, 0, 0, 0, DateTimeKind.Utc ) );
        await AddConversationAsync( db, 2, "foreign" );

        List<AiConversationDto> conversations = await service.GetAllAsync( 1 );

        Assert.Equal( new[] { "new", "old" }, conversations.Select( c => c.ClientId ) );
        Assert.All( conversations, c => Assert.Single( c.Messages ) );
    }

    [Fact]
    public async Task GetByIdAsync_validates_and_scopes_to_user()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();
        AiConversation foreign = await AddConversationAsync( db, 2, "foreign" );
        AiConversation mine = await AddConversationAsync( db, 1, "mine" );

        TestData.AssertError( await service.GetByIdAsync( 1, 0 ), 400, "ConversationIdIsZeroOrNegative" );
        TestData.AssertError( await service.GetByIdAsync( 1, foreign.Id ), 404, $"ConversationIsNotFoundWithId {foreign.Id}" );
        Assert.Equal( "mine", (await service.GetByIdAsync( 1, mine.Id )).Value!.ClientId );
    }

    [Fact]
    public async Task GetByClientIdAsync_trims_and_scopes_to_user()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();
        await AddConversationAsync( db, 1, "abc" );
        await AddConversationAsync( db, 2, "foreign" );

        TestData.AssertError( await service.GetByClientIdAsync( 1, "  " ), 400, "ClientIdIsRequired" );
        TestData.AssertError( await service.GetByClientIdAsync( 1, "foreign" ), 404, "ConversationIsNotFoundWithClientId foreign" );
        Assert.Equal( "abc", (await service.GetByClientIdAsync( 1, " abc " )).Value!.ClientId );
    }

    [Fact]
    public async Task CreateAsync_validates_request()
    {
        (AiConversationService service, _) = CreateSut();

        TestData.AssertError( await service.CreateAsync( 1, null! ), 400, "ConversationIsNull" );
        TestData.AssertError( await service.CreateAsync( 1, Dto( " " ) ), 400, "ClientIdIsRequired" );
    }

    [Fact]
    public async Task CreateAsync_creates_new_conversation_with_messages()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();

        ServiceResult<AiConversationCreateResult> result = await service.CreateAsync( 1, Dto( " c-1 " ) );

        Assert.True( result.Value!.IsCreated );
        Assert.Equal( "c-1", result.Value.Conversation.ClientId );
        Assert.Equal( 2, result.Value.Conversation.Messages.Count );
        AiConversation stored = await db.AiConversations.Include( c => c.Messages ).SingleAsync();
        Assert.Equal( 1, stored.UserId );
        Assert.NotEqual( default, stored.CreatedAt );
    }

    [Fact]
    public async Task CreateAsync_existing_client_id_updates_instead_of_duplicating()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();
        await AddConversationAsync( db, 1, "c-1" );

        ServiceResult<AiConversationCreateResult> result = await service.CreateAsync( 1, Dto( "c-1", "Renamed" ) );

        Assert.False( result.Value!.IsCreated );
        Assert.Equal( "Renamed", result.Value.Conversation.Title );
        Assert.Single( db.AiConversations );
    }

    [Fact]
    public async Task CreateAsync_truncates_long_client_ids()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();

        await service.CreateAsync( 1, Dto( new string( 'c', 80 ) ) );

        Assert.Equal( 64, (await db.AiConversations.SingleAsync()).ClientId.Length );
    }

    [Fact]
    public async Task UpdateAsync_validates_and_applies_changes()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();
        AiConversation mine = await AddConversationAsync( db, 1, "mine" );

        TestData.AssertError( await service.UpdateAsync( 1, 0, Dto( "x" ) ), 400, "ConversationIdIsZeroOrNegative" );
        TestData.AssertError( await service.UpdateAsync( 1, mine.Id, null! ), 400, "RequestIsNull" );
        TestData.AssertError( await service.UpdateAsync( 2, mine.Id, Dto( "x" ) ), 404, $"ConversationIsNotFoundWithId {mine.Id}" );

        ServiceResult<AiConversationDto> result = await service.UpdateAsync( 1, mine.Id, Dto( "ignored", "New title" ) );

        Assert.Equal( "New title", result.Value!.Title );
        Assert.Equal( "mine", result.Value.ClientId );
        Assert.Equal( 2, result.Value.Messages.Count );
    }

    [Fact]
    public async Task UpsertByClientIdAsync_validates_inserts_then_updates()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();

        TestData.AssertError( await service.UpsertByClientIdAsync( 1, "", Dto( "x" ) ), 400, "ClientIdIsRequired" );
        TestData.AssertError( await service.UpsertByClientIdAsync( 1, "c", null! ), 400, "RequestIsNull" );

        await service.UpsertByClientIdAsync( 1, " c ", Dto( "c", "First" ) );
        ServiceResult<AiConversationDto> second = await service.UpsertByClientIdAsync( 1, "c", Dto( "c", "Second" ) );

        Assert.Equal( "Second", second.Value!.Title );
        Assert.Equal( "c", (await db.AiConversations.SingleAsync()).ClientId );
    }

    [Fact]
    public async Task DeleteAsync_validates_and_writes_tombstone()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();
        AiConversation mine = await AddConversationAsync( db, 1, "mine" );

        TestData.AssertError( await service.DeleteAsync( 1, -1 ), 400, "ConversationIdIsZeroOrNegative" );
        TestData.AssertError( await service.DeleteAsync( 2, mine.Id ), 404, $"ConversationIsNotFoundWithId {mine.Id}" );

        Assert.True( (await service.DeleteAsync( 1, mine.Id )).IsSuccess );
        Assert.Empty( db.AiConversations );
        SyncDeletion tombstone = await db.SyncDeletions.SingleAsync();
        Assert.Equal( (SyncEntityTypes.Conversation, mine.Id, 1L), (tombstone.EntityType, tombstone.EntityId, tombstone.UserId) );
    }

    [Fact]
    public async Task DeleteByClientIdAsync_validates_and_writes_tombstone()
    {
        (AiConversationService service, AppDbContext db) = CreateSut();
        AiConversation mine = await AddConversationAsync( db, 1, "mine" );

        TestData.AssertError( await service.DeleteByClientIdAsync( 1, " " ), 400, "ClientIdIsRequired" );
        TestData.AssertError( await service.DeleteByClientIdAsync( 2, "mine" ), 404, "ConversationIsNotFoundWithClientId mine" );

        Assert.True( (await service.DeleteByClientIdAsync( 1, " mine " )).IsSuccess );
        Assert.Empty( db.AiConversations );
        Assert.Equal( mine.Id, (await db.SyncDeletions.SingleAsync()).EntityId );
    }
}
