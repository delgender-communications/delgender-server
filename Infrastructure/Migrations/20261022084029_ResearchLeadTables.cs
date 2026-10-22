using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResearchLeadTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResearchLeads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: true),
                    Industry = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Case = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    WebsiteHost = table.Column<string>(type: "text", nullable: true),
                    NameKey = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DismissReason = table.Column<string>(type: "text", nullable: true),
                    ReviewedByStaffId = table.Column<int>(type: "integer", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OutreachCompanyId = table.Column<int>(type: "integer", nullable: true),
                    AddedByStaffId = table.Column<int>(type: "integer", nullable: true),
                    RunId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchLeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchLeads_OutreachCompanies_OutreachCompanyId",
                        column: x => x.OutreachCompanyId,
                        principalTable: "OutreachCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResearchLeads_Staffs_AddedByStaffId",
                        column: x => x.AddedByStaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResearchLeads_Staffs_ReviewedByStaffId",
                        column: x => x.ReviewedByStaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ResearchLeadEvidenceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResearchLeadId = table.Column<int>(type: "integer", nullable: false),
                    Finding = table.Column<string>(type: "text", nullable: false),
                    SourceUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchLeadEvidenceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchLeadEvidenceItems_ResearchLeads_ResearchLeadId",
                        column: x => x.ResearchLeadId,
                        principalTable: "ResearchLeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchLeadEvidenceItems_ResearchLeadId",
                table: "ResearchLeadEvidenceItems",
                column: "ResearchLeadId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchLeads_AddedByStaffId",
                table: "ResearchLeads",
                column: "AddedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchLeads_NameKey",
                table: "ResearchLeads",
                column: "NameKey");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchLeads_OutreachCompanyId",
                table: "ResearchLeads",
                column: "OutreachCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchLeads_ReviewedByStaffId",
                table: "ResearchLeads",
                column: "ReviewedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchLeads_Status",
                table: "ResearchLeads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchLeads_WebsiteHost",
                table: "ResearchLeads",
                column: "WebsiteHost");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResearchLeadEvidenceItems");

            migrationBuilder.DropTable(
                name: "ResearchLeads");
        }
    }
}
