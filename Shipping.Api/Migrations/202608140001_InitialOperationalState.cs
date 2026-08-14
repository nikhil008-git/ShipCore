using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipCore.Migrations;

public partial class InitialOperationalState : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProcessedIntegrationRequests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CarrierCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                ClientRef = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                CarrierTrackingNumber = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ProcessedIntegrationRequests", x => x.Id));
        migrationBuilder.CreateTable(
            name: "ReconciliationRuns",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                InvoiceWeek = table.Column<DateOnly>(type: "TEXT", nullable: false),
                StartedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                Status = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ReconciliationRuns", x => x.Id));
        migrationBuilder.CreateIndex(
            name: "IX_ProcessedIntegrationRequests_CarrierCode_ClientRef",
            table: "ProcessedIntegrationRequests",
            columns: new[] { "CarrierCode", "ClientRef" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ProcessedIntegrationRequests");
        migrationBuilder.DropTable(name: "ReconciliationRuns");
    }
}
