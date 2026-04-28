using FluentAssertions;
using Xunit;

namespace EMAP.Tests.Unit
{
    public class EvaluationTests
    {
        [Fact]
        public void WeightedMarks_Should_Be_Calculated_Correctly()
        {
            // Arrange
            decimal obtainedMarks = 36;
            decimal totalMarks = 40;
            decimal weightagePercent = 20;

            // Act
            var weightedMarks = Math.Round((obtainedMarks / totalMarks) * weightagePercent, 2);

            // Assert
            weightedMarks.Should().Be(18);
        }

        [Fact]
        public void WeightedMarks_Should_Be_Zero_When_TotalMarks_Is_Zero()
        {
            // Arrange
            decimal obtainedMarks = 0;
            decimal totalMarks = 0;
            decimal weightagePercent = 20;

            // Act
            var weightedMarks = totalMarks <= 0
                ? 0
                : Math.Round((obtainedMarks / totalMarks) * weightagePercent, 2);

            // Assert
            weightedMarks.Should().Be(0);
        }

        [Fact]
        public void Marks_Should_Not_Exceed_MaxMarks()
        {
            // Arrange
            decimal awardedMarks = 8;
            decimal maxMarks = 5;

            // Act
            if (awardedMarks > maxMarks)
                awardedMarks = maxMarks;

            // Assert
            awardedMarks.Should().Be(5);
        }
    }
}
