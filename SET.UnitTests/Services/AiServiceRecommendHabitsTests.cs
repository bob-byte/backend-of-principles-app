using BusinessLogic;

namespace SET.UnitTests.Services;

public class AiServiceRecommendHabitsTests
{
    [Fact]
    public void BuildRecommendHabitsSystemPrompt_RequiresShortGoalMovingActions()
    {
        string prompt = AiService.BuildRecommendHabitsSystemPrompt( "English" );

        Assert.Contains( "short concrete action", prompt, StringComparison.Ordinal );
        Assert.Contains( "2 to 12 words", prompt, StringComparison.Ordinal );
        Assert.Contains( "Good names: \"Meditate when I wake up\"", prompt, StringComparison.Ordinal );
        Assert.Contains( "Set aside $200", prompt, StringComparison.Ordinal );
        Assert.Contains( "Apply to 5 jobs", prompt, StringComparison.Ordinal );
        Assert.DoesNotContain( "Bad name", prompt, StringComparison.Ordinal );
        Assert.DoesNotContain( "Do not put a time", prompt, StringComparison.Ordinal );
        Assert.Contains( "every day", prompt, StringComparison.Ordinal );
        Assert.Contains( "direct lever", prompt, StringComparison.Ordinal );
        Assert.Contains( "ReasonToFollow must name which", prompt, StringComparison.Ordinal );
        Assert.Contains( "English", prompt, StringComparison.Ordinal );
    }

    [Fact]
    public void BuildRecommendUserPrompt_WithGoal_LeadsWithGoalAndDoesNotThemeAroundHabits()
    {
        RecommendHabitsContext context = new()
        {
            Culture = "en",
            Goal = "Buy a Tesla",
            Gender = "man",
            Mission = "Live with strength",
            MainSlogan = "Act today",
            CurrentHabits = new[] { "Meditate", "Drink water" },
            AreasOfLife = new[] { "Health" }
        };

        string prompt = AiService.BuildRecommendUserPrompt( context, "English" );

        Assert.Contains( "direct lever for my goal: \"Buy a Tesla\"", prompt, StringComparison.Ordinal );
        Assert.True( prompt.IndexOf( "Buy a Tesla", StringComparison.Ordinal ) <
                     prompt.IndexOf( "Meditate", StringComparison.Ordinal ) );
        Assert.Contains( "do not repeat them", prompt, StringComparison.Ordinal );
        Assert.Contains( "do not theme the list around them", prompt, StringComparison.Ordinal );
        Assert.DoesNotContain( "related to specified habits", prompt, StringComparison.Ordinal );
        Assert.Contains( "Live with strength", prompt, StringComparison.Ordinal );
        Assert.Contains( "Act today", prompt, StringComparison.Ordinal );
        Assert.Contains( "The goal still decides the habit", prompt, StringComparison.Ordinal );
        Assert.Contains( "English", prompt, StringComparison.Ordinal );
    }

    [Fact]
    public void BuildRecommendUserPrompt_WithoutGoal_UsesGoalList()
    {
        RecommendHabitsContext context = new()
        {
            Culture = "en",
            Goals = new[] { "Buy a Tesla", "Get a promotion" },
            CurrentHabits = new[] { "Journal" }
        };

        string prompt = AiService.BuildRecommendUserPrompt( context, "English" );

        Assert.Contains( "My goals are: Buy a Tesla; Get a promotion", prompt, StringComparison.Ordinal );
        Assert.Contains(
            "Each habit must target one of those goals, and ReasonToFollow must name which goal it moves",
            prompt,
            StringComparison.Ordinal );
        Assert.DoesNotContain( "direct lever for my goal", prompt, StringComparison.Ordinal );
        Assert.DoesNotContain( "related to specified habits", prompt, StringComparison.Ordinal );
        Assert.Contains( "Journal", prompt, StringComparison.Ordinal );
    }
}
