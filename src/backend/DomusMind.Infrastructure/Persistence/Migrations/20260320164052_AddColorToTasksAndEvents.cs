using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomusMind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddColorToTasksAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "household_tasks",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#3B82F6");

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "calendar_events",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#3B82F6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color",
                table: "household_tasks");

            migrationBuilder.DropColumn(
                name: "color",
                table: "calendar_events");
        }
    }
}
