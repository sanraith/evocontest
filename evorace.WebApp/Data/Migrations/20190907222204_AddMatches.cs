using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace evorace.WebApp.Data.Migrations
{
    public partial class AddMatches : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 36, nullable: false),
                    MatchDate = table.Column<DateTime>(nullable: false),
                    JsonResult = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 36, nullable: false),
                    MatchId = table.Column<string>(nullable: false),
                    SubmissionId = table.Column<string>(nullable: false),
                    JsonResult = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measurements_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Measurements_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_MatchId",
                table: "Measurements",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_SubmissionId",
                table: "Measurements",
                column: "SubmissionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Measurements");

            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}
