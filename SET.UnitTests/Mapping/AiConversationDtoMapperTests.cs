using BusinessLogic;
using BusinessLogic.Models;
using SET.Shared.Models;

namespace SET.UnitTests.Mapping;

public class AiConversationDtoMapperTests
{
    [Fact]
    public void ApplyMessages_null_keeps_existing_messages()
    {
        AiConversation entity = new() { Messages = new List<AiMessage> { new() { Role = "user", Content = "hi" } } };

        AiConversationDtoMapper.ApplyMessages( entity, null );

        Assert.Single( entity.Messages );
    }

    [Fact]
    public void ApplyMessages_keeps_only_user_and_assistant_roles()
    {
        AiConversation entity = new();

        AiConversationDtoMapper.ApplyMessages( entity, new List<AiConversationMessageDto>
        {
            new() { Id = "1", Role = " User ", Content = "hello" },
            new() { Id = "2", Role = "system", Content = "ignored" },
            new() { Id = "3", Role = "assistant", Content = "hi" },
        } );

        List<AiMessage> messages = entity.Messages.ToList();
        Assert.Equal( new[] { "user", "assistant" }, messages.Select( m => m.Role ) );
        Assert.Equal( new[] { 0, 1 }, messages.Select( m => m.SortOrder ) );
    }

    [Fact]
    public void ApplyMessages_generates_and_truncates_client_ids_and_keeps_explicit_sort_order()
    {
        AiConversation entity = new();

        AiConversationDtoMapper.ApplyMessages( entity, new List<AiConversationMessageDto>
        {
            new() { Id = "", Role = "user", Content = "a", SortOrder = 10 },
            new() { Id = new string( 'x', 100 ), Role = "assistant", Content = "b" },
        } );

        List<AiMessage> messages = entity.Messages.ToList();
        Assert.Equal( 32, messages[0].ClientId.Length );
        Assert.Equal( 10, messages[0].SortOrder );
        Assert.Equal( 64, messages[1].ClientId.Length );
        Assert.Equal( 1, messages[1].SortOrder );
    }

    [Fact]
    public void ApplyDto_trims_title_and_uses_client_timestamps_as_utc()
    {
        AiConversation entity = new();
        DateTime created = new( 2026, 2, 1, 10, 0, 0, DateTimeKind.Unspecified );
        DateTime updated = new( 2026, 2, 2, 10, 0, 0, DateTimeKind.Unspecified );

        AiConversationDtoMapper.ApplyDto( entity, new AiConversationDto
        {
            Title = "  " + new string( 't', 300 ),
            CreatedAt = created,
            UpdatedAt = updated,
        } );

        Assert.Equal( 255, entity.Title.Length );
        Assert.Equal( DateTimeKind.Utc, entity.CreatedAt.Kind );
        Assert.Equal( created.Ticks, entity.CreatedAt.Ticks );
        Assert.Equal( updated.Ticks, entity.UpdatedAt.Ticks );
    }

    [Fact]
    public void ToDto_orders_messages_and_can_omit_them()
    {
        AiConversation entity = new()
        {
            Id = 3,
            ClientId = "c",
            Title = "Chat",
            Messages = new List<AiMessage>
            {
                new() { ClientId = "late", Role = "assistant", SortOrder = 1 },
                new() { ClientId = "early", Role = "user", SortOrder = 0 },
            },
        };

        Assert.Equal( new[] { "early", "late" }, AiConversationDtoMapper.ToDto( entity ).Messages.Select( m => m.Id ) );
        Assert.Null( AiConversationDtoMapper.ToDto( entity, includeMessages: false ).Messages );
    }
}
