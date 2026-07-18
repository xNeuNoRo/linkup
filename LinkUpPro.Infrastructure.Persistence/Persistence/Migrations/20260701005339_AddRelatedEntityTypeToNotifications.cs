using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUpPro.Infrastructure.Persistence.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedEntityTypeToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Posts_RelatedEntityId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedEntityId",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedEntity",
                table: "Notifications",
                columns: new[] { "RelatedEntityType", "RelatedEntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedEntity",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedEntityId",
                table: "Notifications",
                column: "RelatedEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Posts_RelatedEntityId",
                table: "Notifications",
                column: "RelatedEntityId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
