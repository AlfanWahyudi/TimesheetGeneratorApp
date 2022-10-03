using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetGeneratorApp.Migrations.Commit
{
    public partial class AddColumnAuthorEmailToTableCommitModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "author_email",
                table: "CommitModel",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "author_email",
                table: "CommitModel");
        }
    }
}
