using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstanceManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InternalGroupName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ResourceName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TranslationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CultureName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    DataSetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translations_DataSets_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Translations_CultureName",
                table: "Translations",
                column: "CultureName");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_DataSetId",
                table: "Translations",
                column: "DataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_InternalGroupName_ResourceName_CultureName",
                table: "Translations",
                columns: new[] { "InternalGroupName", "ResourceName", "CultureName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Translations");
        }
    }
}
