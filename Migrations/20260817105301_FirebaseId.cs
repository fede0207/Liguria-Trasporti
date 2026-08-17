using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Liguria_Trasporti.Migrations
{
    /// <inheritdoc />
    public partial class FirebaseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FirebaseId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirebaseId",
                table: "Employees");
        }
    }
}
