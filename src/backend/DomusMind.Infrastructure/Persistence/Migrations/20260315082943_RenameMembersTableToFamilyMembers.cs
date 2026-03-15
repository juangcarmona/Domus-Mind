using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomusMind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameMembersTableToFamilyMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "members",
                newName: "family_members");

            migrationBuilder.RenameIndex(
                name: "IX_members_family_id",
                table: "family_members",
                newName: "IX_family_members_family_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "family_members",
                newName: "members");

            migrationBuilder.RenameIndex(
                name: "IX_family_members_family_id",
                table: "members",
                newName: "IX_members_family_id");
        }
    }
}
