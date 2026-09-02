using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerPersonalData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address_complement",
                table: "customers",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_number",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "birth_date",
                table: "customers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cpf",
                table: "customers",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "neighborhood",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postal_code",
                table: "customers",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "customers",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street",
                table: "customers",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_cpf",
                table: "customers",
                column: "cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_customers_cpf",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "address_complement",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "address_number",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "birth_date",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "city",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "cpf",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "neighborhood",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "postal_code",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "state",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "street",
                table: "customers");
        }
    }
}
