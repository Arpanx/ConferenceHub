using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceHub.Migrations
{
    /// <inheritdoc />
    public partial class RenameServiceToAdditionalService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_Services_ServiceId",
                table: "BookingServices");

            migrationBuilder.DropForeignKey(
                name: "FK_HallServices_Services_ServiceId",
                table: "HallServices");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "HallServices",
                newName: "AdditionalServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_HallServices_ServiceId",
                table: "HallServices",
                newName: "IX_HallServices_AdditionalServiceId");

            migrationBuilder.CreateTable(
                name: "AdditionalServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalServices", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_AdditionalServices_ServiceId",
                table: "BookingServices",
                column: "ServiceId",
                principalTable: "AdditionalServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HallServices_AdditionalServices_AdditionalServiceId",
                table: "HallServices",
                column: "AdditionalServiceId",
                principalTable: "AdditionalServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_AdditionalServices_ServiceId",
                table: "BookingServices");

            migrationBuilder.DropForeignKey(
                name: "FK_HallServices_AdditionalServices_AdditionalServiceId",
                table: "HallServices");

            migrationBuilder.DropTable(
                name: "AdditionalServices");

            migrationBuilder.RenameColumn(
                name: "AdditionalServiceId",
                table: "HallServices",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_HallServices_AdditionalServiceId",
                table: "HallServices",
                newName: "IX_HallServices_ServiceId");

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_Services_ServiceId",
                table: "BookingServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HallServices_Services_ServiceId",
                table: "HallServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
