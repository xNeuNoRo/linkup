using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUpPro.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddActivationCooldownAndTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Identity",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "Identity",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivationEmailSentAt",
                schema: "Identity",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Identity",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastActivationEmailSentAt",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "Identity",
                table: "Users");
        }
    }
}
