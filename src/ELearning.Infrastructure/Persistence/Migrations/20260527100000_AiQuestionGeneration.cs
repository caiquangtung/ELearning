using System;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260527100000_AiQuestionGeneration")]
public partial class AiQuestionGeneration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ai_request_logs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                feature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                prompt_version = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                input_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                token_estimate = table.Column<int>(type: "integer", nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ai_request_logs", x => x.id));

        migrationBuilder.CreateIndex(
            name: "IX_ai_request_logs_feature_created_at",
            table: "ai_request_logs",
            columns: new[] { "feature", "created_at" });

        migrationBuilder.CreateIndex(
            name: "IX_ai_request_logs_input_hash",
            table: "ai_request_logs",
            column: "input_hash");

        migrationBuilder.CreateIndex(
            name: "IX_ai_request_logs_user_id",
            table: "ai_request_logs",
            column: "user_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ai_request_logs");
    }
}
