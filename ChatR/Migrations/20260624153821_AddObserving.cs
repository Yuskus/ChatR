using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChatR.Migrations
{
    /// <inheritdoc />
    public partial class AddObserving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "observing",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    user_from_id = table.Column<int>(type: "integer", nullable: false),
                    user_to_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_observing", x => x.id);
                    table.ForeignKey(
                        name: "fk_observing_users_user_from_id",
                        column: x => x.user_from_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_observing_users_user_to_id",
                        column: x => x.user_to_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_observing_user_from_id_user_to_id",
                table: "observing",
                columns: new[] { "user_from_id", "user_to_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_observing_user_to_id",
                table: "observing",
                column: "user_to_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "observing");
        }
    }
}
