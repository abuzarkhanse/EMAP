using EMAP.Domain.Fyp;
using FluentAssertions;
using Xunit;

namespace EMAP.Tests.Unit
{
    public class SupervisorTests
    {
        [Fact]
        public void Supervisor_Should_Have_Available_Slot_When_CurrentSlots_Less_Than_MaxSlots()
        {
            // Arrange
            var supervisor = new FypSupervisor
            {
                Id = 1,
                Name = "Supervisor 001",
                MaxSlots = 5,
                CurrentSlots = 3,
                IsActive = true
            };

            // Act
            var hasAvailableSlot =
                supervisor.IsActive &&
                supervisor.CurrentSlots < supervisor.MaxSlots;

            // Assert
            hasAvailableSlot.Should().BeTrue();
        }

        [Fact]
        public void Supervisor_Should_Not_Have_Available_Slot_When_Full()
        {
            // Arrange
            var supervisor = new FypSupervisor
            {
                Id = 1,
                Name = "Supervisor 001",
                MaxSlots = 5,
                CurrentSlots = 5,
                IsActive = true
            };

            // Act
            var hasAvailableSlot =
                supervisor.IsActive &&
                supervisor.CurrentSlots < supervisor.MaxSlots;

            // Assert
            hasAvailableSlot.Should().BeFalse();
        }
    }
}
