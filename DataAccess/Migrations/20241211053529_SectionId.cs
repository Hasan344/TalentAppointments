using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForQab.Migrations
{
    /// <inheritdoc />
    public partial class SectionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "AspNetUsers");
        }
    }
}
