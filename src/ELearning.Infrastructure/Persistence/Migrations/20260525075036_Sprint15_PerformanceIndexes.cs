using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint15_PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_training_classes_course_id_status",
                table: "training_classes",
                columns: new[] { "course_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_training_classes_status_created_at",
                table: "training_classes",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_applied_coupon_code",
                table: "orders",
                column: "applied_coupon_code");

            migrationBuilder.CreateIndex(
                name: "IX_orders_checkout_expires_at",
                table: "orders",
                column: "checkout_expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_orders_status_created_at",
                table: "orders",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_order_items_item_type_reference_id",
                table: "order_items",
                columns: new[] { "item_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "IX_courses_price_cents_created_at",
                table: "courses",
                columns: new[] { "price_cents", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_courses_status_created_at",
                table: "courses",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_courses_title",
                table: "courses",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "IX_certificates_course_id_status",
                table: "certificates",
                columns: new[] { "course_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_certificates_user_id_status",
                table: "certificates",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_training_classes_course_id_status",
                table: "training_classes");

            migrationBuilder.DropIndex(
                name: "IX_training_classes_status_created_at",
                table: "training_classes");

            migrationBuilder.DropIndex(
                name: "IX_orders_applied_coupon_code",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_checkout_expires_at",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_status_created_at",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_order_items_item_type_reference_id",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "IX_courses_price_cents_created_at",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_status_created_at",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_title",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_certificates_course_id_status",
                table: "certificates");

            migrationBuilder.DropIndex(
                name: "IX_certificates_user_id_status",
                table: "certificates");
        }
    }
}
