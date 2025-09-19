using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Rental_Management.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "FullName", "Password", "PhoneNumber", "ProfileImageUrl", "Role", "Username" },
                values: new object[] { 1, "admin@gmail.com", "System Administrator", "admin123", "0000000000", "/images/default-profile.png", "Admin", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1);
        }
    }
}
