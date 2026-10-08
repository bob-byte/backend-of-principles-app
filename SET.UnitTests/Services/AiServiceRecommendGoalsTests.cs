using BusinessLogic;

namespace SET.UnitTests.Services;

public class AiServiceRecommendGoalsTests
{
    [Fact]
    public void ParseRecommendedGoalsJson_GoalsArray_MapsGoals()
    {
        const string json =
            """
            {
              "Goals": [
                { "Name": "Run a half marathon", "Reason": "Builds health stamina." },
                { "Name": "Sleep 8 hours nightly", "Reason": "Rest supports recovery." }
              ]
            }
            """;

        IReadOnlyList<RecommendedGoalResult> goals = AiService.ParseRecommendedGoalsJson( json );

        Assert.Equal( 2, goals.Count );
        Assert.Equal( "Run a half marathon", goals[0].Name );
        Assert.Equal( "Builds health stamina.", goals[0].Reason );
        Assert.Equal( "Sleep 8 hours nightly", goals[1].Name );
    }

    [Fact]
    public void BuildRecommendGoalsSystemPrompt_RequiresShortConcreteNames()
    {
        string prompt = AiService.BuildRecommendGoalsSystemPrompt( "English" );

        Assert.Contains( "short but concrete", prompt, StringComparison.Ordinal );
        Assert.Contains( "2 to 6 words", prompt, StringComparison.Ordinal );
        Assert.Contains( "motivating, vivid outcomes", prompt, StringComparison.Ordinal );
        Assert.Contains( "Buy a Tesla", prompt, StringComparison.Ordinal );
        Assert.Contains( "Get a promotion", prompt, StringComparison.Ordinal );
        Assert.Contains( "develop skills", prompt, StringComparison.Ordinal );
        Assert.Contains( "Put explanatory detail only in Reason", prompt, StringComparison.Ordinal );
        Assert.Contains( "English", prompt, StringComparison.Ordinal );
    }

    [Fact]
    public void BuildRecommendGoalsUserPrompt_Context_IncludesAreaAndExistingGoals()
    {
        RecommendGoalsContext context = new()
        {
            Culture = "en",
            AreaOfLife = "Health",
            Gender = "woman",
            Mission = "Live with strength",
            ExistingGoals = new[] { "Quit smoking", "Drink water daily" },
            Draft = "Be fitter"
        };

        string prompt = AiService.BuildRecommendGoalsUserPrompt( context, "English" );

        Assert.Contains( "Health", prompt, StringComparison.Ordinal );
        Assert.Contains( "motivating, outcome-focused", prompt, StringComparison.Ordinal );
        Assert.Contains( "Quit smoking", prompt );
        Assert.Contains( "Be fitter", prompt );
        Assert.Contains( "Live with strength", prompt );
        Assert.Contains( "English", prompt );
    }

    [Fact]
    public void BuildRecommendGoalsUserPrompt_MissingArea_UsesGeneralLifePrompt()
    {
        RecommendGoalsContext context = new()
        {
            Culture = "en",
            Mission = "Live with strength",
            ExistingGoals = new[] { "Quit smoking" }
        };

        string prompt = AiService.BuildRecommendGoalsUserPrompt( context, "English" );

        Assert.Contains( "goals for my life", prompt, StringComparison.Ordinal );
        Assert.Contains( "balanced mix", prompt, StringComparison.Ordinal );
        Assert.DoesNotContain( "area of my life", prompt, StringComparison.Ordinal );
        Assert.Contains( "Quit smoking", prompt );
        Assert.Contains( "Live with strength", prompt );
    }
}
