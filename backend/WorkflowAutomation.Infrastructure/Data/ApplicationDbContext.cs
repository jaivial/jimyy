using Microsoft.EntityFrameworkCore;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowExecution> WorkflowExecutions { get; set; }
        public DbSet<NodeExecution> NodeExecutions { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<WorkflowVersion> WorkflowVersions { get; set; }
        public DbSet<AIConfiguration> AIConfigurations { get; set; }
        public DbSet<CollaborationSession> CollaborationSessions { get; set; }
        public DbSet<ExecutionLog> ExecutionLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore model classes that are only used within JSONB columns
            modelBuilder.Ignore<WorkflowDefinition>();
            modelBuilder.Ignore<Node>();
            modelBuilder.Ignore<Connection>();
            modelBuilder.Ignore<Position>();
            modelBuilder.Ignore<NodeStyle>();
            modelBuilder.Ignore<RetrySettings>();
            modelBuilder.Ignore<WorkflowSettings>();
            modelBuilder.Ignore<CollaboratorInfo>();
            modelBuilder.Ignore<WorkflowChange>();
            modelBuilder.Ignore<AIModelCapabilities>();

            // Workflow configuration
            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.WorkflowData).HasColumnType("jsonb");
                entity.Property(e => e.ActiveEditors).HasColumnType("jsonb");
                entity.HasIndex(e => e.Active);
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.Environment);

                entity.HasOne(e => e.Creator)
                    .WithMany(u => u.Workflows)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ParentWorkflow)
                    .WithMany(w => w.DerivedWorkflows)
                    .HasForeignKey(e => e.ParentWorkflowId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Execution configuration
            modelBuilder.Entity<WorkflowExecution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartedAt);
                entity.HasIndex(e => e.Environment);
                entity.Property(e => e.TriggerData).HasColumnType("jsonb");

                entity.HasOne(e => e.Workflow)
                    .WithMany(w => w.Executions)
                    .HasForeignKey(e => e.WorkflowId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Node Execution configuration
            modelBuilder.Entity<NodeExecution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ExecutionId);
                entity.HasIndex(e => e.NodeId);
                entity.Property(e => e.NodePosition).HasColumnType("jsonb");

                entity.HasOne(e => e.Execution)
                    .WithMany(w => w.NodeExecutions)
                    .HasForeignKey(e => e.ExecutionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Execution Log configuration
            modelBuilder.Entity<ExecutionLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ExecutionId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Level);
                entity.Property(e => e.Metadata).HasColumnType("jsonb");

                entity.HasOne(e => e.Execution)
                    .WithMany(w => w.Logs)
                    .HasForeignKey(e => e.ExecutionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Credential configuration
            modelBuilder.Entity<Credential>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EncryptedData).IsRequired();
                entity.HasIndex(e => new { e.CreatedBy, e.Type });

                entity.HasOne(e => e.Creator)
                    .WithMany(u => u.Credentials)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // WorkflowVersion configuration
            modelBuilder.Entity<WorkflowVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowId);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.WorkflowData).HasColumnType("jsonb");

                entity.HasOne(e => e.Workflow)
                    .WithMany(w => w.Versions)
                    .HasForeignKey(e => e.WorkflowId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AI Configuration
            modelBuilder.Entity<AIConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.IsDefault });
                entity.Property(e => e.Capabilities).HasColumnType("jsonb");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.AIConfigurations)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Collaboration Session
            modelBuilder.Entity<CollaborationSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowId);
                entity.Property(e => e.Collaborators).HasColumnType("jsonb");
                entity.Property(e => e.Changes).HasColumnType("jsonb");

                entity.HasOne(e => e.Workflow)
                    .WithMany()
                    .HasForeignKey(e => e.WorkflowId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.ExpiresAt });

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
