using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace PackageTracker.Identity.Data.Migrations
{
	public partial class securityCode : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "SecurityCode",
				table: "Users",
				maxLength: 200,
				nullable: true);

			migrationBuilder.AddColumn<DateTime>(
				name: "SecurityCodeExpirationDate",
				table: "Users",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "SecurityCode",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "SecurityCodeExpirationDate",
				table: "Users");
		}
	}
}
