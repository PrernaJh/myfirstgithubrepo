using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace PackageTracker.Identity.Data.Migrations
{
	public partial class addTemporaryPasswordExpirationDate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<DateTime>(
				name: "TemporaryPasswordExpirationDate",
				table: "Users",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "TemporaryPasswordExpirationDate",
				table: "Users");
		}
	}
}
