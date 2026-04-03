using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMoviesApp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DigitalRetailers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalRetailers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovieFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieFormats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMovies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TmdbId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    ReleaseDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PosterPath = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMovies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMovies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMovieDigitalRetailers",
                columns: table => new
                {
                    UserMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    DigitalRetailerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMovieDigitalRetailers", x => new { x.UserMovieId, x.DigitalRetailerId });
                    table.ForeignKey(
                        name: "FK_UserMovieDigitalRetailers_DigitalRetailers_DigitalRetailerId",
                        column: x => x.DigitalRetailerId,
                        principalTable: "DigitalRetailers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMovieDigitalRetailers_UserMovies_UserMovieId",
                        column: x => x.UserMovieId,
                        principalTable: "UserMovies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMovieFormats",
                columns: table => new
                {
                    UserMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieFormatId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMovieFormats", x => new { x.UserMovieId, x.MovieFormatId });
                    table.ForeignKey(
                        name: "FK_UserMovieFormats_MovieFormats_MovieFormatId",
                        column: x => x.MovieFormatId,
                        principalTable: "MovieFormats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMovieFormats_UserMovies_UserMovieId",
                        column: x => x.UserMovieId,
                        principalTable: "UserMovies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMovieDigitalRetailers_DigitalRetailerId",
                table: "UserMovieDigitalRetailers",
                column: "DigitalRetailerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMovieFormats_MovieFormatId",
                table: "UserMovieFormats",
                column: "MovieFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMovies_UserId_TmdbId",
                table: "UserMovies",
                columns: new[] { "UserId", "TmdbId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMovieDigitalRetailers");

            migrationBuilder.DropTable(
                name: "UserMovieFormats");

            migrationBuilder.DropTable(
                name: "DigitalRetailers");

            migrationBuilder.DropTable(
                name: "MovieFormats");

            migrationBuilder.DropTable(
                name: "UserMovies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
