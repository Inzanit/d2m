using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace D2M.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Property = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Threads",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AssignedDiscordChannelId = table.Column<ulong>(nullable: false),
                    OpenedByDiscordUserId = table.Column<ulong>(nullable: false),
                    OpenedDateTime = table.Column<DateTime>(nullable: false),
                    ClosedByDiscordUserId = table.Column<ulong>(nullable: false),
                    ClosedDateTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Threads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(nullable: false),
                    SentByDiscordUserId = table.Column<ulong>(nullable: false),
                    SentDateTime = table.Column<DateTime>(nullable: false),
                    ThreadId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Threads_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "Threads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "__Configurations",
                columns: new[] { "Id", "Property", "Type", "Value" },
                values: new object[] { 1, "HasDoneInitialSetUp", 1, "False" });

            migrationBuilder.InsertData(
                table: "__Configurations",
                columns: new[] { "Id", "Property", "Type", "Value" },
                values: new object[] { 2, "Prefix", 0, "?" });

            migrationBuilder.InsertData(
                table: "__Configurations",
                columns: new[] { "Id", "Property", "Type", "Value" },
                values: new object[] { 3, "IsDisabled", 1, "False" });

            migrationBuilder.InsertData(
                table: "__Configurations",
                columns: new[] { "Id", "Property", "Type", "Value" },
                values: new object[] { 4, "ParentCategoryId", 2, "" });

            migrationBuilder.InsertData(
                table: "__Configurations",
                columns: new[] { "Id", "Property", "Type", "Value" },
                values: new object[] { 5, "LogsChannelId", 2, "" });

            migrationBuilder.InsertData(
                table: "__Configurations",
                columns: new[] { "Id", "Property", "Type", "Value" },
                values: new object[] { 6, "StaffRoleId", 2, "" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ThreadId",
                table: "Messages",
                column: "ThreadId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__Configurations");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Threads");
        }
    }
}
