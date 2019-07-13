using Microsoft.EntityFrameworkCore.Migrations;

namespace evorace.WebApp.Data.Migrations
{
    public partial class AddRunnerRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "68df6f0d-7056-4399-9e11-0a5a4ec2c5cb",
                column: "NormalizedName",
                value: "ADMIN");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "fc7b455a-6a16-4f4e-8e59-626d0727cd7a", "00431db5-df2c-4502-8e33-649df720b220", "Runner", "RUNNER" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fc7b455a-6a16-4f4e-8e59-626d0727cd7a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "68df6f0d-7056-4399-9e11-0a5a4ec2c5cb",
                column: "NormalizedName",
                value: null);
        }
    }
}
