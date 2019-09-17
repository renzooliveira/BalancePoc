using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BalancePoc.Migrations
{
    public partial class StartDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    dataHoraEvento = table.Column<DateTime>(nullable: true),
                    tipoEvento = table.Column<int>(nullable: false),
                    nome = table.Column<string>(nullable: false),
                    msg = table.Column<string>(nullable: false),
                    cor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
