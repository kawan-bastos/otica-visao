using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleReversals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reversal_reason",
                table: "sales",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reversed_at_utc",
                table: "sales",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reversal_reason",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "reversed_at_utc",
                table: "sales");
        }
    }
}
