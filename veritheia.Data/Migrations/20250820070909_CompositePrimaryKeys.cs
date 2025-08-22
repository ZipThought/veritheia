using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace veritheia.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompositePrimaryKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_metadata_documents_DocumentId",
                table: "document_metadata");

            migrationBuilder.DropForeignKey(
                name: "FK_documents_knowledge_scopes_ScopeId",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entries_journals_JournalId",
                table: "journal_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_journals_journeys_JourneyId",
                table: "journals");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_document_segments_documents_DocumentId",
                table: "journey_document_segments");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_document_segments_journeys_JourneyId",
                table: "journey_document_segments");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_formations_journeys_JourneyId",
                table: "journey_formations");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_frameworks_journeys_JourneyId",
                table: "journey_frameworks");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_segment_assessments_journey_document_segments_Segme~",
                table: "journey_segment_assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_journeys_personas_PersonaId",
                table: "journeys");

            migrationBuilder.DropForeignKey(
                name: "FK_knowledge_scopes_knowledge_scopes_ParentScopeId",
                table: "knowledge_scopes");

            migrationBuilder.DropForeignKey(
                name: "FK_process_executions_journeys_JourneyId",
                table: "process_executions");

            migrationBuilder.DropForeignKey(
                name: "FK_process_results_process_executions_ExecutionId",
                table: "process_results");

            migrationBuilder.DropForeignKey(
                name: "FK_search_indexes_journey_document_segments_SegmentId",
                table: "search_indexes");

            migrationBuilder.DropForeignKey(
                name: "FK_search_vectors_1536_search_indexes_IndexId",
                table: "search_vectors_1536");

            migrationBuilder.DropForeignKey(
                name: "FK_search_vectors_384_search_indexes_IndexId",
                table: "search_vectors_384");

            migrationBuilder.DropForeignKey(
                name: "FK_search_vectors_768_search_indexes_IndexId",
                table: "search_vectors_768");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_vectors_768",
                table: "search_vectors_768");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_vectors_384",
                table: "search_vectors_384");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_vectors_1536",
                table: "search_vectors_1536");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_indexes",
                table: "search_indexes");

            migrationBuilder.DropIndex(
                name: "IX_search_indexes_SegmentId_VectorModel",
                table: "search_indexes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_process_results",
                table: "process_results");

            migrationBuilder.DropIndex(
                name: "IX_process_results_ExecutionId",
                table: "process_results");

            migrationBuilder.DropPrimaryKey(
                name: "PK_process_executions",
                table: "process_executions");

            migrationBuilder.DropIndex(
                name: "IX_process_executions_JourneyId",
                table: "process_executions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_process_definitions",
                table: "process_definitions");

            migrationBuilder.DropIndex(
                name: "IX_process_definitions_ProcessType",
                table: "process_definitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_process_capabilities",
                table: "process_capabilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_personas",
                table: "personas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_knowledge_scopes",
                table: "knowledge_scopes");

            migrationBuilder.DropIndex(
                name: "IX_knowledge_scopes_ParentScopeId",
                table: "knowledge_scopes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journeys",
                table: "journeys");

            migrationBuilder.DropIndex(
                name: "IX_journeys_PersonaId",
                table: "journeys");

            migrationBuilder.DropIndex(
                name: "IX_journeys_UserId",
                table: "journeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journey_segment_assessments",
                table: "journey_segment_assessments");

            migrationBuilder.DropIndex(
                name: "IX_journey_segment_assessments_SegmentId",
                table: "journey_segment_assessments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journey_frameworks",
                table: "journey_frameworks");

            migrationBuilder.DropIndex(
                name: "IX_journey_frameworks_JourneyId",
                table: "journey_frameworks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journey_formations",
                table: "journey_formations");

            migrationBuilder.DropIndex(
                name: "IX_journey_formations_JourneyId",
                table: "journey_formations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journey_document_segments",
                table: "journey_document_segments");

            migrationBuilder.DropIndex(
                name: "IX_journey_document_segments_DocumentId",
                table: "journey_document_segments");

            migrationBuilder.DropIndex(
                name: "IX_journey_document_segments_JourneyId_DocumentId_SequenceIndex",
                table: "journey_document_segments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journals",
                table: "journals");

            migrationBuilder.DropIndex(
                name: "IX_journals_JourneyId",
                table: "journals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journal_entries",
                table: "journal_entries");

            migrationBuilder.DropIndex(
                name: "IX_journal_entries_JournalId",
                table: "journal_entries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_documents",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_ScopeId",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_UserId",
                table: "documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_document_metadata",
                table: "document_metadata");

            migrationBuilder.DropIndex(
                name: "IX_document_metadata_DocumentId",
                table: "document_metadata");

            migrationBuilder.DropColumn(
                name: "IndexedAt",
                table: "search_indexes");

            migrationBuilder.DropColumn(
                name: "VectorDimension",
                table: "search_indexes");

            migrationBuilder.DropColumn(
                name: "ExecutedAt",
                table: "process_results");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "process_definitions");

            migrationBuilder.DropColumn(
                name: "GrantedAt",
                table: "process_capabilities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "knowledge_scopes");

            migrationBuilder.DropColumn(
                name: "AssessedAt",
                table: "journey_segment_assessments");

            migrationBuilder.DropColumn(
                name: "AssessmentReasoning",
                table: "journey_segment_assessments");

            migrationBuilder.DropColumn(
                name: "ContributionScore",
                table: "journey_segment_assessments");

            migrationBuilder.DropColumn(
                name: "RelevanceScore",
                table: "journey_segment_assessments");

            migrationBuilder.DropColumn(
                name: "ResearchQuestionId",
                table: "journey_segment_assessments");

            migrationBuilder.DropColumn(
                name: "FormationMarker",
                table: "journey_formations");

            migrationBuilder.DropColumn(
                name: "FormationReasoning",
                table: "journey_formations");

            migrationBuilder.DropColumn(
                name: "FormedAt",
                table: "journey_formations");

            migrationBuilder.DropColumn(
                name: "SegmentPurpose",
                table: "journey_document_segments");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "process_capabilities",
                newName: "IsActive");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "search_vectors_768",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "search_vectors_768",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "search_vectors_768",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "search_vectors_768",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "search_vectors_384",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "search_vectors_384",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "search_vectors_384",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "search_vectors_384",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "search_vectors_1536",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "search_vectors_1536",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "search_vectors_1536",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "search_vectors_1536",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "search_indexes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Metadata",
                table: "process_results",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "process_results",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "process_executions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "process_executions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Configuration",
                table: "process_definitions",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "process_definitions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsed",
                table: "process_capabilities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "knowledge_scopes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "journey_segment_assessments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Reasoning",
                table: "journey_segment_assessments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "journey_segment_assessments",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "journey_frameworks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "journey_formations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "StructuralPath",
                table: "journey_document_segments",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "journey_document_segments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "journals",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string[]>(
                name: "Tags",
                table: "journal_entries",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Metadata",
                table: "journal_entries",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "journal_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "ExtendedMetadata",
                table: "document_metadata",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string[]>(
                name: "Authors",
                table: "document_metadata",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "document_metadata",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Abstract",
                table: "document_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DOI",
                table: "document_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Keywords",
                table: "document_metadata",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "document_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_vectors_768",
                table: "search_vectors_768",
                columns: new[] { "UserId", "IndexId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_vectors_384",
                table: "search_vectors_384",
                columns: new[] { "UserId", "IndexId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_vectors_1536",
                table: "search_vectors_1536",
                columns: new[] { "UserId", "IndexId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_indexes",
                table: "search_indexes",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_process_results",
                table: "process_results",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_process_executions",
                table: "process_executions",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_process_definitions",
                table: "process_definitions",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_process_capabilities",
                table: "process_capabilities",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_personas",
                table: "personas",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_knowledge_scopes",
                table: "knowledge_scopes",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_journeys",
                table: "journeys",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_journey_segment_assessments",
                table: "journey_segment_assessments",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_journey_frameworks",
                table: "journey_frameworks",
                columns: new[] { "UserId", "JourneyId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_journey_formations",
                table: "journey_formations",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_journey_document_segments",
                table: "journey_document_segments",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_journals",
                table: "journals",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_journal_entries",
                table: "journal_entries",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_documents",
                table: "documents",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_document_metadata",
                table: "document_metadata",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_search_vectors_768_UserId_CreatedAt",
                table: "search_vectors_768",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_search_vectors_384_UserId_CreatedAt",
                table: "search_vectors_384",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_search_vectors_1536_UserId_CreatedAt",
                table: "search_vectors_1536",
                columns: new[] { "UserId", "CreatedAt" });

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
                name: "IX_process_results_UserId_CreatedAt",
                table: "process_results",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_process_results_UserId_ExecutionId",
                table: "process_results",
                columns: new[] { "UserId", "ExecutionId" },
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
                name: "IX_process_capabilities_UserId_CreatedAt",
                table: "process_capabilities",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_personas_UserId_CreatedAt",
                table: "personas",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_personas_UserId_IsActive",
                table: "personas",
                columns: new[] { "UserId", "IsActive" });

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
                name: "IX_journey_frameworks_UserId_CreatedAt",
                table: "journey_frameworks",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_journey_frameworks_UserId_JourneyType",
                table: "journey_frameworks",
                columns: new[] { "UserId", "JourneyType" });

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
                name: "IX_document_metadata_UserId_CreatedAt",
                table: "document_metadata",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_document_metadata_UserId_DocumentId",
                table: "document_metadata",
                columns: new[] { "UserId", "DocumentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_document_metadata_documents_UserId_DocumentId",
                table: "document_metadata",
                columns: new[] { "UserId", "DocumentId" },
                principalTable: "documents",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_knowledge_scopes_UserId_ScopeId",
                table: "documents",
                columns: new[] { "UserId", "ScopeId" },
                principalTable: "knowledge_scopes",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_journals_UserId_JournalId",
                table: "journal_entries",
                columns: new[] { "UserId", "JournalId" },
                principalTable: "journals",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journals_journeys_UserId_JourneyId",
                table: "journals",
                columns: new[] { "UserId", "JourneyId" },
                principalTable: "journeys",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_document_segments_documents_UserId_DocumentId",
                table: "journey_document_segments",
                columns: new[] { "UserId", "DocumentId" },
                principalTable: "documents",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_document_segments_journeys_UserId_JourneyId",
                table: "journey_document_segments",
                columns: new[] { "UserId", "JourneyId" },
                principalTable: "journeys",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_formations_journeys_UserId_JourneyId",
                table: "journey_formations",
                columns: new[] { "UserId", "JourneyId" },
                principalTable: "journeys",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_frameworks_journeys_UserId_JourneyId",
                table: "journey_frameworks",
                columns: new[] { "UserId", "JourneyId" },
                principalTable: "journeys",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_segment_assessments_journey_document_segments_UserI~",
                table: "journey_segment_assessments",
                columns: new[] { "UserId", "SegmentId" },
                principalTable: "journey_document_segments",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journeys_personas_UserId_PersonaId",
                table: "journeys",
                columns: new[] { "UserId", "PersonaId" },
                principalTable: "personas",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_knowledge_scopes_knowledge_scopes_UserId_ParentScopeId",
                table: "knowledge_scopes",
                columns: new[] { "UserId", "ParentScopeId" },
                principalTable: "knowledge_scopes",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_executions_journeys_UserId_JourneyId",
                table: "process_executions",
                columns: new[] { "UserId", "JourneyId" },
                principalTable: "journeys",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_results_process_executions_UserId_ExecutionId",
                table: "process_results",
                columns: new[] { "UserId", "ExecutionId" },
                principalTable: "process_executions",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_indexes_journey_document_segments_UserId_SegmentId",
                table: "search_indexes",
                columns: new[] { "UserId", "SegmentId" },
                principalTable: "journey_document_segments",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_vectors_1536_search_indexes_UserId_IndexId",
                table: "search_vectors_1536",
                columns: new[] { "UserId", "IndexId" },
                principalTable: "search_indexes",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_vectors_384_search_indexes_UserId_IndexId",
                table: "search_vectors_384",
                columns: new[] { "UserId", "IndexId" },
                principalTable: "search_indexes",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_vectors_768_search_indexes_UserId_IndexId",
                table: "search_vectors_768",
                columns: new[] { "UserId", "IndexId" },
                principalTable: "search_indexes",
                principalColumns: new[] { "UserId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_metadata_documents_UserId_DocumentId",
                table: "document_metadata");

            migrationBuilder.DropForeignKey(
                name: "FK_documents_knowledge_scopes_UserId_ScopeId",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entries_journals_UserId_JournalId",
                table: "journal_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_journals_journeys_UserId_JourneyId",
                table: "journals");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_document_segments_documents_UserId_DocumentId",
                table: "journey_document_segments");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_document_segments_journeys_UserId_JourneyId",
                table: "journey_document_segments");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_formations_journeys_UserId_JourneyId",
                table: "journey_formations");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_frameworks_journeys_UserId_JourneyId",
                table: "journey_frameworks");

            migrationBuilder.DropForeignKey(
                name: "FK_journey_segment_assessments_journey_document_segments_UserI~",
                table: "journey_segment_assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_journeys_personas_UserId_PersonaId",
                table: "journeys");

            migrationBuilder.DropForeignKey(
                name: "FK_knowledge_scopes_knowledge_scopes_UserId_ParentScopeId",
                table: "knowledge_scopes");

            migrationBuilder.DropForeignKey(
                name: "FK_process_executions_journeys_UserId_JourneyId",
                table: "process_executions");

            migrationBuilder.DropForeignKey(
                name: "FK_process_results_process_executions_UserId_ExecutionId",
                table: "process_results");

            migrationBuilder.DropForeignKey(
                name: "FK_search_indexes_journey_document_segments_UserId_SegmentId",
                table: "search_indexes");

            migrationBuilder.DropForeignKey(
                name: "FK_search_vectors_1536_search_indexes_UserId_IndexId",
                table: "search_vectors_1536");

            migrationBuilder.DropForeignKey(
                name: "FK_search_vectors_384_search_indexes_UserId_IndexId",
                table: "search_vectors_384");

            migrationBuilder.DropForeignKey(
                name: "FK_search_vectors_768_search_indexes_UserId_IndexId",
                table: "search_vectors_768");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_vectors_768",
                table: "search_vectors_768");

            migrationBuilder.DropIndex(
                name: "IX_search_vectors_768_UserId_CreatedAt",
                table: "search_vectors_768");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_vectors_384",
                table: "search_vectors_384");

            migrationBuilder.DropIndex(
                name: "IX_search_vectors_384_UserId_CreatedAt",
                table: "search_vectors_384");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_vectors_1536",
                table: "search_vectors_1536");

            migrationBuilder.DropIndex(
                name: "IX_search_vectors_1536_UserId_CreatedAt",
                table: "search_vectors_1536");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_indexes",
                table: "search_indexes");

            migrationBuilder.DropIndex(
                name: "IX_search_indexes_UserId_CreatedAt",
                table: "search_indexes");

            migrationBuilder.DropIndex(
                name: "IX_search_indexes_UserId_SegmentId_VectorModel",
                table: "search_indexes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_process_results",
                table: "process_results");

            migrationBuilder.DropIndex(
                name: "IX_process_results_UserId_CreatedAt",
                table: "process_results");

            migrationBuilder.DropIndex(
                name: "IX_process_results_UserId_ExecutionId",
                table: "process_results");

            migrationBuilder.DropPrimaryKey(
                name: "PK_process_executions",
                table: "process_executions");

            migrationBuilder.DropIndex(
                name: "IX_process_executions_UserId_CreatedAt",
                table: "process_executions");

            migrationBuilder.DropIndex(
                name: "IX_process_executions_UserId_JourneyId",
                table: "process_executions");

            migrationBuilder.DropIndex(
                name: "IX_process_executions_UserId_State",
                table: "process_executions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_process_definitions",
                table: "process_definitions");

            migrationBuilder.DropIndex(
                name: "IX_process_definitions_UserId_Category",
                table: "process_definitions");

            migrationBuilder.DropIndex(
                name: "IX_process_definitions_UserId_CreatedAt",
                table: "process_definitions");

            migrationBuilder.DropIndex(
                name: "IX_process_definitions_UserId_ProcessType",
                table: "process_definitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_process_capabilities",
                table: "process_capabilities");

            migrationBuilder.DropIndex(
                name: "IX_process_capabilities_UserId_CreatedAt",
                table: "process_capabilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_personas",
                table: "personas");

            migrationBuilder.DropIndex(
                name: "IX_personas_UserId_CreatedAt",
                table: "personas");

            migrationBuilder.DropIndex(
                name: "IX_personas_UserId_IsActive",
                table: "personas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_knowledge_scopes",
                table: "knowledge_scopes");

            migrationBuilder.DropIndex(
                name: "IX_knowledge_scopes_UserId_CreatedAt",
                table: "knowledge_scopes");

            migrationBuilder.DropIndex(
                name: "IX_knowledge_scopes_UserId_ParentScopeId",
                table: "knowledge_scopes");

            migrationBuilder.DropIndex(
                name: "IX_knowledge_scopes_UserId_Type",
                table: "knowledge_scopes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journeys",
                table: "journeys");

            migrationBuilder.DropIndex(
                name: "IX_journeys_UserId_CreatedAt",
                table: "journeys");

            migrationBuilder.DropIndex(
                name: "IX_journeys_UserId_PersonaId",
                table: "journeys");

            migrationBuilder.DropIndex(
                name: "IX_journeys_UserId_ProcessType",
                table: "journeys");

            migrationBuilder.DropIndex(
                name: "IX_journeys_UserId_State",
                table: "journeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journey_segment_assessments",
                table: "journey_segment_assessments");

            migrationBuilder.DropIndex(
                name: "IX_journey_segment_assessments_UserId_AssessmentType",
                table: "journey_segment_assessments");

            migrationBuilder.DropIndex(
                name: "IX_journey_segment_assessments_UserId_CreatedAt",
                table: "journey_segment_assessments");

            migrationBuilder.DropIndex(
                name: "IX_journey_segment_assessments_UserId_SegmentId",
                table: "journey_segment_assessments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journey_frameworks",
                table: "journey_frameworks");

            migrationBuilder.DropIndex(
                name: "IX_journey_frameworks_UserId_CreatedAt",
                table: "journey_frameworks");

            migrationBuilder.DropIndex(
                name: "IX_journey_frameworks_UserId_JourneyType",
                table: "journey_frameworks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journey_formations",
                table: "journey_formations");

            migrationBuilder.DropIndex(
                name: "IX_journey_formations_UserId_CreatedAt",
                table: "journey_formations");

            migrationBuilder.DropIndex(
                name: "IX_journey_formations_UserId_InsightType",
                table: "journey_formations");

            migrationBuilder.DropIndex(
                name: "IX_journey_formations_UserId_JourneyId",
                table: "journey_formations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journey_document_segments",
                table: "journey_document_segments");

            migrationBuilder.DropIndex(
                name: "IX_journey_document_segments_UserId_CreatedAt",
                table: "journey_document_segments");

            migrationBuilder.DropIndex(
                name: "IX_journey_document_segments_UserId_DocumentId",
                table: "journey_document_segments");

            migrationBuilder.DropIndex(
                name: "IX_journey_document_segments_UserId_JourneyId",
                table: "journey_document_segments");

            migrationBuilder.DropIndex(
                name: "IX_journey_document_segments_UserId_JourneyId_DocumentId_Seque~",
                table: "journey_document_segments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journals",
                table: "journals");

            migrationBuilder.DropIndex(
                name: "IX_journals_UserId_CreatedAt",
                table: "journals");

            migrationBuilder.DropIndex(
                name: "IX_journals_UserId_JourneyId",
                table: "journals");

            migrationBuilder.DropIndex(
                name: "IX_journals_UserId_Type",
                table: "journals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journal_entries",
                table: "journal_entries");

            migrationBuilder.DropIndex(
                name: "IX_journal_entries_UserId_CreatedAt",
                table: "journal_entries");

            migrationBuilder.DropIndex(
                name: "IX_journal_entries_UserId_JournalId",
                table: "journal_entries");

            migrationBuilder.DropIndex(
                name: "IX_journal_entries_UserId_Significance",
                table: "journal_entries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_documents",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_UserId_CreatedAt",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_UserId_FileName",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_UserId_MimeType",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_UserId_ScopeId",
                table: "documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_document_metadata",
                table: "document_metadata");

            migrationBuilder.DropIndex(
                name: "IX_document_metadata_UserId_CreatedAt",
                table: "document_metadata");

            migrationBuilder.DropIndex(
                name: "IX_document_metadata_UserId_DocumentId",
                table: "document_metadata");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "search_vectors_768");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "search_vectors_768");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "search_vectors_768");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "search_vectors_768");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "search_vectors_384");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "search_vectors_384");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "search_vectors_384");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "search_vectors_384");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "search_vectors_1536");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "search_vectors_1536");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "search_vectors_1536");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "search_vectors_1536");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "search_indexes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "process_results");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "process_executions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "process_definitions");

            migrationBuilder.DropColumn(
                name: "LastUsed",
                table: "process_capabilities");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "knowledge_scopes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "journey_segment_assessments");

            migrationBuilder.DropColumn(
                name: "Reasoning",
                table: "journey_segment_assessments");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "journey_segment_assessments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "journey_frameworks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "journey_formations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "journey_document_segments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "journals");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "journal_entries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "document_metadata");

            migrationBuilder.DropColumn(
                name: "Abstract",
                table: "document_metadata");

            migrationBuilder.DropColumn(
                name: "DOI",
                table: "document_metadata");

            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "document_metadata");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "document_metadata");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "process_capabilities",
                newName: "IsEnabled");

            migrationBuilder.AddColumn<DateTime>(
                name: "IndexedAt",
                table: "search_indexes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "VectorDimension",
                table: "search_indexes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Metadata",
                table: "process_results",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExecutedAt",
                table: "process_results",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "process_executions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Configuration",
                table: "process_definitions",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "process_definitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GrantedAt",
                table: "process_capabilities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "knowledge_scopes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssessedAt",
                table: "journey_segment_assessments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AssessmentReasoning",
                table: "journey_segment_assessments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ContributionScore",
                table: "journey_segment_assessments",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "RelevanceScore",
                table: "journey_segment_assessments",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResearchQuestionId",
                table: "journey_segment_assessments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormationMarker",
                table: "journey_formations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormationReasoning",
                table: "journey_formations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FormedAt",
                table: "journey_formations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "StructuralPath",
                table: "journey_document_segments",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb");

            migrationBuilder.AddColumn<string>(
                name: "SegmentPurpose",
                table: "journey_document_segments",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<List<string>>(
                name: "Tags",
                table: "journal_entries",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string[]),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Metadata",
                table: "journal_entries",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "ExtendedMetadata",
                table: "document_metadata",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<string>>(
                name: "Authors",
                table: "document_metadata",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string[]),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_vectors_768",
                table: "search_vectors_768",
                column: "IndexId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_vectors_384",
                table: "search_vectors_384",
                column: "IndexId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_vectors_1536",
                table: "search_vectors_1536",
                column: "IndexId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_indexes",
                table: "search_indexes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_process_results",
                table: "process_results",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_process_executions",
                table: "process_executions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_process_definitions",
                table: "process_definitions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_process_capabilities",
                table: "process_capabilities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_personas",
                table: "personas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_knowledge_scopes",
                table: "knowledge_scopes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journeys",
                table: "journeys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journey_segment_assessments",
                table: "journey_segment_assessments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journey_frameworks",
                table: "journey_frameworks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journey_formations",
                table: "journey_formations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journey_document_segments",
                table: "journey_document_segments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journals",
                table: "journals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journal_entries",
                table: "journal_entries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_documents",
                table: "documents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_document_metadata",
                table: "document_metadata",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_search_indexes_SegmentId_VectorModel",
                table: "search_indexes",
                columns: new[] { "SegmentId", "VectorModel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_process_results_ExecutionId",
                table: "process_results",
                column: "ExecutionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_process_executions_JourneyId",
                table: "process_executions",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_process_definitions_ProcessType",
                table: "process_definitions",
                column: "ProcessType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_scopes_ParentScopeId",
                table: "knowledge_scopes",
                column: "ParentScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_journeys_PersonaId",
                table: "journeys",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_journeys_UserId",
                table: "journeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_journey_segment_assessments_SegmentId",
                table: "journey_segment_assessments",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_journey_frameworks_JourneyId",
                table: "journey_frameworks",
                column: "JourneyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journey_formations_JourneyId",
                table: "journey_formations",
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
                name: "IX_journals_JourneyId",
                table: "journals",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_JournalId",
                table: "journal_entries",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_documents_ScopeId",
                table: "documents",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_documents_UserId",
                table: "documents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_document_metadata_DocumentId",
                table: "document_metadata",
                column: "DocumentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_document_metadata_documents_DocumentId",
                table: "document_metadata",
                column: "DocumentId",
                principalTable: "documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_knowledge_scopes_ScopeId",
                table: "documents",
                column: "ScopeId",
                principalTable: "knowledge_scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_journals_JournalId",
                table: "journal_entries",
                column: "JournalId",
                principalTable: "journals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journals_journeys_JourneyId",
                table: "journals",
                column: "JourneyId",
                principalTable: "journeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_document_segments_documents_DocumentId",
                table: "journey_document_segments",
                column: "DocumentId",
                principalTable: "documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_document_segments_journeys_JourneyId",
                table: "journey_document_segments",
                column: "JourneyId",
                principalTable: "journeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_formations_journeys_JourneyId",
                table: "journey_formations",
                column: "JourneyId",
                principalTable: "journeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_frameworks_journeys_JourneyId",
                table: "journey_frameworks",
                column: "JourneyId",
                principalTable: "journeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journey_segment_assessments_journey_document_segments_Segme~",
                table: "journey_segment_assessments",
                column: "SegmentId",
                principalTable: "journey_document_segments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journeys_personas_PersonaId",
                table: "journeys",
                column: "PersonaId",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_knowledge_scopes_knowledge_scopes_ParentScopeId",
                table: "knowledge_scopes",
                column: "ParentScopeId",
                principalTable: "knowledge_scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_executions_journeys_JourneyId",
                table: "process_executions",
                column: "JourneyId",
                principalTable: "journeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_results_process_executions_ExecutionId",
                table: "process_results",
                column: "ExecutionId",
                principalTable: "process_executions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_indexes_journey_document_segments_SegmentId",
                table: "search_indexes",
                column: "SegmentId",
                principalTable: "journey_document_segments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_vectors_1536_search_indexes_IndexId",
                table: "search_vectors_1536",
                column: "IndexId",
                principalTable: "search_indexes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_vectors_384_search_indexes_IndexId",
                table: "search_vectors_384",
                column: "IndexId",
                principalTable: "search_indexes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_vectors_768_search_indexes_IndexId",
                table: "search_vectors_768",
                column: "IndexId",
                principalTable: "search_indexes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
