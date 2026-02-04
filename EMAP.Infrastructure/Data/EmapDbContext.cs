using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Infrastructure.Data
{
    public class EmapDbContext : IdentityDbContext<ApplicationUser>
    {
        public EmapDbContext(DbContextOptions<EmapDbContext> options)
            : base(options)
        {
        }

        public DbSet<FypCall> FypCalls => Set<FypCall>();
        public DbSet<FypProject> FypProjects => Set<FypProject>();
        public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
        public DbSet<ProposalSubmission> ProposalSubmissions => Set<ProposalSubmission>();
        public DbSet<FypSupervisor> FypSupervisors => Set<FypSupervisor>();
        public DbSet<ProposalDefenseSchedule> ProposalDefenseSchedules { get; set; } = null!;
        public DbSet<ProposalDefenseEvaluation> ProposalDefenseEvaluations { get; set; } = null!;
        public DbSet<FypChapterAnnouncement> FypChapterAnnouncements { get; set; } = null!;
        public DbSet<FypChapterSubmission> FypChapterSubmissions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ProposalDefenseEvaluation 1-1 ProposalSubmission
            modelBuilder.Entity<ProposalDefenseEvaluation>()
                .HasIndex(x => x.ProposalSubmissionId)
                .IsUnique();

            modelBuilder.Entity<ProposalDefenseEvaluation>()
                .HasOne(x => x.ProposalSubmission)
                .WithOne(p => p.DefenseEvaluation)
                .HasForeignKey<ProposalDefenseEvaluation>(x => x.ProposalSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChapterSubmission relations
            modelBuilder.Entity<FypChapterSubmission>(entity =>
            {
                entity.HasOne(x => x.Group)
                    .WithMany()
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ChapterAnnouncement)
                    .WithMany()
                    .HasForeignKey(x => x.ChapterAnnouncementId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FypSupervisor -> ApplicationUser (Identity)
            modelBuilder.Entity<FypSupervisor>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ StudentGroup -> FypSupervisor (CORRECT)
            modelBuilder.Entity<StudentGroup>()
                .HasOne(g => g.Supervisor)
                .WithMany()
                .HasForeignKey(g => g.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
