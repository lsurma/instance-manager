using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstanceManager.Application.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class allowed_identity_ids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedIdentityIds",
                table: "DataSets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedIdentityIds",
                table: "DataSets");
        }
    }
}
