using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomusMind.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMealPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meal_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    week_start = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    applied_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    shopping_list_id = table.Column<Guid>(type: "uuid", nullable: true),
                    shopping_list_version = table.Column<int>(type: "integer", nullable: false),
                    last_derived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    affects_whole_household = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    prep_time_minutes = table.Column<int>(type: "integer", nullable: true),
                    cook_time_minutes = table.Column<int>(type: "integer", nullable: true),
                    servings = table.Column<int>(type: "integer", nullable: true),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    allowed_meal_types = table.Column<string>(type: "text", nullable: false),
                    tags = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "weekly_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "meal_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    meal_type = table.Column<int>(type: "integer", nullable: false),
                    meal_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meal_source_type = table.Column<int>(type: "integer", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: true),
                    free_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_optional = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    affects_whole_household = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_slots", x => x.id);
                    table.ForeignKey(
                        name: "FK_meal_slots_meal_plans_meal_plan_id",
                        column: x => x.meal_plan_id,
                        principalTable: "meal_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ingredients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingredients", x => x.id);
                    table.ForeignKey(
                        name: "FK_ingredients_recipes_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "recipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meal_slot_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    meal_type = table.Column<int>(type: "integer", nullable: false),
                    weekly_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meal_source_type = table.Column<int>(type: "integer", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: true),
                    free_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_optional = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_slot_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_meal_slot_templates_weekly_templates_weekly_template_id",
                        column: x => x.weekly_template_id,
                        principalTable: "weekly_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ingredients_recipe_id_name",
                table: "ingredients",
                columns: new[] { "recipe_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_meal_plans_family_id_week_start",
                table: "meal_plans",
                columns: new[] { "family_id", "week_start" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_meal_slot_templates_template_day_type",
                table: "meal_slot_templates",
                columns: new[] { "weekly_template_id", "day_of_week", "meal_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_meal_slots_plan_day_type",
                table: "meal_slots",
                columns: new[] { "meal_plan_id", "day_of_week", "meal_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recipes_family_id_name",
                table: "recipes",
                columns: new[] { "family_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_weekly_templates_family_id_name",
                table: "weekly_templates",
                columns: new[] { "family_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ingredients");

            migrationBuilder.DropTable(
                name: "meal_slot_templates");

            migrationBuilder.DropTable(
                name: "meal_slots");

            migrationBuilder.DropTable(
                name: "recipes");

            migrationBuilder.DropTable(
                name: "weekly_templates");

            migrationBuilder.DropTable(
                name: "meal_plans");
        }
    }
}
