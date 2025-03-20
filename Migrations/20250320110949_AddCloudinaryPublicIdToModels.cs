using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgitimSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudinaryPublicIdToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SiteSettings_ActiveLayout",
                table: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_Kurslar_IsActive",
                table: "Kurslar");

            migrationBuilder.DropIndex(
                name: "IX_Kurslar_IsActive_Order",
                table: "Kurslar");

            migrationBuilder.DropIndex(
                name: "IX_Kurslar_Type",
                table: "Kurslar");

            migrationBuilder.AddColumn<string>(
                name: "CloudinaryPublicId",
                table: "SiteSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Kurslar",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "CloudinaryPublicId",
                table: "Kurslar",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CloudinaryPublicId",
                table: "Kadromuz",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CloudinaryPublicId",
                table: "Hakkimizda",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CloudinaryPublicId",
                table: "Duyurular",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CloudinaryPublicId",
                table: "Banners",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "IsImageRight",
                table: "AnasayfaIcerik",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AnasayfaIcerik",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<string>(
                name: "CloudinaryPublicId",
                table: "AnasayfaIcerik",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloudinaryPublicId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "CloudinaryPublicId",
                table: "Kurslar");

            migrationBuilder.DropColumn(
                name: "CloudinaryPublicId",
                table: "Kadromuz");

            migrationBuilder.DropColumn(
                name: "CloudinaryPublicId",
                table: "Hakkimizda");

            migrationBuilder.DropColumn(
                name: "CloudinaryPublicId",
                table: "Duyurular");

            migrationBuilder.DropColumn(
                name: "CloudinaryPublicId",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "CloudinaryPublicId",
                table: "AnasayfaIcerik");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Kurslar",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsImageRight",
                table: "AnasayfaIcerik",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AnasayfaIcerik",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_ActiveLayout",
                table: "SiteSettings",
                column: "ActiveLayout");

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
        }
    }
}
