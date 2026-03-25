using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomusMind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shared_lists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linked_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    linked_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_lists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shared_list_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    @checked = table.Column<bool>(name: "checked", type: "boolean", nullable: false, defaultValue: false),
                    quantity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_member_id = table.Column<Guid>(type: "uuid", nullable: true),
                    shared_list_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_list_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_shared_list_items_shared_lists_shared_list_id",
                        column: x => x.shared_list_id,
                        principalTable: "shared_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shared_list_items_shared_list_id",
                table: "shared_list_items",
                column: "shared_list_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_list_items");

            migrationBuilder.DropTable(
                name: "shared_lists");
        }
    }
}
