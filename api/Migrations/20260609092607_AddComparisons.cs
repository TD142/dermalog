using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dermalog.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddComparisons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comparisons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BeforePhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AfterPhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverallSummary = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: false
                    ),
                    Observations = table.Column<string>(type: "jsonb", nullable: false),
                    SeverityTrend = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    GeneratedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comparisons_photos_AfterPhotoId",
                        column: x => x.AfterPhotoId,
                        principalTable: "photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_comparisons_photos_BeforePhotoId",
                        column: x => x.BeforePhotoId,
                        principalTable: "photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_comparisons_AfterPhotoId",
                table: "comparisons",
                column: "AfterPhotoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_comparisons_BeforePhotoId",
                table: "comparisons",
                column: "BeforePhotoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_comparisons_GeneratedAt",
                table: "comparisons",
                column: "GeneratedAt",
                descending: new bool[0]
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "comparisons");
        }
    }
}
