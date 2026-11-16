using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class ManagerProjectTopUpRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AllowedTopUpHours",
                table: "ApprovalConfigurations",
                newName: "RequiredOfficeHours");

            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "TopUpRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "TopUpRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TopUpRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TopUpEnd",
                table: "TopUpRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TopUpStart",
                table: "TopUpRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "SwipeLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "SwipeLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "ApprovalConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ApprovalConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequiredOfficeDaysPerWeek",
                table: "ApprovalConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalConfigurations_ManagerId_ProjectId",
                table: "ApprovalConfigurations",
                columns: new[] { "ManagerId", "ProjectId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApprovalConfigurations_ManagerId_ProjectId",
                table: "ApprovalConfigurations");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "TopUpRequests");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "TopUpRequests");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TopUpRequests");

            migrationBuilder.DropColumn(
                name: "TopUpEnd",
                table: "TopUpRequests");

            migrationBuilder.DropColumn(
                name: "TopUpStart",
                table: "TopUpRequests");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "SwipeLogs");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "SwipeLogs");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "ApprovalConfigurations");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ApprovalConfigurations");

            migrationBuilder.DropColumn(
                name: "RequiredOfficeDaysPerWeek",
                table: "ApprovalConfigurations");

            migrationBuilder.RenameColumn(
                name: "RequiredOfficeHours",
                table: "ApprovalConfigurations",
                newName: "AllowedTopUpHours");
        }
    }
}
