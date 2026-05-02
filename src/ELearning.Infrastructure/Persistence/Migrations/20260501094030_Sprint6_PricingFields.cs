using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint6_PricingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "training_classes",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<long>(
                name: "price_cents",
                table: "training_classes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "license_pools",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<long>(
                name: "seat_price_cents",
                table: "license_pools",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "courses",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<long>(
                name: "price_cents",
                table: "courses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "currency",
                table: "training_classes");

            migrationBuilder.DropColumn(
                name: "price_cents",
                table: "training_classes");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "license_pools");

            migrationBuilder.DropColumn(
                name: "seat_price_cents",
                table: "license_pools");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "price_cents",
                table: "courses");
        }
    }
}
