using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _100alpha1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "broadcasts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "用户ID"),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "消息内容"),
                    Valid = table.Column<bool>(type: "boolean", nullable: false, comment: "是否有效"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "过期时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broadcasts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "用户ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "用户名"),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "邮箱"),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "出生日期"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "最后登录时间"),
                    LastReadBroadcastAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "最后看公告时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_broadcasts_UserId",
                table: "broadcasts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_broadcasts_Valid_ExpirationDate",
                table: "broadcasts",
                columns: new[] { "Valid", "ExpirationDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "broadcasts");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
