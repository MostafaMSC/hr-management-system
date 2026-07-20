using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceShifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShiftName",
                table: "AttendanceShifts",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "AttendanceShiftId",
                table: "UserInfos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AttendanceShifts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AttendanceShifts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_AttendanceShiftId",
                table: "UserInfos",
                column: "AttendanceShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfos_AttendanceShifts_AttendanceShiftId",
                table: "UserInfos",
                column: "AttendanceShiftId",
                principalTable: "AttendanceShifts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInfos_AttendanceShifts_AttendanceShiftId",
                table: "UserInfos");

            migrationBuilder.DropIndex(
                name: "IX_UserInfos_AttendanceShiftId",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "AttendanceShiftId",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AttendanceShifts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AttendanceShifts");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AttendanceShifts",
                newName: "ShiftName");
        }
    }
}
