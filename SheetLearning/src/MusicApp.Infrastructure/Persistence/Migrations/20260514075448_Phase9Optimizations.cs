using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase9Optimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sessions_user_id",
                table: "sessions");

            migrationBuilder.DropIndex(
                name: "IX_notifications_user_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_lesson_bookings_student_id",
                table: "lesson_bookings");

            migrationBuilder.DropIndex(
                name: "IX_lesson_bookings_teacher_id",
                table: "lesson_bookings");

            migrationBuilder.DropIndex(
                name: "IX_chat_messages_chat_id",
                table: "chat_messages");

            migrationBuilder.DropIndex(
                name: "IX_attempts_user_id",
                table: "attempts");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserExpiry",
                table: "sessions",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserArchivedCreated",
                table: "notifications",
                columns: new[] { "user_id", "is_archived", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonBookings_StudentStato",
                table: "lesson_bookings",
                columns: new[] { "student_id", "stato" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonBookings_TeacherStato",
                table: "lesson_bookings",
                columns: new[] { "teacher_id", "stato" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatCreated",
                table: "chat_messages",
                columns: new[] { "chat_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_UserExerciseCreated",
                table: "attempts",
                columns: new[] { "user_id", "exercise_type_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_UserExpiry",
                table: "sessions");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserArchivedCreated",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_LessonBookings_StudentStato",
                table: "lesson_bookings");

            migrationBuilder.DropIndex(
                name: "IX_LessonBookings_TeacherStato",
                table: "lesson_bookings");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ChatCreated",
                table: "chat_messages");

            migrationBuilder.DropIndex(
                name: "IX_Attempts_UserExerciseCreated",
                table: "attempts");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_user_id",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_bookings_student_id",
                table: "lesson_bookings",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_bookings_teacher_id",
                table: "lesson_bookings",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_chat_id",
                table: "chat_messages",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_attempts_user_id",
                table: "attempts",
                column: "user_id");
        }
    }
}
