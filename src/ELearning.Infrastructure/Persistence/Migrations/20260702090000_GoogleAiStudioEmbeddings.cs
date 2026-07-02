using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations;

[Migration("20260702090000_GoogleAiStudioEmbeddings")]
public partial class GoogleAiStudioEmbeddings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_ai_knowledge_chunks_embedding_vector_cosine;");

        migrationBuilder.Sql(
            """
            UPDATE ai_knowledge_chunks
            SET embedding_vector = NULL;
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE ai_knowledge_chunks
            ALTER COLUMN embedding_vector TYPE vector(768)
            USING NULL::vector(768);
            """);

        migrationBuilder.Sql(
            """
            CREATE INDEX IF NOT EXISTS ix_ai_knowledge_chunks_embedding_vector_cosine
            ON ai_knowledge_chunks
            USING ivfflat (embedding_vector vector_cosine_ops)
            WITH (lists = 100);
            """);

        migrationBuilder.CreateTable(
            name: "ai_query_embedding_cache",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                query_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                normalized_query = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                dimensions = table.Column<int>(type: "integer", nullable: false),
                embedding_json = table.Column<string>(type: "jsonb", nullable: false),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ai_query_embedding_cache", x => x.id);
            });

        migrationBuilder.Sql(
            """
            ALTER TABLE ai_query_embedding_cache
            ADD COLUMN embedding_vector vector(768);
            """);

        migrationBuilder.CreateIndex(
            name: "IX_ai_query_embedding_cache_expires_at",
            table: "ai_query_embedding_cache",
            column: "expires_at");

        migrationBuilder.CreateIndex(
            name: "IX_ai_query_embedding_cache_query_hash_provider_model_dimensions",
            table: "ai_query_embedding_cache",
            columns: new[] { "query_hash", "provider", "model", "dimensions" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ai_query_embedding_cache");

        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_ai_knowledge_chunks_embedding_vector_cosine;");

        migrationBuilder.Sql(
            """
            UPDATE ai_knowledge_chunks
            SET embedding_vector = NULL;
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE ai_knowledge_chunks
            ALTER COLUMN embedding_vector TYPE vector(384)
            USING NULL::vector(384);
            """);

        migrationBuilder.Sql(
            """
            CREATE INDEX IF NOT EXISTS ix_ai_knowledge_chunks_embedding_vector_cosine
            ON ai_knowledge_chunks
            USING ivfflat (embedding_vector vector_cosine_ops)
            WITH (lists = 100);
            """);
    }
}
