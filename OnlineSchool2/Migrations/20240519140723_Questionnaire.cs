using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSchool2.Migrations
{
    /// <inheritdoc />
    public partial class Questionnaire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BeginQuestionnaire",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndQuestionnaire",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeginQuestionnaire",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "EndQuestionnaire",
                table: "Courses");
        }
    }
}
