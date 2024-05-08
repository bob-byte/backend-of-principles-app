using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DeleteExtraTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReading_Books_ReadBooksDuringChallengeId",
                table: "BookReading");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReading_Readings_ReadingFinishedId",
                table: "BookReading");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_Readings_ReadingId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_DietRecipe_Diets_FitDietsId",
                table: "DietRecipe");

            migrationBuilder.DropForeignKey(
                name: "FK_DietRecipe_Recipes_RecipesId",
                table: "DietRecipe");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeReminder_Notices_NoticesId",
                table: "NoticeReminder");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeReminder_Reminders_RemindersId",
                table: "NoticeReminder");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeRepeats_BuiltInFrequencies_BuiltInFrequencyId",
                table: "NoticeRepeats");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeRepeats_EndRepeats_EndRepeatId",
                table: "NoticeRepeats");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeRepeats_UserFrequencies_UserFrequencyId",
                table: "NoticeRepeats");

            migrationBuilder.DropForeignKey(
                name: "FK_Notices_Goals_GoalId",
                table: "Notices");

            migrationBuilder.DropForeignKey(
                name: "FK_Notices_NoticeRepeats_RepeatId",
                table: "Notices");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71s_Challenges_ChallengeId",
                table: "Rd71s");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71s_Diets_DietId",
                table: "Rd71s");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71s_Readings_ReadingId",
                table: "Rd71s");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71s_UserTasks_UserTaskId",
                table: "Rd71s");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71s_Workouts_WorkoutId",
                table: "Rd71s");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeZones_Notices_NoticeId",
                table: "TimeZones");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPrograms_Workouts_WorkoutId",
                table: "TrainingPrograms");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_DevelopmentPlans_DevelopmentPlanId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "RecomendedBooks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workouts",
                table: "Workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTasks",
                table: "UserTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFrequencies",
                table: "UserFrequencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingPrograms",
                table: "TrainingPrograms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeZones",
                table: "TimeZones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Recipes",
                table: "Recipes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Readings",
                table: "Readings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rd71s",
                table: "Rd71s");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notices",
                table: "Notices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NoticeRepeats",
                table: "NoticeRepeats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EndRepeats",
                table: "EndRepeats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Diets",
                table: "Diets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DevelopmentPlans",
                table: "DevelopmentPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BuiltInFrequencies",
                table: "BuiltInFrequencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Books",
                table: "Books");

            migrationBuilder.RenameTable(
                name: "Workouts",
                newName: "Workout");

            migrationBuilder.RenameTable(
                name: "UserTasks",
                newName: "UserTask");

            migrationBuilder.RenameTable(
                name: "UserFrequencies",
                newName: "UserFrequency");

            migrationBuilder.RenameTable(
                name: "TrainingPrograms",
                newName: "TrainingProgram");

            migrationBuilder.RenameTable(
                name: "TimeZones",
                newName: "TimeZone");

            migrationBuilder.RenameTable(
                name: "Reminders",
                newName: "Reminder");

            migrationBuilder.RenameTable(
                name: "Recipes",
                newName: "Recipe");

            migrationBuilder.RenameTable(
                name: "Readings",
                newName: "Reading");

            migrationBuilder.RenameTable(
                name: "Rd71s",
                newName: "Rd71");

            migrationBuilder.RenameTable(
                name: "Notices",
                newName: "Notice");

            migrationBuilder.RenameTable(
                name: "NoticeRepeats",
                newName: "NoticeRepeat");

            migrationBuilder.RenameTable(
                name: "EndRepeats",
                newName: "EndRepeat");

            migrationBuilder.RenameTable(
                name: "Diets",
                newName: "Diet");

            migrationBuilder.RenameTable(
                name: "DevelopmentPlans",
                newName: "DevelopmentPlan");

            migrationBuilder.RenameTable(
                name: "BuiltInFrequencies",
                newName: "BuiltInFrequency");

            migrationBuilder.RenameTable(
                name: "Books",
                newName: "Book");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingPrograms_WorkoutId",
                table: "TrainingProgram",
                newName: "IX_TrainingProgram_WorkoutId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeZones_NoticeId",
                table: "TimeZone",
                newName: "IX_TimeZone_NoticeId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71s_WorkoutId",
                table: "Rd71",
                newName: "IX_Rd71_WorkoutId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71s_UserTaskId",
                table: "Rd71",
                newName: "IX_Rd71_UserTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71s_ReadingId",
                table: "Rd71",
                newName: "IX_Rd71_ReadingId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71s_DietId",
                table: "Rd71",
                newName: "IX_Rd71_DietId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71s_ChallengeId",
                table: "Rd71",
                newName: "IX_Rd71_ChallengeId");

            migrationBuilder.RenameIndex(
                name: "IX_Notices_RepeatId",
                table: "Notice",
                newName: "IX_Notice_RepeatId");

            migrationBuilder.RenameIndex(
                name: "IX_Notices_GoalId",
                table: "Notice",
                newName: "IX_Notice_GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_NoticeRepeats_UserFrequencyId",
                table: "NoticeRepeat",
                newName: "IX_NoticeRepeat_UserFrequencyId");

            migrationBuilder.RenameIndex(
                name: "IX_NoticeRepeats_EndRepeatId",
                table: "NoticeRepeat",
                newName: "IX_NoticeRepeat_EndRepeatId");

            migrationBuilder.RenameIndex(
                name: "IX_NoticeRepeats_BuiltInFrequencyId",
                table: "NoticeRepeat",
                newName: "IX_NoticeRepeat_BuiltInFrequencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Books_ReadingId",
                table: "Book",
                newName: "IX_Book_ReadingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workout",
                table: "Workout",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTask",
                table: "UserTask",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFrequency",
                table: "UserFrequency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingProgram",
                table: "TrainingProgram",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeZone",
                table: "TimeZone",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Recipe",
                table: "Recipe",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reading",
                table: "Reading",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rd71",
                table: "Rd71",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notice",
                table: "Notice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NoticeRepeat",
                table: "NoticeRepeat",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EndRepeat",
                table: "EndRepeat",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Diet",
                table: "Diet",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevelopmentPlan",
                table: "DevelopmentPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BuiltInFrequency",
                table: "BuiltInFrequency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Book",
                table: "Book",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Book_Reading_ReadingId",
                table: "Book",
                column: "ReadingId",
                principalTable: "Reading",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReading_Book_ReadBooksDuringChallengeId",
                table: "BookReading",
                column: "ReadBooksDuringChallengeId",
                principalTable: "Book",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookReading_Reading_ReadingFinishedId",
                table: "BookReading",
                column: "ReadingFinishedId",
                principalTable: "Reading",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DietRecipe_Diet_FitDietsId",
                table: "DietRecipe",
                column: "FitDietsId",
                principalTable: "Diet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DietRecipe_Recipe_RecipesId",
                table: "DietRecipe",
                column: "RecipesId",
                principalTable: "Recipe",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notice_Goals_GoalId",
                table: "Notice",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notice_NoticeRepeat_RepeatId",
                table: "Notice",
                column: "RepeatId",
                principalTable: "NoticeRepeat",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeReminder_Notice_NoticesId",
                table: "NoticeReminder",
                column: "NoticesId",
                principalTable: "Notice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeReminder_Reminder_RemindersId",
                table: "NoticeReminder",
                column: "RemindersId",
                principalTable: "Reminder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeRepeat_BuiltInFrequency_BuiltInFrequencyId",
                table: "NoticeRepeat",
                column: "BuiltInFrequencyId",
                principalTable: "BuiltInFrequency",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeRepeat_EndRepeat_EndRepeatId",
                table: "NoticeRepeat",
                column: "EndRepeatId",
                principalTable: "EndRepeat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeRepeat_UserFrequency_UserFrequencyId",
                table: "NoticeRepeat",
                column: "UserFrequencyId",
                principalTable: "UserFrequency",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71_Challenges_ChallengeId",
                table: "Rd71",
                column: "ChallengeId",
                principalTable: "Challenges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71_Diet_DietId",
                table: "Rd71",
                column: "DietId",
                principalTable: "Diet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71_Reading_ReadingId",
                table: "Rd71",
                column: "ReadingId",
                principalTable: "Reading",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71_UserTask_UserTaskId",
                table: "Rd71",
                column: "UserTaskId",
                principalTable: "UserTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71_Workout_WorkoutId",
                table: "Rd71",
                column: "WorkoutId",
                principalTable: "Workout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeZone_Notice_NoticeId",
                table: "TimeZone",
                column: "NoticeId",
                principalTable: "Notice",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingProgram_Workout_WorkoutId",
                table: "TrainingProgram",
                column: "WorkoutId",
                principalTable: "Workout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_DevelopmentPlan_DevelopmentPlanId",
                table: "Users",
                column: "DevelopmentPlanId",
                principalTable: "DevelopmentPlan",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Book_Reading_ReadingId",
                table: "Book");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReading_Book_ReadBooksDuringChallengeId",
                table: "BookReading");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReading_Reading_ReadingFinishedId",
                table: "BookReading");

            migrationBuilder.DropForeignKey(
                name: "FK_DietRecipe_Diet_FitDietsId",
                table: "DietRecipe");

            migrationBuilder.DropForeignKey(
                name: "FK_DietRecipe_Recipe_RecipesId",
                table: "DietRecipe");

            migrationBuilder.DropForeignKey(
                name: "FK_Notice_Goals_GoalId",
                table: "Notice");

            migrationBuilder.DropForeignKey(
                name: "FK_Notice_NoticeRepeat_RepeatId",
                table: "Notice");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeReminder_Notice_NoticesId",
                table: "NoticeReminder");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeReminder_Reminder_RemindersId",
                table: "NoticeReminder");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeRepeat_BuiltInFrequency_BuiltInFrequencyId",
                table: "NoticeRepeat");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeRepeat_EndRepeat_EndRepeatId",
                table: "NoticeRepeat");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeRepeat_UserFrequency_UserFrequencyId",
                table: "NoticeRepeat");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71_Challenges_ChallengeId",
                table: "Rd71");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71_Diet_DietId",
                table: "Rd71");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71_Reading_ReadingId",
                table: "Rd71");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71_UserTask_UserTaskId",
                table: "Rd71");

            migrationBuilder.DropForeignKey(
                name: "FK_Rd71_Workout_WorkoutId",
                table: "Rd71");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeZone_Notice_NoticeId",
                table: "TimeZone");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingProgram_Workout_WorkoutId",
                table: "TrainingProgram");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_DevelopmentPlan_DevelopmentPlanId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workout",
                table: "Workout");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTask",
                table: "UserTask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFrequency",
                table: "UserFrequency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingProgram",
                table: "TrainingProgram");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeZone",
                table: "TimeZone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Recipe",
                table: "Recipe");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reading",
                table: "Reading");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rd71",
                table: "Rd71");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NoticeRepeat",
                table: "NoticeRepeat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notice",
                table: "Notice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EndRepeat",
                table: "EndRepeat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Diet",
                table: "Diet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DevelopmentPlan",
                table: "DevelopmentPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BuiltInFrequency",
                table: "BuiltInFrequency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Book",
                table: "Book");

            migrationBuilder.RenameTable(
                name: "Workout",
                newName: "Workouts");

            migrationBuilder.RenameTable(
                name: "UserTask",
                newName: "UserTasks");

            migrationBuilder.RenameTable(
                name: "UserFrequency",
                newName: "UserFrequencies");

            migrationBuilder.RenameTable(
                name: "TrainingProgram",
                newName: "TrainingPrograms");

            migrationBuilder.RenameTable(
                name: "TimeZone",
                newName: "TimeZones");

            migrationBuilder.RenameTable(
                name: "Reminder",
                newName: "Reminders");

            migrationBuilder.RenameTable(
                name: "Recipe",
                newName: "Recipes");

            migrationBuilder.RenameTable(
                name: "Reading",
                newName: "Readings");

            migrationBuilder.RenameTable(
                name: "Rd71",
                newName: "Rd71s");

            migrationBuilder.RenameTable(
                name: "NoticeRepeat",
                newName: "NoticeRepeats");

            migrationBuilder.RenameTable(
                name: "Notice",
                newName: "Notices");

            migrationBuilder.RenameTable(
                name: "EndRepeat",
                newName: "EndRepeats");

            migrationBuilder.RenameTable(
                name: "Diet",
                newName: "Diets");

            migrationBuilder.RenameTable(
                name: "DevelopmentPlan",
                newName: "DevelopmentPlans");

            migrationBuilder.RenameTable(
                name: "BuiltInFrequency",
                newName: "BuiltInFrequencies");

            migrationBuilder.RenameTable(
                name: "Book",
                newName: "Books");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingProgram_WorkoutId",
                table: "TrainingPrograms",
                newName: "IX_TrainingPrograms_WorkoutId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeZone_NoticeId",
                table: "TimeZones",
                newName: "IX_TimeZones_NoticeId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71_WorkoutId",
                table: "Rd71s",
                newName: "IX_Rd71s_WorkoutId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71_UserTaskId",
                table: "Rd71s",
                newName: "IX_Rd71s_UserTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71_ReadingId",
                table: "Rd71s",
                newName: "IX_Rd71s_ReadingId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71_DietId",
                table: "Rd71s",
                newName: "IX_Rd71s_DietId");

            migrationBuilder.RenameIndex(
                name: "IX_Rd71_ChallengeId",
                table: "Rd71s",
                newName: "IX_Rd71s_ChallengeId");

            migrationBuilder.RenameIndex(
                name: "IX_NoticeRepeat_UserFrequencyId",
                table: "NoticeRepeats",
                newName: "IX_NoticeRepeats_UserFrequencyId");

            migrationBuilder.RenameIndex(
                name: "IX_NoticeRepeat_EndRepeatId",
                table: "NoticeRepeats",
                newName: "IX_NoticeRepeats_EndRepeatId");

            migrationBuilder.RenameIndex(
                name: "IX_NoticeRepeat_BuiltInFrequencyId",
                table: "NoticeRepeats",
                newName: "IX_NoticeRepeats_BuiltInFrequencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Notice_RepeatId",
                table: "Notices",
                newName: "IX_Notices_RepeatId");

            migrationBuilder.RenameIndex(
                name: "IX_Notice_GoalId",
                table: "Notices",
                newName: "IX_Notices_GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_Book_ReadingId",
                table: "Books",
                newName: "IX_Books_ReadingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workouts",
                table: "Workouts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTasks",
                table: "UserTasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFrequencies",
                table: "UserFrequencies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingPrograms",
                table: "TrainingPrograms",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeZones",
                table: "TimeZones",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Recipes",
                table: "Recipes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Readings",
                table: "Readings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rd71s",
                table: "Rd71s",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NoticeRepeats",
                table: "NoticeRepeats",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notices",
                table: "Notices",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EndRepeats",
                table: "EndRepeats",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Diets",
                table: "Diets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevelopmentPlans",
                table: "DevelopmentPlans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BuiltInFrequencies",
                table: "BuiltInFrequencies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Books",
                table: "Books",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "RecomendedBooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecomendedBooks", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BookReading_Books_ReadBooksDuringChallengeId",
                table: "BookReading",
                column: "ReadBooksDuringChallengeId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookReading_Readings_ReadingFinishedId",
                table: "BookReading",
                column: "ReadingFinishedId",
                principalTable: "Readings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Readings_ReadingId",
                table: "Books",
                column: "ReadingId",
                principalTable: "Readings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DietRecipe_Diets_FitDietsId",
                table: "DietRecipe",
                column: "FitDietsId",
                principalTable: "Diets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DietRecipe_Recipes_RecipesId",
                table: "DietRecipe",
                column: "RecipesId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeReminder_Notices_NoticesId",
                table: "NoticeReminder",
                column: "NoticesId",
                principalTable: "Notices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeReminder_Reminders_RemindersId",
                table: "NoticeReminder",
                column: "RemindersId",
                principalTable: "Reminders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeRepeats_BuiltInFrequencies_BuiltInFrequencyId",
                table: "NoticeRepeats",
                column: "BuiltInFrequencyId",
                principalTable: "BuiltInFrequencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeRepeats_EndRepeats_EndRepeatId",
                table: "NoticeRepeats",
                column: "EndRepeatId",
                principalTable: "EndRepeats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeRepeats_UserFrequencies_UserFrequencyId",
                table: "NoticeRepeats",
                column: "UserFrequencyId",
                principalTable: "UserFrequencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_Goals_GoalId",
                table: "Notices",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_NoticeRepeats_RepeatId",
                table: "Notices",
                column: "RepeatId",
                principalTable: "NoticeRepeats",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71s_Challenges_ChallengeId",
                table: "Rd71s",
                column: "ChallengeId",
                principalTable: "Challenges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71s_Diets_DietId",
                table: "Rd71s",
                column: "DietId",
                principalTable: "Diets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71s_Readings_ReadingId",
                table: "Rd71s",
                column: "ReadingId",
                principalTable: "Readings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71s_UserTasks_UserTaskId",
                table: "Rd71s",
                column: "UserTaskId",
                principalTable: "UserTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rd71s_Workouts_WorkoutId",
                table: "Rd71s",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeZones_Notices_NoticeId",
                table: "TimeZones",
                column: "NoticeId",
                principalTable: "Notices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPrograms_Workouts_WorkoutId",
                table: "TrainingPrograms",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_DevelopmentPlans_DevelopmentPlanId",
                table: "Users",
                column: "DevelopmentPlanId",
                principalTable: "DevelopmentPlans",
                principalColumn: "Id");
        }
    }
}
