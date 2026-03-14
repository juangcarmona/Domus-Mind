using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomusMind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventLog_Expand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "event_log",
                newName: "PayloadJson");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "event_log",
                newName: "EventId");

            migrationBuilder.AddColumn<string>(
                name: "AggregateId",
                table: "event_log",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AggregateType",
                table: "event_log",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CausationId",
                table: "event_log",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "event_log",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Module",
                table: "event_log",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "event_log",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_event_log_AggregateType_AggregateId_Version",
                table: "event_log",
                columns: new[] { "AggregateType", "AggregateId", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_event_log_Module_OccurredAtUtc",
                table: "event_log",
                columns: new[] { "Module", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_log_AggregateType_AggregateId_Version",
                table: "event_log");

            migrationBuilder.DropIndex(
                name: "IX_event_log_Module_OccurredAtUtc",
                table: "event_log");

            migrationBuilder.DropColumn(
                name: "AggregateId",
                table: "event_log");

            migrationBuilder.DropColumn(
                name: "AggregateType",
                table: "event_log");

            migrationBuilder.DropColumn(
                name: "CausationId",
                table: "event_log");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "event_log");

            migrationBuilder.DropColumn(
                name: "Module",
                table: "event_log");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "event_log");

            migrationBuilder.RenameColumn(
                name: "PayloadJson",
                table: "event_log",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "event_log",
                newName: "Id");
        }
    }
}
