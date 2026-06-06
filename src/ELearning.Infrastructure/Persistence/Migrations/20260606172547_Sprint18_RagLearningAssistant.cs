using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint18_RagLearningAssistant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_chat_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_chat_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ai_knowledge_chunks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lesson_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    course_title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    section_title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    lesson_title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    chunk_index = table.Column<int>(type: "integer", nullable: false),
                    content_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    embedding_json = table.Column<string>(type: "jsonb", nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_knowledge_chunks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ai_chat_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    citations_json = table.Column<string>(type: "jsonb", nullable: false),
                    provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    prompt_version = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    used_context = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_ai_chat_messages_ai_chat_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "ai_chat_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_chat_messages_session_id",
                table: "ai_chat_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_chat_messages_session_id_created_at",
                table: "ai_chat_messages",
                columns: new[] { "session_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ai_chat_sessions_user_id",
                table: "ai_chat_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_chat_sessions_user_id_updated_at",
                table: "ai_chat_sessions",
                columns: new[] { "user_id", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ai_knowledge_chunks_content_hash",
                table: "ai_knowledge_chunks",
                column: "content_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_knowledge_chunks_course_id",
                table: "ai_knowledge_chunks",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_knowledge_chunks_course_id_source_type_chunk_index",
                table: "ai_knowledge_chunks",
                columns: new[] { "course_id", "source_type", "chunk_index" });

            migrationBuilder.CreateIndex(
                name: "IX_ai_knowledge_chunks_lesson_id",
                table: "ai_knowledge_chunks",
                column: "lesson_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_chat_messages");

            migrationBuilder.DropTable(
                name: "ai_knowledge_chunks");

            migrationBuilder.DropTable(
                name: "ai_chat_sessions");
        }
    }
}
