using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OutreachData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutreachCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    ContactName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PitchDeckUrl = table.Column<string>(type: "text", nullable: true),
                    PitchDeckPublicId = table.Column<string>(type: "text", nullable: true),
                    PitchDeckFileName = table.Column<string>(type: "text", nullable: true),
                    PitchDeckUploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AddedByStaffId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutreachCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutreachCompanies_Staffs_AddedByStaffId",
                        column: x => x.AddedByStaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutreachLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OutreachCompanyId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: true),
                    ActorName = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutreachLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutreachLogs_OutreachCompanies_OutreachCompanyId",
                        column: x => x.OutreachCompanyId,
                        principalTable: "OutreachCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutreachLogs_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutreachCompanies_AddedByStaffId",
                table: "OutreachCompanies",
                column: "AddedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_OutreachCompanies_Status",
                table: "OutreachCompanies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OutreachLogs_CreatedAt",
                table: "OutreachLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OutreachLogs_OutreachCompanyId",
                table: "OutreachLogs",
                column: "OutreachCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_OutreachLogs_StaffId",
                table: "OutreachLogs",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutreachLogs");

            migrationBuilder.DropTable(
                name: "OutreachCompanies");
        }
    }
}
