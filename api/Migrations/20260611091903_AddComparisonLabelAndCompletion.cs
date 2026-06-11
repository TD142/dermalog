using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dermalog.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddComparisonLabelAndCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "comparisons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "comparisons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "comparisons");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "comparisons");
        }
    }
}
