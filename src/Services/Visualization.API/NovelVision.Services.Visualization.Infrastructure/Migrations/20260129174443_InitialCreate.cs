using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelVision.Services.Visualization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "visualization");

            migrationBuilder.CreateTable(
                name: "VisualizationJobs",
                schema: "visualization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Trigger = table.Column<int>(type: "int", nullable: false),
                    PreferredProvider = table.Column<int>(type: "int", nullable: false),
                    Parameters_Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Parameters_Quality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Parameters_AspectRatio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Parameters_Seed = table.Column<int>(type: "int", nullable: true),
                    Parameters_Steps = table.Column<int>(type: "int", nullable: true),
                    Parameters_CfgScale = table.Column<double>(type: "float", nullable: true),
                    Parameters_Sampler = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Parameters_Upscale = table.Column<bool>(type: "bit", nullable: false),
                    TextSelection_SelectedText = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    TextSelection_StartPosition = table.Column<int>(type: "int", nullable: true),
                    TextSelection_EndPosition = table.Column<int>(type: "int", nullable: true),
                    TextSelection_PageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TextSelection_ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TextSelection_ContextBefore = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TextSelection_ContextAfter = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ProcessingStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalJobId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PromptData_OriginalText = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    PromptData_EnhancedPrompt = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    PromptData_NegativePrompt = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    PromptData_TargetModel = table.Column<int>(type: "int", nullable: true),
                    PromptData_Style = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PromptData_Parameters = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualizationJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedImages",
                schema: "visualization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Format = table.Column<int>(type: "int", nullable: false),
                    BlobPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PromptData_OriginalText = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    PromptData_EnhancedPrompt = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    PromptData_NegativePrompt = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    PromptData_TargetModel = table.Column<int>(type: "int", nullable: false),
                    PromptData_Style = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PromptData_Parameters = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    ExternalJobId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedImages_VisualizationJobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "visualization",
                        principalTable: "VisualizationJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedImages_GeneratedAt",
                schema: "visualization",
                table: "GeneratedImages",
                column: "GeneratedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedImages_IsSelected",
                schema: "visualization",
                table: "GeneratedImages",
                column: "IsSelected",
                filter: "[IsSelected] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedImages_JobId",
                schema: "visualization",
                table: "GeneratedImages",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_VisualizationJobs_BookId",
                schema: "visualization",
                table: "VisualizationJobs",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_VisualizationJobs_CreatedAt",
                schema: "visualization",
                table: "VisualizationJobs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_VisualizationJobs_PageId",
                schema: "visualization",
                table: "VisualizationJobs",
                column: "PageId",
                filter: "[PageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VisualizationJobs_Queue",
                schema: "visualization",
                table: "VisualizationJobs",
                columns: new[] { "Status", "Priority", "CreatedAt" },
                descending: new[] { false, true, false });

            migrationBuilder.CreateIndex(
                name: "IX_VisualizationJobs_Status",
                schema: "visualization",
                table: "VisualizationJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VisualizationJobs_UserId",
                schema: "visualization",
                table: "VisualizationJobs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedImages",
                schema: "visualization");

            migrationBuilder.DropTable(
                name: "VisualizationJobs",
                schema: "visualization");
        }
    }
}
