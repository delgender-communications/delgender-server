using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionVersion",
                table: "RefreshTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrustedDeviceId",
                table: "RefreshTokens",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TrustedDeviceId",
                table: "RefreshTokens",
                column: "TrustedDeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_TrustedDevices_TrustedDeviceId",
                table: "RefreshTokens",
                column: "TrustedDeviceId",
                principalTable: "TrustedDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_TrustedDevices_TrustedDeviceId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TrustedDeviceId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "SessionVersion",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "TrustedDeviceId",
                table: "RefreshTokens");
        }
    }
}
