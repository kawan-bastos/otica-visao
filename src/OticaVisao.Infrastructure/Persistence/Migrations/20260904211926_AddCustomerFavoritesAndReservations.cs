using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861 // Migrations are immutable generated history.

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFavoritesAndReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "reserved_quantity",
                table: "frames",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "frame_favorites",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    frame_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frame_favorites", x => x.id);
                    table.ForeignKey(
                        name: "FK_frame_favorites_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_frame_favorites_frames_frame_id",
                        column: x => x.frame_id,
                        principalTable: "frames",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "frame_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    frame_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    visit_period = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sale_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frame_reservations", x => x.id);
                    table.ForeignKey(
                        name: "FK_frame_reservations_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_frame_reservations_frames_frame_id",
                        column: x => x.frame_id,
                        principalTable: "frames",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_frame_reservations_sales_sale_id",
                        column: x => x.sale_id,
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_frame_favorites_customer_id_frame_id",
                table: "frame_favorites",
                columns: new[] { "customer_id", "frame_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_frame_favorites_frame_id",
                table: "frame_favorites",
                column: "frame_id");

            migrationBuilder.CreateIndex(
                name: "IX_frame_reservations_customer_id_status",
                table: "frame_reservations",
                columns: new[] { "customer_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_frame_reservations_frame_id_status",
                table: "frame_reservations",
                columns: new[] { "frame_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_frame_reservations_sale_id",
                table: "frame_reservations",
                column: "sale_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "frame_favorites");

            migrationBuilder.DropTable(
                name: "frame_reservations");

            migrationBuilder.DropColumn(
                name: "reserved_quantity",
                table: "frames");
        }
    }
}
#pragma warning restore CA1861
