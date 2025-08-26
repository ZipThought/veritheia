using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;
using Pgvector;

#nullable disable

namespace veritheia.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCorrectSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "knowledge_scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentScopeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_scopes", x => new { x.UserId, x.Id });
                    table.CheckConstraint("CK_KnowledgeScope_Type", "\"Type\" IN ('Project', 'Topic', 'Subject', 'Custom')");
                    table.ForeignKey(
                        name: "FK_knowledge_scopes_knowledge_scopes_UserId_ParentScopeId",
                        columns: x => new { x.UserId, x.ParentScopeId },
                        principalTable: "knowledge_scopes",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TriggerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Inputs = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    Configuration = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_definitions", x => new { x.UserId, x.Id });
                    table.CheckConstraint("CK_ProcessDefinition_Category", "\"Category\" IN ('Methodological', 'Developmental', 'Analytical', 'Compositional', 'Reflective')");
                    table.CheckConstraint("CK_ProcessDefinition_Trigger", "\"TriggerType\" IN ('Manual', 'UserInitiated')");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_documents_knowledge_scopes_UserId_ScopeId",
                        columns: x => new { x.UserId, x.ScopeId },
                        principalTable: "knowledge_scopes",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_documents_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "personas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ConceptualVocabulary = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    Patterns = table.Column<List<object>>(type: "jsonb", nullable: false),
                    MethodologicalPreferences = table.Column<List<object>>(type: "jsonb", nullable: false),
                    Markers = table.Column<List<object>>(type: "jsonb", nullable: false),
                    LastEvolved = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personas", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_personas_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_capabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_capabilities", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_process_capabilities_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Authors = table.Column<string[]>(type: "text[]", nullable: true),
                    Abstract = table.Column<string>(type: "text", nullable: true),
                    Keywords = table.Column<string[]>(type: "text[]", nullable: true),
                    PublicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Publisher = table.Column<string>(type: "text", nullable: true),
                    DOI = table.Column<string>(type: "text", nullable: true),
                    ExtendedMetadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_metadata", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_document_metadata_documents_UserId_DocumentId",
                        columns: x => new { x.UserId, x.DocumentId },
                        principalTable: "documents",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_metadata_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Context = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journeys", x => new { x.UserId, x.Id });
                    table.CheckConstraint("CK_Journey_State", "\"State\" IN ('Active', 'Paused', 'Completed', 'Abandoned')");
                    table.ForeignKey(
                        name: "FK_journeys_personas_UserId_PersonaId",
                        columns: x => new { x.UserId, x.PersonaId },
                        principalTable: "personas",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_journeys_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsShareable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journals", x => new { x.UserId, x.Id });
                    table.CheckConstraint("CK_Journal_Type", "\"Type\" IN ('Research', 'Method', 'Decision', 'Reflection')");
                    table.ForeignKey(
                        name: "FK_journals_journeys_UserId_JourneyId",
                        columns: x => new { x.UserId, x.JourneyId },
                        principalTable: "journeys",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_document_segments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceIndex = table.Column<int>(type: "integer", nullable: false),
                    SegmentContent = table.Column<string>(type: "text", nullable: false),
                    SegmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StructuralPath = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    ByteRange = table.Column<NpgsqlRange<int>>(type: "int4range", nullable: true),
                    CreatedByRule = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedForQuestion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_document_segments", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_journey_document_segments_documents_UserId_DocumentId",
                        columns: x => new { x.UserId, x.DocumentId },
                        principalTable: "documents",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_journey_document_segments_journeys_UserId_JourneyId",
                        columns: x => new { x.UserId, x.JourneyId },
                        principalTable: "journeys",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_formations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsightType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InsightContent = table.Column<string>(type: "text", nullable: false),
                    FormedFromSegments = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    FormedThroughQuestions = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_formations", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_journey_formations_journeys_UserId_JourneyId",
                        columns: x => new { x.UserId, x.JourneyId },
                        principalTable: "journeys",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_frameworks",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FrameworkElements = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    ProjectionRules = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_frameworks", x => new { x.UserId, x.JourneyId });
                    table.ForeignKey(
                        name: "FK_journey_frameworks_journeys_UserId_JourneyId",
                        columns: x => new { x.UserId, x.JourneyId },
                        principalTable: "journeys",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Inputs = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_executions", x => new { x.UserId, x.Id });
                    table.CheckConstraint("CK_ProcessExecution_State", "\"State\" IN ('Pending', 'Running', 'Completed', 'Failed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_process_executions_journeys_UserId_JourneyId",
                        columns: x => new { x.UserId, x.JourneyId },
                        principalTable: "journeys",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Significance = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: true),
                    Metadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_entries", x => new { x.UserId, x.Id });
                    table.CheckConstraint("CK_JournalEntry_Significance", "\"Significance\" IN ('Routine', 'Notable', 'Critical', 'Milestone')");
                    table.ForeignKey(
                        name: "FK_journal_entries_journals_UserId_JournalId",
                        columns: x => new { x.UserId, x.JournalId },
                        principalTable: "journals",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_segment_assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    Reasoning = table.Column<string>(type: "text", nullable: false),
                    RubricScores = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    ReasoningChain = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    AssessedByModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_segment_assessments", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_journey_segment_assessments_journey_document_segments_UserI~",
                        columns: x => new { x.UserId, x.SegmentId },
                        principalTable: "journey_document_segments",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_indexes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VectorModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_indexes", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_search_indexes_journey_document_segments_UserId_SegmentId",
                        columns: x => new { x.UserId, x.SegmentId },
                        principalTable: "journey_document_segments",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_document_processing_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentIndex = table.Column<int>(type: "integer", nullable: false),
                    DocumentIdentifier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DocumentTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Authors = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Venue = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    WasProcessedSuccessfully = table.Column<bool>(type: "boolean", nullable: false),
                    FailureStage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FormationImpact = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ProcessingResults = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    AssessedResearchQuestions = table.Column<List<int>>(type: "jsonb", nullable: true),
                    UnassessedResearchQuestions = table.Column<List<int>>(type: "jsonb", nullable: true),
                    MustRead = table.Column<bool>(type: "boolean", nullable: true),
                    ExtractedTopics = table.Column<List<string>>(type: "jsonb", nullable: true),
                    ExtractedEntities = table.Column<List<string>>(type: "jsonb", nullable: true),
                    Keywords = table.Column<List<string>>(type: "jsonb", nullable: true),
                    ProcessingStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessingCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_document_processing_records", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_journey_document_processing_records_journeys_UserId_Journey~",
                        columns: x => new { x.UserId, x.JourneyId },
                        principalTable: "journeys",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_journey_document_processing_records_process_executions_User~",
                        columns: x => new { x.UserId, x.ProcessExecutionId },
                        principalTable: "process_executions",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_journey_document_processing_records_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Data = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    Metadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_results", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_process_results_process_executions_UserId_ExecutionId",
                        columns: x => new { x.UserId, x.ExecutionId },
                        principalTable: "process_executions",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_vectors_1536",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndexId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_vectors_1536", x => new { x.UserId, x.IndexId });
                    table.ForeignKey(
                        name: "FK_search_vectors_1536_search_indexes_UserId_IndexId",
                        columns: x => new { x.UserId, x.IndexId },
                        principalTable: "search_indexes",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_vectors_384",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndexId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(384)", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_vectors_384", x => new { x.UserId, x.IndexId });
                    table.ForeignKey(
                        name: "FK_search_vectors_384_search_indexes_UserId_IndexId",
                        columns: x => new { x.UserId, x.IndexId },
                        principalTable: "search_indexes",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_vectors_768",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndexId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(768)", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_vectors_768", x => new { x.UserId, x.IndexId });
                    table.ForeignKey(
                        name: "FK_search_vectors_768_search_indexes_UserId_IndexId",
                        columns: x => new { x.UserId, x.IndexId },
                        principalTable: "search_indexes",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_metadata_UserId_CreatedAt",
                table: "document_metadata",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_document_metadata_UserId_DocumentId",
                table: "document_metadata",
                columns: new[] { "UserId", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_UserId_CreatedAt",
                table: "documents",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_UserId_FileName",
                table: "documents",
                columns: new[] { "UserId", "FileName" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_UserId_MimeType",
                table: "documents",
                columns: new[] { "UserId", "MimeType" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_UserId_ScopeId",
                table: "documents",
                columns: new[] { "UserId", "ScopeId" });

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_UserId_CreatedAt",
                table: "journal_entries",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_UserId_JournalId",
                table: "journal_entries",
                columns: new[] { "UserId", "JournalId" });

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_UserId_Significance",
                table: "journal_entries",
                columns: new[] { "UserId", "Significance" });

            migrationBuilder.CreateIndex(
                name: "IX_journals_UserId_CreatedAt",
                table: "journals",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_journals_UserId_JourneyId",
                table: "journals",
                columns: new[] { "UserId", "JourneyId" });

            migrationBuilder.CreateIndex(
                name: "IX_journals_UserId_Type",
                table: "journals",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_processing_records_UserId_IsReviewed",
                table: "journey_document_processing_records",
                columns: new[] { "UserId", "IsReviewed" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_processing_records_UserId_JourneyId_MustRe~",
                table: "journey_document_processing_records",
                columns: new[] { "UserId", "JourneyId", "MustRead" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_processing_records_UserId_JourneyId_Proces~",
                table: "journey_document_processing_records",
                columns: new[] { "UserId", "JourneyId", "ProcessExecutionId" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_processing_records_UserId_JourneyId_WasPro~",
                table: "journey_document_processing_records",
                columns: new[] { "UserId", "JourneyId", "WasProcessedSuccessfully" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_processing_records_UserId_ProcessExecution~",
                table: "journey_document_processing_records",
                columns: new[] { "UserId", "ProcessExecutionId" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_segments_UserId_CreatedAt",
                table: "journey_document_segments",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_segments_UserId_DocumentId",
                table: "journey_document_segments",
                columns: new[] { "UserId", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_segments_UserId_JourneyId",
                table: "journey_document_segments",
                columns: new[] { "UserId", "JourneyId" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_segments_UserId_JourneyId_DocumentId_Seque~",
                table: "journey_document_segments",
                columns: new[] { "UserId", "JourneyId", "DocumentId", "SequenceIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journey_formations_UserId_CreatedAt",
                table: "journey_formations",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_formations_UserId_InsightType",
                table: "journey_formations",
                columns: new[] { "UserId", "InsightType" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_formations_UserId_JourneyId",
                table: "journey_formations",
                columns: new[] { "UserId", "JourneyId" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_frameworks_UserId_CreatedAt",
                table: "journey_frameworks",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_frameworks_UserId_JourneyType",
                table: "journey_frameworks",
                columns: new[] { "UserId", "JourneyType" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_segment_assessments_UserId_AssessmentType",
                table: "journey_segment_assessments",
                columns: new[] { "UserId", "AssessmentType" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_segment_assessments_UserId_CreatedAt",
                table: "journey_segment_assessments",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_segment_assessments_UserId_SegmentId",
                table: "journey_segment_assessments",
                columns: new[] { "UserId", "SegmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_journeys_UserId_CreatedAt",
                table: "journeys",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_journeys_UserId_PersonaId",
                table: "journeys",
                columns: new[] { "UserId", "PersonaId" });

            migrationBuilder.CreateIndex(
                name: "IX_journeys_UserId_ProcessType",
                table: "journeys",
                columns: new[] { "UserId", "ProcessType" });

            migrationBuilder.CreateIndex(
                name: "IX_journeys_UserId_State",
                table: "journeys",
                columns: new[] { "UserId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_scopes_UserId_CreatedAt",
                table: "knowledge_scopes",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_scopes_UserId_ParentScopeId",
                table: "knowledge_scopes",
                columns: new[] { "UserId", "ParentScopeId" });

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_scopes_UserId_Type",
                table: "knowledge_scopes",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_personas_UserId_CreatedAt",
                table: "personas",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_personas_UserId_Domain",
                table: "personas",
                columns: new[] { "UserId", "Domain" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personas_UserId_IsActive",
                table: "personas",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_process_capabilities_UserId_CreatedAt",
                table: "process_capabilities",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_process_capabilities_UserId_ProcessType",
                table: "process_capabilities",
                columns: new[] { "UserId", "ProcessType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_process_definitions_UserId_Category",
                table: "process_definitions",
                columns: new[] { "UserId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_process_definitions_UserId_CreatedAt",
                table: "process_definitions",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_process_definitions_UserId_ProcessType",
                table: "process_definitions",
                columns: new[] { "UserId", "ProcessType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_process_executions_UserId_CreatedAt",
                table: "process_executions",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_process_executions_UserId_JourneyId",
                table: "process_executions",
                columns: new[] { "UserId", "JourneyId" });

            migrationBuilder.CreateIndex(
                name: "IX_process_executions_UserId_State",
                table: "process_executions",
                columns: new[] { "UserId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_process_results_UserId_CreatedAt",
                table: "process_results",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_process_results_UserId_ExecutionId",
                table: "process_results",
                columns: new[] { "UserId", "ExecutionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_indexes_UserId_CreatedAt",
                table: "search_indexes",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_search_indexes_UserId_SegmentId_VectorModel",
                table: "search_indexes",
                columns: new[] { "UserId", "SegmentId", "VectorModel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_vectors_1536_UserId_CreatedAt",
                table: "search_vectors_1536",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_search_vectors_384_UserId_CreatedAt",
                table: "search_vectors_384",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_search_vectors_768_UserId_CreatedAt",
                table: "search_vectors_768",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_metadata");

            migrationBuilder.DropTable(
                name: "journal_entries");

            migrationBuilder.DropTable(
                name: "journey_document_processing_records");

            migrationBuilder.DropTable(
                name: "journey_formations");

            migrationBuilder.DropTable(
                name: "journey_frameworks");

            migrationBuilder.DropTable(
                name: "journey_segment_assessments");

            migrationBuilder.DropTable(
                name: "process_capabilities");

            migrationBuilder.DropTable(
                name: "process_definitions");

            migrationBuilder.DropTable(
                name: "process_results");

            migrationBuilder.DropTable(
                name: "search_vectors_1536");

            migrationBuilder.DropTable(
                name: "search_vectors_384");

            migrationBuilder.DropTable(
                name: "search_vectors_768");

            migrationBuilder.DropTable(
                name: "journals");

            migrationBuilder.DropTable(
                name: "process_executions");

            migrationBuilder.DropTable(
                name: "search_indexes");

            migrationBuilder.DropTable(
                name: "journey_document_segments");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "journeys");

            migrationBuilder.DropTable(
                name: "knowledge_scopes");

            migrationBuilder.DropTable(
                name: "personas");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
