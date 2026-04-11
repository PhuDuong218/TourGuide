using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourGuideServer.Migrations
{
    /// <inheritdoc />
    public partial class ForceAddIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    LanguageCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LanguageName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.LanguageCode);
                });

            migrationBuilder.CreateTable(
                name: "POI",
                columns: table => new
                {
                    POIID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POI", x => x.POIID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "POI_Translations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POIID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NarrationText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POI_Translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_POI_Translations_Languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "Languages",
                        principalColumn: "LanguageCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POI_Translations_POI_POIID",
                        column: x => x.POIID,
                        principalTable: "POI",
                        principalColumn: "POIID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRCode",
                columns: table => new
                {
                    QRID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POIID = table.Column<int>(type: "int", nullable: false),
                    QRValue = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRCode", x => x.QRID);
                    table.ForeignKey(
                        name: "FK_QRCode_POI_POIID",
                        column: x => x.POIID,
                        principalTable: "POI",
                        principalColumn: "POIID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitHistory",
                columns: table => new
                {
                    VisitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    POIID = table.Column<int>(type: "int", nullable: false),
                    VisitTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScanMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserLat = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    UserLon = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    LanguageUsed = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitHistory", x => x.VisitID);
                    table.ForeignKey(
                        name: "FK_VisitHistory_POI_POIID",
                        column: x => x.POIID,
                        principalTable: "POI",
                        principalColumn: "POIID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitHistory_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_POI_Translations_LanguageCode",
                table: "POI_Translations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_POI_Translations_POIID_LanguageCode",
                table: "POI_Translations",
                columns: new[] { "POIID", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QRCode_POIID",
                table: "QRCode",
                column: "POIID");

            migrationBuilder.CreateIndex(
                name: "IX_QRCode_QRValue",
                table: "QRCode",
                column: "QRValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitHistory_POIID",
                table: "VisitHistory",
                column: "POIID");

            migrationBuilder.CreateIndex(
                name: "IX_VisitHistory_UserID",
                table: "VisitHistory",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "POI_Translations");

            migrationBuilder.DropTable(
                name: "QRCode");

            migrationBuilder.DropTable(
                name: "VisitHistory");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "POI");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
