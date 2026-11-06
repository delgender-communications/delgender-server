using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanyDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportFileName",
                table: "OutreachCompanies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportPublicId",
                table: "OutreachCompanies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportUploadedAt",
                table: "OutreachCompanies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportUrl",
                table: "OutreachCompanies",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeamDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    FilePublicId = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByStaffId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamDocuments_Staffs_UploadedByStaffId",
                        column: x => x.UploadedByStaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamDocuments_UploadedByStaffId",
                table: "TeamDocuments",
                column: "UploadedByStaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamDocuments");

            migrationBuilder.DropColumn(
                name: "ReportFileName",
                table: "OutreachCompanies");

            migrationBuilder.DropColumn(
                name: "ReportPublicId",
                table: "OutreachCompanies");

            migrationBuilder.DropColumn(
                name: "ReportUploadedAt",
                table: "OutreachCompanies");

            migrationBuilder.DropColumn(
                name: "ReportUrl",
                table: "OutreachCompanies");
        }
    }
}
