using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ParitySync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedByHRId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedByManagerId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedBySecondLineManagerId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_RejectedById",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_UserInfoId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfos_UserInfos_DirectManagerId",
                table: "UserInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfos_UserInfos_ReportToId",
                table: "UserInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfos_UserInfos_SecondLineManagerId",
                table: "UserInfos");

            migrationBuilder.DropIndex(
                name: "IX_LeaveBalances_UserInfoId",
                table: "LeaveBalances");

            migrationBuilder.DropIndex(
                name: "IX_DailyAttendanceSummaries_UserInfoId",
                table: "DailyAttendanceSummaries");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceLogs_UserInfoId",
                table: "AttendanceLogs");

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "SystemSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Sections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Sections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Departments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Departments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DeviceIP",
                table: "AttendanceLogs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Fingerprints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceUserId = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FingerIndex = table.Column<int>(type: "integer", nullable: false),
                    Template = table.Column<byte[]>(type: "bytea", nullable: false),
                    TemplateSize = table.Column<int>(type: "integer", nullable: false),
                    DeviceIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fingerprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fingerprints_UserInfos_UserId",
                        column: x => x.UserId,
                        principalTable: "UserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AttendanceShifts",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "EndTime", "IsDeleted", "LateThreshold", "Name", "StartTime", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2026, 7, 20, 10, 34, 47, 274, DateTimeKind.Utc).AddTicks(5883), null, new TimeSpan(0, 16, 0, 0, 0), false, null, "Normal Shift", new TimeSpan(0, 8, 30, 0, 0), null });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "CreatedAt", "Description", "Key", "Section", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 20, 10, 34, 47, 274, DateTimeKind.Utc).AddTicks(5714), "Default work start time", "workDayStart", "Work", null, "08:30" },
                    { 2, new DateTime(2026, 7, 20, 10, 34, 47, 274, DateTimeKind.Utc).AddTicks(5717), "Default work end time", "workDayEnd", "Work", null, "16:00" },
                    { 3, new DateTime(2026, 7, 20, 10, 34, 47, 274, DateTimeKind.Utc).AddTicks(5719), "Required daily work hours", "requiredDailyHours", "Work", null, "8" },
                    { 4, new DateTime(2026, 7, 20, 10, 34, 47, 274, DateTimeKind.Utc).AddTicks(5720), "Default working days per month", "workingDaysPerMonth", "Work", null, "26" },
                    { 5, new DateTime(2026, 7, 20, 10, 34, 47, 274, DateTimeKind.Utc).AddTicks(5721), "Default allowed monthly leave days", "allowedMonthlyLeaveDays", "Work", null, "1.7" },
                    { 6, new DateTime(2026, 7, 20, 10, 34, 47, 274, DateTimeKind.Utc).AddTicks(5722), "Default allowed monthly leave hours", "allowedMonthlyLeaveHours", "Work", null, "4" },
                    { 7, new DateTime(2026, 7, 20, 10, 34, 47, 274, DateTimeKind.Utc).AddTicks(5723), "Default allowed yearly sick leave days", "allowedSickLeaveDays", "Work", null, "15" },
                    { 8, new DateTime(2026, 7, 20, 10, 34, 47, 274, DateTimeKind.Utc).AddTicks(5724), "First day of week (0=Sunday, 6=Saturday)", "firstDayOfWeek", "Work", null, "6" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_Username",
                table: "UserInfos",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Section_Key",
                table: "SystemSettings",
                columns: new[] { "Section", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_UserInfoId_Year",
                table: "LeaveBalances",
                columns: new[] { "UserInfoId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Date",
                table: "Holidays",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyAttendanceSummaries_UserInfoId_Date",
                table: "DailyAttendanceSummaries",
                columns: new[] { "UserInfoId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_DeviceIP",
                table: "AttendanceLogs",
                column: "DeviceIP");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_UserInfoId_Time",
                table: "AttendanceLogs",
                columns: new[] { "UserInfoId", "Time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fingerprints_DeviceIp",
                table: "Fingerprints",
                column: "DeviceIp");

            migrationBuilder.CreateIndex(
                name: "IX_Fingerprints_DeviceUserId",
                table: "Fingerprints",
                column: "DeviceUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Fingerprints_DeviceUserId_FingerIndex_DeviceIp",
                table: "Fingerprints",
                columns: new[] { "DeviceUserId", "FingerIndex", "DeviceIp" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fingerprints_UserId",
                table: "Fingerprints",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedByHRId",
                table: "LeaveRequests",
                column: "ApprovedByHRId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedByManagerId",
                table: "LeaveRequests",
                column: "ApprovedByManagerId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedBySecondLineManagerId",
                table: "LeaveRequests",
                column: "ApprovedBySecondLineManagerId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_RejectedById",
                table: "LeaveRequests",
                column: "RejectedById",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_UserInfoId",
                table: "LeaveRequests",
                column: "UserInfoId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfos_UserInfos_DirectManagerId",
                table: "UserInfos",
                column: "DirectManagerId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfos_UserInfos_ReportToId",
                table: "UserInfos",
                column: "ReportToId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfos_UserInfos_SecondLineManagerId",
                table: "UserInfos",
                column: "SecondLineManagerId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedByHRId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedByManagerId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedBySecondLineManagerId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_RejectedById",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_UserInfos_UserInfoId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfos_UserInfos_DirectManagerId",
                table: "UserInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfos_UserInfos_ReportToId",
                table: "UserInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfos_UserInfos_SecondLineManagerId",
                table: "UserInfos");

            migrationBuilder.DropTable(
                name: "Fingerprints");

            migrationBuilder.DropIndex(
                name: "IX_UserInfos_Username",
                table: "UserInfos");

            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_Section_Key",
                table: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_LeaveBalances_UserInfoId_Year",
                table: "LeaveBalances");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_Date",
                table: "Holidays");

            migrationBuilder.DropIndex(
                name: "IX_DailyAttendanceSummaries_UserInfoId_Date",
                table: "DailyAttendanceSummaries");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceLogs_DeviceIP",
                table: "AttendanceLogs");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceLogs_UserInfoId_Time",
                table: "AttendanceLogs");

            migrationBuilder.DeleteData(
                table: "AttendanceShifts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "Section",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DeviceIP",
                table: "AttendanceLogs");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_UserInfoId",
                table: "LeaveBalances",
                column: "UserInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAttendanceSummaries_UserInfoId",
                table: "DailyAttendanceSummaries",
                column: "UserInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_UserInfoId",
                table: "AttendanceLogs",
                column: "UserInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedByHRId",
                table: "LeaveRequests",
                column: "ApprovedByHRId",
                principalTable: "UserInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedByManagerId",
                table: "LeaveRequests",
                column: "ApprovedByManagerId",
                principalTable: "UserInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_ApprovedBySecondLineManagerId",
                table: "LeaveRequests",
                column: "ApprovedBySecondLineManagerId",
                principalTable: "UserInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_RejectedById",
                table: "LeaveRequests",
                column: "RejectedById",
                principalTable: "UserInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_UserInfos_UserInfoId",
                table: "LeaveRequests",
                column: "UserInfoId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfos_UserInfos_DirectManagerId",
                table: "UserInfos",
                column: "DirectManagerId",
                principalTable: "UserInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfos_UserInfos_ReportToId",
                table: "UserInfos",
                column: "ReportToId",
                principalTable: "UserInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfos_UserInfos_SecondLineManagerId",
                table: "UserInfos",
                column: "SecondLineManagerId",
                principalTable: "UserInfos",
                principalColumn: "Id");
        }
    }
}
