using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.Identity.Data.Migrations
{
	public partial class addClientAndSubClientToUserModel : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "TemporaryPassword",
				table: "Users",
				maxLength: 200,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)",
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "Site",
				table: "Users",
				maxLength: 50,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)",
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "LastName",
				table: "Users",
				maxLength: 200,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)",
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "FirstName",
				table: "Users",
				maxLength: 200,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)",
				oldNullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Client",
				table: "Users",
				maxLength: 50,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "SubClient",
				table: "Users",
				maxLength: 50,
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Client",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "SubClient",
				table: "Users");

			migrationBuilder.AlterColumn<string>(
				name: "TemporaryPassword",
				table: "Users",
				type: "nvarchar(max)",
				nullable: true,
				oldClrType: typeof(string),
				oldMaxLength: 200,
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "Site",
				table: "Users",
				type: "nvarchar(max)",
				nullable: true,
				oldClrType: typeof(string),
				oldMaxLength: 50,
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "LastName",
				table: "Users",
				type: "nvarchar(max)",
				nullable: true,
				oldClrType: typeof(string),
				oldMaxLength: 200,
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "FirstName",
				table: "Users",
				type: "nvarchar(max)",
				nullable: true,
				oldClrType: typeof(string),
				oldMaxLength: 200,
				oldNullable: true);
		}
	}
}
