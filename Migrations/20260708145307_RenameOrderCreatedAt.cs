using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TillApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrderCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Orders",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Orders",
                newName: "CreateAt");
        }
    }
}
