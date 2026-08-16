using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class AddReminderCleanupDeliveredAtIndex : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(name: "IX_Reminders_DeliveredAt", table: "Reminders", column: "DeliveredAt");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(name: "IX_Reminders_DeliveredAt", table: "Reminders");
		}
	}
}
