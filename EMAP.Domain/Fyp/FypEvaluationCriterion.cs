using System.Collections.Generic;

namespace EMAP.Domain.Fyp
{
    public class FypEvaluationCriterion
    {
        public int Id { get; set; }

        public FypMilestoneType EvaluationType { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal MaxMarks { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<FypEvaluationScore> Scores { get; set; } = new List<FypEvaluationScore>();
    }
}