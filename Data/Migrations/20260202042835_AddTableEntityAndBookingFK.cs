using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Resturant_Menu.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTableEntityAndBookingFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TableNumber",
                table: "Bookings",
                newName: "TableId");

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tables",
                columns: new[] { "Id", "IsAvailable", "Number" },
                values: new object[,]
                {
                    { 1, true, 1 },
                    { 2, true, 2 },
                    { 3, true, 3 },
                    { 4, true, 4 },
                    { 5, true, 5 },
                    { 6, true, 6 },
                    { 7, true, 7 },
                    { 8, true, 8 },
                    { 9, true, 9 },
                    { 10, true, 10 },
                    { 11, true, 11 },
                    { 12, true, 12 },
                    { 13, true, 13 },
                    { 14, true, 14 },
                    { 15, true, 15 },
                    { 16, true, 16 },
                    { 17, true, 17 },
                    { 18, true, 18 },
                    { 19, true, 19 },
                    { 20, true, 20 },
                    { 21, true, 21 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TableId",
                table: "Bookings",
                column: "TableId");

            // Ensure existing booking table ids are within seeded range (1..21). Map invalid values to 1.
            migrationBuilder.Sql(@"UPDATE Bookings SET TableId = CASE WHEN TableId BETWEEN 1 AND 21 THEN TableId ELSE 1 END");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Tables_TableId",
                table: "Bookings",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Tables_TableId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TableId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "TableId",
                table: "Bookings",
                newName: "TableNumber");
        }
    }
}
