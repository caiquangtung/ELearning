using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations;

public partial class Sprint9_Certificates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "certificate_templates",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                html_template = table.Column<string>(type: "text", nullable: false),
                is_default = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_certificate_templates", x => x.id));

        migrationBuilder.CreateTable(
            name: "certificates",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                course_id = table.Column<Guid>(type: "uuid", nullable: false),
                training_class_id = table.Column<Guid>(type: "uuid", nullable: true),
                quiz_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                certificate_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                verification_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                learner_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                course_title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                attendance_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                progress_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                quiz_passed = table.Column<bool>(type: "boolean", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                revocation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_certificates", x => x.id));

        migrationBuilder.CreateIndex("IX_certificate_templates_name", "certificate_templates", "name", unique: true);
        migrationBuilder.CreateIndex("IX_certificates_certificate_number", "certificates", "certificate_number", unique: true);
        migrationBuilder.CreateIndex("IX_certificates_user_id_course_id", "certificates", new[] { "user_id", "course_id" }, unique: true);
        migrationBuilder.CreateIndex("IX_certificates_verification_code", "certificates", "verification_code", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "certificates");
        migrationBuilder.DropTable(name: "certificate_templates");
    }
}
