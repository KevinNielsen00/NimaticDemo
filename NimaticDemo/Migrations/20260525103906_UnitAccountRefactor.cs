using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class UnitAccountRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Customers_CustomerId",
                table: "Units");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Units",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "Units",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("019dc6e4-c799-7750-8d9c-d0b108b7b1c4"));

            migrationBuilder.CreateIndex(
                name: "IX_Units_AccountId",
                table: "Units",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Accounts_AccountId",
                table: "Units",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Customers_CustomerId",
                table: "Units",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Accounts_AccountId",
                table: "Units");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Customers_CustomerId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_AccountId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Units");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Units",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Customers_CustomerId",
                table: "Units",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
