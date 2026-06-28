using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUpPro.Infrastructure.Persistence.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNotificationTypeToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Reactions_Post_User",
                table: "Reactions");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Notifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "FriendRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "BattleshipShips",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "BattleshipAttacks",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Reactions_Post_User",
                table: "Reactions",
                columns: new[] { "PostId", "UserId" },
                unique: true,
                filter: "[DeletedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Reactions_Post_User",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "FriendRequests");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BattleshipShips");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BattleshipAttacks");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "UX_Reactions_Post_User",
                table: "Reactions",
                columns: new[] { "PostId", "UserId" },
                unique: true);
        }
    }
}
