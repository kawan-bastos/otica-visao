using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Migration gerada usa matrizes constantes uma única vez

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "frames",
                columns: new[] { "id", "brand", "code", "color", "is_active", "is_published", "model", "price", "shape", "stock_quantity", "target_audience", "type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Linha Visão", "ARM-001", "Preto", true, true, "Clássica", 219m, "Square", 1, "Adult", "Prescription" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Linha Visão", "ARM-002", "Tartaruga", true, true, "Leve", 219m, "Round", 1, "Adult", "Prescription" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Linha Visão", "ARM-003", "Preto", true, true, "Solar", 219m, "Aviator", 1, "Adult", "Sunglasses" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Linha Visão", "ARM-004", "Azul", true, true, "Colorida", 219m, "Round", 1, "Child", "Prescription" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Linha Visão", "ARM-005", "Dourado", true, true, "Minimal", 219m, "Other", 1, "Adult", "Prescription" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));
        }
    }
}
