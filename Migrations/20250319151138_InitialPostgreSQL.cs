using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EgitimSitesi.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnasayfaIcerik",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImagePath = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ButtonText = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ButtonUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IconClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsImageRight = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnasayfaIcerik", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImagePath = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ButtonText = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ButtonUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BizeYazin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdSoyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TelefonNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Konu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Mesaj = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Okundu = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IpAdresi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BizeYazin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Duyurular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IconClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ButtonText = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ButtonUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    AnnouncementType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DetailContent = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duyurular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gallery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImagePath = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CloudinaryPublicId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gallery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hakkimizda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImagePath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Tarihcemiz = table.Column<string>(type: "text", nullable: false),
                    Vizyonumuz = table.Column<string>(type: "text", nullable: false),
                    Misyonumuz = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hakkimizda", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Iletisim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MerkezSube = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Adress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TelNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GoogleMapsEmbed = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CalismaSaatleri = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FacebookUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InstagramUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    YouTubeUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iletisim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kadromuz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<int>(type: "integer", nullable: false),
                    Field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Exp = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kadromuz", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KayitFormu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TelNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Grade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KayitFormu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kurslar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kurslar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActiveLayout = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TelNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WorkHours = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    MapUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ZoomLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subeler", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Banners_IsActive",
                table: "Banners",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Order",
                table: "Banners",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_BizeYazin_CreationDate",
                table: "BizeYazin",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_BizeYazin_Okundu",
                table: "BizeYazin",
                column: "Okundu");

            migrationBuilder.CreateIndex(
                name: "IX_Duyurular_IsActive_CreationDate",
                table: "Duyurular",
                columns: new[] { "IsActive", "CreationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Gallery_IsActive",
                table: "Gallery",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Gallery_IsActive_Type_Order",
                table: "Gallery",
                columns: new[] { "IsActive", "Type", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Gallery_Type",
                table: "Gallery",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Hakkimizda_IsActive",
                table: "Hakkimizda",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Iletisim_IsActive",
                table: "Iletisim",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kadromuz_Field",
                table: "Kadromuz",
                column: "Field");

            migrationBuilder.CreateIndex(
                name: "IX_Kadromuz_IsActive",
                table: "Kadromuz",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KayitFormu_CreationDate",
                table: "KayitFormu",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_KayitFormu_Email",
                table: "KayitFormu",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Kurslar_IsActive",
                table: "Kurslar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kurslar_IsActive_Order",
                table: "Kurslar",
                columns: new[] { "IsActive", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Kurslar_Type",
                table: "Kurslar",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_ActiveLayout",
                table: "SiteSettings",
                column: "ActiveLayout");

            migrationBuilder.CreateIndex(
                name: "IX_Subeler_Latitude_Longitude",
                table: "Subeler",
                columns: new[] { "Latitude", "Longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnasayfaIcerik");

            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.DropTable(
                name: "BizeYazin");

            migrationBuilder.DropTable(
                name: "Duyurular");

            migrationBuilder.DropTable(
                name: "Gallery");

            migrationBuilder.DropTable(
                name: "Hakkimizda");

            migrationBuilder.DropTable(
                name: "Iletisim");

            migrationBuilder.DropTable(
                name: "Kadromuz");

            migrationBuilder.DropTable(
                name: "KayitFormu");

            migrationBuilder.DropTable(
                name: "Kurslar");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "Subeler");
        }
    }
}
