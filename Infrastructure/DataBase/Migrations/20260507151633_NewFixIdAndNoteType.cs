using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.Migrations;

/// <inheritdoc />
public partial class NewFixIdAndNoteType : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(name: "FK_Notes_NoteTypes_TypeId", table: "Notes");

		migrationBuilder.DropTable(name: "NoteTypes");

		migrationBuilder.DropIndex(name: "IX_Notes_TypeId", table: "Notes");

		migrationBuilder.DropColumn(name: "TypeId", table: "Notes");

		migrationBuilder.AddColumn<DateTime>(
			name: "RegisterDate",
			table: "Users",
			type: "datetime(6)",
			nullable: false,
			defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
		);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(name: "RegisterDate", table: "Users");

		migrationBuilder.AddColumn<int>(name: "TypeId", table: "Notes", type: "int", nullable: false, defaultValue: 0);

		migrationBuilder
			.CreateTable(
				name: "NoteTypes",
				columns: table => new
				{
					Id = table
						.Column<int>(type: "int", nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					TypeName = table
						.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_NoteTypes", x => x.Id);
				}
			)
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateIndex(name: "IX_Notes_TypeId", table: "Notes", column: "TypeId");

		migrationBuilder.AddForeignKey(
			name: "FK_Notes_NoteTypes_TypeId",
			table: "Notes",
			column: "TypeId",
			principalTable: "NoteTypes",
			principalColumn: "Id",
			onDelete: ReferentialAction.Restrict
		);
	}
}
