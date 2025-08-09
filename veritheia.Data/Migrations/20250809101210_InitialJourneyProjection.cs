using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;
using Pgvector;

#nullable disable

namespace veritheia.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialJourneyProjection : Migration
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
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentScopeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_scopes", x => x.Id);
                    table.CheckConstraint("CK_KnowledgeScope_Type", "\"Type\" IN ('Project', 'Topic', 'Subject', 'Custom')");
                    table.ForeignKey(
                        name: "FK_knowledge_scopes_knowledge_scopes_ParentScopeId",
                        column: x => x.ParentScopeId,
                        principalTable: "knowledge_scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TriggerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Inputs = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    Configuration = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_definitions", x => x.Id);
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
                    table.PrimaryKey("PK_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_documents_knowledge_scopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "knowledge_scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    table.PrimaryKey("PK_personas", x => x.Id);
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
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_capabilities", x => x.Id);
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
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Authors = table.Column<List<string>>(type: "text[]", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtendedMetadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_metadata_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "documents",
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
                    table.PrimaryKey("PK_journeys", x => x.Id);
                    table.CheckConstraint("CK_Journey_State", "\"State\" IN ('Active', 'Paused', 'Completed', 'Abandoned')");
                    table.ForeignKey(
                        name: "FK_journeys_personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "personas",
                        principalColumn: "Id",
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
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsShareable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journals", x => x.Id);
                    table.CheckConstraint("CK_Journal_Type", "\"Type\" IN ('Research', 'Method', 'Decision', 'Reflection')");
                    table.ForeignKey(
                        name: "FK_journals_journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_document_segments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentContent = table.Column<string>(type: "text", nullable: false),
                    SegmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SegmentPurpose = table.Column<string>(type: "text", nullable: true),
                    StructuralPath = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    SequenceIndex = table.Column<int>(type: "integer", nullable: false),
                    ByteRange = table.Column<NpgsqlRange<int>>(type: "int4range", nullable: true),
                    CreatedByRule = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedForQuestion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_document_segments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_journey_document_segments_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_journey_document_segments_journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_formations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsightType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InsightContent = table.Column<string>(type: "text", nullable: false),
                    FormedFromSegments = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    FormedThroughQuestions = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    FormationReasoning = table.Column<string>(type: "text", nullable: true),
                    FormationMarker = table.Column<string>(type: "text", nullable: true),
                    FormedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_formations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_journey_formations_journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_frameworks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FrameworkElements = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    ProjectionRules = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_frameworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_journey_frameworks_journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Inputs = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_executions", x => x.Id);
                    table.CheckConstraint("CK_ProcessExecution_State", "\"State\" IN ('Pending', 'Running', 'Completed', 'Failed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_process_executions_journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Significance = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    Metadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_entries", x => x.Id);
                    table.CheckConstraint("CK_JournalEntry_Significance", "\"Significance\" IN ('Routine', 'Notable', 'Critical', 'Milestone')");
                    table.ForeignKey(
                        name: "FK_journal_entries_journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_segment_assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResearchQuestionId = table.Column<int>(type: "integer", nullable: true),
                    RelevanceScore = table.Column<float>(type: "real", nullable: true),
                    ContributionScore = table.Column<float>(type: "real", nullable: true),
                    RubricScores = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    AssessmentReasoning = table.Column<string>(type: "text", nullable: true),
                    ReasoningChain = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    AssessedByModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_segment_assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_journey_segment_assessments_journey_document_segments_Segme~",
                        column: x => x.SegmentId,
                        principalTable: "journey_document_segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_indexes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VectorModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VectorDimension = table.Column<int>(type: "integer", nullable: false),
                    IndexedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_indexes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_search_indexes_journey_document_segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "journey_document_segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Data = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    Metadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_process_results_process_executions_ExecutionId",
                        column: x => x.ExecutionId,
                        principalTable: "process_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_vectors_1536",
                columns: table => new
                {
                    IndexId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_vectors_1536", x => x.IndexId);
                    table.ForeignKey(
                        name: "FK_search_vectors_1536_search_indexes_IndexId",
                        column: x => x.IndexId,
                        principalTable: "search_indexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_vectors_384",
                columns: table => new
                {
                    IndexId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(384)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_vectors_384", x => x.IndexId);
                    table.ForeignKey(
                        name: "FK_search_vectors_384_search_indexes_IndexId",
                        column: x => x.IndexId,
                        principalTable: "search_indexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_vectors_768",
                columns: table => new
                {
                    IndexId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(768)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_vectors_768", x => x.IndexId);
                    table.ForeignKey(
                        name: "FK_search_vectors_768_search_indexes_IndexId",
                        column: x => x.IndexId,
                        principalTable: "search_indexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_metadata_DocumentId",
                table: "document_metadata",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_ScopeId",
                table: "documents",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_JournalId",
                table: "journal_entries",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_journals_JourneyId",
                table: "journals",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_segments_DocumentId",
                table: "journey_document_segments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_journey_document_segments_JourneyId_DocumentId_SequenceIndex",
                table: "journey_document_segments",
                columns: new[] { "JourneyId", "DocumentId", "SequenceIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journey_formations_JourneyId",
                table: "journey_formations",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_journey_frameworks_JourneyId",
                table: "journey_frameworks",
                column: "JourneyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journey_segment_assessments_SegmentId",
                table: "journey_segment_assessments",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_journeys_PersonaId",
                table: "journeys",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_journeys_UserId",
                table: "journeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_scopes_ParentScopeId",
                table: "knowledge_scopes",
                column: "ParentScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_personas_UserId_Domain",
                table: "personas",
                columns: new[] { "UserId", "Domain" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_process_capabilities_UserId_ProcessType",
                table: "process_capabilities",
                columns: new[] { "UserId", "ProcessType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_process_definitions_ProcessType",
                table: "process_definitions",
                column: "ProcessType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_process_executions_JourneyId",
                table: "process_executions",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_process_results_ExecutionId",
                table: "process_results",
                column: "ExecutionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_indexes_SegmentId_VectorModel",
                table: "search_indexes",
                columns: new[] { "SegmentId", "VectorModel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            // Add HNSW indexes for vector search
            migrationBuilder.Sql(@"
                CREATE INDEX idx_vectors_1536_hnsw ON search_vectors_1536 
                USING hnsw (""Embedding"" vector_cosine_ops)
                WITH (m = 16, ef_construction = 64);
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX idx_vectors_768_hnsw ON search_vectors_768 
                USING hnsw (""Embedding"" vector_cosine_ops)
                WITH (m = 16, ef_construction = 64);
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX idx_vectors_384_hnsw ON search_vectors_384 
                USING hnsw (""Embedding"" vector_cosine_ops)
                WITH (m = 16, ef_construction = 64);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_metadata");

            migrationBuilder.DropTable(
                name: "journal_entries");

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
