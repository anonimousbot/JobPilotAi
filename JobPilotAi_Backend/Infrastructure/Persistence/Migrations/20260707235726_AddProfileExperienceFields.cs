using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPilotAi_Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileExperienceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "current_job_title",
                schema: "applyai",
                table: "profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "years_of_experience",
                schema: "applyai",
                table: "profiles",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_job_title",
                schema: "applyai",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "years_of_experience",
                schema: "applyai",
                table: "profiles");
        }
    }
}
