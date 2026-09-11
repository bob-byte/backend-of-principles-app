using BusinessLogic;

namespace SET.UnitTests.Services;

public class AiServiceParseTaskTests
{
    [Fact]
    public void ParseTaskDraftJson_MapsTimeRemindersAndSubtasks()
    {
        const string json =
            """
            {
              "title": "Buy groceries",
              "description": "Evening run",
              "priority": "high",
              "theme": "Home",
              "dueDate": "2026-09-12T18:30",
              "allDay": false,
              "reminders": [30, 0, { "offsetMinutes": 60 }],
              "subtasks": [ "Milk", { "title": "Bread" }, { "name": "Eggs" } ]
            }
            """;

        AiTaskDraftResult draft = AiService.ParseTaskDraftJson( json );

        Assert.Equal( "Buy groceries", draft.Title );
        Assert.Equal( "Evening run", draft.Description );
        Assert.Equal( "high", draft.Priority );
        Assert.Equal( "Home", draft.Theme );
        Assert.Equal( "2026-09-12T18:30", draft.DueDate );
        Assert.False( draft.AllDay );
        Assert.Equal( new[] { 0, 30, 60 }, draft.Reminders );
        Assert.Equal( new[] { "Milk", "Bread", "Eggs" }, draft.Subtasks );
    }

    [Fact]
    public void ParseTaskDraftJson_DateOnlyDefaultsToAllDay()
    {
        const string json =
            """
            { "title": "Read", "dueDate": "2026-09-13" }
            """;

        AiTaskDraftResult draft = AiService.ParseTaskDraftJson( json );

        Assert.Equal( "2026-09-13", draft.DueDate );
        Assert.True( draft.AllDay );
        Assert.Empty( draft.Reminders );
        Assert.Empty( draft.Subtasks );
    }
}
