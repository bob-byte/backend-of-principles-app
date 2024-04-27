using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SET.DataAccess.Migrations;

public partial class Addedtableswithrelations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BuiltInFrequencies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Frequency = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BuiltInFrequencies", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Diets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DescriptionUri = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Diets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "EndRepeats",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Never = table.Column<bool>(type: "bit", nullable: true),
                Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                Timer = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EndRepeats", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PersonalPrinciples",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FullDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ReasonToFollow = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PersonalPrinciples", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Readings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Readings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Recipes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DescriptionUri = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Recipes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Reminders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TimeToMainNotice = table.Column<TimeSpan>(type: "time", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reminders", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserFrequencies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Interval = table.Column<int>(type: "int", nullable: false),
                Frequency = table.Column<TimeSpan>(type: "time", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserFrequencies", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Weight = table.Column<double>(type: "float", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserTasks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ActivityArea = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserTasks", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Workouts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Workouts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Books",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TimesRead = table.Column<int>(type: "int", nullable: true),
                ReadingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Books", x => x.Id);
                table.ForeignKey(
                    name: "FK_Books_Readings_ReadingId",
                    column: x => x.ReadingId,
                    principalTable: "Readings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DietRecipe",
            columns: table => new
            {
                FitDietsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RecipesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DietRecipe", x => new { x.FitDietsId, x.RecipesId });
                table.ForeignKey(
                    name: "FK_DietRecipe_Diets_FitDietsId",
                    column: x => x.FitDietsId,
                    principalTable: "Diets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DietRecipe_Recipes_RecipesId",
                    column: x => x.RecipesId,
                    principalTable: "Recipes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "NoticeRepeats",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BuiltInFrequencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UserFrequencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EndRepeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NoticeRepeats", x => x.Id);
                table.ForeignKey(
                    name: "FK_NoticeRepeats_BuiltInFrequencies_BuiltInFrequencyId",
                    column: x => x.BuiltInFrequencyId,
                    principalTable: "BuiltInFrequencies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_NoticeRepeats_EndRepeats_EndRepeatId",
                    column: x => x.EndRepeatId,
                    principalTable: "EndRepeats",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_NoticeRepeats_UserFrequencies_UserFrequencyId",
                    column: x => x.UserFrequencyId,
                    principalTable: "UserFrequencies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Goals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ReasonToAchieve = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                ActivityArea = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Goals", x => x.Id);
                table.ForeignKey(
                    name: "FK_Goals_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Habits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DescriptionUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ActivityArea = table.Column<int>(type: "int", nullable: false),
                DaysCountToAchieve = table.Column<int>(type: "int", nullable: false),
                TypeOfHabit = table.Column<int>(type: "int", nullable: false),
                TimesCountExecutedAct = table.Column<int>(type: "int", nullable: false),
                NegativeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                PositiveUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                AcquiredUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                OvercomeNegativeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Habits", x => x.Id);
                table.ForeignKey(
                    name: "FK_Habits_Users_AcquiredUserId",
                    column: x => x.AcquiredUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Habits_Users_NegativeUserId",
                    column: x => x.NegativeUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Habits_Users_OvercomeNegativeUserId",
                    column: x => x.OvercomeNegativeUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Habits_Users_PositiveUserId",
                    column: x => x.PositiveUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PersonalPrincipleUser",
            columns: table => new
            {
                PersonalPrinciplesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PersonalPrincipleUser", x => new { x.PersonalPrinciplesId, x.UsersId });
                table.ForeignKey(
                    name: "FK_PersonalPrincipleUser_PersonalPrinciples_PersonalPrinciplesId",
                    column: x => x.PersonalPrinciplesId,
                    principalTable: "PersonalPrinciples",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PersonalPrincipleUser_Users_UsersId",
                    column: x => x.UsersId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Rd71s",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CompletedDays = table.Column<int>(type: "int", nullable: false),
                IsEasyVersion = table.Column<bool>(type: "bit", nullable: false),
                MinTimeWorkoutExecute = table.Column<DateTime>(type: "datetime2", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReadingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DietId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                WorkoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rd71s", x => x.Id);
                table.ForeignKey(
                    name: "FK_Rd71s_Diets_DietId",
                    column: x => x.DietId,
                    principalTable: "Diets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Rd71s_Readings_ReadingId",
                    column: x => x.ReadingId,
                    principalTable: "Readings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Rd71s_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Rd71s_UserTasks_UserTaskId",
                    column: x => x.UserTaskId,
                    principalTable: "UserTasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Rd71s_Workouts_WorkoutId",
                    column: x => x.WorkoutId,
                    principalTable: "Workouts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TrainingPrograms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DescriptionUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TimeExecuteForOneTraining = table.Column<TimeSpan>(type: "time", nullable: false),
                WorkoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TrainingPrograms", x => x.Id);
                table.ForeignKey(
                    name: "FK_TrainingPrograms_Workouts_WorkoutId",
                    column: x => x.WorkoutId,
                    principalTable: "Workouts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BookReading",
            columns: table => new
            {
                ReadBooksDuringChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReadingFinishedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BookReading", x => new { x.ReadBooksDuringChallengeId, x.ReadingFinishedId });
                table.ForeignKey(
                    name: "FK_BookReading_Books_ReadBooksDuringChallengeId",
                    column: x => x.ReadBooksDuringChallengeId,
                    principalTable: "Books",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BookReading_Readings_ReadingFinishedId",
                    column: x => x.ReadingFinishedId,
                    principalTable: "Readings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Notices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsAllDay = table.Column<bool>(type: "bit", nullable: false),
                From = table.Column<DateTime>(type: "datetime2", nullable: true),
                To = table.Column<DateTime>(type: "datetime2", nullable: true),
                HabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RepeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notices", x => x.Id);
                table.ForeignKey(
                    name: "FK_Notices_Goals_GoalId",
                    column: x => x.GoalId,
                    principalTable: "Goals",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Notices_Habits_HabitId",
                    column: x => x.HabitId,
                    principalTable: "Habits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Notices_NoticeRepeats_RepeatId",
                    column: x => x.RepeatId,
                    principalTable: "NoticeRepeats",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "NoticeReminder",
            columns: table => new
            {
                NoticesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RemindersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NoticeReminder", x => new { x.NoticesId, x.RemindersId });
                table.ForeignKey(
                    name: "FK_NoticeReminder_Notices_NoticesId",
                    column: x => x.NoticesId,
                    principalTable: "Notices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_NoticeReminder_Reminders_RemindersId",
                    column: x => x.RemindersId,
                    principalTable: "Reminders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TimeZones",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Zone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                NoticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TimeZones", x => x.Id);
                table.ForeignKey(
                    name: "FK_TimeZones_Notices_NoticeId",
                    column: x => x.NoticeId,
                    principalTable: "Notices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BookReading_ReadingFinishedId",
            table: "BookReading",
            column: "ReadingFinishedId");

        migrationBuilder.CreateIndex(
            name: "IX_Books_ReadingId",
            table: "Books",
            column: "ReadingId",
            unique: true,
            filter: "[ReadingId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_DietRecipe_RecipesId",
            table: "DietRecipe",
            column: "RecipesId");

        migrationBuilder.CreateIndex(
            name: "IX_Goals_UserId",
            table: "Goals",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Habits_AcquiredUserId",
            table: "Habits",
            column: "AcquiredUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Habits_NegativeUserId",
            table: "Habits",
            column: "NegativeUserId",
            unique: true,
            filter: "[NegativeUserId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Habits_OvercomeNegativeUserId",
            table: "Habits",
            column: "OvercomeNegativeUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Habits_PositiveUserId",
            table: "Habits",
            column: "PositiveUserId");

        migrationBuilder.CreateIndex(
            name: "IX_NoticeReminder_RemindersId",
            table: "NoticeReminder",
            column: "RemindersId");

        migrationBuilder.CreateIndex(
            name: "IX_NoticeRepeats_BuiltInFrequencyId",
            table: "NoticeRepeats",
            column: "BuiltInFrequencyId");

        migrationBuilder.CreateIndex(
            name: "IX_NoticeRepeats_EndRepeatId",
            table: "NoticeRepeats",
            column: "EndRepeatId");

        migrationBuilder.CreateIndex(
            name: "IX_NoticeRepeats_UserFrequencyId",
            table: "NoticeRepeats",
            column: "UserFrequencyId");

        migrationBuilder.CreateIndex(
            name: "IX_Notices_GoalId",
            table: "Notices",
            column: "GoalId");

        migrationBuilder.CreateIndex(
            name: "IX_Notices_HabitId",
            table: "Notices",
            column: "HabitId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Notices_RepeatId",
            table: "Notices",
            column: "RepeatId");

        migrationBuilder.CreateIndex(
            name: "IX_PersonalPrincipleUser_UsersId",
            table: "PersonalPrincipleUser",
            column: "UsersId");

        migrationBuilder.CreateIndex(
            name: "IX_Rd71s_DietId",
            table: "Rd71s",
            column: "DietId");

        migrationBuilder.CreateIndex(
            name: "IX_Rd71s_ReadingId",
            table: "Rd71s",
            column: "ReadingId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Rd71s_UserId",
            table: "Rd71s",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Rd71s_UserTaskId",
            table: "Rd71s",
            column: "UserTaskId");

        migrationBuilder.CreateIndex(
            name: "IX_Rd71s_WorkoutId",
            table: "Rd71s",
            column: "WorkoutId");

        migrationBuilder.CreateIndex(
            name: "IX_TimeZones_NoticeId",
            table: "TimeZones",
            column: "NoticeId",
            unique: true,
            filter: "[NoticeId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_TrainingPrograms_WorkoutId",
            table: "TrainingPrograms",
            column: "WorkoutId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BookReading");

        migrationBuilder.DropTable(
            name: "DietRecipe");

        migrationBuilder.DropTable(
            name: "NoticeReminder");

        migrationBuilder.DropTable(
            name: "PersonalPrincipleUser");

        migrationBuilder.DropTable(
            name: "Rd71s");

        migrationBuilder.DropTable(
            name: "TimeZones");

        migrationBuilder.DropTable(
            name: "TrainingPrograms");

        migrationBuilder.DropTable(
            name: "Books");

        migrationBuilder.DropTable(
            name: "Recipes");

        migrationBuilder.DropTable(
            name: "Reminders");

        migrationBuilder.DropTable(
            name: "PersonalPrinciples");

        migrationBuilder.DropTable(
            name: "Diets");

        migrationBuilder.DropTable(
            name: "UserTasks");

        migrationBuilder.DropTable(
            name: "Notices");

        migrationBuilder.DropTable(
            name: "Workouts");

        migrationBuilder.DropTable(
            name: "Readings");

        migrationBuilder.DropTable(
            name: "Goals");

        migrationBuilder.DropTable(
            name: "Habits");

        migrationBuilder.DropTable(
            name: "NoticeRepeats");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "BuiltInFrequencies");

        migrationBuilder.DropTable(
            name: "EndRepeats");

        migrationBuilder.DropTable(
            name: "UserFrequencies");
    }
}
