using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TimesheetGeneratorApp.Migrations.Commit
{
    public partial class AddForeignKeyFromTableMasterProjectModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MasterProjectModelId",
                table: "CommitModel",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MasterProjectModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    project_id = table.Column<string>(type: "text", nullable: false),
                    version_control = table.Column<string>(type: "text", nullable: false),
                    host_url = table.Column<string>(type: "text", nullable: false),
                    accsess_token = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    password = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterProjectModel", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitModel_MasterProjectModelId",
                table: "CommitModel",
                column: "MasterProjectModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitModel_MasterProjectModel_MasterProjectModelId",
                table: "CommitModel",
                column: "MasterProjectModelId",
                principalTable: "MasterProjectModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommitModel_MasterProjectModel_MasterProjectModelId",
                table: "CommitModel");

            migrationBuilder.DropTable(
                name: "MasterProjectModel");

            migrationBuilder.DropIndex(
                name: "IX_CommitModel_MasterProjectModelId",
                table: "CommitModel");

            migrationBuilder.DropColumn(
                name: "MasterProjectModelId",
                table: "CommitModel");
        }
    }
}
