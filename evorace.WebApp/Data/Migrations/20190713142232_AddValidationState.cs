using Microsoft.EntityFrameworkCore.Migrations;

namespace evorace.WebApp.Data.Migrations
{
    public partial class AddValidationState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ValidationState",
                table: "Submissions",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidationState",
                table: "Submissions");
        }
    }
}
