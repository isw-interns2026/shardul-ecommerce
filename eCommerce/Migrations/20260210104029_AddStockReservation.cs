using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddStockReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservedCount",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_Reserved_Non_Negative",
                table: "Products",
                sql: "\"ReservedCount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_Reserved_Within_Stock",
                table: "Products",
                sql: "\"ReservedCount\" <= \"CountInStock\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_Reserved_Non_Negative",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_Reserved_Within_Stock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReservedCount",
                table: "Products");
        }
    }
}
