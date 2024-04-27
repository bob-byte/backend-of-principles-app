using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SET.Shared.Models;

namespace SET.DataAccess.Extensions;

public static class StatementSeedingExtension
{
    public static void SeedDefaultStatements(this EntityTypeBuilder<Statement> entityTypeBuilder)
    {
        entityTypeBuilder.HasData(
            new Statement
            {
                Text = "We cannot solve problems with the kind of thinking we employed when we came up with them.",
                Author = "Albert Einstein"
            },
            new Statement
            {
                Text = "Learn as if you will live forever, live like you will die tomorrow.",
                Author = "Mahatma Gandhi"
            },
            new Statement
            {
                Text = "Stay away from those people who try to disparage your ambitions. Small minds will always do that, " +
                "but great minds will give you a feeling that you can become great too.",
                Author = "Mark Twain"
            },
            new Statement
            {
                Text = "When you give joy to other people, you get more joy in return. You should give a good thought to " +
                "happiness that you can give out.",
                Author = "Eleanor Roosevelt"
            },
            new Statement
            {
                Text = "When you change your thoughts, remember to also change your world.",
                Author = "Norman Vincent Peale"
            },
            new Statement
            {
                Text = "It is only when we take chances, when our lives improve. The initial and the most difficult risk that " +
                "we need to take is to become honest.",
                Author = "Walter Anderson"
            },
            new Statement
            {
                Text = "Nature has given us all the pieces required to achieve exceptional wellness and health, but has left it " +
                "to us to put these pieces together.",
                Author = "Diane McLaren"
            },
            new Statement
            {
                Text = "Concentrate all your thoughts upon the work in hand. The sun's rays do not burn until brought to a focus.",
                Author = "Alexander Graham Bell"
            },
            new Statement
            {
                Text = "Either you run the day or the day runs you.",
                Author = "Jim Rohn"
            },
            new Statement
            {
                Text = "I’m a greater believer in luck, and I find the harder I work the more I have of it.",
                Author = "Thomas Jefferson"
            },
            new Statement
            {
                Text = "When we strive to become better than we are, everything around us becomes better too.",
                Author = "Paulo Coelho"
            },
            new Statement
            {
                Text = "Opportunity is missed by most people because it is dressed in overalls and looks like work.",
                Author = "Thomas Edison"
            },
            new Statement
            {
                Text = "Setting goals is the first step in turning the invisible into the visible.",
                Author = "Tony Robbins"
            },
            new Statement
            {
                Text = "Your work is going to fill a large part of your life, and the only way to be truly satisfied is to do " +
                "what you believe is great work. And the only way to do great work is to love what you do. If you haven't found " +
                "it yet, keep looking. Don't settle. As with all matters of the heart, you'll know when you find it.",
                Author = "Steve Jobs"
            },
            new Statement
            {
                Text = "It’s not about better time management. It’s about better life management",
                Author = "Alexandra of The Productivity Zone"
            },
            new Statement
            {
                Text = "The pessimist sees difficulty in every opportunity. The optimist sees opportunity in every difficulty.",
                Author = "Winston Churchill"
            },
            new Statement
            {
                Text = "Don’t let yesterday take up too much of today.",
                Author = "Will Rogers"
            },
            new Statement
            {
                Text = "You learn more from failure than from success. Don’t let it stop you. Failure builds character.",
            },
            new Statement
            {
                Text = "If you are working on something that you really care about, you don’t have to be pushed. The vision pulls you.",
                Author = "Steve Jobs"
            },
            new Statement
            {
                Text = "Experience is a hard teacher because she gives the test first, the lesson afterwards.",
                Author = "Vernon Sanders Law"
            },
            new Statement
            {
                Text = "To know how much there is to know is the beginning of learning to live.",
                Author = "Dorothy West"
            },
            new Statement
            {
                Text = "Goal setting is the secret to a compelling future.",
                Author = "Tony Robbins"
            },
            new Statement
            {
                Text = "Just one small positive thought in the morning can change your whole day.",
                Author = "Dalai Lama"
            },
            new Statement
            {
                Text = "Opportunities don't happen, you create them.",
                Author = "Chris Grosser"
            },
            new Statement
            {
                Text = "Love your family, work super hard, live your passion.",
                Author = "Gary Vaynerchuk"
            },
            new Statement
            {
                Text = "It is never too late to be what you might have been.",
                Author = "George Eliot"
            },
            new Statement
            {
                Text = "Don't let someone else's opinion of you become your reality",
                Author = "Les Brown"
            },
            new Statement
            {
                Text = "If you’re not positive energy, you’re negative energy.",
                Author = "Mark Cuban"
            },
            new Statement
            {
                Text = "I am not a product of my circumstances. I am a product of my decisions.",
                Author = "Stephen R. Covey"
            },
            new Statement
            {
                Text = "I can’t tell you how many times I’ve been given a no. Only to find that a better, brighter, " +
                "bigger yes was right around the corner.",
                Author = "Arlan Hamilton"
            },
            new Statement
            {
                Text = "The two most important days in your life are the day you’re born and the day you find out why.",
                Author = "Mark Twain"
            },
            new Statement
            {
                Text = "Nothing ever goes away until it teaches us what we need to know.",
                Author = "Pema Chodron"
            },
            new Statement
            {
                Text = "We can see through others only when we can see through ourselves.",
                Author = "Bruce Lee"
            },
            new Statement
            {
                Text = "The best way out is always through.",
                Author = "Robert Frost"
            },
            new Statement
            {
                Text = "If there is no struggle, there is no progress.",
                Author = "Frederick Douglass"
            },
            new Statement
            {
                Text = "Courage is like a muscle. We strengthen it by use",
                Author = "Ruth Gordo"
            },
            new Statement
            {
                Text = "Keep a little fire burning; however small, however hidden.",
                Author = "Cormac McCarthy"
            }
            );
    }
}
