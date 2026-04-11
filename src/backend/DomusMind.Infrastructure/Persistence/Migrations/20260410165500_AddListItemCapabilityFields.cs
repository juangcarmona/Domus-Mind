using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomusMind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddListItemCapabilityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "due_date",
                table: "shared_list_items",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "importance",
                table: "shared_list_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reminder",
                table: "shared_list_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "repeat",
                table: "shared_list_items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "due_date",
                table: "shared_list_items");

            migrationBuilder.DropColumn(
                name: "importance",
                table: "shared_list_items");

            migrationBuilder.DropColumn(
                name: "reminder",
                table: "shared_list_items");

            migrationBuilder.DropColumn(
                name: "repeat",
                table: "shared_list_items");
        }
    }
}
