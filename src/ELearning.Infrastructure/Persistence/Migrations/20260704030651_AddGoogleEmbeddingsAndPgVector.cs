using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleEmbeddingsAndPgVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_ai_query_embedding_cache_expires_at",
                table: "ai_query_embedding_cache",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_ai_query_embedding_cache_query_hash_provider_model_dimensio~",
                table: "ai_query_embedding_cache",
                columns: new[] { "query_hash", "provider", "model", "dimensions" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_query_embedding_cache");
        }
    }
}
