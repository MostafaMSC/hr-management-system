using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyUserInfoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "UserInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "UserInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "UserInfos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Card",
                table: "UserInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceIp",
                table: "UserInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "UserInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "UserInfos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "UserInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShiftType",
                table: "UserInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "UserInfos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "Card",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "DeviceIp",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "ShiftType",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "UserInfos");
        }
    }
}
