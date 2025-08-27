using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Pgvector.EntityFrameworkCore;
using Veritheia.Data.Entities;

namespace Veritheia.Data;

/// <summary>
/// Entity Framework DbContext for Veritheia - manages journey projection spaces
/// </summary>
public class VeritheiaDbContext : DbContext
{
    public VeritheiaDbContext(DbContextOptions<VeritheiaDbContext> options)
        : base(options)
    {
    }

    // User Domain
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Persona> Personas { get; set; } = null!;
    public DbSet<ProcessCapability> ProcessCapabilities { get; set; } = null!;

    // Journey Domain - The Core of Projection Spaces
    public DbSet<Journey> Journeys { get; set; } = null!;
    public DbSet<JourneyFramework> JourneyFrameworks { get; set; } = null!;
    public DbSet<Journal> Journals { get; set; } = null!;
    public DbSet<JournalEntry> JournalEntries { get; set; } = null!;

    // Knowledge Domain
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentMetadata> DocumentMetadata { get; set; } = null!;
    public DbSet<KnowledgeScope> KnowledgeScopes { get; set; } = null!;

    // Journey Projection Entities
    public DbSet<JourneyDocumentSegment> JourneyDocumentSegments { get; set; } = null!;
    public DbSet<JourneySegmentAssessment> JourneySegmentAssessments { get; set; } = null!;
    public DbSet<JourneyFormation> JourneyFormations { get; set; } = null!;

    // Search Infrastructure
    public DbSet<SearchIndex> SearchIndexes { get; set; } = null!;
    public DbSet<SearchVector> SearchVectors { get; set; } = null!;

    // Process Infrastructure
    public DbSet<ProcessDefinition> ProcessDefinitions { get; set; } = null!;
    public DbSet<ProcessExecution> ProcessExecutions { get; set; } = null!;
    public DbSet<ProcessResult> ProcessResults { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");

        // Configure all entities
        ConfigureUserDomain(modelBuilder);
        ConfigureJourneyDomain(modelBuilder);
        ConfigureKnowledgeDomain(modelBuilder);
        ConfigureJourneyProjections(modelBuilder);
        ConfigureSearchInfrastructure(modelBuilder);
        ConfigureProcessInfrastructure(modelBuilder);
    }

