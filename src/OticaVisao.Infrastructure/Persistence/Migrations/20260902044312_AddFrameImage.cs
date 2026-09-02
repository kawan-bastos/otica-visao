using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFrameImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_file_name",
                table: "frames",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "image_file_name",
                value: null);

            migrationBuilder.UpdateData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "image_file_name",
                value: null);

            migrationBuilder.UpdateData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "image_file_name",
                value: null);

            migrationBuilder.UpdateData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "image_file_name",
                value: null);

            migrationBuilder.UpdateData(
                table: "frames",
                keyColumn: "id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "image_file_name",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_file_name",
                table: "frames");
        }
    }
}
