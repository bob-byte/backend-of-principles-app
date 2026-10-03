using BusinessLogic;

namespace SET.UnitTests.Services;

public class AiServiceSuggestProfileTextTests
{
    [Fact]
    public void ParseProfileTextSuggestionsJson_SuggestionsArray_MapsSuggestions()
    {
        const string json =
            """
            {
              "Suggestions": [
                { "Text": "Character over comfort", "Reason": "Guides hard choices." },
                { "Text": "Serve before self", "Reason": "Keeps priorities clear." }
              ]
            }
            """;

        IReadOnlyList<ProfileTextSuggestionResult> suggestions =
            AiService.ParseProfileTextSuggestionsJson( json );

        Assert.Equal( 2, suggestions.Count );
        Assert.Equal( "Character over comfort", suggestions[0].Text );
        Assert.Equal( "Guides hard choices.", suggestions[0].Reason );
        Assert.Equal( "Serve before self", suggestions[1].Text );
    }

    [Fact]
    public void BuildSuggestProfileTextUserPrompt_Context_IncludesHintAndGoals()
    {
        SuggestProfileTextContext context = new()
        {
            Kind = ProfileTextKind.Mission,
            Culture = "en",
            Name = "Alex",
            Gender = "man",
            Hint = "faith and craft",
            Goals = new[] { "Write daily", "Train strength" },
            MainSlogan = "Character over comfort"
        };

        string prompt = AiService.BuildSuggestProfileTextUserPrompt( context, "English" );

        Assert.Contains( "missions", prompt, StringComparison.OrdinalIgnoreCase );
        Assert.Contains( "Alex", prompt );
        Assert.Contains( "faith and craft", prompt );
        Assert.Contains( "Write daily", prompt );
        Assert.Contains( "Character over comfort", prompt );
        Assert.Contains( "English", prompt );
    }
}
