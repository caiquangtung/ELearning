using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPgVectorColumnViaRawSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

            migrationBuilder.Sql("ALTER TABLE ai_knowledge_chunks ADD COLUMN IF NOT EXISTS embedding_vector vector(768);");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ix_ai_knowledge_chunks_embedding_vector 
                ON ai_knowledge_chunks 
                USING ivfflat (embedding_vector vector_cosine_ops) 
                WITH (lists = 100);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_ai_knowledge_chunks_embedding_vector;");
            migrationBuilder.Sql("ALTER TABLE ai_knowledge_chunks DROP COLUMN IF EXISTS embedding_vector;");
        }
    }
}
