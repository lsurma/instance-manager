#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace InstanceManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class user_tracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Translations",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ProjectInstances",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "DataSets",
                type: "TEXT",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ProjectInstances");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "DataSets");
        }
    }
}
