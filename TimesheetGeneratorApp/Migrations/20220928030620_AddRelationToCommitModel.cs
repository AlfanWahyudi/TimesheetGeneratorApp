using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TimesheetGeneratorApp.Migrations
{
    public partial class AddRelationToCommitModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "MasterProjectModel",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "CommitModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    author_name = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    committed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    jam_mulai = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    jam_akhir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    jumlah_jam = table.Column<int>(type: "integer", nullable: true),
                    MasterProjectModelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitModel_MasterProjectModel_MasterProjectModelId",
                        column: x => x.MasterProjectModelId,
                        principalTable: "MasterProjectModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitModel_MasterProjectModelId",
                table: "CommitModel",
                column: "MasterProjectModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitModel");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MasterProjectModel",
                newName: "id");
        }
    }
}
