//using EMAP.Domain.Fyp;
//using EMAP.Tests.Helpers;
//using FluentAssertions;
//using Xunit;

//namespace EMAP.Tests.Integration
//{
//    public class FypWorkflowIntegrationTests
//    {
//        [Fact]
//        public async Task StudentGroup_Should_Be_Created_With_PendingSupervisorSelection_Status()
//        {
//            using var db = TestDbContextFactory.Create();

//            var call = new FypCall
//            {
//                Id = 1,
//                Batch = "2025",
//                Session = "Spring 2025",
//                Title = "FYP Call 2025",
//                Department = Department.SchoolOfComputingSciences,
//                AnnouncementDate = DateTime.UtcNow,
//                ProposalDeadline = DateTime.UtcNow.AddDays(30),
//                IsActive = true
//            };

//            db.FypCalls.Add(call);

//            var group = new StudentGroup
//            {
//                Id = 1,
//                FypCallId = call.Id,
//                LeaderId = "student-1",
//                ProgramCode = "SE",
//                TentativeProjectTitle = "EMAP",
//                Status = GroupStatus.PendingSupervisorSelection
//            };

//            db.StudentGroups.Add(group);
//            await db.SaveChangesAsync();

//            var savedGroup = db.StudentGroups.First();

//            savedGroup.Status.Should().Be(GroupStatus.PendingSupervisorSelection);
//            savedGroup.TentativeProjectTitle.Should().Be("EMAP");
//        }

//        [Fact]
//        public async Task SupervisorApproval_Should_Update_Group_Status_To_Approved()
//        {
//            using var db = TestDbContextFactory.Create();

//            var group = new StudentGroup
//            {
//                Id = 1,
//                FypCallId = 1,
//                LeaderId = "student-1",
//                SupervisorId = 1,
//                ProgramCode = "SE",
//                TentativeProjectTitle = "EMAP",
//                Status = GroupStatus.PendingSupervisorApproval
//            };

//            db.StudentGroups.Add(group);
//            await db.SaveChangesAsync();

//            group.Status = GroupStatus.Approved;
//            await db.SaveChangesAsync();

//            var updatedGroup = db.StudentGroups.First();

//            updatedGroup.Status.Should().Be(GroupStatus.Approved);
//        }

//        [Fact]
//        public async Task ProposalApproval_Should_Update_Status_To_ApprovedForDefense()
//        {
//            using var db = TestDbContextFactory.Create();

//            var proposal = new ProposalSubmission
//            {
//                Id = 1,
//                GroupId = 1,
//                Title = "EMAP Proposal",
//                FilePath = "/uploads/proposals/test.pdf",
//                SubmittedAt = DateTime.UtcNow,
//                Status = ProposalStatus.PendingReview,
//                RevisionNumber = 1
//            };

//            db.ProposalSubmissions.Add(proposal);
//            await db.SaveChangesAsync();

//            proposal.Status = ProposalStatus.ApprovedForDefense;
//            await db.SaveChangesAsync();

//            var updatedProposal = db.ProposalSubmissions.First();

//            updatedProposal.Status.Should().Be(ProposalStatus.ApprovedForDefense);
//        }

//        [Fact]
//        public async Task ChapterSupervisorApproval_Should_Make_Chapter_Ready_For_Coordinator()
//        {
//            using var db = TestDbContextFactory.Create();

//            var chapter = new FypChapterSubmission
//            {
//                Id = 1,
//                GroupId = 1,
//                ChapterAnnouncementId = 1,
//                Title = "SRS Chapter",
//                FilePath = "/uploads/chapters/srs.pdf",
//                SubmittedAt = DateTime.UtcNow,
//                SupervisorId = "supervisor-1",
//                Status = ChapterSubmissionStatus.Submitted
//            };

//            db.FypChapterSubmissions.Add(chapter);
//            await db.SaveChangesAsync();

//            chapter.Status = ChapterSubmissionStatus.SupervisorApproved;
//            chapter.ReviewedAt = DateTime.UtcNow;
//            await db.SaveChangesAsync();

//            var updatedChapter = db.FypChapterSubmissions.First();

//            updatedChapter.Status.Should().Be(ChapterSubmissionStatus.SupervisorApproved);
//        }

//        [Fact]
//        public async Task Fyp2Finalization_Should_Mark_Group_As_Completed_And_Ready_For_LmsSync()
//        {
//            using var db = TestDbContextFactory.Create();

//            var group = new StudentGroup
//            {
//                Id = 1,
//                FypCallId = 1,
//                LeaderId = "student-1",
//                ProgramCode = "SE",
//                TentativeProjectTitle = "EMAP",
//                CurrentStage = FypStage.Fyp2,
//                Status = GroupStatus.Approved,
//                IsFypCompleted = false,
//                ReadyForLmsSync = false
//            };

//            db.StudentGroups.Add(group);
//            await db.SaveChangesAsync();

//            group.IsFypCompleted = true;
//            group.CompletedAt = DateTime.UtcNow;
//            group.ReadyForLmsSync = true;
//            group.CompletionRemarks = "FYP completed successfully.";
//            await db.SaveChangesAsync();

//            var completedGroup = db.StudentGroups.First();

//            completedGroup.IsFypCompleted.Should().BeTrue();
//            completedGroup.ReadyForLmsSync.Should().BeTrue();
//            completedGroup.CompletedAt.Should().NotBeNull();
//        }
//    }
//}
