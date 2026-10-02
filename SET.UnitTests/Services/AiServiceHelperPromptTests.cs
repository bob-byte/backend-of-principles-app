using BusinessLogic;

namespace SET.UnitTests.Services;

public class AiServiceHelperPromptTests
{
    [Fact]
    public void BuildHelperSystemPrompt_IncludesPrinciplesDomainFraming()
    {
        string prompt = AiService.BuildHelperSystemPrompt( null );

        Assert.Contains( "Principles app", prompt, StringComparison.Ordinal );
        Assert.Contains( "identity-oriented", prompt, StringComparison.Ordinal );
        Assert.Contains( "Goal -> Habits -> Results", prompt, StringComparison.Ordinal );
        Assert.Contains( "finite to-dos", prompt, StringComparison.Ordinal );
        Assert.Contains( "consistency over intensity", prompt, StringComparison.Ordinal );
    }

    [Fact]
    public void BuildHelperSystemPrompt_RequiresNativeNaturalTone()
    {
        string prompt = AiService.BuildHelperSystemPrompt( null );

        Assert.Contains( "fluent native speaker", prompt, StringComparison.Ordinal );
        Assert.Contains( "translated English life-coach", prompt, StringComparison.Ordinal );
        Assert.Contains( "natural modern Ukrainian", prompt, StringComparison.Ordinal );
        Assert.Contains( "answer the question directly", prompt, StringComparison.Ordinal );
    }

    [Fact]
    public void BuildHelperSystemPrompt_AppendsOpenTasks()
    {
        ChatUserContext context = new()
        {
            Tasks = new[] { "Buy groceries", "Call coach" }
        };

        string prompt = AiService.BuildHelperSystemPrompt( context );

        Assert.Contains( "Buy groceries; Call coach.", prompt, StringComparison.Ordinal );
        Assert.Contains( "not a full inbox", prompt, StringComparison.Ordinal );
        Assert.Contains( "Never invent names", prompt, StringComparison.Ordinal );
    }

    [Fact]
    public void BuildHelperSystemPrompt_CapsHabits()
    {
        string[] habits = Enumerable.Range( 1, AiService.MaxHelperHabits + 5 )
            .Select( i => $"Habit {i}" )
            .ToArray();

        string prompt = AiService.BuildHelperSystemPrompt( new ChatUserContext { Habits = habits } );

        Assert.Contains( $"Habit {AiService.MaxHelperHabits}.", prompt, StringComparison.Ordinal );
        Assert.DoesNotContain( $"Habit {AiService.MaxHelperHabits + 1}", prompt, StringComparison.Ordinal );
        Assert.Contains( "not a full list", prompt, StringComparison.Ordinal );
    }

    [Fact]
    public void BuildHelperSystemPrompt_CapsOpenTasks()
    {
        string[] tasks = Enumerable.Range( 1, AiService.MaxHelperOpenTasks + 5 )
            .Select( i => $"Task {i}" )
            .ToArray();

        string prompt = AiService.BuildHelperSystemPrompt( new ChatUserContext { Tasks = tasks } );

        Assert.Contains( $"Task {AiService.MaxHelperOpenTasks}.", prompt, StringComparison.Ordinal );
        Assert.DoesNotContain( $"Task {AiService.MaxHelperOpenTasks + 1}", prompt, StringComparison.Ordinal );
    }
}
