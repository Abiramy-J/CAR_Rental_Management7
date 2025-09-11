using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Rental_Management.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCarModelColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "CarModels");

            migrationBuilder.AddColumn<string>(
                name: "BodyType",
                table: "CarModels",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "CarModels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DailyRentPrice",
                table: "CarModels",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "CarModels",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "EngineCapacity",
                table: "CarModels",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FuelType",
                table: "CarModels",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SeatingCapacity",
                table: "CarModels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Transmission",
                table: "CarModels",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "CarModels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyType",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "DailyRentPrice",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "EngineCapacity",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "FuelType",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "SeatingCapacity",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "Transmission",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "CarModels");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "CarModels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
