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
    public DbSet<SearchVector1536> SearchVectors1536 { get; set; } = null!;
    public DbSet<SearchVector768> SearchVectors768 { get; set; } = null!;
    public DbSet<SearchVector384> SearchVectors384 { get; set; } = null!;
    
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
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(255).IsRequired();
        });

        // Persona
        modelBuilder.Entity<Persona>(entity =>
        {
            entity.ToTable("personas");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Domain }).IsUnique();
            entity.Property(e => e.Domain).HasMaxLength(100).IsRequired();
            
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

        // ProcessCapability
        modelBuilder.Entity<ProcessCapability>(entity =>
        {
            entity.ToTable("process_capabilities");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ProcessType }).IsUnique();
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.ProcessCapabilities)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureJourneyDomain(ModelBuilder modelBuilder)
    {
        // Journey
        modelBuilder.Entity<Journey>(entity =>
        {
            entity.ToTable("journeys", t => t.HasCheckConstraint("CK_Journey_State", 
                "\"State\" IN ('Active', 'Paused', 'Completed', 'Abandoned')"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Purpose).IsRequired();
            entity.Property(e => e.State).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Context).HasColumnType("jsonb");
            
            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.Journeys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Persona)
                .WithMany(p => p.Journeys)
                .HasForeignKey(e => e.PersonaId);
        });

        // JourneyFramework
        modelBuilder.Entity<JourneyFramework>(entity =>
        {
            entity.ToTable("journey_frameworks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JourneyId).IsUnique();
            entity.Property(e => e.JourneyType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FrameworkElements).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.ProjectionRules).HasColumnType("jsonb").IsRequired();
            
            entity.HasOne(e => e.Journey)
                .WithOne(j => j.Framework)
                .HasForeignKey<JourneyFramework>(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Journal
        modelBuilder.Entity<Journal>(entity =>
        {
            entity.ToTable("journals", t => t.HasCheckConstraint("CK_Journal_Type",
                "\"Type\" IN ('Research', 'Method', 'Decision', 'Reflection')"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            
            entity.HasOne(e => e.Journey)
                .WithMany(j => j.Journals)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JournalEntry
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("journal_entries", t => t.HasCheckConstraint("CK_JournalEntry_Significance",
                "\"Significance\" IN ('Routine', 'Notable', 'Critical', 'Milestone')"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Significance).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Tags).HasColumnType("text[]");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            
            entity.HasOne(e => e.Journal)
                .WithMany(j => j.Entries)
                .HasForeignKey(e => e.JournalId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureKnowledgeDomain(ModelBuilder modelBuilder)
    {
        // KnowledgeScope
        modelBuilder.Entity<KnowledgeScope>(entity =>
        {
            entity.ToTable("knowledge_scopes", t => t.HasCheckConstraint("CK_KnowledgeScope_Type",
                "\"Type\" IN ('Project', 'Topic', 'Subject', 'Custom')"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            
            entity.HasOne(e => e.ParentScope)
                .WithMany(p => p.ChildScopes)
                .HasForeignKey(e => e.ParentScopeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Document
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Documents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Scope)
                .WithMany(s => s.Documents)
                .HasForeignKey(e => e.ScopeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // DocumentMetadata
        modelBuilder.Entity<DocumentMetadata>(entity =>
        {
            entity.ToTable("document_metadata");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DocumentId).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(1000);
            entity.Property(e => e.Authors).HasColumnType("text[]");
            entity.Property(e => e.ExtendedMetadata).HasColumnType("jsonb");
            
            entity.HasOne(e => e.Document)
                .WithOne(d => d.Metadata)
                .HasForeignKey<DocumentMetadata>(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureJourneyProjections(ModelBuilder modelBuilder)
    {
        // JourneyDocumentSegment
        modelBuilder.Entity<JourneyDocumentSegment>(entity =>
        {
            entity.ToTable("journey_document_segments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.JourneyId, e.DocumentId, e.SequenceIndex }).IsUnique();
            entity.Property(e => e.SegmentContent).IsRequired();
            entity.Property(e => e.SegmentType).HasMaxLength(50);
            entity.Property(e => e.StructuralPath).HasColumnType("jsonb");
            entity.Property(e => e.ByteRange).HasColumnType("int4range");
            entity.Property(e => e.CreatedByRule).HasMaxLength(255);
            entity.Property(e => e.CreatedForQuestion).HasMaxLength(255);
            
            entity.HasOne(e => e.Journey)
                .WithMany(j => j.DocumentSegments)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Document)
                .WithMany(d => d.JourneySegments)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JourneySegmentAssessment
        modelBuilder.Entity<JourneySegmentAssessment>(entity =>
        {
            entity.ToTable("journey_segment_assessments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssessmentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RubricScores).HasColumnType("jsonb");
            entity.Property(e => e.ReasoningChain).HasColumnType("jsonb");
            entity.Property(e => e.AssessedByModel).HasMaxLength(100);
            
            entity.HasOne(e => e.Segment)
                .WithMany(s => s.Assessments)
                .HasForeignKey(e => e.SegmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JourneyFormation
        modelBuilder.Entity<JourneyFormation>(entity =>
        {
            entity.ToTable("journey_formations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InsightType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.InsightContent).IsRequired();
            entity.Property(e => e.FormedFromSegments).HasColumnType("jsonb");
            entity.Property(e => e.FormedThroughQuestions).HasColumnType("jsonb");
            
            entity.HasOne(e => e.Journey)
                .WithMany(j => j.Formations)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureSearchInfrastructure(ModelBuilder modelBuilder)
    {
        // SearchIndex
        modelBuilder.Entity<SearchIndex>(entity =>
        {
            entity.ToTable("search_indexes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SegmentId, e.VectorModel }).IsUnique();
            entity.Property(e => e.VectorModel).HasMaxLength(100).IsRequired();
            
            entity.HasOne(e => e.Segment)
                .WithMany(s => s.SearchIndexes)
                .HasForeignKey(e => e.SegmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SearchVector1536
        modelBuilder.Entity<SearchVector1536>(entity =>
        {
            entity.ToTable("search_vectors_1536");
            entity.HasKey(e => e.IndexId);
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(1536)")
                .IsRequired();
            
            entity.HasOne(e => e.Index)
                .WithOne()
                .HasForeignKey<SearchVector1536>(e => e.IndexId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // HNSW index will be created in migration with raw SQL
        });

        // SearchVector768
        modelBuilder.Entity<SearchVector768>(entity =>
        {
            entity.ToTable("search_vectors_768");
            entity.HasKey(e => e.IndexId);
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(768)")
                .IsRequired();
            
            entity.HasOne(e => e.Index)
                .WithOne()
                .HasForeignKey<SearchVector768>(e => e.IndexId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SearchVector384
        modelBuilder.Entity<SearchVector384>(entity =>
        {
            entity.ToTable("search_vectors_384");
            entity.HasKey(e => e.IndexId);
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(384)")
                .IsRequired();
            
            entity.HasOne(e => e.Index)
                .WithOne()
                .HasForeignKey<SearchVector384>(e => e.IndexId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureProcessInfrastructure(ModelBuilder modelBuilder)
    {
        // ProcessDefinition
        modelBuilder.Entity<ProcessDefinition>(entity =>
        {
            entity.ToTable("process_definitions", t => {
                t.HasCheckConstraint("CK_ProcessDefinition_Category",
                    "\"Category\" IN ('Methodological', 'Developmental', 'Analytical', 'Compositional', 'Reflective')");
                t.HasCheckConstraint("CK_ProcessDefinition_Trigger",
                    "\"TriggerType\" IN ('Manual', 'UserInitiated')");
            });
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProcessType).IsUnique();
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TriggerType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Inputs).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Configuration).HasColumnType("jsonb");
        });

        // ProcessExecution
        modelBuilder.Entity<ProcessExecution>(entity =>
        {
            entity.ToTable("process_executions", t => t.HasCheckConstraint("CK_ProcessExecution_State",
                "\"State\" IN ('Pending', 'Running', 'Completed', 'Failed', 'Cancelled')"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();
            entity.Property(e => e.State).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Inputs).HasColumnType("jsonb").IsRequired();
            
            entity.HasOne(e => e.Journey)
                .WithMany(j => j.ProcessExecutions)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessResult
        modelBuilder.Entity<ProcessResult>(entity =>
        {
            entity.ToTable("process_results");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExecutionId).IsUnique();
            entity.Property(e => e.ProcessType).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Data).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            
            entity.HasOne(e => e.Execution)
                .WithOne(ex => ex.Result)
                .HasForeignKey<ProcessResult>(e => e.ExecutionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}