using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Catalog_20260114_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_ISBN",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "EstimatedReadingTimeMinutes",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.RenameIndex(
                name: "UX_Pages_ChapterId_PageNumber",
                schema: "Catalog",
                table: "Pages",
                newName: "IX_Pages_ChapterId_PageNumber");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                schema: "Catalog",
                table: "Books",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                schema: "Catalog",
                table: "Books",
                newName: "Language");

            migrationBuilder.RenameIndex(
                name: "IX_Authors_Email",
                schema: "Catalog",
                table: "Authors",
                newName: "UX_Authors_Email");

            migrationBuilder.AlterColumn<string>(
                name: "VisualizationPrompts",
                schema: "Catalog",
                table: "Pages",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AuthorVisualizationHint",
                schema: "Catalog",
                table: "Pages",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisualizationPoint",
                schema: "Catalog",
                table: "Pages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VisualizationGeneratedAt",
                schema: "Catalog",
                table: "Pages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisualizationImageUrl",
                schema: "Catalog",
                table: "Pages",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VisualizationJobId",
                schema: "Catalog",
                table: "Pages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisualizationStatus",
                schema: "Catalog",
                table: "Pages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VisualizationThumbnailUrl",
                schema: "Catalog",
                table: "Pages",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Subtitle",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Genres",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CopyrightStatus",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CoverAltText",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverImage_Source",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverThumbnailUrl",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExternalImportedAt",
                schema: "Catalog",
                table: "Books",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExternalLastSyncedAt",
                schema: "Catalog",
                table: "Books",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalSourceType",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSourceUrl",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullTextUrl",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GutenbergId",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasFullText",
                schema: "Catalog",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Metadata_WordCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OpenLibraryEditionId",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenLibraryWorkId",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalPublicationYear",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalTitle",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ReadingDifficulty",
                schema: "Catalog",
                table: "Books",
                type: "float(5)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Stats_AverageRating",
                schema: "Catalog",
                table: "Books",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Stats_CompletedReadCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_DownloadCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_FavoriteCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_RatingCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_ReviewCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_ViewCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stats_VisualizationCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "VS_AllowReaderChoice",
                schema: "Catalog",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VS_AllowedModes",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VS_AutoGenerateOnPublish",
                schema: "Catalog",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VS_IsEnabled",
                schema: "Catalog",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VS_MaxImagesPerPage",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VS_PreferredProvider",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VS_PreferredStyle",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VS_PrimaryMode",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WordCount",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Author_GutenbergAuthorId",
                schema: "Catalog",
                table: "Authors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Author_WikipediaUrl",
                schema: "Catalog",
                table: "Authors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                schema: "Catalog",
                table: "Authors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                schema: "Catalog",
                table: "Authors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeathYear",
                schema: "Catalog",
                table: "Authors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GutenbergAuthorId",
                schema: "Catalog",
                table: "Authors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                schema: "Catalog",
                table: "Authors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenLibraryAuthorId",
                schema: "Catalog",
                table: "Authors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "Catalog",
                table: "Authors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WikidataId",
                schema: "Catalog",
                table: "Authors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WikipediaUrl",
                schema: "Catalog",
                table: "Authors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Subjects",
                schema: "Catalog",
                columns: table => new
                {
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubjectType = table.Column<int>(type: "int", nullable: false),
                    ParentSubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExternalMapping = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BookCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectId);
                    table.ForeignKey(
                        name: "FK_Subjects_Subjects_ParentSubjectId",
                        column: x => x.ParentSubjectId,
                        principalSchema: "Catalog",
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_IsVisualizationPoint",
                schema: "Catalog",
                table: "Pages",
                column: "IsVisualizationPoint",
                filter: "[IsVisualizationPoint] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_VisualizationStatus",
                schema: "Catalog",
                table: "Pages",
                column: "VisualizationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CopyrightStatus",
                schema: "Catalog",
                table: "Books",
                column: "CopyrightStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Source",
                schema: "Catalog",
                table: "Books",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_BirthYear",
                schema: "Catalog",
                table: "Authors",
                column: "BirthYear");

            migrationBuilder.CreateIndex(
                name: "UX_Authors_GutenbergAuthorId",
                schema: "Catalog",
                table: "Authors",
                column: "Author_GutenbergAuthorId",
                unique: true,
                filter: "[GutenbergAuthorId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_ExternalMapping",
                schema: "Catalog",
                table: "Subjects",
                column: "ExternalMapping");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Name",
                schema: "Catalog",
                table: "Subjects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_ParentId",
                schema: "Catalog",
                table: "Subjects",
                column: "ParentSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Type",
                schema: "Catalog",
                table: "Subjects",
                column: "SubjectType");

            migrationBuilder.CreateIndex(
                name: "UX_Subjects_Slug",
                schema: "Catalog",
                table: "Subjects",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Authors_AuthorId",
                schema: "Catalog",
                table: "Books",
                column: "AuthorId",
                principalSchema: "Catalog",
                principalTable: "Authors",
                principalColumn: "AuthorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Authors_AuthorId",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropTable(
                name: "Subjects",
                schema: "Catalog");

            migrationBuilder.DropIndex(
                name: "IX_Pages_IsVisualizationPoint",
                schema: "Catalog",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_VisualizationStatus",
                schema: "Catalog",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Books_CopyrightStatus",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_Source",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Authors_BirthYear",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropIndex(
                name: "UX_Authors_GutenbergAuthorId",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "AuthorVisualizationHint",
                schema: "Catalog",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "IsVisualizationPoint",
                schema: "Catalog",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "VisualizationGeneratedAt",
                schema: "Catalog",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "VisualizationImageUrl",
                schema: "Catalog",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "VisualizationJobId",
                schema: "Catalog",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "VisualizationStatus",
                schema: "Catalog",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "VisualizationThumbnailUrl",
                schema: "Catalog",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "CopyrightStatus",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "CoverAltText",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "CoverImage_Source",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "CoverThumbnailUrl",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "DownloadCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ExternalImportedAt",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ExternalLastSyncedAt",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ExternalSourceType",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ExternalSourceUrl",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "FullTextUrl",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "GutenbergId",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "HasFullText",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Metadata_WordCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "OpenLibraryEditionId",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "OpenLibraryWorkId",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "OriginalPublicationYear",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "OriginalTitle",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ReadingDifficulty",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Stats_AverageRating",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Stats_CompletedReadCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Stats_DownloadCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Stats_FavoriteCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Stats_RatingCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Stats_ReviewCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Stats_ViewCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Stats_VisualizationCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "VS_AllowReaderChoice",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "VS_AllowedModes",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "VS_AutoGenerateOnPublish",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "VS_IsEnabled",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "VS_MaxImagesPerPage",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "VS_PreferredProvider",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "VS_PreferredStyle",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "VS_PrimaryMode",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "WordCount",
                schema: "Catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Author_GutenbergAuthorId",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "Author_WikipediaUrl",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "BirthYear",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "DeathYear",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "GutenbergAuthorId",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "Nationality",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "OpenLibraryAuthorId",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "WikidataId",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "WikipediaUrl",
                schema: "Catalog",
                table: "Authors");

            migrationBuilder.RenameIndex(
                name: "IX_Pages_ChapterId_PageNumber",
                schema: "Catalog",
                table: "Pages",
                newName: "UX_Pages_ChapterId_PageNumber");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "Catalog",
                table: "Books",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "Language",
                schema: "Catalog",
                table: "Books",
                newName: "LanguageId");

            migrationBuilder.RenameIndex(
                name: "UX_Authors_Email",
                schema: "Catalog",
                table: "Authors",
                newName: "IX_Authors_Email");

            migrationBuilder.AlterColumn<string>(
                name: "VisualizationPrompts",
                schema: "Catalog",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Subtitle",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Genres",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Catalog",
                table: "Books",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 10000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LanguageId",
                schema: "Catalog",
                table: "Books",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<double>(
                name: "EstimatedReadingTimeMinutes",
                schema: "Catalog",
                table: "Books",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN",
                schema: "Catalog",
                table: "Books",
                column: "ISBN",
                unique: true,
                filter: "[ISBN] IS NOT NULL");
        }
    }
}
