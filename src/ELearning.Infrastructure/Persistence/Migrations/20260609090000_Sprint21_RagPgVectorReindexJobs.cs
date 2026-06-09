using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations;

[Migration("20260609090000_Sprint21_RagPgVectorReindexJobs")]
public partial class Sprint21_RagPgVectorReindexJobs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

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

        migrationBuilder.Sql(
            """
            ALTER TABLE ai_knowledge_chunks
            ADD COLUMN IF NOT EXISTS embedding_vector vector(384);
            """);

        migrationBuilder.Sql(
            """
            CREATE INDEX IF NOT EXISTS ix_ai_knowledge_chunks_embedding_vector_cosine
            ON ai_knowledge_chunks
            USING ivfflat (embedding_vector vector_cosine_ops)
            WITH (lists = 100);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_ai_knowledge_chunks_embedding_vector_cosine;");
        migrationBuilder.Sql("ALTER TABLE ai_knowledge_chunks DROP COLUMN IF EXISTS embedding_vector;");
        migrationBuilder.DropTable(name: "ai_knowledge_reindex_jobs");
    }
}
