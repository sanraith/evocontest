using Microsoft.EntityFrameworkCore.Migrations;

namespace evorace.WebApp.Data.Migrations
{
    public partial class AddRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "68df6f0d-7056-4399-9e11-0a5a4ec2c5cb", "21e4b3f3-b26b-4b20-9f95-3b4cb87e53f0", "Admin", "ADMIN"});
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "68df6f0d-7056-4399-9e11-0a5a4ec2c5cb");
        }
    }
}
