using Microsoft.EntityFrameworkCore.Migrations;

namespace evorace.WebApp.Data.Migrations
{
    public partial class CustomizedIdentityUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploadFolderName",
                table: "AspNetUsers",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadFolderName",
                table: "AspNetUsers");
        }
    }
}
