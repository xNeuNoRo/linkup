using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUpPro.Infrastructure.Persistence.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameRelatedPostIdToRelatedEntityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Posts_RelatedPostId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "RelatedPostId",
                table: "Notifications",
                newName: "RelatedEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RelatedPostId",
                table: "Notifications",
                newName: "IX_Notifications_RelatedEntityId");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Reactions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Posts_RelatedEntityId",
                table: "Notifications",
                column: "RelatedEntityId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Posts_RelatedEntityId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Reactions");

            migrationBuilder.RenameColumn(
                name: "RelatedEntityId",
                table: "Notifications",
                newName: "RelatedPostId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RelatedEntityId",
                table: "Notifications",
                newName: "IX_Notifications_RelatedPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Posts_RelatedPostId",
                table: "Notifications",
                column: "RelatedPostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
