using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SET.DataAccess.Migrations;

public partial class AddChallangeModelWithoutRelationships : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "DevelopmentPlanId",
            table: "Users",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Challenges",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChallangeName = table.Column<int>(type: "int", nullable: false),
                ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UrlWithFullDescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Challenges", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "DevelopmentPlans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DevelopmentPlanType = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DevelopmentPlans", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RecomendedBooks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Author = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecomendedBooks", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Statements",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Text = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Statements", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "Statements",
            columns: new[] { "Id", "Author", "Text" },
            values: new object[,]
            {
                { new Guid("a8955359-707d-4bc5-8280-0ad1a7b80572"), "Albert Einstein", "We cannot solve problems with the kind of thinking we employed when we came up with them." },
                { new Guid("c1bb84df-ca33-4621-83c4-e0f0aec85760"), "Dorothy West", "To know how much there is to know is the beginning of learning to live." },
                { new Guid("5ade159d-4d36-4f99-b3c9-6ef4cd3a28bf"), "Tony Robbins", "Goal setting is the secret to a compelling future." },
                { new Guid("e9ddb4a7-89fd-4f16-a39e-56ec284bf603"), "Dalai Lama", "Just one small positive thought in the morning can change your whole day." },
                { new Guid("cd9da572-c085-42d4-9560-c240c003c4c6"), "Chris Grosser", "Opportunities don't happen, you create them." },
                { new Guid("e3cb33ae-0363-4884-8a5c-0f43458ffaa1"), "Gary Vaynerchuk", "Love your family, work super hard, live your passion." },
                { new Guid("d7f2e914-fa4d-4295-ace6-4931b28d91be"), "George Eliot", "It is never too late to be what you might have been." },
                { new Guid("0382d15a-a720-4b82-98d2-714c9925a3da"), "Les Brown", "Don't let someone else's opinion of you become your reality" },
                { new Guid("064b356c-a2d3-4b40-98af-ec7f3f8ff853"), "Mark Cuban", "If you’re not positive energy, you’re negative energy." },
                { new Guid("7bc4b2db-a774-4d14-ac7f-628397d5d111"), "Stephen R. Covey", "I am not a product of my circumstances. I am a product of my decisions." },
                { new Guid("d3e61e34-6e7a-4495-a2b9-1334418db0cd"), "Arlan Hamilton", "I can’t tell you how many times I’ve been given a no. Only to find that a better, brighter, bigger yes was right around the corner." },
                { new Guid("6f4bf96f-66f3-482c-b33e-ab228d4b53d9"), "Mark Twain", "The two most important days in your life are the day you’re born and the day you find out why." },
                { new Guid("44839d75-a6ce-4a25-8334-f7596e4d6542"), "Pema Chodron", "Nothing ever goes away until it teaches us what we need to know." },
                { new Guid("9d2a7b82-f0fe-4be1-a58d-78ff9f1f394b"), "Bruce Lee", "We can see through others only when we can see through ourselves." },
                { new Guid("a119d2d7-e5c9-4ceb-a049-832ae8d8faa0"), "Robert Frost", "The best way out is always through." },
                { new Guid("5833352a-c749-4c6e-850d-12d0b26ab3f7"), "Frederick Douglass", "If there is no struggle, there is no progress." },
                { new Guid("3ee26b66-e5b0-4e20-9afe-fba0703e8925"), "Vernon Sanders Law", "Experience is a hard teacher because she gives the test first, the lesson afterwards." },
                { new Guid("b1a9208f-e256-4231-8567-962942434938"), "Ruth Gordo", "Courage is like a muscle. We strengthen it by use" },
                { new Guid("4652f3f0-2fae-444e-ba46-f9dc09868c6b"), "Steve Jobs", "If you are working on something that you really care about, you don’t have to be pushed. The vision pulls you." },
                { new Guid("eb8b35eb-2fa1-49d6-b257-2a1380d7d4d3"), "Will Rogers", "Don’t let yesterday take up too much of today." },
                { new Guid("6fe1342b-d3cd-4711-83de-8aa4dafd4a6e"), "Mahatma Gandhi", "Learn as if you will live forever, live like you will die tomorrow." },
                { new Guid("9899f4d5-f761-4bcd-b6f6-d63457aa8ad8"), "Mark Twain", "Stay away from those people who try to disparage your ambitions. Small minds will always do that, but great minds will give you a feeling that you can become great too." },
                { new Guid("2874ee4e-bc63-45b5-83e9-e4b851cf1c52"), "Eleanor Roosevelt", "When you give joy to other people, you get more joy in return. You should give a good thought to happiness that you can give out." },
                { new Guid("996298ac-8c02-4583-8754-173397c6533a"), "Norman Vincent Peale", "When you change your thoughts, remember to also change your world." },
                { new Guid("86b591ac-2f9a-4ee9-aaae-5eba580c46f7"), "Walter Anderson", "It is only when we take chances, when our lives improve. The initial and the most difficult risk that we need to take is to become honest." },
                { new Guid("9b2fcc50-de33-4423-8372-490a52c437fb"), "Diane McLaren", "Nature has given us all the pieces required to achieve exceptional wellness and health, but has left it to us to put these pieces together." },
                { new Guid("4cd3970a-f235-4495-9578-12f0d8cf7197"), "Alexander Graham Bell", "Concentrate all your thoughts upon the work in hand. The sun's rays do not burn until brought to a focus." },
                { new Guid("a23257cb-e537-4e64-9a89-130d815e3080"), "Jim Rohn", "Either you run the day or the day runs you." },
                { new Guid("588fdb03-63e4-4ad5-8de5-3dacd4bad2f9"), "Thomas Jefferson", "I’m a greater believer in luck, and I find the harder I work the more I have of it." },
                { new Guid("9b72fe1c-17c9-45c3-ac60-f1343c7cc02a"), "Paulo Coelho", "When we strive to become better than we are, everything around us becomes better too." },
                { new Guid("cedbca08-ca3b-4ffc-9df2-ebd42d674d7b"), "Thomas Edison", "Opportunity is missed by most people because it is dressed in overalls and looks like work." },
                { new Guid("30ef31ed-383f-42dd-8bf5-d09cb969fa7a"), "Tony Robbins", "Setting goals is the first step in turning the invisible into the visible." },
                { new Guid("47f4a0fb-7d56-4c94-8efb-5b6e305d2a24"), "Steve Jobs", "Your work is going to fill a large part of your life, and the only way to be truly satisfied is to do what you believe is great work. And the only way to do great work is to love what you do. If you haven't found it yet, keep looking. Don't settle. As with all matters of the heart, you'll know when you find it." },
                { new Guid("0af9ce60-55d3-4f4e-89ea-1a95e979eb8b"), "Alexandra of The Productivity Zone", "It’s not about better time management. It’s about better life management" },
                { new Guid("bd9826ae-2de0-4614-88c2-6f1a6fc181cc"), "Winston Churchill", "The pessimist sees difficulty in every opportunity. The optimist sees opportunity in every difficulty." },
                { new Guid("2f018b93-8440-4a87-b47f-94902010d911"), null, "You learn more from failure than from success. Don’t let it stop you. Failure builds character." },
                { new Guid("1a7e8ee1-540c-4e64-bdfd-9151789037c6"), "Cormac McCarthy", "Keep a little fire burning; however small, however hidden." }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_DevelopmentPlanId",
            table: "Users",
            column: "DevelopmentPlanId");

        migrationBuilder.AddForeignKey(
            name: "FK_Users_DevelopmentPlans_DevelopmentPlanId",
            table: "Users",
            column: "DevelopmentPlanId",
            principalTable: "DevelopmentPlans",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_DevelopmentPlans_DevelopmentPlanId",
            table: "Users");

        migrationBuilder.DropTable(
            name: "Challenges");

        migrationBuilder.DropTable(
            name: "DevelopmentPlans");

        migrationBuilder.DropTable(
            name: "RecomendedBooks");

        migrationBuilder.DropTable(
            name: "Statements");

        migrationBuilder.DropIndex(
            name: "IX_Users_DevelopmentPlanId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "DevelopmentPlanId",
            table: "Users");
    }
}
