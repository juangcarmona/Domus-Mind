using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomusMind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalCalendarIngestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "external_calendar_connections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    provider_account_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    account_display_label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    forward_horizon_days = table.Column<int>(type: "integer", nullable: false),
                    scheduled_refresh_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    scheduled_refresh_interval_minutes = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    last_successful_sync_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sync_attempt_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sync_failure_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    next_scheduled_sync_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sync_lease_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sync_lease_expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    access_token_expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cached_access_token = table.Column<string>(type: "text", nullable: true),
                    encrypted_refresh_token = table.Column<string>(type: "text", nullable: true),
                    granted_scopes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_calendar_connections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_calendar_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feed_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    external_event_id = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ical_uid = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    series_master_id = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    starts_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    original_timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_all_day = table.Column<bool>(type: "boolean", nullable: false),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    participant_summary_json = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    raw_payload_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    provider_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    open_in_provider_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    last_seen_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_calendar_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_calendar_feeds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_calendar_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    calendar_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_selected = table.Column<bool>(type: "boolean", nullable: false),
                    window_start_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    window_end_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_delta_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    last_successful_sync_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_calendar_feeds", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_calendar_feeds_external_calendar_connections_conne~",
                        column: x => x.connection_id,
                        principalTable: "external_calendar_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_external_calendar_connections_lease_expires",
                table: "external_calendar_connections",
                column: "sync_lease_expires_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_external_calendar_connections_member_provider",
                table: "external_calendar_connections",
                columns: new[] { "member_id", "provider" });

            migrationBuilder.CreateIndex(
                name: "ix_external_calendar_connections_next_sync",
                table: "external_calendar_connections",
                column: "next_scheduled_sync_utc");

            migrationBuilder.CreateIndex(
                name: "ix_external_calendar_entries_connection_starts",
                table: "external_calendar_entries",
                columns: new[] { "connection_id", "starts_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_external_calendar_entries_feed_event_unique",
                table: "external_calendar_entries",
                columns: new[] { "feed_id", "external_event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_calendar_entries_feed_ical_uid",
                table: "external_calendar_entries",
                columns: new[] { "feed_id", "ical_uid" });

            migrationBuilder.CreateIndex(
                name: "ix_external_calendar_entries_feed_window",
                table: "external_calendar_entries",
                columns: new[] { "feed_id", "starts_at_utc", "ends_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_external_calendar_feeds_connection_calendar_unique",
                table: "external_calendar_feeds",
                columns: new[] { "connection_id", "provider_calendar_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_calendar_feeds_connection_selected",
                table: "external_calendar_feeds",
                columns: new[] { "connection_id", "is_selected" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_calendar_entries");

            migrationBuilder.DropTable(
                name: "external_calendar_feeds");

            migrationBuilder.DropTable(
                name: "external_calendar_connections");
        }
    }
}
