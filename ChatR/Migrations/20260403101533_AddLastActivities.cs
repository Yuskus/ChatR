using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatR.Migrations
{
    /// <inheritdoc />
    public partial class AddLastActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_login",
                table: "users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_message",
                table: "rooms",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_login",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_message",
                table: "rooms");
        }
    }
}
