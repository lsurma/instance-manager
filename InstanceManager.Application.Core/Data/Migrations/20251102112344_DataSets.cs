using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstanceManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class DataSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataSetInclude",
                columns: table => new
                {
                    ParentDataSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncludedDataSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetInclude", x => new { x.ParentDataSetId, x.IncludedDataSetId });
                    table.ForeignKey(
                        name: "FK_DataSetInclude_DataSets_IncludedDataSetId",
                        column: x => x.IncludedDataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataSetInclude_DataSets_ParentDataSetId",
                        column: x => x.ParentDataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataSetInclude_IncludedDataSetId",
                table: "DataSetInclude",
                column: "IncludedDataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetInclude_ParentDataSetId_IncludedDataSetId",
                table: "DataSetInclude",
                columns: new[] { "ParentDataSetId", "IncludedDataSetId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataSetInclude");

            migrationBuilder.DropTable(
                name: "DataSets");
        }
    }
}
