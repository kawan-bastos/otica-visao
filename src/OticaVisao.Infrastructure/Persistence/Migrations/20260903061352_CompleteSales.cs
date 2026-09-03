using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "cancelled_at_utc",
                table: "sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at_utc",
                table: "sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "final_total",
                table: "sales",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "installments",
                table: "sales",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "sales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancelled_at_utc",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "completed_at_utc",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "final_total",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "installments",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "sales");
        }
    }
}
