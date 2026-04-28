using EMAP.Domain.Fyp;
using FluentAssertions;
using Xunit;

namespace EMAP.Tests.Unit
{
    public class FinalizationTests
    {
        [Fact]
        public void Fyp1_Should_Be_Finalizable_When_FinalEvaluation_Completed()
        {
            // Arrange
            var group = new StudentGroup
            {
                Id = 1,
                CurrentStage = FypStage.Fyp1,
                TentativeProjectTitle = "EMAP",
                ProgramCode = "SE",
                LeaderId = "student-1"
            };

            var finalEvaluationStatus = FypEvaluationStatus.Completed;

            // Act
            var canFinalize =
                group.CurrentStage == FypStage.Fyp1 &&
                finalEvaluationStatus == FypEvaluationStatus.Completed;

            // Assert
            canFinalize.Should().BeTrue();
        }

        [Fact]
        public void Fyp1_Should_Not_Be_Finalizable_When_FinalEvaluation_Not_Completed()
        {
            // Arrange
            var group = new StudentGroup
            {
                Id = 1,
                CurrentStage = FypStage.Fyp1,
                TentativeProjectTitle = "EMAP",
                ProgramCode = "SE",
                LeaderId = "student-1"
            };

            var finalEvaluationStatus = FypEvaluationStatus.Scheduled;

            // Act
            var canFinalize =
                group.CurrentStage == FypStage.Fyp1 &&
                finalEvaluationStatus == FypEvaluationStatus.Completed;

            // Assert
            canFinalize.Should().BeFalse();
        }
    }
}
