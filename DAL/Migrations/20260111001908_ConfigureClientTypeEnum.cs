using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureClientTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_CourseAccessCodes_CourseId",
                table: "CourseAccessCodes");

            migrationBuilder.DropColumn(
                name: "LoginProvider",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "UserTokens");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserTokens",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserTokens",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "Expiration",
                table: "UserTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "UserTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "UserTokens",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TokenType",
                table: "UserTokens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "UserTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "CourseAccessCodes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDisabled",
                table: "CourseAccessCodes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CourseAccessCodes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedBySessionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_Expiration",
                table: "UserTokens",
                column: "Expiration");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_Token",
                table: "UserTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId_TokenType",
                table: "UserTokens",
                columns: new[] { "UserId", "TokenType" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccessCode_CourseId_IsUsed_IsDisabled",
                table: "CourseAccessCodes",
                columns: new[] { "CourseId", "IsUsed", "IsDisabled" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccessCode_CreatedAt",
                table: "CourseAccessCodes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccessCode_ExpiresAt",
                table: "CourseAccessCodes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccessCodes_Code",
                table: "CourseAccessCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSession_UserId_ClientType",
                table: "DeviceSessions",
                columns: new[] { "UserId", "ClientType" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSession_UserId_IsActive",
                table: "DeviceSessions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSessions_DeviceId",
                table: "DeviceSessions",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DeviceSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_Expiration",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_Token",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_UserId_TokenType",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_CourseAccessCode_CourseId_IsUsed_IsDisabled",
                table: "CourseAccessCodes");

            migrationBuilder.DropIndex(
                name: "IX_CourseAccessCode_CreatedAt",
                table: "CourseAccessCodes");

            migrationBuilder.DropIndex(
                name: "IX_CourseAccessCode_ExpiresAt",
                table: "CourseAccessCodes");

            migrationBuilder.DropIndex(
                name: "IX_CourseAccessCodes_Code",
                table: "CourseAccessCodes");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "TokenType",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "UserTokens");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "LoginProvider",
                table: "UserTokens",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "UserTokens",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "UserTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "CourseAccessCodes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDisabled",
                table: "CourseAccessCodes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CourseAccessCodes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccessCodes_CourseId",
                table: "CourseAccessCodes",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
