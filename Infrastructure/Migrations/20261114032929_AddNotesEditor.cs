using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesEditor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutreachNoteEditors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OutreachCompanyId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    EditCount = table.Column<int>(type: "integer", nullable: false),
                    FirstEditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastEditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutreachNoteEditors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutreachNoteEditors_OutreachCompanies_OutreachCompanyId",
                        column: x => x.OutreachCompanyId,
                        principalTable: "OutreachCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutreachNoteEditors_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutreachNoteEditors_OutreachCompanyId_StaffId",
                table: "OutreachNoteEditors",
                columns: new[] { "OutreachCompanyId", "StaffId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutreachNoteEditors_StaffId",
                table: "OutreachNoteEditors",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutreachNoteEditors");
        }
    }
}
