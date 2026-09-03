using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionsAndLaboratoryDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "far_left_addition",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "far_left_axis",
                table: "sale_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "far_left_cylinder",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "far_left_dnp",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "far_left_height",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "far_left_sphere",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "far_right_addition",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "far_right_axis",
                table: "sale_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "far_right_cylinder",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "far_right_dnp",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "far_right_height",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "far_right_sphere",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_left_addition",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "near_left_axis",
                table: "sale_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_left_cylinder",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_left_dnp",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_left_height",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_left_sphere",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_right_addition",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "near_right_axis",
                table: "sale_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_right_cylinder",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_right_dnp",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_right_height",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "near_right_sphere",
                table: "sale_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "document_file_name",
                table: "laboratory_orders",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "far_left_addition",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_left_axis",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_left_cylinder",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_left_dnp",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_left_height",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_left_sphere",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_right_addition",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_right_axis",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_right_cylinder",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_right_dnp",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_right_height",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "far_right_sphere",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_left_addition",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_left_axis",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_left_cylinder",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_left_dnp",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_left_height",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_left_sphere",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_right_addition",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_right_axis",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_right_cylinder",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_right_dnp",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_right_height",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "near_right_sphere",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "document_file_name",
                table: "laboratory_orders");
        }
    }
}
