using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861 // Migrations are immutable generated history.

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HardenFrameReservationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "ck_frames_reserved_quantity",
                table: "frames",
                sql: "reserved_quantity >= 0 AND reserved_quantity <= stock_quantity");

            migrationBuilder.CreateIndex(
                name: "ux_frame_reservations_active_customer_frame",
                table: "frame_reservations",
                columns: new[] { "customer_id", "frame_id" },
                unique: true,
                filter: "status = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_frames_reserved_quantity",
                table: "frames");

            migrationBuilder.DropIndex(
                name: "ux_frame_reservations_active_customer_frame",
                table: "frame_reservations");
        }
    }
}
#pragma warning restore CA1861
