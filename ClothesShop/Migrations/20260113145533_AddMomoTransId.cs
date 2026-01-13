using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothesShop.Migrations
{
    /// <inheritdoc />
    public partial class AddMomoTransId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MomoTransId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MomoTransId",
                table: "Orders");
        }
    }
}
