using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkCustomerAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "account_user_id",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_account_user_id",
                table: "customers",
                column: "account_user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_customers_AspNetUsers_account_user_id",
                table: "customers",
                column: "account_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customers_AspNetUsers_account_user_id",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "ix_customers_account_user_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "account_user_id",
                table: "customers");
        }
    }
}
