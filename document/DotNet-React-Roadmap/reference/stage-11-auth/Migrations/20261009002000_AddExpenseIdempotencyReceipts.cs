using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stage11Auth.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseIdempotencyReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpenseIdempotencyReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NormalizedKey = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false, collation: "Latin1_General_100_BIN2"),
                    RequestHash = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, nullable: false),
                    ExpenseId = table.Column<int>(type: "int", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OriginalNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseIdempotencyReceipts", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_ExpenseIdempotencyReceipts_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseIdempotencyReceipts_ExpenseId",
                table: "ExpenseIdempotencyReceipts",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "UX_ExpenseIdempotencyReceipts_Owner_Key",
                table: "ExpenseIdempotencyReceipts",
                columns: new[] { "OwnerUserId", "NormalizedKey" },
                unique: true)
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Receipt migration rollback is intentionally refused.");
        }
    }
}

