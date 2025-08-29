using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameOfLife.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoardStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GridData = table.Column<string>(type: "TEXT", nullable: false),
                    Rows = table.Column<int>(type: "INTEGER", nullable: false),
                    Columns = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Generation = table.Column<int>(type: "INTEGER", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardStateHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BoardStateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GridData = table.Column<string>(type: "TEXT", nullable: false),
                    Generation = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardStateHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardStateHistory_BoardStates_BoardStateId",
                        column: x => x.BoardStateId,
                        principalTable: "BoardStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardStateHistory_BoardStateId",
                table: "BoardStateHistory",
                column: "BoardStateId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardStateHistory_CreatedAt",
                table: "BoardStateHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BoardStateHistory_Generation",
                table: "BoardStateHistory",
                column: "Generation");

            migrationBuilder.CreateIndex(
                name: "IX_BoardStates_CreatedAt",
                table: "BoardStates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BoardStates_LastModifiedAt",
                table: "BoardStates",
                column: "LastModifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardStateHistory");

            migrationBuilder.DropTable(
                name: "BoardStates");
        }
    }
}
