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
        public DbSet<ProposalDefenseSchedule> ProposalDefenseSchedules => Set<ProposalDefenseSchedule>();
        public DbSet<ProposalDefenseEvaluation> ProposalDefenseEvaluations => Set<ProposalDefenseEvaluation>();
        public DbSet<FypChapterAnnouncement> FypChapterAnnouncements => Set<FypChapterAnnouncement>();
        public DbSet<FypChapterSubmission> FypChapterSubmissions => Set<FypChapterSubmission>();

        public DbSet<FypMilestone> FypMilestones => Set<FypMilestone>();
        public DbSet<FypEvaluation> FypEvaluations => Set<FypEvaluation>();
        public DbSet<FypEvaluationCriterion> FypEvaluationCriteria => Set<FypEvaluationCriterion>();
        public DbSet<FypEvaluationScore> FypEvaluationScores => Set<FypEvaluationScore>();

        public DbSet<FypCommittee> FypCommittees { get; set; }
        public DbSet<FypCommitteeProgram> FypCommitteePrograms { get; set; }

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

            // FypSupervisor -> ApplicationUser
            modelBuilder.Entity<FypSupervisor>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentGroup -> FypSupervisor
            modelBuilder.Entity<StudentGroup>(entity =>
            {
                entity.HasOne(g => g.Supervisor)
                    .WithMany()
                    .HasForeignKey(g => g.SupervisorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.CurrentStage)
                    .HasConversion<int>();
            });

            // ChapterSubmission relations + new stage/milestone config
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

                entity.Property(x => x.Stage)
                    .HasConversion<int>();

                entity.HasOne(x => x.Milestone)
                    .WithMany(m => m.ChapterSubmissions)
                    .HasForeignKey(x => x.MilestoneId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // FypMilestone
            modelBuilder.Entity<FypMilestone>(entity =>
            {
                entity.Property(x => x.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.Property(x => x.Stage)
                    .HasConversion<int>();

                entity.Property(x => x.Type)
                    .HasConversion<int>();

                entity.HasIndex(x => new { x.Stage, x.Type, x.ChapterNumber, x.DisplayOrder });
            });

            // FypEvaluation
            modelBuilder.Entity<FypEvaluation>(entity =>
            {
                entity.Property(x => x.EvaluatorUserId)
                    .HasMaxLength(450)
                    .IsRequired();

                entity.Property(x => x.TotalMarks)
                    .HasColumnType("decimal(5,2)");

                entity.Property(x => x.Status)
                    .HasConversion<int>();

                entity.HasOne(x => x.StudentGroup)
                    .WithMany(g => g.Evaluations)
                    .HasForeignKey(x => x.StudentGroupId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Milestone)
                    .WithMany(m => m.Evaluations)
                    .HasForeignKey(x => x.MilestoneId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // FypEvaluationCriterion
            modelBuilder.Entity<FypEvaluationCriterion>(entity =>
            {
                entity.Property(x => x.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.Property(x => x.MaxMarks)
                    .HasColumnType("decimal(5,2)");

                entity.Property(x => x.EvaluationType)
                    .HasConversion<int>();

                entity.HasIndex(x => new { x.EvaluationType, x.DisplayOrder });
            });

            // FypEvaluationScore
            modelBuilder.Entity<FypEvaluationScore>(entity =>
            {
                entity.Property(x => x.AwardedMarks)
                    .HasColumnType("decimal(5,2)");

                entity.HasOne(x => x.Evaluation)
                    .WithMany(e => e.Scores)
                    .HasForeignKey(x => x.EvaluationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Criterion)
                    .WithMany(c => c.Scores)
                    .HasForeignKey(x => x.CriterionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed FYP milestones
            modelBuilder.Entity<FypMilestone>().HasData(
                new FypMilestone
                {
                    Id = 1,
                    Title = "FYP-1 Chapter 1",
                    Stage = FypStage.Fyp1,
                    Type = FypMilestoneType.Chapter,
                    ChapterNumber = 1,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 1
                },
                new FypMilestone
                {
                    Id = 2,
                    Title = "FYP-1 Chapter 2",
                    Stage = FypStage.Fyp1,
                    Type = FypMilestoneType.Chapter,
                    ChapterNumber = 2,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 2
                },
                new FypMilestone
                {
                    Id = 3,
                    Title = "FYP-1 Chapter 3",
                    Stage = FypStage.Fyp1,
                    Type = FypMilestoneType.Chapter,
                    ChapterNumber = 3,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 3
                },
                new FypMilestone
                {
                    Id = 4,
                    Title = "FYP-1 Mid Evaluation",
                    Stage = FypStage.Fyp1,
                    Type = FypMilestoneType.MidEvaluation,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 4
                },
                new FypMilestone
                {
                    Id = 5,
                    Title = "FYP-1 Final Evaluation",
                    Stage = FypStage.Fyp1,
                    Type = FypMilestoneType.FinalEvaluation,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 5
                },

                new FypMilestone
                {
                    Id = 6,
                    Title = "FYP-2 Chapter 1",
                    Stage = FypStage.Fyp2,
                    Type = FypMilestoneType.Chapter,
                    ChapterNumber = 1,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 1
                },
                new FypMilestone
                {
                    Id = 7,
                    Title = "FYP-2 Chapter 2",
                    Stage = FypStage.Fyp2,
                    Type = FypMilestoneType.Chapter,
                    ChapterNumber = 2,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 2
                },
                new FypMilestone
                {
                    Id = 8,
                    Title = "FYP-2 Chapter 3",
                    Stage = FypStage.Fyp2,
                    Type = FypMilestoneType.Chapter,
                    ChapterNumber = 3,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 3
                },
                new FypMilestone
                {
                    Id = 9,
                    Title = "FYP-2 Mid Evaluation",
                    Stage = FypStage.Fyp2,
                    Type = FypMilestoneType.MidEvaluation,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 4
                },
                new FypMilestone
                {
                    Id = 10,
                    Title = "FYP-2 Pre-Final Evaluation",
                    Stage = FypStage.Fyp2,
                    Type = FypMilestoneType.PreFinalEvaluation,
                    IsOptional = true,
                    IsActive = true,
                    DisplayOrder = 5
                },
                new FypMilestone
                {
                    Id = 11,
                    Title = "FYP-2 Final Evaluation",
                    Stage = FypStage.Fyp2,
                    Type = FypMilestoneType.FinalEvaluation,
                    IsOptional = false,
                    IsActive = true,
                    DisplayOrder = 6
                }
            );

            // Seed evaluation criteria
            modelBuilder.Entity<FypEvaluationCriterion>().HasData(
                new FypEvaluationCriterion
                {
                    Id = 1,
                    EvaluationType = FypMilestoneType.MidEvaluation,
                    Title = "Problem Understanding",
                    MaxMarks = 10,
                    DisplayOrder = 1,
                    IsActive = true
                },
                new FypEvaluationCriterion
                {
                    Id = 2,
                    EvaluationType = FypMilestoneType.MidEvaluation,
                    Title = "Progress and Methodology",
                    MaxMarks = 10,
                    DisplayOrder = 2,
                    IsActive = true
                },
                new FypEvaluationCriterion
                {
                    Id = 3,
                    EvaluationType = FypMilestoneType.MidEvaluation,
                    Title = "Documentation",
                    MaxMarks = 10,
                    DisplayOrder = 3,
                    IsActive = true
                },
                new FypEvaluationCriterion
                {
                    Id = 4,
                    EvaluationType = FypMilestoneType.MidEvaluation,
                    Title = "Presentation",
                    MaxMarks = 10,
                    DisplayOrder = 4,
                    IsActive = true
                },

                new FypEvaluationCriterion
                {
                    Id = 5,
                    EvaluationType = FypMilestoneType.PreFinalEvaluation,
                    Title = "Implementation Progress",
                    MaxMarks = 10,
                    DisplayOrder = 1,
                    IsActive = true
                },
                new FypEvaluationCriterion
                {
                    Id = 6,
                    EvaluationType = FypMilestoneType.PreFinalEvaluation,
                    Title = "Documentation Quality",
                    MaxMarks = 10,
                    DisplayOrder = 2,
                    IsActive = true
                },
                new FypEvaluationCriterion
                {
                    Id = 7,
                    EvaluationType = FypMilestoneType.PreFinalEvaluation,
                    Title = "Presentation and Readiness",
                    MaxMarks = 10,
                    DisplayOrder = 3,
                    IsActive = true
                },

                new FypEvaluationCriterion
                {
                    Id = 8,
                    EvaluationType = FypMilestoneType.FinalEvaluation,
                    Title = "Final Report",
                    MaxMarks = 20,
                    DisplayOrder = 1,
                    IsActive = true
                },
                new FypEvaluationCriterion
                {
                    Id = 9,
                    EvaluationType = FypMilestoneType.FinalEvaluation,
                    Title = "Implementation",
                    MaxMarks = 20,
                    DisplayOrder = 2,
                    IsActive = true
                },
                new FypEvaluationCriterion
                {
                    Id = 10,
                    EvaluationType = FypMilestoneType.FinalEvaluation,
                    Title = "Presentation",
                    MaxMarks = 10,
                    DisplayOrder = 3,
                    IsActive = true
                },
                new FypEvaluationCriterion
                {
                    Id = 11,
                    EvaluationType = FypMilestoneType.FinalEvaluation,
                    Title = "Viva / Question Answer",
                    MaxMarks = 10,
                    DisplayOrder = 4,
                    IsActive = true
                }
            );

            modelBuilder.Entity<FypEvaluationCriterion>().HasData(
                new FypEvaluationCriterion { Id = 3001, EvaluationType = FypMilestoneType.MidEvaluation, Title = "Content & Knowledge", Description = "Understanding of project domain, concepts, and technical knowledge.", MaxMarks = 5, DisplayOrder = 1, IsActive = true },
                new FypEvaluationCriterion { Id = 3002, EvaluationType = FypMilestoneType.MidEvaluation, Title = "Problem Analysis", Description = "Clarity and depth of problem understanding and analysis.", MaxMarks = 5, DisplayOrder = 2, IsActive = true },
                new FypEvaluationCriterion { Id = 3003, EvaluationType = FypMilestoneType.MidEvaluation, Title = "Investigation", Description = "Research effort, literature review, and technical investigation.", MaxMarks = 5, DisplayOrder = 3, IsActive = true },
                new FypEvaluationCriterion { Id = 3004, EvaluationType = FypMilestoneType.MidEvaluation, Title = "Design of Solution", Description = "System design, architecture, modeling, and proposed solution quality.", MaxMarks = 5, DisplayOrder = 4, IsActive = true },
                new FypEvaluationCriterion { Id = 3005, EvaluationType = FypMilestoneType.MidEvaluation, Title = "Progress & Tool Usage", Description = "Implementation progress and effective use of tools/frameworks.", MaxMarks = 5, DisplayOrder = 5, IsActive = true },
                new FypEvaluationCriterion { Id = 3006, EvaluationType = FypMilestoneType.MidEvaluation, Title = "Project Management", Description = "Planning, task management, teamwork, and timeline handling.", MaxMarks = 5, DisplayOrder = 6, IsActive = true },
                new FypEvaluationCriterion { Id = 3007, EvaluationType = FypMilestoneType.MidEvaluation, Title = "Presentation", Description = "Presentation quality, flow, communication, and confidence.", MaxMarks = 5, DisplayOrder = 7, IsActive = true },
                new FypEvaluationCriterion { Id = 3008, EvaluationType = FypMilestoneType.MidEvaluation, Title = "Viva", Description = "Answers during questioning, conceptual clarity, and confidence.", MaxMarks = 5, DisplayOrder = 8, IsActive = true }
            );


            modelBuilder.Entity<FypCommittee>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasMany(x => x.CommitteePrograms)
                      .WithOne(x => x.FypCommittee)
                      .HasForeignKey(x => x.FypCommitteeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FypCommitteeProgram>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ProgramCode)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(x => new { x.FypCommitteeId, x.ProgramCode }).IsUnique();
            });
        }
    }
}
