using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nexus.Data.Migrations
{
    /// <inheritdoc />
    public partial class EngineScaffold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalScriptInstructions",
                table: "Niches",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KaraokeBackgroundColor",
                table: "Niches",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KaraokeFillColor",
                table: "Niches",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KaraokeFontFamily",
                table: "Niches",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "KaraokeFontSize",
                table: "Niches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "KaraokeHighlightColor",
                table: "Niches",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "KaraokeHighlightFontSize",
                table: "Niches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "KaraokeOutlineColor",
                table: "Niches",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "KaraokeYPositionPercent",
                table: "Niches",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Niches",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxWords",
                table: "Niches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OverlayGifLoopCount",
                table: "Niches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OverlayGifPath",
                table: "Niches",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "OverlayGifPositionPercent",
                table: "Niches",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OverlayGifTailSeconds",
                table: "Niches",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TargetWordCount",
                table: "Niches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TtsSpeed",
                table: "Niches",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "TtsVoice",
                table: "Niches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VideoJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NicheId = table.Column<int>(type: "integer", nullable: false),
                    Topic = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    StoryblocksQuery = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Phase = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ScriptBody = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TagsCsv = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    HashtagsCsv = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    FinalOutputPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    RenderedDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ScriptTokens = table.Column<int>(type: "integer", nullable: true),
                    TtsCharacters = table.Column<int>(type: "integer", nullable: true),
                    RenderSeconds = table.Column<double>(type: "double precision", nullable: true),
                    CostUsd = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoJobs_Niches_NicheId",
                        column: x => x.NicheId,
                        principalTable: "Niches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RenderErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseAtFailure = table.Column<int>(type: "integer", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Detail = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenderErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenderErrors_VideoJobs_VideoJobId",
                        column: x => x.VideoJobId,
                        principalTable: "VideoJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAssets_VideoJobs_VideoJobId",
                        column: x => x.VideoJobId,
                        principalTable: "VideoJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Niches",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AdditionalScriptInstructions", "KaraokeBackgroundColor", "KaraokeFillColor", "KaraokeFontFamily", "KaraokeFontSize", "KaraokeHighlightColor", "KaraokeHighlightFontSize", "KaraokeOutlineColor", "KaraokeYPositionPercent", "LanguageCode", "MaxWords", "OverlayGifLoopCount", "OverlayGifPath", "OverlayGifPositionPercent", "OverlayGifTailSeconds", "TargetWordCount", "TtsSpeed", "TtsVoice" },
                values: new object[] { "End every short with a single concrete takeaway.", "#0D1321", "#FFFFFF", "The Bold Font", 96, "#FFD166", 140, "#000000", 7.0, "en-US", 150, 0, "Assets/Overlays/subscribe.gif", 95.299999999999997, 5.0, 130, 1f, "onyx" });

            migrationBuilder.UpdateData(
                table: "Niches",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AdditionalScriptInstructions", "KaraokeBackgroundColor", "KaraokeFillColor", "KaraokeFontFamily", "KaraokeFontSize", "KaraokeHighlightColor", "KaraokeHighlightFontSize", "KaraokeOutlineColor", "KaraokeYPositionPercent", "LanguageCode", "MaxWords", "OverlayGifLoopCount", "OverlayGifPath", "OverlayGifPositionPercent", "OverlayGifTailSeconds", "TargetWordCount", "TtsSpeed", "TtsVoice" },
                values: new object[] { "Open with a 'wait, what?' hook.", "#0D1321", "#FFFFFF", "The Bold Font", 96, "#39FF14", 140, "#000000", 7.0, "en-US", 150, 0, "Assets/Overlays/subscribe.gif", 95.299999999999997, 5.0, 130, 1.05f, "nova" });

            migrationBuilder.UpdateData(
                table: "Niches",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AdditionalScriptInstructions", "KaraokeBackgroundColor", "KaraokeFillColor", "KaraokeFontFamily", "KaraokeFontSize", "KaraokeHighlightColor", "KaraokeHighlightFontSize", "KaraokeOutlineColor", "KaraokeYPositionPercent", "LanguageCode", "MaxWords", "OverlayGifLoopCount", "OverlayGifPath", "OverlayGifPositionPercent", "OverlayGifTailSeconds", "TargetWordCount", "TtsSpeed", "TtsVoice" },
                values: new object[] { "Build to a single chilling fact halfway through.", "#0D1321", "#FFFFFF", "The Bold Font", 96, "#FFD166", 140, "#000000", 7.0, "en-US", 150, 0, "Assets/Overlays/subscribe.gif", 95.299999999999997, 5.0, 130, 1f, "echo" });

            migrationBuilder.InsertData(
                table: "Niches",
                columns: new[] { "Id", "AdditionalScriptInstructions", "ElevenLabsVoiceId", "IsActive", "KaraokeBackgroundColor", "KaraokeFillColor", "KaraokeFontFamily", "KaraokeFontSize", "KaraokeHighlightColor", "KaraokeHighlightFontSize", "KaraokeOutlineColor", "KaraokeYPositionPercent", "LanguageCode", "MaxWords", "MusicDirectory", "Name", "OverlayGifLoopCount", "OverlayGifPath", "OverlayGifPositionPercent", "OverlayGifTailSeconds", "QueuePriority", "ScriptTone", "TargetWordCount", "TtsSpeed", "TtsVoice", "Type" },
                values: new object[,]
                {
                    { 4, "Apri con una scena visiva concreta. Ogni short deve chiudere con una rivelazione storica sorprendente.", "", true, "#1A0F00", "#FFFFFF", "The Bold Font", 96, "#D4AF37", 140, "#1A0F00", 8.0, "it-IT", 170, "Assets/Music/HistoryEpic", "Storia Antica", 0, "Assets/Overlays/subscribe.gif", 95.299999999999997, 5.0, 100, "Drammatico, epico", 150, 0.95f, "onyx", 4 },
                    { 5, "Hook in 4 words or less. No filler.", "", true, "#0D1321", "#FFFFFF", "The Bold Font", 102, "#FF14C8", 150, "#000000", 6.0, "en-US", 130, "Assets/Music/UpliftEpic", "Brainrot Facts", -1, "Assets/Overlays/subscribe.gif", 95.299999999999997, 4.0, 100, "Punchy, fast-paced, slang-friendly", 110, 1.15f, "nova", 5 },
                    { 6, "Always end on a kindness moment, never sad endings.", "", true, "#1B0F2A", "#FFFFFF", "The Bold Font", 92, "#FFB5E8", 130, "#000000", 9.0, "en-US", 140, "Assets/Music/Wholesome", "Wholesome Animals", 0, "Assets/Overlays/subscribe.gif", 95.299999999999997, 5.0, 100, "Gentle, warm, comforting", 120, 0.95f, "shimmer", 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenderErrors_VideoJobId_CreatedAtUtc",
                table: "RenderErrors",
                columns: new[] { "VideoJobId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoAssets_VideoJobId_Kind",
                table: "VideoAssets",
                columns: new[] { "VideoJobId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoJobs_NicheId_CreatedAtUtc",
                table: "VideoJobs",
                columns: new[] { "NicheId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoJobs_Phase",
                table: "VideoJobs",
                column: "Phase");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RenderErrors");

            migrationBuilder.DropTable(
                name: "VideoAssets");

            migrationBuilder.DropTable(
                name: "VideoJobs");

            migrationBuilder.DeleteData(
                table: "Niches",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Niches",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Niches",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "AdditionalScriptInstructions",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "KaraokeBackgroundColor",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "KaraokeFillColor",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "KaraokeFontFamily",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "KaraokeFontSize",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "KaraokeHighlightColor",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "KaraokeHighlightFontSize",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "KaraokeOutlineColor",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "KaraokeYPositionPercent",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "MaxWords",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "OverlayGifLoopCount",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "OverlayGifPath",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "OverlayGifPositionPercent",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "OverlayGifTailSeconds",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "TargetWordCount",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "TtsSpeed",
                table: "Niches");

            migrationBuilder.DropColumn(
                name: "TtsVoice",
                table: "Niches");
        }
    }
}
