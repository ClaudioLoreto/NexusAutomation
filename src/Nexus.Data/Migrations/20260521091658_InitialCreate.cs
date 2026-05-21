using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nexus.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Niches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScriptTone = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ElevenLabsVoiceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MusicDirectory = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    QueuePriority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Niches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NicheId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ScriptText = table.Column<string>(type: "text", nullable: true),
                    MediaTagsJson = table.Column<string>(type: "text", nullable: true),
                    LocalMediaPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    OutputPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_Niches_NicheId",
                        column: x => x.NicheId,
                        principalTable: "Niches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trends",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NicheId = table.Column<int>(type: "integer", nullable: true),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompetitorChannelId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CompetitorVideoId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ViewCount = table.Column<long>(type: "bigint", nullable: false),
                    ViewsPerHour = table.Column<double>(type: "double precision", nullable: false),
                    MeasuredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawPayloadJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trends_Niches_NicheId",
                        column: x => x.NicheId,
                        principalTable: "Niches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Trends_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Niches",
                columns: new[] { "Id", "ElevenLabsVoiceId", "IsActive", "MusicDirectory", "Name", "QueuePriority", "ScriptTone", "Type" },
                values: new object[,]
                {
                    { 1, "", true, "Assets/Music/Finance", "Finance", 100, "Formal, authoritative", 1 },
                    { 2, "", true, "Assets/Music/Tech", "Tech & AI", 100, "Dynamic, enthusiastic", 2 },
                    { 3, "", true, "Assets/Music/Legal", "Legal & Court", 100, "Narrative, dramatic pauses", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Niches_Type",
                table: "Niches",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trends_NicheId_MeasuredAtUtc",
                table: "Trends",
                columns: new[] { "NicheId", "MeasuredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Trends_VideoId",
                table: "Trends",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_NicheId",
                table: "Videos",
                column: "NicheId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trends");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "Niches");
        }
    }
}
