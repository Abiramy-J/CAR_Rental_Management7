using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Rental_Management.Migrations
{
    /// <inheritdoc />
    public partial class logourl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "CarModels",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "CarModels");
        }
    }
}
