using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2_CoursesAndContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "course_sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_sections_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_lessons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_lessons", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_lessons_course_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "course_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_assets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    file_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    content_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_assets", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_assets_course_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "course_lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_content_assets_lesson_id",
                table: "content_assets",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_assets_storage_key",
                table: "content_assets",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_lessons_section_id_sort_order",
                table: "course_lessons",
                columns: new[] { "section_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_sections_course_id_sort_order",
                table: "course_sections",
                columns: new[] { "course_id", "sort_order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_assets");

            migrationBuilder.DropTable(
                name: "course_lessons");

            migrationBuilder.DropTable(
                name: "course_sections");

            migrationBuilder.DropTable(
                name: "courses");
        }
    }
}
