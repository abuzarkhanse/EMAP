namespace EMAP.Domain.Fyp
{
    public enum ProjectSource
    {
        UniversityProposed,
        StudentProposed
    }

    public class FypProject
    {
        public int Id { get; set; }

        public int FypCallId { get; set; }
        public FypCall FypCall { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ProjectSource Source { get; set; }

        // Supervisor assignment
        public string SupervisorId { get; set; } = string.Empty;
    }
}
