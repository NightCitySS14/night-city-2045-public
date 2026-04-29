using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class LobbyRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nc_meta_inventory",
                columns: table => new
                {
                    nc_meta_inventory_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    item_prototype = table.Column<string>(type: "TEXT", nullable: false),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    selected = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nc_meta_inventory", x => x.nc_meta_inventory_id);
                });

            migrationBuilder.CreateTable(
                name: "nc_player_economy",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    night_coins_balance = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nc_player_economy", x => x.user_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nc_meta_inventory_user_id",
                table: "nc_meta_inventory",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nc_meta_inventory");

            migrationBuilder.DropTable(
                name: "nc_player_economy");
        }
    }
}
