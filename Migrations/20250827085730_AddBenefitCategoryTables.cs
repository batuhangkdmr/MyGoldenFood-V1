using Microsoft.EntityFrameworkCore.Migrations;

namespace MyGoldenFood.Migrations
{
    public partial class AddBenefitCategoryTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BenefitCategoryId",
                table: "Benefits",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BenefitCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    ImagePath = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenefitCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BenefitCategoryTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BenefitCategoryId = table.Column<int>(nullable: false),
                    Language = table.Column<string>(maxLength: 255, nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenefitCategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BenefitCategoryTranslations_BenefitCategories_BenefitCategoryId",
                        column: x => x.BenefitCategoryId,
                        principalTable: "BenefitCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Benefits_BenefitCategoryId",
                table: "Benefits",
                column: "BenefitCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BenefitCategoryTranslations_BenefitCategoryId",
                table: "BenefitCategoryTranslations",
                column: "BenefitCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Benefits_BenefitCategories_BenefitCategoryId",
                table: "Benefits",
                column: "BenefitCategoryId",
                principalTable: "BenefitCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Benefits_BenefitCategories_BenefitCategoryId",
                table: "Benefits");

            migrationBuilder.DropTable(
                name: "BenefitCategoryTranslations");

            migrationBuilder.DropTable(
                name: "BenefitCategories");

            migrationBuilder.DropIndex(
                name: "IX_Benefits_BenefitCategoryId",
                table: "Benefits");

            migrationBuilder.DropColumn(
                name: "BenefitCategoryId",
                table: "Benefits");
        }
    }
}
