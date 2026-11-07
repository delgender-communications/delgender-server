using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResearchLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsResearch",
                table: "ResearchLeads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResearchReason",
                table: "ResearchLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteStack",
                table: "ResearchLeads",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsResearch",
                table: "ResearchLeads");

            migrationBuilder.DropColumn(
                name: "ResearchReason",
                table: "ResearchLeads");

            migrationBuilder.DropColumn(
                name: "SiteStack",
                table: "ResearchLeads");
        }
    }
}
