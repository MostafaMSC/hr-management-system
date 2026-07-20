using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveRequestLegacyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "LeaveRequests",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.Sql("ALTER TABLE \"LeaveRequests\" ADD COLUMN \"LeaveType_temp\" integer NOT NULL DEFAULT 0;");

            migrationBuilder.Sql(@"
                UPDATE ""LeaveRequests"" SET ""LeaveType_temp"" = 
                CASE 
                    WHEN ""LeaveType"" = 'Sick' THEN 0
                    WHEN ""LeaveType"" = 'Daily' THEN 1
                    WHEN ""LeaveType"" = 'Hourly' THEN 2
                    WHEN ""LeaveType"" = 'Personal' THEN 3
                    WHEN ""LeaveType"" = 'ChangeShift' THEN 4
                    WHEN ""LeaveType"" = 'Marriage' THEN 5
                    WHEN ""LeaveType"" = 'Bereavement' THEN 6
                    WHEN ""LeaveType"" = 'Maternity' THEN 7
                    WHEN ""LeaveType"" = 'Hajj' THEN 8
                    WHEN ""LeaveType"" = 'Paternity' THEN 9
                    ELSE 0
                END;
            ");

            migrationBuilder.DropColumn(
                name: "LeaveType",
                table: "LeaveRequests");

            migrationBuilder.RenameColumn(
                name: "LeaveType_temp",
                table: "LeaveRequests",
                newName: "LeaveType");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedByHRAt",
                table: "LeaveRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByHRId",
                table: "LeaveRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedByManagerAt",
                table: "LeaveRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByManagerId",
                table: "LeaveRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedBySecondLineManagerAt",
                table: "LeaveRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBySecondLineManagerId",
                table: "LeaveRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "LeaveRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "LeaveRequests",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HrComment",
                table: "LeaveRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaveDate",
                table: "LeaveRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeaveReason",
                table: "LeaveRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerComment",
                table: "LeaveRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "LeaveRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedById",
                table: "LeaveRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedShiftId",
                table: "LeaveRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "LeaveRequests",
                type: "interval",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ApprovedByHRId",
                table: "LeaveRequests",
                column: "ApprovedByHRId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ApprovedByManagerId",
                table: "LeaveRequests",
                column: "ApprovedByManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ApprovedBySecondLineManagerId",
                table: "LeaveRequests",
                column: "ApprovedBySecondLineManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_RejectedById",
                table: "LeaveRequests",
                column: "RejectedById");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_RequestedShiftId",
                table: "LeaveRequests",
                column: "RequestedShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_AttendanceShifts_RequestedShiftId",
                table: "LeaveRequests",
                column: "RequestedShiftId",
                principalTable: "AttendanceShifts",
                principalColumn: "Id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_AttendanceShifts_RequestedShiftId",
                table: "LeaveRequests");

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

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_ApprovedByHRId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_ApprovedByManagerId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_ApprovedBySecondLineManagerId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_RejectedById",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_RequestedShiftId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedByHRAt",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedByHRId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedByManagerAt",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedByManagerId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedBySecondLineManagerAt",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedBySecondLineManagerId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "HrComment",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LeaveDate",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LeaveReason",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ManagerComment",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "RejectedById",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "RequestedShiftId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "LeaveRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "LeaveRequests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LeaveType",
                table: "LeaveRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
