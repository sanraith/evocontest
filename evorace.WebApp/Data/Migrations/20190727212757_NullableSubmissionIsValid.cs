using Microsoft.EntityFrameworkCore.Migrations;

namespace evorace.WebApp.Data.Migrations
{
    public partial class NullableSubmissionIsValid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsValid",
                table: "Submissions",
                nullable: true,
                oldClrType: typeof(bool));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsValid",
                table: "Submissions",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);
        }
    }
}
