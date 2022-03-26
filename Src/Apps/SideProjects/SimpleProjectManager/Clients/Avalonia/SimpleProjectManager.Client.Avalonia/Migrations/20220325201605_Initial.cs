using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleProjectManager.Client.Avalonia.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Data",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Data", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Timeout",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    DataKey = table.Column<string>(type: "TEXT", nullable: false),
                    Timeout = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timeout", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Data");

            migrationBuilder.DropTable(
                name: "Timeout");
        }
    }
}
