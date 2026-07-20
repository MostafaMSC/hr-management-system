using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceLogType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PunchType",
                table: "AttendanceLogs",
                newName: "LogsType");

            migrationBuilder.AddColumn<string>(
                name: "CheckStatus",
                table: "AttendanceLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckStatus",
                table: "AttendanceLogs");

            migrationBuilder.RenameColumn(
                name: "LogsType",
                table: "AttendanceLogs",
                newName: "PunchType");
        }
    }
}
