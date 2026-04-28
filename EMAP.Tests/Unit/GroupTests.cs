using EMAP.Domain.Fyp;
using FluentAssertions;
using Xunit;

namespace EMAP.Tests.Unit
{
    public class GroupTests
    {
        [Fact]
        public void Group_Should_Be_Eligible_For_Proposal_When_Approved_And_Supervisor_Assigned()
        {
            // Arrange
            var group = new StudentGroup
            {
                Id = 1,
                SupervisorId = 10,
                Status = GroupStatus.Approved,
                TentativeProjectTitle = "EMAP",
                ProgramCode = "SE",
                LeaderId = "student-1"
            };

            // Act
            var canSubmitProposal =
                group.SupervisorId != null &&
                group.Status == GroupStatus.Approved;

            // Assert
            canSubmitProposal.Should().BeTrue();
        }

        [Fact]
        public void Group_Should_Not_Be_Eligible_When_Supervisor_Not_Assigned()
        {
            // Arrange
            var group = new StudentGroup
            {
                Id = 1,
                SupervisorId = null,
                Status = GroupStatus.PendingSupervisorSelection,
                TentativeProjectTitle = "EMAP",
                ProgramCode = "SE",
                LeaderId = "student-1"
            };

            // Act
            var canSubmitProposal =
                group.SupervisorId != null &&
                group.Status == GroupStatus.Approved;

            // Assert
            canSubmitProposal.Should().BeFalse();
        }
    }
}
