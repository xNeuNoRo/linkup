using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUpPro.Infrastructure.Persistence.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Reaction_Unique_Index_Filter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El modelo EF Core espera un índice único filtrado WHERE [DeletedAt] IS NULL,
            // pero la migración inicial creó el índice sin filtro:
            //   CREATE UNIQUE INDEX [UX_Reactions_Post_User] ON [Reactions] ([PostId], [UserId])
            //
            // Esto impide que un mismo usuario pueda reaccionar a un post después de
            // eliminar/quitar su reacción anterior (soft delete).
            // Se debe recrear el índice con el filtro esperado.

            migrationBuilder.DropIndex(
                name: "UX_Reactions_Post_User",
                table: "Reactions");

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

            migrationBuilder.CreateIndex(
                name: "UX_Reactions_Post_User",
                table: "Reactions",
                columns: new[] { "PostId", "UserId" },
                unique: true);
        }
    }
}
