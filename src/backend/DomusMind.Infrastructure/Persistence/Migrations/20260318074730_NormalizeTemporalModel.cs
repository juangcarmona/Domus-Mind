using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomusMind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeTemporalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "start_time",
                table: "calendar_events");

            migrationBuilder.RenameColumn(
                name: "end_time",
                table: "calendar_events",
                newName: "event_end_time");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "due_date",
                table: "household_tasks",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "due_time",
                table: "household_tasks",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "task_schedule_kind",
                table: "household_tasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "event_end_time",
                table: "calendar_events",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "event_date",
                table: "calendar_events",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "event_end_date",
                table: "calendar_events",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "event_time",
                table: "calendar_events",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "event_time_kind",
                table: "calendar_events",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "due_time",
                table: "household_tasks");

            migrationBuilder.DropColumn(
                name: "task_schedule_kind",
                table: "household_tasks");

            migrationBuilder.DropColumn(
                name: "event_date",
                table: "calendar_events");

            migrationBuilder.DropColumn(
                name: "event_end_date",
                table: "calendar_events");

            migrationBuilder.DropColumn(
                name: "event_time",
                table: "calendar_events");

            migrationBuilder.DropColumn(
                name: "event_time_kind",
                table: "calendar_events");

            migrationBuilder.RenameColumn(
                name: "event_end_time",
                table: "calendar_events",
                newName: "end_time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "due_date",
                table: "household_tasks",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_time",
                table: "calendar_events",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_time",
                table: "calendar_events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
