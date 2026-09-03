using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OticaVisao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleReversalActor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reversed_by",
                table: "sales",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reversed_by",
                table: "sales");
        }
    }
}
