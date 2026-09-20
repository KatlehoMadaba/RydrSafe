using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RydrSafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComplianceLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VerificationHistories_UserId",
                table: "VerificationHistories");

            migrationBuilder.DropIndex(
                name: "IX_Reports_DriverId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "ImageHashes",
                table: "VerificationHistories",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImagesDiscardedAt",
                table: "VerificationHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OcrDataPurgedAt",
                table: "VerificationHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OcrDataRetainedUntil",
                table: "VerificationHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAgreementAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptedAgreementVersion",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosureReason",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Classification",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "CategoryA");

            migrationBuilder.AddColumn<DateTime>(
                name: "CorroboratedAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CorroboratedBy",
                table: "Reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorroborationPath",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<DateTime>(
                name: "CorroborationRevokedAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorroborationRevokedReason",
                table: "Reports",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficialReference",
                table: "Reports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OfficialReferenceVerified",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfficialReferenceVerifiedAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficialReferenceVerifiedBy",
                table: "Reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicRecordSourceReference",
                table: "Reports",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicRecordSourceType",
                table: "Reports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicRecordSourceUrl",
                table: "Reports",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicRecordVerifiedAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicRecordVerifiedBy",
                table: "Reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReclassifiedAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReclassifiedBy",
                table: "Reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionDeviceHash",
                table: "Reports",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionIpHash",
                table: "Reports",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WithdrawnAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PublicStatusSuspended",
                table: "Drivers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RightOfReplyOfferedAt",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RightOfReplyRespondedAt",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RightOfReplyResponse",
                table: "Drivers",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Section18NoticeSentAt",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverAppeals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Grounds = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Detail = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdentityEvidenceNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IdentityVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IdentityVerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IdentityVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedTo = table.Column<Guid>(type: "uuid", nullable: true),
                    PublicStatusSuspended = table.Column<bool>(type: "boolean", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverAppeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverAppeals_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverRecordAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequesterIpHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Surface = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LookupTermHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Matched = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverRecordAccessLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriverStatusAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "text", nullable: false),
                    ToStatus = table.Column<string>(type: "text", nullable: false),
                    RiskScoreAtDecision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReviewedRiskScore = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedDriverResponse = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedScoringLogic = table.Column<bool>(type: "boolean", nullable: false),
                    RightOfReplyOffered = table.Column<bool>(type: "boolean", nullable: false),
                    RightOfReplyOfferedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RelatedAppealId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverStatusAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverStatusAudits_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportStatusAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "text", nullable: false),
                    ToStatus = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReviewedReportContent = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedDriverResponse = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedRiskScore = table.Column<bool>(type: "boolean", nullable: false),
                    RelatedAppealId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportStatusAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportStatusAudits_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AgreementVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Locale = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CollectionSurface = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    WithdrawnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConsents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationHistories_OcrDataRetainedUntil",
                table: "VerificationHistories",
                column: "OcrDataRetainedUntil");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationHistories_UserId_VerifiedAt",
                table: "VerificationHistories",
                columns: new[] { "UserId", "VerifiedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CreatedAt",
                table: "Reports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DriverId_Status",
                table: "Reports",
                columns: new[] { "DriverId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status",
                table: "Reports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_PhoneNumber",
                table: "Drivers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_RiskScore",
                table: "Drivers",
                column: "RiskScore");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAppeals_ContactEmail",
                table: "DriverAppeals",
                column: "ContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAppeals_CreatedAt",
                table: "DriverAppeals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAppeals_DriverId_Status",
                table: "DriverAppeals",
                columns: new[] { "DriverId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverRecordAccessLogs_CreatedAt",
                table: "DriverRecordAccessLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DriverRecordAccessLogs_RequesterIpHash_CreatedAt",
                table: "DriverRecordAccessLogs",
                columns: new[] { "RequesterIpHash", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverStatusAudits_ActorUserId",
                table: "DriverStatusAudits",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverStatusAudits_DriverId_CreatedAt",
                table: "DriverStatusAudits",
                columns: new[] { "DriverId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportStatusAudits_ActorUserId",
                table: "ReportStatusAudits",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportStatusAudits_ReportId_CreatedAt",
                table: "ReportStatusAudits",
                columns: new[] { "ReportId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_AcceptedAt",
                table: "UserConsents",
                column: "AcceptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserId_ConsentKey_AcceptedAt",
                table: "UserConsents",
                columns: new[] { "UserId", "ConsentKey", "AcceptedAt" });

            // ---- Data migration ----
            // The new enum columns and the reshaped ReportStatus have to be reconciled with rows
            // that already exist, or the app throws on the first read of an unparseable value.

            // Clause 6.1: everything defaults to Category A except the one category that alleges
            // no offence. 'Other' stays Category A until a moderator reclassifies it.
            migrationBuilder.Sql(@"
                UPDATE ""Reports""
                SET ""Classification"" = CASE
                    WHEN ""Category"" = 'UnsafeVehicle' THEN 'CategoryB'
                    ELSE 'CategoryA'
                END;");

            // 'Escalated' no longer exists. Those reports were awaiting a moderator decision, so
            // they go back to Pending rather than silently gaining or losing standing.
            migrationBuilder.Sql(@"
                UPDATE ""Reports""
                SET ""Status"" = 'Pending'
                WHERE ""Status"" = 'Escalated';");

            // Clause 6.2: nothing in the old data cleared the corroboration threshold, because
            // the threshold did not exist. Previously-approved reports keep their approval but
            // are not treated as corroborated, which means they no longer surface publicly or
            // feed the risk score until a moderator works through them.
            migrationBuilder.Sql(@"
                UPDATE ""Reports""
                SET ""CorroborationPath"" = 'None'
                WHERE ""CorroborationPath"" IS NULL OR ""CorroborationPath"" = '';");

            // Risk scores were computed from pending and approved reports under the old rules, so
            // every stored score is now overstated. Reset them; the next moderator decision
            // recomputes from corroborated reports only.
            migrationBuilder.Sql(@"
                UPDATE ""Drivers""
                SET ""RiskScore"" = 0,
                    ""Status"" = 'Safe',
                    ""UpdatedAt"" = NOW() AT TIME ZONE 'UTC';");

            // Existing accounts predate Part D. They have no consent rows and no recorded
            // agreement version; clause 17.2 re-acceptance covers them on next sign-in.
            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""AcceptedAgreementVersion"" = NULL
                WHERE ""AcceptedAgreementVersion"" = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverAppeals");

            migrationBuilder.DropTable(
                name: "DriverRecordAccessLogs");

            migrationBuilder.DropTable(
                name: "DriverStatusAudits");

            migrationBuilder.DropTable(
                name: "ReportStatusAudits");

            migrationBuilder.DropTable(
                name: "UserConsents");

            migrationBuilder.DropIndex(
                name: "IX_VerificationHistories_OcrDataRetainedUntil",
                table: "VerificationHistories");

            migrationBuilder.DropIndex(
                name: "IX_VerificationHistories_UserId_VerifiedAt",
                table: "VerificationHistories");

            migrationBuilder.DropIndex(
                name: "IX_Reports_CreatedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_DriverId_Status",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Status",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_PhoneNumber",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_RiskScore",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "ImageHashes",
                table: "VerificationHistories");

            migrationBuilder.DropColumn(
                name: "ImagesDiscardedAt",
                table: "VerificationHistories");

            migrationBuilder.DropColumn(
                name: "OcrDataPurgedAt",
                table: "VerificationHistories");

            migrationBuilder.DropColumn(
                name: "OcrDataRetainedUntil",
                table: "VerificationHistories");

            migrationBuilder.DropColumn(
                name: "AcceptedAgreementAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AcceptedAgreementVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClosureReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CorroboratedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CorroboratedBy",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CorroborationPath",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CorroborationRevokedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CorroborationRevokedReason",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "OfficialReference",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "OfficialReferenceVerified",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "OfficialReferenceVerifiedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "OfficialReferenceVerifiedBy",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PublicRecordSourceReference",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PublicRecordSourceType",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PublicRecordSourceUrl",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PublicRecordVerifiedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PublicRecordVerifiedBy",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReclassifiedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReclassifiedBy",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "SubmissionDeviceHash",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "SubmissionIpHash",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "WithdrawnAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PublicStatusSuspended",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "RightOfReplyOfferedAt",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "RightOfReplyRespondedAt",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "RightOfReplyResponse",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Section18NoticeSentAt",
                table: "Drivers");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationHistories_UserId",
                table: "VerificationHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DriverId",
                table: "Reports",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }
    }
}