    private void ConfigureUserDomain(ModelBuilder modelBuilder)
    {
        // User - Only entity with single primary key (root of partition hierarchy)
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(255).IsRequired();
        });

        // Persona - Composite primary key for partition enforcement
        modelBuilder.Entity<Persona>(entity =>
        {
            entity.ToTable("personas");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.HasIndex(e => new { e.UserId, e.Domain }).IsUnique();
            entity.Property(e => e.Domain).HasMaxLength(100).IsRequired();

            // Partition-aware indexes for locality
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.IsActive });

            // JSONB columns
            entity.Property(e => e.ConceptualVocabulary).HasColumnType("jsonb");
            entity.Property(e => e.Patterns).HasColumnType("jsonb");
            entity.Property(e => e.MethodologicalPreferences).HasColumnType("jsonb");
            entity.Property(e => e.Markers).HasColumnType("jsonb");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.Personas)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessCapability - Composite primary key for partition enforcement
        modelBuilder.Entity<ProcessCapability>(entity =>
        {
            entity.ToTable("process_capabilities");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.HasIndex(e => new { e.UserId, e.ProcessType }).IsUnique();
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });

            entity.HasOne(e => e.User)
                .WithMany(u => u.ProcessCapabilities)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureJourneyDomain(ModelBuilder modelBuilder)
    {
        // Journey - Composite primary key for partition enforcement
        modelBuilder.Entity<Journey>(entity =>
        {
            entity.ToTable("journeys", t => t.HasCheckConstraint("CK_Journey_State",
                "\"State\" IN ('Active', 'Paused', 'Completed', 'Abandoned')"));
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Purpose).IsRequired();
            entity.Property(e => e.State).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Context).HasColumnType("jsonb");

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.State });
            entity.HasIndex(e => new { e.UserId, e.ProcessType });

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.Journeys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Persona)
                .WithMany(p => p.Journeys)
                .HasForeignKey(e => new { e.UserId, e.PersonaId });
        });

        // JourneyFramework - Composite primary key for partition enforcement
        modelBuilder.Entity<JourneyFramework>(entity =>
        {
            entity.ToTable("journey_frameworks");
            // CORRECT: Composite primary key (UserId, JourneyId)
            entity.HasKey(e => new { e.UserId, e.JourneyId });
            entity.Property(e => e.JourneyType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FrameworkElements).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.ProjectionRules).HasColumnType("jsonb").IsRequired();

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.JourneyType });

            entity.HasOne(e => e.Journey)
                .WithOne(j => j.Framework)
                .HasForeignKey<JourneyFramework>(e => new { e.UserId, e.JourneyId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Journal - Composite primary key for partition enforcement
        modelBuilder.Entity<Journal>(entity =>
        {
            entity.ToTable("journals", t => t.HasCheckConstraint("CK_Journal_Type",
                "\"Type\" IN ('Research', 'Method', 'Decision', 'Reflection')"));
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.Type });

            entity.HasOne(e => e.Journey)
                .WithMany(j => j.Journals)
                .HasForeignKey(e => new { e.UserId, e.JourneyId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JournalEntry - Composite primary key for partition enforcement
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("journal_entries", t => t.HasCheckConstraint("CK_JournalEntry_Significance",
                "\"Significance\" IN ('Routine', 'Notable', 'Critical', 'Milestone')"));
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Significance).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Tags).HasColumnType("text[]");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.Significance });

            entity.HasOne(e => e.Journal)
                .WithMany(j => j.Entries)
                .HasForeignKey(e => new { e.UserId, e.JournalId })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureKnowledgeDomain(ModelBuilder modelBuilder)
    {
        // KnowledgeScope - Composite primary key for partition enforcement
        modelBuilder.Entity<KnowledgeScope>(entity =>
        {
            entity.ToTable("knowledge_scopes", t => t.HasCheckConstraint("CK_KnowledgeScope_Type",
                "\"Type\" IN ('Project', 'Topic', 'Subject', 'Custom')"));
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.Type });

            entity.HasOne(e => e.ParentScope)
                .WithMany(p => p.ChildScopes)
                .HasForeignKey(e => new { e.UserId, e.ParentScopeId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Document - Composite primary key for partition enforcement
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.MimeType });
            entity.HasIndex(e => new { e.UserId, e.FileName });

            entity.HasOne(e => e.User)
                .WithMany(u => u.Documents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Scope)
                .WithMany(s => s.Documents)
                .HasForeignKey(e => new { e.UserId, e.ScopeId })
                .OnDelete(DeleteBehavior.SetNull);
        });

        // DocumentMetadata - Composite primary key for partition enforcement
        modelBuilder.Entity<DocumentMetadata>(entity =>
        {
            entity.ToTable("document_metadata");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.HasIndex(e => new { e.UserId, e.DocumentId }).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(1000);
            entity.Property(e => e.Authors).HasColumnType("text[]");
            entity.Property(e => e.ExtendedMetadata).HasColumnType("jsonb");

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });

            entity.HasOne(e => e.Document)
                .WithOne(d => d.Metadata)
                .HasForeignKey<DocumentMetadata>(e => new { e.UserId, e.DocumentId })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureJourneyProjections(ModelBuilder modelBuilder)
    {
        // JourneyDocumentSegment - Composite primary key for partition enforcement
        modelBuilder.Entity<JourneyDocumentSegment>(entity =>
        {
            entity.ToTable("journey_document_segments");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.HasIndex(e => new { e.UserId, e.JourneyId, e.DocumentId, e.SequenceIndex }).IsUnique();
            entity.Property(e => e.SegmentContent).IsRequired();
            entity.Property(e => e.SegmentType).HasMaxLength(50);
            entity.Property(e => e.StructuralPath).HasColumnType("jsonb");
            entity.Property(e => e.ByteRange).HasColumnType("int4range");
            entity.Property(e => e.CreatedByRule).HasMaxLength(255);
            entity.Property(e => e.CreatedForQuestion).HasMaxLength(255);

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.JourneyId });
            entity.HasIndex(e => new { e.UserId, e.DocumentId });

            entity.HasOne(e => e.Journey)
                .WithMany(j => j.DocumentSegments)
                .HasForeignKey(e => new { e.UserId, e.JourneyId })
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Document)
                .WithMany(d => d.JourneySegments)
                .HasForeignKey(e => new { e.UserId, e.DocumentId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JourneySegmentAssessment - Composite primary key for partition enforcement
        modelBuilder.Entity<JourneySegmentAssessment>(entity =>
        {
            entity.ToTable("journey_segment_assessments");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.Property(e => e.AssessmentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RubricScores).HasColumnType("jsonb");
            entity.Property(e => e.ReasoningChain).HasColumnType("jsonb");
            entity.Property(e => e.AssessedByModel).HasMaxLength(100);

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.AssessmentType });

            entity.HasOne(e => e.Segment)
                .WithMany(s => s.Assessments)
                .HasForeignKey(e => new { e.UserId, e.SegmentId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JourneyFormation - Composite primary key for partition enforcement
        modelBuilder.Entity<JourneyFormation>(entity =>
        {
            entity.ToTable("journey_formations");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.Property(e => e.InsightType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.InsightContent).IsRequired();
            entity.Property(e => e.FormedFromSegments).HasColumnType("jsonb");
            entity.Property(e => e.FormedThroughQuestions).HasColumnType("jsonb");

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.InsightType });

            entity.HasOne(e => e.Journey)
                .WithMany(j => j.Formations)
                .HasForeignKey(e => new { e.UserId, e.JourneyId })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureSearchInfrastructure(ModelBuilder modelBuilder)
    {
        // SearchIndex - Composite primary key for partition enforcement
        modelBuilder.Entity<SearchIndex>(entity =>
        {
            entity.ToTable("search_indexes");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.HasIndex(e => new { e.UserId, e.SegmentId, e.VectorModel }).IsUnique();
            entity.Property(e => e.VectorModel).HasMaxLength(100).IsRequired();

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });

            entity.HasOne(e => e.Segment)
                .WithMany(s => s.SearchIndexes)
                .HasForeignKey(e => new { e.UserId, e.SegmentId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SearchVector - Unified vector table with orthogonal transformation
        modelBuilder.Entity<SearchVector>(entity =>
        {
            entity.ToTable("search_vectors");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            
            // Vector column - variable dimensions (384, 768, 1536)
            entity.Property(e => e.Embedding)
                .HasColumnType("vector")
                .IsRequired();
            
            // Dimension tracking
            entity.Property(e => e.Dimension)
                .IsRequired();
            
            entity.Property(e => e.VectorModel)
                .HasMaxLength(100)
                .IsRequired();

            // Partition-aware indexes for user isolation
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.SegmentId });
            entity.HasIndex(e => new { e.UserId, e.JourneyId })
                .HasFilter("\"JourneyId\" IS NOT NULL");

            // Relationships
            entity.HasOne(e => e.Segment)
                .WithMany()
                .HasForeignKey(e => new { e.UserId, e.SegmentId })
                .OnDelete(DeleteBehavior.Cascade);

            // Note: HNSW index will be created in migration
            // CREATE INDEX idx_vectors_hnsw ON search_vectors 
            //     USING hnsw (embedding vector_cosine_ops)
            //     WITH (m = 16, ef_construction = 64);
        });
    }

    private void ConfigureProcessInfrastructure(ModelBuilder modelBuilder)
    {
        // ProcessDefinition - Composite primary key for partition enforcement
        modelBuilder.Entity<ProcessDefinition>(entity =>
        {
            entity.ToTable("process_definitions", t =>
            {
                t.HasCheckConstraint("CK_ProcessDefinition_Category",
                    "\"Category\" IN ('Methodological', 'Developmental', 'Analytical', 'Compositional', 'Reflective')");
                t.HasCheckConstraint("CK_ProcessDefinition_Trigger",
                    "\"TriggerType\" IN ('Manual', 'UserInitiated')");
            });
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.HasIndex(e => new { e.UserId, e.ProcessType }).IsUnique();
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TriggerType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Inputs).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Configuration).HasColumnType("jsonb");

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.Category });
        });

        // ProcessExecution - Composite primary key for partition enforcement
        modelBuilder.Entity<ProcessExecution>(entity =>
        {
            entity.ToTable("process_executions", t => t.HasCheckConstraint("CK_ProcessExecution_State",
                "\"State\" IN ('Pending', 'Running', 'Completed', 'Failed', 'Cancelled')"));
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();
            entity.Property(e => e.State).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Inputs).HasColumnType("jsonb").IsRequired();

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.State });

            entity.HasOne(e => e.Journey)
                .WithMany(j => j.ProcessExecutions)
                .HasForeignKey(e => new { e.UserId, e.JourneyId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessResult - Composite primary key for partition enforcement
        modelBuilder.Entity<ProcessResult>(entity =>
        {
            entity.ToTable("process_results");
            // CORRECT: Composite primary key (UserId, Id)
            entity.HasKey(e => new { e.UserId, e.Id });
            entity.HasIndex(e => new { e.UserId, e.ExecutionId }).IsUnique();
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Data).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Metadata).HasColumnType("jsonb");

            // Partition-aware indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });

            entity.HasOne(e => e.Execution)
                .WithOne(ex => ex.Result)
                .HasForeignKey<ProcessResult>(e => new { e.UserId, e.ExecutionId })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}