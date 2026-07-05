using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingAiTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_knowledge_reindex_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    indexed_courses = table.Column<int>(type: "integer", nullable: false),
                    indexed_chunks = table.Column<int>(type: "integer", nullable: false),
                    deleted_stale_chunks = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_knowledge_reindex_jobs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_knowledge_reindex_jobs_course_id",
                table: "ai_knowledge_reindex_jobs",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_knowledge_reindex_jobs_created_at",
                table: "ai_knowledge_reindex_jobs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ai_knowledge_reindex_jobs_status_created_at",
                table: "ai_knowledge_reindex_jobs",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateTable(
                name: "ai_rag_evaluation_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dataset_version = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    total_cases = table.Column<int>(type: "integer", nullable: false),
                    passed_cases = table.Column<int>(type: "integer", nullable: false),
                    retrieval_hit_rate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    citation_validity_rate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    refusal_accuracy_rate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    groundedness_rate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_rag_evaluation_runs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_rag_evaluation_runs_created_at",
                table: "ai_rag_evaluation_runs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ai_rag_evaluation_runs_status_created_at",
                table: "ai_rag_evaluation_runs",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ai_rag_evaluation_runs");
            migrationBuilder.DropTable(name: "ai_knowledge_reindex_jobs");
        }
    }
}
