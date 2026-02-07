using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HolyBeats.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGenreAndLanguageToTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Tracks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Tracks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Tracks");
        }
    }
}
