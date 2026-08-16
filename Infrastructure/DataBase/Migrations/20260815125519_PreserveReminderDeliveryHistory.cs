using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class PreserveReminderDeliveryHistory : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<DateTime>(
				name: "DeliveredAt",
				table: "Reminders",
				type: "datetime(6)",
				nullable: true
			);

			migrationBuilder.AddColumn<DateTime>(
				name: "PublishedAt",
				table: "Reminders",
				type: "datetime(6)",
				nullable: true
			);

			migrationBuilder.CreateIndex(
				name: "IX_Reminders_PublishedAt_RemindAt",
				table: "Reminders",
				columns: new[] { "PublishedAt", "RemindAt" }
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(name: "IX_Reminders_PublishedAt_RemindAt", table: "Reminders");

			migrationBuilder.DropColumn(name: "DeliveredAt", table: "Reminders");

			migrationBuilder.DropColumn(name: "PublishedAt", table: "Reminders");
		}
	}
}
